using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Player;
using Protogenesis.Slots;

namespace Protogenesis.Progression
{
    /// <summary>
    /// GeneticTreeSystem — Árbol genético de mutaciones in-run (GDD v4.6 §Fase 4).
    ///
    /// Capa separada del UpgradeTree (mejoras estructurales). El árbol genético
    /// gestiona MUTACIONES fenotípicas que amplifican el camino evolutivo elegido.
    ///
    /// Estructura: 4 ramas × 6 nodos = 24 nodos totales.
    ///   ADAPTACIÓN       — universal, accesible siempre
    ///   METABOLISMO_GEN  — amplifica rutas del MetabolismEngine
    ///   ESPECIALIZACIÓN  — bloqueada por fenotipo; amplifica el fenotipo activo
    ///   SIMBIOSIS        — desbloquea mecánicas cooperativas (ERA 3+)
    ///
    /// Moneda: GenomicPoints (GP).
    /// Cada nodo se desbloquea instantáneamente al costear su precio.
    /// Prerequisito: el nodo anterior de su rama (columna 0 no tiene requisito).
    ///
    /// Efectos aplicados directamente a:
    ///   InstabilitySystem, MetabolicHeatSystem, BalanceSystem,
    ///   PhenotypeSystem, ResourceManager, EnvironmentManager.
    /// </summary>
    public class GeneticTreeSystem : MonoBehaviour
    {
        public static GeneticTreeSystem Instance { get; private set; }

        // ── Nodo genético (runtime, sin ScriptableObject) ─────────────────────────
        public class GeneticNode
        {
            public string          id;
            public string          displayName;
            public string          description;
            public GeneticBranch   branch;
            public int             column;         // 0-5 dentro de la rama
            public float           costGP;         // Costo en GenomicPoints
            public object          requiredPhenotype;  // Unknown = sin restricción
            public int             requiredEra;
            public GeneticEffect   effect;
            public float           effectValue;
            public bool            isUnlocked;
        }

        public enum GeneticBranch
        {
            Adaptacion,
            MetabolismoGen,
            Especializacion,
            Simbiosis
        }

        public enum GeneticEffect
        {
            // ADAPTACIÓN
            ReduceInstability,          // -X Instabilidad máx (baja el cap o reduce tasa)
            ReduceHeatGeneration,       // -X% calor generado por ATP
            EnvironmentTolerance,       // +X% modificador ambiental global
            AdaptationDebtReduction,    // -X% deuda de adaptación al instalar slots
            StallResistance,            // -X% multiplicador Anti-Stall
            FarmingDecayReduction,      // -X% tasa de FarmingDecay

            // METABOLISMO GEN
            RouteEfficiencyBoost,       // +X% ATP en todas las rutas
            FermentationUnlock,         // Fermentación produce +X Biomasa extra
            PhotosynthesisAmplify,      // Fotosíntesis produce +X O2/tick
            MixotrophyUnlock,           // Mixotrofia reduce consumo Glucosa -X%
            LithotrophyUnlock,          // Quimiolitotrofia reduce consumo Nitrógeno -X%
            ATPCriticalThreshold,       // Reduce umbral crítico de ATP de 15 a X

            // ESPECIALIZACIÓN (bloqueadas por fenotipo)
            SpeedMutation,              // BacteriaVeloz: +X% velocidad CAP
            PhotoMutation,              // Microalga: O2 producido se convierte parcialmente en ATP
            SilicaMutation,             // Diatomea: pérdida de Sílice -X% por daño
            PredatorMutation,           // Ameba: daño recibido de presas -X%
            CiliaMutation,              // Paramecio: stun de cilios propio -X% cooldown
            ColonyMutation,             // Biofilm: InstabilitySystem.AddInstability reducida -X%
            RegenMutation,              // Hidra: regeneración HP +X/seg
            ExtremophileMutation,       // Tardígrado: resistencia a calor +X%
            MulticelularMutation,       // Multicel: aura CAP radio +X

