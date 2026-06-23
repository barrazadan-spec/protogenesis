using System;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Slots;

namespace Protogenesis.Progression
{
    /// <summary>
    /// SpecializationTree — Árbol de desbloqueos post-consolidación (Primordia, Prompt 4.3).
    ///
    /// Estructura:
    ///   · Cada una de las 16 especializaciones tiene 3 nodos en cadena lineal (Tier 1→2→3).
    ///   · Tier 1: 20 Nucleotides  — mejora base, desbloqueada inmediatamente tras consolidar.
    ///   · Tier 2: 35 Nucleotides  — requiere Tier 1 desbloqueado.
    ///   · Tier 3: 55 Nucleotides  — requiere Tier 2, efecto definitorio de la especialización.
    ///
    ///   Total: 48 nodos (solo los 3 de la especialización consolidada son accesibles).
    ///
    /// Condiciones de desbloqueo:
    ///   · La especialización del nodo debe estar consolidada en CellState.
    ///   · El Tier anterior debe estar desbloqueado.
    ///   · ResourceManager.Consume(Nucleotides, coste) debe tener éxito.
    ///
    /// Los efectos se aplican a CellEntity en el momento del desbloqueo (permanentes).
    ///
    /// ID de nodo: "{specIndex}_{tier}"  →  e.g. "0_1", "0_2", "0_3", "3_1" …
    /// </summary>
    public class SpecializationTree : MonoBehaviour
    {
        public static SpecializationTree Instance { get; private set; }

        // ── Definición de nodo ────────────────────────────────────────────────────
        public struct NodeDef
        {
            public string   Id;
            public string   DisplayName;
            public string   Description;
            public float    NucleotideCost;
            public int      Tier;           // 1, 2 ó 3
            public Action<CellEntity> ApplyBonus;
        }

        // ── Estado ────────────────────────────────────────────────────────────────
        private readonly HashSet<string>              _unlocked = new HashSet<string>();
        private readonly Dictionary<string, NodeDef>  _nodes    = new Dictionary<string, NodeDef>();

        // Costes por tier
        private const float CostT1 = 20f;
        private const float CostT2 = 35f;
        private const float CostT3 = 55f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            BuildAllNodes();
        }

        private void OnEnable()
        {
            EventBus.OnSpecializationConsolidated += OnConsolidated;
        }

