using System.Collections;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Player;
using Protogenesis.Views;

namespace Protogenesis.Rendering
{
    /// <summary>
    /// Zoom biológicamente vinculado al fenotipo activo (GDD v4.6).
    ///
    /// Cada fenotipo representa un organismo de tamaño real diferente:
    ///   Unknown / Protocélula    → 900× mag  → ortho ~2.5  (0.5–2 µm)
    ///   BacteriaVeloz            → 800× mag  → ortho ~3.0  (1–10 µm)
    ///   MicroalgaFototrofica     → 500× mag  → ortho ~5.0  (10–50 µm)
    ///   DiatomEaFortaleza        → 300× mag  → ortho ~7.0  (50–200 µm)
    ///   AmebaCazadora            → 180× mag  → ortho ~11.0 (100–500 µm)
    ///   ParmecioCiliar           → 300× mag  → ortho ~7.0  (50–200 µm)
    ///   BiofilmColonial          → 100× mag  → ortho ~18.0 (variable)
    ///   HidraDepredadora         → 40×  mag  → ortho ~28.0 (1–15 mm)
    ///   TardigradoExtremofilo    → 25×  mag  → ortho ~35.0 (0.1–1.5 mm)
    ///   MulticelularComplejo     → 60×  mag  → ortho ~20.0 (multicel.)
    ///
    /// Scroll wheel: ±(zoomBase × 0.08) por notch dentro de [base×0.7, base×1.4].
    /// Transición normal: lerp ease-in-out 3 s.
    /// Transición HidraDepredadora / TardigradoExtremofilo: 5 s + pausa cinemática 2 s.
    /// Regresión evolutiva: zoom IN rojo/naranja 3 s via TriggerRegressionZoom().
    /// </summary>
    public class ZoomManager : MonoBehaviour
    {
        public static ZoomManager Instance { get; private set; }

        // ── WorldSize del jugador por fenotipo (índice = (int)PhenotypeType) ────
        // targetOrthographicSize = playerWorldSize × zoomConstant
        // Referencia GDD v4.6:
        //   WorldSize 0.005 (bacteria)  → ortho ~4     (zoom 800×)
        //   WorldSize 0.15  (paramecio) → ortho ~30    (zoom 300×)
        //   WorldSize 500   (tardigrado)→ ortho ~10000 (zoom 25×)
        private static readonly float[] PlayerWorldSizeByPhenotype =
        {
            0.005f,   // 0 Unknown              (Protocélula — bacteria)
            0.005f,   // 1 BacteriaVeloz
            0.020f,   // 2 MicroalgaFototrofica (20 µm)
            0.050f,   // 3 DiatomEaFortaleza    (50 µm)
            0.300f,   // 4 AmebaCazadora        (300 µm)
            0.150f,   // 5 ParmecioCiliar       (150 µm)
            0.500f,   // 6 BiofilmColonial      (colonia ~500 µm)
            5.000f,   // 7 HidraDepredadora     (5 mm)
          500.000f,   // 8 TardigradoExtremofilo(0.5 mm en escala relativa)
            2.000f,   // 9 MulticelularComplejo (2 mm)
        };

        [Tooltip("Multiplica el WorldSize del jugador para calcular orthographicSize objetivo.")]
        [SerializeField] private float zoomConstant = 2400f;

        private const float TransitionNormal     = 3.0f;
        private const float TransitionEpic       = 5.0f;
        private const float CinematicPauseEpic   = 2.0f;
        private const float TransitionRegression = 3.0f;
        private const float ScrollFraction       = 0.08f;
        // Un cambio de ortho mayor a este factor se considera "épico"
        private const float EpicZoomRatio        = 5.0f;

        // ── Estado ───────────────────────────────────────────────────────────────
        private float _currentZoomBase = 1.0f;   // = PlayerWorldSize × zoomConstant
        private bool  _regressionFlash = false;
        private float _regressionTimer = 0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            float initialOrtho = PlayerWorldSizeByPhenotype[0] * zoomConstant;
            _currentZoomBase = initialOrtho;
            ApplyCameraOrtho(initialOrtho);
        }

        private void OnEnable()
        {
            EventBus.OnSpecializationConsolidated += OnSpecializationConsolidated;
        }

        private void OnDisable()
        {
            EventBus.OnSpecializationConsolidated -= OnSpecializationConsolidated;
        }

