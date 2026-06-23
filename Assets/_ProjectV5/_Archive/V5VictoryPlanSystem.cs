using UnityEngine;

namespace Protogenesis.V5
{
    public class V5VictoryPlanSystem : MonoBehaviour
    {
        public enum VictoryPlan
        {
            EcologicalDominance,
            StableBiosphere,
            ApexAscension,
            PredatoryElimination,
            ResearchSupremacy
        }

        public VictoryPlan ActivePlan = VictoryPlan.EcologicalDominance;
        public float CurrentProgress;
        public string CurrentAdvice = "coloniza y estabiliza la gota";
        public string LastReward = "sin recompensa";

        private bool showPanel;
        private float tick;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.ScrollLock)) showPanel = !showPanel;
            tick += Time.deltaTime;
            if (tick >= 1.0f)
            {
                tick = 0f;
                EvaluatePlan();
                ApplySmallGuidanceReward();
            }
        }

        private void EvaluatePlan()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null)
            {
                CurrentProgress = 0f;
                return;
            }
            V5EnvironmentGrid env = gm.Environment;
            float colonization = env != null ? env.AverageColonization() : 0f;
            float oxygen = env != null ? env.AverageOxygen() : 0f;
            float toxins = env != null ? env.AverageToxins() : 0f;
            float acidity = env != null ? env.AverageAcidity() : 0.5f;
            int enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            int playerCells = gm.PlayerCells != null ? gm.PlayerCells.Count : 0;

            if (ActivePlan == VictoryPlan.EcologicalDominance)
            {
                CurrentProgress = Mathf.Clamp01(colonization / 0.40f);
                CurrentAdvice = CurrentProgress < 0.65f ? "usa Colonize, Red colonial y postura Expansion/Terraform" : "protege zonas colonizadas hasta cerrar 40%";
            }
            else if (ActivePlan == VictoryPlan.StableBiosphere)
            {
                float stability = Mathf.Clamp01(colonization / 0.32f) * 0.35f + Mathf.Clamp01(oxygen / 0.45f) * 0.20f + Mathf.Clamp01((0.22f - toxins) / 0.22f) * 0.25f + Mathf.Clamp01(1f - Mathf.Abs(acidity - 0.52f) * 2.5f) * 0.20f;
                CurrentProgress = Mathf.Clamp01(stability);
                CurrentAdvice = "baja toxinas/acidez y combina colonización con fotosíntesis/homeostasis";
            }
            else if (ActivePlan == VictoryPlan.ApexAscension)
            {
                bool apex = gm.Apex != null && gm.Apex.ApexSpawned;
                float time = Mathf.Clamp01(gm.ElapsedSeconds / V5Balance.ApexMinimumTime);
                float resources = gm.MotherCell != null ? Mathf.Clamp01(gm.MotherCell.Resources.atp / V5Balance.ApexCostATP) * 0.35f + Mathf.Clamp01(gm.MotherCell.Resources.biomass / V5Balance.ApexCostBiomass) * 0.35f + Mathf.Clamp01(gm.MotherCell.Resources.nucleotides / V5Balance.ApexCostNucleotides) * 0.30f : 0f;
                float gene = gm.Genes != null && gm.Genes.HasGene(V5GeneId.ApexMaturation) ? 1f : 0f;
                CurrentProgress = apex ? 1f : Mathf.Clamp01(time * 0.30f + resources * 0.35f + gene * 0.35f);
                CurrentAdvice = "prepara recursos, desbloquea Maduración Apex y defiende la madre";
            }
            else if (ActivePlan == VictoryPlan.PredatoryElimination)
            {
                float pressure = Mathf.Clamp01(1f - enemies / 18f);
                float army = Mathf.Clamp01(playerCells / 12f);
                CurrentProgress = Mathf.Clamp01(pressure * 0.65f + army * 0.35f);
                CurrentAdvice = "crea predators, usa Intel de counters y ordena ataque prioritario";
            }
            else if (ActivePlan == VictoryPlan.ResearchSupremacy)
            {
                V5ResearchSystem research = FindFirstObjectByType<V5ResearchSystem>();
                int completed = 0;
                int total = 0;
                if (research != null)
                {
                    total = research.Projects.Count;
                    for (int i = 0; i < research.Projects.Count; i++) if (research.Projects[i].completed) completed++;
                }
                CurrentProgress = total > 0 ? Mathf.Clamp01(completed / (float)total) : 0f;
                CurrentAdvice = "mantén la madre segura, alta síntesis y proyectos activos";
            }
        }

        private void ApplySmallGuidanceReward()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || CurrentProgress < 0.25f) return;
            V5CellEntity mother = gm.MotherCell;
            float reward = Mathf.Clamp(CurrentProgress * 0.012f, 0.002f, 0.012f);
            if (ActivePlan == VictoryPlan.EcologicalDominance && gm.Environment != null)
            {
                gm.Environment.ModifyArea(mother.transform.position, 2.4f, 0f, 0f, 0.0008f, -0.0008f, 0f, reward, 0f);
                LastReward = "micro-bonus colonizador";
            }
            else if (ActivePlan == VictoryPlan.StableBiosphere)
            {
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - CurrentProgress * 0.035f);
                LastReward = "homeostasis de biosfera";
            }
            else if (ActivePlan == VictoryPlan.ApexAscension)
            {
                mother.Resources.nucleotides += CurrentProgress * 0.01f;
                mother.Resources.aminoAcids += CurrentProgress * 0.015f;
                LastReward = "precursores apex";
            }
            else if (ActivePlan == VictoryPlan.PredatoryElimination)
            {
                mother.Resources.atp += CurrentProgress * 0.018f;
                LastReward = "impulso táctico";
            }
            else if (ActivePlan == VictoryPlan.ResearchSupremacy)
            {
                mother.Resources.nucleotides += CurrentProgress * 0.018f;
                LastReward = "datos genéticos";
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            Rect r = new Rect(430, 206, 405, 312);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "PLAN DE VICTORIA — 1.6", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), "Plan activo: " + ActivePlan + " | Progreso " + (CurrentProgress * 100f).ToString("0") + "%");
            GUI.Box(new Rect(r.x + 12, r.y + 64, r.width - 24, 18), "");
            GUI.Box(new Rect(r.x + 14, r.y + 66, (r.width - 28) * CurrentProgress, 14), "");

            float y = r.y + 94;
            DrawButton(r, ref y, VictoryPlan.EcologicalDominance, "Controlar 40% de colonización.");
            DrawButton(r, ref y, VictoryPlan.StableBiosphere, "Ambiente estable: O2 alto, toxina baja, pH viable.");
            DrawButton(r, ref y, VictoryPlan.ApexAscension, "Invocar una forma apex y sostenerla.");
            DrawButton(r, ref y, VictoryPlan.PredatoryElimination, "Eliminar o neutralizar amenazas activas.");
            DrawButton(r, ref y, VictoryPlan.ResearchSupremacy, "Completar la mayoría del árbol de research.");

            GUI.Label(new Rect(r.x + 12, r.y + r.height - 54, r.width - 24, 20), "Consejo: " + CurrentAdvice);
            GUI.Label(new Rect(r.x + 12, r.y + r.height - 30, r.width - 24, 20), "Bonus guía: " + LastReward);
        }

        private void DrawButton(Rect r, ref float y, VictoryPlan plan, string desc)
        {
            if (GUI.Button(new Rect(r.x + 12, y, 150, 28), plan.ToString(), button))
            {
                ActivePlan = plan;
                EvaluatePlan();
                Toast("Plan de victoria: " + plan);
            }
            GUI.Label(new Rect(r.x + 172, y + 4, r.width - 184, 22), desc);
            y += 34;
        }

        private void Toast(string text)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(text);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.fontSize = 12;
            title = new GUIStyle(GUI.skin.label); title.fontSize = 16; title.fontStyle = FontStyle.Bold; title.normal.textColor = new Color(0.85f, 1f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 11; button.wordWrap = true;
        }
    }
}
