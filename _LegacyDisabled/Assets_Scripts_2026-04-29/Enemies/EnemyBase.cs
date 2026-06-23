using System.Collections;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Player;

namespace Protogenesis.Enemies
{
    /// <summary>
    /// Clase base para todos los enemigos del juego (GDD v4.6 §Fase 3).
    ///
    /// FSM de 7 estados:
    ///   Idle → Seeking → Approaching → Attacking → Special → Fleeing → Dead
    ///
    /// Arquitectura de 5 capas evaluadas cada frame:
    ///   1. Environment  — Comprueba zona, O2, temperatura (puede forzar Flee)
    ///   2. Perception   — Adquiere/actualiza target por prioridad
    ///   3. Decision     — Decide estado según HP, amenaza, nicho
    ///   4. Action       — Ejecuta movimiento o ataque
    ///   5. Survival     — Aplica huida si HP bajo umbral
    ///
    /// Integración con sistemas v4.6:
    ///   · BalanceSystem  — Notifica inicio/fin de combate; daño recibido escalado
    ///   · InstabilitySystem — Añade +1.5 inestabilidad al CAP por cada golpe recibido
    ///   · MetabolicHeatSystem — Añade +0.5 calor al recibir daño (choque térmico)
    ///
    /// Todos los enemigos llevan tag "Enemy".
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable
    {
        // ── Tamaño de mundo — GDD v4.6 (en unidades de mundo = micrometros aprox.) ──
        /// <summary>
        /// Tamaño real del organismo en unidades de mundo.
        /// Se aplica como transform.localScale en Awake y define radio de percepción.
        /// Subclases deben sobreescribir con el valor canónico del GDD v4.6.
        /// </summary>
        public virtual float WorldSize => 1f;

        /// <summary>Multiplicador: PerceptionRadius = WorldSize × PerceptionMultiplier.</summary>
        public virtual float PerceptionMultiplier => 10f;

        /// <summary>Radio de percepción en espacio de mundo (no depende del zoom).</summary>
        public float PerceptionRadius => WorldSize * PerceptionMultiplier;

        // WorldSize del jugador por defecto — actualizar cuando CAP exponga la propiedad
        protected const float PlayerDefaultWorldSize = 0.005f;

        // ── Reglas de interacción por tamaño relativo (GDD v4.6 §Fase 3) ──────────
        /// <summary>
        /// ¿Puede el depredador fagocitar a la presa?
        /// La presa debe ser ≤ 60% del tamaño del depredador.
        /// </summary>
        public static bool CanPhagocytose(float predatorSize, float preySize)
            => preySize <= predatorSize * 0.6f;

        /// <summary>
        /// ¿Puede el observador detectar al objetivo?
        /// El objetivo debe ser ≥ 10% del tamaño del observador.
        /// Si el objetivo es < 10%, es demasiado pequeño para detectarse (parece partícula).
        /// </summary>
        public static bool CanBeDetectedBy(float observerSize, float targetSize)
            => targetSize >= observerSize * 0.1f;

        /// <summary>
        /// ¿La entidad aparece como pared/fondo para el observador?
        /// Ocurre cuando la entidad es > 8× más grande que el observador.
        /// En ese caso, WorldSpaceRenderer debe renderizarla como textura de fondo,
        /// no como entidad con hitbox. (Ver WorldSpaceRenderer — pendiente de crear.)
        /// </summary>
        public static bool AppearsAsWall(float observerSize, float entitySize)
            => entitySize >= observerSize * 8f;

        // ── Facción (GDD v5.2 — MVP PvP) ─────────────────────────────────────────
        /// <summary>0 = NPC neutral  |  1+ = facción de jugador.</summary>
        public int FactionId { get; private set; } = 0;
        public void SetFaction(int id) => FactionId = id;

        // ── Stats ─────────────────────────────────────────────────────────────────
        [Header("Stats")]
        public float maxHP           = 50f;
        public float moveSpeed       = 2f;
        public float damage          = 8f;
        public float attackRange     = 1.0f;
        public float attackCooldown  = 1.5f;
        public float experienceValue = 10f;
        public string enemyName      = "Enemy";

        [Header("Supervivencia")]
        [Tooltip("Porcentaje de HP al que el enemigo intenta huir (0 = nunca huye).")]
        [SerializeField] protected float fleeHPThreshold = 0.20f;
        [Tooltip("Tiempo en segundos que huye antes de reintentar acercarse.")]
        [SerializeField] protected float fleeDuration    = 4f;

        // ── HP ────────────────────────────────────────────────────────────────────
        public float CurrentHP { get; protected set; }
        public float HPRatio   => maxHP > 0f ? CurrentHP / maxHP : 0f;
        public bool  IsAlive   => CurrentHP > 0f && _state != EnemyState.Dead;

        // ── FSM ───────────────────────────────────────────────────────────────────
        protected enum EnemyState
        {
            Idle,
            Seeking,
            Approaching,
            Attacking,
            Special,
            Fleeing,
            Dead
        }
        protected EnemyState _state = EnemyState.Idle;

        // ── Target ────────────────────────────────────────────────────────────────
        protected Transform _target;
        private   float     _attackTimer  = 0f;
        private   float     _fleeTimer    = 0f;
        private   bool      _inCombat     = false;

        // ── CombatScore (GDD v5.2 §9 — evaluado cada 0.6 s) ──────────────────────
        private float _combatScoreTimer  = 0f;
        private float _ownCombatScore    = 0f;
        private float _targetCombatScore = 0f;

        // ── Componentes ───────────────────────────────────────────────────────────
        protected Rigidbody2D    _rb;
        protected SpriteRenderer _sr;

        // ── Tipo de daño (para variación antigénica) ──────────────────────────────
        public string LastDamageSourceType { get; private set; } = "None";

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        protected virtual void Awake()
        {
            _rb  = GetComponent<Rigidbody2D>();
            _sr  = GetComponent<SpriteRenderer>();

            _rb.bodyType       = RigidbodyType2D.Kinematic;
            _rb.gravityScale   = 0f;
            _rb.freezeRotation = true;

            // Escalar el GO al tamaño de mundo canónico del GDD v4.6
            transform.localScale = Vector3.one * WorldSize;

            CurrentHP = maxHP;
            tag       = "Enemy";
        }

        protected virtual void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            if (_state == EnemyState.Dead) return;

            _attackTimer -= Time.deltaTime;

            // 5 capas evaluadas en orden cada frame
            LayerEnvironment();
            LayerPerception();
            LayerDecision();
            LayerAction();
            LayerSurvival();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Capa 1 — Entorno

        /// <summary>
        /// Evalúa si las condiciones ambientales fuerzan un comportamiento.
        /// Subclases pueden sobreescribir para nichos específicos.
        /// </summary>
        protected virtual void LayerEnvironment()
        {
            // Hostilidad de zona: si O2 es muy bajo, los patógenos aerobios se
            // debilitan. Aquí se puede añadir lógica de zona en Fase 3+.
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Capa 2 — Percepción

        protected virtual void LayerPerception()
        {
            // No busca nuevo target si está atacando, huyendo o muerto
            if (_state == EnemyState.Attacking ||
                _state == EnemyState.Fleeing   ||
                _state == EnemyState.Special   ||
                _state == EnemyState.Dead) return;

            // Refresca target si está en Seeking o el target actual se perdió
            if (_state == EnemyState.Seeking ||
                _target == null ||
                !_target.gameObject.activeInHierarchy)
            {
                _target = GetPriorityTarget();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Capa 3 — Decisión

        protected virtual void LayerDecision()
        {
            if (_state == EnemyState.Dead ||
                _state == EnemyState.Special) return;

            // Huida en curso: no interrumpir
            if (_state == EnemyState.Fleeing) return;

            // ── CombatScore — GDD v5.2 §9: evaluar cada 0.6s ────────────────────
            // Si el target supera 1.5× el propio score → Flee.
            // Si el propio score supera 1/0.7× el del target → forzar Approach/Attack.
            if (_target != null && _target.gameObject.activeInHierarchy)
            {
                _combatScoreTimer += Time.deltaTime;
                if (_combatScoreTimer >= DamageSystem.CombatScoreInterval)
                {
                    _combatScoreTimer    = 0f;
                    _ownCombatScore      = EvaluateOwnScore();
                    _targetCombatScore   = EvaluateTargetScore();

                    if (_targetCombatScore > _ownCombatScore * 1.5f)
                    {
                        StartFleeing();
                        return;
                    }
                }
            }

            // Transición de estado según target y distancia
            if (_state == EnemyState.Idle)
            {
                TransitionTo(EnemyState.Seeking);
                return;
            }

            if (_state == EnemyState.Seeking)
            {
                if (_target != null)
                    TransitionTo(EnemyState.Approaching);
                return;
            }

            if (_target == null || !_target.gameObject.activeInHierarchy)
            {
                TransitionTo(EnemyState.Seeking);
                return;
            }

            float dist = Vector2.Distance(transform.position, _target.position);

            if (_state == EnemyState.Approaching && dist <= attackRange)
            {
                TransitionTo(EnemyState.Attacking);
                NotifyEnterCombat();
            }
            else if (_state == EnemyState.Attacking && dist > attackRange * 1.3f)
            {
                TransitionTo(EnemyState.Approaching);
            }
        }

        // ── Helpers de combatScore ────────────────────────────────────────────────
        private float EvaluateOwnScore()
        {
            float virtualATP = damage * 5f;
            float entropy    = (1f - HPRatio) * 30f;   // herido = alta entropía
            return DamageSystem.EvaluateCombatScore(virtualATP, CurrentHP, entropy, 0f);
        }

        private float EvaluateTargetScore()
        {
            if (_target == null) return 0f;

            // Target es el jugador (CAP)
            // TODO: Primordia — if (CAP.Instance != null && _target == CAP.Instance.transform)
            {
                var rm  = ResourceManager.Instance;
                float atp = rm != null ? rm.GetResource(ResourceType.ATP) : 60f;
                float ent = InstabilitySystem.Instance != null ? InstabilitySystem.Instance.Instability : 0f;
                float ht  = MetabolicHeatSystem.Instance != null ? MetabolicHeatSystem.Instance.HeatLevel : 0f;
                // TODO: Primordia — return DamageSystem.EvaluateCombatScore(atp, CAP.Instance.CurrentHP, ent, ht);
            }

            // Target es otro enemigo (e.g., conflicto inter-NPC futuro)
            var enemy = _target.GetComponent<EnemyBase>();
            if (enemy != null)
                return DamageSystem.EvaluateCombatScore(enemy.damage * 5f, enemy.CurrentHP,
                       (1f - enemy.HPRatio) * 30f, 0f);

            return 25f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Capa 4 — Acción

        protected virtual void LayerAction()
        {
            switch (_state)
            {
                case EnemyState.Approaching:
                    if (_target != null)
                        MoveTowards(_target.position);
                    break;

                case EnemyState.Attacking:
                    TryAttack();
                    break;

                case EnemyState.Fleeing:
                    ExecuteFlee();
                    break;

                case EnemyState.Special:
                    ExecuteSpecialBehavior();
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Capa 5 — Supervivencia

        protected virtual void LayerSurvival()
        {
            if (_state == EnemyState.Dead || _state == EnemyState.Fleeing) return;
            if (fleeHPThreshold <= 0f) return;

            if (HPRatio <= fleeHPThreshold)
                StartFleeing();
        }

        protected void StartFleeing()
        {
            if (_state == EnemyState.Dead) return;
            _fleeTimer = fleeDuration;
            NotifyExitCombat();
            TransitionTo(EnemyState.Fleeing);
            if (_sr != null)
                _sr.color = new Color(1f, 0.5f, 0.5f); // tinte rojo al huir
        }

        private void ExecuteFlee()
        {
            _fleeTimer -= Time.deltaTime;

            // Moverse en dirección opuesta al target
            Vector2 fleeDir = _target != null
                ? ((Vector2)transform.position - (Vector2)_target.position).normalized
                : Random.insideUnitCircle.normalized;

            if (_rb != null)
                _rb.MovePosition(_rb.position + fleeDir * moveSpeed * 1.2f * Time.deltaTime);

            if (_fleeTimer <= 0f)
            {
                if (_sr != null) _sr.color = Color.white;
                TransitionTo(EnemyState.Seeking);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region FSM helpers

        protected void TransitionTo(EnemyState newState)
        {
            _state = newState;
            OnStateEnter(newState);
        }

        protected virtual void OnStateEnter(EnemyState state) { }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Combate

        private void TryAttack()
        {
            if (_attackTimer > 0f) return;
            _attackTimer = attackCooldown;

            var damageable = _target?.GetComponent<IDamageable>();
            if (damageable == null) return;

            // BalanceSystem escala el daño saliente por Anti-Stall
            float finalDamage = damage;
            if (BalanceSystem.Instance != null)
                finalDamage *= BalanceSystem.Instance.StallDamageMultiplier;

            damageable.TakeDamage(finalDamage);
            OnAttackPerformed();
        }

        protected virtual void OnAttackPerformed() { }

        // IDamageable
        public virtual void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f) return;

            CurrentHP = Mathf.Max(0f, CurrentHP - amount);

            if (CurrentHP <= 0f)
                OnDeath();
        }

        public virtual void TakeDamage(float amount, string sourceType)
        {
            LastDamageSourceType = sourceType;
            TakeDamage(amount);
        }

        protected virtual void OnDeath()
        {
            if (_state == EnemyState.Dead) return;
            _state = EnemyState.Dead;

            NotifyExitCombat();
            EventBus.TriggerUnitDied(gameObject, enemyName);

            OnDeathEffects();
            gameObject.SetActive(false);
        }

        protected virtual void OnDeathEffects() { }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Integración con sistemas v4.6

        private void NotifyEnterCombat()
        {
            if (_inCombat) return;
            _inCombat = true;
            BalanceSystem.Instance?.NotifyCombatStart();
        }

        private void NotifyExitCombat()
        {
            if (!_inCombat) return;
            _inCombat = false;
            BalanceSystem.Instance?.NotifyCombatEnd();
        }

        /// <summary>
        /// Notifica a InstabilitySystem y MetabolicHeatSystem cuando el CAP recibe un golpe.
        /// Llamar desde subclases si el target es el CAP.
        /// </summary>
        protected void NotifyHitOnCAP()
        {
            InstabilitySystem.Instance?.AddInstability(1.5f);
            MetabolicHeatSystem.Instance?.AddHeat(0.5f);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region IA — Target

        protected virtual Transform GetPriorityTarget()
        {
            float radius = PerceptionRadius;

            // TODO: Primordia — detección de jugador usaba CAP (deprecado).
            // En Primordia los enemigos buscarán PlayerCellExterior (Fase 3.2 + 5.5).

            // Detectar membranas dentro del radio de percepción
            GameObject[] membranes = GameObject.FindGameObjectsWithTag("Membrane");
            Transform nearestMembrane = FindNearestInRadius(membranes, radius);
            if (nearestMembrane != null) return nearestMembrane;

            return null;
        }

        protected Transform FindNearest(GameObject[] candidates)
        {
            Transform best = null;
            float minDist  = float.MaxValue;
            foreach (var go in candidates)
            {
                if (go == null || !go.activeInHierarchy) continue;
                float d = Vector2.Distance(transform.position, go.transform.position);
                if (d < minDist) { minDist = d; best = go.transform; }
            }
            return best;
        }

        /// <summary>Encuentra el transform más cercano dentro de radio, filtrando por CanBeDetectedBy.</summary>
        protected Transform FindNearestInRadius(GameObject[] candidates, float radius)
        {
            Transform best = null;
            float minDist  = float.MaxValue;
            foreach (var go in candidates)
            {
                if (go == null || !go.activeInHierarchy) continue;
                float d = Vector2.Distance(transform.position, go.transform.position);
                if (d > radius) continue;

                // Solo detectar si el tamaño del objetivo es ≥ 10% del propio tamaño
                float targetSize = go.transform.localScale.x; // usa la escala como proxy de WorldSize
                if (!CanBeDetectedBy(WorldSize, targetSize)) continue;

                if (d < minDist) { minDist = d; best = go.transform; }
            }
            return best;
        }

        protected void MoveTowards(Vector3 targetPos)
        {
            if (_rb == null) return;
            Vector2 dir = ((Vector2)targetPos - _rb.position).normalized;
            _rb.MovePosition(_rb.position + dir * moveSpeed * Time.deltaTime);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Comportamiento especial

        protected virtual void ExecuteSpecialBehavior() { }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Reset (para pool futuro)

        public virtual void ResetEnemy()
        {
            CurrentHP          = maxHP;
            _state             = EnemyState.Idle;
            _target            = null;
            _attackTimer       = 0f;
            _fleeTimer         = 0f;
            _inCombat          = false;
            _combatScoreTimer  = 0f;
            _ownCombatScore    = 0f;
            _targetCombatScore = 0f;
            gameObject.SetActive(true);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Redireccionamiento de target (PvP / CoopNetwork)

        public void SetNewTarget(Vector2 worldPosition)
        {
            var pivot = new GameObject("_EnemyRedirectPivot");
            pivot.transform.position = worldPosition;

            _target = pivot.transform;
            TransitionTo(EnemyState.Approaching);

            Destroy(pivot, 30f);
            Debug.Log($"[{enemyName}] Target redirigido a {worldPosition}");
        }

        #endregion
    }
}
