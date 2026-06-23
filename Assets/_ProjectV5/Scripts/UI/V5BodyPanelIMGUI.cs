using UnityEngine;

namespace Protogenesis.V5
{
    public class V5BodyPanelIMGUI : MonoBehaviour
    {
        public bool ShowPanel;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle small;
        private GUIStyle button;

        private void Start()
        {
            V5PanelRouter.Register("Cuerpo", () => ShowPanel, v => ShowPanel = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.B)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Cuerpo"); ShowPanel = !ShowPanel; }
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Body == null) return;

            Rect r = new Rect(Screen.width - 430, 92, 420, 460);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "CUERPO MULTICELULAR", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), gm.Body.Summary, small);
            GUI.Label(new Rect(r.x + 12, r.y + 60, r.width - 24, 20), "Soporte: ATP +" + gm.Body.AtpPerSecondFromBody.ToString("0.00") + "/s | Bio +" + gm.Body.BiomassPerSecondFromBody.ToString("0.00") + "/s | Stress -" + gm.Body.StressReductionPerSecond.ToString("0.00") + "/s", small);
            GUI.Label(new Rect(r.x + 12, r.y + 80, r.width - 24, 20), gm.Body.AttachmentUnlocked ? "Adesina activa: puedes adherir hijas." : gm.Body.AdhesionRequirementText(), small);

            float y = r.y + 112f;
            for (int i = 0; i < gm.Body.MaxSlots; i++)
            {
                V5BodySlotDefinition def = gm.Body.GetSlotDefinition(i);
                V5CellEntity occupant = gm.Body.GetSlotOccupant(i);
                string status = occupant != null ? occupant.FunctionalCasteLabel + " - " + occupant.PhenotypeLabel + " [" + occupant.PhenotypeRecipeCode + "]" : "vacio";
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 22), "#" + i + " " + def.ring + " / " + def.preferredRole + " : " + status, small);
                y += 24f;
            }

            y += 8f;
            GUI.enabled = gm.Body.AttachmentUnlocked;
            if (GUI.Button(new Rect(r.x + 12, y, 128, 34), "Adherir sel.", button))
            {
                if (gm.Selection != null) gm.Selection.AttachSelectedToBody();
            }
            GUI.enabled = true;
            if (GUI.Button(new Rect(r.x + 146, y, 128, 34), "Desprender", button))
            {
                if (gm.Selection != null) gm.Selection.DetachSelectedFromBody();
            }
            if (GUI.Button(new Rect(r.x + 280, y, 128, 34), "Cerrar", button)) ShowPanel = false;

            y += 42f;
            bool huskCd = gm.Body.HuskDropOnCooldown;
            GUI.enabled = !huskCd;
            string huskLabel = huskCd ? "Husk Drop (CD)" : "Husk Drop\n+12 stress, CD 30s";
            if (GUI.Button(new Rect(r.x + 12, y, 200, 38), huskLabel, button)) gm.Body.TryHuskDrop();
            GUI.enabled = true;

            if (gm.Body.ChampionActive)
            {
                GUI.Label(new Rect(r.x + 220, y, r.width - 232, 38), "CAMPEON " + gm.Body.ChampionTimeRemaining.ToString("0") + "s restantes", small);
            }
            else if (huskCd)
            {
                GUI.Label(new Rect(r.x + 220, y, r.width - 232, 38), "Champion bloqueado (Husk Drop reciente)", small);
            }

            y += 46f;
            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 54), "Hotkeys: B panel | A adherir | X desprender. Champion: solo si cuerpo tuvo 6+ slots por 45s y no hubo Husk Drop.", small);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label); title.fontSize = 16; title.fontStyle = FontStyle.Bold; title.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            small = new GUIStyle(GUI.skin.label); small.fontSize = 12; small.normal.textColor = new Color(0.92f, 1f, 1f, 1f); small.wordWrap = true;
            button = new GUIStyle(GUI.skin.button); button.fontSize = 12; button.wordWrap = true;
        }
    }
}
