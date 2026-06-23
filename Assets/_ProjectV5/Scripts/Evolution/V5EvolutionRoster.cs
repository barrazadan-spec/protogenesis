namespace Protogenesis.V5
{
    public static class V5EvolutionRoster
    {
        public static readonly V5EvolutionPath[] ProkaryoteRoutes =
        {
            V5EvolutionPath.Bacteria,
            V5EvolutionPath.Archaea,
            V5EvolutionPath.Cyanobacteria
        };

        public static readonly V5EvolutionPath[] EukaryoteRoutes =
        {
            V5EvolutionPath.Amoeba,
            V5EvolutionPath.Flagellate,
            V5EvolutionPath.Ciliate,
            V5EvolutionPath.Microalga,
            V5EvolutionPath.Fungus,
            V5EvolutionPath.SlimeMold
        };

        public static readonly V5EvolutionPath[] MulticellularRoutes =
        {
            V5EvolutionPath.Rotifer,
            V5EvolutionPath.Nematode
        };

        public static readonly V5EvolutionPath[] NeutralOrApexRoutes =
        {
            V5EvolutionPath.Tardigrade
        };

        public static readonly V5EvolutionPath[] PrimaryRoutes =
        {
            V5EvolutionPath.Bacteria,
            V5EvolutionPath.Archaea,
            V5EvolutionPath.Cyanobacteria,
            V5EvolutionPath.Amoeba,
            V5EvolutionPath.Flagellate,
            V5EvolutionPath.Ciliate,
            V5EvolutionPath.Microalga,
            V5EvolutionPath.Fungus,
            V5EvolutionPath.SlimeMold,
            V5EvolutionPath.Rotifer,
            V5EvolutionPath.Nematode
        };

        public static bool IsPrimaryRoute(V5EvolutionPath path)
        {
            for (int i = 0; i < PrimaryRoutes.Length; i++)
                if (PrimaryRoutes[i] == path) return true;
            return false;
        }

        public static string CategoryName(V5EvolutionPath path)
        {
            if (Contains(ProkaryoteRoutes, path)) return "Procariota";
            if (Contains(EukaryoteRoutes, path)) return "Eucariota";
            if (Contains(MulticellularRoutes, path)) return "Multicelular";
            if (Contains(NeutralOrApexRoutes, path)) return "Neutral/Apex";
            return "Legado";
        }

        private static bool Contains(V5EvolutionPath[] routes, V5EvolutionPath path)
        {
            for (int i = 0; i < routes.Length; i++)
                if (routes[i] == path) return true;
            return false;
        }
    }
}
