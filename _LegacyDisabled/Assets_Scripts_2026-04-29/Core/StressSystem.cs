using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.Core
{
    /// <summary>
    /// StressSystem — Estrés celular 0-100 (Protogenesis: Primordia, GDD v2).
    ///
    /// Reemplaza gradualmente a InstabilitySystem (v4.6) durante la migración a Primordia.
    /// Ambos sistemas coexisten durante la transición; StressSystem es el canónico.
    ///
    /// 8 factores de estrés (StressFactor enum, definido en EventBus.cs):
    ///   ATPStarvation        — ATP < 20% del máximo           (+15/s)
    ///   AminoAcidStarvation  — AminoAcids < 20% del máximo   (+10/s)
    ///   NucleotideStarvation — Nucleotides < 20% del máximo  (+10/s)
    ///   LipidStarvation      — Lipids < 20% del máximo       (+8/s)
    ///   AcidicEnvironment    — pH < 5.5 ó > 8.5              (+12/s)
    ///   ThermalStress        — temp > 70°C ó < 5°C           (+12/s)
    ///   HypoxicStress        — O2 < 20%                      (+8/s)
    ///   MechanicalDamage     — daño recibido reciente         (+5 por golpe)
    ///
    /// Cada factor activo suma su tasa. Sin factores activos: -5/s (recuperación pasiva).
    ///
    /// 5 umbrales → StressLevel:
    ///   Calm        (0–20)  — sin penalización
    ///   Mild        (21–40) — -10% eficiencia orgánulos
    ///   Moderate    (41–60) — -15% eficiencia, overlay amarillo
    ///   Critical    (61–80) — -30% eficiencia, advertencia apoptosis
    ///   Catastrophic(81-100)— apoptosis inminente (5 s de gracia)
    ///
    /// API pública:
    ///   AddStress(StressFactor, float) — puntual (daño mecánico)
    ///   ReduceStress(float)            — puntual (orgánulo reparador)
    ///   IsFactorActive(StressFactor)   — consulta estado de un factor
    ///   CurrentLevel                   — StressLevel actual
    ///   StressNormalized               — 0-1 para UI
    ///   EfficiencyMultiplier           — penalización actual para orgánulos
    /// </summary>
    public class StressSystem : MonoBehaviour
    {
        public static StressSystem Instance { get; private set; }

        // ── Tasas de estrés por factor (por segundo mientras activo) ─────────────
        [Header("Tasas de estrés por factor (/s)")]
        [SerializeField] private float rateATPStarvation        = 15f;
        [SerializeField] private float rateAminoAcidStarvation  = 10f;
        [SerializeField] private float rateNucleotideStarvation = 10f;
        [SerializeField] private float rateLipidStarvation      =  8f;
        [SerializeField] private float rateAcidicEnvironment    = 12f;
        [SerializeField] private float rateThermalStress        = 12f;
        [SerializeField] private float rateHypoxicStress        =  8f;

        [Header("Recuperación")]
        [Tooltip("Reducción pasiva de estrés por segundo cuando no hay factores activos.")]
        [SerializeField] private float passiveRecoveryRate = 5f;

        [Tooltip("Reducción pasiva base mientras hay factores activos (parcial).")]
        [SerializeField] private float activeRecoveryRate  = 3f;

        [Header("Umbrales de nivel")]
        [SerializeField] private float thresholdMild        = 20f;
        [SerializeField] private float thresholdModerate    = 40f;
        [SerializeField] private float thresholdCritical    = 60f;
        [SerializeField] private float thresholdCatastrophic= 80f;

        [Header("Apoptosis")]
        [Tooltip("Segundos en nivel Catastrophic antes de iniciar apoptosis.")]
        [SerializeField] private float catastrophicGracePeriod = 5f;

        // ── Estado ────────────────────────────────────────────────────────────────
        /// <summary>Valor de estrés actual (0-100).</summary>
        public float Stress { get; private set; } = 0f;

        /// <summary>Estrés normalizado 0-1 para UI.</summary>
        public float StressNormalized => Stress / 100f;

        /// <summary>Nivel de estrés actual.</summary>
        public StressLevel CurrentLevel { get; private set; } = StressLevel.Calm;

        // Estado de los 8 factores
        private readonly Dictionary<StressFactor, bool> _activeFactors
            = new Dictionary<StressFactor, bool>();

        private float _catastrophicTimer = 0f;
        private float _mechanicalDamageDecayTimer = 0f;
        private const float MechanicalDamageWindow = 5f; // segundos de ventana de "daño reciente"
        private bool  _mechanicalDamageActive = false;

        // ── Eficiencia ────────────────────────────────────────────────────────────
        /// <summary>Multiplicador de eficiencia resultante del nivel de estrés.</summary>
        public float EfficiencyMultiplier => CurrentLevel switch
        {
            StressLevel.Calm         => 1.00f,
            StressLevel.Mild         => 0.90f,
            StressLevel.Moderate     => 0.85f,
            StressLevel.Critical     => 0.70f,
            StressLevel.Catastrophic => 0.50f,
            _                        => 1.00f
        };

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;

            foreach (StressFactor f in System.Enum.GetValues(typeof(StressFactor)))
                _activeFactors[f] = false;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                return;

            float dt = Time.deltaTime;

            EvaluateFactors();
            TickStress(dt);
            UpdateLevel();
            CheckConsequences(dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Evaluación de factores

        private void EvaluateFactors()
        {
            var rm  = ResourceManager.Instance;
            var em  = EnvironmentManager.Instance;

            // ── Recursos ──────────────────────────────────────────────────────────
            SetFactor(StressFactor.ATPStarvation,
                rm != null && rm.GetPercent(ResourceType.ATP)         < 0.20f);
            SetFactor(StressFactor.AminoAcidStarvation,
                rm != null && rm.GetPercent(ResourceType.AminoAcids)  < 0.20f);
            SetFactor(StressFactor.NucleotideStarvation,
                rm != null && rm.GetPercent(ResourceType.Nucleotides) < 0.20f);
            SetFactor(StressFactor.LipidStarvation,
                rm != null && rm.GetPercent(ResourceType.Lipids)      < 0.20f);

            // ── Ambiente ──────────────────────────────────────────────────────────
            if (em != null)
            {
                SetFactor(StressFactor.AcidicEnvironment,
                    em.CurrentPH < 5.5f || em.CurrentPH > 8.5f);
                SetFactor(StressFactor.ThermalStress,
                    em.CurrentTemperature > 70f || em.CurrentTemperature < 5f);
                SetFactor(StressFactor.HypoxicStress,
                    em.CurrentO2 < 0.20f);
            }

            // ── Daño mecánico (ventana deslizante de 5 s) ─────────────────────────
            SetFactor(StressFactor.MechanicalDamage, _mechanicalDamageActive);
        }

        private void SetFactor(StressFactor factor, bool active)
        {
            _activeFactors[factor] = active;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Tick de estrés

        private void TickStress(float dt)
        {
            float rate = CalculateNetRate();
            float before = Stress;

            Stress = Mathf.Clamp(Stress + rate * dt, 0f, 100f);

            float delta = Stress - before;
            if (Mathf.Abs(delta) > 0.001f)
                EventBus.TriggerStressChanged(Stress, delta);

            // Ventana de daño mecánico
            if (_mechanicalDamageActive)
            {
                _mechanicalDamageDecayTimer -= dt;
                if (_mechanicalDamageDecayTimer <= 0f)
                {
                    _mechanicalDamageActive = false;
                    _activeFactors[StressFactor.MechanicalDamage] = false;
                }
            }
        }

        private float CalculateNetRate()
        {
            float totalInflow = 0f;

            if (_activeFactors[StressFactor.ATPStarvation])        totalInflow += rateATPStarvation;
            if (_activeFactors[StressFactor.AminoAcidStarvation])  totalInflow += rateAminoAcidStarvation;
            if (_activeFactors[StressFactor.NucleotideStarvation]) totalInflow += rateNucleotideStarvation;
            if (_activeFactors[StressFactor.LipidStarvation])      totalInflow += rateLipidStarvation;
            if (_activeFactors[StressFactor.AcidicEnvironment])    totalInflow += rateAcidicEnvironment;
            if (_activeFactors[StressFactor.ThermalStress])        totalInflow += rateThermalStress;
            if (_activeFactors[StressFactor.HypoxicStress])        totalInflow += rateHypoxicStress;
            // MechanicalDamage es puntual (AddStress), no continuo

            float recovery = totalInflow > 0f ? activeRecoveryRate : passiveRecoveryRate;
            return totalInflow - recovery;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Actualización de nivel

        private void UpdateLevel()
        {
            StressLevel newLevel = Stress switch
            {
                float s when s > thresholdCatastrophic => StressLevel.Catastrophic,
                float s when s > thresholdCritical     => StressLevel.Critical,
                float s when s > thresholdModerate     => StressLevel.Moderate,
                float s when s > thresholdMild         => StressLevel.Mild,
                _                                      => StressLevel.Calm
            };

            if (newLevel != CurrentLevel)
            {
                EventBus.TriggerStressLevelChanged(CurrentLevel, newLevel);
                CurrentLevel = newLevel;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Consecuencias

        private void CheckConsequences(float dt)
        {
            if (CurrentLevel == StressLevel.Catastrophic)
            {
                _catastrophicTimer += dt;
                if (_catastrophicTimer >= catastrophicGracePeriod && GameManager.Instance != null)
                {
                    Debug.Log("[StressSystem] Estrés catastrófico — apoptosis entrópica iniciada.");
                    EventBus.TriggerCellDeath();
                    GameManager.Instance.StartApoptosis();
                    _catastrophicTimer = 0f;
                }
            }
            else
            {
                _catastrophicTimer = 0f;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Añade una cantidad puntual de estrés (ej: daño mecánico de un enemigo).
        /// Activa el factor MechanicalDamage durante <see cref="MechanicalDamageWindow"/> segundos.
        /// </summary>
        public void AddStress(float amount)
        {
            if (amount <= 0f) return;
            Stress = Mathf.Clamp(Stress + amount, 0f, 100f);
            EventBus.TriggerStressChanged(Stress, amount);

            // Activar ventana de daño mecánico
            _mechanicalDamageActive = true;
            _mechanicalDamageDecayTimer = MechanicalDamageWindow;
        }

        /// <summary>
        /// Añade estrés de un factor específico con etiqueta semántica.
        /// Usa <see cref="AddStress(float)"/> para daño mecánico puntual; este
        /// método es para factores que quieren identificarse al llamar.
        /// </summary>
        public void AddStress(StressFactor source, float amount)
        {
            if (source == StressFactor.MechanicalDamage)
            {
                AddStress(amount); // gestiona la ventana deslizante
                return;
            }
            Stress = Mathf.Clamp(Stress + amount, 0f, 100f);
            EventBus.TriggerStressChanged(Stress, amount);
        }

        /// <summary>Reduce estrés puntualmente (orgánulo reparador, habilidad de mejora).</summary>
        public void ReduceStress(float amount)
        {
            if (amount <= 0f) return;
            float before = Stress;
            Stress = Mathf.Max(0f, Stress - amount);
            EventBus.TriggerStressChanged(Stress, Stress - before);
        }

        /// <summary>True si un factor de estrés específico está activo ahora.</summary>
        public bool IsFactorActive(StressFactor factor)
            => _activeFactors.TryGetValue(factor, out bool v) && v;

        /// <summary>True si el estrés está en Calm o Mild (operación normal).</summary>
        public bool IsCalm => CurrentLevel <= StressLevel.Mild;

        /// <summary>True si el estrés es bajo (Calm). Análogo al IsStable de InstabilitySystem.</summary>
        public bool IsStable => CurrentLevel == StressLevel.Calm;

        /// <summary>Número de factores de estrés activos actualmente.</summary>
        public int ActiveFactorCount
        {
            get
            {
                int count = 0;
                foreach (var v in _activeFactors.Values)
                    if (v) count++;
                return count;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Debug

        [ContextMenu("Print Stress State")]
        public void PrintStressState()
        {
            Debug.Log($"[StressSystem] Estrés: {Stress:F1}/100 → {CurrentLevel} | Eficiencia: {EfficiencyMultiplier:P0}");
            foreach (var kvp in _activeFactors)
                if (kvp.Value)
                    Debug.Log($"  · {kvp.Key} — ACTIVO");
        }

        #endregion
    }
}
