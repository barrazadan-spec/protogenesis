using UnityEngine;

namespace Protogenesis.V5
{
    public struct V5CellModeDefinition
    {
        public V5CellModeId id;
        public string displayName;
        public string effectSummary;
        public V5Directive directive;
        public V5LineageRole lineageRole;
        public float speedMultiplier;
        public float synthesisMultiplier;
        public float damageMultiplier;
        public float damageTakenMultiplier;
        public float colonizationMultiplier;
        public float repairMultiplier;
        public float stressPerSecond;
        public Color color;

        public V5CellModeDefinition(
            V5CellModeId id,
            string displayName,
            string effectSummary,
            V5Directive directive,
            V5LineageRole lineageRole,
            float speedMultiplier,
            float synthesisMultiplier,
            float damageMultiplier,
            float damageTakenMultiplier,
            float colonizationMultiplier,
            float repairMultiplier,
            float stressPerSecond,
            Color color)
        {
            this.id = id;
            this.displayName = displayName;
            this.effectSummary = effectSummary;
            this.directive = directive;
            this.lineageRole = lineageRole;
            this.speedMultiplier = speedMultiplier;
            this.synthesisMultiplier = synthesisMultiplier;
            this.damageMultiplier = damageMultiplier;
            this.damageTakenMultiplier = damageTakenMultiplier;
            this.colonizationMultiplier = colonizationMultiplier;
            this.repairMultiplier = repairMultiplier;
            this.stressPerSecond = stressPerSecond;
            this.color = color;
        }
    }

    public static class V5CellModeLibrary
    {
        public static V5CellModeDefinition Get(V5CellModeId id)
        {
            switch (id)
            {
                case V5CellModeId.Gather:
                    return new V5CellModeDefinition(id, "Recolectar", "+sintesis, -combate", V5Directive.Farm, V5LineageRole.Farmer, 1.08f, 1.22f, 0.72f, 1.08f, 1f, 1f, 0f, new Color(0.45f, 1f, 0.45f, 1f));
                case V5CellModeId.Defend:
                    return new V5CellModeDefinition(id, "Defender", "+resistencia, -movilidad", V5Directive.Defend, V5LineageRole.Defender, 0.88f, 0.95f, 0.95f, 0.82f, 1f, 1.08f, -0.03f, new Color(0.55f, 0.7f, 1f, 1f));
                case V5CellModeId.Scout:
                    return new V5CellModeDefinition(id, "Explorar", "+velocidad, +riesgo", V5Directive.Explore, V5LineageRole.Scout, 1.18f, 0.92f, 0.75f, 1.12f, 1f, 0.85f, 0.02f, new Color(0.35f, 0.9f, 1f, 1f));
                case V5CellModeId.Colonize:
                    return new V5CellModeDefinition(id, "Colonizar", "+biofilm/red, -movilidad", V5Directive.Colonize, V5LineageRole.Colonizer, 0.82f, 1f, 0.85f, 0.96f, 1.35f, 1f, 0.01f, new Color(0.9f, 0.55f, 1f, 1f));
                case V5CellModeId.Hunt:
                    return new V5CellModeDefinition(id, "Cazar", "+dano, +stress", V5Directive.Attack, V5LineageRole.Predator, 1.06f, 0.88f, 1.22f, 1.08f, 1f, 0.75f, 0.04f, new Color(1f, 0.35f, 0.28f, 1f));
                case V5CellModeId.Recover:
                    return new V5CellModeDefinition(id, "Reparar", "+reparacion, -ataque", V5Directive.ReturnHome, V5LineageRole.Generalist, 0.92f, 0.92f, 0.55f, 0.88f, 1f, 1.45f, -0.05f, new Color(0.86f, 1f, 0.82f, 1f));
                case V5CellModeId.RouteSpecial:
                    return new V5CellModeDefinition(id, "Especial de ruta", "deriva desde la biologia actual", V5Directive.Idle, V5LineageRole.Generalist, 1f, 1f, 1f, 1f, 1f, 1f, 0f, new Color(1f, 0.92f, 0.55f, 1f));
                default:
                    return new V5CellModeDefinition(V5CellModeId.FollowLineage, "Seguir linaje", "cohesion y escolta", V5Directive.FollowMother, V5LineageRole.Generalist, 1f, 1f, 1f, 1f, 1f, 1f, -0.01f, new Color(0.95f, 0.95f, 0.95f, 1f));
            }
        }

        public static V5CellModeId ModeForDirective(V5Directive directive)
        {
            switch (directive)
            {
                case V5Directive.Farm: return V5CellModeId.Gather;
                case V5Directive.Defend: return V5CellModeId.Defend;
                case V5Directive.Explore: return V5CellModeId.Scout;
                case V5Directive.Colonize: return V5CellModeId.Colonize;
                case V5Directive.Attack: return V5CellModeId.Hunt;
                case V5Directive.ReturnHome: return V5CellModeId.Recover;
                default: return V5CellModeId.FollowLineage;
            }
        }

        public static V5CellModeId ResolveRouteSpecial(V5CellEntity cell)
        {
            if (cell == null) return V5CellModeId.FollowLineage;
            switch (cell.EvolutionPath)
            {
                case V5EvolutionPath.Bacteria:
                case V5EvolutionPath.Cyanobacteria:
                case V5EvolutionPath.Fungus:
                case V5EvolutionPath.SlimeMold:
                    return V5CellModeId.Colonize;
                case V5EvolutionPath.Archaea:
                case V5EvolutionPath.Microalga:
                    return V5CellModeId.Gather;
                case V5EvolutionPath.Amoeba:
                case V5EvolutionPath.Flagellate:
                case V5EvolutionPath.Ciliate:
                case V5EvolutionPath.Rotifer:
                case V5EvolutionPath.Nematode:
                    return V5CellModeId.Hunt;
                default:
                    return V5CellModeId.Defend;
            }
        }
    }
}
