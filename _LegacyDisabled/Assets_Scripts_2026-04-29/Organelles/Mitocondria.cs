using System.Collections;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Mitocondria — "La central energética de la célula".
    ///
    /// Produce ATP continuamente. Su eficiencia se ve afectada por:
    ///   - HP del orgánulo (degradación por daño)
    ///   - Ambiente (ROS, temperatura, O2)
    ///   - Modo Turbo (nivel 3): x2 ATP durante 20 seg, luego 40 seg de enfriamiento
    ///
    /// Límite global: máximo 6 mitocondrias simultáneas (biológicamente justificado:
    /// las células tienen un número regulado de mitocondrias para evitar exceso de ROS).
    ///
    /// Mitofagia: cuando HP < 25%, puede ser devorada por un Lisosoma cercano
    /// para recuperar recursos de forma controlada en lugar de explotar.
    /// </summary>
    public class Mitocondria : OrganelleBase
    {
        public static int ActiveCount  { get; private set; } = 0;
        public const  int MaxAllowed   = 6;
        public static int ExtraSlots   = 0;   // Incrementado por el árbol de mejoras

        // ── Producción de ATP por nivel ───────────────────────────────────────────
        private static readonly float[] ATPPerSec = { 10f, 25f, 50f };

        public float CurrentATPRate => ATPPerSec[CurrentLevel - 1] * Efficiency;

        // ── Modo Turbo (nivel 3) ──────────────────────────────────────────────────
        [Header("Modo Turbo (nivel 3)")]
        [SerializeField] private float turboDuration  = 20f;
        [SerializeField] private float turboCooldown  = 40f;
        [SerializeField] private KeyCode turboKey     = KeyCode.T;

        public bool IsTurboActive      { get; private set; } = false;
        public bool IsTurboCoolingDown { get; private set; } = false;
        public float TurboCooldownRemaining { get; private set; } = 0f;

        // ── Mitofagia ─────────────────────────────────────────────────────────────
        /// <summary>True cuando HP < 25% y está lista para ser fagocitada.</summary>
        public bool ReadyForMitophagy => HPPercent < 0.25f && !IsMitophagyInProgress;
        public bool IsMitophagyInProgress { get; private set; } = false;

        // ── Selección ─────────────────────────────────────────────────────────────
        private bool _isSelected = false;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            ActiveCount++;
        }

        private void Start()
        {
            StartCoroutine(ProduceATPLoop());
            EventBus.TriggerOrganelleBuilt(gameObject, OrganelleType);
        }

        protected override void Update()
        {
            base.Update();

            if (TurboCooldownRemaining > 0f)
                TurboCooldownRemaining -= Time.deltaTime;

            // Turbo solo disponible seleccionada y nivel 3
            if (_isSelected && CurrentLevel == 3 &&
                Input.GetKeyDown(turboKey) &&
                !IsTurboActive && !IsTurboCoolingDown)
            {
                StartCoroutine(TurboMode());
            }
        }

        private void OnDestroy()
        {
            ActiveCount = Mathf.Max(0, ActiveCount - 1);
            if (ResourceManager.Instance != null)
                ResourceManager.Instance.UnregisterProductionRate(ResourceType.ATP, CurrentATPRate);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Producción de ATP

        private IEnumerator ProduceATPLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (GameManager.Instance != null &&
                   (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                    continue;

                if (Efficiency <= 0f) continue;

                float rate = ATPPerSec[CurrentLevel - 1] * Efficiency;
                if (IsTurboActive) rate *= 2f;

                ResourceManager.Instance?.AddResource(ResourceType.ATP, rate);

                // Nivel 2+: eficiencia mejorada (representada por mayor producción de ATP)
                if (CurrentLevel >= 2)
                    ResourceManager.Instance?.AddResource(ResourceType.ATP,
                        rate * 0.05f);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Modo Turbo

        private IEnumerator TurboMode()
        {
            IsTurboActive = true;
            Debug.Log("[Mitocondria] Modo Turbo activado — x2 ATP por 20 seg.");

            yield return new WaitForSeconds(turboDuration);

            IsTurboActive      = false;
            IsTurboCoolingDown = true;
            TurboCooldownRemaining = turboCooldown;
            Debug.Log("[Mitocondria] Modo Turbo terminado — enfriamiento 40 seg.");

            yield return new WaitForSeconds(turboCooldown);

            IsTurboCoolingDown     = false;
            TurboCooldownRemaining = 0f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Mitofagia

        /// <summary>
        /// Inicia la mitofagia: el Lisosoma devora la mitocondria dañada
        /// y devuelve el 40% del coste de construcción en recursos.
        /// CANON BIOLÓGICO: la mitofagia es el proceso por el que las células
        /// degradan selectivamente las mitocondrias dañadas para evitar la
        /// acumulación de ROS y mantener la homeostasis energética.
        /// </summary>
        public void StartMitophagy()
        {
            if (IsMitophagyInProgress) return;
            StartCoroutine(MitophagyRoutine());
        }

        private IEnumerator MitophagyRoutine()
        {
            IsMitophagyInProgress = true;
            Debug.Log("[Mitocondria] Mitofagia iniciada — digestión controlada en 2 seg.");

            yield return new WaitForSeconds(2f);

            // Devolver recursos (40% del coste de construcción)
            var (costATP, costProteins) = GetBuildCost();
            ResourceManager.Instance?.AddResource(ResourceType.ATP,    costATP      * 0.4f);
            ResourceManager.Instance?.AddResource(ResourceType.Biomass, costProteins * 0.4f);

            Debug.Log($"[Mitocondria] Mitofagia completa. " +
                      $"+{costATP * 0.4f:F0} ATP / +{costProteins * 0.4f:F0} Proteínas recuperados.");

            EventBus.TriggerOrganelleDestroyed(gameObject, OrganelleType);
            Destroy(gameObject);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region OrganelleBase overrides

        protected override void OnLevelUp()
        {
            Debug.Log($"[Mitocondria] Nivel {CurrentLevel}: producción → {ATPPerSec[CurrentLevel - 1]} ATP/s");
        }

        protected override void OnOrganelleDestroyed()
        {
            // Al ser destruida libera un pulso de ROS que daña orgánulos cercanos
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
            foreach (var hit in hits)
            {
                if (!hit.CompareTag("AllyOrganelle") || hit.gameObject == gameObject) continue;
                var org = hit.GetComponent<OrganelleBase>();
                org?.TakeDamage(10f);
            }
            Debug.Log("[Mitocondria] Destruida — pulso de ROS dañó orgánulos cercanos.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Selección (click)

        private void OnMouseDown() => _isSelected = true;
        private void OnMouseExit()  => _isSelected = false;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Validación de construcción

        /// <summary>Verifica si se puede construir otra mitocondria (límite = 6).</summary>
        public static bool CanBuildNew() => ActiveCount < MaxAllowed + ExtraSlots;

        /// <summary>Añade bonus de NADH extra por tick de producción (nodo árbol de mejoras).</summary>
        public void AddNADHBonusPerTick(float bonus) => _nadhBonusPerTick += bonus;
        private float _nadhBonusPerTick = 0f;

        #endregion
    }
}
