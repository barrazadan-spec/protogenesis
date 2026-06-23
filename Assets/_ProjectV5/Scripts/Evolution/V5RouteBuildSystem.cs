using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RouteBuildSystem : MonoBehaviour, IV5RunResettable
    {
        public int BacteriaStage;
        public int AmoebaStage;
        public int ProducerStage;
        public int VolvoxStage;
        public int TotalBuildMilestones;
        public V5MvpRoute ActiveRoute = V5MvpRoute.None;
        public int ActiveStage;
        public int ActiveTargetCount;
        public float ActiveProgress01;
        public V5AdaptationId ActiveNextTarget = V5AdaptationId.None;
        public string Summary = "Build MVP: sin ruta.";
        public string LastBuildMoment = "Build MVP listo.";

        private float scanTimer;

        public void ResetForNewRun()
        {
            BacteriaStage = 0;
            AmoebaStage = 0;
            ProducerStage = 0;
            VolvoxStage = 0;
            TotalBuildMilestones = 0;
            ActiveRoute = V5MvpRoute.None;
            ActiveStage = 0;
            ActiveTargetCount = 0;
            ActiveProgress01 = 0f;
            ActiveNextTarget = V5AdaptationId.None;
            Summary = "Build MVP: sin ruta.";
            LastBuildMoment = "Build MVP listo.";
            scanTimer = 0f;
        }

        private void Update()
        {
            scanTimer += Time.deltaTime;
            if (scanTimer < 0.45f) return;
            scanTimer = 0f;
            EvaluateNow(V5GameManager.Instance);
        }

        public bool EvaluateNow(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null)
            {
                Summary = "Build MVP: sin genoma.";
                return false;
            }

            RefreshSnapshot(gm);

            if (ActiveRoute == V5MvpRoute.None) return false;

            int previous = StoredStage(ActiveRoute);
            if (ActiveStage <= previous) return false;

            for (int stage = previous + 1; stage <= ActiveStage; stage++)
                CompleteBuildStage(ActiveRoute, stage, gm);

            StoreStage(ActiveRoute, ActiveStage);
            Summary = V5MvpCanon.BuildProgressText(ActiveRoute, gm.Adaptations);
            return true;
        }

        public void RefreshSnapshot(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null)
            {
                ActiveRoute = V5MvpRoute.None;
                ActiveStage = 0;
                ActiveTargetCount = 0;
                ActiveProgress01 = 0f;
                ActiveNextTarget = V5AdaptationId.None;
                Summary = "Build MVP: sin genoma.";
                return;
            }

            V5MvpRoute route = gm.MvpIntent != null ? gm.MvpIntent.EffectiveRoute(gm) : V5MvpCanon.CurrentRoute(gm);
            ActiveRoute = route;
            ActiveTargetCount = V5MvpCanon.BuildTargetCount(route);
            ActiveStage = V5MvpCanon.BuildStage(route, gm.Adaptations);
            ActiveProgress01 = V5MvpCanon.BuildProgress01(route, gm.Adaptations);
            ActiveNextTarget = V5MvpCanon.NextBuildTarget(route, gm.Adaptations);
            Summary = V5MvpCanon.BuildProgressText(route, gm.Adaptations);
        }

        public int StageFor(V5MvpRoute route)
        {
            return StoredStage(route);
        }

        private void CompleteBuildStage(V5MvpRoute route, int stage, V5GameManager gm)
        {
            TotalBuildMilestones++;
            V5AdaptationId target = V5MvpCanon.BuildTargetAt(route, stage - 1);
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(target);
            string targetName = def != null ? def.shortName : target.ToString();
            LastBuildMoment = "Build " + V5MvpCanon.DisplayName(route) + " " + stage + "/" +
                              V5MvpCanon.BuildTargetCount(route) + ": " +
                              V5MvpCanon.BuildStageName(route, stage) + " por " + targetName + ".";

            ApplyBuildPulse(route, stage, gm);

            if (gm != null && gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(route), 4f + stage, "build " + targetName, "mvp_build");
            if (gm != null && gm.Codex != null)
                gm.Codex.Unlock("Build MVP: " + V5MvpCanon.DisplayName(route), LastBuildMoment);
            if (gm != null && gm.Hud != null)
                gm.Hud.Toast(LastBuildMoment);
        }

        private void ApplyBuildPulse(V5MvpRoute route, int stage, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            float scale = Mathf.Max(1f, stage);
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    mother.Resources.biomass += 5f + scale * 3f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - (1f + scale * 0.5f));
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 3.4f, 0.004f, 0f, 0.003f, -0.006f, 0f, 0.012f + scale * 0.003f, 0f);
                    break;
                case V5MvpRoute.Amoeba:
                    mother.Resources.aminoAcids += 4f + scale * 3f;
                    if (stage >= 4) mother.Stats.physicalDamagePerSecond += 0.035f;
                    mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 3f + scale * 2f);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    mother.Resources.atp += 8f + scale * 4f;
                    mother.Resources.minerals += 2f + scale;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 4.8f, 0.004f, 0.010f + scale * 0.004f, 0.018f + scale * 0.005f, -0.008f, 0f, 0.008f, 0f);
                    break;
                case V5MvpRoute.Volvox:
                    mother.Resources.lipids += 4f + scale * 3f;
                    mother.Resources.nucleotides += 2f + scale * 2f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - (1.5f + scale * 0.6f));
                    if (gm.Body != null && stage >= 5) gm.Body.LastMessage = "Build Volvox: adhesion colonial estabilizada.";
                    break;
            }
        }

        private int StoredStage(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return BacteriaStage;
                case V5MvpRoute.Amoeba: return AmoebaStage;
                case V5MvpRoute.PhotosyntheticProducer: return ProducerStage;
                case V5MvpRoute.Volvox: return VolvoxStage;
                default: return 0;
            }
        }

        private void StoreStage(V5MvpRoute route, int stage)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaStage = stage; break;
                case V5MvpRoute.Amoeba: AmoebaStage = stage; break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerStage = stage; break;
                case V5MvpRoute.Volvox: VolvoxStage = stage; break;
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
    }
}
