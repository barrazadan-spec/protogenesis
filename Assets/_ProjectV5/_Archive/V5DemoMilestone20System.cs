using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Demo milestone layer: measures whether the current run has completed the full
    /// world-builder + RTS loop and exports a lightweight report for playtests.
    /// </summary>
    public class V5DemoMilestone20System : MonoBehaviour
    {
        public string CurrentMilestone = "Start a run and awaken LUCA.";
        public float DemoReadiness01 { get; private set; }
        public string RecommendedFocus = "Install core structures.";
        public string LastExportPath = "";

        private readonly List<Milestone> milestones = new List<Milestone>();
        private bool showPanel;
        private float tick;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;
        private GUIStyle small;

        private string ReportPath
        {
            get { return Path.Combine(Application.persistentDataPath, "protogenesis_v5_demo_2_0_report.json"); }
        }

        private void Awake()
        {
            BuildMilestones();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.End)) showPanel = !showPanel;
            if (Input.GetKeyDown(KeyCode.PageUp) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) ExportReport();
            if (Input.GetKeyDown(KeyCode.PageDown) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) StabilizeForDemo();
            if (Input.GetKeyDown(KeyCode.Insert) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))) SpawnDemoPressure();

            tick += Time.deltaTime;
            if (tick >= 0.75f)
            {
                tick = 0f;
                EvaluateMilestones();
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;

            Rect r = new Rect(Screen.width - 480, 190, 464, Screen.height - 245);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "DEMO MILESTONE 2.0", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), string.Format("Readiness: {0:0}% | Focus: {1}", DemoReadiness01 * 100f, RecommendedFocus));
            GUI.Box(new Rect(r.x + 12, r.y + 64, r.width - 24, 18), "");
            GUI.Box(new Rect(r.x + 14, r.y + 66, (r.width - 28) * DemoReadiness01, 14), "");
            GUI.Label(new Rect(r.x + 12, r.y + 90, r.width - 24, 34), "Current: " + CurrentMilestone, small);

            if (GUI.Button(new Rect(r.x + 12, r.y + 126, 138, 30), "Stabilize demo", button)) StabilizeForDemo();
            if (GUI.Button(new Rect(r.x + 158, r.y + 126, 138, 30), "Spawn pressure", button)) SpawnDemoPressure();
            if (GUI.Button(new Rect(r.x + 304, r.y + 126, 138, 30), "Export report", button)) ExportReport();

            float y = r.y + 168;
            for (int i = 0; i < milestones.Count; i++)
            {
                Milestone m = milestones[i];
                string prefix = m.complete ? "✓ " : "□ ";
                GUI.Label(new Rect(r.x + 12, y, r.width - 24, 20), prefix + m.label + " — " + (m.progress * 100f).ToString("0") + "%", small);
                GUI.Box(new Rect(r.x + 12, y + 20, r.width - 24, 10), "");
                GUI.Box(new Rect(r.x + 14, y + 22, (r.width - 28) * Mathf.Clamp01(m.progress), 6), "");
                y += 38;
            }

            y += 4;
            GUI.Label(new Rect(r.x + 12, y, r.width - 24, 56), "Hotkeys: End panel | Ctrl+PageDown stabilize | Ctrl+Insert pressure | Ctrl+PageUp export.\nGoal: prove the full loop: build interior → divide → control colony → alter world → claim districts → survive pressure → win.", small);
            if (!string.IsNullOrEmpty(LastExportPath)) GUI.Label(new Rect(r.x + 12, r.y + r.height - 30, r.width - 24, 20), "Last export: " + LastExportPath, small);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.fontSize = 12;
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.9f, 0.95f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 11; button.wordWrap = true;
            small = new GUIStyle(GUI.skin.label); small.fontSize = 11; small.wordWrap = true; small.normal.textColor = Color.white;
        }

        private void BuildMilestones()
        {
            milestones.Clear();
            milestones.Add(new Milestone("LUCA awake", "Select or create mother cell."));
            milestones.Add(new Milestone("Interior online", "Install the genetic compartment, metabolism and synthesis machinery."));
            milestones.Add(new Milestone("Domain chosen", "Pick a metabolism and commit to prokaryote/eukaryote."));
            milestones.Add(new Milestone("Microcolony", "Reach 5 player-owned cells."));
            milestones.Add(new Milestone("RTS control", "Have at least one farmer/explorer/colonizer directive in the colony."));
            milestones.Add(new Milestone("World altered", "Reach 10% average colonization."));
            milestones.Add(new Milestone("District claimed", "Claim at least one world-builder district."));
            milestones.Add(new Milestone("Crisis pressure", "Survive with active enemies or boss pressure."));
            milestones.Add(new Milestone("Endgame path", "Reach 40% colonization, apex, or stable biosphere trajectory."));
        }

        private void EvaluateMilestones()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            V5CellEntity m = gm.MotherCell;
            float colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            V5ColonyDistrictSystem districts = FindFirstObjectByType<V5ColonyDistrictSystem>();

            SetMilestone(0, m != null ? 1f : 0f);
            SetMilestone(1, m != null ? CoreStructureProgress(m) : 0f);
            SetMilestone(2, m != null && m.Domain != V5CellDomain.LUCA ? 1f : 0f);
            SetMilestone(3, Mathf.Clamp01(gm.PlayerCellCount() / 5f));
            SetMilestone(4, DirectiveProgress(gm));
            SetMilestone(5, Mathf.Clamp01(colonization / 0.10f));
            SetMilestone(6, districts != null ? Mathf.Clamp01(districts.ClaimedCount / 1f) : 0f);
            SetMilestone(7, Mathf.Clamp01(gm.NonPlayerCells.Count / 3f));
            float endgame = Mathf.Max(Mathf.Clamp01(colonization / 0.40f), gm.Apex != null && gm.Apex.ApexSpawned ? 1f : 0f, gm.Phase == V5GamePhase.Victory ? 1f : 0f);
            SetMilestone(8, endgame);

            float total = 0f;
            int complete = 0;
            for (int i = 0; i < milestones.Count; i++)
            {
                total += milestones[i].progress;
                if (milestones[i].complete) complete++;
            }
            DemoReadiness01 = milestones.Count > 0 ? total / milestones.Count : 0f;
            CurrentMilestone = FirstIncompleteMilestone();
            RecommendedFocus = FocusFromState(gm, districts, colonization, complete);
        }

        private void SetMilestone(int index, float progress)
        {
            if (index < 0 || index >= milestones.Count) return;
            milestones[index].progress = Mathf.Clamp01(progress);
            milestones[index].complete = milestones[index].progress >= 0.999f;
        }

        private float CoreStructureProgress(V5CellEntity cell)
        {
            int score = 0;
            if (cell.HasStructure(V5StructureId.GeneticCompartment)) score++;
            if (cell.HasStructure(V5StructureId.MetabolicEngine)) score++;
            if (cell.HasStructure(V5StructureId.SynthesisMachinery)) score++;
            return score / 3f;
        }

        private float DirectiveProgress(V5GameManager gm)
        {
            if (gm == null) return 0f;
            bool farm = false;
            bool explore = false;
            bool colonize = false;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                farm |= c.Directive == V5Directive.Farm;
                explore |= c.Directive == V5Directive.Explore;
                colonize |= c.Directive == V5Directive.Colonize;
            }
            int count = (farm ? 1 : 0) + (explore ? 1 : 0) + (colonize ? 1 : 0);
            return count / 3f;
        }

        private string FirstIncompleteMilestone()
        {
            for (int i = 0; i < milestones.Count; i++)
            {
                if (!milestones[i].complete) return milestones[i].description;
            }
            return "Full demo loop complete. Export report and start tuning.";
        }

        private string FocusFromState(V5GameManager gm, V5ColonyDistrictSystem districts, float colonization, int complete)
        {
            if (gm.MotherCell == null) return "Spawn/restore mother cell.";
            if (gm.MotherCell.Domain == V5CellDomain.LUCA) return "Choose metabolism/domain.";
            if (gm.PlayerCellCount() < 5) return "Divide into a microcolony.";
            if (colonization < 0.10f) return "Assign colonizers and engineer habitat.";
            if (districts == null || districts.ClaimedCount < 1) return "Claim the first district.";
            if (gm.NonPlayerCells.Count < 3) return "Add controlled pressure for combat testing.";
            if (colonization < 0.40f && (gm.Apex == null || !gm.Apex.ApexSpawned)) return "Push an endgame victory route.";
            return complete >= milestones.Count ? "Demo loop ready." : "Finish remaining milestones.";
        }

        private void StabilizeForDemo()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity m = gm.MotherCell;
            m.Resources.atp += 80f;
            m.Resources.biomass += 60f;
            m.Resources.aminoAcids += 35f;
            m.Resources.lipids += 35f;
            m.Resources.nucleotides += 20f;
            m.Resources.minerals += 20f;
            m.Stats.currentHp = Mathf.Min(m.Stats.maxHp, m.Stats.currentHp + 50f);
            m.Stats.stress = Mathf.Max(0f, m.Stats.stress - 35f);
            if (gm.Environment != null) gm.Environment.ModifyArea(m.transform.position, 8f, 0.08f, 0.02f, 0.05f, -0.12f, 0f, 0.08f, 0.02f);
            Toast("2.0 demo stabilized around mother cell.");
        }

        private void SpawnDemoPressure()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null || gm.MotherCell == null) return;
            Vector2 center = gm.MotherCell.transform.position;
            for (int i = 0; i < 5; i++)
            {
                Vector2 p = center + UnityEngine.Random.insideUnitCircle.normalized * UnityEngine.Random.Range(10f, 16f);
                V5EvolutionPath path = (i % 3 == 0) ? V5EvolutionPath.Amoeba : ((i % 3 == 1) ? V5EvolutionPath.Bacteria : V5EvolutionPath.Flagellate);
                V5CellEntity enemy = gm.CellFactory.SpawnNeutral(p, path);
                enemy.Directive = V5Directive.Attack;
            }
            Toast("2.0 demo pressure spawned.");
        }

        private void ExportReport()
        {
            V5GameManager gm = V5GameManager.Instance;
            DemoReport report = new DemoReport();
            report.version = "2.0";
            report.timeSeconds = gm != null ? gm.ElapsedSeconds : 0f;
            report.phase = gm != null ? gm.Phase.ToString() : "None";
            report.demoReadiness = DemoReadiness01;
            report.recommendedFocus = RecommendedFocus;
            if (gm != null)
            {
                report.playerCells = gm.PlayerCellCount();
                report.enemyCells = gm.NonPlayerCells.Count;
                report.colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
                report.oxygen = gm.Environment != null ? gm.Environment.AverageOxygen() : 0f;
                report.toxins = gm.Environment != null ? gm.Environment.AverageToxins() : 0f;
                report.acidity = gm.Environment != null ? gm.Environment.AverageAcidity() : 0f;
                report.motherPath = gm.MotherCell != null ? gm.MotherCell.EvolutionPath.ToString() : "None";
                report.motherDomain = gm.MotherCell != null ? gm.MotherCell.Domain.ToString() : "None";
                V5ColonyDistrictSystem districts = FindFirstObjectByType<V5ColonyDistrictSystem>();
                report.claimedDistricts = districts != null ? districts.ClaimedCount : 0;
                report.networkEfficiency = districts != null ? districts.NetworkEfficiency : 0f;
                report.supplyPressure = districts != null ? districts.SupplyPressure : 0f;
            }
            for (int i = 0; i < milestones.Count; i++) report.milestones.Add(milestones[i].label + ":" + (milestones[i].progress * 100f).ToString("0"));
            File.WriteAllText(ReportPath, JsonUtility.ToJson(report, true));
            LastExportPath = ReportPath;
            Toast("2.0 demo report exported.");
        }

        private void Toast(string message)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(message);
        }

        private class Milestone
        {
            public string label;
            public string description;
            public float progress;
            public bool complete;

            public Milestone(string label, string description)
            {
                this.label = label;
                this.description = description;
            }
        }

        [Serializable]
        private class DemoReport
        {
            public string version;
            public float timeSeconds;
            public string phase;
            public float demoReadiness;
            public string recommendedFocus;
            public int playerCells;
            public int enemyCells;
            public float colonization;
            public float oxygen;
            public float toxins;
            public float acidity;
            public string motherPath;
            public string motherDomain;
            public int claimedDistricts;
            public float networkEfficiency;
            public float supplyPressure;
            public List<string> milestones = new List<string>();
        }
    }
}
