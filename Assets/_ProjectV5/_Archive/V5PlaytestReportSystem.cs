using System;
using System.IO;
using UnityEngine;

namespace Protogenesis.V5
{
    [Serializable]
    public class V5PlaytestReportData
    {
        public string build = "V5 Prototype 1.16.0";
        public string schema = "playtest_report_v5_16_0";
        public string trigger;
        public string timestampUtc;
        public V5ScenarioId scenario;
        public V5GamePhase phase;
        public float elapsedSeconds;
        public int playerCells;
        public int enemies;
        public float colonization;
        public float oxygen;
        public float toxins;
        public float acidity;
        public float fpsEstimate;
        public string genomeMode;
        public string genes;
        public string adaptations;
        public string identity;
        public string mvpRoute;
        public string mvpRouteGoal;
        public float mvpRouteGoalProgress;
        public int mvpRoutesCompleted;
        public string mvpRouteMilestone;
        public string mvpIdentityMoment;
        public int mvpRouteWorldCueCount;
        public string mvpRouteWorldCue;
        public int mvpMicroObjectiveCompletedCount;
        public string mvpMicroObjective;
        public string mvpMicroMilestone;
        public float mvpMicroObjectiveProgress;
        public string nicheSummary;
        public string nicheAdvice;
        public string nicheEffect;
        public float nicheScore;
        public string nicheBand;
        public int nicheFavorablePulses;
        public int nicheStressPulses;
        public string worldEvent;
        public int worldEventCount;
        public string routeEcologyEvent;
        public string routeEcologyRoute;
        public int routeEcologyEventCount;
        public float routeEcologyNicheScore;
        public bool routeOpportunityActive;
        public string routeOpportunity;
        public float routeOpportunityProgress;
        public int routeOpportunityCompletedCount;
        public string routeOpportunityReward;
        public int routeAbilityComboCount;
        public string routeAbilityComboRoute;
        public string routeAbilityCombo;
        public float routeAbilityComboPower;
        public string routeMasterySummary;
        public string routeMasteryBestRoute;
        public float routeMasteryBestScore;
        public int routeMasteryTotalCompletions;
        public string routeMasteryLastMoment;
        public string routeBuildSummary;
        public string routeBuildActiveRoute;
        public int routeBuildStage;
        public int routeBuildTargetCount;
        public float routeBuildProgress;
        public int routeBuildMilestones;
        public string routeBuildNextTarget;
        public string routeBuildLastMoment;
        public string routeAbilityStatus;
        public string routeAbilityLast;
        public int routeAbilityCasts;
        public string routeClimaxSummary;
        public string routeClimaxRoute;
        public float routeClimaxScore;
        public bool routeClimaxVictoryReady;
        public string routeClimaxLastMoment;
        public string routeClimaxBranch;
        public string routeClimaxBranchFinale;
        public string routeClimaxBranchDoctrine;
        public string routeClimaxBranchDoctrineChoice;
        public float routeClimaxBranchObjectiveScore;
        public bool routeClimaxBranchFinaleReady;
        public string routeChapterSummary;
        public string routeChapterCurrent;
        public string routeChapterRoute;
        public int routeChapterActive;
        public float routeChapterProgress;
        public int routeChapterTotalCompletions;
        public int routeChapterCompletedForRoute;
        public string routeChapterLastMoment;
        public string routeBranchSummary;
        public string routeBranchRoute;
        public string routeBranchActive;
        public string routeBranchRunnerUp;
        public float routeBranchScore;
        public float routeBranchRunnerUpScore;
        public int routeBranchEstablishments;
        public string routeBranchLastMoment;
        public int routeBranchAbilitySynergyCount;
        public string routeBranchAbilitySynergy;
        public int routeBranchDoctrineAbilityCount;
        public string routeBranchDoctrineAbility;
        public string routeBranchDoctrineObjective;
        public float routeBranchDoctrineObjectiveProgress;
        public int routeBranchDoctrineObjectiveCompletions;
        public string routeBranchDoctrineObjectiveLastMoment;
        public bool routeBranchDoctrineObjectiveActive;
        public string routeBranchObjective;
        public float routeBranchObjectiveProgress;
        public int routeBranchObjectiveCompletions;
        public string routeBranchObjectiveLastMoment;
        public float routeBranchPassiveReadiness;
        public int routeBranchPassivePulseCount;
        public string routeBranchPassiveEffect;
        public string routeBranchVisualCue;
        public bool routeBranchAuraVisible;
        public float routeBranchAuraRadius;
        public bool routeBranchDoctrineAvailable;
        public int routeBranchDoctrineCommitments;
        public string routeBranchDoctrineChoice;
        public string routeBranchDoctrineOffer;
        public string routeBranchDoctrineMoment;
        public float routeBranchTradeoffPressure;
        public string routeCounterSummary;
        public bool routeCounterActive;
        public string routeCounterRoute;
        public string routeCounterBranch;
        public int routeCounterCount;
        public int routeCounterAnsweredCount;
        public string routeCounterLast;
        public string routeCounterAdvice;
        public string routeCounterplayResult;
        public float routeCounterPressure;
        public string routeCounterVisualCue;
        public bool routeCounterMarkerVisible;
        public string routeCounterDoctrine;
        public string routeCounterDoctrineChoice;
        public float routeCounterDoctrineMultiplier;
        public int activeAdaptationCount;
        public int activeAdaptationCap;
        public float adaptationInstalls;
        public float milestoneInstalls;
        public float adaptationFailures;
        public float capBlockedAttempts;
        public string adaptationSummary;
        public string adaptationRoute;
        public string adaptationFailureSummary;
        public string topAdaptations;
        public string mother;
        public string resources;
        public string body;
        public string squad;
        public string battlefield;
        public string playableLoop;
        public int diagnosticsScore;
        public string diagnosticsStatus;
        public string diagnosticsAdvice;
        public string diagnosticsCoachAdvice;
        public string diagnosticsCoachAction;
        public string diagnosticsCoachAdaptation;
        public string diagnosticsCoachAdaptationStatus;
        public string diagnostics;
        public int liveCoachNotifications;
        public string liveCoachSummary;
        public string liveCoachLastAction;
        public string liveCoachSuggestedAdaptation;
        public string liveCoachSuggestedAdaptationStatus;
        public string telemetry;
        public string tutorial;
        public string summary;
        public string readiness;
    }

