using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Organelles;

namespace Protogenesis.Progression
{
    /// <summary>
    /// UpgradeTree — Árbol de mejoras evolutivas con cola de investigación.
    ///
    /// Flujo:
    ///   1. La UI llama a RequestResearch(nodeId).
    ///   2. El nodo entra en _researchQueue.
    ///   3. ResearchLoop() lo procesa uno a uno (coroutine).
    ///   4. Al completarse llama a ApplyEffect(node) y marca el nodo como desbloqueado.
    ///   5. Los efectos se aplican directamente a los singletons/sistemas correspondientes.
    ///
    /// Mecánica extra — Deriva Genética:
    ///   Cuando se produce la misma unidad ≥50 veces en una sesión, hay un 15% de
    ///   probabilidad de que esa línea sufra una mutación beneficiosa permanente
    ///   (+10% daño o +10% HP). Esto se guarda en GeneManager.
    /// </summary>
    public class UpgradeTree : MonoBehaviour
    {
        public static UpgradeTree Instance { get; private set; }

        // ── Catálogo de nodos ─────────────────────────────────────────────────────
        [Header("Catálogo de nodos (asignar los 16 ScriptableObjects)")]
        [SerializeField] private UpgradeNodeData[] allNodes;

        // ── Estado de investigación ───────────────────────────────────────────────
        private readonly HashSet<string>      _unlocked  = new HashSet<string>();
        private readonly Queue<UpgradeNodeData> _queue   = new Queue<UpgradeNodeData>();
        private UpgradeNodeData               _current   = null;
        private float                         _progress  = 0f;   // 0-1

        // ── Deriva genética ───────────────────────────────────────────────────────
        private readonly Dictionary<string, int>   _unitProductionCount = new Dictionary<string, int>();
        private readonly HashSet<string>           _mutatedLineages     = new HashSet<string>();
        private const int   GeneticDriftThreshold = 50;
        private const float GeneticDriftChance    = 0.15f;
        private const float GeneticDriftBonus     = 0.10f;

        // ── Propiedades públicas ──────────────────────────────────────────────────
        public UpgradeNodeData CurrentResearch    => _current;
        public float           ResearchProgress   => _progress;
        public IReadOnlyCollection<string> Unlocked => _unlocked;
        public IReadOnlyCollection<UpgradeNodeData> Queue => _queue;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            if (allNodes == null || allNodes.Length == 0)
                BuildDefaultCatalog();
            EventBus.OnUnitSpawned += OnUnitProducedEvent;
        }

