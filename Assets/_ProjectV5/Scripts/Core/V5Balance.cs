using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public static class V5Balance
    {
        public const int SoftControllableEntityCount = 10;
        public const int HardControllableEntityCap = 24;
        public const int CorePlayerCellCap = 100;
        public const int CoreTotalPlayerCellHardCap = 160;
        public const int SoftCellCap = SoftControllableEntityCount;
        public const int HardCellCap = HardControllableEntityCap;
        public const float SoftPopulationLoad = 14f;
        public const float HardPopulationLoad = 30f;
        public const float MaxAuxiliaryPopulationLoadRatio = 0.30f;
        public const float SymbiosisAuxiliaryPopulationLoadRatio = 0.40f;
        public const float RouteEmergenceAffinityThreshold = 0.32f;
        public const float RouteConsolidationAffinityThreshold = 0.60f;
        public const float RouteApexAffinityThreshold = 0.95f;
        public const float GerminalAuxiliaryAffinityThreshold = 0.32f;
        public const int AffinityEventCapacity = 96;
        public const float AffinityEventMemorySeconds = 720f;
        public const float AffinityEventMinimumRetention = 0.35f;
        public const float AffinityEventScoreCap = 28f;
        public const float AffinityEventMinInterval = 5f;
        public const float MotherCombatDamageMultiplier = 0.35f;
        public const float MotherCombatIncomingDamageMultiplier = 1.18f;
        public const float MotherCombatStressMultiplier = 1.55f;
        public const float MotherDeathDelay = 0.25f;
        public const float BaseDivisionATP = 42f;
        public const float BaseDivisionBiomass = 18f;
        public const float DaughterHp = 78f;
        public const float GranddaughterHp = 48f;
        public const float DaughterInheritanceChance = 0.40f;
        public const float StrongInheritanceChance = 0.65f;
        public const float ResourcePickupRadius = 1.05f;
        public const float ResourceCarryCapacity = 70f;
        public const float ResourceDepositRadius = 2.2f;
        public const float CellSeparationRadius = 0.9f;
        public const float CellSeparationStrength = 1.9f;
        public const float DefaultMapRadius = 42f;
        public const float EnvironmentTick = 0.25f;
        public const float AiTick = 0.15f;
        public const float CombatTick = 0.12f;
        public const float ObjectiveTick = 0.5f;
        public const float SaveVersion = 2.97f;
        public const float ApexCostATP = 150f;
        public const float ApexCostBiomass = 80f;
        public const float ApexCostAminoAcids = 45f;
        public const float ApexCostNucleotides = 25f;
        public const float ApexMinimumTime = 720f;

        public static float DivisionCostATP(V5CellEntity cell)
        {
            float generationFactor = cell.Role == V5CellRole.Mother ? 1f : 1.25f;
            if (cell.Generation >= 2) generationFactor += 0.25f;
            float structureCost = cell.ActiveStructureCount * 8f;
            float stressReduction = Mathf.Clamp(cell.Stats.stress * 0.35f, 0f, 22f);
            float domainModifier = cell.Domain == V5CellDomain.Prokaryote ? -5f : 0f;
            float efficiency = Mathf.Max(0.35f, cell.Stats.divisionEfficiency);
            float result = Mathf.Max(18f, (BaseDivisionATP * generationFactor + structureCost + domainModifier - stressReduction) / efficiency);
            if (cell.IsPlayerOwned && cell.Role == V5CellRole.Mother && cell.Generation == 0 && V5GameManager.Instance != null && V5GameManager.Instance.PlayerCellCount() <= 1)
                result *= 0.72f;
            if (cell.IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.Genes != null)
                result *= V5GameManager.Instance.Genes.DivisionCostMultiplier(cell);
            if (cell.IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.Adaptations != null)
                result *= V5GameManager.Instance.Adaptations.DivisionCostMultiplier(cell);
            if (cell.IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.BalanceProfile != null)
                result *= V5GameManager.Instance.BalanceProfile.DivisionCostMultiplier();
            return result;
        }

        public static float DivisionCostBiomass(V5CellEntity cell)
        {
            float factor = cell.Domain == V5CellDomain.Prokaryote ? 0.82f : 1f;
            if (cell.IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.Adaptations != null)
                factor *= V5GameManager.Instance.Adaptations.DivisionCostMultiplier(cell);
            return Mathf.Max(10f, BaseDivisionBiomass * factor);
        }

        public static float BiomassLoadRatio(V5CellEntity cell)
        {
            float load = 0f;
            for (int i = 0; i < cell.Structures.Count; i++)
            {
                V5StructureDefinition def = V5EvolutionLibrary.GetStructure(cell.Structures[i]);
                load += Mathf.Max(1f, def.biomassLoad);
            }
            return load / Mathf.Max(1f, cell.Stats.maxBiomassLoad);
        }

        public static float StressFromPopulation(int count)
        {
            if (count <= SoftCellCap) return 0f;
            return (count - SoftCellCap) * 0.2f;
        }

        public static float PopulationLoad(IReadOnlyList<V5CellEntity> cells)
        {
            if (cells == null) return 0f;
            float load = 0f;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell != null) load += V5RosterBalance.PopulationWeight(cell);
            }
            return load;
        }

        public static float ProjectedChildPopulationLoad(V5CellEntity parent)
        {
            if (parent == null) return V5RosterBalance.PopulationWeight(V5EvolutionPath.Uncommitted, V5CellRole.Daughter);
            V5CellRole childRole = parent.Generation == 0 ? V5CellRole.Daughter : V5CellRole.Granddaughter;
            return V5RosterBalance.PopulationWeight(parent.EvolutionPath, childRole);
        }

        public static bool WouldExceedPlayerPopulationCap(V5GameManager gm, V5CellEntity parent)
        {
            if (gm == null) return false;
            if (gm.CoreMode)
            {
                if (gm.PlayerTotalCellCount() >= CoreTotalPlayerCellHardCap) return true;
                return gm.PlayerFreeCellCount() >= gm.PlayerCellCap();
            }
            if (gm.PlayerCellCount() >= gm.PlayerCellCap()) return true;
            if (parent != null && parent.EvolutionPath != V5EvolutionPath.Uncommitted)
            {
                int routeHardCap = V5RosterBalance.RecommendedHardCap(parent.EvolutionPath);
                if (routeHardCap <= 0 || RouteCount(gm.PlayerCells, parent.EvolutionPath) >= routeHardCap) return true;
            }
            return PopulationLoad(gm.PlayerCells) + ProjectedChildPopulationLoad(parent) > HardPopulationLoad + 0.001f;
        }

        public static int RouteCount(IReadOnlyList<V5CellEntity> cells, V5EvolutionPath path)
        {
            if (cells == null) return 0;
            int count = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell != null && cell.EvolutionPath == path) count++;
            }
            return count;
        }

        public static float StressFromPopulationLoad(float load)
        {
            if (load <= SoftPopulationLoad) return 0f;
            return (load - SoftPopulationLoad) * 0.16f;
        }
    }
}
