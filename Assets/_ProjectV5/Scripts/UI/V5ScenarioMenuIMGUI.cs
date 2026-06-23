using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ScenarioMenuIMGUI : MonoBehaviour
    {
        public bool Visible;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Start()
        {
            Visible = false;
            V5PanelRouter.Register("Escenarios", () => Visible, v => Visible = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10)) { if (!Visible) V5PanelRouter.CloseOthers("Escenarios"); Visible = !Visible; }
        }

        private void OnGUI()
        {
            if (!Visible) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            Rect r = new Rect(Screen.width * 0.5f - 260f, 84f, 520f, 370f);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 14, r.y + 12, r.width - 28, 26), "SELECTOR DE ESCENARIO", title);
            GUI.Label(new Rect(r.x + 14, r.y + 42, r.width - 28, 42), "F10 abre/cierra. Cambiar escenario reinicia la run actual, respeta la arquitectura V5 y regenera mundo, células y recursos.");
            float y = r.y + 94f;
            ScenarioButton(gm, V5ScenarioId.FirstDrop, r.x + 14, y); y += 48f;
            ScenarioButton(gm, V5ScenarioId.OxygenWar, r.x + 14, y); y += 48f;
            ScenarioButton(gm, V5ScenarioId.AcidFrontier, r.x + 14, y); y += 48f;
            ScenarioButton(gm, V5ScenarioId.PredatorBloom, r.x + 14, y); y += 48f;
            ScenarioButton(gm, V5ScenarioId.Freeplay, r.x + 14, y); y += 58f;
            GUI.Label(new Rect(r.x + 14, y, r.width - 28, 40), "Actual: " + gm.ScenarioId + " | Consejo: guarda con F5 antes de cambiar si quieres conservar la run.");
        }

        private void ScenarioButton(V5GameManager gm, V5ScenarioId id, float x, float y)
        {
            V5ScenarioDefinition def = V5ScenarioLibrary.Get(id);
            string label = def.displayName + " - " + def.primaryObjective;
            if (GUI.Button(new Rect(x, y, 492, 38), label, button))
            {
                V5RunResetSystem reset = FindFirstObjectByType<V5RunResetSystem>();
                if (reset != null) reset.RestartScenario(id);
                Visible = false;
            }
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
            button.alignment = TextAnchor.MiddleLeft;
            button.wordWrap = true;
        }
    }
}
