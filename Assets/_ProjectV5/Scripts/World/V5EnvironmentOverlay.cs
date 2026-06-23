using UnityEngine;

namespace Protogenesis.V5
{
    [RequireComponent(typeof(SpriteRenderer))]
    public class V5EnvironmentOverlay : MonoBehaviour
    {
        public V5EnvironmentGrid Environment;
        public V5OverlayMode Mode = V5OverlayMode.Nutrients;
        public float refreshInterval = 0.25f;
        public float alpha = 0.48f;

        private Texture2D texture;
        private SpriteRenderer spriteRenderer;
        private float timer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = -50;
        }

        private void Update()
        {
            if (Environment == null) Environment = FindFirstObjectByType<V5EnvironmentGrid>();
            if (Environment == null || Environment.nutrients == null) return;
            timer += Time.deltaTime;
            if (texture == null || texture.width != Environment.Width || texture.height != Environment.Height)
            {
                CreateTexture();
                RefreshTexture();
            }
            else if (timer >= refreshInterval)
            {
                timer = 0f;
                RefreshTexture();
            }
        }

        private void CreateTexture()
        {
            texture = new Texture2D(Environment.Width, Environment.Height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Bilinear;
            texture.wrapMode = TextureWrapMode.Clamp;
            Sprite s = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f / Environment.TileSize);
            spriteRenderer.sprite = s;
            transform.position = Vector3.zero;
        }

        private void RefreshTexture()
        {
            if (Mode == V5OverlayMode.None)
            {
                spriteRenderer.enabled = false;
                return;
            }
            spriteRenderer.enabled = true;
            for (int x = 0; x < Environment.Width; x++)
            {
                for (int y = 0; y < Environment.Height; y++)
                {
                    float v = ChannelValue(x, y);
                    Color c = ColorForMode(Mode, v);
                    Vector2 p = Environment.TileCenterWorld(x, y);
                    if (p.magnitude > Environment.MapRadius) c.a = 0f;
                    else c.a *= alpha;
                    texture.SetPixel(x, y, c);
                }
            }
            texture.Apply(false);
        }

        private float ChannelValue(int x, int y)
        {
            switch (Mode)
            {
                case V5OverlayMode.Nutrients: return Environment.nutrients[x, y];
                case V5OverlayMode.Light: return Environment.lightLevel[x, y];
                case V5OverlayMode.Oxygen: return Environment.oxygen[x, y];
                case V5OverlayMode.Toxins: return Environment.toxins[x, y];
                case V5OverlayMode.Acidity: return Environment.acidity[x, y];
                case V5OverlayMode.Colonization: return Environment.colonization[x, y];
                case V5OverlayMode.Temperature: return Environment.temperature[x, y];
                default: return 0f;
            }
        }

        private Color ColorForMode(V5OverlayMode mode, float v)
        {
            switch (mode)
            {
                case V5OverlayMode.Nutrients: return new Color(0.15f, 0.85f, 0.35f, v);
                case V5OverlayMode.Light: return new Color(1.0f, 0.88f, 0.25f, v);
                case V5OverlayMode.Oxygen: return new Color(0.25f, 0.7f, 1.0f, v);
                case V5OverlayMode.Toxins: return new Color(0.85f, 0.1f, 0.85f, v);
                case V5OverlayMode.Acidity: return new Color(1.0f, 0.32f, 0.15f, v);
                case V5OverlayMode.Colonization: return new Color(0.65f, 1.0f, 0.5f, v);
                case V5OverlayMode.Temperature: return new Color(1.0f, 0.45f, 0.1f, v);
                default: return Color.clear;
            }
        }

        public void CycleMode()
        {
            int next = ((int)Mode + 1) % System.Enum.GetValues(typeof(V5OverlayMode)).Length;
            Mode = (V5OverlayMode)next;
            timer = refreshInterval;
        }
    }
}
