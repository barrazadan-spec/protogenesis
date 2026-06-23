using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5WorkOrderType
    {
        Harvest,
        Colonize,
        Defend,
        Detox,
        Hunt,
        Expand
    }

    [Serializable]
    public class V5WorkOrderSnapshot
    {
        public string type;
        public float x;
        public float y;
        public float progress;
        public bool complete;
    }

    [Serializable]
    public class V5OperationsSnapshot
    {
        public string version = "2.1";
        public float logisticsHealth;
        public float nutrientFlow;
        public float networkLoad;
        public string bottleneck;
        public List<V5WorkOrderSnapshot> orders = new List<V5WorkOrderSnapshot>();
    }

    /// <summary>
    /// V5 2.1 macro layer. It gives the prototype a clearer RTS/world-builder loop:
    /// the colony has logistics health, bottlenecks and work orders that can assign
    /// cells to harvesting, colonizing, defending, detoxing, hunting and expansion.
    /// </summary>
    public class V5ColonyOperationsSystem : MonoBehaviour
    {
        public float LogisticsHealth { get; private set; }
        public float NutrientFlow { get; private set; }
        public float NetworkLoad { get; private set; }
        public float WorkerCoverage { get; private set; }
        public string Bottleneck { get; private set; }
        public string LastAction { get; private set; }
        public bool AutoAssign = true;
        public bool AutoStabilize = true;

        private readonly List<WorkOrder> orders = new List<WorkOrder>(16);
        private float tickTimer;
        private float effectTimer;
        private bool showPanel;
        private Vector2 scroll;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle small;
        private GUIStyle button;

        private string ExportPath
        {
            get { return Path.Combine(Application.persistentDataPath, "protogenesis_v5_operations_2_1.json"); }
        }

        private void Awake()
        {
            Bottleneck = "Waiting for colony data.";
            LastAction = "Operations online.";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadEnter)) showPanel = !showPanel;
            if (Input.GetKeyDown(KeyCode.Keypad0)) AutoAssign = !AutoAssign;
            if (Input.GetKeyDown(KeyCode.KeypadPeriod)) AutoStabilize = !AutoStabilize;
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            {
                if (Input.GetKeyDown(KeyCode.Keypad1)) AddOrderAtMouse(V5WorkOrderType.Harvest);
                if (Input.GetKeyDown(KeyCode.Keypad2)) AddOrderAtMouse(V5WorkOrderType.Colonize);
                if (Input.GetKeyDown(KeyCode.Keypad3)) AddOrderAtMouse(V5WorkOrderType.Defend);
                if (Input.GetKeyDown(KeyCode.Keypad4)) AddOrderAtMouse(V5WorkOrderType.Detox);
                if (Input.GetKeyDown(KeyCode.Keypad5)) AddOrderAtMouse(V5WorkOrderType.Hunt);
                if (Input.GetKeyDown(KeyCode.Keypad6)) AddOrderAtMouse(V5WorkOrderType.Expand);
            }

            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.75f)
            {
                tickTimer = 0f;
                EvaluateLogistics();
                UpdateOrders();
                if (AutoAssign) AssignWorkersToOrders();
            }

            effectTimer += Time.deltaTime;
            if (effectTimer >= 2.0f)
            {
                effectTimer = 0f;
                ApplyLogisticsEffects();
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;

            Rect r = new Rect(Screen.width - 560, 112, 540, Screen.height - 180);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "COLONY OPERATIONS 2.1 — LOGISTICS + WORK ORDERS", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), string.Format("Health {0:0}% | Flow {1:0}% | Load {2:0}% | Workers {3:0}%", LogisticsHealth * 100f, NutrientFlow * 100f, NetworkLoad * 100f, WorkerCoverage * 100f), small);
            GUI.Label(new Rect(r.x + 12, r.y + 60, r.width - 24, 40), "Bottleneck: " + Bottleneck + "\nLast: " + LastAction, small);

            float y = r.y + 106;
            AutoAssign = GUI.Toggle(new Rect(r.x + 12, y, 170, 22), AutoAssign, "Auto-assign workers");
            AutoStabilize = GUI.Toggle(new Rect(r.x + 190, y, 160, 22), AutoStabilize, "Auto-stabilize");
            if (GUI.Button(new Rect(r.x + 360, y, 72, 24), "Export", button)) ExportSnapshot();
            if (GUI.Button(new Rect(r.x + 438, y, 82, 24), "Clear done", button)) ClearCompleted();
            y += 34;

            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 20), "Create order at mouse position:", small);
            y += 22;
            DrawOrderButton(r.x + 12, y, 78, V5WorkOrderType.Harvest);
            DrawOrderButton(r.x + 94, y, 78, V5WorkOrderType.Colonize);
            DrawOrderButton(r.x + 176, y, 78, V5WorkOrderType.Defend);
            DrawOrderButton(r.x + 258, y, 78, V5WorkOrderType.Detox);
            DrawOrderButton(r.x + 340, y, 78, V5WorkOrderType.Hunt);
            DrawOrderButton(r.x + 422, y, 78, V5WorkOrderType.Expand);
            y += 38;

            if (GUI.Button(new Rect(r.x + 12, y, 160, 28), "Recommended order", button)) CreateRecommendedOrder();
            if (GUI.Button(new Rect(r.x + 178, y, 160, 28), "Balance resources", button)) BalanceResourceFlow();
            if (GUI.Button(new Rect(r.x + 344, y, 176, 28), "Send idle to jobs", button)) AssignWorkersToOrders();
            y += 42;

            Rect view = new Rect(r.x + 12, y, r.width - 24, r.height - (y - r.y) - 12);
            Rect content = new Rect(0, 0, view.width - 20, Mathf.Max(260, orders.Count * 72 + 8));
            scroll = GUI.BeginScrollView(view, scroll, content);
            float cy = 0f;
            if (orders.Count == 0)
            {
                GUI.Label(new Rect(0, cy, content.width, 80), "No work orders yet. Create one manually or press Recommended order.\nHotkeys: KeypadEnter panel | Ctrl+Keypad 1-6 create orders | Keypad0 auto-assign | Keypad . auto-stabilize", small);
            }
            for (int i = 0; i < orders.Count; i++)
            {
                WorkOrder o = orders[i];
                GUI.Box(new Rect(0, cy, content.width, 64), "", box);
                GUI.Label(new Rect(8, cy + 6, content.width - 16, 18), string.Format("#{0} {1} at {2:0.0},{3:0.0} | assigned {4} | {5:0}%", i + 1, o.type, o.position.x, o.position.y, o.assignedCount, o.progress * 100f), small);
                GUI.Label(new Rect(8, cy + 26, content.width - 110, 32), o.status, small);
                if (GUI.Button(new Rect(content.width - 96, cy + 22, 86, 26), o.complete ? "Remove" : "Cancel", button))
                {
                    orders.RemoveAt(i);
                    i--;
                    cy += 68;
                    continue;
                }
                cy += 68;
            }
            GUI.EndScrollView();
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.fontSize = 11; box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 14; title.normal.textColor = new Color(0.86f, 1f, 0.92f, 1f);
            small = new GUIStyle(GUI.skin.label); small.fontSize = 11; small.wordWrap = true; small.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button); button.fontSize = 10; button.wordWrap = true;
        }

        private void DrawOrderButton(float x, float y, float w, V5WorkOrderType type)
        {
            if (GUI.Button(new Rect(x, y, w, 28), type.ToString(), button)) AddOrderAtMouse(type);
        }

        private void EvaluateLogistics()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null)
            {
                LogisticsHealth = 0f;
                NutrientFlow = 0f;
                NetworkLoad = 1f;
                WorkerCoverage = 0f;
                Bottleneck = "No mother cell.";
                return;
            }

            int cells = gm.PlayerCellCount();
            int farmers = 0;
            int colonizers = 0;
            int defenders = 0;
            int starving = 0;
            float avgDistance = 0f;
            IReadOnlyList<V5CellEntity> list = gm.PlayerCells;
            Vector2 motherPos = gm.MotherCell.transform.position;
            for (int i = 0; i < list.Count; i++)
            {
                V5CellEntity c = list[i];
                if (c == null) continue;
                if (c.Directive == V5Directive.Farm || c.LineageRole == V5LineageRole.Farmer) farmers++;
                if (c.Directive == V5Directive.Colonize || c.LineageRole == V5LineageRole.Colonizer) colonizers++;
                if (c.Directive == V5Directive.Defend || c.LineageRole == V5LineageRole.Defender) defenders++;
                if (c.Resources.atp < 8f || c.Resources.biomass < 4f) starving++;
                avgDistance += Vector2.Distance(motherPos, c.transform.position);
            }
            avgDistance = list.Count > 0 ? avgDistance / list.Count : 0f;

            V5EnvironmentGrid env = gm.Environment;
            float colonization = env != null ? env.AverageColonization() : 0f;
            float toxins = env != null ? env.AverageToxins() : 0f;
            float oxygen = env != null ? env.AverageOxygen() : 0f;
            V5ColonyDistrictSystem districts = FindFirstObjectByType<V5ColonyDistrictSystem>();
            float districtNetwork = districts != null ? districts.NetworkEfficiency : Mathf.Clamp01(colonization * 2.5f);
            float districtPressure = districts != null ? districts.SupplyPressure : Mathf.Clamp01((cells - V5Balance.SoftControllableEntityCount) / 10f);

            WorkerCoverage = Mathf.Clamp01((farmers * 0.35f + colonizers * 0.25f + defenders * 0.18f) / Mathf.Max(1f, cells * 0.45f));
            NetworkLoad = Mathf.Clamp01((cells / Mathf.Max(1f, V5Balance.HardControllableEntityCap)) * 0.55f + districtPressure * 0.35f + avgDistance / 120f + starving / Mathf.Max(1f, cells) * 0.35f);
            NutrientFlow = Mathf.Clamp01(0.22f + WorkerCoverage * 0.32f + districtNetwork * 0.26f + Mathf.Clamp01(gm.MotherCell.Resources.biomass / 120f) * 0.12f + oxygen * 0.08f - toxins * 0.16f);
            LogisticsHealth = Mathf.Clamp01(NutrientFlow * 0.58f + districtNetwork * 0.28f + (1f - NetworkLoad) * 0.30f);

            if (starving > 0) Bottleneck = starving + " cells are resource-starved. Balance flow or send farmers.";
            else if (farmers == 0 && cells > 2) Bottleneck = "No farmers assigned; economy can stall.";
            else if (districtPressure > 0.35f) Bottleneck = "District supply pressure is high; claim or specialize districts.";
            else if (toxins > 0.35f) Bottleneck = "Toxicity is degrading colony flow.";
            else if (avgDistance > 22f) Bottleneck = "Cells are stretched far from mother; create expansion/defense orders.";
            else if (LogisticsHealth > 0.78f) Bottleneck = "Logistics healthy; push endgame route.";
            else Bottleneck = "Moderate logistics; improve worker mix and colonization network.";
        }

        private void ApplyLogisticsEffects()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            float stressDelta = 0f;
            if (LogisticsHealth < 0.34f) stressDelta = 1.8f;
            else if (LogisticsHealth < 0.52f) stressDelta = 0.55f;
            else if (LogisticsHealth > 0.76f) stressDelta = -0.8f;

            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                c.Stats.stress = Mathf.Clamp(c.Stats.stress + stressDelta, 0f, 100f);
                if (LogisticsHealth > 0.72f && c.Stats.currentHp < c.Stats.maxHp && gm.MotherCell.Resources.atp > 8f && gm.MotherCell.Resources.lipids > 4f)
                {
                    float repair = 0.45f;
                    c.Stats.currentHp = Mathf.Min(c.Stats.maxHp, c.Stats.currentHp + repair);
                    gm.MotherCell.Resources.atp -= 0.08f;
                    gm.MotherCell.Resources.lipids -= 0.04f;
                }
            }

            if (AutoStabilize && LogisticsHealth < 0.42f) SoftStabilizeColony();
        }

        private void SoftStabilizeColony()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            if (gm.Environment != null && gm.MotherCell.Resources.atp > 12f)
            {
                gm.Environment.ModifyArea(gm.MotherCell.transform.position, 5.5f, 0.025f, 0f, 0.025f, -0.035f, 0f, 0.015f, 0.015f);
                gm.MotherCell.Resources.atp -= 1.5f;
                LastAction = "Auto-stabilized mother area due low logistics.";
            }
        }

        private void AddOrderAtMouse(V5WorkOrderType type)
        {
            Vector2 world = MouseWorldPosition();
            AddOrder(type, world);
        }

        private void AddOrder(V5WorkOrderType type, Vector2 position)
        {
            WorkOrder order = new WorkOrder();
            order.type = type;
            order.position = position;
            order.status = "Queued.";
            orders.Add(order);
            LastAction = "Created " + type + " order.";
            Toast(LastAction);
        }

        private Vector2 MouseWorldPosition()
        {
            Camera cam = Camera.main;
            if (cam == null) return V5GameManager.Instance != null && V5GameManager.Instance.MotherCell != null ? (Vector2)V5GameManager.Instance.MotherCell.transform.position : Vector2.zero;
            Vector3 wp = cam.ScreenToWorldPoint(Input.mousePosition);
            return new Vector2(wp.x, wp.y);
        }

        private void CreateRecommendedOrder()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            Vector2 p = gm.MotherCell.transform.position;
            if (Bottleneck.Contains("resource-starved") || Bottleneck.Contains("No farmers")) AddOrder(V5WorkOrderType.Harvest, p + UnityEngine.Random.insideUnitCircle.normalized * 12f);
            else if (Bottleneck.Contains("Toxicity")) AddOrder(V5WorkOrderType.Detox, p);
            else if (Bottleneck.Contains("stretched") || Bottleneck.Contains("District")) AddOrder(V5WorkOrderType.Expand, p + UnityEngine.Random.insideUnitCircle.normalized * 16f);
            else if (gm.NonPlayerCells.Count > 0) AddOrder(V5WorkOrderType.Defend, p);
            else AddOrder(V5WorkOrderType.Colonize, p + UnityEngine.Random.insideUnitCircle.normalized * 10f);
        }

        private void UpdateOrders()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            for (int i = 0; i < orders.Count; i++)
            {
                WorkOrder o = orders[i];
                if (o.complete) continue;
                o.assignedCount = CountAssigned(o);
                o.progress = Mathf.Max(o.progress, EvaluateOrderProgress(gm, o));
                if (o.progress >= 1f)
                {
                    o.complete = true;
                    o.status = "Complete.";
                    LastAction = o.type + " order completed.";
                    Toast(LastAction);
                }
                else
                {
                    o.status = BuildOrderStatus(o);
                }
            }
        }

        private int CountAssigned(WorkOrder o)
        {
            int count = 0;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                if (Vector2.Distance(c.DirectiveTarget, o.position) < 2.2f) count++;
            }
            return count;
        }

        private float EvaluateOrderProgress(V5GameManager gm, WorkOrder o)
        {
            V5EnvironmentGrid env = gm.Environment;
            if (env == null) return o.progress;
            int tx, ty;
            env.WorldToTile(o.position, out tx, out ty);
            float progress = o.progress;
            switch (o.type)
            {
                case V5WorkOrderType.Harvest:
                    progress = Mathf.Max(progress, Mathf.Clamp01(o.assignedCount / 2f));
                    if (gm.MotherCell != null && gm.MotherCell.Resources.biomass > 80f) progress = Mathf.Max(progress, 1f);
                    break;
                case V5WorkOrderType.Colonize:
                case V5WorkOrderType.Expand:
                    progress = Mathf.Max(progress, env.colonization[tx, ty] / 0.26f);
                    break;
                case V5WorkOrderType.Detox:
                    progress = Mathf.Max(progress, 1f - Mathf.Clamp01(env.toxins[tx, ty] / 0.22f));
                    break;
                case V5WorkOrderType.Defend:
                    progress = Mathf.Max(progress, Mathf.Clamp01(CountFriendlyNear(o.position, 5f, V5Directive.Defend) / 2f));
                    break;
                case V5WorkOrderType.Hunt:
                    progress = Mathf.Max(progress, gm.NonPlayerCells.Count == 0 ? 1f : Mathf.Clamp01(1f - NearestEnemyDistance(o.position) / 18f));
                    break;
            }
            return Mathf.Clamp01(progress);
        }

        private string BuildOrderStatus(WorkOrder o)
        {
            if (o.assignedCount == 0) return "Waiting for worker assignment.";
            return "Active. Progress depends on local ecology and assigned cells.";
        }

        private int CountFriendlyNear(Vector2 pos, float radius, V5Directive directive)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return 0;
            int count = 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                if (c.Directive == directive && Vector2.Distance(pos, c.transform.position) <= radius) count++;
            }
            return count;
        }

        private float NearestEnemyDistance(Vector2 pos)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.NonPlayerCells.Count == 0) return 999f;
            float best = 999f;
            IReadOnlyList<V5CellEntity> enemies = gm.NonPlayerCells;
            for (int i = 0; i < enemies.Count; i++)
            {
                V5CellEntity e = enemies[i];
                if (e == null) continue;
                best = Mathf.Min(best, Vector2.Distance(pos, e.transform.position));
            }
            return best;
        }

        private void AssignWorkersToOrders()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            for (int i = 0; i < orders.Count; i++)
            {
                WorkOrder o = orders[i];
                if (o.complete) continue;
                int targetWorkers = TargetWorkers(o.type);
                int assigned = CountAssigned(o);
                while (assigned < targetWorkers)
                {
                    V5CellEntity worker = FindBestWorker(o);
                    if (worker == null) break;
                    ApplyOrderToCell(worker, o);
                    assigned++;
                }
            }
        }

        private int TargetWorkers(V5WorkOrderType type)
        {
            switch (type)
            {
                case V5WorkOrderType.Harvest: return 2;
                case V5WorkOrderType.Colonize: return 2;
                case V5WorkOrderType.Defend: return 2;
                case V5WorkOrderType.Detox: return 2;
                case V5WorkOrderType.Hunt: return 3;
                case V5WorkOrderType.Expand: return 3;
                default: return 1;
            }
        }

        private V5CellEntity FindBestWorker(WorkOrder order)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return null;
            V5CellEntity best = null;
            float bestScore = 99999f;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c.Role == V5CellRole.Mother || c.Role == V5CellRole.Apex) continue;
                if (Vector2.Distance(c.DirectiveTarget, order.position) < 2.2f) continue;
                float score = Vector2.Distance(c.transform.position, order.position);
                score += c.CarryAmount > 0.01f ? 8f : 0f;
                score += c.Stats.currentHp < c.Stats.maxHp * 0.35f ? 18f : 0f;
                score -= RoleAffinity(c, order.type) * 6f;
                if (score < bestScore)
                {
                    bestScore = score;
                    best = c;
                }
            }
            return best;
        }

        private float RoleAffinity(V5CellEntity c, V5WorkOrderType type)
        {
            if (type == V5WorkOrderType.Harvest && c.LineageRole == V5LineageRole.Farmer) return 1f;
            if ((type == V5WorkOrderType.Colonize || type == V5WorkOrderType.Expand) && c.LineageRole == V5LineageRole.Colonizer) return 1f;
            if (type == V5WorkOrderType.Defend && c.LineageRole == V5LineageRole.Defender) return 1f;
            if (type == V5WorkOrderType.Hunt && c.LineageRole == V5LineageRole.Predator) return 1f;
            if (type == V5WorkOrderType.Detox && c.LineageRole == V5LineageRole.Recycler) return 1f;
            return 0f;
        }

        private void ApplyOrderToCell(V5CellEntity cell, WorkOrder order)
        {
            switch (order.type)
            {
                case V5WorkOrderType.Harvest:
                    cell.Directive = V5Directive.Farm;
                    break;
                case V5WorkOrderType.Colonize:
                case V5WorkOrderType.Expand:
                    cell.Directive = V5Directive.Colonize;
                    cell.DirectiveTarget = order.position;
                    break;
                case V5WorkOrderType.Defend:
                    cell.Directive = V5Directive.Defend;
                    cell.DirectiveTarget = order.position;
                    break;
                case V5WorkOrderType.Detox:
                    cell.Directive = V5Directive.Colonize;
                    cell.DirectiveTarget = order.position;
                    if (cell.HasStructure(V5StructureId.Catalase)) cell.Stats.toxinResistance = Mathf.Max(cell.Stats.toxinResistance, 0.45f);
                    break;
                case V5WorkOrderType.Hunt:
                    cell.Directive = V5Directive.Attack;
                    cell.DirectiveTarget = order.position;
                    break;
            }
            LastAction = "Assigned " + cell.name + " to " + order.type + ".";
        }

        private void BalanceResourceFlow()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity m = gm.MotherCell;
            int supported = 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c == m) continue;
                if (Vector2.Distance(c.transform.position, m.transform.position) > 16f && LogisticsHealth < 0.55f) continue;
                if (c.Resources.atp < 10f && m.Resources.atp > 20f)
                {
                    float give = Mathf.Min(8f, m.Resources.atp - 18f);
                    m.Resources.atp -= give;
                    c.Resources.atp += give;
                    supported++;
                }
                if (c.Resources.biomass < 6f && m.Resources.biomass > 28f)
                {
                    float give = Mathf.Min(4f, m.Resources.biomass - 24f);
                    m.Resources.biomass -= give;
                    c.Resources.biomass += give;
                }
            }
            LastAction = "Balanced resource flow to " + supported + " cells.";
            Toast(LastAction);
        }

        private void ExportSnapshot()
        {
            V5OperationsSnapshot s = new V5OperationsSnapshot();
            s.logisticsHealth = LogisticsHealth;
            s.nutrientFlow = NutrientFlow;
            s.networkLoad = NetworkLoad;
            s.bottleneck = Bottleneck;
            for (int i = 0; i < orders.Count; i++)
            {
                WorkOrder o = orders[i];
                V5WorkOrderSnapshot e = new V5WorkOrderSnapshot();
                e.type = o.type.ToString();
                e.x = o.position.x;
                e.y = o.position.y;
                e.progress = o.progress;
                e.complete = o.complete;
                s.orders.Add(e);
            }
            File.WriteAllText(ExportPath, JsonUtility.ToJson(s, true));
            LastAction = "Exported operations snapshot to " + ExportPath;
            Toast("Operations snapshot exported.");
        }

        private void ClearCompleted()
        {
            orders.RemoveAll(o => o.complete);
            LastAction = "Cleared completed work orders.";
        }

        private void Toast(string msg)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(msg);
        }

        private class WorkOrder
        {
            public V5WorkOrderType type;
            public Vector2 position;
            public float progress;
            public bool complete;
            public int assignedCount;
            public string status;
        }
    }
}
