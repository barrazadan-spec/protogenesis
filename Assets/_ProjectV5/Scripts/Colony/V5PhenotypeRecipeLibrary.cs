using UnityEngine;

namespace Protogenesis.V5
{
    public struct V5PhenotypeRecipeDefinition
    {
        public V5GerminalCasteId casteId;
        public string code;
        public string bodyPlan;
        public string organellePlan;
        public string inheritancePlan;
        public string effectSummary;
        public V5CellModeId defaultMode;
        public float radiusMultiplier;
        public float repairMultiplier;
        public float toxinResistanceBonus;
        public float attackRangeBonus;
        public float biomassLoadBonus;
        public float divisionEfficiencyMultiplier;
        public Color plateColor;

        public V5PhenotypeRecipeDefinition(
            V5GerminalCasteId casteId,
            string code,
            string bodyPlan,
            string organellePlan,
            string inheritancePlan,
            string effectSummary,
            V5CellModeId defaultMode,
            float radiusMultiplier,
            float repairMultiplier,
            float toxinResistanceBonus,
            float attackRangeBonus,
            float biomassLoadBonus,
            float divisionEfficiencyMultiplier,
            Color plateColor)
        {
            this.casteId = casteId;
            this.code = code;
            this.bodyPlan = bodyPlan;
            this.organellePlan = organellePlan;
            this.inheritancePlan = inheritancePlan;
            this.effectSummary = effectSummary;
            this.defaultMode = defaultMode;
            this.radiusMultiplier = radiusMultiplier;
            this.repairMultiplier = repairMultiplier;
            this.toxinResistanceBonus = toxinResistanceBonus;
            this.attackRangeBonus = attackRangeBonus;
            this.biomassLoadBonus = biomassLoadBonus;
            this.divisionEfficiencyMultiplier = divisionEfficiencyMultiplier;
            this.plateColor = plateColor;
        }
    }

    public struct V5PhenotypeRecipeMaturity
    {
        public float score01;
        public float required01;
        public bool mature;
        public string label;
        public string reason;

        public float Ratio01 { get { return required01 <= 0f ? 1f : Mathf.Clamp01(score01 / required01); } }
    }

    public static class V5PhenotypeRecipeLibrary
    {
        public static V5PhenotypeRecipeDefinition Get(V5GerminalCasteId id)
        {
            switch (id)
            {
                case V5GerminalCasteId.LineageGatherer:
                    return Recipe(id, "LIN-GAT", "Cuerpo liviano con vacuolas de carga", "Vacuola de almacenamiento + membrana permeable", "Hereda identidad, optimiza transporte", "Recoge mejor, pelea peor y vuelve rapido a la madre.", V5CellModeId.Gather, 0.94f, 0.95f, 0f, -0.05f, 12f, 1.04f, new Color(0.45f, 1f, 0.45f, 1f));
                case V5GerminalCasteId.LineageScout:
                    return Recipe(id, "LIN-SCT", "Cuerpo delgado con sensores extendidos", "Sensores quimicos + membrana elastica", "Hereda identidad, prioriza exploracion", "Explora lejos, detecta antes y evita combate sostenido.", V5CellModeId.Scout, 0.86f, 0.85f, 0f, 0.05f, -8f, 1f, new Color(0.35f, 0.9f, 1f, 1f));
                case V5GerminalCasteId.LineageDefender:
                    return Recipe(id, "LIN-DEF", "Cuerpo compacto con membrana densa", "Capsula + matriz protectora", "Hereda identidad, estabiliza sucesion", "Aguanta dano, repara mejor y protege la zona madre.", V5CellModeId.Defend, 1.08f, 1.18f, 0.05f, 0f, 6f, 0.96f, new Color(0.55f, 0.7f, 1f, 1f));
                case V5GerminalCasteId.LineageRaider:
                    return Recipe(id, "LIN-HNT", "Cuerpo contractil orientado a persecucion", "Vesiculas agresivas + sensores de presa", "Hereda identidad, sacrifica estabilidad", "Caza mejor, sube stress y requiere micro tactica.", V5CellModeId.Hunt, 0.96f, 0.82f, 0f, 0.25f, -6f, 0.92f, new Color(1f, 0.35f, 0.28f, 1f));
                case V5GerminalCasteId.AmoeboidGuard:
                    return Recipe(id, "AUX-AMG", "Masa flexible de absorcion local", "Lisosomas + vacuolas grandes", "Auxiliar, no redefine linaje", "Tanque fagocitico lento para cerrar huecos.", V5CellModeId.Defend, 1.16f, 1.12f, 0.04f, 0.05f, 10f, 0.88f, new Color(0.83f, 0.66f, 0.78f, 1f));
                case V5GerminalCasteId.CiliateController:
                    return Recipe(id, "AUX-CIL", "Corona ciliada para control de flujo", "Cilios + boca filtradora", "Auxiliar, depende de medio fluido", "Controla swarms pequenos y filtra en zona nutritiva.", V5CellModeId.Defend, 1f, 1.02f, 0.02f, 0.35f, 0f, 0.94f, new Color(0.56f, 0.64f, 0.68f, 1f));
                case V5GerminalCasteId.BacterialSymbiont:
                    return Recipe(id, "AUX-BIO", "Microcolonia compacta de bajo coste", "Fimbriae + biofilm inicial", "Auxiliar, carga biologica baja", "Coloniza barato y forma superficie viva.", V5CellModeId.Colonize, 0.72f, 0.9f, 0.03f, -0.10f, -12f, 1.08f, new Color(0.42f, 0.71f, 0.83f, 1f));
                case V5GerminalCasteId.MicroalgaSupport:
                    return Recipe(id, "AUX-LUX", "Celula luminica con plastidos visibles", "Cloroplasto + reserva fotosintetica", "Auxiliar, necesita luz o plastidos", "Produce soporte energetico en zonas claras.", V5CellModeId.Gather, 0.95f, 1.02f, 0f, -0.05f, 8f, 0.96f, new Color(0.18f, 0.78f, 0.46f, 1f));
                default:
                    return Recipe(id, "LIN-PLS", "Gota joven de plasticidad alta", "Nucleo brillante + membrana sin especializar", "Hereda identidad sin fijar rol", "Heredera viable y flexible para reaccionar.", V5CellModeId.FollowLineage, 1f, 1f, 0f, 0f, 0f, 1.08f, V5Colors.LUCA);
            }
        }

