using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RunResetSystem : MonoBehaviour
    {
        public string LastMessage = "";

        public void RestartScenario(V5ScenarioId id)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            V5ScenarioDefinition def = V5ScenarioLibrary.Get(id);
            gm.ScenarioId = id;
            gm.ElapsedSeconds = 0f;
            gm.Phase = V5GamePhase.Primordial;
            gm.Paused = false;
            if (gm.Hud != null) gm.Hud.EndMessage = "";
            V5PanelRouter.CloseAll();

            if (gm.Selection != null) gm.Selection.ClearSelection();
            gm.ClearCellsForLoad();
            ClearResources(gm);
            ClearTransientEnemies();

            if (gm.Environment != null)
            {
                int width = gm.CoreMode ? Mathf.Max(def.mapWidth, 280) : def.mapWidth;
                int height = gm.CoreMode ? Mathf.Max(def.mapHeight, 280) : def.mapHeight;
                gm.Environment.Initialize(width, height, def.tileSize, Mathf.Min(width, height) * def.tileSize * 0.47f);
                gm.Environment.ApplyScenarioBias(def);
            }
            if (gm.Resources != null)
            {
                gm.Resources.InitialNodeCount = gm.CoreMode ? Mathf.Clamp(def.startingResources, 62, 78) : def.startingResources;
                gm.Resources.SpawnInitialNodes();
            }

            if (gm.CellFactory != null)
            {
                Vector2 motherStart = gm.CoreMode && gm.Environment != null ? Vector2.left * gm.Environment.MapRadius * 0.72f : Vector2.zero;
                gm.CellFactory.SpawnMother(motherStart);
                if (gm.CoreMode && gm.CoreMotherProduction != null) gm.CoreMotherProduction.SeedStartingCells(gm, 3);
                if (gm.CoreMode && gm.Resources != null) SpawnCoreBaseResources(gm, motherStart, 18);
                if (def.spawnEnemies)
                {
                    for (int i = 0; i < def.neutralCells; i++)
                    {
                        Vector2 p = Random.insideUnitCircle.normalized * Random.Range(10f, gm.Environment.MapRadius * 0.8f);
                        V5EvolutionPath path = V5EcologySpawnPolicy.PickInitialPath(id, i);
                        V5CellEntity enemy = gm.CellFactory.SpawnNeutral(p, path);
                        V5EcologySpawnPolicy.ConfigureInitialNpc(enemy, id, i);
                    }
                }
            }
            if (gm.Scenario != null) gm.Scenario.BeginScenario(id);
            if (gm.Selection != null && gm.MotherCell != null) gm.Selection.AddSelection(gm.MotherCell);
            LastMessage = "Escenario reiniciado: " + def.displayName;
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        private void SpawnCoreBaseResources(V5GameManager gm, Vector2 center, int count)
        {
            if (gm == null || gm.Resources == null) return;
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2.3999632f;
                float ring = Random.Range(4.5f, 15.5f);
                Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ring + Random.insideUnitCircle * 1.7f;
                if (gm.Environment != null && pos.magnitude > gm.Environment.MapRadius * 0.88f) pos = pos.normalized * gm.Environment.MapRadius * 0.88f;
                gm.Resources.SpawnNode(pos, V5ResourceKind.Biomass, Random.Range(34f, 76f), 1.05f);
            }
        }

        private void ClearResources(V5GameManager gm)
        {
            if (gm == null || gm.Resources == null) return;
            List<V5ResourceNode> nodes = new List<V5ResourceNode>(gm.Resources.Nodes);
            for (int i = 0; i < nodes.Count; i++) if (nodes[i] != null) Destroy(nodes[i].gameObject);
            gm.Resources.Nodes.Clear();
        }

        private void ClearTransientEnemies()
        {
            V5EnemyBrain[] brains = FindObjectsByType<V5EnemyBrain>(FindObjectsSortMode.None);
            for (int i = 0; i < brains.Length; i++) if (brains[i] != null) Destroy(brains[i].gameObject);
        }
    }
}
