using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5FormationPlannerIMGUI : MonoBehaviour
    {
        private enum FormationMode { Swarm, Ring, Screen, Line, HarvestFan, ColonizeWeb }

        private bool showPanel;
        private FormationMode lastMode = FormationMode.Swarm;
        private string lastAction = "sin formación";
        private float spacing = 1.35f;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Start() => V5PanelRouter.Register("Formaciones", () => showPanel, v => showPanel = v);

        private void Update()
        {
            // BackQuote removed — RuntimeSettings uses it for MinimalOverlay; open via Paneles menu
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            Rect r = new Rect(20, 212, 390, 318);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "FORMACIONES RTS — 1.6", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 36), "Reordena las células seleccionadas. Si no hay selección, usa toda la colonia excepto la madre.");
            GUI.Label(new Rect(r.x + 12, r.y + 76, 150, 20), "Espaciado " + spacing.ToString("0.0"));
            spacing = GUI.HorizontalSlider(new Rect(r.x + 126, r.y + 80, r.width - 148, 20), spacing, 0.7f, 3.0f);

            float y = r.y + 108;
            if (GUI.Button(new Rect(r.x + 12, y, 116, 30), "Swarm", button)) Apply(FormationMode.Swarm);
            GUI.Label(new Rect(r.x + 138, y + 4, r.width - 150, 26), "Agrupa en blob alrededor del cursor.");
            y += 36;
            if (GUI.Button(new Rect(r.x + 12, y, 116, 30), "Anillo madre", button)) Apply(FormationMode.Ring);
            GUI.Label(new Rect(r.x + 138, y + 4, r.width - 150, 26), "Órbita defensiva alrededor de la madre.");
            y += 36;
            if (GUI.Button(new Rect(r.x + 12, y, 116, 30), "Pantalla", button)) Apply(FormationMode.Screen);
            GUI.Label(new Rect(r.x + 138, y + 4, r.width - 150, 26), "Línea entre madre y cursor.");
            y += 36;
            if (GUI.Button(new Rect(r.x + 12, y, 116, 30), "Línea", button)) Apply(FormationMode.Line);
            GUI.Label(new Rect(r.x + 138, y + 4, r.width - 150, 26), "Despliegue lineal centrado en cursor.");
            y += 36;
            if (GUI.Button(new Rect(r.x + 12, y, 116, 30), "Fan farmeo", button)) Apply(FormationMode.HarvestFan);
            GUI.Label(new Rect(r.x + 138, y + 4, r.width - 150, 26), "Asigna farm y dispersa recolectoras.");
            y += 36;
            if (GUI.Button(new Rect(r.x + 12, y, 116, 30), "Red colonial", button)) Apply(FormationMode.ColonizeWeb);
            GUI.Label(new Rect(r.x + 138, y + 4, r.width - 150, 26), "Crea nodos colonizadores alrededor del cursor.");

            GUI.Label(new Rect(r.x + 12, r.y + r.height - 28, r.width - 24, 20), lastAction);
        }

        private void Apply(FormationMode mode)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            List<V5CellEntity> cells = GatherCells(gm);
            if (cells.Count == 0)
            {
                Toast("No hay células para formar");
                return;
            }

            Vector2 cursor = MouseWorld();
            Vector2 motherPos = gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : cursor;
            Vector2 forward = cursor - motherPos;
            if (forward.sqrMagnitude < 0.1f) forward = Vector2.up;
            forward.Normalize();
            Vector2 right = new Vector2(forward.y, -forward.x);

            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell == null) continue;
                Vector2 target = cursor;
                if (mode == FormationMode.Swarm)
                {
                    target = cursor + Random.insideUnitCircle * spacing * Mathf.Sqrt(i + 1f) * 0.32f;
                    cell.Directive = V5Directive.Move;
                }
                else if (mode == FormationMode.Ring)
                {
                    float a = (Mathf.PI * 2f) * i / Mathf.Max(1, cells.Count);
                    target = motherPos + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * (2.5f + spacing);
                    cell.Directive = V5Directive.Defend;
                }
                else if (mode == FormationMode.Screen)
                {
                    float offset = (i - (cells.Count - 1) * 0.5f) * spacing;
                    target = motherPos + forward * 4.0f + right * offset;
                    cell.Directive = V5Directive.Move;
                }
                else if (mode == FormationMode.Line)
                {
                    float offset = (i - (cells.Count - 1) * 0.5f) * spacing;
                    target = cursor + right * offset;
                    cell.Directive = V5Directive.Move;
                }
                else if (mode == FormationMode.HarvestFan)
                {
                    float offset = (i - (cells.Count - 1) * 0.5f) * spacing;
                    target = motherPos + forward * (4.5f + (i % 3) * 1.5f) + right * offset;
                    cell.Directive = V5Directive.Farm;
                }
                else if (mode == FormationMode.ColonizeWeb)
                {
                    float a = (Mathf.PI * 2f) * i / Mathf.Max(1, cells.Count);
                    float ring = 2.2f + (i % 3) * spacing;
                    target = cursor + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ring;
                    cell.Directive = V5Directive.Colonize;
                }
                cell.DirectiveTarget = target;
                if (gm.MotherCell != null) cell.Mother = gm.MotherCell;
            }
            lastMode = mode;
            lastAction = string.Format("{0}: {1} células", mode, cells.Count);
            Toast(lastAction);
        }

        private List<V5CellEntity> GatherCells(V5GameManager gm)
        {
            List<V5CellEntity> result = new List<V5CellEntity>(32);
            if (gm.Selection != null && gm.Selection.Selected.Count > 0)
            {
                for (int i = 0; i < gm.Selection.Selected.Count; i++)
                {
                    V5CellEntity c = gm.Selection.Selected[i];
                    if (c != null && c.IsPlayerOwned) result.Add(c);
                }
                return result;
            }
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c != null && c.Role != V5CellRole.Mother) result.Add(c);
            }
            return result;
        }

        private Vector2 MouseWorld()
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector2.zero;
            Vector3 m = Input.mousePosition;
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -cam.transform.position.z));
            return new Vector2(w.x, w.y);
        }

        private void Toast(string text)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(text);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.fontSize = 12;
            title = new GUIStyle(GUI.skin.label); title.fontSize = 16; title.fontStyle = FontStyle.Bold; title.normal.textColor = new Color(0.85f, 1f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 12; button.wordWrap = true;
        }
    }
}
