using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CorePredatorSystem : MonoBehaviour
    {
        public KeyCode DebugSpawnKey = KeyCode.K;
        public float FirstPredatorDelaySeconds = 45f;
        public string LastMessage = "Depredador listo: K.";
        public bool FirstPredatorSpawned { get; private set; }
        public bool FirstPredatorDefeated { get; private set; }

        private V5OrganismMorph firstPredatorOrganism;

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode) return;
            if (!RivalColonyModeActive()) TickFirstPredatorProgression(gm);
            if (Input.GetKeyDown(DebugSpawnKey)) SpawnDebugPredator(gm);
        }

        private bool RivalColonyModeActive()
        {
            V5CoreRivalColonySystem rival = FindFirstObjectByType<V5CoreRivalColonySystem>();
            return rival != null && rival.EnableRivalColony;
        }

        public V5CellEntity SpawnDebugPredator(V5GameManager gm)
        {
            V5OrganismMorph organism = SpawnPredatorOrganism(gm, false);
            return organism != null ? organism.NucleusCell : null;
        }

        private void TickFirstPredatorProgression(V5GameManager gm)
        {
            if (gm == null) return;
            if (!FirstPredatorSpawned && !FirstPredatorDefeated && gm.ElapsedSeconds >= FirstPredatorDelaySeconds)
            {
                firstPredatorOrganism = SpawnPredatorOrganism(gm, true);
                if (firstPredatorOrganism != null && firstPredatorOrganism.NucleusCell != null)
                {
                    FirstPredatorSpawned = true;
                    LastMessage = "Primer Tardigrado enemigo: derrotalo para desbloquear el Tardigrado.";
                    Push(LastMessage, firstPredatorOrganism.NucleusCell.transform.position);
                }
            }

            if (!FirstPredatorSpawned || FirstPredatorDefeated) return;
            if (firstPredatorOrganism != null && firstPredatorOrganism.IsAlive) return;
            FirstPredatorDefeated = true;
            UnlockTardigrade(gm);
        }

        private V5OrganismMorph SpawnPredatorOrganism(V5GameManager gm, bool progressionPredator)
        {
            if (gm == null || gm.CellFactory == null || gm.Environment == null || gm.MotherCell == null || gm.OrganismMorph == null) return null;

            V5OrganismMorph existing = ActivePredatorOrganism(gm);
            if (progressionPredator && existing != null && existing.NucleusCell != null)
            {
                LastMessage = "Tardigrado enemigo ya activo.";
                Push(LastMessage, existing.NucleusCell.transform.position);
                return existing;
            }

            Vector2 direction = Random.insideUnitCircle.normalized;
            if (direction.sqrMagnitude < 0.01f) direction = Vector2.right;
            Vector2 spawn = direction * gm.Environment.MapRadius * 0.84f;

            V5OrganismMorph organism = gm.OrganismMorph.SpawnEnemyOrganism(V5OrganismBlueprintKind.Tardigrade, spawn, 16);
            if (organism == null || organism.NucleusCell == null) return null;
            organism.NucleusCell.Directive = V5Directive.Attack;
            organism.NucleusCell.AttackTarget = gm.MotherCell;
            organism.NucleusCell.DirectiveTarget = gm.MotherCell.transform.position;
            if (progressionPredator) firstPredatorOrganism = organism;

            LastMessage = progressionPredator ? "Primer Tardigrado enemigo: desbloquea evolucion al caer." : "Tardigrado enemigo rojo generado en el borde.";
            Push(LastMessage, organism.NucleusCell.transform.position);
            return organism;
        }

        private void UnlockTardigrade(V5GameManager gm)
        {
            V5OrganismMorph morph = gm != null ? gm.OrganismMorph : null;
            if (morph == null) morph = FindFirstObjectByType<V5OrganismMorph>();
            bool newlyUnlocked = morph != null && morph.UnlockTardigrade();
            LastMessage = "¡Evolución desbloqueada: Tardígrado!";

            Vector2 world = gm != null && gm.MotherCell != null ? gm.MotherCell.transform.position : Vector2.zero;
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null)
            {
                feedback.Push("¡Evolución desbloqueada: Tardígrado!", world, new Color(1f, 0.78f, 0.22f, 1f));
                if (newlyUnlocked) feedback.PushFloating("Tardígrado disponible", world + Vector2.up * 0.9f, new Color(1f, 0.9f, 0.35f, 1f));
            }
        }

        private V5OrganismMorph ActivePredatorOrganism(V5GameManager gm)
        {
            V5OrganismMorph morph = gm != null ? gm.OrganismMorph : null;
            if (morph == null) morph = FindFirstObjectByType<V5OrganismMorph>();
            return morph != null ? morph.FirstActiveEnemyOrganism() : null;
        }

        private void Push(string message, Vector2 world)
        {
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push(message, world, new Color(1f, 0.28f, 0.22f, 1f));
        }
    }

}
