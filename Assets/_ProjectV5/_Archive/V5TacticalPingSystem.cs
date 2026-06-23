using System;
using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5TacticalPingType
    {
        Rally,
        Harvest,
        Expand,
        FallBack,
        AttackThreat,
        DefendMother
    }

    [Serializable]
    public class V5TacticalPing
    {
        public V5TacticalPingType type;
        public Vector2 position;
        public float radius;
        public float timeLeft;
        public int affected;
    }

    /// <summary>
    /// V5 2.2 macro command layer. Pings let the player steer groups by intent
    /// without selecting individual cells: rally, harvest, expand, fall back,
    /// attack nearby threats, or defend mother.
    /// </summary>
    public class V5TacticalPingSystem : MonoBehaviour
    {
        public string LastPing { get; private set; }
        public bool DrawPings = true;

        private readonly List<V5TacticalPing> pings = new List<V5TacticalPing>(12);
        private bool showPanel;
        private V5TacticalPingType selectedType = V5TacticalPingType.Rally;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle small;
        private GUIStyle button;

        private void Awake()
        {
            LastPing = "No tactical pings yet.";
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.KeypadDivide)) showPanel = !showPanel;
            bool shift = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (shift && Input.GetKeyDown(KeyCode.Keypad7)) IssuePingAtMouse(V5TacticalPingType.Rally);
            if (shift && Input.GetKeyDown(KeyCode.Keypad8)) IssuePingAtMouse(V5TacticalPingType.Harvest);
            if (shift && Input.GetKeyDown(KeyCode.Keypad9)) IssuePingAtMouse(V5TacticalPingType.Expand);
            if (shift && Input.GetKeyDown(KeyCode.Keypad4)) IssuePingAtMouse(V5TacticalPingType.FallBack);
            if (shift && Input.GetKeyDown(KeyCode.Keypad5)) IssuePingAtMouse(V5TacticalPingType.AttackThreat);
            if (shift && Input.GetKeyDown(KeyCode.Keypad6)) IssuePingAtMouse(V5TacticalPingType.DefendMother);

            for (int i = pings.Count - 1; i >= 0; i--)
            {
                pings[i].timeLeft -= Time.deltaTime;
                if (pings[i].timeLeft <= 0f) pings.RemoveAt(i);
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            Rect r = new Rect(18, 196, 420, 280);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "TACTICAL PINGS 2.2 — MACRO INTENT", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 42), LastPing + "\nHotkeys: Keypad/ panel | Shift+7 rally | Shift+8 harvest | Shift+9 expand | Shift+4 fallback | Shift+5 attack | Shift+6 defend", small);

            float y = r.y + 88;
            DrawTypeButton(r.x + 12, y, 126, V5TacticalPingType.Rally);
            DrawTypeButton(r.x + 146, y, 126, V5TacticalPingType.Harvest);
            DrawTypeButton(r.x + 280, y, 126, V5TacticalPingType.Expand);
            y += 34;
            DrawTypeButton(r.x + 12, y, 126, V5TacticalPingType.FallBack);
            DrawTypeButton(r.x + 146, y, 126, V5TacticalPingType.AttackThreat);
            DrawTypeButton(r.x + 280, y, 126, V5TacticalPingType.DefendMother);
            y += 42;

            if (GUI.Button(new Rect(r.x + 12, y, 170, 30), "Issue at mouse", button)) IssuePingAtMouse(selectedType);
            DrawPings = GUI.Toggle(new Rect(r.x + 200, y + 4, 120, 22), DrawPings, "Draw pings");
            if (GUI.Button(new Rect(r.x + 320, y, 86, 30), "Clear", button)) pings.Clear();
            y += 42;

            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 76), "Pings are intentionally broad. They operate on nearby player cells first, then fall back to all non-mother cells if no local workers are available. This helps test RTS macro before final selection UX.", small);
        }

        private void OnDrawGizmos()
        {
            if (!DrawPings || pings == null) return;
            for (int i = 0; i < pings.Count; i++)
            {
                V5TacticalPing p = pings[i];
                Color c = ColorForPing(p.type);
                c.a = Mathf.Clamp01(p.timeLeft / 8f) * 0.55f;
                Gizmos.color = c;
                Gizmos.DrawWireSphere(p.position, p.radius);
            }
        }

        private void IssuePingAtMouse(V5TacticalPingType type)
        {
            Vector2 world = MouseWorld();
            IssuePing(type, world);
        }

        public void IssuePing(V5TacticalPingType type, Vector2 world)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            float radius = RadiusFor(type);
            int affected = ApplyPing(type, world, radius);
            V5TacticalPing ping = new V5TacticalPing();
            ping.type = type;
            ping.position = world;
            ping.radius = radius;
            ping.timeLeft = 8f;
            ping.affected = affected;
            pings.Add(ping);
            LastPing = type + " ping affected " + affected + " cells.";
            if (gm.Hud != null) gm.Hud.Toast(LastPing);
        }

        private int ApplyPing(V5TacticalPingType type, Vector2 world, float radius)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            int affected = 0;
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    V5CellEntity c = cells[i];
                    if (c == null || c.Role == V5CellRole.Mother) continue;
                    bool inRadius = Vector2.Distance(c.transform.position, world) <= radius;
                    if (pass == 0 && !inRadius) continue;
                    if (pass == 1 && affected > 0) continue;
                    ApplyPingToCell(c, type, world);
                    affected++;
                    if (affected >= 10) return affected;
                }
            }
            return affected;
        }

        private void ApplyPingToCell(V5CellEntity c, V5TacticalPingType type, Vector2 world)
        {
            V5GameManager gm = V5GameManager.Instance;
            switch (type)
            {
                case V5TacticalPingType.Rally:
                    c.Directive = V5Directive.Move;
                    c.DirectiveTarget = world + UnityEngine.Random.insideUnitCircle * 1.4f;
                    break;
                case V5TacticalPingType.Harvest:
                    c.Directive = V5Directive.Farm;
                    c.LineageRole = V5LineageRole.Farmer;
                    c.DirectiveTarget = world;
                    break;
                case V5TacticalPingType.Expand:
                    c.Directive = V5Directive.Colonize;
                    c.LineageRole = V5LineageRole.Colonizer;
                    c.DirectiveTarget = world + UnityEngine.Random.insideUnitCircle * 2.0f;
                    break;
                case V5TacticalPingType.FallBack:
                    c.Directive = V5Directive.FollowMother;
                    c.DirectiveTarget = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : world;
                    break;
                case V5TacticalPingType.AttackThreat:
                    c.Directive = V5Directive.Attack;
                    c.LineageRole = V5LineageRole.Predator;
                    c.DirectiveTarget = world;
                    c.AttackTarget = FindNearestEnemy(world, 10f);
                    break;
                case V5TacticalPingType.DefendMother:
                    c.Directive = V5Directive.Defend;
                    c.LineageRole = V5LineageRole.Defender;
                    c.DirectiveTarget = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : world;
                    break;
            }
        }

        private V5CellEntity FindNearestEnemy(Vector2 world, float maxDist)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return null;
            V5CellEntity best = null;
            float bestD = maxDist;
            IReadOnlyList<V5CellEntity> enemies = gm.NonPlayerCells;
            for (int i = 0; i < enemies.Count; i++)
            {
                V5CellEntity e = enemies[i];
                if (e == null) continue;
                float d = Vector2.Distance(world, e.transform.position);
                if (d < bestD)
                {
                    bestD = d;
                    best = e;
                }
            }
            return best;
        }

        private float RadiusFor(V5TacticalPingType type)
        {
            switch (type)
            {
                case V5TacticalPingType.FallBack: return 99f;
                case V5TacticalPingType.DefendMother: return 99f;
                case V5TacticalPingType.AttackThreat: return 16f;
                default: return 12f;
            }
        }

        private Color ColorForPing(V5TacticalPingType type)
        {
            switch (type)
            {
                case V5TacticalPingType.Harvest: return new Color(0.55f, 1f, 0.55f, 1f);
                case V5TacticalPingType.Expand: return new Color(0.35f, 0.8f, 1f, 1f);
                case V5TacticalPingType.FallBack: return new Color(1f, 0.9f, 0.3f, 1f);
                case V5TacticalPingType.AttackThreat: return new Color(1f, 0.25f, 0.25f, 1f);
                case V5TacticalPingType.DefendMother: return new Color(1f, 0.65f, 0.2f, 1f);
                default: return new Color(0.8f, 0.8f, 1f, 1f);
            }
        }

        private void DrawTypeButton(float x, float y, float w, V5TacticalPingType type)
        {
            if (selectedType == type) GUI.color = Color.Lerp(Color.white, ColorForPing(type), 0.35f);
            if (GUI.Button(new Rect(x, y, w, 28), type.ToString(), button)) selectedType = type;
            GUI.color = Color.white;
        }

        private Vector2 MouseWorld()
        {
            Camera cam = Camera.main;
            if (cam == null) return Vector2.zero;
            Vector3 p = Input.mousePosition;
            p.z = -cam.transform.position.z;
            return cam.ScreenToWorldPoint(p);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.fontSize = 11; box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 14; title.normal.textColor = new Color(0.92f, 0.94f, 1f, 1f);
            small = new GUIStyle(GUI.skin.label); small.fontSize = 11; small.wordWrap = true; small.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button); button.fontSize = 10; button.wordWrap = true;
        }
    }
}
