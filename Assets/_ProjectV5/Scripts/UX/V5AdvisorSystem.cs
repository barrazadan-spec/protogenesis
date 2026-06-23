using UnityEngine;

namespace Protogenesis.V5
{
    public class V5AdvisorSystem : MonoBehaviour
    {
        public string CurrentAdvice { get; private set; }
        public string RecommendedAction { get; private set; }
        public V5StructureId RecommendedStructure { get; private set; }
        public V5GeneId RecommendedGene { get; private set; }
        public float Confidence01 { get; private set; }
        public bool UsingDiagnosticAdvice { get; private set; }
        public string DiagnosticStatus { get; private set; }

        private float timer;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < 1.5f) return;
            timer = 0f;
            Recalculate();
        }

        public void Recalculate()
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity m = gm != null ? gm.MotherCell : null;
            if (m == null)
            {
                Set("Sin celula madre detectada.", "Reinicia la escena o carga una run.", V5StructureId.GeneticCompartment, V5GeneId.None, 0.2f);
                return;
            }

            if (gm.Adaptations != null)
            {
                RecalculateAdaptiveAdvice(gm, m);
                return;
            }

            RecalculateLegacyAdvice(gm, m);
        }

        private void RecalculateAdaptiveAdvice(V5GameManager gm, V5CellEntity m)
        {
            UsingDiagnosticAdvice = false;
            DiagnosticStatus = gm.Diagnostics != null ? gm.Diagnostics.ShortStatus : "";

            if (gm.Adaptations.ActiveCount() == 0)
            {
                V5EnvironmentGrid env = gm.Environment;
                float light = env != null ? env.Sample(V5OverlayMode.Light, m.transform.position) : 0.5f;
                float oxy = env != null ? env.Sample(V5OverlayMode.Oxygen, m.transform.position) : 0.3f;
                if (light > 0.55f)
                    Set("Zona iluminada: tilacoide temprano convierte luz en economia.", "Abre Genoma (G) y activa Tilacoide procariota.", V5StructureId.Thylakoid, V5GeneId.Photosynthesis, 0.86f);
                else if (oxy < 0.22f)
                    Set("Zona pobre en oxigeno: una bomba de protones o membrana extremofila encaja bien.", "Abre Genoma (G) y activa Bomba de protones o Membrana extremofila.", V5StructureId.Catalase, V5GeneId.Chemolithotrophy, 0.82f);
                else
                    Set("Necesitas una primera decision biologica simple.", "Abre Genoma (G) y activa Pared bacteriana, Flagelo o Adesina basica.", V5StructureId.PeptidoglycanWall, V5GeneId.None, 0.88f);
                return;
            }

            if (TryUseDiagnosticAdvice(gm))
            {
                return;
            }

            if (gm.Body != null && !gm.Body.AttachmentUnlocked && gm.PlayerCellCount() >= 2)
            {
                Set("Ya tienes hijas: falta pegamento biologico para formar cuerpo.", "Activa Adesina basica en Genoma (G). Es barata y ensena el sistema de cuerpo.", V5StructureId.Fimbriae, V5GeneId.Adhesion, 0.92f);
                return;
            }

            if (gm.Environment != null && gm.Environment.AverageToxins() > 0.35f && !gm.Adaptations.Has(V5AdaptationId.CatalaseROS))
            {
                Set("El ambiente esta acumulando toxinas.", "Activa Catalasa/ROS antes de expandir mas territorio.", V5StructureId.Catalase, V5GeneId.TotalReabsorption, 0.88f);
                return;
            }

            if (!gm.Adaptations.Has(V5AdaptationId.Nucleus) && gm.Adaptations.CountInTier(V5AdaptationTier.T1Prokaryote) >= 2)
            {
                Set("Puedes quedarte procariota o dar el salto eucariota.", "Activa Nucleo si quieres Ameba, Ciliado, Flagelado, Hongo, Moho o Microalga.", V5StructureId.GeneticCompartment, V5GeneId.Respiration, 0.76f);
                return;
            }

            if (gm.PlayerCellCount() < 3 && m.CanDivide())
            {
                Set("Tienes recursos para presencia RTS.", "Divide la madre y asigna hijas a farmear, defender o colonizar.", V5StructureId.StorageVacuole, V5GeneId.None, 0.84f);
                return;
            }

            if (m.Stats.stress > 70f)
            {
                Set("Stress alto: la colonia esta cerca de colapso.", "Reduce poblacion, repara o activa una adaptacion de homeostasis.", V5StructureId.Catalase, V5GeneId.TotalReabsorption, 0.90f);
                return;
            }

            if (gm.Environment != null && gm.Environment.AverageColonization() < 0.12f && gm.PlayerCellCount() >= 3)
            {
                Set("Tienes celulas, pero poco territorio.", "Usa modo Colonizar o refuerza adhesion, hifas, plasmodio o fotosintesis.", V5StructureId.Fimbriae, V5GeneId.Adhesion, 0.80f);
                return;
            }

            if (gm.NonPlayerCells.Count > gm.PlayerCellCount() + 4)
            {
                Set("La presion enemiga esta subiendo.", "Crea defensores o toma Lisosoma, Seudopodos, Cilios o Flagelo.", V5StructureId.Lysosome, V5GeneId.Motility, 0.78f);
                return;
            }

            if (gm.Identity != null && gm.Identity.SuggestedNext != V5AdaptationId.None)
            {
                V5AdaptationDefinition next = V5AdaptationLibrary.Get(gm.Identity.SuggestedNext);
                if (next != null && !gm.Adaptations.Has(next.id))
                {
                    Set("Identidad actual: " + gm.Identity.DisplayName + ".", "Siguiente adaptacion sugerida: " + next.displayName + ".", V5StructureId.RecognitionReceptors, V5GeneId.None, 0.64f);
                    return;
                }
            }

            if (!gm.Adaptations.Has(V5AdaptationId.BiologicalChampion) && gm.ElapsedSeconds > 650f)
            {
                Set("Tu colonia ya puede pensar en late game.", "Busca Comunicacion/Senales y luego Campeon biologico.", V5StructureId.StemPlasticity, V5GeneId.ApexMaturation, 0.70f);
                return;
            }

            Set("La colonia esta estable.", "Expande territorio, profundiza adaptaciones o busca una condicion de victoria.", V5StructureId.StorageVacuole, V5GeneId.Symbiosis, 0.55f);
        }

        private bool TryUseDiagnosticAdvice(V5GameManager gm)
        {
            if (gm == null || gm.Diagnostics == null) return false;
            gm.Diagnostics.Scan();
            DiagnosticStatus = gm.Diagnostics.ShortStatus;
            if (gm.Diagnostics.WarningCount <= 0) return false;
            if (gm.Diagnostics.Score >= 95 && gm.Diagnostics.WarningCount <= 1) return false;

            UsingDiagnosticAdvice = true;
            Set("Diagnostico: " + gm.Diagnostics.ShortStatus + " (" + gm.Diagnostics.WarningCount + " alertas). " + gm.Diagnostics.CoachAdvice,
                gm.Diagnostics.CoachAction,
                V5StructureId.RecognitionReceptors,
                V5GeneId.None,
                Mathf.Clamp01(1f - gm.Diagnostics.Score / 140f));
            return true;
        }

        private void RecalculateLegacyAdvice(V5GameManager gm, V5CellEntity m)
        {
            UsingDiagnosticAdvice = false;
            DiagnosticStatus = "";

            if (!m.HasStructure(V5StructureId.GeneticCompartment))
            {
                Set("La madre aun no tiene centro de comando.", "Instala Compartimento Genetico.", V5StructureId.GeneticCompartment, V5GeneId.None, 0.95f);
                return;
            }
            if (!m.HasStructure(V5StructureId.MetabolicEngine))
            {
                Set("Tu economia depende del motor metabolico.", "Instala Motor Metabolico para generar ATP estable.", V5StructureId.MetabolicEngine, V5GeneId.None, 0.95f);
                return;
            }
            if (gm.Genes != null && gm.Genes.Ring1 == V5GeneId.None)
            {
                V5EnvironmentGrid env = gm.Environment;
                float light = env != null ? env.Sample(V5OverlayMode.Light, m.transform.position) : 0.5f;
                float oxy = env != null ? env.Sample(V5OverlayMode.Oxygen, m.transform.position) : 0.3f;
                if (light > 0.55f) Set("Estas en zona iluminada: fotosintesis tiene buen valor.", "Activa Gen Fotosintesis en G.", V5StructureId.Thylakoid, V5GeneId.Photosynthesis, 0.82f);
                else if (oxy > 0.28f) Set("Hay oxigeno suficiente para una ruta eficiente.", "Activa Gen Respiracion aerobica.", V5StructureId.MetabolicEngine, V5GeneId.Respiration, 0.75f);
                else Set("La zona es pobre en oxigeno: conviene ruta procariota.", "Activa Fermentacion o Quimiolitotrofia.", V5StructureId.Catalase, V5GeneId.Fermentation, 0.78f);
                return;
            }
            Set("La colonia esta estable.", "Expande territorio, mejora genes o busca una condicion de victoria.", V5StructureId.StorageVacuole, V5GeneId.Symbiosis, 0.55f);
        }

        private void Set(string advice, string action, V5StructureId structure, V5GeneId gene, float confidence)
        {
            CurrentAdvice = advice;
            RecommendedAction = action;
            RecommendedStructure = structure;
            RecommendedGene = gene;
            Confidence01 = confidence;
        }
    }
}
