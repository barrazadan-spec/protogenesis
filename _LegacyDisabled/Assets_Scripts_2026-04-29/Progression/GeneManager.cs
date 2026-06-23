using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.Progression
{
    /// <summary>
    /// GeneManager — Meta-progresión entre partidas mediante genes persistentes.
    ///
    /// 5 genes desbloqueables que persisten entre sesiones (PlayerPrefs + JSON):
    ///   GenResistance  — Daño recibido por orgánulos -5% por nivel (max 3)
    ///   GenMitochondrial — ATP base +10% por nivel (max 3)
    ///   GenImmune      — Lisosoma gana +5% daño por nivel (max 3)
    ///   GenMemory      — ARN inicial +10 por nivel (max 3)
    ///   GenAncestral   — Desbloquea unidad ancestral exclusiva al nivel 3
    ///
    /// Los genes se desbloquean/nivelan gastando "Puntos Evolutivos" (PE),
    /// que se ganan al completar partidas, descubrir eras y con Deriva Genética.
    ///
    /// Los efectos pasivos se aplican automáticamente al inicio de cada sesión.
    /// </summary>
    public class GeneManager : MonoBehaviour
    {
        public static GeneManager Instance { get; private set; }

        // ── Datos de genes ────────────────────────────────────────────────────────
        [System.Serializable]
        public class GeneData
        {
            public string id;
            public string displayName;
            [TextArea(1, 3)] public string description;
            public int    currentLevel;
            public int    maxLevel;
            public int[]  costPerLevel;   // PE necesarios para cada nivel
        }

        [Header("Definición de genes (configurar en Inspector)")]
        [SerializeField] private GeneData[] geneDefinitions;

        // ── Puntos Evolutivos ─────────────────────────────────────────────────────
        [Header("Puntos Evolutivos")]
        [SerializeField] private int _evolutionPoints = 0;
        public int EvolutionPoints => _evolutionPoints;

        // ── Mutaciones de Deriva Genética (sesión actual) ─────────────────────────
        private readonly List<GeneticDriftRecord> _driftRecords = new List<GeneticDriftRecord>();

        [System.Serializable]
        private struct GeneticDriftRecord
        {
            public string lineage;
            public string bonusType;
            public float  value;
        }

        // ── Clave de guardado ─────────────────────────────────────────────────────
        private const string SaveKey = "GeneManager_Save";
        private const string PEKey   = "GeneManager_PE";

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitDefaultGenes();
            Load();
        }

        private void Start()
        {
            ApplyPassiveEffects();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Inicialización de genes

        private void InitDefaultGenes()
        {
            // Solo se inicializa si el Inspector no proveyó datos
            if (geneDefinitions != null && geneDefinitions.Length > 0) return;

            geneDefinitions = new GeneData[]
            {
                new GeneData
                {
                    id           = "GenResistance",
                    displayName  = "Gen de Resistencia",
                    description  = "Orgánulos reciben -5% daño por nivel.",
                    currentLevel = 0,
                    maxLevel     = 3,
                    costPerLevel = new[] { 3, 6, 12 }
                },
                new GeneData
                {
                    id           = "GenMitochondrial",
                    displayName  = "Gen Mitocondrial",
                    description  = "ATP base +10% por nivel.",
                    currentLevel = 0,
                    maxLevel     = 3,
                    costPerLevel = new[] { 3, 6, 12 }
                },
                new GeneData
                {
                    id           = "GenImmune",
                    displayName  = "Gen Inmune",
                    description  = "Lisosoma +5% daño por nivel.",
                    currentLevel = 0,
                    maxLevel     = 3,
                    costPerLevel = new[] { 3, 6, 12 }
                },
                new GeneData
                {
                    id           = "GenMemory",
                    displayName  = "Gen de Memoria",
                    description  = "ARN inicial +10 por nivel.",
                    currentLevel = 0,
                    maxLevel     = 3,
                    costPerLevel = new[] { 2, 4, 8 }
                },
                new GeneData
                {
                    id           = "GenAncestral",
                    displayName  = "Gen Ancestral",
                    description  = "Nivel 3: desbloquea la unidad Proto-Arquea.",
                    currentLevel = 0,
                    maxLevel     = 3,
                    costPerLevel = new[] { 5, 10, 20 }
                }
            };
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>Intenta mejorar un gen gastando Puntos Evolutivos.</summary>
        public bool UpgradeGene(string geneId)
        {
            var gene = FindGene(geneId);
            if (gene == null)
            {
                Debug.LogWarning($"[GeneManager] Gen '{geneId}' no encontrado.");
                return false;
            }
            if (gene.currentLevel >= gene.maxLevel)
            {
                Debug.Log($"[GeneManager] '{geneId}' ya está al nivel máximo.");
                return false;
            }

            int cost = gene.costPerLevel[gene.currentLevel];
            if (_evolutionPoints < cost)
            {
                Debug.Log($"[GeneManager] PE insuficientes para '{geneId}' (costo {cost}, tienes {_evolutionPoints}).");
                return false;
            }

            _evolutionPoints -= cost;
            gene.currentLevel++;

            ApplyGeneEffect(gene, gene.currentLevel);
            Save();

            Debug.Log($"[GeneManager] '{geneId}' mejorado a nivel {gene.currentLevel}.");
            return true;
        }

        /// <summary>Añade Puntos Evolutivos (al completar era, partida, etc.).</summary>
        public void AddEvolutionPoints(int amount)
        {
            _evolutionPoints += amount;
            PlayerPrefs.SetInt(PEKey, _evolutionPoints);
            PlayerPrefs.Save();
            Debug.Log($"[GeneManager] +{amount} PE → Total: {_evolutionPoints}");
        }

        /// <summary>Registra una mutación de Deriva Genética (llamado por UpgradeTree).</summary>
        public void RegisterGeneticDrift(string lineage, string bonusType, float value)
        {
            _driftRecords.Add(new GeneticDriftRecord
            {
                lineage   = lineage,
                bonusType = bonusType,
                value     = value
            });
            // La Deriva Genética concede 1 PE extra
            AddEvolutionPoints(1);
        }

        /// <summary>Llamado por UpgradeTree al completar un nodo.</summary>
        public void OnNodeUnlocked(string nodeId)
        {
            // Cada nodo investigado regala 1 PE
            AddEvolutionPoints(1);
        }

        /// <summary>Llamado por GameManager al completar la partida.</summary>
        public void OnSessionCompleted(int erasReached)
        {
            AddEvolutionPoints(erasReached * 2);
        }

        public GeneData[] GetAllGenes() => geneDefinitions;

        public GeneData GetGene(string geneId) => FindGene(geneId);

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Efectos pasivos

        /// <summary>Aplica todos los efectos pasivos de genes desbloqueados al inicio.</summary>
        private void ApplyPassiveEffects()
        {
            if (geneDefinitions == null) return;
            foreach (var gene in geneDefinitions)
            {
                for (int lvl = 1; lvl <= gene.currentLevel; lvl++)
                    ApplyGeneEffect(gene, lvl);
            }
        }

        private void ApplyGeneEffect(GeneData gene, int level)
        {
            switch (gene.id)
            {
                case "GenResistance":
                    // -5% daño recibido por orgánulos por nivel
                    GlobalUpgradeFlags.MembraneResistanceBonus += 0.05f;
                    break;

                case "GenMitochondrial":
                    // +10% producción de ATP
                    Core.ResourceManager.Instance?.SetProductionMultiplier(
                        Core.ResourceType.ATP,
                        1f + 0.10f * level);
                    break;

                case "GenImmune":
                    // +5% daño de lisosomas (via flag)
                    GlobalUpgradeFlags.UnitDamageBonus += 0.05f;
                    break;

                case "GenMemory":
                    // +10 ARN inicial — se aplica solo en nivel 1 para no acumular en re-aplicaciones
                    if (level == 1)
                        Core.ResourceManager.Instance?.AddResource(Core.ResourceType.GenomicPoints, 10f);
                    break;

                case "GenAncestral":
                    if (level >= 3)
                        GlobalUpgradeFlags.UnlockedUnitTypes.Add("ProtoArchaea");
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Persistencia (PlayerPrefs + JSON)

        [System.Serializable]
        private class SaveData
        {
            public int[]   geneLevels;
            public int     evolutionPoints;
        }

        private void Save()
        {
            if (geneDefinitions == null) return;
            var sd = new SaveData
            {
                geneLevels      = new int[geneDefinitions.Length],
                evolutionPoints = _evolutionPoints
            };
            for (int i = 0; i < geneDefinitions.Length; i++)
                sd.geneLevels[i] = geneDefinitions[i].currentLevel;

            string json = JsonUtility.ToJson(sd);
            PlayerPrefs.SetString(SaveKey, json);
            PlayerPrefs.SetInt(PEKey, _evolutionPoints);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            _evolutionPoints = PlayerPrefs.GetInt(PEKey, 0);

            if (!PlayerPrefs.HasKey(SaveKey)) return;
            string json = PlayerPrefs.GetString(SaveKey);
            var sd = JsonUtility.FromJson<SaveData>(json);
            if (sd == null || sd.geneLevels == null) return;

            for (int i = 0; i < geneDefinitions.Length && i < sd.geneLevels.Length; i++)
                geneDefinitions[i].currentLevel = sd.geneLevels[i];

            _evolutionPoints = sd.evolutionPoints;
        }

        /// <summary>Borra toda la meta-progresión (botón "Reiniciar" en menú).</summary>
        public void ResetAllProgress()
        {
            PlayerPrefs.DeleteKey(SaveKey);
            PlayerPrefs.DeleteKey(PEKey);
            _evolutionPoints = 0;
            foreach (var g in geneDefinitions)
                g.currentLevel = 0;
            GlobalUpgradeFlags.Reset();
            Debug.Log("[GeneManager] Meta-progresión reiniciada.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private GeneData FindGene(string geneId)
        {
            if (geneDefinitions == null) return null;
            foreach (var g in geneDefinitions)
                if (g.id == geneId) return g;
            return null;
        }

        #endregion
    }
}
