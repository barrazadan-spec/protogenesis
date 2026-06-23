using UnityEngine;

namespace Protogenesis.V5
{
    public class V5DifficultyDirector : MonoBehaviour, IV5RunResettable
    {
        public float ThreatLevel { get; private set; }
        public int WavesSpawned { get; private set; }
        public string LastDirectorAction = "director estable";

        private float tick;
        private float spawnTimer;

        public void ResetForNewRun()
        {
            ThreatLevel = 0f;
            WavesSpawned = 0;
            LastDirectorAction = "director estable";
            tick = 0f;
            spawnTimer = 0f;
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;

            tick += Time.deltaTime;
            spawnTimer += Time.deltaTime;
            if (tick >= 1.0f)
            {
                tick = 0f;
                RecalculateThreat(gm);
                AssistIfStalled(gm);
            }

            float interval = Mathf.Lerp(135f, 36f, Mathf.Clamp01(ThreatLevel));
            if (spawnTimer >= interval)
            {
                spawnTimer = 0f;
                SpawnPressureWave(gm);
            }
        }

        private void RecalculateThreat(V5GameManager gm)
        {
            float time = Mathf.Clamp01(gm.ElapsedSeconds / (30f * 60f));
            float colonized = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float playerStrength = gm.PlayerPopulationLoad() / V5Balance.HardPopulationLoad;
            float enemyPressure = Mathf.Clamp01(gm.NonPlayerCells.Count / 18f);
            float awakening = V5EcologySpawnPolicy.EcologicalAwakening(gm);
            ThreatLevel = Mathf.Clamp01(time * 0.18f + colonized * 0.25f + playerStrength * 0.18f + enemyPressure * 0.10f + awakening * 0.42f);
        }

        private void AssistIfStalled(V5GameManager gm)
        {
            if (gm.MotherCell == null || gm.Resources == null) return;
            bool lowAtp = gm.MotherCell.Resources.atp < 15f;
            bool lowBio = gm.MotherCell.Resources.biomass < 12f;
            if ((lowAtp || lowBio) && gm.ElapsedSeconds > 45f && Random.value < 0.025f)
            {
                Vector2 p = (Vector2)gm.MotherCell.transform.position + Random.insideUnitCircle.normalized * Random.Range(4f, 8f);
                gm.Resources.SpawnNode(p, lowAtp ? V5ResourceKind.ATP : V5ResourceKind.Biomass, Random.Range(25f, 60f));
                LastDirectorAction = "asistencia ecológica: apareció un recurso cercano";
            }
        }

        private void SpawnPressureWave(V5GameManager gm)
        {
            if (gm == null || gm.CellFactory == null || gm.Environment == null) return;
            float awakening = V5EcologySpawnPolicy.EcologicalAwakening(gm);
            if (gm.ElapsedSeconds < V5EcologySpawnPolicy.HostileGraceSeconds(gm.ScenarioId) && awakening < 0.55f) return;
            if (gm.ScenarioId == V5ScenarioId.FirstDrop && awakening < 0.42f) return;
            int count = Mathf.Clamp(1 + Mathf.RoundToInt(ThreatLevel * 4f), 1, 5);
            float radius = gm.Environment.MapRadius * Random.Range(0.65f, 0.95f);
            Vector2 center = Random.insideUnitCircle.normalized * radius;
            for (int i = 0; i < count; i++)
            {
                V5EvolutionPath path = PickEnemyPath(gm);
                V5CellEntity enemy = gm.CellFactory.SpawnNeutral(center + Random.insideUnitCircle * 4f, path);
                if (enemy != null && enemy.GetComponent<V5EnemyBrain>() == null) enemy.gameObject.AddComponent<V5EnemyBrain>();
            }
            WavesSpawned++;
            LastDirectorAction = "ola biológica: " + count + " organismos hostiles entraron";
        }

        private V5EvolutionPath PickEnemyPath(V5GameManager gm)
        {
            if (ThreatLevel < 0.25f) return Random.value < 0.7f ? V5EvolutionPath.Bacteria : V5EvolutionPath.Flagellate;
            if (ThreatLevel < 0.55f)
            {
                float r = Random.value;
                if (r < 0.35f) return V5EvolutionPath.Amoeba;
                if (r < 0.65f) return V5EvolutionPath.Flagellate;
                return V5EvolutionPath.Bacteria;
            }
            else
            {
                float r = Random.value;
            if (r < 0.22f) return V5EvolutionPath.Amoeba;
            if (r < 0.44f) return V5EvolutionPath.Rotifer;
            if (r < 0.64f) return V5EvolutionPath.Nematode;
            if (r < 0.82f) return V5EvolutionPath.SlimeMold;
            return V5EvolutionPath.Flagellate;
            }
        }
    }
}
