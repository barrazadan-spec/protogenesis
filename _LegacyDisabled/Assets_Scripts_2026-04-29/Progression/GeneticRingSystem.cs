using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Slots;
using Protogenesis.Views;

namespace Protogenesis.Progression
{
    /// <summary>
    /// GeneticRingSystem — Árbol génico de 3 anillos (GDD v4.3, Prompt 3.4 + Addendum v4.1).
    ///
    /// Anillo 1 (al instalar Mitocondria/Motor Metabólico): define dominio procariota/eucariota.
    ///   Gen 0: Respiración aeróbica  → Eukaryote
    ///   Gen 1: Fotosíntesis          → Eukaryote
    ///   Gen 2: Fermentación          → Prokaryote
    ///   Gen 3: Quimiolitotrofía      → Prokaryote
    ///
    /// Anillo 2 (al instalar 1ª estructura de identidad — fase Specializing).
    /// Anillo 3 (al consolidar: 40 pts Prokaryote / 60 pts Eukaryote).
    ///
    /// UI: OnGUI con 3 filas de 4 botones (solo visible con panel E abierto).
    /// </summary>
    public class GeneticRingSystem : MonoBehaviour
    {
        public static GeneticRingSystem Instance { get; private set; }

        // ── Estado de anillos ─────────────────────────────────────────────────────
        private int  _ring1Active = -1; // -1 = ninguno activo
        private int  _ring2Active = -1;
        private int  _ring3Active = -1;

        private bool _ring1Unlocked;
        private bool _ring2Unlocked;
        private bool _ring3Unlocked;

        private bool _showUI;

        // ── Propiedades públicas ──────────────────────────────────────────────────
        public int  ActiveRing1Gene => _ring1Active;
        public int  ActiveRing2Gene => _ring2Active;
        public int  ActiveRing3Gene => _ring3Active;

        public bool Ring1Unlocked => _ring1Unlocked;
        public bool Ring2Unlocked => _ring2Unlocked;
        public bool Ring3Unlocked => _ring3Unlocked;

        // ─────────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnOrganelleBuilt             += OnOrganelleBuilt;
            EventBus.OnSpecializationConsolidated += OnConsolidated;
            CellProgressionPhase.OnPhaseChanged   += OnPhaseChanged;
        }

        private void OnDisable()
        {
            EventBus.OnOrganelleBuilt             -= OnOrganelleBuilt;
            EventBus.OnSpecializationConsolidated -= OnConsolidated;
            CellProgressionPhase.OnPhaseChanged   -= OnPhaseChanged;
        }

