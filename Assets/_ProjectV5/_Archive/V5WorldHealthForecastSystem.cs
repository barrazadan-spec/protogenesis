using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Lightweight strategic forecast. It samples the ecosystem over time and turns raw world values
    /// into readable warnings, endgame guidance and recommended habitat projects.
    /// Toggle with Backspace.
    /// </summary>
    public class V5WorldHealthForecastSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public string Forecast = "Esperando datos ecológicos.";
        public string Recommendation = "Explora y coloniza.";
        public float StabilityScore { get; private set; }
        public float CollapseRisk { get; private set; }
        public float ExpansionReadiness { get; private set; }
        public float LastColonizationTrend { get; private set; }
        public float LastToxinTrend { get; private set; }
        public float LastOxygenTrend { get; private set; }

        private float previousColonization;
        private float previousToxins;
        private float previousOxygen;
        private float timer;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle goodStyle;
        private GUIStyle badStyle;

        private void Start() => V5PanelRouter.Register("Pronóstico", () => ShowPanel, v => ShowPanel = v);

        private void Update()
        {
            // Backspace removed — DemoBuildPrepSystem uses it; open via Paneles menu
            timer += Time.deltaTime;
            if (timer < 2f) return;
            timer = 0f;
            Evaluate();
        }

        public void Evaluate()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null || gm.MotherCell == null) return;
            V5EnvironmentGrid env = gm.Environment;
            float colonization = env.AverageColonization();
            float toxins = env.AverageToxins();
            float oxygen = env.AverageOxygen();
            float acidity = env.AverageAcidity();
            float stress = gm.MotherCell.Stats.stress / 100f;
            float enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0f;
            float cells = Mathf.Max(1f, gm.PlayerCellCount());

            LastColonizationTrend = colonization - previousColonization;
            LastToxinTrend = toxins - previousToxins;
            LastOxygenTrend = oxygen - previousOxygen;
            previousColonization = colonization;
            previousToxins = toxins;
            previousOxygen = oxygen;

            float phRisk = Mathf.Clamp01(Mathf.Abs(acidity - 0.50f) * 1.8f);
            float toxinRisk = Mathf.Clamp01(toxins * 1.45f + Mathf.Max(0f, LastToxinTrend) * 8f);
            float pressureRisk = Mathf.Clamp01(enemies / Mathf.Max(5f, cells * 1.5f));
            CollapseRisk = Mathf.Clamp01(stress * 0.34f + toxinRisk * 0.27f + phRisk * 0.18f + pressureRisk * 0.21f);
            StabilityScore = Mathf.Clamp01(1f - CollapseRisk + colonization * 0.35f + oxygen * 0.10f);
            ExpansionReadiness = Mathf.Clamp01((1f - CollapseRisk) * 0.55f + colonization * 0.35f + gm.MotherCell.Resources.biomass / 220f);

            if (CollapseRisk > 0.72f) Forecast = "RIESGO DE COLAPSO: reduce stress, limpia toxinas o cambia a doctrina Recovery.";
            else if (ExpansionReadiness > 0.72f) Forecast = "VENTANA DE EXPANSIÓN: divide, coloniza y reclama microzonas.";
            else if (colonization > 0.30f && toxins < 0.25f) Forecast = "BIOSFERA ESTABLE: busca apex, victoria ecológica o contratos avanzados.";
            else Forecast = "ECOSISTEMA EN DESARROLLO: consolida economía y reduce amenazas locales.";

            Recommendation = BuildRecommendation(gm, env, toxins, acidity, oxygen, colonization);
        }

        private string BuildRecommendation(V5GameManager gm, V5EnvironmentGrid env, float toxins, float acidity, float oxygen, float colonization)
        {
            V5CellEntity m = gm.MotherCell;
            if (m == null) return "Sin madre.";
            if (m.Resources.atp < 25f || m.Resources.biomass < 20f) return "Prioridad: farmers + Bloom nutritivo. Falta economía base.";
            if (toxins > 0.34f || LastToxinTrend > 0.015f) return "Prioridad: Detox Biofilm o Catalasa. Las toxinas están escalando.";
            if (Mathf.Abs(acidity - 0.50f) > 0.22f) return "Prioridad: Acid Buffer. El pH se aleja de zona segura.";
            if ((m.Metabolism == V5MetabolismType.Respiration || m.Domain == V5CellDomain.Eukaryote) && oxygen < 0.25f) return "Prioridad: Oxygen Pocket. La respiración necesita oxígeno estable.";
            if ((m.Metabolism == V5MetabolismType.Photosynthesis || m.HasPhotosynthesis) && gm.Environment.Sample(V5OverlayMode.Light, m.transform.position) < 0.45f) return "Prioridad: Light Lens o mover colonia a zona fótica.";
            if (colonization < 0.15f) return "Prioridad: colonizers + Defensive Matrix en frontera.";
            if (gm.NonPlayerCells != null && gm.NonPlayerCells.Count > gm.PlayerCellCount()) return "Prioridad: defensores/predadores. Hay demasiadas amenazas.";
            return "Prioridad: especialización, anillo 4 y apex. La base está estable.";
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(18f, Screen.height - 430f, 470f, 342f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("FORECAST ECOLÓGICO — 1.8", titleStyle);
            GUILayout.Label("Backspace abre/cierra. Lee tendencias del mapa y sugiere el próximo movimiento macro.", bodyStyle);
            GUILayout.Space(8f);
            DrawMetric("Estabilidad", StabilityScore, true);
            DrawMetric("Riesgo de colapso", CollapseRisk, false);
            DrawMetric("Readiness expansión", ExpansionReadiness, true);
            GUILayout.Space(8f);
            GUILayout.Label("Tendencias 2s — Colonización " + Signed(LastColonizationTrend) + " | Toxinas " + Signed(LastToxinTrend) + " | O₂ " + Signed(LastOxygenTrend), bodyStyle);
            GUILayout.Space(8f);
            GUILayout.Label("Forecast: " + Forecast, CollapseRisk > 0.7f ? badStyle : bodyStyle);
            GUILayout.Label("Recomendación: " + Recommendation, bodyStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Recalcular ahora")) Evaluate();
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndArea();
        }

        private void DrawMetric(string label, float value, bool highIsGood)
        {
            GUILayout.Label(label + ": " + (value * 100f).ToString("0") + "%", highIsGood ? goodStyle : badStyle);
            Rect rect = GUILayoutUtility.GetRect(1f, 12f, GUILayout.ExpandWidth(true));
            GUI.Box(rect, GUIContent.none);
            Rect fill = new Rect(rect.x + 2f, rect.y + 2f, Mathf.Max(0f, rect.width - 4f) * Mathf.Clamp01(value), Mathf.Max(0f, rect.height - 4f));
            GUI.Box(fill, GUIContent.none);
        }

        private string Signed(float v)
        {
            return (v >= 0f ? "+" : "") + v.ToString("0.000");
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.fontSize = 17; titleStyle.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
            goodStyle = new GUIStyle(bodyStyle); goodStyle.normal.textColor = new Color(0.68f, 1f, 0.72f, 1f);
            badStyle = new GUIStyle(bodyStyle); badStyle.normal.textColor = new Color(1f, 0.56f, 0.44f, 1f);
        }
    }
}
