using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// Playtest tool and actual design layer: a visible, semi-automatic build order for each evolutionary route.
    /// Toggle with 8. It helps convert the sandbox into guided RTS decisions.
    /// </summary>
    public class V5BuildOrderPlannerIMGUI : MonoBehaviour
    {
        public bool ShowPanel;
        public bool AutoExecute;
        public V5EvolutionPath TargetPath = V5EvolutionPath.Bacteria;
        public string LastAction = "Plan listo.";
        public string NextStep = "";

        private float autoTimer;
        private Vector2 scroll;
        private GUIStyle panelStyle;
        private GUIStyle titleStyle;
        private GUIStyle bodyStyle;
        private GUIStyle smallStyle;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha8)) ShowPanel = !ShowPanel;
            autoTimer += Time.deltaTime;
            if (AutoExecute && autoTimer >= 2.0f)
            {
                autoTimer = 0f;
                ExecuteNextStep(false);
            }
        }

        public void ExecuteNextStep(bool forceToast)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity mother = gm.MotherCell;
            if (!V5RosterBalance.IsPlayablePath(TargetPath)) TargetPath = V5EvolutionPath.Nematode;

            V5GeneId ring1 = RecommendedRing1(TargetPath);
            if (gm.Genes != null && gm.Genes.Ring1 == V5GeneId.None)
            {
                if (gm.Genes.CanUnlock(ring1, mother))
                {
                    gm.Genes.Unlock(ring1, mother);
                    LastAction = "Build order: gen inicial " + ring1;
                    Toast(forceToast);
                    return;
                }
                NextStep = "Reunir recursos/tiempo para gen inicial: " + ring1;
                return;
            }

            V5StructureId[] baseStructures = new V5StructureId[]
            {
                V5StructureId.MetabolicEngine,
                V5StructureId.SynthesisMachinery,
                V5StructureId.Catalase
            };

            for (int i = 0; i < baseStructures.Length; i++)
            {
                if (!mother.HasStructure(baseStructures[i]))
                {
                    TryInstall(mother, baseStructures[i], forceToast);
                    return;
                }
            }

            V5StructureId[] key = RecommendedStructures(TargetPath);
            for (int i = 0; i < key.Length; i++)
            {
                if (!mother.HasStructure(key[i]))
                {
                    TryInstall(mother, key[i], forceToast);
                    return;
                }
            }

            if (gm.Genes != null && gm.Genes.Ring2 == V5GeneId.None)
            {
                V5GeneId g = RecommendedRing2(TargetPath);
                if (gm.Genes.CanUnlock(g, mother))
                {
                    gm.Genes.Unlock(g, mother);
                    LastAction = "Build order: anillo 2 " + g;
                    Toast(forceToast);
                    return;
                }
                NextStep = "Esperar/ahorrar para Anillo 2: " + g;
                return;
            }

            if (gm.Genes != null && gm.Genes.Ring3 == V5GeneId.None)
            {
                V5GeneId g = RecommendedRing3(TargetPath);
                if (gm.Genes.CanUnlock(g, mother))
                {
                    gm.Genes.Unlock(g, mother);
                    LastAction = "Build order: anillo 3 " + g;
                    Toast(forceToast);
                    return;
                }
                NextStep = "Esperar/ahorrar para Anillo 3: " + g;
                return;
            }

            if (gm.Genes != null && gm.Genes.Ring4 == V5GeneId.None)
            {
                V5GeneId g = RecommendedRing4(TargetPath);
                if (gm.Genes.CanUnlock(g, mother))
                {
                    gm.Genes.Unlock(g, mother);
                    LastAction = "Build order: anillo 4 " + g;
                    Toast(forceToast);
                    return;
                }
                NextStep = "Esperar/ahorrar para Anillo 4: " + g;
                return;
            }

            NextStep = "Plan completado. Divide, coloniza o invoca apex.";
            LastAction = NextStep;
        }

        private void TryInstall(V5CellEntity mother, V5StructureId id, bool forceToast)
        {
            if (mother.CanInstall(id))
            {
                mother.InstallStructure(id);
                LastAction = "Build order: estructura " + V5EvolutionLibrary.GetStructure(id).displayName;
                Toast(forceToast);
                return;
            }
            V5StructureDefinition def = V5EvolutionLibrary.GetStructure(id);
            NextStep = "Ahorrar para " + def.displayName + " — " + CostText(def.cost);
        }

        private void Toast(bool forceToast)
        {
            if (!forceToast) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastAction);
        }

        private V5GeneId RecommendedRing1(V5EvolutionPath path)
        {
            if (path == V5EvolutionPath.Cyanobacteria) return V5GeneId.Photosynthesis;
            if (path == V5EvolutionPath.Archaea) return V5GeneId.Chemolithotrophy;
            if (path == V5EvolutionPath.Bacteria) return V5GeneId.Fermentation;
            if (path == V5EvolutionPath.Fungus || path == V5EvolutionPath.SlimeMold) return V5GeneId.Respiration;
            return V5GeneId.Respiration;
        }

        private V5GeneId RecommendedRing2(V5EvolutionPath path)
        {
            if (path == V5EvolutionPath.Bacteria || path == V5EvolutionPath.Fungus || path == V5EvolutionPath.Cyanobacteria || path == V5EvolutionPath.SlimeMold || path == V5EvolutionPath.Microalga) return V5GeneId.Adhesion;
            if (path == V5EvolutionPath.Flagellate || path == V5EvolutionPath.Nematode) return V5GeneId.Motility;
            if (path == V5EvolutionPath.Rotifer || path == V5EvolutionPath.Tardigrade) return V5GeneId.Recognition;
            return V5GeneId.Recognition;
        }

        private V5GeneId RecommendedRing3(V5EvolutionPath path)
        {
            if (path == V5EvolutionPath.Bacteria || path == V5EvolutionPath.Cyanobacteria) return V5GeneId.RapidDivision;
            if (path == V5EvolutionPath.Archaea || path == V5EvolutionPath.Fungus || path == V5EvolutionPath.Ciliate || path == V5EvolutionPath.SlimeMold || path == V5EvolutionPath.Tardigrade) return V5GeneId.Autonomy;
            if (path == V5EvolutionPath.StemCell || path == V5EvolutionPath.Microalga) return V5GeneId.TotalReabsorption;
            return V5GeneId.StrongInheritance;
        }

        private V5GeneId RecommendedRing4(V5EvolutionPath path)
        {
            if (path == V5EvolutionPath.StemCell || path == V5EvolutionPath.Amoeba || path == V5EvolutionPath.Ciliate || path == V5EvolutionPath.Microalga || path == V5EvolutionPath.Rotifer || path == V5EvolutionPath.Nematode || path == V5EvolutionPath.Tardigrade) return V5GeneId.ApexMaturation;
            return V5GeneId.Symbiosis;
        }

        private V5StructureId[] RecommendedStructures(V5EvolutionPath path)
        {
            V5PathDefinition def = V5EvolutionLibrary.GetPath(path);
            return def.keyStructures != null ? def.keyStructures : new V5StructureId[0];
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity m = gm != null ? gm.MotherCell : null;
            if (!V5RosterBalance.IsPlayablePath(TargetPath)) TargetPath = V5EvolutionPath.Nematode;

            Rect r = new Rect(Screen.width - 468f, 82f, 450f, 560f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("BUILD ORDER EVOLUTIVO — 1.7", titleStyle);
            GUILayout.Label("Elige una ruta y deja que el planner recomiende genes/estructuras. Teclas: 8 abre este panel.", bodyStyle);
            AutoExecute = GUILayout.Toggle(AutoExecute, "Auto-ejecutar cuando haya recursos/tiempo");
            GUILayout.Space(8f);

            GUILayout.Label("Ruta objetivo:", bodyStyle);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(190f));
            DrawPathButton(V5EvolutionPath.Bacteria);
            DrawPathButton(V5EvolutionPath.Archaea);
            DrawPathButton(V5EvolutionPath.Cyanobacteria);
            DrawPathButton(V5EvolutionPath.Fungus);
            DrawPathButton(V5EvolutionPath.Amoeba);
            DrawPathButton(V5EvolutionPath.Flagellate);
            DrawPathButton(V5EvolutionPath.Ciliate);
            DrawPathButton(V5EvolutionPath.Microalga);
            DrawPathButton(V5EvolutionPath.SlimeMold);
            DrawPathButton(V5EvolutionPath.Rotifer);
            DrawPathButton(V5EvolutionPath.Nematode);
            GUILayout.EndScrollView();

            GUILayout.Space(8f);
            GUILayout.Label("Plan recomendado:", titleStyle);
            GUILayout.Label(PlanText(), smallStyle);
            if (m != null) GUILayout.Label("Madre: " + m.EvolutionPath + " / " + m.Domain + " / recursos " + ResourceSummary(m), smallStyle);
            GUILayout.Label("Siguiente: " + NextStep, bodyStyle);
            GUILayout.Label("Última acción: " + LastAction, bodyStyle);
            GUILayout.Space(8f);
            if (GUILayout.Button("Ejecutar siguiente paso ahora", GUILayout.Height(32f))) ExecuteNextStep(true);
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndArea();
        }

        private void DrawPathButton(V5EvolutionPath path)
        {
            if (!V5RosterBalance.IsPlayablePath(path)) return;
            GUI.enabled = TargetPath != path;
            if (GUILayout.Button(V5EvolutionRoster.CategoryName(path) + " / " + path, GUILayout.Height(26f))) TargetPath = path;
            GUI.enabled = true;
        }

        private string PlanText()
        {
            V5StructureId[] structures = RecommendedStructures(TargetPath);
            string s = "R1 " + RecommendedRing1(TargetPath) + " → base: Motor/Síntesis/Catalasa → ";
            for (int i = 0; i < structures.Length; i++) s += structures[i] + (i < structures.Length - 1 ? ", " : "");
            s += " → R2 " + RecommendedRing2(TargetPath) + " → R3 " + RecommendedRing3(TargetPath) + " → R4 " + RecommendedRing4(TargetPath);
            return s;
        }

        private string ResourceSummary(V5CellEntity m)
        {
            return "ATP " + m.Resources.atp.ToString("0") + " Bio " + m.Resources.biomass.ToString("0") + " AA " + m.Resources.aminoAcids.ToString("0") + " Lip " + m.Resources.lipids.ToString("0") + " NT " + m.Resources.nucleotides.ToString("0") + " Min " + m.Resources.minerals.ToString("0");
        }

        private string CostText(V5ResourceWallet c)
        {
            return "ATP " + c.atp.ToString("0") + " Bio " + c.biomass.ToString("0") + " AA " + c.aminoAcids.ToString("0") + " Lip " + c.lipids.ToString("0") + " NT " + c.nucleotides.ToString("0") + " Min " + c.minerals.ToString("0");
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            titleStyle = new GUIStyle(GUI.skin.label); titleStyle.fontStyle = FontStyle.Bold; titleStyle.fontSize = 17; titleStyle.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            bodyStyle = new GUIStyle(GUI.skin.label); bodyStyle.wordWrap = true; bodyStyle.normal.textColor = Color.white;
            smallStyle = new GUIStyle(bodyStyle); smallStyle.fontSize = 12;
        }
    }
}