        private void Update()
        {
            // Toggle UI con clic en Núcleo — se activa desde el panel E
            if (Input.GetKeyDown(KeyCode.G) && ViewManager.Instance?.IsExteriorActive == false)
                _showUI = !_showUI;

            // Verificar desbloqueo de Anillo 3 dinámicamente
            CheckRing3Unlock();
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region Desbloqueo de anillos

        private void OnOrganelleBuilt(GameObject go, string type)
        {
            string t = type?.ToLower() ?? "";

            // Ring 1: al instalar Motor Metabólico / Mitocondria / Cadena
            if (!_ring1Unlocked && (t.Contains("motor") || t.Contains("mitocondria") || t.Contains("cadena")))
                _ring1Unlocked = true;

            // Ring 2: al instalar primera estructura de identidad (fase Specializing)
            if (!_ring2Unlocked)
            {
                var phase = CellProgressionPhase.Instance?.CurrentPhase;
                if (phase == ProgressionPhase.Specializing)
                    _ring2Unlocked = true;
            }
        }

        private void OnPhaseChanged(ProgressionPhase phase)
        {
            // Ring 2 también se desbloquea cuando arranca Specializing
            if (phase == ProgressionPhase.Specializing)
                _ring2Unlocked = true;
        }

        private void CheckRing3Unlock()
        {
            if (_ring3Unlocked) return;

            var tracker = SpecializationTracker.Instance;
            if (tracker == null) return;

            float required = IsRing3Unlocked_Required();
            if (tracker.LeadingScore >= required)
            {
                _ring3Unlocked = true;
                Debug.Log($"[GeneticRingSystem] Anillo 3 desbloqueado ({tracker.LeadingScore:F0} pts)");
            }
        }

        private float IsRing3Unlocked_Required()
        {
            var domain = CellProgressionPhase.Instance?.CurrentDomain ?? CellDomain.Undefined;
            return domain == CellDomain.Prokaryote ? 40f : 60f;
        }

        private void OnConsolidated(SpecializationType type, string displayName)
        {
            _ring3Unlocked = true;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        public void ActivateGene(GeneticRing ring, int geneIndex)
        {
            int prev = GetActiveGene(ring);
            if (prev == geneIndex) return;

            SetActiveGene(ring, geneIndex);
            EventBus.TriggerGeneActivated(ring, geneIndex);

            // Anillo 1: define el dominio biológico
            if (ring == GeneticRing.Ring1)
            {
                CellProgressionPhase.Instance?.SetDomainFromRing1Gene(geneIndex);
                var domain = CellProgressionPhase.Instance?.CurrentDomain ?? CellDomain.Undefined;
                UpdateStructureNames(domain);
            }

            // Notificar al tracker
            SpecializationTracker.Instance?.OnGeneActivated(ring, geneIndex);

            Debug.Log($"[GeneticRingSystem] {ring} Gen {geneIndex} activado");
        }

        public int GetActiveGene(GeneticRing ring) => ring switch
        {
            GeneticRing.Ring1 => _ring1Active,
            GeneticRing.Ring2 => _ring2Active,
            GeneticRing.Ring3 => _ring3Active,
            _                 => -1
        };

        public bool IsRingUnlocked(GeneticRing ring) => ring switch
        {
            GeneticRing.Ring1 => _ring1Unlocked,
            GeneticRing.Ring2 => _ring2Unlocked,
            GeneticRing.Ring3 => _ring3Unlocked,
            _                 => false
        };

        public void ShowUI() => _showUI = true;
        public void HideUI() => _showUI = false;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Cambio de nombres de estructuras

        private void UpdateStructureNames(CellDomain domain)
        {
            var sm = SlotManager.Instance;
            if (sm == null) return;

            foreach (var slot in sm.GetAllInstalled())
            {
                string id = (slot.slotId ?? slot.displayName ?? "").ToLower();

                if (id.Contains("compartimento") || id.Contains("genetico"))
                {
                    slot.displayName = domain == CellDomain.Prokaryote ? "Nucleoide" : "Núcleo";
                }
                else if (id.Contains("motor") || id.Contains("metabolico") || id.Contains("mitocondria") || id.Contains("cadena"))
                {
                    slot.displayName = domain == CellDomain.Prokaryote
                        ? "Cadena de Transporte (membrana)"
                        : "Mitocondria";
                }
                else if (id.Contains("maquinaria") || id.Contains("ribosoma") || id.Contains("sintesis"))
                {
                    slot.displayName = domain == CellDomain.Prokaryote ? "Ribosoma 70S" : "Ribosoma 80S";
                }
            }

            Debug.Log($"[GeneticRingSystem] Estructuras renombradas para {domain}");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private void SetActiveGene(GeneticRing ring, int geneIndex)
        {
            switch (ring)
            {
                case GeneticRing.Ring1: _ring1Active = geneIndex; break;
                case GeneticRing.Ring2: _ring2Active = geneIndex; break;
                case GeneticRing.Ring3: _ring3Active = geneIndex; break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI — Árbol génico

        private static readonly string[][] GeneNames = new string[][]
        {
            new[] { "Respiración\naer.", "Fotosíntesis", "Fermentación", "Quimiolitotrofía" },
            new[] { "Motilidad\nactiva", "Secreción\npasiva", "Reconocimiento", "Adhesión" },
            new[] { "División\nrápida", "Herencia\nfuerte", "Autonomía\ncel.", "Reabsorción\ntotal" },
        };

        private void OnGUI()
        {
            if (!_showUI) return;

            float panelW = 360f;
            float panelH = 220f;
            float px     = (Screen.width  - panelW) * 0.5f;
            float py     = (Screen.height - panelH) * 0.5f;

            GUI.color = new Color(0f, 0f, 0f, 0.85f);
            GUI.DrawTexture(new Rect(px - 8f, py - 8f, panelW + 16f, panelH + 16f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            var titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 13, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            titleStyle.normal.textColor = new Color(0.7f, 0.9f, 1f);
            GUI.Label(new Rect(px, py, panelW, 20f), "Árbol Génico — 3 Anillos", titleStyle);

            for (int ring = 0; ring < 3; ring++)
            {
                GeneticRing gr     = (GeneticRing)ring;
                bool unlocked      = IsRingUnlocked(gr);
                int  active        = GetActiveGene(gr);
                float ry           = py + 28f + ring * 60f;

                var ringStyle = new GUIStyle(GUI.skin.label) { fontSize = 10 };
                ringStyle.normal.textColor = unlocked ? new Color(0.6f, 0.8f, 1f) : new Color(0.4f, 0.4f, 0.4f);
                GUI.Label(new Rect(px, ry, 70f, 50f), $"Anillo {ring + 1}", ringStyle);

                for (int g = 0; g < 4; g++)
                {
                    float bx = px + 70f + g * 72f;
                    float by = ry + 2f;

                    Color btnCol;
                    if (!unlocked)
                        btnCol = new Color(0.25f, 0.25f, 0.25f, 0.9f); // bloqueado: gris
                    else if (g == active)
                        btnCol = new Color(0.2f, 0.75f, 0.2f, 0.9f);   // activo: verde
                    else
                        btnCol = new Color(0.2f, 0.4f, 0.8f, 0.9f);    // disponible: azul

                    GUI.color = btnCol;
                    bool clicked = GUI.Button(new Rect(bx, by, 68f, 48f), GeneNames[ring][g]);
                    GUI.color = Color.white;

                    if (clicked && unlocked)
                        ActivateGene(gr, g);
                }
            }

            if (GUI.Button(new Rect(px + panelW - 24f, py, 24f, 20f), "×"))
                _showUI = false;
        }

        #endregion
    }
}
