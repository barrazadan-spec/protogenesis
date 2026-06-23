using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5GameManager : MonoBehaviour
    {
        public static V5GameManager Instance { get; private set; }

        [Header("Runtime Systems")]
        public V5EnvironmentGrid Environment;
        public V5ResourceSystem Resources;
        public V5CellFactory CellFactory;
        public V5SelectionSystem Selection;
        public V5CombatSystem Combat;
        public V5AbilitySystem Abilities;
        public V5ScenarioSystem Scenario;
        public V5GeneSystem Genes;
        public V5AdaptationSystem Adaptations;
        public V5MvpRouteIntentSystem MvpIntent;
        public V5NichePressureSystem NichePressure;
        public V5RouteMasterySystem RouteMastery;
        public V5RouteBuildSystem RouteBuilds;
        public V5RouteClimaxSystem RouteClimax;
        public V5RouteChapterSystem RouteChapters;
        public V5RouteBranchSystem RouteBranches;
        public V5RouteCounterPressureSystem RouteCounters;
        public V5IdentityRecognizer Identity;
        public V5ApexSystem Apex;
        public V5WorldEventSystem WorldEvents;
        public V5FogOfWarSystem Fog;
        public V5DifficultyDirector Director;
        public V5MissionSystem Mission;
        public V5PlayableLoopSystem PlayableLoop;
        public V5TelemetrySystem Telemetry;
        public V5RunDiagnosticsSystem Diagnostics;
        public V5DebugCheats DebugCheats;
        public V5SaveSystem Save;
        public V5AdvisorSystem Advisor;
        public V5LiveCoachSystem LiveCoach;
        public V5CodexSystem Codex;
        public V5RuntimeSettings RuntimeSettings;
        public V5HudIMGUI Hud;
        public V5CameraController CameraController;
        public V5ControlGroupSystem ControlGroups;
        public V5SquadTacticsSystem Squads;
        public V5LineageSystem Lineages;
        public V5BiomeSystem Biomes;
        public V5LineageUpgradeSystem LineageUpgrades;
        public V5ThreatEcologySystem ThreatEcology;
        public V5BalanceProfileSystem BalanceProfile;
        public V5RunResetSystem RunReset;
        public V5ColonialContinuitySystem Continuity;
        public V5MulticellularBodySystem Body;
        public V5BattlefieldStateRecognizer Battlefield;
        public V5GerminalProductionSystem Germinal;
        public V5AffinityEventLog AffinityLog;
        public V5EvolutionRouteSystem RouteLifecycle;
        public V5BodyPanelIMGUI BodyPanel;
        public V5GenomePanelIMGUI GenomePanel;
        public V5OrganismMorph OrganismMorph;
        public V5CoreTerritorySystem CoreTerritory;
        public V5CoreMotherProductionSystem CoreMotherProduction;

        [Header("State")]
        public V5CellEntity MotherCell;
        public V5GamePhase Phase = V5GamePhase.Primordial;
        public V5ScenarioId ScenarioId = V5ScenarioId.FirstDrop;
        public float ElapsedSeconds;
        public bool Paused;
        public bool CoreMode;

        private readonly List<V5CellEntity> playerCells = new List<V5CellEntity>(64);
        private readonly List<V5CellEntity> nonPlayerCells = new List<V5CellEntity>(64);

        public IReadOnlyList<V5CellEntity> PlayerCells { get { return playerCells; } }
        public IReadOnlyList<V5CellEntity> NonPlayerCells { get { return nonPlayerCells; } }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void OnEnable()
        {
            if (Instance == null) Instance = this;
        }

        private void Update()
        {
            if (Paused || Phase == V5GamePhase.Victory || Phase == V5GamePhase.Defeat) return;
            ElapsedSeconds += Time.deltaTime;
            if (CoreMode) Phase = V5GamePhase.Primordial;
            else UpdatePhase();
            CleanupDeadLists();
        }

        private void UpdatePhase()
        {
            if (ElapsedSeconds < 300f) Phase = V5GamePhase.Primordial;
            else if (ElapsedSeconds < 900f) Phase = V5GamePhase.Differentiation;
            else if (ElapsedSeconds < 1500f) Phase = V5GamePhase.Expansion;
            else if (ElapsedSeconds < 2100f) Phase = V5GamePhase.Dominance;
            else Phase = V5GamePhase.Resolution;
        }

        public void RegisterCell(V5CellEntity cell)
        {
            if (cell == null) return;
            List<V5CellEntity> list = cell.IsPlayerOwned ? playerCells : nonPlayerCells;
            if (!list.Contains(cell)) list.Add(cell);
            if (Codex != null) Codex.ObserveCell(cell);
        }

        public void UnregisterCell(V5CellEntity cell)
        {
            if (cell == null) return;
            if (OrganismMorph != null) OrganismMorph.NotifyCellUnavailable(cell);
            playerCells.Remove(cell);
            nonPlayerCells.Remove(cell);
            if (Selection != null) Selection.Deselect(cell);
        }

        public void ClearCellsForLoad()
        {
            List<V5CellEntity> copy = new List<V5CellEntity>(playerCells.Count + nonPlayerCells.Count);
            copy.AddRange(playerCells);
            copy.AddRange(nonPlayerCells);
            for (int i = 0; i < copy.Count; i++)
            {
                if (copy[i] != null) Destroy(copy[i].gameObject);
            }
            playerCells.Clear();
            nonPlayerCells.Clear();
            MotherCell = null;
            if (Selection != null) Selection.ClearSelection();
            ResetAllRunSystems();
        }

        public void ResetAllRunSystems()
        {
            MonoBehaviour[] systems = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < systems.Length; i++)
            {
                IV5RunResettable resettable = systems[i] as IV5RunResettable;
                if (resettable != null) resettable.ResetForNewRun();
            }

            if (Fog != null) Fog.FullyRevealed = false;
        }

        private void CleanupDeadLists()
        {
            for (int i = playerCells.Count - 1; i >= 0; i--)
                if (playerCells[i] == null) playerCells.RemoveAt(i);
            for (int i = nonPlayerCells.Count - 1; i >= 0; i--)
                if (nonPlayerCells[i] == null) nonPlayerCells.RemoveAt(i);
        }

        public int PlayerCellCount()
        {
            return playerCells.Count;
        }

        public int PlayerTotalCellCount()
        {
            int count = 0;
            for (int i = 0; i < playerCells.Count; i++)
                if (IsLivingPlayerCell(playerCells[i])) count++;
            return count;
        }

        public int PlayerFreeCellCount()
        {
            int count = 0;
            for (int i = 0; i < playerCells.Count; i++)
            {
                V5CellEntity cell = playerCells[i];
                if (IsLivingPlayerCell(cell) && !cell.IsMorphedOrganism) count++;
            }
            return count;
        }

        private bool IsLivingPlayerCell(V5CellEntity cell)
        {
            return cell != null && cell.IsPlayerOwned && cell.Stats.currentHp > 0f;
        }

        public int PlayerCellCap()
        {
            if (CoreMode) return V5Balance.CorePlayerCellCap + (CoreMotherProduction != null ? CoreMotherProduction.FreeCellCapBonus : 0);
            return V5Balance.HardControllableEntityCap;
        }

        public float PlayerPopulationLoad()
        {
            return V5Balance.PopulationLoad(playerCells);
        }

        public bool CanAddPlayerCellFrom(V5CellEntity parent)
        {
            return !V5Balance.WouldExceedPlayerPopulationCap(this, parent);
        }

        public void Win(string reason)
        {
            Phase = V5GamePhase.Victory;
            if (Hud != null) Hud.EndMessage = "VICTORIA - " + reason;
            if (Codex != null) Codex.Unlock("Victoria ecologica", "Ganaste una run mediante dominancia, supervivencia o control de amenazas.");
        }

        public void Lose(string reason)
        {
            if (CoreMode)
            {
                V5CoreRivalColonySystem rival = FindFirstObjectByType<V5CoreRivalColonySystem>();
                if (rival != null && rival.ShouldAllowCoreMotherDefeat())
                {
                    Phase = V5GamePhase.Defeat;
                    if (Hud != null) Hud.EndMessage = "DERROTA - " + reason;
                    V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                    if (feedback != null) feedback.Push("DERROTA: " + reason, MotherCell != null ? (Vector2)MotherCell.transform.position : Vector2.zero, new Color(1f, 0.25f, 0.2f, 1f));
                    return;
                }

                Phase = V5GamePhase.Primordial;
                if (Hud != null) Hud.EndMessage = "";
                Debug.LogWarning("[V5Core] Ignored sandbox defeat: " + reason);
                return;
            }

            Phase = V5GamePhase.Defeat;
            if (Hud != null) Hud.EndMessage = "DERROTA - " + reason;
        }
    }
}
