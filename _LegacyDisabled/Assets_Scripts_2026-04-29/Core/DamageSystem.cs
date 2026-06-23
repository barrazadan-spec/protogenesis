using UnityEngine;
using Protogenesis.Slots;

namespace Protogenesis.Core
{
    /// <summary>
    /// DamageSystem — Daño entre facciones + combatScore (GDD v5.2 §9, §19.1 prioridad 4 y 6).
    ///
    /// Regla de facciones:
    ///   · Solo se aplica daño entre entidades de DISTINTA facción.
    ///   · FactionId 0 = NPC neutral — puede recibir daño de cualquiera.
    ///   · Misma facción: nunca se dañan entre sí.
    ///
    /// combatScore (IA): (ATP*0.5) + (HP*0.3) - (entropy*0.7) - (heat*0.4)
    ///   · Evaluado cada 0.6s por los FSMs de células IA.
    ///   · Si el combatScore del objetivo es > propio * 1.5: Flee.
    ///   · Si el combatScore del objetivo es < propio * 0.7: Attack.
    ///
    /// CanReach: ratio tamaño ≥ 0.1f. Sin excepciones salvo viral y ambiental.
    /// Anti-stall: cap duro x2.5 sobre el tamaño base.
    /// </summary>
    public static class DamageSystem
    {
        // ── Constantes GDD v5.2 §20 ──────────────────────────────────────────────
        public const float CombatScoreInterval = 0.6f;
        public const float CanReachMinRatio    = 0.1f;
        public const float AntiStallCap        = 2.5f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Aplicar daño

        /// <summary>
        /// Intenta aplicar daño de <paramref name="attackerFaction"/> a un IDamageable.
        /// Retorna el daño efectivamente aplicado (0 si la facción no lo permite).
        /// </summary>
        public static float ApplyDamage(
            int     attackerFaction,
            IDamageable target,
            int     targetFaction,
            float   rawDamage,
            DamageType type = DamageType.Physical)
        {
            // Misma facción → no hay daño
            if (attackerFaction != 0 && attackerFaction == targetFaction)
                return 0f;

            float effective = CalculateEffectiveDamage(rawDamage, type);
            target.TakeDamage(effective);
            return effective;
        }

        /// <summary>
        /// Versión que toma directamente el GameObject objetivo e infiere su facción.
        /// Busca EnemyBase, ColonyUnit o CAP en el GameObject.
        /// </summary>
        public static float ApplyDamageToGameObject(
            int     attackerFaction,
            GameObject targetGO,
            float   rawDamage,
            DamageType type = DamageType.Physical)
        {
            if (targetGO == null) return 0f;

            int targetFaction = GetFaction(targetGO);
            var damageable    = targetGO.GetComponent<IDamageable>();
            if (damageable == null) return 0f;

            return ApplyDamage(attackerFaction, damageable, targetFaction, rawDamage, type);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region combatScore

        /// <summary>
        /// Evalúa el combatScore de una entidad según el GDD v5.2 §9.
        /// Fórmula: (ATP*0.5) + (HP*0.3) - (entropy*0.7) - (heat*0.4)
        /// </summary>
        public static float EvaluateCombatScore(float atp, float hp, float entropy, float heat)
            => (atp * 0.5f) + (hp * 0.3f) - (entropy * 0.7f) - (heat * 0.4f);

        /// <summary>
        /// Calcula el combatScore del jugador leyendo los sistemas activos.
        /// </summary>
        public static float PlayerCombatScore()
        {
            var rm   = ResourceManager.Instance;
            // TODO: Primordia — var cap  = Player.CAP.Instance;
            object cap = null; // Primordia migration stub
            var inst = InstabilitySystem.Instance;
            var heat = MetabolicHeatSystem.Instance;

            float atp     = rm   != null ? rm.GetResource(ResourceType.ATP) : 0f;
            float hp      = 0f; // Primordia stub: cap is always null during migration
            float entropy = inst != null ? inst.Instability                  : 0f;
            float heatVal = heat != null ? heat.HeatLevel                   : 0f;

            return EvaluateCombatScore(atp, hp, entropy, heatVal);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region CanReach

        /// <summary>
        /// Regla de interacción por tamaño relativo.
        /// CanReach: ratio ≥ 0.1f. Sin excepciones salvo viral y ambiental.
        /// </summary>
        public static bool CanReach(float attackerSize, float defenderSize)
            => defenderSize > 0f && (attackerSize / defenderSize) >= CanReachMinRatio;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers privados

        private static float CalculateEffectiveDamage(float raw, DamageType type)
        {
            float reduction = 0f;
            var entity = CellEntity.Instance;

            if (entity != null)
            {
                switch (type)
                {
                    case DamageType.Physical:
                        reduction = entity.DamageReduction;
                        break;
                    case DamageType.Chemical:
                        // Las bacterias con PeptidoglycanWall reducen daño químico
                        // Por ahora usa la misma reducción base
                        reduction = entity.DamageReduction * 0.5f;
                        break;
                    case DamageType.Environmental:
                    case DamageType.Viral:
                        // Sin reducción — bypass de defensas normales
                        reduction = 0f;
                        break;
                }
            }

            return Mathf.Max(0f, raw * (1f - reduction));
        }

        private static int GetFaction(GameObject go)
        {
            var enemy  = go.GetComponent<Enemies.EnemyBase>();
            if (enemy  != null) return enemy.FactionId;

            var colony = go.GetComponent<Units.ColonyUnit>();
            if (colony != null) return colony.FactionId;

            // TODO: Primordia — var cap    = go.GetComponent<Player.CAP>();
            object cap = null; // Primordia migration stub
            if (cap    != null) return CellEntity.Instance?.FactionId ?? 1;

            return 0; // NPC neutral por defecto
        }

        #endregion
    }

    // ─────────────────────────────────────────────────────────────────────────────
    /// <summary>Tipos de daño según el GDD v5.2.</summary>
    public enum DamageType
    {
        /// <summary>Daño físico (mordida, colisión, Flagelo). Reducido por armadura.</summary>
        Physical,
        /// <summary>Toxinas, H2S, ácidos. Reducido por PeptidoglycanWall.</summary>
        Chemical,
        /// <summary>Radiación UV, anoxia, temperatura extrema. Sin reducción.</summary>
        Environmental,
        /// <summary>Bacteriófago, infección viral. Sin reducción. Ignora tamaño.</summary>
        Viral
    }
}
