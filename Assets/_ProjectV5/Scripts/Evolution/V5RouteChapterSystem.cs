using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RouteChapterSystem : MonoBehaviour, IV5RunResettable
    {
        private const int MaxChapters = 4;
        private const float CompleteThreshold = 0.995f;

        public V5MvpRoute ActiveRoute = V5MvpRoute.None;
        public int ActiveChapter;
        public float ActiveChapterProgress01;
        public int TotalChapterCompletions;
        public V5MvpRoute LastCompletedRoute = V5MvpRoute.None;
        public int LastCompletedChapter;
        public string Summary = "Capitulos MVP: elige una ruta.";
        public string CurrentChapterText = "Capitulo MVP: sin ruta.";
        public string LastChapterMoment = "Capitulos MVP listos.";

        public int BacteriaChapter;
        public int AmoebaChapter;
        public int ProducerChapter;
        public int VolvoxChapter;

        private float tick;

        public void ResetForNewRun()
        {
            ActiveRoute = V5MvpRoute.None;
            ActiveChapter = 0;
            ActiveChapterProgress01 = 0f;
            TotalChapterCompletions = 0;
            LastCompletedRoute = V5MvpRoute.None;
            LastCompletedChapter = 0;
            Summary = "Capitulos MVP: elige una ruta.";
            CurrentChapterText = "Capitulo MVP: sin ruta.";
            LastChapterMoment = "Capitulos MVP listos.";
            BacteriaChapter = 0;
            AmoebaChapter = 0;
            ProducerChapter = 0;
            VolvoxChapter = 0;
            tick = 0f;
        }

        private void Update()
        {
            tick += Time.deltaTime;
            if (tick < 0.65f) return;
            tick = 0f;
            EvaluateNow(V5GameManager.Instance);
        }

        public bool EvaluateNow(V5GameManager gm)
        {
            RefreshSnapshot(gm);
            if (gm == null || ActiveRoute == V5MvpRoute.None) return false;

            bool completedAny = false;
            int guard = 0;
            while (guard++ < MaxChapters)
            {
                int completed = CompletedFor(ActiveRoute);
                if (completed >= MaxChapters) break;

                int chapter = completed + 1;
                float progress = ChapterProgress01(ActiveRoute, chapter, gm);
                if (progress < CompleteThreshold) break;

                CompleteChapter(ActiveRoute, chapter, gm);
                completedAny = true;
            }

            RefreshSnapshot(gm);
            return completedAny;
        }

        public int CompletedFor(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return BacteriaChapter;
                case V5MvpRoute.Amoeba: return AmoebaChapter;
                case V5MvpRoute.PhotosyntheticProducer: return ProducerChapter;
                case V5MvpRoute.Volvox: return VolvoxChapter;
                default: return 0;
            }
        }

        public float RouteChapterScore01(V5MvpRoute route)
        {
            return Mathf.Clamp01(CompletedFor(route) / (float)MaxChapters);
        }

        public string ChapterStatus(V5GameManager gm)
        {
            RefreshSnapshot(gm);
            return Summary;
        }

        private void RefreshSnapshot(V5GameManager gm)
        {
            ActiveRoute = ActiveRouteFor(gm);
            if (ActiveRoute == V5MvpRoute.None)
            {
                ActiveChapter = 0;
                ActiveChapterProgress01 = 0f;
                CurrentChapterText = "Capitulo MVP: sin ruta.";
                Summary = "Capitulos MVP: elige una ruta.";
                return;
            }

            int completed = CompletedFor(ActiveRoute);
            ActiveChapter = Mathf.Clamp(completed + 1, 1, MaxChapters);
            ActiveChapterProgress01 = completed >= MaxChapters ? 1f : ChapterProgress01(ActiveRoute, ActiveChapter, gm);
            CurrentChapterText = ChapterTitle(ActiveRoute, ActiveChapter) + ": " + ChapterObjective(ActiveRoute, ActiveChapter, gm);
            Summary = "Capitulo " + V5MvpCanon.DisplayName(ActiveRoute) + " " +
                      Mathf.Min(MaxChapters, completed + 1) + "/" + MaxChapters +
                      " " + Percent(ActiveChapterProgress01) +
                      (completed >= MaxChapters ? " | ruta narrada completa" : " | " + CurrentChapterText);
        }

        private float ChapterProgress01(V5MvpRoute route, int chapter, V5GameManager gm)
        {
            if (gm == null || route == V5MvpRoute.None) return 0f;
            int buildStage = V5MvpCanon.BuildStage(route, gm.Adaptations);
            float buildProgress = V5MvpCanon.BuildProgress01(route, gm.Adaptations);
            float micro = gm.MvpIntent != null ? gm.MvpIntent.RouteMicroObjectiveProgress01(gm) : 0f;
            int combos = gm.WorldEvents != null ? gm.WorldEvents.ComboCountForRoute(route) : 0;
            int opportunities = gm.RouteMastery != null ? gm.RouteMastery.CompletionCount(route) : 0;
            int counterAnswers = gm.RouteCounters != null ? gm.RouteCounters.AnsweredCountForRoute(route) : 0;
            float goal = gm.MvpIntent != null ? gm.MvpIntent.RouteGoalProgress01(gm) : 0f;
            float climax = gm.RouteClimax != null && gm.RouteClimax.ActiveRoute == route ? gm.RouteClimax.ClimaxScore01 : 0f;

            switch (chapter)
            {
                case 1:
                    return Mathf.Clamp01(Mathf.Max(buildProgress, buildStage >= 1 ? 1f : 0f, micro * 0.82f));
                case 2:
                    return Mathf.Clamp01((buildStage >= 2 ? 0.72f : Mathf.Clamp01(buildStage / 2f) * 0.72f) +
                                         (RouteAbilityUsed(route, gm) ? 0.28f : 0f));
                case 3:
                    return Mathf.Clamp01(Mathf.Max(opportunities >= 1 ? 1f : 0f, combos >= 1 ? 1f : 0f));
                case 4:
                    return Mathf.Clamp01((counterAnswers >= 1 ? 0.50f : 0f) +
                                         goal * 0.32f +
                                         climax * 0.18f);
                default:
                    return 0f;
            }
        }

        private void CompleteChapter(V5MvpRoute route, int chapter, V5GameManager gm)
        {
            int completed = CompletedFor(route);
            if (chapter <= completed || chapter < 1 || chapter > MaxChapters) return;

            StoreCompleted(route, chapter);
            TotalChapterCompletions++;
            LastCompletedRoute = route;
            LastCompletedChapter = chapter;
            LastChapterMoment = "Capitulo " + chapter + " " + V5MvpCanon.DisplayName(route) + ": " +
                                ChapterTitle(route, chapter) + " completado.";

            ApplyChapterReward(route, chapter, gm);

            if (gm != null && gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(route), 5f + chapter * 1.5f, "capitulo MVP " + chapter, "mvp_chapter");
            if (gm != null && gm.Codex != null)
                gm.Codex.Unlock("Capitulo MVP: " + V5MvpCanon.DisplayName(route), LastChapterMoment + " " + ChapterObjective(route, chapter, gm));
            if (gm != null && gm.Hud != null)
                gm.Hud.Toast(LastChapterMoment);

            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null)
            {
                Vector2 pos = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position + Vector2.up * 1.6f : Vector2.zero;
                feedback.PushFloating("Capitulo " + chapter + " " + V5MvpCanon.DisplayName(route), pos, ChapterColor(route));
                feedback.Ping(chapter >= 4 ? "gene" : "structure");
            }
        }

        private void StoreCompleted(V5MvpRoute route, int chapter)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaChapter = Mathf.Max(BacteriaChapter, chapter); break;
                case V5MvpRoute.Amoeba: AmoebaChapter = Mathf.Max(AmoebaChapter, chapter); break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerChapter = Mathf.Max(ProducerChapter, chapter); break;
                case V5MvpRoute.Volvox: VolvoxChapter = Mathf.Max(VolvoxChapter, chapter); break;
            }
        }

        private void ApplyChapterReward(V5MvpRoute route, int chapter, V5GameManager gm)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            float scale = 1f + chapter * 0.25f;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    mother.Resources.biomass += 8f * scale;
                    mother.Resources.aminoAcids += chapter >= 3 ? 5f * scale : 0f;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 1.5f * chapter);
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 3.6f + chapter * 0.35f, 0.004f, 0f, 0.004f, -0.008f, 0f, 0.018f * scale, 0.002f);
                    break;
                case V5MvpRoute.Amoeba:
                    mother.Resources.biomass += 7f * scale;
                    mother.Resources.aminoAcids += 8f * scale;
                    if (chapter >= 2) mother.Stats.physicalDamagePerSecond += 0.025f * scale;
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    mother.Resources.atp += 12f * scale;
                    mother.Resources.minerals += 4f * scale;
                    if (chapter >= 2) mother.Stats.synthesisRate += 0.010f * scale;
                    if (gm.Environment != null) gm.Environment.ModifyArea(mother.transform.position, 4.8f + chapter * 0.4f, 0.004f, 0.010f, 0.028f * scale, -0.010f, -0.002f, 0.010f, 0f);
                    break;
                case V5MvpRoute.Volvox:
                    mother.Resources.lipids += 8f * scale;
                    mother.Resources.nucleotides += 5f * scale;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 2f * chapter);
                    if (chapter >= 3 && gm.Body != null) gm.Body.LastMessage = "Capitulo Volvox: cohesion narrativa de cuerpo reforzada.";
                    break;
            }
        }

        private string ChapterTitle(V5MvpRoute route, int chapter)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    if (chapter == 1) return "Adhesion inicial";
                    if (chapter == 2) return "Marea de biofilm";
                    if (chapter == 3) return "Ventana de biomasa";
                    return "Fago neutralizado";
                case V5MvpRoute.Amoeba:
                    if (chapter == 1) return "Presa marcada";
                    if (chapter == 2) return "Fagocitosis alfa";
                    if (chapter == 3) return "Banquete de nicho";
                    return "Depredador alfa";
                case V5MvpRoute.PhotosyntheticProducer:
                    if (chapter == 1) return "Primer tilacoide";
                    if (chapter == 2) return "Bloom fotosintetico";
                    if (chapter == 3) return "Pulso fotico";
                    return "Sombra rota";
                case V5MvpRoute.Volvox:
                    if (chapter == 1) return "Adhesion colonial";
                    if (chapter == 2) return "Sincronia Volvox";
                    if (chapter == 3) return "Cuerpo estable";
                    return "Cizalla respondida";
                default:
                    return "Capitulo";
            }
        }

        private string ChapterObjective(V5MvpRoute route, int chapter, V5GameManager gm)
        {
            switch (chapter)
            {
                case 1:
                    return "instala la primera pieza de build o completa la micro-mision.";
                case 2:
                    return "alcanza build 2 y usa la habilidad de ruta.";
                case 3:
                    return "convierte una oportunidad de ruta en combo o mastery.";
                case 4:
                    return "responde la contra-presion y acerca la meta de ruta al climax.";
                default:
                    return "ruta completa.";
            }
        }

        private bool RouteAbilityUsed(V5MvpRoute route, V5GameManager gm)
        {
            if (gm == null || gm.Abilities == null || gm.Abilities.RouteFantasyCastCount <= 0) return false;
            string last = gm.Abilities.LastRouteFantasyAbility;
            return !string.IsNullOrEmpty(last) && last.Contains(V5MvpCanon.DisplayName(route));
        }

        private V5MvpRoute ActiveRouteFor(V5GameManager gm)
        {
            if (gm == null) return V5MvpRoute.None;
            if (gm.MvpIntent != null) return gm.MvpIntent.EffectiveRoute(gm);
            return V5MvpCanon.CurrentRoute(gm);
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

        private Color ChapterColor(V5MvpRoute route)
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

        private string Percent(float value)
        {
            return (Mathf.Clamp01(value) * 100f).ToString("0") + "%";
        }
    }
}
