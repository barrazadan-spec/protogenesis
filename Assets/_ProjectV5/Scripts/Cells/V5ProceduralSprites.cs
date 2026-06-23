using UnityEngine;

namespace Protogenesis.V5
{
    public static class V5ProceduralSprites
    {
        public static Sprite CreateCircleSprite(int size)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.48f;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    float a = Mathf.Clamp01((radius - d) / 2.2f);
                    float edge = Mathf.Clamp01(1f - Mathf.Abs(radius * 0.78f - d) / (radius * 0.25f));
                    Color c = new Color(1f, 1f, 1f, a * (0.58f + edge * 0.42f));
                    tex.SetPixel(x, y, c);
                }
            }
            tex.Apply(false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }

        public static Sprite CreateRingSprite(int size, float thickness01)
        {
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float outer = size * 0.48f;
            float inner = outer * Mathf.Clamp01(1f - thickness01);
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    float outerA = Mathf.Clamp01((outer - d) / 1.8f);
                    float innerA = Mathf.Clamp01((d - inner) / 1.8f);
                    float a = Mathf.Min(outerA, innerA);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
                }
            }
            tex.Apply(false);
            return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
