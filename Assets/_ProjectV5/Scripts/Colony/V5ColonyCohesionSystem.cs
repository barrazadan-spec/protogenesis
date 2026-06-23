using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Computes how coherent the colony is as an RTS organism. Cohesion is not just UI: high cohesion
    /// gently reduces stress and increases recovery; scattered colonies accumulate light stress pressure.
    /// </summary>
    public class V5ColonyCohesionSystem : MonoBehaviour
    {
        public float Cohesion01 { get; private set; }
        public float AverageDistanceFromMother { get; private set; }
        public int CellsNearMother { get; private set; }
        public int CellsFarFromMother { get; private set; }
        public string Summary { get; private set; }

        private float nextTick;

        private void Update()
        {
            if (Time.time < nextTick) return;
            nextTick = Time.time + 0.75f;
            Tick();
        }

        private void Tick()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            Vector2 motherPos = gm.MotherCell.transform.position;
            int count = 0;
            float sum = 0f;
            CellsNearMother = 0;
            CellsFarFromMother = 0;

            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                float d = Vector2.Distance(c.transform.position, motherPos);
                sum += d;
                count++;
                if (d <= 8f) CellsNearMother++;
                if (d >= 22f) CellsFarFromMother++;
            }

            AverageDistanceFromMother = count > 0 ? sum / count : 0f;
            float distanceScore = Mathf.InverseLerp(28f, 6f, AverageDistanceFromMother);
            float nearScore = count > 0 ? CellsNearMother / (float)count : 1f;
            float farPenalty = count > 0 ? CellsFarFromMother / (float)count : 0f;
            Cohesion01 = Mathf.Clamp01(distanceScore * 0.55f + nearScore * 0.55f - farPenalty * 0.35f);

            if (gm.MotherCell != null)
            {
                if (Cohesion01 > 0.72f)
                {
                    gm.MotherCell.Stats.stress = Mathf.Max(0f, gm.MotherCell.Stats.stress - 0.45f);
                    gm.MotherCell.Resources.lipids += 0.02f * count;
                }
                else if (Cohesion01 < 0.32f && count > 5)
                {
                    gm.MotherCell.Stats.stress = Mathf.Min(100f, gm.MotherCell.Stats.stress + 0.35f);
                }
            }

            Summary = string.Format("Cohesión {0:0}% | dist. media {1:0.0}u | cerca {2} | lejos {3}", Cohesion01 * 100f, AverageDistanceFromMother, CellsNearMother, CellsFarFromMother);
        }
    }
}
