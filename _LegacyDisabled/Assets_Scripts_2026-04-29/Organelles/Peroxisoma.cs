using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Peroxisoma — "La planta de desintoxicación celular".
    ///
    /// Reduce la inestabilidad metabólica pasivamente cada segundo.
    /// Al nivel 3, también absorbe parte del calor metabólico.
    ///
    /// Niveles:
    ///   Nivel 1: -0.15 inestabilidad/s
    ///   Nivel 2: -0.30 inestabilidad/s
    ///   Nivel 3: -0.50 inestabilidad/s  +  -0.25 calor/s
    /// </summary>
    public class Peroxisoma : OrganelleBase
    {
        private static readonly float[] InstabilityReduction = { 0.15f, 0.30f, 0.50f };
        private const float HeatReductionLvl3 = 0.25f;

        private float _tickTimer = 0f;
        private const float TickInterval = 1f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Start()
        {
            EventBus.TriggerOrganelleBuilt(gameObject, OrganelleType);
        }

        protected override void Update()
        {
            base.Update();

            _tickTimer += Time.deltaTime;
            if (_tickTimer < TickInterval) return;
            _tickTimer = 0f;

            ApplyDetoxification();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Detoxificación

        private void ApplyDetoxification()
        {
            float reduction = InstabilityReduction[CurrentLevel - 1] * Efficiency;

            InstabilitySystem.Instance?.ReduceInstability(reduction);

            if (CurrentLevel >= 3)
                MetabolicHeatSystem.Instance?.AddHeat(-HeatReductionLvl3 * Efficiency);
        }

        protected override void OnLevelUp()
        {
            Debug.Log($"[Peroxisoma] Nivel {CurrentLevel}: reducción de inestabilidad = {InstabilityReduction[CurrentLevel - 1]:F2}/s");
        }

        #endregion
    }
}
