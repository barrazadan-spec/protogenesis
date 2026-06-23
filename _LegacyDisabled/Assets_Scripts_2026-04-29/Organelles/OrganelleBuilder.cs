using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Gestiona la construcción de orgánulos.
    ///
    /// Flujo:
    ///   1. Click derecho en una zona válida → se abre el BuildMenu
    ///   2. El jugador selecciona el orgánulo a construir
    ///   3. Se verifica: zona correcta, recursos suficientes, era desbloqueada
    ///   4. Si todo OK: se instantia el prefab y se consumen recursos
    ///
    /// También gestiona el preview del orgánulo mientras el cursor está sobre
    /// una zona válida (sprite semitransparente que sigue al ratón).
    /// </summary>
    public class OrganelleBuilder : MonoBehaviour
    {
        public static OrganelleBuilder Instance { get; private set; }

        // ── Catálogo de orgánulos construibles ────────────────────────────────────
        [Header("Catálogo de orgánulos")]
        [SerializeField] private OrganelleBuildEntry[] catalogue;

        // ── Preview ───────────────────────────────────────────────────────────────
        [Header("Preview")]
        [SerializeField] private Color validColor   = new Color(0f, 1f, 0f, 0.4f);
        [SerializeField] private Color invalidColor = new Color(1f, 0f, 0f, 0.4f);

        private GameObject  _previewInstance;
        private SpriteRenderer _previewRenderer;

        // ── Estado ────────────────────────────────────────────────────────────────
        private bool   _buildModeActive = false;
        private string _selectedType    = null;
        private OrganelleBuildEntry _selectedEntry;

        // ── Menú contextual (simple en-world) ─────────────────────────────────────
        [Header("UI")]
        [SerializeField] private GameObject buildMenuPrefab;
        private GameObject _menuInstance;
        private Vector2    _menuWorldPos;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            HandleInput();
            UpdatePreview();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Input

        private void HandleInput()
        {
            // DEPRECATED v4.6: click derecho era exclusivo de FluidMovementSystem (mover célula).
            // OrganelleBuilder se abre desde SlotInstallUI (G), no con click derecho.
            // if (Input.GetMouseButtonDown(1) && !_buildModeActive) { OpenBuildMenu(...); return; }

            // En modo construcción: click izquierdo coloca el orgánulo
            if (_buildModeActive)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    TryBuild(worldPos);
                }
                // Escape cancela (click derecho eliminado — reservado para FluidMovementSystem)
                if (Input.GetKeyDown(KeyCode.Escape))
                    CancelBuild();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Build Menu

        private void OpenBuildMenu(Vector2 worldPos)
        {
            _menuWorldPos = worldPos;
            CloseMenu();

            if (buildMenuPrefab != null)
            {
                _menuInstance = Instantiate(buildMenuPrefab,
                                            worldPos,
                                            Quaternion.identity);
                // La UI del menú llama a SelectOrganelle(string type) en este script
            }
            else
            {
                // Fallback: log de opciones disponibles (sin prefab de menú asignado)
                Debug.Log("[OrganelleBuilder] Menú de construcción en " + worldPos);
                if (catalogue != null)
                    foreach (var entry in catalogue)
                        Debug.Log($"  → {entry.organelleType} | {entry.buildCostATP} ATP + {entry.buildCostProteins} prot | ERA {entry.eraRequired}");
            }
        }

        private void CloseMenu()
        {
            if (_menuInstance != null)
                Destroy(_menuInstance);
        }

        /// <summary>
        /// Llamado por los botones del BuildMenu cuando el jugador elige un orgánulo.
        /// </summary>
        public void SelectOrganelle(string organelleType)
        {
            CloseMenu();

            OrganelleBuildEntry entry = FindEntry(organelleType);
            if (entry == null)
            {
                Debug.LogWarning($"[OrganelleBuilder] Tipo '{organelleType}' no encontrado en el catálogo.");
                return;
            }

            _selectedType  = organelleType;
            _selectedEntry = entry;
            _buildModeActive = true;

            CreatePreview(entry);
            Debug.Log($"[OrganelleBuilder] Modo construcción: {organelleType}. Click para colocar, ESC para cancelar.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Construcción

        /// <summary>
        /// Intenta construir el orgánulo seleccionado en la posición dada.
        /// </summary>
        private void TryBuild(Vector2 worldPos)
        {
            if (_selectedEntry == null) return;

            // 1. Verificar era
            int currentEra = GameManager.Instance != null ? GameManager.Instance.CurrentEra : 0;
            if (currentEra < _selectedEntry.eraRequired)
            {
                Debug.Log($"[OrganelleBuilder] Requiere ERA {_selectedEntry.eraRequired}.");
                return;
            }

            // 2. Verificar zona
            if (ZoneSystem.Instance != null &&
                !ZoneSystem.Instance.IsValidPlacement(worldPos, _selectedEntry.zone))
            {
                ZoneType? current = ZoneSystem.Instance.GetZoneAt(worldPos);
                Debug.Log($"[OrganelleBuilder] Zona incorrecta. " +
                          $"Requiere {_selectedEntry.zone}, posición en {current?.ToString() ?? "fuera de célula"}.");
                return;
            }

            // 3. Verificar límites especiales (ej: max 6 mitocondrias)
            if (!CheckSpecialLimits(_selectedEntry.organelleType))
                return;

            // 4. Verificar recursos
            var rm = ResourceManager.Instance;
            if (rm == null || !rm.CanAffordAll(
                (ResourceType.ATP,      _selectedEntry.buildCostATP),
                (ResourceType.Biomass, _selectedEntry.buildCostProteins)))
            {
                Debug.Log($"[OrganelleBuilder] Recursos insuficientes: " +
                          $"{_selectedEntry.buildCostATP} ATP + {_selectedEntry.buildCostProteins} prot.");
                return;
            }

            // 5. Construir
            rm.ConsumeAll(
                (ResourceType.ATP,      _selectedEntry.buildCostATP),
                (ResourceType.Biomass, _selectedEntry.buildCostProteins));

            GameObject built = Instantiate(_selectedEntry.prefab, worldPos, Quaternion.identity);
            built.tag = "AllyOrganelle";
            EventBus.TriggerOrganelleBuilt(built, _selectedEntry.organelleType);

            Debug.Log($"[OrganelleBuilder] {_selectedEntry.organelleType} construido en {worldPos}.");
            CancelBuild();
        }

        private bool CheckSpecialLimits(string type)
        {
            if (type == "Mitocondria" && !Mitocondria.CanBuildNew())
            {
                Debug.Log($"[OrganelleBuilder] Límite de Mitocondrias alcanzado ({Mitocondria.MaxAllowed}).");
                return false;
            }
            return true;
        }

        private void CancelBuild()
        {
            _buildModeActive = false;
            _selectedType    = null;
            _selectedEntry   = null;
            DestroyPreview();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Preview

        private void CreatePreview(OrganelleBuildEntry entry)
        {
            DestroyPreview();
            if (entry.prefab == null) return;

            _previewInstance = Instantiate(entry.prefab);
            _previewInstance.name = "BuildPreview";

            // Desactivar scripts del preview para que no produzca nada
            foreach (var comp in _previewInstance.GetComponents<MonoBehaviour>())
                comp.enabled = false;

            _previewRenderer = _previewInstance.GetComponentInChildren<SpriteRenderer>();
        }

        private void UpdatePreview()
        {
            if (!_buildModeActive || _previewInstance == null) return;

            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            _previewInstance.transform.position = worldPos;

            if (_previewRenderer == null) return;

            // Color según validez de la posición
            bool valid = ZoneSystem.Instance == null ||
                         ZoneSystem.Instance.IsValidPlacement(worldPos, _selectedEntry.zone);

            _previewRenderer.color = valid ? validColor : invalidColor;
        }

        private void DestroyPreview()
        {
            if (_previewInstance != null)
                Destroy(_previewInstance);
            _previewInstance = null;
            _previewRenderer = null;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private OrganelleBuildEntry FindEntry(string type)
        {
            foreach (var e in catalogue)
                if (e.organelleType == type) return e;
            return null;
        }

        /// <summary>
        /// Construye un orgánulo directamente desde la red (confirmación del servidor).
        /// Llamado por NetworkGameManager.ConfirmBuildClientRpc y CoopNetwork.ReceiveOrganelleClientRpc.
        /// Omite validaciones de recursos (ya validadas en el servidor).
        /// </summary>
        public void BuildFromNetwork(string organelleType, Vector2 position)
        {
            OrganelleBuildEntry entry = FindEntry(organelleType);
            if (entry == null || entry.prefab == null)
            {
                Debug.LogWarning($"[OrganelleBuilder] BuildFromNetwork: tipo '{organelleType}' no encontrado en catálogo.");
                return;
            }

            Vector2 buildPos = position == Vector2.zero
                ? (Vector2)transform.position
                : position;

            GameObject built = Instantiate(entry.prefab, buildPos, Quaternion.identity);
            built.tag = "AllyOrganelle";
            EventBus.TriggerOrganelleBuilt(built, organelleType);

            Debug.Log($"[OrganelleBuilder] BuildFromNetwork: {organelleType} en {buildPos}.");
        }

        /// <summary>
        /// Devuelve todas las entradas del catálogo disponibles para la era actual.
        /// Útil para la UI del BuildMenu.
        /// </summary>
        public List<OrganelleBuildEntry> GetAvailableEntries()
        {
            int era = GameManager.Instance != null ? GameManager.Instance.CurrentEra : 0;
            var result = new List<OrganelleBuildEntry>();
            foreach (var e in catalogue)
                if (e.eraRequired <= era) result.Add(e);
            return result;
        }

        #endregion
    }

    // ─────────────────────────────────────────────────────────────────────────────
    /// <summary>Entrada del catálogo de construcción de orgánulos.</summary>
    [System.Serializable]
    public class OrganelleBuildEntry
    {
        public string     organelleType;
        public GameObject prefab;
        public ZoneType   zone;
        public int        eraRequired;
        public float      buildCostATP;
        public float      buildCostProteins;
        [TextArea(1, 3)]
        public string     description;
    }
}
