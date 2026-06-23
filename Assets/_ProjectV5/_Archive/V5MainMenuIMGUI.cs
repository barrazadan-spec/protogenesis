using UnityEngine;

namespace Protogenesis.V5
{
    public class V5MainMenuIMGUI : MonoBehaviour
    {
        public bool ShowMenu;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Start()
        {
            ShowMenu = false;
            V5PanelRouter.Register("Menu", () => ShowMenu, v => ShowMenu = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (!ShowMenu) V5PanelRouter.CloseOthers("Menu");
                ShowMenu = !ShowMenu;
            }
        }

        private void OnGUI()
        {
            if (!ShowMenu) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            Rect r = new Rect(Screen.width / 2 - 260, Screen.height / 2 - 220, 520, 440);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 20, r.y + 18, r.width - 40, 34), "PROTOGENESIS: PRIMORDIA", title);
            GUI.Label(new Rect(r.x + 20, r.y + 56, r.width - 40, 52), "V5 Prototype 0.8 — Cellular World Builder + Biological RTS\nEsta pantalla es runtime: puedes seguir jugando, cambiar escenario o resetear la run.");

            float y = r.y + 126;
            if (GUI.Button(new Rect(r.x + 30, y, 210, 42), "Continuar", button)) ShowMenu = false;
            if (GUI.Button(new Rect(r.x + 280, y, 210, 42), "Reset run", button))
            {
                if (gm != null && gm.RunReset != null) gm.RunReset.RestartScenario(gm.ScenarioId);
                ShowMenu = false;
            }
            y += 56;
            GUI.Label(new Rect(r.x + 30, y, 460, 24), "Escenarios rápidos:");
            y += 28;
            ScenarioButton(gm, V5ScenarioId.FirstDrop, r.x + 30, y, 145);
            ScenarioButton(gm, V5ScenarioId.OxygenWar, r.x + 188, y, 145);
            ScenarioButton(gm, V5ScenarioId.AcidFrontier, r.x + 346, y, 145);
            y += 48;
            ScenarioButton(gm, V5ScenarioId.PredatorBloom, r.x + 30, y, 224);
            ScenarioButton(gm, V5ScenarioId.Freeplay, r.x + 266, y, 224);
            y += 62;
            if (gm != null)
            {
                string state = "Fase: " + gm.Phase + " | Escenario: " + gm.ScenarioId;
                if (gm.MotherCell != null) state += "\nMadre: " + gm.MotherCell.EvolutionPath + " / " + gm.MotherCell.Domain + " | células " + gm.PlayerCellCount();
                if (gm.Environment != null) state += "\nColonización: " + (gm.Environment.AverageColonization() * 100f).ToString("0") + "% | toxinas " + (gm.Environment.AverageToxins() * 100f).ToString("0") + "%";
                GUI.Label(new Rect(r.x + 30, y, 460, 72), state);
            }
            GUI.Label(new Rect(r.x + 30, r.yMax - 46, 460, 24), "ESC abre/cierra menú | V plan evolutivo | K performance | Shift+R validador");
        }

        private void ScenarioButton(V5GameManager gm, V5ScenarioId id, float x, float y, float w)
        {
            if (GUI.Button(new Rect(x, y, w, 36), id.ToString(), button))
            {
                if (gm != null && gm.RunReset != null) gm.RunReset.RestartScenario(id);
                ShowMenu = false;
            }
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.fontSize = 13;
            title = new GUIStyle(GUI.skin.label); title.fontSize = 24; title.fontStyle = FontStyle.Bold; title.alignment = TextAnchor.MiddleCenter; title.normal.textColor = new Color(0.85f, 1f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 13; button.wordWrap = true;
        }
    }
}
