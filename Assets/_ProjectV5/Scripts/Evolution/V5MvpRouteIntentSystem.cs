using UnityEngine;

namespace Protogenesis.V5
{
    public class V5MvpRouteIntentSystem : MonoBehaviour, IV5RunResettable
    {
        private const float GoalCompleteThreshold = 0.995f;

        public V5MvpRoute Intent = V5MvpRoute.None;
        public string LastMessage = "Ruta MVP en autodeteccion.";
        public int PrepareCount;
        public V5MvpRoute LastPreparedRoute = V5MvpRoute.None;
        public int CompletedGoalCount;
        public V5MvpRoute LastCompletedRoute = V5MvpRoute.None;
        public string LastGoalMilestone = "Sin hito MVP completado.";
        public string LastIdentityMoment = "Sin momento de identidad MVP.";
        public int OpeningNudgeCount;
        public string LastOpeningNudge = "Sin apertura MVP.";
        public int WorldCueCount;
        public string LastWorldCue = "Sin cue de mundo MVP.";
        public int MicroObjectiveCompletedCount;
        public string LastMicroObjective = "Sin micro-mision MVP.";
        public string LastMicroMilestone = "Sin micro-hito MVP.";

        private bool bacteriaGoalCompleted;
        private bool amoebaGoalCompleted;
        private bool producerGoalCompleted;
        private bool volvoxGoalCompleted;
        private bool bacteriaMicroCompleted;
        private bool amoebaMicroCompleted;
        private bool producerMicroCompleted;
        private bool volvoxMicroCompleted;
        private bool bacteriaWorldCuePlayed;
        private bool amoebaWorldCuePlayed;
        private bool producerWorldCuePlayed;
        private bool volvoxWorldCuePlayed;
        private Vector2 bacteriaWorldCuePosition;
        private Vector2 amoebaWorldCuePosition;
        private Vector2 producerWorldCuePosition;
        private Vector2 volvoxWorldCuePosition;
        private V5MvpRoute activeMicroRoute = V5MvpRoute.None;
        private Vector2 activeMicroPosition;
        private V5CellEntity activeMicroTarget;
        private float activeMicroTargetInitialHp;
        private float goalTimer;

        public bool HasIntent { get { return Intent != V5MvpRoute.None; } }

        public void ResetForNewRun()
        {
            Intent = V5MvpRoute.None;
            LastMessage = "Ruta MVP en autodeteccion.";
            PrepareCount = 0;
            LastPreparedRoute = V5MvpRoute.None;
            CompletedGoalCount = 0;
            LastCompletedRoute = V5MvpRoute.None;
            LastGoalMilestone = "Sin hito MVP completado.";
            LastIdentityMoment = "Sin momento de identidad MVP.";
            OpeningNudgeCount = 0;
            LastOpeningNudge = "Sin apertura MVP.";
            WorldCueCount = 0;
            LastWorldCue = "Sin cue de mundo MVP.";
            MicroObjectiveCompletedCount = 0;
            LastMicroObjective = "Sin micro-mision MVP.";
            LastMicroMilestone = "Sin micro-hito MVP.";
            bacteriaGoalCompleted = false;
            amoebaGoalCompleted = false;
            producerGoalCompleted = false;
            volvoxGoalCompleted = false;
            bacteriaMicroCompleted = false;
            amoebaMicroCompleted = false;
            producerMicroCompleted = false;
            volvoxMicroCompleted = false;
            bacteriaWorldCuePlayed = false;
            amoebaWorldCuePlayed = false;
            producerWorldCuePlayed = false;
            volvoxWorldCuePlayed = false;
            bacteriaWorldCuePosition = Vector2.zero;
            amoebaWorldCuePosition = Vector2.zero;
            producerWorldCuePosition = Vector2.zero;
            volvoxWorldCuePosition = Vector2.zero;
            activeMicroRoute = V5MvpRoute.None;
            activeMicroPosition = Vector2.zero;
            activeMicroTarget = null;
            activeMicroTargetInitialHp = 0f;
            goalTimer = 0f;
        }

        private void Update()
        {
            goalTimer += Time.deltaTime;
            if (goalTimer < 0.5f) return;
            goalTimer = 0f;
            EvaluateMicroObjectiveNow(V5GameManager.Instance);
            EvaluateRouteGoalNow(V5GameManager.Instance);
        }

        public V5MvpRoute EffectiveRoute(V5GameManager gm)
        {
            if (Intent != V5MvpRoute.None) return Intent;
            return V5MvpCanon.CurrentRoute(gm);
        }

        public void SetIntent(V5MvpRoute route)
        {
            Intent = route;
            LastMessage = route == V5MvpRoute.None
                ? "Ruta MVP en autodeteccion."
                : "Intencion MVP: " + V5MvpCanon.DisplayName(route) + ".";
            if (route != V5MvpRoute.None)
            {
                ApplyOpeningNudge(route, V5GameManager.Instance);
                ApplyRouteWorldCue(route, V5GameManager.Instance);
                ActivateMicroObjective(route, V5GameManager.Instance);
            }
            Toast();
        }

        public V5AdaptationId SuggestedCoreAdaptation(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None || gm == null || gm.Adaptations == null) return V5AdaptationId.None;

