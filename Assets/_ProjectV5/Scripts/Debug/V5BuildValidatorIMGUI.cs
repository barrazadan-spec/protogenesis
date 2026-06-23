using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5BuildValidatorIMGUI : MonoBehaviour
    {
        public bool Show;
        public string LastValidation = "";
        private readonly List<string> issues = new List<string>();
        private GUIStyle box;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.R) && Input.GetKey(KeyCode.LeftShift))
            {
                Show = !Show;
                Validate();
            }
        }

        public void Validate()
        {
            issues.Clear();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) { issues.Add("No existe V5GameManager."); return; }
            if (gm.Environment == null) issues.Add("EnvironmentGrid no asignado.");
            if (gm.Resources == null) issues.Add("ResourceSystem no asignado.");
            if (gm.CellFactory == null) issues.Add("CellFactory no asignado.");
            if (gm.MotherCell == null) issues.Add("No existe célula madre.");
            if (Camera.main == null) issues.Add("No existe MainCamera.");
            if (gm.Environment != null && gm.Environment.nutrients == null) issues.Add("EnvironmentGrid no inicializado.");
            if (gm.MotherCell != null && gm.MotherCell.Stats.maxHp <= 0f) issues.Add("Madre con HP máximo inválido.");
            if (issues.Count == 0) issues.Add("OK: runtime V5 consistente.");
            LastValidation = string.Join("\n", issues.ToArray());
            if (gm.Hud != null) gm.Hud.Toast("Validación V5: " + (issues.Count == 1 && issues[0].StartsWith("OK") ? "OK" : issues.Count + " issues"));
        }

        private void OnGUI()
        {
            if (!Show) return;
            if (box == null) { box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.wordWrap = true; }
            GUI.Box(new Rect(460, Screen.height - 212, 460, 104), "VALIDADOR V5\n" + LastValidation, box);
        }
    }
}
