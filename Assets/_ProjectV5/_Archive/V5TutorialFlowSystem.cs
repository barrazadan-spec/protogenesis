using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Guided internal-demo onboarding. It does not block sandbox play; it simply gives the player
    /// a production-style objective ladder with rewards and readable teaching text.
    /// Toggle with F11.
    /// </summary>
    public class V5TutorialFlowSystem : MonoBehaviour
    {
        private class StepDef
        {
            public string id;
            public string title;
            public string instruction;
            public string reward;
            public System.Func<V5GameManager, float> progress;
            public System.Action<V5GameManager> grantReward;
            public bool completed;
        }

        public bool ShowPanel;
        public bool Enabled = true;
        public int CurrentStep { get; private set; }
        public float OverallProgress01 { get; private set; }
        public string CurrentInstruction { get; private set; }
        public string LastCompletion { get; private set; }

        private readonly List<StepDef> steps = new List<StepDef>(12);
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle doneStyle;
        private float tick;

        private void Awake()
        {
            BuildSteps();
        }

        private void Start()
        {
            ShowPanel = false;
            V5PanelRouter.Register("Tutorial", () => ShowPanel, v => ShowPanel = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F11)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Tutorial"); ShowPanel = !ShowPanel; }
            if (!Enabled) return;

            tick += Time.deltaTime;
            if (tick < 0.25f) return;
            tick = 0f;
            Evaluate();
        }

        private void BuildSteps()
        {
            steps.Clear();
            Add("core", "Construye el núcleo celular", "Abre el panel interior con E e instala Motor Metabólico y Maquinaria de Síntesis.", "+12 ATP y +8 biomasa", gm =>
            {
                V5CellEntity m = gm.MotherCell;
                if (m == null) return 0f;
                float p = 0f;
                if (m.HasStructure(V5StructureId.MetabolicEngine)) p += 0.5f;
                if (m.HasStructure(V5StructureId.SynthesisMachinery)) p += 0.5f;
                return p;
            }, gm => Reward(gm, 12f, 8f, 3f, 2f, 1f, 0f));

            Add("domain", "Define el dominio", "Elige metabolismo: respiración/fotosíntesis para eucariota, fermentación/quimio para procariota.", "+10 nucleótidos", gm => gm.MotherCell != null && gm.MotherCell.Metabolism != V5MetabolismType.None ? 1f : 0f, gm => Reward(gm, 0f, 0f, 0f, 0f, 10f, 0f));

            Add("farm", "Levanta economía", "Recolecta recursos o manda una hija a recolectar con el modo 2.", "+20 biomasa", gm =>
            {
                if (gm.MotherCell == null) return 0f;
                int farmers = CountDirective(gm, V5Directive.Farm);
                float stocked = Mathf.Clamp01((gm.MotherCell.Resources.biomass + gm.MotherCell.Resources.atp) / 140f);
                return Mathf.Clamp01(stocked * 0.65f + Mathf.Clamp01(farmers / 1f) * 0.35f);
            }, gm => Reward(gm, 0f, 20f, 5f, 5f, 0f, 0f));

            Add("divide", "Crea la primera microcolonia", "Presiona D para dividirte hasta tener al menos 3 células aliadas.", "+15 ATP y +8 lípidos", gm => Mathf.Clamp01((gm.PlayerCellCount() - 1f) / 2f), gm => Reward(gm, 15f, 0f, 0f, 8f, 0f, 0f));

            Add("directive", "Usa comportamiento RTS", "Selecciona hijas y asigna al menos una a defender/explorar/colonizar.", "+1 draft de mutación", gm =>
            {
                int useful = CountDirective(gm, V5Directive.Defend) + CountDirective(gm, V5Directive.Explore) + CountDirective(gm, V5Directive.Colonize);
                return Mathf.Clamp01(useful / 1f);
            }, gm =>
            {
                V5MutationDraftSystem draft = FindFirstObjectByType<V5MutationDraftSystem>();
                if (draft != null) draft.GrantDraft("tutorial: conducta RTS dominada");
            });

            Add("genes", "Activa el árbol evolutivo", "Abre G y desbloquea al menos 2 genes. El segundo gen define el plan de juego.", "+12 aminoácidos y +8 NT", gm => gm.Genes != null ? Mathf.Clamp01(gm.Genes.UnlockedCount / 2f) : 0f, gm => Reward(gm, 0f, 0f, 12f, 0f, 8f, 0f));

            Add("identity", "Instala identidad biológica", "Instala una estructura de identidad: flagelo, cápsula, tilacoide, lisosoma, hifa, etc.", "+8 minerales y baja stress", gm =>
            {
                V5CellEntity m = gm.MotherCell;
                if (m == null) return 0f;
                for (int i = 0; i < m.Structures.Count; i++)
                {
                    V5StructureId id = m.Structures[i];
                    if (id != V5StructureId.GeneticCompartment && id != V5StructureId.MetabolicEngine && id != V5StructureId.SynthesisMachinery && id != V5StructureId.StorageVacuole && id != V5StructureId.Catalase) return 1f;
                }
                return 0f;
            }, gm =>
            {
                Reward(gm, 0f, 0f, 0f, 0f, 0f, 8f);
                if (gm.MotherCell != null) gm.MotherCell.Stats.stress = Mathf.Max(0f, gm.MotherCell.Stats.stress - 12f);
            });

            Add("world", "Transforma la gota", "Coloniza al menos 15% del mapa con modo 5, matriz colonial o metabolismo fotosintético.", "+1 draft de mutación", gm => gm.Environment != null ? Mathf.Clamp01(gm.Environment.AverageColonization() / 0.15f) : 0f, gm =>
            {
                V5MutationDraftSystem draft = FindFirstObjectByType<V5MutationDraftSystem>();
                if (draft != null) draft.GrantDraft("tutorial: world builder activado");
            });

            Add("stability", "Estabiliza o domina", "Llega a 25% colonización, elimina amenazas o invoca una forma apex.", "+30 recursos mixtos", gm =>
            {
                float c = gm.Environment != null ? gm.Environment.AverageColonization() / 0.25f : 0f;
                float enemies = gm.NonPlayerCells != null && gm.NonPlayerCells.Count == 0 && gm.ElapsedSeconds > 120f ? 1f : 0f;
                float apex = gm.Apex != null && gm.Apex.ApexSpawned ? 1f : 0f;
                return Mathf.Clamp01(Mathf.Max(c, enemies, apex));
            }, gm => Reward(gm, 18f, 18f, 8f, 8f, 6f, 4f));
        }

        private void Add(string id, string title, string instruction, string reward, System.Func<V5GameManager, float> progress, System.Action<V5GameManager> grantReward)
        {
            StepDef s = new StepDef();
            s.id = id;
            s.title = title;
            s.instruction = instruction;
            s.reward = reward;
            s.progress = progress;
            s.grantReward = grantReward;
            steps.Add(s);
        }

        private void Evaluate()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || steps.Count == 0) return;

            int completed = 0;
            for (int i = 0; i < steps.Count; i++) if (steps[i].completed) completed++;
            OverallProgress01 = completed / Mathf.Max(1f, (float)steps.Count);

            CurrentStep = Mathf.Clamp(CurrentStep, 0, steps.Count - 1);
            while (CurrentStep < steps.Count && steps[CurrentStep].completed) CurrentStep++;
            if (CurrentStep >= steps.Count)
            {
                CurrentInstruction = "Tutorial completo. Juega por dominancia ecológica o prueba otro escenario.";
                return;
            }

            StepDef current = steps[CurrentStep];
            float p = Mathf.Clamp01(current.progress != null ? current.progress(gm) : 0f);
            CurrentInstruction = current.title + ": " + current.instruction + " (" + (p * 100f).ToString("0") + "%)";
            if (p >= 1f)
            {
                current.completed = true;
                if (current.grantReward != null) current.grantReward(gm);
                LastCompletion = current.title + " completado. Recompensa: " + current.reward;
                if (gm.Hud != null) gm.Hud.Toast(LastCompletion);
                V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                if (feedback != null && gm.MotherCell != null) feedback.Push("✓ " + current.title, gm.MotherCell.transform.position, new Color(0.6f, 1f, 0.75f, 1f));
            }
        }

        private static int CountDirective(V5GameManager gm, V5Directive directive)
        {
            if (gm == null || gm.PlayerCells == null) return 0;
            int count = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++) if (gm.PlayerCells[i] != null && gm.PlayerCells[i].Directive == directive) count++;
            return count;
        }

        private static void Reward(V5GameManager gm, float atp, float biomass, float aa, float lipids, float nt, float minerals)
        {
            if (gm == null || gm.MotherCell == null) return;
            gm.MotherCell.Resources.atp += atp;
            gm.MotherCell.Resources.biomass += biomass;
            gm.MotherCell.Resources.aminoAcids += aa;
            gm.MotherCell.Resources.lipids += lipids;
            gm.MotherCell.Resources.nucleotides += nt;
            gm.MotherCell.Resources.minerals += minerals;
        }

        private void OnGUI()
        {
            if (!ShowPanel || !Enabled) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 430f, Screen.height - 335f, 420f, 220f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 10f, r.width - 28f, r.height - 20f));
            GUILayout.Label("TUTORIAL GUIADO 1.0", titleStyle);
            GUILayout.Label("F11 muestra/oculta. No bloquea el sandbox.", bodyStyle);
            GUILayout.Space(4f);
            GUILayout.Label(CurrentInstruction, bodyStyle);
            GUILayout.Box("Progreso total: " + (OverallProgress01 * 100f).ToString("0") + "%", GUILayout.Height(24f));
            if (!string.IsNullOrEmpty(LastCompletion)) GUILayout.Label(LastCompletion, doneStyle);
            GUILayout.Space(4f);
            if (GUILayout.Button("Saltar tutorial / modo libre")) Enabled = false;
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.normal.textColor = new Color(0.85f, 1f, 0.95f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
            doneStyle = new GUIStyle(bodyStyle); doneStyle.normal.textColor = new Color(0.7f, 1f, 0.7f, 1f);
        }
    }
}