            V5AdaptationId firstBlocked = V5AdaptationId.None;
            foreach (V5AdaptationId id in V5MvpCanon.CoreAdaptationArray(route))
            {
                if (gm.Adaptations.Has(id)) continue;
                if (firstBlocked == V5AdaptationId.None) firstBlocked = id;

                string reason;
                if (gm.MotherCell != null && gm.Adaptations.CanInstall(id, gm.MotherCell, out reason)) return id;
            }

            V5AdaptationId primer = SuggestedNucleusPrimer(route, gm);
            return primer != V5AdaptationId.None ? primer : firstBlocked;
        }

        public bool PrepareRoutePlaytest(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None)
            {
                LastMessage = "Elige una ruta MVP antes de preparar test.";
                Toast();
                return false;
            }

            if (gm == null || gm.MotherCell == null)
            {
                LastMessage = "No hay celula madre para preparar test.";
                Toast();
                return false;
            }

            TopUpResources(gm.MotherCell, ResourceFloor(route));
            gm.MotherCell.Stats.currentHp = Mathf.Max(gm.MotherCell.Stats.currentHp, gm.MotherCell.Stats.maxHp * 0.85f);
            gm.MotherCell.Stats.stress = Mathf.Min(gm.MotherCell.Stats.stress, 22f);

            PrepareCount++;
            LastPreparedRoute = route;

