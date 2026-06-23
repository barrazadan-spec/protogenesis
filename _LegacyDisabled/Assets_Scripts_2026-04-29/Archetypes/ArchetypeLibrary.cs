using UnityEngine;

namespace Protogenesis.Archetypes
{
    /// <summary>
    /// Biblioteca de arquetipos canónicos — creados en runtime (GDD v3 §9).
    ///
    /// Genera los 8 ArchetypeDefinition como ScriptableObjects en memoria sin necesitar
    /// assets de editor. ArchetypeResolver y ArchetypeAbilityUnlocker lo consultan
    /// automáticamente si sus arrays serializados están vacíos.
    ///
    /// Los parámetros de scoring reflejan el GDD v3 §9:
    ///   Bacteria Veloz          → alta reproducción, bajo ATP neto, muchos kills
    ///   Microalga Fototrófica   → alto ATP neto, sin kills, sin fermentación
    ///   Diatomea Fortaleza      → alta protección, bajo movimiento, territorio fijo
    ///   Ameba Cazadora          → kills moderados, alta ingesta, versatilidad
    ///   Paramecio Ciliar        → alta velocidad, kills por movimiento, sin colonia
    ///   Biofilm Colonial        → muchos aliados, bajo kill individual, alta señal social
    ///   Hidra Depredadora       → kills de alto valor, territorio, fermentación opcional
    ///   Tardígrado Extremófilo  → condiciones extremas, bajo ATP, criptobiosis
    /// </summary>
    public class ArchetypeLibrary : MonoBehaviour
    {
        public static ArchetypeLibrary Instance { get; private set; }

        public ArchetypeDefinition[] All { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            BuildLibrary();
        }

        private void BuildLibrary()
        {
            All = new ArchetypeDefinition[]
            {
                BuildBacteriaVeloz(),
                BuildMicroalgaFototrofica(),
                BuildDiatomeaFortaleza(),
                BuildAmebaCazadora(),
                BuildParamecioGiliar(),
                BuildBiofilmColonial(),
                BuildHidraDepredadora(),
                BuildTardigrExemofilo(),
            };
            Debug.Log("[ArchetypeLibrary] 8 arquetipos canónicos construidos en runtime.");
        }

        // ─────────────────────────────────────────────────────────────────────────
        // Los 8 arquetipos

        private static ArchetypeDefinition BuildBacteriaVeloz()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.AggressiveHeterotroph;
            d.displayName        = "Bacteria Veloz";
            d.description        = "Expansión temprana y dominio por número. Rápida, frágil e imparable en masa.";
            d.themeColor         = new Color(0.9f, 0.3f, 0.2f);
            d.minATPNetRate      = 0f;
            d.maxATPNetRate      = 15f;   // no domina en ATP puro
            d.requiresFermentation = false;
            d.minAlliesNearby    = 0;
            d.minRecentKills     = 3;     // necesita haber cazado algo
            d.scoreIfLowO2       = false;
            // Bonos
            d.damageBonus        = 0.30f;
            d.moveSpeedBonus     = 0.20f;
            d.uniqueAbilityId    = "ability_quorum_burst";
            return d;
        }

