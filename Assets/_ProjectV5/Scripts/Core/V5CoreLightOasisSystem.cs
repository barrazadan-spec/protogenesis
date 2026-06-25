using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CoreLightOasisSystem : MonoBehaviour
    {
        public int OasisCount = 5;
        public float OasisRadius = 9f;
        public IReadOnlyList<Vector2> OasisCenters { get { return oasisCenters; } }

        private readonly List<Vector2> oasisCenters = new List<Vector2>(8);
        private readonly List<SpriteRenderer> glowRenderers = new List<SpriteRenderer>(8);
        private readonly List<SpriteRenderer> ringRenderers = new List<SpriteRenderer>(8);
        private static Sprite glowSprite;
        private static Sprite ringSprite;

        public void Build(V5GameManager gm, Vector2 motherPosition)
        {
            ClearVisuals();
            if (gm == null || !gm.CoreMode || gm.Environment == null) return;

            V5EnvironmentGrid environment = gm.Environment;
            float mapRadius = environment.MapRadius;
            Vector2[] centers =
            {
                motherPosition + new Vector2(mapRadius * 0.10f, mapRadius * 0.07f),
                new Vector2(-0.38f, -0.28f) * mapRadius,
                new Vector2(-0.05f, 0.34f) * mapRadius,
                new Vector2(0.28f, -0.32f) * mapRadius,
                new Vector2(0.60f, 0.18f) * mapRadius
            };

            int count = Mathf.Max(1, OasisCount);
            for (int i = 0; i < count; i++)
            {
                Vector2 center;
                if (i < centers.Length)
                {
                    center = centers[i];
                }
                else
                {
                    float angle = i * 2.3999632f;
                    center = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * mapRadius * 0.55f;
                }

                float allowedRadius = Mathf.Max(1f, mapRadius - OasisRadius - 2f);
                if (center.magnitude > allowedRadius) center = center.normalized * allowedRadius;
                oasisCenters.Add(center);
                environment.RaiseLightOasis(center, OasisRadius);
                CreateOasisVisual(center, i);
            }
        }

        public bool TryGetRandomOasis(out Vector2 center)
        {
            if (oasisCenters.Count == 0)
            {
                center = Vector2.zero;
                return false;
            }

            center = oasisCenters[Random.Range(0, oasisCenters.Count)];
            return true;
        }

        private void Update()
        {
            for (int i = 0; i < glowRenderers.Count; i++)
            {
                float pulse = 0.5f + Mathf.Sin(Time.time * 0.65f + i * 1.7f) * 0.5f;
                SpriteRenderer glow = glowRenderers[i];
                SpriteRenderer ring = ringRenderers[i];
                if (glow != null) glow.color = new Color(1f, 0.88f, 0.32f, 0.075f + pulse * 0.035f);
                if (ring != null) ring.color = new Color(1f, 0.92f, 0.42f, 0.22f + pulse * 0.10f);
            }
        }

        private void CreateOasisVisual(Vector2 center, int index)
        {
            if (glowSprite == null) glowSprite = V5ProceduralSprites.CreateCircleSprite(128);
            if (ringSprite == null) ringSprite = V5ProceduralSprites.CreateRingSprite(128, 0.045f);

            GameObject root = new GameObject("LightOasis_" + (index + 1));
            root.transform.SetParent(transform, false);
            root.transform.position = center;

            SpriteRenderer glow = root.AddComponent<SpriteRenderer>();
            glow.sprite = glowSprite;
            glow.sortingOrder = -39;
            glow.transform.localScale = Vector3.one * OasisRadius * 2f;

            GameObject ringObject = new GameObject("LightOasisRing");
            ringObject.transform.SetParent(root.transform, false);
            SpriteRenderer ring = ringObject.AddComponent<SpriteRenderer>();
            ring.sprite = ringSprite;
            ring.sortingOrder = -38;
            ring.transform.localScale = Vector3.one * OasisRadius * 2f;

            glowRenderers.Add(glow);
            ringRenderers.Add(ring);
        }

        private void ClearVisuals()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (Application.isPlaying) Destroy(child.gameObject);
                else DestroyImmediate(child.gameObject);
            }
            oasisCenters.Clear();
            glowRenderers.Clear();
            ringRenderers.Clear();
        }
    }
}
