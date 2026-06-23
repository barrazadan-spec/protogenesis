using UnityEngine;

namespace Protogenesis.V5
{
    public static class V5EcologySpawnPolicy
    {
        public static bool InitialSpawnIsHostile(V5ScenarioId scenario, int index)
        {
            switch (scenario)
            {
                case V5ScenarioId.FirstDrop:
                case V5ScenarioId.Freeplay:
                    return false;
                case V5ScenarioId.OxygenWar:
                    return index >= 3;
                case V5ScenarioId.AcidFrontier:
                    return index >= 2;
                case V5ScenarioId.PredatorBloom:
                    return index >= 1;
                default:
                    return index > 2;
            }
        }

        public static V5EvolutionPath PickInitialPath(V5ScenarioId scenario, int index)
        {
            switch (scenario)
            {
                case V5ScenarioId.FirstDrop:
                    return index == 0 ? V5EvolutionPath.Bacteria : V5EvolutionPath.Cyanobacteria;
                case V5ScenarioId.OxygenWar:
                    return index % 3 == 0 ? V5EvolutionPath.Cyanobacteria : (index % 3 == 1 ? V5EvolutionPath.Bacteria : V5EvolutionPath.Flagellate);
                case V5ScenarioId.AcidFrontier:
                    return index % 2 == 0 ? V5EvolutionPath.Archaea : V5EvolutionPath.Bacteria;
                case V5ScenarioId.PredatorBloom:
                    return index % 4 == 0 ? V5EvolutionPath.Amoeba : (index % 4 == 1 ? V5EvolutionPath.Flagellate : V5EvolutionPath.Bacteria);
                case V5ScenarioId.Freeplay:
                    return index % 2 == 0 ? V5EvolutionPath.Bacteria : V5EvolutionPath.Cyanobacteria;
                default:
                    return V5EvolutionPath.Bacteria;
            }
        }

        public static void ConfigureInitialNpc(V5CellEntity cell, V5ScenarioId scenario, int index)
        {
            if (cell == null) return;
            bool hostile = InitialSpawnIsHostile(scenario, index);
            if (hostile)
            {
                if (cell.GetComponent<V5EnemyBrain>() == null) cell.gameObject.AddComponent<V5EnemyBrain>();
                return;
            }

            cell.Directive = index % 3 == 0 ? V5Directive.Explore : (index % 3 == 1 ? V5Directive.Colonize : V5Directive.Farm);
            cell.Stats.sensorRange *= 0.55f;
            cell.Stats.speed *= 0.72f;
            cell.Stats.physicalDamagePerSecond *= 0.20f;
            cell.Stats.chemicalDamagePerSecond *= 0.20f;
            cell.Stats.colonizationPower *= 0.55f;
        }

        public static float EcologicalAwakening(V5GameManager gm)
        {
            if (gm == null) return 0f;
            V5CellEntity mother = gm.MotherCell;
            float structureScore = mother != null ? Mathf.Clamp01(Mathf.Max(0, mother.ActiveStructureCount - 1) / 6f) : 0f;
            if (gm.Adaptations != null) structureScore = Mathf.Max(structureScore, Mathf.Clamp01(gm.Adaptations.ActiveCount() / 8f));
            float metabolismScore = mother != null && mother.Metabolism != V5MetabolismType.None ? 0.16f : 0f;
            float colonizationScore = gm.Environment != null ? Mathf.Clamp01(gm.Environment.AverageColonization() / 0.18f) : 0f;
            float populationScore = Mathf.Clamp01(Mathf.Max(0, gm.PlayerCellCount() - 1) / 8f);
            float geneScore = gm.Genes != null ? Mathf.Clamp01(gm.Genes.UnlockedCount / 4f) : 0f;
            if (gm.Adaptations != null) geneScore = Mathf.Max(geneScore, Mathf.Clamp01(gm.Adaptations.ActiveCount() / 6f));
            float timeScore = Mathf.Clamp01((gm.ElapsedSeconds - 90f) / 720f);

            float identityBonus = 0f;
            if (gm.Adaptations != null)
            {
                V5AdaptationSystem a = gm.Adaptations;
                if (a.Has(V5AdaptationId.BacterialFlagellum) || a.Has(V5AdaptationId.EukaryoticFlagellum) || a.Has(V5AdaptationId.Cilia) || a.Has(V5AdaptationId.ChemicalMemory)) identityBonus += 0.12f;
                if (a.Has(V5AdaptationId.ProkaryoticThylakoid) || a.Has(V5AdaptationId.Chloroplast) || a.Has(V5AdaptationId.CelluloseWall) || a.Has(V5AdaptationId.SilicaFrustule)) identityBonus += 0.12f;
                if (a.Has(V5AdaptationId.SlimePlasmodium) || a.Has(V5AdaptationId.FungalHypha) || a.Has(V5AdaptationId.ExtracellularEnzymes) || a.Has(V5AdaptationId.ColonialAdhesin)) identityBonus += 0.12f;
                if (a.Has(V5AdaptationId.Lysosome) || a.Has(V5AdaptationId.Pseudopods) || a.Has(V5AdaptationId.Cilia)) identityBonus += 0.14f;
                if (gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA) identityBonus += 0.06f;
            }
            else if (mother != null)
            {
                if (mother.HasStructure(V5StructureId.BacterialFlagellum) || mother.HasStructure(V5StructureId.EukaryoticFlagellum) || mother.HasStructure(V5StructureId.Cilia)) identityBonus += 0.12f;
                if (mother.HasStructure(V5StructureId.Thylakoid) || mother.HasStructure(V5StructureId.MicroalgalChloroplast)) identityBonus += 0.12f;
                if (mother.HasStructure(V5StructureId.MucilageMatrix) || mother.HasStructure(V5StructureId.InvasiveHypha)) identityBonus += 0.12f;
                if (mother.HasStructure(V5StructureId.Lysosome) || mother.HasStructure(V5StructureId.PiercingStylet) || mother.HasStructure(V5StructureId.CoronaCilia)) identityBonus += 0.14f;
            }

            return Mathf.Clamp01(structureScore * 0.30f + metabolismScore + colonizationScore * 0.20f + populationScore * 0.16f + geneScore * 0.12f + timeScore * 0.06f + identityBonus);
        }

        public static float HostileGraceSeconds(V5ScenarioId scenario)
        {
            switch (scenario)
            {
                case V5ScenarioId.FirstDrop: return 300f;
                case V5ScenarioId.Freeplay: return 240f;
                case V5ScenarioId.OxygenWar: return 170f;
                case V5ScenarioId.AcidFrontier: return 150f;
                case V5ScenarioId.PredatorBloom: return 65f;
                default: return 150f;
            }
        }
    }
}
