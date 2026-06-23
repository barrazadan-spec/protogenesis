using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5WorldEventSystem : MonoBehaviour, IV5RunResettable
    {
        public string CurrentEvent = "ecosistema estable";
        public float NextEventIn = 55f;
        public int EventCount;
        public int RouteEventCount;
        public V5MvpRoute LastRouteEvent = V5MvpRoute.None;
        public string LastRouteEventSummary = "sin evento ecologico de ruta";
        public float LastRouteEventNicheScore;
        public bool RouteOpportunityActive;
        public V5MvpRoute ActiveRouteOpportunity = V5MvpRoute.None;
        public Vector2 RouteOpportunityCenter;
        public string RouteOpportunityText = "Oportunidad: ninguna.";
        public string RouteOpportunitySummary = "Oportunidad: ninguna.";
        public float RouteOpportunityProgress01;
        public float RouteOpportunityTimeLeft;
        public int RouteOpportunityCompletedCount;
        public string LastRouteOpportunityReward = "sin recompensa de oportunidad";
        public int RouteAbilityComboCount;
        public int BacteriaRouteAbilityCombos;
        public int AmoebaRouteAbilityCombos;
        public int ProducerRouteAbilityCombos;
        public int VolvoxRouteAbilityCombos;
        public V5MvpRoute LastRouteAbilityComboRoute = V5MvpRoute.None;
        public string LastRouteAbilityCombo = "sin combo de habilidad de ruta";
        public float LastRouteAbilityComboPower;

        private const float RouteOpportunityDuration = 80f;
        private float timer;
        private float routeOpportunityTimer;
        private readonly List<V5CellEntity> activeOpportunityPrey = new List<V5CellEntity>(4);
        private int activeOpportunityPreySpawned;

        public void ResetRun()
        {
            timer = 0f;
            EventCount = 0;
            RouteEventCount = 0;
            LastRouteEvent = V5MvpRoute.None;
            LastRouteEventSummary = "sin evento ecologico de ruta";
            LastRouteEventNicheScore = 0f;
            RouteOpportunityActive = false;
            ActiveRouteOpportunity = V5MvpRoute.None;
            RouteOpportunityCenter = Vector2.zero;
            RouteOpportunityText = "Oportunidad: ninguna.";
            RouteOpportunitySummary = "Oportunidad: ninguna.";
            RouteOpportunityProgress01 = 0f;
            RouteOpportunityTimeLeft = 0f;
            RouteOpportunityCompletedCount = 0;
            LastRouteOpportunityReward = "sin recompensa de oportunidad";
            RouteAbilityComboCount = 0;
            BacteriaRouteAbilityCombos = 0;
            AmoebaRouteAbilityCombos = 0;
            ProducerRouteAbilityCombos = 0;
            VolvoxRouteAbilityCombos = 0;
            LastRouteAbilityComboRoute = V5MvpRoute.None;
            LastRouteAbilityCombo = "sin combo de habilidad de ruta";
            LastRouteAbilityComboPower = 0f;
            routeOpportunityTimer = 0f;
            activeOpportunityPrey.Clear();
            activeOpportunityPreySpawned = 0;
            NextEventIn = Random.Range(45f, 75f);
            CurrentEvent = "ecosistema estable";
        }

        void IV5RunResettable.ResetForNewRun() => ResetRun();

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;

            UpdateRouteOpportunity(gm);

            timer += Time.deltaTime;
            if (timer >= NextEventIn)
            {
                timer = 0f;
                TriggerRandomEvent();
                NextEventIn = Random.Range(65f, 115f);
            }
        }

        private void TriggerRandomEvent()
        {
            V5GameManager gm = V5GameManager.Instance;
            V5MvpRoute route = ActiveRoute(gm);
            if (route != V5MvpRoute.None && Random.value < 0.48f && TriggerRouteEvent(route, gm))
            {
                EventCount++;
                return;
            }

            int roll = Random.Range(0, 5);
            if (roll == 0) NutrientBloom();
            else if (roll == 1) ToxinBloom();
            else if (roll == 2) OxygenBloom();
            else if (roll == 3) AcidFront();
            else PredatorMigration();
            EventCount++;
        }

        public bool TriggerRouteEventNow(V5MvpRoute route)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (route == V5MvpRoute.None) route = ActiveRoute(gm);
            if (!TriggerRouteEvent(route, gm)) return false;
            EventCount++;
            return true;
        }

        private bool TriggerRouteEvent(V5MvpRoute route, V5GameManager gm)
        {
            if (route == V5MvpRoute.None || gm == null || gm.Environment == null) return false;

            Vector2 center = RouteEventPoint(gm, route);
            float score = gm.NichePressure != null ? gm.NichePressure.ScanNow(gm) : 0.5f;
            LastRouteEvent = route;
            LastRouteEventNicheScore = score;
            RouteEventCount++;
            StartRouteOpportunity(route, center);

            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    gm.Environment.ModifyArea(center, 6.4f, 0.075f, 0f, 0.018f, -0.045f, -0.004f, 0.135f, 0.020f);
                    SpawnEventResource(gm, center + Random.insideUnitCircle * 3.2f, V5ResourceKind.Biomass, 34f);
                    SpawnEventResource(gm, center + Random.insideUnitCircle * 3.2f, V5ResourceKind.AminoAcids, 18f);
                    CurrentEvent = "Evento Bacteria: ventana de biofilm, ocupa territorio y convierte biomasa.";
                    RouteOpportunityText = "Oportunidad Bacteria: sube colonizacion local o instala Pared/Pili.";
                    break;
                case V5MvpRoute.Amoeba:
                    gm.Environment.ModifyArea(center, 6.0f, 0.035f, 0f, 0.018f, -0.010f, 0f, 0.010f, 0.160f);
                    for (int i = 0; i < 3; i++)
                    {
                        V5CellEntity prey = SpawnWeakPrey(gm, center + Random.insideUnitCircle * 3.5f);
                        if (prey != null) activeOpportunityPrey.Add(prey);
                    }
                    activeOpportunityPreySpawned = activeOpportunityPrey.Count;
                    CurrentEvent = "Evento Ameba: deriva de presas y detritus, momento de cazar.";
                    RouteOpportunityText = "Oportunidad Ameba: caza presas debiles o convierte detritus en impulso.";
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    gm.Environment.ModifyArea(center, 8.5f, 0.020f, 0.220f, 0.180f, -0.060f, -0.015f, 0.030f, 0f);
                    SpawnEventResource(gm, center + Random.insideUnitCircle * 3.6f, V5ResourceKind.Minerals, 24f);
                    SpawnEventResource(gm, center + Random.insideUnitCircle * 3.6f, V5ResourceKind.ATP, 26f);
                    CurrentEvent = "Evento Productor: pulso fotico, luz y O2 abren economia solar.";
                    RouteOpportunityText = "Oportunidad Productor: estabiliza luz/O2 o instala Tilacoide/Cloroplasto.";
                    break;
                case V5MvpRoute.Volvox:
                    gm.Environment.ModifyArea(center, 7.2f, 0.040f, 0.045f, 0.075f, -0.055f, -0.006f, 0.095f, 0.012f);
                    SpawnEventResource(gm, center + Random.insideUnitCircle * 3.3f, V5ResourceKind.Lipids, 28f);
                    SpawnEventResource(gm, center + Random.insideUnitCircle * 3.3f, V5ResourceKind.Nucleotides, 18f);
                    if (gm.MotherCell != null) gm.MotherCell.Stats.stress = Mathf.Max(0f, gm.MotherCell.Stats.stress - 3f);
                    CurrentEvent = "Evento Volvox: calma colonial, adhesion y castas ganan espacio estable.";
                    RouteOpportunityText = "Oportunidad Volvox: agrega cuerpo/adhesion y conserva zona estable.";
                    break;
            }

            RouteOpportunityProgress01 = OpportunityProgress(gm);
            RouteOpportunitySummary = OpportunitySummaryText();
            LastRouteEventSummary = CurrentEvent + " Nicho " + (score * 100f).ToString("0") + "%.";
            if (gm.Hud != null) gm.Hud.Toast(CurrentEvent);
            return true;
        }

        public bool EvaluateRouteOpportunityNow(V5GameManager gm = null)
        {
            if (gm == null) gm = V5GameManager.Instance;
            if (!RouteOpportunityActive || gm == null) return false;
            RouteOpportunityProgress01 = OpportunityProgress(gm);
            RouteOpportunitySummary = OpportunitySummaryText();
            if (RouteOpportunityProgress01 < 0.995f) return false;
            CompleteRouteOpportunity(gm);
            return true;
        }

        public bool RegisterRouteAbilityCast(V5MvpRoute route, Vector2 castCenter, float power, string abilityLabel, V5GameManager gm = null)
        {
            if (gm == null) gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null || !RouteOpportunityActive) return false;
            if (route == V5MvpRoute.None || route != ActiveRouteOpportunity) return false;

            float distance = Vector2.Distance(castCenter, RouteOpportunityCenter);
            if (distance > 9.0f) return false;

            float before = RouteOpportunityProgress01;
            ApplyRouteAbilityOpportunityPulse(route, gm, Mathf.Max(0.8f, power));
            RouteOpportunityProgress01 = OpportunityProgress(gm);

            RouteAbilityComboCount++;
            IncrementRouteAbilityCombo(route);
            LastRouteAbilityComboRoute = route;
            LastRouteAbilityComboPower = power;
            LastRouteAbilityCombo = "Combo " + V5MvpCanon.DisplayName(route) + ": " +
                                    (string.IsNullOrEmpty(abilityLabel) ? "habilidad de ruta" : abilityLabel) +
                                    " impulso " + (Mathf.Max(0f, RouteOpportunityProgress01 - before) * 100f).ToString("0") + "%.";

            if (gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(route), 7f + power * 2f, "combo " + abilityLabel, "route_ability_combo");
            if (gm.Hud != null) gm.Hud.Toast(LastRouteAbilityCombo);

            bool completed = EvaluateRouteOpportunityNow(gm);
            RouteOpportunitySummary = OpportunitySummaryText();
            return completed || RouteOpportunityProgress01 > before + 0.02f;
        }

        public int ComboCountForRoute(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return BacteriaRouteAbilityCombos;
                case V5MvpRoute.Amoeba: return AmoebaRouteAbilityCombos;
                case V5MvpRoute.PhotosyntheticProducer: return ProducerRouteAbilityCombos;
                case V5MvpRoute.Volvox: return VolvoxRouteAbilityCombos;
                default: return 0;
            }
        }

        private void StartRouteOpportunity(V5MvpRoute route, Vector2 center)
        {
            RouteOpportunityActive = true;
            ActiveRouteOpportunity = route;
            RouteOpportunityCenter = center;
            RouteOpportunityProgress01 = 0f;
            RouteOpportunityTimeLeft = RouteOpportunityDuration;
            routeOpportunityTimer = 0f;
            activeOpportunityPrey.Clear();
            activeOpportunityPreySpawned = 0;
            if (RouteOpportunityCompletedCount <= 0) LastRouteOpportunityReward = "oportunidad en curso";
            RouteOpportunityText = "Oportunidad " + V5MvpCanon.DisplayName(route) + ": lee el nicho y aprovecha el evento.";
            RouteOpportunitySummary = OpportunitySummaryText();
        }

        private void UpdateRouteOpportunity(V5GameManager gm)
        {
            if (!RouteOpportunityActive) return;

            routeOpportunityTimer += Time.deltaTime;
            RouteOpportunityTimeLeft = Mathf.Max(0f, RouteOpportunityDuration - routeOpportunityTimer);
            EvaluateRouteOpportunityNow(gm);
            if (!RouteOpportunityActive) return;

            if (routeOpportunityTimer >= RouteOpportunityDuration)
            {
                RouteOpportunityActive = false;
                RouteOpportunityText = "Oportunidad " + V5MvpCanon.DisplayName(ActiveRouteOpportunity) + " expiro.";
                RouteOpportunitySummary = OpportunitySummaryText();
            }
        }

        private float OpportunityProgress(V5GameManager gm)
        {
            if (gm == null || gm.Environment == null || ActiveRouteOpportunity == V5MvpRoute.None) return 0f;

            switch (ActiveRouteOpportunity)
            {
                case V5MvpRoute.Bacteria:
                    return Mathf.Clamp01(LocalChannel01(gm.Environment, RouteOpportunityCenter, 4.8f, V5OverlayMode.Colonization) / 0.18f * 0.72f +
                                         HasAnyAdaptation(gm, V5AdaptationId.BacterialWall, V5AdaptationId.PiliFimbriae) * 0.28f);
                case V5MvpRoute.Amoeba:
                    return Mathf.Clamp01(Mathf.Max(DefeatedPreyProgress(), LocalDetritus01(gm.Environment, RouteOpportunityCenter, 4.6f) / 0.30f) +
                                         HasAnyAdaptation(gm, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods) * 0.12f);
                case V5MvpRoute.PhotosyntheticProducer:
                    return Mathf.Clamp01(LocalChannel01(gm.Environment, RouteOpportunityCenter, 5.8f, V5OverlayMode.Light) / 0.70f * 0.34f +
                                         LocalChannel01(gm.Environment, RouteOpportunityCenter, 5.8f, V5OverlayMode.Oxygen) / 0.46f * 0.44f +
                                         HasAnyAdaptation(gm, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast) * 0.22f);
                case V5MvpRoute.Volvox:
                    return Mathf.Clamp01(LocalChannel01(gm.Environment, RouteOpportunityCenter, 5.0f, V5OverlayMode.Colonization) / 0.13f * 0.34f +
                                         BodyProgress(gm) * 0.34f +
                                         HasAnyAdaptation(gm, V5AdaptationId.BasicAdhesin, V5AdaptationId.ColonialAdhesin) * 0.32f);
                default:
                    return 0f;
            }
        }

        private void CompleteRouteOpportunity(V5GameManager gm)
        {
            if (!RouteOpportunityActive) return;

            V5MvpRoute route = ActiveRouteOpportunity;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            RouteOpportunityCompletedCount++;
            RouteOpportunityProgress01 = 1f;
            RouteOpportunityActive = false;

            if (mother != null)
            {
                switch (route)
                {
                    case V5MvpRoute.Bacteria:
                        mother.Resources.biomass += 22f;
                        mother.Resources.aminoAcids += 8f;
                        mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 4f);
                        LastRouteOpportunityReward = "Recompensa Bacteria: biomasa de biofilm y stress reducido.";
                        break;
                    case V5MvpRoute.Amoeba:
                        mother.Resources.biomass += 18f;
                        mother.Resources.aminoAcids += 16f;
                        mother.Stats.physicalDamagePerSecond += 0.10f;
                        mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 4f);
                        LastRouteOpportunityReward = "Recompensa Ameba: digestion rapida y mordida mas fuerte.";
                        break;
                    case V5MvpRoute.PhotosyntheticProducer:
                        mother.Resources.atp += 34f;
                        mother.Resources.minerals += 10f;
                        mother.Stats.synthesisRate += 0.04f;
                        LastRouteOpportunityReward = "Recompensa Productor: reserva solar y sintesis afinada.";
                        break;
                    case V5MvpRoute.Volvox:
                        mother.Resources.lipids += 18f;
                        mother.Resources.nucleotides += 10f;
                        mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 7f);
                        LastRouteOpportunityReward = "Recompensa Volvox: cohesion colonial y materiales de cuerpo.";
                        break;
                }
            }
            else
            {
                LastRouteOpportunityReward = "Recompensa " + V5MvpCanon.DisplayName(route) + ": oportunidad completada.";
            }

            RouteOpportunityText = "Oportunidad completada: " + LastRouteOpportunityReward;
            RouteOpportunitySummary = OpportunitySummaryText();
            if (gm != null && gm.RouteMastery != null) gm.RouteMastery.RecordOpportunity(route, gm);
            if (gm != null && gm.Hud != null) gm.Hud.Toast(RouteOpportunityText);
            PushOpportunityFeedback(mother, route);
        }

        private void IncrementRouteAbilityCombo(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaRouteAbilityCombos++; break;
                case V5MvpRoute.Amoeba: AmoebaRouteAbilityCombos++; break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerRouteAbilityCombos++; break;
                case V5MvpRoute.Volvox: VolvoxRouteAbilityCombos++; break;
            }
        }

        private void ApplyRouteAbilityOpportunityPulse(V5MvpRoute route, V5GameManager gm, float power)
        {
            Vector2 center = RouteOpportunityCenter;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    gm.Environment.ModifyArea(center, 5.2f + power, 0.020f * power, 0f, 0.006f * power, -0.018f * power, 0f, 0.120f * power, 0.006f);
                    break;
                case V5MvpRoute.Amoeba:
                    gm.Environment.ModifyArea(center, 4.8f + power * 0.4f, 0.025f * power, 0f, 0f, -0.008f, 0f, 0.008f, 0.180f * power);
                    DamageOpportunityPrey(center, 4.8f + power, 24f * power);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    gm.Environment.ModifyArea(center, 6.4f + power, 0.012f * power, 0.120f * power, 0.180f * power, -0.040f * power, -0.010f, 0.030f * power, 0f);
                    break;
                case V5MvpRoute.Volvox:
                    gm.Environment.ModifyArea(center, 5.4f + power, 0.016f * power, 0.015f * power, 0.050f * power, -0.026f * power, 0f, 0.110f * power, 0f);
                    if (gm.MotherCell != null) gm.MotherCell.Stats.stress = Mathf.Max(0f, gm.MotherCell.Stats.stress - 4f * power);
                    break;
            }
        }

        private void DamageOpportunityPrey(Vector2 center, float radius, float damage)
        {
            for (int i = 0; i < activeOpportunityPrey.Count; i++)
            {
                V5CellEntity prey = activeOpportunityPrey[i];
                if (prey == null) continue;
                if (Vector2.Distance(center, prey.transform.position) > radius) continue;
                prey.Damage(damage, V5DamageKind.Physical, center);
            }
        }

        private float LocalChannel01(V5EnvironmentGrid env, Vector2 center, float radius, V5OverlayMode channel)
        {
            if (env == null || env.nutrients == null) return 0f;
            int cx, cy;
            env.WorldToTile(center, out cx, out cy);
            int tr = Mathf.CeilToInt(radius / Mathf.Max(0.01f, env.TileSize));
            float total = 0f;
            int count = 0;
            for (int x = Mathf.Max(0, cx - tr); x <= Mathf.Min(env.Width - 1, cx + tr); x++)
            {
                for (int y = Mathf.Max(0, cy - tr); y <= Mathf.Min(env.Height - 1, cy + tr); y++)
                {
                    if (Vector2.Distance(center, env.TileCenterWorld(x, y)) > radius) continue;
                    total += ChannelValue(env, channel, x, y);
                    count++;
                }
            }
            return count > 0 ? Mathf.Clamp01(total / count) : 0f;
        }

        private float LocalDetritus01(V5EnvironmentGrid env, Vector2 center, float radius)
        {
            if (env == null || env.detritus == null) return 0f;
            int cx, cy;
            env.WorldToTile(center, out cx, out cy);
            int tr = Mathf.CeilToInt(radius / Mathf.Max(0.01f, env.TileSize));
            float total = 0f;
            int count = 0;
            for (int x = Mathf.Max(0, cx - tr); x <= Mathf.Min(env.Width - 1, cx + tr); x++)
            {
                for (int y = Mathf.Max(0, cy - tr); y <= Mathf.Min(env.Height - 1, cy + tr); y++)
                {
                    if (Vector2.Distance(center, env.TileCenterWorld(x, y)) > radius) continue;
                    total += env.detritus[x, y];
                    count++;
                }
            }
            return count > 0 ? Mathf.Clamp01(total / count) : 0f;
        }

        private float ChannelValue(V5EnvironmentGrid env, V5OverlayMode channel, int x, int y)
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

        private float DefeatedPreyProgress()
        {
            if (activeOpportunityPreySpawned <= 0) return 0f;
            int defeated = 0;
            for (int i = 0; i < activeOpportunityPrey.Count; i++)
            {
                V5CellEntity prey = activeOpportunityPrey[i];
                if (prey == null || prey.Stats.currentHp <= 0f) defeated++;
            }
            return Mathf.Clamp01((float)defeated / Mathf.Max(1, activeOpportunityPreySpawned));
        }

        private float BodyProgress(V5GameManager gm)
        {
            if (gm == null || gm.Body == null) return 0f;
            return Mathf.Clamp01(gm.Body.OccupiedSlots / 2f);
        }

        private float HasAnyAdaptation(V5GameManager gm, params V5AdaptationId[] ids)
        {
            if (gm == null || gm.Adaptations == null || ids == null) return 0f;
            for (int i = 0; i < ids.Length; i++)
                if (gm.Adaptations.Has(ids[i])) return 1f;
            return 0f;
        }

        private string OpportunitySummaryText()
        {
            if (ActiveRouteOpportunity == V5MvpRoute.None) return "Oportunidad: ninguna.";
            string state = RouteOpportunityActive ? "activa" : "cerrada";
            return "Oportunidad " + V5MvpCanon.DisplayName(ActiveRouteOpportunity) + " " +
                   (RouteOpportunityProgress01 * 100f).ToString("0") + "% " + state +
                   " | " + RouteOpportunityText;
        }

        private void PushOpportunityFeedback(V5CellEntity mother, V5MvpRoute route)
        {
            if (mother == null) return;
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback == null) return;
            Color color = new Color(0.82f, 1f, 0.76f, 1f);
            if (route == V5MvpRoute.Amoeba) color = new Color(1f, 0.72f, 0.88f, 1f);
            else if (route == V5MvpRoute.PhotosyntheticProducer) color = new Color(0.78f, 1f, 0.48f, 1f);
            else if (route == V5MvpRoute.Volvox) color = new Color(0.62f, 0.92f, 1f, 1f);
            feedback.PushFloating("Oportunidad " + V5MvpCanon.DisplayName(route), mother.transform.position, color);
        }

        private Vector2 RandomMapPoint(float inner = 0.15f, float outer = 0.90f)
        {
            V5EnvironmentGrid env = V5GameManager.Instance != null ? V5GameManager.Instance.Environment : null;
            float r = env != null ? env.MapRadius : V5Balance.DefaultMapRadius;
            return Random.insideUnitCircle.normalized * Random.Range(r * inner, r * outer);
        }

        private V5MvpRoute ActiveRoute(V5GameManager gm)
        {
            if (gm == null) return V5MvpRoute.None;
            return gm.MvpIntent != null ? gm.MvpIntent.EffectiveRoute(gm) : V5MvpCanon.CurrentRoute(gm);
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

        private Vector2 RouteEventPoint(V5GameManager gm, V5MvpRoute route)
        {
            Vector2 origin = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;
            Vector2 dir;
            switch (route)
            {
                case V5MvpRoute.Bacteria: dir = new Vector2(-0.90f, 0.28f); break;
                case V5MvpRoute.Amoeba: dir = new Vector2(0.90f, 0.20f); break;
                case V5MvpRoute.PhotosyntheticProducer: dir = new Vector2(0.12f, 1f); break;
                case V5MvpRoute.Volvox: dir = new Vector2(-0.20f, -1f); break;
                default: dir = Vector2.right; break;
            }

            dir = (dir + Random.insideUnitCircle * 0.18f).normalized;
            Vector2 pos = origin + dir * (route == V5MvpRoute.PhotosyntheticProducer ? 7.0f : 5.4f);
            V5EnvironmentGrid env = gm != null ? gm.Environment : null;
            if (env != null)
            {
                float max = env.MapRadius * 0.84f;
                if (pos.magnitude > max) pos = origin - dir * 5.4f;
                if (pos.magnitude > max) pos = pos.normalized * max;
            }
            return pos;
        }

        private void SpawnEventResource(V5GameManager gm, Vector2 position, V5ResourceKind kind, float amount)
        {
            if (gm == null || gm.Resources == null) return;
            V5EnvironmentGrid env = gm.Environment;
            if (env != null && position.magnitude > env.MapRadius * 0.92f)
                position = position.normalized * env.MapRadius * 0.82f;
            gm.Resources.SpawnNode(position, kind, amount);
        }

        private V5CellEntity SpawnWeakPrey(V5GameManager gm, Vector2 position)
        {
            if (gm == null || gm.CellFactory == null) return null;
            V5EnvironmentGrid env = gm.Environment;
            if (env != null && position.magnitude > env.MapRadius * 0.92f)
                position = position.normalized * env.MapRadius * 0.82f;

            V5CellEntity prey = gm.CellFactory.SpawnNeutral(position, Random.value < 0.68f ? V5EvolutionPath.Bacteria : V5EvolutionPath.Flagellate);
            if (prey == null) return null;
            prey.Stats.currentHp = Mathf.Min(prey.Stats.currentHp, 20f);
            prey.Stats.speed *= 0.68f;
            prey.Directive = V5Directive.Idle;
            prey.Resources.biomass += 8f;
            prey.Resources.aminoAcids += 5f;
            return prey;
        }

        private void NutrientBloom()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return;
            Vector2 center = RandomMapPoint();
            gm.Environment.ModifyArea(center, 7f, 0.22f, 0f, 0f, -0.04f, 0f, 0f, 0.12f);
            if (gm.Resources != null)
            {
                for (int i = 0; i < 12; i++)
                {
                    Vector2 p = center + Random.insideUnitCircle * 5f;
                    gm.Resources.SpawnNode(p, i % 3 == 0 ? V5ResourceKind.AminoAcids : V5ResourceKind.Biomass, Random.Range(30f, 80f));
                }
            }
            CurrentEvent = "Bloom nutritivo: recursos nuevos en la gota.";
        }

        private void ToxinBloom()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return;
            Vector2 center = RandomMapPoint();
            gm.Environment.ModifyArea(center, 8f, -0.05f, 0f, -0.02f, 0.28f, 0.02f, -0.03f, 0.05f);
            CurrentEvent = "Bloom tóxico: una zona se volvió peligrosa.";
        }

        private void OxygenBloom()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return;
            Vector2 center = RandomMapPoint(0.05f, 0.8f);
            gm.Environment.ModifyArea(center, 9f, 0.02f, 0.18f, 0.24f, -0.05f, -0.01f, 0.02f, 0f);
            CurrentEvent = "Pulso de luz y oxígeno: fotosintéticos favorecidos.";
        }

        private void AcidFront()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return;
            Vector2 center = RandomMapPoint(0.35f, 0.98f);
            gm.Environment.ModifyArea(center, 10f, -0.04f, -0.02f, -0.04f, 0.10f, 0.25f, -0.03f, 0.08f);
            CurrentEvent = "Frente ácido: rutas resistentes ganan valor.";
        }

        private void PredatorMigration()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null) return;
            Vector2 center = RandomMapPoint(0.35f, 0.95f);
            int count = Mathf.Clamp(2 + EventCount / 2, 2, 7);
            for (int i = 0; i < count; i++)
            {
                Vector2 p = center + Random.insideUnitCircle * 4f;
                V5EvolutionPath path = Random.value < 0.65f ? V5EvolutionPath.Amoeba : V5EvolutionPath.Flagellate;
                V5CellEntity enemy = gm.CellFactory.SpawnNeutral(p, path);
                if (enemy != null) enemy.gameObject.AddComponent<V5EnemyBrain>();
            }
            CurrentEvent = "Migración depredadora: organismos hostiles entraron a la gota.";
        }

        public float TimeUntilNext()
        {
            return Mathf.Max(0f, NextEventIn - timer);
        }
    }
}
