using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public static class V5AdaptationLibrary
    {
        private static readonly List<V5AdaptationDefinition> ordered = new List<V5AdaptationDefinition>(32);
        private static readonly Dictionary<V5AdaptationId, V5AdaptationDefinition> defs = new Dictionary<V5AdaptationId, V5AdaptationDefinition>();
        private static readonly V5AdaptationId[] noPrerequisites = new V5AdaptationId[0];
        private static readonly V5StructureId[] noStructures = new V5StructureId[0];

        public static IReadOnlyList<V5AdaptationDefinition> All()
        {
            Build();
            return ordered;
        }

        public static IReadOnlyList<V5AdaptationDefinition> AllMvpEnabled()
        {
            Build();
            List<V5AdaptationDefinition> result = new List<V5AdaptationDefinition>(ordered.Count);
            for (int i = 0; i < ordered.Count; i++)
                if (ordered[i].mvpEnabled) result.Add(ordered[i]);
            return result;
        }

        public static V5AdaptationDefinition Get(V5AdaptationId id)
        {
            Build();
            V5AdaptationDefinition def;
            return defs.TryGetValue(id, out def) ? def : null;
        }

        private static void Build()
        {
            if (defs.Count > 0) return;

            Add(Def(V5AdaptationId.BacterialWall, "Pared bacteriana", "Pared", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(8f, 6f, 2f, 1f, 0f, 1f),
                "Defensa barata: estabiliza la membrana y empuja a identidad bacteriana.",
                new[] { V5StructureId.PeptidoglycanWall }, V5MetabolismType.None, V5EvolutionPath.Bacteria,
                new Color(0.42f, 0.71f, 0.83f, 1f), "HP y resistencia inicial.", "", "", "Base de Bacteria/Biofilm."));

            Add(Def(V5AdaptationId.BacterialFlagellum, "Flagelo bacteriano", "Flagelo", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(10f, 5f, 5f, 2f, 0f, 1f),
                "Movimiento lineal y exploracion temprana para procariotas.",
                new[] { V5StructureId.BacterialFlagellum }, V5MetabolismType.None, V5EvolutionPath.Bacteria,
                new Color(0.38f, 0.85f, 0.95f, 1f), "Velocidad y sensor.", "", "", "Bacteria movil / raider."));

            Add(Def(V5AdaptationId.PolysaccharideCapsule, "Capsula polisacarida", "Capsula", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(10f, 7f, 2f, 3f, 0f, 1f),
                "Capa externa defensiva: aguanta toxinas y presion ambiental.",
                new[] { V5StructureId.Capsule }, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.72f, 0.62f, 0.44f, 1f), "Mas HP y tolerancia.", "", "", "Biofilm o Arquea defensiva."));

            Add(Def(V5AdaptationId.PiliFimbriae, "Pili / Fimbrias", "Pili", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(7f, 4f, 3f, 1f, 0f, 0f),
                "Contacto y anclaje ligero: mejora colonizacion y adhesion bacteriana.",
                new[] { V5StructureId.Fimbriae }, V5MetabolismType.None, V5EvolutionPath.Bacteria,
                new Color(0.58f, 0.82f, 0.78f, 1f), "Colonizacion temprana.", "", "Ayuda a formar biofilm.", "Bacteria social."));

            Add(Def(V5AdaptationId.BasicAdhesin, "Adesina basica", "Adesina", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(5f, 3f, 1f, 1f, 0f, 0f),
                "Primer pegamento biologico. Desbloquea pegar hijas al cuerpo y funciona como tutorial de cuerpo.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.93f, 0.80f, 0.42f, 1f), "Aumenta colonizacion.", "", "Desbloquea attachment basico.", "Puente hacia multicelularidad."));

            Add(Def(V5AdaptationId.RapidDivision, "Division rapida", "Division", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(14f, 8f, 4f, 2f, 3f, 1f),
                "Reduce el costo de division y empuja builds swarm.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Bacteria,
                new Color(0.76f, 0.92f, 0.46f, 1f), "Division mas barata.", "Produce mas hijas.", "", "Swarm procarionte."));

            Add(Def(V5AdaptationId.ProkaryoticThylakoid, "Tilacoide procariota", "Tilacoide", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(16f, 9f, 3f, 4f, 3f, 2f),
                "Fotosintesis simple: explota luz y libera oxigeno.",
                new[] { V5StructureId.Thylakoid }, V5MetabolismType.Photosynthesis, V5EvolutionPath.Cyanobacteria,
                new Color(0.25f, 0.76f, 0.35f, 1f), "ATP por luz.", "Economia luminica.", "", "Cianobacteria."));

            Add(Def(V5AdaptationId.ExtremophileMembrane, "Membrana extremofila", "Extremofila", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(12f, 6f, 2f, 3f, 2f, 3f),
                "Arquea temprana: tolera calor, acidez y zonas pobres.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.64f, 0.47f, 0.33f, 1f), "Resistencia ambiental.", "", "", "Arquea."));

            Add(Def(V5AdaptationId.ProtonPump, "Bomba de protones", "Protones", V5AdaptationTier.T1Prokaryote,
                V5ResourceWallet.Cost(14f, 6f, 2f, 2f, 3f, 4f),
                "Quimiolitotrofia: gana ATP en ambientes extremos y acidifica el terreno.",
                noStructures, V5MetabolismType.Chemolithotrophy, V5EvolutionPath.Uncommitted,
                new Color(0.82f, 0.56f, 0.36f, 1f), "ATP en extremos.", "Economia extrema.", "", "Arquea agresiva."));

            Add(Def(V5AdaptationId.Nucleus, "Nucleo", "Nucleo", V5AdaptationTier.T2Eukaryogenesis,
                V5ResourceWallet.Cost(26f, 15f, 8f, 6f, 8f, 2f),
                "Hito eucariota: abre organelos, depredacion y especializaciones complejas.",
                new[] { V5StructureId.GeneticCompartment }, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.72f, 0.78f, 0.95f, 1f), "Mas control genetico.", "", "", "Eucariota base.", V5AdaptationKind.Milestone, false));

            Add(Def(V5AdaptationId.Mitochondria, "Mitocondria", "Mitocondria", V5AdaptationTier.T2Eukaryogenesis,
                V5ResourceWallet.Cost(24f, 13f, 5f, 5f, 6f, 2f),
                "Respiracion eucariota: alto ATP si hay oxigeno.",
                new[] { V5StructureId.MetabolicEngine }, V5MetabolismType.Respiration, V5EvolutionPath.Uncommitted,
                new Color(0.95f, 0.58f, 0.42f, 1f), "ATP alto.", "Produccion sostenida.", "", "Protista base.",
                prerequisites: new[] { V5AdaptationId.Nucleus }));

            Add(Def(V5AdaptationId.Chloroplast, "Cloroplasto", "Cloroplasto", V5AdaptationTier.T2Eukaryogenesis,
                V5ResourceWallet.Cost(28f, 16f, 5f, 8f, 6f, 4f),
                "Fotosintesis eucariota: economia de luz para microalgas y Volvox.",
                new[] { V5StructureId.MicroalgalChloroplast }, V5MetabolismType.Photosynthesis, V5EvolutionPath.Microalga,
                new Color(0.20f, 0.86f, 0.44f, 1f), "ATP por luz.", "Economia luminica avanzada.", "", "Microalga.",
                prerequisites: new[] { V5AdaptationId.Nucleus }));

            Add(Def(V5AdaptationId.Lysosome, "Lisosoma", "Lisosoma", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(18f, 10f, 6f, 3f, 4f, 1f),
                "Digestor interno: habilita depredacion por fagocitosis.",
                new[] { V5StructureId.Lysosome }, V5MetabolismType.None, V5EvolutionPath.Amoeba,
                new Color(0.92f, 0.58f, 0.78f, 1f), "Fagocitosis.", "Recicla presas.", "", "Ameba / depredador.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.Pseudopods }));

            Add(Def(V5AdaptationId.Pseudopods, "Seudopodos", "Seudopodos", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(16f, 10f, 4f, 3f, 3f, 1f),
                "Forma flexible: caza por contacto, empuja y envuelve.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Amoeba,
                new Color(0.86f, 0.63f, 0.82f, 1f), "Contacto mas fuerte.", "", "", "Ameba.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.Nucleus },
                2, true, "Ataque simultaneo a 3 amenazas cercanas.", "Toxinas persistentes, kiteo, espiculas.", "pickrate, contacto_dps, champion_multihit"));

            Add(Def(V5AdaptationId.ContractileVacuole, "Vacuola contractil", "Vacuola", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(14f, 8f, 3f, 3f, 2f, 1f),
                "Regula presion osmotica y mejora supervivencia en agua cambiante.",
                new[] { V5StructureId.StorageVacuole }, V5MetabolismType.None, V5EvolutionPath.Amoeba,
                new Color(0.66f, 0.76f, 0.88f, 1f), "Menos stress ambiental.", "", "", "Ciliado/Ameba estable.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.Nucleus }));

            Add(Def(V5AdaptationId.Cilia, "Cilios", "Cilios", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(19f, 10f, 7f, 4f, 3f, 1f),
                "Control de flujo: arrastra presas y mejora maniobra.",
                new[] { V5StructureId.Cilia }, V5MetabolismType.None, V5EvolutionPath.Amoeba,
                new Color(0.60f, 0.72f, 0.90f, 1f), "Maniobra y control.", "", "", "Ciliado.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.Nucleus }));

            Add(Def(V5AdaptationId.EukaryoticFlagellum, "Flagelo eucariota", "Flagelo E", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(18f, 10f, 7f, 4f, 3f, 1f),
                "Velocidad flexible y raid eucariota.",
                new[] { V5StructureId.EukaryoticFlagellum }, V5MetabolismType.None, V5EvolutionPath.Amoeba,
                new Color(0.34f, 0.84f, 0.76f, 1f), "Velocidad y sensor.", "", "", "Flagelado.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.Nucleus }));

            Add(Def(V5AdaptationId.CelluloseWall, "Pared de celulosa", "Celulosa", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(18f, 12f, 3f, 5f, 2f, 3f),
                "Defensa fotosintetica: microalga mas lenta y resistente.",
                new[] { V5StructureId.CelluloseWall }, V5MetabolismType.None, V5EvolutionPath.Microalga,
                new Color(0.36f, 0.78f, 0.48f, 1f), "Defensa vegetal.", "", "", "Microalga.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.Chloroplast }));

            Add(Def(V5AdaptationId.SilicaFrustule, "Frustula de silice", "Silice", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(22f, 12f, 3f, 5f, 3f, 7f),
                "Caja mineral de diatomea: defensa enorme, movilidad reducida.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Microalga,
                new Color(0.72f, 0.92f, 0.96f, 1f), "Armadura mineral.", "", "", "Diatomea.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.Chloroplast }));

            Add(Def(V5AdaptationId.CatalaseROS, "Catalasa / ROS", "Catalasa", V5AdaptationTier.T3Specialization,
                V5ResourceWallet.Cost(14f, 8f, 4f, 2f, 3f, 2f),
                "Resiste oxigeno reactivo y toxinas oxidativas.",
                new[] { V5StructureId.Catalase }, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.88f, 0.78f, 0.56f, 1f), "Resistencia a ROS.", "", "", "Counter ambiental."));

            Add(Def(V5AdaptationId.ColonialAdhesin, "Adesina colonial", "Adesina II", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(24f, 16f, 7f, 6f, 5f, 2f),
                "Pegamento fuerte para colonias estables, biofilm avanzado y cuerpos tempranos.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.96f, 0.82f, 0.42f, 1f), "Colonizacion alta.", "", "Mas estabilidad del cuerpo.", "Colonialidad.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.BasicAdhesin }));

            Add(Def(V5AdaptationId.FungalHypha, "Hifa fungica", "Hifa", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(24f, 18f, 6f, 7f, 4f, 4f),
                "Red fija territorial: ancla nodos y prepara digestion externa.",
                new[] { V5StructureId.InvasiveHypha }, V5MetabolismType.None, V5EvolutionPath.Fungus,
                new Color(0.86f, 0.70f, 0.34f, 1f), "Red defensiva.", "Nodo hifal.", "Anclaje territorial.", "Hongo incompleto.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.ColonialAdhesin, V5AdaptationId.Nucleus },
                3, false, "", "Nematodos, cortes de red, sequia local.", "pickrate, red_area, cortes_recibidos", false));

            Add(Def(V5AdaptationId.ExtracellularEnzymes, "Enzimas extracelulares", "Enzimas", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(20f, 12f, 7f, 3f, 4f, 2f),
                "Digestor externo: convierte detritus y cadaveres cercanos en economia de red.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Fungus,
                new Color(0.92f, 0.76f, 0.38f, 1f), "Reciclaje saprofito.", "Recicladora hifal.", "Red digiere zona propia.", "Hongo saprofito.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.FungalHypha },
                2, false, "", "Diatomea acorazada, corte de hifas, zonas pobres en detritus.", "pickrate, detritus_convertido, supervivencia_hongo", false));

            Add(Def(V5AdaptationId.SlimePlasmodium, "Plasmodio mucilaginoso", "Plasmodio", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(24f, 18f, 6f, 6f, 5f, 3f),
                "Red movil: reconfigura rutas y busca recursos lejos.",
                new[] { V5StructureId.MucilageMatrix }, V5MetabolismType.None, V5EvolutionPath.SlimeMold,
                new Color(0.94f, 0.78f, 0.28f, 1f), "Red movil.", "Expansion economica.", "Conecta colonias.", "Moho mucilaginoso.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.ColonialAdhesin, V5AdaptationId.Nucleus }, mvpEnabled: false));

            Add(Def(V5AdaptationId.ChemicalMemory, "Memoria quimica", "Memoria", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(18f, 10f, 5f, 3f, 6f, 2f),
                "Recuerda zonas productivas y amenazas recientes. Rasgo distintivo de Physarum.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.98f, 0.86f, 0.36f, 1f), "Mejor sensor.", "Rastreo de recursos.", "Red inteligente.", "Moho mucilaginoso.",
                V5AdaptationKind.Active, true, new[] { V5AdaptationId.ColonialAdhesin }));

            Add(Def(V5AdaptationId.PersistentAdhesion, "Adhesion persistente", "Persistente", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(26f, 18f, 8f, 7f, 6f, 3f),
                "Las hijas pegadas ya funcionan como cuerpo estable y no como contacto temporal.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.93f, 0.84f, 0.50f, 1f), "Mas estabilidad.", "", "Cuerpo mas resistente.", "Pre-Volvox.",
                V5AdaptationKind.Milestone, false, new[] { V5AdaptationId.ColonialAdhesin }));

            Add(Def(V5AdaptationId.CellDifferentiation, "Diferenciacion celular", "Diferenciacion", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(30f, 20f, 9f, 8f, 9f, 4f),
                "Las hijas pueden expresar roles internos coherentes dentro del cuerpo.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.70f, 0.92f, 0.66f, 1f), "Roles internos.", "Castas mas claras.", "Cuerpo funcional.", "Multicelularidad.",
                V5AdaptationKind.Milestone, false, new[] { V5AdaptationId.PersistentAdhesion }));

            Add(Def(V5AdaptationId.SignalingCommunication, "Comunicacion / senales", "Senales", V5AdaptationTier.T4ColonialBody,
                V5ResourceWallet.Cost(28f, 18f, 8f, 6f, 10f, 3f),
                "Coordina defensa, produccion y retirada entre celulas del cuerpo.",
                new[] { V5StructureId.RecognitionReceptors }, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(0.66f, 0.90f, 0.88f, 1f), "Sensor y coordinacion.", "", "Sincronia corporal.", "Cuerpo avanzado.",
                prerequisites: new[] { V5AdaptationId.CellDifferentiation }));

            Add(Def(V5AdaptationId.BiologicalChampion, "Campeon biologico", "Campeon", V5AdaptationTier.T5Apex,
                V5ResourceWallet.Cost(70f, 45f, 22f, 18f, 18f, 10f),
                "Forma apex rara: no reemplaza la identidad, la corona por una unidad premium.",
                noStructures, V5MetabolismType.None, V5EvolutionPath.Uncommitted,
                new Color(1.00f, 0.92f, 0.45f, 1f), "Pico de poder.", "Produce forma premium.", "Campeon temporal.", "Apex.",
                V5AdaptationKind.Apex, false, new[] { V5AdaptationId.SignalingCommunication },
                4, false, "", "Dano concentrado durante ventana, control de zona.", "activaciones, winrate_post_champion, tiempo_a_morir"));
        }

        private static V5AdaptationDefinition Def(
            V5AdaptationId id,
            string displayName,
            string shortName,
            V5AdaptationTier tier,
            V5ResourceWallet cost,
            string description,
            V5StructureId[] legacyStructures,
            V5MetabolismType metabolism,
            V5EvolutionPath routeHint,
            Color color,
            string motherEffect,
            string productionEffect,
            string bodyEffect,
            string identityEffect,
            V5AdaptationKind kind = V5AdaptationKind.Active,
            bool countsTowardCap = true,
            V5AdaptationId[] prerequisites = null,
            int visualTier = 1,
            bool latentInChampion = false,
            string championEffect = "",
            string naturalCounters = "",
            string telemetryKey = "",
            bool mvpEnabled = true)
        {
            V5AdaptationDefinition d = new V5AdaptationDefinition();
            d.id = id;
            d.displayName = displayName;
            d.shortName = shortName;
            d.tier = tier;
            d.kind = kind;
            d.cost = cost;
            d.description = description;
            d.legacyStructures = legacyStructures ?? noStructures;
            d.legacyMetabolism = metabolism;
            d.routeHint = routeHint;
            d.color = color;
            d.motherEffect = motherEffect;
            d.productionEffect = productionEffect;
            d.bodyEffect = bodyEffect;
            d.identityEffect = identityEffect;
            d.countsTowardCap = countsTowardCap;
            d.prerequisites = prerequisites ?? noPrerequisites;
            d.visualTier = Mathf.Clamp(visualTier, 1, 4);
            d.latentInChampion = latentInChampion;
            d.championEffect = championEffect;
            d.naturalCounters = naturalCounters;
            d.telemetryKey = telemetryKey;
            d.mvpEnabled = mvpEnabled;
            return d;
        }

        private static void Add(V5AdaptationDefinition def)
        {
            defs[def.id] = def;
            ordered.Add(def);
        }
    }
}
