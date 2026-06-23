using System.Collections;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Enemies
{
    /// <summary>
    /// Didinium — Depredador especializado de Paramecio (GDD v4.6 §Fase 3).
    ///
    /// CANON BIOLÓGICO: Didinium nasutum es un depredador ciliopodo que caza
    /// exclusivamente Paramecium. Tiene dos cilios en bandas y un tubo de
    /// captura (probóscide) con el que engulle presas de su mismo tamaño.
    ///
    /// Comportamiento específico:
    ///   · PRIORIDAD: busca Paramecio activo antes que atacar al CAP
    ///   · Ataque "engullir" — si alcanza un Paramecio, lo devora en 2 seg
    ///     (el Paramecio muere instantáneamente, Didinium recupera 40 HP)
    ///   · Tras engullir, entra en estado Digesting (3 seg lento, invulnerable)
    ///   · Sin Paramecio disponible: ataca al CAP con daño reducido (-50%)
    ///   · Al morir: suelta GenomicPoints (material genético valioso)
    ///
    /// Stats GDD v4.6:
    ///   HP: 200 | Velocidad: 2.0 (4.0 al cazar) | Daño vs CAP: 6 | Cooldown: 3s
    ///   Recompensa: +8 GenomicPoints al morir
    /// </summary>
    public class Didinium_NPC : EnemyBase
    {
        // ── Tamaño de mundo (GDD v4.6): 250 µm ──────────────────────────────────
        public override float WorldSize            => 0.25f;
        public override float PerceptionMultiplier => 12f;   // radio = 3 unidades

        [Header("Didinium")]
        [SerializeField] private float huntSpeed          = 4.0f;   // velocidad al perseguir Paramecio
        [SerializeField] private float swallowRange       = 0.8f;   // rango para "engullir"
        [SerializeField] private float swallowHealAmount  = 40f;
        [SerializeField] private float digestDuration     = 3.0f;
        [SerializeField] private float genomicPointsDrop  = 8f;

        private enum DidiniumMode { Hunting, Digesting, Stalking }
        private DidiniumMode _mode = DidiniumMode.Stalking;

        private Paramecio_NPC _paramecioTarget = null;
        private bool          _isDigesting     = false;

        // ─────────────────────────────────────────────────────────────────────────
        #region Awake

        protected override void Awake()
        {
            base.Awake();
            enemyName       = "Didinium";
            maxHP           = 200f;
            moveSpeed       = 2.0f;
            // TODO: Primordia — damage          = 6f;     // daño reducido vs CAP
            attackRange     = 1.5f;
            attackCooldown  = 3.0f;
            fleeHPThreshold = 0.15f;  // Solo huye a HP muy bajo
            CurrentHP       = maxHP;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Percepción — prioriza Paramecio

        protected override void LayerPerception()
        {
            if (_isDigesting || _state == EnemyState.Dead) return;

            // Busca Paramecio como presa prioritaria
            _paramecioTarget = FindNearestParamecio();

            if (_paramecioTarget != null)
            {
                _mode   = DidiniumMode.Hunting;
                _target = _paramecioTarget.transform;
                moveSpeed = huntSpeed;

                if (_state != EnemyState.Approaching && _state != EnemyState.Attacking)
                    TransitionTo(EnemyState.Approaching);
            }
            else
            {
                // Sin presas — busca CAP como fallback con velocidad reducida
                _mode     = DidiniumMode.Stalking;
                moveSpeed = 2.0f;
                base.LayerPerception();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Acción — engullir

        protected override void LayerAction()
        {
            if (_isDigesting) return;

            if (_mode == DidiniumMode.Hunting && _paramecioTarget != null)
            {
                float dist = Vector2.Distance(transform.position, _paramecioTarget.transform.position);

                if (dist <= swallowRange)
                {
                    StartCoroutine(SwallowRoutine(_paramecioTarget));
                    return;
                }

                MoveTowards(_paramecioTarget.transform.position);
            }
            else
            {
                base.LayerAction();
            }
        }

        private IEnumerator SwallowRoutine(Paramecio_NPC prey)
        {
            _isDigesting = true;
            TransitionTo(EnemyState.Special);

            Debug.Log("[Didinium] ¡Engullendo Paramecio!");

            // El Paramecio muere instantáneamente
            prey.TakeDamage(prey.CurrentHP + 1f);

            // Recuperar HP
            CurrentHP = Mathf.Min(maxHP, CurrentHP + swallowHealAmount);

            // Digestión: 3 seg lento e invulnerable
            float baseSpeed = moveSpeed;
            moveSpeed = 0.5f;

            yield return new WaitForSeconds(digestDuration);

            moveSpeed    = baseSpeed;
            _isDigesting = false;
            _paramecioTarget = null;
            TransitionTo(EnemyState.Seeking);

            Debug.Log("[Didinium] Digestión completada — listo para cazar.");
        }

        protected override void OnAttackPerformed()
        {
            // Al atacar al CAP (modo Stalking), notifica presión
            if (_mode == DidiniumMode.Stalking)
                NotifyHitOnCAP();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Muerte — suelta GenomicPoints

        protected override void OnDeathEffects()
        {
            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                rm.AddResource(ResourceType.GenomicPoints, genomicPointsDrop);
                Debug.Log($"[Didinium] Muerto — +{genomicPointsDrop} GenomicPoints liberados.");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private Paramecio_NPC FindNearestParamecio()
        {
            var all = FindObjectsByType<Paramecio_NPC>(FindObjectsSortMode.None);
            Paramecio_NPC nearest = null;
            float minDist = float.MaxValue;

            foreach (var p in all)
            {
                if (!p.IsAlive) continue;
                float d = Vector2.Distance(transform.position, p.transform.position);
                if (d < minDist) { minDist = d; nearest = p; }
            }

            return nearest;
        }

        #endregion
    }
}
