using UnityEngine;

namespace Protogenesis.Core
{
    /// <summary>
    /// MetabolicHeatSystem — Calor metabólico 0-100 (GDD v4.6 §Fase 2).
    ///
    /// Anti-loop para la mutación de Eficiencia Térmica: produce calor a medida
    /// que la célula trabaja y requiere disipación activa o pasiva.
    ///
    /// Fuentes de calor (aumentan HeatLevel):
    ///   · Producción de ATP activa: +0.3/seg base (escalado por ruta metabólica)
    ///   · Turbo de Mitocondria activo: +2.5/seg adicional
    ///   · Temperatura ambiental alta: +1.0/seg si EnvironmentManager.Temperature > umbral
    ///
    /// Disipación (reducen HeatLevel):
    ///   · Natural: -0.5/seg siempre
    ///   · Zona Béntica: -1.5/seg adicional (más fría estratégicamente)
    ///   · Slot de Protección con aislamiento térmico: -1.0/seg (aplicado externamente)
    ///
    /// Consecuencias:
    ///   · >50: –10% eficiencia de Mitocondria y Ribosoma
    ///   · >75: orgánulos cercanos reciben daño pasivo (0.5 HP/seg)
    ///   · =100: destrucción de orgánulos aleatorios (evento catastrófico)
    /// </summary>
    public class MetabolicHeatSystem : MonoBehaviour
    {
        public static MetabolicHeatSystem Instance { get; private set; }

        [Header("Calor")]
        [SerializeField] private float initialHeat         = 0f;
        [SerializeField] private float maxHeat             = 100f;
        [SerializeField] private float baseHeatPerSec      = 0.3f;
        [SerializeField] private float turboHeatPerSec     = 2.5f;
        [SerializeField] private float ambientHeatPerSec   = 1.0f;
        [SerializeField] private float ambientTempThreshold= 60f;   // Temperatura EnvironmentManager

        [Header("Disipación")]
        [SerializeField] private float naturalDissipation  = 0.5f;  // /seg siempre
        [SerializeField] private float benthicDissipation  = 1.5f;  // /seg en zona béntica

        [Header("Umbrales de consecuencia")]
        [SerializeField] private float warningThreshold    = 50f;
        [SerializeField] private float dangerThreshold     = 75f;
        [SerializeField] private float damageTickInterval  = 1f;    // seg entre daños

        // ── Estado ───────────────────────────────────────────────────────────────
        public float HeatLevel { get; private set; } = 0f;

        /// <summary>Calor normalizado 0-1 (para UI).</summary>
        public float HeatNormalized => HeatLevel / maxHeat;

        // Modificador externo de disipación (slots, upgrades)
        private float _extraDissipation = 0f;

        // Zona béntica activa (informada por ZoneSystem en Fase 3)
        public bool IsInBenthicZone { get; set; } = false;

        private float _damageTimer = 0f;
        private bool  _catastropheTriggered = false;

        // ── Multiplicador de eficiencia ───────────────────────────────────────────
        /// <summary>Penalización de eficiencia por calor (1.0 = sin penalización).</summary>
        public float EfficiencyMultiplier =>
            HeatLevel > warningThreshold ? 0.90f : 1.0f;

        /// <summary>True si el calor está en rango seguro para Dominancia Positiva.</summary>
        public bool IsCool => HeatLevel < 30f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            HeatLevel = initialHeat;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                return;

            float dt = Time.deltaTime;

            float gain = CalculateHeatGain() * dt;
            float diss = CalculateDissipation() * dt;

            HeatLevel = Mathf.Clamp(HeatLevel + gain - diss, 0f, maxHeat);

            CheckConsequences(dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Cálculo

        private float CalculateHeatGain()
        {
            var rm  = ResourceManager.Instance;
            var me  = MetabolismEngine.Instance;
            var env = EnvironmentManager.Instance;

            float heat = 0f;

            // Calor base por producción activa (escalado por ruta)
            if (rm != null)
            {
                float routeMult = me != null ? me.ATPRouteMultiplier : 1f;
                heat += baseHeatPerSec * routeMult;
            }

            // Turbo de mitocondria (detectado por tasa elevada)
            // Las Mitocondrias en Turbo elevan la tasa de producción — inferido aquí
            // TODO: Mitocondria.IsTurboActive puede consultarse directamente en Fase 3
            if (me != null && me.ATPRouteMultiplier >= 2f)
                heat += turboHeatPerSec;

            // Temperatura ambiental alta
            if (env != null && env.CurrentTemperature > ambientTempThreshold)
                heat += ambientHeatPerSec;

            return heat * Progression.GeneticFlags.HeatGenerationMultiplier;
        }

        private float CalculateDissipation()
        {
            float diss = naturalDissipation;

            if (IsInBenthicZone)
                diss += benthicDissipation;

            diss += _extraDissipation;

            return diss;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Consecuencias

        private void CheckConsequences(float dt)
        {
            if (HeatLevel >= maxHeat && !_catastropheTriggered)
            {
                _catastropheTriggered = true;
                TriggerCatastrophe();
                return;
            }

            if (HeatLevel < maxHeat) _catastropheTriggered = false;

            if (HeatLevel >= dangerThreshold)
            {
                _damageTimer += dt;
                if (_damageTimer >= damageTickInterval)
                {
                    _damageTimer = 0f;
                    DamageNearbyOrganelles(0.5f);
                }
            }
            else
            {
                _damageTimer = 0f;
            }
        }

        private static void DamageNearbyOrganelles(float amount)
        {
            // TODO: Primordia — var cap = Player.CAP.Instance;
            object cap = null; // Primordia migration stub
            if (cap == null) return;

            // TODO: Primordia — var hits = Physics2D.OverlapCircleAll(cap.transform.position, 5f);
            Collider2D[] hits = System.Array.Empty<Collider2D>(); // Primordia stub
            foreach (var hit in hits)
            {
                if (!hit.CompareTag("AllyOrganelle")) continue;
                hit.GetComponent<Organelles.OrganelleBase>()?.TakeDamage(amount);
            }
        }

        private void TriggerCatastrophe()
        {
            Debug.LogWarning("[MetabolicHeatSystem] CATÁSTROFE TÉRMICA — destruyendo orgánulos aleatorios.");

            var all = UnityEngine.Object.FindObjectsByType<Organelles.OrganelleBase>(
                UnityEngine.FindObjectsSortMode.None);

            // Destruye hasta 2 orgánulos al azar
            int destroyed = 0;
            foreach (var org in all)
            {
                if (destroyed >= 2) break;
                org.TakeDamage(org.CurrentHP); // mata instantáneamente
                destroyed++;
            }

            // Resetea calor tras catástrofe
            HeatLevel = maxHeat * 0.5f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Añade disipación extra permanente (ej: slot de aislamiento térmico, upgrade).
        /// </summary>
        public void AddExtraDissipation(float amount) => _extraDissipation += amount;

        /// <summary>Añade calor directamente (ej: explosión de ROS, efecto enemigo).</summary>
        public void AddHeat(float amount)
        {
            HeatLevel = Mathf.Clamp(HeatLevel + amount, 0f, maxHeat);
        }

        #endregion
    }
}
