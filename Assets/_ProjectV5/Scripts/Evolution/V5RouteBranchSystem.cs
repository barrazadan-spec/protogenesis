using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5RouteBranchId
    {
        None,
        BacteriaBiofilm,
        BacteriaSwarm,
        AmoebaHunter,
        AmoebaDigestive,
        ProducerBloom,
        ProducerTerraformer,
        VolvoxBody,
        VolvoxCaste
    }

    public enum V5BranchDoctrineChoice
    {
        None,
        Stabilize,
        Radicalize
    }

    public class V5RouteBranchSystem : MonoBehaviour, IV5RunResettable
    {
        public V5MvpRoute ActiveRoute = V5MvpRoute.None;
        public V5RouteBranchId ActiveBranch = V5RouteBranchId.None;
        public V5RouteBranchId RunnerUpBranch = V5RouteBranchId.None;
        public V5RouteBranchId LastEstablishedBranch = V5RouteBranchId.None;
        public float ActiveBranchScore01;
        public float RunnerUpScore01;
        public int BranchEstablishments;
        public int BranchAbilitySynergyCount;
        public int BranchDoctrineAbilityTriggerCount;
        public string ActiveBranchName = "sin rama";
        public string Summary = "Rama MVP: sin ruta.";
        public string LastBranchMoment = "Ramas MVP listas.";
        public string LastBranchAbilitySynergy = "sin sinergia de rama";
        public string LastBranchDoctrineAbility = "sin habilidad doctrinal";
        public string BranchObjectiveText = "Objetivo de rama: sin rama.";
        public float BranchObjectiveProgress01;
        public int BranchObjectiveCompletions;
        public string LastBranchObjectiveMoment = "sin objetivo de rama completado";
        public string BranchDoctrineObjectiveText = "Objetivo doctrinal: sin doctrina.";
        public float BranchDoctrineObjectiveProgress01;
        public int BranchDoctrineObjectiveCompletions;
        public string LastBranchDoctrineObjectiveMoment = "sin objetivo doctrinal completado";
        public bool BranchDoctrineObjectiveActive;
        public float BranchPassiveReadiness01;
        public int BranchPassivePulseCount;
        public string LastBranchPassiveEffect = "sin pasivo de rama";
        public string LastBranchVisualCue = "sin visual de rama";
        public float BranchAuraRadius;
        public bool BranchAuraVisible { get { return branchAuraRoot != null && branchAuraRoot.activeSelf; } }
        public V5RouteBranchId DoctrineBranch = V5RouteBranchId.None;
        public V5BranchDoctrineChoice ActiveBranchDoctrine = V5BranchDoctrineChoice.None;
        public bool BranchDoctrineAvailable;
        public int BranchDoctrineCommitments;
        public string ActiveBranchDoctrineName = "sin doctrina";
        public string BranchDoctrineOffer = "Doctrina de rama: completa el objetivo de rama.";
        public string LastBranchDoctrineMoment = "sin doctrina de rama";
        public float BranchTradeoffPressure01;

        public V5RouteBranchId BacteriaBranch = V5RouteBranchId.None;
        public V5RouteBranchId AmoebaBranch = V5RouteBranchId.None;
        public V5RouteBranchId ProducerBranch = V5RouteBranchId.None;
        public V5RouteBranchId VolvoxBranch = V5RouteBranchId.None;

        private const float BranchPassiveInterval = 9f;
        private float tick;
        private float passiveTick;
        private float branchAuraPulse;
        private float branchAuraPulseBoost;
        private int completedBranchObjectiveMask;
        private int completedBranchDoctrineObjectiveMask;
        private GameObject branchAuraRoot;
        private SpriteRenderer branchAuraZone;
        private SpriteRenderer branchAuraRing;
        private SpriteRenderer branchAuraCore;
        private static Sprite branchAuraCircleSprite;
        private static Sprite branchAuraRingSprite;

        public void ResetForNewRun()
        {
            ActiveRoute = V5MvpRoute.None;
            ActiveBranch = V5RouteBranchId.None;
            RunnerUpBranch = V5RouteBranchId.None;
            LastEstablishedBranch = V5RouteBranchId.None;
            ActiveBranchScore01 = 0f;
            RunnerUpScore01 = 0f;
            BranchEstablishments = 0;
            BranchAbilitySynergyCount = 0;
            BranchDoctrineAbilityTriggerCount = 0;
            ActiveBranchName = "sin rama";
            Summary = "Rama MVP: sin ruta.";
            LastBranchMoment = "Ramas MVP listas.";
            LastBranchAbilitySynergy = "sin sinergia de rama";
            LastBranchDoctrineAbility = "sin habilidad doctrinal";
            BranchObjectiveText = "Objetivo de rama: sin rama.";
            BranchObjectiveProgress01 = 0f;
            BranchObjectiveCompletions = 0;
            LastBranchObjectiveMoment = "sin objetivo de rama completado";
            BranchDoctrineObjectiveText = "Objetivo doctrinal: sin doctrina.";
            BranchDoctrineObjectiveProgress01 = 0f;
            BranchDoctrineObjectiveCompletions = 0;
            LastBranchDoctrineObjectiveMoment = "sin objetivo doctrinal completado";
            BranchDoctrineObjectiveActive = false;
            BranchPassiveReadiness01 = 0f;
            BranchPassivePulseCount = 0;
            LastBranchPassiveEffect = "sin pasivo de rama";
            LastBranchVisualCue = "sin visual de rama";
            BranchAuraRadius = 0f;
            DoctrineBranch = V5RouteBranchId.None;
            ActiveBranchDoctrine = V5BranchDoctrineChoice.None;
            BranchDoctrineAvailable = false;
            BranchDoctrineCommitments = 0;
            ActiveBranchDoctrineName = "sin doctrina";
            BranchDoctrineOffer = "Doctrina de rama: completa el objetivo de rama.";
            LastBranchDoctrineMoment = "sin doctrina de rama";
            BranchTradeoffPressure01 = 0f;
            BacteriaBranch = V5RouteBranchId.None;
            AmoebaBranch = V5RouteBranchId.None;
            ProducerBranch = V5RouteBranchId.None;
            VolvoxBranch = V5RouteBranchId.None;
            tick = 0f;
            passiveTick = 0f;
            branchAuraPulse = 0f;
            branchAuraPulseBoost = 0f;
            completedBranchObjectiveMask = 0;
            completedBranchDoctrineObjectiveMask = 0;
            HideBranchAura(null);
        }

        private void Update()
        {
            tick += Time.deltaTime;
            passiveTick += Time.deltaTime;
            V5GameManager gm = V5GameManager.Instance;
            if (tick >= 0.75f)
            {
                tick = 0f;
                EvaluateNow(gm);
            }

            if (gm == null || ActiveRoute == V5MvpRoute.None || StoredBranch(ActiveRoute) == V5RouteBranchId.None)
            {
                BranchPassiveReadiness01 = 0f;
                HideBranchAura("sin rama establecida");
                return;
            }

            UpdateBranchAura(gm);
            BranchPassiveReadiness01 = Mathf.Clamp01(passiveTick / BranchPassiveInterval);
            if (passiveTick >= BranchPassiveInterval)
            {
                passiveTick = 0f;
                ApplyBranchPassiveNow(gm);
            }
        }

        public bool EvaluateNow(V5GameManager gm)
        {
            RefreshSnapshot(gm);
            if (gm == null || ActiveRoute == V5MvpRoute.None || ActiveBranch == V5RouteBranchId.None) return false;

            V5RouteBranchId stored = StoredBranch(ActiveRoute);
            bool changed = false;
            bool enoughSignal = ActiveBranchScore01 >= 0.38f && ActiveBranchScore01 >= RunnerUpScore01 + 0.06f;
            if (enoughSignal && stored != ActiveBranch)
            {
                StoreBranch(ActiveRoute, ActiveBranch);
                LastEstablishedBranch = ActiveBranch;
                BranchEstablishments++;
                LastBranchMoment = "Rama " + V5MvpCanon.DisplayName(ActiveRoute) + ": " + BranchName(ActiveBranch) + " establecida.";
                ApplyBranchPulse(ActiveBranch, gm);
                ShowBranchAura(ActiveBranch, gm);

                if (gm.AffinityLog != null)
                    gm.AffinityLog.AddEvent(RouteToAffinityPath(ActiveRoute), 6f + ActiveBranchScore01 * 8f, BranchName(ActiveBranch), "mvp_branch");
                if (gm.Codex != null)
                    gm.Codex.Unlock("Rama MVP: " + V5MvpCanon.DisplayName(ActiveRoute), LastBranchMoment + " " + BranchAdvice(ActiveBranch));
                if (gm.Hud != null) gm.Hud.Toast(LastBranchMoment);

                V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                if (feedback != null && gm.MotherCell != null)
                    feedback.PushFloating(BranchName(ActiveBranch), (Vector2)gm.MotherCell.transform.position + Vector2.up * 1.9f, BranchColor(ActiveBranch));
                RefreshSnapshot(gm);
                changed = true;
            }

            if (TryCompleteBranchObjective(gm)) changed = true;
            if (TryCompleteBranchDoctrineObjective(gm)) changed = true;
            return changed;
        }

        public float BranchScoreForRoute(V5MvpRoute route)
        {
            if (route == ActiveRoute) return ActiveBranchScore01;
            return StoredBranch(route) != V5RouteBranchId.None ? 0.65f : 0f;
        }

        public V5RouteBranchId BranchForRoute(V5MvpRoute route)
        {
            V5RouteBranchId stored = StoredBranch(route);
            if (stored != V5RouteBranchId.None) return stored;
            return route == ActiveRoute ? ActiveBranch : V5RouteBranchId.None;
        }

        public bool RegisterRouteAbilitySynergy(V5MvpRoute route, Vector2 castCenter, float power, string abilityLabel, V5GameManager gm = null)
        {
            if (gm == null) gm = V5GameManager.Instance;
            if (gm == null || route == V5MvpRoute.None) return false;

            RefreshSnapshot(gm);
            V5RouteBranchId branch = BranchForRoute(route);
            if (branch == V5RouteBranchId.None) return false;

            float p = Mathf.Max(0.8f, power);
            ApplyBranchAbilitySynergy(branch, gm, castCenter, p);
            BranchAbilitySynergyCount++;
            LastBranchAbilitySynergy = "Sinergia " + BranchName(branch) + ": " + BranchAbilityText(branch) + ".";
            PulseBranchAura(branch, gm, "sinergia de habilidad activa");
            if (HasBranchDoctrine(branch))
            {
                ApplyBranchDoctrineAbilitySynergy(branch, ActiveBranchDoctrine, gm, castCenter, p);
                BranchDoctrineAbilityTriggerCount++;
                LastBranchDoctrineAbility = "Doctrina " + ActiveBranchDoctrineName + ": " + BranchDoctrineAbilityText(branch, ActiveBranchDoctrine) + ".";
                LastBranchAbilitySynergy += " " + LastBranchDoctrineAbility;
                PulseBranchAura(branch, gm, "habilidad doctrinal " + ActiveBranchDoctrineName);
            }

            if (gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(route), 4f + p, abilityLabel + " + " + BranchName(branch), "mvp_branch_ability");
            if (gm.Codex != null)
                gm.Codex.Unlock("Sinergia de rama: " + BranchName(branch), LastBranchAbilitySynergy);
            if (gm.Hud != null) gm.Hud.Toast(LastBranchAbilitySynergy);

            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null)
                feedback.PushFloating(BranchName(branch), castCenter + Vector2.up * 1.4f, BranchColor(branch));
            return true;
        }

        public string BranchStatus(V5GameManager gm)
        {
            RefreshSnapshot(gm);
            return Summary;
        }

        public string BranchObjectiveStatus(V5GameManager gm)
        {
            RefreshSnapshot(gm);
            return BranchObjectiveText + " | " + Percent(BranchObjectiveProgress01) + " | completados " + BranchObjectiveCompletions;
        }

        public bool IsBranchObjectiveCompleted(V5RouteBranchId branch)
        {
            return BranchObjectiveCompleted(branch);
        }

        public string BranchDoctrineObjectiveStatus(V5GameManager gm)
        {
            RefreshSnapshot(gm);
            if (ActiveBranchDoctrine == V5BranchDoctrineChoice.None || DoctrineBranch == V5RouteBranchId.None)
                return BranchDoctrineObjectiveText;
            return BranchDoctrineObjectiveText + " | " + Percent(BranchDoctrineObjectiveProgress01) + " | completados " + BranchDoctrineObjectiveCompletions;
        }

        public bool IsBranchDoctrineObjectiveCompleted(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            return BranchDoctrineObjectiveCompleted(branch, choice);
        }

        public string BranchPassiveStatus()
        {
            return LastBranchPassiveEffect + " | pasivo " + Percent(BranchPassiveReadiness01);
        }

        public bool ApplyBranchPassiveNow(V5GameManager gm = null)
        {
            if (gm == null) gm = V5GameManager.Instance;
            if (gm == null) return false;

            RefreshSnapshot(gm);
            V5RouteBranchId branch = BranchForRoute(ActiveRoute);
            if (branch == V5RouteBranchId.None || StoredBranch(ActiveRoute) == V5RouteBranchId.None) return false;

            ApplyBranchPassive(branch, gm);
            if (ActiveBranchDoctrine != V5BranchDoctrineChoice.None && DoctrineBranch == branch)
                ApplyBranchDoctrinePassive(branch, ActiveBranchDoctrine, gm);
            BranchPassivePulseCount++;
            BranchPassiveReadiness01 = 0f;
            passiveTick = 0f;
            LastBranchPassiveEffect = "Pasivo " + BranchName(branch) + ": " + BranchPassiveText(branch) + ".";
            if (ActiveBranchDoctrine != V5BranchDoctrineChoice.None && DoctrineBranch == branch)
                LastBranchPassiveEffect += " Doctrina " + ActiveBranchDoctrineName + " activa.";
            PulseBranchAura(branch, gm, "pasivo de rama");

            if (gm.AffinityLog != null && BranchPassivePulseCount <= 2)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(ActiveRoute), 2f, BranchName(branch), "mvp_branch_passive");
            if (gm.Hud != null && BranchPassivePulseCount == 1)
                gm.Hud.Toast(LastBranchPassiveEffect);
            return true;
        }

        public bool ForceBranchAuraNow(V5GameManager gm = null)
        {
            if (gm == null) gm = V5GameManager.Instance;
            if (gm == null) return false;

            RefreshSnapshot(gm);
            V5RouteBranchId branch = BranchForRoute(ActiveRoute);
            if (branch == V5RouteBranchId.None || StoredBranch(ActiveRoute) == V5RouteBranchId.None) return false;

            ShowBranchAura(branch, gm);
            return BranchAuraVisible;
        }

        public bool CanCommitBranchDoctrine(V5BranchDoctrineChoice choice, V5GameManager gm = null)
        {
            if (choice == V5BranchDoctrineChoice.None) return false;
            if (gm == null) gm = V5GameManager.Instance;
            if (gm == null) return false;

            RefreshSnapshot(gm);
            V5RouteBranchId branch = DoctrineBranch != V5RouteBranchId.None ? DoctrineBranch : BranchForRoute(ActiveRoute);
            return ActiveBranchDoctrine == V5BranchDoctrineChoice.None &&
                   branch != V5RouteBranchId.None &&
                   BranchObjectiveCompleted(branch);
        }

        public bool CommitBranchDoctrine(V5BranchDoctrineChoice choice, V5GameManager gm = null)
        {
            if (gm == null) gm = V5GameManager.Instance;
            if (!CanCommitBranchDoctrine(choice, gm)) return false;

            V5RouteBranchId branch = DoctrineBranch != V5RouteBranchId.None ? DoctrineBranch : BranchForRoute(ActiveRoute);
            DoctrineBranch = branch;
            ActiveBranchDoctrine = choice;
            BranchDoctrineAvailable = false;
            BranchDoctrineCommitments++;
            ActiveBranchDoctrineName = BranchDoctrineName(branch, choice);
            BranchTradeoffPressure01 = BranchDoctrinePressure(branch, choice);
            LastBranchDoctrineMoment = "Doctrina " + ActiveBranchDoctrineName + ": " +
                                       BranchDoctrineEffectText(branch, choice) +
                                       " | Coste: " + BranchDoctrineTradeoffText(branch, choice) + ".";
            BranchDoctrineObjectiveActive = true;
            BranchDoctrineObjectiveProgress01 = BranchDoctrineObjectiveProgress(branch, choice, gm);
            BranchDoctrineObjectiveText = "Objetivo doctrinal " + ActiveBranchDoctrineName + ": " + BranchDoctrineObjectiveGoal(branch, choice);
            LastBranchDoctrineObjectiveMoment = "Objetivo doctrinal abierto: " + ActiveBranchDoctrineName + ".";

            ApplyBranchDoctrine(branch, choice, gm);
            PulseBranchAura(branch, gm, "doctrina " + ActiveBranchDoctrineName);

            if (gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(ActiveRoute), 5f + BranchTradeoffPressure01 * 4f, ActiveBranchDoctrineName, "mvp_branch_doctrine");
            if (gm.Codex != null)
                gm.Codex.Unlock("Doctrina de rama: " + BranchName(branch), LastBranchDoctrineMoment);
            if (gm.Hud != null)
                gm.Hud.Toast(LastBranchDoctrineMoment);
            return true;
        }

        public string BranchDoctrineStatus(V5GameManager gm)
        {
            RefreshSnapshot(gm);
            V5RouteBranchId branch = BranchForRoute(ActiveRoute);
            if (branch == V5RouteBranchId.None || StoredBranch(ActiveRoute) == V5RouteBranchId.None)
                return "Doctrina de rama: sin rama establecida.";
            if (ActiveBranchDoctrine != V5BranchDoctrineChoice.None && DoctrineBranch == branch)
                return "Doctrina " + ActiveBranchDoctrineName + " | tradeoff " + Percent(BranchTradeoffPressure01);
            if (BranchDoctrineAvailable)
                return BranchDoctrineOffer;
            return "Doctrina " + BranchName(branch) + ": completa objetivo de rama.";
        }

        public bool HasBranchDoctrine(V5RouteBranchId branch)
        {
            return branch != V5RouteBranchId.None &&
                   DoctrineBranch == branch &&
                   ActiveBranchDoctrine != V5BranchDoctrineChoice.None;
        }

        public V5BranchDoctrineChoice DoctrineForBranch(V5RouteBranchId branch)
        {
            return HasBranchDoctrine(branch) ? ActiveBranchDoctrine : V5BranchDoctrineChoice.None;
        }

        public string DoctrineNameForBranch(V5RouteBranchId branch)
        {
            return HasBranchDoctrine(branch) ? ActiveBranchDoctrineName : "sin doctrina";
        }

        public float DoctrinePressureForBranch(V5RouteBranchId branch)
        {
            return HasBranchDoctrine(branch) ? BranchTradeoffPressure01 : 0f;
        }

        public float DoctrineCounterMultiplierForBranch(V5RouteBranchId branch)
        {
            if (!HasBranchDoctrine(branch)) return 1f;
            if (ActiveBranchDoctrine == V5BranchDoctrineChoice.Stabilize) return 0.92f;
            return Mathf.Clamp(1.12f + BranchTradeoffPressure01 * 0.18f, 1.12f, 1.28f);
        }

        public string DoctrineCounterNameForBranch(V5RouteBranchId branch, string baseCounterName)
        {
            if (!HasBranchDoctrine(branch)) return baseCounterName;
            if (ActiveBranchDoctrine == V5BranchDoctrineChoice.Stabilize)
                return baseCounterName + " disciplinado por " + ActiveBranchDoctrineName;
            return baseCounterName + " mutado por " + ActiveBranchDoctrineName;
        }

        public string DoctrineCounterAdviceForBranch(V5RouteBranchId branch)
        {
            if (!HasBranchDoctrine(branch)) return "";
            if (ActiveBranchDoctrine == V5BranchDoctrineChoice.Stabilize)
                return "La doctrina estabilizada reduce picos hostiles, pero vuelve el counter mas persistente.";
            return "La doctrina radical acelera la respuesta hostil: responde pronto o el coste evolutivo sube.";
        }

        public string DoctrineFinaleNameForBranch(V5RouteBranchId branch, string baseFinaleName)
        {
            if (!HasBranchDoctrine(branch)) return baseFinaleName;
            if (ActiveBranchDoctrine == V5BranchDoctrineChoice.Stabilize)
                return baseFinaleName + ": " + ActiveBranchDoctrineName;
            return ActiveBranchDoctrineName + " de " + baseFinaleName;
        }

        private void RefreshSnapshot(V5GameManager gm)
        {
            ActiveRoute = ActiveRouteFor(gm);
            if (gm == null || ActiveRoute == V5MvpRoute.None)
            {
                ActiveBranch = V5RouteBranchId.None;
                RunnerUpBranch = V5RouteBranchId.None;
                ActiveBranchScore01 = 0f;
                RunnerUpScore01 = 0f;
                ActiveBranchName = "sin rama";
                Summary = "Rama MVP: sin ruta.";
                BranchObjectiveText = "Objetivo de rama: sin ruta.";
                BranchObjectiveProgress01 = 0f;
                BranchDoctrineObjectiveText = "Objetivo doctrinal: sin ruta.";
                BranchDoctrineObjectiveProgress01 = 0f;
                BranchDoctrineObjectiveActive = false;
                RefreshBranchDoctrineOffer(V5RouteBranchId.None);
                return;
            }

            ScorePair pair = ScoreBranches(ActiveRoute, gm);
            ActiveBranch = pair.best;
            RunnerUpBranch = pair.runnerUp;
            ActiveBranchScore01 = pair.bestScore;
            RunnerUpScore01 = pair.runnerUpScore;
            ActiveBranchName = BranchName(ActiveBranch);

            V5RouteBranchId stored = StoredBranch(ActiveRoute);
            V5RouteBranchId objectiveBranch = stored != V5RouteBranchId.None ? stored : ActiveBranch;
            BranchObjectiveProgress01 = BranchObjectiveProgress(objectiveBranch, gm);
            BranchObjectiveText = "Objetivo " + BranchName(objectiveBranch) + ": " + BranchObjectiveGoal(objectiveBranch);
            RefreshBranchDoctrineOffer(stored);
            RefreshBranchDoctrineObjective(objectiveBranch, gm);
            string state = stored != V5RouteBranchId.None ? "establecida " + BranchName(stored) : "emergente";
            string doctrineObjective = ActiveBranchDoctrine != V5BranchDoctrineChoice.None && DoctrineBranch == objectiveBranch
                ? " | Obj doctrina " + Percent(BranchDoctrineObjectiveProgress01)
                : "";
            Summary = "Rama " + V5MvpCanon.DisplayName(ActiveRoute) + ": " + ActiveBranchName +
                      " " + Percent(ActiveBranchScore01) +
                      " vs " + BranchName(RunnerUpBranch) + " " + Percent(RunnerUpScore01) +
                      " | " + state + " | Obj " + Percent(BranchObjectiveProgress01) +
                      doctrineObjective +
                      " | " + BranchAdvice(ActiveBranch);
        }

        private ScorePair ScoreBranches(V5MvpRoute route, V5GameManager gm)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return Pair(V5RouteBranchId.BacteriaBiofilm, BacteriaBiofilmScore(gm), V5RouteBranchId.BacteriaSwarm, BacteriaSwarmScore(gm));
                case V5MvpRoute.Amoeba:
                    return Pair(V5RouteBranchId.AmoebaHunter, AmoebaHunterScore(gm), V5RouteBranchId.AmoebaDigestive, AmoebaDigestiveScore(gm));
                case V5MvpRoute.PhotosyntheticProducer:
                    return Pair(V5RouteBranchId.ProducerBloom, ProducerBloomScore(gm), V5RouteBranchId.ProducerTerraformer, ProducerTerraformerScore(gm));
                case V5MvpRoute.Volvox:
                    return Pair(V5RouteBranchId.VolvoxBody, VolvoxBodyScore(gm), V5RouteBranchId.VolvoxCaste, VolvoxCasteScore(gm));
                default:
                    return new ScorePair();
            }
        }

        private ScorePair Pair(V5RouteBranchId a, float aScore, V5RouteBranchId b, float bScore)
        {
            ScorePair p = new ScorePair();
            if (aScore >= bScore)
            {
                p.best = a;
                p.bestScore = Mathf.Clamp01(aScore);
                p.runnerUp = b;
                p.runnerUpScore = Mathf.Clamp01(bScore);
            }
            else
            {
                p.best = b;
                p.bestScore = Mathf.Clamp01(bScore);
                p.runnerUp = a;
                p.runnerUpScore = Mathf.Clamp01(aScore);
            }
            return p;
        }

        private float BacteriaBiofilmScore(V5GameManager gm)
        {
            return Mathf.Clamp01(Has(gm, V5AdaptationId.BacterialWall) * 0.28f +
                                 Has(gm, V5AdaptationId.PiliFimbriae) * 0.24f +
                                 ColonizationProgress(gm, 0.12f) * 0.26f +
                                 ChapterProgress(gm, V5MvpRoute.Bacteria) * 0.12f +
                                 CounterplayProgress(gm, V5MvpRoute.Bacteria) * 0.10f);
        }

        private float BacteriaSwarmScore(V5GameManager gm)
        {
            return Mathf.Clamp01(Has(gm, V5AdaptationId.BacterialFlagellum) * 0.28f +
                                 SwarmProgress(gm) * 0.28f +
                                 BuildProgress(gm, V5MvpRoute.Bacteria) * 0.22f +
                                 CasteProgress(gm, 2) * 0.12f +
                                 ComboProgress(gm, V5MvpRoute.Bacteria) * 0.10f);
        }

        private float AmoebaHunterScore(V5GameManager gm)
        {
            return Mathf.Clamp01(Has(gm, V5AdaptationId.Pseudopods) * 0.27f +
                                 Has(gm, V5AdaptationId.Lysosome) * 0.18f +
                                 CombatAffinityProgress(gm, V5EvolutionPath.Amoeba, 18f) * 0.22f +
                                 ComboProgress(gm, V5MvpRoute.Amoeba) * 0.18f +
                                 CounterplayProgress(gm, V5MvpRoute.Amoeba) * 0.15f);
        }

        private float AmoebaDigestiveScore(V5GameManager gm)
        {
            return Mathf.Clamp01(Has(gm, V5AdaptationId.Lysosome) * 0.30f +
                                 Has(gm, V5AdaptationId.Mitochondria) * 0.18f +
                                 ResourceProgress(gm, V5ResourceKind.AminoAcids, 80f) * 0.22f +
                                 OpportunityProgress(gm, V5MvpRoute.Amoeba) * 0.18f +
                                 ChapterProgress(gm, V5MvpRoute.Amoeba) * 0.12f);
        }

        private float ProducerBloomScore(V5GameManager gm)
        {
            return Mathf.Clamp01(Has(gm, V5AdaptationId.ProkaryoticThylakoid) * 0.28f +
                                 Has(gm, V5AdaptationId.Chloroplast) * 0.24f +
                                 OxygenProgress(gm, 0.34f) * 0.24f +
                                 ComboProgress(gm, V5MvpRoute.PhotosyntheticProducer) * 0.14f +
                                 ChapterProgress(gm, V5MvpRoute.PhotosyntheticProducer) * 0.10f);
        }

        private float ProducerTerraformerScore(V5GameManager gm)
        {
            return Mathf.Clamp01(Has(gm, V5AdaptationId.CatalaseROS) * 0.24f +
                                 Mathf.Max(Has(gm, V5AdaptationId.CelluloseWall), Has(gm, V5AdaptationId.SilicaFrustule)) * 0.22f +
                                 ColonizationProgress(gm, 0.14f) * 0.20f +
                                 LowToxinProgress(gm) * 0.18f +
                                 CounterplayProgress(gm, V5MvpRoute.PhotosyntheticProducer) * 0.16f);
        }

        private float VolvoxBodyScore(V5GameManager gm)
        {
            return Mathf.Clamp01(Mathf.Max(Has(gm, V5AdaptationId.BasicAdhesin), Has(gm, V5AdaptationId.ColonialAdhesin)) * 0.26f +
                                 BodyProgress(gm, 4) * 0.32f +
                                 ChapterProgress(gm, V5MvpRoute.Volvox) * 0.18f +
                                 CounterplayProgress(gm, V5MvpRoute.Volvox) * 0.12f +
                                 LowStressProgress(gm) * 0.12f);
        }

        private float VolvoxCasteScore(V5GameManager gm)
        {
            return Mathf.Clamp01(CasteProgress(gm, 3) * 0.34f +
                                 GerminalProgress(gm) * 0.22f +
                                 BuildProgress(gm, V5MvpRoute.Volvox) * 0.18f +
                                 ComboProgress(gm, V5MvpRoute.Volvox) * 0.14f +
                                 Has(gm, V5AdaptationId.BasicAdhesin) * 0.12f);
        }

        private void ApplyBranchPulse(V5RouteBranchId branch, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    mother.Resources.biomass += 18f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 5f);
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 4.6f, 0.006f, 0f, 0.006f, -0.018f, 0f, 0.055f, 0.004f);
                    break;
                case V5RouteBranchId.BacteriaSwarm:
                    mother.Resources.atp += 20f;
                    mother.Resources.biomass += 12f;
                    for (int i = 0; gm.PlayerCells != null && i < gm.PlayerCells.Count; i++)
                        if (gm.PlayerCells[i] != null) gm.PlayerCells[i].Stats.speed += 0.035f;
                    break;
                case V5RouteBranchId.AmoebaHunter:
                    mother.Stats.physicalDamagePerSecond += 0.08f;
                    mother.Resources.aminoAcids += 14f;
                    break;
                case V5RouteBranchId.AmoebaDigestive:
                    mother.Resources.biomass += 18f;
                    mother.Resources.aminoAcids += 22f;
                    mother.Stats.synthesisRate += 0.015f;
                    break;
                case V5RouteBranchId.ProducerBloom:
                    mother.Resources.atp += 30f;
                    mother.Stats.atpPerSecond += 0.05f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 6.0f, 0.006f, 0.018f, 0.060f, -0.020f, -0.004f, 0.014f, 0f);
                    break;
                case V5RouteBranchId.ProducerTerraformer:
                    mother.Resources.minerals += 14f;
                    mother.Stats.toxinResistance += 0.025f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 5.6f, 0.008f, 0.006f, 0.035f, -0.050f, -0.012f, 0.035f, 0.002f);
                    break;
                case V5RouteBranchId.VolvoxBody:
                    mother.Resources.lipids += 18f;
                    mother.Stats.repairPerSecond += 0.05f;
                    if (gm.Body != null) gm.Body.LastMessage = "Rama Volvox cuerpo: reparacion colonial reforzada.";
                    break;
                case V5RouteBranchId.VolvoxCaste:
                    mother.Resources.nucleotides += 16f;
                    mother.Resources.lipids += 10f;
                    if (gm.Germinal != null) gm.Germinal.LastMessage = "Rama Volvox castas: receta colonial favorecida.";
                    break;
            }
        }

        private void ApplyBranchAbilitySynergy(V5RouteBranchId branch, V5GameManager gm, Vector2 castCenter, float power)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    mother.Resources.biomass += 8f * power;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 3f * power);
                    mother.Stats.toxinResistance += 0.010f * power;
                    if (gm.Environment != null) gm.Environment.ModifyArea(castCenter, 5.8f + power, 0.010f * power, 0f, 0.008f * power, -0.055f * power, -0.010f, 0.090f * power, 0.004f);
                    break;
                case V5RouteBranchId.BacteriaSwarm:
                    mother.Resources.atp += 10f * power;
                    BuffNearbyPlayerCells(gm, castCenter, 7.0f + power, power, V5RouteBranchId.BacteriaSwarm);
                    break;
                case V5RouteBranchId.AmoebaHunter:
                    DamageEnemiesAround(gm, castCenter, 4.8f + power, 10f * power, V5DamageKind.Physical);
                    mother.Resources.aminoAcids += 8f * power;
                    mother.Stats.physicalDamagePerSecond += 0.025f * power;
                    break;
                case V5RouteBranchId.AmoebaDigestive:
                    mother.Resources.biomass += 9f * power;
                    mother.Resources.aminoAcids += 14f * power;
                    mother.Stats.synthesisRate += 0.012f * power;
                    if (gm.Environment != null) gm.Environment.ModifyArea(castCenter, 4.8f + power, 0.035f * power, 0f, 0.004f, -0.006f, 0f, 0.008f, 0.090f * power);
                    break;
                case V5RouteBranchId.ProducerBloom:
                    mother.Resources.atp += 18f * power;
                    mother.Stats.atpPerSecond += 0.025f * power;
                    mother.Stats.synthesisRate += 0.010f * power;
                    if (gm.Environment != null) gm.Environment.ModifyArea(castCenter, 7.0f + power, 0.006f, 0.120f * power, 0.150f * power, -0.035f * power, -0.006f, 0.030f * power, 0f);
                    break;
                case V5RouteBranchId.ProducerTerraformer:
                    mother.Resources.minerals += 10f * power;
                    mother.Stats.toxinResistance += 0.018f * power;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 4f * power);
                    if (gm.Environment != null) gm.Environment.ModifyArea(castCenter, 6.4f + power, 0.012f * power, 0.010f, 0.040f * power, -0.110f * power, -0.020f, 0.085f * power, 0.004f);
                    break;
                case V5RouteBranchId.VolvoxBody:
                    BuffNearbyPlayerCells(gm, castCenter, 7.2f + power, power, V5RouteBranchId.VolvoxBody);
                    mother.Stats.repairPerSecond += 0.020f * power;
                    if (gm.Body != null) gm.Body.LastMessage = "Sinergia de cuerpo Volvox: reparacion de red sincronizada.";
                    break;
                case V5RouteBranchId.VolvoxCaste:
                    BuffNearbyPlayerCells(gm, castCenter, 8.0f + power, power, V5RouteBranchId.VolvoxCaste);
                    mother.Resources.nucleotides += 9f * power;
                    mother.Resources.lipids += 6f * power;
                    if (gm.Germinal != null) gm.Germinal.LastMessage = "Sinergia de castas Volvox: funciones coloniales amplificadas.";
                    break;
            }
        }

        private void ApplyBranchDoctrineAbilitySynergy(V5RouteBranchId branch, V5BranchDoctrineChoice choice, V5GameManager gm, Vector2 castCenter, float power)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null || choice == V5BranchDoctrineChoice.None) return;

            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 4f * power);
                mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 5f * power);
                if (gm.Environment != null) gm.Environment.ModifyArea(castCenter, 5.6f + power, 0.006f * power, 0.004f, 0.008f, -0.040f * power, -0.006f, 0.045f * power, 0.004f);
            }
            else
            {
                mother.Stats.stress = Mathf.Min(100f, mother.Stats.stress + 2.2f * power);
                mother.Resources.atp += 7f * power;
                DamageEnemiesAround(gm, castCenter, 4.2f + power, 4.5f * power, V5DamageKind.Physical);
                if (gm.Environment != null) gm.Environment.ModifyArea(castCenter, 6.0f + power, 0.010f * power, 0.006f, 0.006f, 0.016f * power, 0.004f, 0.020f, 0.006f);
            }

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        mother.Stats.toxinResistance += 0.010f * power;
                        DirectNearbyCells(gm, castCenter, 6.2f + power, V5Directive.Colonize);
                    }
                    else
                    {
                        mother.Stats.chemicalDamagePerSecond += 0.012f * power;
                        mother.Stats.colonizationPower += 0.014f * power;
                    }
                    break;
                case V5RouteBranchId.BacteriaSwarm:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        BuffNearbyPlayerCells(gm, castCenter, 7.6f + power, 0.45f * power, V5RouteBranchId.BacteriaSwarm);
                        DirectNearbyCells(gm, castCenter, 7.6f + power, V5Directive.Explore);
                    }
                    else
                    {
                        mother.Stats.speed += 0.020f * power;
                        BuffNearbyPlayerCells(gm, castCenter, 8.4f + power, 0.75f * power, V5RouteBranchId.BacteriaSwarm);
                    }
                    break;
                case V5RouteBranchId.AmoebaHunter:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        mother.Stats.attackRange += 0.08f * power;
                        DirectNearbyCells(gm, castCenter, 6.4f + power, V5Directive.Attack);
                    }
                    else
                    {
                        mother.Stats.physicalDamagePerSecond += 0.020f * power;
                        DamageEnemiesAround(gm, castCenter, 5.2f + power, 8f * power, V5DamageKind.Physical);
                    }
                    break;
                case V5RouteBranchId.AmoebaDigestive:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        mother.Resources.aminoAcids += 8f * power;
                        DirectNearbyCells(gm, castCenter, 5.8f + power, V5Directive.Farm);
                    }
                    else
                    {
                        mother.Resources.aminoAcids += 14f * power;
                        mother.Stats.synthesisRate += 0.010f * power;
                    }
                    break;
                case V5RouteBranchId.ProducerBloom:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        mother.Stats.atpPerSecond += 0.010f * power;
                        DirectNearbyCells(gm, castCenter, 6.8f + power, V5Directive.Farm);
                    }
                    else
                    {
                        mother.Resources.atp += 16f * power;
                        mother.Stats.atpPerSecond += 0.018f * power;
                    }
                    break;
                case V5RouteBranchId.ProducerTerraformer:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        mother.Stats.toxinResistance += 0.012f * power;
                        DirectNearbyCells(gm, castCenter, 6.6f + power, V5Directive.Colonize);
                    }
                    else
                    {
                        mother.Resources.minerals += 12f * power;
                        mother.Stats.colonizationPower += 0.014f * power;
                    }
                    break;
                case V5RouteBranchId.VolvoxBody:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        BuffNearbyPlayerCells(gm, castCenter, 8.0f + power, 0.85f * power, V5RouteBranchId.VolvoxBody);
                        DirectNearbyCells(gm, castCenter, 7.0f + power, V5Directive.Defend);
                    }
                    else
                    {
                        mother.Stats.speed += 0.018f * power;
                        BuffNearbyPlayerCells(gm, castCenter, 7.6f + power, 0.65f * power, V5RouteBranchId.VolvoxBody);
                    }
                    break;
                case V5RouteBranchId.VolvoxCaste:
                    if (choice == V5BranchDoctrineChoice.Stabilize)
                    {
                        BuffNearbyPlayerCells(gm, castCenter, 8.2f + power, 0.75f * power, V5RouteBranchId.VolvoxCaste);
                        mother.Resources.nucleotides += 5f * power;
                    }
                    else
                    {
                        BuffNearbyPlayerCells(gm, castCenter, 8.8f + power, 0.95f * power, V5RouteBranchId.VolvoxCaste);
                        mother.Resources.nucleotides += 10f * power;
                    }
                    break;
            }
        }

        private bool TryCompleteBranchObjective(V5GameManager gm)
        {
            if (gm == null || ActiveRoute == V5MvpRoute.None) return false;
            V5RouteBranchId branch = BranchForRoute(ActiveRoute);
            if (branch == V5RouteBranchId.None || BranchObjectiveCompleted(branch)) return false;

            BranchObjectiveProgress01 = BranchObjectiveProgress(branch, gm);
            BranchObjectiveText = "Objetivo " + BranchName(branch) + ": " + BranchObjectiveGoal(branch);
            if (BranchObjectiveProgress01 < 0.90f) return false;

            MarkBranchObjectiveCompleted(branch);
            BranchObjectiveCompletions++;
            LastBranchObjectiveMoment = "Objetivo " + BranchName(branch) + " completado: " + BranchObjectiveRewardText(branch) + ".";
            ApplyBranchObjectiveReward(branch, gm);
            UnlockBranchDoctrine(branch, gm);
            PulseBranchAura(branch, gm, "objetivo de rama completado");

            if (gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(ActiveRoute), 7f + BranchObjectiveCompletions, BranchName(branch), "mvp_branch_objective");
            if (gm.Codex != null)
                gm.Codex.Unlock("Objetivo de rama: " + BranchName(branch), LastBranchObjectiveMoment);
            if (gm.Hud != null) gm.Hud.Toast(LastBranchObjectiveMoment);

            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null && gm.MotherCell != null)
                feedback.PushFloating("Objetivo rama", (Vector2)gm.MotherCell.transform.position + Vector2.up * 2.1f, BranchColor(branch));
            RefreshSnapshot(gm);
            return true;
        }

        private bool TryCompleteBranchDoctrineObjective(V5GameManager gm)
        {
            if (gm == null || ActiveBranchDoctrine == V5BranchDoctrineChoice.None || DoctrineBranch == V5RouteBranchId.None) return false;
            V5RouteBranchId branch = DoctrineBranch;
            V5BranchDoctrineChoice choice = ActiveBranchDoctrine;
            if (BranchDoctrineObjectiveCompleted(branch, choice)) return false;

            BranchDoctrineObjectiveActive = true;
            BranchDoctrineObjectiveProgress01 = BranchDoctrineObjectiveProgress(branch, choice, gm);
            BranchDoctrineObjectiveText = "Objetivo doctrinal " + ActiveBranchDoctrineName + ": " + BranchDoctrineObjectiveGoal(branch, choice);
            if (BranchDoctrineObjectiveProgress01 < 0.95f) return false;

            MarkBranchDoctrineObjectiveCompleted(branch, choice);
            BranchDoctrineObjectiveActive = false;
            BranchDoctrineObjectiveProgress01 = 1f;
            BranchDoctrineObjectiveCompletions++;
            LastBranchDoctrineObjectiveMoment = "Objetivo doctrinal " + ActiveBranchDoctrineName + " completado: " +
                                                BranchDoctrineObjectiveRewardText(branch, choice) + ".";
            ApplyBranchDoctrineObjectiveReward(branch, choice, gm);
            PulseBranchAura(branch, gm, "objetivo doctrinal completado");

            if (gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(RouteForBranch(branch)), 8f + BranchTradeoffPressure01 * 4f, ActiveBranchDoctrineName, "mvp_branch_doctrine_objective");
            if (gm.Codex != null)
                gm.Codex.Unlock("Objetivo doctrinal: " + ActiveBranchDoctrineName, LastBranchDoctrineObjectiveMoment);
            if (gm.Hud != null) gm.Hud.Toast(LastBranchDoctrineObjectiveMoment);

            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null && gm.MotherCell != null)
                feedback.PushFloating("Objetivo doctrina", (Vector2)gm.MotherCell.transform.position + Vector2.up * 2.35f, BranchColor(branch));
            RefreshSnapshot(gm);
            return true;
        }

        private void ApplyBranchObjectiveReward(V5RouteBranchId branch, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    mother.Resources.biomass += 22f;
                    mother.Stats.toxinResistance += 0.025f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 8f);
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 6.2f, 0.014f, 0f, 0.010f, -0.080f, -0.018f, 0.130f, 0.006f);
                    break;
                case V5RouteBranchId.BacteriaSwarm:
                    mother.Resources.atp += 34f;
                    mother.Resources.biomass += 18f;
                    BuffNearbyPlayerCells(gm, mother.transform.position, 9.0f, 1.5f, V5RouteBranchId.BacteriaSwarm);
                    break;
                case V5RouteBranchId.AmoebaHunter:
                    mother.Resources.aminoAcids += 28f;
                    mother.Stats.physicalDamagePerSecond += 0.060f;
                    DamageEnemiesAround(gm, mother.transform.position, 5.6f, 14f, V5DamageKind.Physical);
                    break;
                case V5RouteBranchId.AmoebaDigestive:
                    mother.Resources.biomass += 24f;
                    mother.Resources.aminoAcids += 32f;
                    mother.Stats.synthesisRate += 0.020f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 5.2f, 0.045f, 0f, 0.006f, -0.012f, 0f, 0.018f, 0.120f);
                    break;
                case V5RouteBranchId.ProducerBloom:
                    mother.Resources.atp += 42f;
                    mother.Stats.atpPerSecond += 0.050f;
                    mother.Stats.synthesisRate += 0.018f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 8.0f, 0.010f, 0.180f, 0.220f, -0.060f, -0.010f, 0.040f, 0f);
                    break;
                case V5RouteBranchId.ProducerTerraformer:
                    mother.Resources.minerals += 28f;
                    mother.Stats.toxinResistance += 0.035f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 7.2f, 0.018f, 0.020f, 0.060f, -0.150f, -0.035f, 0.120f, 0.004f);
                    break;
                case V5RouteBranchId.VolvoxBody:
                    mother.Resources.lipids += 26f;
                    mother.Stats.repairPerSecond += 0.060f;
                    BuffNearbyPlayerCells(gm, mother.transform.position, 9.0f, 1.6f, V5RouteBranchId.VolvoxBody);
                    if (gm.Body != null) gm.Body.LastMessage = "Objetivo Cuerpo Volvox: red corporal estabilizada.";
                    break;
                case V5RouteBranchId.VolvoxCaste:
                    mother.Resources.nucleotides += 24f;
                    mother.Resources.lipids += 18f;
                    BuffNearbyPlayerCells(gm, mother.transform.position, 9.0f, 1.5f, V5RouteBranchId.VolvoxCaste);
                    if (gm.Germinal != null) gm.Germinal.LastMessage = "Objetivo Castas Volvox: especializacion colonial reforzada.";
                    break;
            }
        }

        private void ApplyBranchDoctrineObjectiveReward(V5RouteBranchId branch, V5BranchDoctrineChoice choice, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null || choice == V5BranchDoctrineChoice.None) return;

            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 12f);
                mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 16f);
                mother.Stats.repairPerSecond += 0.020f;
                mother.Resources.biomass += 18f;
                if (gm.Environment != null)
                    gm.Environment.ModifyArea(mother.transform.position, BranchAuraRadiusFor(branch) + 1.0f, 0.008f, 0.006f, 0.016f, -0.060f, -0.010f, 0.070f, 0.006f);
            }
            else
            {
                mother.Resources.atp += 36f;
                mother.Resources.biomass += 16f;
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 4f);
                DamageEnemiesAround(gm, mother.transform.position, BranchAuraRadiusFor(branch) + 1.6f, 16f, V5DamageKind.Chemical);
                if (gm.Environment != null)
                    gm.Environment.ModifyArea(mother.transform.position, BranchAuraRadiusFor(branch) + 1.4f, 0.014f, 0.010f, 0.012f, -0.018f, 0.004f, 0.044f, 0.006f);
            }

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    mother.Stats.colonizationPower += choice == V5BranchDoctrineChoice.Stabilize ? 0.026f : 0.050f;
                    mother.Stats.toxinResistance += choice == V5BranchDoctrineChoice.Stabilize ? 0.024f : 0.012f;
                    break;
                case V5RouteBranchId.BacteriaSwarm:
                    mother.Stats.speed += choice == V5BranchDoctrineChoice.Stabilize ? 0.020f : 0.060f;
                    BuffNearbyPlayerCells(gm, mother.transform.position, 8.8f, choice == V5BranchDoctrineChoice.Stabilize ? 0.8f : 1.4f, branch);
                    break;
                case V5RouteBranchId.AmoebaHunter:
                    mother.Stats.physicalDamagePerSecond += choice == V5BranchDoctrineChoice.Stabilize ? 0.040f : 0.080f;
                    mother.Resources.aminoAcids += choice == V5BranchDoctrineChoice.Stabilize ? 16f : 30f;
                    break;
                case V5RouteBranchId.AmoebaDigestive:
                    mother.Stats.synthesisRate += choice == V5BranchDoctrineChoice.Stabilize ? 0.026f : 0.052f;
                    mother.Resources.aminoAcids += choice == V5BranchDoctrineChoice.Stabilize ? 18f : 34f;
                    break;
                case V5RouteBranchId.ProducerBloom:
                    mother.Stats.atpPerSecond += choice == V5BranchDoctrineChoice.Stabilize ? 0.030f : 0.064f;
                    mother.Resources.atp += choice == V5BranchDoctrineChoice.Stabilize ? 18f : 34f;
                    break;
                case V5RouteBranchId.ProducerTerraformer:
                    mother.Stats.toxinResistance += choice == V5BranchDoctrineChoice.Stabilize ? 0.040f : 0.026f;
                    mother.Resources.minerals += choice == V5BranchDoctrineChoice.Stabilize ? 20f : 38f;
                    break;
                case V5RouteBranchId.VolvoxBody:
                    mother.Stats.maxHp += choice == V5BranchDoctrineChoice.Stabilize ? 14f : 8f;
                    mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 18f);
                    if (gm.Body != null) gm.Body.LastMessage = "Objetivo doctrinal Volvox: cuerpo sincronico resuelto.";
                    break;
                case V5RouteBranchId.VolvoxCaste:
                    mother.Resources.nucleotides += choice == V5BranchDoctrineChoice.Stabilize ? 18f : 34f;
                    mother.Resources.lipids += choice == V5BranchDoctrineChoice.Stabilize ? 14f : 26f;
                    if (gm.Germinal != null) gm.Germinal.LastMessage = "Objetivo doctrinal Volvox: castas consolidadas.";
                    break;
            }
        }

        private void ApplyBranchPassive(V5RouteBranchId branch, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    mother.Resources.biomass += 3f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 1.4f);
                    mother.Stats.toxinResistance += 0.002f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 4.2f, 0.003f, 0f, 0.002f, -0.018f, -0.002f, 0.025f, 0.001f);
                    DirectNearbyCells(gm, mother.transform.position, 5.2f, V5Directive.Colonize);
                    break;
                case V5RouteBranchId.BacteriaSwarm:
                    mother.Resources.atp += 5f;
                    BuffNearbyPlayerCells(gm, mother.transform.position, 7.2f, 0.35f, V5RouteBranchId.BacteriaSwarm);
                    DirectNearbyCells(gm, mother.transform.position, 7.2f, V5Directive.Explore);
                    break;
                case V5RouteBranchId.AmoebaHunter:
                    mother.Resources.aminoAcids += 3f;
                    mother.Stats.physicalDamagePerSecond += 0.004f;
                    DamageEnemiesAround(gm, mother.transform.position, 4.2f, 2.6f, V5DamageKind.Physical);
                    DirectNearbyCells(gm, mother.transform.position, 6.0f, V5Directive.Attack);
                    break;
                case V5RouteBranchId.AmoebaDigestive:
                    mother.Resources.biomass += 3f;
                    mother.Resources.aminoAcids += 4f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 4.4f, 0.009f, 0f, 0f, -0.004f, 0f, 0.004f, 0.025f);
                    DirectNearbyCells(gm, mother.transform.position, 5.8f, V5Directive.Farm);
                    break;
                case V5RouteBranchId.ProducerBloom:
                    mother.Resources.atp += 7f;
                    mother.Stats.atpPerSecond += 0.004f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 6.0f, 0.002f, 0.035f, 0.045f, -0.010f, -0.001f, 0.006f, 0f);
                    DirectNearbyCells(gm, mother.transform.position, 6.2f, V5Directive.Farm);
                    break;
                case V5RouteBranchId.ProducerTerraformer:
                    mother.Resources.minerals += 4f;
                    mother.Stats.toxinResistance += 0.004f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 5.8f, 0.004f, 0.004f, 0.012f, -0.035f, -0.006f, 0.028f, 0.001f);
                    DirectNearbyCells(gm, mother.transform.position, 5.8f, V5Directive.Colonize);
                    break;
                case V5RouteBranchId.VolvoxBody:
                    mother.Resources.lipids += 4f;
                    mother.Stats.repairPerSecond += 0.006f;
                    BuffNearbyPlayerCells(gm, mother.transform.position, 7.4f, 0.45f, V5RouteBranchId.VolvoxBody);
                    DirectNearbyCells(gm, mother.transform.position, 7.4f, V5Directive.Defend);
                    break;
                case V5RouteBranchId.VolvoxCaste:
                    mother.Resources.nucleotides += 3f;
                    mother.Resources.lipids += 2f;
                    BuffNearbyPlayerCells(gm, mother.transform.position, 7.8f, 0.38f, V5RouteBranchId.VolvoxCaste);
                    DirectNearbyCells(gm, mother.transform.position, 7.8f, V5Directive.FollowMother);
                    break;
            }
        }

        private void BuffNearbyPlayerCells(V5GameManager gm, Vector2 center, float radius, float power, V5RouteBranchId branch)
        {
            if (gm == null || gm.PlayerCells == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                if (Vector2.Distance(center, c.transform.position) > radius && branch != V5RouteBranchId.VolvoxBody) continue;

                switch (branch)
                {
                    case V5RouteBranchId.BacteriaSwarm:
                        c.Resources.atp += 3f * power;
                        c.Stats.speed += 0.025f * power;
                        c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 2f * power);
                        if (c.Role != V5CellRole.Mother) c.Directive = V5Directive.Explore;
                        break;
                    case V5RouteBranchId.VolvoxBody:
                        if (c.Role != V5CellRole.Mother && !c.IsAttachedToBody && Vector2.Distance(center, c.transform.position) > radius) continue;
                        c.Stats.currentHp = Mathf.Min(c.Stats.maxHp, c.Stats.currentHp + 9f * power);
                        c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 7f * power);
                        if (c.Role != V5CellRole.Mother) c.Directive = V5Directive.Defend;
                        break;
                    case V5RouteBranchId.VolvoxCaste:
                        if (c.Role == V5CellRole.Mother) continue;
                        c.Resources.atp += 4f * power;
                        c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 3f * power);
                        switch (c.FunctionalCaste)
                        {
                            case V5FunctionalCasteId.Attacker: c.Stats.physicalDamagePerSecond += 0.020f * power; break;
                            case V5FunctionalCasteId.Defender:
                            case V5FunctionalCasteId.Structural:
                                c.Stats.currentHp = Mathf.Min(c.Stats.maxHp, c.Stats.currentHp + 8f * power);
                                break;
                            case V5FunctionalCasteId.Producer: c.Stats.synthesisRate += 0.010f * power; break;
                            case V5FunctionalCasteId.Sensor: c.Stats.sensorRange += 0.35f * power; break;
                            case V5FunctionalCasteId.Gatherer: c.Stats.speed += 0.018f * power; break;
                        }
                        break;
                }
            }
        }

        private void DirectNearbyCells(V5GameManager gm, Vector2 center, float radius, V5Directive directive)
        {
            if (gm == null || gm.PlayerCells == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (Vector2.Distance(center, c.transform.position) > radius) continue;
                c.Directive = directive;
            }
        }

        private void DamageEnemiesAround(V5GameManager gm, Vector2 center, float radius, float damage, V5DamageKind kind)
        {
            if (gm == null || gm.NonPlayerCells == null) return;
            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity e = gm.NonPlayerCells[i];
                if (e == null) continue;
                float d = Vector2.Distance(center, e.transform.position);
                if (d > radius) continue;
                float falloff = 1f - Mathf.Clamp01(d / Mathf.Max(0.01f, radius));
                e.Damage(damage * (0.35f + falloff), kind, center);
            }
        }

        private void UnlockBranchDoctrine(V5RouteBranchId branch, V5GameManager gm)
        {
            if (branch == V5RouteBranchId.None || ActiveBranchDoctrine != V5BranchDoctrineChoice.None) return;

            DoctrineBranch = branch;
            BranchDoctrineAvailable = true;
            ActiveBranchDoctrineName = "sin doctrina";
            BranchTradeoffPressure01 = 0f;
            BranchDoctrineOffer = BranchDoctrineOfferText(branch);
            LastBranchDoctrineMoment = BranchDoctrineOffer;

            if (gm != null && gm.Codex != null)
                gm.Codex.Unlock("Decision de rama: " + BranchName(branch), BranchDoctrineOffer);
        }

        private void RefreshBranchDoctrineOffer(V5RouteBranchId storedBranch)
        {
            if (ActiveBranchDoctrine != V5BranchDoctrineChoice.None)
            {
                BranchDoctrineAvailable = false;
                return;
            }

            if (storedBranch != V5RouteBranchId.None && BranchObjectiveCompleted(storedBranch))
            {
                DoctrineBranch = storedBranch;
                BranchDoctrineAvailable = true;
                BranchDoctrineOffer = BranchDoctrineOfferText(storedBranch);
            }
            else
            {
                if (DoctrineBranch == V5RouteBranchId.None) BranchDoctrineOffer = "Doctrina de rama: completa el objetivo de rama.";
                BranchDoctrineAvailable = false;
            }
        }

        private void RefreshBranchDoctrineObjective(V5RouteBranchId visibleBranch, V5GameManager gm)
        {
            if (ActiveBranchDoctrine == V5BranchDoctrineChoice.None || DoctrineBranch == V5RouteBranchId.None)
            {
                BranchDoctrineObjectiveActive = false;
                BranchDoctrineObjectiveProgress01 = 0f;
                BranchDoctrineObjectiveText = BranchDoctrineAvailable
                    ? "Objetivo doctrinal: elige Anclar o Radicalizar."
                    : "Objetivo doctrinal: sin doctrina.";
                return;
            }

            V5RouteBranchId branch = DoctrineBranch != V5RouteBranchId.None ? DoctrineBranch : visibleBranch;
            if (branch == V5RouteBranchId.None)
            {
                BranchDoctrineObjectiveActive = false;
                BranchDoctrineObjectiveProgress01 = 0f;
                BranchDoctrineObjectiveText = "Objetivo doctrinal: sin rama.";
                return;
            }

            bool completed = BranchDoctrineObjectiveCompleted(branch, ActiveBranchDoctrine);
            BranchDoctrineObjectiveActive = !completed;
            BranchDoctrineObjectiveProgress01 = completed ? 1f : BranchDoctrineObjectiveProgress(branch, ActiveBranchDoctrine, gm);
            BranchDoctrineObjectiveText = "Objetivo doctrinal " + ActiveBranchDoctrineName + ": " +
                                          BranchDoctrineObjectiveGoal(branch, ActiveBranchDoctrine);
        }

        private void ApplyBranchDoctrine(V5RouteBranchId branch, V5BranchDoctrineChoice choice, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 10f);
                mother.Stats.repairPerSecond += 0.018f;
                mother.Resources.biomass += 12f;
                mother.Resources.atp = Mathf.Max(0f, mother.Resources.atp - 8f);
                if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, BranchAuraRadiusFor(branch), 0.006f, 0.004f, 0.010f, -0.028f, -0.006f, 0.036f, 0.004f);
            }
            else if (choice == V5BranchDoctrineChoice.Radicalize)
            {
                mother.Stats.stress = Mathf.Min(100f, mother.Stats.stress + 9f);
                mother.Resources.atp += 26f;
                mother.Resources.biomass += 8f;
                if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, BranchAuraRadiusFor(branch) + 0.8f, 0.012f, 0.010f, 0.010f, 0.012f, 0.008f, 0.020f, 0.004f);
            }

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    mother.Stats.toxinResistance += choice == V5BranchDoctrineChoice.Stabilize ? 0.035f : 0.018f;
                    mother.Stats.colonizationPower += choice == V5BranchDoctrineChoice.Stabilize ? 0.025f : 0.060f;
                    if (choice == V5BranchDoctrineChoice.Radicalize) mother.Stats.chemicalDamagePerSecond += 0.018f;
                    break;
                case V5RouteBranchId.BacteriaSwarm:
                    mother.Stats.speed += choice == V5BranchDoctrineChoice.Stabilize ? 0.018f : 0.070f;
                    mother.Stats.sensorRange += choice == V5BranchDoctrineChoice.Stabilize ? 0.45f : 0.85f;
                    if (choice == V5BranchDoctrineChoice.Radicalize) mother.Stats.repairPerSecond = Mathf.Max(0f, mother.Stats.repairPerSecond - 0.006f);
                    break;
                case V5RouteBranchId.AmoebaHunter:
                    mother.Stats.physicalDamagePerSecond += choice == V5BranchDoctrineChoice.Stabilize ? 0.035f : 0.085f;
                    mother.Stats.attackRange += choice == V5BranchDoctrineChoice.Stabilize ? 0.18f : 0.34f;
                    if (choice == V5BranchDoctrineChoice.Radicalize) mother.Stats.toxinResistance = Mathf.Max(0f, mother.Stats.toxinResistance - 0.008f);
                    break;
                case V5RouteBranchId.AmoebaDigestive:
                    mother.Stats.synthesisRate += choice == V5BranchDoctrineChoice.Stabilize ? 0.030f : 0.060f;
                    mother.Resources.aminoAcids += choice == V5BranchDoctrineChoice.Stabilize ? 14f : 30f;
                    if (choice == V5BranchDoctrineChoice.Radicalize) mother.Stats.speed = Mathf.Max(0.35f, mother.Stats.speed - 0.020f);
                    break;
                case V5RouteBranchId.ProducerBloom:
                    mother.Stats.atpPerSecond += choice == V5BranchDoctrineChoice.Stabilize ? 0.030f : 0.070f;
                    mother.Stats.synthesisRate += choice == V5BranchDoctrineChoice.Stabilize ? 0.018f : 0.035f;
                    if (choice == V5BranchDoctrineChoice.Radicalize) mother.Stats.toxinResistance = Mathf.Max(0f, mother.Stats.toxinResistance - 0.010f);
                    break;
                case V5RouteBranchId.ProducerTerraformer:
                    mother.Stats.toxinResistance += choice == V5BranchDoctrineChoice.Stabilize ? 0.045f : 0.030f;
                    mother.Resources.minerals += choice == V5BranchDoctrineChoice.Stabilize ? 16f : 34f;
                    if (choice == V5BranchDoctrineChoice.Radicalize) mother.Stats.stress = Mathf.Min(100f, mother.Stats.stress + 4f);
                    break;
                case V5RouteBranchId.VolvoxBody:
                    mother.Stats.maxHp += choice == V5BranchDoctrineChoice.Stabilize ? 16f : 8f;
                    mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + (choice == V5BranchDoctrineChoice.Stabilize ? 20f : 10f));
                    mother.Stats.repairPerSecond += choice == V5BranchDoctrineChoice.Stabilize ? 0.045f : 0.020f;
                    if (choice == V5BranchDoctrineChoice.Radicalize) mother.Stats.speed += 0.035f;
                    break;
                case V5RouteBranchId.VolvoxCaste:
                    mother.Resources.nucleotides += choice == V5BranchDoctrineChoice.Stabilize ? 14f : 32f;
                    mother.Resources.lipids += choice == V5BranchDoctrineChoice.Stabilize ? 10f : 22f;
                    if (choice == V5BranchDoctrineChoice.Stabilize) mother.Stats.repairPerSecond += 0.020f;
                    else mother.Stats.sensorRange += 0.65f;
                    break;
            }
        }

        private void ApplyBranchDoctrinePassive(V5RouteBranchId branch, V5BranchDoctrineChoice choice, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 1.5f);
                mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 1.8f);
            }
            else if (choice == V5BranchDoctrineChoice.Radicalize)
            {
                mother.Stats.stress = Mathf.Min(100f, mother.Stats.stress + 1.2f);
                mother.Resources.atp += 3f;
                if (branch == V5RouteBranchId.AmoebaHunter) mother.Stats.physicalDamagePerSecond += 0.002f;
                if (branch == V5RouteBranchId.ProducerBloom) mother.Stats.atpPerSecond += 0.002f;
                if (branch == V5RouteBranchId.BacteriaBiofilm) mother.Stats.colonizationPower += 0.002f;
            }
        }

        private void ShowBranchAura(V5RouteBranchId branch, V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null || branch == V5RouteBranchId.None)
            {
                HideBranchAura("sin rama establecida");
                return;
            }

            EnsureBranchAura();
            if (branchAuraRoot == null) return;

            Color color = BranchColor(branch);
            BranchAuraRadius = BranchAuraRadiusFor(branch);
            branchAuraRoot.SetActive(true);
            branchAuraZone.color = new Color(color.r, color.g, color.b, 0.11f);
            branchAuraRing.color = new Color(color.r, color.g, color.b, 0.64f);
            branchAuraCore.color = new Color(1f, 1f, 1f, 0.36f);
            LastBranchVisualCue = "Aura " + BranchName(branch) + ": " + BranchVisualText(branch) + ".";
            UpdateBranchAuraVisual(branch, gm);
        }

        private void PulseBranchAura(V5RouteBranchId branch, V5GameManager gm, string cue)
        {
            ShowBranchAura(branch, gm);
            branchAuraPulseBoost = 1f;
            LastBranchVisualCue = "Aura " + BranchName(branch) + ": " + cue + " | " + BranchVisualText(branch) + ".";

            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null && gm != null && gm.MotherCell != null)
                feedback.PushFloating("Aura " + BranchName(branch), (Vector2)gm.MotherCell.transform.position + Vector2.up * 1.55f, BranchColor(branch));
        }

        private void HideBranchAura(string cue)
        {
            if (!string.IsNullOrEmpty(cue)) LastBranchVisualCue = cue;
            BranchAuraRadius = 0f;
            branchAuraPulseBoost = 0f;
            if (branchAuraRoot != null) branchAuraRoot.SetActive(false);
        }

        private void UpdateBranchAura(V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null || ActiveRoute == V5MvpRoute.None)
            {
                HideBranchAura("sin rama establecida");
                return;
            }

            V5RouteBranchId branch = StoredBranch(ActiveRoute);
            if (branch == V5RouteBranchId.None)
            {
                HideBranchAura("sin rama establecida");
                return;
            }

            if (!BranchAuraVisible) ShowBranchAura(branch, gm);
            UpdateBranchAuraVisual(branch, gm);
        }

        private void UpdateBranchAuraVisual(V5RouteBranchId branch, V5GameManager gm)
        {
            if (branchAuraRoot == null || !branchAuraRoot.activeSelf || gm == null || gm.MotherCell == null) return;

            branchAuraPulse += Time.deltaTime;
            branchAuraPulseBoost = Mathf.MoveTowards(branchAuraPulseBoost, 0f, Time.deltaTime * 1.8f);

            Vector3 motherPos = gm.MotherCell.transform.position;
            branchAuraRoot.transform.position = new Vector3(motherPos.x, motherPos.y, -0.055f);

            float radius = Mathf.Max(1f, BranchAuraRadiusFor(branch));
            float activity = Mathf.Clamp01(Mathf.Max(BranchPassiveReadiness01, BranchObjectiveProgress01) + branchAuraPulseBoost * 0.35f);
            float wave = Mathf.Sin(branchAuraPulse * Mathf.Lerp(2.4f, 5.8f, activity));
            float pulse = 1f + wave * Mathf.Lerp(0.03f, 0.08f, activity) + branchAuraPulseBoost * 0.08f;

            branchAuraZone.transform.localScale = Vector3.one * radius * 2.0f * (1.03f + activity * 0.08f);
            branchAuraRing.transform.localScale = Vector3.one * radius * 2.0f * pulse;
            branchAuraCore.transform.localScale = Vector3.one * Mathf.Lerp(0.36f, 0.64f, 0.5f + wave * 0.5f + branchAuraPulseBoost * 0.25f);

            Color zone = branchAuraZone.color;
            zone.a = Mathf.Lerp(0.08f, 0.16f, activity);
            branchAuraZone.color = zone;

            Color ring = branchAuraRing.color;
            ring.a = Mathf.Lerp(0.46f, 0.82f, activity);
            branchAuraRing.color = ring;

            Color core = branchAuraCore.color;
            core.a = Mathf.Lerp(0.24f, 0.52f, activity);
            branchAuraCore.color = core;
        }

        private void EnsureBranchAura()
        {
            if (branchAuraRoot != null) return;
            if (branchAuraCircleSprite == null) branchAuraCircleSprite = V5ProceduralSprites.CreateCircleSprite(96);
            if (branchAuraRingSprite == null) branchAuraRingSprite = V5ProceduralSprites.CreateRingSprite(128, 0.10f);

            branchAuraRoot = new GameObject("V5_RouteBranchAura");
            branchAuraRoot.transform.SetParent(transform, false);
            branchAuraZone = CreateBranchAuraRenderer("BranchAuraZone", branchAuraCircleSprite, 3);
            branchAuraRing = CreateBranchAuraRenderer("BranchAuraRing", branchAuraRingSprite, 8);
            branchAuraCore = CreateBranchAuraRenderer("BranchAuraCore", branchAuraCircleSprite, 9);
            branchAuraRoot.SetActive(false);
        }

        private SpriteRenderer CreateBranchAuraRenderer(string childName, Sprite sprite, int sortingOrder)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(branchAuraRoot.transform, false);
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private float BranchAuraRadiusFor(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return 3.6f;
                case V5RouteBranchId.BacteriaSwarm: return 4.2f;
                case V5RouteBranchId.AmoebaHunter: return 3.4f;
                case V5RouteBranchId.AmoebaDigestive: return 3.7f;
                case V5RouteBranchId.ProducerBloom: return 4.6f;
                case V5RouteBranchId.ProducerTerraformer: return 4.1f;
                case V5RouteBranchId.VolvoxBody: return 4.0f;
                case V5RouteBranchId.VolvoxCaste: return 4.4f;
                default: return 3.4f;
            }
        }

        private V5RouteBranchId StoredBranch(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return BacteriaBranch;
                case V5MvpRoute.Amoeba: return AmoebaBranch;
                case V5MvpRoute.PhotosyntheticProducer: return ProducerBranch;
                case V5MvpRoute.Volvox: return VolvoxBranch;
                default: return V5RouteBranchId.None;
            }
        }

        private void StoreBranch(V5MvpRoute route, V5RouteBranchId branch)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaBranch = branch; break;
                case V5MvpRoute.Amoeba: AmoebaBranch = branch; break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerBranch = branch; break;
                case V5MvpRoute.Volvox: VolvoxBranch = branch; break;
            }
        }

        private V5MvpRoute ActiveRouteFor(V5GameManager gm)
        {
            if (gm == null) return V5MvpRoute.None;
            if (gm.MvpIntent != null) return gm.MvpIntent.EffectiveRoute(gm);
            return V5MvpCanon.CurrentRoute(gm);
        }

        private V5MvpRoute RouteForBranch(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                case V5RouteBranchId.BacteriaSwarm:
                    return V5MvpRoute.Bacteria;
                case V5RouteBranchId.AmoebaHunter:
                case V5RouteBranchId.AmoebaDigestive:
                    return V5MvpRoute.Amoeba;
                case V5RouteBranchId.ProducerBloom:
                case V5RouteBranchId.ProducerTerraformer:
                    return V5MvpRoute.PhotosyntheticProducer;
                case V5RouteBranchId.VolvoxBody:
                case V5RouteBranchId.VolvoxCaste:
                    return V5MvpRoute.Volvox;
                default:
                    return V5MvpRoute.None;
            }
        }

        private float Has(V5GameManager gm, V5AdaptationId id)
        {
            return gm != null && gm.Adaptations != null && gm.Adaptations.Has(id) ? 1f : 0f;
        }

        private float BuildProgress(V5GameManager gm, V5MvpRoute route)
        {
            return gm != null ? V5MvpCanon.BuildProgress01(route, gm.Adaptations) : 0f;
        }

        private float ChapterProgress(V5GameManager gm, V5MvpRoute route)
        {
            return gm != null && gm.RouteChapters != null ? gm.RouteChapters.RouteChapterScore01(route) : 0f;
        }

        private float ComboProgress(V5GameManager gm, V5MvpRoute route)
        {
            return gm != null && gm.WorldEvents != null ? Mathf.Clamp01(gm.WorldEvents.ComboCountForRoute(route) / 2f) : 0f;
        }

        private float OpportunityProgress(V5GameManager gm, V5MvpRoute route)
        {
            return gm != null && gm.RouteMastery != null ? Mathf.Clamp01(gm.RouteMastery.CompletionCount(route) / 2f) : 0f;
        }

        private float CounterplayProgress(V5GameManager gm, V5MvpRoute route)
        {
            return gm != null && gm.RouteCounters != null ? Mathf.Clamp01(gm.RouteCounters.AnsweredCountForRoute(route)) : 0f;
        }

        private float CombatAffinityProgress(V5GameManager gm, V5EvolutionPath path, float target)
        {
            return gm != null && gm.AffinityLog != null ? Mathf.Clamp01(gm.AffinityLog.ScoreBonus(path) / Mathf.Max(1f, target)) : 0f;
        }

        private float ResourceProgress(V5GameManager gm, V5ResourceKind kind, float target)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return 0f;
            float value = 0f;
            switch (kind)
            {
                case V5ResourceKind.ATP: value = mother.Resources.atp; break;
                case V5ResourceKind.Biomass: value = mother.Resources.biomass; break;
                case V5ResourceKind.AminoAcids: value = mother.Resources.aminoAcids; break;
                case V5ResourceKind.Lipids: value = mother.Resources.lipids; break;
                case V5ResourceKind.Nucleotides: value = mother.Resources.nucleotides; break;
                case V5ResourceKind.Minerals: value = mother.Resources.minerals; break;
            }
            return Mathf.Clamp01(value / Mathf.Max(1f, target));
        }

        private float SwarmProgress(V5GameManager gm)
        {
            return gm != null ? Mathf.Clamp01(Mathf.Max(0, gm.PlayerCellCount() - 1) / 5f) : 0f;
        }

        private float BodyProgress(V5GameManager gm, int target)
        {
            return gm != null && gm.Body != null ? Mathf.Clamp01((float)gm.Body.OccupiedSlots / Mathf.Max(1, target)) : 0f;
        }

        private float GerminalProgress(V5GameManager gm)
        {
            return gm != null && gm.Germinal != null ? Mathf.Clamp01(Mathf.Max(0, gm.PlayerCellCount() - 1) / 3f) : 0f;
        }

        private float CasteProgress(V5GameManager gm, int target)
        {
            if (gm == null || gm.PlayerCells == null) return 0f;
            bool gatherer = false;
            bool attacker = false;
            bool defender = false;
            bool producer = false;
            bool sensor = false;
            bool structural = false;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                switch (c.FunctionalCaste)
                {
                    case V5FunctionalCasteId.Gatherer: gatherer = true; break;
                    case V5FunctionalCasteId.Attacker: attacker = true; break;
                    case V5FunctionalCasteId.Defender: defender = true; break;
                    case V5FunctionalCasteId.Producer: producer = true; break;
                    case V5FunctionalCasteId.Sensor: sensor = true; break;
                    case V5FunctionalCasteId.Structural: structural = true; break;
                }
            }
            int count = 0;
            if (gatherer) count++;
            if (attacker) count++;
            if (defender) count++;
            if (producer) count++;
            if (sensor) count++;
            if (structural) count++;
            return Mathf.Clamp01((float)count / Mathf.Max(1, target));
        }

        private float ColonizationProgress(V5GameManager gm, float target)
        {
            return gm != null && gm.Environment != null ? Mathf.Clamp01(gm.Environment.AverageColonization() / Mathf.Max(0.01f, target)) : 0f;
        }

        private float OxygenProgress(V5GameManager gm, float target)
        {
            return gm != null && gm.Environment != null ? Mathf.Clamp01(gm.Environment.AverageOxygen() / Mathf.Max(0.01f, target)) : 0f;
        }

        private float LowToxinProgress(V5GameManager gm)
        {
            return gm != null && gm.Environment != null ? Mathf.Clamp01(1f - gm.Environment.AverageToxins()) : 0f;
        }

        private float LowStressProgress(V5GameManager gm)
        {
            return gm != null && gm.MotherCell != null ? Mathf.Clamp01(1f - gm.MotherCell.Stats.stress / 100f) : 0f;
        }

        private float BranchObjectiveProgress(V5RouteBranchId branch, V5GameManager gm)
        {
            float synergy = BranchAbilitySynergyCount > 0 ? 1f : 0f;
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    return Mathf.Clamp01(synergy * 0.32f +
                                         CounterReady(gm, V5MvpRoute.Bacteria) * 0.30f +
                                         BuildStageReady(gm, V5MvpRoute.Bacteria, 2) * 0.17f +
                                         ColonizationProgress(gm, 0.10f) * 0.05f +
                                         LowToxinProgress(gm) * 0.16f);
                case V5RouteBranchId.BacteriaSwarm:
                    return Mathf.Clamp01(synergy * 0.38f +
                                         CounterReady(gm, V5MvpRoute.Bacteria) * 0.30f +
                                         ComboReady(gm, V5MvpRoute.Bacteria) * 0.17f +
                                         BuildStageReady(gm, V5MvpRoute.Bacteria, 2) * 0.15f);
                case V5RouteBranchId.AmoebaHunter:
                    return Mathf.Clamp01(synergy * 0.35f +
                                         CounterReady(gm, V5MvpRoute.Amoeba) * 0.25f +
                                         ComboReady(gm, V5MvpRoute.Amoeba) * 0.20f +
                                         CombatAffinityProgress(gm, V5EvolutionPath.Amoeba, 18f) * 0.10f +
                                         BuildStageReady(gm, V5MvpRoute.Amoeba, 2) * 0.10f);
                case V5RouteBranchId.AmoebaDigestive:
                    return Mathf.Clamp01(synergy * 0.35f +
                                         OpportunityProgress(gm, V5MvpRoute.Amoeba) * 0.20f +
                                         ResourceProgress(gm, V5ResourceKind.AminoAcids, 70f) * 0.20f +
                                         ComboReady(gm, V5MvpRoute.Amoeba) * 0.10f +
                                         BuildStageReady(gm, V5MvpRoute.Amoeba, 2) * 0.15f);
                case V5RouteBranchId.ProducerBloom:
                    return Mathf.Clamp01(synergy * 0.35f +
                                         ComboReady(gm, V5MvpRoute.PhotosyntheticProducer) * 0.20f +
                                         OxygenProgress(gm, 0.32f) * 0.20f +
                                         BuildStageReady(gm, V5MvpRoute.PhotosyntheticProducer, 2) * 0.15f +
                                         ResourceProgress(gm, V5ResourceKind.ATP, 80f) * 0.10f);
                case V5RouteBranchId.ProducerTerraformer:
                    return Mathf.Clamp01(synergy * 0.35f +
                                         CounterReady(gm, V5MvpRoute.PhotosyntheticProducer) * 0.25f +
                                         LowToxinProgress(gm) * 0.15f +
                                         ColonizationProgress(gm, 0.14f) * 0.10f +
                                         BuildStageReady(gm, V5MvpRoute.PhotosyntheticProducer, 2) * 0.15f);
                case V5RouteBranchId.VolvoxBody:
                    return Mathf.Clamp01(synergy * 0.35f +
                                         CounterReady(gm, V5MvpRoute.Volvox) * 0.20f +
                                         BodyProgress(gm, 3) * 0.25f +
                                         LowStressProgress(gm) * 0.10f +
                                         BuildStageReady(gm, V5MvpRoute.Volvox, 2) * 0.10f);
                case V5RouteBranchId.VolvoxCaste:
                    return Mathf.Clamp01(synergy * 0.35f +
                                         CasteProgress(gm, 3) * 0.25f +
                                         ComboReady(gm, V5MvpRoute.Volvox) * 0.15f +
                                         GerminalProgress(gm) * 0.15f +
                                         BuildStageReady(gm, V5MvpRoute.Volvox, 2) * 0.10f);
                default:
                    return 0f;
            }
        }

        private float BranchDoctrineObjectiveProgress(V5RouteBranchId branch, V5BranchDoctrineChoice choice, V5GameManager gm)
        {
            if (branch == V5RouteBranchId.None || choice == V5BranchDoctrineChoice.None || !HasBranchDoctrine(branch)) return 0f;
            if (BranchDoctrineObjectiveCompleted(branch, choice)) return 1f;

            V5MvpRoute route = RouteForBranch(branch);
            float commitment = BranchDoctrineCommitments > 0 ? 1f : 0f;
            float branchProof = BranchObjectiveCompleted(branch) ? 1f : 0f;
            float doctrineAbility = BranchDoctrineAbilityTriggerCount > 0 ? 1f : 0f;
            float passive = BranchPassivePulseCount > 0 ? 1f : 0f;
            float counter = CounterReady(gm, route);
            float combo = ComboReady(gm, route);
            float style = BranchDoctrineObjectiveStyleProgress(branch, choice, gm);

            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                return Mathf.Clamp01(commitment * 0.10f +
                                     branchProof * 0.10f +
                                     doctrineAbility * 0.34f +
                                     passive * 0.16f +
                                     Mathf.Max(counter, combo) * 0.12f +
                                     style * 0.18f);
            }

            return Mathf.Clamp01(commitment * 0.10f +
                                 branchProof * 0.10f +
                                 doctrineAbility * 0.40f +
                                 counter * 0.22f +
                                 combo * 0.10f +
                                 Mathf.Clamp01(BranchTradeoffPressure01 / 0.60f) * 0.08f +
                                 style * 0.05f);
        }

        private float BranchDoctrineObjectiveStyleProgress(V5RouteBranchId branch, V5BranchDoctrineChoice choice, V5GameManager gm)
        {
            bool stable = choice == V5BranchDoctrineChoice.Stabilize;
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    return stable ? Mathf.Max(ColonizationProgress(gm, 0.18f), LowToxinProgress(gm)) : Mathf.Max(ColonizationProgress(gm, 0.22f), ResourceProgress(gm, V5ResourceKind.Biomass, 110f));
                case V5RouteBranchId.BacteriaSwarm:
                    return stable ? Mathf.Max(SwarmProgress(gm), LowStressProgress(gm)) : Mathf.Max(SwarmProgress(gm), ComboProgress(gm, V5MvpRoute.Bacteria));
                case V5RouteBranchId.AmoebaHunter:
                    return stable ? Mathf.Max(CounterplayProgress(gm, V5MvpRoute.Amoeba), CombatAffinityProgress(gm, V5EvolutionPath.Amoeba, 24f)) : Mathf.Max(CombatAffinityProgress(gm, V5EvolutionPath.Amoeba, 30f), CounterplayProgress(gm, V5MvpRoute.Amoeba));
                case V5RouteBranchId.AmoebaDigestive:
                    return stable ? ResourceProgress(gm, V5ResourceKind.AminoAcids, 95f) : Mathf.Max(ResourceProgress(gm, V5ResourceKind.AminoAcids, 120f), OpportunityProgress(gm, V5MvpRoute.Amoeba));
                case V5RouteBranchId.ProducerBloom:
                    return stable ? Mathf.Max(OxygenProgress(gm, 0.42f), ResourceProgress(gm, V5ResourceKind.ATP, 110f)) : Mathf.Max(ResourceProgress(gm, V5ResourceKind.ATP, 140f), ComboProgress(gm, V5MvpRoute.PhotosyntheticProducer));
                case V5RouteBranchId.ProducerTerraformer:
                    return stable ? Mathf.Max(LowToxinProgress(gm), ColonizationProgress(gm, 0.18f)) : Mathf.Max(ResourceProgress(gm, V5ResourceKind.Minerals, 95f), CounterplayProgress(gm, V5MvpRoute.PhotosyntheticProducer));
                case V5RouteBranchId.VolvoxBody:
                    return stable ? Mathf.Max(BodyProgress(gm, 4), LowStressProgress(gm)) : Mathf.Max(BodyProgress(gm, 5), CounterplayProgress(gm, V5MvpRoute.Volvox));
                case V5RouteBranchId.VolvoxCaste:
                    return stable ? Mathf.Max(CasteProgress(gm, 4), GerminalProgress(gm)) : Mathf.Max(CasteProgress(gm, 5), ResourceProgress(gm, V5ResourceKind.Nucleotides, 95f));
                default:
                    return 0f;
            }
        }

        private float BuildStageReady(V5GameManager gm, V5MvpRoute route, int stage)
        {
            return gm != null && V5MvpCanon.BuildStage(route, gm.Adaptations) >= stage ? 1f : 0f;
        }

        private float ComboReady(V5GameManager gm, V5MvpRoute route)
        {
            return gm != null && gm.WorldEvents != null && gm.WorldEvents.ComboCountForRoute(route) > 0 ? 1f : 0f;
        }

        private float CounterReady(V5GameManager gm, V5MvpRoute route)
        {
            return gm != null && gm.RouteCounters != null && gm.RouteCounters.AnsweredCountForRoute(route) > 0 ? 1f : 0f;
        }

        private bool BranchObjectiveCompleted(V5RouteBranchId branch)
        {
            int bit = BranchObjectiveBit(branch);
            return bit != 0 && (completedBranchObjectiveMask & bit) != 0;
        }

        private void MarkBranchObjectiveCompleted(V5RouteBranchId branch)
        {
            int bit = BranchObjectiveBit(branch);
            if (bit != 0) completedBranchObjectiveMask |= bit;
        }

        private bool BranchDoctrineObjectiveCompleted(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            int bit = BranchDoctrineObjectiveBit(branch, choice);
            return bit != 0 && (completedBranchDoctrineObjectiveMask & bit) != 0;
        }

        private void MarkBranchDoctrineObjectiveCompleted(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            int bit = BranchDoctrineObjectiveBit(branch, choice);
            if (bit != 0) completedBranchDoctrineObjectiveMask |= bit;
        }

        private int BranchObjectiveBit(V5RouteBranchId branch)
        {
            int index = (int)branch;
            return index > 0 ? 1 << (index - 1) : 0;
        }

        private int BranchDoctrineObjectiveBit(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            int index = (int)branch;
            if (index <= 0 || choice == V5BranchDoctrineChoice.None) return 0;
            int offset = choice == V5BranchDoctrineChoice.Stabilize ? 0 : 8;
            return 1 << (offset + index - 1);
        }

        public string BranchName(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "Biofilm defensivo";
                case V5RouteBranchId.BacteriaSwarm: return "Swarm expansivo";
                case V5RouteBranchId.AmoebaHunter: return "Cazadora alfa";
                case V5RouteBranchId.AmoebaDigestive: return "Digestiva metabolica";
                case V5RouteBranchId.ProducerBloom: return "Bloom solar";
                case V5RouteBranchId.ProducerTerraformer: return "Terraformadora";
                case V5RouteBranchId.VolvoxBody: return "Cuerpo sincronico";
                case V5RouteBranchId.VolvoxCaste: return "Castas coloniales";
                default: return "sin rama";
            }
        }

        private string BranchAdvice(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "Refuerza colonizacion local, detox y defensa territorial.";
                case V5RouteBranchId.BacteriaSwarm: return "Multiplica unidades, movilidad y control de zonas.";
                case V5RouteBranchId.AmoebaHunter: return "Prioriza presas, daño fisico y counterplay agresivo.";
                case V5RouteBranchId.AmoebaDigestive: return "Convierte biomasa y aminoacidos en economia sostenida.";
                case V5RouteBranchId.ProducerBloom: return "Escala luz, ATP y oxigeno para abrir macro.";
                case V5RouteBranchId.ProducerTerraformer: return "Reduce toxinas y transforma territorio hostil.";
                case V5RouteBranchId.VolvoxBody: return "Haz del cuerpo una plataforma reparable y resistente.";
                case V5RouteBranchId.VolvoxCaste: return "Diversifica castas y produccion germinal.";
                default: return "Define estilo con tus adaptaciones y acciones.";
            }
        }

        private string BranchObjectiveGoal(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "fortifica territorio y neutraliza toxinas.";
                case V5RouteBranchId.BacteriaSwarm: return "encadena habilidad, combo y counterplay para sostener momentum.";
                case V5RouteBranchId.AmoebaHunter: return "marca presa, remata con habilidad y responde presion agresiva.";
                case V5RouteBranchId.AmoebaDigestive: return "convierte presas/detritus en aminoacidos y biomasa.";
                case V5RouteBranchId.ProducerBloom: return "abre luz, oxigeno y ATP con una ventana de bloom.";
                case V5RouteBranchId.ProducerTerraformer: return "limpia zona toxica y consolida colonizacion.";
                case V5RouteBranchId.VolvoxBody: return "mantiene cuerpo colonial reparado bajo presion.";
                case V5RouteBranchId.VolvoxCaste: return "coordina castas distintas en una respuesta colonial.";
                default: return "define un estilo y ejecuta su loop.";
            }
        }

        private string BranchObjectiveRewardText(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "matriz defensiva y detox reforzados";
                case V5RouteBranchId.BacteriaSwarm: return "pulso de ATP, biomasa y velocidad de swarm";
                case V5RouteBranchId.AmoebaHunter: return "dano fisico y aminoacidos de caza";
                case V5RouteBranchId.AmoebaDigestive: return "economia metabolica sostenida";
                case V5RouteBranchId.ProducerBloom: return "pico fotosintetico de ATP y oxigeno";
                case V5RouteBranchId.ProducerTerraformer: return "resistencia y terraformacion local";
                case V5RouteBranchId.VolvoxBody: return "reparacion corporal sincronizada";
                case V5RouteBranchId.VolvoxCaste: return "potenciacion funcional de castas";
                default: return "recompensa de rama";
            }
        }

        private string BranchDoctrineObjectiveGoal(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            bool stable = choice == V5BranchDoctrineChoice.Stabilize;
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    return stable ? "sostiene matriz limpia, pasivo activo y habilidad doctrinal." : "convierte biofilm en avance quimico con counterplay resuelto.";
                case V5RouteBranchId.BacteriaSwarm:
                    return stable ? "ordena scouts con pasivo activo y recuperacion." : "encadena velocidad, habilidad doctrinal y presion respondida.";
                case V5RouteBranchId.AmoebaHunter:
                    return stable ? "controla caza alfa sin perder estabilidad." : "remata con habilidad doctrinal y acepta la corona depredadora.";
                case V5RouteBranchId.AmoebaDigestive:
                    return stable ? "convierte combate en economia metabolica estable." : "quema recursos en crecimiento digestivo explosivo.";
                case V5RouteBranchId.ProducerBloom:
                    return stable ? "mantiene bloom con oxigeno y ATP sostenidos." : "sobrecarga luz y ATP sin colapsar la ruta.";
                case V5RouteBranchId.ProducerTerraformer:
                    return stable ? "consolida suelo vivo y baja toxinas." : "purga territorio con habilidad doctrinal y coste visible.";
                case V5RouteBranchId.VolvoxBody:
                    return stable ? "defiende cuerpo sincronico con reparacion constante." : "mueve el cuerpo como ofensiva coordinada.";
                case V5RouteBranchId.VolvoxCaste:
                    return stable ? "ordena castas y produccion germinal segura." : "acelera incubacion especializada con presion colonial.";
                default:
                    return stable ? "resuelve la doctrina por estabilidad." : "resuelve la doctrina por poder.";
            }
        }

        private string BranchDoctrineObjectiveRewardText(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            bool stable = choice == V5BranchDoctrineChoice.Stabilize;
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    return stable ? "matriz anclada y detox doctrinal" : "matriz invasiva convertida en potencia";
                case V5RouteBranchId.BacteriaSwarm:
                    return stable ? "scouts disciplinados y recuperacion colonial" : "oleada nomada con pico de expansion";
                case V5RouteBranchId.AmoebaHunter:
                    return stable ? "emboscada alfa controlada" : "corona depredadora consolidada";
                case V5RouteBranchId.AmoebaDigestive:
                    return stable ? "reciclaje lento convertido en economia" : "horno metabolico canalizado";
                case V5RouteBranchId.ProducerBloom:
                    return stable ? "canopia estable sostenida" : "radiancia solar canalizada";
                case V5RouteBranchId.ProducerTerraformer:
                    return stable ? "suelo vivo consolidado" : "purga quimica domesticada";
                case V5RouteBranchId.VolvoxBody:
                    return stable ? "escudo coral resuelto" : "marcha sincronica dominada";
                case V5RouteBranchId.VolvoxCaste:
                    return stable ? "gremios coloniales consolidados" : "incubadora de castas dominada";
                default:
                    return stable ? "doctrina estable resuelta" : "doctrina radical resuelta";
            }
        }

        private Color BranchColor(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return new Color(0.50f, 1f, 0.66f, 1f);
                case V5RouteBranchId.BacteriaSwarm: return new Color(0.82f, 1f, 0.48f, 1f);
                case V5RouteBranchId.AmoebaHunter: return new Color(1f, 0.58f, 0.80f, 1f);
                case V5RouteBranchId.AmoebaDigestive: return new Color(1f, 0.72f, 0.42f, 1f);
                case V5RouteBranchId.ProducerBloom: return new Color(0.90f, 1f, 0.38f, 1f);
                case V5RouteBranchId.ProducerTerraformer: return new Color(0.48f, 1f, 0.78f, 1f);
                case V5RouteBranchId.VolvoxBody: return new Color(0.58f, 0.92f, 1f, 1f);
                case V5RouteBranchId.VolvoxCaste: return new Color(0.80f, 0.78f, 1f, 1f);
                default: return new Color(0.86f, 1f, 0.92f, 1f);
            }
        }

        private string BranchAbilityText(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "la habilidad convierte el area en territorio protegido";
                case V5RouteBranchId.BacteriaSwarm: return "la habilidad acelera la colonia y empuja expansion";
                case V5RouteBranchId.AmoebaHunter: return "la habilidad remata presas cercanas y sube dano fisico";
                case V5RouteBranchId.AmoebaDigestive: return "la habilidad transforma detritus en economia metabolica";
                case V5RouteBranchId.ProducerBloom: return "la habilidad abre un pico de luz, ATP y oxigeno";
                case V5RouteBranchId.ProducerTerraformer: return "la habilidad limpia toxinas y estabiliza territorio";
                case V5RouteBranchId.VolvoxBody: return "la habilidad repara la red corporal sincronizada";
                case V5RouteBranchId.VolvoxCaste: return "la habilidad potencia castas segun su funcion";
                default: return "la habilidad responde al estilo dominante";
            }
        }

        private string BranchDoctrineAbilityText(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                switch (branch)
                {
                    case V5RouteBranchId.BacteriaBiofilm: return "la marea ancla colonizacion y detox en zona segura";
                    case V5RouteBranchId.BacteriaSwarm: return "la marea ordena exploracion sin perder recuperacion";
                    case V5RouteBranchId.AmoebaHunter: return "la fagocitosis marca presas y controla rango";
                    case V5RouteBranchId.AmoebaDigestive: return "la digestion convierte combate en economia estable";
                    case V5RouteBranchId.ProducerBloom: return "el bloom sostiene ATP sin exponer tanto a toxinas";
                    case V5RouteBranchId.ProducerTerraformer: return "el pulso limpia y consolida territorio";
                    case V5RouteBranchId.VolvoxBody: return "la sincronía repara y defiende el cuerpo";
                    case V5RouteBranchId.VolvoxCaste: return "la sincronía protege castas y estabiliza produccion";
                }
            }

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "la marea convierte matriz en avance quimico";
                case V5RouteBranchId.BacteriaSwarm: return "la marea dispara velocidad y expansion";
                case V5RouteBranchId.AmoebaHunter: return "la fagocitosis remata con dano alfa";
                case V5RouteBranchId.AmoebaDigestive: return "la digestion quema recursos en crecimiento explosivo";
                case V5RouteBranchId.ProducerBloom: return "el bloom sobrecarga ATP y luz";
                case V5RouteBranchId.ProducerTerraformer: return "el pulso purga y mineraliza territorio";
                case V5RouteBranchId.VolvoxBody: return "la sincronía convierte el cuerpo en empuje movil";
                case V5RouteBranchId.VolvoxCaste: return "la sincronía acelera incubacion de castas";
                default: return "la habilidad radicaliza la doctrina activa";
            }
        }

        private string BranchPassiveText(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "la matriz estabiliza toxinas y empuja colonizacion";
                case V5RouteBranchId.BacteriaSwarm: return "el swarm recarga ATP y prioriza exploracion";
                case V5RouteBranchId.AmoebaHunter: return "la colonia presiona amenazas cercanas";
                case V5RouteBranchId.AmoebaDigestive: return "la economia digestiva convierte zona en comida";
                case V5RouteBranchId.ProducerBloom: return "la luz local alimenta ATP y oxigeno";
                case V5RouteBranchId.ProducerTerraformer: return "el territorio baja toxicidad y gana minerales";
                case V5RouteBranchId.VolvoxBody: return "el cuerpo colonial se repara y defiende";
                case V5RouteBranchId.VolvoxCaste: return "las castas reciben micro-potenciacion funcional";
                default: return "la rama mantiene su identidad";
            }
        }

        private string BranchVisualText(V5RouteBranchId branch)
        {
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "matriz verde defensiva alrededor de la madre";
                case V5RouteBranchId.BacteriaSwarm: return "anillo movil de expansion y velocidad";
                case V5RouteBranchId.AmoebaHunter: return "pulso rosado de caza agresiva";
                case V5RouteBranchId.AmoebaDigestive: return "aura ambar de digestion metabolica";
                case V5RouteBranchId.ProducerBloom: return "corona solar de luz y oxigeno";
                case V5RouteBranchId.ProducerTerraformer: return "manto turquesa de terraformacion local";
                case V5RouteBranchId.VolvoxBody: return "campo celeste de cuerpo sincronico";
                case V5RouteBranchId.VolvoxCaste: return "trama violeta de castas coloniales";
                default: return "identidad visual latente";
            }
        }

        private string BranchDoctrineOfferText(V5RouteBranchId branch)
        {
            return "Doctrina disponible " + BranchName(branch) + ": " +
                   BranchDoctrineName(branch, V5BranchDoctrineChoice.Stabilize) + " o " +
                   BranchDoctrineName(branch, V5BranchDoctrineChoice.Radicalize) + ".";
        }

        private string BranchDoctrineName(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            bool stable = choice == V5BranchDoctrineChoice.Stabilize;
            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return stable ? "Fortaleza de Matriz" : "Matriz Invasiva";
                case V5RouteBranchId.BacteriaSwarm: return stable ? "Red de Scouts" : "Oleada Nomada";
                case V5RouteBranchId.AmoebaHunter: return stable ? "Emboscada Alfa" : "Corona Depredadora";
                case V5RouteBranchId.AmoebaDigestive: return stable ? "Reciclaje Lento" : "Horno Metabolico";
                case V5RouteBranchId.ProducerBloom: return stable ? "Canopia Estable" : "Radiancia Solar";
                case V5RouteBranchId.ProducerTerraformer: return stable ? "Suelo Vivo" : "Purga Quimica";
                case V5RouteBranchId.VolvoxBody: return stable ? "Escudo Coral" : "Marcha Sincronica";
                case V5RouteBranchId.VolvoxCaste: return stable ? "Gremios Coloniales" : "Incubadora de Castas";
                default: return stable ? "Doctrina estable" : "Doctrina radical";
            }
        }

        private string BranchDoctrineEffectText(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                switch (branch)
                {
                    case V5RouteBranchId.BacteriaBiofilm: return "refuerza detox, colonizacion segura y reparacion";
                    case V5RouteBranchId.BacteriaSwarm: return "mantiene exploracion con menos desgaste";
                    case V5RouteBranchId.AmoebaHunter: return "caza con mas control y rango";
                    case V5RouteBranchId.AmoebaDigestive: return "convierte recursos sin romper movilidad";
                    case V5RouteBranchId.ProducerBloom: return "sostiene fotosintesis con bajo riesgo";
                    case V5RouteBranchId.ProducerTerraformer: return "limpia territorio con resistencia estable";
                    case V5RouteBranchId.VolvoxBody: return "vuelve el cuerpo mas durable y reparable";
                    case V5RouteBranchId.VolvoxCaste: return "ordena castas con economia segura";
                }
            }

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "convierte la matriz en avance agresivo";
                case V5RouteBranchId.BacteriaSwarm: return "sacrifica calma por expansion veloz";
                case V5RouteBranchId.AmoebaHunter: return "apuesta por dano alfa y remates";
                case V5RouteBranchId.AmoebaDigestive: return "quema presas en economia explosiva";
                case V5RouteBranchId.ProducerBloom: return "sube ATP y luz a costa de fragilidad";
                case V5RouteBranchId.ProducerTerraformer: return "purga el mapa con coste metabolico";
                case V5RouteBranchId.VolvoxBody: return "mueve el cuerpo como una ofensiva coordinada";
                case V5RouteBranchId.VolvoxCaste: return "acelera produccion colonial especializada";
                default: return "radicaliza el estilo de rama";
            }
        }

        private string BranchDoctrineTradeoffText(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                switch (branch)
                {
                    case V5RouteBranchId.BacteriaSwarm: return "menos pico de velocidad que la opcion radical";
                    case V5RouteBranchId.AmoebaHunter: return "menos dano explosivo";
                    case V5RouteBranchId.ProducerBloom: return "crecimiento de ATP mas lento";
                    case V5RouteBranchId.VolvoxCaste: return "produccion de castas menos explosiva";
                    default: return "consume ATP inicial y prioriza seguridad sobre pico de poder";
                }
            }

            switch (branch)
            {
                case V5RouteBranchId.BacteriaBiofilm: return "sube estres y deja huella quimica";
                case V5RouteBranchId.BacteriaSwarm: return "sube estres y reduce recuperacion";
                case V5RouteBranchId.AmoebaHunter: return "sube estres y baja resistencia";
                case V5RouteBranchId.AmoebaDigestive: return "sube estres y reduce movilidad";
                case V5RouteBranchId.ProducerBloom: return "sube estres y baja tolerancia a toxinas";
                case V5RouteBranchId.ProducerTerraformer: return "sube estres por purga territorial";
                case V5RouteBranchId.VolvoxBody: return "sube estres para mover el cuerpo como arma";
                case V5RouteBranchId.VolvoxCaste: return "sube estres por sobreproduccion colonial";
                default: return "sube estres a cambio de poder";
            }
        }

        private float BranchDoctrinePressure(V5RouteBranchId branch, V5BranchDoctrineChoice choice)
        {
            if (choice == V5BranchDoctrineChoice.Stabilize)
            {
                switch (branch)
                {
                    case V5RouteBranchId.BacteriaSwarm:
                    case V5RouteBranchId.AmoebaHunter:
                    case V5RouteBranchId.ProducerBloom:
                    case V5RouteBranchId.VolvoxCaste:
                        return 0.30f;
                    default:
                        return 0.24f;
                }
            }

            switch (branch)
            {
                case V5RouteBranchId.AmoebaHunter:
                case V5RouteBranchId.ProducerBloom:
                case V5RouteBranchId.BacteriaSwarm:
                    return 0.68f;
                case V5RouteBranchId.ProducerTerraformer:
                case V5RouteBranchId.VolvoxCaste:
                    return 0.62f;
                default:
                    return 0.56f;
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

        private struct ScorePair
        {
            public V5RouteBranchId best;
            public V5RouteBranchId runnerUp;
            public float bestScore;
            public float runnerUpScore;
        }
    }
}
