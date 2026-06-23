using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Slots;

namespace Protogenesis.Archetypes
{
    /// <summary>
    /// Escucha cambios de arquetipo y aplica/revierte los bonos pasivos del arquetipo
    /// sobre CellEntity. También notifica el desbloqueo de la habilidad única.
    ///
    /// Principio: los bonos del arquetipo anterior se revierten antes de aplicar el nuevo.
    /// </summary>
    public class ArchetypeAbilityUnlocker : MonoBehaviour
    {
        [Header("Definiciones de arquetipos")]
        [SerializeField] private ArchetypeDefinition[] definitions;

        private ArchetypeDefinition _currentDef;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void OnEnable()
        {
            EventBus.OnArchetypeChanged += OnArchetypeChanged;
        }

        private void OnDisable()
        {
            EventBus.OnArchetypeChanged -= OnArchetypeChanged;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Handlers

        private void OnArchetypeChanged(ArchetypeType previous, ArchetypeType next)
        {
            var entity = CellEntity.Instance;
            if (entity == null) return;

            // Revertir bonos del arquetipo anterior
            if (_currentDef != null)
                RemoveBonuses(_currentDef, entity);

            // Aplicar bonos del nuevo arquetipo
            _currentDef = FindDefinition(next);
            if (_currentDef != null)
            {
                ApplyBonuses(_currentDef, entity);

                if (!string.IsNullOrEmpty(_currentDef.uniqueAbilityId))
                    EventBus.TriggerArchetypeAbilityUnlocked(next, _currentDef.uniqueAbilityId);

                Debug.Log($"[ArchetypeAbilityUnlocker] Bonos de '{_currentDef.displayName}' aplicados.");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private void ApplyBonuses(ArchetypeDefinition def, CellEntity entity)
        {
            if (def.damageBonus        > 0f) entity.AddDamageMultiplier(def.damageBonus);
            if (def.moveSpeedBonus     > 0f) entity.AddMoveSpeedMult(def.moveSpeedBonus);
            if (def.maxHPBonus         > 0f) entity.AddMaxHPBonus(def.maxHPBonus);
            if (def.atpProductionBonus > 0f) entity.AddATPMult(def.atpProductionBonus);
            if (def.damageReduction    > 0f) entity.AddDamageReduction(def.damageReduction);
            if (def.hpRegenBonus       > 0f) entity.AddHPRegen(def.hpRegenBonus);
        }

        private void RemoveBonuses(ArchetypeDefinition def, CellEntity entity)
        {
            if (def.damageBonus        > 0f) entity.RemoveDamageMultiplier(def.damageBonus);
            if (def.moveSpeedBonus     > 0f) entity.RemoveMoveSpeedMult(def.moveSpeedBonus);
            if (def.maxHPBonus         > 0f) entity.RemoveMaxHPBonus(def.maxHPBonus);
            if (def.atpProductionBonus > 0f) entity.RemoveATPMult(def.atpProductionBonus);
            if (def.damageReduction    > 0f) entity.RemoveDamageReduction(def.damageReduction);
            if (def.hpRegenBonus       > 0f) entity.RemoveHPRegen(def.hpRegenBonus);
        }

        private ArchetypeDefinition FindDefinition(ArchetypeType type)
        {
            if (definitions == null) return null;
            foreach (var def in definitions)
                if (def != null && def.archetypeType == type) return def;
            return null;
        }

        #endregion
    }
}
