using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Units;
using Protogenesis.Slots;
using Protogenesis.Ecosystem;
using Protogenesis.Victory;
// [Primordia] using Protogenesis.Campaign;
using Protogenesis.Rendering;
using Protogenesis.Views;
using Protogenesis.UI;
using Protogenesis.Progression;

namespace Protogenesis.Core
{
    /// <summary>
    /// Bootstrap de escena para pruebas.
    ///
    /// Añade este script a un GameObject vacío en la SampleScene.
    /// Al ejecutar en modo Play, crea automáticamente todos los
    /// GameObjects necesarios para que el juego funcione:
    ///
    ///   [Managers]   → GameManager, ResourceManager, EnvironmentManager
    ///   [CAP]        → La célula del jugador con sus componentes
    ///   [Systems]    → MetabolismEngine, InstabilitySystem, MetabolicHeatSystem, BalanceSystem, UnitSpawner, QuorumSensor, OrganelleBuilder
    ///   [Zone]       → ZoneSystem centrado en la CAP
    ///   [UI]         → Canvas con HUD básico (ResourceHUD, AlertSystem, AbilityUI, GeneticTreeUI)
    ///   [Camera]     → Cámara top-down con fondo #0A1628
    ///
    /// INSTRUCCIONES:
    ///   1. Abre Assets/Scenes/SampleScene.unity
    ///   2. Crea un GameObject vacío llamado "Bootstrap"
    ///   3. Añade este script como componente
    ///   4. Pulsa Play — el juego se inicializa automáticamente
    ///
    /// NOTA: Para producción, crea una escena ERA_0 real con prefabs y UI completa.
    /// </summary>
    [DefaultExecutionOrder(-100)]   // Se ejecuta antes que cualquier otro script
    public class SceneBootstrap : MonoBehaviour
    {
        [Header("¿Crear sistemas automáticamente?")]
        [SerializeField] private bool autoCreateManagers    = true;
        [SerializeField] private bool autoCreateCAP         = true;
        [SerializeField] private bool autoCreateSystems     = true;
        [SerializeField] private bool autoCreateCamera      = true;

        [Header("Config de prueba")]
        [SerializeField] private Vector2 capStartPosition  = Vector2.zero;
        [Tooltip("Si true, spawna un enemigo de prueba a los 5 segundos.")]
        [SerializeField] private bool spawnTestEnemy        = true;

        // ── Estado para overlay ───────────────────────────────────────────────────
        private GUIStyle _boxStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private bool     _stylesReady = false;

        private void Awake()
        {
            // Garantizar que la física 2D simula automáticamente cada FixedUpdate
            Physics2D.simulationMode = SimulationMode2D.FixedUpdate;

            if (autoCreateManagers) CreateManagers();
            if (autoCreateCAP)      CreateCAP();
            if (autoCreateSystems)  CreateSystems();
            if (autoCreateCamera)   SetupCamera();
            CreateV3Systems();
            CreateV43Systems();

            Debug.Log("[SceneBootstrap] ¡Escena inicializada (v3 AAA)!");
        }

        private void Start()
        {
            RegisterBaseMetabolism();
            PopulateEcosystem();
            Victory.VictoryManager.Instance?.StartMatch();
            RegisterUnitTypes();
            InstallBaseSlots();
            SpawnResourceNodes();

            if (spawnTestEnemy)
                Invoke(nameof(SpawnTestEnemies), 3f);
        }

        private void LateUpdate()
        {
            // Cámara sigue al jugador en la vista exterior
            var cam = Camera.main;
            if (cam == null) return;
            var player = PlayerCellExterior.Instance;
            if (player == null) return;
            var p = player.transform.position;
            cam.transform.position = new Vector3(p.x, p.y, cam.transform.position.z);
        }

        /// <summary>
        /// Instala las estructuras innatas con las que empieza la célula.
        /// El Protonúcleo ocupa Social[0] sin coste ni deuda — es la base genética.
        /// </summary>
        private void InstallBaseSlots()
        {
            var sm = Slots.SlotManager.Instance;
            if (sm == null) return;

            var protonucleo = ScriptableObject.CreateInstance<Slots.SlotData>();
            protonucleo.slotId           = "Protonucleo";
            protonucleo.displayName      = "Protonúcleo";
            protonucleo.requiredSlotType = Slots.SlotType.Social;
            protonucleo.structureLevel   = 0;
            protonucleo.description      = "Núcleo primitivo innato — habilita el Árbol Genético (2 upgrades máx.)";
            protonucleo.costTypes        = new ResourceType[0];
            protonucleo.costAmounts      = new float[0];
            protonucleo.effects          = new Slots.SlotEffectBase[0];
            protonucleo.isPassive        = true;

            sm.InstallBaseStructure(protonucleo);
        }

