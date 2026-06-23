using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Ribosoma — "La fábrica de proteínas de la célula".
    ///
    /// Produce proteínas continuamente y gestiona una cola de producción
    /// de unidades. Cuando hay suficientes proteínas para la primera unidad
    /// en cola, las consume y dispara el spawn vía EventBus.
    ///
    /// Sinergia con Retículo Endoplásmico:
    ///   Si hay un RE a menos de 1.5u, la velocidad de producción de proteínas
    ///   aumenta un 30%. Se re-verifica cada 5 seg.
    ///
    /// Infección por bacteriófago:
    ///   Cuando está infectado, produce unidades enemigas en lugar de aliadas
    ///   durante 30 seg, hasta que la infección es eliminada.
    /// </summary>
    public class Ribosoma : OrganelleBase
    {
        // ── Producción de proteínas por nivel ─────────────────────────────────────
        private static readonly float[] ProteinsPerSec = { 1f, 2f, 4f };
        private static readonly int[]   QueueCapacity  = { 3, 6, 10 };

        public float CurrentProteinRate => ProteinsPerSec[CurrentLevel - 1] * SynergyMultiplier * Efficiency;

        // ── Sinergia con RE ───────────────────────────────────────────────────────
        public float SynergyMultiplier { get; private set; } = 1f;
        private float _synergyCheckTimer = 0f;
        private const float SynergyCheckInterval = 5f;
        private const float SynergyBonus = 1.3f;
        private const float SynergyRadius = 1.5f;

        // ── Cola de producción ────────────────────────────────────────────────────
        private readonly Queue<UnitOrder> _productionQueue = new Queue<UnitOrder>();

        public int QueueCount   => _productionQueue.Count;
        public int MaxQueueSize => QueueCapacity[CurrentLevel - 1] + _queueCapacityBonus;

        // Bonus de capacidad de cola (añadido por árbol de mejoras)
        private int _queueCapacityBonus = 0;

        /// <summary>Expande la capacidad de la cola de producción (nodo árbol de mejoras).</summary>
        public void ExpandQueueCapacity(int extraSlots) => _queueCapacityBonus += extraSlots;

        // Prefabs de unidades disponibles (asignar en Inspector)
        [Header("Prefabs de unidades (asignar en orden de UnitType)")]
        [SerializeField] private UnitPrefabEntry[] unitPrefabs;

        // ── Infección ─────────────────────────────────────────────────────────────
        private bool      _infected           = false;
        private float     _infectionTimer     = 0f;
        private const float InfectionDuration = 30f;

        [Header("Prefab de enemigo spawneado al estar infectado")]
        [SerializeField] private GameObject infectedSpawnPrefab;

        // ── Estado de producción ──────────────────────────────────────────────────
        public bool IsProducing { get; private set; } = false;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Start()
        {
            StartCoroutine(ProteinProductionLoop());
            StartCoroutine(UnitProductionLoop());
            EventBus.TriggerOrganelleBuilt(gameObject, OrganelleType);
            CheckSynergy();
        }

        protected override void Update()
        {
            base.Update();

            // Re-verificar sinergia periódicamente
            _synergyCheckTimer += Time.deltaTime;
            if (_synergyCheckTimer >= SynergyCheckInterval)
            {
                _synergyCheckTimer = 0f;
                CheckSynergy();
            }

            // Contador de infección
            if (_infected)
            {
                _infectionTimer -= Time.deltaTime;
                if (_infectionTimer <= 0f)
                    ClearInfection();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Producción de proteínas

        private IEnumerator ProteinProductionLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(1f);

                if (GameManager.Instance != null &&
                   (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                    continue;

                if (Efficiency <= 0f || _infected) continue;

                ResourceManager.Instance?.AddResource(ResourceType.Biomass, CurrentProteinRate);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Cola de unidades

        /// <summary>
        /// Añade una unidad a la cola de producción.
        /// </summary>
        /// <param name="unitType">Nombre del tipo de unidad.</param>
        /// <param name="proteinCost">Coste en proteínas.</param>
        /// <returns>True si fue añadida a la cola, false si está llena.</returns>
        public bool EnqueueUnit(string unitType, float proteinCost)
        {
            if (_productionQueue.Count >= MaxQueueSize)
            {
                Debug.Log($"[Ribosoma] Cola llena ({MaxQueueSize}).");
                return false;
            }
            _productionQueue.Enqueue(new UnitOrder(unitType, proteinCost));
            Debug.Log($"[Ribosoma] '{unitType}' añadido a la cola ({_productionQueue.Count}/{MaxQueueSize}).");
            return true;
        }

        private IEnumerator UnitProductionLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.5f);

                if (GameManager.Instance != null &&
                   (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                    continue;

                if (_productionQueue.Count == 0) { IsProducing = false; continue; }
                if (Efficiency <= 0f)            continue;

                UnitOrder order = _productionQueue.Peek();
                var rm = ResourceManager.Instance;

                if (rm == null || !rm.CanAfford(ResourceType.Biomass, order.ProteinCost))
                    continue;

                rm.ConsumeResource(ResourceType.Biomass, order.ProteinCost);
                _productionQueue.Dequeue();
                IsProducing = true;

                if (_infected)
                    SpawnEnemyUnit();
                else
                    SpawnAllyUnit(order.UnitType);
            }
        }

        private void SpawnAllyUnit(string unitType)
        {
            EventBus.TriggerUnitSpawned(gameObject, unitType);
            Debug.Log($"[Ribosoma] '{unitType}' producida.");
        }

        private void SpawnEnemyUnit()
        {
            if (infectedSpawnPrefab == null) return;
            Vector2 offset = Random.insideUnitCircle * 1f;
            Instantiate(infectedSpawnPrefab, (Vector2)transform.position + offset, Quaternion.identity);
            Debug.Log("[Ribosoma] ¡INFECTADO! Produciendo unidad enemiga.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Sinergia con Retículo Endoplásmico

        private void CheckSynergy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, SynergyRadius);
            bool reFound = false;
            foreach (var hit in hits)
            {
                if (hit.GetComponent<ReticuloEndoplasmatico>() != null)
                {
                    reFound = true;
                    break;
                }
            }

            float previous = SynergyMultiplier;
            SynergyMultiplier = reFound ? SynergyBonus : 1f;

            if (!Mathf.Approximately(previous, SynergyMultiplier))
                Debug.Log($"[Ribosoma] Sinergia RE: {(reFound ? "+30% velocidad" : "desactivada")}.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Infección

        /// <summary>
        /// Activa la infección por bacteriófago. El ribosoma produce enemigos
        /// durante <see cref="InfectionDuration"/> segundos.
        /// </summary>
        public void GetInfected()
        {
            if (_infected) return;
            _infected       = true;
            _infectionTimer = InfectionDuration;
            SetInfected(true);
            Debug.Log("[Ribosoma] ¡INFECTADO por bacteriófago! Produciendo enemigos 30 seg.");
        }

        private void ClearInfection()
        {
            _infected = false;
            SetInfected(false);
            Debug.Log("[Ribosoma] Infección eliminada.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region OrganelleBase overrides

        protected override void OnLevelUp()
        {
            Debug.Log($"[Ribosoma] Nivel {CurrentLevel}: {ProteinsPerSec[CurrentLevel - 1]} prot/s, cola {QueueCapacity[CurrentLevel - 1]}");
            // Al subir de nivel añadir tipos extra de producción según el GDD
            if (CurrentLevel == 3)
                Debug.Log("[Ribosoma] Nivel 3: puede producir 2 tipos de unidades simultáneos.");
            CheckSynergy();
        }

        protected override void OnOrganelleDestroyed()
        {
            _productionQueue.Clear();
        }

        #endregion
    }

    // ─────────────────────────────────────────────────────────────────────────────
    /// <summary>Orden de producción de una unidad en la cola del ribosoma.</summary>
    public struct UnitOrder
    {
        public string UnitType    { get; }
        public float  ProteinCost { get; }

        public UnitOrder(string unitType, float cost)
        {
            UnitType    = unitType;
            ProteinCost = cost;
        }
    }

    /// <summary>Par prefab-tipo para el array del Inspector.</summary>
    [System.Serializable]
    public struct UnitPrefabEntry
    {
        public string     unitType;
        public GameObject prefab;
    }

    /// <summary>
    /// Stub del Retículo Endoplásmico para la detección de sinergia.
    /// Se reemplazará por la clase completa en un prompt posterior.
    /// </summary>
    public class ReticuloEndoplasmatico : OrganelleBase
    {
        protected override void OnLevelUp() { }
    }
}
