namespace Protogenesis.V5
{
    public enum V5OrganismSizeClass
    {
        Pico,
        Small,
        Medium,
        Large,
        Microfauna,
        Apex
    }

    public enum V5RosterPlayStatus
    {
        Mvp,
        Core,
        Experimental,
        NeutralOnly,
        Retired
    }

    public struct V5RosterBalanceEntry
    {
        public V5EvolutionPath path;
        public V5OrganismSizeClass sizeClass;
        public V5RosterPlayStatus playStatus;
        public float populationWeight;
        public int recommendedSquadMin;
        public int recommendedSquadMax;
        public int recommendedHardCap;
        public int visualOrganismMin;
        public int visualOrganismMax;
        public string combatNiche;

        public V5RosterBalanceEntry(V5EvolutionPath path, V5OrganismSizeClass sizeClass, V5RosterPlayStatus playStatus, float populationWeight, int squadMin, int squadMax, int hardCap, int visualMin, int visualMax, string combatNiche)
        {
            this.path = path;
            this.sizeClass = sizeClass;
            this.playStatus = playStatus;
            this.populationWeight = populationWeight;
            recommendedSquadMin = squadMin;
            recommendedSquadMax = squadMax;
            recommendedHardCap = hardCap;
            visualOrganismMin = visualMin;
            visualOrganismMax = visualMax;
            this.combatNiche = combatNiche;
        }
    }

    public static class V5RosterBalance
    {
        public static V5RosterBalanceEntry Get(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Bacteria:
                    return Entry(path, V5OrganismSizeClass.Pico, V5RosterPlayStatus.Mvp, 0.65f, 10, 18, 24, 6, 12, "swarm, biofilm y toxina local");
                case V5EvolutionPath.Archaea:
                    return Entry(path, V5OrganismSizeClass.Small, V5RosterPlayStatus.Mvp, 0.80f, 6, 12, 22, 2, 6, "resistencia extrema y control de pH");
                case V5EvolutionPath.Cyanobacteria:
                    return Entry(path, V5OrganismSizeClass.Small, V5RosterPlayStatus.Mvp, 0.75f, 9, 16, 24, 5, 10, "fotosintesis, oxigenacion y economia territorial");
                case V5EvolutionPath.Amoeba:
                    return Entry(path, V5OrganismSizeClass.Large, V5RosterPlayStatus.Mvp, 1.55f, 3, 6, 12, 1, 1, "fagocitosis activa y duelos por contacto");
                case V5EvolutionPath.Flagellate:
                    return Entry(path, V5OrganismSizeClass.Medium, V5RosterPlayStatus.Core, 1.05f, 5, 10, 18, 1, 3, "movilidad, flanqueo y persecucion");
                case V5EvolutionPath.Ciliate:
                    return Entry(path, V5OrganismSizeClass.Medium, V5RosterPlayStatus.Core, 1.20f, 4, 8, 16, 1, 2, "corrientes, control de particulas y caza pasiva");
                case V5EvolutionPath.Microalga:
                    return Entry(path, V5OrganismSizeClass.Medium, V5RosterPlayStatus.Core, 1.00f, 6, 12, 20, 2, 6, "economia luminica y defensa celular");
                case V5EvolutionPath.Fungus:
                    return Entry(path, V5OrganismSizeClass.Large, V5RosterPlayStatus.Core, 1.35f, 3, 7, 12, 1, 3, "red fija, digestion territorial y defensa");
                case V5EvolutionPath.SlimeMold:
                    return Entry(path, V5OrganismSizeClass.Large, V5RosterPlayStatus.Core, 1.25f, 3, 8, 14, 1, 3, "red movil, memoria quimica y rutas de detritus");
                case V5EvolutionPath.Rotifer:
                    return Entry(path, V5OrganismSizeClass.Microfauna, V5RosterPlayStatus.Experimental, 1.80f, 2, 5, 8, 1, 1, "filtrador anti-swarm y control de area");
                case V5EvolutionPath.Nematode:
                    return Entry(path, V5OrganismSizeClass.Microfauna, V5RosterPlayStatus.Experimental, 1.90f, 2, 4, 8, 1, 1, "perforador lineal y ruptura de biofilms");
                case V5EvolutionPath.Tardigrade:
                    return Entry(path, V5OrganismSizeClass.Microfauna, V5RosterPlayStatus.NeutralOnly, 2.20f, 0, 0, 0, 1, 1, "criptobiosis como gen, neutral raro o apex defensivo");
                case V5EvolutionPath.Neutrophil:
                case V5EvolutionPath.Macrophage:
                case V5EvolutionPath.NaturalKiller:
                case V5EvolutionPath.BCell:
                    return Entry(path, V5OrganismSizeClass.Medium, V5RosterPlayStatus.Retired, 1.20f, 3, 6, 0, 1, 1, "ruta inmune retirada del roster biologico principal");
                default:
                    return Entry(path, V5OrganismSizeClass.Small, V5RosterPlayStatus.Core, 1.00f, 4, 8, 18, 1, 3, "generalista sin especializacion definida");
            }
        }