    /// <summary>
    /// Exports a single-file JSON report for balancing/playtesting.
    /// Stored at Application.persistentDataPath and also copied to PlayerPrefs for quick access.
    /// </summary>
    public class V5PlaytestReportSystem : MonoBehaviour
    {
        public string LastPath { get; private set; }
        public string LastMessage { get; private set; }
        private const string LastReportKey = "ProtogenesisV5LastReport";

        private void Update()
        {
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.F7)) ExportReport("ctrl_f7");
        }

        public V5PlaytestReportData CreateReport(string trigger)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5PlaytestReportData data = new V5PlaytestReportData();
            data.trigger = trigger;
            data.timestampUtc = DateTime.UtcNow.ToString("o");
            if (gm == null) return data;

            data.scenario = gm.ScenarioId;
            data.phase = gm.Phase;
            data.elapsedSeconds = gm.ElapsedSeconds;
            data.playerCells = gm.PlayerCellCount();
            data.enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            if (gm.Environment != null)
            {
                data.colonization = gm.Environment.AverageColonization();
                data.oxygen = gm.Environment.AverageOxygen();
                data.toxins = gm.Environment.AverageToxins();
                data.acidity = gm.Environment.AverageAcidity();
            }
            V5OptimizationGuardSystem opt = FindFirstObjectByType<V5OptimizationGuardSystem>();
            data.fpsEstimate = opt != null ? opt.EstimatedFps : 0f;
            data.genomeMode = gm.Adaptations != null ? "Adaptations" : "LegacyGenes";
            data.genes = gm.Genes != null ? gm.Genes.Summary() : "sin genes";
            if (gm.Adaptations != null)
            {
                data.adaptations = gm.Adaptations.InstalledNames();
                data.activeAdaptationCount = gm.Adaptations.ActiveCount();
                data.activeAdaptationCap = gm.Adaptations.ActiveCap;
            }
            else
            {
                data.adaptations = "sin adaptaciones";
            }
            data.identity = gm.Identity != null ? gm.Identity.Summary : "sin identidad";
            if (gm.MvpIntent != null)
            {
                V5MvpRoute route = gm.MvpIntent.EffectiveRoute(gm);
                data.mvpRoute = V5MvpCanon.DisplayName(route);
                data.mvpRouteGoal = gm.MvpIntent.RouteGoalText(gm);
                data.mvpRouteGoalProgress = gm.MvpIntent.RouteGoalProgress01(gm);
                data.mvpRoutesCompleted = gm.MvpIntent.CompletedGoalCount;
                data.mvpRouteMilestone = gm.MvpIntent.LastGoalMilestone;
                data.mvpIdentityMoment = gm.MvpIntent.LastIdentityMoment;
                data.mvpRouteWorldCueCount = gm.MvpIntent.WorldCueCount;
                data.mvpRouteWorldCue = gm.MvpIntent.LastWorldCue;
                data.mvpMicroObjectiveCompletedCount = gm.MvpIntent.MicroObjectiveCompletedCount;
                data.mvpMicroObjective = gm.MvpIntent.RouteMicroObjectiveText(gm);
                data.mvpMicroMilestone = gm.MvpIntent.LastMicroMilestone;
                data.mvpMicroObjectiveProgress = gm.MvpIntent.RouteMicroObjectiveProgress01(gm);
            }
            else
            {
                data.mvpRoute = "sin sistema MVP";
                data.mvpRouteGoal = "sin meta MVP";
                data.mvpRouteMilestone = "sin hito MVP";
                data.mvpIdentityMoment = "sin momento MVP";
                data.mvpRouteWorldCue = "sin cue mundo MVP";
                data.mvpMicroObjective = "sin micro MVP";
                data.mvpMicroMilestone = "sin micro hito MVP";
            }
            if (gm.NichePressure != null)
            {
                gm.NichePressure.ScanNow(gm);
                data.nicheSummary = gm.NichePressure.Summary;
                data.nicheAdvice = gm.NichePressure.LastNicheAdvice;
                data.nicheEffect = gm.NichePressure.LastNicheEffect;
                data.nicheScore = gm.NichePressure.CurrentScore;
                data.nicheBand = gm.NichePressure.CurrentBand;
                data.nicheFavorablePulses = gm.NichePressure.FavorablePulseCount;
                data.nicheStressPulses = gm.NichePressure.StressPulseCount;
            }
            else
            {
                data.nicheSummary = "sin presion de nicho";
                data.nicheAdvice = "sin consejo de nicho";
                data.nicheEffect = "sin efecto de nicho";
                data.nicheBand = "sin nicho";
            }
            if (gm.WorldEvents != null)
            {
                data.worldEvent = gm.WorldEvents.CurrentEvent;
                data.worldEventCount = gm.WorldEvents.EventCount;
                data.routeEcologyEvent = gm.WorldEvents.LastRouteEventSummary;
                data.routeEcologyRoute = V5MvpCanon.DisplayName(gm.WorldEvents.LastRouteEvent);
                data.routeEcologyEventCount = gm.WorldEvents.RouteEventCount;
                data.routeEcologyNicheScore = gm.WorldEvents.LastRouteEventNicheScore;
                data.routeOpportunityActive = gm.WorldEvents.RouteOpportunityActive;
                data.routeOpportunity = gm.WorldEvents.RouteOpportunitySummary;
                data.routeOpportunityProgress = gm.WorldEvents.RouteOpportunityProgress01;
                data.routeOpportunityCompletedCount = gm.WorldEvents.RouteOpportunityCompletedCount;
                data.routeOpportunityReward = gm.WorldEvents.LastRouteOpportunityReward;
                data.routeAbilityComboCount = gm.WorldEvents.RouteAbilityComboCount;
                data.routeAbilityComboRoute = V5MvpCanon.DisplayName(gm.WorldEvents.LastRouteAbilityComboRoute);
                data.routeAbilityCombo = gm.WorldEvents.LastRouteAbilityCombo;
                data.routeAbilityComboPower = gm.WorldEvents.LastRouteAbilityComboPower;
            }
            else
            {
                data.worldEvent = "sin eventos de mundo";
                data.routeEcologyEvent = "sin evento ecologico de ruta";
                data.routeEcologyRoute = "sin ruta";
                data.routeOpportunity = "sin oportunidad de ruta";
                data.routeOpportunityReward = "sin recompensa de oportunidad";
                data.routeAbilityComboRoute = "sin ruta";
                data.routeAbilityCombo = "sin combo de habilidad";
            }
            if (gm.RouteMastery != null)
            {
                data.routeMasterySummary = gm.RouteMastery.Summary;
                data.routeMasteryBestRoute = V5MvpCanon.DisplayName(gm.RouteMastery.BestRoute);
                data.routeMasteryBestScore = gm.RouteMastery.BestMastery01;
                data.routeMasteryTotalCompletions = gm.RouteMastery.TotalCompletions;
                data.routeMasteryLastMoment = gm.RouteMastery.LastMasteryMoment;
            }
            else
            {
                data.routeMasterySummary = "sin mastery de ruta";
                data.routeMasteryBestRoute = "sin ruta";
                data.routeMasteryLastMoment = "sin memoria de ruta";
            }
            if (gm.RouteBuilds != null)
            {
                gm.RouteBuilds.RefreshSnapshot(gm);
                data.routeBuildSummary = gm.RouteBuilds.Summary;
                data.routeBuildActiveRoute = V5MvpCanon.DisplayName(gm.RouteBuilds.ActiveRoute);
                data.routeBuildStage = gm.RouteBuilds.ActiveStage;
                data.routeBuildTargetCount = gm.RouteBuilds.ActiveTargetCount;
                data.routeBuildProgress = gm.RouteBuilds.ActiveProgress01;
                data.routeBuildMilestones = gm.RouteBuilds.TotalBuildMilestones;
                V5AdaptationDefinition nextBuild = V5AdaptationLibrary.Get(gm.RouteBuilds.ActiveNextTarget);
                data.routeBuildNextTarget = nextBuild != null ? nextBuild.shortName : "build completo";
                data.routeBuildLastMoment = gm.RouteBuilds.LastBuildMoment;
            }
            else
            {
                data.routeBuildSummary = "sin build de ruta";
                data.routeBuildActiveRoute = "sin ruta";
                data.routeBuildNextTarget = "sin siguiente build";
                data.routeBuildLastMoment = "sin momento de build";
            }
            if (gm.Abilities != null)
            {
                data.routeAbilityStatus = gm.Abilities.RouteFantasyStatus(gm);
                data.routeAbilityLast = gm.Abilities.LastRouteFantasyAbility;
                data.routeAbilityCasts = gm.Abilities.RouteFantasyCastCount;
            }
            else
            {
                data.routeAbilityStatus = "sin sistema de habilidades";
                data.routeAbilityLast = "sin habilidad de ruta";
            }
            if (gm.RouteClimax != null)
            {
                gm.RouteClimax.RefreshNow(gm);
                data.routeClimaxSummary = gm.RouteClimax.Summary;
                data.routeClimaxRoute = V5MvpCanon.DisplayName(gm.RouteClimax.ActiveRoute);
                data.routeClimaxScore = gm.RouteClimax.ClimaxScore01;
                data.routeClimaxVictoryReady = gm.RouteClimax.VictoryReady;
                data.routeClimaxLastMoment = gm.RouteClimax.LastClimaxMoment;
                data.routeClimaxBranch = gm.RouteClimax.ActiveBranchName;
                data.routeClimaxBranchFinale = gm.RouteClimax.BranchFinaleTitle;
                data.routeClimaxBranchDoctrine = gm.RouteClimax.ActiveBranchDoctrineName;
                data.routeClimaxBranchDoctrineChoice = gm.RouteClimax.ActiveBranchDoctrine.ToString();
                data.routeClimaxBranchObjectiveScore = gm.RouteClimax.BranchObjectiveScore01;
                data.routeClimaxBranchFinaleReady = gm.RouteClimax.BranchFinaleReady;
            }
            else
            {
                data.routeClimaxSummary = "sin climax de ruta";
                data.routeClimaxRoute = "sin ruta";
                data.routeClimaxLastMoment = "sin momento climax";
                data.routeClimaxBranch = "sin rama";
                data.routeClimaxBranchFinale = "sin final de rama";
                data.routeClimaxBranchDoctrine = "sin doctrina";
                data.routeClimaxBranchDoctrineChoice = "None";
            }
            if (gm.RouteChapters != null)
            {
                gm.RouteChapters.EvaluateNow(gm);
                data.routeChapterSummary = gm.RouteChapters.Summary;
                data.routeChapterCurrent = gm.RouteChapters.CurrentChapterText;
                data.routeChapterRoute = V5MvpCanon.DisplayName(gm.RouteChapters.ActiveRoute);
                data.routeChapterActive = gm.RouteChapters.ActiveChapter;
                data.routeChapterProgress = gm.RouteChapters.ActiveChapterProgress01;
                data.routeChapterTotalCompletions = gm.RouteChapters.TotalChapterCompletions;
                data.routeChapterCompletedForRoute = gm.RouteChapters.CompletedFor(gm.RouteChapters.ActiveRoute);
                data.routeChapterLastMoment = gm.RouteChapters.LastChapterMoment;
            }
            else
            {
                data.routeChapterSummary = "sin capitulos de ruta";
                data.routeChapterCurrent = "sin capitulo";
                data.routeChapterRoute = "sin ruta";
                data.routeChapterLastMoment = "sin momento de capitulo";
            }
            if (gm.RouteBranches != null)
            {
                gm.RouteBranches.EvaluateNow(gm);
                data.routeBranchSummary = gm.RouteBranches.Summary;
                data.routeBranchRoute = V5MvpCanon.DisplayName(gm.RouteBranches.ActiveRoute);
                data.routeBranchActive = gm.RouteBranches.BranchName(gm.RouteBranches.ActiveBranch);
                data.routeBranchRunnerUp = gm.RouteBranches.BranchName(gm.RouteBranches.RunnerUpBranch);
                data.routeBranchScore = gm.RouteBranches.ActiveBranchScore01;
                data.routeBranchRunnerUpScore = gm.RouteBranches.RunnerUpScore01;
                data.routeBranchEstablishments = gm.RouteBranches.BranchEstablishments;
                data.routeBranchLastMoment = gm.RouteBranches.LastBranchMoment;
                data.routeBranchAbilitySynergyCount = gm.RouteBranches.BranchAbilitySynergyCount;
                data.routeBranchAbilitySynergy = gm.RouteBranches.LastBranchAbilitySynergy;
                data.routeBranchDoctrineAbilityCount = gm.RouteBranches.BranchDoctrineAbilityTriggerCount;
                data.routeBranchDoctrineAbility = gm.RouteBranches.LastBranchDoctrineAbility;
                data.routeBranchDoctrineObjective = gm.RouteBranches.BranchDoctrineObjectiveText;
                data.routeBranchDoctrineObjectiveProgress = gm.RouteBranches.BranchDoctrineObjectiveProgress01;
                data.routeBranchDoctrineObjectiveCompletions = gm.RouteBranches.BranchDoctrineObjectiveCompletions;
                data.routeBranchDoctrineObjectiveLastMoment = gm.RouteBranches.LastBranchDoctrineObjectiveMoment;
                data.routeBranchDoctrineObjectiveActive = gm.RouteBranches.BranchDoctrineObjectiveActive;
                data.routeBranchObjective = gm.RouteBranches.BranchObjectiveText;
                data.routeBranchObjectiveProgress = gm.RouteBranches.BranchObjectiveProgress01;
                data.routeBranchObjectiveCompletions = gm.RouteBranches.BranchObjectiveCompletions;
                data.routeBranchObjectiveLastMoment = gm.RouteBranches.LastBranchObjectiveMoment;
                data.routeBranchPassiveReadiness = gm.RouteBranches.BranchPassiveReadiness01;
                data.routeBranchPassivePulseCount = gm.RouteBranches.BranchPassivePulseCount;
                data.routeBranchPassiveEffect = gm.RouteBranches.LastBranchPassiveEffect;
                data.routeBranchVisualCue = gm.RouteBranches.LastBranchVisualCue;
                data.routeBranchAuraVisible = gm.RouteBranches.BranchAuraVisible;
                data.routeBranchAuraRadius = gm.RouteBranches.BranchAuraRadius;
                data.routeBranchDoctrineAvailable = gm.RouteBranches.BranchDoctrineAvailable;
                data.routeBranchDoctrineCommitments = gm.RouteBranches.BranchDoctrineCommitments;
                data.routeBranchDoctrineChoice = gm.RouteBranches.ActiveBranchDoctrineName;
                data.routeBranchDoctrineOffer = gm.RouteBranches.BranchDoctrineOffer;
                data.routeBranchDoctrineMoment = gm.RouteBranches.LastBranchDoctrineMoment;
                data.routeBranchTradeoffPressure = gm.RouteBranches.BranchTradeoffPressure01;
            }
            else
            {
                data.routeBranchSummary = "sin rama de ruta";
                data.routeBranchRoute = "sin ruta";
                data.routeBranchActive = "sin rama";
                data.routeBranchRunnerUp = "sin rama";
                data.routeBranchLastMoment = "sin momento de rama";
                data.routeBranchAbilitySynergy = "sin sinergia de rama";
                data.routeBranchDoctrineAbility = "sin habilidad doctrinal";
                data.routeBranchDoctrineObjective = "sin objetivo doctrinal";
                data.routeBranchDoctrineObjectiveLastMoment = "sin objetivo doctrinal";
                data.routeBranchObjective = "sin objetivo de rama";
                data.routeBranchObjectiveLastMoment = "sin objetivo de rama";
                data.routeBranchPassiveEffect = "sin pasivo de rama";
                data.routeBranchVisualCue = "sin visual de rama";
                data.routeBranchDoctrineChoice = "sin doctrina";
                data.routeBranchDoctrineOffer = "sin oferta de doctrina";
                data.routeBranchDoctrineMoment = "sin doctrina de rama";
            }
            if (gm.RouteCounters != null)
            {
                data.routeCounterSummary = gm.RouteCounters.LastCounterSummary;
                data.routeCounterActive = gm.RouteCounters.ActiveCounter;
                data.routeCounterRoute = V5MvpCanon.DisplayName(gm.RouteCounters.ActiveCounterRoute);
                data.routeCounterBranch = gm.RouteCounters.LastCounterBranchName;
                data.routeCounterCount = gm.RouteCounters.CounterEventCount;
                data.routeCounterAnsweredCount = gm.RouteCounters.CounterAnsweredCount;
                data.routeCounterLast = gm.RouteCounters.LastCounterName;
                data.routeCounterAdvice = gm.RouteCounters.LastCounterAdvice;
                data.routeCounterplayResult = gm.RouteCounters.LastCounterplayResult;
                data.routeCounterPressure = gm.RouteCounters.LastCounterPressure;
                data.routeCounterVisualCue = gm.RouteCounters.LastCounterVisualCue;
                data.routeCounterMarkerVisible = gm.RouteCounters.CounterMarkerVisible;
                data.routeCounterDoctrine = gm.RouteCounters.LastCounterDoctrineName;
                data.routeCounterDoctrineChoice = gm.RouteCounters.LastCounterDoctrine.ToString();
                data.routeCounterDoctrineMultiplier = gm.RouteCounters.LastCounterDoctrineMultiplier;
            }
            else
            {
                data.routeCounterSummary = "sin contra-presion de ruta";
                data.routeCounterRoute = "sin ruta";
                data.routeCounterBranch = "sin rama";
                data.routeCounterLast = "sin counter";
                data.routeCounterAdvice = "sin consejo counter";
                data.routeCounterplayResult = "sin counterplay";
                data.routeCounterVisualCue = "sin marcador de contra-presion";
                data.routeCounterDoctrine = "sin doctrina";
                data.routeCounterDoctrineChoice = "None";
                data.routeCounterDoctrineMultiplier = 1f;
            }
            data.telemetry = gm.Telemetry != null ? gm.Telemetry.Summary : "sin telemetry";
            if (gm.Telemetry != null)
            {
                data.adaptationInstalls = gm.Telemetry.AdaptationsInstalled;
                data.milestoneInstalls = gm.Telemetry.MilestonesInstalled;
                data.adaptationFailures = gm.Telemetry.FailedAdaptationAttempts;
                data.capBlockedAttempts = gm.Telemetry.CapBlockedAttempts;
                data.adaptationSummary = gm.Telemetry.AdaptationSummary;
                data.adaptationRoute = gm.Telemetry.DominantRouteSummary;
                data.adaptationFailureSummary = gm.Telemetry.FailureSummary;
                data.topAdaptations = gm.Telemetry.TopAdaptationsText(8);
            }
            data.body = gm.Body != null ? gm.Body.Summary : "sin cuerpo";
            data.squad = gm.Squads != null ? gm.Squads.Summary : "sin squad";
            data.battlefield = gm.Battlefield != null ? gm.Battlefield.Summary : "sin battlefield";
            data.playableLoop = gm.PlayableLoop != null ? gm.PlayableLoop.LoopSummary + " | " + gm.PlayableLoop.NextAction : "sin loop";
            V5RunDiagnosticsSystem diagnostics = gm.Diagnostics != null ? gm.Diagnostics : FindFirstObjectByType<V5RunDiagnosticsSystem>();
            if (diagnostics != null)
            {
                diagnostics.Scan();
                data.diagnosticsScore = diagnostics.Score;
                data.diagnosticsStatus = diagnostics.ShortStatus;
                data.diagnosticsAdvice = diagnostics.PriorityAdvice;
                data.diagnosticsCoachAdvice = diagnostics.CoachAdvice;
                data.diagnosticsCoachAction = diagnostics.CoachAction;
                data.diagnosticsCoachAdaptation = diagnostics.CoachAdaptationLabel;
                data.diagnosticsCoachAdaptationStatus = diagnostics.CoachAdaptationStatus;
                data.diagnostics = diagnostics.LastReport;
            }
            if (gm.LiveCoach != null)
            {
                data.liveCoachNotifications = gm.LiveCoach.NotificationsShown;
                data.liveCoachSummary = gm.LiveCoach.Summary;
                data.liveCoachLastAction = gm.LiveCoach.LastCoachAction;
                data.liveCoachSuggestedAdaptation = gm.LiveCoach.LastSuggestedAdaptationLabel;
                data.liveCoachSuggestedAdaptationStatus = gm.LiveCoach.LastSuggestedAdaptationStatus;
            }
            V5TutorialFlowSystem tutorial = FindFirstObjectByType<V5TutorialFlowSystem>();
            data.tutorial = tutorial != null ? tutorial.CurrentInstruction : "sin tutorial";
            V5RunSummarySystem runSummary = FindFirstObjectByType<V5RunSummarySystem>();
            if (runSummary != null)
            {
                runSummary.BuildSummary(false);
                data.summary = runSummary.LastSummary;
            }
            V5ReleaseReadinessSystem readiness = FindFirstObjectByType<V5ReleaseReadinessSystem>();
            data.readiness = readiness != null ? readiness.LastReport : "sin readiness";
            if (gm.MotherCell != null)
            {
                V5CellEntity m = gm.MotherCell;
                data.mother = m.EvolutionPath + " / " + m.Domain + " / " + m.Metabolism + " HP " + m.Stats.currentHp.ToString("0") + "/" + m.Stats.maxHp.ToString("0") + " stress " + m.Stats.stress.ToString("0");
                data.resources = "ATP " + m.Resources.atp.ToString("0") +
                                 " | Bio " + m.Resources.biomass.ToString("0") +
                                 " | AA " + m.Resources.aminoAcids.ToString("0") +
                                 " | Lip " + m.Resources.lipids.ToString("0") +
                                 " | Nuc " + m.Resources.nucleotides.ToString("0") +
                                 " | Min " + m.Resources.minerals.ToString("0");
            }
            return data;
        }

        public string ExportReport(string trigger)
        {
            V5PlaytestReportData data = CreateReport(trigger);
            string json = JsonUtility.ToJson(data, true);
            PlayerPrefs.SetString(LastReportKey, json);
            PlayerPrefs.Save();

            try
            {
                string safeTime = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                string dir = Path.Combine(Application.persistentDataPath, "ProtogenesisV5Reports");
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                LastPath = Path.Combine(dir, "playtest_report_" + safeTime + ".json");
                File.WriteAllText(LastPath, json);
                LastMessage = "Reporte exportado: " + LastPath;
            }
            catch (Exception e)
            {
                LastPath = "PlayerPrefs:" + LastReportKey;
                LastMessage = "Reporte guardado en PlayerPrefs; archivo falló: " + e.Message;
            }

            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
            return LastPath;
        }
    }
}
