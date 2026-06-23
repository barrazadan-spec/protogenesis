using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Player;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Clase base para todos los orgánulos construibles.
    ///
    /// Gestiona:
    ///   - HP y sistema de eficiencia degradante por daño
    ///   - Niveles (1-3) con upgrade y coste
    ///   - Reciclaje (devuelve 50% del coste)
    ///   - Zona requerida para construcción
    ///   - Integración con HSP (protección de choque térmico de la CAP)
    ///   - Integración con el modificador ambiental global
    ///
    /// Tabla de eficiencia por HP:
    ///   100% HP → eficiencia 1.00
    ///    75% HP → eficiencia 0.90
    ///    50% HP → eficiencia 0.70
    ///    25% HP → eficiencia 0.40
    ///     0% HP → destruido
    /// </summary>
    public abstract class OrganelleBase : MonoBehaviour, IHSPReceiver
    {
        // ── Identificación ────────────────────────────────────────────────────────
        [Header("Identificación")]
        [SerializeField] private string organelleType = "Organelle";
        [SerializeField] private int    eraRequired   = 0;

        /// <summary>Tipo de orgánulo usado por EraManager y la UI.</summary>
        public string OrganelleType => organelleType;

        /// <summary>Era mínima requerida para construirlo.</summary>
        public int EraRequired => eraRequired;

        // ── Zona ──────────────────────────────────────────────────────────────────
        [Header("Zona")]
        [SerializeField] private ZoneType requiredZone = ZoneType.Middle;
        public ZoneType RequiredZone => requiredZone;

        // ── HP ────────────────────────────────────────────────────────────────────
        [Header("HP")]
        [SerializeField] protected float maxHP = 100f;

        public float CurrentHP { get; private set; }
        public float MaxHP     => maxHP;
        public float HPPercent => maxHP > 0f ? CurrentHP / maxHP : 0f;

        // ── Eficiencia ────────────────────────────────────────────────────────────
        /// <summary>
        /// Eficiencia actual del orgánulo (0-1).
        /// Se degrada con el daño y se multiplica por los modificadores activos.
        /// </summary>
        public float Efficiency { get; private set; } = 1f;

        // Modificadores externos
        private float _hspMultiplier   = 1f; // Proteínas de Choque Térmico (1.0-1.3)
        private float _envMultiplier   = 1f; // Ambiente (pH, O2, temperatura)

        // ── Niveles ───────────────────────────────────────────────────────────────
        [Header("Niveles y costes")]
        [SerializeField] private int   currentLevel   = 1;
        [SerializeField] private float buildCostATP   = 20f;
        [SerializeField] private float buildCostProteins = 5f;

        // Costes de upgrade por nivel (índice 0 = coste para subir a nivel 2, etc.)
        [SerializeField] private float[] upgradeCostATP      = { 40f, 80f };
        [SerializeField] private float[] upgradeCostProteins = { 10f, 20f };

        public int CurrentLevel => currentLevel;
        public bool MaxLevelReached => currentLevel >= 3;

        // ── Estado especial ───────────────────────────────────────────────────────
        public bool IsInfected { get; private set; } = false;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        protected virtual void Awake()
        {
            CurrentHP  = maxHP;
            Efficiency = 1f;
        }

        protected virtual void Update()
        {
            // Sincronizar modificador ambiental cada frame
            if (EnvironmentManager.Instance != null)
            {
                _envMultiplier = EnvironmentManager.Instance.GlobalEnvironmentModifier;
                RecalculateEfficiency();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Daño y eficiencia

        /// <summary>Aplica daño al orgánulo y recalcula la eficiencia.</summary>
        public virtual void TakeDamage(float amount)
        {
            if (amount <= 0f) return;

            // HSP reduce el daño recibido cuando están activas
            float reducedAmount = amount / _hspMultiplier;
            CurrentHP = Mathf.Max(0f, CurrentHP - reducedAmount);

            RecalculateEfficiency();
            EventBus.TriggerOrganelleDamaged(gameObject, CurrentHP, maxHP);

            if (CurrentHP <= 0f)
                OnDestroyedByDamage();
        }

        private void RecalculateEfficiency()
        {
            float baseEff = HPPercent switch
            {
                >= 0.75f => 1.00f,
                >= 0.50f => 0.90f,
                >= 0.25f => 0.70f,
                >  0.00f => 0.40f,
                _        => 0.00f
            };

            Efficiency = baseEff * _hspMultiplier * _envMultiplier;
        }

        private void OnDestroyedByDamage()
        {
            EventBus.TriggerOrganelleDestroyed(gameObject, organelleType);
            OnOrganelleDestroyed();
            Destroy(gameObject);
        }

        /// <summary>
        /// Hook para que las subclases ejecuten efectos al ser destruidas
        /// (ej: Lisosoma libera enzimas, Vacuola explota con recursos).
        /// </summary>
        protected virtual void OnOrganelleDestroyed() { }

        /// <summary>Repara HP del orgánulo.</summary>
        public void Repair(float amount)
        {
            CurrentHP = Mathf.Min(maxHP, CurrentHP + amount);
            RecalculateEfficiency();
        }

        /// <summary>Repara el orgánulo al 100%.</summary>
        public void RepairFull()
        {
            CurrentHP = maxHP;
            Efficiency = 1f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Upgrade y Recycle

        /// <summary>
        /// Intenta subir el orgánulo al siguiente nivel.
        /// Verifica recursos, los descuenta y llama OnLevelUp.
        /// </summary>
        /// <returns>True si el upgrade fue exitoso.</returns>
        public bool Upgrade()
        {
            if (MaxLevelReached)
            {
                Debug.Log($"[{organelleType}] Ya está en nivel máximo.");
                return false;
            }

            int upgradeIndex = currentLevel - 1; // Nivel 1→2 usa índice 0
            if (upgradeIndex >= upgradeCostATP.Length) return false;

            float costATP      = upgradeCostATP[upgradeIndex];
            float costProteins = upgradeCostProteins[upgradeIndex];

            var rm = ResourceManager.Instance;
            if (rm == null || !rm.CanAffordAll(
                (ResourceType.ATP,      costATP),
                (ResourceType.Biomass, costProteins)))
            {
                Debug.Log($"[{organelleType}] Recursos insuficientes para upgrade.");
                return false;
            }

            rm.ConsumeAll(
                (ResourceType.ATP,      costATP),
                (ResourceType.Biomass, costProteins));

            currentLevel++;
            RepairFull(); // El upgrade repara el orgánulo
            OnLevelUp();

            EventBus.TriggerOrganelleUpgraded(gameObject, currentLevel);
            Debug.Log($"[{organelleType}] Upgrade a nivel {currentLevel}.");
            return true;
        }

        /// <summary>
        /// Recicla el orgánulo: devuelve el 50% del coste de construcción y se destruye.
        /// </summary>
        public void Recycle()
        {
            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                rm.AddResource(ResourceType.ATP,      buildCostATP      * 0.5f);
                rm.AddResource(ResourceType.Biomass, buildCostProteins * 0.5f);
            }
            Debug.Log($"[{organelleType}] Reciclado. ATP +{buildCostATP * 0.5f:F0} / Proteínas +{buildCostProteins * 0.5f:F0}.");
            EventBus.TriggerOrganelleDestroyed(gameObject, organelleType);
            Destroy(gameObject);
        }

        /// <summary>Devuelve el coste de construcción base.</summary>
        public (float atp, float proteins) GetBuildCost() => (buildCostATP, buildCostProteins);

        /// <summary>Devuelve el coste del próximo upgrade (o (0,0) si ya es nivel máximo).</summary>
        public (float atp, float proteins) GetUpgradeCost()
        {
            int i = currentLevel - 1;
            if (i >= upgradeCostATP.Length) return (0f, 0f);
            return (upgradeCostATP[i], upgradeCostProteins[i]);
        }

        /// <summary>Hook abstracto para que cada orgánulo aplique su bonus de nivel.</summary>
        protected abstract void OnLevelUp();

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region IHSPReceiver — Proteínas de Choque Térmico

        /// <summary>
        /// Recibe la protección de choque térmico de la CAP.
        /// Multiplica la resistencia al daño y la eficiencia.
        /// </summary>
        public void ApplyHSP(bool active, float resistanceMultiplier)
        {
            _hspMultiplier = active ? resistanceMultiplier : 1f;
            RecalculateEfficiency();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Mejoras externas

        /// <summary>Aplica multiplicador de HP máximo (nodo de árbol de mejoras).</summary>
        public void ApplyMaxHPMultiplier(float multiplier)
        {
            float ratio = maxHP > 0f ? CurrentHP / maxHP : 1f;
            maxHP     *= multiplier;
            // CurrentHP tiene setter privado; usamos la ruta de Repair
            float newHP = maxHP * ratio;
            Repair(newHP - CurrentHP);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Infección

        /// <summary>Marca el orgánulo como infectado (por bacteriófago).</summary>
        public void SetInfected(bool infected)
        {
            IsInfected = infected;
            if (infected)
                EventBus.TriggerOrganelleInfected(gameObject);
            else
                EventBus.TriggerInfectionCleared(gameObject);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            // Barra de HP visual en el editor
            Gizmos.color = Color.Lerp(Color.red, Color.green, HPPercent);
            Gizmos.DrawWireCube(transform.position + Vector3.up * 0.8f,
                                new Vector3(HPPercent, 0.1f, 0f));
        }

        #endregion
    }

    // ─────────────────────────────────────────────────────────────────────────────
    /// <summary>Zonas del citoplasma donde se pueden construir orgánulos.</summary>
    public enum ZoneType
    {
        Nuclear,    // Centro (radio 0-3u): Núcleo, RE, Ribosomas
        Middle,     // Anillo medio (3-6u): Mitocondrias, Golgi
        Peripheral  // Periferia (6-9u): Lisosomas, Vacuolas, Defensas
    }
}