            V5AdaptationId next = SuggestedCoreAdaptation(gm);
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(next);
            string nextText = def != null ? def.shortName : "ruta completa";
            LastMessage = "Playtest " + V5MvpCanon.DisplayName(route) + ": presupuesto listo; siguiente " + nextText + ".";
            Toast();
            return true;
        }

        public string RouteObjectiveText(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None) return "Elige Bacteria, Ameba, Productor o Volvox en Genoma.";

            V5AdaptationId next = SuggestedCoreAdaptation(gm);
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(next);
            string nextText = def != null ? def.shortName : "ruta completa";
            string build = V5MvpCanon.BuildProgressText(route, gm != null ? gm.Adaptations : null);
            return RouteGoalText(gm) + " | " + build + " | " + V5MvpCanon.Objective(route) + " Siguiente: " + nextText + ".";
        }

        public string OpeningStepText(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None) return "Abre Genoma (G), elige una de las 4 rutas MVP y luego instala la adaptacion sugerida.";
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(SuggestedCoreAdaptation(gm));
            string nextText = def != null ? def.shortName : "ruta completa";
            return "Ruta " + V5MvpCanon.DisplayName(route) + ": instala " + nextText + ". " + RouteMicroObjectiveText(gm) + " " + V5MvpCanon.Fantasy(route);
        }

        public string RouteGoalText(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None) return "Meta MVP: sin ruta.";
            string done = IsRouteGoalCompleted(route) ? " completada" : "";
            return V5MvpCanon.DisplayName(route) + " meta " + Percent(RouteGoalProgress01(gm)) + done + ": " + RouteGoalMetricText(route, gm);
        }

        public string RouteMicroObjectiveText(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None || !HasIntent) return "Micro MVP: elige una ruta manual para activar una escena jugable.";
            string done = IsMicroObjectiveCompleted(route) ? " completada" : "";
            return "Micro " + V5MvpCanon.DisplayName(route) + " " + Percent(RouteMicroObjectiveProgress01(gm)) + done + ": " + RouteMicroMetricText(route, gm);
        }

        public float RouteMicroObjectiveProgress01(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None || gm == null || !HasIntent) return 0f;
            if (IsMicroObjectiveCompleted(route)) return 1f;

            Vector2 cue = CuePositionFor(route, gm);
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return Mathf.Clamp01(HasAny(gm, V5AdaptationId.BacterialWall, V5AdaptationId.PiliFimbriae) * 0.42f +
                                         LocalChannel01(gm, cue, 4.2f, V5OverlayMode.Colonization) / 0.060f * 0.58f);
                case V5MvpRoute.Amoeba:
                    return Mathf.Clamp01(AmoebaPreyProgress());
                case V5MvpRoute.PhotosyntheticProducer:
                    return Mathf.Clamp01(HasAny(gm, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast) * 0.42f +
                                         LocalChannel01(gm, cue, 4.8f, V5OverlayMode.Oxygen) / 0.330f * 0.58f);
                case V5MvpRoute.Volvox:
                    return Mathf.Clamp01(HasAny(gm, V5AdaptationId.BasicAdhesin, V5AdaptationId.ColonialAdhesin) * 0.42f +
                                         BodySlotProgress(gm, 1) * 0.36f +
                                         LocalChannel01(gm, cue, 4.2f, V5OverlayMode.Colonization) / 0.045f * 0.22f);
                default:
                    return 0f;
            }
        }

        public bool EvaluateMicroObjectiveNow(V5GameManager gm)
        {
            if (!HasIntent || gm == null) return false;
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None || IsMicroObjectiveCompleted(route)) return false;
            if (activeMicroRoute != route) ActivateMicroObjective(route, gm);
            if (RouteMicroObjectiveProgress01(gm) < 0.995f) return false;
            return CompleteMicroObjective(route, gm, "auto");
        }

        public bool CompleteMicroObjective(V5MvpRoute route, V5GameManager gm, string source)
        {
            if (route == V5MvpRoute.None || IsMicroObjectiveCompleted(route)) return false;

            SetMicroObjectiveCompleted(route);
            MicroObjectiveCompletedCount++;
            LastMicroMilestone = "Micro-hito " + V5MvpCanon.DisplayName(route) + ": " + RouteMicroMetricText(route, gm);
            LastMicroObjective = LastMicroMilestone;
            LastMessage = LastMicroMilestone;
            ApplyMicroReward(route, gm);

            if (gm != null && gm.Codex != null)
            {
                string origin = string.IsNullOrEmpty(source) ? "run" : source;
                gm.Codex.Unlock("Micro MVP: " + V5MvpCanon.DisplayName(route),
                    LastMicroMilestone + " Completado por " + origin + ".");
            }
            Toast();
            return true;
        }

        public bool IsMicroObjectiveCompleted(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return bacteriaMicroCompleted;
                case V5MvpRoute.Amoeba: return amoebaMicroCompleted;
                case V5MvpRoute.PhotosyntheticProducer: return producerMicroCompleted;
                case V5MvpRoute.Volvox: return volvoxMicroCompleted;
                default: return false;
            }
        }

        public float RouteGoalProgress01(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None || gm == null) return 0f;

            float core = V5MvpCanon.Progress01(route, gm.Adaptations);
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return Mathf.Clamp01(core * 0.45f + SwarmProgress(gm) * 0.35f + ColonizationProgress(gm, 0.12f) * 0.20f);
                case V5MvpRoute.Amoeba:
                    return Mathf.Clamp01(core * 0.50f + AmoebaPredatorKitProgress(gm) * 0.35f + AmoebaCombatProgress(gm) * 0.15f);
                case V5MvpRoute.PhotosyntheticProducer:
                    return Mathf.Clamp01(core * 0.45f + ProducerKitProgress(gm) * 0.25f + OxygenProgress(gm, 0.32f) * 0.20f + ColonizationProgress(gm, 0.12f) * 0.10f);
                case V5MvpRoute.Volvox:
                    return Mathf.Clamp01(core * 0.45f + BodySlotProgress(gm, 4) * 0.30f + CasteDiversityProgress(gm, 3) * 0.25f);
                default:
                    return 0f;
            }
        }

        public string RouteGoalMetricText(V5MvpRoute route, V5GameManager gm)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return "swarm " + PlayerCellCount(gm) + "/6 cel, colonia " + Percent(Colonization01(gm)) + "/12%.";
                case V5MvpRoute.Amoeba:
                    return "kit depredador " + Percent(AmoebaPredatorKitProgress(gm)) + ", afinidad combate " + Percent(AmoebaCombatProgress(gm)) + ".";
                case V5MvpRoute.PhotosyntheticProducer:
                    return "O2 " + Percent(Oxygen01(gm)) + "/32%, colonia " + Percent(Colonization01(gm)) + "/12%.";
                case V5MvpRoute.Volvox:
                    return "cuerpo " + BodySlots(gm) + "/4 slots, castas " + DistinctCasteCount(gm) + "/3.";
                default:
                    return "elige una ruta MVP.";
            }
        }

        private string RouteMicroMetricText(V5MvpRoute route, V5GameManager gm)
        {
            Vector2 cue = CuePositionFor(route, gm);
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return "Pared/Pili " + CheckText(HasAny(gm, V5AdaptationId.BacterialWall, V5AdaptationId.PiliFimbriae)) +
                           ", biofilm local " + Percent(LocalChannel01(gm, cue, 4.2f, V5OverlayMode.Colonization)) + "/6%.";
                case V5MvpRoute.Amoeba:
                    if (activeMicroTarget == null || activeMicroTarget.Stats.currentHp <= 0f) return "presa marcada absorbida.";
                    return "presa marcada HP " + activeMicroTarget.Stats.currentHp.ToString("0") + "/" + Mathf.Max(1f, activeMicroTargetInitialHp).ToString("0") + ".";
                case V5MvpRoute.PhotosyntheticProducer:
                    return "Tilacoide " + CheckText(HasAny(gm, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast)) +
                           ", O2 local " + Percent(LocalChannel01(gm, cue, 4.8f, V5OverlayMode.Oxygen)) + "/33%.";
                case V5MvpRoute.Volvox:
                    return "Adesina " + CheckText(HasAny(gm, V5AdaptationId.BasicAdhesin, V5AdaptationId.ColonialAdhesin)) +
                           ", cuerpo " + BodySlots(gm) + "/1, zona " + Percent(LocalChannel01(gm, cue, 4.2f, V5OverlayMode.Colonization)) + "/4.5%.";
                default:
                    return "elige una ruta MVP.";
            }
        }

        public bool EvaluateRouteGoalNow(V5GameManager gm)
        {
            V5MvpRoute route = EffectiveRoute(gm);
            if (route == V5MvpRoute.None || IsRouteGoalCompleted(route)) return false;
            if (RouteGoalProgress01(gm) < GoalCompleteThreshold) return false;
            return CompleteRouteGoal(route, gm, "auto");
        }

        public bool CompleteRouteGoal(V5MvpRoute route, V5GameManager gm, string source)
        {
            if (route == V5MvpRoute.None || IsRouteGoalCompleted(route)) return false;

            SetRouteGoalCompleted(route);
            CompletedGoalCount++;
            LastCompletedRoute = route;
            LastGoalMilestone = "Hito MVP " + V5MvpCanon.DisplayName(route) + ": " + RouteGoalMetricText(route, gm);
            LastMessage = LastGoalMilestone;

            ApplyMilestonePulse(route, gm);
            LastGoalMilestone += " | " + LastIdentityMoment;
            LastMessage = LastGoalMilestone;
            if (gm != null && gm.Codex != null)
            {
                string origin = string.IsNullOrEmpty(source) ? "run" : source;
                gm.Codex.Unlock("Hito MVP: " + V5MvpCanon.DisplayName(route),
                    V5MvpCanon.Objective(route) + " Completado por " + origin + ". " + RouteGoalMetricText(route, gm) + " " + LastIdentityMoment);
            }
            Toast();
            return true;
        }

        public bool IsRouteGoalCompleted(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return bacteriaGoalCompleted;
                case V5MvpRoute.Amoeba: return amoebaGoalCompleted;
                case V5MvpRoute.PhotosyntheticProducer: return producerGoalCompleted;
                case V5MvpRoute.Volvox: return volvoxGoalCompleted;
                default: return false;
            }
        }

        private V5AdaptationId SuggestedNucleusPrimer(V5MvpRoute route, V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return V5AdaptationId.None;
            if (gm.Adaptations.CountInTier(V5AdaptationTier.T1Prokaryote) >= 2) return V5AdaptationId.None;

            V5AdaptationId[] primers = V5MvpCanon.PrimerAdaptationArray(route);
            V5AdaptationId firstBlocked = V5AdaptationId.None;
            for (int i = 0; i < primers.Length; i++)
            {
                V5AdaptationId id = primers[i];
                if (gm.Adaptations.Has(id)) continue;
                if (firstBlocked == V5AdaptationId.None) firstBlocked = id;

                string reason;
                if (gm.MotherCell != null && gm.Adaptations.CanInstall(id, gm.MotherCell, out reason)) return id;
            }
            return firstBlocked;
        }

        private V5ResourceWallet ResourceFloor(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return V5ResourceWallet.Cost(120f, 85f, 42f, 30f, 22f, 18f);
                case V5MvpRoute.Amoeba:
                    return V5ResourceWallet.Cost(190f, 125f, 62f, 45f, 42f, 22f);
                case V5MvpRoute.PhotosyntheticProducer:
                    return V5ResourceWallet.Cost(205f, 140f, 56f, 52f, 46f, 36f);
                case V5MvpRoute.Volvox:
                    return V5ResourceWallet.Cost(265f, 180f, 84f, 72f, 72f, 38f);
                default:
                    return V5ResourceWallet.Cost(120f, 85f, 42f, 30f, 22f, 18f);
            }
        }

        private void TopUpResources(V5CellEntity cell, V5ResourceWallet floor)
        {
            cell.Resources.atp = Mathf.Max(cell.Resources.atp, floor.atp);
            cell.Resources.biomass = Mathf.Max(cell.Resources.biomass, floor.biomass);
            cell.Resources.aminoAcids = Mathf.Max(cell.Resources.aminoAcids, floor.aminoAcids);
            cell.Resources.lipids = Mathf.Max(cell.Resources.lipids, floor.lipids);
            cell.Resources.nucleotides = Mathf.Max(cell.Resources.nucleotides, floor.nucleotides);
            cell.Resources.minerals = Mathf.Max(cell.Resources.minerals, floor.minerals);
        }

        private void ApplyOpeningNudge(V5MvpRoute route, V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null) return;
            if (OpeningNudgeCount > 0) return;
            if (gm.ElapsedSeconds > 180f) return;
            if (gm.Adaptations != null && gm.Adaptations.ActiveCount() > 0) return;

            V5CellEntity mother = gm.MotherCell;
            OpeningNudgeCount++;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    TopUpResources(mother, V5ResourceWallet.Cost(92f, 62f, 28f, 24f, 15f, 12f));
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 3.6f, 0.010f, 0f, 0.006f, -0.006f, 0f, 0.018f, 0f);
                    LastOpeningNudge = "Apertura Bacteria: pared o flagelo ya son viables.";
                    break;
                case V5MvpRoute.Amoeba:
                    TopUpResources(mother, V5ResourceWallet.Cost(96f, 64f, 30f, 24f, 16f, 12f));
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 3.6f, 0.016f, 0f, 0.004f, 0f, 0f, 0.006f, 0.018f);
                    LastOpeningNudge = "Apertura Ameba: primero dos rasgos tempranos, luego Nucleo.";
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    TopUpResources(mother, V5ResourceWallet.Cost(100f, 66f, 30f, 26f, 16f, 13f));
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 4.2f, 0.010f, 0.018f, 0.018f, -0.008f, 0f, 0.010f, 0f);
                    LastOpeningNudge = "Apertura Productor: Tilacoide es el primer gesto de economia luminica.";
                    break;
                case V5MvpRoute.Volvox:
                    TopUpResources(mother, V5ResourceWallet.Cost(96f, 64f, 30f, 24f, 16f, 12f));
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 4f);
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 3.8f, 0.010f, 0.006f, 0.012f, -0.006f, 0f, 0.014f, 0f);
                    LastOpeningNudge = "Apertura Volvox: Adesina y luz preparan el cuerpo colonial.";
                    break;
            }

            LastMessage = LastOpeningNudge;
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push("Ruta MVP: " + V5MvpCanon.DisplayName(route), mother.transform.position, new Color(0.86f, 1f, 0.92f, 1f));
        }

        private void ActivateMicroObjective(V5MvpRoute route, V5GameManager gm)
        {
            if (route == V5MvpRoute.None || gm == null || gm.MotherCell == null) return;

            activeMicroRoute = route;
            activeMicroPosition = CuePositionFor(route, gm);
            if (route != V5MvpRoute.Amoeba)
            {
                activeMicroTarget = null;
                activeMicroTargetInitialHp = 0f;
            }
            else if (activeMicroTarget == null && !IsMicroObjectiveCompleted(route))
            {
                V5CellEntity prey = SpawnCueNeutral(gm, activeMicroPosition, V5EvolutionPath.Bacteria);
                SetupAmoebaPrey(prey);
            }

            LastMicroObjective = IsMicroObjectiveCompleted(route)
                ? "Micro " + V5MvpCanon.DisplayName(route) + " completada."
                : RouteMicroObjectiveText(gm);
            LastMessage = LastMicroObjective;
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push("Micro MVP", activeMicroPosition, RouteMicroColor(route));
        }

        private void ApplyRouteWorldCue(V5MvpRoute route, V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null) return;
            if (RouteWorldCuePlayed(route)) return;

            V5CellEntity mother = gm.MotherCell;
            Vector2 origin = mother.transform.position;
            Vector2 cue = RouteCuePosition(gm, origin, route, 3.6f);
            Color color = new Color(0.86f, 1f, 0.92f, 1f);
            string title = "Ruta MVP";

            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 5.0f, 0.018f, 0f, 0.008f, -0.018f, 0f, 0.075f, 0.004f);
                    SpawnCueResource(gm, cue + Vector2.right * 0.8f, V5ResourceKind.Biomass, 28f);
                    LastWorldCue = "Mundo Bacteria: parche de biofilm y biomasa cercana para ocupar territorio.";
                    title = "Biofilm inicial";
                    color = new Color(0.58f, 1f, 0.72f, 1f);
                    break;
                case V5MvpRoute.Amoeba:
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 4.2f, 0.018f, 0f, 0.004f, -0.006f, 0f, 0.006f, 0.060f);
                    V5CellEntity prey = SpawnCueNeutral(gm, cue, V5EvolutionPath.Bacteria);
                    SetupAmoebaPrey(prey);
                    LastWorldCue = "Mundo Ameba: presa debilitada y rastro de detritus para practicar caza.";
                    title = "Presa marcada";
                    color = new Color(1f, 0.70f, 0.88f, 1f);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    cue = RouteCuePosition(gm, origin, route, 4.2f);
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 6.6f, 0.012f, 0.120f, 0.090f, -0.025f, -0.010f, 0.018f, 0f);
                    SpawnCueResource(gm, cue + Vector2.up * 0.9f, V5ResourceKind.Minerals, 20f);
                    LastWorldCue = "Mundo Productor: claro luminoso con oxigeno naciente para empezar economia solar.";
                    title = "Claro luminoso";
                    color = new Color(0.78f, 1f, 0.48f, 1f);
                    break;
                case V5MvpRoute.Volvox:
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 5.4f, 0.014f, 0.018f, 0.032f, -0.016f, 0f, 0.045f, 0.006f);
                    SpawnCueResource(gm, cue + Vector2.down * 0.8f, V5ResourceKind.Lipids, 24f);
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 2f);
                    LastWorldCue = "Mundo Volvox: zona estable con lipidos para preparar adhesion y cuerpo colonial.";
                    title = "Zona colonial";
                    color = new Color(0.62f, 0.92f, 1f, 1f);
                    break;
            }

            StoreRouteCuePosition(route, cue);
            SetRouteWorldCuePlayed(route);
            WorldCueCount++;
            LastMessage = LastWorldCue;
            PushMomentFeedback(mother, title, color);
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push(V5MvpCanon.DisplayName(route), cue, color);
        }

        private bool RouteWorldCuePlayed(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return bacteriaWorldCuePlayed;
                case V5MvpRoute.Amoeba: return amoebaWorldCuePlayed;
                case V5MvpRoute.PhotosyntheticProducer: return producerWorldCuePlayed;
                case V5MvpRoute.Volvox: return volvoxWorldCuePlayed;
                default: return true;
            }
        }

        private void SetRouteWorldCuePlayed(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: bacteriaWorldCuePlayed = true; break;
                case V5MvpRoute.Amoeba: amoebaWorldCuePlayed = true; break;
                case V5MvpRoute.PhotosyntheticProducer: producerWorldCuePlayed = true; break;
                case V5MvpRoute.Volvox: volvoxWorldCuePlayed = true; break;
            }
        }

        private Vector2 RouteCuePosition(V5GameManager gm, Vector2 origin, V5MvpRoute route, float distance)
        {
            Vector2 dir;
            switch (route)
            {
                case V5MvpRoute.Bacteria: dir = new Vector2(-0.85f, 0.30f); break;
                case V5MvpRoute.Amoeba: dir = new Vector2(0.85f, 0.18f); break;
                case V5MvpRoute.PhotosyntheticProducer: dir = new Vector2(0.15f, 1f); break;
                case V5MvpRoute.Volvox: dir = new Vector2(-0.20f, -1f); break;
                default: dir = Vector2.right; break;
            }

            dir.Normalize();
            Vector2 pos = origin + dir * distance;
            if (gm != null && gm.Environment != null)
            {
                float max = gm.Environment.MapRadius * 0.82f;
                if (pos.magnitude > max) pos = origin - dir * distance;
                if (pos.magnitude > max) pos = pos.normalized * max;
            }
            return pos;
        }

        private V5CellEntity SpawnCueNeutral(V5GameManager gm, Vector2 position, V5EvolutionPath path)
        {
            if (gm == null || gm.CellFactory == null) return null;
            V5EnvironmentGrid env = gm.Environment;
            if (env != null && position.magnitude > env.MapRadius * 0.92f)
                position = position.normalized * env.MapRadius * 0.82f;
            return gm.CellFactory.SpawnNeutral(position, path);
        }

        private void SpawnCueResource(V5GameManager gm, Vector2 position, V5ResourceKind kind, float amount)
        {
            if (gm == null || gm.Resources == null) return;
            V5EnvironmentGrid env = gm.Environment;
            if (env != null && position.magnitude > env.MapRadius * 0.92f)
                position = position.normalized * env.MapRadius * 0.82f;
            gm.Resources.SpawnNode(position, kind, amount);
        }

        private void SetupAmoebaPrey(V5CellEntity prey)
        {
            if (prey == null) return;
            prey.Stats.currentHp = Mathf.Min(prey.Stats.currentHp, 18f);
            prey.Stats.speed *= 0.62f;
            prey.Directive = V5Directive.Idle;
            prey.Resources.biomass += 12f;
            activeMicroTarget = prey;
            activeMicroTargetInitialHp = Mathf.Max(1f, prey.Stats.currentHp);
        }

        private void StoreRouteCuePosition(V5MvpRoute route, Vector2 cue)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: bacteriaWorldCuePosition = cue; break;
                case V5MvpRoute.Amoeba: amoebaWorldCuePosition = cue; break;
                case V5MvpRoute.PhotosyntheticProducer: producerWorldCuePosition = cue; break;
                case V5MvpRoute.Volvox: volvoxWorldCuePosition = cue; break;
            }
        }

        private Vector2 CuePositionFor(V5MvpRoute route, V5GameManager gm)
        {
            if (RouteWorldCuePlayed(route))
            {
                switch (route)
                {
                    case V5MvpRoute.Bacteria: return bacteriaWorldCuePosition;
                    case V5MvpRoute.Amoeba: return amoebaWorldCuePosition;
                    case V5MvpRoute.PhotosyntheticProducer: return producerWorldCuePosition;
                    case V5MvpRoute.Volvox: return volvoxWorldCuePosition;
                }
            }
            Vector2 origin = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;
            return RouteCuePosition(gm, origin, route, route == V5MvpRoute.PhotosyntheticProducer ? 4.2f : 3.6f);
        }

        private float AmoebaPreyProgress()
        {
            if (activeMicroRoute == V5MvpRoute.Amoeba && activeMicroTarget == null) return 1f;
            if (activeMicroTarget == null) return 0f;
            if (activeMicroTarget.Stats.currentHp <= 0f) return 1f;
            float initial = Mathf.Max(1f, activeMicroTargetInitialHp);
            return Mathf.Clamp01(0.12f + (1f - activeMicroTarget.Stats.currentHp / initial) * 0.88f);
        }

        private void SetMicroObjectiveCompleted(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: bacteriaMicroCompleted = true; break;
                case V5MvpRoute.Amoeba: amoebaMicroCompleted = true; break;
                case V5MvpRoute.PhotosyntheticProducer: producerMicroCompleted = true; break;
                case V5MvpRoute.Volvox: volvoxMicroCompleted = true; break;
            }
        }

        private void ApplyMicroReward(V5MvpRoute route, V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity mother = gm.MotherCell;
            Vector2 cue = CuePositionFor(route, gm);
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    mother.Resources.biomass += 18f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 4.0f, 0.006f, 0f, 0.006f, -0.012f, 0f, 0.040f, 0f);
                    break;
                case V5MvpRoute.Amoeba:
                    mother.Resources.biomass += 22f;
                    mother.Resources.aminoAcids += 12f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 3.4f, 0.018f, 0f, 0f, -0.008f, 0f, 0.004f, 0.030f);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    mother.Resources.atp += 28f;
                    mother.Resources.minerals += 10f;
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 4.8f, 0.004f, 0.030f, 0.045f, -0.020f, -0.006f, 0.012f, 0f);
                    break;
                case V5MvpRoute.Volvox:
                    mother.Resources.lipids += 22f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 5f);
                    if (gm.Body != null) gm.Body.LastMessage = "Micro Volvox: primera cohesion colonial estable.";
                    if (gm.Environment != null) gm.Environment.ModifyArea(cue, 4.0f, 0.008f, 0.006f, 0.014f, -0.010f, 0f, 0.032f, 0f);
                    break;
            }
            PushMomentFeedback(mother, "Micro " + V5MvpCanon.DisplayName(route), RouteMicroColor(route));
        }

        private float HasAny(V5GameManager gm, params V5AdaptationId[] ids)
        {
            if (gm == null || gm.Adaptations == null || ids == null) return 0f;
            for (int i = 0; i < ids.Length; i++)
                if (gm.Adaptations.Has(ids[i])) return 1f;
            return 0f;
        }

        private string CheckText(float value)
        {
            return value > 0.5f ? "[x]" : "[ ]";
        }

        private Color RouteMicroColor(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return new Color(0.58f, 1f, 0.72f, 1f);
                case V5MvpRoute.Amoeba: return new Color(1f, 0.70f, 0.88f, 1f);
                case V5MvpRoute.PhotosyntheticProducer: return new Color(0.78f, 1f, 0.48f, 1f);
                case V5MvpRoute.Volvox: return new Color(0.62f, 0.92f, 1f, 1f);
                default: return new Color(0.86f, 1f, 0.92f, 1f);
            }
        }

        private float LocalChannel01(V5GameManager gm, Vector2 world, float radius, V5OverlayMode channel)
        {
            V5EnvironmentGrid env = gm != null ? gm.Environment : null;
            if (env == null || env.nutrients == null) return 0f;

            int cx, cy;
            env.WorldToTile(world, out cx, out cy);
            int r = Mathf.CeilToInt(radius / Mathf.Max(0.01f, env.TileSize));
            float total = 0f;
            int count = 0;
            for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(env.Width - 1, cx + r); x++)
            {
                for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(env.Height - 1, cy + r); y++)
                {
                    if (Vector2.Distance(world, env.TileCenterWorld(x, y)) > radius) continue;
                    total += ChannelValue(env, x, y, channel);
                    count++;
                }
            }
            return count > 0 ? Mathf.Clamp01(total / count) : 0f;
        }

        private float ChannelValue(V5EnvironmentGrid env, int x, int y, V5OverlayMode channel)
        {
            switch (channel)
            {
                case V5OverlayMode.Nutrients: return env.nutrients[x, y];
                case V5OverlayMode.Light: return env.lightLevel[x, y];
                case V5OverlayMode.Oxygen: return env.oxygen[x, y];
                case V5OverlayMode.Toxins: return env.toxins[x, y];
                case V5OverlayMode.Acidity: return env.acidity[x, y];
                case V5OverlayMode.Colonization: return env.colonization[x, y];
                case V5OverlayMode.Temperature: return env.temperature[x, y];
                default: return 0f;
            }
        }

        private void SetRouteGoalCompleted(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: bacteriaGoalCompleted = true; break;
                case V5MvpRoute.Amoeba: amoebaGoalCompleted = true; break;
                case V5MvpRoute.PhotosyntheticProducer: producerGoalCompleted = true; break;
                case V5MvpRoute.Volvox: volvoxGoalCompleted = true; break;
            }
        }

        private void ApplyMilestonePulse(V5MvpRoute route, V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity mother = gm.MotherCell;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    mother.Resources.atp += 60f;
                    mother.Resources.biomass += 40f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 8f);
                    ApplyBacteriaMoment(gm, mother);
                    break;
                case V5MvpRoute.Amoeba:
                    mother.Resources.biomass += 45f;
                    mother.Resources.aminoAcids += 30f;
                    mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 25f);
                    ApplyAmoebaMoment(gm, mother);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    mother.Resources.atp += 70f;
                    mother.Resources.nucleotides += 30f;
                    mother.Resources.minerals += 20f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 6f);
                    ApplyProducerMoment(gm, mother);
                    break;
                case V5MvpRoute.Volvox:
                    mother.Resources.atp += 50f;
                    mother.Resources.lipids += 50f;
                    mother.Resources.nucleotides += 30f;
                    mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 20f);
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 12f);
                    ApplyVolvoxMoment(gm, mother);
                    break;
            }
        }

        private void ApplyBacteriaMoment(V5GameManager gm, V5CellEntity mother)
        {
            LastIdentityMoment = "Momento Bacteria: expansion de swarm y biofilm local.";
            if (gm.Environment != null)
            {
                for (int i = 0; i < gm.PlayerCells.Count; i++)
                {
                    V5CellEntity c = gm.PlayerCells[i];
                    if (c == null) continue;
                    gm.Environment.ModifyArea(c.transform.position, 3.2f, -0.004f, 0f, 0.006f, -0.012f, 0f, 0.055f, 0.004f);
                }
            }
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 4f);
            }
            PushMomentFeedback(mother, "Swarm MVP", new Color(0.60f, 1f, 0.72f, 1f));
        }

        private void ApplyAmoebaMoment(V5GameManager gm, V5CellEntity mother)
        {
            LastIdentityMoment = "Momento Ameba: pulso depredador y digestion cercana.";
            mother.Stats.physicalDamagePerSecond += 0.18f;
            mother.Stats.attackRange = Mathf.Max(mother.Stats.attackRange, 1.52f);
            if (gm.Environment != null)
                gm.Environment.ModifyArea(mother.transform.position, 4.4f, 0.030f, 0f, 0.010f, -0.020f, 0f, 0.018f, 0.055f);
            DamageNearbyEnemies(gm, mother, 4.6f, 9f, V5DamageKind.Physical);
            PushMomentFeedback(mother, "Fagocitosis MVP", new Color(1f, 0.72f, 0.88f, 1f));
        }

        private void ApplyProducerMoment(V5GameManager gm, V5CellEntity mother)
        {
            LastIdentityMoment = "Momento Productor: bloom fotosintetico local.";
            mother.Stats.atpPerSecond += 0.12f;
            if (gm.Environment != null)
                gm.Environment.ModifyArea(mother.transform.position, 8.0f, 0.030f, 0.020f, 0.140f, -0.060f, -0.020f, 0.070f, 0.020f);
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                c.Resources.atp += 8f;
                if (gm.Environment != null) gm.Environment.ModifyArea(c.transform.position, 2.6f, 0.008f, 0.005f, 0.025f, -0.010f, 0f, 0.015f, 0f);
            }
            PushMomentFeedback(mother, "Bloom MVP", new Color(0.76f, 1f, 0.48f, 1f));
        }

        private void ApplyVolvoxMoment(V5GameManager gm, V5CellEntity mother)
        {
            LastIdentityMoment = "Momento Volvox: sincronia corporal y reparacion colonial.";
            mother.Stats.repairPerSecond += 0.15f;
            if (gm.Environment != null)
                gm.Environment.ModifyArea(mother.transform.position, 5.8f, 0.012f, 0.006f, 0.035f, -0.025f, 0f, 0.045f, 0.006f);
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                bool inBody = c.Role == V5CellRole.Mother || c.IsAttachedToBody;
                if (!inBody) continue;
                c.Stats.currentHp = Mathf.Min(c.Stats.maxHp, c.Stats.currentHp + 16f);
                c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 10f);
                if (c.Role != V5CellRole.Mother) c.Directive = V5Directive.Defend;
            }
            if (gm.Body != null) gm.Body.LastMessage = "Sincronia Volvox: cuerpo reparado y en defensa.";
            PushMomentFeedback(mother, "Sincronia MVP", new Color(0.62f, 0.92f, 1f, 1f));
        }

        private void DamageNearbyEnemies(V5GameManager gm, V5CellEntity source, float radius, float damage, V5DamageKind kind)
        {
            if (gm == null || source == null || gm.NonPlayerCells == null) return;
            Vector2 origin = source.transform.position;
            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity enemy = gm.NonPlayerCells[i];
                if (enemy == null) continue;
                float dist = Vector2.Distance(origin, enemy.transform.position);
                if (dist > radius) continue;
                float falloff = 1f - dist / Mathf.Max(0.01f, radius);
                enemy.Stats.stress = Mathf.Min(100f, enemy.Stats.stress + 8f * falloff);
                enemy.Damage(damage * Mathf.Lerp(0.35f, 1f, falloff), kind, origin);
            }
        }

        private void PushMomentFeedback(V5CellEntity anchor, string text, Color color)
        {
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            Vector2 pos = anchor != null ? (Vector2)anchor.transform.position + Vector2.up * 1.1f : Vector2.zero;
            if (feedback != null) feedback.Push(text, pos, color);
        }

        private float SwarmProgress(V5GameManager gm)
        {
            return Mathf.Clamp01(Mathf.Max(0f, PlayerCellCount(gm) - 1f) / 5f);
        }

        private float ColonizationProgress(V5GameManager gm, float target)
        {
            return Mathf.Clamp01(Colonization01(gm) / Mathf.Max(0.01f, target));
        }

        private float OxygenProgress(V5GameManager gm, float target)
        {
            return Mathf.Clamp01(Oxygen01(gm) / Mathf.Max(0.01f, target));
        }

        private float AmoebaPredatorKitProgress(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            float score = 0f;
            if (gm.Adaptations.Has(V5AdaptationId.Nucleus)) score += 0.25f;
            if (gm.Adaptations.Has(V5AdaptationId.Mitochondria)) score += 0.15f;
            if (gm.Adaptations.Has(V5AdaptationId.Lysosome)) score += 0.25f;
            if (gm.Adaptations.Has(V5AdaptationId.Pseudopods)) score += 0.25f;
            if (gm.Adaptations.Has(V5AdaptationId.ContractileVacuole) || gm.Adaptations.Has(V5AdaptationId.Cilia)) score += 0.10f;
            return Mathf.Clamp01(score);
        }

        private float AmoebaCombatProgress(V5GameManager gm)
        {
            if (gm == null || gm.AffinityLog == null) return 0f;
            return Mathf.Clamp01(gm.AffinityLog.ScoreBonus(V5EvolutionPath.Amoeba) / 18f);
        }

        private float ProducerKitProgress(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            float score = 0f;
            if (gm.Adaptations.Has(V5AdaptationId.ProkaryoticThylakoid)) score += 0.25f;
            if (gm.Adaptations.Has(V5AdaptationId.CatalaseROS)) score += 0.15f;
            if (gm.Adaptations.Has(V5AdaptationId.Nucleus)) score += 0.15f;
            if (gm.Adaptations.Has(V5AdaptationId.Chloroplast)) score += 0.25f;
            if (gm.Adaptations.Has(V5AdaptationId.CelluloseWall) || gm.Adaptations.Has(V5AdaptationId.SilicaFrustule)) score += 0.20f;
            return Mathf.Clamp01(score);
        }

        private float BodySlotProgress(V5GameManager gm, int target)
        {
            return Mathf.Clamp01((float)BodySlots(gm) / Mathf.Max(1, target));
        }

        private float CasteDiversityProgress(V5GameManager gm, int target)
        {
            return Mathf.Clamp01((float)DistinctCasteCount(gm) / Mathf.Max(1, target));
        }

        private int PlayerCellCount(V5GameManager gm)
        {
            return gm != null ? gm.PlayerCellCount() : 0;
        }

        private int BodySlots(V5GameManager gm)
        {
            return gm != null && gm.Body != null ? gm.Body.OccupiedSlots : 0;
        }

        private int DistinctCasteCount(V5GameManager gm)
        {
            if (gm == null || gm.PlayerCells == null) return 0;
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
            return count;
        }

        private float Colonization01(V5GameManager gm)
        {
            return gm != null && gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
        }

        private float Oxygen01(V5GameManager gm)
        {
            return gm != null && gm.Environment != null ? gm.Environment.AverageOxygen() : 0f;
        }

        private string Percent(float value)
        {
            return (Mathf.Clamp01(value) * 100f).ToString("0") + "%";
        }

        private void Toast()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
        }
    }
}
