using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.Core
{
    /// <summary>
    /// Gestiona los 4 recursos de Protogenesis: Primordia (GDD v2):
    ///   ATP          — energía celular,    máx 500
    ///   AminoAcids   — construcción,       máx 200
    ///   Nucleotides  — información,        máx 200
    ///   Lipids       — membrana/almacén,   máx 200
    ///
    /// API Primordia:
    ///   Produce(type, amount)   — añade recurso (colección, producción)
    ///   Consume(type, amount)   — consume recurso; devuelve false si no hay suficiente
    ///   HasEnough(type, amount) — comprueba disponibilidad
    ///   Get(type)               — valor actual
    ///   GetPercent(type)        — valor actual / máximo (0-1)
    ///
    /// Compatibilidad legacy: AddResource / ConsumeResource / CanAfford / GetResource
    /// siguen funcionando para no romper código v4.6 aún no migrado.
    /// Las operaciones sobre ResourceType legacy (Glucose, Biomass, etc.) son no-op
    /// en runtime: el diccionario no tiene entrada para ellos.
    ///
    /// ATP crítico: si cae del 20% durante más de 5 s → apoptosis.
    /// </summary>
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }

        // ── Valores iniciales (GDD v2 §3.2) ──────────────────────────────────────
        [Header("ATP (máx 500)")]
        [SerializeField] private float atpInitial = 120f;
        [SerializeField] private float atpMax     = 500f;

        [Header("Amino Acids (máx 200)")]
        [SerializeField] private float aminoAcidsInitial = 50f;
        [SerializeField] private float aminoAcidsMax     = 200f;

        [Header("Nucleotides (máx 200)")]
        [SerializeField] private float nucleotidesInitial = 50f;
        [SerializeField] private float nucleotidesMax     = 200f;

        [Header("Lipids (máx 200)")]
        [SerializeField] private float lipidsInitial = 40f;
        [SerializeField] private float lipidsMax     = 200f;

        [Header("Apoptosis")]
        [Tooltip("Segundos con ATP crítico (<20%) antes de iniciar apoptosis.")]
        [SerializeField] private float atpCriticalGracePeriod = 5f;

        // ── Estado ────────────────────────────────────────────────────────────────
        private readonly Dictionary<ResourceType, ResourceData> _resources
            = new Dictionary<ResourceType, ResourceData>();

        // Seguimiento de umbral crítico por recurso
        private readonly Dictionary<ResourceType, bool> _wasCritical
            = new Dictionary<ResourceType, bool>();

        private float _atpCriticalTimer = 0f;

        // ── Estadísticas ──────────────────────────────────────────────────────────
        /// <summary>ATP total producido durante la sesión (acumulado histórico).</summary>
        public float TotalATPProduced { get; private set; } = 0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            InitializeResources();
        }

        private void Update()
        {
            if (Instance != this) return;
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                return;

            float dt = Time.deltaTime;

            foreach (var kvp in _resources)
                TickResource(kvp.Key, kvp.Value, dt);

            CheckCriticalThresholds(dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Inicialización

        private void InitializeResources()
        {
            _resources[ResourceType.ATP]         = new ResourceData(atpInitial,        atpMax);
            _resources[ResourceType.AminoAcids]  = new ResourceData(aminoAcidsInitial, aminoAcidsMax);
            _resources[ResourceType.Nucleotides] = new ResourceData(nucleotidesInitial, nucleotidesMax);
            _resources[ResourceType.Lipids]      = new ResourceData(lipidsInitial,     lipidsMax);

            foreach (var key in _resources.Keys)
                _wasCritical[key] = false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Tick

        private void TickResource(ResourceType type, ResourceData data, float dt)
        {
            float netRate = (data.ProductionRate * data.ProductionMultiplier) - data.ConsumptionRate;
            float delta   = netRate * dt;

            if (delta == 0f) return;

            float previous = data.Current;
            data.Current = Mathf.Clamp(data.Current + delta, 0f, data.Max);
            float actualDelta = data.Current - previous;

            if (Mathf.Abs(actualDelta) < 0.001f) return;

            if (type == ResourceType.ATP && actualDelta > 0f)
            {
                TotalATPProduced += actualDelta;
                if (GameManager.Instance != null)
                    GameManager.Instance.TotalATPProduced += actualDelta;
            }

            FireChangedEvent(type, data.Current, actualDelta);

            if (data.Current <= 0f)
                EventBus.TriggerResourceDepleted(type);
        }

        private void CheckCriticalThresholds(float dt)
        {
            foreach (var kvp in _resources)
            {
                ResourceType type = kvp.Key;
                ResourceData data = kvp.Value;
                bool isCritical   = data.Current < data.Max * 0.20f;

                _wasCritical.TryGetValue(type, out bool wasCritical);

                if (isCritical && !wasCritical)
                {
                    _wasCritical[type] = true;
                    EventBus.TriggerResourceCritical(type);
                    if (type == ResourceType.ATP) EventBus.TriggerATPCritical();
                }
                else if (!isCritical && wasCritical)
                {
                    _wasCritical[type] = false;
                    EventBus.TriggerResourceRecovered(type);
                    if (type == ResourceType.ATP) EventBus.TriggerATPRecovered();
                }
            }

            // Apoptosis si ATP crítico durante demasiado tiempo
            if (_resources.TryGetValue(ResourceType.ATP, out var atp))
            {
                if (atp.Current <= 0f)
                {
                    _atpCriticalTimer += dt;
                    if (_atpCriticalTimer >= atpCriticalGracePeriod && GameManager.Instance != null)
                    {
                        GameManager.Instance.StartApoptosis();
                        _atpCriticalTimer = 0f;
                    }
                }
                else
                {
                    _atpCriticalTimer = 0f;
                    if (GameManager.Instance != null && GameManager.Instance.IsInApoptosis)
                        GameManager.Instance.CancelApoptosis();
                }
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API Primordia

        /// <summary>Añade una cantidad de recurso (colección, producción puntual).</summary>
        /// <returns>Cantidad realmente añadida (puede ser menor si llega al máximo).</returns>
        public float Produce(ResourceType type, float amount)
        {
            if (!_resources.TryGetValue(type, out var data) || amount <= 0f) return 0f;

            float before  = data.Current;
            data.Current  = Mathf.Min(data.Current + amount, data.Max);
            float added   = data.Current - before;

            if (added > 0f)
            {
                if (type == ResourceType.ATP) TotalATPProduced += added;
                FireChangedEvent(type, data.Current, added);
            }

            return added;
        }

        /// <summary>Consume una cantidad de recurso.</summary>
        /// <returns>True si había suficiente y se consumió.</returns>
        public bool Consume(ResourceType type, float amount)
        {
            if (!HasEnough(type, amount)) return false;

            _resources[type].Current -= amount;
            FireChangedEvent(type, _resources[type].Current, -amount);
            return true;
        }

        /// <summary>Comprueba si hay al menos <paramref name="amount"/> de un recurso.</summary>
        public bool HasEnough(ResourceType type, float amount)
            => _resources.TryGetValue(type, out var data) && data.Current >= amount;

        /// <summary>Valor actual de un recurso.</summary>
        public float Get(ResourceType type)
            => _resources.TryGetValue(type, out var data) ? data.Current : 0f;

        /// <summary>Porcentaje actual respecto al máximo (0-1).</summary>
        public float GetPercent(ResourceType type)
        {
            if (!_resources.TryGetValue(type, out var data) || data.Max <= 0f) return 0f;
            return data.Current / data.Max;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API Legacy (compatibilidad v4.6 — deprecar gradualmente)

        /// <summary>Devuelve el valor actual de un recurso.</summary>
        public float GetResource(ResourceType type) => Get(type);

        /// <summary>Devuelve el valor máximo de un recurso.</summary>
        public float GetMaxResource(ResourceType type)
            => _resources.TryGetValue(type, out var data) ? data.Max : 0f;

        /// <summary>Devuelve la tasa neta actual (producción - consumo) de un recurso.</summary>
        public float GetNetRate(ResourceType type)
        {
            if (!_resources.TryGetValue(type, out var data)) return 0f;
            return (data.ProductionRate * data.ProductionMultiplier) - data.ConsumptionRate;
        }

        /// <summary>Añade una cantidad directa de recurso.</summary>
        public float AddResource(ResourceType type, float amount) => Produce(type, amount);

        /// <summary>Consume una cantidad de recurso.</summary>
        public bool ConsumeResource(ResourceType type, float amount) => Consume(type, amount);

        /// <summary>Verifica si hay suficiente cantidad de un recurso.</summary>
        public bool CanAfford(ResourceType type, float amount) => HasEnough(type, amount);

        /// <summary>Verifica si se puede pagar un coste compuesto.</summary>
        public bool CanAffordAll(params (ResourceType type, float amount)[] costs)
        {
            foreach (var (type, amount) in costs)
                if (!HasEnough(type, amount)) return false;
            return true;
        }

        /// <summary>Consume múltiples recursos. No consume ninguno si alguno falla.</summary>
        public bool ConsumeAll(params (ResourceType type, float amount)[] costs)
        {
            if (!CanAffordAll(costs)) return false;
            foreach (var (type, amount) in costs)
                Consume(type, amount);
            return true;
        }

        /// <summary>Registra una tasa de producción continua por segundo.</summary>
        public void RegisterProductionRate(ResourceType type, float ratePerSecond)
        {
            if (_resources.TryGetValue(type, out var data))
                data.ProductionRate += ratePerSecond;
        }

        /// <summary>Elimina una tasa de producción previamente registrada.</summary>
        public void UnregisterProductionRate(ResourceType type, float ratePerSecond)
        {
            if (_resources.TryGetValue(type, out var data))
                data.ProductionRate = Mathf.Max(0f, data.ProductionRate - ratePerSecond);
        }

        /// <summary>Registra una tasa de consumo continua por segundo.</summary>
        public void RegisterConsumptionRate(ResourceType type, float ratePerSecond)
        {
            if (_resources.TryGetValue(type, out var data))
                data.ConsumptionRate += ratePerSecond;
        }

        /// <summary>Elimina una tasa de consumo previamente registrada.</summary>
        public void UnregisterConsumptionRate(ResourceType type, float ratePerSecond)
        {
            if (_resources.TryGetValue(type, out var data))
                data.ConsumptionRate = Mathf.Max(0f, data.ConsumptionRate - ratePerSecond);
        }

        /// <summary>Aplica un multiplicador global a la producción de un recurso.</summary>
        public void SetProductionMultiplier(ResourceType type, float multiplier)
        {
            if (_resources.TryGetValue(type, out var data))
                data.ProductionMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>Expande el máximo de un recurso.</summary>
        public void ExpandMax(ResourceType type, float newMax)
        {
            if (_resources.TryGetValue(type, out var data))
                data.Max = Mathf.Max(data.Max, newMax);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Debug

        [ContextMenu("Print Resource State")]
        public void PrintResourceState()
        {
            foreach (var kvp in _resources)
            {
                var d = kvp.Value;
                Debug.Log($"[ResourceManager] {kvp.Key}: {d.Current:F1}/{d.Max:F1} " +
                          $"({GetPercent(kvp.Key):P0}) " +
                          $"| Net: {GetNetRate(kvp.Key):F1}/s");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private void FireChangedEvent(ResourceType type, float current, float delta)
        {
            if (type == ResourceType.ATP)
                EventBus.TriggerATPChanged(current, delta);
            else
                EventBus.TriggerResourceChanged(type, current, delta);
        }

        #endregion
    }

    // ─────────────────────────────────────────────────────────────────────────────
    /// <summary>Datos internos de un recurso individual.</summary>
    internal class ResourceData
    {
        public float Current              { get; set; }
        public float Max                  { get; set; }
        public float ProductionRate       { get; set; } = 0f;
        public float ConsumptionRate      { get; set; } = 0f;
        public float ProductionMultiplier { get; set; } = 1f;

        public ResourceData(float initial, float max)
        {
            Current = initial;
            Max     = max;
        }
    }
}