        public static V5PhenotypeRecipeMaturity EvaluateMaturity(V5GerminalCasteDefinition caste, V5CellEntity mother, V5EvolutionPath targetPath)
        {
            V5PhenotypeRecipeMaturity result = new V5PhenotypeRecipeMaturity();
            if (caste == null)
            {
                result.label = "sin receta";
                result.reason = "receta inexistente";
                return result;
            }

            result.required01 = RequiredMaturity(caste.id);
            result.score01 = RecipeAffinityScore(caste.id, mother, targetPath);
            result.mature = result.required01 <= 0.001f || result.score01 >= result.required01;
            if (result.mature)
            {
                result.label = "madura";
                result.reason = "senales suficientes (" + (result.score01 * 100f).ToString("0") + "%/" + (result.required01 * 100f).ToString("0") + "%)";
            }
            else
            {
                result.label = "inestable";
                result.reason = "faltan senales (" + (result.score01 * 100f).ToString("0") + "%/" + (result.required01 * 100f).ToString("0") + "%)";
            }
            return result;
        }

        public static float StressAdjustment(V5GerminalCasteDefinition caste, V5CellEntity mother, V5EvolutionPath targetPath)
        {
            V5PhenotypeRecipeMaturity maturity = EvaluateMaturity(caste, mother, targetPath);
            if (maturity.required01 <= 0.001f) return 0f;
            if (maturity.mature) return -2f;
            return Mathf.Lerp(6f, 1f, maturity.Ratio01);
        }

        public static void ApplyToCell(V5CellEntity cell, V5GerminalCasteDefinition caste, V5CellEntity mother, V5EvolutionPath targetPath)
        {
            if (cell == null || caste == null) return;
            V5PhenotypeRecipeDefinition recipe = Get(caste.id);
            V5PhenotypeRecipeMaturity maturity = EvaluateMaturity(caste, mother, targetPath);
            float quality = maturity.mature ? 1.12f : Mathf.Lerp(0.72f, 1f, maturity.Ratio01);
            cell.PhenotypeCaste = caste.id;
            cell.SetFunctionalCaste(V5CasteLibrary.FromGerminalCaste(caste.id));
            cell.PhenotypeRecipeCode = recipe.code + (maturity.mature ? "*" : "~");
            cell.PhenotypeRecipeSummary = recipe.effectSummary + " Receta " + maturity.label + ".";
            cell.ApplyCellMode(recipe.defaultMode);
            cell.Stats.radius *= Mathf.Max(0.45f, 1f + (recipe.radiusMultiplier - 1f) * quality);
            cell.Stats.repairPerSecond *= Mathf.Max(0.2f, 1f + (recipe.repairMultiplier - 1f) * quality);
            cell.Stats.toxinResistance = Mathf.Clamp01(cell.Stats.toxinResistance + recipe.toxinResistanceBonus * quality);
            cell.Stats.attackRange = Mathf.Max(0.35f, cell.Stats.attackRange + recipe.attackRangeBonus * quality);
            cell.Stats.maxBiomassLoad = Mathf.Max(10f, cell.Stats.maxBiomassLoad + recipe.biomassLoadBonus * quality);
            cell.Stats.divisionEfficiency *= Mathf.Max(0.4f, 1f + (recipe.divisionEfficiencyMultiplier - 1f) * quality);
        }

