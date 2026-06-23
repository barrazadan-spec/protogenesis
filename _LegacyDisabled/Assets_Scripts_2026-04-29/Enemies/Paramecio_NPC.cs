using System.Collections;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Enemies
{
    /// <summary>
    /// Paramecio — NPC ciliado de dificultad media (GDD v4.6 §Fase 3).
    ///
    /// CANON BIOLÓGICO: Paramecium caudatum es un protista ciliado que se
    /// desplaza usando miles de cilios sincronizados. Es depredador de bacterias
    /// y presa del Didinium.
    ///
    /// Comportamiento específico:
    ///   · Movimiento en zigzag — los cilios producen aceleración irregular
    ///   · Ataque por "trichocyst discharge" — descarga de tricocistos que
    ///     paralizan al objetivo 1.5 seg (stun)
    ///   · Huye activamente del Didinium (LayerEnvironment detecta amenaza)
    ///   · Al morir suelta Biomasa (nutrientes para el ecosistema)
    ///   · HP < 40%: libera vesículas defensivas (+2 Instabilidad al atacante)
    ///
    /// Stats GDD v4.6:
    ///   HP: 120 | Velocidad: 3.5 | Daño: 8 | Cooldown ataque: 2s
    ///   Recompensa: +15 Biomasa al morir
    /// </summary>
    public class Paramecio_NPC : EnemyBase
    {
        // ── Tamaño de mundo (GDD v4.6): 150 µm ──────────────────────────────────
        public override float WorldSize            => 0.15f;
        public override float PerceptionMultiplier => 8f;    // radio = 1.2 unidades

        [Header("Paramecio")]
        [SerializeField] private float stunDuration        = 1.5f;
        [SerializeField] private float zigzagAmplitude     = 1.2f;
        [SerializeField] private float zigzagFrequency     = 2.5f;
        [SerializeField] private float biomassDrop         = 15f;
        [SerializeField] private float defensiveInstability= 2f;  // instabilidad al atacante cuando HP < 40%

        private float _zigzagTimer = 0f;
        private bool  _stunApplied = false;

        // ─────────────────────────────────────────────────────────────────────────
        #region Awake

        protected override void Awake()
        {
            base.Awake();
            enemyName       = "Paramecio";
            maxHP           = 120f;
            moveSpeed       = 3.5f;
            damage          = 8f;
            attackRange     = 1.2f;
            attackCooldown  = 2.0f;
            fleeHPThreshold = 0.30f;  // Huye al 30% HP
            CurrentHP       = maxHP;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Capas FSM especializadas

        protected override void LayerEnvironment()
        {
            // Detecta Didinium cercano (depredador natural) y fuerza huida
            var didiinia = FindObjectsByType<Didinium_NPC>(FindObjectsSortMode.None);
            foreach (var d in didiinia)
            {
                if (Vector2.Distance(transform.position, d.transform.position) < 3.5f)
                {
                    _target = d.transform; // huye del Didinium
                    // La capa de supervivencia detectará fleeHPThreshold,
                    // pero aquí forzamos huida independientemente del HP
                    if (_state != EnemyState.Fleeing && _state != EnemyState.Dead)
                    {
                        TransitionTo(EnemyState.Fleeing);
                    }
                    return;
                }
            }
        }

        protected override void LayerAction()
        {
            if (_state == EnemyState.Approaching && _target != null)
                MoveZigzag();
            else
                base.LayerAction();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Movimiento en zigzag

        private void MoveZigzag()
        {
            if (_rb == null || _target == null) return;

            _zigzagTimer += Time.deltaTime * zigzagFrequency;

            Vector2 toTarget = ((Vector2)_target.position - _rb.position).normalized;
            Vector2 perpendicular = new Vector2(-toTarget.y, toTarget.x);
            Vector2 zigzag = toTarget + perpendicular * (Mathf.Sin(_zigzagTimer) * zigzagAmplitude);

            _rb.MovePosition(_rb.position + zigzag.normalized * moveSpeed * Time.deltaTime);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Ataque — descarga de tricocistos

        protected override void OnAttackPerformed()
        {
            if (_stunApplied || _target == null) return;

            // Intenta aplicar stun al objetivo
            // TODO: Primordia — var cap = _target.GetComponent<Player.CAP>();
            object cap = null; // Primordia migration stub
            if (cap != null)
            {
                StartCoroutine(StunCAPRoutine());
                NotifyHitOnCAP();
            }
        }

        private IEnumerator StunCAPRoutine()
        {
            _stunApplied = true;
            // TODO: Primordia — Debug.Log("[Paramecio] ¡Descarga de tricocistos! CAP paralizada.");

            // El stun se simula bloqueando el movimiento vía pausa breve
            // En Fase 5 UI mostrará el efecto visual
            // TODO: Primordia — float original = Player.CAP.Instance != null
                // TODO: Primordia — ? Player.CAP.Instance.MaxHP : 0f;
            // No hay API de stun directa aún — placeholder para Fase 5

            yield return new WaitForSeconds(stunDuration);
            _stunApplied = false;
        }

        // Vesículas defensivas cuando HP bajo
        public override void TakeDamage(float amount)
        {
            if (HPRatio < 0.40f && !_stunApplied)
            {
                // Añade inestabilidad al atacante (representado en el CAP)
                InstabilitySystem.Instance?.AddInstability(defensiveInstability);
            }

            base.TakeDamage(amount);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Muerte — suelta Biomasa

        protected override void OnDeathEffects()
        {
            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                rm.AddResource(ResourceType.Biomass, biomassDrop);
                Debug.Log($"[Paramecio] Muerto — +{biomassDrop} Biomasa liberada.");
            }
        }

        #endregion
    }
}
