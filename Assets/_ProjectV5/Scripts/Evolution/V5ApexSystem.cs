using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ApexSystem : MonoBehaviour, IV5RunResettable
    {
        public bool ApexSpawned;
        public V5ApexForm LastApex = V5ApexForm.None;
        public string LastMessage = "";

        public void ResetRun()
        {
            ApexSpawned = false;
            LastApex = V5ApexForm.None;
            LastMessage = "Forma apex bloqueada.";
        }

        void IV5RunResettable.ResetForNewRun() => ResetRun();

        public V5ApexForm RecommendedForm(V5CellEntity mother)
        {
            if (mother == null) return V5ApexForm.None;
            if (mother.Domain == V5CellDomain.Prokaryote)
            {
                if (mother.EvolutionPath == V5EvolutionPath.Cyanobacteria || mother.EvolutionPath == V5EvolutionPath.Bacteria) return V5ApexForm.Volvox;
                return V5ApexForm.Hydra;
            }

            if (mother.EvolutionPath == V5EvolutionPath.Microalga) return V5ApexForm.Volvox;
            if (mother.EvolutionPath == V5EvolutionPath.Ciliate || mother.EvolutionPath == V5EvolutionPath.Rotifer || mother.EvolutionPath == V5EvolutionPath.StemCell) return V5ApexForm.Paramecium;
            if (mother.EvolutionPath == V5EvolutionPath.Tardigrade || mother.EvolutionPath == V5EvolutionPath.Nematode) return V5ApexForm.Tardigrade;
            return V5ApexForm.Tardigrade;
        }

        public bool CanSpawn(V5ApexForm form, V5CellEntity mother)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null || mother == null) return false;
            if (ApexSpawned) return false;
            if (form == V5ApexForm.None) return false;
            if (gm.ElapsedSeconds < V5Balance.ApexMinimumTime) return false;
            bool legacyApexGene = gm.Genes != null && gm.Genes.HasGene(V5GeneId.ApexMaturation);
            bool adaptationApex = gm.Adaptations != null && gm.Adaptations.Has(V5AdaptationId.BiologicalChampion);
            if (!legacyApexGene && !adaptationApex) return false;
            if (!RouteMature(mother)) return false;
            V5ResourceWallet cost = ApexCost(form);
            return mother.Resources.CanPay(cost);
        }

        private bool RouteMature(V5CellEntity mother)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (mother == null) return false;
            if (gm != null && gm.Adaptations != null && gm.Adaptations.Has(V5AdaptationId.BiologicalChampion) && gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA) return true;
            if (mother.EvolutionPath == V5EvolutionPath.Uncommitted) return false;
            if (gm != null && gm.RouteLifecycle != null) return gm.RouteLifecycle.IsApexReadyFor(mother.EvolutionPath);
            return V5EvolutionAffinitySystem.Score01(mother, mother.EvolutionPath) >= V5Balance.RouteApexAffinityThreshold;
        }

        public V5ResourceWallet ApexCost(V5ApexForm form)
        {
            float mult = 1f;
            if (form == V5ApexForm.Hydra) mult = 1.05f;
            if (form == V5ApexForm.Tardigrade) mult = 1.12f;
            if (form == V5ApexForm.Paramecium) mult = 0.95f;
            return V5ResourceWallet.Cost(V5Balance.ApexCostATP * mult, V5Balance.ApexCostBiomass * mult, V5Balance.ApexCostAminoAcids * mult, 18f * mult, V5Balance.ApexCostNucleotides * mult, 10f * mult);
        }

        public V5CellEntity SpawnRecommended()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return null;
            return Spawn(RecommendedForm(gm.MotherCell));
        }

        public V5CellEntity Spawn(V5ApexForm form)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || gm.CellFactory == null) return null;
            V5CellEntity mother = gm.MotherCell;
            if (!CanSpawn(form, mother))
            {
                LastMessage = "Apex no disponible: requiere Campeon biologico/Maduracion Apex, 12 min, identidad madura y recursos.";
                return null;
            }

            mother.Resources.Pay(ApexCost(form));
            Vector2 spawn = (Vector2)mother.transform.position + Random.insideUnitCircle.normalized * 2.2f;
            V5CellEntity apex = gm.CellFactory.SpawnApex(spawn, form, mother);
            ApexSpawned = apex != null;
            LastApex = form;
            LastMessage = ApexSpawned ? "Forma apex emergio: " + form : "No se pudo invocar apex.";
            return apex;
        }
    }
}
