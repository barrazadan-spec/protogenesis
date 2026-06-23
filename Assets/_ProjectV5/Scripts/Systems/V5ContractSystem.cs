using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5ContractType { ColonizePatch, CleanToxins, HuntThreats, GrowColony, StabilizeAcidity, GatherResources }

    [System.Serializable]
    public class V5Contract
    {
        public V5ContractType type;
        public string title;
        public string description;
        public float target;
        public float progress;
        public bool accepted;
        public bool completed;
        public V5ResourceWallet reward;
        public float startColonization;
        public float startToxins;
        public float startResources;
        public int startEnemies;
    }

    public class V5ContractSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public string LastMessage = "Contratos ecológicos listos. Pulsa , para abrir.";
        public readonly List<V5Contract> Offered = new List<V5Contract>(3);
        public V5Contract ActiveContract;
        public int CompletedContracts;
        private float offerTimer;
        private float tickTimer;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Start() { V5PanelRouter.Register("Contratos", () => ShowPanel, v => ShowPanel = v); GenerateOffers(); }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Comma)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Contratos"); ShowPanel = !ShowPanel; }
            if (V5GameManager.Instance == null) return;
            offerTimer += Time.deltaTime;
            if (ActiveContract == null && (Offered.Count == 0 || offerTimer > 95f)) GenerateOffers();
            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.35f) { tickTimer = 0f; TickActiveContract(); }
        }

        public void GenerateOffers()
        {
            Offered.Clear(); offerTimer = 0f;
            V5ContractType[] all = (V5ContractType[])System.Enum.GetValues(typeof(V5ContractType));
            int guard = 0;
            while (Offered.Count < 3 && guard < 32)
            {
                guard++;
                V5ContractType type = all[Random.Range(0, all.Length)];
                bool exists = false;
                for (int i = 0; i < Offered.Count; i++) if (Offered[i].type == type) exists = true;
                if (!exists) Offered.Add(CreateContract(type));
            }
            LastMessage = "Nuevos contratos disponibles.";
        }

        private V5Contract CreateContract(V5ContractType type)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5Contract c = new V5Contract();
            c.type = type;
            c.startColonization = gm != null && gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            c.startToxins = gm != null && gm.Environment != null ? gm.Environment.AverageToxins() : 0f;
            c.startEnemies = gm != null && gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            c.startResources = gm != null && gm.MotherCell != null ? TotalResources(gm.MotherCell.Resources) : 0f;
            if (type == V5ContractType.ColonizePatch) { c.title = "Colonizar matriz local"; c.description = "Aumenta la colonización media de la gota."; c.target = 0.055f; c.reward = V5ResourceWallet.Cost(40f, 32f, 18f, 14f, 8f, 7f); }
            else if (type == V5ContractType.CleanToxins) { c.title = "Detoxificar microambiente"; c.description = "Reduce toxinas medias con catalasa, movimiento o eventos favorables."; c.target = 0.035f; c.reward = V5ResourceWallet.Cost(48f, 18f, 14f, 22f, 7f, 5f); }
            else if (type == V5ContractType.HuntThreats) { c.title = "Cazar amenazas"; c.description = "Elimina organismos hostiles o minibosses."; c.target = 3f; c.reward = V5ResourceWallet.Cost(65f, 28f, 22f, 12f, 10f, 8f); }
            else if (type == V5ContractType.GrowColony) { c.title = "Microcolonia estable"; c.description = "Llega a varias hijas/nietas sin perder la madre."; c.target = 7f; c.reward = V5ResourceWallet.Cost(38f, 42f, 16f, 16f, 9f, 6f); }
            else if (type == V5ContractType.StabilizeAcidity) { c.title = "Homeostasis de pH"; c.description = "Mantén acidez media cerca de 0.50 durante 30 segundos acumulados."; c.target = 30f; c.reward = V5ResourceWallet.Cost(52f, 22f, 16f, 18f, 12f, 5f); }
            else { c.title = "Recolectar precursores"; c.description = "Aumenta la reserva total de recursos de la madre."; c.target = 130f; c.reward = V5ResourceWallet.Cost(35f, 25f, 25f, 20f, 14f, 12f); }
            return c;
        }

        public void Accept(V5Contract contract)
        {
            if (contract == null) return;
            ActiveContract = contract; ActiveContract.accepted = true; Offered.Clear();
            LastMessage = "Contrato aceptado: " + ActiveContract.title; Toast(LastMessage);
        }

        public void AbandonActive()
        {
            if (ActiveContract == null) return;
            LastMessage = "Contrato abandonado: " + ActiveContract.title; ActiveContract = null; GenerateOffers();
        }

        private void TickActiveContract()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || ActiveContract == null || ActiveContract.completed) return;
            V5Contract c = ActiveContract;
            if (c.type == V5ContractType.ColonizePatch) c.progress = Mathf.Max(0f, (gm.Environment != null ? gm.Environment.AverageColonization() : 0f) - c.startColonization);
            else if (c.type == V5ContractType.CleanToxins) c.progress = Mathf.Max(0f, c.startToxins - (gm.Environment != null ? gm.Environment.AverageToxins() : c.startToxins));
            else if (c.type == V5ContractType.HuntThreats)
            {
                int current = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
                c.progress = Mathf.Max(0, c.startEnemies - current);
                V5BossEncounterSystem bosses = FindFirstObjectByType<V5BossEncounterSystem>();
                if (bosses != null) c.progress = Mathf.Max(c.progress, bosses.BossesDefeated);
            }
            else if (c.type == V5ContractType.GrowColony) c.progress = gm.PlayerCellCount();
            else if (c.type == V5ContractType.StabilizeAcidity)
            {
                float a = gm.Environment != null ? gm.Environment.AverageAcidity() : 0.5f;
                if (Mathf.Abs(a - 0.5f) <= 0.08f) c.progress += 0.35f;
            }
            else if (c.type == V5ContractType.GatherResources) c.progress = Mathf.Max(0f, (gm.MotherCell != null ? TotalResources(gm.MotherCell.Resources) : c.startResources) - c.startResources);
            if (c.progress >= c.target) CompleteActive();
        }

        private void CompleteActive()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || ActiveContract == null) return;
            ActiveContract.completed = true; CompletedContracts++;
            if (gm.MotherCell != null)
            {
                gm.MotherCell.Resources.atp += ActiveContract.reward.atp;
                gm.MotherCell.Resources.biomass += ActiveContract.reward.biomass;
                gm.MotherCell.Resources.aminoAcids += ActiveContract.reward.aminoAcids;
                gm.MotherCell.Resources.lipids += ActiveContract.reward.lipids;
                gm.MotherCell.Resources.nucleotides += ActiveContract.reward.nucleotides;
                gm.MotherCell.Resources.minerals += ActiveContract.reward.minerals;
                gm.MotherCell.Stats.stress = Mathf.Max(0f, gm.MotherCell.Stats.stress - 12f);
            }
            if (gm.Codex != null) gm.Codex.Unlock("Contratos ecológicos", "Objetivos opcionales que recompensan dominio ecológico y RTS.");
            LastMessage = "Contrato completado: " + ActiveContract.title; Toast(LastMessage); ActiveContract = null; GenerateOffers();
        }

        private float TotalResources(V5ResourceWallet r) { return r.atp + r.biomass + r.aminoAcids + r.lipids + r.nucleotides + r.minerals; }
        private void Toast(string message) { V5GameManager gm = V5GameManager.Instance; if (gm != null && gm.Hud != null) gm.Hud.Toast(message); }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(18f, 190f, 500f, 380f);
            GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 10f, r.width - 28f, r.height - 20f));
            GUILayout.Label("CONTRATOS ECOLÓGICOS 1.1  [,]", title);
            GUILayout.Label(LastMessage, body); GUILayout.Space(8f);
            if (ActiveContract != null)
            {
                GUILayout.Label("Activo: " + ActiveContract.title, title); GUILayout.Label(ActiveContract.description, body);
                GUILayout.Label("Progreso: " + ActiveContract.progress.ToString("0.0") + " / " + ActiveContract.target.ToString("0.0"));
                Rect bar = GUILayoutUtility.GetRect(430f, 18f); GUI.Box(bar, ""); GUI.Box(new Rect(bar.x + 2f, bar.y + 2f, (bar.width - 4f) * Mathf.Clamp01(ActiveContract.progress / Mathf.Max(0.01f, ActiveContract.target)), bar.height - 4f), "");
                GUILayout.Label("Recompensa: " + RewardText(ActiveContract.reward), body);
                if (GUILayout.Button("Abandonar contrato")) AbandonActive();
            }
            else
            {
                GUILayout.Label("Ofertas disponibles", title);
                for (int i = 0; i < Offered.Count; i++)
                {
                    V5Contract c = Offered[i];
                    GUILayout.BeginVertical(GUI.skin.box); GUILayout.Label(c.title, title); GUILayout.Label(c.description, body);
                    GUILayout.Label("Objetivo: " + c.target.ToString("0.0") + " | Recompensa: " + RewardText(c.reward), body);
                    if (GUILayout.Button("Aceptar")) { Accept(c); GUILayout.EndVertical(); break; }
                    GUILayout.EndVertical();
                }
                if (GUILayout.Button("Renovar ofertas")) GenerateOffers();
            }
            GUILayout.Label("Completados: " + CompletedContracts); GUILayout.EndArea();
        }
        private string RewardText(V5ResourceWallet w) { return "+" + w.atp.ToString("0") + " ATP, +" + w.biomass.ToString("0") + " Bio"; }
        private void EnsureStyles() { if (panel != null) return; panel = new GUIStyle(GUI.skin.box); title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.86f, 1f, 1f, 1f); body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white; }
    }
}
