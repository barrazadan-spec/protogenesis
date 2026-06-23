using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5EvolutionRouteBoardSystem : MonoBehaviour
    {
        public bool Visible;
        private Vector2 scroll;
        private readonly List<V5EvolutionPath> order = new List<V5EvolutionPath>(V5EvolutionRoster.PrimaryRoutes);

        private void Start() => V5PanelRouter.Register("Evolucion", () => Visible, v => Visible = v);

        private void Update()
        {
            // Open from Paneles; this is the primary upgrade/evolution hub.
        }

        private void OnGUI()
        {
            if (!Visible) return;
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            GUILayout.BeginArea(new Rect(20, 212, 480, 500), GUI.skin.box);
            GUILayout.Label("CENTRO DE EVOLUCION");
            GUILayout.Label("Elige una ruta: define metabolismo, instala estructuras y activa genes desde un solo lugar.");
            if (mother == null)
            {
                GUILayout.Label("Sin celula madre.");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label("Actual: " + mother.EvolutionPath + " | Dominio: " + mother.Domain + " | Metabolismo: " + mother.Metabolism);
            GUILayout.Label("Regla de colonia: la madre concentra banco/genoma; las hijas heredan al dividir y luego pueden especializarse como unidades.");
            GUILayout.Label(Recommendation(mother, gm));
            DrawMutationSection();
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(360));
            for (int i = 0; i < order.Count; i++) DrawPathCard(mother, order[i]);
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawMutationSection()
        {
            V5MutationDraftSystem draft = FindFirstObjectByType<V5MutationDraftSystem>();
            if (draft == null) return;
            GUILayout.BeginHorizontal(GUI.skin.box);
            string state = draft.HasPendingDraft ? ("Mutaciones pendientes: " + draft.PendingDrafts) : "Mutaciones: sin draft pendiente";
            if (!string.IsNullOrEmpty(draft.LastMutation)) state += " | Ultima: " + draft.LastMutation;
            GUILayout.Label(state);
            if (GUILayout.Button("Abrir", GUILayout.Width(70f))) draft.OpenPanel();
            GUILayout.EndHorizontal();
        }

        private void DrawPathCard(V5CellEntity mother, V5EvolutionPath path)
        {
            if (!V5RosterBalance.IsPlayablePath(path)) return;
            V5PathDefinition def = V5EvolutionLibrary.GetPath(path);
            V5EvolutionAffinityResult affinity = V5EvolutionAffinitySystem.Evaluate(mother, path);
            bool domainCompatible = mother.Domain == V5CellDomain.LUCA || mother.Domain == def.domain || path == V5EvolutionPath.Cyanobacteria || (mother.Domain == V5CellDomain.Eukaryote && def.domain == V5CellDomain.Multicellular) || (mother.Domain == V5CellDomain.Multicellular && def.domain == V5CellDomain.Eukaryote);
            int owned = 0;
            for (int i = 0; i < def.keyStructures.Length; i++) if (mother.HasStructure(def.keyStructures[i])) owned++;

            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(def.displayName + " - " + def.fantasy);
            GUILayout.Label("Dominio: " + def.domain + " | Progreso estructural: " + owned + "/" + def.keyStructures.Length + (domainCompatible ? "" : " | dominio incompatible"));
            GUILayout.Label("Afinidad: " + affinity.PercentLabel + " | " + affinity.reasons);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.AffinityLog != null) GUILayout.Label("Historial: " + gm.AffinityLog.RouteSummary(path, 2));
            GUILayout.Label("Estructuras clave:");
            for (int i = 0; i < def.keyStructures.Length; i++)
            {
                V5StructureDefinition s = V5EvolutionLibrary.GetStructure(def.keyStructures[i]);
                GUILayout.Label("  " + (mother.HasStructure(s.id) ? "[x] " : "[ ] ") + s.displayName);
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button(affinity.Score01 >= V5Balance.RouteConsolidationAffinityThreshold ? "Consolidar" : "Afinidad insuf."))
            {
                if (affinity.Score01 >= V5Balance.RouteConsolidationAffinityThreshold)
                {
                    mother.ApplyPath(path, true);
                    if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Ruta consolidada: " + def.displayName);
                }
                else if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Faltan senales para " + def.displayName + ": " + affinity.reasons);
            }
            if (GUILayout.Button("Metabolismo")) PickMetabolismFor(mother, path);
            if (GUILayout.Button("Estructura")) InstallNext(mother, def);
            if (GUILayout.Button("Gen")) UnlockRecommendedGene(mother, path);
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }

        private string Recommendation(V5CellEntity mother, V5GameManager gm)
        {
            if (mother == null) return "";
            if (mother.Metabolism == V5MetabolismType.None) return "Paso recomendado: elige una ruta y define metabolismo. Eso decide dominio y desbloquea el plan.";
            if (!mother.HasStructure(V5StructureId.MetabolicEngine) || !mother.HasStructure(V5StructureId.SynthesisMachinery)) return "Paso recomendado: instala Motor Metabolico y Maquinaria de Sintesis antes de expandirte.";
            if (gm != null && gm.Genes != null && gm.Genes.Ring1 == V5GeneId.None) return "Paso recomendado: activa un gen de Anillo 1 para consolidar el metabolismo elegido.";
            if (gm != null && gm.PlayerCellCount() < 3) return "Paso recomendado: divide con D y manda hijas a farmear o colonizar.";
            if (gm != null && gm.Genes != null && gm.Genes.Ring2 == V5GeneId.None) return "Paso recomendado: activa un gen funcional de Anillo 2 segun tu ruta.";
            return "Paso recomendado: completa estructuras clave, coloniza y prepara Anillos 3-4.";
        }

        private void InstallNext(V5CellEntity mother, V5PathDefinition def)
        {
            for (int i = 0; i < def.keyStructures.Length; i++)
            {
                V5StructureId id = def.keyStructures[i];
                if (mother.HasStructure(id)) continue;
                if (mother.CanInstall(id))
                {
                    mother.InstallStructure(id);
                    if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Instalada: " + V5EvolutionLibrary.GetStructure(id).displayName);
                    return;
                }
                if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("No alcanza para: " + V5EvolutionLibrary.GetStructure(id).displayName);
                return;
            }
            if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Ruta estructural completa");
        }

        private void PickMetabolismFor(V5CellEntity mother, V5EvolutionPath path)
        {
            if (path == V5EvolutionPath.Bacteria) mother.ApplyMetabolism(V5MetabolismType.Fermentation);
            else if (path == V5EvolutionPath.Archaea) mother.ApplyMetabolism(V5MetabolismType.Chemolithotrophy);
            else if (path == V5EvolutionPath.Cyanobacteria) mother.ApplyMetabolism(V5MetabolismType.Photosynthesis);
            else if (path == V5EvolutionPath.Microalga || path == V5EvolutionPath.Fungus || path == V5EvolutionPath.SlimeMold) mother.ApplyMetabolism(V5MetabolismType.Respiration);
            else mother.ApplyMetabolism(V5MetabolismType.Respiration);
            if (V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Metabolismo aplicado: " + mother.Metabolism);
        }

        private void UnlockRecommendedGene(V5CellEntity mother, V5EvolutionPath path)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Genes == null || mother == null) return;
            V5GeneId gene = RecommendedGene(gm.Genes, path);
            if (gene == V5GeneId.None)
            {
                if (gm.Hud != null) gm.Hud.Toast("No hay gen recomendado disponible todavia.");
                return;
            }

            if (gm.Genes.Unlock(gene, mother))
            {
                if (gm.Hud != null) gm.Hud.Toast("Gen activado: " + gm.Genes.Get(gene).name);
            }
            else if (gm.Hud != null) gm.Hud.Toast(gm.Genes.LastMessage);
        }

        private V5GeneId RecommendedGene(V5GeneSystem genes, V5EvolutionPath path)
        {
            if (genes.Ring1 == V5GeneId.None)
            {
                if (path == V5EvolutionPath.Cyanobacteria || path == V5EvolutionPath.Microalga) return V5GeneId.Photosynthesis;
                if (path == V5EvolutionPath.Archaea) return V5GeneId.Chemolithotrophy;
                if (path == V5EvolutionPath.Bacteria) return V5GeneId.Fermentation;
                return V5GeneId.Respiration;
            }
            if (genes.Ring2 == V5GeneId.None)
            {
                if (path == V5EvolutionPath.Flagellate || path == V5EvolutionPath.Ciliate || path == V5EvolutionPath.Rotifer || path == V5EvolutionPath.Nematode) return V5GeneId.Motility;
                if (path == V5EvolutionPath.Fungus || path == V5EvolutionPath.SlimeMold || path == V5EvolutionPath.Cyanobacteria || path == V5EvolutionPath.Microalga) return V5GeneId.Adhesion;
                if (path == V5EvolutionPath.Amoeba) return V5GeneId.Recognition;
                return V5GeneId.Secretion;
            }
            if (genes.Ring3 == V5GeneId.None) return V5GeneId.StrongInheritance;
            if (genes.Ring4 == V5GeneId.None) return path == V5EvolutionPath.Tardigrade || path == V5EvolutionPath.Rotifer || path == V5EvolutionPath.Nematode ? V5GeneId.ApexMaturation : V5GeneId.Symbiosis;
            return V5GeneId.None;
        }
    }
}
