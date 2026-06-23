using UnityEngine;
using Protogenesis.Player;

namespace Protogenesis.Core
{
    /// <summary>
    /// BalanceSystem — Sistema de balance anti-abuso (GDD v4.6 §Fase 2).
    ///
    /// Tres sub-sistemas:
    ///
    /// 1. ANTI-STALL (Escalada de combate)
    ///    Cuanto más tiempo lleva un combate sin resolverse, el daño recibido
    ///    aumenta progresivamente (×1.0 → ×2.0 en 60 seg).
    ///    Incentivo: terminar combates rápido o evitarlos.
    ///
    /// 2. PRESIÓN POR REPETICIÓN
    ///    · FarmingDecay: si el jugador permanece >30 seg en la misma zona sin
    ///      avanzar, la producción de recursos de esa zona se reduce gradualmente.
    ///    · PredatorStress: matar al mismo tipo de enemigo repetidamente reduce
    ///      la recompensa de recursos en un 30% (acumulado, se recupera al cambiar).
    ///
    /// 3. DOMINANCIA POSITIVA
    ///    Si se cumplen las 5 condiciones simultáneamente:
    ///      · Zona óptima para el fenotipo actual
    ///      · ATP > 70% del máximo
    ///      · Glucosa > 30% del máximo
    ///      · Inestabilidad < 30
    ///      · Calor metabólico < 30
    ///    → +20% eficiencia global + penalizaciones × 0.5 durante 15 seg.
    ///    Tiene cooldown de 60 seg para evitar abuso.
    /// </summary>
    public class BalanceSystem : MonoBehaviour
    {
        public static BalanceSystem Instance { get; private set; }

        // ── Anti-Stall ────────────────────────────────────────────────────────────
        [Header("Anti-Stall")]
        [Tooltip("Segundos hasta alcanzar el multiplicador de daño máximo.")]
        [SerializeField] private float stallRampTime       = 60f;
        [SerializeField] private float stallMaxMultiplier  = 2.0f;

        private float _combatTimer    = 0f;
        private bool  _inCombat       = false;

        /// <summary>Multiplicador de daño recibido por stall (1.0 = sin penalización).</summary>
        public float StallDamageMultiplier { get; private set; } = 1.0f;

        // ── Presión por Repetición ────────────────────────────────────────────────
        [Header("Presión por Repetición")]
        [SerializeField] private float farmingZoneTimeout  = 30f;
        [SerializeField] private float farmingDecayRate    = 0.01f;  // por seg
        [SerializeField] private float predatorStressCap   = 0.30f;  // máximo 30% reducción

        private float _farmingTimer       = 0f;
        private string _currentZoneId    = "";
        private float _farmingDecay       = 0f;   // 0-1 acumulado

        private string _lastKilledEnemyType = "";
        private int    _repeatKillCount     = 0;

        /// <summary>Multiplicador de recompensa de recursos por farming (1.0 = completo).</summary>
        public float FarmingRewardMultiplier => 1f - _farmingDecay;

        /// <summary>Multiplicador de recompensa por matar al mismo enemigo repetido.</summary>
        public float PredatorRewardMultiplier =>
            1f - Mathf.Min(_repeatKillCount * 0.05f, predatorStressCap);

        // ── Dominancia Positiva ───────────────────────────────────────────────────
        [Header("Dominancia Positiva")]
        [SerializeField] private float positiveDominanceDuration = 15f;
        [SerializeField] private float positiveDominanceCooldown = 60f;
        [SerializeField] private float atpThresholdFraction      = 0.70f;
        [SerializeField] private float glucoseThresholdFraction  = 0.30f;

        private bool  _positiveDominanceActive   = false;
        private float _positiveDominanceTimer    = 0f;
        private float _positiveDominanceCoolTimer= 0f;

        public bool PositiveDominanceActive => _positiveDominanceActive;

        /// <summary>Multiplicador de eficiencia global por Dominancia Positiva (1.2 o 1.0).</summary>
        public float PositiveDominanceEfficiencyBonus => _positiveDominanceActive ? 1.20f : 1.0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnUnitDied += OnUnitDied;
        }

