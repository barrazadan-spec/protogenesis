using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Simple runtime budget monitor. It favors safe warnings and optional soft cleanup over risky optimizations.
    /// Ctrl+O toggles automatic soft culling of distant non-player threats if the run gets too heavy.
    /// </summary>
    public class V5OptimizationGuardSystem : MonoBehaviour
    {
        public float EstimatedFps { get; private set; }
        public string Status { get; private set; }
        public bool AutoSoftCull;
        public int MaxNonPlayerCells = 70;
        public int MaxResourceNodes = 180;

        private float emaDelta = 1f / 60f;
        private float tick;

        private void Awake()
        {
            Application.targetFrameRate = 60;
        }

        private void Update()
        {
            emaDelta = Mathf.Lerp(emaDelta, Time.unscaledDeltaTime, 0.05f);
            EstimatedFps = 1f / Mathf.Max(0.0001f, emaDelta);
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.O))
            {
                AutoSoftCull = !AutoSoftCull;
                Notify("AutoSoftCull " + (AutoSoftCull ? "ON" : "OFF"));
            }

            tick += Time.unscaledDeltaTime;
            if (tick < 1.0f) return;
            tick = 0f;
            EvaluateBudget();
        }

        private void EvaluateBudget()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null)
            {
                Status = "Sin GM.";
                return;
            }

            int player = gm.PlayerCellCount();
            int enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            int nodes = gm.Resources != null && gm.Resources.Nodes != null ? gm.Resources.Nodes.Count : 0;
            Status = "FPS~" + EstimatedFps.ToString("0") + " | cells " + player + "/" + enemies + " | nodes " + nodes;

            if (AutoSoftCull && (EstimatedFps < 35f || enemies > MaxNonPlayerCells || nodes > MaxResourceNodes))
            {
                int culled = SoftCull(gm, enemies - MaxNonPlayerCells, nodes - MaxResourceNodes);
                if (culled > 0) Notify("OptimizationGuard limpió " + culled + " entidades lejanas");
            }
        }

        private int SoftCull(V5GameManager gm, int enemyOverflow, int nodeOverflow)
        {
            int removed = 0;
            Vector2 origin = gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;

            if (enemyOverflow > 0 && gm.NonPlayerCells != null)
            {
                for (int i = gm.NonPlayerCells.Count - 1; i >= 0 && removed < enemyOverflow; i--)
                {
                    V5CellEntity c = gm.NonPlayerCells[i];
                    if (c == null) continue;
                    if (Vector2.Distance(origin, c.transform.position) < 18f) continue;
                    gm.UnregisterCell(c);
                    Destroy(c.gameObject);
                    removed++;
                }
            }

            if (nodeOverflow > 0 && gm.Resources != null && gm.Resources.Nodes != null)
            {
                for (int i = gm.Resources.Nodes.Count - 1; i >= 0 && removed < enemyOverflow + nodeOverflow; i--)
                {
                    V5ResourceNode n = gm.Resources.Nodes[i];
                    if (n == null) { gm.Resources.Nodes.RemoveAt(i); continue; }
                    if (Vector2.Distance(origin, n.transform.position) < 14f) continue;
                    gm.Resources.Nodes.RemoveAt(i);
                    Destroy(n.gameObject);
                    removed++;
                }
            }
            return removed;
        }

        private void Notify(string msg)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(msg);
        }
    }
}
