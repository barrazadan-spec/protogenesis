using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5SupplyRelayType
    {
        MetabolicHub,
        DetoxFilter,
        DefensiveMatrix,
        NurseryNode,
        PhoticCollector
    }

    [Serializable]
    public class V5SupplyRelaySnapshot
    {
        public string type;
        public float x;
        public float y;
        public float integrity;
        public float radius;
        public float throughput;
    }

    [Serializable]
    public class V5SupplyNetworkSnapshot
    {
        public string version = "2.2";
        public float coverage;
        public float throughput;
        public float stability;
        public string bottleneck;
        public List<V5SupplyRelaySnapshot> relays = new List<V5SupplyRelaySnapshot>();
    }

    [Serializable]
    public class V5SupplyRelay
    {
        public V5SupplyRelayType type;
        public Vector2 position;
        public float integrity = 1f;
        public float radius = 6f;
        public float throughput = 1f;
        public bool active = true;

        public string Label
        {
            get { return type + " @ " + position.x.ToString("0.0") + "," + position.y.ToString("0.0"); }
        }
    }

    /// <summary>
    /// V5 2.2. Adds a real world-builder infrastructure layer: supply relays.
    /// Relays create a soft logistics network that improves colony stability,
    /// detoxifies/fortifies zones and makes expansion easier without needing
    /// to micro every cell.
    /// </summary>
    public class V5SupplyNetworkSystem : MonoBehaviour
    {
        public float Coverage { get; private set; }
        public float Throughput { get; private set; }
        public float Stability { get; private set; }
        public string Bottleneck { get; private set; }
        public string LastAction { get; private set; }
        public bool AutoMaintain = true;
        public bool DrawNetwork = true;

        private readonly List<V5SupplyRelay> relays = new List<V5SupplyRelay>(24);
        private V5SupplyRelayType selectedType = V5SupplyRelayType.MetabolicHub;
        private bool showPanel;
        private Vector2 scroll;
        private float tickTimer;
        private float effectTimer;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle small;
        private GUIStyle button;

        private string ExportPath
        {
            get { return Path.Combine(Application.persistentDataPath, "protogenesis_v5_supply_network_2_2.json"); }
        }

        private void Awake()
        {
            Bottleneck = "Network not evaluated yet.";
            LastAction = "Supply Network online.";
        }

        private void Update()
        {
            // Keypad+ removed — RuntimeSettings uses it for speed; open via Paneles menu
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
            bool alt = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (ctrl && Input.GetKeyDown(KeyCode.KeypadPlus)) BuildRelayAtMouse(selectedType);
            if (alt && Input.GetKeyDown(KeyCode.KeypadPlus)) RemoveNearestRelayAtMouse();

            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.65f)
            {
                tickTimer = 0f;
                EvaluateNetwork();
                if (AutoMaintain) MaintainDamagedRelays();
            }

            effectTimer += Time.deltaTime;
            if (effectTimer >= 1.15f)
            {
                effectTimer = 0f;
                ApplyRelayEffects();
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();

            Rect r = new Rect(Screen.width - 590, 132, 570, Screen.height - 210);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "SUPPLY NETWORK 2.2 — COLONY INFRASTRUCTURE", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 38), string.Format("Coverage {0:0}% | Throughput {1:0}% | Stability {2:0}% | Relays {3}", Coverage * 100f, Throughput * 100f, Stability * 100f, relays.Count) + "\nBottleneck: " + Bottleneck, small);

            float y = r.y + 82;
            AutoMaintain = GUI.Toggle(new Rect(r.x + 12, y, 130, 22), AutoMaintain, "Auto-maintain");
            DrawNetwork = GUI.Toggle(new Rect(r.x + 148, y, 120, 22), DrawNetwork, "Draw network");
            if (GUI.Button(new Rect(r.x + 282, y - 2, 88, 26), "Export", button)) ExportSnapshot();
            if (GUI.Button(new Rect(r.x + 376, y - 2, 84, 26), "Optimize", button)) OptimizeRelayRoles();
            if (GUI.Button(new Rect(r.x + 466, y - 2, 84, 26), "Clear", button)) ClearRelays();
            y += 34;

            GUI.Label(new Rect(r.x + 12, y, 240, 20), "Relay type:", small);
            y += 22;
            DrawRelayTypeButton(r.x + 12, y, 104, V5SupplyRelayType.MetabolicHub);
            DrawRelayTypeButton(r.x + 120, y, 104, V5SupplyRelayType.DetoxFilter);
            DrawRelayTypeButton(r.x + 228, y, 104, V5SupplyRelayType.DefensiveMatrix);
            DrawRelayTypeButton(r.x + 336, y, 104, V5SupplyRelayType.NurseryNode);
            DrawRelayTypeButton(r.x + 444, y, 104, V5SupplyRelayType.PhoticCollector);
            y += 36;

            if (GUI.Button(new Rect(r.x + 12, y, 170, 28), "Build at mouse", button)) BuildRelayAtMouse(selectedType);
            if (GUI.Button(new Rect(r.x + 188, y, 170, 28), "Remove nearest", button)) RemoveNearestRelayAtMouse();
            if (GUI.Button(new Rect(r.x + 364, y, 186, 28), "Send workers to repair", button)) SendWorkersToRepairNetwork();
            y += 40;

            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 36), LastAction + "\nHotkeys: Keypad+ panel | Ctrl+Keypad+ build | Alt+Keypad+ remove nearest.", small);
            y += 44;

            Rect view = new Rect(r.x + 12, y, r.width - 24, r.height - (y - r.y) - 12);
            Rect content = new Rect(0, 0, view.width - 18, Mathf.Max(260, relays.Count * 72 + 8));
            scroll = GUI.BeginScrollView(view, scroll, content);
            float cy = 0f;
            if (relays.Count == 0)
            {
                GUI.Label(new Rect(0, cy, content.width, 82), "No relays yet. Build a Metabolic Hub near your mother, then push Detox/Nursery/Defense relays into colonized territory. Relays are data-only infrastructure, so they are cheap to iterate before final art.", small);
            }
            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay relay = relays[i];
                GUI.Box(new Rect(0, cy, content.width, 64), "", box);
                GUI.Label(new Rect(8, cy + 6, content.width - 118, 18), string.Format("#{0} {1} | integrity {2:0}% | radius {3:0.0} | throughput {4:0.0}", i + 1, relay.type, relay.integrity * 100f, relay.radius, relay.throughput), small);
                GUI.Label(new Rect(8, cy + 26, content.width - 118, 30), RelayEffectText(relay.type), small);
                if (GUI.Button(new Rect(content.width - 102, cy + 18, 92, 28), "Remove", button))
                {
                    RefundRelay(relay, 0.35f);
                    relays.RemoveAt(i);
                    LastAction = "Removed relay #" + (i + 1);
                    i--;
                    cy += 68;
                    continue;
                }
                cy += 68;
            }
            GUI.EndScrollView();
        }

        private void OnDrawGizmos()
        {
            if (!DrawNetwork || relays == null || relays.Count == 0) return;
            V5GameManager gm = V5GameManager.Instance;
            Vector3 origin = gm != null && gm.MotherCell != null ? gm.MotherCell.transform.position : Vector3.zero;
            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                Color c = ColorForRelay(r.type);
                c.a = 0.35f;
                Gizmos.color = c;
                Gizmos.DrawWireSphere(r.position, r.radius);
                Gizmos.DrawLine(origin, r.position);
            }
        }

        public bool IsCovered(Vector2 world)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.MotherCell != null && Vector2.Distance(world, gm.MotherCell.transform.position) < 8f) return true;
            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                if (!r.active) continue;
                if (Vector2.Distance(world, r.position) <= r.radius * Mathf.Lerp(0.65f, 1f, r.integrity)) return true;
            }
            return false;
        }

        public float SupportMultiplierAt(Vector2 world)
        {
            float support = 1f;
            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                if (!r.active) continue;
                float d = Vector2.Distance(world, r.position);
                if (d <= r.radius) support += (1f - d / Mathf.Max(0.01f, r.radius)) * 0.08f * r.integrity;
            }
            return Mathf.Clamp(support, 1f, 1.45f);
        }

        private void EvaluateNetwork()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null)
            {
                Coverage = 0f;
                Throughput = 0f;
                Stability = 0f;
                Bottleneck = "No mother cell.";
                return;
            }

            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            int covered = 0;
            int farCells = 0;
            float integrity = 0f;
            float totalThroughput = 0f;
            Vector2 motherPos = gm.MotherCell.transform.position;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                if (IsCovered(c.transform.position)) covered++;
                if (Vector2.Distance(motherPos, c.transform.position) > 18f) farCells++;
            }

            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                if (!r.active) continue;
                integrity += r.integrity;
                totalThroughput += r.throughput * Mathf.Clamp01(r.integrity);
                float localTox = gm.Environment != null ? gm.Environment.Sample(V5OverlayMode.Toxins, r.position) : 0f;
                r.integrity = Mathf.Clamp01(r.integrity - localTox * 0.004f);
            }

            Coverage = cells.Count > 0 ? covered / (float)cells.Count : 0f;
            Throughput = Mathf.Clamp01((totalThroughput + 1f) / Mathf.Max(2f, cells.Count * 0.55f));
            Stability = relays.Count > 0 ? Mathf.Clamp01((integrity / relays.Count) * 0.65f + Coverage * 0.25f + Throughput * 0.10f) : Mathf.Clamp01(Coverage * 0.5f);

            if (relays.Count == 0) Bottleneck = "No relay infrastructure. Expansion depends on manual micro.";
            else if (Coverage < 0.45f) Bottleneck = "Coverage gap: too many cells outside support radius.";
            else if (Throughput < 0.45f) Bottleneck = "Throughput low: build metabolic/nursery relays or reduce overexpansion.";
            else if (Stability < 0.55f) Bottleneck = "Relay integrity low: detox or repair network.";
            else if (farCells > cells.Count * 0.45f) Bottleneck = "Long-distance expansion: add forward relays.";
            else Bottleneck = "Network stable.";
        }

        private void ApplyRelayEffects()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null || gm.MotherCell == null) return;

            V5CellEntity mother = gm.MotherCell;
            float upkeep = relays.Count * 0.08f;
            if (mother.Resources.atp >= upkeep) mother.Resources.atp -= upkeep;
            else
            {
                for (int i = 0; i < relays.Count; i++) relays[i].integrity = Mathf.Clamp01(relays[i].integrity - 0.01f);
            }

            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                if (!r.active || r.integrity <= 0.05f) continue;
                float k = r.integrity * r.throughput;
                switch (r.type)
                {
                    case V5SupplyRelayType.MetabolicHub:
                        gm.Environment.ModifyArea(r.position, r.radius, -0.001f * k, 0f, 0.002f * k, -0.001f * k, 0f, 0.002f * k, 0f);
                        mother.Resources.atp += 0.06f * k;
                        break;
                    case V5SupplyRelayType.DetoxFilter:
                        gm.Environment.ModifyArea(r.position, r.radius, 0f, 0f, 0.001f * k, -0.010f * k, -0.001f * k, 0.001f * k, 0f);
                        break;
                    case V5SupplyRelayType.DefensiveMatrix:
                        gm.Environment.ModifyArea(r.position, r.radius, 0f, 0f, 0f, -0.002f * k, 0f, 0.003f * k, 0f);
                        ReinforceCellsInRelay(r, 0.10f * k, 0.18f * k);
                        break;
                    case V5SupplyRelayType.NurseryNode:
                        gm.Environment.ModifyArea(r.position, r.radius, -0.002f * k, 0f, 0.001f * k, -0.001f * k, 0f, 0.008f * k, 0.001f * k);
                        ReinforceCellsInRelay(r, 0.04f * k, 0.32f * k);
                        break;
                    case V5SupplyRelayType.PhoticCollector:
                        gm.Environment.ModifyArea(r.position, r.radius, 0f, 0.006f * k, 0.006f * k, 0f, 0f, 0.002f * k, 0f);
                        mother.Resources.atp += 0.04f * k;
                        break;
                }
            }
        }

        private void ReinforceCellsInRelay(V5SupplyRelay relay, float hpPerTick, float stressReduction)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                if (Vector2.Distance(c.transform.position, relay.position) > relay.radius) continue;
                c.Stats.currentHp = Mathf.Min(c.Stats.maxHp, c.Stats.currentHp + hpPerTick);
                c.Stats.stress = Mathf.Max(0f, c.Stats.stress - stressReduction);
            }
        }

        private void BuildRelayAtMouse(V5SupplyRelayType type)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            Vector2 world = MouseWorld();
            if (gm.Environment != null && world.magnitude > gm.Environment.MapRadius)
            {
                LastAction = "Cannot build outside the drop.";
                Toast(LastAction);
                return;
            }
            V5ResourceWallet cost = RelayCost(type);
            if (!gm.MotherCell.Resources.CanPay(cost))
            {
                LastAction = "Not enough resources for " + type + ": " + CostText(cost);
                Toast(LastAction);
                return;
            }
            if (!IsCovered(world) && relays.Count > 0)
            {
                LastAction = "Relay must be inside existing support coverage or near mother.";
                Toast(LastAction);
                return;
            }
            gm.MotherCell.Resources.Pay(cost);
            V5SupplyRelay relay = new V5SupplyRelay();
            relay.type = type;
            relay.position = world;
            relay.radius = RelayRadius(type);
            relay.throughput = RelayThroughput(type);
            relay.integrity = 1f;
            relays.Add(relay);
            LastAction = "Built " + type + " at " + world.x.ToString("0.0") + "," + world.y.ToString("0.0");
            Toast(LastAction);
            if (gm.Environment != null) gm.Environment.ModifyArea(world, relay.radius * 0.7f, 0f, 0f, 0f, -0.025f, 0f, 0.03f, 0f);
        }

        private void RemoveNearestRelayAtMouse()
        {
            if (relays.Count == 0) return;
            Vector2 world = MouseWorld();
            int bestIndex = -1;
            float best = 99999f;
            for (int i = 0; i < relays.Count; i++)
            {
                float d = Vector2.Distance(world, relays[i].position);
                if (d < best)
                {
                    best = d;
                    bestIndex = i;
                }
            }
            if (bestIndex >= 0 && best <= 8f)
            {
                RefundRelay(relays[bestIndex], 0.45f);
                LastAction = "Removed nearest relay: " + relays[bestIndex].type;
                relays.RemoveAt(bestIndex);
                Toast(LastAction);
            }
            else
            {
                LastAction = "No relay near mouse.";
                Toast(LastAction);
            }
        }

        private void RefundRelay(V5SupplyRelay relay, float factor)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5ResourceWallet c = RelayCost(relay.type);
            gm.MotherCell.Resources.atp += c.atp * factor;
            gm.MotherCell.Resources.biomass += c.biomass * factor;
            gm.MotherCell.Resources.aminoAcids += c.aminoAcids * factor;
            gm.MotherCell.Resources.lipids += c.lipids * factor;
            gm.MotherCell.Resources.nucleotides += c.nucleotides * factor;
            gm.MotherCell.Resources.minerals += c.minerals * factor;
        }

        private void MaintainDamagedRelays()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || relays.Count == 0) return;
            V5CellEntity mother = gm.MotherCell;
            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                if (r.integrity >= 0.82f) continue;
                float cost = 0.035f;
                if (mother.Resources.biomass >= cost && mother.Resources.atp >= cost * 2f)
                {
                    mother.Resources.biomass -= cost;
                    mother.Resources.atp -= cost * 2f;
                    r.integrity = Mathf.Clamp01(r.integrity + 0.012f);
                }
            }
        }

        private void SendWorkersToRepairNetwork()
        {
            if (relays.Count == 0) return;
            V5SupplyRelay target = null;
            float worst = 2f;
            for (int i = 0; i < relays.Count; i++)
            {
                if (relays[i].integrity < worst)
                {
                    worst = relays[i].integrity;
                    target = relays[i];
                }
            }
            if (target == null) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            int sent = 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count && sent < 5; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (c.LineageRole == V5LineageRole.Farmer || c.LineageRole == V5LineageRole.Defender || c.Directive == V5Directive.Idle)
                {
                    c.Directive = V5Directive.Move;
                    c.DirectiveTarget = target.position + UnityEngine.Random.insideUnitCircle * 1.2f;
                    sent++;
                }
            }
            LastAction = "Sent " + sent + " workers to repair " + target.type;
            Toast(LastAction);
        }

        private void OptimizeRelayRoles()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return;
            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                float toxins = gm.Environment.Sample(V5OverlayMode.Toxins, r.position);
                float light = gm.Environment.Sample(V5OverlayMode.Light, r.position);
                float colonization = gm.Environment.Sample(V5OverlayMode.Colonization, r.position);
                if (toxins > 0.42f) r.type = V5SupplyRelayType.DetoxFilter;
                else if (light > 0.62f) r.type = V5SupplyRelayType.PhoticCollector;
                else if (colonization > 0.45f) r.type = V5SupplyRelayType.NurseryNode;
                else r.type = V5SupplyRelayType.MetabolicHub;
                r.radius = RelayRadius(r.type);
                r.throughput = RelayThroughput(r.type);
            }
            LastAction = "Optimized relay roles from local ecology.";
            Toast(LastAction);
        }

        private void ClearRelays()
        {
            for (int i = 0; i < relays.Count; i++) RefundRelay(relays[i], 0.25f);
            relays.Clear();
            LastAction = "Cleared supply network.";
            Toast(LastAction);
        }

        private V5ResourceWallet RelayCost(V5SupplyRelayType type)
        {
            switch (type)
            {
                case V5SupplyRelayType.DetoxFilter: return V5ResourceWallet.Cost(16, 8, 4, 4, 0, 3);
                case V5SupplyRelayType.DefensiveMatrix: return V5ResourceWallet.Cost(18, 10, 3, 6, 0, 4);
                case V5SupplyRelayType.NurseryNode: return V5ResourceWallet.Cost(20, 12, 7, 5, 2, 2);
                case V5SupplyRelayType.PhoticCollector: return V5ResourceWallet.Cost(18, 9, 4, 5, 1, 3);
                default: return V5ResourceWallet.Cost(14, 8, 4, 3, 0, 2);
            }
        }

        private float RelayRadius(V5SupplyRelayType type)
        {
            switch (type)
            {
                case V5SupplyRelayType.DefensiveMatrix: return 5.2f;
                case V5SupplyRelayType.NurseryNode: return 6.8f;
                case V5SupplyRelayType.PhoticCollector: return 7.2f;
                default: return 6.0f;
            }
        }

        private float RelayThroughput(V5SupplyRelayType type)
        {
            switch (type)
            {
                case V5SupplyRelayType.MetabolicHub: return 1.25f;
                case V5SupplyRelayType.NurseryNode: return 1.15f;
                case V5SupplyRelayType.PhoticCollector: return 1.10f;
                case V5SupplyRelayType.DetoxFilter: return 0.95f;
                case V5SupplyRelayType.DefensiveMatrix: return 0.90f;
                default: return 1f;
            }
        }

        private string RelayEffectText(V5SupplyRelayType type)
        {
            switch (type)
            {
                case V5SupplyRelayType.MetabolicHub: return "Boosts ATP flow and local oxygen; basic logistics backbone.";
                case V5SupplyRelayType.DetoxFilter: return "Reduces toxins/acidity and protects route integrity.";
                case V5SupplyRelayType.DefensiveMatrix: return "Reinforces nearby cells and stabilizes border zones.";
                case V5SupplyRelayType.NurseryNode: return "Improves colonization, stress recovery and safe expansion.";
                case V5SupplyRelayType.PhoticCollector: return "Amplifies local light/oxygen and powers photosynthetic colonies.";
                default: return "Generic supply relay.";
            }
        }

        private Color ColorForRelay(V5SupplyRelayType type)
        {
            switch (type)
            {
                case V5SupplyRelayType.DetoxFilter: return new Color(0.4f, 1f, 0.6f, 1f);
                case V5SupplyRelayType.DefensiveMatrix: return new Color(1f, 0.75f, 0.25f, 1f);
                case V5SupplyRelayType.NurseryNode: return new Color(0.65f, 0.85f, 1f, 1f);
                case V5SupplyRelayType.PhoticCollector: return new Color(0.9f, 1f, 0.3f, 1f);
                default: return new Color(0.65f, 1f, 0.95f, 1f);
            }
        }

        private void DrawRelayTypeButton(float x, float y, float w, V5SupplyRelayType type)
        {
            bool old = GUI.enabled;
            if (selectedType == type) GUI.color = Color.Lerp(Color.white, ColorForRelay(type), 0.35f);
            if (GUI.Button(new Rect(x, y, w, 28), type.ToString(), button)) selectedType = type;
            GUI.color = Color.white;
            GUI.enabled = old;
        }

        private Vector2 MouseWorld()
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector2.zero;
            Vector3 p = Input.mousePosition;
            p.z = -cam.transform.position.z;
            return cam.ScreenToWorldPoint(p);
        }

        private string CostText(V5ResourceWallet c)
        {
            return string.Format("ATP {0:0} Bio {1:0} AA {2:0} Lip {3:0} NT {4:0} Min {5:0}", c.atp, c.biomass, c.aminoAcids, c.lipids, c.nucleotides, c.minerals);
        }

        private void ExportSnapshot()
        {
            V5SupplyNetworkSnapshot s = new V5SupplyNetworkSnapshot();
            s.coverage = Coverage;
            s.throughput = Throughput;
            s.stability = Stability;
            s.bottleneck = Bottleneck;
            for (int i = 0; i < relays.Count; i++)
            {
                V5SupplyRelay r = relays[i];
                V5SupplyRelaySnapshot rs = new V5SupplyRelaySnapshot();
                rs.type = r.type.ToString();
                rs.x = r.position.x;
                rs.y = r.position.y;
                rs.integrity = r.integrity;
                rs.radius = r.radius;
                rs.throughput = r.throughput;
                s.relays.Add(rs);
            }
            try
            {
                File.WriteAllText(ExportPath, JsonUtility.ToJson(s, true));
                LastAction = "Exported supply snapshot: " + ExportPath;
            }
            catch (Exception e)
            {
                LastAction = "Export failed: " + e.Message;
            }
            Toast(LastAction);
        }

        private void Toast(string text)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(text);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.fontSize = 11; box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 14; title.normal.textColor = new Color(0.88f, 1f, 0.98f, 1f);
            small = new GUIStyle(GUI.skin.label); small.fontSize = 11; small.wordWrap = true; small.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button); button.fontSize = 10; button.wordWrap = true;
        }
    }
}
