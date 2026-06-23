using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5AdaptationId
    {
        None,
        BacterialWall,
        BacterialFlagellum,
        PolysaccharideCapsule,
        PiliFimbriae,
        BasicAdhesin,
        RapidDivision,
        ProkaryoticThylakoid,
        ExtremophileMembrane,
        ProtonPump,
        Nucleus,
        Mitochondria,
        Chloroplast,
        Lysosome,
        Pseudopods,
        ContractileVacuole,
        Cilia,
        EukaryoticFlagellum,
        CelluloseWall,
        SilicaFrustule,
        CatalaseROS,
        ColonialAdhesin,
        FungalHypha,
        ExtracellularEnzymes,
        SlimePlasmodium,
        ChemicalMemory,
        PersistentAdhesion,
        CellDifferentiation,
        SignalingCommunication,
        BiologicalChampion
    }

    public enum V5AdaptationKind { Active, Milestone, State, Apex }
    public enum V5AdaptationTier { T1Prokaryote, T2Eukaryogenesis, T3Specialization, T4ColonialBody, T5Apex }

    public enum V5IdentityId
    {
        LUCA,
        BacteriaSwarm,
        Biofilm,
        Archaea,
        Cyanobacteria,
        ProtistBase,
        Amoeba,
        Flagellate,
        Ciliate,
        Microalga,
        Diatom,
        Fungus,
        SlimeMold,
        VolvoxEarly,
        VolvoxComplete
    }

    public class V5AdaptationDefinition
    {
        public V5AdaptationId id;
        public string displayName;
        public string shortName;
        public string description;
        public V5AdaptationKind kind;
        public V5AdaptationTier tier;
        public V5ResourceWallet cost;
        public bool countsTowardCap = true;
        public bool p0 = true;
        public V5AdaptationId[] prerequisites;
        public V5StructureId[] legacyStructures;
        public V5MetabolismType legacyMetabolism = V5MetabolismType.None;
        public V5EvolutionPath routeHint = V5EvolutionPath.Uncommitted;
        public Color color = Color.white;
        public string motherEffect;
        public string productionEffect;
        public string bodyEffect;
        public string identityEffect;
        public int visualTier = 1;
        public string naturalCounters;
        public string telemetryKey;
        public bool latentInChampion;
        public string championEffect;
        public bool mvpEnabled = true;
    }
}
