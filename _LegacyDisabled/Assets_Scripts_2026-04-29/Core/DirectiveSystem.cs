using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Progression;

namespace Protogenesis.Views
{
    /// <summary>
    /// DirectiveSystem — Menú radial de 5 directivas para células hijas (GDD v4.3, Prompt 2.4).
    ///
    /// Click derecho sobre una hija no controlada → menú radial con 5 opciones.
    /// El ícono flotante de directiva activa se dibuja en DaughterCellBehavior.OnGUI.
    /// </summary>
    public class DirectiveSystem : MonoBehaviour
    {
        public static DirectiveSystem Instance { get; private set; }

        // ── Estado del menú ────────────────────────────────────────────────────────
        private bool            _menuOpen;
        private Vector2         _menuScreenPos;
        private DaughterCellBehavior _menuTarget;

        private const float MenuRadius   = 48f;
        private const float ButtonSize   = 36f;

        // ─────────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Update()
        {
            if (ViewManager.Instance != null && !ViewManager.Instance.IsExteriorActive) return;

            if (Input.GetMouseButtonDown(1))
                HandleRightClick();
        }

        // ─────────────────────────────────────────────────────────────────────────
        private void HandleRightClick()
        {
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector2 worldPos = cam.ScreenToWorldPoint(Input.mousePosition);
            float hitRadius  = 0.45f;

            // Check if we clicked on a daughter cell
            DaughterCellBehavior hit = null;
            float minDist = hitRadius;
            var cds = CellDivisionSystem.Instance;
            var daughters = cds != null ? cds.Daughters : (System.Collections.Generic.IReadOnlyList<CellDivisionSystem.DaughterCell>)new CellDivisionSystem.DaughterCell[0];
            foreach (var d in daughters)
            {
                if (d.go == null) continue;
                float dist = Vector2.Distance(worldPos, d.go.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    hit = d.go.GetComponent<DaughterCellBehavior>();
                }
            }

            if (hit != null)
            {
                _menuOpen      = true;
                _menuTarget    = hit;
                _menuScreenPos = Input.mousePosition;
                _menuScreenPos.y = Screen.height - _menuScreenPos.y;
            }
            else
            {
                _menuOpen = false;
            }
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI — menú radial

        private void OnGUI()
        {
            if (!_menuOpen || _menuTarget == null) return;
            if (ViewManager.Instance != null && !ViewManager.Instance.IsExteriorActive) return;

            // Close if target died
            if (_menuTarget.gameObject == null) { _menuOpen = false; return; }

            // Close on any key or left click
            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                // Handled below via button clicks
            }

            DrawRadialMenu();
        }

        private static readonly (DirectiveType type, string label, Color color)[] MenuItems = new[]
        {
            (DirectiveType.Follow,  "Seguir",   new Color(1f,   1f,   1f,   0.9f)),
            (DirectiveType.Farm,    "Farmear",  new Color(0.4f, 0.7f, 1f,   0.9f)),
            (DirectiveType.Attack,  "Atacar",   new Color(1f,   0.35f,0.35f,0.9f)),
            (DirectiveType.Defend,  "Defender", new Color(1f,   0.9f, 0.2f, 0.9f)),
            (DirectiveType.Explore, "Explorar", new Color(0.4f, 1f,   0.55f,0.9f)),
        };

        private void DrawRadialMenu()
        {
            float cx = _menuScreenPos.x;
            float cy = _menuScreenPos.y;

            // Background overlay semi-transparente
            GUI.color = new Color(0f, 0f, 0f, 0.35f);
            GUI.DrawTexture(new Rect(cx - MenuRadius - ButtonSize, cy - MenuRadius - ButtonSize,
                (MenuRadius + ButtonSize) * 2f, (MenuRadius + ButtonSize) * 2f),
                Texture2D.whiteTexture);
            GUI.color = Color.white;

            for (int i = 0; i < MenuItems.Length; i++)
            {
                float angle = (i / (float)MenuItems.Length) * Mathf.PI * 2f - Mathf.PI * 0.5f;
                float bx = cx + Mathf.Cos(angle) * MenuRadius - ButtonSize * 0.5f;
                float by = cy + Mathf.Sin(angle) * MenuRadius - ButtonSize * 0.5f;

                var (dtype, label, color) = MenuItems[i];
                bool isActive = _menuTarget.Directive == dtype;

                GUI.color = isActive ? color : new Color(color.r * 0.6f, color.g * 0.6f, color.b * 0.6f, 0.85f);

                if (GUI.Button(new Rect(bx, by, ButtonSize, ButtonSize), label.Substring(0, 1)))
                {
                    ApplyDirective(_menuTarget, dtype);
                    _menuOpen = false;
                }

                // Label below icon
                GUI.color = new Color(1f, 1f, 1f, 0.8f);
                var labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 9, alignment = TextAnchor.UpperCenter };
                GUI.Label(new Rect(bx - 10f, by + ButtonSize, ButtonSize + 20f, 14f), label, labelStyle);
            }

            GUI.color = Color.white;

            // Close button
            if (GUI.Button(new Rect(cx - 14f, cy - 14f, 28f, 28f), "×"))
                _menuOpen = false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        private void ApplyDirective(DaughterCellBehavior target, DirectiveType dtype)
        {
            target.Directive = dtype;

            // Sync with CellDivisionSystem record
            var cds = CellDivisionSystem.Instance;
            if (cds != null)
            {
                foreach (var d in cds.Daughters)
                {
                    if (d.go == target.gameObject)
                    {
                        d.directive = dtype;
                        break;
                    }
                }
            }

            EventBus.TriggerDirectiveChanged(target.gameObject, dtype);
            Debug.Log($"[DirectiveSystem] {target.name} → {dtype}");
        }
    }
}
