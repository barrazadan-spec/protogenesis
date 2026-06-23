using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public struct V5EvolutionAffinityResult
    {
        public V5EvolutionPath path;
        public float score;
        public string reasons;

        public float Score01 { get { return Mathf.Clamp01(score / 100f); } }
        public string PercentLabel { get { return (Score01 * 100f).ToString("0") + "%"; } }
    }

    public static class V5EvolutionAffinitySystem
    {
        public static V5EvolutionAffinityResult Evaluate(V5CellEntity cell, V5EvolutionPath path)
        {
            V5EvolutionAffinityResult result = new V5EvolutionAffinityResult();
            result.path = path;
            if (cell == null || !V5RosterBalance.IsPlayablePath(path))
            {
                result.score = 0f;
                result.reasons = "ruta no jugable";
                return result;
            }

            V5PathDefinition def = V5EvolutionLibrary.GetPath(path);
            List<string> reasons = new List<string>(8);
            float score = 0f;

            AddDomain(cell, def, ref score, reasons);
            AddKeyStructures(cell, def, ref score, reasons);
            AddMetabolism(cell, path, ref score, reasons);
            AddGenes(path, ref score, reasons);
            AddEnvironment(cell, path, ref score, reasons);
            AddHistory(path, ref score, reasons);
            AddCurrentCommitment(cell, path, ref score, reasons);

            result.score = Mathf.Clamp(score, 0f, 100f);
            result.reasons = reasons.Count > 0 ? string.Join(", ", reasons.ToArray()) : "sin senales fuertes";
            return result;
        }

        public static V5EvolutionAffinityResult BestRoute(V5CellEntity cell)
        {
            V5EvolutionAffinityResult best = new V5EvolutionAffinityResult();
            best.path = V5EvolutionPath.Uncommitted;
            best.score = 0f;
            best.reasons = "sin ruta dominante";
            V5EvolutionPath[] routes = V5EvolutionRoster.PrimaryRoutes;
            for (int i = 0; i < routes.Length; i++)
            {
                V5EvolutionAffinityResult candidate = Evaluate(cell, routes[i]);
                if (candidate.score > best.score) best = candidate;
            }
            return best;
        }

        public static float Score01(V5CellEntity cell, V5EvolutionPath path)
        {
            return Evaluate(cell, path).Score01;
        }

        public static bool IsConsolidated(V5CellEntity cell, V5EvolutionPath path)
        {
            return Score01(cell, path) >= V5Balance.RouteConsolidationAffinityThreshold;
        }

        private static void AddDomain(V5CellEntity cell, V5PathDefinition def, ref float score, List<string> reasons)
        {
            if (cell.Domain == def.domain)
            {
                Add(ref score, reasons, 12f, "dominio " + def.domain);
                return;
            }
            if (cell.Domain == V5CellDomain.LUCA)
            {
                Add(ref score, reasons, 5f, "LUCA plastica");
                return;
            }
            if ((cell.Domain == V5CellDomain.Eukaryote && def.domain == V5CellDomain.Multicellular) ||
                (cell.Domain == V5CellDomain.Multicellular && def.domain == V5CellDomain.Eukaryote))
            {
                Add(ref score, reasons, 7f, "continuidad eucariota");
            }
        }

        private static void AddKeyStructures(V5CellEntity cell, V5PathDefinition def, ref float score, List<string> reasons)
        {
            if (def.keyStructures == null) return;
            for (int i = 0; i < def.keyStructures.Length; i++)
            {
                V5StructureId id = def.keyStructures[i];
                if (!cell.HasStructure(id)) continue;
                float value = i == 0 ? 22f : 13f;
                Add(ref score, reasons, value, V5EvolutionLibrary.GetStructure(id).displayName);
            }
        }

        private static void AddMetabolism(V5CellEntity cell, V5EvolutionPath path, ref float score, List<string> reasons)
        {
            V5MetabolismType m = cell.Metabolism;
            if (path == V5EvolutionPath.Bacteria && m == V5MetabolismType.Fermentation) Add(ref score, reasons, 16f, "fermentacion");
            if (path == V5EvolutionPath.Archaea && m == V5MetabolismType.Chemolithotrophy) Add(ref score, reasons, 18f, "quimiolitotrofia");
            if (path == V5EvolutionPath.Cyanobacteria && m == V5MetabolismType.Photosynthesis) Add(ref score, reasons, 18f, "fotosintesis procariota");
            if (path == V5EvolutionPath.Microalga && (m == V5MetabolismType.Photosynthesis || cell.HasPhotosynthesis)) Add(ref score, reasons, 18f, "fotosintesis organelar");
            if ((path == V5EvolutionPath.Amoeba || path == V5EvolutionPath.Flagellate || path == V5EvolutionPath.Ciliate || path == V5EvolutionPath.Fungus || path == V5EvolutionPath.SlimeMold || path == V5EvolutionPath.Rotifer || path == V5EvolutionPath.Nematode) && m == V5MetabolismType.Respiration) Add(ref score, reasons, 10f, "respiracion eucariota");
        }

        private static void AddGenes(V5EvolutionPath path, ref float score, List<string> reasons)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5GeneSystem genes = gm != null ? gm.Genes : null;
            if (genes == null) return;

            if (path == V5EvolutionPath.Bacteria)
            {
                AddGene(genes, V5GeneId.Fermentation, 10f, "gen fermentacion", ref score, reasons);
                AddGene(genes, V5GeneId.RapidDivision, 9f, "division rapida", ref score, reasons);
                AddGene(genes, V5GeneId.Secretion, 7f, "secrecion", ref score, reasons);
                AddGene(genes, V5GeneId.Adhesion, 6f, "adhesion", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Archaea)
            {
                AddGene(genes, V5GeneId.Chemolithotrophy, 12f, "gen quimio", ref score, reasons);
                AddGene(genes, V5GeneId.TotalReabsorption, 5f, "reciclaje", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Cyanobacteria)
            {
                AddGene(genes, V5GeneId.Photosynthesis, 12f, "gen fotosintesis", ref score, reasons);
                AddGene(genes, V5GeneId.Adhesion, 7f, "tapete/biofilm", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Amoeba)
            {
                AddGene(genes, V5GeneId.Recognition, 8f, "reconocimiento de presa", ref score, reasons);
                AddGene(genes, V5GeneId.StrongInheritance, 6f, "herencia fuerte", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Flagellate)
            {
                AddGene(genes, V5GeneId.Motility, 14f, "motilidad", ref score, reasons);
                AddGene(genes, V5GeneId.Autonomy, 7f, "autonomia", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Ciliate)
            {
                AddGene(genes, V5GeneId.Motility, 9f, "motilidad", ref score, reasons);
                AddGene(genes, V5GeneId.Recognition, 7f, "lectura de corrientes", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Microalga)
            {
                AddGene(genes, V5GeneId.Photosynthesis, 14f, "gen fotosintesis", ref score, reasons);
                AddGene(genes, V5GeneId.Symbiosis, 7f, "simbiosis", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Fungus)
            {
                AddGene(genes, V5GeneId.Adhesion, 13f, "adhesion/hifas", ref score, reasons);
                AddGene(genes, V5GeneId.Secretion, 9f, "enzimas externas", ref score, reasons);
            }
            else if (path == V5EvolutionPath.SlimeMold)
            {
                AddGene(genes, V5GeneId.Adhesion, 10f, "matriz movil", ref score, reasons);
                AddGene(genes, V5GeneId.TotalReabsorption, 7f, "memoria/reciclaje", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Rotifer)
            {
                AddGene(genes, V5GeneId.Motility, 8f, "corona movil", ref score, reasons);
                AddGene(genes, V5GeneId.Recognition, 8f, "filtracion selectiva", ref score, reasons);
                AddGene(genes, V5GeneId.ApexMaturation, 5f, "maduracion microfauna", ref score, reasons);
            }
            else if (path == V5EvolutionPath.Nematode)
            {
                AddGene(genes, V5GeneId.Motility, 8f, "movimiento lineal", ref score, reasons);
                AddGene(genes, V5GeneId.Recognition, 7f, "quimiorrecepcion", ref score, reasons);
                AddGene(genes, V5GeneId.ApexMaturation, 5f, "maduracion microfauna", ref score, reasons);
            }
        }

        private static void AddEnvironment(V5CellEntity cell, V5EvolutionPath path, ref float score, List<string> reasons)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return;
            int tx, ty;
            gm.Environment.WorldToTile(cell.transform.position, out tx, out ty);
            float light = gm.Environment.lightLevel[tx, ty];
            float oxygen = gm.Environment.oxygen[tx, ty];
            float toxins = gm.Environment.toxins[tx, ty];
            float acid = gm.Environment.acidity[tx, ty];
            float detritus = gm.Environment.detritus[tx, ty];
            float nutrients = gm.Environment.nutrients[tx, ty];
            float localFlux = Mathf.Abs(nutrients - oxygen) + Mathf.Abs(light - toxins);

            if ((path == V5EvolutionPath.Cyanobacteria || path == V5EvolutionPath.Microalga) && light > 0.52f) Add(ref score, reasons, 7f, "luz alta");
            if (path == V5EvolutionPath.Archaea && (acid > 0.66f || toxins > 0.42f)) Add(ref score, reasons, 7f, "zona extrema");
            if ((path == V5EvolutionPath.Fungus || path == V5EvolutionPath.SlimeMold) && detritus > 0.42f) Add(ref score, reasons, 7f, "detritus abundante");
            if (path == V5EvolutionPath.Ciliate && (nutrients > 0.38f || localFlux > 0.35f)) Add(ref score, reasons, 6f, "corriente nutritiva");
            if ((path == V5EvolutionPath.Amoeba || path == V5EvolutionPath.Rotifer) && nutrients > 0.45f) Add(ref score, reasons, 5f, "presas/particulas");
            if (path == V5EvolutionPath.Flagellate && localFlux > 0.38f) Add(ref score, reasons, 5f, "gradiente para raideo");
            if (path == V5EvolutionPath.Nematode && (oxygen < 0.35f || detritus > 0.35f)) Add(ref score, reasons, 5f, "corredor bentico");
            if ((path == V5EvolutionPath.Amoeba || path == V5EvolutionPath.Flagellate || path == V5EvolutionPath.Ciliate) && oxygen > 0.42f) Add(ref score, reasons, 4f, "oxigeno util");
        }

        private static void AddCurrentCommitment(V5CellEntity cell, V5EvolutionPath path, ref float score, List<string> reasons)
        {
            if (cell.EvolutionPath == path) Add(ref score, reasons, 8f, "identidad actual");
        }

        private static void AddHistory(V5EvolutionPath path, ref float score, List<string> reasons)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5AffinityEventLog log = gm != null ? gm.AffinityLog : null;
            if (log == null) return;
            float bonus = log.ScoreBonus(path);
            if (bonus <= 0.01f) return;
            Add(ref score, reasons, bonus, "historial conductual");
        }

        private static void AddGene(V5GeneSystem genes, V5GeneId gene, float value, string reason, ref float score, List<string> reasons)
        {
            if (genes.HasGene(gene)) Add(ref score, reasons, value, reason);
        }

        private static void Add(ref float score, List<string> reasons, float value, string reason)
        {
            score += value;
            if (reasons.Count < 5) reasons.Add("+" + value.ToString("0") + " " + reason);
        }
    }
}
