using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ControlGroupSystem : MonoBehaviour
    {
        private readonly List<V5CellEntity>[] groups = new List<V5CellEntity>[10];
        public string LastMessage = "Control groups: Shift+1..0 asigna, 1..0 selecciona.";

        private void Awake()
        {
            for (int i = 0; i < groups.Length; i++) groups[i] = new List<V5CellEntity>(16);
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Selection == null) return;
            for (int i = 0; i <= 9; i++)
            {
                KeyCode key = i == 0 ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha1 + (i - 1));
                if (!Input.GetKeyDown(key)) continue;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) AssignGroup(i, gm.Selection.Selected);
                else if (gm.CoreMode && Count(i) > 0) SelectGroup(i, gm.Selection);
            }
        }

        public void AssignGroup(int index, List<V5CellEntity> selection)
        {
            if (index < 0 || index >= groups.Length || selection == null) return;
            groups[index].Clear();
            for (int i = 0; i < selection.Count; i++)
            {
                V5CellEntity cell = selection[i];
                if (cell != null && cell.IsPlayerOwned && !groups[index].Contains(cell)) groups[index].Add(cell);
            }
            LastMessage = "Grupo " + index + " asignado: " + groups[index].Count + " células.";
            Toast(LastMessage);
        }

        public void SelectGroup(int index, V5SelectionSystem selection)
        {
            if (index < 0 || index >= groups.Length || selection == null) return;
            selection.ClearSelection();
            for (int i = groups[index].Count - 1; i >= 0; i--)
            {
                V5CellEntity cell = groups[index][i];
                if (cell == null) { groups[index].RemoveAt(i); continue; }
                selection.AddSelection(cell);
            }
            LastMessage = "Grupo " + index + " seleccionado: " + groups[index].Count + " células.";
            Toast(LastMessage);
        }

        public int Count(int index)
        {
            if (index < 0 || index >= groups.Length) return 0;
            int count = 0;
            for (int i = groups[index].Count - 1; i >= 0; i--)
            {
                if (groups[index][i] == null) groups[index].RemoveAt(i);
                else count++;
            }
            return count;
        }

        private void Toast(string message)
        {
            if (V5GameManager.Instance != null && V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast(message);
        }
    }
}