        private void OnDisable()
        {
            EventBus.OnSpecializationConsolidated -= OnConsolidated;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Construcción del árbol

        private void BuildAllNodes()
        {
            // ── Energía ──────────────────────────────────────────────────────────
            Add(0, 1, "Clorofila Primitiva",     "ATP +20% en zona Fótica.",
                ce => ce.AddATPMult(0.20f));
            Add(0, 2, "Fotosistema II",          "ATP +15% global. Percepción de luz.",
                ce => { ce.AddATPMult(0.15f); ce.EnablePhotoreception(); });
            Add(0, 3, "Fijación de CO₂",         "ATP +20% global. HP regen +1/s.",
                ce => { ce.AddATPMult(0.20f); ce.AddHPRegen(1f); });

            Add(1, 1, "Oxidorreductasa",         "ATP +15%. Reducción de daño +8%.",
                ce => { ce.AddATPMult(0.15f); ce.AddDamageReduction(0.08f); });
            Add(1, 2, "Litotrofía Profunda",     "ATP +15%. Velocidad +10% en zona Béntica.",
                ce => { ce.AddATPMult(0.15f); ce.AddMoveSpeedMult(0.10f); });
            Add(1, 3, "Ciclo del Azufre",        "ATP +20%. Estrés térmico -30%.",
                ce => ce.AddATPMult(0.20f));   // reducción de ThermalStress se gestiona en StressSystem

            Add(2, 1, "Glucólisis Mejorada",     "Fermentación +20%. ATP +10%.",
                ce => { ce.AddFermentationBonus(0.20f); ce.AddATPMult(0.10f); });
            Add(2, 2, "Vía Pentosa Fosfato",     "Fermentación +20%. Nucleotides +10% producción.",
                ce => ce.AddFermentationBonus(0.20f));
            Add(2, 3, "Fermentación Mixta",      "ATP +25% sin O₂. HP regen +1.5/s.",
                ce => { ce.AddATPMult(0.25f); ce.AddHPRegen(1.5f); });

            // ── Movilidad ────────────────────────────────────────────────────────
            Add(3, 1, "Flagelo Simple",          "Velocidad +20%.",
                ce => ce.AddMoveSpeedMult(0.20f));
            Add(3, 2, "Flagelo Helicoidal",      "Velocidad +20%. Radio de percepción +1u.",
                ce => { ce.AddMoveSpeedMult(0.20f); ce.AddPerceptionRadius(1f); });
            Add(3, 3, "Multiflagelación",        "Velocidad +25%. Daño de choque +20%.",
                ce => { ce.AddMoveSpeedMult(0.25f); ce.AddDamageMultiplier(0.20f); });

            Add(4, 1, "Pseudópodo Corto",        "Rango de fagocitosis +0.5u.",
                ce => ce.AddFagocytosisRange(0.5f));
            Add(4, 2, "Reticulopodio",           "Rango de fagocitosis +0.5u. Velocidad +10%.",
                ce => { ce.AddFagocytosisRange(0.5f); ce.AddMoveSpeedMult(0.10f); });
            Add(4, 3, "Filopodio Sensorial",     "Rango de fagocitosis +1u. Quimiorreceptores.",
                ce => { ce.AddFagocytosisRange(1f); ce.EnableChemoreception(); });

            // ── Protección ───────────────────────────────────────────────────────
            Add(5, 1, "Quiste Básico",           "HP regen +2/s en estado estático.",
                ce => ce.AddHPRegen(2f));
            Add(5, 2, "Quiste Reforzado",        "HP regen +2/s. Reducción de daño +15%.",
                ce => { ce.AddHPRegen(2f); ce.AddDamageReduction(0.15f); });
            Add(5, 3, "Esporo de Resistencia",   "HP max +30. Reducción de daño +10%.",
                ce => { ce.AddMaxHPBonus(30f); ce.AddDamageReduction(0.10f); });

            Add(6, 1, "Quitina Básica",          "Reducción de daño +10%. HP +10.",
                ce => { ce.AddDamageReduction(0.10f); ce.AddMaxHPBonus(10f); });
            Add(6, 2, "Quitina Cristalina",      "Reducción de daño +10%. HP +15.",
                ce => { ce.AddDamageReduction(0.10f); ce.AddMaxHPBonus(15f); });
            Add(6, 3, "Coraza de Silicio",       "Reducción de daño +15%. HP +20.",
                ce => { ce.AddDamageReduction(0.15f); ce.AddMaxHPBonus(20f); });

            // ── Alimentación ─────────────────────────────────────────────────────
            Add(7, 1, "Fagocitosis Básica",      "Biomasa +20%. Rango +0.5u.",
                ce => { ce.AddBiomassMult(0.20f); ce.AddFagocytosisRange(0.5f); });
            Add(7, 2, "Endocitosis Mediada",     "Biomasa +20%. AminoAcids +15%.",
                ce => ce.AddBiomassMult(0.20f));
            Add(7, 3, "Macrofagocitosis",        "Biomasa +25%. Daño +15%.",
                ce => { ce.AddBiomassMult(0.25f); ce.AddDamageMultiplier(0.15f); });

            Add(8, 1, "Canal Osmótico",          "Filtración +1.5/s.",
                ce => ce.AddFiltrationRate(1.5f));
            Add(8, 2, "Acuaporina",              "Filtración +1.5/s. Velocidad +5%.",
                ce => { ce.AddFiltrationRate(1.5f); ce.AddMoveSpeedMult(0.05f); });
            Add(8, 3, "Ósmosis Activa",          "Filtración +2/s. HP regen +1/s.",
                ce => { ce.AddFiltrationRate(2f); ce.AddHPRegen(1f); });

            Add(9, 1, "Cilio Filtrante",         "Filtración +2/s.",
                ce => ce.AddFiltrationRate(2f));
            Add(9, 2, "Red de Cilios",           "Filtración +2/s. Rango percepción +1u.",
                ce => { ce.AddFiltrationRate(2f); ce.AddPerceptionRadius(1f); });
            Add(9, 3, "Trampa Mucilaginosa",     "Filtración +3/s. Biomasa +15%.",
                ce => { ce.AddFiltrationRate(3f); ce.AddBiomassMult(0.15f); });

            // ── Reproducción ─────────────────────────────────────────────────────
            Add(10, 1, "Esporo Básico",          "Reproducción +20%. HP +10.",
                ce => { ce.AddReproductionRate(0.20f); ce.AddMaxHPBonus(10f); });
            Add(10, 2, "Esporulación Múltiple",  "Reproducción +20%. Colonia +1.",
                ce => { ce.AddReproductionRate(0.20f); ce.AddColonySize(1); });
            Add(10, 3, "Hipno-espora",           "Reproducción +25%. HP regen +1.5/s.",
                ce => { ce.AddReproductionRate(0.25f); ce.AddHPRegen(1.5f); });

            Add(11, 1, "Pilus de Conjugación",   "Reproducción +25%.",
                ce => ce.AddReproductionRate(0.25f));
            Add(11, 2, "Transferencia de ADN",   "Reproducción +20%. Colonia +1.",
                ce => { ce.AddReproductionRate(0.20f); ce.AddColonySize(1); });
            Add(11, 3, "Conjugación Horizontal", "Reproducción +25%. ATP +10%.",
                ce => { ce.AddReproductionRate(0.25f); ce.AddATPMult(0.10f); });

            // ── Social ───────────────────────────────────────────────────────────
            Add(12, 1, "Matriz Extracelular",    "Colonia +1. HP regen +1/s.",
                ce => { ce.AddColonySize(1); ce.AddHPRegen(1f); });
            Add(12, 2, "Canal de Comunicación",  "Radio quorum +1.5u. Colonia +1.",
                ce => { ce.AddQuorumRadius(1.5f); ce.AddColonySize(1); });
            Add(12, 3, "Biofilm Maduro",         "Colonia +2. Reducción daño +10%.",
                ce => { ce.AddColonySize(2); ce.AddDamageReduction(0.10f); });

            Add(13, 1, "Señal Química",          "Radio quorum +1u. ATP +10%.",
                ce => { ce.AddQuorumRadius(1f); ce.AddATPMult(0.10f); });
            Add(13, 2, "Coevolución",            "Radio quorum +1.5u. Biomasa +15%.",
                ce => { ce.AddQuorumRadius(1.5f); ce.AddBiomassMult(0.15f); });
            Add(13, 3, "Mutualismo Obligado",    "Radio quorum +2u. HP regen +2/s.",
                ce => { ce.AddQuorumRadius(2f); ce.AddHPRegen(2f); });

            // ── Sensorial ────────────────────────────────────────────────────────
            Add(14, 1, "Quimiorreceptor Básico", "Percepción +1.5u.",
                ce => ce.AddPerceptionRadius(1.5f));
            Add(14, 2, "Gradiente Químico",      "Percepción +1.5u. Velocidad +10%.",
                ce => { ce.AddPerceptionRadius(1.5f); ce.AddMoveSpeedMult(0.10f); });
            Add(14, 3, "Red Quimiosensorial",    "Percepción +2u. Quimiorreceptores activos.",
                ce => { ce.AddPerceptionRadius(2f); ce.EnableChemoreception(); });

            Add(15, 1, "Fotorreceptor Simple",   "Percepción +1u. Fotorreceptores activos.",
                ce => { ce.AddPerceptionRadius(1f); ce.EnablePhotoreception(); });
            Add(15, 2, "Opsina de Membrana",     "Percepción +1.5u. ATP +8%.",
                ce => { ce.AddPerceptionRadius(1.5f); ce.AddATPMult(0.08f); });
            Add(15, 3, "Fotonavegación",         "Percepción +2u. Velocidad +15% en Fótica.",
                ce => { ce.AddPerceptionRadius(2f); ce.AddMoveSpeedMult(0.15f); });
        }

        private void Add(int specIdx, int tier, string name, string desc, Action<CellEntity> bonus)
        {
            string id = $"{specIdx}_{tier}";
            float cost = tier switch { 1 => CostT1, 2 => CostT2, _ => CostT3 };
            _nodes[id] = new NodeDef
            {
                Id            = id,
                DisplayName   = name,
                Description   = desc,
                NucleotideCost= cost,
                Tier          = tier,
                ApplyBonus    = bonus
            };
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Intenta desbloquear un nodo del árbol.
        /// Requiere: especialización consolidada, tier anterior desbloqueado, Nucleotides suficientes.
        /// </summary>
        public bool TryUnlock(string nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) return false;
            if (_unlocked.Contains(nodeId)) return false;

            int specIdx = ParseSpecIdx(nodeId);
            var specType = (SpecializationType)specIdx;

            // ¿Especialización consolidada?
            var cs = Views.CellState.Instance;
            if (cs == null || !cs.IsSpecializationConsolidated) return false;
            if (cs.ConsolidatedSpecialization != specType)       return false;

            // ¿Tier anterior desbloqueado?
            if (node.Tier > 1 && !_unlocked.Contains($"{specIdx}_{node.Tier - 1}")) return false;

            // ¿Recursos suficientes?
            var rm = ResourceManager.Instance;
            if (rm == null || !rm.Consume(ResourceType.Nucleotides, node.NucleotideCost)) return false;

            // Desbloquear
            _unlocked.Add(nodeId);
            node.ApplyBonus?.Invoke(CellEntity.Instance);

            EventBus.TriggerUpgradeUnlocked(nodeId);
            Debug.Log($"[SpecializationTree] Nodo desbloqueado: {node.DisplayName} ({nodeId})");
            return true;
        }

        /// <summary>True si el nodo ya fue desbloqueado.</summary>
        public bool IsUnlocked(string nodeId) => _unlocked.Contains(nodeId);

        /// <summary>True si el nodo puede desbloquearse ahora (sin verificar recursos).</summary>
        public bool IsAvailable(string nodeId)
        {
            if (!_nodes.TryGetValue(nodeId, out var node)) return false;
            if (_unlocked.Contains(nodeId)) return false;

            int specIdx = ParseSpecIdx(nodeId);
            var cs = Views.CellState.Instance;
            if (cs == null || !cs.IsSpecializationConsolidated) return false;
            if (cs.ConsolidatedSpecialization != (SpecializationType)specIdx) return false;

            if (node.Tier > 1 && !_unlocked.Contains($"{specIdx}_{node.Tier - 1}")) return false;
            return true;
        }

        /// <summary>Devuelve los 3 nodos de la especialización indicada (Tier 1, 2, 3).</summary>
        public NodeDef[] GetNodesFor(SpecializationType type)
        {
            int idx = (int)type;
            return new[]
            {
                _nodes[$"{idx}_1"],
                _nodes[$"{idx}_2"],
                _nodes[$"{idx}_3"],
            };
        }

        /// <summary>Devuelve los 3 nodos de la especialización consolidada actualmente.</summary>
        public NodeDef[] GetConsolidatedNodes()
        {
            var cs = Views.CellState.Instance;
            if (cs == null || !cs.IsSpecializationConsolidated) return Array.Empty<NodeDef>();
            return GetNodesFor(cs.ConsolidatedSpecialization.Value);
        }

        /// <summary>Cantidad de nodos desbloqueados de la especialización dada (0-3).</summary>
        public int UnlockedTier(SpecializationType type)
        {
            int idx = (int)type;
            for (int t = 3; t >= 1; t--)
                if (_unlocked.Contains($"{idx}_{t}")) return t;
            return 0;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private static int ParseSpecIdx(string nodeId)
            => int.Parse(nodeId.Split('_')[0]);

        private void OnConsolidated(SpecializationType type, string displayName)
        {
            // Tier 1 se ofrece automáticamente; no se desbloquea solo, el jugador debe pagarlo.
            Debug.Log($"[SpecializationTree] Árbol disponible para {displayName}. " +
                      $"Tier 1 desbloqueables con {CostT1} Nucleotides.");
        }

        #endregion
    }
}