        /// <summary>
        /// Genera los 16 nodos del árbol de mejoras usando valores del GDD.
        /// Se usa cuando no hay ScriptableObjects asignados en el Inspector.
        /// </summary>
        private void BuildDefaultCatalog()
        {
            allNodes = new UpgradeNodeData[]
            {
                // ── METABOLISMO ──────────────────────────────────────────────────
                MakeNode("META_01", "Glucólisis Mejorada",    UpgradeBranch.Metabolism, 0,
                         UpgradeEffectType.ATPProductionBonus,      0.20f, "",        0, 200f, 8f,  20f),
                MakeNode("META_02", "Fosforilación Oxidativa", UpgradeBranch.Metabolism, 1,
                         UpgradeEffectType.OxidativePhosphorylation,2f,    "META_01", 2, 400f, 15f, 40f),
                MakeNode("META_03", "Fermentación Eficiente",  UpgradeBranch.Metabolism, 2,
                         UpgradeEffectType.FermentationEfficiency,   0.25f, "META_02", 1, 300f, 10f, 30f),
                MakeNode("META_04", "NADH Sintasa",            UpgradeBranch.Metabolism, 3,
                         UpgradeEffectType.NADHProductionBonus,      0.30f, "META_02", 3, 600f, 20f, 60f),

                // ── DEFENSA ──────────────────────────────────────────────────────
                MakeNode("DEF_01", "Membrana Reforzada",   UpgradeBranch.Defense, 0,
                         UpgradeEffectType.MembraneResistance,  0.15f, "",       0, 250f, 10f, 25f),
                MakeNode("DEF_02", "Proteínas HSP+",       UpgradeBranch.Defense, 1,
                         UpgradeEffectType.HSPRadius,           1.5f,  "DEF_01",1, 350f, 12f, 35f),
                // TODO: Primordia — MakeNode("DEF_03", "Expansión HP CAP",     UpgradeBranch.Defense, 2,
                         // TODO: Primordia —          UpgradeEffectType.CAPMaxHPBonus, 0.25f, "DEF_01",2, 500f, 18f, 50f),
                MakeNode("DEF_04", "Blindaje Orgánulos",   UpgradeBranch.Defense, 3,
                         UpgradeEffectType.OrganelleMaxHPBonus, 0.30f, "DEF_02",3, 700f, 25f, 70f),

                // ── PRODUCCIÓN ───────────────────────────────────────────────────
                MakeNode("PRO_01", "Ribosoma Veloz",     UpgradeBranch.Production, 0,
                         UpgradeEffectType.ProteinProductionBonus, 0.20f, "",       0, 200f, 8f,  20f),
                MakeNode("PRO_02", "Cola de Síntesis",   UpgradeBranch.Production, 1,
                         UpgradeEffectType.RibosomeQueueCapacity,  2f,    "PRO_01",1, 300f, 10f, 30f),
                MakeNode("PRO_03", "ARN Polimerasa II",  UpgradeBranch.Production, 2,
                         UpgradeEffectType.RNAProductionBonus,     0.25f, "PRO_01",2, 400f, 15f, 40f),
                MakeNode("PRO_04", "Daño Unidades +",    UpgradeBranch.Production, 3,
                         UpgradeEffectType.UnitDamageBonus,        0.15f, "PRO_02",2, 500f, 18f, 50f),

                // ── EXPANSIÓN ────────────────────────────────────────────────────
                MakeNode("EXP_01", "Zona Expandida",         UpgradeBranch.Expansion, 0,
                         UpgradeEffectType.ZoneRadiusExpansion, 1.5f,  "",       0, 300f, 12f, 30f),
                MakeNode("EXP_02", "Slot Extra Mitocondria", UpgradeBranch.Expansion, 1,
                         UpgradeEffectType.ExtraOrganelleSlot,  1f,    "EXP_01",2, 450f, 15f, 45f),
                MakeNode("EXP_03", "Unidades más Rápidas",   UpgradeBranch.Expansion, 2,
                         UpgradeEffectType.UnitSpeedBonus,      0.20f, "EXP_01",1, 350f, 12f, 35f),
                MakeNode("EXP_04", "Nueva Línea Celular",    UpgradeBranch.Expansion, 3,
                         UpgradeEffectType.NewUnitType,          0f,    "EXP_02",3, 800f, 30f, 80f,
                         "EukMacrophage"),
            };
            Debug.Log("[UpgradeTree] Catálogo por defecto generado: 16 nodos (4 ramas × 4 columnas).");
        }

        private static UpgradeNodeData MakeNode(
            string id, string name, UpgradeBranch branch, int col,
            UpgradeEffectType effect, float value, string prereq,
            int era, float costATP, float costRNA, float time,
            string target = "")
        {
            var n = ScriptableObject.CreateInstance<UpgradeNodeData>();
            n.nodeId             = id;
            n.displayName        = name;
            n.branch             = branch;
            n.column             = col;
            n.effectType         = effect;
            n.effectValue        = value;
            n.prerequisiteNodeId = prereq;
            n.requiredEra        = era;
            n.costATP            = costATP;
            n.costRNA            = costRNA;
            n.researchTime       = time;
            n.effectTarget       = target;
            return n;
        }

