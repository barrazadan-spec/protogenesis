using UnityEngine;

namespace Protogenesis.V5
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class V5FogOfWarSystem : MonoBehaviour
    {
        public V5EnvironmentGrid Environment;
        public float revealInterval = 0.18f;
        public float baseRevealRadius = 5.5f;
        public float fadePerSecond = 0.08f;
        public float fogAlpha = 0.72f;
        public float exploredAlpha = 0.28f;

        public float DiscoveredPercent { get; private set; }
        public bool FullyRevealed;

        private float[,] revealed;
        private float[,] explored;
        private Texture2D texture;
        private SpriteRenderer spriteRenderer;
        private float timer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = -20;
        }

        private void Update()
        {
            if (Environment == null && V5GameManager.Instance != null) Environment = V5GameManager.Instance.Environment;
            if (Environment == null || Environment.nutrients == null) return;
            EnsureBuffers();
            timer += Time.deltaTime;
            if (timer >= revealInterval)
            {
                timer = 0f;
                TickReveal();
                RefreshTexture();
            }
        }

        public void RevealAll()
        {
            EnsureBuffers();
            FullyRevealed = true;
            if (Environment == null) return;
            for (int x = 0; x < Environment.Width; x++)
            {
                for (int y = 0; y < Environment.Height; y++)
                {
                    revealed[x, y] = 1f;
                    explored[x, y] = 1f;
                }
            }
            DiscoveredPercent = 1f;
            RefreshTexture();
        }

        private void EnsureBuffers()
        {
            if (Environment == null) return;
            if (revealed == null || revealed.GetLength(0) != Environment.Width || revealed.GetLength(1) != Environment.Height)
            {
                revealed = new float[Environment.Width, Environment.Height];
                explored = new float[Environment.Width, Environment.Height];
            }
            if (texture == null || texture.width != Environment.Width || texture.height != Environment.Height)
            {
                texture = new Texture2D(Environment.Width, Environment.Height, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Bilinear;
                texture.wrapMode = TextureWrapMode.Clamp;
                Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f / Environment.TileSize);
                spriteRenderer.sprite = sprite;
                transform.position = Vector3.zero;
            }
        }

        private void TickReveal()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || Environment == null) return;
            for (int x = 0; x < Environment.Width; x++)
            {
                for (int y = 0; y < Environment.Height; y++)
                {
                    revealed[x, y] = Mathf.Max(0f, revealed[x, y] - fadePerSecond * revealInterval);
                }
            }

            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (cell == null) continue;
                float radius = baseRevealRadius + cell.Stats.sensorRange * 0.35f;
                if (cell.HasRecognition) radius += 2.0f;
                RevealArea(cell.transform.position, radius);
            }

            int discovered = 0;
            int valid = 0;
            for (int x = 0; x < Environment.Width; x += 2)
            {
                for (int y = 0; y < Environment.Height; y += 2)
                {
                    Vector2 p = Environment.TileCenterWorld(x, y);
                    if (p.magnitude > Environment.MapRadius) continue;
                    valid++;
                    if (explored[x, y] > 0.1f) discovered++;
                }
            }
            DiscoveredPercent = valid > 0 ? (float)discovered / valid : 0f;
        }

        private void RevealArea(Vector2 world, float radius)
        {
            int cx, cy;
            Environment.WorldToTile(world, out cx, out cy);
            int r = Mathf.CeilToInt(radius / Environment.TileSize);
            for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(Environment.Width - 1, cx + r); x++)
            {
                for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(Environment.Height - 1, cy + r); y++)
                {
                    Vector2 p = Environment.TileCenterWorld(x, y);
                    float d = Vector2.Distance(world, p);
                    if (d > radius) continue;
                    float v = 1f - d / Mathf.Max(0.001f, radius);
                    revealed[x, y] = Mathf.Max(revealed[x, y], Mathf.Clamp01(0.35f + v));
                    explored[x, y] = 1f;
                }
            }
        }

        private void RefreshTexture()
        {
            if (texture == null || Environment == null) return;
            for (int x = 0; x < Environment.Width; x++)
            {
                for (int y = 0; y < Environment.Height; y++)
                {
                    Vector2 p = Environment.TileCenterWorld(x, y);
                    if (p.magnitude > Environment.MapRadius)
                    {
                        texture.SetPixel(x, y, Color.clear);
                        continue;
                    }
                    float alpha = FullyRevealed ? 0f : fogAlpha;
                    if (explored[x, y] > 0.1f) alpha = exploredAlpha;
                    alpha = Mathf.Lerp(alpha, 0f, revealed[x, y]);
                    texture.SetPixel(x, y, new Color(0.0f, 0.01f, 0.025f, alpha));
                }
            }
            texture.Apply(false);
        }
    }
}
