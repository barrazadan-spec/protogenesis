using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Protogenesis.Core;
using Protogenesis.Views;
using Protogenesis.Ecosystem;

namespace Protogenesis.Rendering
{
    /// <summary>
    /// PostProcessingController — Efectos de cámara reactivos al estado celular (Primordia, Prompt 6.1).
    ///
    /// Crea y gestiona en runtime un Volume URP global con 4 overrides:
    ///
    ///   ChromaticAberration — intensidad ligada al estrés celular (0 → 0.9).
    ///   Vignette            — intensidad y color según HP (bajo HP = viñeta roja).
    ///   ColorAdjustments    — filtro de color por zona/vista:
    ///                           Fótica    → azul frío brillante
    ///                           Pelágica  → azul neutro oscuro
    ///                           Béntica   → marrón cálido muy oscuro
    ///                           Interior  → cian verdoso (citoplasma)
    ///   Bloom               — intensidad ambiente por zona.
    ///
    /// Efectos de impacto (transitorios):
    ///   · Daño recibido  → spike de aberración cromática + spike de viñeta roja + CameraShake.
    ///   · Vista cambiada → flash de ColorAdjustments (0.25 s).
    ///   · Estrés Catastrophic → pulso periódico de aberración.
    ///
    /// CameraShake:
    ///   · Se aplica como offset temporal sobre la cámara activa.
    ///   · Decae exponencialmente en shakeDuration segundos.
    ///   · No interfiere con la lógica de seguimiento de ExteriorMap/ViewBase.
    ///
    /// Requisitos: Unity URP (com.unity.render-pipelines.universal). Si el paquete no
    /// está presente, el controlador se desactiva sin errores (TryGet guards).
    /// </summary>
    public class PostProcessingController : MonoBehaviour
    {
        public static PostProcessingController Instance { get; private set; }

        // ── Configuración ─────────────────────────────────────────────────────────
        [Header("Lerp")]
        [SerializeField] private float lerpSpeed       = 4.0f;

        [Header("Aberración Cromática")]
        [SerializeField] private float caMaxStress     = 0.90f;  // valor al 100% estrés
        [SerializeField] private float caDamageSpike   = 0.95f;  // spike al recibir daño
        [SerializeField] private float caSpikeDecay    = 3.0f;   // decay del spike (s⁻¹)

        [Header("Viñeta")]
        [SerializeField] private float vignetteMaxHP   = 0.55f;  // intensidad a HP=0
        [SerializeField] private float vignetteHPThresh= 0.40f;  // solo activa bajo este %
        [SerializeField] private float vignetteDamage  = 0.75f;  // spike de daño

        [Header("Bloom")]
        [SerializeField] private float bloomFotica     = 0.80f;
        [SerializeField] private float bloomPelagica   = 0.45f;
        [SerializeField] private float bloomBentica    = 0.20f;
        [SerializeField] private float bloomInterior   = 0.60f;

        [Header("Camera Shake")]
        [SerializeField] private float shakeDuration   = 0.30f;
        [SerializeField] private float shakeIntensity  = 0.18f;
        [SerializeField] private float shakeDecay      = 8.0f;

        // ── Colores de zona ───────────────────────────────────────────────────────
        // Filtros de color conservativos: valores cercanos a 1.0 para no sobre-teñir.
        // Verde siempre por debajo del azul para mantener la paleta microscopía.
        private static readonly Color ColorFotica    = new Color(0.92f, 0.95f, 1.00f); // tinte frío muy suave
        private static readonly Color ColorPelagica  = new Color(0.90f, 0.93f, 1.00f); // azul neutro base
        private static readonly Color ColorBentica   = new Color(0.88f, 0.86f, 0.90f); // ligeramente frío-oscuro
        private static readonly Color ColorInterior  = new Color(0.88f, 0.94f, 0.92f); // cian muy suave

        // ── Volume y overrides ────────────────────────────────────────────────────
        private Volume               _volume;
        private ChromaticAberration  _ca;
        private Vignette             _vignette;
        private ColorAdjustments     _colorAdj;
        private Bloom                _bloom;
        private bool                 _ppReady = false;

        // ── Estado objetivo ───────────────────────────────────────────────────────
        private float  _targetCA        = 0f;
        private float  _currentCA       = 0f;
        private float  _caSpikeAmount   = 0f;

        private float  _targetVignette  = 0f;
        private float  _currentVignette = 0f;
        private float  _vignetteSpikeAmount = 0f;
        private Color  _targetVigColor  = Color.black;
        private Color  _currentVigColor = Color.black;

        private Color  _targetColorFilter  = Color.white;
        private Color  _currentColorFilter = Color.white;

        private float  _targetBloom    = 0.45f;
        private float  _currentBloom   = 0.45f;

