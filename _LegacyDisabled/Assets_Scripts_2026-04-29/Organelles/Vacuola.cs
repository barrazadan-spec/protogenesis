using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Vacuola — "El depósito de reserva celular".
    ///
    /// Expande la capacidad máxima de ATP y Glucosa al construirse.
    /// Al ser destruida, libera el 25% de los recursos almacenados.
    ///
    /// Niveles:
    ///   Nivel 1: +100 ATP máx, +50 Glucosa máx
    ///   Nivel 2: +200 ATP máx, +100 Glucosa máx  (acumulativo)
    ///   Nivel 3: +400 ATP máx, +200 Glucosa máx  (acumulativo)
    /// </summary>
    public class Vacuola : OrganelleBase
    {
        // ── Capacidad extra por nivel (acumulativa) ───────────────────────────────
        private static readonly float[] ATPCapacityBonus     = { 100f, 200f, 400f };
        private static readonly float[] GlucoseCapacityBonus = {  50f, 100f, 200f };

        private float _appliedATPBonus     = 0f;
        private float _appliedGlucoseBonus = 0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            ApplyCapacityBonus(CurrentLevel);
            EventBus.TriggerOrganelleBuilt(gameObject, OrganelleType);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Capacidad

        private void ApplyCapacityBonus(int level)
        {
            var rm = ResourceManager.Instance;
            if (rm == null) return;

            // Eliminar el bonus anterior antes de aplicar el nuevo nivel
            if (_appliedATPBonus > 0f)
            {
                rm.ExpandMax(ResourceType.ATP,     -_appliedATPBonus);
                rm.ExpandMax(ResourceType.Glucose, -_appliedGlucoseBonus);
            }

            int idx = Mathf.Clamp(level - 1, 0, ATPCapacityBonus.Length - 1);
            _appliedATPBonus     = ATPCapacityBonus[idx];
            _appliedGlucoseBonus = GlucoseCapacityBonus[idx];

            rm.ExpandMax(ResourceType.ATP,     _appliedATPBonus);
            rm.ExpandMax(ResourceType.Glucose, _appliedGlucoseBonus);

            Debug.Log($"[Vacuola] Capacidad aplicada (nivel {level}): +{_appliedATPBonus} ATP máx, +{_appliedGlucoseBonus} Glucosa máx.");
        }

        protected override void OnLevelUp()
        {
            ApplyCapacityBonus(CurrentLevel);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Destrucción — libera recursos

        protected override void OnOrganelleDestroyed()
        {
            // Retirar el bonus de capacidad
            var rm = ResourceManager.Instance;
            if (rm != null && _appliedATPBonus > 0f)
            {
                rm.ExpandMax(ResourceType.ATP,     -_appliedATPBonus);
                rm.ExpandMax(ResourceType.Glucose, -_appliedGlucoseBonus);

                // Libera el 25% del bonus como ATP libre
                rm.AddResource(ResourceType.ATP, _appliedATPBonus * 0.25f);
            }
        }

        #endregion
    }
}
