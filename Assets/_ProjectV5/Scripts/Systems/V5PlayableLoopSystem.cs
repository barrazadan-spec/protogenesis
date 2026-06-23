using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5PlayableLoopStage
    {
        StabilizeMetabolism,
        GatherResources,
        GrowSquad,
        UnlockAdhesion,
        AssignSquadRoles,
        ClaimHabitat,
        EvolveRoute,
        SurvivePressure,
        PushVictory
    }

    public class V5PlayableLoopSystem : MonoBehaviour
    {
        public V5PlayableLoopStage Stage { get; private set; }
        public string Goal { get; private set; }
        public string NextAction { get; private set; }
        public string LoopSummary { get; private set; }
        public float Progress01 { get; private set; }

        private V5PlayableLoopStage lastStage;
        private float tick;

        private void Awake()
        {
            Stage = V5PlayableLoopStage.StabilizeMetabolism;
            lastStage = Stage;
            Goal = "Elige la primera adaptacion.";
            NextAction = "Abre G y activa una adaptacion barata: Pared, Flagelo, Tilacoide, Bomba o Adesina basica.";
            LoopSummary = "Loop: primera adaptacion";
        }

        private void Update()
        {
            tick += Time.deltaTime;
            if (tick < 0.25f) return;
            tick = 0f;
            Evaluate();
        }

        public void RefreshNow()
        {
            tick = 0f;
            Evaluate();
        }

        private void Evaluate()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null)
            {
                Set(V5PlayableLoopStage.StabilizeMetabolism, 0f, "Crea una celula madre.", "Carga una escena V5 con bootstrap activo.");
                return;
            }

            V5CellEntity mother = gm.MotherCell;
            V5AdaptationSystem adaptations = gm.Adaptations;
            bool hasFirstAdaptation = adaptations != null && adaptations.ActiveCount() > 0;
            bool hasMetabolism = mother.Metabolism != V5MetabolismType.None ||
                                 (adaptations != null && (adaptations.Has(V5AdaptationId.ProkaryoticThylakoid) ||
                                                          adaptations.Has(V5AdaptationId.ProtonPump) ||
                                                          adaptations.Has(V5AdaptationId.Mitochondria) ||
                                                          adaptations.Has(V5AdaptationId.Chloroplast)));

            if (!hasFirstAdaptation)
            {
                if (gm.MvpIntent != null && gm.MvpIntent.EffectiveRoute(gm) != V5MvpRoute.None)
                {
                    Set(V5PlayableLoopStage.StabilizeMetabolism, 0f, "Inicia tu ruta MVP.", gm.MvpIntent.OpeningStepText(gm));
                }
                else
                {
                    string next = "Abre G: elige Bacteria, Ameba, Productor o Volvox. La ruta enfocara el arbol y el primer objetivo.";
                    Set(V5PlayableLoopStage.StabilizeMetabolism, 0f, "Elige una ruta MVP en Genoma.", next);
                }
                return;
            }

            float divisionAtp = V5Balance.DivisionCostATP(mother);
            float divisionBiomass = V5Balance.DivisionCostBiomass(mother);
            float economyProgress = Mathf.Min(
                mother.Resources.atp / Mathf.Max(1f, divisionAtp),
                mother.Resources.biomass / Mathf.Max(1f, divisionBiomass));
            if (gm.PlayerCellCount() <= 1 && economyProgress < 1f)
            {
                Set(V5PlayableLoopStage.GatherResources, Mathf.Clamp01(economyProgress), "Acumula recursos para la primera division.", "Mueve la madre a nodos cercanos o asigna Farm cuando tengas hijas.");
                return;
            }

            int playerCount = gm.PlayerCellCount();
            if (playerCount < 3)
            {
                Set(V5PlayableLoopStage.GrowSquad, Mathf.Clamp01((playerCount - 1f) / 2f), "Haz crecer una microescuadra de 3 celulas.", "Presiona D cuando la madre tenga ATP y biomasa suficientes.");
                return;
            }

            if (gm.Body != null && !gm.Body.AttachmentUnlocked)
            {
                bool canUnlockAdhesion = adaptations != null && adaptations.CanInstall(V5AdaptationId.BasicAdhesin, mother, out _);
                float progress = canUnlockAdhesion ? 0.85f : (hasMetabolism ? 0.55f : 0.35f);
                string next = canUnlockAdhesion ? "Abre G y activa Adesina basica para poder pegar hijas al cuerpo." :
                    "Farmea ATP/Biomasa: Adesina basica es barata y debe ser tu primer puente corporal.";
                Set(V5PlayableLoopStage.UnlockAdhesion, progress, "Desbloquea adhesina para formar cuerpo.", next);
                return;
            }

            int roleCells = CountDirective(gm, V5Directive.Farm) + CountDirective(gm, V5Directive.Explore) + CountDirective(gm, V5Directive.Defend) + CountDirective(gm, V5Directive.Colonize);
            float selectedSquad = gm.Squads != null ? Mathf.InverseLerp(0f, 3f, gm.Squads.SelectedCount) : 0f;
            float directedSquad = Mathf.InverseLerp(0f, 2f, roleCells);
            float squadProgress = Mathf.Clamp01(Mathf.Max(selectedSquad, directedSquad));
            if (squadProgress < 1f)
            {
                Set(V5PlayableLoopStage.AssignSquadRoles, squadProgress, "Convierte celulas sueltas en escuadras utiles.", "Selecciona 2+ hijas y asigna 2 Farm, 4 Explore, 5 Colonize o 3 Defend.");
                return;
            }

            float colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float reveal = gm.Fog != null ? gm.Fog.DiscoveredPercent : 0f;
            float habitatProgress = Mathf.Max(colonization / 0.12f, reveal / 0.25f);
            if (habitatProgress < 1f)
            {
                Set(V5PlayableLoopStage.ClaimHabitat, Mathf.Clamp01(habitatProgress), "Reclama habitat dentro del campo vivo.", "Usa Colonize en zonas nutritivas y Tab para leer luz, oxigeno, toxinas y acidez.");
                return;
            }

            bool evolvedRoute = V5EvolutionRoster.IsPrimaryRoute(mother.EvolutionPath) || (gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA);
            float adaptationProgress = adaptations != null ? Mathf.Clamp01(adaptations.ActiveCount() / 3f) : 0f;
            float routeProgress = Mathf.Max(evolvedRoute ? 1f : 0f, adaptationProgress);
            if (routeProgress < 1f)
            {
                Set(V5PlayableLoopStage.EvolveRoute, routeProgress, "Consolida una identidad biologica jugable.", "Abre G y combina adaptaciones: Bacteria, Arquea, Cianobacteria, Ameba, Ciliado, Microalga, Hongo o Moho.");
                return;
            }

            float threat = gm.Director != null ? gm.Director.ThreatLevel : 0f;
            float toxins = gm.Environment != null ? gm.Environment.AverageToxins() : 0f;
            float stress = mother.Stats.stress / 100f;
            float pressure = Mathf.Clamp01(Mathf.Max(threat, toxins, stress));
            bool pressureHandled = pressure < 0.55f && gm.NonPlayerCells.Count <= playerCount + 4;
            if (!pressureHandled)
            {
                Set(V5PlayableLoopStage.SurvivePressure, 1f - pressure, "Sobrevive la presion ecologica.", "Defiende la madre, elimina amenazas cercanas o detoxifica antes de expandirte mas.");
                return;
            }

            float victoryProgress = Mathf.Max(colonization / 0.40f, gm.Apex != null && gm.Apex.ApexSpawned ? 0.8f : 0f);
            Set(V5PlayableLoopStage.PushVictory, Mathf.Clamp01(victoryProgress), "Empuja una condicion de victoria.", "Coloniza 40%, estabiliza quimica o madura una forma apex.");
        }

        private void Set(V5PlayableLoopStage stage, float progress, string goal, string nextAction)
        {
            Stage = stage;
            Progress01 = Mathf.Clamp01(progress);
            Goal = goal;
            NextAction = nextAction;
            LoopSummary = "Loop: " + Stage + " " + (Progress01 * 100f).ToString("0") + "%";

            if (Stage != lastStage)
            {
                lastStage = Stage;
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.Hud != null) gm.Hud.Toast("Nuevo tramo del loop: " + Stage);
            }
        }

        private int CountDirective(V5GameManager gm, V5Directive directive)
        {
            int count = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c != null && c.Role != V5CellRole.Mother && c.Directive == directive) count++;
            }
            return count;
        }
    }
}
