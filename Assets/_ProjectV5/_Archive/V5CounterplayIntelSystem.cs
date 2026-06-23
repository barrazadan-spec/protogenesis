using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CounterplayIntelSystem : MonoBehaviour
    {
        private bool showPanel;
        private float scanTimer;
        private string threatSummary = "sin escaneo";
        private string recommendation = "explora para detectar amenazas";
        private V5StructureId recommendedStructure = V5StructureId.Catalase;
        private V5EvolutionPath dominantThreat = V5EvolutionPath.Uncommitted;
        private V5CellEntity priorityTarget;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Start() => V5PanelRouter.Register("Intel", () => showPanel, v => showPanel = v);

        private void Update()
        {
            // Delete removed — DynamicObjectiveSystem uses it; open via Paneles menu
            scanTimer += Time.deltaTime;
            if (scanTimer >= 1.2f)
            {
                scanTimer = 0f;
                Scan();
            }
        }

        private void Scan()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            int biofilm = 0, extremophile = 0, photo = 0, web = 0, predators = 0, fast = 0, microfauna = 0;
            float nearest = 9999f;
            priorityTarget = null;
            Vector2 origin = gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;

            for (int i = 0; i < gm.NonPlayerCells.Count; i++)
            {
                V5CellEntity e = gm.NonPlayerCells[i];
                if (e == null) continue;
                if (e.EvolutionPath == V5EvolutionPath.Bacteria) biofilm++;
                else if (e.EvolutionPath == V5EvolutionPath.Archaea) extremophile++;
                else if (e.EvolutionPath == V5EvolutionPath.Cyanobacteria || e.EvolutionPath == V5EvolutionPath.Microalga) photo++;
                else if (e.EvolutionPath == V5EvolutionPath.Fungus || e.EvolutionPath == V5EvolutionPath.SlimeMold) web++;
                else if (e.EvolutionPath == V5EvolutionPath.Amoeba) predators++;
                else if (e.EvolutionPath == V5EvolutionPath.Flagellate || e.EvolutionPath == V5EvolutionPath.Ciliate) fast++;
                else if (e.EvolutionPath == V5EvolutionPath.Rotifer || e.EvolutionPath == V5EvolutionPath.Nematode || e.EvolutionPath == V5EvolutionPath.Tardigrade) microfauna++;

                float score = Vector2.Distance(origin, e.transform.position) - e.Stats.physicalDamagePerSecond * 0.55f - e.Stats.chemicalDamagePerSecond * 0.85f;
                if (score < nearest)
                {
                    nearest = score;
                    priorityTarget = e;
                }
            }

            int bestCount = 0;
            dominantThreat = V5EvolutionPath.Uncommitted;
            Consider(V5EvolutionPath.Bacteria, biofilm, ref bestCount);
            Consider(V5EvolutionPath.Archaea, extremophile, ref bestCount);
            Consider(V5EvolutionPath.Cyanobacteria, photo, ref bestCount);
            Consider(V5EvolutionPath.Fungus, web, ref bestCount);
            Consider(V5EvolutionPath.Amoeba, predators, ref bestCount);
            Consider(V5EvolutionPath.Flagellate, fast, ref bestCount);
            Consider(V5EvolutionPath.Rotifer, microfauna, ref bestCount);

            threatSummary = string.Format("Biofilm {0} | Extremos {1} | Foto {2} | Redes {3} | Dep {4} | Rap {5} | Microfauna {6}", biofilm, extremophile, photo, web, predators, fast, microfauna);
            BuildRecommendation(biofilm, extremophile, photo, web, predators, fast, microfauna);
        }

        private void Consider(V5EvolutionPath path, int count, ref int bestCount)
        {
            if (count > bestCount)
            {
                bestCount = count;
                dominantThreat = path;
            }
        }

        private void BuildRecommendation(int biofilm, int extremophile, int photo, int web, int predators, int fast, int microfauna)
        {
            if (biofilm + web + photo >= Mathf.Max(2, predators + fast + microfauna))
            {
                recommendedStructure = V5StructureId.Catalase;
                recommendation = "Mucho dano quimico/ambiental: instala Catalasa y evita pelear dentro de red rival.";
            }
            else if (predators > 0)
            {
                recommendedStructure = V5StructureId.Capsule;
                recommendation = "Depredadores por contacto: capsula/pared y pantalla defensiva alrededor de la madre.";
            }
            else if (fast > 0)
            {
                recommendedStructure = V5StructureId.RecognitionReceptors;
                recommendation = "Amenazas rapidas: reconocimiento + formacion Screen para interceptar.";
            }
            else if (microfauna > 0)
            {
                recommendedStructure = V5StructureId.Cuticle;
                recommendation = "Microfauna avanzada: cuticula, distancia y focus fire antes de que perfore o filtre swarms.";
            }
            else if (extremophile > 0)
            {
                recommendedStructure = V5StructureId.EukaryoticFlagellum;
                recommendation = "Zonas extremas/arqueas: movilidad para evitar nube hostil o terraformar alrededor.";
            }
            else
            {
                recommendedStructure = V5StructureId.SynthesisMachinery;
                recommendation = "Poca amenaza detectada: invierte en sintesis, division y colonizacion.";
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 420, 420, 405, 260);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "INTELIGENCIA DE COUNTERPLAY V5.2", title);
            GUI.Label(new Rect(r.x + 12, r.y + 42, r.width - 24, 20), "Amenaza dominante: " + dominantThreat);
            GUI.Label(new Rect(r.x + 12, r.y + 66, r.width - 24, 20), threatSummary);
            GUI.Label(new Rect(r.x + 12, r.y + 92, r.width - 24, 52), recommendation);
            GUI.Label(new Rect(r.x + 12, r.y + 148, r.width - 24, 20), "Estructura sugerida: " + V5EvolutionLibrary.GetStructure(recommendedStructure).displayName);

            if (GUI.Button(new Rect(r.x + 12, r.y + 176, 180, 30), "Instalar sugerida en madre", button)) TryInstallRecommended();
            if (GUI.Button(new Rect(r.x + 204, r.y + 176, 188, 30), "Atacar prioridad", button)) CommandAttackPriority();
            string target = "ninguna";
            if (priorityTarget != null)
            {
                Vector2 reference = Vector2.zero;
                if (V5GameManager.Instance != null && V5GameManager.Instance.MotherCell != null) reference = V5GameManager.Instance.MotherCell.transform.position;
                target = priorityTarget.EvolutionPath + " a " + Vector2.Distance(priorityTarget.transform.position, reference).ToString("0.0") + "u";
            }
            GUI.Label(new Rect(r.x + 12, r.y + 214, r.width - 24, 20), "Objetivo prioritario: " + target);
        }

        private void TryInstallRecommended()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            if (gm.MotherCell.InstallStructure(recommendedStructure)) Toast("Instalada: " + V5EvolutionLibrary.GetStructure(recommendedStructure).displayName);
            else Toast("No se pudo instalar la estructura sugerida");
        }

        private void CommandAttackPriority()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || priorityTarget == null) return;
            int ordered = 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                if (c.LineageRole == V5LineageRole.Predator || c.Directive == V5Directive.Attack || c.HasPhagocytosis || c.Stats.physicalDamagePerSecond > 1.25f)
                {
                    c.Directive = V5Directive.Attack;
                    c.AttackTarget = priorityTarget;
                    c.DirectiveTarget = priorityTarget.transform.position;
                    ordered++;
                }
            }
            Toast("Ataque de counterplay: " + ordered + " celulas asignadas");
        }

        private void Toast(string text)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(text);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.alignment = TextAnchor.UpperLeft;
            box.normal.textColor = Color.white;
            box.fontSize = 12;
            title = new GUIStyle(GUI.skin.label);
            title.fontSize = 16;
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = new Color(0.85f, 1f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button);
            button.fontSize = 12;
            button.wordWrap = true;
        }
    }
}
