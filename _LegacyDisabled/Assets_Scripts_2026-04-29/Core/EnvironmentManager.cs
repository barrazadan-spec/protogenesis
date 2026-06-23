using UnityEngine;

namespace Protogenesis.Core
{
    /// <summary>
    /// Gestiona las variables ambientales del entorno celular: pH, O2 y temperatura.
    ///
    /// CANON BIOLÓGICO:
    ///   - pH ácido (~5) desnaturaliza enzimas → orgánulos pierden eficiencia.
    ///   - Falta de O2 fuerza la fermentación → ATP producido cae drásticamente.
    ///   - Temperatura alta desnaturaliza proteínas → ribosomas fallan.
    ///
    /// Este manager aplica modificadores de eficiencia globales que otros sistemas
    /// consultan. Los valores pueden ser alterados por enemigos (Toxinas, Priones),
    /// por el ambiente del mapa (ScriptableObject de escenario) o por mejoras.
    /// </summary>
    public class EnvironmentManager : MonoBehaviour
    {
        public static EnvironmentManager Instance { get; private set; }

        // ── Valores iniciales (fisiológicos neutros) ──────────────────────────────
        [Header("pH (0-14, neutro = 7.0)")]
        [SerializeField, Range(0f, 14f)] private float initialPH   = 7.0f;
        [SerializeField, Range(0f, 14f)] private float minSafePH   = 5.5f;
        [SerializeField, Range(0f, 14f)] private float maxSafePH   = 8.5f;

        [Header("Oxígeno (0-1, normal = 1.0)")]
        [SerializeField, Range(0f, 1f)]  private float initialO2   = 1.0f;
        [SerializeField, Range(0f, 1f)]  private float lowO2Threshold = 0.2f;

        [Header("Temperatura (0-100, normal = 37)")]
        [SerializeField, Range(0f, 100f)] private float initialTemp = 37f;
        [SerializeField, Range(0f, 100f)] private float highTempThreshold = 70f;

        // ── Estado actual ─────────────────────────────────────────────────────────
        public float CurrentPH          { get; private set; }
        public float CurrentO2          { get; private set; }
        public float CurrentTemperature { get; private set; }

        // ── Modificadores calculados (0-1, donde 1 = sin penalización) ───────────
        /// <summary>Penalización de eficiencia por pH fuera de rango seguro.</summary>
        public float PHEfficiencyModifier          { get; private set; } = 1f;

        /// <summary>Penalización de eficiencia por falta de oxígeno.</summary>
        public float OxygenEfficiencyModifier      { get; private set; } = 1f;

        /// <summary>Penalización de eficiencia por temperatura alta.</summary>
        public float TemperatureEfficiencyModifier { get; private set; } = 1f;

        /// <summary>Modificador global combinado de todos los factores ambientales.</summary>
        public float GlobalEnvironmentModifier =>
            Mathf.Min(1f, PHEfficiencyModifier * OxygenEfficiencyModifier * TemperatureEfficiencyModifier
                          + Progression.GeneticFlags.EnvironmentToleranceBonus);

        // ── Flags de modo de ruta metabólica ─────────────────────────────────────
        /// <summary>True cuando el O2 está bajo el umbral y se activa la fermentación.</summary>
        public bool IsFermentationActive { get; private set; } = false;

        // ── Rate de cambio natural (el ambiente se estabiliza solo con el tiempo) ──
        [Header("Recuperación natural")]
        [SerializeField] private float phRecoveryRate   = 0.05f; // pH/seg hacia neutro
        [SerializeField] private float o2RecoveryRate   = 0.01f; // O2/seg hacia 1.0
        [SerializeField] private float tempRecoveryRate = 0.5f;  // grados/seg hacia 37

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            CurrentPH          = initialPH;
            CurrentO2          = initialO2;
            CurrentTemperature = initialTemp;

            RecalculateModifiers();
        }

