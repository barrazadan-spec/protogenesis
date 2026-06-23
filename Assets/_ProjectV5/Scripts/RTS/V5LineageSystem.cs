using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5LineageSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public string LastMessage = "Asigna roles de linaje con L.";
        private bool showUpgradesTab;
        private float tick;
        private GUIStyle box;
        private GUIStyle button;
        private GUIStyle title;

        private void Start()
        {
            V5PanelRouter.Register("Linaje", () => ShowPanel, v => ShowPanel = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.L)) TogglePanel(false);
            tick += Time.deltaTime;
            if (tick >= 1.0f)
            {
                tick = 0f;
                MaintainRoles();
            }
        }

        public void TogglePanel(bool upgradesTab)
        {
            bool next = !ShowPanel || showUpgradesTab != upgradesTab;
            showUpgradesTab = upgradesTab;
            if (next) V5PanelRouter.CloseOthers("Linaje");
            ShowPanel = next;
        }

        public void ApplyRoleToSelection(V5LineageRole role)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Selection == null) return;
            int count = 0;
            for (int i = 0; i < gm.Selection.Selected.Count; i++)
            {
                V5CellEntity cell = gm.Selection.Selected[i];
                if (cell == null || !cell.IsPlayerOwned) continue;
                ApplyRole(cell, role, true);
                count++;
            }
            LastMessage = "Rol " + role + " asignado a " + count + " células.";
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        public void ApplyRole(V5CellEntity cell, V5LineageRole role, bool forceDirective)
        {
            if (cell == null) return;
            cell.LineageRole = role;
            if (!forceDirective) return;
            switch (role)
            {
                case V5LineageRole.Farmer: cell.ApplyCellMode(V5CellModeId.Gather); break;
                case V5LineageRole.Scout: cell.ApplyCellMode(V5CellModeId.Scout); break;
                case V5LineageRole.Defender: cell.ApplyCellMode(V5CellModeId.Defend); break;
                case V5LineageRole.Colonizer: cell.ApplyCellMode(V5CellModeId.Colonize); break;
                case V5LineageRole.Predator: cell.ApplyCellMode(V5CellModeId.Hunt); break;
                case V5LineageRole.Recycler: cell.ApplyCellMode(V5CellModeId.Gather); cell.LineageRole = V5LineageRole.Recycler; break;
                default: break;
            }
        }

        private void MaintainRoles()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell == null || cell.Role == V5CellRole.Mother) continue;
                if (cell.Directive == V5Directive.Move || cell.Directive == V5Directive.ReturnHome || cell.Directive == V5Directive.Attack) continue;
                if (cell.LineageRole == V5LineageRole.Farmer && cell.Directive != V5Directive.Farm) cell.ApplyCellMode(V5CellModeId.Gather);
                else if (cell.LineageRole == V5LineageRole.Scout && cell.Directive != V5Directive.Explore) cell.ApplyCellMode(V5CellModeId.Scout);
                else if (cell.LineageRole == V5LineageRole.Defender && cell.Directive != V5Directive.Defend) cell.ApplyCellMode(V5CellModeId.Defend);
                else if (cell.LineageRole == V5LineageRole.Colonizer && cell.Directive != V5Directive.Colonize) cell.ApplyCellMode(V5CellModeId.Colonize);
                else if (cell.LineageRole == V5LineageRole.Recycler && cell.Directive != V5Directive.Farm) { cell.ApplyCellMode(V5CellModeId.Gather); cell.LineageRole = V5LineageRole.Recycler; }
            }
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Selection == null) return;
            DrawUnifiedLineagePanel(gm);
            return;
            /*
            Rect r = new Rect(Screen.width - 420, 490, 410, 230);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "ROLES DE LINAJE", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 38), "Selecciona células y asigna una intención persistente. Las hijas mantienen su rol al cargar la run.");
            float y = r.y + 82;
            RoleButton(V5LineageRole.Generalist, r.x + 12, y, 122); RoleButton(V5LineageRole.Farmer, r.x + 144, y, 122); RoleButton(V5LineageRole.Scout, r.x + 276, y, 122);
            y += 38;
            RoleButton(V5LineageRole.Defender, r.x + 12, y, 122); RoleButton(V5LineageRole.Colonizer, r.x + 144, y, 122); RoleButton(V5LineageRole.Predator, r.x + 276, y, 122);
            y += 38;
            RoleButton(V5LineageRole.Recycler, r.x + 12, y, 122);
            GUI.Label(new Rect(r.x + 144, y, 254, 54), LastMessage);
            y += 62;
            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 22), "Selección: " + gm.Selection.Selected.Count + " | L cierra este panel");
            */
        }

        private void DrawUnifiedLineagePanel(V5GameManager gm)
        {
            Rect r = new Rect(Screen.width - 500, 110, 490, 430);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "LINAJE - ROLES Y MEJORAS", title);
            GUI.enabled = showUpgradesTab;
            if (GUI.Button(new Rect(r.x + 12, r.y + 40, 110, 28), "Roles", button)) showUpgradesTab = false;
            GUI.enabled = !showUpgradesTab;
            if (GUI.Button(new Rect(r.x + 128, r.y + 40, 110, 28), "Mejoras", button)) showUpgradesTab = true;
            GUI.enabled = true;
            GUI.Label(new Rect(r.x + 250, r.y + 44, r.width - 262, 22), "L roles | V mejoras");

            if (showUpgradesTab) DrawUpgradesTab(gm, r);
            else DrawRolesTab(gm, r);
        }

        private void DrawRolesTab(V5GameManager gm, Rect r)
        {
            GUI.Label(new Rect(r.x + 12, r.y + 78, r.width - 24, 38), "Selecciona celulas y asigna una intencion persistente. Las hijas conservan el rol como doctrina simple.");
            float y = r.y + 126;
            RoleButton(V5LineageRole.Generalist, r.x + 12, y, 146); RoleButton(V5LineageRole.Farmer, r.x + 170, y, 146); RoleButton(V5LineageRole.Scout, r.x + 328, y, 146);
            y += 42;
            RoleButton(V5LineageRole.Defender, r.x + 12, y, 146); RoleButton(V5LineageRole.Colonizer, r.x + 170, y, 146); RoleButton(V5LineageRole.Predator, r.x + 328, y, 146);
            y += 42;
            RoleButton(V5LineageRole.Recycler, r.x + 12, y, 146);
            GUI.Label(new Rect(r.x + 170, y, 304, 54), LastMessage);
            y += 70;
            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 22), "Seleccion: " + gm.Selection.Selected.Count + " | Mejoras activas: " + (gm.LineageUpgrades != null ? gm.LineageUpgrades.Summary() : "N/A"));
        }

        private void DrawUpgradesTab(V5GameManager gm, Rect r)
        {
            V5LineageUpgradeSystem upgrades = gm.LineageUpgrades;
            if (upgrades == null)
            {
                GUI.Label(new Rect(r.x + 12, r.y + 78, r.width - 24, 28), "Sistema de mejoras no instalado.");
                return;
            }

            GUI.Label(new Rect(r.x + 12, r.y + 78, r.width - 24, 38), "Las mejoras convierten roles RTS en ramas semipermanentes sin crear nuevas especies.");
            float y = r.y + 122;
            foreach (V5LineageUpgradeId id in System.Enum.GetValues(typeof(V5LineageUpgradeId)))
            {
                bool active = upgrades.Has(id);
                V5ResourceWallet cost = upgrades.CostFor(id);
                GUI.enabled = !active && gm.MotherCell != null && gm.MotherCell.Resources.CanPay(cost);
                string label = (active ? "[x] " : "") + id + "\n" + upgrades.DescriptionFor(id) + "\n" + upgrades.CostLabel(cost);
                if (GUI.Button(new Rect(r.x + 12, y, r.width - 24, 54), label, button)) upgrades.TryUnlock(id);
                GUI.enabled = true;
                y += 58f;
                if (y > r.y + r.height - 62f) break;
            }
            GUI.Label(new Rect(r.x + 12, r.y + r.height - 32, r.width - 24, 24), upgrades.LastMessage);
        }

        private void RoleButton(V5LineageRole role, float x, float y, float w)
        {
            if (GUI.Button(new Rect(x, y, w, 32), role.ToString(), button)) ApplyRoleToSelection(role);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.normal.background = Texture2D.whiteTexture;
            box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label);
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button);
            button.wordWrap = true;
        }
    }
}
