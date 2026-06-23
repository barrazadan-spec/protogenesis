using UnityEngine;

namespace Protogenesis.V5
{
    public class V5MinimapIMGUI : MonoBehaviour
    {
        public bool Visible;
        public int Size = 178;
        private Texture2D dot;
        private float repaintTimer;

        private void Start()
        {
            Visible = false;
            V5PanelRouter.Register("Minimapa", () => Visible, v => Visible = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.N)) { if (!Visible) V5PanelRouter.CloseOthers("Minimapa"); Visible = !Visible; }
            repaintTimer += Time.unscaledDeltaTime;
        }

        private void OnGUI()
        {
            if (!Visible) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return;
            EnsureTexture();
            Rect r = new Rect(Screen.width - Size - 12, Screen.height - Size - 104, Size, Size);
            GUI.Box(r, "");
            DrawEnvironment(gm, r);
            DrawResourceDots(gm, r);
            DrawCellDots(gm, r);
            DrawRouteCounterDot(gm, r);
            GUI.Label(new Rect(r.x + 8, r.y + 6, 120, 20), "MINIMAPA");
            if (gm.Fog != null) GUI.Label(new Rect(r.x + 8, r.y + r.height - 22, 160, 18), "Mapa " + (gm.Fog.DiscoveredPercent * 100f).ToString("0") + "% | N ocultar");
        }

        private void DrawEnvironment(V5GameManager gm, Rect r)
        {
            V5EnvironmentGrid env = gm.Environment;
            int step = Mathf.Max(2, env.Width / 30);
            for (int x = 0; x < env.Width; x += step)
            {
                for (int y = 0; y < env.Height; y += step)
                {
                    Vector2 world = env.TileCenterWorld(x, y);
                    if (world.magnitude > env.MapRadius) continue;
                    float colonized = env.colonization[x, y];
                    float tox = env.toxins[x, y];
                    float oxy = env.oxygen[x, y];
                    Color c = new Color(0.04f + oxy * 0.16f, 0.06f + colonized * 0.35f, 0.08f + tox * 0.25f, 0.58f);
                    DrawDot(WorldToMinimap(env, r, world), Mathf.Max(1.5f, r.width / 75f), c);
                }
            }
        }

        private void DrawResourceDots(V5GameManager gm, Rect r)
        {
            if (gm.Resources == null) return;
            V5EnvironmentGrid env = gm.Environment;
            for (int i = 0; i < gm.Resources.Nodes.Count; i += 2)
            {
                V5ResourceNode n = gm.Resources.Nodes[i];
                if (n == null || n.depleted) continue;
                Color c = new Color(1f, 0.85f, 0.25f, 0.45f);
                DrawDot(WorldToMinimap(env, r, n.transform.position), 2f, c);
            }
        }

        private void DrawCellDots(V5GameManager gm, Rect r)
        {
            V5EnvironmentGrid env = gm.Environment;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                DrawDot(WorldToMinimap(env, r, c.transform.position), c.Role == V5CellRole.Mother ? 5f : 3f, c.Role == V5CellRole.Mother ? Color.white : Color.cyan);
            }
            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity c = gm.NonPlayerCells[i];
                if (c == null) continue;
                DrawDot(WorldToMinimap(env, r, c.transform.position), 3f, Color.red);
            }
        }

        private void DrawRouteCounterDot(V5GameManager gm, Rect r)
        {
            if (gm.RouteCounters == null || !gm.RouteCounters.ActiveCounter) return;
            V5EnvironmentGrid env = gm.Environment;
            Color c = gm.RouteCounters.RouteMarkerColor(gm.RouteCounters.ActiveCounterRoute);
            c.a = 0.92f;
            DrawDot(WorldToMinimap(env, r, gm.RouteCounters.CounterCenter), 8f, c);
            DrawDot(WorldToMinimap(env, r, gm.RouteCounters.CounterCenter), 3f, Color.white);
        }

        private Vector2 WorldToMinimap(V5EnvironmentGrid env, Rect r, Vector2 world)
        {
            float nx = Mathf.InverseLerp(-env.MapRadius, env.MapRadius, world.x);
            float ny = Mathf.InverseLerp(-env.MapRadius, env.MapRadius, world.y);
            return new Vector2(r.x + nx * r.width, r.y + (1f - ny) * r.height);
        }

        private void DrawDot(Vector2 p, float size, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(new Rect(p.x - size * 0.5f, p.y - size * 0.5f, size, size), dot);
            GUI.color = old;
        }

        private void EnsureTexture()
        {
            if (dot != null) return;
            dot = Texture2D.whiteTexture;
        }
    }
}
