using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public struct V5FunctionalCasteDefinition
    {
        public V5FunctionalCasteId id;
        public string displayName;
        public string shortName;
        public string effectSummary;
        public Color primaryColor;
        public V5CellModeId defaultMode;
        public V5BodySlotRole preferredBodyRole;
        public bool prefersBody;
        public bool bodyOnly;

        public V5FunctionalCasteDefinition(
            V5FunctionalCasteId id,
            string displayName,
            string shortName,
            string effectSummary,
            Color primaryColor,
            V5CellModeId defaultMode,
            V5BodySlotRole preferredBodyRole,
            bool prefersBody,
            bool bodyOnly)
        {
            this.id = id;
            this.displayName = displayName;
            this.shortName = shortName;
            this.effectSummary = effectSummary;
            this.primaryColor = primaryColor;
            this.defaultMode = defaultMode;
            this.preferredBodyRole = preferredBodyRole;
            this.prefersBody = prefersBody;
            this.bodyOnly = bodyOnly;
        }
    }

    public static class V5CasteLibrary
    {
        public static V5FunctionalCasteDefinition Get(V5FunctionalCasteId id)
        {
            switch (id)
            {
                case V5FunctionalCasteId.Gatherer:
                    return Def(id, "Recolectora", "Rec", "Recolecta y transporta mejor; combate peor.", new Color(0.48f, 1f, 0.46f, 1f), V5CellModeId.Gather, V5BodySlotRole.Producer, false, false);
                case V5FunctionalCasteId.Attacker:
                    return Def(id, "Atacante", "Atk", "Caza y persigue mejor; economia peor.", new Color(1f, 0.38f, 0.22f, 1f), V5CellModeId.Hunt, V5BodySlotRole.Mouth, false, false);
                case V5FunctionalCasteId.Defender:
                    return Def(id, "Defensora", "Def", "Absorbe presion y protege formaciones; se mueve lento.", new Color(0.74f, 0.57f, 1f, 1f), V5CellModeId.Defend, V5BodySlotRole.Armor, true, false);
                case V5FunctionalCasteId.Producer:
                    return Def(id, "Productora", "Prod", "Genera y recolecta soporte; vulnerable fuera de posicion.", new Color(0.84f, 1f, 0.30f, 1f), V5CellModeId.Gather, V5BodySlotRole.Producer, true, false);
                case V5FunctionalCasteId.Sensor:
                    return Def(id, "Sensora", "Sen", "Explora y detecta amenazas; combate directo debil.", new Color(1f, 0.88f, 0.28f, 1f), V5CellModeId.Scout, V5BodySlotRole.Sensor, false, false);
                case V5FunctionalCasteId.Structural:
                    return Def(id, "Estructural", "Est", "Da soporte al cuerpo y conectividad; fuera del cuerpo rinde poco.", new Color(0.66f, 0.62f, 0.78f, 1f), V5CellModeId.Colonize, V5BodySlotRole.Connector, true, true);
                default:
                    return Def(V5FunctionalCasteId.Hybrid, "Hibrida", "Hib", "Flexible, sin bonus fuertes ni rol optimo.", new Color(0.88f, 0.91f, 0.88f, 1f), V5CellModeId.FollowLineage, V5BodySlotRole.Connector, false, false);
            }
        }

        public static V5FunctionalCasteId FromGerminalCaste(V5GerminalCasteId id)
        {
            switch (id)
            {
                case V5GerminalCasteId.LineageGatherer: return V5FunctionalCasteId.Gatherer;
                case V5GerminalCasteId.LineageScout: return V5FunctionalCasteId.Sensor;
                case V5GerminalCasteId.LineageDefender: return V5FunctionalCasteId.Defender;
                case V5GerminalCasteId.LineageRaider: return V5FunctionalCasteId.Attacker;
                case V5GerminalCasteId.AmoeboidGuard: return V5FunctionalCasteId.Defender;
                case V5GerminalCasteId.CiliateController: return V5FunctionalCasteId.Sensor;
                case V5GerminalCasteId.BacterialSymbiont: return V5FunctionalCasteId.Structural;
                case V5GerminalCasteId.MicroalgaSupport: return V5FunctionalCasteId.Producer;
                default: return V5FunctionalCasteId.Hybrid;
            }
        }

        public static float DamageMultiplier(V5FunctionalCasteId id, V5CellModeId mode, bool attached)
        {
            switch (id)
            {
                case V5FunctionalCasteId.Attacker:
                    return mode == V5CellModeId.Gather ? 0.55f : 1.35f;
                case V5FunctionalCasteId.Gatherer:
                    return mode == V5CellModeId.Gather ? 0.70f : 0.55f;
                case V5FunctionalCasteId.Defender:
                    return mode == V5CellModeId.Defend || attached ? 0.85f : 0.70f;
                case V5FunctionalCasteId.Producer:
                    return attached ? 0.45f : 0.35f;
                case V5FunctionalCasteId.Sensor:
                    return 0.55f;
                case V5FunctionalCasteId.Structural:
                    return attached ? 0.55f : 0.30f;
                default:
                    return 1f;
            }
        }

        public static float SpeedMultiplier(V5FunctionalCasteId id, bool attached)
        {
            if (attached) return 1f;
            switch (id)
            {
                case V5FunctionalCasteId.Attacker: return 1.08f;
                case V5FunctionalCasteId.Defender: return 0.78f;
                case V5FunctionalCasteId.Producer: return 0.48f;
                case V5FunctionalCasteId.Sensor: return 1.24f;
                case V5FunctionalCasteId.Structural: return 0.28f;
                default: return 1f;
            }
        }

        public static float SynthesisMultiplier(V5FunctionalCasteId id, V5CellModeId mode, bool attached)
        {
            switch (id)
            {
                case V5FunctionalCasteId.Gatherer:
                    return mode == V5CellModeId.Gather ? 1.35f : 1.10f;
                case V5FunctionalCasteId.Producer:
                    return attached ? 1.55f : 1.28f;
                case V5FunctionalCasteId.Sensor:
                    return mode == V5CellModeId.Scout ? 0.92f : 0.82f;
                case V5FunctionalCasteId.Attacker:
                    return 0.65f;
                case V5FunctionalCasteId.Defender:
                    return 0.75f;
                case V5FunctionalCasteId.Structural:
                    return attached ? 0.95f : 0.45f;
                default:
                    return 1f;
            }
        }

        public static float DamageTakenMultiplier(V5FunctionalCasteId id, bool attached)
        {
            switch (id)
            {
                case V5FunctionalCasteId.Defender: return 0.78f;
                case V5FunctionalCasteId.Structural: return attached ? 0.66f : 1.45f;
                case V5FunctionalCasteId.Producer: return 1.18f;
                case V5FunctionalCasteId.Sensor: return 1.15f;
                case V5FunctionalCasteId.Gatherer: return 1.08f;
                default: return 1f;
            }
        }

        public static float ColonizationMultiplier(V5FunctionalCasteId id, bool attached)
        {
            switch (id)
            {
                case V5FunctionalCasteId.Structural: return attached ? 1.35f : 0.90f;
                case V5FunctionalCasteId.Gatherer: return 1.10f;
                case V5FunctionalCasteId.Producer: return 1.08f;
                case V5FunctionalCasteId.Attacker: return 0.70f;
                case V5FunctionalCasteId.Sensor: return 0.88f;
                default: return 1f;
            }
        }

        public static float RepairMultiplier(V5FunctionalCasteId id, bool attached)
        {
            switch (id)
            {
                case V5FunctionalCasteId.Defender: return 1.20f;
                case V5FunctionalCasteId.Structural: return attached ? 1.30f : 0.75f;
                case V5FunctionalCasteId.Producer: return 0.88f;
                default: return 1f;
            }
        }

        public static string CompositionSummary(IReadOnlyList<V5CellEntity> cells)
        {
            if (cells == null || cells.Count == 0) return "sin hijas";
            int hybrid = 0;
            int gatherer = 0;
            int attacker = 0;
            int defender = 0;
            int producer = 0;
            int sensor = 0;
            int structural = 0;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                switch (c.FunctionalCaste)
                {
                    case V5FunctionalCasteId.Gatherer: gatherer++; break;
                    case V5FunctionalCasteId.Attacker: attacker++; break;
                    case V5FunctionalCasteId.Defender: defender++; break;
                    case V5FunctionalCasteId.Producer: producer++; break;
                    case V5FunctionalCasteId.Sensor: sensor++; break;
                    case V5FunctionalCasteId.Structural: structural++; break;
                    default: hybrid++; break;
                }
            }

            int total = hybrid + gatherer + attacker + defender + producer + sensor + structural;
            if (total == 0) return "sin hijas";
            return "Rec " + gatherer + " / Atk " + attacker + " / Def " + defender + " / Prod " + producer + " / Sen " + sensor + " / Est " + structural + " / Hib " + hybrid;
        }

        private static V5FunctionalCasteDefinition Def(
            V5FunctionalCasteId id,
            string displayName,
            string shortName,
            string effectSummary,
            Color color,
            V5CellModeId defaultMode,
            V5BodySlotRole preferredBodyRole,
            bool prefersBody,
            bool bodyOnly)
        {
            return new V5FunctionalCasteDefinition(id, displayName, shortName, effectSummary, color, defaultMode, preferredBodyRole, prefersBody, bodyOnly);
        }
    }
}
