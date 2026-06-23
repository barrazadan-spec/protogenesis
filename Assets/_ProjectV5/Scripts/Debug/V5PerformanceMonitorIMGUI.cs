using UnityEngine;

namespace Protogenesis.V5
{
    public class V5PerformanceMonitorIMGUI : MonoBehaviour
    {
        public bool Show;
        public float Fps { get; private set; }
        public string Summary { get; private set; }
        private float accum;
        private int frames;
        private float timeLeft = 0.5f;
        private GUIStyle box;

        private void Update()
        {
            // K removed — AbilitySystem uses K; open via Paneles menu
            timeLeft -= Time.unscaledDeltaTime;
            accum += Time.unscaledDeltaTime;
            frames++;
            if (timeLeft <= 0f)
            {
                Fps = frames / Mathf.Max(0.0001f, accum);
                frames = 0;
                accum = 0f;
                timeLeft = 0.5f;
                V5GameManager gm = V5GameManager.Instance;
                int p = gm != null ? gm.PlayerCells.Count : 0;
                int e = gm != null ? gm.NonPlayerCells.Count : 0;
                int r = gm != null && gm.Resources != null ? gm.Resources.Nodes.Count : 0;
                Summary = "FPS " + Fps.ToString("0") + " | player " + p + " | enemy " + e + " | nodes " + r + " | mem " + (System.GC.GetTotalMemory(false) / (1024 * 1024)).ToString("0") + "MB";
            }
        }

        private void OnGUI()
        {
            if (!Show) return;
            if (box == null) { box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; }
            GUI.Box(new Rect(10, Screen.height - 152, 430, 48), Summary, box);
        }
    }
}
