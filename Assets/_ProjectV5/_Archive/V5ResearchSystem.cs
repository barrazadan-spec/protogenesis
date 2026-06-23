using System;
using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5ResearchId
    {
        None,
        EfficientDivision,
        MembraneEconomy,
        GuidedAutonomy,
        EnvironmentalEngineering,
        ApexPreparation,
        HomeostaticControl
    }

    [Serializable]
    public class V5ResearchProject
    {
        public V5ResearchId id;
        public string title;
        public string description;
        public float required;
        public float progress;
        public bool completed;
        public V5ResourceWallet cost;
    }

    public class V5ResearchSystem : MonoBehaviour
    {
        public bool Visible;
        public V5ResearchId ActiveResearch = V5ResearchId.None;
        public readonly List<V5ResearchProject> Projects = new List<V5ResearchProject>();

        private float tick;
        private Vector2 scroll;

        public bool Has(V5ResearchId id)
        {
            for (int i = 0; i < Projects.Count; i++)
                if (Projects[i].id == id) return Projects[i].completed;
            return false;
        }

        public float DivisionCostMultiplier()
        {
            return Has(V5ResearchId.EfficientDivision) ? 0.88f : 1f;
        }

        public float StructureCostMultiplier()
        {
            return Has(V5ResearchId.MembraneEconomy) ? 0.90f : 1f;
        }

        public float ColonizationMultiplier()
        {
            return Has(V5ResearchId.EnvironmentalEngineering) ? 1.18f : 1f;
        }

        public float StressMultiplier()
        {
            return Has(V5ResearchId.HomeostaticControl) ? 0.82f : 1f;
        }

        private void Awake()
        {
            BuildProjects();
        }

        private void Update()
        {
            // End removed — DemoMilestone20System also uses it; open via Paneles menu
            if (V5GameManager.Instance == null || V5GameManager.Instance.Paused) return;

            tick += Time.deltaTime;
            if (tick >= 0.5f)
            {
                tick = 0f;
                TickResearch(0.5f);
                ApplyPassiveEffects();
            }
        }

        private void BuildProjects()
        {
            if (Projects.Count > 0) return;
            Add(V5ResearchId.EfficientDivision, "División eficiente", "Reduce costo de división y permite colonias más grandes antes del colapso.", 95f, V5ResourceWallet.Cost(18, 8, 8, 2, 8, 0));
            Add(V5ResearchId.MembraneEconomy, "Economía de membrana", "Optimiza lípidos y biomasa usados al instalar estructuras.", 90f, V5ResourceWallet.Cost(16, 10, 6, 12, 2, 0));
            Add(V5ResearchId.GuidedAutonomy, "Autonomía guiada", "Mejora comportamiento de hijas en farm/defensa/exploración.", 85f, V5ResourceWallet.Cost(12, 8, 8, 4, 12, 0));
            Add(V5ResearchId.EnvironmentalEngineering, "Ingeniería ambiental", "Aumenta colonización y conversión ecológica del mapa.", 110f, V5ResourceWallet.Cost(20, 12, 8, 8, 8, 8));
            Add(V5ResearchId.ApexPreparation, "Preparación apex", "Reduce fricción para invocar formas apex y estabiliza la transición.", 130f, V5ResourceWallet.Cost(35, 18, 16, 12, 16, 8));
            Add(V5ResearchId.HomeostaticControl, "Control homeostático", "Reduce stress pasivo, deuda adaptativa y daño ambiental indirecto.", 105f, V5ResourceWallet.Cost(24, 10, 14, 8, 10, 4));
        }

        private void Add(V5ResearchId id, string title, string desc, float req, V5ResourceWallet cost)
        {
            Projects.Add(new V5ResearchProject { id = id, title = title, description = desc, required = req, cost = cost });
        }

        private void TickResearch(float dt)
        {
            if (ActiveResearch == V5ResearchId.None) return;
            V5ResearchProject project = Find(ActiveResearch);
            if (project == null || project.completed) return;
            V5CellEntity mother = V5GameManager.Instance.MotherCell;
            if (mother == null) return;

            float rate = 1.0f + mother.Stats.synthesisRate * 0.55f + mother.ActiveStructureCount * 0.08f;
            if (mother.HasStructure(V5StructureId.GeneticCompartment)) rate += 0.6f;
            if (mother.HasStructure(V5StructureId.SynthesisMachinery)) rate += 0.8f;
            if (mother.EvolutionPath == V5EvolutionPath.StemCell) rate += 0.7f;
            project.progress = Mathf.Min(project.required, project.progress + rate * dt);
            if (project.progress >= project.required)
            {
                project.completed = true;
                ActiveResearch = V5ResearchId.None;
                ApplyCompletion(project.id);
                if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Investigación completada: " + project.title);
                if (V5GameManager.Instance.Codex != null) V5GameManager.Instance.Codex.Unlock("Investigación: " + project.title, project.description);
            }
        }

        private V5ResearchProject Find(V5ResearchId id)
        {
            for (int i = 0; i < Projects.Count; i++) if (Projects[i].id == id) return Projects[i];
            return null;
        }

        private void ApplyCompletion(V5ResearchId id)
        {
            V5CellEntity mother = V5GameManager.Instance != null ? V5GameManager.Instance.MotherCell : null;
            if (mother == null) return;
            if (id == V5ResearchId.EfficientDivision) mother.Stats.divisionEfficiency += 0.18f;
            if (id == V5ResearchId.GuidedAutonomy) mother.Stats.sensorRange += 2f;
            if (id == V5ResearchId.EnvironmentalEngineering) mother.Stats.colonizationPower += 0.18f;
            if (id == V5ResearchId.HomeostaticControl) mother.Stats.toxinResistance += 0.08f;
            if (id == V5ResearchId.ApexPreparation) mother.Resources.atp += 35f;
        }

        private void ApplyPassiveEffects()
        {
            if (!Has(V5ResearchId.GuidedAutonomy) && !Has(V5ResearchId.HomeostaticControl) && !Has(V5ResearchId.EnvironmentalEngineering)) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (cell == null) continue;
                if (Has(V5ResearchId.GuidedAutonomy) && cell.Role != V5CellRole.Mother && cell.Directive == V5Directive.Idle) cell.Directive = V5Directive.Farm;
                if (Has(V5ResearchId.HomeostaticControl)) cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 0.12f);
                if (Has(V5ResearchId.EnvironmentalEngineering) && cell.Directive == V5Directive.Colonize && gm.Environment != null)
                    gm.Environment.ModifyArea(cell.transform.position, 1.3f, 0f, 0f, 0.001f, -0.001f, 0f, 0.002f, 0f);
            }
        }

        public void StartResearch(V5ResearchId id)
        {
            V5ResearchProject project = Find(id);
            V5CellEntity mother = V5GameManager.Instance != null ? V5GameManager.Instance.MotherCell : null;
            if (project == null || project.completed || mother == null) return;
            if (project.progress <= 0.01f)
            {
                if (!mother.Resources.CanPay(project.cost))
                {
                    if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Faltan recursos para iniciar investigación");
                    return;
                }
                mother.Resources.Pay(project.cost);
            }
            ActiveResearch = id;
            if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Investigando: " + project.title);
        }

        private void OnGUI()
        {
            if (!Visible) return;
            GUILayout.BeginArea(new Rect(Screen.width - 430, 70, 410, 520), GUI.skin.box);
            GUILayout.Label("INVESTIGACIÓN CELULAR 1.4");
            GUILayout.Label("End: cerrar · Click: investigar");
            V5CellEntity mother = V5GameManager.Instance != null ? V5GameManager.Instance.MotherCell : null;
            if (mother != null) GUILayout.Label("Banco madre ATP " + mother.Resources.atp.ToString("0") + " | Bio " + mother.Resources.biomass.ToString("0") + " | NT " + mother.Resources.nucleotides.ToString("0"));
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(440));
            for (int i = 0; i < Projects.Count; i++)
            {
                V5ResearchProject p = Projects[i];
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.Label((p.completed ? "✓ " : ActiveResearch == p.id ? "▶ " : "○ ") + p.title + " — " + Mathf.RoundToInt((p.progress / Mathf.Max(1f, p.required)) * 100f) + "%");
                GUILayout.Label(p.description);
                GUILayout.Label("Costo inicial: ATP " + p.cost.atp + " Bio " + p.cost.biomass + " AA " + p.cost.aminoAcids + " Lip " + p.cost.lipids + " NT " + p.cost.nucleotides + " Min " + p.cost.minerals);
                if (!p.completed && GUILayout.Button(ActiveResearch == p.id ? "Investigando" : "Iniciar / continuar")) StartResearch(p.id);
                GUILayout.EndVertical();
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }
    }
}
