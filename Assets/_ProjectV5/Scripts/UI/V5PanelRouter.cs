using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Protogenesis.V5
{
    // Central secondary-panel manager.
    // Each secondary system calls Register() in Start/Awake and CloseOthers() before opening.
    // CloseAll() uses reflection as a fallback for unregistered panels.
    public class V5PanelRouter : MonoBehaviour
    {
        public static V5PanelRouter Instance { get; private set; }

        private struct Entry
        {
            public string Label;
            public Func<bool> Getter;
            public Action<bool> Setter;
        }

        private readonly List<Entry> entries = new List<Entry>();
        private bool showMenu;
        private GUIStyle btnOff;
        private GUIStyle btnOn;
        private GUIStyle menuBox;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void AutoInstall()
        {
            if (Instance != null) return;
            GameObject go = new GameObject("V5_PanelRouter");
            DontDestroyOnLoad(go);
            Instance = go.AddComponent<V5PanelRouter>();
        }

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // Call from each secondary system's Start() or Awake().
        public static void Register(string label, Func<bool> getter, Action<bool> setter)
        {
            if (Instance == null) return;
            if (Instance.entries.Exists(e => e.Label == label)) return;
            Instance.entries.Add(new Entry { Label = label, Getter = getter, Setter = setter });
        }

        // Call this BEFORE setting ShowPanel = true in any secondary system.
        public static void CloseOthers(string exceptLabel)
        {
            if (Instance == null) return;
            foreach (var e in Instance.entries)
                if (e.Label != exceptLabel) e.Setter(false);
        }

        // Closes every registered panel. Also sweeps unregistered panels via reflection.
        public static void CloseAll()
        {
            if (Instance != null)
                foreach (var e in Instance.entries) e.Setter(false);

            // Fallback: close any MonoBehaviour with a common panel visibility bool field.
            MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            foreach (MonoBehaviour mb in all)
            {
                if (mb == null) continue;
                FieldInfo f = mb.GetType().GetField("ShowPanel", flags)
                           ?? mb.GetType().GetField("showPanel", flags)
                           ?? mb.GetType().GetField("Visible", flags)
                           ?? mb.GetType().GetField("ShowMenu", flags)
                           ?? mb.GetType().GetField("showMenu", flags)
                           ?? mb.GetType().GetField("show", flags);
                if (f != null && f.FieldType == typeof(bool)) f.SetValue(mb, false);
            }
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape)) showMenu = false;
        }

        private void OnGUI()
        {
            EnsureStyles();

            // "Paneles ▼" toggle button — top-right corner, below top HUD.
            float bw = 88f, bh = 22f;
            Rect toggleRect = new Rect(Screen.width - bw - 8f, 212f, bw, bh);
            if (GUI.Button(toggleRect, showMenu ? "Paneles ▲" : "Paneles ▼", btnOff))
                showMenu = !showMenu;

            if (!showMenu || entries.Count == 0) return;

            int cols = 2;
            float entryW = 110f, entryH = 22f, gap = 2f;
            float menuW = cols * entryW + (cols - 1) * gap + 8f;
            int rows = Mathf.CeilToInt((float)entries.Count / cols);
            float menuH = rows * (entryH + gap) + 8f;
            Rect menu = new Rect(Screen.width - menuW - 8f, toggleRect.yMax + gap, menuW, menuH);
            GUI.Box(menu, GUIContent.none, menuBox);

            float x0 = menu.x + 4f, y0 = menu.y + 4f;
            for (int i = 0; i < entries.Count; i++)
            {
                int col = i % cols;
                int row = i / cols;
                float ex = x0 + col * (entryW + gap);
                float ey = y0 + row * (entryH + gap);
                bool open = entries[i].Getter();
                if (GUI.Button(new Rect(ex, ey, entryW, entryH), entries[i].Label, open ? btnOn : btnOff))
                {
                    bool next = !open;
                    if (next) CloseOthers(entries[i].Label);
                    entries[i].Setter(next);
                }
            }
        }

        private void EnsureStyles()
        {
            if (btnOff != null) return;
            btnOff = new GUIStyle(GUI.skin.button) { fontSize = 10 };
            btnOn = new GUIStyle(GUI.skin.button) { fontSize = 10, fontStyle = FontStyle.Bold };
            btnOn.normal.textColor = new Color(0.2f, 1f, 0.5f);
            menuBox = new GUIStyle(GUI.skin.box);
        }
    }
}