        private void Update()
        {
            if (Instance != this) return;
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                return;

            float dt = Time.deltaTime;
            NaturalRecovery(dt);
            RecalculateModifiers();
            CheckHazards();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Recuperación natural

        private void NaturalRecovery(float dt)
        {
            // El pH tiende al valor inicial (fisiológico)
            CurrentPH = Mathf.MoveTowards(CurrentPH, initialPH, phRecoveryRate * dt);

            // El O2 se recupera lentamente (difusión)
            CurrentO2 = Mathf.MoveTowards(CurrentO2, 1f, o2RecoveryRate * dt);

            // La temperatura baja hacia los 37°C
            CurrentTemperature = Mathf.MoveTowards(CurrentTemperature, initialTemp, tempRecoveryRate * dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Modificadores

        private void RecalculateModifiers()
        {
            PHEfficiencyModifier          = CalculatePHModifier(CurrentPH);
            OxygenEfficiencyModifier      = CalculateO2Modifier(CurrentO2);
            TemperatureEfficiencyModifier = CalculateTempModifier(CurrentTemperature);

            bool shouldFerment = CurrentO2 <= lowO2Threshold;
            if (shouldFerment != IsFermentationActive)
            {
                IsFermentationActive = shouldFerment;
                if (shouldFerment)
                    Debug.Log("[Environment] O2 crítico — Fermentación activada (25% eficiencia ATP)");
            }
        }

        /// <summary>
        /// Enzimas tienen una curva de eficiencia con pico en pH 7.
        /// Por debajo de 5.5 o encima de 8.5 la actividad enzimática cae.
        /// </summary>
        private float CalculatePHModifier(float ph)
        {
            if (ph >= minSafePH && ph <= maxSafePH) return 1f;

            float distanceFromSafe = ph < minSafePH
                ? minSafePH - ph
                : ph - maxSafePH;

            // Cada unidad de pH fuera del rango seguro reduce un 15% la eficiencia
            return Mathf.Clamp01(1f - distanceFromSafe * 0.15f);
        }

        /// <summary>
        /// Sin oxígeno la cadena respiratoria falla. Por debajo del 20% O2
        /// se activa fermentación (25% de eficiencia base).
        /// </summary>
        private float CalculateO2Modifier(float o2)
        {
            if (o2 >= lowO2Threshold) return 1f;
            // Interpolación: 20% O2 = 1.0 eficiencia, 0% O2 = 0.25 (fermentación)
            // El bonus de fermentación sube el piso mínimo de 0.25 hasta 0.55 como máximo.
            float minEff = Mathf.Lerp(0.25f, 0.55f, _fermentationEfficiencyBonus);
            return Mathf.Lerp(minEff, 1f, o2 / lowO2Threshold);
        }

        /// <summary>
        /// Proteínas se desnaturalizan por encima de 70°C.
        /// Por encima de 90°C colapso total.
        /// </summary>
        private float CalculateTempModifier(float temp)
        {
            if (temp <= highTempThreshold) return 1f;
            if (temp >= 90f) return 0f;
            return Mathf.Lerp(1f, 0f, (temp - highTempThreshold) / (90f - highTempThreshold));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Detección de peligros ambientales

        private bool _acidWarningActive    = false;
        private bool _alkaliWarningActive  = false;
        private bool _lowO2WarningActive   = false;
        private bool _highTempWarning      = false;

        private void CheckHazards()
        {
            // pH ácido
            if (CurrentPH < minSafePH && !_acidWarningActive)
            {
                EventBus.TriggerEnvironmentalHazard(EnvironmentalHazard.AcidicPH, CurrentPH);
                _acidWarningActive = true;
            }
            else if (CurrentPH >= minSafePH) _acidWarningActive = false;

            // pH alcalino
            if (CurrentPH > maxSafePH && !_alkaliWarningActive)
            {
                EventBus.TriggerEnvironmentalHazard(EnvironmentalHazard.AlkalinePH, CurrentPH);
                _alkaliWarningActive = true;
            }
            else if (CurrentPH <= maxSafePH) _alkaliWarningActive = false;

            // Bajo O2
            if (CurrentO2 < lowO2Threshold && !_lowO2WarningActive)
            {
                EventBus.TriggerEnvironmentalHazard(EnvironmentalHazard.LowOxygen, CurrentO2);
                _lowO2WarningActive = true;
            }
            else if (CurrentO2 >= lowO2Threshold) _lowO2WarningActive = false;

            // Alta temperatura
            if (CurrentTemperature > highTempThreshold && !_highTempWarning)
            {
                EventBus.TriggerEnvironmentalHazard(EnvironmentalHazard.HighTemperature, CurrentTemperature);
                _highTempWarning = true;
            }
            else if (CurrentTemperature <= highTempThreshold) _highTempWarning = false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>Modifica el pH del entorno (positivo = más ácido, negativo = más alcalino).</summary>
        public void AlterPH(float delta)
        {
            float previous = CurrentPH;
            CurrentPH = Mathf.Clamp(CurrentPH + delta, 0f, 14f);
            if (!Mathf.Approximately(previous, CurrentPH))
                EventBus.TriggerPHChanged(CurrentPH);
        }

        /// <summary>Modifica el nivel de O2 (negativo = consumo, positivo = aporte).</summary>
        public void AlterOxygen(float delta)
        {
            float previous = CurrentO2;
            CurrentO2 = Mathf.Clamp01(CurrentO2 + delta);
            if (!Mathf.Approximately(previous, CurrentO2))
                EventBus.TriggerOxygenChanged(CurrentO2);
        }

        /// <summary>Modifica la temperatura del entorno.</summary>
        public void AlterTemperature(float delta)
        {
            float previous = CurrentTemperature;
            CurrentTemperature = Mathf.Clamp(CurrentTemperature + delta, 0f, 100f);
            if (!Mathf.Approximately(previous, CurrentTemperature))
                EventBus.TriggerTemperatureChanged(CurrentTemperature);
        }

        /// <summary>
        /// Fuerza los valores ambientales a los de un ScriptableObject de escenario.
        /// Llamar al cargar un mapa nuevo.
        /// </summary>
        public void ApplyScenarioEnvironment(float ph, float o2, float temp)
        {
            CurrentPH          = Mathf.Clamp(ph,   0f, 14f);
            CurrentO2          = Mathf.Clamp01(o2);
            CurrentTemperature = Mathf.Clamp(temp, 0f, 100f);
            RecalculateModifiers();

            EventBus.TriggerPHChanged(CurrentPH);
            EventBus.TriggerOxygenChanged(CurrentO2);
            EventBus.TriggerTemperatureChanged(CurrentTemperature);
        }

        /// <summary>
        /// Mejora la eficiencia de la fermentación (nodo de árbol de mejoras).
        /// Sube el modificador mínimo de O2 para que la fermentación rinda más.
        /// </summary>
        public void BoostFermentationEfficiency(float bonus)
        {
            // Se guarda como ajuste al cálculo de OxygenEfficiencyModifier mínimo.
            // El bonus reduce el impacto negativo cuando O2 está bajo.
            _fermentationEfficiencyBonus = Mathf.Clamp01(_fermentationEfficiencyBonus + bonus);
        }

        private float _fermentationEfficiencyBonus = 0f;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Debug

        [ContextMenu("Print Environment State")]
        public void PrintState()
        {
            Debug.Log($"[Environment] pH: {CurrentPH:F2} (mod:{PHEfficiencyModifier:F2}) | " +
                      $"O2: {CurrentO2:P0} (mod:{OxygenEfficiencyModifier:F2}) | " +
                      $"Temp: {CurrentTemperature:F1}°C (mod:{TemperatureEfficiencyModifier:F2}) | " +
                      $"Global: {GlobalEnvironmentModifier:F2} | Fermentación: {IsFermentationActive}");
        }

        #endregion
    }
}
