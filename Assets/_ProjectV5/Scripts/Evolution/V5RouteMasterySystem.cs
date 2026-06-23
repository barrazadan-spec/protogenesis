using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RouteMasterySystem : MonoBehaviour, IV5RunResettable
    {
        private const float OpportunityMasteryGain = 0.34f;

        public float BacteriaMastery;
        public float AmoebaMastery;
        public float ProducerMastery;
        public float VolvoxMastery;
        public int BacteriaCompletions;
        public int AmoebaCompletions;
        public int ProducerCompletions;
        public int VolvoxCompletions;
        public int TotalCompletions;
        public V5MvpRoute BestRoute = V5MvpRoute.None;
        public float BestMastery01;
        public string Summary = "Mastery MVP: sin memoria de ruta.";
        public string LastMasteryMoment = "Memoria MVP lista.";

        public void ResetForNewRun()
        {
            BacteriaMastery = 0f;
            AmoebaMastery = 0f;
            ProducerMastery = 0f;
            VolvoxMastery = 0f;
            BacteriaCompletions = 0;
            AmoebaCompletions = 0;
            ProducerCompletions = 0;
            VolvoxCompletions = 0;
            TotalCompletions = 0;
            BestRoute = V5MvpRoute.None;
            BestMastery01 = 0f;
            Summary = "Mastery MVP: sin memoria de ruta.";
            LastMasteryMoment = "Memoria MVP lista.";
        }

        public float Mastery01(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return BacteriaMastery;
                case V5MvpRoute.Amoeba: return AmoebaMastery;
                case V5MvpRoute.PhotosyntheticProducer: return ProducerMastery;
                case V5MvpRoute.Volvox: return VolvoxMastery;
                default: return 0f;
            }
        }

        public int CompletionCount(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return BacteriaCompletions;
                case V5MvpRoute.Amoeba: return AmoebaCompletions;
                case V5MvpRoute.PhotosyntheticProducer: return ProducerCompletions;
                case V5MvpRoute.Volvox: return VolvoxCompletions;
                default: return 0;
            }
        }

        public float NicheRewardMultiplier(V5MvpRoute route)
        {
            return 1f + Mastery01(route) * 0.35f;
        }

        public float NicheStressMultiplier(V5MvpRoute route)
        {
            return Mathf.Lerp(1f, 0.72f, Mastery01(route));
        }

        public void RecordOpportunity(V5MvpRoute route, V5GameManager gm)
        {
            if (route == V5MvpRoute.None) return;

            SetMastery(route, Mathf.Clamp01(Mastery01(route) + OpportunityMasteryGain));
            IncrementCompletion(route);
            TotalCompletions++;
            RebuildSummary();
            ApplyMasteryPulse(route, gm);

            if (gm != null && gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(route), 10f, "oportunidad MVP " + V5MvpCanon.DisplayName(route), "mvp_mastery");

            LastMasteryMoment = "Mastery " + V5MvpCanon.DisplayName(route) + " " + Percent(Mastery01(route)) +
                                " por oportunidad completada.";
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMasteryMoment);
        }

        private void SetMastery(V5MvpRoute route, float value)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaMastery = value; break;
                case V5MvpRoute.Amoeba: AmoebaMastery = value; break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerMastery = value; break;
                case V5MvpRoute.Volvox: VolvoxMastery = value; break;
            }
        }

        private void IncrementCompletion(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaCompletions++; break;
                case V5MvpRoute.Amoeba: AmoebaCompletions++; break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerCompletions++; break;
                case V5MvpRoute.Volvox: VolvoxCompletions++; break;
            }
        }

        private void RebuildSummary()
        {
            BestRoute = V5MvpRoute.None;
            BestMastery01 = 0f;
            ConsiderBest(V5MvpRoute.Bacteria, BacteriaMastery);
            ConsiderBest(V5MvpRoute.Amoeba, AmoebaMastery);
            ConsiderBest(V5MvpRoute.PhotosyntheticProducer, ProducerMastery);
            ConsiderBest(V5MvpRoute.Volvox, VolvoxMastery);

            if (BestRoute == V5MvpRoute.None)
            {
                Summary = "Mastery MVP: sin memoria de ruta.";
                return;
            }

            Summary = "Mastery " + V5MvpCanon.DisplayName(BestRoute) + " " + Percent(BestMastery01) +
                      " | B " + Percent(BacteriaMastery) +
                      " A " + Percent(AmoebaMastery) +
                      " P " + Percent(ProducerMastery) +
                      " V " + Percent(VolvoxMastery);
        }

        private void ConsiderBest(V5MvpRoute route, float value)
        {
            if (value <= BestMastery01) return;
            BestMastery01 = value;
            BestRoute = route;
        }

        private void ApplyMasteryPulse(V5MvpRoute route, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            float mastery = Mastery01(route);
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    mother.Resources.biomass += 6f + mastery * 8f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - (1.5f + mastery * 2f));
                    break;
                case V5MvpRoute.Amoeba:
                    mother.Resources.aminoAcids += 5f + mastery * 7f;
                    mother.Stats.physicalDamagePerSecond += 0.03f + mastery * 0.03f;
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    mother.Resources.atp += 10f + mastery * 14f;
                    mother.Stats.synthesisRate += 0.01f + mastery * 0.02f;
                    break;
                case V5MvpRoute.Volvox:
                    mother.Resources.lipids += 5f + mastery * 8f;
                    mother.Resources.nucleotides += 3f + mastery * 5f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - (2f + mastery * 3f));
                    break;
            }
        }

        private V5EvolutionPath RouteToAffinityPath(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return V5EvolutionPath.Bacteria;
                case V5MvpRoute.Amoeba: return V5EvolutionPath.Amoeba;
                case V5MvpRoute.PhotosyntheticProducer: return V5EvolutionPath.Cyanobacteria;
                case V5MvpRoute.Volvox: return V5EvolutionPath.Microalga;
                default: return V5EvolutionPath.Uncommitted;
            }
        }

        private string Percent(float value)
        {
            return (Mathf.Clamp01(value) * 100f).ToString("0") + "%";
        }
    }
}
