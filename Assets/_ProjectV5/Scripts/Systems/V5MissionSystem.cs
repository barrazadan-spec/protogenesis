using UnityEngine;

namespace Protogenesis.V5
{
    public class V5MissionSystem : MonoBehaviour
    {
        public int Step { get; private set; }
        public string CurrentObjective { get; private set; }
        public string LastCompleted { get; private set; }
        public float Progress01 { get; private set; }

        private float timer;
        private V5PlayableLoopStage lastLoopStage;

        private void Start()
        {
            Step = 0;
            lastLoopStage = V5PlayableLoopStage.StabilizeMetabolism;
            CurrentObjective = "Abre Genoma (G) e instala una adaptacion barata.";
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < 0.25f) return;
            timer = 0f;
            Evaluate();
        }

        private void Evaluate()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;

            if (gm.PlayableLoop != null)
            {
                Step = (int)gm.PlayableLoop.Stage;
                Progress01 = gm.PlayableLoop.Progress01;
                CurrentObjective = gm.PlayableLoop.Goal;
                if (gm.PlayableLoop.Stage != lastLoopStage)
                {
                    LastCompleted = "Tramo completado: " + lastLoopStage;
                    lastLoopStage = gm.PlayableLoop.Stage;
                }
                return;
            }

            if (Step == 0)
            {
                Progress01 = ActiveAdaptationCount(gm) > 0 ? 1f : 0f;
                CurrentObjective = "Abre Genoma (G) e instala una adaptacion barata.";
                if (Progress01 >= 1f) Complete("Primera adaptacion instalada.");
            }
            else if (Step == 1)
            {
                Progress01 = AdhesionUnlocked(gm) ? 1f : 0f;
                CurrentObjective = "Desbloquea Adesina basica para permitir cuerpo pegado.";
                if (Progress01 >= 1f) Complete("Adhesion inicial desbloqueada.");
            }
            else if (Step == 2)
            {
                Progress01 = Mathf.Clamp01((gm.PlayerCellCount() - 1f) / 2f);
                CurrentObjective = "Dividete hasta tener 3 celulas aliadas. Tecla D.";
                if (Progress01 >= 1f) Complete("Primera microcolonia creada.");
            }
            else if (Step == 3)
            {
                int farmers = CountDirective(gm, V5Directive.Farm);
                int colonizers = CountDirective(gm, V5Directive.Colonize);
                Progress01 = Mathf.Clamp01((farmers + colonizers) / 2f);
                CurrentObjective = "Asigna 2 hijas a farmear o colonizar.";
                if (Progress01 >= 1f) Complete("Economia celular establecida.");
            }
            else if (Step == 4)
            {
                float c = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
                Progress01 = Mathf.Clamp01(c / 0.10f);
                CurrentObjective = "Coloniza 10% de la gota.";
                if (Progress01 >= 1f) Complete("Territorio celular inicial asegurado.");
            }
            else if (Step == 5)
            {
                bool identityReady = gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA;
                Progress01 = identityReady ? 1f : Mathf.Clamp01(ActiveAdaptationCount(gm) / 3f);
                CurrentObjective = "Combina 2-3 adaptaciones hasta que emerja una identidad.";
                if (Progress01 >= 1f) Complete("Linaje con identidad evolutiva.");
            }
            else if (Step == 6)
            {
                Progress01 = Mathf.Clamp01(ActiveAdaptationCount(gm) / 5f);
                CurrentObjective = "Profundiza tu identidad con 5 adaptaciones activas.";
                if (Progress01 >= 1f) Complete("Colonia adaptativa consolidada.");
            }
            else
            {
                float c = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
                Progress01 = Mathf.Clamp01(c / 0.40f);
                CurrentObjective = "Objetivo final: coloniza 40% o estabiliza el ecosistema.";
            }
        }

        private int CountDirective(V5GameManager gm, V5Directive directive)
        {
            int count = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c != null && c.Directive == directive) count++;
            }
            return count;
        }

        private int ActiveAdaptationCount(V5GameManager gm)
        {
            return gm != null && gm.Adaptations != null ? gm.Adaptations.ActiveCount() : 0;
        }

        private bool AdhesionUnlocked(V5GameManager gm)
        {
            if (gm == null) return false;
            if (gm.Body != null && gm.Body.AttachmentUnlocked) return true;
            return gm.Adaptations != null && gm.Adaptations.Has(V5AdaptationId.BasicAdhesin);
        }

        private void Complete(string message)
        {
            LastCompleted = message;
            Step++;
        }
    }
}
