using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5FeedbackSystem : MonoBehaviour
    {
        private class FloatingLabel
        {
            public string text;
            public Vector2 world;
            public Color color;
            public float until;
            public float lifetime;
        }

        public bool ShowFloatingLabels = true;
        public string LastFeedback = "";
        private readonly List<FloatingLabel> labels = new List<FloatingLabel>(32);
        private int lastCellCount;
        private int lastGeneCount;
        private int lastStructureCount;
        private float nextLowResourceWarning;
        private GUIStyle labelStyle;

        private void Start()
        {
            Snapshot();
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;

            int cells = gm.PlayerCellCount();
            if (lastCellCount > 0 && cells > lastCellCount)
            {
                Push("Nueva división celular", gm.MotherCell.transform.position, new Color(0.75f, 1f, 0.85f, 1f));
                Ping("division");
            }
            lastCellCount = cells;

            bool adaptationMode = gm.Adaptations != null;
            int genomeProgress = gm.Genes != null ? gm.Genes.UnlockedCount : 0;
            if (adaptationMode) genomeProgress = Mathf.Max(genomeProgress, gm.Adaptations.Installed.Count);
            bool adaptationChanged = adaptationMode && genomeProgress > lastGeneCount;
            if (genomeProgress > lastGeneCount)
            {
                Push(adaptationMode ? "Adaptacion activada" : "Gen activado", (Vector2)gm.MotherCell.transform.position + Vector2.up * 1.2f, new Color(0.95f, 0.85f, 1f, 1f));
                Ping("gene");
            }
            lastGeneCount = genomeProgress;

            int structures = gm.MotherCell.Structures.Count;
            if (structures > lastStructureCount)
            {
                Push(adaptationMode ? (adaptationChanged ? "Fenotipo actualizado" : "Rasgo corporal actualizado") : "Estructura instalada", (Vector2)gm.MotherCell.transform.position + Vector2.right * 1.2f, new Color(0.85f, 0.95f, 1f, 1f));
                Ping("structure");
            }
            lastStructureCount = structures;

            if (Time.unscaledTime > nextLowResourceWarning && gm.MotherCell.Resources.atp < 18f)
            {
                nextLowResourceWarning = Time.unscaledTime + 10f;
                Push("ATP bajo", gm.MotherCell.transform.position, new Color(1f, 0.72f, 0.28f, 1f));
                LastFeedback = "ATP bajo: farmea o abre Genoma (G) para una adaptacion economica.";
                if (gm.Hud != null) gm.Hud.Toast(LastFeedback);
                Ping("warning");
            }

            for (int i = labels.Count - 1; i >= 0; i--)
            {
                if (Time.unscaledTime > labels[i].until) labels.RemoveAt(i);
            }
        }

        public void Push(string text, Vector2 world, Color color)
        {
            LastFeedback = text;
            if (V5GameManager.Instance != null && V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast(text);
            AddFloating(text, world, color, 2.2f);
        }

        public void PushFloating(string text, Vector2 world, Color color)
        {
            LastFeedback = text;
            AddFloating(text, world, color, 1.05f);
        }

        private void AddFloating(string text, Vector2 world, Color color, float lifetime)
        {
            FloatingLabel f = new FloatingLabel();
            f.text = text;
            f.world = world;
            f.color = color;
            f.lifetime = Mathf.Max(0.1f, lifetime);
            f.until = Time.unscaledTime + f.lifetime;
            labels.Add(f);
            if (labels.Count > 30) labels.RemoveAt(0);
        }

        public void Ping(string kind)
        {
            V5AudioFeedback audio = FindFirstObjectByType<V5AudioFeedback>();
            if (audio != null) audio.PlayCue(kind);
        }

        private void Snapshot()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            lastCellCount = gm.PlayerCellCount();
            lastGeneCount = gm.Genes != null ? gm.Genes.UnlockedCount : 0;
            if (gm.Adaptations != null) lastGeneCount = Mathf.Max(lastGeneCount, gm.Adaptations.Installed.Count);
            lastStructureCount = gm.MotherCell != null ? gm.MotherCell.Structures.Count : 0;
        }

        private void OnGUI()
        {
            if (!ShowFloatingLabels || Camera.main == null) return;
            if (labelStyle == null)
            {
                labelStyle = new GUIStyle(GUI.skin.label);
                labelStyle.fontStyle = FontStyle.Bold;
                labelStyle.alignment = TextAnchor.MiddleCenter;
            }

            for (int i = 0; i < labels.Count; i++)
            {
                FloatingLabel f = labels[i];
                Vector3 sp = Camera.main.WorldToScreenPoint(f.world + Vector2.up * ((f.until - Time.unscaledTime) * 0.25f));
                if (sp.z < 0f) continue;
                float alpha = Mathf.Clamp01((f.until - Time.unscaledTime) / Mathf.Max(0.1f, f.lifetime));
                labelStyle.normal.textColor = new Color(f.color.r, f.color.g, f.color.b, alpha);
                GUI.Label(new Rect(sp.x - 120f, Screen.height - sp.y - 16f, 240f, 32f), f.text, labelStyle);
            }
        }
    }
}