        private void Update()
        {
            HandleScrollWheel();
            HandleRegressionOverlay();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Zoom por fenotipo

        private void OnSpecializationConsolidated(Progression.SpecializationType type, string displayName)
        {
            // Mapea los 16 tipos de especialización a los 10 índices de WorldSize
            int phenoIdx = Mathf.Clamp((int)type / 2, 0, PlayerWorldSizeByPhenotype.Length - 1);
            float targetBase = ZoomBase(phenoIdx);
            _currentZoomBase = targetBase;

            float ratio = Mathf.Abs(CameraOrtho() / Mathf.Max(0.001f, targetBase));
            if (ratio > EpicZoomRatio || ratio < 1f / EpicZoomRatio)
                StartCoroutine(EpicZoomRoutine(targetBase));
            else
                StartCoroutine(StandardZoomRoutine(targetBase));
        }

        private IEnumerator StandardZoomRoutine(float targetBase)
        {
            float startOrtho = CameraOrtho();
            float elapsed    = 0f;

            while (elapsed < TransitionNormal)
            {
                elapsed += Time.deltaTime;
                float t      = elapsed / TransitionNormal;
                float smooth = t * t * (3f - 2f * t);
                ApplyCameraOrtho(Mathf.Lerp(startOrtho, targetBase, smooth));
                yield return null;
            }

            ApplyCameraOrtho(targetBase);
        }

        private IEnumerator EpicZoomRoutine(float targetBase)
        {
            float startOrtho = CameraOrtho();
            float elapsed    = 0f;
            float overshoot  = targetBase * 1.12f;

            while (elapsed < TransitionEpic)
            {
                elapsed += Time.deltaTime;
                float t      = elapsed / TransitionEpic;
                float smooth = t * t * (3f - 2f * t);
                ApplyCameraOrtho(Mathf.Lerp(startOrtho, overshoot, smooth));
                yield return null;
            }

            float pauseTimer = 0f;
            while (pauseTimer < CinematicPauseEpic)
            {
                pauseTimer += Time.deltaTime;
                float wobble = Mathf.Sin(pauseTimer * Mathf.PI * 2f) * 0.5f;
                ApplyCameraOrtho(overshoot + wobble);
                yield return null;
            }

            ApplyCameraOrtho(targetBase);
        }

        private IEnumerator RegressionZoomRoutine(float targetBase)
        {
            _regressionFlash = true;
            _regressionTimer = TransitionRegression;

            float startOrtho = CameraOrtho();
            float elapsed    = 0f;

            while (elapsed < TransitionRegression)
            {
                elapsed += Time.deltaTime;
                float t      = elapsed / TransitionRegression;
                float smooth = t * t * (3f - 2f * t);
                ApplyCameraOrtho(Mathf.Lerp(startOrtho, targetBase, smooth));
                yield return null;
            }

            ApplyCameraOrtho(targetBase);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Scroll manual

        private void HandleScrollWheel()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.001f) return;

            float step     = _currentZoomBase * ScrollFraction;
            float minOrtho = _currentZoomBase * 0.7f;
            float maxOrtho = _currentZoomBase * 1.4f;

            float current = CameraOrtho();
            float next    = Mathf.Clamp(current - scroll * step * 5f, minOrtho, maxOrtho);
            ApplyCameraOrtho(next);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Overlay de regresión

        private void HandleRegressionOverlay()
        {
            if (!_regressionFlash) return;
            _regressionTimer -= Time.deltaTime;
            if (_regressionTimer <= 0f) _regressionFlash = false;
        }

        private void OnGUI()
        {
            if (!_regressionFlash) return;

            float alpha = (_regressionTimer / TransitionRegression) * 0.45f;
            var   col   = new Color(0.85f, 0.25f, 0.05f, alpha);
            GUI.color = col;
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>Dispara el zoom de regresión desde código externo (DeathSystem).</summary>
        public void TriggerRegressionZoom()
        {
            StartCoroutine(RegressionZoomRoutine(_currentZoomBase));
        }

        /// <summary>Zoom base del fenotipo actual.</summary>
        public float CurrentZoomBase => _currentZoomBase;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        // TODO: Primordia Fase 3.4 — ZoomBase se reescribirá con WorldSize por especialización
        private float ZoomBase(int specializationIdx = 0)
        {
            float worldSize = (specializationIdx >= 0 && specializationIdx < PlayerWorldSizeByPhenotype.Length)
                ? PlayerWorldSizeByPhenotype[specializationIdx]
                : PlayerWorldSizeByPhenotype[0];
            return worldSize * zoomConstant;
        }

        // Primordia: preferir la cámara de la vista activa; fallback a Camera.main
        private static Camera ActiveCamera()
        {
            if (Views.ViewManager.Instance != null)
            {
                var cam = Views.ViewManager.Instance.GetCamera(Views.ViewManager.Instance.CurrentView);
                if (cam != null) return cam;
            }
            return Camera.main;
        }

        private static float CameraOrtho()
        {
            var cam = ActiveCamera();
            return cam != null ? cam.orthographicSize : 10f;
        }

        private static void ApplyCameraOrtho(float size)
        {
            var cam = ActiveCamera();
            if (cam != null)
                cam.orthographicSize = Mathf.Max(0.5f, size);
        }

        #endregion
    }
}
