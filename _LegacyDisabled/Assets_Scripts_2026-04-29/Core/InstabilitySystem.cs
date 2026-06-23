using UnityEngine;
using Protogenesis.Slots;

namespace Protogenesis.Core
{
    /// <summary>
    /// InstabilitySystem — Inestabilidad celular 0-100 (GDD v4.6 §Fase 2).
    ///
    /// DEPRECATED — Primordia: reemplazado por <see cref="StressSystem"/> (Prompt 0.4).
    /// Se mantiene activo para no romper las referencias existentes.
    /// AddInstability() y ReduceInstability() delegan en StressSystem cuando está disponible.
    ///
    /// Anti-meta: penaliza la sobre-complejidad y recompensa la especialización.
    ///
    /// Fuentes de entropía (aumentan Instabilidad):
    ///   · Ramas activas diversas: (activeBranches² × 0.15) / seg
    ///   · Deuda de adaptación > 0 genera entropía al 30% de la tasa normal
    ///   · Daño recibido: +2 por golpe (aplicado externamente vía AddInstability)
    ///
    /// Reducción de entropía:
    ///   · Especialización (IdentityThreshold = 2 ó 3): -0.5/seg
    ///   · Fenotipo estable ≥ 60 seg: -0.3/seg adicional
    ///   · Zona óptima para el fenotipo actual: -0.2/seg adicional
    ///
    /// Consecuencias:
    ///   · >50: los orgánulos reciben –15% eficiencia global
    ///   · >75: inicio de Apoptosis si se mantiene >10 seg (aviso de caos celular)
    ///   · =100: Game Over por colapso celular (apoptosis entrópica)
    /// </summary>
    public class InstabilitySystem : MonoBehaviour
    {
        public static InstabilitySystem Instance { get; private set; }

        [Header("Valores base")]
        [SerializeField] private float initialInstability  = 0f;
        [SerializeField] private float apoptosisThreshold  = 75f;
        [SerializeField] private float apoptosisDelay      = 10f;   // seg antes de activar apoptosis
        [SerializeField] private float collapseThreshold   = 100f;

        [Header("Tasas de cambio (por segundo)")]
        [SerializeField] private float branchEntropyFactor    = 0.15f; // multiplicado por ramas²
        [SerializeField] private float debtEntropyFraction    = 0.30f; // % de tasa normal durante deuda
        [SerializeField] private float specialReduction       = 0.5f;
        [SerializeField] private float stableFenotypeReduction= 0.3f;
        [SerializeField] private float optimalZoneReduction   = 0.2f;

        // ── Estado ───────────────────────────────────────────────────────────────
        public float Instability { get; private set; } = 0f;

        /// <summary>Instabilidad normalizada 0-1 (para UI).</summary>
        public float InstabilityNormalized => Instability / collapseThreshold;

        private float _apoptosisTimer  = 0f;
        private bool  _apoptosisWarned = false;

        // ── Eficiencia global resultante ──────────────────────────────────────────
        /// <summary>Multiplicador de eficiencia por inestabilidad (1.0 = sin penalización).</summary>
        public float EfficiencyMultiplier =>
            Instability > 50f ? 0.85f : 1.0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            Instability = initialInstability;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                return;

            float dt = Time.deltaTime;

            // Entropía por diversidad de ramas
            float entropy = CalculateEntropyRate() * dt;

            // Reducción por especialización y estabilidad
            float reduction = CalculateReductionRate() * dt;

            Instability = Mathf.Clamp(Instability + entropy - reduction, 0f, collapseThreshold);

