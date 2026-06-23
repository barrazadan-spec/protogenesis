using System.Text;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Lightweight release checklist for prototype milestones. Toggle with F12.
    /// It does not try to replace Unity build validation; it checks runtime playability.
    /// </summary>
    public class V5ReleaseReadinessSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public int Score { get; private set; }
        public string LastReport { get; private set; }
        public string ShortStatus { get; private set; }

        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private float nextScan;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F12)) ShowPanel = !ShowPanel;
            if (Time.unscaledTime >= nextScan)
            {
                nextScan = Time.unscaledTime + 2f;
                Scan();
            }
        }

        public void Scan()
        {
            V5GameManager gm = V5GameManager.Instance;
            int score = 0;
            StringBuilder sb = new StringBuilder(1024);
            sb.AppendLine("PROTOGENESIS V5 1.0 — RELEASE READINESS");

            Check(sb, "GameManager activo", gm != null, ref score, 8);
            Check(sb, "Cámara principal", Camera.main != null, ref score, 6);
            Check(sb, "EnvironmentGrid inicializado", gm != null && gm.Environment != null && gm.Environment.nutrients != null, ref score, 10);
            Check(sb, "Célula madre viva", gm != null && gm.MotherCell != null && gm.MotherCell.Stats.currentHp > 0f, ref score, 12);
            Check(sb, "ResourceSystem con nodos", gm != null && gm.Resources != null && gm.Resources.Nodes != null && gm.Resources.Nodes.Count > 0, ref score, 8);
            Check(sb, "Selection + Combat + Genes", gm != null && gm.Selection != null && gm.Combat != null && gm.Genes != null, ref score, 10);
            Check(sb, "Save/Load presente", gm != null && gm.Save != null, ref score, 6);
            Check(sb, "HUD presente", gm != null && gm.Hud != null, ref score, 6);
            Check(sb, "Tutorial 1.0 presente", FindFirstObjectByType<V5TutorialFlowSystem>() != null, ref score, 6);
            Check(sb, "Run Summary presente", FindFirstObjectByType<V5RunSummarySystem>() != null, ref score, 6);
            Check(sb, "Performance guard presente", FindFirstObjectByType<V5OptimizationGuardSystem>() != null, ref score, 5);
            Check(sb, "Sin exceso crítico de entidades", gm != null && gm.PlayerCellCount() <= V5Balance.HardControllableEntityCap && (gm.NonPlayerCells == null || gm.NonPlayerCells.Count <= 80), ref score, 7);
            Check(sb, "Condiciones de victoria activas", FindFirstObjectByType<V5EndgameDirector>() != null, ref score, 6);
            Check(sb, "Escenario activo", gm != null && gm.Scenario != null && !string.IsNullOrEmpty(gm.Scenario.ObjectiveText), ref score, 4);

            Score = Mathf.Clamp(score, 0, 100);
            ShortStatus = Score >= 85 ? "LISTO PARA PLAYTEST" : Score >= 65 ? "JUGABLE CON RIESGOS" : "NECESITA CORRECCIÓN";
            sb.AppendLine("Score: " + Score + "/100 — " + ShortStatus);
            LastReport = sb.ToString();
        }

        private void Check(StringBuilder sb, string label, bool ok, ref int score, int points)
        {
            if (ok) score += points;
            sb.AppendLine((ok ? "✓ " : "✗ ") + label + " (" + (ok ? points : 0) + "/" + points + ")");
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 500f, 70f, 490f, 500f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 12f, r.width - 28f, r.height - 24f));
            GUILayout.Label("RELEASE READINESS — F12", titleStyle);
            GUILayout.Label(ShortStatus + " | Score " + Score + "/100", bodyStyle);
            GUILayout.TextArea(LastReport ?? "Escaneando...", bodyStyle, GUILayout.ExpandHeight(true));
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Re-escanear")) Scan();
            if (GUILayout.Button("Exportar reporte"))
            {
                V5PlaytestReportSystem report = FindFirstObjectByType<V5PlaytestReportSystem>();
                if (report != null) report.ExportReport("readiness_panel");
            }
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.fontSize = 16; titleStyle.normal.textColor = new Color(0.9f, 1f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
        }
    }
}
