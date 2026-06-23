using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Keeps morphology renderers attached to all V5 cells and offers a runtime toggle for performance checks.
    /// </summary>
    public class V5MorphologyVisualSystem : MonoBehaviour
    {
        public bool MorphologyEnabled = true;
        public int RenderersAttached { get; private set; }
        public string LastMessage = "morfología procedural activa";

        private float nextScan;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F10) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
            {
                MorphologyEnabled = !MorphologyEnabled;
                LastMessage = MorphologyEnabled ? "morfología ON" : "morfología OFF";
                ToggleExisting();
                if (V5GameManager.Instance != null && V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast(LastMessage);
            }

            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 0.75f;
            AttachMissing();
        }

        private void AttachMissing()
        {
            RenderersAttached = 0;
            V5CellEntity[] cells = FindObjectsByType<V5CellEntity>(FindObjectsSortMode.None);
            for (int i = 0; i < cells.Length; i++)
            {
                if (cells[i] == null) continue;
                V5CellMorphologyRenderer r = cells[i].GetComponent<V5CellMorphologyRenderer>();
                if (r == null) r = cells[i].gameObject.AddComponent<V5CellMorphologyRenderer>();
                r.enabled = MorphologyEnabled;
                RenderersAttached++;
            }
        }

        private void ToggleExisting()
        {
            V5CellMorphologyRenderer[] renderers = FindObjectsByType<V5CellMorphologyRenderer>(FindObjectsSortMode.None);
            for (int i = 0; i < renderers.Length; i++) renderers[i].enabled = MorphologyEnabled;
        }
    }
}
