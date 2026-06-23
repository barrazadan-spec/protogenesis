namespace Protogenesis.Archetypes
{
    /// <summary>
    /// Arquetipos de comportamiento de NPCs/enemigos (sistema v2 — legado).
    ///
    /// NOTA v5.2: El sistema canónico de FENOTIPOS del jugador está en PhenotypeType.cs.
    /// Este enum define los perfiles de comportamiento que ArchetypeLibrary usa para
    /// configurar las 8 especies NPC canónicas del GDD (enemigos y neutrales).
    ///
    /// Mapeo a nombres canónicos del GDD v5.2:
    ///   AggressiveHeterotroph  → Bacteria Veloz / Ameba Cazadora
    ///   PeacefulAutotroph      → Microalga Fototrófica
    ///   StrategicParasite      → Parásito Obligado (ruta parasitaria)
    ///   CooperativeSymbiont    → Biofilm Colonial
    ///   OpportunisticDecomposer→ Arquea Termófila / Bacteria Fermentadora
    ///   ApexPredator           → Hidra Depredadora
    ///   EcologicalEngineer     → Diatomea Fortaleza
    ///   ExtremophileSurvivor   → Tardígrado Extremófilo
    /// </summary>
    public enum ArchetypeType
    {
        None = 0,

        /// <summary>GDD: Bacteria Veloz / Ameba Cazadora.
        /// Alta producción ATP + muchos ataques. Bonus: +30% daño, +20% vel. ataque.</summary>
        AggressiveHeterotroph,

        /// <summary>GDD: Microalga Fototrófica.
        /// Alta fotosíntesis. Bonus: +50% producción de recursos pasivos, -20% daño recibido.</summary>
        PeacefulAutotroph,

        /// <summary>GDD: Parásito Obligado (ruta parasitaria v5.2).
        /// Drena ATP de huéspedes. Bonus: roba recursos al infectar, +25% vel. movimiento.</summary>
        StrategicParasite,

        /// <summary>GDD: Biofilm Colonial.
        /// Alto número de aliados en masa. Bonus: halo regenerador, aliados +15% stats.</summary>
        CooperativeSymbiont,

        /// <summary>GDD: Arquea Termófila / Bacteria Fermentadora.
        /// Consume cadáveres + alta fermentación. Bonus: ATP al destruir, +40% eficiencia ferment.</summary>
        OpportunisticDecomposer,

        /// <summary>GDD: Hidra Depredadora.
        /// Elimina targets de alta jerarquía. Bonus: +60% daño a organismos grandes, inmunidad debuffs.</summary>
        ApexPredator,

        /// <summary>GDD: Diatomea Fortaleza.
        /// Modifica pH/O2, zona de bioma. Bonus: control ambiental de zona, enemigos en zona -30%.</summary>
        EcologicalEngineer,

        /// <summary>GDD: Tardígrado Extremófilo.
        /// Sobrevive condiciones extremas. Bonus: penalizaciones ambientales → bonos, +80% HP.</summary>
        ExtremophileSurvivor
    }
}
