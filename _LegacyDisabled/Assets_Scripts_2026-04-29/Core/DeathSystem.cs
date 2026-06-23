using System.Collections;
using UnityEngine;
using Protogenesis.Slots;
using Protogenesis.Views;
using Protogenesis.Progression;

namespace Protogenesis.Core
{
    /// <summary>
    /// DeathSystem — 5 tipos de muerte con secuencias visuales (GDD v4.3, Prompt 4.5).
    ///
    /// Lysis: flash rojo, dissolve 0.8s.
    /// Intoxication: tint marrón, fade 3s.
    /// MetabolicCollapse: Time.timeScale→0.3, desaturación, restore.
    /// Apoptosis: fragmentación 4-6 piezas 1.5s.
    /// Freezing: tint azul helado, queda como fósil.
    /// </summary>
    public class DeathSystem : MonoBehaviour
    {
        public static DeathSystem Instance { get; private set; }

        // ── Config ────────────────────────────────────────────────────────────────
        [Header("Umbrales de detección")]
        [SerializeField] private float atpCollapseDelay    = 3f;
        [SerializeField] private float toxinLethalThreshold = 0.8f;
        [SerializeField] private float toxinLethalTime      = 4f;
        [SerializeField] private float apoptosisGraceTime   = 2f;
        [SerializeField] private float freezeTime           = 5f;
        [SerializeField] private float freezingThreshold    = -5f;

        // ── Estado ────────────────────────────────────────────────────────────────
        public bool      IsInCollapse { get; private set; }
        private DeathType _deathType;
        private string    _diagnosis;

        // Timers de condición
        private float _atpTimer;
        private float _toxinTimer;
        private float _apoptosisTimer;
        private float _freezeTimer;

        // Visual overlay
        private float _overlayAlpha;
        private Color _overlayColor;
        private bool  _showOverlay;
        private float _timeScaleTarget = 1f;

