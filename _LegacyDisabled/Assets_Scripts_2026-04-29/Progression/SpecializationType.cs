namespace Protogenesis.Progression
{
    // GDD v4.3 — Dominio biológico: se define al elegir el gen del Anillo 1 (minuto 3).
    public enum CellDomain
    {
        Undefined  = 0,  // antes de elegir el gen del Anillo 1 (LUCA)
        Prokaryote = 1,  // Fermentación o Quimiolitotrofía
        Eukaryote  = 2,  // Respiración aeróbica o Fotosíntesis
    }

    // 12 tipos biológicos canónicos. Tardigrado/Volvox/Paramecio son unidades Premium.
    public enum SpecializationType
    {
        None          = 0,
        Bacteria      = 1,   // 3.8 Ga — Swarm / Zerg
        Arquea        = 2,   // 3.5 Ga — Control de terreno
        Cianobacteria = 3,   // 2.7 Ga — Económico / Pasivo
        Hongo         = 4,   // 1.5 Ga — Red / Area denial
        Ameba         = 5,   // 1.0 Ga — Bruiser / Assassin
        Flagelado     = 6,   // 1.0 Ga — Raider / Harass
        Ciliado       = 7,   // 0.8 Ga — Generalista / Terran
        Neutrofilo    = 8,   // 0.5 Ga — Swarm sacrificial
        Macrofago     = 9,   // 0.5 Ga — Tank / Control
        CelulaNK      = 10,  // 0.5 Ga — Assassin dirigido
        CelulaB       = 11,  // 0.4 Ga — Ranged / Anticuerpos
        CelulaMadre   = 12,  // 0.3 Ga — Flex / Pivot
    }

    public enum CellGeneration { Mother, Daughter, Granddaughter, Premium }

    public enum DirectiveType { Follow, Farm, Attack, Defend, Explore }

    public enum GeneticRing { Ring1, Ring2, Ring3 }

    public enum DeathType { Lysis, Intoxication, MetabolicCollapse, Apoptosis, Freezing }

    public enum CellVisualState { Stable, LowEnergy, Overheat, HighStress, Collapse }

    // Helper: dominio canónico de cada especialización.
    public static class SpecializationDomainMap
    {
        public static CellDomain GetDomain(SpecializationType t) => t switch
        {
            SpecializationType.Bacteria      => CellDomain.Prokaryote,
            SpecializationType.Arquea        => CellDomain.Prokaryote,
            SpecializationType.Cianobacteria => CellDomain.Prokaryote,
            _                                => CellDomain.Eukaryote,
        };
    }
}