        /// <summary>
        /// Registra todos los tipos de unidad concretos en el UnitSpawner para que
        /// puedan instanciarse programáticamente sin necesidad de prefabs.
        /// </summary>
        private void RegisterUnitTypes()
        {
            var us = UnitSpawner.Instance;
            if (us == null) return;

            // Colonial (placeholder hasta que exista clase ColonyUnit propia)
            us.RegisterUnitType("ColonyUnit", typeof(Units.ProcarioteBasic));
            // ERA 1 — Procariotas
            us.RegisterUnitType("ProcarioteBasic",     typeof(Units.ProcarioteBasic));
            us.RegisterUnitType("ProcarioteChitin",    typeof(Units.ProcarioteChitin));
            us.RegisterUnitType("ProcarioteSecreting", typeof(Units.ProcarioteSecreting));
            us.RegisterUnitType("ProcarioteFlag",      typeof(Units.ProcarioteFlag));
            // ERA 2 — Archaea
            us.RegisterUnitType("ArchaeaThermo",   typeof(Units.ArchaeaThermo));
            us.RegisterUnitType("ArchaeaHalo",     typeof(Units.ArchaeaHalo));
            us.RegisterUnitType("ArchaeaMethano",  typeof(Units.ArchaeaMethano));
            us.RegisterUnitType("ArchaeaElectro",  typeof(Units.ArchaeaElectro));
            // ERA 3 — Proto-Eucariotas
            us.RegisterUnitType("ProtoEukMitosoma",     typeof(Units.ProtoEukMitosoma));
            us.RegisterUnitType("ProtoEukVacuolar",     typeof(Units.ProtoEukVacuolar));
            us.RegisterUnitType("ProtoEukCytoskeletal", typeof(Units.ProtoEukCytoskeletal));
            us.RegisterUnitType("ProtoEukNucleated",    typeof(Units.ProtoEukNucleated));
            // ERA 4 — Eucariotas
            us.RegisterUnitType("EukMacrophage", typeof(Units.EukMacrophage));
            us.RegisterUnitType("EukNKCell",     typeof(Units.EukNKCell));
            us.RegisterUnitType("EukNeuron",     typeof(Units.EukNeuron));
            us.RegisterUnitType("EukStemCell",   typeof(Units.EukStemCell));

            Debug.Log("[Bootstrap] 16 tipos de unidad registrados en UnitSpawner.");
        }

        /// <summary>
        /// Registra tasas de producción base que simulan el metabolismo mínimo
        /// de una protocélula sin orgánulos construidos.
        /// Los orgánulos reales añadirán sus propias tasas encima de estas.
        /// </summary>
        /// <summary>
        /// Popula EcosystemManager con las 8 especies canónicas del GDD v3.5.
        /// Parámetros Lotka-Volterra calibrados para límites poblacionales duros v3.5:
        ///   Bacteria máx 40 | Paramecio máx 18 | Didinium máx 4
        ///   Hidra máx 2 | Tardígrado máx 2 | Bacteriófago máx 12
        /// </summary>
        private void PopulateEcosystem()
        {
            // TODO: Primordia — EcosystemPopulation fue deprecado.
            // En Primordia el ecosistema no usa Lotka-Volterra; los nodos de
            // recursos del ExteriorMap reemplazan este sistema.
        }

        private void RegisterBaseMetabolism()
        {
            var rm = ResourceManager.Instance;
            if (rm == null) return;

            // Primordia: 4 recursos activos (GDD v2 §3.2)
            rm.RegisterProductionRate(ResourceType.ATP,         5f);   // Glucólisis básica
            rm.RegisterProductionRate(ResourceType.AminoAcids,  1f);   // Síntesis proteica mínima
            rm.RegisterProductionRate(ResourceType.Nucleotides, 0.5f); // Transcripción mínima
            rm.RegisterProductionRate(ResourceType.Lipids,      0.5f); // Síntesis lipídica mínima

            Debug.Log("[Bootstrap] Metabolismo base Primordia: ATP+5/s, AA+1/s, NT+0.5/s, Lip+0.5/s");
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region Overlay debug (OnGUI — no necesita prefabs)

        private void InitStyles()
        {
            _boxStyle = new GUIStyle(GUI.skin.box)
            {
                padding  = new RectOffset(10, 10, 8, 8),
                fontSize = 13
            };
            _boxStyle.normal.background = MakeTex(2, 2, new Color(0.05f, 0.10f, 0.20f, 0.85f));

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 15,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = new Color(0f, 0.85f, 0.55f);

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 13 };
            _labelStyle.normal.textColor = Color.white;

            _stylesReady = true;
        }

        private void OnGUI()
        {
            if (!_stylesReady) InitStyles();

            // ── Panel de recursos (esquina superior izquierda) ──────────────────
            GUILayout.BeginArea(new Rect(10, 10, 220, 260), _boxStyle);
            GUILayout.Label("PROTOGENESIS  —  Recursos", _titleStyle);
            GUILayout.Space(4);

            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                DrawResource("ATP",         rm.GetResource(ResourceType.ATP),         rm.GetMaxResource(ResourceType.ATP),         new Color(0.29f, 0.56f, 0.85f));
                DrawResource("Amino Acids", rm.GetResource(ResourceType.AminoAcids),  rm.GetMaxResource(ResourceType.AminoAcids),  new Color(0.95f, 0.61f, 0.07f));
                DrawResource("Nucleotides", rm.GetResource(ResourceType.Nucleotides), rm.GetMaxResource(ResourceType.Nucleotides), new Color(0.18f, 0.80f, 0.44f));
                DrawResource("Lipids",      rm.GetResource(ResourceType.Lipids),      rm.GetMaxResource(ResourceType.Lipids),      new Color(0.55f, 0.22f, 0.85f));
            }
            else
            {
                GUILayout.Label("ResourceManager no disponible", _labelStyle);
            }
            GUILayout.EndArea();

