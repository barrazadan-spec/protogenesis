using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Protogenesis.Core;

namespace Protogenesis.Progression
{
    /// <summary>
    /// UpgradeUI — Panel del árbol de mejoras evolutivas (tecla I).
    ///
    /// Layout:
    ///   - 4 columnas (una por rama: Metabolismo / Defensa / Producción / Expansión)
    ///   - Cada columna tiene hasta 4 nodos
    ///   - Nodos conectados por líneas UI (LineRenderer o Image stretched)
    ///   - Barra de progreso de investigación actual
    ///   - Panel lateral de genes persistentes (PE + botones de mejora)
    ///
    /// Estados de un nodo:
    ///   Locked     — gris, sin icono de check
    ///   Available  — verde pulsante, se puede investigar
    ///   Queued     — amarillo, en cola
    ///   Researching— barra de progreso animada
    ///   Unlocked   — azul, con checkmark
    /// </summary>
    public class UpgradeUI : MonoBehaviour
    {
        // ── Panel raíz ────────────────────────────────────────────────────────────
        [Header("Panel principal")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private KeyCode    toggleKey = KeyCode.I;

        // ── Árbol de nodos ────────────────────────────────────────────────────────
        [Header("Árbol de nodos")]
        [SerializeField] private Transform  nodeParent;
        [SerializeField] private GameObject nodeButtonPrefab;   // Image + Button + TMP + StatusImage

        // ── Investigación en curso ────────────────────────────────────────────────
        [Header("Investigación en curso")]
        [SerializeField] private GameObject      researchBanner;
        [SerializeField] private TextMeshProUGUI researchNameText;
        [SerializeField] private Slider          researchProgressBar;
        [SerializeField] private Button          cancelResearchButton;

        // ── Panel de genes ────────────────────────────────────────────────────────
        [Header("Panel de Genes (meta-progresión)")]
        [SerializeField] private Transform       geneParent;
        [SerializeField] private GameObject      geneRowPrefab;   // Button + TMP + Level dots
        [SerializeField] private TextMeshProUGUI peText;

        // ── Cola de investigación ─────────────────────────────────────────────────
        [Header("Cola")]
        [SerializeField] private TextMeshProUGUI queueText;

        // ── Colores de estado ─────────────────────────────────────────────────────
        private static readonly Color ColorLocked      = new Color(0.35f, 0.35f, 0.35f);
        private static readonly Color ColorAvailable   = new Color(0.18f, 0.80f, 0.44f);
        private static readonly Color ColorQueued      = new Color(0.95f, 0.80f, 0.10f);
        private static readonly Color ColorResearching = new Color(0.10f, 0.60f, 0.95f);
        private static readonly Color ColorUnlocked    = new Color(0.30f, 0.50f, 1.00f);

        // ── Estado interno ────────────────────────────────────────────────────────
        private bool _open = false;
        private readonly Dictionary<string, GameObject> _nodeButtons = new Dictionary<string, GameObject>();
        private readonly List<GameObject>               _geneRows    = new List<GameObject>();

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Start()
        {
            if (panelRoot != null)         panelRoot.SetActive(false);
            if (researchBanner != null)    researchBanner.SetActive(false);
            if (cancelResearchButton != null)
                cancelResearchButton.onClick.AddListener(OnCancelResearch);

            EventBus.OnResearchStarted    += OnResearchStarted;
            EventBus.OnResearchCompleted  += OnResearchCompleted;
            EventBus.OnResearchQueued     += OnResearchQueued;
            EventBus.OnGeneticDrift       += OnGeneticDrift;

            BuildNodeButtons();
            BuildGeneRows();
        }

        private void OnDestroy()
        {
            EventBus.OnResearchStarted   -= OnResearchStarted;
            EventBus.OnResearchCompleted -= OnResearchCompleted;
            EventBus.OnResearchQueued    -= OnResearchQueued;
            EventBus.OnGeneticDrift      -= OnGeneticDrift;
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
                TogglePanel();

            if (_open)
            {
                RefreshNodeStates();
                RefreshResearchProgress();
                RefreshGenePanel();
                RefreshQueue();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Construcción de la UI

        private void BuildNodeButtons()
        {
            if (nodeParent == null || nodeButtonPrefab == null) return;

            var tree = UpgradeTree.Instance;
            if (tree == null) return;

            var nodes = tree.GetAllNodes();
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                if (node == null) continue;

                var go     = Instantiate(nodeButtonPrefab, nodeParent);
                go.name    = node.nodeId;

                // Texto e ícono
                var nameText = go.GetComponentInChildren<TextMeshProUGUI>();
                if (nameText != null) nameText.text = node.displayName;

                var icon = go.transform.Find("Icon")?.GetComponent<Image>();
                if (icon != null && node.icon != null) icon.sprite = node.icon;

                // Click → investigar
                string capturedId = node.nodeId;
                var btn = go.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(() => OnNodeClicked(capturedId));

                _nodeButtons[node.nodeId] = go;
            }
        }

        private void BuildGeneRows()
        {
            if (geneParent == null || geneRowPrefab == null) return;

            var gm = GeneManager.Instance;
            if (gm == null) return;

            foreach (var gene in gm.GetAllGenes())
            {
                var row     = Instantiate(geneRowPrefab, geneParent);
                row.name    = gene.id;

                var text = row.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = $"{gene.displayName} [{gene.currentLevel}/{gene.maxLevel}]";

                string capturedId = gene.id;
                var btn = row.GetComponent<Button>();
                if (btn != null)
                    btn.onClick.AddListener(() => OnGeneUpgradeClicked(capturedId));

                _geneRows.Add(row);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Refresco

        private void RefreshNodeStates()
        {
            var tree = UpgradeTree.Instance;
            if (tree == null) return;

            var nodes = tree.GetAllNodes();
            if (nodes == null) return;

            foreach (var node in nodes)
            {
                if (node == null || !_nodeButtons.TryGetValue(node.nodeId, out var go)) continue;

                var bg   = go.GetComponent<Image>();
                bool res = tree.CurrentResearch?.nodeId == node.nodeId;
                bool que = IsInQueue(node.nodeId);
                bool unl = tree.IsUnlocked(node.nodeId);
                bool avl = !unl && !res && !que &&
                           tree.CanResearch(node, out _);

                Color c = unl  ? ColorUnlocked    :
                          res  ? ColorResearching  :
                          que  ? ColorQueued        :
                          avl  ? ColorAvailable    :
                                 ColorLocked;

                if (bg != null) bg.color = c;

                // Interactividad
                var btn = go.GetComponent<Button>();
                if (btn != null) btn.interactable = avl;
            }
        }

        private void RefreshResearchProgress()
        {
            var tree = UpgradeTree.Instance;
            if (tree == null) return;

            bool active = tree.CurrentResearch != null;
            if (researchBanner != null) researchBanner.SetActive(active);

            if (active)
            {
                if (researchNameText   != null)
                    researchNameText.text = $"Investigando: {tree.CurrentResearch.displayName}";
                if (researchProgressBar != null)
                    researchProgressBar.value = tree.ResearchProgress;
            }
        }

        private void RefreshGenePanel()
        {
            var gm = GeneManager.Instance;
            if (gm == null) return;

            if (peText != null) peText.text = $"PE: {gm.EvolutionPoints}";

            var genes = gm.GetAllGenes();
            for (int i = 0; i < _geneRows.Count && i < genes.Length; i++)
            {
                var text = _geneRows[i].GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                    text.text = $"{genes[i].displayName} [{genes[i].currentLevel}/{genes[i].maxLevel}]";

                var btn = _geneRows[i].GetComponent<Button>();
                if (btn != null)
                {
                    bool canUpgrade = genes[i].currentLevel < genes[i].maxLevel &&
                                      i < genes[i].costPerLevel.Length &&
                                      gm.EvolutionPoints >= genes[i].costPerLevel[genes[i].currentLevel];
                    btn.interactable = canUpgrade;
                }
            }
        }

        private void RefreshQueue()
        {
            if (queueText == null) return;
            var tree = UpgradeTree.Instance;
            if (tree == null) return;

            int count = 0;
            foreach (var _ in tree.Queue) count++;
            queueText.text = count > 0 ? $"Cola: {count} pendiente(s)" : "Cola vacía";
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Handlers de botones

        private void OnNodeClicked(string nodeId)
        {
            bool queued = UpgradeTree.Instance?.RequestResearch(nodeId) ?? false;
            if (!queued)
                UI.AlertSystem.Instance?.ShowAlert(
                    "No se puede investigar ese nodo ahora",
                    new Color(0.9f, 0.5f, 0.1f));
        }

        private void OnGeneUpgradeClicked(string geneId)
        {
            bool upgraded = GeneManager.Instance?.UpgradeGene(geneId) ?? false;
            if (!upgraded)
                UI.AlertSystem.Instance?.ShowAlert(
                    "PE insuficientes o gen al nivel máximo",
                    new Color(0.9f, 0.5f, 0.1f));
        }

        private void OnCancelResearch()
        {
            UpgradeTree.Instance?.CancelCurrentResearch();
        }

        private void TogglePanel()
        {
            _open = !_open;
            if (panelRoot != null) panelRoot.SetActive(_open);
            if (_open)
            {
                RefreshNodeStates();
                RefreshGenePanel();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Suscriptores EventBus

        private void OnResearchStarted(string nodeId, string name, float duration)
        {
            if (!_open) return;
            RefreshNodeStates();
        }

        private void OnResearchCompleted(string nodeId, string name)
        {
            RefreshNodeStates();
            UI.AlertSystem.Instance?.ShowAlert(
                $"🧬 Investigación completada: {name}",
                new Color(0.18f, 0.80f, 0.44f),
                UI.AlertPriority.High);
        }

        private void OnResearchQueued(string nodeId, string name)
        {
            if (!_open) return;
            RefreshNodeStates();
        }

        private void OnGeneticDrift(string lineage, string bonusType, float value)
        {
            UI.AlertSystem.Instance?.ShowAlert(
                $"🔬 DERIVA GENÉTICA — Línea '{lineage}' mutó: +{value:P0} {bonusType}",
                new Color(0.60f, 0.20f, 0.90f),
                UI.AlertPriority.High);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private bool IsInQueue(string nodeId)
        {
            var tree = UpgradeTree.Instance;
            if (tree == null) return false;
            foreach (var q in tree.Queue)
                if (q.nodeId == nodeId) return true;
            return false;
        }

        #endregion
    }
}
