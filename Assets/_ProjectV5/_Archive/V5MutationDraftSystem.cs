using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// V5 0.9: mutation draft layer. This gives the run a roguelite/strategy arc without requiring assets.
    /// Press V to open, then choose 1/2/3 when choices are available.
    /// </summary>
    public class V5MutationDraftSystem : MonoBehaviour, IV5RunResettable
    {
        private class MutationDef
        {
            public string id;
            public string title;
            public string description;
            public System.Action<V5GameManager> apply;
        }

        public bool ShowPanel;
        public int PendingDrafts { get; private set; }
        public readonly List<string> TakenMutations = new List<string>(16);
        public string LastMutation = "";
        public bool HasPendingDraft { get { return PendingDrafts > 0; } }

        private readonly List<MutationDef> library = new List<MutationDef>(16);
        private readonly List<MutationDef> currentChoices = new List<MutationDef>(3);
        private float nextTimedDraft = 150f;
        private int lastCellThreshold = 3;
        private Texture2D panelTexture;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;

        private void Awake()
        {
            BuildLibrary();
        }

        private void Start()
        {
            V5PanelRouter.Register("Mutaciones", () => ShowPanel, v =>
            {
                ShowPanel = v;
                if (ShowPanel && PendingDrafts > 0 && currentChoices.Count == 0) RollChoices();
            });
        }

        public void ResetForNewRun()
        {
            PendingDrafts = 0;
            TakenMutations.Clear();
            currentChoices.Clear();
            LastMutation = "";
            nextTimedDraft = 150f;
            lastCellThreshold = 3;
            ShowPanel = false;
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;

            if (gm.ElapsedSeconds >= nextTimedDraft)
            {
                nextTimedDraft += 180f;
                GrantDraft("mutación temporal disponible");
            }

            int cells = gm.PlayerCellCount();
            if (cells >= lastCellThreshold)
            {
                lastCellThreshold += 4;
                GrantDraft("la colonia alcanzó masa crítica");
            }

            if (Input.GetKeyDown(KeyCode.V))
            {
                if (!ShowPanel) V5PanelRouter.CloseOthers("Mutaciones");
                ShowPanel = !ShowPanel;
                if (ShowPanel && PendingDrafts > 0 && currentChoices.Count == 0) RollChoices();
            }

            if (ShowPanel && PendingDrafts > 0)
            {
                if (Input.GetKeyDown(KeyCode.Alpha1)) Choose(0);
                if (Input.GetKeyDown(KeyCode.Alpha2)) Choose(1);
                if (Input.GetKeyDown(KeyCode.Alpha3)) Choose(2);
            }
        }

        public void GrantDraft(string reason)
        {
            PendingDrafts++;
            if (currentChoices.Count == 0) RollChoices();
            Notify("Draft de mutación: " + reason + ". Abre V o Evolucion.");
        }

        private void RollChoices()
        {
            currentChoices.Clear();
            List<MutationDef> available = new List<MutationDef>(library.Count);
            for (int i = 0; i < library.Count; i++) if (!TakenMutations.Contains(library[i].id)) available.Add(library[i]);
            for (int i = 0; i < 3 && available.Count > 0; i++)
            {
                int idx = Random.Range(0, available.Count);
                currentChoices.Add(available[idx]);
                available.RemoveAt(idx);
            }
        }

        private void Choose(int index)
        {
            if (index < 0 || index >= currentChoices.Count || PendingDrafts <= 0) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            MutationDef choice = currentChoices[index];
            TakenMutations.Add(choice.id);
            choice.apply(gm);
            LastMutation = choice.title;
            PendingDrafts--;
            currentChoices.Clear();
            if (PendingDrafts > 0) RollChoices(); else ShowPanel = false;
            Notify("Mutación aplicada: " + choice.title);
        }

        public void OpenPanel()
        {
            V5PanelRouter.CloseOthers("Mutaciones");
            ShowPanel = true;
            if (PendingDrafts > 0 && currentChoices.Count == 0) RollChoices();
        }

        private void BuildLibrary()
        {
            library.Clear();
            Add("membrane_hardening", "Membrana reforzada", "+12% HP máximo y +0.05 resistencia a toxinas para todas tus células actuales.", gm => ForEachPlayer(gm, c => { c.Stats.maxHp *= 1.12f; c.Stats.currentHp += 8f; c.Stats.toxinResistance += 0.05f; }));
            Add("metabolic_efficiency", "Eficiencia metabólica", "+0.35 ATP/s y -6 stress en todas tus células actuales.", gm => ForEachPlayer(gm, c => { c.Stats.atpPerSecond += 0.35f; c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 6f); }));
            Add("division_priming", "Priming de división", "+0.18 eficiencia de división y +18 biomasa en la madre.", gm => { ForEachPlayer(gm, c => c.Stats.divisionEfficiency += 0.18f); if (gm.MotherCell != null) gm.MotherCell.Resources.biomass += 18f; });
            Add("sensor_membranes", "Membranas sensoras", "+35% sensor range, ideal para RTS y niebla de guerra.", gm => ForEachPlayer(gm, c => c.Stats.sensorRange *= 1.35f));
            Add("colonial_matrix", "Matriz colonial", "+0.22 poder de colonización y baja stress en territorio colonizado.", gm => ForEachPlayer(gm, c => { c.Stats.colonizationPower += 0.22f; c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 4f); }));
            Add("lysosomal_bias", "Sesgo lisosomal", "Las celulas con lisosoma/depredadoras ganan dano fisico y reparacion.", gm => ForEachPlayer(gm, c => { if (c.HasStructure(V5StructureId.Lysosome) || c.EvolutionPath == V5EvolutionPath.Amoeba || c.EvolutionPath == V5EvolutionPath.Rotifer) { c.Stats.physicalDamagePerSecond += 1.2f; c.Stats.repairPerSecond += 0.5f; } }));
            Add("photosystem_stack", "Fotosistemas apilados", "Fotosintéticas ganan ATP/s y convierten su entorno en oxígeno/colonización.", gm => { ForEachPlayer(gm, c => { if (c.HasPhotosynthesis || c.Metabolism == V5MetabolismType.Photosynthesis) c.Stats.atpPerSecond += 0.55f; }); if (gm.MotherCell != null && gm.Environment != null) gm.Environment.ModifyArea(gm.MotherCell.transform.position, 5.5f, -0.02f, 0f, 0.18f, -0.04f, 0f, 0.08f, 0f); });
            Add("acidophile_proteome", "Proteoma acidófilo", "Arqueas/quimiolitótrofos suben tolerancia química y térmica.", gm => ForEachPlayer(gm, c => { if (c.EvolutionPath == V5EvolutionPath.Archaea || c.Metabolism == V5MetabolismType.Chemolithotrophy) { c.Stats.toxinResistance += 0.15f; c.Stats.thermalResistance += 0.15f; c.Stats.phTolerance = Mathf.Clamp01(c.Stats.phTolerance + 0.18f); } }));
            Add("swarm_protocol", "Protocolo swarm", "Bacterias/procariotas actuales ganan velocidad, pero suben un poco su stress.", gm => ForEachPlayer(gm, c => { if (c.Domain == V5CellDomain.Prokaryote) { c.Stats.speed *= 1.18f; c.Stats.divisionEfficiency += 0.12f; c.Stats.stress += 3f; } }));
            Add("homeostasis_loop", "Loop homeostático", "Todas tus células curan stress y aumentan reparación lenta.", gm => ForEachPlayer(gm, c => { c.Stats.repairPerSecond += 0.35f; c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 12f); }));
        }

        private void Add(string id, string title, string description, System.Action<V5GameManager> apply)
        {
            MutationDef d = new MutationDef();
            d.id = id;
            d.title = title;
            d.description = description;
            d.apply = apply;
            library.Add(d);
        }

        private void ForEachPlayer(V5GameManager gm, System.Action<V5CellEntity> action)
        {
            if (gm == null || action == null) return;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++) if (cells[i] != null) action(cells[i]);
        }

        private void Notify(string msg)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(msg);
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null)
            {
                Vector2 p = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;
                feedback.Push(msg, p, new Color(0.95f, 0.82f, 1f, 1f));
                feedback.Ping("gene");
            }
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(18f, Screen.height * 0.5f - 150f, 420f, 300f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 10f, r.width - 28f, r.height - 20f));
            GUILayout.Label("MUTACIONES DE RUN", titleStyle);
            GUILayout.Label("V abre/cierra. Pendientes: " + PendingDrafts + ". Elige con 1/2/3.", bodyStyle);
            GUILayout.Space(8f);
            if (PendingDrafts <= 0)
            {
                GUILayout.Label("Sin mutaciones pendientes. La siguiente llega por tiempo o crecimiento de colonia.", bodyStyle);
                if (!string.IsNullOrEmpty(LastMutation)) GUILayout.Label("Última: " + LastMutation, bodyStyle);
            }
            else
            {
                for (int i = 0; i < currentChoices.Count; i++)
                {
                    MutationDef c = currentChoices[i];
                    GUILayout.Label((i + 1) + ") " + c.title, titleStyle);
                    GUILayout.Label(c.description, bodyStyle);
                    GUILayout.Space(5f);
                }
            }
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            panelTexture = MakeTexture(new Color(0.055f, 0.045f, 0.075f, 0.96f));
            panelStyle.normal.background = panelTexture;
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.wordWrap = true;
            titleStyle.normal.textColor = new Color(0.98f, 0.86f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label);
            bodyStyle.wordWrap = true;
            bodyStyle.normal.textColor = new Color(0.92f, 0.95f, 1f, 1f);
        }

        private Texture2D MakeTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            tex.SetPixel(0, 0, color);
            tex.Apply(false);
            return tex;
        }
    }
}
