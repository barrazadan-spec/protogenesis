using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5AutomationDoctrine
    {
        Balanced,
        Expansion,
        Turtle,
        BloomEconomy,
        PredatorWeb,
        Recovery
    }

    /// <summary>
    /// Macro layer for RTS play: assigns lineage roles and default directives so the player can run a colony,
    /// not babysit every cell. Toggle panel with 7.
    /// </summary>
    public class V5ColonyAutomationSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public bool AutomationEnabled = true;
        public bool AutoAssignRoles = true;
        public bool AutoAssignDirectives = true;
        public bool AutoStressCare = true;
        public V5AutomationDoctrine Doctrine = V5AutomationDoctrine.Balanced;
        public string LastAction = "Automatización lista.";
        public float LastEconomyNeed { get; private set; }
        public float LastDefenseNeed { get; private set; }
        public float LastExpansionNeed { get; private set; }
        public float LastRecoveryNeed { get; private set; }

        private float tickTimer;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private Vector2 scroll;

        private void Start() => V5PanelRouter.Register("Auto", () => ShowPanel, v => ShowPanel = v);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha7)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Auto"); ShowPanel = !ShowPanel; }
            tickTimer += Time.deltaTime;
            if (tickTimer < 1.0f) return;
            tickTimer = 0f;
            if (!AutomationEnabled) return;
            TickAutomation();
        }

        private void TickAutomation()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;

            EvaluateNeeds(gm);
            List<V5CellEntity> cells = AlivePlayerCells(gm);
            if (cells.Count == 0) return;

            if (AutoAssignRoles) AssignRoles(gm, cells);
            if (AutoAssignDirectives) AssignDirectives(gm, cells);
            if (AutoStressCare) ApplyStressCare(gm, cells);
        }

        private List<V5CellEntity> AlivePlayerCells(V5GameManager gm)
        {
            List<V5CellEntity> result = new List<V5CellEntity>(gm.PlayerCells.Count);
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c != null && c.Stats.currentHp > 0f) result.Add(c);
            }
            return result;
        }

        private void EvaluateNeeds(V5GameManager gm)
        {
            V5CellEntity m = gm.MotherCell;
            float cells = Mathf.Max(1f, gm.PlayerCellCount());
            float enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0f;
            float resources = m.Resources.atp + m.Resources.biomass + m.Resources.aminoAcids + m.Resources.lipids + m.Resources.nucleotides;
            float colonized = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;

            LastEconomyNeed = Mathf.Clamp01(1f - resources / 260f);
            LastDefenseNeed = Mathf.Clamp01(enemies / Mathf.Max(4f, cells * 1.6f));
            LastExpansionNeed = Mathf.Clamp01(1f - colonized / 0.35f);
            LastRecoveryNeed = Mathf.Clamp01((m.Stats.stress / 100f) + (1f - m.Stats.currentHp / Mathf.Max(1f, m.Stats.maxHp)) * 0.5f);
        }

        private void AssignRoles(V5GameManager gm, List<V5CellEntity> cells)
        {
            int nonMother = Mathf.Max(0, cells.Count - 1);
            if (nonMother <= 0) return;

            int desiredFarmers = Mathf.CeilToInt(nonMother * 0.35f);
            int desiredScouts = Mathf.CeilToInt(nonMother * 0.12f);
            int desiredDefenders = Mathf.CeilToInt(nonMother * 0.18f);
            int desiredColonizers = Mathf.CeilToInt(nonMother * 0.25f);
            int desiredPredators = Mathf.CeilToInt(nonMother * 0.10f);

            if (Doctrine == V5AutomationDoctrine.Expansion) { desiredColonizers += 2; desiredScouts += 1; desiredDefenders = Mathf.Max(1, desiredDefenders - 1); }
            else if (Doctrine == V5AutomationDoctrine.Turtle) { desiredDefenders += 3; desiredColonizers = Mathf.Max(1, desiredColonizers - 2); }
            else if (Doctrine == V5AutomationDoctrine.BloomEconomy) { desiredFarmers += 3; desiredPredators = Mathf.Max(0, desiredPredators - 1); }
            else if (Doctrine == V5AutomationDoctrine.PredatorWeb) { desiredPredators += 3; desiredFarmers = Mathf.Max(1, desiredFarmers - 1); }
            else if (Doctrine == V5AutomationDoctrine.Recovery) { desiredFarmers += 2; desiredDefenders += 2; desiredColonizers = Mathf.Max(1, desiredColonizers - 2); desiredPredators = 0; }

            int farmers = CountRole(cells, V5LineageRole.Farmer);
            int scouts = CountRole(cells, V5LineageRole.Scout);
            int defenders = CountRole(cells, V5LineageRole.Defender);
            int colonizers = CountRole(cells, V5LineageRole.Colonizer);
            int predators = CountRole(cells, V5LineageRole.Predator);

            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c.Role == V5CellRole.Mother || c.Role == V5CellRole.Apex) continue;
                if (c.LineageRole != V5LineageRole.Generalist) continue;

                if (farmers < desiredFarmers) { c.LineageRole = V5LineageRole.Farmer; farmers++; continue; }
                if (scouts < desiredScouts) { c.LineageRole = V5LineageRole.Scout; scouts++; continue; }
                if (defenders < desiredDefenders) { c.LineageRole = V5LineageRole.Defender; defenders++; continue; }
                if (colonizers < desiredColonizers) { c.LineageRole = V5LineageRole.Colonizer; colonizers++; continue; }
                if (predators < desiredPredators) { c.LineageRole = V5LineageRole.Predator; predators++; continue; }
            }

            LastAction = "Roles recalculados: F" + farmers + " S" + scouts + " D" + defenders + " C" + colonizers + " P" + predators;
        }

        private int CountRole(List<V5CellEntity> cells, V5LineageRole role)
        {
            int count = 0;
            for (int i = 0; i < cells.Count; i++) if (cells[i] != null && cells[i].LineageRole == role) count++;
            return count;
        }

        private void AssignDirectives(V5GameManager gm, List<V5CellEntity> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (c.Directive == V5Directive.Move) continue; // respect manual move orders until player changes directive

                if (Doctrine == V5AutomationDoctrine.Recovery && c.Stats.stress > 65f)
                {
                    c.ApplyCellMode(V5CellModeId.Defend);
                    c.Mother = gm.MotherCell;
                    continue;
                }

                switch (c.LineageRole)
                {
                    case V5LineageRole.Farmer: c.ApplyCellMode(V5CellModeId.Gather); break;
                    case V5LineageRole.Scout: c.ApplyCellMode(V5CellModeId.Scout); break;
                    case V5LineageRole.Defender: c.ApplyCellMode(V5CellModeId.Defend); c.Mother = gm.MotherCell; break;
                    case V5LineageRole.Colonizer: c.ApplyCellMode(V5CellModeId.Colonize); break;
                    case V5LineageRole.Predator: c.ApplyCellMode(V5CellModeId.Hunt); break;
                    case V5LineageRole.Recycler: c.ApplyCellMode(V5CellModeId.Gather); c.LineageRole = V5LineageRole.Recycler; break;
                    default:
                        c.ApplyCellMode(LastEconomyNeed > LastExpansionNeed ? V5CellModeId.Gather : V5CellModeId.Colonize);
                        break;
                }
            }
        }

        private void ApplyStressCare(V5GameManager gm, List<V5CellEntity> cells)
        {
            V5CellEntity m = gm.MotherCell;
            if (m == null) return;
            if (LastRecoveryNeed < 0.45f) return;

            float available = Mathf.Min(m.Resources.atp, m.Resources.lipids * 2f);
            if (available < 8f) return;

            float stressDrop = Mathf.Min(7.5f, available * 0.18f);
            m.Resources.atp -= stressDrop * 0.7f;
            m.Resources.lipids -= stressDrop * 0.25f;
            m.Stats.stress = Mathf.Max(0f, m.Stats.stress - stressDrop);
            m.Stats.currentHp = Mathf.Min(m.Stats.maxHp, m.Stats.currentHp + stressDrop * 0.8f);
            LastAction = "Homeostasis automática: -" + stressDrop.ToString("0.0") + " stress en madre.";
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(18f, 212f, 420f, 490f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("AUTOMATIZACION COLONIAL - POST-MVP", titleStyle);
            AutomationEnabled = GUILayout.Toggle(AutomationEnabled, "Activar macro-IA de colonia");
            AutoAssignRoles = GUILayout.Toggle(AutoAssignRoles, "Auto-asignar roles de linaje");
            AutoAssignDirectives = GUILayout.Toggle(AutoAssignDirectives, "Auto-asignar modos celulares");
            AutoStressCare = GUILayout.Toggle(AutoStressCare, "Homeostasis automática si hay crisis");
            GUILayout.Space(8f);
            GUILayout.Label("Doctrina:", bodyStyle);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(170f));
            DrawDoctrineButton(V5AutomationDoctrine.Balanced, "Balanced", "Reparte colonia entre farm, defensa y colonización.");
            DrawDoctrineButton(V5AutomationDoctrine.Expansion, "Expansion", "Más scouts y colonizers. Ideal para ganar por territorio.");
            DrawDoctrineButton(V5AutomationDoctrine.Turtle, "Turtle", "Más defensa. Protege a la madre y estabiliza.");
            DrawDoctrineButton(V5AutomationDoctrine.BloomEconomy, "Bloom Economy", "Más farmers. Acelera recursos y build orders.");
            DrawDoctrineButton(V5AutomationDoctrine.PredatorWeb, "Predator Web", "Más ataque. Útil contra oleadas y miniboss.");
            DrawDoctrineButton(V5AutomationDoctrine.Recovery, "Recovery", "Baja agresión y prioriza stress/HP.");
            GUILayout.EndScrollView();
            GUILayout.Space(8f);
            GUILayout.Label("Needs — Econ " + Percent(LastEconomyNeed) + " | Def " + Percent(LastDefenseNeed) + " | Exp " + Percent(LastExpansionNeed) + " | Rec " + Percent(LastRecoveryNeed), bodyStyle);
            GUILayout.Label("Última acción: " + LastAction, bodyStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndArea();
        }

        private void DrawDoctrineButton(V5AutomationDoctrine doctrine, string label, string desc)
        {
            GUI.enabled = Doctrine != doctrine;
            if (GUILayout.Button(label + " - " + desc, GUILayout.Height(40f))) Doctrine = doctrine;
            GUI.enabled = true;
        }

        private string Percent(float value)
        {
            return (Mathf.Clamp01(value) * 100f).ToString("0") + "%";
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.fontSize = 17; titleStyle.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
        }
    }
}
