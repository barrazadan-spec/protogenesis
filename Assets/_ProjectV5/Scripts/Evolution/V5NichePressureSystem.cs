using UnityEngine;

namespace Protogenesis.V5
{
    public class V5NichePressureSystem : MonoBehaviour, IV5RunResettable
    {
        private const float TickSeconds = 1.25f;

        public V5MvpRoute CurrentRoute = V5MvpRoute.None;
        public float CurrentScore;
        public string CurrentBand = "Sin ruta";
        public string Summary = "Nicho: sin ruta MVP.";
        public string LastNicheAdvice = "Elige una ruta MVP para leer el nicho.";
        public string LastNicheEffect = "Sin efecto de nicho.";
        public int FavorablePulseCount;
        public int StressPulseCount;

        private float tickTimer;

        public void ResetForNewRun()
        {
            CurrentRoute = V5MvpRoute.None;
            CurrentScore = 0f;
            CurrentBand = "Sin ruta";
            Summary = "Nicho: sin ruta MVP.";
            LastNicheAdvice = "Elige una ruta MVP para leer el nicho.";
            LastNicheEffect = "Sin efecto de nicho.";
            FavorablePulseCount = 0;
            StressPulseCount = 0;
            tickTimer = 0f;
        }

        private void Update()
        {
            tickTimer += Time.deltaTime;
            if (tickTimer < TickSeconds) return;
            tickTimer = 0f;
            Evaluate(V5GameManager.Instance, true);
        }

        public float ScanNow(V5GameManager gm)
        {
            Evaluate(gm, false);
            return CurrentScore;
        }

        public bool EvaluateAndApplyNow(V5GameManager gm)
        {
            return Evaluate(gm, true);
        }

        private bool Evaluate(V5GameManager gm, bool applyEffect)
        {
            if (gm == null || gm.MotherCell == null || gm.Environment == null)
            {
                ResetReadableState("Nicho: sin madre o ambiente.", "No hay lectura de nicho disponible.");
                return false;
            }

            V5MvpRoute route = gm.MvpIntent != null ? gm.MvpIntent.EffectiveRoute(gm) : V5MvpCanon.CurrentRoute(gm);
            if (route == V5MvpRoute.None)
            {
                ResetReadableState("Nicho: sin ruta MVP.", "Elige una ruta MVP para activar presion de nicho.");
                return false;
            }

            Vector2 pos = gm.MotherCell.transform.position;
            V5EnvironmentGrid env = gm.Environment;
            float nutrients = env.Sample(V5OverlayMode.Nutrients, pos);
            float light = env.Sample(V5OverlayMode.Light, pos);
            float oxygen = env.Sample(V5OverlayMode.Oxygen, pos);
            float toxins = env.Sample(V5OverlayMode.Toxins, pos);
            float acidity = env.Sample(V5OverlayMode.Acidity, pos);
            float colonization = env.Sample(V5OverlayMode.Colonization, pos);
            float temperature = env.Sample(V5OverlayMode.Temperature, pos);
            float detritus = DetritusAt(env, pos);
            float prey = NearbyPrey01(gm, pos, 8f);

            CurrentRoute = route;
            CurrentScore = ScoreRoute(route, nutrients, light, oxygen, toxins, acidity, colonization, temperature, detritus, prey);
            CurrentBand = Band(CurrentScore);
            LastNicheAdvice = Advice(route, nutrients, light, oxygen, toxins, acidity, colonization, temperature, detritus, prey);
            Summary = V5MvpCanon.DisplayName(route) + " nicho " + Percent(CurrentScore) + " " + CurrentBand +
                      " | luz " + Percent(light) + " O2 " + Percent(oxygen) + " tox " + Percent(toxins) +
                      " col " + Percent(colonization);

            if (applyEffect) ApplyNicheEffect(route, gm.MotherCell);
            return true;
        }

        private void ResetReadableState(string summary, string advice)
        {
            CurrentRoute = V5MvpRoute.None;
            CurrentScore = 0f;
            CurrentBand = "Sin ruta";
            Summary = summary;
            LastNicheAdvice = advice;
            LastNicheEffect = "Sin efecto de nicho.";
        }

        private float ScoreRoute(V5MvpRoute route, float nutrients, float light, float oxygen, float toxins, float acidity, float colonization, float temperature, float detritus, float prey)
        {
            float lowToxins = 1f - toxins;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return Mathf.Clamp01(nutrients * 0.24f +
                                         Mathf.Clamp01(colonization / 0.12f) * 0.24f +
                                         lowToxins * 0.18f +
                                         Optimal01(acidity, 0.50f, 0.32f) * 0.17f +
                                         Optimal01(temperature, 0.55f, 0.30f) * 0.17f);
                case V5MvpRoute.Amoeba:
                    return Mathf.Clamp01(prey * 0.28f +
                                         detritus * 0.22f +
                                         nutrients * 0.18f +
                                         lowToxins * 0.17f +
                                         Optimal01(temperature, 0.55f, 0.35f) * 0.15f);
                case V5MvpRoute.PhotosyntheticProducer:
                    return Mathf.Clamp01(light * 0.38f +
                                         oxygen * 0.22f +
                                         lowToxins * 0.16f +
                                         Optimal01(acidity, 0.48f, 0.30f) * 0.14f +
                                         nutrients * 0.10f);
                case V5MvpRoute.Volvox:
                    return Mathf.Clamp01(lowToxins * 0.24f +
                                         oxygen * 0.22f +
                                         Mathf.Clamp01(colonization / 0.10f) * 0.20f +
                                         Optimal01(acidity, 0.50f, 0.20f) * 0.18f +
                                         Optimal01(temperature, 0.52f, 0.24f) * 0.16f);
                default:
                    return 0f;
            }
        }