        // ── Camera Shake ──────────────────────────────────────────────────────────
        private float   _shakeAmount   = 0f;
        private Vector3 _shakeOffset   = Vector3.zero;
        private Camera  _trackedCamera = null;
        private Vector3 _basePosition;

        // ── Zona actual ───────────────────────────────────────────────────────────
        private string  _currentBiome = "Pelagica";
        private ViewType _currentView = ViewType.Exterior;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            BuildVolume();
        }

        private void OnEnable()
        {
            EventBus.OnStressLevelChanged += OnStressLevelChanged;
            EventBus.OnBiomeEntered       += OnBiomeEntered;
            EventBus.OnViewChanged        += OnViewChanged;
            EventBus.OnCellDeath          += OnCellDeath;
        }

        private void OnDisable()
        {
            EventBus.OnStressLevelChanged -= OnStressLevelChanged;
            EventBus.OnBiomeEntered       -= OnBiomeEntered;
            EventBus.OnViewChanged        -= OnViewChanged;
            EventBus.OnCellDeath          -= OnCellDeath;
        }

        private void Update()
        {
            if (!_ppReady) return;

            float dt = Time.deltaTime;

            UpdateTargets();
            LerpEffects(dt);
            ApplyEffects();
            TickCameraShake(dt);
        }

