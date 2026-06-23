using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RouteCounterPressureSystem : MonoBehaviour, IV5RunResettable
    {
        public bool ActiveCounter;
        public V5MvpRoute ActiveCounterRoute = V5MvpRoute.None;
        public V5RouteBranchId LastCounterBranch = V5RouteBranchId.None;
        public Vector2 CounterCenter;
        public float CounterTimeLeft;
        public float LastCounterPressure;
        public int CounterEventCount;
        public int CounterAnsweredCount;
        public int BacteriaCounters;
        public int AmoebaCounters;
        public int ProducerCounters;
        public int VolvoxCounters;
        public int BacteriaCounterAnswers;
        public int AmoebaCounterAnswers;
        public int ProducerCounterAnswers;
        public int VolvoxCounterAnswers;
        public string LastCounterName = "sin contra-presion de ruta";
        public string LastCounterSummary = "Contra-presion MVP: latente.";
        public string LastCounterAdvice = "La ecologia aun no responde a tu ruta.";
        public string LastCounterplayResult = "sin counterplay de ruta";
        public string LastCounterVisualCue = "sin marcador de contra-presion";
        public string LastCounterBranchName = "sin rama";
        public V5BranchDoctrineChoice LastCounterDoctrine = V5BranchDoctrineChoice.None;
        public string LastCounterDoctrineName = "sin doctrina";
        public float LastCounterDoctrineMultiplier = 1f;
        public float CounterMarkerRadius;
        public bool CounterMarkerVisible { get { return markerRoot != null && markerRoot.activeSelf; } }

        private const float CounterDuration = 70f;
        private readonly List<V5CellEntity> spawnedCounterThreats = new List<V5CellEntity>(6);
        private float tick;
        private float counterTimer;
        private float spawnCooldown;
        private float markerPulse;
        private GameObject markerRoot;
        private SpriteRenderer markerZone;
        private SpriteRenderer markerRing;
        private SpriteRenderer markerCore;
        private GUIStyle markerLabelStyle;
        private static Sprite markerCircleSprite;
        private static Sprite markerRingSprite;

        public void ResetForNewRun()
        {
            ActiveCounter = false;
            ActiveCounterRoute = V5MvpRoute.None;
            LastCounterBranch = V5RouteBranchId.None;
            CounterCenter = Vector2.zero;
            CounterTimeLeft = 0f;
            LastCounterPressure = 0f;
            CounterEventCount = 0;
            CounterAnsweredCount = 0;
            BacteriaCounters = 0;
            AmoebaCounters = 0;
            ProducerCounters = 0;
            VolvoxCounters = 0;
            BacteriaCounterAnswers = 0;
            AmoebaCounterAnswers = 0;
            ProducerCounterAnswers = 0;
            VolvoxCounterAnswers = 0;
            LastCounterName = "sin contra-presion de ruta";
            LastCounterSummary = "Contra-presion MVP: latente.";
            LastCounterAdvice = "La ecologia aun no responde a tu ruta.";
            LastCounterplayResult = "sin counterplay de ruta";
            LastCounterVisualCue = "sin marcador de contra-presion";
            LastCounterBranchName = "sin rama";
            LastCounterDoctrine = V5BranchDoctrineChoice.None;
            LastCounterDoctrineName = "sin doctrina";
            LastCounterDoctrineMultiplier = 1f;
            CounterMarkerRadius = 0f;
            spawnedCounterThreats.Clear();
            HideCounterMarker("marcador reset");
            tick = 0f;
            counterTimer = 0f;
            spawnCooldown = 42f;
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;

            tick += Time.deltaTime;
            if (ActiveCounter)
            {
                counterTimer += Time.deltaTime;
                CounterTimeLeft = Mathf.Max(0f, CounterDuration - counterTimer);
                ApplyCounterDrip(gm);
                if (counterTimer >= CounterDuration)
                {
                    ActiveCounter = false;
                    LastCounterplayResult = "Contra-presion no respondida: " + LastCounterName + ".";
                    HideCounterMarker("Expirado: " + LastCounterName);
                    RebuildSummary();
                }
                else
                {
                    UpdateCounterMarkerVisual();
                }
            }

            if (tick < 1.0f) return;
            tick = 0f;
            spawnCooldown = Mathf.Max(0f, spawnCooldown - 1f);
            LastCounterPressure = CounterPressure01(gm, ActiveRoute(gm));
            if (!ActiveCounter && spawnCooldown <= 0f && ShouldSpawnCounter(gm, LastCounterPressure))
            {
                TriggerCounterPressure(ActiveRoute(gm), gm, false);
            }
            else if (!ActiveCounter)
            {
                RebuildSummary();
            }
        }

        public bool ForceCounterPressureNow(V5MvpRoute route)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return false;
            if (route == V5MvpRoute.None) route = ActiveRoute(gm);
            return TriggerCounterPressure(route, gm, true);
        }

        public bool RegisterRouteAbilityCounterplay(V5MvpRoute route, Vector2 castCenter, float power, string abilityLabel, V5GameManager gm = null)
        {
            if (gm == null) gm = V5GameManager.Instance;
            if (gm == null || !ActiveCounter || route == V5MvpRoute.None || route != ActiveCounterRoute) return false;
            if (Vector2.Distance(castCenter, CounterCenter) > 10.5f) return false;

            float p = Mathf.Max(0.8f, power);
            ResolveCounterEnvironment(route, gm, p);
            DamageCounterThreats(CounterCenter, 7.0f + p, 20f * p);
            RewardCounterplay(route, gm, p);

            CounterAnsweredCount++;
            IncrementRouteCounterAnswer(route);
            ActiveCounter = false;
            CounterTimeLeft = 0f;
            LastCounterplayResult = "Counterplay " + V5MvpCanon.DisplayName(route) + ": " +
                                    (string.IsNullOrEmpty(abilityLabel) ? "habilidad de ruta" : abilityLabel) +
                                    " neutralizo " + LastCounterName + BranchSuffix() + ".";
            HideCounterMarker("Neutralizado: " + LastCounterName);
            if (gm.AffinityLog != null)
                gm.AffinityLog.AddEvent(RouteToAffinityPath(route), 8f + p * 2f, "counterplay " + LastCounterName, "route_counter");
            if (gm.Codex != null)
                gm.Codex.Unlock("Counterplay MVP: " + V5MvpCanon.DisplayName(route), LastCounterplayResult);
            if (gm.Hud != null) gm.Hud.Toast(LastCounterplayResult);
            RebuildSummary();
            return true;
        }

        private bool TriggerCounterPressure(V5MvpRoute route, V5GameManager gm, bool forced)
        {
            if (route == V5MvpRoute.None || gm == null || gm.Environment == null) return false;
            ActiveCounter = true;
            ActiveCounterRoute = route;
            LastCounterBranch = ResolveBranch(route, gm);
            LastCounterBranchName = BranchName(gm, LastCounterBranch);
            ResolveDoctrine(gm);
            CounterCenter = CounterPoint(gm, route);
            CounterTimeLeft = CounterDuration;
            counterTimer = 0f;
            spawnCooldown = forced ? 24f : Mathf.Lerp(110f, 55f, LastCounterPressure);
            spawnedCounterThreats.Clear();

            CounterEventCount++;
            IncrementRouteCounter(route);
            ApplyCounterStart(route, gm);
            ApplyCounterDoctrineModifier(gm);
            ShowCounterMarker(route);
            RebuildSummary();
            if (gm.Hud != null) gm.Hud.Toast(LastCounterSummary);
            return true;
        }

        private void ApplyCounterStart(V5MvpRoute route, V5GameManager gm)
        {
            if (ApplyBranchCounterStart(route, gm)) return;

            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    LastCounterName = "Fago litico";
                    LastCounterAdvice = "Responde con Marea de Biofilm o mueve la colonia fuera de toxinas.";
                    gm.Environment.ModifyArea(CounterCenter, 6.0f, -0.030f, 0f, -0.015f, 0.150f, 0.040f, -0.080f, 0.030f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Flagellate, CounterCenter + Random.insideUnitCircle * 3.2f, 1.25f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Bacteria, CounterCenter + Random.insideUnitCircle * 3.2f, 0.85f, true);
                    break;
                case V5MvpRoute.Amoeba:
                    LastCounterName = "Presa espinosa";
                    LastCounterAdvice = "Usa Fagocitosis Alfa cerca de la presa o cambia a defensa antes de perseguir.";
                    gm.Environment.ModifyArea(CounterCenter, 5.6f, -0.015f, 0f, 0f, 0.035f, 0f, -0.015f, -0.080f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Ciliate, CounterCenter + Random.insideUnitCircle * 3.0f, 1.20f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Nematode, CounterCenter + Random.insideUnitCircle * 3.0f, 1.10f, true);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    LastCounterName = "Sombra toxica";
                    LastCounterAdvice = "Usa Bloom Fotosintetico para reabrir luz/O2 o abandona la zona sombreada.";
                    gm.Environment.ModifyArea(CounterCenter, 7.2f, -0.010f, -0.260f, -0.160f, 0.100f, 0.010f, -0.030f, 0.010f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Rotifer, CounterCenter + Random.insideUnitCircle * 3.4f, 1.05f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Flagellate, CounterCenter + Random.insideUnitCircle * 3.4f, 1.05f, true);
                    break;
                case V5MvpRoute.Volvox:
                    LastCounterName = "Cizalla colonial";
                    LastCounterAdvice = "Usa Sincronia Volvox para reparar cuerpo y fijar defensa.";
                    gm.Environment.ModifyArea(CounterCenter, 6.4f, -0.010f, -0.020f, -0.030f, 0.060f, 0f, -0.070f, 0.015f);
                    StressBody(gm, 5.5f, 8f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Rotifer, CounterCenter + Random.insideUnitCircle * 3.2f, 1.15f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Nematode, CounterCenter + Random.insideUnitCircle * 3.2f, 1.05f, true);
                    break;
            }
        }

        private bool ApplyBranchCounterStart(V5MvpRoute route, V5GameManager gm)
        {
            if (LastCounterBranch == V5RouteBranchId.None || gm == null || gm.Environment == null) return false;

            switch (LastCounterBranch)
            {
                case V5RouteBranchId.BacteriaBiofilm:
                    LastCounterName = "Fago de biofilm";
                    LastCounterAdvice = "Tu biofilm atrae lisis local: responde con Marea de Biofilm dentro del marcador.";
                    gm.Environment.ModifyArea(CounterCenter, 6.2f, -0.025f, 0f, -0.012f, 0.170f, 0.030f, -0.105f, 0.040f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Bacteria, CounterCenter + Random.insideUnitCircle * 3.2f, 1.10f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Flagellate, CounterCenter + Random.insideUnitCircle * 3.2f, 1.05f, true);
                    return true;
                case V5RouteBranchId.BacteriaSwarm:
                    LastCounterName = "Fago movil";
                    LastCounterAdvice = "Tu swarm recibe persecucion: usa Marea de Biofilm cerca del marcador o dispersa celulas.";
                    gm.Environment.ModifyArea(CounterCenter, 6.0f, -0.020f, 0f, -0.008f, 0.120f, 0.018f, -0.060f, 0.025f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Flagellate, CounterCenter + Random.insideUnitCircle * 3.4f, 1.35f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Flagellate, CounterCenter + Random.insideUnitCircle * 3.4f, 1.18f, true);
                    return true;
                case V5RouteBranchId.AmoebaHunter:
                    LastCounterName = "Presa blindada";
                    LastCounterAdvice = "Tu caza subio la defensa de presas: entra con Fagocitosis Alfa y evita perseguir sin energia.";
                    gm.Environment.ModifyArea(CounterCenter, 5.7f, -0.010f, 0f, 0f, 0.045f, 0f, -0.020f, -0.060f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Nematode, CounterCenter + Random.insideUnitCircle * 3.0f, 1.32f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Ciliate, CounterCenter + Random.insideUnitCircle * 3.0f, 1.12f, true);
                    return true;
                case V5RouteBranchId.AmoebaDigestive:
                    LastCounterName = "Detritus infectado";
                    LastCounterAdvice = "Tu metabolismo atrae toxinas: limpia con Fagocitosis Alfa y convierte la zona en comida segura.";
                    gm.Environment.ModifyArea(CounterCenter, 5.5f, -0.020f, 0f, -0.006f, 0.095f, 0.010f, -0.010f, 0.160f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Bacteria, CounterCenter + Random.insideUnitCircle * 2.8f, 1.18f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Ciliate, CounterCenter + Random.insideUnitCircle * 3.0f, 1.05f, true);
                    return true;
                case V5RouteBranchId.ProducerBloom:
                    LastCounterName = "Eclipse toxico";
                    LastCounterAdvice = "Tu bloom provoco sombra hostil: usa Bloom Fotosintetico para recuperar luz y oxigeno.";
                    gm.Environment.ModifyArea(CounterCenter, 7.8f, -0.006f, -0.300f, -0.190f, 0.115f, 0.008f, -0.025f, 0.008f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Rotifer, CounterCenter + Random.insideUnitCircle * 3.5f, 1.12f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Flagellate, CounterCenter + Random.insideUnitCircle * 3.5f, 1.08f, true);
                    return true;
                case V5RouteBranchId.ProducerTerraformer:
                    LastCounterName = "Reflujo acido";
                    LastCounterAdvice = "Tu terraformacion encontro suelo recalcitrante: responde con Bloom Fotosintetico y estabiliza pH/toxinas.";
                    gm.Environment.ModifyArea(CounterCenter, 7.0f, -0.015f, -0.080f, -0.070f, 0.155f, 0.180f, -0.040f, 0.035f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Bacteria, CounterCenter + Random.insideUnitCircle * 3.2f, 1.25f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Rotifer, CounterCenter + Random.insideUnitCircle * 3.2f, 1.02f, true);
                    return true;
                case V5RouteBranchId.VolvoxBody:
                    LastCounterName = "Cizalla corporal";
                    LastCounterAdvice = "Tu cuerpo colonial recibe tension mecanica: usa Sincronia Volvox dentro del marcador.";
                    gm.Environment.ModifyArea(CounterCenter, 6.5f, -0.010f, -0.018f, -0.026f, 0.060f, 0f, -0.080f, 0.018f);
                    StressBody(gm, 5.8f, 10f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Rotifer, CounterCenter + Random.insideUnitCircle * 3.2f, 1.20f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Nematode, CounterCenter + Random.insideUnitCircle * 3.2f, 1.08f, true);
                    return true;
                case V5RouteBranchId.VolvoxCaste:
                    LastCounterName = "Parasito de castas";
                    LastCounterAdvice = "Tu division funcional atrae parasitos: sincroniza Volvox para proteger castas especializadas.";
                    gm.Environment.ModifyArea(CounterCenter, 6.2f, -0.012f, -0.010f, -0.018f, 0.075f, 0f, -0.055f, 0.030f);
                    StressNonMotherCells(gm, 6.0f, 7f);
                    SpawnCounterThreat(gm, V5EvolutionPath.Ciliate, CounterCenter + Random.insideUnitCircle * 3.0f, 1.22f, true);
                    SpawnCounterThreat(gm, V5EvolutionPath.Nematode, CounterCenter + Random.insideUnitCircle * 3.0f, 1.12f, true);
                    return true;
            }

            return false;
        }

        private void ApplyCounterDrip(V5GameManager gm)
        {
            if (gm == null || gm.Environment == null || ActiveCounterRoute == V5MvpRoute.None) return;
            float dt = Time.deltaTime;
            switch (ActiveCounterRoute)
            {
                case V5MvpRoute.Bacteria:
                    gm.Environment.ModifyArea(CounterCenter, 4.8f, -0.0015f * dt, 0f, 0f, 0.006f * dt, 0.001f * dt, -0.003f * dt, 0f);
                    break;
                case V5MvpRoute.Amoeba:
                    gm.Environment.ModifyArea(CounterCenter, 4.4f, -0.001f * dt, 0f, 0f, 0.0015f * dt, 0f, 0f, -0.0035f * dt);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    gm.Environment.ModifyArea(CounterCenter, 5.6f, 0f, -0.0045f * dt, -0.003f * dt, 0.002f * dt, 0f, 0f, 0f);
                    break;
                case V5MvpRoute.Volvox:
                    StressBody(gm, 4.8f, 0.55f * dt);
                    gm.Environment.ModifyArea(CounterCenter, 4.8f, 0f, 0f, -0.001f * dt, 0.0015f * dt, 0f, -0.0025f * dt, 0f);
                    break;
            }
        }

        private void ResolveCounterEnvironment(V5MvpRoute route, V5GameManager gm, float power)
        {
            if (gm == null || gm.Environment == null) return;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    gm.Environment.ModifyArea(CounterCenter, 6.2f, 0.020f * power, 0f, 0.010f, -0.160f * power, -0.030f, 0.110f * power, 0.010f);
                    break;
                case V5MvpRoute.Amoeba:
                    gm.Environment.ModifyArea(CounterCenter, 5.8f, 0.025f * power, 0f, 0.005f, -0.050f * power, 0f, 0.015f, 0.120f * power);
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    gm.Environment.ModifyArea(CounterCenter, 7.5f, 0.012f, 0.250f * power, 0.200f * power, -0.120f * power, -0.010f, 0.040f * power, 0f);
                    break;
                case V5MvpRoute.Volvox:
                    gm.Environment.ModifyArea(CounterCenter, 6.4f, 0.012f, 0.020f, 0.050f * power, -0.070f * power, 0f, 0.120f * power, 0f);
                    break;
            }
        }

        private void RewardCounterplay(V5MvpRoute route, V5GameManager gm, float power)
        {
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;
            if (LastCounterDoctrine == V5BranchDoctrineChoice.Stabilize)
            {
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 2f * power);
            }
            else if (LastCounterDoctrine == V5BranchDoctrineChoice.Radicalize)
            {
                mother.Resources.atp += 4f * power;
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 1f * power);
            }

            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    mother.Resources.biomass += 14f * power;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 5f * power);
                    break;
                case V5MvpRoute.Amoeba:
                    mother.Resources.aminoAcids += 14f * power;
                    mother.Stats.physicalDamagePerSecond += 0.035f * power;
                    break;
                case V5MvpRoute.PhotosyntheticProducer:
                    mother.Resources.atp += 24f * power;
                    mother.Resources.minerals += 6f * power;
                    break;
                case V5MvpRoute.Volvox:
                    mother.Resources.lipids += 10f * power;
                    mother.Resources.nucleotides += 7f * power;
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 6f * power);
                    break;
            }
        }

        private bool ShouldSpawnCounter(V5GameManager gm, float pressure)
        {
            if (gm == null || ActiveRoute(gm) == V5MvpRoute.None) return false;
            if (gm.ElapsedSeconds < 180f && pressure < 0.55f) return false;
            float chance = Mathf.Lerp(0.02f, 0.18f, Mathf.Clamp01((pressure - 0.25f) / 0.75f));
            return pressure >= 0.28f && Random.value < chance;
        }

        private float CounterPressure01(V5GameManager gm, V5MvpRoute route)
        {
            if (gm == null || route == V5MvpRoute.None) return 0f;
            float build = V5MvpCanon.BuildProgress01(route, gm.Adaptations);
            float mastery = gm.RouteMastery != null ? gm.RouteMastery.Mastery01(route) : 0f;
            float climax = gm.RouteClimax != null ? gm.RouteClimax.ClimaxScore01 : 0f;
            float colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float threat = gm.Director != null ? gm.Director.ThreatLevel : 0f;
            float doctrine = 0f;
            if (gm.RouteBranches != null)
            {
                V5RouteBranchId branch = ResolveBranch(route, gm);
                doctrine = gm.RouteBranches.DoctrinePressureForBranch(branch);
            }
            return Mathf.Clamp01(build * 0.28f + mastery * 0.20f + climax * 0.22f + colonization * 0.12f + threat * 0.12f + doctrine * 0.06f);
        }

        private Vector2 CounterPoint(V5GameManager gm, V5MvpRoute route)
        {
            Vector2 origin = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;
            if (gm != null && gm.WorldEvents != null && gm.WorldEvents.RouteOpportunityActive && gm.WorldEvents.ActiveRouteOpportunity == route)
                origin = gm.WorldEvents.RouteOpportunityCenter;

            Vector2 dir;
            switch (route)
            {
                case V5MvpRoute.Bacteria: dir = new Vector2(0.72f, -0.46f); break;
                case V5MvpRoute.Amoeba: dir = new Vector2(-0.70f, 0.38f); break;
                case V5MvpRoute.PhotosyntheticProducer: dir = new Vector2(0.40f, -0.92f); break;
                case V5MvpRoute.Volvox: dir = new Vector2(0.78f, 0.62f); break;
                default: dir = Vector2.right; break;
            }
            Vector2 pos = origin + (dir + Random.insideUnitCircle * 0.16f).normalized * 3.2f;
            V5EnvironmentGrid env = gm != null ? gm.Environment : null;
            if (env != null && pos.magnitude > env.MapRadius * 0.86f) pos = pos.normalized * env.MapRadius * 0.86f;
            return pos;
        }

        private V5CellEntity SpawnCounterThreat(V5GameManager gm, V5EvolutionPath path, Vector2 pos, float strength, bool hostile)
        {
            if (gm == null || gm.CellFactory == null) return null;
            V5EnvironmentGrid env = gm.Environment;
            if (env != null && pos.magnitude > env.MapRadius * 0.90f) pos = pos.normalized * env.MapRadius * 0.82f;
            V5CellEntity enemy = gm.CellFactory.SpawnNeutral(pos, path);
            if (enemy == null) return null;
            enemy.name = "V5_COUNTER_" + LastCounterName + "_" + path;
            float doctrineStrength = strength * Mathf.Max(0.75f, LastCounterDoctrineMultiplier);
            enemy.Stats.maxHp *= Mathf.Max(0.75f, doctrineStrength);
            enemy.Stats.currentHp = enemy.Stats.maxHp;
            enemy.Stats.speed *= Mathf.Lerp(0.95f, 1.30f, Mathf.Clamp01(doctrineStrength - 0.8f));
            enemy.Stats.sensorRange += 2.5f * doctrineStrength;
            enemy.Stats.physicalDamagePerSecond += 0.8f * doctrineStrength;
            enemy.Stats.chemicalDamagePerSecond += path == V5EvolutionPath.Bacteria ? 0.9f * doctrineStrength : 0f;
            enemy.Directive = hostile ? V5Directive.Attack : V5Directive.Colonize;
            if (hostile && enemy.GetComponent<V5EnemyBrain>() == null) enemy.gameObject.AddComponent<V5EnemyBrain>();
            spawnedCounterThreats.Add(enemy);
            return enemy;
        }

        private void DamageCounterThreats(Vector2 center, float radius, float damage)
        {
            for (int i = 0; i < spawnedCounterThreats.Count; i++)
            {
                V5CellEntity enemy = spawnedCounterThreats[i];
                if (enemy == null) continue;
                float dist = Vector2.Distance(center, enemy.transform.position);
                if (dist > radius) continue;
                enemy.Damage(damage * Mathf.Lerp(1f, 0.35f, dist / Mathf.Max(0.01f, radius)), V5DamageKind.Physical, center);
            }
        }

        private void StressBody(V5GameManager gm, float radius, float stress)
        {
            if (gm == null || gm.PlayerCells == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                if (!c.IsAttachedToBody && c.Role != V5CellRole.Mother) continue;
                if (Vector2.Distance(c.transform.position, CounterCenter) > radius) continue;
                c.Stats.stress = Mathf.Min(100f, c.Stats.stress + stress);
            }
        }

        private void StressNonMotherCells(V5GameManager gm, float radius, float stress)
        {
            if (gm == null || gm.PlayerCells == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (Vector2.Distance(c.transform.position, CounterCenter) > radius) continue;
                c.Stats.stress = Mathf.Min(100f, c.Stats.stress + stress);
            }
        }

        private V5RouteBranchId ResolveBranch(V5MvpRoute route, V5GameManager gm)
        {
            if (gm == null || gm.RouteBranches == null || route == V5MvpRoute.None) return V5RouteBranchId.None;
            gm.RouteBranches.EvaluateNow(gm);
            return gm.RouteBranches.BranchForRoute(route);
        }

        private void ResolveDoctrine(V5GameManager gm)
        {
            LastCounterDoctrine = V5BranchDoctrineChoice.None;
            LastCounterDoctrineName = "sin doctrina";
            LastCounterDoctrineMultiplier = 1f;
            if (gm == null || gm.RouteBranches == null || LastCounterBranch == V5RouteBranchId.None) return;

            LastCounterDoctrine = gm.RouteBranches.DoctrineForBranch(LastCounterBranch);
            LastCounterDoctrineName = gm.RouteBranches.DoctrineNameForBranch(LastCounterBranch);
            LastCounterDoctrineMultiplier = gm.RouteBranches.DoctrineCounterMultiplierForBranch(LastCounterBranch);
        }

        private void ApplyCounterDoctrineModifier(V5GameManager gm)
        {
            if (gm == null || gm.RouteBranches == null || LastCounterDoctrine == V5BranchDoctrineChoice.None) return;

            LastCounterName = gm.RouteBranches.DoctrineCounterNameForBranch(LastCounterBranch, LastCounterName);
            string advice = gm.RouteBranches.DoctrineCounterAdviceForBranch(LastCounterBranch);
            if (!string.IsNullOrEmpty(advice)) LastCounterAdvice += " " + advice;

            LastCounterPressure = Mathf.Clamp01(Mathf.Max(LastCounterPressure, 0.42f) * LastCounterDoctrineMultiplier);
            if (LastCounterDoctrine == V5BranchDoctrineChoice.Radicalize)
            {
                CounterTimeLeft = Mathf.Max(38f, CounterTimeLeft - 8f);
                gm.Environment.ModifyArea(CounterCenter, CounterRadius(ActiveCounterRoute) + 0.7f, -0.006f, -0.010f, -0.006f, 0.035f, 0.010f, -0.015f, 0.010f);
            }
            else if (LastCounterDoctrine == V5BranchDoctrineChoice.Stabilize)
            {
                CounterTimeLeft = Mathf.Min(CounterDuration + 8f, CounterTimeLeft + 6f);
                gm.Environment.ModifyArea(CounterCenter, CounterRadius(ActiveCounterRoute), -0.003f, 0f, 0.002f, 0.018f, 0.002f, -0.006f, 0.004f);
            }
        }

        private string BranchName(V5GameManager gm, V5RouteBranchId branch)
        {
            if (gm != null && gm.RouteBranches != null) return gm.RouteBranches.BranchName(branch);
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

        private string BranchSuffix()
        {
            string suffix = LastCounterBranch != V5RouteBranchId.None ? " [" + LastCounterBranchName + "]" : "";
            if (LastCounterDoctrine != V5BranchDoctrineChoice.None) suffix += " <" + LastCounterDoctrineName + ">";
            return suffix;
        }

        private V5MvpRoute ActiveRoute(V5GameManager gm)
        {
            if (gm == null) return V5MvpRoute.None;
            if (gm.MvpIntent != null) return gm.MvpIntent.EffectiveRoute(gm);
            return V5MvpCanon.CurrentRoute(gm);
        }

        private void IncrementRouteCounter(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaCounters++; break;
                case V5MvpRoute.Amoeba: AmoebaCounters++; break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerCounters++; break;
                case V5MvpRoute.Volvox: VolvoxCounters++; break;
            }
        }

        public int AnsweredCountForRoute(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return BacteriaCounterAnswers;
                case V5MvpRoute.Amoeba: return AmoebaCounterAnswers;
                case V5MvpRoute.PhotosyntheticProducer: return ProducerCounterAnswers;
                case V5MvpRoute.Volvox: return VolvoxCounterAnswers;
                default: return 0;
            }
        }

        private void IncrementRouteCounterAnswer(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: BacteriaCounterAnswers++; break;
                case V5MvpRoute.Amoeba: AmoebaCounterAnswers++; break;
                case V5MvpRoute.PhotosyntheticProducer: ProducerCounterAnswers++; break;
                case V5MvpRoute.Volvox: VolvoxCounterAnswers++; break;
            }
        }

        private void RebuildSummary()
        {
            if (ActiveCounter)
            {
                LastCounterSummary = "Contra " + V5MvpCanon.DisplayName(ActiveCounterRoute) + ": " +
                                     LastCounterName + BranchSuffix() + " " + CounterTimeLeft.ToString("0") + "s | " + LastCounterAdvice;
            }
            else
            {
                LastCounterSummary = "Contra-presion MVP " + (LastCounterPressure * 100f).ToString("0") +
                                     "% | eventos " + CounterEventCount +
                                     " | respondidas " + CounterAnsweredCount +
                                     " | " + LastCounterplayResult;
            }
        }

        private void ShowCounterMarker(V5MvpRoute route)
        {
            EnsureCounterMarker();
            if (markerRoot == null) return;

            Color color = RouteMarkerColor(route);
            CounterMarkerRadius = CounterRadiusForDoctrine(route);
            markerRoot.SetActive(true);
            markerRoot.transform.position = new Vector3(CounterCenter.x, CounterCenter.y, -0.05f);
            markerPulse = 0f;
            markerZone.color = new Color(color.r, color.g, color.b, 0.16f);
            markerRing.color = new Color(color.r, color.g, color.b, 0.78f);
            markerCore.color = new Color(1f, 0.95f, 0.58f, 0.85f);
            UpdateCounterMarkerVisual();

            LastCounterVisualCue = "Marcador " + V5MvpCanon.DisplayName(route) + BranchSuffix() + ": " + LastCounterName + " en zona de respuesta.";
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null)
            {
                feedback.PushFloating("Contra: " + LastCounterName + BranchSuffix(), CounterCenter + Vector2.up * 1.2f, color);
                feedback.Ping("warning");
            }
        }

        private void HideCounterMarker(string cue)
        {
            if (!string.IsNullOrEmpty(cue)) LastCounterVisualCue = cue;
            CounterMarkerRadius = 0f;
            if (markerRoot != null) markerRoot.SetActive(false);
        }

        private void UpdateCounterMarkerVisual()
        {
            if (markerRoot == null || !markerRoot.activeSelf) return;
            markerPulse += Time.deltaTime;
            markerRoot.transform.position = new Vector3(CounterCenter.x, CounterCenter.y, -0.05f);
            float urgency = Mathf.Clamp01(1f - CounterTimeLeft / CounterDuration);
            float pulse = 1f + Mathf.Sin(markerPulse * Mathf.Lerp(3.4f, 6.4f, urgency)) * Mathf.Lerp(0.04f, 0.10f, urgency);
            float radius = Mathf.Max(1f, CounterMarkerRadius);

            markerZone.transform.localScale = Vector3.one * radius * 2.0f * (1.02f + urgency * 0.08f);
            markerRing.transform.localScale = Vector3.one * radius * 2.0f * pulse;
            markerCore.transform.localScale = Vector3.one * Mathf.Lerp(0.38f, 0.72f, 0.5f + Mathf.Sin(markerPulse * 5.0f) * 0.5f);

            Color ring = markerRing.color;
            ring.a = Mathf.Lerp(0.58f, 0.95f, urgency);
            markerRing.color = ring;
        }

        private void EnsureCounterMarker()
        {
            if (markerRoot != null) return;
            if (markerCircleSprite == null) markerCircleSprite = V5ProceduralSprites.CreateCircleSprite(96);
            if (markerRingSprite == null) markerRingSprite = V5ProceduralSprites.CreateRingSprite(128, 0.12f);

            markerRoot = new GameObject("V5_RouteCounterMarker");
            markerRoot.transform.SetParent(transform, false);
            markerZone = CreateMarkerRenderer("CounterZone", markerCircleSprite, 4);
            markerRing = CreateMarkerRenderer("CounterRing", markerRingSprite, 18);
            markerCore = CreateMarkerRenderer("CounterCore", markerCircleSprite, 19);
            markerRoot.SetActive(false);
        }

        private SpriteRenderer CreateMarkerRenderer(string childName, Sprite sprite, int sortingOrder)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(markerRoot.transform, false);
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private float CounterRadius(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return 6.0f;
                case V5MvpRoute.Amoeba: return 5.6f;
                case V5MvpRoute.PhotosyntheticProducer: return 7.2f;
                case V5MvpRoute.Volvox: return 6.4f;
                default: return 5.8f;
            }
        }

        private float CounterRadiusForDoctrine(V5MvpRoute route)
        {
            float radius = CounterRadius(route);
            if (LastCounterDoctrine == V5BranchDoctrineChoice.Radicalize) return radius * 1.10f;
            if (LastCounterDoctrine == V5BranchDoctrineChoice.Stabilize) return radius * 0.96f;
            return radius;
        }

        public Color RouteMarkerColor(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return new Color(0.55f, 1f, 0.68f, 1f);
                case V5MvpRoute.Amoeba: return new Color(1f, 0.62f, 0.86f, 1f);
                case V5MvpRoute.PhotosyntheticProducer: return new Color(0.90f, 1f, 0.42f, 1f);
                case V5MvpRoute.Volvox: return new Color(0.58f, 0.90f, 1f, 1f);
                default: return new Color(1f, 0.86f, 0.42f, 1f);
            }
        }

        private string CounterplayHint(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return "E: Marea de Biofilm";
                case V5MvpRoute.Amoeba: return "E: Fagocitosis Alfa";
                case V5MvpRoute.PhotosyntheticProducer: return "E: Bloom Fotosintetico";
                case V5MvpRoute.Volvox: return "E: Sincronia Volvox";
                default: return "E: habilidad de ruta";
            }
        }

        private void OnGUI()
        {
            if (!ActiveCounter || Camera.main == null) return;
            if (markerLabelStyle == null)
            {
                markerLabelStyle = new GUIStyle(GUI.skin.box);
                markerLabelStyle.alignment = TextAnchor.MiddleCenter;
                markerLabelStyle.fontSize = 11;
                markerLabelStyle.fontStyle = FontStyle.Bold;
                markerLabelStyle.wordWrap = true;
                markerLabelStyle.normal.textColor = Color.white;
            }

            Vector3 sp = Camera.main.WorldToScreenPoint(CounterCenter + Vector2.up * (Mathf.Max(1f, CounterMarkerRadius) + 0.9f));
            if (sp.z < 0f) return;
            float w = 280f;
            float h = 54f;
            float x = Mathf.Clamp(sp.x - w * 0.5f, 8f, Screen.width - w - 8f);
            float y = Mathf.Clamp(Screen.height - sp.y - h * 0.5f, 48f, Screen.height - h - 12f);
            Color old = GUI.color;
            Color c = RouteMarkerColor(ActiveCounterRoute);
            GUI.color = new Color(c.r, c.g, c.b, 0.92f);
            GUI.Box(new Rect(x, y, w, h), LastCounterName + BranchSuffix() + "  " + CounterTimeLeft.ToString("0") + "s\n" + CounterplayHint(ActiveCounterRoute), markerLabelStyle);
            GUI.color = old;
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