        private static ArchetypeDefinition BuildMicroalgaFototrofica()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.PeacefulAutotroph;
            d.displayName        = "Microalga Fototrófica";
            d.description        = "Economía solar. Produce mientras los demás combaten.";
            d.themeColor         = new Color(0.2f, 0.9f, 0.4f);
            d.minATPNetRate      = 8f;    // requiere producción pasiva alta
            d.maxATPNetRate      = 999f;
            d.requiresFermentation = false;
            d.minAlliesNearby    = 0;
            d.minRecentKills     = 0;     // sin kills recientes
            d.scoreIfLowO2       = false;
            // Bonos
            d.atpProductionBonus = 0.50f;
            d.damageReduction    = 0.20f;
            d.uniqueAbilityId    = "ability_photosynthesis_burst";
            return d;
        }

        private static ArchetypeDefinition BuildDiatomeaFortaleza()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.ExtremophileSurvivor;  // reutilizamos el más cercano
            d.displayName        = "Diatomea Fortaleza";
            d.description        = "Defensa superior y control espacial. Pesa más que cualquier amenaza directa.";
            d.themeColor         = new Color(0.7f, 0.7f, 0.9f);
            d.minATPNetRate      = 2f;
            d.maxATPNetRate      = 999f;
            d.requiresFermentation = false;
            d.minAlliesNearby    = 0;
            d.minRecentKills     = 0;
            d.scoreIfLowO2       = false;
            // Bonos: defensa extrema
            d.damageReduction    = 0.40f;
            d.maxHPBonus         = 80f;
            d.uniqueAbilityId    = "ability_silica_shell";
            return d;
        }

        private static ArchetypeDefinition BuildAmebaCazadora()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.StrategicParasite;
            d.displayName        = "Ameba Cazadora";
            d.description        = "Flexibilidad total y depredación oportunista. Envuelve, ingiere, cambia de objetivo.";
            d.themeColor         = new Color(0.7f, 0.2f, 0.8f);
            d.minATPNetRate      = 0f;
            d.maxATPNetRate      = 999f;
            d.requiresFermentation = false;
            d.minAlliesNearby    = 0;
            d.minRecentKills     = 2;
            d.scoreIfLowO2       = false;
            // Bonos
            d.damageBonus        = 0.25f;
            d.moveSpeedBonus     = 0.25f;
            d.uniqueAbilityId    = "ability_phagocytosis_engulf";
            return d;
        }

        private static ArchetypeDefinition BuildParamecioGiliar()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.AggressiveHeterotroph;
            d.displayName        = "Paramecio Ciliar";
            d.description        = "Movimiento de élite y economía cinética. Domina por control corporal.";
            d.themeColor         = new Color(0.3f, 0.8f, 1.0f);
            d.minATPNetRate      = 3f;
            d.maxATPNetRate      = 999f;
            d.requiresFermentation = false;
            d.minAlliesNearby    = 0;
            d.minRecentKills     = 1;
            d.scoreIfLowO2       = false;
            // Bonos
            d.moveSpeedBonus     = 0.50f;
            d.atpProductionBonus = 0.15f;
            d.uniqueAbilityId    = "ability_ciliary_boost";
            return d;
        }

        private static ArchetypeDefinition BuildBiofilmColonial()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.CooperativeSymbiont;
            d.displayName        = "Biofilm Colonial";
            d.description        = "Arquitectura viva. Divide el trabajo, fortifica el territorio, escala como sistema.";
            d.themeColor         = new Color(0.2f, 0.8f, 0.9f);
            d.minATPNetRate      = 2f;
            d.maxATPNetRate      = 999f;
            d.requiresFermentation = false;
            d.minAlliesNearby    = 3;   // necesita colonia activa
            d.minRecentKills     = 0;
            d.scoreIfLowO2       = false;
            // Bonos: escala con colonia
            d.hpRegenBonus       = 3f;
            d.atpProductionBonus = 0.20f;
            d.uniqueAbilityId    = "ability_quorum_signal";
            return d;
        }

        private static ArchetypeDefinition BuildHidraDepredadora()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.ApexPredator;
            d.displayName        = "Hidra Depredadora";
            d.description        = "Apex bentónico. Autoridad, amenaza y longevidad en un solo cuerpo.";
            d.themeColor         = new Color(1.0f, 0.4f, 0.1f);
            d.minATPNetRate      = 0f;
            d.maxATPNetRate      = 999f;
            d.requiresFermentation = false;
            d.minAlliesNearby    = 0;
            d.minRecentKills     = 4;   // predador ápex necesita kills consistentes
            d.scoreIfLowO2       = false;
            // Bonos
            d.damageBonus        = 0.60f;
            d.hpRegenBonus       = 2f;
            d.uniqueAbilityId    = "ability_cnidocyte_burst";
            return d;
        }

        private static ArchetypeDefinition BuildTardigrExemofilo()
        {
            var d = ScriptableObject.CreateInstance<ArchetypeDefinition>();
            d.archetypeType      = ArchetypeType.ExtremophileSurvivor;
            d.displayName        = "Tardígrado Extremófilo";
            d.description        = "Supervivencia total. Donde todo muere, él persiste. Épica de resistencia.";
            d.themeColor         = new Color(0.8f, 0.8f, 1.0f);
            d.minATPNetRate      = 0f;
            d.maxATPNetRate      = 6f;   // subsiste con poco ATP
            d.requiresFermentation = true; // necesita condiciones hostiles
            d.minAlliesNearby    = 0;
            d.minRecentKills     = 0;
            d.scoreIfLowO2       = true;  // puntúa más en hipoxia
            // Bonos
            d.maxHPBonus         = 120f;
            d.damageReduction    = 0.35f;
            d.hpRegenBonus       = 1.5f;
            d.uniqueAbilityId    = "ability_cryptobiosis";
            return d;
        }
    }
}