        public static string CompactPlan(V5GerminalCasteId id)
        {
            V5PhenotypeRecipeDefinition recipe = Get(id);
            return recipe.code + " | " + recipe.bodyPlan + " | " + V5CellModeLibrary.Get(recipe.defaultMode).displayName;
        }

        public static V5BodySlotRole RecommendedBodyRole(V5GerminalCasteId id)
        {
            switch (id)
            {
                case V5GerminalCasteId.LineageGatherer:
                case V5GerminalCasteId.MicroalgaSupport:
                    return V5BodySlotRole.Producer;
                case V5GerminalCasteId.LineageScout:
                    return V5BodySlotRole.Motor;
                case V5GerminalCasteId.LineageDefender:
                case V5GerminalCasteId.AmoeboidGuard:
                    return V5BodySlotRole.Armor;
                case V5GerminalCasteId.LineageRaider:
                case V5GerminalCasteId.CiliateController:
                    return V5BodySlotRole.Mouth;
                case V5GerminalCasteId.BacterialSymbiont:
                    return V5BodySlotRole.Connector;
                default:
                    return V5BodySlotRole.Connector;
            }
        }

        private static V5PhenotypeRecipeDefinition Recipe(
            V5GerminalCasteId casteId,
            string code,
            string bodyPlan,
            string organellePlan,
            string inheritancePlan,
            string effectSummary,
            V5CellModeId defaultMode,
            float radiusMultiplier,
            float repairMultiplier,
            float toxinResistanceBonus,
            float attackRangeBonus,
            float biomassLoadBonus,
            float divisionEfficiencyMultiplier,
            Color plateColor)
        {
            return new V5PhenotypeRecipeDefinition(casteId, code, bodyPlan, organellePlan, inheritancePlan, effectSummary, defaultMode, radiusMultiplier, repairMultiplier, toxinResistanceBonus, attackRangeBonus, biomassLoadBonus, divisionEfficiencyMultiplier, plateColor);
        }

        private static float RequiredMaturity(V5GerminalCasteId id)
        {
            switch (id)
            {
                case V5GerminalCasteId.PlasticDaughter: return 0f;
                case V5GerminalCasteId.LineageGatherer: return 0.08f;
                case V5GerminalCasteId.LineageScout: return 0.12f;
                case V5GerminalCasteId.LineageDefender: return 0.14f;
                case V5GerminalCasteId.LineageRaider: return 0.18f;
                case V5GerminalCasteId.BacterialSymbiont: return 0.24f;
                case V5GerminalCasteId.AmoeboidGuard:
                case V5GerminalCasteId.CiliateController:
                case V5GerminalCasteId.MicroalgaSupport:
                    return V5Balance.GerminalAuxiliaryAffinityThreshold;
                default: return 0.15f;
            }
        }

        private static float RecipeAffinityScore(V5GerminalCasteId id, V5CellEntity mother, V5EvolutionPath targetPath)
        {
            if (mother == null) return 0f;
            float adaptationScore = AdaptationRecipeScore(id, targetPath);
            float legacyScore;
            switch (id)
            {
                case V5GerminalCasteId.PlasticDaughter:
                    return 1f;
                case V5GerminalCasteId.LineageGatherer:
                    legacyScore = MaxAffinity(mother, targetPath, V5EvolutionPath.Microalga, V5EvolutionPath.SlimeMold, V5EvolutionPath.Cyanobacteria);
                    return Mathf.Max(adaptationScore, legacyScore);
                case V5GerminalCasteId.LineageScout:
                    legacyScore = MaxAffinity(mother, targetPath, V5EvolutionPath.Flagellate, V5EvolutionPath.Nematode, V5EvolutionPath.Ciliate);
                    return Mathf.Max(adaptationScore, legacyScore);
                case V5GerminalCasteId.LineageDefender:
                    legacyScore = MaxAffinity(mother, targetPath, V5EvolutionPath.Archaea, V5EvolutionPath.Fungus, V5EvolutionPath.Cyanobacteria);
                    return Mathf.Max(adaptationScore, legacyScore);
                case V5GerminalCasteId.LineageRaider:
                    legacyScore = MaxAffinity(mother, targetPath, V5EvolutionPath.Amoeba, V5EvolutionPath.Flagellate, V5EvolutionPath.Nematode);
                    return Mathf.Max(adaptationScore, legacyScore);
                default:
                    legacyScore = V5RosterBalance.IsPlayablePath(targetPath) ? V5EvolutionAffinitySystem.Evaluate(mother, targetPath).Score01 : 0f;
                    return Mathf.Max(adaptationScore, legacyScore);
            }
        }

