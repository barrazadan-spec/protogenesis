using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CoreTerritorySystem : MonoBehaviour
    {
        [Header("Core Colony Territory")]
        public float Radius = 14f;
        public float ReturnMargin = 1.2f;
        public bool ShowTerritoryRing;
        public Color RingColor = new Color(0.48f, 1f, 0.68f, 0.18f);
        public int RingSortingOrder = 4;

        private SpriteRenderer ringRenderer;
        private static Sprite ringSprite;

        public Vector2 Center
        {
            get
            {
                V5CellEntity mother = CurrentMother();
                return mother != null ? (Vector2)mother.transform.position : (Vector2)transform.position;
            }
        }

        private void Awake()
        {
            EnsureVisual();
        }

        private void OnEnable()
        {
            EnsureVisual();
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            bool active = gm != null && gm.CoreMode && gm.MotherCell != null;
            EnsureVisual();
            if (ringRenderer != null) ringRenderer.enabled = active && ShowTerritoryRing;
            if (!active) return;

            Radius = Mathf.Max(1f, Radius);
            transform.position = gm.MotherCell.transform.position;
            transform.localScale = Vector3.one * (Radius * 2f);
            ringRenderer.color = RingColor;
            ringRenderer.sortingOrder = RingSortingOrder;
        }

        public bool IsInside(Vector2 worldPosition)
        {
            return Vector2.Distance(worldPosition, Center) <= Radius;
        }

        public Vector2 ClampInside(Vector2 worldPosition)
        {
            return ClampInside(worldPosition, 0f);
        }

        public Vector2 ClampInside(Vector2 worldPosition, float margin)
        {
            Vector2 center = Center;
            float allowedRadius = Mathf.Max(0.5f, Radius - Mathf.Max(0f, margin));
            Vector2 offset = worldPosition - center;
            if (offset.sqrMagnitude <= allowedRadius * allowedRadius) return worldPosition;
            if (offset.sqrMagnitude <= 0.0001f) return center;
            return center + offset.normalized * allowedRadius;
        }

        public Vector2 ReturnTarget(Vector2 worldPosition)
        {
            return ClampInside(worldPosition, ReturnMargin);
        }

        public Vector2 ScoutTarget(Vector2 origin, Vector2 direction, float distance)
        {
            Vector2 dir = direction.sqrMagnitude > 0.0001f ? direction.normalized : Random.insideUnitCircle.normalized;
            return ClampInside(origin + dir * distance, 0.6f);
        }

        public V5ResourceNode FindNearestNode(Vector2 from, float maxRange)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Resources == null) return null;

            List<V5ResourceNode> nodes = gm.Resources.Nodes;
            V5ResourceNode best = null;
            float bestDist = maxRange;
            float territorySqr = Radius * Radius;
            Vector2 center = Center;
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                V5ResourceNode node = nodes[i];
                if (node == null)
                {
                    nodes.RemoveAt(i);
                    continue;
                }
                if (node.depleted) continue;
                Vector2 nodePosition = node.transform.position;
                if ((nodePosition - center).sqrMagnitude > territorySqr) continue;
                float distance = Vector2.Distance(from, nodePosition);
                if (distance < bestDist)
                {
                    bestDist = distance;
                    best = node;
                }
            }
            return best;
        }

        private V5CellEntity CurrentMother()
        {
            V5GameManager gm = V5GameManager.Instance;
            return gm != null ? gm.MotherCell : null;
        }

        private void EnsureVisual()
        {
            if (ringRenderer == null)
            {
                ringRenderer = GetComponent<SpriteRenderer>();
                if (ringRenderer == null) ringRenderer = gameObject.AddComponent<SpriteRenderer>();
            }
            if (ringSprite == null) ringSprite = V5ProceduralSprites.CreateRingSprite(256, 0.025f);
            ringRenderer.sprite = ringSprite;
            ringRenderer.color = RingColor;
            ringRenderer.sortingOrder = RingSortingOrder;
            ringRenderer.enabled = false;
        }
    }
}
