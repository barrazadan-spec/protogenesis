using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public struct V5StructureDefinition
    {
        public V5StructureId id;
        public string displayName;
        public V5CellDomain domain;
        public V5ResourceWallet cost;
        public float biomassLoad;
        public float hpBonus;
        public float speedMultiplier;
        public float atpBonus;
        public float synthesisMultiplier;
        public float toxinResist;
        public float chemicalDamage;
        public float physicalDamage;
        public float colonization;
        public bool enablesPhotosynthesis;
        public bool enablesPhagocytosis;
        public bool enablesBiofilm;
        public bool enablesRecognition;
        public string description;
    }

    public struct V5PathDefinition
    {
        public V5EvolutionPath path;
        public V5CellDomain domain;
        public string displayName;
        public Color color;
        public float worldSize;
        public V5StructureId[] keyStructures;
        public string fantasy;
    }

    public static class V5EvolutionLibrary
    {
        private static Dictionary<V5StructureId, V5StructureDefinition> structures;
        private static Dictionary<V5EvolutionPath, V5PathDefinition> paths;

        public static V5StructureDefinition GetStructure(V5StructureId id)
        {
            Ensure();
            return structures[id];
        }

        public static IEnumerable<V5StructureDefinition> AllStructures()
        {
            Ensure();
            return structures.Values;
        }

        public static V5PathDefinition GetPath(V5EvolutionPath path)
        {
            Ensure();
            return paths[path];
        }

        public static Color ColorForPath(V5EvolutionPath path)
        {
            Ensure();
            return paths.ContainsKey(path) ? paths[path].color : V5Colors.LUCA;
        }

        private static void Ensure()
        {
            if (structures != null && paths != null) return;
            structures = new Dictionary<V5StructureId, V5StructureDefinition>();
            paths = new Dictionary<V5EvolutionPath, V5PathDefinition>();
            AddStructures();
            AddPaths();
        }

        private static void Add(V5StructureId id, string name, V5CellDomain domain, V5ResourceWallet cost, float load, float hp, float speedMul, float atp, float synthMul, float toxin, float chem, float phys, float colonize, bool photo, bool phago, bool biofilm, bool recog, string desc)
        {
            V5StructureDefinition d = new V5StructureDefinition();
            d.id = id;
            d.displayName = name;
            d.domain = domain;
            d.cost = cost;
            d.biomassLoad = load;
            d.hpBonus = hp;
            d.speedMultiplier = speedMul;
            d.atpBonus = atp;
            d.synthesisMultiplier = synthMul;
            d.toxinResist = toxin;
            d.chemicalDamage = chem;
            d.physicalDamage = phys;
            d.colonization = colonize;
            d.enablesPhotosynthesis = photo;
            d.enablesPhagocytosis = phago;
            d.enablesBiofilm = biofilm;
            d.enablesRecognition = recog;
            d.description = desc;
            structures[id] = d;
        }

        private static void AddStructures()
        {
            Add(V5StructureId.GeneticCompartment, "Compartimento genetico", V5CellDomain.LUCA, V5ResourceWallet.Cost(12, 10, 0, 2, 8, 0), 15, 0, 1f, 0, 1f, 0, 0, 0, 0.05f, false, false, false, false, "Centro de comando. Requisito para division avanzada.");
            Add(V5StructureId.MetabolicEngine, "Motor metabolico", V5CellDomain.LUCA, V5ResourceWallet.Cost(18, 8, 2, 4, 0, 2), 8, 0, 1f, 2.2f, 1f, 0, 0, 0, 0.05f, false, false, false, false, "Aumenta ATP pasivo y habilita metabolismo.");
            Add(V5StructureId.SynthesisMachinery, "Maquinaria de sintesis", V5CellDomain.LUCA, V5ResourceWallet.Cost(14, 7, 8, 0, 2, 0), 6, 0, 1f, 0, 1.35f, 0, 0, 0, 0.05f, false, false, false, false, "Instala estructuras mas rapido y mejora reparacion.");
            Add(V5StructureId.StorageVacuole, "Vacuola de almacenamiento", V5CellDomain.LUCA, V5ResourceWallet.Cost(10, 5, 0, 5, 0, 0), 5, 0, 0.96f, 0, 1f, 0, 0, 0, 0.1f, false, false, false, false, "Mas capacidad de recursos y colonizacion segura.");
            Add(V5StructureId.Catalase, "Catalasa / Peroxisoma", V5CellDomain.LUCA, V5ResourceWallet.Cost(12, 6, 6, 1, 0, 1), 6, 0, 1f, 0, 1f, 0.35f, 0, 0, 0, false, false, false, false, "Resistencia contra toxinas y ROS.");

            Add(V5StructureId.BacterialFlagellum, "Flagelo bacteriano", V5CellDomain.Prokaryote, V5ResourceWallet.Cost(16, 7, 5, 2, 0, 0), 7, 0, 1.75f, 0, 1f, 0, 0, 0.5f, 0, false, false, false, false, "Motor rotatorio barato para swarm.");
            Add(V5StructureId.PeptidoglycanWall, "Pared de peptidoglicano", V5CellDomain.Prokaryote, V5ResourceWallet.Cost(14, 8, 0, 6, 0, 2), 8, 48, 0.86f, 0, 1f, 0.25f, 0, 0, 0.08f, false, false, false, false, "Defensa fuerte, menor movilidad.");
            Add(V5StructureId.Capsule, "Capsula", V5CellDomain.Prokaryote, V5ResourceWallet.Cost(12, 7, 0, 7, 0, 0), 7, 28, 0.94f, 0, 1f, 0.28f, 0, 0, 0.16f, false, false, false, false, "Halo protector contra fagocitosis.");
            Add(V5StructureId.Fimbriae, "Fimbrias", V5CellDomain.Prokaryote, V5ResourceWallet.Cost(12, 8, 4, 3, 0, 1), 8, 8, 0.92f, 0, 1f, 0.12f, 0.3f, 0, 0.42f, false, false, true, false, "Adhesion y biofilm territorial.");
            Add(V5StructureId.Plasmid, "Plasmido conjugativo", V5CellDomain.Prokaryote, V5ResourceWallet.Cost(15, 6, 2, 0, 8, 0), 6, 0, 1f, 0.2f, 1.05f, 0, 0.2f, 0, 0.08f, false, false, false, false, "Comparte rasgos y acelera adaptacion.");
            Add(V5StructureId.Thylakoid, "Tilacoide", V5CellDomain.LUCA, V5ResourceWallet.Cost(20, 11, 4, 6, 2, 2), 11, 0, 0.96f, 0, 1f, 0, 0, 0, 0.1f, true, false, false, false, "Fotosintesis, oxigeno y ATP en luz.");

            Add(V5StructureId.EukaryoticFlagellum, "Flagelo eucariota", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(22, 10, 8, 4, 0, 0), 10, 0, 2.1f, 0, 1f, 0, 0, 0.8f, 0, false, false, false, false, "Movimiento ondulante, hit-and-run.");
            Add(V5StructureId.Lysosome, "Lisosoma", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(18, 9, 10, 3, 0, 0), 9, 0, 0.98f, 0, 1f, 0, 0, 2.6f, 0, false, true, false, false, "Fagocitosis y digestion al contacto.");
            Add(V5StructureId.CelluloseWall, "Pared de celulosa/quitina", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(16, 8, 0, 6, 0, 2), 8, 48, 0.84f, 0, 1f, 0.25f, 0, 0, 0.12f, false, false, false, false, "Defensa de rutas vegetales, hongo y microalga.");
            Add(V5StructureId.InvasiveHypha, "Hifa invasiva", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(24, 12, 8, 8, 0, 3), 12, 8, 0.82f, 0, 1f, 0.12f, 1.4f, 0, 0.55f, false, false, true, false, "Red territorial que digiere y coloniza.");
            Add(V5StructureId.Cilia, "Cilios", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(20, 9, 9, 3, 0, 0), 9, 0, 1.55f, 0, 1f, 0, 0.4f, 0.4f, 0.2f, false, false, false, false, "Corrientes y control de microzona.");
            Add(V5StructureId.AzurophilicGranule, "Granulo azurofilo", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(20, 9, 12, 2, 0, 1), 9, 0, 0.98f, 0, 1f, 0, 1.8f, 0, 0, false, false, false, false, "Dano quimico heredado; recomendado como ability futura, no ruta base.");
            Add(V5StructureId.RecognitionReceptors, "Receptores de reconocimiento", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(18, 7, 8, 2, 4, 0), 7, 0, 1f, 0, 1.05f, 0, 0, 0.2f, 0, false, false, false, true, "Detecta firmas biologicas y mejora respuesta tactica.");
            Add(V5StructureId.SecretoryVesicle, "Vesicula secretora", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(18, 8, 8, 5, 2, 0), 8, 0, 0.96f, 0, 1f, 0, 1.0f, 0, 0.08f, false, false, false, false, "Secrecion a distancia simplificada para enzimas/toxinas.");
            Add(V5StructureId.StemPlasticity, "Plasticidad madre", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(30, 15, 12, 8, 8, 4), 18, 20, 0.96f, 0.5f, 1.15f, 0.1f, 0, 0, 0.15f, false, false, false, false, "Ruta adaptadora especial con pivot evolutivo.");

            Add(V5StructureId.MicroalgalChloroplast, "Cloroplasto microalgal", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(26, 12, 6, 6, 15, 3), 13, 0, 0.92f, 0.4f, 1f, 0.05f, 0, 0, 0.18f, true, false, false, false, "Fotosintesis organelar: mas ATP y oxigeno en luz, empuja Microalga.");
            Add(V5StructureId.MucilageMatrix, "Matriz de mucilago", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(22, 12, 10, 8, 3, 2), 12, 18, 0.88f, 0, 1.08f, 0.18f, 0.75f, 0, 0.62f, false, false, true, false, "Red movil de mucilago: captura detritus, deja memoria quimica y ralentiza zonas.");
            Add(V5StructureId.CoronaCilia, "Corona ciliada", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(24, 10, 12, 4, 0, 1), 11, 8, 1.22f, 0, 1.12f, 0.08f, 0.25f, 1.1f, 0.18f, false, true, false, false, "Filtracion y succion frontal para Rotifero/Ciliado; fuerte contra swarms pequenos.");
            Add(V5StructureId.PiercingStylet, "Estilete perforador", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(24, 12, 15, 3, 4, 4), 12, 4, 1.08f, 0, 1f, 0, 0, 3.4f, 0.02f, false, false, false, true, "Ataque de puncion: rompe biofilms, hifas y membranas blandas.");
            Add(V5StructureId.Cuticle, "Cuticula flexible", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(18, 11, 4, 9, 0, 6), 10, 54, 0.90f, 0, 1f, 0.30f, 0, 0, 0.06f, false, false, false, false, "Capa defensiva de microfauna; mejora resistencia ambiental a cambio de velocidad.");
            Add(V5StructureId.CryptobiosisTun, "Criptobiosis / tun", V5CellDomain.Eukaryote, V5ResourceWallet.Cost(28, 14, 8, 12, 6, 8), 16, 46, 0.76f, 0.15f, 0.90f, 0.55f, 0, 0, 0.25f, false, false, false, false, "Modo de supervivencia extrema: aguanta eventos y conserva colonizacion minima.");
        }

        private static void AddPath(V5EvolutionPath path, V5CellDomain domain, string name, Color color, float size, V5StructureId[] keys, string fantasy)
        {
            V5PathDefinition p = new V5PathDefinition();
            p.path = path;
            p.domain = domain;
            p.displayName = name;
            p.color = color;
            p.worldSize = size;
            p.keyStructures = keys;
            p.fantasy = fantasy;
            paths[path] = p;
        }

        private static void AddPaths()
        {
            AddPath(V5EvolutionPath.Uncommitted, V5CellDomain.LUCA, "LUCA", V5Colors.LUCA, 0.55f, new V5StructureId[] { V5StructureId.GeneticCompartment, V5StructureId.MetabolicEngine, V5StructureId.SynthesisMachinery }, "protocelula flexible");
            AddPath(V5EvolutionPath.Bacteria, V5CellDomain.Prokaryote, "Bacteria", V5Colors.Bacteria, 0.32f, new V5StructureId[] { V5StructureId.BacterialFlagellum, V5StructureId.Plasmid, V5StructureId.Fimbriae }, "swarm rapido y biofilm");
            AddPath(V5EvolutionPath.Archaea, V5CellDomain.Prokaryote, "Arquea", V5Colors.Archaea, 0.40f, new V5StructureId[] { V5StructureId.Capsule, V5StructureId.PeptidoglycanWall, V5StructureId.Catalase }, "fortaleza quimica extrema");
            AddPath(V5EvolutionPath.Cyanobacteria, V5CellDomain.Prokaryote, "Cianobacteria", V5Colors.Cyanobacteria, 0.38f, new V5StructureId[] { V5StructureId.Thylakoid, V5StructureId.PeptidoglycanWall, V5StructureId.Catalase }, "terraformadora de oxigeno");
            AddPath(V5EvolutionPath.Fungus, V5CellDomain.Eukaryote, "Hongo", V5Colors.Fungus, 0.55f, new V5StructureId[] { V5StructureId.InvasiveHypha, V5StructureId.CelluloseWall, V5StructureId.SecretoryVesicle }, "red territorial digestiva");
            AddPath(V5EvolutionPath.Amoeba, V5CellDomain.Eukaryote, "Ameba", V5Colors.Amoeba, 0.95f, new V5StructureId[] { V5StructureId.Lysosome, V5StructureId.StorageVacuole, V5StructureId.Catalase }, "depredador grande por fagocitosis");
            AddPath(V5EvolutionPath.Flagellate, V5CellDomain.Eukaryote, "Flagelado", V5Colors.Flagellate, 0.64f, new V5StructureId[] { V5StructureId.EukaryoticFlagellum, V5StructureId.MetabolicEngine, V5StructureId.Catalase }, "hit-and-run movil");
            AddPath(V5EvolutionPath.Ciliate, V5CellDomain.Eukaryote, "Ciliado", V5Colors.Ciliate, 0.68f, new V5StructureId[] { V5StructureId.Cilia, V5StructureId.StorageVacuole, V5StructureId.Lysosome }, "corrientes y control de recursos");
            AddPath(V5EvolutionPath.Microalga, V5CellDomain.Eukaryote, "Microalga", V5Colors.Microalga, 0.58f, new V5StructureId[] { V5StructureId.MicroalgalChloroplast, V5StructureId.CelluloseWall, V5StructureId.Thylakoid }, "economia luminica organelar");
            AddPath(V5EvolutionPath.SlimeMold, V5CellDomain.Eukaryote, "Moho mucilaginoso", V5Colors.SlimeMold, 0.72f, new V5StructureId[] { V5StructureId.MucilageMatrix, V5StructureId.SecretoryVesicle, V5StructureId.StorageVacuole }, "red movil, memoria quimica y detritus");
            AddPath(V5EvolutionPath.Rotifer, V5CellDomain.Multicellular, "Rotifero", V5Colors.Rotifer, 0.86f, new V5StructureId[] { V5StructureId.CoronaCilia, V5StructureId.Lysosome, V5StructureId.Cuticle }, "filtracion anti-swarm y control de particulas");
            AddPath(V5EvolutionPath.Nematode, V5CellDomain.Multicellular, "Nematodo", V5Colors.Nematode, 0.80f, new V5StructureId[] { V5StructureId.PiercingStylet, V5StructureId.Cuticle, V5StructureId.EukaryoticFlagellum }, "perforador de biofilms y corredores");
            AddPath(V5EvolutionPath.Tardigrade, V5CellDomain.Multicellular, "Tardigrado neutral/apex", V5Colors.Tardigrade, 0.92f, new V5StructureId[] { V5StructureId.CryptobiosisTun, V5StructureId.Cuticle, V5StructureId.Catalase }, "criptobiosis extrema como neutral raro o apex defensivo");

            AddPath(V5EvolutionPath.Neutrophil, V5CellDomain.Eukaryote, "Neutrofilo (legado)", V5Colors.Neutrophil, 0.55f, new V5StructureId[] { V5StructureId.AzurophilicGranule, V5StructureId.RecognitionReceptors, V5StructureId.Lysosome }, "ruta heredada: mover a ability/expansion");
            AddPath(V5EvolutionPath.Macrophage, V5CellDomain.Eukaryote, "Macrofago (legado)", V5Colors.Macrophage, 0.72f, new V5StructureId[] { V5StructureId.Lysosome, V5StructureId.RecognitionReceptors, V5StructureId.StorageVacuole }, "ruta heredada: rasgos migran a Ameba/Rotifero");
            AddPath(V5EvolutionPath.NaturalKiller, V5CellDomain.Eukaryote, "Celula NK (legado)", V5Colors.NaturalKiller, 0.58f, new V5StructureId[] { V5StructureId.RecognitionReceptors, V5StructureId.AzurophilicGranule, V5StructureId.EukaryoticFlagellum }, "ruta heredada: firma selectiva como mutacion futura");
            AddPath(V5EvolutionPath.BCell, V5CellDomain.Eukaryote, "Celula B (legado)", V5Colors.BCell, 0.58f, new V5StructureId[] { V5StructureId.RecognitionReceptors, V5StructureId.SecretoryVesicle, V5StructureId.StorageVacuole }, "ruta heredada: soporte/marcadores en expansion");
            AddPath(V5EvolutionPath.StemCell, V5CellDomain.Eukaryote, "Celula Madre adaptadora", V5Colors.StemCell, 0.80f, new V5StructureId[] { V5StructureId.StemPlasticity, V5StructureId.GeneticCompartment, V5StructureId.MetabolicEngine }, "pivot evolutivo especial, no ruta primaria");
        }
    }
}
