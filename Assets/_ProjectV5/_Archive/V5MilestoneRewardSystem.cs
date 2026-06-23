using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5MilestoneRewardSystem : MonoBehaviour
    {
        private readonly HashSet<string> completed = new HashSet<string>();
        public string LastMilestone = "";
        public int CompletedCount { get { return completed.Count; } }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity m = gm.MotherCell;

            Check("Primera estructura", m.Structures.Count >= 1, m, V5ResourceWallet.Cost(10, 5, 3, 2, 1, 0));
            Check("Dominio definido", m.Domain != V5CellDomain.LUCA, m, V5ResourceWallet.Cost(12, 5, 2, 2, 2, 1));
            Check("Primera división", gm.PlayerCellCount() >= 2, m, V5ResourceWallet.Cost(16, 8, 4, 4, 2, 0));
            Check("Microcolonia", gm.PlayerCellCount() >= 5, m, V5ResourceWallet.Cost(22, 10, 6, 5, 3, 2));
            Check("Primer anillo génico", gm.Genes != null && gm.Genes.UnlockedCount >= 1, m, V5ResourceWallet.Cost(12, 6, 3, 2, 2, 1));
            Check("Colonización inicial", gm.Environment != null && gm.Environment.AverageColonization() >= 0.08f, m, V5ResourceWallet.Cost(20, 8, 5, 4, 3, 2));
            Check("Dominio ecológico", gm.Environment != null && gm.Environment.AverageColonization() >= 0.25f, m, V5ResourceWallet.Cost(35, 15, 8, 8, 6, 4));
            Check("Forma apex", gm.Apex != null && gm.Apex.ApexSpawned, m, V5ResourceWallet.Cost(45, 20, 12, 10, 8, 5));
        }

        private void Check(string id, bool condition, V5CellEntity mother, V5ResourceWallet reward)
        {
            if (!condition || completed.Contains(id)) return;
            completed.Add(id);
            mother.Resources.atp += reward.atp;
            mother.Resources.biomass += reward.biomass;
            mother.Resources.aminoAcids += reward.aminoAcids;
            mother.Resources.lipids += reward.lipids;
            mother.Resources.nucleotides += reward.nucleotides;
            mother.Resources.minerals += reward.minerals;
            LastMilestone = id + " — recompensa ecológica recibida";
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null)
            {
                if (gm.Hud != null) gm.Hud.Toast(LastMilestone);
                if (gm.Codex != null) gm.Codex.Unlock("Hito: " + id, "La colonia alcanzó un hito de progresión y recibió recursos de estabilización.");
            }
            V5FeedbackSystem fb = FindFirstObjectByType<V5FeedbackSystem>();
            if (fb != null) fb.Push(LastMilestone, mother.transform.position, new Color(1f, 0.9f, 0.55f, 1f));
        }

        public bool IsCompleted(string id)
        {
            return completed.Contains(id);
        }
    }
}