            // ── Panel de slots (centro inferior) ────────────────────────────────
            DrawSlotPanel();

            // ── Panel de controles (esquina inferior izquierda) ─────────────────
            GUILayout.BeginArea(new Rect(10, Screen.height - 140, 250, 130), _boxStyle);
            GUILayout.Label("CONTROLES", _titleStyle);
            GUILayout.Space(4);
            GUILayout.Label("Click derecho  →  Mover", _labelStyle);
            GUILayout.Label("E / Tab        →  Interior ↔ Exterior", _labelStyle);
            GUILayout.Label("G              →  Instalar orgánulos", _labelStyle);
            GUILayout.Label("Q              →  Seguir recurso (auto)", _labelStyle);
            GUILayout.Label("Rueda          →  Zoom", _labelStyle);
            GUILayout.Label("Esc            →  Pausa", _labelStyle);
            GUILayout.EndArea();

            // DEPRECATED v4.6 — C (directiva colonia) desactivado en GDD v3.
            // if (Input.GetKeyDown(KeyCode.C)) { ... rs.SetDirective(...); }

            // Panel colonia (si hay unidades)
            DrawColonyStatus();

            // ── Panel de estado (esquina superior derecha) ──────────────────────
            GUILayout.BeginArea(new Rect(Screen.width - 210, 10, 200, 160), _boxStyle);
            GUILayout.Label("ESTADO", _titleStyle);
            GUILayout.Space(4);

            var gm = GameManager.Instance;
            if (gm != null)
            {
                GUILayout.Label($"ERA:    {gm.CurrentEra}", _labelStyle);
                GUILayout.Label($"Tiempo: {gm.ActivePlayTime:F0}s", _labelStyle);
            }

            // TODO: Primordia — HP de célula se muestra en PersistentHUD (Fase 1.3)

            // Metabolismo activo y tasa neta de ATP
            var me = MetabolismEngine.Instance;
            var rmState = ResourceManager.Instance;
            if (me != null)
            {
                var prev2 = GUI.color;
                GUI.color = me.ActiveRoute == "Fermentacion" ? new Color(0.9f, 0.5f, 0.1f)
                          : me.ActiveRoute == "Fotosintesis"  ? new Color(0.3f, 0.9f, 0.3f)
                          : me.ActiveRoute == "Mixotrofia"    ? new Color(0.5f, 0.8f, 1.0f)
                          : Color.white;
                GUILayout.Label($"Ruta:   {me.ActiveRoute}", _labelStyle);
                GUI.color = prev2;
            }
            if (rmState != null)
                GUILayout.Label($"ATP/s:  {rmState.GetNetRate(ResourceType.ATP):+0.0;-0.0}", _labelStyle);

            GUILayout.EndArea();
        }

        private void DrawColonyStatus()
        {
            var rs = Units.ReproductionSystem.Instance;
            if (rs == null || rs.ColonyCount == 0) return;

            GUILayout.BeginArea(new Rect(Screen.width - 210, 140, 200, 70), _boxStyle);
            GUILayout.Label("COLONIA", _titleStyle);
            GUILayout.Label($"Unidades: {rs.ColonyCount} / {rs.MaxColonySize}", _labelStyle);
            var prev = GUI.color;
            GUI.color = rs.CurrentDirective switch
            {
                Units.ColonyDirective.Defend  => new Color(0.9f, 0.3f, 0.3f),
                Units.ColonyDirective.Explore => new Color(0.3f, 0.7f, 1.0f),
                Units.ColonyDirective.Produce => new Color(0.2f, 0.9f, 0.4f),
                _                             => Color.white
            };
            GUILayout.Label($"Directiva: {rs.CurrentDirective}", _labelStyle);
            GUI.color = prev;
            GUILayout.EndArea();
        }

        private void DrawSlotPanel()
        {
            var sm = Slots.SlotManager.Instance;
            float w = 220f;
            float h = 210f;
            GUILayout.BeginArea(new Rect((Screen.width - w) / 2f, Screen.height - h - 10f, w, h), _boxStyle);
            GUILayout.Label("SLOTS DE EVOLUCIÓN", _titleStyle);
            GUILayout.Space(4);

            if (sm != null)
            {
                GUILayout.Label($"Instalados: {sm.TotalInstalledSlots}", _labelStyle);
                GUILayout.Space(2);
                foreach (SlotType t in System.Enum.GetValues(typeof(SlotType)))
                {
                    int used = sm.GetUsedSlots(t);
                    var prev = GUI.color;
                    GUI.color = used > 0 ? new Color(0.18f, 0.80f, 0.44f) : new Color(0.5f, 0.5f, 0.5f);
                    GUILayout.Label($"  {t,-14} ×{used}", _labelStyle);
                    GUI.color = prev;
                }
            }
            else
            {
                GUILayout.Label("SlotManager no disponible", _labelStyle);
            }
            GUILayout.EndArea();
        }

