using UnityEngine;

namespace Protogenesis.V5
{
    [RequireComponent(typeof(V5CellEntity))]
    public class V5EnemyBrain : MonoBehaviour
    {
        private V5CellEntity cell;
        private float timer;
        private Vector2 wander;

        private void Awake()
        {
            cell = GetComponent<V5CellEntity>();
            wander = Random.insideUnitCircle.normalized;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < 0.35f) return;
            timer = 0f;
            Think();
        }

        private void Think()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5CellEntity nearest = null;
            float best = 999f;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity p = gm.PlayerCells[i];
                if (p == null) continue;
                float d = Vector2.Distance(transform.position, p.transform.position);
                if (d < best) { best = d; nearest = p; }
            }

            if (nearest != null && best < cell.Stats.sensorRange)
            {
                if (cell.IsMorphedOrganism && gm.OrganismMorph != null)
                {
                    V5OrganismMorph organism = gm.OrganismMorph.EnemyOrganismForCell(cell);
                    if (organism != null)
                    {
                        organism.IssueAttack(nearest);
                        return;
                    }
                }

                if (cell.EvolutionPath == V5EvolutionPath.Amoeba || cell.EvolutionPath == V5EvolutionPath.Flagellate || cell.EvolutionPath == V5EvolutionPath.Bacteria || cell.EvolutionPath == V5EvolutionPath.Rotifer || cell.EvolutionPath == V5EvolutionPath.Nematode)
                {
                    cell.Directive = V5Directive.Attack;
                    cell.AttackTarget = nearest;
                    cell.DirectiveTarget = nearest.transform.position;
                }
                else
                {
                    cell.Directive = V5Directive.Colonize;
                }
            }
            else
            {
                if (Random.value < 0.25f) wander = (wander + Random.insideUnitCircle * 0.8f).normalized;
                cell.Directive = V5Directive.Move;
                cell.DirectiveTarget = (Vector2)transform.position + wander * 6f;
            }

            if (gm.CoreMode) return;

            if (cell.CanDivide() && Random.value < 0.03f)
            {
                V5CellEntity child = cell.Divide();
                if (child != null)
                {
                    child.IsPlayerOwned = false;
                    child.gameObject.AddComponent<V5EnemyBrain>();
                }
            }
        }
    }
}