            CheckConsequences(dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Cálculo

        private float CalculateEntropyRate()
        {
            var sm     = SlotManager.Instance;
            var entity = Slots.CellEntity.Instance;

            int activeBranches = CountActiveBranches(sm);
            float entropyFromBranches = (activeBranches * activeBranches) * branchEntropyFactor;

            // groupEntropy GDD v5.2 §13.2: unitCount * 0.1f por unidad colonial
            float groupEntropy = CalculateGroupEntropy();

            // Durante deuda de adaptación, la tasa es 30% de la normal
            float debtFactor = 1f;
            if (entity != null && entity.AdaptationDebt > 0f)
                debtFactor = debtEntropyFraction * Progression.GeneticFlags.AdaptationDebtMultiplier;

            // GeneticFlags: InstabilityRateMultiplier reduce la tasa global
            return (entropyFromBranches + groupEntropy) * debtFactor
                   * Progression.GeneticFlags.InstabilityRateMultiplier;
        }

        /// <summary>
        /// groupEntropy (GDD v5.2 §13.2): cada unidad colonial suma 0.1/s de entropía.
        /// Grupos > 20 unidades: coordinationLoss adicional de (unitCount-20)*0.05/s.
        /// Composición Óptima (4 roles presentes): -25% groupEntropy.
        /// HomeostasisNode Nv2: -40% groupEntropy.
        /// </summary>
        private static float CalculateGroupEntropy()
        {
            var rs = Units.ReproductionSystem.Instance;
            if (rs == null) return 0f;

            int unitCount = rs.ColonyCount;
            if (unitCount == 0) return 0f;

            float baseEntropy         = unitCount * 0.1f;
            float coordinationPenalty = unitCount > 20
                ? (unitCount - 20) * 0.05f
                : 0f;

            float total = baseEntropy + coordinationPenalty;

            // HomeostasisNode Nv2 proxy: si DamageReduction alta, asumimos homeostasis activa
            var entity = Slots.CellEntity.Instance;
            if (entity != null && entity.DamageReduction >= 0.3f)
                total *= 0.60f; // -40%

            return total;
        }

        private float CalculateReductionRate()
        {
            var sm = SlotManager.Instance;
            // TODO: Primordia — var ps = Player.PhenotypeSystem.Instance;
            object ps = null; // Primordia migration stub

            float reduction = 0f;

            // Especialización: IdentityThreshold 2 ó 3
            if (sm != null && sm.IdentityThreshold >= 2)
                reduction += specialReduction;

            // Fenotipo estable ≥ 60 seg
            // TODO: Primordia — if (ps != null && ps.PhenotypeSurvivalTime >= 60f
                           // TODO: Primordia — && ps.CurrentPhenotype != Player.PhenotypeType.Unknown)
                reduction += stableFenotypeReduction;

            // Zona óptima (placeholder — ZoneSystem lo informará en Fase 3)
            // Por ahora: si la eficiencia ambiental es alta, cuenta como zona óptima
            var env = EnvironmentManager.Instance;
            if (env != null && env.GlobalEnvironmentModifier >= 0.85f)
                reduction += optimalZoneReduction;

            // Fenotipo Colonial: reduce inestabilidad adicional (nodo árbol genético)
            // TODO: Primordia — if (ps != null && ps.CurrentPhenotype == Player.PhenotypeType.BiofilmColonial)
                reduction += Progression.GeneticFlags.ColonyInstabilityReduction;

            return reduction;
        }

        private static int CountActiveBranches(SlotManager sm)
        {
            if (sm == null) return 0;
            int count = 0;
            foreach (SlotType st in System.Enum.GetValues(typeof(SlotType)))
                if (sm.GetUsedSlots(st) > 0) count++;
            return count;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Consecuencias

        private void CheckConsequences(float dt)
        {
            if (Instability >= collapseThreshold)
            {
                Debug.Log("[InstabilitySystem] COLAPSO CELULAR — Inestabilidad 100.");
                GameManager.Instance?.GameOver();
                return;
            }

            if (Instability >= apoptosisThreshold)
            {
                _apoptosisTimer += dt;
                if (!_apoptosisWarned)
                {
                    _apoptosisWarned = true;
                    Debug.LogWarning($"[InstabilitySystem] Inestabilidad crítica ({Instability:F0}) — apoptosis en {apoptosisDelay}s si no se reduce.");
                }

                if (_apoptosisTimer >= apoptosisDelay)
                {
                    Debug.Log("[InstabilitySystem] Apoptosis entrópica iniciada.");
                    GameManager.Instance?.StartApoptosis();
                    _apoptosisTimer = 0f;
                }
            }
            else
            {
                _apoptosisTimer  = 0f;
                _apoptosisWarned = false;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>Añade inestabilidad directamente (ej: daño recibido, evento externo).</summary>
        public void AddInstability(float amount)
        {
            Instability = Mathf.Clamp(Instability + amount, 0f, collapseThreshold);
            // Primordia: también notificar a StressSystem
            StressSystem.Instance?.AddStress(StressFactor.MechanicalDamage, amount * 0.5f);
        }

        /// <summary>Reduce inestabilidad directamente (ej: consumir GenomicPoints en árbol).</summary>
        public void ReduceInstability(float amount)
        {
            Instability = Mathf.Max(0f, Instability - amount);
            // Primordia: también notificar a StressSystem
            StressSystem.Instance?.ReduceStress(amount * 0.5f);
        }

        /// <summary>True si la instabilidad está en rango seguro para Dominancia Positiva.</summary>
        public bool IsStable => Instability < 30f;

        #endregion
    }
}