        public static float PopulationWeight(V5EvolutionPath path)
        {
            return Get(path).populationWeight;
        }

        public static float PopulationWeight(V5EvolutionPath path, V5CellRole role)
        {
            float weight = PopulationWeight(path);
            if (role == V5CellRole.Mother) return weight * 1.25f;
            if (role == V5CellRole.Granddaughter) return weight * 0.85f;
            if (role == V5CellRole.Apex) return weight * 2.35f;
            return weight;
        }

        public static float PopulationWeight(V5CellEntity cell)
        {
            if (cell == null) return 0f;
            return PopulationWeight(cell.EvolutionPath, cell.Role);
        }

        public static int RecommendedHardCap(V5EvolutionPath path)
        {
            return Get(path).recommendedHardCap;
        }

        public static int VisualOrganismMin(V5EvolutionPath path)
        {
            return Get(path).visualOrganismMin;
        }

        public static int VisualOrganismMax(V5EvolutionPath path)
        {
            return Get(path).visualOrganismMax;
        }

        public static bool RepresentsMicrocolony(V5EvolutionPath path)
        {
            return Get(path).visualOrganismMax > 1;
        }

        public static string EntityScaleLabel(V5EvolutionPath path)
        {
            V5RosterBalanceEntry entry = Get(path);
            if (entry.visualOrganismMax <= 1) return "1 organismo";
            return "microcolonia " + entry.visualOrganismMin + "-" + entry.visualOrganismMax + " org.";
        }

        public static bool IsMvpPath(V5EvolutionPath path)
        {
            return Get(path).playStatus == V5RosterPlayStatus.Mvp;
        }

        public static bool IsExperimentalPath(V5EvolutionPath path)
        {
            return Get(path).playStatus == V5RosterPlayStatus.Experimental;
        }

        public static bool IsPlayablePath(V5EvolutionPath path)
        {
            V5RosterPlayStatus status = Get(path).playStatus;
            return status == V5RosterPlayStatus.Mvp || status == V5RosterPlayStatus.Core || status == V5RosterPlayStatus.Experimental;
        }

        public static bool IsNeutralOnlyPath(V5EvolutionPath path)
        {
            return Get(path).playStatus == V5RosterPlayStatus.NeutralOnly;
        }

        public static string SizeLabel(V5EvolutionPath path)
        {
            switch (Get(path).sizeClass)
            {
                case V5OrganismSizeClass.Pico: return "pico";
                case V5OrganismSizeClass.Small: return "pequeno";
                case V5OrganismSizeClass.Medium: return "medio";
                case V5OrganismSizeClass.Large: return "grande";
                case V5OrganismSizeClass.Microfauna: return "microfauna";
                case V5OrganismSizeClass.Apex: return "apex";
                default: return "sin clase";
            }
        }

        private static V5RosterBalanceEntry Entry(V5EvolutionPath path, V5OrganismSizeClass sizeClass, V5RosterPlayStatus playStatus, float weight, int squadMin, int squadMax, int hardCap, int visualMin, int visualMax, string combatNiche)
        {
            return new V5RosterBalanceEntry(path, sizeClass, playStatus, weight, squadMin, squadMax, hardCap, visualMin, visualMax, combatNiche);
        }
    }
}