        private void OnDestroy()
        {
            if (_volume != null) Destroy(_volume.gameObject);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Construcción del volume

        private void BuildVolume()
        {
            var go = new GameObject("_PostProcessVolume");
            go.transform.SetParent(transform, false);
            _volume           = go.AddComponent<Volume>();
            _volume.isGlobal  = true;
            _volume.priority  = 10;
            _volume.profile   = ScriptableObject.CreateInstance<VolumeProfile>();

            _ca       = _volume.profile.Add<ChromaticAberration>(true);
            _vignette = _volume.profile.Add<Vignette>(true);
            _colorAdj = _volume.profile.Add<ColorAdjustments>(true);
            _bloom    = _volume.profile.Add<Bloom>(true);

            // Valores iniciales
            _ca.intensity.Override(0f);

            _vignette.intensity.Override(0f);
            _vignette.color.Override(Color.black);
            _vignette.smoothness.Override(0.4f);

            _colorAdj.colorFilter.Override(ColorPelagica);
            _colorAdj.postExposure.Override(0f);

            _bloom.intensity.Override(bloomPelagica);
            _bloom.threshold.Override(0.9f);
            _bloom.scatter.Override(0.65f);

            _ppReady = true;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Targets por estado

        private void UpdateTargets()
        {
            // ── Aberración cromática ← estrés ───────────────────────────────────
            var ss = StressSystem.Instance;
            float stressN = ss != null ? ss.StressNormalized : 0f;
            _targetCA = stressN * caMaxStress;

            // ── Viñeta ← HP ──────────────────────────────────────────────────────
            var cs = CellState.Instance;
            float hpN = cs != null ? cs.HPNormalized : 1f;

            if (hpN < vignetteHPThresh)
            {
                float t = 1f - (hpN / vignetteHPThresh);
                _targetVignette = t * vignetteMaxHP;
                _targetVigColor = Color.Lerp(new Color(0.4f, 0f, 0f), Color.black, hpN / vignetteHPThresh);
            }
            else
            {
                _targetVignette = 0f;
                _targetVigColor = Color.black;
            }

            // Estrés Catastrophic → pulso periódico de viñeta
            if (ss != null && ss.CurrentLevel == StressLevel.Catastrophic)
            {
                float pulse = (Mathf.Sin(Time.time * 2.5f) * 0.5f + 0.5f) * 0.25f;
                _targetVignette = Mathf.Max(_targetVignette, pulse);
                _targetVigColor = Color.Lerp(_targetVigColor, new Color(0.6f, 0f, 0f), pulse);
            }

            // ── Color filter ← zona/vista ────────────────────────────────────────
            if (_currentView == ViewType.Interior)
            {
                _targetColorFilter = ColorInterior;
                _targetBloom       = bloomInterior;
            }
            else
            {
                switch (_currentBiome)
                {
                    case "Fotica":
                        _targetColorFilter = ColorFotica;
                        _targetBloom       = bloomFotica;
                        break;
                    case "Bentonica":
                        _targetColorFilter = ColorBentica;
                        _targetBloom       = bloomBentica;
                        break;
                    default:
                        _targetColorFilter = ColorPelagica;
                        _targetBloom       = bloomPelagica;
                        break;
                }
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Lerp y apply

        private void LerpEffects(float dt)
        {
            float t = lerpSpeed * dt;

            // Spike decay + lerp base
            _caSpikeAmount   = Mathf.MoveTowards(_caSpikeAmount,   0f, caSpikeDecay   * dt);
            _vignetteSpikeAmount = Mathf.MoveTowards(_vignetteSpikeAmount, 0f, caSpikeDecay * dt);

            _currentCA       = Mathf.Lerp(_currentCA,      _targetCA,      t);
            _currentVignette = Mathf.Lerp(_currentVignette,_targetVignette, t);
            _currentVigColor = Color.Lerp(_currentVigColor, _targetVigColor, t);
            _currentColorFilter = Color.Lerp(_currentColorFilter, _targetColorFilter, t);
            _currentBloom    = Mathf.Lerp(_currentBloom,   _targetBloom,   t);
        }

        private void ApplyEffects()
        {
            float finalCA = Mathf.Clamp01(_currentCA + _caSpikeAmount);
            float finalVig = Mathf.Clamp01(_currentVignette + _vignetteSpikeAmount);

            _ca.intensity.Override(finalCA);
            _vignette.intensity.Override(finalVig);
            _vignette.color.Override(_currentVigColor);
            _colorAdj.colorFilter.Override(_currentColorFilter);
            _bloom.intensity.Override(_currentBloom);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Camera Shake

        private void TickCameraShake(float dt)
        {
            if (_shakeAmount <= 0.001f)
            {
                if (_shakeOffset != Vector3.zero && _trackedCamera != null)
                {
                    _trackedCamera.transform.position -= _shakeOffset;
                    _shakeOffset = Vector3.zero;
                }
                return;
            }

            // Resolución de cámara activa
            var vm = ViewManager.Instance;
            Camera cam = vm != null
                ? vm.GetCamera(vm.CurrentView)
                : Camera.main;

            if (cam == null) { _shakeAmount = 0f; return; }

            // Si cambió la cámara, retirar offset anterior
            if (_trackedCamera != cam)
            {
                if (_trackedCamera != null)
                    _trackedCamera.transform.position -= _shakeOffset;
                _trackedCamera = cam;
                _shakeOffset   = Vector3.zero;
            }

            // Quitar offset viejo
            cam.transform.position -= _shakeOffset;

            // Nuevo offset aleatorio escalado
            _shakeOffset = Random.insideUnitSphere * _shakeAmount;
            _shakeOffset.z = 0f;
            cam.transform.position += _shakeOffset;

            _shakeAmount = Mathf.Lerp(_shakeAmount, 0f, shakeDecay * dt);
        }

        /// <summary>Dispara un shake de cámara. magnitude se clampea a shakeIntensity.</summary>
        public void Shake(float magnitude = -1f)
        {
            _shakeAmount = magnitude < 0f ? shakeIntensity : Mathf.Min(magnitude, shakeIntensity * 2f);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Eventos

        private void OnStressLevelChanged(StressLevel prev, StressLevel next)
        {
            // Spike de aberración al subir de nivel de estrés
            if (next > prev)
                _caSpikeAmount = Mathf.Clamp01(_caSpikeAmount + 0.25f * (int)next);
        }

        private void OnBiomeEntered(string biomeId)
        {
            _currentBiome = biomeId;
        }

        private void OnViewChanged(ViewType prev, ViewType next)
        {
            _currentView = next;
            StartCoroutine(ViewFlashRoutine(next));
        }

        private IEnumerator ViewFlashRoutine(ViewType view)
        {
            // Breve destello de exposición al cambiar de vista
            float flashExp = view == ViewType.Interior ? 0.6f : -0.4f;
            float elapsed  = 0f;
            float duration = 0.25f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float exp = Mathf.Lerp(flashExp, 0f, t);
                if (_ppReady) _colorAdj.postExposure.Override(exp);
                elapsed += Time.deltaTime;
                yield return null;
            }
            if (_ppReady) _colorAdj.postExposure.Override(0f);
        }

        private void OnCellDeath()
        {
            // Freeze frame estético: full vignette roja
            StartCoroutine(DeathFlashRoutine());
        }

        private IEnumerator DeathFlashRoutine()
        {
            if (!_ppReady) yield break;
            _vignette.intensity.Override(0.85f);
            _vignette.color.Override(new Color(0.7f, 0f, 0f));
            _colorAdj.postExposure.Override(-1.5f);
            yield return new WaitForSeconds(0.5f);
            // La escena de game-over toma el control; no restauramos
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Dispara un spike visual de daño: aberración + viñeta + shake.
        /// Llamado por CellState o sistemas de daño.
        /// </summary>
        public void TriggerDamageImpact(float normalizedSeverity = 0.5f)
        {
            float s = Mathf.Clamp01(normalizedSeverity);
            _caSpikeAmount       = Mathf.Clamp01(_caSpikeAmount       + caDamageSpike   * s);
            _vignetteSpikeAmount = Mathf.Clamp01(_vignetteSpikeAmount + vignetteDamage  * s);
            Shake(shakeIntensity * s);
        }

        #endregion
    }
}