        private void OnDisable()
        {
            EventBus.OnUnitDied -= OnUnitDied;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                return;

            float dt = Time.deltaTime;

            TickAntiStall(dt);
            TickFarmingDecay(dt);
            TickPositiveDominance(dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Anti-Stall

        private void TickAntiStall(float dt)
        {
            if (!_inCombat)
            {
                _combatTimer        = Mathf.Max(0f, _combatTimer - dt * 2f); // recupera fuera de combate
                StallDamageMultiplier = Mathf.Max(1f, StallDamageMultiplier - dt * 0.5f);
                return;
            }

            _combatTimer += dt;
            float t = Mathf.Clamp01(_combatTimer / stallRampTime);
            float cap = Mathf.Min(stallMaxMultiplier, Progression.GeneticFlags.StallMultiplierCap);
            StallDamageMultiplier = Mathf.Lerp(1.0f, cap, t);
        }

        /// <summary>Notifica que el jugador entró en combate.</summary>
        public void NotifyCombatStart() => _inCombat = true;

        /// <summary>Notifica que el combate terminó (todos los enemigos cercanos muertos).</summary>
        public void NotifyCombatEnd()
        {
            _inCombat    = false;
            _combatTimer = 0f;
            StallDamageMultiplier = 1f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Presión por Repetición

        private void TickFarmingDecay(float dt)
        {
            _farmingTimer += dt;

            if (_farmingTimer >= farmingZoneTimeout)
            {
                float rate = farmingDecayRate * Progression.GeneticFlags.FarmingDecayRateMultiplier;
                _farmingDecay = Mathf.Min(1f, _farmingDecay + rate * dt);
            }
        }

        /// <summary>Notifica que el jugador cambió de zona — resetea el farming decay.</summary>
        public void NotifyZoneChanged(string newZoneId)
        {
            if (newZoneId == _currentZoneId) return;
            _currentZoneId = newZoneId;
            _farmingTimer  = 0f;
            _farmingDecay  = Mathf.Max(0f, _farmingDecay - 0.3f); // reducción parcial al moverse
        }

        private void OnUnitDied(UnityEngine.GameObject unit, string unitType)
        {
            if (unitType == _lastKilledEnemyType)
            {
                _repeatKillCount++;
            }
            else
            {
                _lastKilledEnemyType = unitType;
                _repeatKillCount     = 1;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Dominancia Positiva

        private void TickPositiveDominance(float dt)
        {
            // Cooldown
            if (_positiveDominanceCoolTimer > 0f)
            {
                _positiveDominanceCoolTimer -= dt;
                return;
            }

            // Si ya está activo, cuenta el tiempo
            if (_positiveDominanceActive)
            {
                _positiveDominanceTimer -= dt;
                if (_positiveDominanceTimer <= 0f)
                    DeactivatePositiveDominance();
                return;
            }

            // Evaluar las 5 condiciones
            if (CheckPositiveDominanceConditions())
                ActivatePositiveDominance();
        }

        private bool CheckPositiveDominanceConditions()
        {
            var rm   = ResourceManager.Instance;
            var inst = InstabilitySystem.Instance;
            var heat = MetabolicHeatSystem.Instance;
            // TODO: Primordia — var ps   = PhenotypeSystem.Instance;
            object ps = null; // Primordia migration stub

            if (rm == null) return false;

            // Condición 1: Zona óptima — fenotipo conocido (no Unknown)
            bool optimalZone = ps != null; // Primordia stub: se activa cuando ps exista

            // Condición 2: ATP > 70%
            float atp    = rm.GetResource(ResourceType.ATP);
            float atpMax = rm.GetMaxResource(ResourceType.ATP);
            bool  highATP = atpMax > 0f && atp >= atpMax * atpThresholdFraction;

            // Condición 3: Glucosa > 30%
            float gluc    = rm.GetResource(ResourceType.Glucose);
            float glucMax = rm.GetMaxResource(ResourceType.Glucose);
            bool  highGluc = glucMax > 0f && gluc >= glucMax * glucoseThresholdFraction;

            // Condición 4: Inestabilidad < 30
            bool stableInst = inst == null || inst.IsStable;

            // Condición 5: Calor < 30
            bool coolHeat = heat == null || heat.IsCool;

            return optimalZone && highATP && highGluc && stableInst && coolHeat;
        }

        private void ActivatePositiveDominance()
        {
            _positiveDominanceActive = true;
            _positiveDominanceTimer  = positiveDominanceDuration;
            Debug.Log("[BalanceSystem] DOMINANCIA POSITIVA activada — +20% eficiencia, penalizaciones×0.5.");
            EventBus.TriggerPositiveDominanceActivated();
        }

        private void DeactivatePositiveDominance()
        {
            _positiveDominanceActive     = false;
            _positiveDominanceCoolTimer  = positiveDominanceCooldown;
            Debug.Log("[BalanceSystem] Dominancia Positiva expirada — cooldown 60 seg.");
            EventBus.TriggerPositiveDominanceDeactivated();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API

        /// <summary>
        /// Multiplicador combinado de penalización para un hit de daño entrante.
        /// Considera Anti-Stall y Dominancia Positiva.
        /// </summary>
        public float IncomingDamageMultiplier()
        {
            float mult = StallDamageMultiplier;
            if (_positiveDominanceActive) mult *= 0.5f; // penalizaciones reducidas
            return mult;
        }

        #endregion
    }
}