        // ─────────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Update()
        {
            if (IsInCollapse) return;

            float dt     = Time.deltaTime;
            var   player = PlayerCellExterior.Instance;
            var   rm     = ResourceManager.Instance;
            var   cs     = CellState.Instance;
            var   mss    = player != null ? player.GetComponent<MembraneSegmentSystem>() : null;

            // Lysis: todos los segmentos rotos
            if (mss != null && mss.BrokenCount >= MembraneSegmentSystem.SegmentCount)
            { TriggerDeath(DeathType.Lysis); return; }

            // MetabolicCollapse: ATP = 0
            if (rm != null && rm.Get(ResourceType.ATP) <= 0f)
            {
                _atpTimer += dt;
                if (_atpTimer >= atpCollapseDelay) { TriggerDeath(DeathType.MetabolicCollapse); return; }
            }
            else _atpTimer = 0f;

            // Intoxication
            var cgs = ChemicalGradientSystem.Instance;
            if (cgs != null && player != null)
            {
                float toxin = cgs.ToxinAt(player.transform.position);
                if (toxin >= toxinLethalThreshold)
                {
                    _toxinTimer += dt;
                    if (_toxinTimer >= toxinLethalTime) { TriggerDeath(DeathType.Intoxication); return; }
                }
                else _toxinTimer = 0f;
            }

            // Apoptosis: HP < 10% y ATP < 10%
            if (cs != null && rm != null)
            {
                float hpN   = cs.HPNormalized;
                float atpN  = rm.Get(ResourceType.ATP) / Mathf.Max(1f, rm.GetMaxResource(ResourceType.ATP));
                if (hpN < 0.10f && atpN < 0.10f)
                {
                    _apoptosisTimer += dt;
                    if (_apoptosisTimer >= apoptosisGraceTime) { TriggerDeath(DeathType.Apoptosis); return; }
                }
                else _apoptosisTimer = 0f;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        public void TriggerDeath(DeathType type)
        {
            if (IsInCollapse) return;
            IsInCollapse = true;
            _deathType   = type;
            StartCoroutine(DeathSequence(type));
        }

        public string LastDiagnosis => _diagnosis;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Secuencias

        private IEnumerator DeathSequence(DeathType type)
        {
            EventBus.TriggerMotherDied(type);

            switch (type)
            {
                case DeathType.Lysis:             yield return StartCoroutine(LysisSequence());             break;
                case DeathType.Intoxication:      yield return StartCoroutine(IntoxicationSequence());      break;
                case DeathType.MetabolicCollapse: yield return StartCoroutine(MetabolicCollapseSequence()); break;
                case DeathType.Apoptosis:         yield return StartCoroutine(ApoptosisSequence());         break;
                case DeathType.Freezing:          yield return StartCoroutine(FreezingSequence());          break;
            }

            OnDeathComplete();
        }

        private IEnumerator LysisSequence()
        {
            _diagnosis = "Membrana perforada. Girá para proteger los segmentos dañados.";
            _overlayColor = new Color(0.9f, 0.1f, 0.1f);
            _showOverlay  = true;

            // Flash rojo intenso → dissolve en 0.8s
            float elapsed = 0f;
            while (elapsed < 0.8f)
            {
                elapsed += Time.deltaTime;
                _overlayAlpha = Mathf.Sin(elapsed / 0.8f * Mathf.PI) * 0.7f;
                yield return null;
            }
            _showOverlay = false;
        }

        private IEnumerator IntoxicationSequence()
        {
            _diagnosis = "Toxinas no expulsadas. Instalá Peroxisoma.";
            _overlayColor = new Color(0.35f, 0.25f, 0.10f);
            _showOverlay  = true;

            // Oscurecimiento progresivo (tint marrón) en 3s
            float elapsed = 0f;
            while (elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                _overlayAlpha = Mathf.Lerp(0f, 0.75f, elapsed / 3f);
                yield return null;
            }
            _showOverlay = false;
        }

        private IEnumerator MetabolicCollapseSequence()
        {
            _diagnosis = "Sin ATP. Instalá Mitocondria primero.";
            _overlayColor = new Color(0.15f, 0.15f, 0.15f);
            _showOverlay  = true;

            // Ralentizar tiempo a 0.3× en 3s
            float elapsed = 0f;
            while (elapsed < 3f)
            {
                elapsed += Time.deltaTime;
                Time.timeScale = Mathf.Lerp(1f, 0.3f, elapsed / 3f);
                _overlayAlpha  = Mathf.Lerp(0f, 0.55f, elapsed / 3f);
                yield return null;
            }

            yield return new WaitForSecondsRealtime(0.5f);

            // Restaurar timescale
            Time.timeScale = 1f;
            _showOverlay   = false;
        }

        private IEnumerator ApoptosisSequence()
        {
            _diagnosis = "NK aprendió tu firma. Pivoteá.";
            _overlayColor = new Color(0.3f, 0.5f, 0.85f);
            _showOverlay  = true;

            // Fragmentación visual: encogimiento en 1.5s
            var player = PlayerCellExterior.Instance;
            if (player != null)
            {
                float elapsed = 0f;
                Vector3 startScale = player.transform.localScale;
                while (elapsed < 1.5f)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed / 1.5f;
                    player.transform.localScale = startScale * (1f - t * 0.8f);
                    _overlayAlpha = t * 0.5f;
                    yield return null;
                }
            }
            _showOverlay = false;
        }

        private IEnumerator FreezingSequence()
        {
            _diagnosis = "Frío letal. El Hongo puede farmearte.";
            _overlayColor = new Color(0.7f, 0.85f, 1.0f);
            _showOverlay  = true;

            // Tint azul helado progresivo — NO se destruye
            float elapsed = 0f;
            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                _overlayAlpha = Mathf.Lerp(0f, 0.45f, elapsed / 2f);
                yield return null;
            }
            // Queda como fósil: no destruye el GO
            IsInCollapse = false; // permite que el fósil persista sin trigger loop
            yield break;          // skipea OnDeathComplete
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        private void OnDeathComplete()
        {
            EventBus.TriggerMatchEnd(playerWon: false);
            EventBus.TriggerPowerMoment("elimination");

            var player = PlayerCellExterior.Instance;
            if (player != null && _deathType != DeathType.Freezing)
                Destroy(player.gameObject, 0.1f);

            Debug.Log($"[DeathSystem] Muerte: {_deathType} — {_diagnosis}");
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI — overlay

        private void OnGUI()
        {
            if (!_showOverlay || _overlayAlpha <= 0.01f) return;

            GUI.color = new Color(_overlayColor.r, _overlayColor.g, _overlayColor.b, _overlayAlpha);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            if (!string.IsNullOrEmpty(_diagnosis) && _overlayAlpha > 0.3f)
            {
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                };
                style.normal.textColor = new Color(1f, 1f, 1f, Mathf.Clamp01(_overlayAlpha * 2f));
                GUI.Label(new Rect(Screen.width * 0.5f - 200f, Screen.height * 0.5f - 15f, 400f, 30f),
                    _diagnosis, style);
            }
        }

        #endregion
    }
}
