using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Production-style end screen for playtests. Watches victory/defeat and gives a readable scorecard.
    /// Toggle with F7 after or during a run.
    /// </summary>
    public class V5RunSummarySystem : MonoBehaviour, IV5RunResettable
    {
        public bool ShowPanel { get; private set; }
        public string LastSummary { get; private set; }
        public string Grade { get; private set; }
        public float FinalScore { get; private set; }

        private V5GamePhase lastPhase;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle bigStyle;

        public void ResetForNewRun()
        {
            ShowPanel = false;
            LastSummary = "";
            Grade = "";
            FinalScore = 0f;
            V5GameManager gm = V5GameManager.Instance;
            lastPhase = gm != null ? gm.Phase : V5GamePhase.Primordial;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F7))
            {
                BuildSummary(false);
                ShowPanel = !ShowPanel;
            }

            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            if (gm.Phase != lastPhase)
            {
                if (gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat)
                {
                    BuildSummary(true);
                    ShowPanel = true;
                }
                lastPhase = gm.Phase;
            }
        }

        public void BuildSummary(bool final)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null)
            {
                LastSummary = "Sin GameManager.";
                return;
            }

            float colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float toxin = gm.Environment != null ? gm.Environment.AverageToxins() : 0f;
            float acid = gm.Environment != null ? gm.Environment.AverageAcidity() : 0.5f;
            int cells = gm.PlayerCellCount();
            int enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            bool usingAdaptations = gm.Adaptations != null;
            int genomeCount = usingAdaptations ? gm.Adaptations.Installed.Count : (gm.Genes != null ? gm.Genes.UnlockedCount : 0);
            string genomeLabel = usingAdaptations ? "Adaptaciones" : "Genes";
            bool apex = gm.Apex != null && gm.Apex.ApexSpawned;
            float minutes = gm.ElapsedSeconds / 60f;

            int bodySlots = gm.Body != null ? gm.Body.OccupiedSlots : 0;
            int bodyMax = gm.Body != null ? gm.Body.MaxSlots : 6;
            string bodyState = gm.Body != null ? gm.Body.BodyStateLabel() : "sin cuerpo";
            string continuityText = gm.Continuity != null ? gm.Continuity.LastBand + " " + gm.Continuity.LastScore.ToString("0") : "N/A";
            string squadText = gm.Squads != null ? gm.Squads.Summary : "N/A";
            string affinityText = gm.MotherCell != null ? gm.MotherCell.EvolutionPath.ToString() : "N/A";
            if (gm.AffinityLog != null && gm.MotherCell != null)
                affinityText += " — " + gm.AffinityLog.RouteSummary(gm.MotherCell.EvolutionPath, 3);

            FinalScore = 0f;
            FinalScore += colonization * 360f;
            FinalScore += Mathf.Clamp01(cells / 12f) * 120f;
            FinalScore += genomeCount * 55f;
            FinalScore += apex ? 120f : 0f;
            FinalScore += Mathf.Clamp01(1f - toxin) * 65f;
            FinalScore += Mathf.Clamp01(1f - Mathf.Abs(acid - 0.5f) * 2f) * 65f;
            FinalScore += gm.Phase == V5GamePhase.Victory ? 180f : 0f;
            FinalScore -= enemies * 7f;
            FinalScore += bodySlots * 8f;
            if (final && gm.Phase == V5GamePhase.Defeat) FinalScore *= 0.65f;

            if (FinalScore >= 820f) Grade = "S — ecosistema dominante";
            else if (FinalScore >= 660f) Grade = "A — colonia avanzada";
            else if (FinalScore >= 500f) Grade = "B — linaje viable";
            else if (FinalScore >= 340f) Grade = "C — protocultura estable";
            else Grade = "D — supervivencia fragil";

            string telemetry = gm.Telemetry != null ? gm.Telemetry.Summary : "sin telemetria";
            string adaptationTelemetry = gm.Telemetry != null ? gm.Telemetry.AdaptationSummary : "sin telemetria de adaptaciones";
            string routeClimax = gm.RouteClimax != null ? gm.RouteClimax.Summary : "Climax MVP: sin sistema.";
            string routeChapter = gm.RouteChapters != null ? gm.RouteChapters.Summary : "Capitulos MVP: sin sistema.";
            string routeBranch = gm.RouteBranches != null ? gm.RouteBranches.Summary : "Rama MVP: sin sistema.";
            string routeCounter = gm.RouteCounters != null ? gm.RouteCounters.LastCounterSummary : "Contra-presion MVP: sin sistema.";
            LastSummary =
                "Escenario: " + gm.ScenarioId + "\n" +
                "Tiempo: " + minutes.ToString("0.0") + " min | Fase: " + gm.Phase + "\n" +
                "Celulas: " + cells + " | Enemigos: " + enemies + " | " + genomeLabel + ": " + genomeCount + " | Apex: " + (apex ? "si" : "no") + "\n" +
                "Colonizacion: " + (colonization * 100f).ToString("0.0") + "% | Toxinas: " + (toxin * 100f).ToString("0") + "% | Acidez: " + acid.ToString("0.00") + "\n" +
                "Cuerpo: " + bodySlots + "/" + bodyMax + " slots | " + bodyState + "\n" +
                routeChapter + "\n" +
                routeBranch + "\n" +
                routeClimax + "\n" +
                routeCounter + "\n" +
                "Continuidad: " + continuityText + "\n" +
                "Afinidad: " + affinityText + "\n" +
                "Squad: " + squadText + "\n" +
                "Puntaje: " + FinalScore.ToString("0") + " | Grado: " + Grade + "\n" +
                "Telemetria: " + telemetry + "\n" +
                "Genoma: " + adaptationTelemetry;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width * 0.5f - 280f, 90f, 560f, 480f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 16f, r.y + 12f, r.width - 32f, r.height - 24f));
            GUILayout.Label("RUN SUMMARY 1.0", bigStyle);
            GUILayout.Label(Grade, titleStyle);
            GUILayout.Space(8f);
            GUILayout.Label(string.IsNullOrEmpty(LastSummary) ? "Presiona F7 para calcular resumen de la run." : LastSummary, bodyStyle);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Exportar reporte JSON"))
            {
                V5PlaytestReportSystem report = FindFirstObjectByType<V5PlaytestReportSystem>();
                if (report != null) report.ExportReport("manual_summary");
            }
            if (GUILayout.Button("Reiniciar escenario"))
            {
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.RunReset != null) gm.RunReset.RestartScenario(gm.ScenarioId);
                ShowPanel = false;
            }
            if (GUILayout.Button("Cerrar F7")) ShowPanel = false;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            bigStyle = new GUIStyle(GUI.skin.label); bigStyle.fontSize = 20; bigStyle.fontStyle = FontStyle.Bold; bigStyle.normal.textColor = new Color(0.92f, 1f, 1f, 1f);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontSize = 15; titleStyle.fontStyle = FontStyle.Bold; titleStyle.normal.textColor = new Color(1f, 0.9f, 0.45f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
        }
    }
}
