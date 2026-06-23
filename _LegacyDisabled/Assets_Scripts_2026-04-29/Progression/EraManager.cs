using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Slots;
using Protogenesis.Views;

namespace Protogenesis.Progression
{
    /// <summary>
    /// EraManager — Evalúa condiciones de avance de era y llama a GameManager.AdvanceEra()
    /// cuando se cumplen todas las condiciones de la era actual (Primordia, Prompt 8).
    ///
    /// Condiciones por era:
    ///   ERA 0 → 1: Cualquier especialización consolidada (score ≥ 60 en SpecializationTracker).
    ///   ERA 1 → 2: ≥3 orgánulos instalados (SlotManager) + ≥500 ATP actuales + ≥120 s activos.
    ///   ERA 2 → 3: ≥3 especializaciones con score ≥ 30 + ≥200 s activos.
    ///   ERA 3 → 4: Algún nodo de tier 3 desbloqueado (SpecializationTree) + ≥300 s activos.
    ///   ERA 4    : Victoria — dispara EventBus.TriggerVictory.
    ///
    /// La evaluación se hace cada checkInterval segundos para no saturar la CPU.
    /// Expone ConditionResults[] para que EraProgressUI los dibuje sin recomputar.
    /// </summary>
    public class EraManager : MonoBehaviour
    {
        public static EraManager Instance { get; private set; }

        [SerializeField] private float checkInterval = 0.5f;

        /// <summary>Resultados de las condiciones de la era actual (true = cumplida).</summary>
        public bool[] ConditionResults { get; private set; } = System.Array.Empty<bool>();

        /// <summary>True cuando todas las condiciones del era actual están cumplidas.</summary>
        public bool AllConditionsMet { get; private set; } = false;

        private float _timer          = 0f;
        private bool  _victoryFired   = false;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver) return;
            if (SettingsController.IsPaused)  return;

            _timer += Time.deltaTime;
            if (_timer < checkInterval) return;
            _timer = 0f;

            Evaluate(gm);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Evaluación

        private void Evaluate(GameManager gm)
        {
            int era = gm.CurrentEra;

            // Victoria ya alcanzada
            if (era >= 4)
            {
                if (!_victoryFired)
                {
                    _victoryFired = true;
                    EventBus.TriggerVictory("eucariota_complejo");
                }
                ConditionResults = new[] { true };
                AllConditionsMet  = true;
                return;
            }

            bool[] results = ComputeConditions(era, gm);
            ConditionResults = results;
            AllConditionsMet  = AllTrue(results);

            // GameManager.AdvanceEra() guarda contra avances duplicados o retroactivos
            if (AllConditionsMet)
                gm.AdvanceEra(era + 1);
        }

        private bool[] ComputeConditions(int era, GameManager gm)
        {
            var st  = SpecializationTracker.Instance;
            var sm  = SlotManager.Instance;
            var rm  = ResourceManager.Instance;
            var tree = SpecializationTree.Instance;
            var cs  = CellState.Instance;

            return era switch
            {
                0 => new[]
                {
                    // Cualquier especialización consolidada
                    cs != null && cs.IsSpecializationConsolidated,
                },
                1 => new[]
                {
                    sm != null && sm.TotalInstalledSlots >= 3,
                    rm != null && rm.GetResource(ResourceType.ATP) >= 500f,
                    gm.ActivePlayTime >= 120f,
                },
                2 => new[]
                {
                    CountSpecsAbove(st, 30f) >= 3,
                    gm.ActivePlayTime >= 200f,
                },
                3 => new[]
                {
                    HasTier3Unlocked(tree),
                    gm.ActivePlayTime >= 300f,
                },
                _ => System.Array.Empty<bool>(),
            };
        }

        private static int CountSpecsAbove(SpecializationTracker st, float threshold)
        {
            if (st == null) return 0;
            int count = 0;
            foreach (SpecializationType type in System.Enum.GetValues(typeof(SpecializationType)))
                if (st.GetScore(type) >= threshold) count++;
            return count;
        }

        private static bool HasTier3Unlocked(SpecializationTree tree)
        {
            if (tree == null) return false;
            foreach (SpecializationType type in System.Enum.GetValues(typeof(SpecializationType)))
                if (tree.UnlockedTier(type) >= 3) return true;
            return false;
        }

        private static bool AllTrue(bool[] arr)
        {
            if (arr == null || arr.Length == 0) return false;
            foreach (var b in arr)
                if (!b) return false;
            return true;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Descripciones de las condiciones para la era indicada (para mostrar en HUD).
        /// </summary>
        public static string[] GetConditionLabels(int era)
        {
            return era switch
            {
                0 => new[] { "Consolidar una especialización (≥60 pts)" },
                1 => new[] { "3+ orgánulos instalados", "500 ATP actuales", "120 s activos" },
                2 => new[] { "3+ especializaciones ≥30 pts", "200 s activos" },
                3 => new[] { "Nodo Tier 3 desbloqueado", "300 s activos" },
                4 => new[] { "¡Eucariota Complejo alcanzado!" },
                _ => System.Array.Empty<string>(),
            };
        }

        #endregion
    }
}
