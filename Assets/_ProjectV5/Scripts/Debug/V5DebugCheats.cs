using UnityEngine;

namespace Protogenesis.V5
{
    public class V5DebugCheats : MonoBehaviour
    {
        public bool enableCheats = true;
        public string LastCheat = "";

        private void Update()
        {
            if (!enableCheats) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;

            if (Input.GetKeyDown(KeyCode.T))
            {
                gm.MotherCell.Resources.atp += 150f;
                gm.MotherCell.Resources.biomass += 100f;
                gm.MotherCell.Resources.aminoAcids += 60f;
                gm.MotherCell.Resources.lipids += 60f;
                gm.MotherCell.Resources.nucleotides += 40f;
                gm.MotherCell.Resources.minerals += 40f;
                LastCheat = "recursos añadidos";
            }
            if (Input.GetKeyDown(KeyCode.Y))
            {
                gm.MotherCell.Stats.stress = 0f;
                gm.MotherCell.Stats.currentHp = gm.MotherCell.Stats.maxHp;
                LastCheat = "madre curada";
            }
            if (Input.GetKeyDown(KeyCode.U) && gm.CellFactory != null && gm.Environment != null)
            {
                Vector2 p = (Vector2)gm.MotherCell.transform.position + Random.insideUnitCircle.normalized * Random.Range(7f, 12f);
                V5CellEntity enemy = gm.CellFactory.SpawnNeutral(p, Random.value < 0.5f ? V5EvolutionPath.Amoeba : V5EvolutionPath.Flagellate);
                if (enemy != null) enemy.gameObject.AddComponent<V5EnemyBrain>();
                LastCheat = "enemigo de prueba generado";
            }
            if (Input.GetKeyDown(KeyCode.I) && gm.Fog != null)
            {
                gm.Fog.RevealAll();
                LastCheat = "mapa revelado";
            }
            if (Input.GetKeyDown(KeyCode.O) && gm.Environment != null)
            {
                gm.Environment.ModifyArea(gm.MotherCell.transform.position, 10f, 0.05f, 0f, 0.03f, -0.05f, 0f, 0.25f, 0f);
                LastCheat = "zona colonizada para test";
            }
        }
    }
}
