using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RouteClimaxSystem : MonoBehaviour, IV5RunResettable
    {
        public V5MvpRoute ActiveRoute = V5MvpRoute.None;
        public V5RouteBranchId ActiveBranch = V5RouteBranchId.None;
        public float BuildScore01;
        public float GoalScore01;
        public float MasteryScore01;
        public float ChapterScore01;
        public float BranchScore01;
        public float BranchObjectiveScore01;
        public float BranchDoctrineObjectiveScore01;
        public float OpportunityScore01;
        public float ComboScore01;
        public float ClimaxScore01;
        public bool VictoryReady;
        public bool VictoryClaimed;
        public bool BranchFinaleReady;
        public int BranchObjectiveCompletions;
        public int BranchDoctrineObjectiveCompletions;
        public string ActiveBranchName = "sin rama";
        public string BranchFinaleTitle = "sin final de rama";
        public V5BranchDoctrineChoice ActiveBranchDoctrine = V5BranchDoctrineChoice.None;
        public string ActiveBranchDoctrineName = "sin doctrina";
        public string Summary = "Climax MVP: sin ruta.";
        public string LastClimaxMoment = "Climax de ruta listo para evaluar.";

        private float tick;
        private bool lastReady;

        public void ResetForNewRun()
        {
            ActiveRoute = V5MvpRoute.None;
            ActiveBranch = V5RouteBranchId.None;
            BuildScore01 = 0f;
            GoalScore01 = 0f;
            MasteryScore01 = 0f;
            ChapterScore01 = 0f;
            BranchScore01 = 0f;
            BranchObjectiveScore01 = 0f;
            BranchDoctrineObjectiveScore01 = 0f;
            OpportunityScore01 = 0f;
            ComboScore01 = 0f;
            ClimaxScore01 = 0f;
            VictoryReady = false;
            VictoryClaimed = false;
            BranchFinaleReady = false;
            BranchObjectiveCompletions = 0;
            BranchDoctrineObjectiveCompletions = 0;
            ActiveBranchName = "sin rama";
            BranchFinaleTitle = "sin final de rama";
            ActiveBranchDoctrine = V5BranchDoctrineChoice.None;
            ActiveBranchDoctrineName = "sin doctrina";
            Summary = "Climax MVP: sin ruta.";
            LastClimaxMoment = "Climax de ruta listo para evaluar.";
            tick = 0f;
            lastReady = false;
        }

        private void Update()
        {
            tick += Time.deltaTime;
            if (tick < 0.75f) return;
            tick = 0f;
            RefreshNow(V5GameManager.Instance);
        }

        public void RefreshNow(V5GameManager gm)
        {
            if (gm == null)
            {
                ResetSnapshot("Climax MVP: sin GameManager.");
                return;
            }

            ActiveRoute = gm.MvpIntent != null ? gm.MvpIntent.EffectiveRoute(gm) : V5MvpCanon.CurrentRoute(gm);
            if (ActiveRoute == V5MvpRoute.None)
            {
                ResetSnapshot("Climax MVP: elige una ruta.");
                return;
            }

            BuildScore01 = V5MvpCanon.BuildProgress01(ActiveRoute, gm.Adaptations);
            GoalScore01 = gm.MvpIntent != null ? RouteGoalScore(gm, ActiveRoute) : 0f;
            MasteryScore01 = gm.RouteMastery != null ? gm.RouteMastery.Mastery01(ActiveRoute) : 0f;
            ChapterScore01 = gm.RouteChapters != null ? gm.RouteChapters.RouteChapterScore01(ActiveRoute) : 0f;
            if (gm.RouteBranches != null) gm.RouteBranches.EvaluateNow(gm);
            ActiveBranch = gm.RouteBranches != null ? gm.RouteBranches.BranchForRoute(ActiveRoute) : V5RouteBranchId.None;
            ActiveBranchName = gm.RouteBranches != null ? gm.RouteBranches.BranchName(ActiveBranch) : "sin rama";
            BranchScore01 = gm.RouteBranches != null ? gm.RouteBranches.BranchScoreForRoute(ActiveRoute) : 0f;
            BranchObjectiveScore01 = gm.RouteBranches != null ? gm.RouteBranches.BranchObjectiveProgress01 : 0f;
            BranchObjectiveCompletions = gm.RouteBranches != null ? gm.RouteBranches.BranchObjectiveCompletions : 0;
            BranchDoctrineObjectiveScore01 = gm.RouteBranches != null ? gm.RouteBranches.BranchDoctrineObjectiveProgress01 : 0f;
            BranchDoctrineObjectiveCompletions = gm.RouteBranches != null ? gm.RouteBranches.BranchDoctrineObjectiveCompletions : 0;
            BranchFinaleTitle = BranchFinaleName(ActiveBranch);
            ActiveBranchDoctrine = gm.RouteBranches != null ? gm.RouteBranches.DoctrineForBranch(ActiveBranch) : V5BranchDoctrineChoice.None;
            ActiveBranchDoctrineName = gm.RouteBranches != null ? gm.RouteBranches.DoctrineNameForBranch(ActiveBranch) : "sin doctrina";
            if (gm.RouteBranches != null)
                BranchFinaleTitle = gm.RouteBranches.DoctrineFinaleNameForBranch(ActiveBranch, BranchFinaleTitle);
            bool branchObjectiveReady = gm.RouteBranches == null || ActiveBranch == V5RouteBranchId.None || gm.RouteBranches.IsBranchObjectiveCompleted(ActiveBranch);
            bool doctrineObjectiveReady = gm.RouteBranches == null ||
                                          ActiveBranchDoctrine == V5BranchDoctrineChoice.None ||
                                          gm.RouteBranches.IsBranchDoctrineObjectiveCompleted(ActiveBranch, ActiveBranchDoctrine);
            BranchFinaleReady = branchObjectiveReady && doctrineObjectiveReady;
            int opportunities = gm.RouteMastery != null ? gm.RouteMastery.CompletionCount(ActiveRoute) : 0;
            OpportunityScore01 = Mathf.Clamp01(opportunities / 2f);
            int combos = gm.WorldEvents != null ? gm.WorldEvents.ComboCountForRoute(ActiveRoute) : 0;
            ComboScore01 = Mathf.Clamp01(combos / 2f);

            ClimaxScore01 = Mathf.Clamp01(BuildScore01 * 0.26f +
                                          GoalScore01 * 0.18f +
                                          MasteryScore01 * 0.15f +
                                          ChapterScore01 * 0.08f +
                                          BranchScore01 * 0.07f +
                                          BranchObjectiveScore01 * 0.07f +
                                          BranchDoctrineObjectiveScore01 * 0.03f +
                                          OpportunityScore01 * 0.08f +
                                          ComboScore01 * 0.08f);

            VictoryReady = BuildScore01 >= 0.85f &&
                           GoalScore01 >= 0.95f &&
                           MasteryScore01 >= 0.34f &&
                           opportunities >= 1 &&
                           combos >= 1 &&
                           BranchFinaleReady &&
                           ClimaxScore01 >= 0.86f;

            Summary = "Climax " + V5MvpCanon.DisplayName(ActiveRoute) + " " + Percent(ClimaxScore01) +
                      " | " + BranchFinaleTitle +
                      " | build " + Percent(BuildScore01) +
                      " meta " + Percent(GoalScore01) +
                      " mastery " + Percent(MasteryScore01) +
                      " cap " + Percent(ChapterScore01) +
                      " rama " + ActiveBranchName + " " + Percent(BranchScore01) +
                      (ActiveBranchDoctrine != V5BranchDoctrineChoice.None ? " doctrina " + ActiveBranchDoctrineName : "") +
                      " obj " + Percent(BranchObjectiveScore01) +
                      (ActiveBranchDoctrine != V5BranchDoctrineChoice.None ? " objDoc " + Percent(BranchDoctrineObjectiveScore01) : "") +
                      " opp " + opportunities +
                      " combo " + combos +
                      (VictoryReady ? " | victoria lista" : "");

            if (VictoryReady && !lastReady)
            {
                LastClimaxMoment = "Climax " + V5MvpCanon.DisplayName(ActiveRoute) + " / " + ActiveBranchName + " listo: " + BranchFinaleTitle + ".";
                if (gm.Hud != null) gm.Hud.Toast(LastClimaxMoment);
                if (gm.Codex != null) gm.Codex.Unlock("Climax MVP: " + V5MvpCanon.DisplayName(ActiveRoute), LastClimaxMoment);
            }
            lastReady = VictoryReady;
        }

        public bool ClaimVictory(V5GameManager gm)
        {
            if (gm == null || VictoryClaimed) return false;
            RefreshNow(gm);
            if (!VictoryReady) return false;

            VictoryClaimed = true;
            LastClimaxMoment = "Victoria por climax " + V5MvpCanon.DisplayName(ActiveRoute) + " / " + ActiveBranchName + ": " + BranchFinaleTitle + ".";
            gm.Win("climax de ruta: " + V5MvpCanon.DisplayName(ActiveRoute) + " / " + ActiveBranchName);
            return true;
        }

        private float RouteGoalScore(V5GameManager gm, V5MvpRoute route)
        {
            if (gm == null || gm.MvpIntent == null) return 0f;
            if (gm.MvpIntent.IsRouteGoalCompleted(route)) return 1f;
            return gm.MvpIntent.RouteGoalProgress01(gm);
        }

        private void ResetSnapshot(string summary)
        {
            ActiveRoute = V5MvpRoute.None;
            ActiveBranch = V5RouteBranchId.None;
            BuildScore01 = 0f;
            GoalScore01 = 0f;
            MasteryScore01 = 0f;
            ChapterScore01 = 0f;
            BranchScore01 = 0f;
            BranchObjectiveScore01 = 0f;
            BranchDoctrineObjectiveScore01 = 0f;
            OpportunityScore01 = 0f;
            ComboScore01 = 0f;
            ClimaxScore01 = 0f;
            VictoryReady = false;
            BranchFinaleReady = false;
            BranchObjectiveCompletions = 0;
            BranchDoctrineObjectiveCompletions = 0;
            ActiveBranchName = "sin rama";
            BranchFinaleTitle = "sin final de rama";
            ActiveBranchDoctrine = V5BranchDoctrineChoice.None;
            ActiveBranchDoctrineName = "sin doctrina";
            Summary = summary;
            lastReady = false;
        }

        private string BranchFinaleName(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "Dominio de Matriz Viva";
                case V5RouteBranchId.BacteriaSwarm: return "Estampida Microbiana";
                case V5RouteBranchId.AmoebaHunter: return "Corona Depredadora";
                case V5RouteBranchId.AmoebaDigestive: return "Motor Metabolico";
                case V5RouteBranchId.ProducerBloom: return "Aurora Fotosintetica";
                case V5RouteBranchId.ProducerTerraformer: return "Jardin Terraformado";
                case V5RouteBranchId.VolvoxBody: return "Cuerpo Coral Sincronico";
                case V5RouteBranchId.VolvoxCaste: return "Ciudad de Castas";
                default: return "Final de ruta sin rama";
            }
        }

        private string Percent(float value)
        {
            return (Mathf.Clamp01(value) * 100f).ToString("0") + "%";
        }
    }
}