            // SIMBIOSIS
            QuorumBoost,                // +X% efectividad de QuorumSensor
            SymbioticAura,              // Aura CAP da +X HP/seg a orgánulos cercanos
            ColectiveDNA,               // Ganar +X GP por cada orgánulo activo/min
            HorizontalGeneTransfer,     // Al cambiar fenotipo, conservar 50% bonuses
            EndosymbiosisUnlock         // Desbloquea Mitocondria nivel 4 (extrasimbiótica)
        }

        // ── Catálogo runtime ──────────────────────────────────────────────────────
        private readonly List<GeneticNode> _catalog = new List<GeneticNode>();
        private readonly HashSet<string>   _unlocked = new HashSet<string>();

        public IReadOnlyList<GeneticNode> Catalog  => _catalog;
        public IReadOnlyCollection<string> Unlocked => _unlocked;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            BuildCatalog();
        }

        private void Start()
        {
            // El Protonúcleo se instala en SceneBootstrap.Start() — leer el nivel aquí
            int nucleusLevel = GetNucleusLevel();
            if (nucleusLevel > 0)
                Debug.Log($"[GeneticTreeSystem] Árbol genético disponible — límite: {MaxUnlocksForNucleus} upgrades.");
            else
                Debug.Log("[GeneticTreeSystem] Árbol genético bloqueado — instala un Núcleo en slot Social.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Construcción del catálogo

        private void BuildCatalog()
        {
            // ── RAMA ADAPTACIÓN ──────────────────────────────────────────────────
            Add("ADA_01", "Resistencia Ácida I",    GeneticBranch.Adaptacion, 0,
                "Tasa inestabilidad -15%/s\nEfecto continuo (sin coste metabólico)\nSinergia: ninguna — universal",
                GeneticEffect.ReduceInstability, 0.15f, 12f);

            Add("ADA_02", "Regulación Térmica I",  GeneticBranch.Adaptacion, 1,
                "Calor generado por ATP -20%\nRequiere: producción ATP activa\nSinergia: Mitocondria Nv1+",
                GeneticEffect.ReduceHeatGeneration, 0.20f, 20f, prereq: "ADA_01");

            Add("ADA_03", "Adaptación a Zona Ácida", GeneticBranch.Adaptacion, 2,
                "Tolerancia ambiental global +15%\nElimina penalización en pH bajo\nSinergia: cualquier slot de Energía",
                GeneticEffect.EnvironmentTolerance, 0.15f, 30f, prereq: "ADA_02");

            Add("ADA_04", "Plasticidad Fenotípica", GeneticBranch.Adaptacion, 3,
                "Deuda de adaptación -25% por instalación\nReducción costes de cambio fenotípico\nSinergia: slots Nv2+",
                GeneticEffect.AdaptationDebtReduction, 0.25f, 40f, prereq: "ADA_03");

            Add("ADA_05", "Persistencia Metabólica", GeneticBranch.Adaptacion, 4,
                "Multiplicador anti-stall -30%\nCombate prolongado pierde menos ATP\nSinergia: Mitocondria Nv1+",
                GeneticEffect.StallResistance, 0.30f, 50f, prereq: "ADA_04");

            Add("ADA_06", "Movilidad Oportunista", GeneticBranch.Adaptacion, 5,
                "El FarmingDecay se acumula un 40% más lento.",
                GeneticEffect.FarmingDecayReduction, 0.40f, 60f, prereq: "ADA_05");

            // ── RAMA METABOLISMO GEN ─────────────────────────────────────────────
            Add("MET_01", "Ciclo ATP Eficiente",    GeneticBranch.MetabolismoGen, 0,
                "Producción ATP +10% en todas las rutas\nEfecto pasivo permanente\nSinergia: Mitocondria / Cloroplasto",
                GeneticEffect.RouteEfficiencyBoost, 0.10f, 15f);

            Add("MET_02", "Optimización Anaeróbica", GeneticBranch.MetabolismoGen, 1,
                "Fermentación produce +3 Biomasa/tick\nPotencia ruta sin O2\nSinergia: Fermentación instalada",
                GeneticEffect.FermentationUnlock, 3f, 20f, prereq: "MET_01");

            Add("MET_03", "Clorofila Mejorada",    GeneticBranch.MetabolismoGen, 2,
                "Fotosíntesis produce +0.5 O2 extra por tick.",
                GeneticEffect.PhotosynthesisAmplify, 0.5f, 30f, prereq: "MET_02");

            Add("MET_04", "Mixotrofia Eficiente",  GeneticBranch.MetabolismoGen, 3,
                "Mixotrofia consume un 25% menos de Glucosa.",
                GeneticEffect.MixotrophyUnlock, 0.25f, 40f, prereq: "MET_03");

            Add("MET_05", "Litotrofia Avanzada",   GeneticBranch.MetabolismoGen, 4,
                "Quimiolitotrofia consume un 30% menos de Nitrógeno.",
                GeneticEffect.LithotrophyUnlock, 0.30f, 50f, prereq: "MET_04");

            Add("MET_06", "Resiliencia Energética",GeneticBranch.MetabolismoGen, 5,
                "El umbral de ATP crítico baja de 15 a 8.",
                GeneticEffect.ATPCriticalThreshold, 8f, 80f, prereq: "MET_05");

            // ── RAMA ESPECIALIZACIÓN (fenotipo-específica) ───────────────────────
            // TODO: Primordia — requiredPhenotype args comentados hasta migrar PhenotypeType
            Add("ESP_01", "Flagelo Superveloz",    GeneticBranch.Especializacion, 0,
                "BacteriaVeloz: velocidad CAP +20%.", // Primordia stub
                GeneticEffect.SpeedMutation, 0.20f, 25f);

            Add("ESP_02", "Sensibilidad Lumínica Avanzada", GeneticBranch.Especializacion, 0,
                "O2 producido genera +15% ATP adicional\nOptimiza cloroplasto existente, no crea uno\nSinergia: Cloroplasto Nv1+",
                GeneticEffect.PhotoMutation, 0.15f, 25f);

            Add("ESP_03", "Frústula Reforzada",    GeneticBranch.Especializacion, 0,
                "Diatomea: la pérdida de Sílice por daño se reduce un 40%.",
                GeneticEffect.SilicaMutation, 0.40f, 25f);

            Add("ESP_04", "Pseudópodo Armado",     GeneticBranch.Especializacion, 0,
                "Ameba: el daño recibido de presas se reduce un 20%.",
                GeneticEffect.PredatorMutation, 0.20f, 25f);

            Add("ESP_05", "Sincronía Ciliar",      GeneticBranch.Especializacion, 0,
                "Paramecio (Ciliar): stun de cilios recarga un 30% más rápido.",
                GeneticEffect.CiliaMutation, 0.30f, 25f);

            Add("ESP_06", "Señal de Quorum+",      GeneticBranch.Especializacion, 0,
                "Biofilm: la inestabilidad generada por el jugador se reduce un 25%.",
                GeneticEffect.ColonyMutation, 0.25f, 25f);

            Add("ESP_07", "Regeneración Hidra",    GeneticBranch.Especializacion, 0,
                "Hidra: la CAP regenera +1 HP/seg.", // Primordia stub
                GeneticEffect.RegenMutation, 1f, 25f);

            Add("ESP_08", "Proteínas Tun Mejoradas",GeneticBranch.Especializacion, 0,
                "Tardígrado: resistencia al calor metabólico +30%.",
                GeneticEffect.ExtremophileMutation, 0.30f, 25f);

            Add("ESP_09", "Tejido Coordinado",     GeneticBranch.Especializacion, 0,
                "Multicelular: el aura de la CAP se expande +1.5 unidades.",
                GeneticEffect.MulticelularMutation, 1.5f, 35f);

            // ── RAMA SIMBIOSIS ────────────────────────────────────────────────────
            Add("SIM_01", "Señalización Mejorada", GeneticBranch.Simbiosis, 0,
                "QuorumSensor opera un 20% más eficazmente.",
                GeneticEffect.QuorumBoost, 0.20f, 20f, requiredEra: 1);

            Add("SIM_02", "Aura Simbiótica",       GeneticBranch.Simbiosis, 1,
                "El aura de la CAP regenera +0.5 HP/seg a orgánulos cercanos.", // Primordia stub
                GeneticEffect.SymbioticAura, 0.5f, 30f,
                prereq: "SIM_01", requiredEra: 2);

            Add("SIM_03", "ADN Colectivo",         GeneticBranch.Simbiosis, 2,
                "Cada orgánulo activo genera +0.1 GenomicPoints/min.",
                GeneticEffect.ColectiveDNA, 0.1f, 40f,
                prereq: "SIM_02", requiredEra: 2);

            Add("SIM_04", "Transferencia Génica",  GeneticBranch.Simbiosis, 3,
                "Al cambiar de fenotipo se conserva el 50% de los bonuses anteriores.",
                GeneticEffect.HorizontalGeneTransfer, 0.50f, 60f,
                prereq: "SIM_03", requiredEra: 3);

            Add("SIM_05", "Endosimbiosis",         GeneticBranch.Simbiosis, 4,
                "Desbloquea el slot de Mitocondria nivel 4 (extrasimbiótica).",
                GeneticEffect.EndosymbiosisUnlock, 1f, 100f,
                prereq: "SIM_04", requiredEra: 3);

            Debug.Log($"[GeneticTreeSystem] Catálogo construido: {_catalog.Count} nodos.");
        }

        private void Add(
            string id, string displayName,
            GeneticBranch branch, int column,
            string description,
            GeneticEffect effect, float effectValue, float costGP,
            string prereq = null,
            object requiredPhenotype = null,
            int requiredEra = 0)
        {
            _catalog.Add(new GeneticNode
            {
                id               = id,
                displayName      = displayName,
                description      = description,
                branch           = branch,
                column           = column,
                costGP           = costGP,
                requiredPhenotype = requiredPhenotype,
                requiredEra      = requiredEra,
                effect           = effect,
                effectValue      = effectValue,
                isUnlocked       = false,
                // prerequisito guardado como campo de conveniencia
            });
            // Almacena prerequisito en la descripción internamente (simple lookup)
            if (!string.IsNullOrEmpty(prereq))
                _prereqMap[id] = prereq;
        }

        private readonly Dictionary<string, string> _prereqMap = new Dictionary<string, string>();

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Intenta desbloquear un nodo genético.
        /// Valida era, fenotipo, prerequisito y GP disponibles.
        /// </summary>
        public bool TryUnlock(string nodeId)
        {
            var node = FindNode(nodeId);
            if (node == null) { Debug.LogWarning($"[GeneticTree] Nodo '{nodeId}' no encontrado."); return false; }
            if (node.isUnlocked) { Debug.Log($"[GeneticTree] '{nodeId}' ya desbloqueado."); return false; }

            if (!CanUnlock(node, out string reason))
            {
                Debug.Log($"[GeneticTree] No se puede desbloquear '{nodeId}': {reason}");
                return false;
            }

            var rm = ResourceManager.Instance;
            if (rm == null || !rm.CanAfford(ResourceType.GenomicPoints, node.costGP))
            {
                Debug.Log($"[GeneticTree] GP insuficientes para '{nodeId}' ({node.costGP} GP).");
                return false;
            }

            rm.ConsumeResource(ResourceType.GenomicPoints, node.costGP);
            node.isUnlocked = true;
            _unlocked.Add(nodeId);

            ApplyEffect(node);
            EventBus.TriggerNucleoInstalled(nodeId.GetHashCode());  // Reutiliza evento NucleoInstalled
            Debug.Log($"[GeneticTree] '{node.displayName}' desbloqueado.");
            return true;
        }

        public bool IsUnlocked(string nodeId) => _unlocked.Contains(nodeId);

        /// <summary>
        /// Devuelve el nivel de Núcleo instalado en el slot Social.
        /// 0 = ninguno, 1 = Protonucleo (max 2 desbloqueos),
        /// 2 = Nucleo (max 6), 3 = NucleoAvanzado (max 10).
        /// </summary>
        public int GetNucleusLevel()
        {
            var sm = SlotManager.Instance;
            if (sm == null) return 0;

            int maxSlots = sm.GetMaxSlots(SlotType.Social);
            for (int i = 0; i < maxSlots; i++)
            {
                var slot = sm.GetInstalled(SlotType.Social, i);
                if (slot == null) continue;
                string id = slot.slotId ?? slot.displayName ?? "";
                if (id.Contains("NucleoAvanzado") || id.Contains("Nucleo Avanzado")) return 3;
                if (id.Contains("Nucleo") || id.Contains("Núcleo"))                  return 2;
                if (id.Contains("Protonucleo") || id.Contains("Protonúcleo"))        return 1;
            }
            return 0;
        }

        /// <summary>Máximo de desbloqueos permitidos según el Núcleo instalado.</summary>
        public int MaxUnlocksForNucleus => GetNucleusLevel() switch
        {
            1 => 2,
            2 => 6,
            3 => 10,
            _ => 0
        };

        public bool CanUnlock(GeneticNode node, out string reason)
        {
            reason = "";

            // Núcleo prerequisite
            int nucleusLevel = GetNucleusLevel();
            if (nucleusLevel == 0)
            { reason = "Requiere Núcleo en slot Social"; return false; }

            if (_unlocked.Count >= MaxUnlocksForNucleus)
            { reason = $"Límite de {MaxUnlocksForNucleus} desbloqueos (nivel de Núcleo)"; return false; }

            // Era
            var gm = GameManager.Instance;
            if (gm != null && gm.CurrentEra < node.requiredEra)
            { reason = $"Requiere ERA {node.requiredEra}"; return false; }

            // Fenotipo
            // TODO: Primordia — if (node.requiredPhenotype != PhenotypeType.Unknown)
            {
                // TODO: Primordia — var ps = PhenotypeSystem.Instance;
                object ps = null; // Primordia migration stub
                // TODO: Primordia — if (ps == null || ps.CurrentPhenotype != node.requiredPhenotype)
                { reason = $"Requiere fenotipo {node.requiredPhenotype}"; return false; }
            }

            // Prerequisito
            if (_prereqMap.TryGetValue(node.id, out string prereq) &&
                !_unlocked.Contains(prereq))
            { reason = $"Requiere '{prereq}'"; return false; }

            return true;
        }

        public GeneticNode FindNode(string nodeId)
        {
            foreach (var n in _catalog)
                if (n.id == nodeId) return n;
            return null;
        }

        public List<GeneticNode> GetBranch(GeneticBranch branch)
        {
            var result = new List<GeneticNode>();
            foreach (var n in _catalog)
                if (n.branch == branch) result.Add(n);
            return result;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Aplicar efectos

        private void ApplyEffect(GeneticNode node)
        {
            switch (node.effect)
            {
                // ── ADAPTACIÓN ────────────────────────────────────────────────────
                case GeneticEffect.ReduceInstability:
                    // Reduce la tasa de entropía aplicando un descuento permanente
                    // vía un flag global consultado por InstabilitySystem
                    GeneticFlags.InstabilityRateMultiplier *= (1f - node.effectValue);
                    break;

                case GeneticEffect.ReduceHeatGeneration:
                    GeneticFlags.HeatGenerationMultiplier *= (1f - node.effectValue);
                    break;

                case GeneticEffect.EnvironmentTolerance:
                    GeneticFlags.EnvironmentToleranceBonus += node.effectValue;
                    break;

                case GeneticEffect.AdaptationDebtReduction:
                    GeneticFlags.AdaptationDebtMultiplier *= (1f - node.effectValue);
                    break;

                case GeneticEffect.StallResistance:
                    GeneticFlags.StallMultiplierCap *= (1f - node.effectValue);
                    break;

                case GeneticEffect.FarmingDecayReduction:
                    GeneticFlags.FarmingDecayRateMultiplier *= (1f - node.effectValue);
                    break;

                // ── METABOLISMO GEN ───────────────────────────────────────────────
                case GeneticEffect.RouteEfficiencyBoost:
                    ResourceManager.Instance?.SetProductionMultiplier(
                        ResourceType.ATP,
                        1f + node.effectValue);
                    break;

                case GeneticEffect.FermentationUnlock:
                    GeneticFlags.FermentationBiomassBonus += node.effectValue;
                    break;

                case GeneticEffect.PhotosynthesisAmplify:
                    GeneticFlags.PhotosynthesisO2Bonus += node.effectValue;
                    break;

                case GeneticEffect.MixotrophyUnlock:
                    GeneticFlags.MixotrophyGlucoseDiscount += node.effectValue;
                    break;

                case GeneticEffect.LithotrophyUnlock:
                    GeneticFlags.LithotrophyNitrogenDiscount += node.effectValue;
                    break;

                case GeneticEffect.ATPCriticalThreshold:
                    GeneticFlags.ATPCriticalThresholdOverride = node.effectValue;
                    break;

                // ── ESPECIALIZACIÓN ───────────────────────────────────────────────
                case GeneticEffect.SpeedMutation:
                    // TODO: Primordia — Player.CAP.Instance?.ApplyMoveSpeedMultiplier(1f + node.effectValue);
                    break;

                case GeneticEffect.PhotoMutation:
                    GeneticFlags.PhotoATPConversionBonus += node.effectValue;
                    break;

                case GeneticEffect.SilicaMutation:
                    GeneticFlags.SilicaDamageReduction += node.effectValue;
                    break;

                case GeneticEffect.PredatorMutation:
                    GeneticFlags.PredatorDamageReduction += node.effectValue;
                    break;

                case GeneticEffect.CiliaMutation:
                    GeneticFlags.CiliaStunCooldownReduction += node.effectValue;
                    break;

                case GeneticEffect.ColonyMutation:
                    GeneticFlags.ColonyInstabilityReduction += node.effectValue;
                    break;

                case GeneticEffect.RegenMutation:
                    GeneticFlags.HydraHPRegenPerSec += node.effectValue;
                    StartCoroutine(HydraRegenLoop(node.effectValue));
                    break;

                case GeneticEffect.ExtremophileMutation:
                    MetabolicHeatSystem.Instance?.AddExtraDissipation(
                        node.effectValue * 5f);  // +30% → ~1.5 disipación extra
                    break;

                case GeneticEffect.MulticelularMutation:
                    // TODO: Primordia — Player.CAP.Instance?.ExpandHSPRadius(node.effectValue);
                    break;

                // ── SIMBIOSIS ─────────────────────────────────────────────────────
                case GeneticEffect.QuorumBoost:
                    GeneticFlags.QuorumEfficiencyBonus += node.effectValue;
                    break;

                case GeneticEffect.SymbioticAura:
                    GeneticFlags.AuraHealBonus += node.effectValue;
                    break;

                case GeneticEffect.ColectiveDNA:
                    StartCoroutine(ColectiveDNALoop(node.effectValue));
                    break;

                case GeneticEffect.HorizontalGeneTransfer:
                    GeneticFlags.HorizontalGeneTransferActive = true;
                    break;

                case GeneticEffect.EndosymbiosisUnlock:
                    Organelles.Mitocondria.ExtraSlots += 1;
                    GlobalUpgradeFlags.UnlockedUnitTypes.Add("Mitocondria_L4");
                    break;
            }
        }

        // Hidra: regeneración continua de HP
        private IEnumerator HydraRegenLoop(float hpPerSec)
        {
            var wait = new WaitForSeconds(1f);
            while (true)
            {
                yield return wait;
                if (GameManager.Instance != null &&
                   (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) continue;
                // TODO: Primordia — Player.CAP.Instance?.Heal(hpPerSec);
            }
        }

        // Simbiosis: GP pasivos por orgánulo activo
        private IEnumerator ColectiveDNALoop(float gpPerOrganulePerMin)
        {
            var wait = new WaitForSeconds(60f);
            while (true)
            {
                yield return wait;
                if (GameManager.Instance != null &&
                   (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) continue;

                int organuleCount = FindObjectsByType<Organelles.OrganelleBase>(
                    FindObjectsSortMode.None).Length;

                float gpGained = organuleCount * gpPerOrganulePerMin;
                ResourceManager.Instance?.AddResource(ResourceType.GenomicPoints, gpGained);
                Debug.Log($"[GeneticTree] ADN Colectivo: +{gpGained:F1} GP ({organuleCount} orgánulos).");
            }
        }

        #endregion
    }

    // ── Flags globales de mutaciones genéticas ────────────────────────────────────
    /// <summary>
    /// Modificadores acumulativos aplicados por GeneticTreeSystem.
    /// Consultados por MetabolismEngine, InstabilitySystem, BalanceSystem, etc.
    /// </summary>
    public static class GeneticFlags
    {
        // Adaptación
        public static float InstabilityRateMultiplier    = 1.0f;
        public static float HeatGenerationMultiplier     = 1.0f;
        public static float EnvironmentToleranceBonus    = 0.0f;
        public static float AdaptationDebtMultiplier     = 1.0f;
        public static float StallMultiplierCap           = 2.0f;  // max stall mult
        public static float FarmingDecayRateMultiplier   = 1.0f;

        // Metabolismo
        public static float FermentationBiomassBonus     = 0.0f;
        public static float PhotosynthesisO2Bonus        = 0.0f;
        public static float MixotrophyGlucoseDiscount    = 0.0f;
        public static float LithotrophyNitrogenDiscount  = 0.0f;
        public static float ATPCriticalThresholdOverride = 0f;  // 0 = usa default (15)

        // Especialización
        public static float PhotoATPConversionBonus      = 0.0f;
        public static float SilicaDamageReduction        = 0.0f;
        public static float PredatorDamageReduction      = 0.0f;
        public static float CiliaStunCooldownReduction   = 0.0f;
        public static float ColonyInstabilityReduction   = 0.0f;
        public static float HydraHPRegenPerSec           = 0.0f;
        public static float QuorumEfficiencyBonus        = 0.0f;

        // Simbiosis
        public static float AuraHealBonus                = 0.0f;
        public static bool  HorizontalGeneTransferActive = false;

        public static void Reset()
        {
            InstabilityRateMultiplier   = 1.0f;
            HeatGenerationMultiplier    = 1.0f;
            EnvironmentToleranceBonus   = 0.0f;
            AdaptationDebtMultiplier    = 1.0f;
            StallMultiplierCap          = 2.0f;
            FarmingDecayRateMultiplier  = 1.0f;
            FermentationBiomassBonus    = 0.0f;
            PhotosynthesisO2Bonus       = 0.0f;
            MixotrophyGlucoseDiscount   = 0.0f;
            LithotrophyNitrogenDiscount = 0.0f;
            ATPCriticalThresholdOverride= 0f;
            PhotoATPConversionBonus     = 0.0f;
            SilicaDamageReduction       = 0.0f;
            PredatorDamageReduction     = 0.0f;
            CiliaStunCooldownReduction  = 0.0f;
            ColonyInstabilityReduction  = 0.0f;
            HydraHPRegenPerSec          = 0.0f;
            QuorumEfficiencyBonus       = 0.0f;
            AuraHealBonus               = 0.0f;
            HorizontalGeneTransferActive= false;
        }
    }
}
