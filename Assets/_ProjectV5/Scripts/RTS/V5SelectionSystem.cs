using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5SelectionSystem : MonoBehaviour
    {
        public readonly List<V5CellEntity> Selected = new List<V5CellEntity>(32);
        private const float DragThresholdPixels = 8f;
        private const KeyCode SelectFreeCellsKey = KeyCode.S;
        private const KeyCode FarmSelectionKey = KeyCode.F;
        private Camera cam;
        private Vector2 dragStartScreen;
        private Vector2 dragCurrentScreen;
        private bool pointerDown;
        private bool dragSelecting;
        private bool dragAdditive;

        private void Start()
        {
            cam = Camera.main;
        }

        private void Update()
        {
            if (cam == null) cam = Camera.main;
            if (cam == null) return;
            HandleInput();
        }

        private void HandleInput()
        {
            V5GameManager gm = V5GameManager.Instance;
            bool coreMode = gm != null && gm.CoreMode;
            if (coreMode && Input.GetKey(KeyCode.A) && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)) && !V5CoreHudIMGUI.PointerBlocksWorldInput(Input.mousePosition))
            {
                IssueAttackMove(WorldMouse());
                return;
            }

            HandleSelectionInput();
            if (Input.GetMouseButtonDown(1) && !V5CoreHudIMGUI.PointerBlocksWorldInput(Input.mousePosition)) IssueContextCommand();
            if (coreMode && CoreControlGroupKeyPressed()) return;
            bool legacyModeModifier = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            if (!coreMode && !legacyModeModifier && Input.GetKeyDown(KeyCode.Alpha1)) SetCellMode(V5CellModeId.FollowLineage);
            if (!coreMode && !legacyModeModifier && Input.GetKeyDown(KeyCode.Alpha2)) SetCellMode(V5CellModeId.Gather);
            if (!coreMode && !legacyModeModifier && Input.GetKeyDown(KeyCode.Alpha3)) SetCellMode(V5CellModeId.Defend);
            if (!coreMode && !legacyModeModifier && Input.GetKeyDown(KeyCode.Alpha4)) SetCellMode(V5CellModeId.Scout);
            if (!coreMode && !legacyModeModifier && Input.GetKeyDown(KeyCode.Alpha5)) SetCellMode(V5CellModeId.Colonize);
            if (!coreMode && !legacyModeModifier && Input.GetKeyDown(KeyCode.Alpha6)) SetCellMode(V5CellModeId.Hunt);
            if (!HasSelectionShortcutModifier() && Input.GetKeyDown(SelectFreeCellsKey)) SelectAllFreeCells(false);
            if (!HasSelectionShortcutModifier() && Input.GetKeyDown(FarmSelectionKey)) IssueFarm();
            if (!coreMode && Input.GetKeyDown(KeyCode.A)) AttachSelectedToBody();
            if (Input.GetKeyDown(KeyCode.X)) DetachSelectedFromBody();
            if (Input.GetKeyDown(KeyCode.D) && (V5GameManager.Instance == null || !V5GameManager.Instance.CoreMode)) DivideSelectedOrMother();
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                V5EnvironmentOverlay overlay = FindFirstObjectByType<V5EnvironmentOverlay>();
                if (overlay != null) overlay.CycleMode();
            }
        }

        private void HandleSelectionInput()
        {
            if (!pointerDown && V5CoreHudIMGUI.PointerBlocksWorldInput(Input.mousePosition)) return;

            if (Input.GetMouseButtonDown(0))
            {
                pointerDown = true;
                dragSelecting = false;
                dragAdditive = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                dragStartScreen = Input.mousePosition;
                dragCurrentScreen = dragStartScreen;
            }

            if (pointerDown && Input.GetMouseButton(0))
            {
                dragCurrentScreen = Input.mousePosition;
                if ((dragCurrentScreen - dragStartScreen).sqrMagnitude >= DragThresholdPixels * DragThresholdPixels)
                    dragSelecting = true;
            }

            if (pointerDown && Input.GetMouseButtonUp(0))
            {
                dragCurrentScreen = Input.mousePosition;
                if (dragSelecting) SelectInDragRect(dragAdditive);
                else SelectUnderMouse(dragAdditive);
                pointerDown = false;
                dragSelecting = false;
            }
        }

        private Vector2 WorldMouse()
        {
            Vector3 m = Input.mousePosition;
            Vector3 w = cam.ScreenToWorldPoint(new Vector3(m.x, m.y, -cam.transform.position.z));
            return new Vector2(w.x, w.y);
        }

        private V5CellEntity CellUnderMouse()
        {
            Collider2D hit = Physics2D.OverlapPoint(WorldMouse());
            return hit != null ? hit.GetComponent<V5CellEntity>() : null;
        }

        private void IssueContextCommand()
        {
            V5CellEntity target = CellUnderMouse();
            V5GameManager gm = V5GameManager.Instance;
            if (target != null && gm != null && !target.IsPlayerOwned && target.Stats.currentHp > 0f)
            {
                IssueAttack(target);
                return;
            }

            IssueMove(WorldMouse());
        }

        private void SelectUnderMouse(bool additive)
        {
            Vector2 p = WorldMouse();
            Collider2D hit = Physics2D.OverlapPoint(p);
            V5CellEntity cell = hit != null ? hit.GetComponent<V5CellEntity>() : null;
            if (!additive) ClearSelection();
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.CoreMode && cell != null && cell == gm.MotherCell)
            {
                AddSelection(cell);
                V5CoreHudIMGUI.OpenMotherPanel(true);
                return;
            }
            if (gm != null && gm.OrganismMorph != null && gm.OrganismMorph.IsOrganismCell(cell))
            {
                gm.OrganismMorph.SelectOrganismForCell(cell, true);
                return;
            }
            if (CanSelectOwnedAlive(cell)) AddSelection(cell);
        }

        private void SelectInDragRect(bool additive)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            Rect rect = ScreenRect(dragStartScreen, dragCurrentScreen);
            rect.xMin -= 12f;
            rect.xMax += 12f;
            rect.yMin -= 12f;
            rect.yMax += 12f;

            if (!additive) ClearSelection();
            V5OrganismMorph morph = gm.OrganismMorph;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (!CanSelectOwnedAlive(cell)) continue;
                Vector3 screen = cam.WorldToScreenPoint(cell.transform.position);
                if (screen.z < 0f) continue;
                if (!rect.Contains(new Vector2(screen.x, screen.y))) continue;
                if (morph != null && morph.IsOrganismCell(cell))
                {
                    morph.SelectOrganismForCell(cell, true);
                    continue;
                }
                AddSelection(cell);
            }
        }

        public void SelectAllFreeCells(bool additive)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;

            if (!additive) ClearSelection();
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (!CanSelectOwnedAlive(cell)) continue;
                if (cell.Role == V5CellRole.Mother || cell.IsAttachedToBody || cell.IsMorphedOrganism) continue;
                AddSelection(cell);
            }
        }

        private bool CanSelectOwnedAlive(V5CellEntity cell)
        {
            return cell != null && cell.IsPlayerOwned && cell.Stats.currentHp > 0f;
        }

        private bool HasSelectionShortcutModifier()
        {
            return Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift) ||
                Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) ||
                Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
        }

        private bool CoreControlGroupKeyPressed()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || gm.ControlGroups == null) return false;
            for (int i = 0; i <= 9; i++)
            {
                KeyCode key = i == 0 ? KeyCode.Alpha0 : (KeyCode)((int)KeyCode.Alpha1 + (i - 1));
                if (!Input.GetKeyDown(key)) continue;
                if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) return true;
                if (gm.ControlGroups.Count(i) > 0) return true;
            }
            return false;
        }

        public void AddSelection(V5CellEntity cell)
        {
            if (cell == null || Selected.Contains(cell)) return;
            Selected.Add(cell);
            cell.SetSelected(true);
        }

        public void Deselect(V5CellEntity cell)
        {
            if (cell == null) return;
            if (Selected.Remove(cell)) cell.SetSelected(false);
        }

        public void ClearSelection()
        {
            for (int i = 0; i < Selected.Count; i++) if (Selected[i] != null) Selected[i].SetSelected(false);
            Selected.Clear();
        }

        public void IssueMove(Vector2 target)
        {
            V5GameManager gm = V5GameManager.Instance;
            bool movedOrganism = false;
            if (gm != null && gm.OrganismMorph != null && gm.OrganismMorph.SelectionContainsOrganism(Selected))
            {
                gm.OrganismMorph.IssueMove(target);
                movedOrganism = true;
            }

            if (Selected.Count == 0 && gm != null && gm.MotherCell != null)
            {
                if (gm.OrganismMorph != null && gm.OrganismMorph.IsMorphed)
                {
                    gm.OrganismMorph.IssueMove(target);
                    return;
                }
                if (gm.CoreMode) return;
                AddSelection(gm.MotherCell);
            }
            for (int i = 0; i < Selected.Count; i++)
            {
                if (Selected[i] == null) continue;
                if (Selected[i].IsAttachedToBody || Selected[i].IsMorphedOrganism) continue;
                if (movedOrganism && gm != null && gm.OrganismMorph != null && gm.OrganismMorph.IsOrganismCell(Selected[i])) continue;
                Selected[i].SetPlayerMoveOrder(target + Random.insideUnitCircle * 0.5f);
            }
        }

        public void IssueAttack(V5CellEntity target)
        {
            if (target == null) return;
            V5GameManager gm = V5GameManager.Instance;
            bool movedOrganism = false;
            if (gm != null && gm.OrganismMorph != null && (gm.OrganismMorph.SelectionContainsOrganism(Selected) || Selected.Count == 0))
            {
                gm.OrganismMorph.IssueAttack(target);
                movedOrganism = true;
            }

            for (int i = 0; i < Selected.Count; i++)
            {
                V5CellEntity cell = Selected[i];
                if (cell == null) continue;
                if (cell.IsAttachedToBody || cell.IsMorphedOrganism) continue;
                if (movedOrganism && gm != null && gm.OrganismMorph != null && gm.OrganismMorph.IsOrganismCell(cell)) continue;
                cell.SetPlayerAttackOrder(target);
            }
        }

        public void IssueAttackMove(Vector2 target)
        {
            V5GameManager gm = V5GameManager.Instance;
            bool movedOrganism = false;
            if (gm != null && gm.OrganismMorph != null && (gm.OrganismMorph.SelectionContainsOrganism(Selected) || Selected.Count == 0))
            {
                gm.OrganismMorph.IssueAttackMove(target);
                movedOrganism = true;
            }

            for (int i = 0; i < Selected.Count; i++)
            {
                V5CellEntity cell = Selected[i];
                if (cell == null) continue;
                if (cell.IsAttachedToBody || cell.IsMorphedOrganism) continue;
                if (movedOrganism && gm != null && gm.OrganismMorph != null && gm.OrganismMorph.IsOrganismCell(cell)) continue;
                cell.SetPlayerAttackMoveOrder(target + Random.insideUnitCircle * 0.55f);
            }
        }

        public void IssueFarm()
        {
            V5GameManager gm = V5GameManager.Instance;
            bool farmedOrganism = false;
            if (gm != null && gm.OrganismMorph != null && (gm.OrganismMorph.SelectionContainsOrganism(Selected) || Selected.Count == 0))
            {
                gm.OrganismMorph.IssueFarm();
                farmedOrganism = true;
            }

            for (int i = 0; i < Selected.Count; i++)
            {
                V5CellEntity cell = Selected[i];
                if (cell == null) continue;
                if (cell.IsAttachedToBody || cell.IsMorphedOrganism || cell.Role == V5CellRole.Mother) continue;
                if (farmedOrganism && gm != null && gm.OrganismMorph != null && gm.OrganismMorph.IsOrganismCell(cell)) continue;
                cell.SetPlayerFarmOrder();
            }
        }

        public void SetDirective(V5Directive directive)
        {
            if (directive == V5Directive.Farm)
            {
                IssueFarm();
                return;
            }
            SetCellMode(V5CellModeLibrary.ModeForDirective(directive));
        }

        public void SetCellMode(V5CellModeId mode)
        {
            if (Selected.Count == 0 && V5GameManager.Instance != null && V5GameManager.Instance.MotherCell != null) AddSelection(V5GameManager.Instance.MotherCell);
            for (int i = 0; i < Selected.Count; i++)
            {
                if (Selected[i] == null) continue;
                if (Selected[i].IsMorphedOrganism) continue;
                Selected[i].ApplyCellMode(mode);
            }
        }

        public void AttachSelectedToBody()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Body == null)
                return;

            int attached = 0;
            for (int i = 0; i < Selected.Count; i++)
            {
                V5CellEntity cell = Selected[i];
                if (cell == null || cell.Role == V5CellRole.Mother || cell.IsMorphedOrganism) continue;
                V5BodySlotRole role = V5PhenotypeRecipeLibrary.RecommendedBodyRole(cell.PhenotypeCaste);
                if (gm.Body.TryAttach(cell, role)) attached++;
            }

            if (attached == 0 && gm.Hud != null)
                gm.Hud.Toast(gm.Body != null && !string.IsNullOrEmpty(gm.Body.LastMessage) ? gm.Body.LastMessage : "Selecciona hijas cercanas a la madre para adherirlas.");
        }

        public void DetachSelectedFromBody()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Body == null)
                return;

            int detached = 0;
            for (int i = 0; i < Selected.Count; i++)
            {
                V5CellEntity cell = Selected[i];
                if (cell != null && cell.IsAttachedToBody && gm.Body.Detach(cell, true)) detached++;
            }

            if (detached == 0 && gm.Hud != null) gm.Hud.Toast("No hay celulas adheridas seleccionadas.");
        }

        private void DivideSelectedOrMother()
        {
            V5GameManager gm = V5GameManager.Instance;

            V5CellEntity target = null;
            for (int i = 0; i < Selected.Count; i++)
            {
                if (Selected[i] != null && Selected[i].CanDivide()) { target = Selected[i]; break; }
            }
            if (target == null && gm != null) target = gm.MotherCell;
            if (target != null && target.Role == V5CellRole.Mother && target.IsPlayerOwned && gm != null && gm.Germinal != null)
            {
                gm.Germinal.TryProduce(V5GerminalCasteId.PlasticDaughter, true);
                return;
            }
            if (target != null) target.Divide();
        }

        private void OnGUI()
        {
            if (!pointerDown || !dragSelecting) return;
            Rect rect = ScreenRect(dragStartScreen, dragCurrentScreen);
            Rect guiRect = new Rect(rect.x, Screen.height - rect.yMax, rect.width, rect.height);
            DrawSelectionRect(guiRect, new Color(0.25f, 0.95f, 1f, 0.12f), new Color(0.75f, 1f, 0.95f, 0.86f));
        }

        private Rect ScreenRect(Vector2 a, Vector2 b)
        {
            float xMin = Mathf.Min(a.x, b.x);
            float xMax = Mathf.Max(a.x, b.x);
            float yMin = Mathf.Min(a.y, b.y);
            float yMax = Mathf.Max(a.y, b.y);
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        private void DrawSelectionRect(Rect rect, Color fill, Color border)
        {
            Color previous = GUI.color;
            GUI.color = fill;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = border;
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, rect.width, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMax - 2f, rect.width, 2f), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMin, rect.yMin, 2f, rect.height), Texture2D.whiteTexture);
            GUI.DrawTexture(new Rect(rect.xMax - 2f, rect.yMin, 2f, rect.height), Texture2D.whiteTexture);
            GUI.color = previous;
        }
    }
}
