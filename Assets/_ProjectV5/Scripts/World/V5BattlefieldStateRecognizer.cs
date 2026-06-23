using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5BattlefieldStateType
    {
        ToxicScar,
        NutrientCorridor,
        OxygenFront,
        AcidPocket,
        LivingNetwork,
        ChemicalPanic
    }

    public struct V5BattlefieldState
    {
        public V5BattlefieldStateType type;
        public string label;
        public Vector2 position;
        public float radius;
        public float intensity;
        public string cause;
        public string response;
    }

    public class V5BattlefieldStateRecognizer : MonoBehaviour, IV5RunResettable
    {
        public bool ShowPanel;
        public bool ShowWorldLabels = true;
        public float ScanInterval = 2f;
        public string Summary = "Campo estable";
        public readonly List<V5BattlefieldState> ActiveStates = new List<V5BattlefieldState>(6);

        private float scanTimer;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;
        private GUIStyle label;

        private void Start()
        {
            V5PanelRouter.Register("Campo", () => ShowPanel, v => ShowPanel = v);
        }

        private void Update()
        {
            scanTimer += Time.deltaTime;
            if (scanTimer < ScanInterval) return;
            scanTimer = 0f;
            Scan();
        }

        public void ResetForNewRun()
        {
            ActiveStates.Clear();
            Summary = "Campo estable";
            scanTimer = 0f;
        }

        public bool HasState(V5BattlefieldStateType type)
        {
            for (int i = 0; i < ActiveStates.Count; i++)
                if (ActiveStates[i].type == type) return true;
            return false;
        }

        private void Scan()
        {
            V5GameManager gm = V5GameManager.Instance;
            V5EnvironmentGrid env = gm != null ? gm.Environment : FindFirstObjectByType<V5EnvironmentGrid>();
            if (env == null || env.nutrients == null) return;

            Candidate[] candidates = new Candidate[6];
            int step = Mathf.Max(2, env.Width / 34);

            for (int x = 1; x < env.Width - 1; x += step)
            {
                for (int y = 1; y < env.Height - 1; y += step)
                {
                    Vector2 world = env.TileCenterWorld(x, y);
                    if (world.magnitude > env.MapRadius) continue;
                    EvaluateTile(env, x, y, world, step, candidates);
                }
            }

            EvaluateChemicalPanicFromCells(gm, env, candidates);

            ActiveStates.Clear();
            AddCandidate(candidates, V5BattlefieldStateType.ToxicScar);
            AddCandidate(candidates, V5BattlefieldStateType.NutrientCorridor);
            AddCandidate(candidates, V5BattlefieldStateType.OxygenFront);
            AddCandidate(candidates, V5BattlefieldStateType.AcidPocket);
            AddCandidate(candidates, V5BattlefieldStateType.LivingNetwork);
            AddCandidate(candidates, V5BattlefieldStateType.ChemicalPanic);
            Summary = BuildSummary();
        }

        private void EvaluateTile(V5EnvironmentGrid env, int x, int y, Vector2 world, int step, Candidate[] candidates)
        {
            float nutrients = env.nutrients[x, y];
            float oxygen = env.oxygen[x, y];
            float toxins = env.toxins[x, y];
            float acidity = env.acidity[x, y];
            float colonization = env.colonization[x, y];
            float detritus = env.detritus[x, y];

            float toxicScore = Mathf.Clamp01(toxins * 2.9f + detritus * 1.1f + Mathf.Max(0f, 0.30f - oxygen) * 0.6f);
            if (toxins > 0.18f || (toxins > 0.10f && detritus > 0.16f))
                Offer(candidates, V5BattlefieldStateType.ToxicScar, world, 4.2f, toxicScore, "Cicatriz toxica", "toxinas y cadaveres acumulados", "evitar, catalasa o rutas resistentes");

            float nutrientScore = Mathf.Clamp01(nutrients * 0.78f + detritus * 0.55f - toxins * 0.28f);
            if (nutrients > 0.78f && toxins < 0.34f)
                Offer(candidates, V5BattlefieldStateType.NutrientCorridor, world, 5.5f, nutrientScore, "Corredor nutritivo", "nutrientes concentrados y baja toxicidad", "farmear, colonizar o disputar");

            float oxygenGradient = OxygenGradient(env, x, y, step);
            float oxygenScore = Mathf.Clamp01(oxygen * 0.72f + oxygenGradient * 3.2f);
            if ((oxygen > 0.36f && oxygenGradient > 0.035f) || oxygen > 0.50f)
                Offer(candidates, V5BattlefieldStateType.OxygenFront, world, 5.0f, oxygenScore, "Frente oxigenado", "fotosintesis o mezcla de aguas crea borde de O2", "usar aerobios o presionar anaerobios");

            float acidDistance = Mathf.Abs(acidity - 0.50f);
            float acidScore = Mathf.Clamp01(acidDistance * 2.8f);
            if (acidity > 0.66f || acidity < 0.28f)
                Offer(candidates, V5BattlefieldStateType.AcidPocket, world, 4.8f, acidScore, acidity > 0.50f ? "Bolsa acida" : "Bolsa alcalina", "pH local extremo", "usar Arquea, cuticula o rodear");

            float networkScore = Mathf.Clamp01(colonization * 1.8f + detritus * 0.35f);
            if (colonization > 0.20f)
                Offer(candidates, V5BattlefieldStateType.LivingNetwork, world, 6.0f, networkScore, "Red viva", "biofilm, hifas o mucilago sostienen territorio", "defender nodos o cortar conexiones");

            float panicScore = Mathf.Clamp01(toxins * 1.7f + Mathf.Max(0f, 0.24f - oxygen) * 1.1f + acidDistance * 0.65f);
            if (panicScore > 0.62f && (toxins > 0.16f || oxygen < 0.18f))
                Offer(candidates, V5BattlefieldStateType.ChemicalPanic, world, 4.5f, panicScore, "Panico quimico", "toxinas, bajo O2 o pH empujan retirada", "replegar, reparar o adaptar membrana");
        }

        private float OxygenGradient(V5EnvironmentGrid env, int x, int y, int step)
        {
            int left = Mathf.Max(0, x - step);
            int right = Mathf.Min(env.Width - 1, x + step);
            int down = Mathf.Max(0, y - step);
            int up = Mathf.Min(env.Height - 1, y + step);
            float center = env.oxygen[x, y];
            float gx = Mathf.Max(Mathf.Abs(center - env.oxygen[left, y]), Mathf.Abs(center - env.oxygen[right, y]));
            float gy = Mathf.Max(Mathf.Abs(center - env.oxygen[x, down]), Mathf.Abs(center - env.oxygen[x, up]));
            return Mathf.Max(gx, gy);
        }

        private void EvaluateChemicalPanicFromCells(V5GameManager gm, V5EnvironmentGrid env, Candidate[] candidates)
        {
            if (gm == null || env == null) return;
            EvaluateCellListForPanic(gm.PlayerCells, gm.NonPlayerCells, env, candidates, true);
            EvaluateCellListForPanic(gm.NonPlayerCells, gm.PlayerCells, env, candidates, false);
        }

        private void EvaluateCellListForPanic(IReadOnlyList<V5CellEntity> cells, IReadOnlyList<V5CellEntity> enemies, V5EnvironmentGrid env, Candidate[] candidates, bool playerOwned)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell == null || cell.Stats.currentHp <= 0f) continue;
                int tx, ty;
                env.WorldToTile(cell.transform.position, out tx, out ty);
                int threats = playerOwned ? CountNearby(enemies, cell.transform.position, 5.5f) : 0;
                float localToxins = env.toxins[tx, ty];
                float stressScore = cell.Stats.stress / 100f;
                float score = Mathf.Clamp01(stressScore * 0.72f + localToxins * 1.35f + threats * 0.12f);
                if (cell.Stats.stress > 68f || threats >= 2)
                    Offer(candidates, V5BattlefieldStateType.ChemicalPanic, cell.transform.position, 4.5f, score, "Panico quimico", "stress celular y firmas hostiles cercanas", "retirada, cohesion o adaptacion");
            }
        }

        private int CountNearby(IReadOnlyList<V5CellEntity> cells, Vector2 center, float radius)
        {
            int count = 0;
            float r2 = radius * radius;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell == null || cell.Stats.currentHp <= 0f) continue;
                if (((Vector2)cell.transform.position - center).sqrMagnitude <= r2) count++;
            }
            return count;
        }

        private void Offer(Candidate[] candidates, V5BattlefieldStateType type, Vector2 position, float radius, float intensity, string labelText, string cause, string response)
        {
            int index = (int)type;
            if (index < 0 || index >= candidates.Length) return;
            if (candidates[index].found && candidates[index].state.intensity >= intensity) return;

            V5BattlefieldState state = new V5BattlefieldState();
            state.type = type;
            state.label = labelText;
            state.position = position;
            state.radius = radius;
            state.intensity = Mathf.Clamp01(intensity);
            state.cause = cause;
            state.response = response;
            candidates[index] = new Candidate { found = true, state = state };
        }

        private void AddCandidate(Candidate[] candidates, V5BattlefieldStateType type)
        {
            int index = (int)type;
            if (index >= 0 && index < candidates.Length && candidates[index].found)
                ActiveStates.Add(candidates[index].state);
        }

        private string BuildSummary()
        {
            if (ActiveStates.Count == 0) return "Campo estable";
            string result = "";
            int limit = Mathf.Min(3, ActiveStates.Count);
            for (int i = 0; i < limit; i++)
            {
                result += ActiveStates[i].label;
                if (i < limit - 1) result += ", ";
            }
            if (ActiveStates.Count > limit) result += " +" + (ActiveStates.Count - limit);
            return result;
        }

        private void OnGUI()
        {
            EnsureStyles();
            if (ShowWorldLabels) DrawWorldLabels();
            if (!ShowPanel) return;
            DrawPanel();
        }

        private void DrawPanel()
        {
            Rect r = new Rect(20f, 212f, 460f, 360f);
            GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 12f, r.width - 28f, r.height - 24f));
            GUILayout.Label("LIVING BATTLEFIELD", title);
            GUILayout.Label("Estados detectados: " + Summary, body);
            ShowWorldLabels = GUILayout.Toggle(ShowWorldLabels, "Etiquetas en el mundo");
            GUILayout.Space(8f);
            if (ActiveStates.Count == 0)
            {
                GUILayout.Label("Campo estable: no hay patrones fuertes detectados.", body);
            }
            for (int i = 0; i < ActiveStates.Count; i++)
            {
                V5BattlefieldState state = ActiveStates[i];
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label(state.label + "  " + (state.intensity * 100f).ToString("0") + "%", title);
                GUILayout.Label("Causa: " + state.cause, body);
                GUILayout.Label("Respuesta: " + state.response, body);
                GUILayout.Label("Posicion: " + state.position.x.ToString("0.0") + ", " + state.position.y.ToString("0.0"), body);
                GUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }

        private void DrawWorldLabels()
        {
            if (ActiveStates.Count == 0) return;
            Camera cam = Camera.main;
            if (cam == null) return;
            for (int i = 0; i < ActiveStates.Count; i++)
            {
                V5BattlefieldState state = ActiveStates[i];
                Vector3 screen = cam.WorldToScreenPoint(state.position);
                if (screen.z < 0f) continue;
                Rect rect = new Rect(screen.x + 8f, Screen.height - screen.y - 12f + i * 2f, 150f, 20f);
                Color oldColor = GUI.color;
                GUI.color = ColorForState(state.type);
                GUI.Box(rect, state.label + " " + (state.intensity * 100f).ToString("0") + "%", label);
                GUI.color = oldColor;
            }
        }

        private Color ColorForState(V5BattlefieldStateType type)
        {
            switch (type)
            {
                case V5BattlefieldStateType.ToxicScar: return new Color(1f, 0.35f, 0.9f, 0.84f);
                case V5BattlefieldStateType.NutrientCorridor: return new Color(0.35f, 1f, 0.45f, 0.84f);
                case V5BattlefieldStateType.OxygenFront: return new Color(0.35f, 0.78f, 1f, 0.84f);
                case V5BattlefieldStateType.AcidPocket: return new Color(1f, 0.44f, 0.22f, 0.84f);
                case V5BattlefieldStateType.LivingNetwork: return new Color(0.72f, 1f, 0.55f, 0.84f);
                case V5BattlefieldStateType.ChemicalPanic: return new Color(1f, 0.82f, 0.25f, 0.84f);
                default: return Color.white;
            }
        }

        private void EnsureStyles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box);
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white;
            label = new GUIStyle(GUI.skin.box); label.fontSize = 10; label.normal.textColor = Color.white;
        }

        private struct Candidate
        {
            public bool found;
            public V5BattlefieldState state;
        }
    }
}
