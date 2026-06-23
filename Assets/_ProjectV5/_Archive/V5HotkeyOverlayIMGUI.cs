using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// One-screen control reference for playtests. Toggle with backslash.
    /// </summary>
    public class V5HotkeyOverlayIMGUI : MonoBehaviour
    {
        public bool ShowPanel;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backslash)) ShowPanel = !ShowPanel;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(18f, 212f, 540f, 560f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 12f, r.width - 28f, r.height - 24f));
            GUILayout.Label("CONTROLES — PROTOGENESIS V5", titleStyle);
            GUILayout.Label("\\ muestra/oculta esta ayuda", bodyStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Movimiento: Click/drag seleccionar | Shift suma | Click derecho mover | Flechas o mouse medio cámara | Scroll zoom", bodyStyle);
            GUILayout.Label("RTS: D dividir | 1 seguir | 2 farmear | 3 defender | 4 explorar | 5 colonizar | 6 atacar | Shift+S reagrupar | Ctrl+1-0 guardar grupo | Alt+1-0 seleccionar grupo", bodyStyle);
            GUILayout.Label("Paneles: E interior | G genes | Tab overlay ambiental | N minimapa | M misión | H advisor | C codex | F seguir madre", bodyStyle);
            GUILayout.Label("Acciones: Q pulso metabólico | W pulso ecológico | R reparar | V mutación draft | P invocar Apex | Shift+0 ejecutar hábitat", bodyStyle);
            GUILayout.Label("Panel rápido (hotkey): B biomas | 7 auto | 9 dashboard | K habilidades | [ crisis | / ecología | , contratos | Z amenazas", bodyStyle);
            GUILayout.Label("Paneles extras: botón «Paneles▼» arriba a la derecha del HUD", bodyStyle);
            GUILayout.Label("Sistema: Espacio pausa | = / - velocidad | F5 guardar | F9 cargar | F10 escenario | F11 tutorial | F12 readiness | F7 resumen", bodyStyle);
            GUILayout.Label("Debug: F1/F2/F3/F4 Run Doctor | T/Y/U/I/O cheats | Ctrl+O optimización | Backspace demo prep | Shift+R validar build", bodyStyle);
            GUILayout.Space(12f);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null)
            {
                GUILayout.Label("Estado actual: " + gm.ScenarioId + " | " + gm.Phase + " | células " + gm.PlayerCellCount() + " | enemigos " + (gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0), bodyStyle);
                V5ColonyAutomationSystem auto = FindFirstObjectByType<V5ColonyAutomationSystem>();
                if (auto != null) GUILayout.Label("1.7 Auto: " + (auto.AutomationEnabled ? "ON" : "OFF") + " / " + auto.Doctrine + " — " + auto.LastAction, bodyStyle);
                V5ColonyHealthDashboardIMGUI dash = FindFirstObjectByType<V5ColonyHealthDashboardIMGUI>();
                if (dash != null) GUILayout.Label("1.7 Salud colonial: " + (dash.OverallScore * 100f).ToString("0") + "% — " + dash.Diagnosis, bodyStyle);
                V5OptimizationGuardSystem opt = FindFirstObjectByType<V5OptimizationGuardSystem>();
                if (opt != null) GUILayout.Label("Rendimiento: " + opt.Status + " | AutoSoftCull " + (opt.AutoSoftCull ? "ON" : "OFF"), bodyStyle);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.fontSize = 18; titleStyle.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
        }
    }
}
