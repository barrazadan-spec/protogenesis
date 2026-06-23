using UnityEngine;

namespace Protogenesis.V5
{
    public class V5EvolutionPlannerIMGUI : MonoBehaviour
    {
        private bool show;
        private Vector2 scroll;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle small;

        private static readonly V5EvolutionPath[] PrimaryRoutes = V5EvolutionRoster.PrimaryRoutes;

        private void Start()
        {
            V5PanelRouter.Register("Planner", () => show, v => show = v);
        }

        private void Update()
        {
            // V is owned by the unified lineage panel; open this from Paneles.
        }

        private void OnGUI()
        {
            if (!show) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity cell = gm.MotherCell;

            Rect r = new Rect(Screen.width - 520, 92, 510, 560);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "PLAN EVOLUTIVO - CANON BIOLOGICO", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 38), Recommendation(cell, gm), small);

            Rect view = new Rect(r.x + 10, r.y + 84, r.width - 20, r.height - 96);
            Rect content = new Rect(0, 0, view.width - 22, PrimaryRoutes.Length * 104f);
            scroll = GUI.BeginScrollView(view, scroll, content);
            float y = 0f;
            for (int i = 0; i < PrimaryRoutes.Length; i++) y = DrawPathBlock(cell, PrimaryRoutes[i], y, gm);
            GUI.EndScrollView();
        }

        private string Recommendation(V5CellEntity cell, V5GameManager gm)
        {
            if (gm.Adaptations != null)
            {
                if (gm.Adaptations.ActiveCount() == 0) return "Primero elige una adaptacion barata en G: Pared, Flagelo, Tilacoide, Bomba de protones o Adesina basica.";
                if (gm.Body != null && !gm.Body.AttachmentUnlocked) return "Antes de hacer unidades pegadas, desbloquea Adesina basica en Genoma (G). Es barata y ensena el cuerpo.";
                if (gm.Identity != null && gm.Identity.Identity == V5IdentityId.LUCA) return "Combina 2-3 adaptaciones para que emerja una identidad: Bacteria, Arquea, Ciano, Protista, Ameba o Microalga.";
                if (gm.Environment != null && gm.Environment.AverageToxins() > 0.35f && !gm.Adaptations.Has(V5AdaptationId.CatalaseROS)) return "Toxinas altas: instala Catalasa/ROS antes de expandirte.";
                if (gm.PlayerCellCount() < 3) return "Falta presencia RTS: divide y asigna hijas a farmear, defender o colonizar.";
                if (gm.Environment != null && gm.Environment.AverageColonization() < 0.18f) return "Cuello de botella: territorio. Usa Colonizar, adhesion, hifas, plasmodio o fotosintesis sostenida.";
                return "Colonia estable: busca adaptaciones T4, cuerpo colonial, forma apex o victoria ecologica.";
            }

            if (cell.Domain == V5CellDomain.LUCA) return "Primero define metabolismo: fermentacion/quimio para procariota, respiracion para eucariota, fotosintesis para cianobacteria.";
            if (gm.Environment != null && gm.Environment.AverageToxins() > 0.35f && !cell.HasStructure(V5StructureId.Catalase)) return "Toxinas altas: instala Catalasa/Peroxisoma antes de expandirte.";
            if (gm.PlayerCellCount() < 3) return "Falta presencia RTS: divide y asigna hijas a farmear, defender o colonizar.";
            if (gm.Genes != null && gm.Genes.UnlockedCount < 2) return "Desbloquea Anillo 2 para orientar funcion: Motilidad, Secrecion, Reconocimiento o Adhesion.";
            if (gm.Environment != null && gm.Environment.AverageColonization() < 0.18f) return "Cuello de botella: territorio. Usa Colonizar, biofilm, hifas, mucilago o fotosintesis sostenida.";
            return "Colonia estable: busca adaptaciones T4, cuerpo colonial, forma apex o victoria ecologica.";
        }

        private float DrawPathBlock(V5CellEntity cell, V5EvolutionPath path, float y, V5GameManager gm)
        {
            V5PathDefinition def = V5EvolutionLibrary.GetPath(path);
            V5EvolutionAffinityResult affinity = V5EvolutionAffinitySystem.Evaluate(cell, path);
            float adaptationScore = gm != null && gm.Adaptations != null ? V5BiologyCanon.RouteAdaptationScore01(path, gm.Adaptations) : 0f;
            float score = Mathf.Max(affinity.Score01, adaptationScore);
            string progressLabel = gm != null && gm.Adaptations != null ? (score * 100f).ToString("0") + "%" : affinity.PercentLabel;
            GUI.Label(new Rect(0, y, 150, 22), def.displayName + " " + progressLabel, small);
            GUI.Box(new Rect(155, y + 3, 190, 14), "");
            GUI.Box(new Rect(157, y + 5, 186 * Mathf.Clamp01(score), 10), "");
            GUI.Label(new Rect(355, y, 130, 22), V5EvolutionRoster.CategoryName(path), small);
            y += 24;

            string keys = gm != null && gm.Adaptations != null ? "Adaptaciones: " : "Estructuras: ";
            if (gm != null && gm.Adaptations != null)
            {
                V5AdaptationId[] canon = V5BiologyCanon.AdaptationsForRoute(path);
                for (int i = 0; i < canon.Length; i++)
                {
                    V5AdaptationDefinition a = V5AdaptationLibrary.Get(canon[i]);
                    bool has = gm.Adaptations.Has(canon[i]);
                    keys += (has ? "[x] " : "[ ] ") + (a != null ? a.shortName : canon[i].ToString());
                    if (i < canon.Length - 1) keys += ", ";
                }
                if (canon.Length == 0) keys += "sin canon P0";
            }
            else
            {
                for (int i = 0; i < def.keyStructures.Length; i++)
                {
                    bool has = cell.HasStructure(def.keyStructures[i]);
                    keys += (has ? "[x] " : "[ ] ") + V5EvolutionLibrary.GetStructure(def.keyStructures[i]).displayName;
                    if (i < def.keyStructures.Length - 1) keys += ", ";
                }
            }
            GUI.Label(new Rect(16, y, 460, 20), keys, small);
            y += 21;
            string scoreText = gm != null && gm.Adaptations != null
                ? "Adaptacion " + (adaptationScore * 100f).ToString("0") + "% | Afinidad: " + affinity.reasons
                : "Afinidad: " + affinity.reasons;
            GUI.Label(new Rect(16, y, 460, 30), scoreText, small);
            y += 30;
            string history = gm != null && gm.AffinityLog != null ? gm.AffinityLog.RouteSummary(path, 2) : "sin historial";
            GUI.Label(new Rect(16, y, 460, 22), "Historial: " + history, small);
            y += 31;
            return y;
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.alignment = TextAnchor.UpperLeft;
            box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label);
            title.fontSize = 16;
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = new Color(0.85f, 1f, 1f, 1f);
            small = new GUIStyle(GUI.skin.label);
            small.fontSize = 12;
            small.wordWrap = true;
            small.normal.textColor = Color.white;
        }
    }
}
