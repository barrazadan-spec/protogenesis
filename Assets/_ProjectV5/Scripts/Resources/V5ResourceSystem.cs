using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ResourceSystem : MonoBehaviour
    {
        public int InitialNodeCount = 90;
        public readonly List<V5ResourceNode> Nodes = new List<V5ResourceNode>(256);
        private float respawnTimer;

        public void SpawnInitialNodes()
        {
            V5EnvironmentGrid env = V5GameManager.Instance != null ? V5GameManager.Instance.Environment : null;
            float r = env != null ? env.MapRadius : V5Balance.DefaultMapRadius;
            for (int i = 0; i < InitialNodeCount; i++)
            {
                Vector2 p = Random.insideUnitCircle * r * 0.92f;
                SpawnNode(p, RandomKind(), Random.Range(22f, 70f));
            }
        }

        private void Update()
        {
            respawnTimer += Time.deltaTime;
            if (respawnTimer > 3f)
            {
                respawnTimer = 0f;
                PruneMissingNodes();
                if (IsCoreMode())
                {
                    while (Nodes.Count < InitialNodeCount) SpawnAmbientNode();
                    return;
                }

                if (Nodes.Count < InitialNodeCount + 30) SpawnAmbientNode();
            }
        }

        private void SpawnAmbientNode()
        {
            V5EnvironmentGrid env = V5GameManager.Instance != null ? V5GameManager.Instance.Environment : null;
            if (env == null) return;
            Vector2 p = Random.insideUnitCircle * env.MapRadius * 0.94f;
            int x, y;
            env.WorldToTile(p, out x, out y);
            float amount = Mathf.Lerp(15f, 55f, env.nutrients[x, y] + env.detritus[x, y]);
            SpawnNode(p, RandomKind(), amount);
        }

        public V5ResourceNode SpawnNode(Vector2 position, V5ResourceKind kind, float amount)
        {
            return SpawnNode(position, kind, amount, 1f);
        }

        public V5ResourceNode SpawnNode(Vector2 position, V5ResourceKind kind, float amount, float visualScaleMultiplier)
        {
            kind = V5ResourceKind.Biomass;
            GameObject go = new GameObject("V5_Resource_" + kind);
            go.transform.position = position;
            V5ResourceNode node = go.AddComponent<V5ResourceNode>();
            node.Owner = this;
            node.Setup(kind, amount);
            node.SetVisualScaleMultiplier(visualScaleMultiplier);
            Nodes.Add(node);
            return node;
        }

        public void NotifyNodeDepleted(V5ResourceNode node)
        {
            if (node == null) return;
            Nodes.Remove(node);
            SpawnAmbientNode();
            Destroy(node.gameObject);
        }

        public V5ResourceNode FindNearestNode(Vector2 pos, float maxRange)
        {
            V5ResourceNode best = null;
            float bestDist = maxRange;
            for (int i = Nodes.Count - 1; i >= 0; i--)
            {
                V5ResourceNode n = Nodes[i];
                if (n == null) { Nodes.RemoveAt(i); continue; }
                if (n.depleted) continue;
                float d = Vector2.Distance(pos, n.transform.position);
                if (d < bestDist)
                {
                    bestDist = d;
                    best = n;
                }
            }
            return best;
        }

        private V5ResourceKind RandomKind()
        {
            return V5ResourceKind.Biomass;
        }

        private void PruneMissingNodes()
        {
            for (int i = Nodes.Count - 1; i >= 0; i--)
                if (Nodes[i] == null) Nodes.RemoveAt(i);
        }

        private bool IsCoreMode()
        {
            V5GameManager gm = V5GameManager.Instance;
            return gm != null && gm.CoreMode;
        }
    }
}
