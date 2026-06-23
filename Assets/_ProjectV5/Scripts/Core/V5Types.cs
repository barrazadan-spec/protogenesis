using System;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5CellDomain { LUCA, Prokaryote, Eukaryote, Multicellular }
    public enum V5CellRole { Mother, Daughter, Granddaughter, Apex, Neutral, Enemy }
    public enum V5MetabolismType { None, Respiration, Photosynthesis, Fermentation, Chemolithotrophy }
    public enum V5StructureId
    {
        GeneticCompartment,
        MetabolicEngine,
        SynthesisMachinery,
        StorageVacuole,
        Catalase,
        BacterialFlagellum,
        EukaryoticFlagellum,
        PeptidoglycanWall,
        CelluloseWall,
        Capsule,
        Fimbriae,
        Plasmid,
        Thylakoid,
        Lysosome,
        AzurophilicGranule,
        InvasiveHypha,
        Cilia,
        RecognitionReceptors,
        SecretoryVesicle,
        StemPlasticity,
        MicroalgalChloroplast,
        MucilageMatrix,
        CoronaCilia,
        PiercingStylet,
        Cuticle,
        CryptobiosisTun
    }

    public enum V5EvolutionPath
    {
        Uncommitted,
        Bacteria,
        Archaea,
        Cyanobacteria,
        Fungus,
        Amoeba,
        Flagellate,
        Ciliate,
        Neutrophil,
        Macrophage,
        NaturalKiller,
        BCell,
        StemCell,
        Microalga,
        SlimeMold,
        Rotifer,
        Nematode,
        Tardigrade
    }

    public enum V5Directive { Idle, Move, FollowMother, Farm, Defend, Explore, Colonize, Attack, ReturnHome }
    public enum V5CellModeId { FollowLineage, Gather, Defend, Scout, Colonize, Hunt, Recover, RouteSpecial }
    public enum V5BodyRing { Nucleus, Inner, Outer }
    public enum V5BodySlotRole { None, Armor, Motor, Producer, Mouth, Connector, Sensor, Reserve }
    public enum V5BodyState { Exposed, Partial, Complete, Overloaded }
    public enum V5CellDeploymentMode { Auto, FreeSquad, AttachedBody }
    public enum V5AttachmentState { Free, Attached, Detaching, Cooldown }
    public enum V5LineageRole { Generalist, Farmer, Scout, Defender, Colonizer, Predator, Recycler }
    public enum V5GerminalCasteId { PlasticDaughter, LineageGatherer, LineageScout, LineageDefender, LineageRaider, AmoeboidGuard, CiliateController, BacterialSymbiont, MicroalgaSupport }
    public enum V5FunctionalCasteId { Hybrid, Gatherer, Attacker, Defender, Producer, Sensor, Structural }
    public enum V5ResourceKind { ATP, Biomass, AminoAcids, Lipids, Nucleotides, Minerals }
    public enum V5DamageKind { Physical, Chemical, Osmotic, Thermal, Apoptotic, Oxidative, Acid, Piercing }
    public enum V5OverlayMode { Nutrients, Light, Oxygen, Toxins, Acidity, Colonization, Temperature, None }
    public enum V5ScenarioId { FirstDrop, OxygenWar, AcidFrontier, PredatorBloom, Freeplay }
    public enum V5GamePhase { Primordial, Differentiation, Expansion, Dominance, Resolution, Victory, Defeat }
    public enum V5GeneRing { Ring1Metabolism, Ring2Function, Ring3Division, Ring4Ecosystem }
    public enum V5GeneId
    {
        None,
        Respiration,
        Photosynthesis,
        Fermentation,
        Chemolithotrophy,
        Motility,
        Secretion,
        Recognition,
        Adhesion,
        RapidDivision,
        StrongInheritance,
        Autonomy,
        TotalReabsorption,
        Symbiosis,
        ApexMaturation
    }
    public enum V5ApexForm { None, Volvox, Hydra, Tardigrade, Paramecium }

    [Serializable]
    public struct V5ResourceWallet
    {
        public float atp;
        public float biomass;
        public float aminoAcids;
        public float lipids;
        public float nucleotides;
        public float minerals;

        public static V5ResourceWallet Starter()
        {
            V5ResourceWallet w = new V5ResourceWallet();
            w.atp = 85f;
            w.biomass = 58f;
            w.aminoAcids = 0f;
            w.lipids = 0f;
            w.nucleotides = 0f;
            w.minerals = 0f;
            return w;
        }

        public float Get(V5ResourceKind kind)
        {
            switch (kind)
            {
                case V5ResourceKind.ATP: return atp;
                case V5ResourceKind.Biomass: return biomass;
                case V5ResourceKind.AminoAcids: return aminoAcids;
                case V5ResourceKind.Lipids: return lipids;
                case V5ResourceKind.Nucleotides: return nucleotides;
                case V5ResourceKind.Minerals: return minerals;
                default: return 0f;
            }
        }

        public void Add(V5ResourceKind kind, float amount)
        {
            switch (kind)
            {
                case V5ResourceKind.ATP: atp += amount; break;
                case V5ResourceKind.Biomass: biomass += amount; break;
                case V5ResourceKind.AminoAcids: aminoAcids += amount; break;
                case V5ResourceKind.Lipids: lipids += amount; break;
                case V5ResourceKind.Nucleotides: nucleotides += amount; break;
                case V5ResourceKind.Minerals: minerals += amount; break;
            }
        }

        public bool CanPay(V5ResourceWallet cost)
        {
            return atp >= cost.atp && biomass >= cost.biomass;
        }

        public void Pay(V5ResourceWallet cost)
        {
            atp -= cost.atp;
            biomass -= cost.biomass;
        }

        public static V5ResourceWallet Cost(float atp, float biomass, float aa, float lipids, float nt, float minerals)
        {
            V5ResourceWallet w = new V5ResourceWallet();
            w.atp = atp;
            w.biomass = biomass;
            w.aminoAcids = aa;
            w.lipids = lipids;
            w.nucleotides = nt;
            w.minerals = minerals;
            return w;
        }
    }

    [Serializable]
    public struct V5CellStats
    {
        public float maxHp;
        public float currentHp;
        public float speed;
        public float turnRate;
        public float radius;
        public float stress;
        public float maxBiomassLoad;
        public float synthesisRate;
        [Range(0f, 1f)] public float physicalArmor;
        public float toxinResistance;
        public float thermalResistance;
        public float phTolerance;
        public float atpPerSecond;
        public float sensorRange;
        public float attackRange;
        public float chemicalDamagePerSecond;
        public float physicalDamagePerSecond;
        public float repairPerSecond;
        public float colonizationPower;
        public float divisionEfficiency;

        public static V5CellStats MotherDefaults()
        {
            V5CellStats s = new V5CellStats();
            s.maxHp = 120f;
            s.currentHp = 120f;
            s.speed = 2.1f;
            s.turnRate = 5f;
            s.radius = 0.55f;
            s.stress = 0f;
            s.maxBiomassLoad = 100f;
            s.synthesisRate = 1f;
            s.physicalArmor = 0f;
            s.toxinResistance = 0f;
            s.thermalResistance = 0f;
            s.phTolerance = 0.5f;
            s.atpPerSecond = 0.7f;
            s.sensorRange = 7f;
            s.attackRange = 1.1f;
            s.chemicalDamagePerSecond = 0f;
            s.physicalDamagePerSecond = 1.0f;
            s.repairPerSecond = 0.8f;
            s.colonizationPower = 0.3f;
            s.divisionEfficiency = 1f;
            return s;
        }
    }

    [Serializable]
    public struct V5MembraneSegment
    {
        public float hp;
        public float maxHp;
    }

    public static class V5Colors
    {
        public static readonly Color LUCA = new Color(0.66f, 0.77f, 0.83f, 1f);
        public static readonly Color Bacteria = new Color(0.42f, 0.71f, 0.83f, 1f);
        public static readonly Color Archaea = new Color(0.55f, 0.42f, 0.29f, 1f);
        public static readonly Color Cyanobacteria = new Color(0.30f, 0.69f, 0.31f, 1f);
        public static readonly Color Fungus = new Color(0.83f, 0.66f, 0.26f, 1f);
        public static readonly Color Amoeba = new Color(0.83f, 0.66f, 0.78f, 1f);
        public static readonly Color Flagellate = new Color(0.30f, 0.71f, 0.67f, 1f);
        public static readonly Color Ciliate = new Color(0.56f, 0.64f, 0.68f, 1f);
        public static readonly Color Neutrophil = new Color(0.94f, 0.33f, 0.31f, 1f);
        public static readonly Color Macrophage = new Color(1.00f, 0.44f, 0.26f, 1f);
        public static readonly Color NaturalKiller = new Color(0.67f, 0.28f, 0.74f, 1f);
        public static readonly Color BCell = new Color(0.26f, 0.65f, 0.96f, 1f);
        public static readonly Color StemCell = new Color(0.51f, 0.78f, 0.52f, 1f);
        public static readonly Color Microalga = new Color(0.18f, 0.78f, 0.46f, 1f);
        public static readonly Color SlimeMold = new Color(0.92f, 0.74f, 0.30f, 1f);
        public static readonly Color Rotifer = new Color(0.72f, 0.82f, 0.92f, 1f);
        public static readonly Color Nematode = new Color(0.86f, 0.78f, 0.64f, 1f);
        public static readonly Color Tardigrade = new Color(0.62f, 0.70f, 0.58f, 1f);
        public static readonly Color Selected = new Color(1f, 0.86f, 0.2f, 1f);
    }
}
