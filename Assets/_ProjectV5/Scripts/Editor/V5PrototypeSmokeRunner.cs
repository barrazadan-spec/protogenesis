#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Protogenesis.V5.EditorTools
{
    [InitializeOnLoad]
    public static class V5PrototypeSmokeRunner
    {
        private const string ScenePath = "Assets/_ProjectV5/Scenes/V5_FullPrototype.unity";
        private const string SessionActiveKey = "V5SmokeRunnerActive";
        private static readonly List<string> failures = new List<string>();
        private static double startTime;
        private static bool enteredPlayMode;
        private static bool actionsDone;
        private static bool finalChecksDone;

        static V5PrototypeSmokeRunner()
        {
            if (SessionState.GetBool(SessionActiveKey, false))
            {
                failures.Clear();
                enteredPlayMode = EditorApplication.isPlaying;
                actionsDone = false;
                finalChecksDone = false;
                startTime = EditorApplication.timeSinceStartup;
                Hook();
                Debug.Log("[V5Smoke] Resumed after domain reload. isPlaying=" + EditorApplication.isPlaying);
            }
        }

        [MenuItem("Protogenesis/V5/Run Full Prototype Smoke Test")]
        public static void RunFullPrototypeSmoke()
        {
            failures.Clear();
            enteredPlayMode = false;
            actionsDone = false;
            finalChecksDone = false;
            startTime = EditorApplication.timeSinceStartup;

            SessionState.SetBool(SessionActiveKey, true);
            Hook();

            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            Debug.Log("[V5Smoke] Loaded " + ScenePath + ". Entering Play Mode.");
            EditorApplication.isPlaying = true;
        }

        private static void Hook()
        {
            Unhook();
            Application.logMessageReceived += OnLogMessage;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += Tick;
        }

        private static void Unhook()
        {
            Application.logMessageReceived -= OnLogMessage;
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.update -= Tick;
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                enteredPlayMode = true;
                startTime = EditorApplication.timeSinceStartup;
                Debug.Log("[V5Smoke] Play Mode entered.");
            }
        }

        private static void Tick()
        {
            if (!enteredPlayMode)
            {
                if (EditorApplication.timeSinceStartup > startTime + 20.0) Finish(1, "Timed out before entering Play Mode.");
                return;
            }

            double age = EditorApplication.timeSinceStartup - startTime;
            if (age > 18.0)
            {
                Finish(1, "Timed out while running smoke test.");
                return;
            }

            if (age < 1.0) return;

            if (!actionsDone)
            {
                actionsDone = true;
                RunStartupChecks();
                RunFirstMinuteActions();
                return;
            }

            if (!finalChecksDone && age > 2.25)
            {
                finalChecksDone = true;
                RunFinalChecks();
                Finish(failures.Count == 0 ? 0 : 1, "Smoke test complete.");
            }
        }

        private static void RunStartupChecks()
        {
            V5GameManager gm = V5GameManager.Instance;
            Check(gm != null, "GameManager exists.");
            if (gm == null) return;

            Check(gm.Environment != null && gm.Environment.nutrients != null && gm.Environment.lightLevel != null, "Environment initialized.");
            Check(gm.Resources != null && gm.Resources.Nodes.Count >= 20, "Resource nodes spawned.");
            Check(gm.CellFactory != null, "CellFactory exists.");
            Check(gm.MotherCell != null, "Mother cell spawned.");
            Check(gm.PlayerCellCount() >= 1, "Player cell registry populated.");
            Check(gm.Abilities != null, "Ability system installed.");
            Check(gm.NonPlayerCells.Count >= 1, "Neutral/ecological organisms spawned.");
            Check(gm.PlayableLoop != null, "Playable loop system installed.");
            Check(gm.Hud != null, "HUD installed.");
            Check(gm.Body != null && gm.Body.MaxSlots >= 6, "Multicellular body system installed.");
            Check(gm.Germinal != null, "Germinal production system installed.");
            Check(gm.Lineages != null && gm.LineageUpgrades != null, "Unified lineage systems installed.");
            Check(gm.RouteLifecycle != null, "Evolution route lifecycle system installed.");
            Check(gm.Adaptations != null, "Adaptation system installed.");
            Check(gm.Telemetry != null, "Telemetry system installed.");
            Check(gm.Diagnostics != null, "Run diagnostics system installed.");
            Check(gm.LiveCoach != null, "Live coach system installed.");
            Check(UnityEngine.Object.FindFirstObjectByType<V5PlaytestReportSystem>() != null, "Playtest report exporter installed.");
            Check(gm.Identity != null, "Identity recognizer installed.");
            Check(gm.GenomePanel != null, "Genome/adaptation panel installed.");
            Check(gm.MvpIntent != null, "MVP route intent system installed.");
            Check(gm.NichePressure != null, "Niche pressure system installed.");
            Check(gm.RouteMastery != null, "Route mastery system installed.");
            Check(gm.RouteBuilds != null, "Route build system installed.");
            Check(gm.RouteClimax != null, "Route climax system installed.");
            Check(gm.RouteChapters != null, "Route chapter system installed.");
            Check(gm.RouteBranches != null, "Route branch system installed.");
            Check(gm.RouteCounters != null, "Route counter-pressure system installed.");
            Check(gm.WorldEvents != null, "World event system installed.");
            if (gm.MvpIntent != null && gm.MotherCell != null)
            {
                gm.MvpIntent.SetIntent(V5MvpRoute.Amoeba);
                Check(gm.MvpIntent.SuggestedCoreAdaptation(gm) == V5AdaptationId.BacterialFlagellum, "MVP Amoeba suggests early bridge before Nucleus.");
                Check(gm.MvpIntent.RouteObjectiveText(gm).Contains("Ameba"), "MVP route objective describes selected route.");
                Check(V5MvpCanon.NextBuildTarget(V5MvpRoute.Amoeba, gm.Adaptations) == V5AdaptationId.BacterialFlagellum, "MVP route build defines first target mutation.");
                Check(V5MvpCanon.BuildProgressText(V5MvpRoute.Amoeba, gm.Adaptations).Contains("Build Ameba"), "MVP route build exposes readable build status.");
                Check(gm.MvpIntent.OpeningNudgeCount == 1 && gm.MvpIntent.OpeningStepText(gm).Contains("Ameba"), "MVP route choice gives one early opening nudge.");
                Check(gm.MvpIntent.WorldCueCount == 1 && gm.MvpIntent.LastWorldCue.Contains("Ameba"), "MVP route choice stages a world cue.");
                Check(gm.MvpIntent.RouteMicroObjectiveText(gm).Contains("Ameba"), "MVP route choice exposes a micro objective.");
                Check(gm.MvpIntent.RouteMicroObjectiveProgress01(gm) >= 0f && gm.MvpIntent.RouteMicroObjectiveProgress01(gm) <= 1f, "MVP micro objective progress stays normalized.");
                if (gm.NichePressure != null)
                {
                    float nicheScore = gm.NichePressure.ScanNow(gm);
                    Check(nicheScore >= 0f && nicheScore <= 1f && gm.NichePressure.Summary.Contains("Ameba"), "Niche pressure reads selected MVP route.");
                    Check(!string.IsNullOrEmpty(gm.NichePressure.LastNicheAdvice), "Niche pressure provides route advice.");
                }
                if (gm.WorldEvents != null)
                {
                    Check(gm.WorldEvents.TriggerRouteEventNow(V5MvpRoute.Amoeba) && gm.WorldEvents.LastRouteEvent == V5MvpRoute.Amoeba, "World events trigger MVP route ecology event.");
                    Check(gm.WorldEvents.LastRouteEventSummary.Contains("Ameba"), "World events describe selected MVP route ecology.");
                    Check(gm.WorldEvents.RouteOpportunityActive && gm.WorldEvents.RouteOpportunityText.Contains("Ameba"), "Route ecology event opens an MVP opportunity objective.");
                    if (gm.Environment != null)
                    {
                        gm.Environment.ModifyArea(gm.WorldEvents.RouteOpportunityCenter, 4.8f, 0f, 0f, 0f, 0f, 0f, 0f, 1.0f);
                        Check(gm.WorldEvents.EvaluateRouteOpportunityNow(gm) && gm.WorldEvents.RouteOpportunityCompletedCount >= 1, "Route ecology opportunity completes from world state.");
                        Check(gm.WorldEvents.LastRouteOpportunityReward.Contains("Ameba"), "Route ecology opportunity applies identity reward.");
                        Check(gm.RouteMastery != null && gm.RouteMastery.Mastery01(V5MvpRoute.Amoeba) > 0f, "Route mastery records completed opportunity.");
                        Check(gm.RouteMastery != null && gm.RouteMastery.NicheRewardMultiplier(V5MvpRoute.Amoeba) > 1f, "Route mastery improves future niche rewards.");
                    }
                }
                Check(gm.MvpIntent.CompleteMicroObjective(V5MvpRoute.Amoeba, gm, "smoke"), "MVP micro objective can be completed once.");
                Check(!gm.MvpIntent.CompleteMicroObjective(V5MvpRoute.Amoeba, gm, "smoke_duplicate"), "MVP micro objective blocks duplicates.");
                Check(gm.MvpIntent.MicroObjectiveCompletedCount == 1 && gm.MvpIntent.LastMicroMilestone.Contains("Ameba"), "MVP micro objective records completion.");
                Check(gm.MvpIntent.RouteGoalProgress01(gm) >= 0f && gm.MvpIntent.RouteGoalProgress01(gm) <= 1f, "MVP route goal progress stays normalized.");
                Check(gm.MvpIntent.PrepareRoutePlaytest(gm), "MVP route playtest budget prepares selected route.");
                Check(gm.MotherCell.Resources.atp >= 190f && gm.MotherCell.Stats.stress <= 22f, "MVP route playtest stabilizes budget and stress.");
                float damageBeforeMvp = gm.MotherCell.Stats.physicalDamagePerSecond;
                Check(gm.MvpIntent.CompleteRouteGoal(V5MvpRoute.Amoeba, gm, "smoke"), "MVP route milestone can be completed once.");
                Check(!gm.MvpIntent.CompleteRouteGoal(V5MvpRoute.Amoeba, gm, "smoke_duplicate"), "MVP route milestone blocks duplicates.");
                Check(gm.MvpIntent.CompletedGoalCount == 1 && gm.MvpIntent.LastCompletedRoute == V5MvpRoute.Amoeba, "MVP route milestone records completion state.");
                Check(gm.MvpIntent.LastIdentityMoment.Contains("Ameba") && gm.MotherCell.Stats.physicalDamagePerSecond > damageBeforeMvp, "MVP route identity moment applies route pulse.");
                gm.MvpIntent.SetIntent(V5MvpRoute.None);
            }
            Check(gm.Adaptations == null || gm.Adaptations.ActiveCap == 14, "Adaptation cap starts at 14.");
            Check(V5AdaptationLibrary.Get(V5AdaptationId.ExtracellularEnzymes) != null, "P0 includes ExtracellularEnzymes for Fungus.");
            Check(gm.Adaptations != null && gm.Adaptations.FirstMissingPrerequisite(V5AdaptationId.Mitochondria) == V5AdaptationId.Nucleus, "Adaptation explainer finds missing prerequisite.");
            Check(gm.Adaptations != null && gm.Adaptations.PrerequisiteChecklist(V5AdaptationId.Mitochondria).Contains("[ ] Nucleo"), "Adaptation explainer marks missing prerequisite.");
            Check(gm.Adaptations != null && gm.MotherCell != null && gm.Adaptations.NextStepFor(V5AdaptationId.Mitochondria, gm.MotherCell).Contains("Nucleo"), "Adaptation explainer gives next prerequisite step.");
            Check(V5BiologyCanon.GenesForRoute(V5EvolutionPath.Bacteria).Length >= 3, "Biology canon defines genes for Bacteria.");
            Check(V5BiologyCanon.StructuresForRoute(V5EvolutionPath.Fungus).Length >= 3, "Biology canon defines structures for Fungus.");
            Check(V5BiologyCanon.AdaptationsForRoute(V5EvolutionPath.Bacteria).Length >= 3, "Biology canon defines adaptation path for Bacteria.");
            Check(HasAdaptation(V5BiologyCanon.AdaptationsForRoute(V5EvolutionPath.Fungus), V5AdaptationId.ExtracellularEnzymes), "Fungus canon requires extracellular enzymes.");
            Check(V5BiologyCanon.NaturalPhenotypesForRoute(V5EvolutionPath.Amoeba).Length >= 2, "Biology canon defines natural phenotypes for Amoeba.");
            Check(!V5EvolutionRoster.IsPrimaryRoute(V5EvolutionPath.Tardigrade), "Tardigrade is not a primary playable route.");
            Check(V5CasteLibrary.Get(V5FunctionalCasteId.Gatherer).displayName == "Recolectora", "Functional caste library exposes Gatherer.");
            Check(V5CasteLibrary.FromGerminalCaste(V5GerminalCasteId.LineageRaider) == V5FunctionalCasteId.Attacker, "Germinal raider maps to attacker caste.");
        }

        private static void RunFirstMinuteActions()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;

            V5CellEntity mother = gm.MotherCell;
            if (gm.Adaptations != null)
            {
                Check(gm.Adaptations.Install(V5AdaptationId.BacterialWall, mother), "Mother can install BacterialWall adaptation.");
                Check(gm.Adaptations.Install(V5AdaptationId.BacterialFlagellum, mother), "Mother can install BacterialFlagellum adaptation.");
                Check(gm.Adaptations.Has(V5AdaptationId.BacterialWall) && gm.Adaptations.Has(V5AdaptationId.BacterialFlagellum), "Adaptation tracker records early adaptations.");
                Check(gm.Telemetry != null && gm.Telemetry.AdaptationsInstalled >= 2f, "Telemetry records early adaptation installs.");
                Check(mother.HasStructure(V5StructureId.PeptidoglycanWall) && mother.HasStructure(V5StructureId.BacterialFlagellum), "Adaptations map to legacy structures.");
                Check(gm.Identity != null && gm.Identity.Identity == V5IdentityId.BacteriaSwarm, "Identity recognizer consolidates BacteriaSwarm.");
                if (gm.RouteBuilds != null)
                {
                    int buildBefore = gm.RouteBuilds.TotalBuildMilestones;
                    Check(gm.RouteBuilds.EvaluateNow(gm), "Route build recognizes installed target mutations.");
                    Check(gm.RouteBuilds.ActiveRoute == V5MvpRoute.Bacteria && gm.RouteBuilds.ActiveStage >= 2, "Route build advances Bacteria signature stage.");
                    Check(gm.RouteBuilds.TotalBuildMilestones >= buildBefore + 2 && gm.RouteBuilds.LastBuildMoment.Contains("Bacteria"), "Route build records milestone rewards.");
                }
                if (gm.RouteChapters != null)
                {
                    Check(gm.RouteChapters.EvaluateNow(gm) && gm.RouteChapters.CompletedFor(V5MvpRoute.Bacteria) >= 1, "Route chapters complete Bacteria opening chapter from build.");
                    Check(gm.RouteChapters.Summary.Contains("Bacteria"), "Route chapters expose readable active chapter summary.");
                }
                if (gm.RouteBranches != null)
                {
                    Check(gm.RouteBranches.EvaluateNow(gm) && gm.RouteBranches.ActiveRoute == V5MvpRoute.Bacteria, "Route branches read Bacteria style from early route state.");
                    Check(gm.RouteBranches.ActiveBranch != V5RouteBranchId.None && gm.RouteBranches.Summary.Contains("Bacteria"), "Route branches expose readable specialization.");
                }
                if (gm.Abilities != null)
                {
                    mother.Resources.atp += 80f;
                    mother.Resources.biomass += 35f;
                    if (gm.WorldEvents != null)
                    {
                        Check(gm.WorldEvents.TriggerRouteEventNow(V5MvpRoute.Bacteria) && gm.WorldEvents.RouteOpportunityActive, "Route ability has a matching ecology opportunity.");
                    }
                    if (gm.RouteCounters != null)
                    {
                        Check(gm.RouteCounters.ForceCounterPressureNow(V5MvpRoute.Bacteria) && gm.RouteCounters.ActiveCounter, "Route counter-pressure opens a Bacteria counter event.");
                        Check(gm.RouteCounters.LastCounterBranchName != "sin rama" && gm.RouteCounters.LastCounterSummary.Contains(gm.RouteCounters.LastCounterBranchName), "Route counter-pressure reads active branch specialization.");
                        Check(gm.RouteCounters.CounterMarkerVisible && gm.RouteCounters.LastCounterVisualCue.Contains("Fago"), "Route counter-pressure shows a readable world marker.");
                    }
                    Check(gm.Abilities.RouteFantasyStatus(gm).Contains("Bacteria"), "Route fantasy ability reads active MVP route.");
                    Check(gm.Abilities.TryRouteFantasyAbility(), "Route fantasy ability casts from completed build.");
                    Check(gm.Abilities.RouteFantasyCastCount >= 1 && gm.Abilities.LastRouteFantasyAbility.Contains("Biofilm"), "Route fantasy ability records route-specific cast.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.BranchAbilitySynergyCount >= 1 && gm.RouteBranches.LastBranchAbilitySynergy.Contains("Sinergia"), "Route branch modifies route fantasy ability.");
                    Check(gm.Abilities.LastRouteFantasyAbility.Contains("rama"), "Route fantasy ability reports branch synergy.");
                    Check(gm.WorldEvents != null && gm.WorldEvents.RouteAbilityComboCount >= 1 && gm.WorldEvents.LastRouteAbilityCombo.Contains("Bacteria"), "Route fantasy ability combos with ecology opportunity.");
                    Check(gm.RouteCounters != null && gm.RouteCounters.CounterAnsweredCount >= 1 && gm.RouteCounters.LastCounterplayResult.Contains("Bacteria"), "Route fantasy ability counters matching route pressure.");
                    Check(gm.RouteCounters != null && !gm.RouteCounters.CounterMarkerVisible && gm.RouteCounters.LastCounterVisualCue.Contains("Neutralizado"), "Route counter marker clears after counterplay.");
                    Check(gm.RouteChapters != null && gm.RouteChapters.EvaluateNow(gm) && gm.RouteChapters.CompletedFor(V5MvpRoute.Bacteria) >= 3, "Route chapters chain through ability and opportunity combo.");
                    if (gm.RouteBranches != null) gm.RouteBranches.EvaluateNow(gm);
                    Check(gm.RouteBranches != null && gm.RouteBranches.BranchEstablishments >= 1, "Route branches establish a Bacteria specialization from player style.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.BranchObjectiveCompletions >= 1 && gm.RouteBranches.LastBranchObjectiveMoment.Contains("Objetivo"), "Route branch objective completes from branch-specific loop.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.ApplyBranchPassiveNow(gm) && gm.RouteBranches.BranchPassivePulseCount >= 1 && gm.RouteBranches.LastBranchPassiveEffect.Contains("Pasivo"), "Route branch passive pulse applies established specialization.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.ForceBranchAuraNow(gm) && gm.RouteBranches.BranchAuraVisible && gm.RouteBranches.LastBranchVisualCue.Contains("Aura"), "Route branch visual aura follows established specialization.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.BranchDoctrineAvailable, "Route branch objective unlocks a doctrine decision.");
                    float stressBeforeDoctrine = mother.Stats.stress;
                    Check(gm.RouteBranches != null && gm.RouteBranches.CommitBranchDoctrine(V5BranchDoctrineChoice.Radicalize, gm) && gm.RouteBranches.BranchDoctrineCommitments >= 1 && gm.RouteBranches.LastBranchDoctrineMoment.Contains("Doctrina"), "Route branch doctrine commits a tradeoff choice.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.BranchTradeoffPressure01 > 0f && mother.Stats.stress >= stressBeforeDoctrine, "Route branch doctrine applies a readable tradeoff cost.");
                    Check(gm.RouteCounters != null && gm.RouteCounters.ForceCounterPressureNow(V5MvpRoute.Bacteria) && gm.RouteCounters.LastCounterDoctrineName == gm.RouteBranches.ActiveBranchDoctrineName && gm.RouteCounters.LastCounterSummary.Contains(gm.RouteCounters.LastCounterDoctrineName), "Route counter-pressure adapts to branch doctrine.");
                    Check(gm.RouteCounters != null && gm.RouteCounters.LastCounterDoctrineMultiplier > 1f, "Route counter-pressure scales radical doctrine pressure.");
                    Check(gm.RouteCounters != null && gm.RouteCounters.RegisterRouteAbilityCounterplay(V5MvpRoute.Bacteria, gm.RouteCounters.CounterCenter, 1.1f, "smoke doctrine", gm), "Route doctrine counter-pressure can be answered.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.RegisterRouteAbilitySynergy(V5MvpRoute.Bacteria, mother.transform.position, 1.2f, "smoke doctrine cast", gm) && gm.RouteBranches.BranchDoctrineAbilityTriggerCount >= 1 && gm.RouteBranches.LastBranchDoctrineAbility.Contains(gm.RouteBranches.ActiveBranchDoctrineName), "Route branch doctrine mutates route ability synergy.");
                    Check(gm.Abilities != null && gm.Abilities.RouteFantasyStatus(gm).Contains(gm.RouteBranches.ActiveBranchDoctrineName), "Route fantasy status shows active branch doctrine.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.EvaluateNow(gm) && gm.RouteBranches.BranchDoctrineObjectiveCompletions >= 1 && gm.RouteBranches.LastBranchDoctrineObjectiveMoment.Contains(gm.RouteBranches.ActiveBranchDoctrineName), "Route branch doctrine objective completes after doctrine actions.");
                    Check(gm.RouteBranches != null && gm.RouteBranches.BranchDoctrineObjectiveStatus(gm).Contains("100%"), "Route branch doctrine objective status is readable.");
                    if (gm.RouteClimax != null)
                    {
                        gm.RouteClimax.RefreshNow(gm);
                        Check(gm.RouteClimax.ActiveRoute == V5MvpRoute.Bacteria && gm.RouteClimax.ClimaxScore01 > 0f && gm.RouteClimax.BranchScore01 > 0f, "Route climax reads build, combo, branch, and route progress.");
                        Check(gm.RouteClimax.ActiveBranch != V5RouteBranchId.None && gm.RouteClimax.BranchObjectiveScore01 > 0f && gm.RouteClimax.Summary.Contains(gm.RouteClimax.ActiveBranchName), "Route climax names active branch finale.");
                        Check(gm.RouteClimax.ActiveBranchDoctrineName == gm.RouteBranches.ActiveBranchDoctrineName && gm.RouteClimax.BranchFinaleTitle.Contains(gm.RouteBranches.ActiveBranchDoctrineName), "Route climax finale reflects branch doctrine.");
                    }
                }
            }

            Check(mother.CanDivide(), "Mother can afford first natural division after early adaptations.");
            if (!mother.CanDivide())
            {
                mother.Resources.atp += 160f;
                mother.Resources.biomass += 90f;
            }

            V5CellEntity child = mother.Divide();
            Check(child != null, "Mother can perform first division.");
            if (child != null)
            {
                child.Directive = V5Directive.Farm;
                child.Mother = mother;
                if (gm.Selection != null)
                {
                    gm.Selection.ClearSelection();
                    gm.Selection.AddSelection(child);
                }
                if (gm.Body != null)
                {
                    V5BodySlotRole role = V5PhenotypeRecipeLibrary.RecommendedBodyRole(child.PhenotypeCaste);
                    Check(!gm.Body.TryAttach(child, role), "Body attachment is locked before BasicAdhesin.");
                    mother.Resources.atp += 80f;
                    mother.Resources.biomass += 40f;
                    mother.Resources.aminoAcids += 20f;
                    mother.Resources.lipids += 20f;
                    mother.Resources.nucleotides += 10f;
                    Check(gm.Adaptations != null && gm.Adaptations.Install(V5AdaptationId.BasicAdhesin, mother), "Mother can unlock cheap BasicAdhesin before body attachment.");
                    float failuresBefore = gm.Telemetry != null ? gm.Telemetry.FailedAdaptationAttempts : 0f;
                    Check(gm.Adaptations != null && !gm.Adaptations.Install(V5AdaptationId.BasicAdhesin, mother), "Duplicate adaptation install is blocked.");
                    Check(gm.Telemetry != null && gm.Telemetry.FailedAdaptationAttempts > failuresBefore, "Telemetry records blocked adaptation attempts.");
                    Check(gm.Body.TryAttach(child, role), "Daughter can attach to the multicellular body.");
                    Check(child.IsAttachedToBody && gm.Body.OccupiedSlots >= 1, "Body records attached daughter.");
                    float attachedDistance = Vector2.Distance(child.transform.position, mother.transform.position);
                    float visualContact = (mother.Stats.radius + child.Stats.radius) * 0.96f;
                    Check(attachedDistance <= visualContact, "Attached daughter snaps into visual contact with mother.");
                }
            }

            if (gm.Germinal != null && gm.Body != null)
            {
                mother.Resources.atp += 240f;
                mother.Resources.biomass += 130f;
                mother.Resources.aminoAcids += 50f;
                mother.Resources.lipids += 50f;
                mother.Resources.nucleotides += 30f;
                mother.Resources.minerals += 20f;
                mother.Stats.stress = Mathf.Min(mother.Stats.stress, 20f);

                int slotsBefore = gm.Body.OccupiedSlots;
                Check(gm.Germinal.TryProduce(V5GerminalCasteId.PlasticDaughter, V5CellDeploymentMode.AttachedBody, true), "Genome Lab can produce directly into the body.");
                Check(gm.Body.OccupiedSlots > slotsBefore, "Germinal body destination fills an open slot.");

                int cellsBefore = gm.PlayerCellCount();
                Check(gm.Germinal.TryProduce(V5GerminalCasteId.PlasticDaughter, V5CellDeploymentMode.FreeSquad, true), "Genome Lab can produce a free squad unit.");
                Check(gm.PlayerCellCount() > cellsBefore, "Germinal free destination adds a controllable unit.");

                mother.Resources.atp += 90f;
                mother.Resources.biomass += 60f;
                mother.Resources.aminoAcids += 24f;
                mother.Resources.lipids += 18f;
                mother.Resources.nucleotides += 10f;
                mother.Resources.minerals += 6f;
                int raiderBefore = gm.PlayerCellCount();
                Check(gm.Germinal.TryProduce(V5GerminalCasteId.LineageRaider, V5CellDeploymentMode.FreeSquad, true), "Genome Lab can produce an attacker caste unit.");
                if (gm.PlayerCellCount() > raiderBefore)
                {
                    V5CellEntity newest = gm.PlayerCells[gm.PlayerCells.Count - 1];
                    Check(newest != null && newest.FunctionalCaste == V5FunctionalCasteId.Attacker, "Produced raider carries attacker functional caste.");
                    Check(newest != null && newest.ModeDamageMultiplier() > 1f, "Attacker caste improves combat multiplier.");
                }
            }

            if (gm.Environment != null)
                gm.Environment.ModifyArea(mother.transform.position, 4f, 0.04f, 0.02f, 0.04f, -0.04f, 0f, 0.03f, 0.01f);
        }

        private static void RunFinalChecks()
        {
            V5GameManager gm = V5GameManager.Instance;
            Check(gm != null, "GameManager survived first tick.");
            if (gm == null) return;

            Check(gm.MotherCell != null && gm.MotherCell.Stats.currentHp > 0f, "Mother survived smoke actions.");
            Check(gm.PlayerCellCount() >= 2, "At least one daughter exists.");
            Check(gm.Body != null && gm.Body.OccupiedSlots >= 1, "Body still has an attached slot.");
            Check(gm.Telemetry != null && gm.Telemetry.AdaptationsInstalled >= 3f, "Telemetry persists adaptation count through smoke.");
            if (gm.Diagnostics != null)
            {
                gm.Diagnostics.Scan();
                Check(gm.Diagnostics.Score > 0, "Run diagnostics produces a score.");
                Check(!string.IsNullOrEmpty(gm.Diagnostics.LastReport), "Run diagnostics produces a report.");
            }
            V5PlaytestReportSystem report = UnityEngine.Object.FindFirstObjectByType<V5PlaytestReportSystem>();
            if (report != null)
            {
                V5PlaytestReportData data = report.CreateReport("smoke");
                Check(data.genomeMode == "Adaptations", "Playtest report exports adaptation genome mode.");
                Check(data.activeAdaptationCount >= 3, "Playtest report exports adaptation count.");
                Check(!string.IsNullOrEmpty(data.adaptationSummary), "Playtest report exports adaptation telemetry summary.");
                Check(data.mvpRoutesCompleted >= 1 && data.mvpRouteMilestone.Contains("Hito MVP"), "Playtest report exports MVP route milestone.");
                Check(!string.IsNullOrEmpty(data.mvpIdentityMoment), "Playtest report exports MVP identity moment.");
                Check(data.mvpRouteWorldCueCount >= 1 && data.mvpRouteWorldCue.Contains("Ameba"), "Playtest report exports MVP world cue.");
                Check(data.mvpMicroObjectiveCompletedCount >= 1 && data.mvpMicroMilestone.Contains("Ameba"), "Playtest report exports MVP micro objective.");
                Check(!string.IsNullOrEmpty(data.nicheSummary) && data.nicheScore >= 0f && data.nicheScore <= 1f, "Playtest report exports niche pressure.");
                Check(data.routeEcologyEventCount >= 1 && data.routeEcologyEvent.Contains("Evento"), "Playtest report exports route ecology event.");
                Check(data.routeOpportunityCompletedCount >= 1 && data.routeOpportunityReward.Contains("Recompensa"), "Playtest report exports route opportunity reward.");
                Check(data.routeMasteryTotalCompletions >= 1 && data.routeMasteryBestScore > 0f && data.routeMasteryLastMoment.Contains("Mastery"), "Playtest report exports route mastery memory.");
                Check(data.routeBuildMilestones >= 1 && data.routeBuildSummary.Contains("Build"), "Playtest report exports route build progress.");
                Check(data.routeAbilityCasts >= 1 && data.routeAbilityLast.Contains("Biofilm"), "Playtest report exports route fantasy ability.");
                Check(data.routeAbilityComboCount >= 1 && data.routeAbilityCombo.Contains("Bacteria"), "Playtest report exports route ability opportunity combo.");
                Check(!string.IsNullOrEmpty(data.routeClimaxSummary) && data.routeClimaxScore > 0f, "Playtest report exports route climax progress.");
                Check(data.routeClimaxBranch != "sin rama" && data.routeClimaxBranchObjectiveScore > 0f && data.routeClimaxBranchFinale != "sin final de rama", "Playtest report exports branch-aware climax finale.");
                Check(data.routeClimaxBranchDoctrine != "sin doctrina" && data.routeClimaxBranchFinale.Contains(data.routeClimaxBranchDoctrine), "Playtest report exports doctrine-aware branch finale.");
                Check(data.routeChapterTotalCompletions >= 3 && data.routeChapterSummary.Contains("Bacteria"), "Playtest report exports route chapter chain.");
                Check(data.routeBranchEstablishments >= 1 && data.routeBranchSummary.Contains("Bacteria"), "Playtest report exports route branch specialization.");
                Check(data.routeBranchAbilitySynergyCount >= 1 && data.routeBranchAbilitySynergy.Contains("Sinergia"), "Playtest report exports branch ability synergy.");
                Check(data.routeBranchDoctrineAbilityCount >= 1 && data.routeBranchDoctrineAbility.Contains(data.routeBranchDoctrineChoice), "Playtest report exports branch doctrine ability mutation.");
                Check(data.routeBranchDoctrineObjectiveCompletions >= 1 && data.routeBranchDoctrineObjectiveLastMoment.Contains(data.routeBranchDoctrineChoice), "Playtest report exports branch doctrine objective.");
                Check(data.routeBranchObjectiveCompletions >= 1 && data.routeBranchObjectiveLastMoment.Contains("Objetivo"), "Playtest report exports branch objective completion.");
                Check(data.routeBranchPassivePulseCount >= 1 && data.routeBranchPassiveEffect.Contains("Pasivo"), "Playtest report exports branch passive pulse.");
                Check(data.routeBranchAuraVisible && data.routeBranchAuraRadius > 0f && data.routeBranchVisualCue.Contains("Aura"), "Playtest report exports branch visual identity.");
                Check(data.routeBranchDoctrineCommitments >= 1 && data.routeBranchDoctrineChoice != "sin doctrina" && data.routeBranchDoctrineMoment.Contains("Doctrina") && data.routeBranchTradeoffPressure > 0f, "Playtest report exports branch doctrine tradeoff.");
                Check(data.routeCounterAnsweredCount >= 1 && data.routeCounterplayResult.Contains("Bacteria"), "Playtest report exports route counter-pressure counterplay.");
                Check(data.routeCounterBranch != "sin rama" && data.routeCounterplayResult.Contains(data.routeCounterBranch), "Playtest report exports branch-aware counter-pressure.");
                Check(data.routeCounterDoctrine != "sin doctrina" && data.routeCounterDoctrineMultiplier > 1f && data.routeCounterplayResult.Contains(data.routeCounterDoctrine), "Playtest report exports doctrine-aware counter-pressure.");
                Check(!data.routeCounterMarkerVisible && data.routeCounterVisualCue.Contains("Neutralizado"), "Playtest report exports route counter marker state.");
                Check(data.diagnosticsScore > 0, "Playtest report exports diagnostics score.");
                Check(!string.IsNullOrEmpty(data.diagnosticsAdvice), "Playtest report exports diagnostics advice.");
                Check(!string.IsNullOrEmpty(data.diagnosticsCoachAction), "Playtest report exports diagnostics coach action.");
                Check(!string.IsNullOrEmpty(data.diagnosticsCoachAdaptation), "Playtest report exports diagnostics adaptation suggestion.");
            }
            if (gm.Advisor != null && gm.MotherCell != null && gm.Diagnostics != null)
            {
                float oldStress = gm.MotherCell.Stats.stress;
                gm.MotherCell.Stats.stress = 96f;
                gm.Diagnostics.Scan();
                Check(gm.Diagnostics.CoachAdaptation == V5AdaptationId.CatalaseROS, "Diagnostics maps stress to Catalase/ROS suggestion.");
                Check(gm.Diagnostics.CoachAction.Contains("Catalasa"), "Diagnostics coach action names suggested adaptation.");
                gm.Advisor.Recalculate();
                Check(gm.Advisor.UsingDiagnosticAdvice, "Advisor uses diagnostics when run needs coaching.");
                Check(!string.IsNullOrEmpty(gm.Advisor.RecommendedAction), "Advisor exposes diagnostics coach action.");
                int coachBefore = gm.LiveCoach != null ? gm.LiveCoach.NotificationsShown : 0;
                Check(gm.LiveCoach != null && gm.LiveCoach.EvaluateNow(true), "Live coach emits forced diagnostic toast.");
                Check(gm.LiveCoach != null && gm.LiveCoach.NotificationsShown > coachBefore, "Live coach counts intervention.");
                Check(gm.LiveCoach != null && gm.LiveCoach.LastSuggestedAdaptation == V5AdaptationId.CatalaseROS, "Live coach preserves suggested adaptation.");
                if (gm.GenomePanel != null)
                {
                    gm.GenomePanel.OpenFocused(gm.Diagnostics.CoachAdaptation);
                    Check(gm.GenomePanel.ShowPanel, "Genome panel opens from coach suggestion.");
                    Check(gm.GenomePanel.SelectedAdaptation == gm.Diagnostics.CoachAdaptation, "Genome panel focuses coach adaptation.");
                }
                gm.MotherCell.Stats.stress = oldStress;
                gm.Diagnostics.Scan();
            }
            V5PlaytestReportSystem reportAfterCoach = UnityEngine.Object.FindFirstObjectByType<V5PlaytestReportSystem>();
            if (reportAfterCoach != null)
            {
                V5PlaytestReportData data = reportAfterCoach.CreateReport("smoke_after_coach");
                Check(data.liveCoachNotifications >= 1, "Playtest report exports live coach interventions.");
                Check(!string.IsNullOrEmpty(data.liveCoachLastAction), "Playtest report exports live coach action.");
                Check(data.liveCoachSuggestedAdaptation.Contains("Catalasa"), "Playtest report exports live coach adaptation suggestion.");
            }
            if (gm.PlayableLoop != null) gm.PlayableLoop.RefreshNow();
            Check(gm.PlayableLoop != null && gm.PlayableLoop.Stage != V5PlayableLoopStage.StabilizeMetabolism, "Playable loop advances past metabolism.");
            Check(gm.Environment != null && gm.Environment.AverageColonization() >= 0f, "Environment remains sampleable.");
        }

        private static void Check(bool condition, string message)
        {
            if (condition)
            {
                Debug.Log("[V5Smoke] OK: " + message);
                return;
            }

            failures.Add(message);
            Debug.LogError("[V5Smoke] FAIL: " + message);
        }

        private static bool HasAdaptation(V5AdaptationId[] ids, V5AdaptationId id)
        {
            if (ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
                if (ids[i] == id) return true;
            return false;
        }

        private static void OnLogMessage(string condition, string stackTrace, LogType type)
        {
            if (type != LogType.Error && type != LogType.Exception && type != LogType.Assert) return;
            if (condition.Contains("[V5Smoke]")) return;
            if (IsUnitySearchIndexStartupNoise(condition, stackTrace)) return;
            failures.Add(condition);
        }

        private static bool IsUnitySearchIndexStartupNoise(string condition, string stackTrace)
        {
            if (!condition.Contains("ArgumentOutOfRangeException")) return false;
            return stackTrace.Contains("UnityEditor.Search.SearchDatabase") ||
                   stackTrace.Contains("UnityEditor.Search.SearchInit");
        }

        private static void Finish(int exitCode, string reason)
        {
            Debug.Log("[V5Smoke] " + reason + " Failures: " + failures.Count);
            for (int i = 0; i < failures.Count; i++) Debug.LogError("[V5Smoke] Failure " + (i + 1) + ": " + failures[i]);

            SessionState.SetBool(SessionActiveKey, false);
            Unhook();
            EditorApplication.isPlaying = false;
            EditorApplication.Exit(exitCode);
        }
    }
}
#endif