        private void DrawResource(string name, float current, float max, Color color)
        {
            var prev = GUI.color;
            GUI.color = color;
            GUILayout.Label($"{name,-10} {current,5:F0} / {max:F0}", _labelStyle);
            GUI.color = prev;
        }

        private static Texture2D MakeTex(int w, int h, Color col)
        {
            var pix = new Color[w * h];
            for (int i = 0; i < pix.Length; i++) pix[i] = col;
            var tex = new Texture2D(w, h);
            tex.SetPixels(pix);
            tex.Apply();
            return tex;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Crear Managers

        private void CreateManagers()
        {
            CreateIfMissing<GameManager>("GameManager");
            CreateIfMissing<ResourceManager>("ResourceManager");
            CreateIfMissing<EnvironmentManager>("EnvironmentManager");
            // Primordia — Vista Dual (Prompts 1.1-1.3)
            CreateIfMissing<Views.ViewManager>("ViewManager");
            CreateIfMissing<Views.CellState>("CellState");
            Debug.Log("[Bootstrap] Managers creados.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Crear CAP

        private void CreateCAP()
        {
            // TODO: Primordia — if (FindFirstObjectByType<Player.CAP>() != null) return;

            var capGO = new GameObject("CAP_Player");
            capGO.transform.position = capStartPosition;
            capGO.tag = "Player";

            // Componentes físicos
            var rb = capGO.AddComponent<Rigidbody2D>();
            rb.bodyType       = RigidbodyType2D.Kinematic;
            rb.gravityScale   = 0f;
            rb.freezeRotation = false;  // FluidMovementSystem rota el transform para squash/stretch

            var col = capGO.AddComponent<CircleCollider2D>();
            col.radius    = 0.4f;
            col.isTrigger = false;

            // Sprite placeholder — desactivado; ExteriorCellRenderer renderiza vía OnGUI
            var sr = capGO.AddComponent<SpriteRenderer>();
            sr.sprite  = CreateCircleSprite(32, Color.white);
            sr.color   = new Color(0.78f, 0.90f, 0.94f, 0.65f); // azul-traslúcido (fallback)
            sr.sortingOrder = 10;
            sr.enabled = false;  // ECR lo reemplaza

            // Punto de membrana — los enemigos buscan el tag "Membrane"
            var membraneGO = new GameObject("Membrane");
            membraneGO.transform.SetParent(capGO.transform, false);
            membraneGO.tag = "Membrane";
            var memCol = membraneGO.AddComponent<CircleCollider2D>();
            memCol.radius    = 0.4f;
            memCol.isTrigger = true;

            // ── Componentes de vista exterior (Primordia v3) ──────────────────────
            // Orden: requisitos primero, luego los que los requieren.
            capGO.AddComponent<MembraneSegmentSystem>();    // requerido por ExteriorCellRenderer
            capGO.AddComponent<FluidMovementSystem>();      // movimiento Bézier + squash/stretch
            capGO.AddComponent<ChemotaxisSystem>();         // seguimiento de gradientes
            capGO.AddComponent<FagocytosisSystem>();        // absorción de presas
            capGO.AddComponent<ExteriorCellRenderer>();     // renderizado procedural OnGUI
            capGO.AddComponent<PlayerCellExterior>();       // controlador principal del jugador

            // ZoneSystem centrado en la CAP
            var zoneGO = new GameObject("ZoneSystem");
            zoneGO.AddComponent<Organelles.ZoneSystem>();

            Debug.Log($"[Bootstrap] CAP creada en {capStartPosition} con V3 exterior components.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Crear Sistemas

        private void CreateSystems()
        {
            // MetabolismEngine (v4.6)
            if (FindFirstObjectByType<MetabolismEngine>() == null)
            {
                var me = new GameObject("MetabolismEngine");
                me.AddComponent<MetabolismEngine>();
            }

            // StressSystem (Primordia — reemplaza InstabilitySystem)
            if (FindFirstObjectByType<StressSystem>() == null)
            {
                var ss = new GameObject("StressSystem");
                ss.AddComponent<StressSystem>();
            }

            // InstabilitySystem (v4.6 legacy — coexiste con StressSystem durante migración)
            if (FindFirstObjectByType<InstabilitySystem>() == null)
            {
                var ins = new GameObject("InstabilitySystem");
                ins.AddComponent<InstabilitySystem>();
            }

            // MetabolicHeatSystem (v4.6)
            if (FindFirstObjectByType<MetabolicHeatSystem>() == null)
            {
                var mhs = new GameObject("MetabolicHeatSystem");
                mhs.AddComponent<MetabolicHeatSystem>();
            }

            // BalanceSystem (v4.6)
            if (FindFirstObjectByType<BalanceSystem>() == null)
            {
                var bs = new GameObject("BalanceSystem");
                bs.AddComponent<BalanceSystem>();
            }

            // GeneticTreeSystem (v4.6)
            if (FindFirstObjectByType<Progression.GeneticTreeSystem>() == null)
            {
                var gts = new GameObject("GeneticTreeSystem");
                gts.AddComponent<Progression.GeneticTreeSystem>();
            }

            // SpecializationTracker (Primordia Prompt 4.1)
            if (FindFirstObjectByType<Progression.SpecializationTracker>() == null)
            {
                var st = new GameObject("SpecializationTracker");
                st.AddComponent<Progression.SpecializationTracker>();
            }

            // CellProgressionPhase — progresión lineal en 4 fases
            if (FindFirstObjectByType<Progression.CellProgressionPhase>() == null)
            {
                var cpp = new GameObject("CellProgressionPhase");
                cpp.AddComponent<Progression.CellProgressionPhase>();
            }

            // TutorialArrow — flecha parpadeante sobre la célula en fase Protocell
            if (FindFirstObjectByType<UI.TutorialArrow>() == null)
            {
                var ta = new GameObject("TutorialArrow");
                ta.AddComponent<UI.TutorialArrow>();
            }

            // SpecializationTree (Primordia Prompt 4.3)
            if (FindFirstObjectByType<Progression.SpecializationTree>() == null)
            {
                var stree = new GameObject("SpecializationTree");
                stree.AddComponent<Progression.SpecializationTree>();
            }

            // ChallengeManager (Primordia Prompt 5.1)
            if (FindFirstObjectByType<Online.ChallengeManager>() == null)
            {
                var cm = new GameObject("ChallengeManager");
                cm.AddComponent<Online.ChallengeManager>();
            }

            // CombatMinigame (Primordia Prompt 5.2)
            if (FindFirstObjectByType<Online.CombatMinigame>() == null)
            {
                var cmg = new GameObject("CombatMinigame");
                cmg.AddComponent<Online.CombatMinigame>();
            }

            // EraManager (Primordia Prompt 8)
            if (FindFirstObjectByType<Progression.EraManager>() == null)
            {
                var em = new GameObject("EraManager");
                em.AddComponent<Progression.EraManager>();
            }

            // ZoneSpawner (Primordia Prompt 9) — enemies + nutrient hotspots en mapa exterior
            if (FindFirstObjectByType<Views.ZoneSpawner>() == null)
            {
                var zs = new GameObject("ZoneSpawner");
                zs.AddComponent<Views.ZoneSpawner>();
            }

            // OrganelleVisualizer (Primordia Prompt 10) — puntos de orgánulos en vista Interior
            // Se adjunta a PlayerCellInterior si existe; si no, se crea en espera.
            var pci = FindFirstObjectByType<Views.PlayerCellInterior>();
            if (pci != null && pci.GetComponent<Views.OrganelleVisualizer>() == null)
                pci.gameObject.AddComponent<Views.OrganelleVisualizer>();

            // UnitSpawner
            if (FindFirstObjectByType<UnitSpawner>() == null)
            {
                var us = new GameObject("UnitSpawner");
                us.AddComponent<UnitSpawner>();
            }

            // QuorumSensor
            if (FindFirstObjectByType<QuorumSensor>() == null)
            {
                var qs = new GameObject("QuorumSensor");
                qs.AddComponent<QuorumSensor>();
            }

            // OrganelleBuilder
            if (FindFirstObjectByType<Organelles.OrganelleBuilder>() == null)
            {
                var ob = new GameObject("OrganelleBuilder");
                ob.AddComponent<Organelles.OrganelleBuilder>();
            }

            // Progression
            if (FindFirstObjectByType<Progression.UpgradeTree>() == null)
            {
                var ut = new GameObject("ProgressionSystem");
                ut.AddComponent<Progression.UpgradeTree>();
                ut.AddComponent<Progression.GeneManager>();
            }

            // UI
            CreateBasicHUD();

            Debug.Log("[Bootstrap] Sistemas creados.");
        }

        /// <summary>
        /// Crea los sistemas de las Fases 2–7 del GDD v4.3 DEFINITIVO.
        /// Llamado después de CreateV3Systems() para garantizar que Camera.main y
        /// ExteriorView ya existen.
        /// </summary>
        private void CreateV43Systems()
        {
            // ── Fase 2 — División Celular ─────────────────────────────────────────
            if (FindFirstObjectByType<CellDivisionSystem>() == null)
            {
                var go = new GameObject("CellDivisionSystem");
                go.AddComponent<CellDivisionSystem>();
            }
            if (FindFirstObjectByType<DirectiveSystem>() == null)
            {
                var go = new GameObject("DirectiveSystem");
                go.AddComponent<DirectiveSystem>();
            }
            if (FindFirstObjectByType<ReabsorptionSystem>() == null)
            {
                var go = new GameObject("ReabsorptionSystem");
                go.AddComponent<ReabsorptionSystem>();
            }

            // ── Fase 3 — Especialización ──────────────────────────────────────────
            if (FindFirstObjectByType<ChemicalGradientSystem>() == null)
            {
                var go = new GameObject("ChemicalGradientSystem");
                go.AddComponent<ChemicalGradientSystem>();
            }
            if (FindFirstObjectByType<GeneticRingSystem>() == null)
            {
                var go = new GameObject("GeneticRingSystem");
                go.AddComponent<GeneticRingSystem>();
            }

            // ── Fase 4 — Visualización ────────────────────────────────────────────
            if (FindFirstObjectByType<PowerMomentSystem>() == null)
            {
                var go = new GameObject("PowerMomentSystem");
                go.AddComponent<PowerMomentSystem>();
            }
            if (FindFirstObjectByType<ZoomIntelligenceSystem>() == null)
            {
                var go = new GameObject("ZoomIntelligenceSystem");
                go.AddComponent<ZoomIntelligenceSystem>();
            }

            // ── Fase 5 — IA y Presión Sistémica ──────────────────────────────────
            if (FindFirstObjectByType<BiomassSystem>() == null)
            {
                var go = new GameObject("BiomassSystem");
                go.AddComponent<BiomassSystem>();
            }
            if (FindFirstObjectByType<PremiumUnitSystem>() == null)
            {
                var go = new GameObject("PremiumUnitSystem");
                go.AddComponent<PremiumUnitSystem>();
            }
            // Rival de prueba — Depredador, Normal
            if (FindFirstObjectByType<RivalCellController>() == null)
            {
                var rivalGO = new GameObject("RivalCell_AI");
                rivalGO.transform.position = new Vector2(6f, 5f);
                rivalGO.tag = "Enemy";

                var rb = rivalGO.AddComponent<Rigidbody2D>();
                rb.bodyType     = RigidbodyType2D.Kinematic;
                rb.gravityScale = 0f;

                var col = rivalGO.AddComponent<CircleCollider2D>();
                col.radius    = 0.4f;
                col.isTrigger = false;

                rivalGO.AddComponent<MembraneSegmentSystem>();
                rivalGO.AddComponent<FluidMovementSystem>();
                rivalGO.AddComponent<RivalCellController>();
            }

            // ── Fase 6 — Victoria y Post-Partida ─────────────────────────────────
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                if (canvas.GetComponent<PostMatchUI>() == null)
                    canvas.gameObject.AddComponent<PostMatchUI>();
                if (canvas.GetComponent<CellStateVisuals>() == null)
                    canvas.gameObject.AddComponent<CellStateVisuals>();
            }

            // ── Vista Interior — PlayerCellInterior + AtmosphereParticles ─────────
            var iv = FindFirstObjectByType<InteriorView>();
            if (iv != null)
            {
                var ivGO = iv.gameObject;
                if (ivGO.GetComponent<PlayerCellInterior>() == null)
                {
                    if (ivGO.GetComponent<Collider2D>() == null)
                    {
                        var boxCol = ivGO.AddComponent<BoxCollider2D>();
                        boxCol.size = new Vector2(8f, 8f);
                    }
                    ivGO.AddComponent<PlayerCellInterior>();
                }
                if (ivGO.GetComponent<AtmosphereParticles>() == null)
                    ivGO.AddComponent<AtmosphereParticles>();
            }

            Debug.Log("[Bootstrap] Sistemas v4.3 creados (Fases 2-7).");
        }

        private void CreateBasicHUD()
        {
            if (FindFirstObjectByType<Canvas>() != null) return;

            var canvasGO = new GameObject("Canvas_HUD");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            canvasGO.AddComponent<UI.ResourceHUD>();
            canvasGO.AddComponent<UI.AlertSystem>();
            canvasGO.AddComponent<UI.AbilityUI>();
            canvasGO.AddComponent<UI.EraProgressUI>();
            canvasGO.AddComponent<UI.EnvironmentMonitor>();

            // UI v3
            canvasGO.AddComponent<UI.PostMortemUI>();
            // TODO: Primordia — canvasGO.AddComponent<UI.CounterVisualizationSystem>();

            // Primordia — HUD de vista dual (Prompt 1.4)
            canvasGO.AddComponent<UI.DualViewHUD>();

            // Primordia — Panel de especialización (Prompt 4.2)
            canvasGO.AddComponent<UI.SpecializationUI>();

            // Primordia — Panel del árbol de especialización (Prompt 4.3)
            canvasGO.AddComponent<UI.SpecializationTreeUI>();

            // Primordia — HUD del encuentro 1v1 (Prompt 5.1)
            canvasGO.AddComponent<UI.ChallengeHUD>();

            // Primordia — Apoptosis countdown + Game Over screen (Prompt 7)
            canvasGO.AddComponent<UI.GameOverHUD>();

            // Primordia — Pantalla de victoria (Prompt 8)
            canvasGO.AddComponent<UI.VictoryHUD>();

            // Primordia — Minimapa exterior (Prompt 9)
            canvasGO.AddComponent<UI.MinimapHUD>();

            // Primordia — Stats de la célula en vista Interior (Prompt 10)
            canvasGO.AddComponent<UI.CellStatsHUD>();

            Debug.Log("[Bootstrap] HUD básico creado.");
        }

        /// <summary>
        /// Crea los sistemas nuevos del GDD v3:
        ///   - CellEntity (proxy de stats)
        ///   - SlotManager (plasticidad fenotípica v3)
        ///   - DeathSystem (regresión evolutiva)
        ///   - EcologicalDirector (moderador sistémico)
        ///   - EcosystemManager (Lotka-Volterra)
        ///   - MicroscopeModeManager (filtros ópticos + zoom)
        ///   - ReproductionSystem (colonia autónoma)
        ///   - ChemicalSignalSystem (lenguaje social)
        ///   - MorphogenesisVisualizer (en CAP)
        ///   - VictoryManager + CampaignManager
        /// </summary>
        private void CreateV3Systems()
        {
            // ── Core v3 ───────────────────────────────────────────────────────────
            var v3 = new GameObject("Systems_v3");

            CreateIfMissingOn<Slots.CellEntity>(v3);
            CreateIfMissingOn<Slots.SlotManager>(v3);
            CreateIfMissingOn<DeathSystem>(v3);
            // TODO: Primordia — CreateIfMissingOn<Ecosystem.EcologicalDirector>(v3);
            CreateIfMissingOn<Ecosystem.EcosystemManager>(v3);
            CreateIfMissingOn<Enemies.EnemySpawner>(v3);
            // ArchetypeResolver y ArchetypeAbilityUnlocker eliminados (v3 →  PhenotypeSystem v4.6)
            CreateIfMissingOn<Victory.VictoryManager>(v3);
            // TODO: Primordia — CreateIfMissingOn<Campaign.CampaignManager>(v3);
            CreateIfMissingOn<Units.ReproductionSystem>(v3);

            // ── Sistemas v5.2 (MVP PvP + Legado) ─────────────────────────────────
            // TODO: Primordia — CreateIfMissingOn<Player.ProtobacteriaSystem>(v3);
            // TODO: Primordia — CreateIfMissingOn<Ecosystem.GranOxigenacion>(v3);
            CreateIfMissingOn<LegacySystem>(v3);

            // ── Rendering v3 ──────────────────────────────────────────────────────
            var rendering = new GameObject("Rendering_v3");
            // TODO: Primordia — CreateIfMissingOn<Rendering.MicroscopeModeManager>(rendering);
            CreateIfMissingOn<Rendering.ZoomManager>(rendering);
            CreateIfMissingOn<Rendering.MapVisualizer>(rendering);

            // Primordia — Post-process reactivo (Prompt 6.1)
            CreateIfMissingOn<Rendering.PostProcessingController>(rendering);

            // Primordia — Audio reactivo (Prompt 6.2)
            CreateIfMissingOn<Rendering.AudioController>(rendering);

            // Primordia — Settings / Pausa (Prompt 6.3)
            if (FindFirstObjectByType<SettingsController>() == null)
            {
                var sc = new GameObject("SettingsController");
                sc.AddComponent<SettingsController>();
            }

            // TODO: Primordia — Visualizers deprecados. En Primordia los orgánulos
            // se visualizan en la vista Interior (Fase 2).

            // ── HUD v3 en el Canvas ───────────────────────────────────────────────
            var canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                if (canvas.GetComponent<UI.EvolutionHUD>() == null)
                    canvas.gameObject.AddComponent<UI.EvolutionHUD>();
                if (canvas.GetComponent<UI.SlotInstallUI>() == null)
                    canvas.gameObject.AddComponent<UI.SlotInstallUI>();
                if (canvas.GetComponent<UI.GeneticTreeUI>() == null)
                    canvas.gameObject.AddComponent<UI.GeneticTreeUI>();
                if (canvas.GetComponent<UI.DivisionHintUI>() == null)
                    canvas.gameObject.AddComponent<UI.DivisionHintUI>();
                // TODO: Primordia — if (canvas.GetComponent<UI.ProtobacteriaSelectUI>() == null)
                    // TODO: Primordia — canvas.gameObject.AddComponent<UI.ProtobacteriaSelectUI>();
            }

            // ── Vista Dual — ExteriorView / InteriorView ──────────────────────────
            // Deben crearse DESPUÉS de SetupCamera para que Camera.main esté disponible.
            // Sus Awake llaman ViewManager.RegisterView(this) y asignan viewCamera = Camera.main.
            // ExteriorView — con FogOfWar como hijo para que GetComponentInChildren lo encuentre
            var evView = FindFirstObjectByType<Views.ExteriorView>();
            if (evView == null)
            {
                var evGO = new GameObject("ExteriorView");
                evView = evGO.AddComponent<Views.ExteriorView>();
            }
            if (evView.GetComponentInChildren<Views.FogOfWar>(true) == null)
            {
                var fogGO = new GameObject("FogOfWar");
                fogGO.transform.SetParent(evView.transform);
                fogGO.AddComponent<Views.FogOfWar>();
            }

            if (FindFirstObjectByType<Views.InteriorView>() == null)
            {
                var ivGO = new GameObject("InteriorView");
                ivGO.AddComponent<Views.InteriorView>();
            }

            // Prompt 7.2 — Memoria Química
            if (FindFirstObjectByType<Views.ChemicalMemorySystem>() == null)
            {
                var cmGO = new GameObject("ChemicalMemorySystem");
                cmGO.AddComponent<Views.ChemicalMemorySystem>();
            }

            // Prompt 7.3 — Frontera circular
            if (FindFirstObjectByType<Views.CircularMapBoundary>() == null)
            {
                var cbGO = new GameObject("CircularMapBoundary");
                cbGO.AddComponent<Views.CircularMapBoundary>();
            }

            Debug.Log("[Bootstrap] Sistemas v3 AAA creados.");
        }

        private static T CreateIfMissingOn<T>(GameObject parent) where T : Component
        {
            var existing = FindFirstObjectByType<T>();
            if (existing != null) return existing;
            return parent.AddComponent<T>();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Cámara

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null)
            {
                var camGO = new GameObject("Main Camera");
                camGO.tag = "MainCamera";
                cam = camGO.AddComponent<Camera>();
                camGO.AddComponent<AudioListener>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 12f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.backgroundColor = new Color(0.42f, 0.56f, 0.64f, 1f);  // #6B8FA3 — agua microscopía
            cam.clearFlags = CameraClearFlags.SolidColor;

            Debug.Log("[Bootstrap] Cámara top-down configurada.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Enemy de prueba

        private void SpawnTestEnemies()
        {
            // Bacteriófago de prueba
            var phageGO = new GameObject("TestBacteriophage");
            phageGO.transform.position = new Vector2(8f, 0f);
            phageGO.tag = "Enemy";

            var phageRb = phageGO.AddComponent<Rigidbody2D>();
            phageRb.bodyType       = RigidbodyType2D.Kinematic;
            phageRb.gravityScale   = 0f;
            phageRb.freezeRotation = true;

            phageGO.AddComponent<CircleCollider2D>();

            var phageSr = phageGO.AddComponent<SpriteRenderer>();
            phageSr.sprite = CreateCircleSprite(16, Color.white);
            phageSr.color  = new Color(0.75f, 0.22f, 0.17f); // #C0392B
            phageSr.sortingOrder = 5;

            phageGO.AddComponent<Enemies.Bacteriophage>();

            Debug.Log("[Bootstrap] Bacteriófago de prueba spawneado en (8, 0).");

            // TEMP: TestTardigrado desactivado — WorldSize 500 genera escala (500,500,500)
            // que cubre todo el mapa con el sprite verde. Reactivar con prefab a escala correcta.
            // var tardiGO = new GameObject("TestTardigrado"); ...
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Nodos de recursos (GDD §6.1)

        /// <summary>
        /// Spawna los nodos de recursos iniciales en las tres zonas del mapa.
        ///
        /// Distribución canónica:
        ///   Fótica   (y  5…11) — 10 nodos Glucosa,  respawn 15 s
        ///   Pelágica (y -3… 3) —  3 nodos Glucosa,  respawn 25 s  (escasos)
        ///   Béntica  (y-11…-5) —  5 nodos Nitrógeno, respawn 20 s
        /// </summary>
        private void SpawnResourceNodes()
        {
            var parent = new GameObject("ResourceNodes");

            // Primordia — nodos mapeados a los 4 recursos activos
            // ── Zona Fótica (ATP, AminoAcids) ────────────────────────────────────
            for (int i = 0; i < 10; i++)
            {
                var pos = new Vector2(Random.Range(-13f, 13f), Random.Range(5f, 11f));
                SpawnNode(parent, pos, ResourceType.ATP, 1.0f, 15f, ZoneType.Fotica);
            }

            // ── Zona Pelágica (AminoAcids) ────────────────────────────────────────
            for (int i = 0; i < 3; i++)
            {
                var pos = new Vector2(Random.Range(-13f, 13f), Random.Range(-3f, 3f));
                SpawnNode(parent, pos, ResourceType.AminoAcids, 0.8f, 25f, ZoneType.Pelagica);
            }

            // ── Zona Béntica (Nucleotides) ────────────────────────────────────────
            for (int i = 0; i < 5; i++)
            {
                var pos = new Vector2(Random.Range(-13f, 13f), Random.Range(-11f, -5f));
                SpawnNode(parent, pos, ResourceType.Nucleotides, 1.5f, 20f, ZoneType.Bentonica);
            }

            Debug.Log("[Bootstrap] ResourceNodes spawneados: 10 Fótica(ATP), 3 Pelágica(AA), 5 Béntica(NT).");
        }

        private static void SpawnNode(GameObject parent, Vector2 pos,
                                      ResourceType type, float amount,
                                      float respawn, Ecosystem.ZoneType zone)
        {
            var go = new GameObject($"Node_{zone}_{type}");
            go.transform.SetParent(parent.transform);
            go.transform.position = pos;
            go.layer = LayerMask.NameToLayer("Default");

            var node = go.AddComponent<ResourceNode>();
            node.Init(type, amount, respawn, zone);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private static T CreateIfMissing<T>(string goName) where T : Component
        {
            var existing = FindFirstObjectByType<T>();
            if (existing != null) return existing;

            var go = new GameObject(goName);
            return go.AddComponent<T>();
        }

        /// <summary>Genera un sprite circular en tiempo de ejecución (para pruebas sin assets).</summary>
        private static Sprite CreateCircleSprite(int resolution, Color color)
        {
            var tex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float center = resolution / 2f;
            float radius = resolution / 2f - 1f;

            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    tex.SetPixel(x, y, dist <= radius ? color : Color.clear);
                }
            }

            tex.Apply();
            return Sprite.Create(tex,
                                 new Rect(0, 0, resolution, resolution),
                                 new Vector2(0.5f, 0.5f),
                                 resolution);
        }

        #endregion
    }
}
