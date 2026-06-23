using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5CampaignEpisodeId
    {
        LucaAwakening,
        MetabolicFork,
        FirstColony,
        TerritorialMatrix,
        EcologicalCrisis,
        ApexEmergence,
        StableBiosphere
    }

    public class V5CampaignEpisodeSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public V5CampaignEpisodeId CurrentEpisode = V5CampaignEpisodeId.LucaAwakening;
        public string CurrentTask = "Instala Motor Metabólico o elige una ruta metabólica.";
        public float EpisodeProgress;
        public int CompletedEpisodes;
        public string LastReward = "Sin recompensa todavía.";

        private readonly HashSet<V5CampaignEpisodeId> completed = new HashSet<V5CampaignEpisodeId>();
        private float tickTimer;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageUp)) ShowPanel = !ShowPanel;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Paused) return;
            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.75f)
            {
                tickTimer = 0f;
                Evaluate(gm);
            }
        }

        private void Evaluate(V5GameManager gm)
        {
            V5CellEntity mother = gm.MotherCell;
            if (mother == null) return;
            float colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            int cellCount = gm.PlayerCellCount();
            int genes = gm.Genes != null ? gm.Genes.UnlockedCount : 0;
            bool apex = gm.Apex != null && gm.Apex.ApexSpawned;
            V5EcosystemSuccessionSystem succession = FindFirstObjectByType<V5EcosystemSuccessionSystem>();
            float stability = succession != null ? succession.StabilityScore : 0f;
            V5SuccessionStage stage = succession != null ? succession.Stage : V5SuccessionStage.PrimordialSoup;

            if (!completed.Contains(V5CampaignEpisodeId.LucaAwakening))
            {
                CurrentEpisode = V5CampaignEpisodeId.LucaAwakening;
                CurrentTask = "Despierta LUCA: consigue Compartimento Genético + Motor Metabólico y reúne ATP/Biomasa.";
                EpisodeProgress = Mathf.Clamp01((mother.HasStructure(V5StructureId.GeneticCompartment) ? 0.35f : 0f) + (mother.HasStructure(V5StructureId.MetabolicEngine) ? 0.35f : 0f) + Mathf.Clamp01(mother.Resources.atp / 80f) * 0.30f);
                if (mother.HasStructure(V5StructureId.GeneticCompartment) && (mother.HasStructure(V5StructureId.MetabolicEngine) || mother.Metabolism != V5MetabolismType.None)) Complete(gm, V5CampaignEpisodeId.LucaAwakening);
                return;
            }

            if (!completed.Contains(V5CampaignEpisodeId.MetabolicFork))
            {
                CurrentEpisode = V5CampaignEpisodeId.MetabolicFork;
                CurrentTask = "Elige el metabolismo que define dominio: respiración/foto = eucariota, fermentación/quimio = procariota.";
                EpisodeProgress = mother.Domain == V5CellDomain.LUCA ? 0.15f : 1f;
                if (mother.Domain != V5CellDomain.LUCA && mother.Metabolism != V5MetabolismType.None) Complete(gm, V5CampaignEpisodeId.MetabolicFork);
                return;
            }

            if (!completed.Contains(V5CampaignEpisodeId.FirstColony))
            {
                CurrentEpisode = V5CampaignEpisodeId.FirstColony;
                CurrentTask = "Forma una microcolonia: divide, asigna roles y mantén al menos 4 células vivas.";
                EpisodeProgress = Mathf.Clamp01(cellCount / 4f);
                if (cellCount >= 4) Complete(gm, V5CampaignEpisodeId.FirstColony);
                return;
            }

            if (!completed.Contains(V5CampaignEpisodeId.TerritorialMatrix))
            {
                CurrentEpisode = V5CampaignEpisodeId.TerritorialMatrix;
                CurrentTask = "Convierte el mapa en territorio vivo: coloniza 15% y activa 2 genes.";
                EpisodeProgress = Mathf.Clamp01(colonization / 0.15f * 0.65f + genes / 2f * 0.35f);
                if (colonization >= 0.15f && genes >= 2) Complete(gm, V5CampaignEpisodeId.TerritorialMatrix);
                return;
            }

            if (!completed.Contains(V5CampaignEpisodeId.EcologicalCrisis))
            {
                CurrentEpisode = V5CampaignEpisodeId.EcologicalCrisis;
                CurrentTask = "Supera una crisis ecológica: reduce hostilidad o alcanza etapa Engineered Microbiome.";
                float hostile = succession != null ? succession.HostilityScore : 0f;
                EpisodeProgress = Mathf.Clamp01(stability * 0.55f + ((int)stage >= (int)V5SuccessionStage.EngineeredMicrobiome ? 0.45f : 0f));
                if (((int)stage >= (int)V5SuccessionStage.EngineeredMicrobiome && hostile < 0.70f) || stability > 0.62f) Complete(gm, V5CampaignEpisodeId.EcologicalCrisis);
                return;
            }

            if (!completed.Contains(V5CampaignEpisodeId.ApexEmergence))
            {
                CurrentEpisode = V5CampaignEpisodeId.ApexEmergence;
                CurrentTask = "Desbloquea maduración apex o demuestra dominancia con 30% de colonización.";
                EpisodeProgress = Mathf.Clamp01((apex ? 1f : 0f) + colonization / 0.30f * 0.65f + genes / 4f * 0.35f);
                if (apex || colonization >= 0.30f) Complete(gm, V5CampaignEpisodeId.ApexEmergence);
                return;
            }

            CurrentEpisode = V5CampaignEpisodeId.StableBiosphere;
            CurrentTask = "Cierra la demo: estabiliza la biosfera con 40% de colonización y estabilidad alta.";
            EpisodeProgress = Mathf.Clamp01(colonization / 0.40f * 0.65f + stability / 0.80f * 0.35f);
            if (!completed.Contains(V5CampaignEpisodeId.StableBiosphere) && colonization >= 0.40f && stability >= 0.78f)
            {
                Complete(gm, V5CampaignEpisodeId.StableBiosphere);
                gm.Win("campaña interna completada");
            }
        }

        private void Complete(V5GameManager gm, V5CampaignEpisodeId id)
        {
            if (completed.Contains(id)) return;
            completed.Add(id);
            CompletedEpisodes = completed.Count;
            EpisodeProgress = 1f;
            GiveReward(gm, id);
            if (gm.Hud != null) gm.Hud.Toast("Episodio completado: " + id);
            if (gm.Codex != null) gm.Codex.Unlock("Campaña: " + id, "Completaste un episodio de la campaña interna V5.");
        }

        private void GiveReward(V5GameManager gm, V5CampaignEpisodeId id)
        {
            V5CellEntity mother = gm.MotherCell;
            if (mother == null) return;
            if (id == V5CampaignEpisodeId.LucaAwakening)
            {
                mother.Resources.atp += 35f; mother.Resources.biomass += 18f; mother.Resources.nucleotides += 6f;
                LastReward = "+35 ATP, +18 biomasa, +6 nucleótidos.";
            }
            else if (id == V5CampaignEpisodeId.MetabolicFork)
            {
                mother.Resources.aminoAcids += 18f; mother.Resources.lipids += 14f; mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 8f);
                LastReward = "+18 AA, +14 lípidos, -8 stress.";
            }
            else if (id == V5CampaignEpisodeId.FirstColony)
            {
                mother.Resources.atp += 45f; mother.Resources.biomass += 32f;
                LastReward = "+45 ATP, +32 biomasa.";
            }
            else if (id == V5CampaignEpisodeId.TerritorialMatrix)
            {
                mother.Stats.colonizationPower += 0.35f; mother.Resources.minerals += 18f;
                LastReward = "+0.35 colonización madre, +18 minerales.";
            }
            else if (id == V5CampaignEpisodeId.EcologicalCrisis)
            {
                mother.Stats.toxinResistance += 0.12f; mother.Stats.repairPerSecond += 0.35f;
                LastReward = "+resistencia toxinas, +reparación.";
            }
            else if (id == V5CampaignEpisodeId.ApexEmergence)
            {
                mother.Resources.atp += 75f; mother.Resources.biomass += 40f; mother.Resources.aminoAcids += 25f;
                LastReward = "+75 ATP, +40 biomasa, +25 AA.";
            }
            else
            {
                LastReward = "Dominancia ecológica validada.";
            }
        }

        public string Summary()
        {
            return CurrentEpisode + " " + (EpisodeProgress * 100f).ToString("0") + "% | completados " + CompletedEpisodes + "/7";
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(540f, 70f, 475f, 260f);
            GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("CAMPAÑA INTERNA 1.3  [PageUp]", title);
            GUILayout.Label("Episodio: " + CurrentEpisode, body);
            GUILayout.Label("Progreso: " + (EpisodeProgress * 100f).ToString("0") + "% | Completados: " + CompletedEpisodes + "/7", body);
            GUILayout.Label("Objetivo: " + CurrentTask, body);
            GUILayout.Label("Última recompensa: " + LastReward, body);
            GUILayout.Space(6f);
            GUILayout.Label("Esta capa ordena la demo: desde LUCA hasta biosfera estable sin depender de PvP.", body);
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box);
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 16; title.normal.textColor = new Color(0.92f, 0.95f, 1f, 1f);
            body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white;
        }
    }
}
