using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Macro dashboard: scores the run as Economy / Defense / Expansion / Ecology.
    /// Toggle with 9. Designed for quick playtest reads and balance tuning.
    /// </summary>
    public class V5ColonyHealthDashboardIMGUI : MonoBehaviour
    {
        public bool ShowPanel;
        public float EconomyScore { get; private set; }
        public float DefenseScore { get; private set; }
        public float ExpansionScore { get; private set; }
        public float EcologyScore { get; private set; }
        public float OverallScore { get; private set; }
        public string Diagnosis { get; private set; }

        private float tickTimer;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        private void Start() => V5PanelRouter.Register("Dashboard", () => ShowPanel, v => ShowPanel = v);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha9)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Dashboard"); ShowPanel = !ShowPanel; }
            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.8f)
            {
                tickTimer = 0f;
                Recalculate();
            }
        }

        public void Recalculate()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity m = gm.MotherCell;
            float resourceTotal = m.Resources.atp + m.Resources.biomass;
            float colonized = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float toxins = gm.Environment != null ? gm.Environment.AverageToxins() : 0f;
            float oxygen = gm.Environment != null ? gm.Environment.AverageOxygen() : 0f;
            float enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0f;
            float cells = Mathf.Max(1f, gm.PlayerCellCount());
            float stress = m.Stats.stress / 100f;
            float hp = m.Stats.currentHp / Mathf.Max(1f, m.Stats.maxHp);

            EconomyScore = Mathf.Clamp01(resourceTotal / 420f + cells / 60f);
            DefenseScore = Mathf.Clamp01(hp * 0.55f + (1f - stress) * 0.25f + Mathf.Clamp01(cells / Mathf.Max(1f, enemies + 1f)) * 0.20f);
            ExpansionScore = Mathf.Clamp01(colonized / 0.40f + Mathf.Clamp01(cells / 18f) * 0.25f);
            EcologyScore = Mathf.Clamp01((1f - toxins) * 0.45f + oxygen * 0.25f + (1f - Mathf.Abs(0.5f - (gm.Environment != null ? gm.Environment.AverageAcidity() : 0.5f)) * 2f) * 0.20f + colonized * 0.10f);
            OverallScore = (EconomyScore + DefenseScore + ExpansionScore + EcologyScore) / 4f;

            if (DefenseScore < 0.35f) Diagnosis = "CRÍTICO: defiende la madre, repara y activa doctrina Recovery/Turtle.";
            else if (EconomyScore < 0.35f) Diagnosis = "ECONOMÍA BAJA: asigna farmers o activa Bloom Economy.";
            else if (ExpansionScore < 0.35f) Diagnosis = "EXPANSIÓN BAJA: usa colonizers, adhesión o biofilm.";
            else if (EcologyScore < 0.35f) Diagnosis = "ECOLOGÍA INESTABLE: catalasa, detox, oxígeno o simbiosis.";
            else if (OverallScore > 0.75f) Diagnosis = "COLONIA ESTABLE: empuja apex/endgame o sube dificultad.";
            else Diagnosis = "COLONIA VIABLE: sigue especializando y colonizando.";
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width * 0.5f - 230f, 88f, 460f, 330f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 12f, r.width - 28f, r.height - 24f));
            GUILayout.Label("DASHBOARD DE SALUD COLONIAL", titleStyle);
            DrawBar("Economía", EconomyScore);
            DrawBar("Defensa", DefenseScore);
            DrawBar("Expansión", ExpansionScore);
            DrawBar("Ecología", EcologyScore);
            GUILayout.Space(8f);
            DrawBar("Global", OverallScore);
            GUILayout.Space(8f);
            GUILayout.Label(Diagnosis, bodyStyle);
            V5ColonyAutomationSystem automation = FindFirstObjectByType<V5ColonyAutomationSystem>();
            if (automation != null)
            {
                GUILayout.Space(6f);
                GUILayout.Label("Automatización: " + (automation.AutomationEnabled ? "ON" : "OFF") + " / " + automation.Doctrine + " | " + automation.LastAction, bodyStyle);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndArea();
        }

        private void DrawBar(string label, float value)
        {
            Rect rect = GUILayoutUtility.GetRect(1f, 24f, GUILayout.ExpandWidth(true));
            GUI.Label(new Rect(rect.x, rect.y, 96f, rect.height), label, bodyStyle);
            Rect bg = new Rect(rect.x + 104f, rect.y + 4f, rect.width - 160f, 14f);
            GUI.Box(bg, GUIContent.none);
            GUI.Box(new Rect(bg.x + 2f, bg.y + 2f, Mathf.Max(0f, bg.width - 4f) * Mathf.Clamp01(value), Mathf.Max(0f, bg.height - 4f)), GUIContent.none);
            GUI.Label(new Rect(rect.x + rect.width - 48f, rect.y, 48f, rect.height), (value * 100f).ToString("0") + "%", bodyStyle);
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.fontSize = 17; titleStyle.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
        }
    }
}