        private void OnDestroy()
        {
            EventBus.OnUnitSpawned -= OnUnitProducedEvent;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>Intenta encolar un nodo para investigación.</summary>
        public bool RequestResearch(string nodeId)
        {
            var node = FindNode(nodeId);
            if (node == null)
            {
                Debug.LogWarning($"[UpgradeTree] Nodo '{nodeId}' no encontrado.");
                return false;
            }

            if (!CanResearch(node, out string reason))
            {
                Debug.Log($"[UpgradeTree] No se puede investigar '{nodeId}': {reason}");
                return false;
            }

            // Cobrar recursos
            var rm = ResourceManager.Instance;
            if (rm == null) return false;
            if (!rm.CanAffordAll((ResourceType.ATP, node.costATP),
                                 (ResourceType.GenomicPoints, node.costRNA)))
            {
                Debug.Log($"[UpgradeTree] Sin recursos para '{nodeId}'.");
                return false;
            }
            rm.ConsumeAll((ResourceType.ATP, node.costATP),
                          (ResourceType.GenomicPoints, node.costRNA));

            _queue.Enqueue(node);

            if (_current == null)
                StartCoroutine(ResearchLoop());

            EventBus.TriggerResearchQueued(node.nodeId, node.displayName);
            return true;
        }

        /// <summary>Devuelve true si el nodo está desbloqueado.</summary>
        public bool IsUnlocked(string nodeId) => _unlocked.Contains(nodeId);

        /// <summary>Cancela la investigación en curso y devuelve los recursos.</summary>
        public void CancelCurrentResearch()
        {
            if (_current == null) return;
            var rm = ResourceManager.Instance;
            rm?.AddResource(ResourceType.ATP, _current.costATP * (1f - _progress));
            rm?.AddResource(ResourceType.GenomicPoints, _current.costRNA * (1f - _progress));
            StopAllCoroutines();
            _current  = null;
            _progress = 0f;
            if (_queue.Count > 0)
                StartCoroutine(ResearchLoop());
        }

        /// <summary>
        /// Verifica si un nodo puede investigarse (era, prerequisito, no ya desbloqueado).
        /// </summary>
        public bool CanResearch(UpgradeNodeData node, out string reason)
        {
            reason = "";
            if (_unlocked.Contains(node.nodeId))
            {
                reason = "Ya desbloqueado";
                return false;
            }
            // Comprobar si ya está en cola
            foreach (var q in _queue)
            {
                if (q.nodeId == node.nodeId) { reason = "Ya en cola"; return false; }
            }
            if (_current != null && _current.nodeId == node.nodeId)
            {
                reason = "En investigación";
                return false;
            }
            if (GameManager.Instance != null &&
                GameManager.Instance.CurrentEra < node.requiredEra)
            {
                reason = $"Requiere ERA {node.requiredEra}";
                return false;
            }
            if (!string.IsNullOrEmpty(node.prerequisiteNodeId) &&
                !_unlocked.Contains(node.prerequisiteNodeId))
            {
                reason = $"Requiere '{node.prerequisiteNodeId}'";
                return false;
            }
            return true;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Cola de investigación

        private IEnumerator ResearchLoop()
        {
            while (_queue.Count > 0)
            {
                _current  = _queue.Dequeue();
                _progress = 0f;
                EventBus.TriggerResearchStarted(_current.nodeId, _current.displayName,
                                                _current.researchTime);

                float elapsed = 0f;
                while (elapsed < _current.researchTime)
                {
                    elapsed   += Time.deltaTime;
                    _progress  = Mathf.Clamp01(elapsed / _current.researchTime);
                    yield return null;
                }

                _unlocked.Add(_current.nodeId);
                ApplyEffect(_current);
                EventBus.TriggerResearchCompleted(_current.nodeId, _current.displayName);
                GeneManager.Instance?.OnNodeUnlocked(_current.nodeId);

                _current  = null;
                _progress = 0f;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Aplicar efectos

        private void ApplyEffect(UpgradeNodeData node)
        {
            switch (node.effectType)
            {
                // ── METABOLISMO ──────────────────────────────────────────────────
                case UpgradeEffectType.ATPProductionBonus:
                    ResourceManager.Instance?.SetProductionMultiplier(
                        ResourceType.ATP,
                        1f + node.effectValue);
                    break;

                case UpgradeEffectType.NADHProductionBonus:
                    ResourceManager.Instance?.SetProductionMultiplier(
                        ResourceType.ATP,
                        1f + node.effectValue);
                    break;

                case UpgradeEffectType.FermentationEfficiency:
                    EnvironmentManager.Instance?.BoostFermentationEfficiency(node.effectValue);
                    break;

                case UpgradeEffectType.OxidativePhosphorylation:
                    ApplyToAllMitocondrias(m => m.AddNADHBonusPerTick(node.effectValue));
                    break;

                // ── DEFENSA ──────────────────────────────────────────────────────
                case UpgradeEffectType.CAPMaxHPBonus:
                    // TODO: Primordia — Player.CAP.Instance?.ApplyMaxHPMultiplier(1f + node.effectValue);
                    break;

                case UpgradeEffectType.OrganelleMaxHPBonus:
                    foreach (var o in FindObjectsByType<OrganelleBase>(FindObjectsSortMode.None))
                        o.ApplyMaxHPMultiplier(1f + node.effectValue);
                    break;

                case UpgradeEffectType.HSPRadius:
                    // TODO: Primordia — Player.CAP.Instance?.ExpandHSPRadius(node.effectValue);
                    break;

                case UpgradeEffectType.MembraneResistance:
                    // Implementado a través de un flag global consultado en EnemyBase
                    GlobalUpgradeFlags.MembraneResistanceBonus += node.effectValue;
                    break;

                // ── PRODUCCIÓN ───────────────────────────────────────────────────
                case UpgradeEffectType.ProteinProductionBonus:
                    ResourceManager.Instance?.SetProductionMultiplier(
                        ResourceType.Biomass,
                        1f + node.effectValue);
                    break;

                case UpgradeEffectType.RNAProductionBonus:
                    ResourceManager.Instance?.SetProductionMultiplier(
                        ResourceType.GenomicPoints,
                        1f + node.effectValue);
                    break;

                case UpgradeEffectType.RibosomeQueueCapacity:
                    foreach (var r in FindObjectsByType<Ribosoma>(FindObjectsSortMode.None))
                        r.ExpandQueueCapacity(Mathf.RoundToInt(node.effectValue));
                    break;

                case UpgradeEffectType.UnitDamageBonus:
                    GlobalUpgradeFlags.UnitDamageBonus += node.effectValue;
                    break;

                // ── EXPANSIÓN ────────────────────────────────────────────────────
                case UpgradeEffectType.ZoneRadiusExpansion:
                    ZoneSystem.Instance?.ExpandCell(node.effectValue);
                    break;

                case UpgradeEffectType.ExtraOrganelleSlot:
                    Mitocondria.ExtraSlots++;
                    break;

                case UpgradeEffectType.UnitSpeedBonus:
                    GlobalUpgradeFlags.UnitSpeedBonus += node.effectValue;
                    break;

                case UpgradeEffectType.NewUnitType:
                    GlobalUpgradeFlags.UnlockedUnitTypes.Add(node.effectTarget);
                    break;
            }

            Debug.Log($"[UpgradeTree] Efecto aplicado: {node.nodeId} ({node.effectType} +{node.effectValue})");
        }

        private void ApplyToAllMitocondrias(System.Action<Mitocondria> action)
        {
            foreach (var m in FindObjectsByType<Mitocondria>(FindObjectsSortMode.None))
                action(m);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Deriva Genética

        private void OnUnitProducedEvent(UnityEngine.GameObject go, string unitType)
            => OnUnitProduced(unitType);

        private void OnUnitProduced(string unitType)
        {
            if (_mutatedLineages.Contains(unitType)) return;

            _unitProductionCount.TryGetValue(unitType, out int count);
            count++;
            _unitProductionCount[unitType] = count;

            if (count >= GeneticDriftThreshold)
            {
                if (Random.value < GeneticDriftChance)
                    TriggerGeneticDrift(unitType);
            }
        }

        private void TriggerGeneticDrift(string unitType)
        {
            _mutatedLineages.Add(unitType);

            // Elige aleatoriamente entre mutación de daño o de HP
            bool damageMutation = Random.value < 0.5f;
            if (damageMutation)
            {
                GlobalUpgradeFlags.UnitDamageBonus += GeneticDriftBonus;
                GeneManager.Instance?.RegisterGeneticDrift(unitType, "DamageBonus", GeneticDriftBonus);
                Debug.Log($"[GeneticDrift] Línea '{unitType}' mutó: +{GeneticDriftBonus:P0} daño.");
            }
            else
            {
                GlobalUpgradeFlags.UnitHPBonus += GeneticDriftBonus;
                GeneManager.Instance?.RegisterGeneticDrift(unitType, "HPBonus", GeneticDriftBonus);
                Debug.Log($"[GeneticDrift] Línea '{unitType}' mutó: +{GeneticDriftBonus:P0} HP.");
            }

            EventBus.TriggerGeneticDrift(unitType, damageMutation ? "DamageBonus" : "HPBonus",
                                          GeneticDriftBonus);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private UpgradeNodeData FindNode(string nodeId)
        {
            if (allNodes == null) return null;
            foreach (var n in allNodes)
                if (n != null && n.nodeId == nodeId) return n;
            return null;
        }

        /// <summary>Devuelve todos los nodos del catálogo (para la UI).</summary>
        public UpgradeNodeData[] GetAllNodes() => allNodes;

        #endregion
    }

    // ── Flags globales (modificadores acumulativos de upgrades) ───────────────────
    /// <summary>
    /// Contenedor de bonificaciones globales aplicadas por el árbol de mejoras.
    /// Consultado por UnitBase, EnemyBase, etc.
    /// </summary>
    public static class GlobalUpgradeFlags
    {
        public static float          UnitDamageBonus      = 0f;   // Aditivo (0.15 = +15%)
        public static float          UnitSpeedBonus       = 0f;
        public static float          UnitHPBonus          = 0f;
        public static float          MembraneResistanceBonus = 0f;
        public static HashSet<string> UnlockedUnitTypes   = new HashSet<string>();

        public static void Reset()
        {
            UnitDamageBonus         = 0f;
            UnitSpeedBonus          = 0f;
            UnitHPBonus             = 0f;
            MembraneResistanceBonus = 0f;
            UnlockedUnitTypes.Clear();
        }
    }
}
