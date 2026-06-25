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
            int debrisCount = Mathf.RoundToInt(InitialNodeCount * 0.30f);
            int microPlanktonCount = Mathf.RoundToInt(InitialNodeCount * 0.40f);
            for (int i = 0; i < InitialNodeCount; i++)
            {
                V5ResourceEcology ecology = i < debrisCount
                    ? V5ResourceEcology.Debris
                    : (i < debrisCount + microPlanktonCount ? V5ResourceEcology.MicroPlankton : V5ResourceEcology.Algae);
                Vector2 p = ChooseSpawnPosition(env, r * 0.92f, ecology);
                SpawnNode(p, RandomKind(), Random.Range(22f, 70f), 1f, ecology);
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
            V5ResourceEcology ecology = RandomEcology();
            Vector2 p = ChooseSpawnPosition(env, env.MapRadius * 0.94f, ecology);
            int x, y;
            env.WorldToTile(p, out x, out y);
            float amount = Mathf.Lerp(15f, 55f, env.nutrients[x, y] + env.detritus[x, y]);
            SpawnNode(p, RandomKind(), amount, 1f, ecology);
        }

        public V5ResourceNode SpawnNode(Vector2 position, V5ResourceKind kind, float amount)
        {
            return SpawnNode(position, kind, amount, 1f);
        }

        public V5ResourceNode SpawnNode(Vector2 position, V5ResourceKind kind, float amount, float visualScaleMultiplier)
        {
            return SpawnNode(position, kind, amount, visualScaleMultiplier, false);
        }

        public V5ResourceNode SpawnNode(Vector2 position, V5ResourceKind kind, float amount, float visualScaleMultiplier, bool living)
        {
            return SpawnNode(position, kind, amount, visualScaleMultiplier,
                living ? V5ResourceEcology.MicroPlankton : V5ResourceEcology.Debris);
        }

        public V5ResourceNode SpawnNode(Vector2 position, V5ResourceKind kind, float amount, float visualScaleMultiplier, V5ResourceEcology ecology)
        {
            kind = V5ResourceKind.Biomass;
            string suffix = ecology == V5ResourceEcology.Algae ? "_Algae" : (ecology == V5ResourceEcology.MicroPlankton ? "_Plankton" : "_Debris");
            GameObject go = new GameObject("V5_Resource_" + kind + suffix);
            go.transform.position = position;
            V5ResourceNode node = go.AddComponent<V5ResourceNode>();
            node.Owner = this;
            float yieldAmount = ecology == V5ResourceEcology.Algae ? amount * 1.8f : amount;
            node.Setup(kind, yieldAmount);
            node.SetVisualScaleMultiplier(visualScaleMultiplier);
            node.SetEcology(ecology);
            Nodes.Add(node);
            return node;
        }

        private Vector2 ChooseSpawnPosition(V5EnvironmentGrid environment, float randomRadius, V5ResourceEcology ecology)
        {
            if (ecology == V5ResourceEcology.Algae && IsCoreMode())
            {
                V5CoreLightOasisSystem oases = FindFirstObjectByType<V5CoreLightOasisSystem>();
                Vector2 oasisCenter;
                if (oases != null && oases.TryGetRandomOasis(out oasisCenter) && Random.value < 0.85f)
                {
                    Vector2 direction = Random.insideUnitCircle.normalized;
                    if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
                    Vector2 nearOasis = oasisCenter + direction * Random.Range(oases.OasisRadius * 0.65f, oases.OasisRadius * 1.65f);
                    float allowedRadius = environment != null ? environment.MapRadius * 0.94f : randomRadius;
                    if (nearOasis.magnitude > allowedRadius) nearOasis = nearOasis.normalized * allowedRadius;
                    return nearOasis;
                }
            }
            return Random.insideUnitCircle * randomRadius;
        }

        private V5ResourceEcology RandomEcology()
        {
            float roll = Random.value;
            if (roll < 0.30f) return V5ResourceEcology.Debris;
            if (roll < 0.70f) return V5ResourceEcology.MicroPlankton;
            return V5ResourceEcology.Algae;
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
