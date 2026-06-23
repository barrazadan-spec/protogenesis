using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// First playable squad layer for v5.2. It keeps the existing per-cell controls,
    /// but scores selected cells as biological squads and applies the GDD blob penalty.
    /// </summary>
    public class V5SquadTacticsSystem : MonoBehaviour
    {
        public float SelectedCohesion01 { get; private set; }
        public int SelectedCount { get; private set; }
        public int BlobbedCells { get; private set; }
        public string Summary { get; private set; }
        public string LastMessage = "Shift+S reagrupar squad. 3-7 celulas = escuadra optima.";

        private readonly Dictionary<int, float> damageMultipliers = new Dictionary<int, float>(32);
        private readonly List<V5CellEntity> scratch = new List<V5CellEntity>(24);
        private float nextTick;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.S) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))) RegroupSelected();
            if (Time.time < nextTick) return;
            nextTick = Time.time + 0.35f;
            Tick();
        }

        public float DamageTakenMultiplier(V5CellEntity cell, V5DamageKind kind)
        {
            if (cell == null) return 1f;
            if (kind != V5DamageKind.Chemical && kind != V5DamageKind.Oxidative && kind != V5DamageKind.Acid) return 1f;
            float mult;
            return damageMultipliers.TryGetValue(cell.GetInstanceID(), out mult) ? mult : 1f;
        }

        private void Tick()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            UpdateBlobPenalty(gm);
            UpdateSelectionSquad(gm);
        }

        private void UpdateBlobPenalty(V5GameManager gm)
        {
            damageMultipliers.Clear();
            BlobbedCells = 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || c.IsAttachedToBody) continue;
                int nearby = 0;
                Vector2 pos = c.transform.position;
                for (int j = 0; j < cells.Count; j++)
                {
                    V5CellEntity other = cells[j];
                    if (other == null || other.IsAttachedToBody) continue;
                    if (Vector2.Distance(pos, other.transform.position) <= 2.0f) nearby++;
                }
                if (nearby >= 8)
                {
                    damageMultipliers[c.GetInstanceID()] = 1.15f;
                    c.Stats.stress = Mathf.Min(100f, c.Stats.stress + 0.025f);
                    BlobbedCells++;
                }
            }
        }

        private void UpdateSelectionSquad(V5GameManager gm)
        {
            scratch.Clear();
            if (gm.Selection != null)
            {
                for (int i = 0; i < gm.Selection.Selected.Count; i++)
                {
                    V5CellEntity c = gm.Selection.Selected[i];
                    if (c != null && c.IsPlayerOwned && !c.IsAttachedToBody) scratch.Add(c);
                }
            }

            SelectedCount = scratch.Count;
            if (scratch.Count <= 0)
            {
                SelectedCohesion01 = 0f;
                Summary = "Squad: sin seleccion | blob penalty: " + BlobbedCells;
                return;
            }

            Vector2 center = Vector2.zero;
            for (int i = 0; i < scratch.Count; i++) center += (Vector2)scratch[i].transform.position;
            center /= Mathf.Max(1, scratch.Count);

            float sum = 0f;
            int sharedDirective = 0;
            V5Directive d0 = scratch[0].Directive;
            for (int i = 0; i < scratch.Count; i++)
            {
                sum += Vector2.Distance(center, scratch[i].transform.position);
                if (scratch[i].Directive == d0) sharedDirective++;
            }

            float avg = sum / Mathf.Max(1, scratch.Count);
            SelectedCohesion01 = Mathf.Clamp01(Mathf.InverseLerp(5.5f, 1.25f, avg));
            bool goodSize = scratch.Count >= 3 && scratch.Count <= 7;
            bool shared = sharedDirective >= Mathf.CeilToInt(scratch.Count * 0.7f);

            if (goodSize && shared && SelectedCohesion01 > 0.62f)
            {
                for (int i = 0; i < scratch.Count; i++)
                {
                    scratch[i].Stats.stress = Mathf.Max(0f, scratch[i].Stats.stress - 0.035f);
                }
            }

            string sizeState = goodSize ? "optima" : (scratch.Count < 3 ? "chica" : "grande");
            Summary = "Squad: " + scratch.Count + " (" + sizeState + ") | cohesion " + (SelectedCohesion01 * 100f).ToString("0") + "% | orden " + d0 + " | blob " + BlobbedCells;
        }

        private void RegroupSelected()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Selection == null) return;

            scratch.Clear();
            bool hasSelectedOrganism = gm.OrganismMorph != null && gm.OrganismMorph.SelectionContainsOrganism(gm.Selection.Selected);
            Vector2 selectedCenter = Vector2.zero;
            int selectedCount = 0;
            for (int i = 0; i < gm.Selection.Selected.Count; i++)
            {
                V5CellEntity c = gm.Selection.Selected[i];
                if (c == null || !c.IsPlayerOwned || c.Role == V5CellRole.Mother || c.IsAttachedToBody) continue;
                selectedCenter += (Vector2)c.transform.position;
                selectedCount++;
                if (!c.IsMorphedOrganism) scratch.Add(c);
            }
            if (selectedCount > 0) selectedCenter /= selectedCount;

            if (scratch.Count == 0 && !hasSelectedOrganism)
            {
                LastMessage = "No hay hijas seleccionadas para reagrupar.";
                Toast();
                return;
            }

            Vector2 center = Vector2.zero;
            for (int i = 0; i < scratch.Count; i++) center += (Vector2)scratch[i].transform.position;
            center = scratch.Count > 0 ? center / Mathf.Max(1, scratch.Count) : selectedCenter;

            if (gm.MotherCell != null && Vector2.Distance(center, gm.MotherCell.transform.position) > 14f)
                center = Vector2.Lerp(center, gm.MotherCell.transform.position, 0.35f);

            if (hasSelectedOrganism && gm.OrganismMorph != null) gm.OrganismMorph.IssueMove(center);

            for (int i = 0; i < scratch.Count; i++)
            {
                float a = (Mathf.PI * 2f) * i / Mathf.Max(1, scratch.Count);
                float ring = scratch.Count <= 4 ? 1.25f : 1.85f;
                scratch[i].Directive = V5Directive.Move;
                scratch[i].DirectiveTarget = center + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * ring;
                if (gm.MotherCell != null) scratch[i].Mother = gm.MotherCell;
            }

            LastMessage = "Squad reagrupado: " + scratch.Count + " celulas" + (hasSelectedOrganism ? " + organismos." : ".");
            Toast();
        }

        private void Toast()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
        }
    }
}