        private void ApplyNicheEffect(V5MvpRoute route, V5CellEntity mother)
        {
            if (mother == null) return;

            V5GameManager gm = V5GameManager.Instance;
            V5RouteMasterySystem mastery = gm != null ? gm.RouteMastery : null;
            float rewardMult = mastery != null ? mastery.NicheRewardMultiplier(route) : 1f;
            float pressureMult = mastery != null ? mastery.NicheStressMultiplier(route) : 1f;

            if (CurrentScore >= 0.72f)
            {
                FavorablePulseCount++;
                switch (route)
                {
                    case V5MvpRoute.Bacteria:
                        mother.Resources.biomass += 1.2f * rewardMult;
                        mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 0.55f * rewardMult);
                        LastNicheEffect = "Nicho favorable: biofilm convierte territorio en biomasa.";
                        break;
                    case V5MvpRoute.Amoeba:
                        mother.Resources.biomass += 0.8f * rewardMult;
                        mother.Resources.aminoAcids += 0.6f * rewardMult;
                        mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 0.35f * rewardMult);
                        LastNicheEffect = "Nicho favorable: presas/detritus sostienen caza.";
                        break;
                    case V5MvpRoute.PhotosyntheticProducer:
                        mother.Resources.atp += 1.8f * rewardMult;
                        mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 0.40f * rewardMult);
                        LastNicheEffect = "Nicho favorable: luz y O2 elevan ATP.";
                        break;
                    case V5MvpRoute.Volvox:
                        mother.Resources.atp += 0.9f * rewardMult;
                        mother.Resources.lipids += 0.5f * rewardMult;
                        mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 0.75f * rewardMult);
                        LastNicheEffect = "Nicho favorable: estabilidad ayuda cohesion colonial.";
                        break;
                }
            }
            else if (CurrentScore <= 0.28f)
            {
                StressPulseCount++;
                float pressure = Mathf.Lerp(0.35f, 1.35f, Mathf.Clamp01((0.28f - CurrentScore) / 0.28f)) * pressureMult;
                mother.Stats.stress = Mathf.Min(100f, mother.Stats.stress + pressure);
                LastNicheEffect = "Nicho hostil: la ruta elegida sufre presion ambiental.";
            }
            else
            {
                LastNicheEffect = "Nicho neutro: sin bonus ni presion fuerte.";
            }
        }

        private string Advice(V5MvpRoute route, float nutrients, float light, float oxygen, float toxins, float acidity, float colonization, float temperature, float detritus, float prey)
        {
            if (toxins > 0.45f) return "Toxinas altas: busca Catalasa/ROS o cambia de zona.";
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    if (colonization < 0.05f) return "Bacteria quiere territorio: usa Pared, Pili o modo Colonizar.";
                    if (nutrients < 0.30f) return "Bacteria necesita sustrato: mueve el swarm hacia nutrientes.";
                    return "Bacteria esta en buen nicho: expande biofilm y divide.";
                case V5MvpRoute.Amoeba:
                    if (prey < 0.20f) return "Ameba necesita presas: explora bordes con detritus o enemigos pequenos.";
                    if (detritus < 0.12f) return "Ameba escala mejor donde hay restos: fuerza contacto o caza.";
                    return "Ameba tiene nicho de caza: instala Lisosoma/Seudopodos.";
                case V5MvpRoute.PhotosyntheticProducer:
                    if (light < 0.45f) return "Productor necesita luz: sube hacia zona fotica.";
                    if (oxygen < 0.24f) return "Productor aun no transforma el medio: Tilacoide/Cloroplasto suben O2.";
                    return "Productor esta en claro luminoso: invierte en economia solar.";
                case V5MvpRoute.Volvox:
                    if (colonization < 0.035f) return "Volvox necesita zona estable: prepara Adesina y cuerpo.";
                    if (oxygen < 0.22f) return "Volvox agradece O2/luz: combina adhesion con cloroplasto.";
                    return "Volvox tiene nicho estable: crece cuerpo y castas.";
                default:
                    return "Elige una ruta MVP para leer nicho.";
            }
        }

        private float DetritusAt(V5EnvironmentGrid env, Vector2 world)
        {
            if (env == null || env.detritus == null) return 0f;
            int x, y;
            env.WorldToTile(world, out x, out y);
            return env.detritus[x, y];
        }

        private float NearbyPrey01(V5GameManager gm, Vector2 pos, float range)
        {
            if (gm == null || gm.NonPlayerCells == null) return 0f;
            int count = 0;
            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity c = gm.NonPlayerCells[i];
                if (c == null || c.Stats.currentHp <= 0f) continue;
                if (Vector2.Distance(pos, c.transform.position) <= range) count++;
            }
            return Mathf.Clamp01(count / 3f);
        }

        private float Optimal01(float value, float target, float tolerance)
        {
            return 1f - Mathf.Clamp01(Mathf.Abs(value - target) / Mathf.Max(0.01f, tolerance));
        }

        private string Band(float score)
        {
            if (score >= 0.72f) return "favorable";
            if (score <= 0.28f) return "hostil";
            return "neutro";
        }

        private string Percent(float value)
        {
            return (Mathf.Clamp01(value) * 100f).ToString("0") + "%";
        }
    }
}