        private static float AdaptationRecipeScore(V5GerminalCasteId id, V5EvolutionPath targetPath)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5AdaptationSystem adaptations = gm != null ? gm.Adaptations : null;
            if (adaptations == null) return 0f;

            float routeScore = V5BiologyCanon.RouteAdaptationScore01(targetPath, adaptations);
            switch (id)
            {
                case V5GerminalCasteId.PlasticDaughter:
                    return 1f;
                case V5GerminalCasteId.LineageGatherer:
                    return Mathf.Max(routeScore * 0.55f, AnyAdaptationScore(adaptations,
                        V5AdaptationId.ContractileVacuole,
                        V5AdaptationId.ProkaryoticThylakoid,
                        V5AdaptationId.Chloroplast,
                        V5AdaptationId.SlimePlasmodium,
                        V5AdaptationId.ExtracellularEnzymes));
                case V5GerminalCasteId.LineageScout:
                    return Mathf.Max(routeScore * 0.55f, AnyAdaptationScore(adaptations,
                        V5AdaptationId.BacterialFlagellum,
                        V5AdaptationId.EukaryoticFlagellum,
                        V5AdaptationId.Cilia,
                        V5AdaptationId.ChemicalMemory));
                case V5GerminalCasteId.LineageDefender:
                    return Mathf.Max(routeScore * 0.55f, AnyAdaptationScore(adaptations,
                        V5AdaptationId.BacterialWall,
                        V5AdaptationId.PolysaccharideCapsule,
                        V5AdaptationId.ExtremophileMembrane,
                        V5AdaptationId.CatalaseROS,
                        V5AdaptationId.CelluloseWall,
                        V5AdaptationId.SilicaFrustule));
                case V5GerminalCasteId.LineageRaider:
                    return Mathf.Max(routeScore * 0.55f, AnyAdaptationScore(adaptations,
                        V5AdaptationId.Lysosome,
                        V5AdaptationId.Pseudopods,
                        V5AdaptationId.BacterialFlagellum,
                        V5AdaptationId.EukaryoticFlagellum,
                        V5AdaptationId.Cilia));
                case V5GerminalCasteId.AmoeboidGuard:
                    return Mathf.Max(routeScore * 0.4f, AllAdaptationScore(adaptations, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods));
                case V5GerminalCasteId.CiliateController:
                    return Mathf.Max(routeScore * 0.4f, AnyAdaptationScore(adaptations, V5AdaptationId.Cilia));
                case V5GerminalCasteId.BacterialSymbiont:
                    return Mathf.Max(routeScore * 0.4f, AnyAdaptationScore(adaptations, V5AdaptationId.BacterialWall, V5AdaptationId.PiliFimbriae, V5AdaptationId.BasicAdhesin));
                case V5GerminalCasteId.MicroalgaSupport:
                    return Mathf.Max(routeScore * 0.4f, AnyAdaptationScore(adaptations, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast));
                default:
                    return routeScore;
            }
        }

        private static float AnyAdaptationScore(V5AdaptationSystem adaptations, params V5AdaptationId[] ids)
        {
            if (adaptations == null || ids == null || ids.Length == 0) return 0f;
            int hits = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (adaptations.Has(ids[i])) hits++;
            }

            if (hits <= 0) return 0f;
            return Mathf.Clamp01(0.20f + hits * 0.18f);
        }

        private static float AllAdaptationScore(V5AdaptationSystem adaptations, params V5AdaptationId[] ids)
        {
            if (adaptations == null || ids == null || ids.Length == 0) return 0f;
            int hits = 0;
            for (int i = 0; i < ids.Length; i++)
            {
                if (adaptations.Has(ids[i])) hits++;
            }

            if (hits <= 0) return 0f;
            return Mathf.Clamp01((float)hits / ids.Length * 0.74f);
        }

        private static float MaxAffinity(V5CellEntity mother, V5EvolutionPath preferred, V5EvolutionPath a, V5EvolutionPath b, V5EvolutionPath c)
        {
            float best = 0f;
            if (V5RosterBalance.IsPlayablePath(preferred)) best = Mathf.Max(best, V5EvolutionAffinitySystem.Evaluate(mother, preferred).Score01);
            best = Mathf.Max(best, V5EvolutionAffinitySystem.Evaluate(mother, a).Score01);
            best = Mathf.Max(best, V5EvolutionAffinitySystem.Evaluate(mother, b).Score01);
            best = Mathf.Max(best, V5EvolutionAffinitySystem.Evaluate(mother, c).Score01);
            return best;
        }
    }
}
