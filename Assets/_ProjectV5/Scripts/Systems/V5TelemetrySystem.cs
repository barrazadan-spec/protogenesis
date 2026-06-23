using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5TelemetrySystem : MonoBehaviour, IV5RunResettable
    {
        public float CellsCreated { get; private set; }
        public float MaxCells { get; private set; }
        public float StructuresInstalled { get; private set; }
        public float ResourcesCollected { get; private set; }
        public float DamageDealtEstimate { get; private set; }
        public float AdaptationsInstalled { get; private set; }
        public float MilestonesInstalled { get; private set; }
        public float FailedAdaptationAttempts { get; private set; }
        public float CapBlockedAttempts { get; private set; }
        public string Summary { get; private set; } = "Telemetria esperando run.";
        public string AdaptationSummary { get; private set; } = "Sin adaptaciones registradas.";
        public string DominantRouteSummary { get; private set; } = "Sin ruta dominante.";
        public string FailureSummary { get; private set; } = "Sin bloqueos.";
        public string LastAdaptationEvent { get; private set; } = "Sin eventos de adaptacion.";

        private int lastCellCount;
        private float lastAtp;
        private float timer;
        private bool showPanel;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;

        private readonly Dictionary<V5AdaptationId, int> adaptationPicks = new Dictionary<V5AdaptationId, int>();
        private readonly Dictionary<V5AdaptationId, float> firstInstallTime = new Dictionary<V5AdaptationId, float>();
        private readonly Dictionary<V5EvolutionPath, int> routePicks = new Dictionary<V5EvolutionPath, int>();
        private readonly Dictionary<V5AdaptationTier, int> tierPicks = new Dictionary<V5AdaptationTier, int>();
        private readonly Dictionary<string, int> failureBuckets = new Dictionary<string, int>();

        private void Start()
        {
            V5PanelRouter.Register("Telemetria", () => showPanel, value => showPanel = value);
        }

        public void ResetForNewRun()
        {
            CellsCreated = 0f;
            MaxCells = 0f;
            StructuresInstalled = 0f;
            ResourcesCollected = 0f;
            DamageDealtEstimate = 0f;
            AdaptationsInstalled = 0f;
            MilestonesInstalled = 0f;
            FailedAdaptationAttempts = 0f;
            CapBlockedAttempts = 0f;
            lastCellCount = 0;
            lastAtp = 0f;
            timer = 0f;
            adaptationPicks.Clear();
            firstInstallTime.Clear();
            routePicks.Clear();
            tierPicks.Clear();
            failureBuckets.Clear();
            LastAdaptationEvent = "Sin eventos de adaptacion.";
            RebuildText(V5GameManager.Instance);
        }

        public void RecordAdaptationInstalled(V5AdaptationDefinition def, int activeCount, int activeCap)
        {
            if (def == null) return;

            if (!adaptationPicks.ContainsKey(def.id)) adaptationPicks[def.id] = 0;
            adaptationPicks[def.id]++;

            V5GameManager gm = V5GameManager.Instance;
            float elapsed = gm != null ? gm.ElapsedSeconds : Time.timeSinceLevelLoad;
            if (!firstInstallTime.ContainsKey(def.id)) firstInstallTime[def.id] = elapsed;

            if (def.countsTowardCap) AdaptationsInstalled += 1f;
            else MilestonesInstalled += 1f;

            if (!tierPicks.ContainsKey(def.tier)) tierPicks[def.tier] = 0;
            tierPicks[def.tier]++;

            V5EvolutionPath route = def.routeHint;
            if (route == V5EvolutionPath.Uncommitted && gm != null && gm.Identity != null)
            {
                route = gm.Identity.EvolutionPath;
            }
            if (!routePicks.ContainsKey(route)) routePicks[route] = 0;
            routePicks[route]++;

            LastAdaptationEvent = FormatTime(elapsed) + " +" + def.shortName + " (" +
                                  (def.countsTowardCap ? activeCount + "/" + activeCap : "hito") + ")";
            RebuildText(gm);
        }

        public void RecordAdaptationInstallFailed(V5AdaptationId id, string reason)
        {
            FailedAdaptationAttempts += 1f;
            string bucket = FailureBucket(reason);
            if (bucket == "cap") CapBlockedAttempts += 1f;
            if (!failureBuckets.ContainsKey(bucket)) failureBuckets[bucket] = 0;
            failureBuckets[bucket]++;

            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            string label = def != null ? def.shortName : id.ToString();
            LastAdaptationEvent = "Bloqueada " + label + ": " + reason;
            RebuildText(V5GameManager.Instance);
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            timer += Time.deltaTime;
            if (timer < 1.0f) return;
            timer = 0f;

            int cells = gm.PlayerCellCount();
            if (cells > lastCellCount) CellsCreated += cells - lastCellCount;
            lastCellCount = cells;
            MaxCells = Mathf.Max(MaxCells, cells);

            float atp = gm.MotherCell.Resources.atp;
            if (atp > lastAtp) ResourcesCollected += atp - lastAtp;
            lastAtp = atp;

            StructuresInstalled = gm.Adaptations != null ? gm.Adaptations.ActiveCount() : gm.MotherCell.Structures.Count;
            RebuildText(gm);
        }

        public string TopAdaptationsText(int max)
        {
            if (adaptationPicks.Count == 0) return "ninguna";
            string result = "";
            int written = 0;
            List<KeyValuePair<V5AdaptationId, int>> sorted = new List<KeyValuePair<V5AdaptationId, int>>(adaptationPicks);
            sorted.Sort((a, b) => b.Value.CompareTo(a.Value));
            for (int i = 0; i < sorted.Count && written < max; i++)
            {
                V5AdaptationDefinition def = V5AdaptationLibrary.Get(sorted[i].Key);
                if (def == null) continue;
                if (result.Length > 0) result += ", ";
                result += def.shortName + " x" + sorted[i].Value;
                written++;
            }
            return result.Length > 0 ? result : "ninguna";
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            Rect rect = new Rect(Screen.width - 448f, 84f, 430f, 300f);
            GUI.Box(rect, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(rect.x + 14f, rect.y + 12f, rect.width - 28f, rect.height - 24f));
            GUILayout.Label("TELEMETRIA DE PLAYTEST v5.15.1", titleStyle);
            GUILayout.Space(6f);
            GUILayout.Label(Summary, bodyStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Genoma", smallStyle);
            GUILayout.Label(AdaptationSummary, bodyStyle);
            GUILayout.Label("Ruta: " + DominantRouteSummary, bodyStyle);
            GUILayout.Label("Fallos: " + FailureSummary, bodyStyle);
            GUILayout.Space(6f);
            GUILayout.Label("Ultimo evento", smallStyle);
            GUILayout.Label(LastAdaptationEvent, bodyStyle);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Reset datos")) ResetForNewRun();
            if (GUILayout.Button("Cerrar")) showPanel = false;
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void RebuildText(V5GameManager gm)
        {
            int activeCount = gm != null && gm.Adaptations != null ? gm.Adaptations.ActiveCount() : Mathf.RoundToInt(StructuresInstalled);
            int activeCap = gm != null && gm.Adaptations != null ? gm.Adaptations.ActiveCap : 0;
            Summary = string.Format(
                "Celulas {0:0} | Max {1:0} | Adapt {2:0}/{3:0} | Hitos {4:0} | Recursos +{5:0}",
                CellsCreated,
                MaxCells,
                activeCount,
                activeCap,
                MilestonesInstalled,
                ResourcesCollected);

            AdaptationSummary = "Top: " + TopAdaptationsText(4) + " | Tier: " + TierSummary();
            DominantRouteSummary = RouteSummary();
            FailureSummary = FailedAdaptationAttempts <= 0f ? "Sin bloqueos." : FailureBucketSummary();
        }

        private string TierSummary()
        {
            if (tierPicks.Count == 0) return "sin datos";
            string result = "";
            AppendTier(ref result, V5AdaptationTier.T1Prokaryote, "T1");
            AppendTier(ref result, V5AdaptationTier.T2Eukaryogenesis, "T2");
            AppendTier(ref result, V5AdaptationTier.T3Specialization, "T3");
            AppendTier(ref result, V5AdaptationTier.T4ColonialBody, "T4");
            AppendTier(ref result, V5AdaptationTier.T5Apex, "T5");
            return result.Length > 0 ? result : "sin datos";
        }

        private void AppendTier(ref string result, V5AdaptationTier tier, string label)
        {
            int count;
            if (!tierPicks.TryGetValue(tier, out count) || count <= 0) return;
            if (result.Length > 0) result += " ";
            result += label + ":" + count;
        }

        private string RouteSummary()
        {
            if (routePicks.Count == 0) return "Sin ruta dominante.";
            V5EvolutionPath best = V5EvolutionPath.Uncommitted;
            int bestCount = -1;
            foreach (KeyValuePair<V5EvolutionPath, int> pair in routePicks)
            {
                if (pair.Value > bestCount)
                {
                    best = pair.Key;
                    bestCount = pair.Value;
                }
            }
            return best + " (" + bestCount + " senales)";
        }

        private string FailureBucketSummary()
        {
            string result = "";
            AppendBucket(ref result, "cap", "cap");
            AppendBucket(ref result, "recursos", "rec");
            AppendBucket(ref result, "requisitos", "req");
            AppendBucket(ref result, "duplicada", "dup");
            AppendBucket(ref result, "otro", "otro");
            return result.Length > 0 ? result : "sin detalle";
        }

        private void AppendBucket(ref string result, string bucket, string label)
        {
            int count;
            if (!failureBuckets.TryGetValue(bucket, out count) || count <= 0) return;
            if (result.Length > 0) result += " ";
            result += label + ":" + count;
        }

        private string FailureBucket(string reason)
        {
            if (string.IsNullOrEmpty(reason)) return "otro";
            if (reason.StartsWith("Cap activo")) return "cap";
            if (reason.StartsWith("Faltan recursos")) return "recursos";
            if (reason.StartsWith("Requiere")) return "requisitos";
            if (reason.StartsWith("Ya instalada")) return "duplicada";
            return "otro";
        }

        private string FormatTime(float seconds)
        {
            return (seconds / 60f).ToString("0.0") + "m";
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontSize = 16;
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.85f, 1f, 0.95f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(GUI.skin.label);
            smallStyle.fontSize = 11;
            smallStyle.fontStyle = FontStyle.Bold;
            smallStyle.normal.textColor = new Color(0.7f, 0.95f, 1f, 1f);
        }
    }
}
