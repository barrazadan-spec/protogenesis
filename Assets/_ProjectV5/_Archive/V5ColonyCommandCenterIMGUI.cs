using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ColonyCommandCenterIMGUI : MonoBehaviour
    {
        private bool show;
        private V5EvolutionPath targetPath = V5EvolutionPath.Bacteria;
        private string lastAction = "sin orden macro";
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Start() => V5PanelRouter.Register("Comando", () => show, v => show = v);

        private void Update()
        {
            // Alpha0 removed because HabitatEngineeringSystem also uses 0; open via Paneles.
            // F removed because HudIMGUI uses F for follow camera; RecallAll remains in panel.
        }

        private void OnGUI()
        {
            if (!show) return;
            EnsureStyles();
            if (!V5RosterBalance.IsPlayablePath(targetPath)) targetPath = V5EvolutionPath.Nematode;
            Rect r = new Rect(460, 212, 500, 430);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "CENTRO DE COMANDO COLONIAL", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), "Abre con Paneles▼ | último: " + lastAction);
            float y = r.y + 68;
            if (GUI.Button(new Rect(r.x + 12, y, 150, 32), "Modo recolectar", button)) SetAll(V5Directive.Farm, V5LineageRole.Farmer);
            if (GUI.Button(new Rect(r.x + 174, y, 150, 32), "Modo defender", button)) SetAll(V5Directive.Defend, V5LineageRole.Defender);
            if (GUI.Button(new Rect(r.x + 336, y, 150, 32), "Modo explorar", button)) SetAll(V5Directive.Explore, V5LineageRole.Scout);
            y += 38;
            if (GUI.Button(new Rect(r.x + 12, y, 150, 32), "Modo colonizar", button)) SetAll(V5Directive.Colonize, V5LineageRole.Colonizer);
            if (GUI.Button(new Rect(r.x + 174, y, 150, 32), "Modo cazar", button)) SetAll(V5Directive.Attack, V5LineageRole.Predator);
            if (GUI.Button(new Rect(r.x + 336, y, 150, 32), "Recall", button)) RecallAll();
            y += 44;
            if (GUI.Button(new Rect(r.x + 12, y, 230, 34), "Dividir madre", button)) DivideMother();
            if (GUI.Button(new Rect(r.x + 256, y, 230, 34), "Formación defensiva", button)) DefensiveRing();
            y += 52;
            GUI.Label(new Rect(r.x + 12, y, 460, 22), "Plan rápido: " + targetPath, title); y += 28;
            if (GUI.Button(new Rect(r.x + 12, y, 110, 28), "Bacteria", button)) targetPath = V5EvolutionPath.Bacteria;
            if (GUI.Button(new Rect(r.x + 130, y, 110, 28), "Arquea", button)) targetPath = V5EvolutionPath.Archaea;
            if (GUI.Button(new Rect(r.x + 248, y, 110, 28), "Ciano", button)) targetPath = V5EvolutionPath.Cyanobacteria;
            if (GUI.Button(new Rect(r.x + 366, y, 110, 28), "Ameba", button)) targetPath = V5EvolutionPath.Amoeba;
            y += 32;
            if (GUI.Button(new Rect(r.x + 12, y, 110, 28), "Flagelado", button)) targetPath = V5EvolutionPath.Flagellate;
            if (GUI.Button(new Rect(r.x + 130, y, 110, 28), "Ciliado", button)) targetPath = V5EvolutionPath.Ciliate;
            if (GUI.Button(new Rect(r.x + 248, y, 110, 28), "Microalga", button)) targetPath = V5EvolutionPath.Microalga;
            if (GUI.Button(new Rect(r.x + 366, y, 110, 28), "Hongo", button)) targetPath = V5EvolutionPath.Fungus;
            y += 32;
            if (GUI.Button(new Rect(r.x + 12, y, 110, 28), "Moho", button)) targetPath = V5EvolutionPath.SlimeMold;
            if (GUI.Button(new Rect(r.x + 130, y, 110, 28), "Rotifero", button)) targetPath = V5EvolutionPath.Rotifer;
            if (GUI.Button(new Rect(r.x + 248, y, 110, 28), "Nematodo", button)) targetPath = V5EvolutionPath.Nematode;
            GUI.enabled = false;
            GUI.Button(new Rect(r.x + 366, y, 110, 28), "Cripto/NPC", button);
            GUI.enabled = true;
            y += 36;
            if (GUI.Button(new Rect(r.x + 12, y, 230, 34), "Adaptacion sugerida", button)) ApplyRecommendedAdaptation();
            V5GameManager gm = V5GameManager.Instance;
            string structureLabel = gm != null && gm.Adaptations != null ? "Rasgo corporal legacy" : "Instalar estructura clave";
            if (GUI.Button(new Rect(r.x + 256, y, 230, 34), structureLabel, button)) InstallNextStructure();
            y += 48;
            V5ColonyCohesionSystem cohesion = FindFirstObjectByType<V5ColonyCohesionSystem>();
            if (cohesion != null) GUI.Label(new Rect(r.x + 12, y, 460, 22), cohesion.Summary); y += 24;
            V5MorphologyVisualSystem morph = FindFirstObjectByType<V5MorphologyVisualSystem>();
            if (morph != null) GUI.Label(new Rect(r.x + 12, y, 460, 22), "Morfología: " + (morph.MorphologyEnabled ? "ON" : "OFF") + " | Ctrl+F10");
        }

        private void SetAll(V5Directive directive, V5LineageRole role)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                c.ApplyCellMode(V5CellModeLibrary.ModeForDirective(directive));
                c.LineageRole = role;
                c.Mother = gm.MotherCell;
                if (directive == V5Directive.Colonize) c.DirectiveTarget = c.transform.position;
            }
            lastAction = "hijas -> " + V5CellModeLibrary.Get(V5CellModeLibrary.ModeForDirective(directive)).displayName;
            Toast(lastAction);
        }

        private void RecallAll()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                c.ApplyCellMode(V5CellModeId.FollowLineage);
                c.Mother = gm.MotherCell;
                c.DirectiveTarget = gm.MotherCell.transform.position;
            }
            lastAction = "recall global";
            Toast(lastAction);
        }

        private void DivideMother()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity child = gm.MotherCell.Divide();
            lastAction = child != null ? "madre dividida" : "madre no puede dividirse";
            Toast(lastAction);
        }

        private void DefensiveRing()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            int index = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null || c.Role == V5CellRole.Mother) continue;
                float a = index * 137.5f * Mathf.Deg2Rad;
                c.Directive = V5Directive.Move;
                c.LineageRole = V5LineageRole.Defender;
                c.DirectiveTarget = (Vector2)gm.MotherCell.transform.position + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * 5f;
                index++;
            }
            lastAction = "anillo defensivo";
            Toast(lastAction);
        }

        private void ApplyRecommendedAdaptation()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            if (gm.Adaptations == null)
            {
                ApplyRecommendedMetabolicGene();
                return;
            }

            V5AdaptationId[] plan = AdaptationPlanFor(targetPath);
            for (int i = 0; i < plan.Length; i++)
            {
                if (gm.Adaptations.Has(plan[i])) continue;
                string reason;
                if (gm.Adaptations.CanInstall(plan[i], gm.MotherCell, out reason))
                {
                    V5AdaptationDefinition def = V5AdaptationLibrary.Get(plan[i]);
                    bool ok = gm.Adaptations.Install(plan[i], gm.MotherCell);
                    lastAction = ok && def != null ? "adaptacion para " + targetPath + ": " + def.shortName : gm.Adaptations.LastMessage;
                    Toast(lastAction);
                    return;
                }
                V5AdaptationDefinition blocked = V5AdaptationLibrary.Get(plan[i]);
                lastAction = "bloqueada " + (blocked != null ? blocked.shortName : plan[i].ToString()) + ": " + reason;
                Toast(lastAction);
                return;
            }

            lastAction = "plan de adaptaciones completo para " + targetPath;
            Toast(lastAction);
        }

        private V5AdaptationId[] AdaptationPlanFor(V5EvolutionPath path)
        {
            if (path == V5EvolutionPath.Bacteria)
                return new[] { V5AdaptationId.BacterialWall, V5AdaptationId.BacterialFlagellum, V5AdaptationId.RapidDivision, V5AdaptationId.BasicAdhesin };
            if (path == V5EvolutionPath.Archaea)
                return new[] { V5AdaptationId.ExtremophileMembrane, V5AdaptationId.ProtonPump, V5AdaptationId.CatalaseROS, V5AdaptationId.BasicAdhesin };
            if (path == V5EvolutionPath.Cyanobacteria)
                return new[] { V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.CatalaseROS, V5AdaptationId.BasicAdhesin, V5AdaptationId.ColonialAdhesin };
            if (path == V5EvolutionPath.Amoeba)
                return new[] { V5AdaptationId.BacterialWall, V5AdaptationId.BasicAdhesin, V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods };
            if (path == V5EvolutionPath.Flagellate)
                return new[] { V5AdaptationId.BacterialWall, V5AdaptationId.BasicAdhesin, V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.EukaryoticFlagellum };
            if (path == V5EvolutionPath.Ciliate)
                return new[] { V5AdaptationId.BacterialWall, V5AdaptationId.BasicAdhesin, V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.Cilia, V5AdaptationId.ContractileVacuole };
            if (path == V5EvolutionPath.Microalga)
                return new[] { V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.BasicAdhesin, V5AdaptationId.Nucleus, V5AdaptationId.Chloroplast, V5AdaptationId.CelluloseWall };
            if (path == V5EvolutionPath.Fungus)
                return new[] { V5AdaptationId.BacterialWall, V5AdaptationId.BasicAdhesin, V5AdaptationId.Nucleus, V5AdaptationId.ColonialAdhesin, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes };
            if (path == V5EvolutionPath.SlimeMold)
                return new[] { V5AdaptationId.BacterialWall, V5AdaptationId.BasicAdhesin, V5AdaptationId.Nucleus, V5AdaptationId.ColonialAdhesin, V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory };
            return new[] { V5AdaptationId.BacterialWall, V5AdaptationId.BasicAdhesin, V5AdaptationId.Nucleus, V5AdaptationId.SignalingCommunication };
        }

        private void ApplyRecommendedMetabolicGene()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5PathDefinition def = V5EvolutionLibrary.GetPath(targetPath);
            V5GeneId gene = V5GeneId.Respiration;
            if (def.path == V5EvolutionPath.Cyanobacteria || def.path == V5EvolutionPath.Microalga) gene = V5GeneId.Photosynthesis;
            else if (def.path == V5EvolutionPath.Archaea) gene = V5GeneId.Chemolithotrophy;
            else if (def.domain == V5CellDomain.Prokaryote) gene = V5GeneId.Fermentation;

            if (gm.Genes != null)
            {
                if (gm.Genes.HasGene(gene))
                {
                    lastAction = "gen metabolico ya activo: " + gene;
                }
                else
                {
                    bool ok = gm.Genes.Unlock(gene, gm.MotherCell);
                    lastAction = ok ? "gen metabolico para " + def.displayName : gm.Genes.LastMessage;
                }
                Toast(lastAction);
                return;
            }

            if (gene == V5GeneId.Photosynthesis) gm.MotherCell.ApplyMetabolism(V5MetabolismType.Photosynthesis);
            else if (gene == V5GeneId.Chemolithotrophy) gm.MotherCell.ApplyMetabolism(V5MetabolismType.Chemolithotrophy);
            else if (gene == V5GeneId.Fermentation) gm.MotherCell.ApplyMetabolism(V5MetabolismType.Fermentation);
            else gm.MotherCell.ApplyMetabolism(V5MetabolismType.Respiration);
            lastAction = "metabolismo legacy para " + def.displayName;
            Toast(lastAction);
        }

        private void InstallNextStructure()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5PathDefinition def = V5EvolutionLibrary.GetPath(targetPath);
            for (int i = 0; i < def.keyStructures.Length; i++)
            {
                V5StructureId id = def.keyStructures[i];
                if (gm.MotherCell.HasStructure(id)) continue;
                bool ok = gm.MotherCell.InstallStructure(id);
                lastAction = ok ? "instalada " + V5EvolutionLibrary.GetStructure(id).displayName : "faltan recursos/dominio";
                Toast(lastAction);
                return;
            }
            lastAction = "ruta clave completa";
            Toast(lastAction);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.fontSize = 13; box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label); title.fontSize = 16; title.fontStyle = FontStyle.Bold; title.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 12; button.wordWrap = true;
        }

        private void Toast(string text)
        {
            if (V5GameManager.Instance != null && V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast(text);
        }
    }
}
