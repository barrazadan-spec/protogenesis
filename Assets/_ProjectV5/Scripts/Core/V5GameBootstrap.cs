using UnityEngine;

namespace Protogenesis.V5
{
    public class V5GameBootstrap : MonoBehaviour
    {
        [Header("Prototype Settings")]
        public V5ScenarioId scenario = V5ScenarioId.FirstDrop;
        public int mapWidth = 96;
        public int mapHeight = 96;
        public float tileSize = 0.85f;
        public int startingResourceNodes = 85;
        public int neutralCells = 10;
        public bool useScenarioPreset = true;
        public bool createCamera = true;
        public bool spawnEnemies = true;
        public bool coreMode = false;

        private void Awake()
        {
            BuildRuntime();
        }

        public void BuildRuntime()
        {
            V5GameManager gm = FindFirstObjectByType<V5GameManager>();
            if (gm == null)
            {
                GameObject root = new GameObject("V5_GameManager");
                gm = root.AddComponent<V5GameManager>();
            }
            gm.CoreMode = coreMode;

            if (coreMode)
            {
                BuildCoreRuntime(gm);
                return;
            }

            gm.ScenarioId = scenario;
            V5ScenarioDefinition scenarioDef = V5ScenarioLibrary.Get(scenario);
            int finalWidth = useScenarioPreset ? scenarioDef.mapWidth : mapWidth;
            int finalHeight = useScenarioPreset ? scenarioDef.mapHeight : mapHeight;
            float finalTileSize = useScenarioPreset ? scenarioDef.tileSize : tileSize;
            int finalResources = useScenarioPreset ? scenarioDef.startingResources : startingResourceNodes;
            finalWidth = Mathf.Max(finalWidth, 180);
            finalHeight = Mathf.Max(finalHeight, 180);
            finalResources = Mathf.Clamp(finalResources, 62, 78);
            int finalNeutrals = useScenarioPreset ? scenarioDef.neutralCells : neutralCells;
            bool finalSpawnEnemies = spawnEnemies && (!useScenarioPreset || scenarioDef.spawnEnemies);

            gm.Environment = GetOrCreate<V5EnvironmentGrid>("V5_EnvironmentGrid");
            gm.Environment.Initialize(finalWidth, finalHeight, finalTileSize, Mathf.Min(finalWidth, finalHeight) * finalTileSize * 0.47f);
            gm.Environment.ApplyScenarioBias(scenarioDef);

            gm.Resources = GetOrCreate<V5ResourceSystem>("V5_ResourceSystem");
            gm.Resources.InitialNodeCount = finalResources;

            gm.CellFactory = GetOrCreate<V5CellFactory>("V5_CellFactory");
            gm.Selection = GetOrCreate<V5SelectionSystem>("V5_SelectionSystem");
            gm.ControlGroups = GetOrCreate<V5ControlGroupSystem>("V5_ControlGroups");
            gm.Combat = GetOrCreate<V5CombatSystem>("V5_CombatSystem");
            gm.Abilities = GetOrCreate<V5AbilitySystem>("V5_AbilitySystem");
            gm.Scenario = GetOrCreate<V5ScenarioSystem>("V5_ScenarioSystem");
            gm.Genes = GetOrCreate<V5GeneSystem>("V5_GeneSystem");
            gm.Adaptations = GetOrCreate<V5AdaptationSystem>("V5_AdaptationSystem");
            gm.MvpIntent = GetOrCreate<V5MvpRouteIntentSystem>("V5_MvpRouteIntentSystem");
            gm.NichePressure = GetOrCreate<V5NichePressureSystem>("V5_NichePressureSystem");
            gm.RouteMastery = GetOrCreate<V5RouteMasterySystem>("V5_RouteMasterySystem");
            gm.RouteBuilds = GetOrCreate<V5RouteBuildSystem>("V5_RouteBuildSystem");
            gm.RouteClimax = GetOrCreate<V5RouteClimaxSystem>("V5_RouteClimaxSystem");
            gm.RouteChapters = GetOrCreate<V5RouteChapterSystem>("V5_RouteChapterSystem");
            gm.RouteBranches = GetOrCreate<V5RouteBranchSystem>("V5_RouteBranchSystem");
            gm.RouteCounters = GetOrCreate<V5RouteCounterPressureSystem>("V5_RouteCounterPressureSystem");
            gm.Identity = GetOrCreate<V5IdentityRecognizer>("V5_IdentityRecognizer");
            gm.Apex = GetOrCreate<V5ApexSystem>("V5_ApexSystem");
            gm.WorldEvents = GetOrCreate<V5WorldEventSystem>("V5_WorldEventSystem");
            gm.Fog = GetOrCreate<V5FogOfWarSystem>("V5_FogOfWar");
            gm.Fog.Environment = gm.Environment;
            gm.Director = GetOrCreate<V5DifficultyDirector>("V5_DifficultyDirector");
            gm.PlayableLoop = GetOrCreate<V5PlayableLoopSystem>("V5_PlayableLoopSystem");
            gm.Mission = GetOrCreate<V5MissionSystem>("V5_MissionSystem");
            gm.Telemetry = GetOrCreate<V5TelemetrySystem>("V5_TelemetrySystem");
            gm.Diagnostics = GetOrCreate<V5RunDiagnosticsSystem>("V5_RunDiagnosticsSystem");
            gm.DebugCheats = GetOrCreate<V5DebugCheats>("V5_DebugCheats");
            gm.Save = GetOrCreate<V5SaveSystem>("V5_SaveSystem");
            gm.Advisor = GetOrCreate<V5AdvisorSystem>("V5_AdvisorSystem");
            gm.LiveCoach = GetOrCreate<V5LiveCoachSystem>("V5_LiveCoachSystem");
            gm.Codex = GetOrCreate<V5CodexSystem>("V5_CodexSystem");
            gm.RuntimeSettings = GetOrCreate<V5RuntimeSettings>("V5_RuntimeSettings");
            gm.ControlGroups = GetOrCreate<V5ControlGroupSystem>("V5_ControlGroups");
            gm.Squads = GetOrCreate<V5SquadTacticsSystem>("V5_SquadTacticsSystem");
            gm.Lineages = GetOrCreate<V5LineageSystem>("V5_LineageSystem");
            gm.Biomes = GetOrCreate<V5BiomeSystem>("V5_BiomeSystem");
            gm.LineageUpgrades = GetOrCreate<V5LineageUpgradeSystem>("V5_LineageUpgradeSystem");
            gm.ThreatEcology = GetOrCreate<V5ThreatEcologySystem>("V5_ThreatEcologySystem");
            gm.BalanceProfile = GetOrCreate<V5BalanceProfileSystem>("V5_BalanceProfileSystem");
            gm.RunReset = GetOrCreate<V5RunResetSystem>("V5_RunResetSystem");
            gm.Continuity = GetOrCreate<V5ColonialContinuitySystem>("V5_ColonialContinuitySystem");
            gm.Body = GetOrCreate<V5MulticellularBodySystem>("V5_MulticellularBodySystem");
            gm.Battlefield = GetOrCreate<V5BattlefieldStateRecognizer>("V5_BattlefieldStateRecognizer");
            gm.Germinal = GetOrCreate<V5GerminalProductionSystem>("V5_GerminalProductionSystem");
            gm.AffinityLog = GetOrCreate<V5AffinityEventLog>("V5_AffinityEventLog");
            gm.RouteLifecycle = GetOrCreate<V5EvolutionRouteSystem>("V5_EvolutionRouteSystem");
            gm.GenomePanel = GetOrCreate<V5GenomePanelIMGUI>("V5_GenomePanel");
            gm.Hud = GetOrCreate<V5HudIMGUI>("V5_HUD");
            gm.BodyPanel = GetOrCreate<V5BodyPanelIMGUI>("V5_BodyPanel");
            gm.Hud.EndMessage = "";
            GetOrCreate<V5RunSummarySystem>("V5_RunSummarySystem");
            GetOrCreate<V5PlaytestReportSystem>("V5_PlaytestReportSystem");
            GetOrCreate<V5FeedbackSystem>("V5_FeedbackSystem");
            GetOrCreate<V5MinimapIMGUI>("V5_Minimap");
            GetOrCreate<V5ScenarioMenuIMGUI>("V5_ScenarioMenu");
            V5EnvironmentOverlay overlay = GetOrCreate<V5EnvironmentOverlay>("V5_EnvironmentOverlay");
            overlay.Environment = gm.Environment;

            Camera mainCamera = Camera.main;
            if (createCamera && mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            if (mainCamera != null)
            {
                ConfigureMainCamera(mainCamera, gm.Environment);
                gm.CameraController = mainCamera.GetComponent<V5CameraController>();
                if (gm.CameraController == null) gm.CameraController = mainCamera.gameObject.AddComponent<V5CameraController>();
            }

            V5CellEntity mother = gm.CellFactory.SpawnMother(Vector2.zero);
            if (gm.CameraController != null) gm.CameraController.SnapTo(mother != null ? mother.transform : null);
            gm.Resources.SpawnInitialNodes();
            gm.ResetAllRunSystems();

            if (finalSpawnEnemies)
            {
                for (int i = 0; i < finalNeutrals; i++)
                {
                    Vector2 p = Random.insideUnitCircle.normalized * Random.Range(10f, gm.Environment.MapRadius * 0.8f);
                    V5EvolutionPath path = V5EcologySpawnPolicy.PickInitialPath(scenario, i);
                    V5CellEntity enemy = gm.CellFactory.SpawnNeutral(p, path);
                    V5EcologySpawnPolicy.ConfigureInitialNpc(enemy, scenario, i);
                }
            }

            gm.Scenario.BeginScenario(scenario);
        }

        private void BuildCoreRuntime(V5GameManager gm)
        {
            gm.ScenarioId = scenario;
            gm.Phase = V5GamePhase.Primordial;

            V5ScenarioDefinition scenarioDef = V5ScenarioLibrary.Get(scenario);
            int finalWidth = useScenarioPreset ? scenarioDef.mapWidth : mapWidth;
            int finalHeight = useScenarioPreset ? scenarioDef.mapHeight : mapHeight;
            float finalTileSize = useScenarioPreset ? scenarioDef.tileSize : tileSize;
            int finalResources = useScenarioPreset ? scenarioDef.startingResources : startingResourceNodes;
            finalWidth = Mathf.Max(finalWidth, 280);
            finalHeight = Mathf.Max(finalHeight, 280);

            gm.Environment = GetOrCreate<V5EnvironmentGrid>("V5_EnvironmentGrid");
            gm.Environment.Initialize(finalWidth, finalHeight, finalTileSize, Mathf.Min(finalWidth, finalHeight) * finalTileSize * 0.47f);
            gm.Environment.ApplyScenarioBias(scenarioDef);

            gm.Resources = GetOrCreate<V5ResourceSystem>("V5_ResourceSystem");
            gm.Resources.InitialNodeCount = finalResources;

            gm.CellFactory = GetOrCreate<V5CellFactory>("V5_CellFactory");
            gm.Selection = GetOrCreate<V5SelectionSystem>("V5_SelectionSystem");
            gm.Combat = GetOrCreate<V5CombatSystem>("V5_CombatSystem");
            gm.Body = GetOrCreate<V5MulticellularBodySystem>("V5_MulticellularBodySystem");
            gm.OrganismMorph = GetOrCreate<V5OrganismMorph>("V5_OrganismMorph");
            gm.OrganismMorph.MaxActiveOrganisms = Mathf.Max(gm.OrganismMorph.MaxActiveOrganisms, V5OrganismMorph.DefaultMaxActiveOrganisms);
            gm.CoreTerritory = GetOrCreate<V5CoreTerritorySystem>("V5_CoreTerritory");
            gm.CoreMotherProduction = GetOrCreate<V5CoreMotherProductionSystem>("V5_CoreMotherProduction");
            V5CoreLightOasisSystem lightOases = GetOrCreate<V5CoreLightOasisSystem>("V5_CoreLightOases");
            GetOrCreate<V5CoreRivalColonySystem>("V5_CoreRivalColony");
            GetOrCreate<V5CoreNeutralCampSystem>("V5_CoreNeutralCamps");
            GetOrCreate<V5CorePredatorSystem>("V5_CorePredatorSystem");
            GetOrCreate<V5CoreHudIMGUI>("V5_CoreHUD");
            GetOrCreate<V5FeedbackSystem>("V5_FeedbackSystem");

            V5EnvironmentOverlay overlay = GetOrCreate<V5EnvironmentOverlay>("V5_EnvironmentOverlay");
            overlay.Environment = gm.Environment;
            overlay.Mode = V5OverlayMode.None;

            Camera mainCamera = Camera.main;
            if (createCamera && mainCamera == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                mainCamera = camObj.AddComponent<Camera>();
                camObj.AddComponent<AudioListener>();
            }

            if (mainCamera != null)
            {
                ConfigureMainCamera(mainCamera, gm.Environment);
                gm.CameraController = mainCamera.GetComponent<V5CameraController>();
                if (gm.CameraController == null) gm.CameraController = mainCamera.gameObject.AddComponent<V5CameraController>();
                gm.CameraController.panSpeed = Mathf.Max(gm.CameraController.panSpeed, 24f);
                gm.CameraController.maxZoom = Mathf.Max(gm.CameraController.maxZoom, gm.Environment.MapRadius * 0.58f);
            }

            Vector2 motherStart = Vector2.left * gm.Environment.MapRadius * 0.72f;
            V5CellEntity mother = gm.CellFactory.SpawnMother(motherStart);
            if (gm.CoreMotherProduction != null) gm.CoreMotherProduction.SeedStartingCells(gm, 3);
            if (gm.CameraController != null) gm.CameraController.SnapTo(mother != null ? mother.transform : null);
            lightOases.Build(gm, motherStart);
            gm.Resources.SpawnInitialNodes();
            SpawnCoreBaseResources(gm, motherStart, 18);
            gm.ResetAllRunSystems();
        }

        private void SpawnCoreBaseResources(V5GameManager gm, Vector2 center, int count)
        {
            if (gm == null || gm.Resources == null) return;
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2.3999632f;
                float ring = Random.Range(4.5f, 15.5f);
                Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ring + Random.insideUnitCircle * 1.7f;
                if (gm.Environment != null && pos.magnitude > gm.Environment.MapRadius * 0.88f) pos = pos.normalized * gm.Environment.MapRadius * 0.88f;
                gm.Resources.SpawnNode(pos, V5ResourceKind.Biomass, Random.Range(34f, 76f), 1.05f);
            }
        }

        private T GetOrCreate<T>(string name) where T : Component
        {
            T existing = FindFirstObjectByType<T>();
            if (existing != null) return existing;
            GameObject go = new GameObject(name);
            return go.AddComponent<T>();
        }

        private void ConfigureMainCamera(Camera cam, V5EnvironmentGrid environment)
        {
            cam.tag = "MainCamera";
            cam.orthographic = true;
            cam.orthographicSize = environment != null ? Mathf.Clamp(environment.MapRadius * 0.34f, 10f, 18f) : 14f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.035f, 0.065f, 0.085f, 1f);
            cam.nearClipPlane = 0.1f;
            cam.farClipPlane = 200f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
        }

    }
}
