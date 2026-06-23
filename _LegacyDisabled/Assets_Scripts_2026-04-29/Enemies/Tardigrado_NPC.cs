using System.Collections;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Enemies
{
    /// <summary>
    /// Tardígrado — Megadepredador ambiental (GDD v4.6 §Fase 4).
    ///
    /// CANON BIOLÓGICO: Ramazzottius varieornatus mide ~500 µm y es el animal
    /// más resistente conocido. Sobrevive vacío espacial, radiación extrema y
    /// desecación total mediante criptobiosis (estado anhidrobiosis).
    ///
    /// A escala del juego, el Tardígrado es un coloso: la CAP en fase bacteria
    /// (WorldSize 0.005) es 100.000× más pequeña — el Tardígrado aparece como
    /// una pared de carne que cruza la pantalla lentamente.
    ///
    /// FSM:
    ///   Wandering  — movimiento aleatorio lento, consume microorganismos de paso
    ///   Feeding    — TriggerEnter2D: 25 dmg/s Perforante, ignora 95% resistencia
    ///   Cryptobiosis — Special: toma >50 dmg en 2s → inmune 8s + vulnerable 3s
    ///
    /// Stats GDD v4.6:
    ///   HP: 1500 | WorldSize: 500 µm | Speed: 0.002 u/s | Daño: 25/s (Perforante)
    ///   Recompensa: +50 Biomasa +20 GenomicPoints al morir
    /// </summary>
    public class Tardigrado_NPC : EnemyBase
    {
        // ── Tamaño de mundo (GDD v4.6): 500 µm ──────────────────────────────────
        public override float WorldSize            => 500f;
        public override float PerceptionMultiplier => 0.004f;  // radius = 2.0 unidades

        [Header("Tardígrado")]
        [SerializeField] private float piercingDamagePerSecond  = 25f;
        [SerializeField] private float piercingResistanceBypass = 0.95f;  // ignora 95% resistencias
        [SerializeField] private float cryptobiosisImmuneDuration = 8f;
        [SerializeField] private float cryptobiosisVulnerableDuration = 3f;
        [SerializeField] private float cryptobioisTriggerDamage = 50f;
        [SerializeField] private float cryptobioisTriggerWindow = 2f;
        [SerializeField] private float wanderChangeInterval     = 4f;
        [SerializeField] private float biomassDrop              = 50f;
        [SerializeField] private float genomicPointsDrop        = 20f;

        private enum TardiState { Wandering, Feeding, Cryptobiosis }
        private TardiState _tardiState = TardiState.Wandering;

        // Cryptobiosis tracking
        private float _recentDamage      = 0f;
        private float _recentDamageTimer = 0f;
        private bool  _cryptobiosisActive = false;
        private bool  _isCryptobiosisVulnerable = false;

        // Wander
        private Vector2 _wanderDir      = Vector2.right;
        private float   _wanderTimer    = 0f;

        // Feeding target (from trigger)
        private IDamageable _feedTarget = null;

        // ─────────────────────────────────────────────────────────────────────────
        #region Awake

        protected override void Awake()
        {
            base.Awake();
            enemyName       = "Tardígrado";
            maxHP           = 1500f;
            moveSpeed       = 0.002f;
            damage          = 0f;         // daño se aplica por contacto, no por ataque
            fleeHPThreshold = 0f;         // nunca huye
            attackRange     = WorldSize;  // contacto directo
            attackCooldown  = 0f;
            CurrentHP       = maxHP;

            // Dirección inicial aleatoria
            float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            _wanderDir  = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Update override

        protected override void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            if (!IsAlive) return;

            TickCryptobiosisWindow();

            switch (_tardiState)
            {
                case TardiState.Wandering:
                    UpdateWander();
                    break;

                case TardiState.Feeding:
                    ApplyFeedingDamage();
                    break;

                case TardiState.Cryptobiosis:
                    // Controlado por coroutine
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Wandering

        private void UpdateWander()
        {
            if (_rb == null) return;

            _wanderTimer += Time.deltaTime;
            if (_wanderTimer >= wanderChangeInterval)
            {
                _wanderTimer = 0f;
                float angle = Random.Range(-60f, 60f) * Mathf.Deg2Rad;
                float cos   = Mathf.Cos(angle), sin = Mathf.Sin(angle);
                _wanderDir  = new Vector2(
                    _wanderDir.x * cos - _wanderDir.y * sin,
                    _wanderDir.x * sin + _wanderDir.y * cos
                ).normalized;
            }

            _rb.MovePosition(_rb.position + _wanderDir * moveSpeed * Time.deltaTime);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Feeding (contacto)

        private void ApplyFeedingDamage()
        {
            if (_feedTarget == null)
            {
                _tardiState = TardiState.Wandering;
                return;
            }

            // Daño perforante: se aplica directamente ignorando el 95% de resistencias
            float rawDmg     = piercingDamagePerSecond * Time.deltaTime;
            float piercedDmg = rawDmg / (1f - piercingResistanceBypass);  // fuerza el daño neto
            _feedTarget.TakeDamage(piercedDmg);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (_cryptobiosisActive || _tardiState == TardiState.Cryptobiosis) return;
            if (!IsAlive) return;

            var damageable = other.GetComponent<IDamageable>();
            if (damageable == null) return;

            // Solo ataca entidades significativamente más pequeñas (CanPhagocytose)
            var enemy = other.GetComponent<EnemyBase>();
            float targetSize = enemy != null ? enemy.WorldSize : 0.005f;  // default bacteria
            if (!CanPhagocytose(WorldSize, targetSize)) return;

            _feedTarget = damageable;
            _tardiState = TardiState.Feeding;
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.GetComponent<IDamageable>() == _feedTarget)
            {
                _feedTarget = null;
                if (_tardiState == TardiState.Feeding)
                    _tardiState = TardiState.Wandering;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Criptobiosis

        private void TickCryptobiosisWindow()
        {
            if (_cryptobiosisActive) return;

            _recentDamageTimer -= Time.deltaTime;
            if (_recentDamageTimer < 0f)
            {
                _recentDamage      = 0f;
                _recentDamageTimer = 0f;
            }
        }

        public override void TakeDamage(float amount)
        {
            if (!IsAlive) return;

            // Durante criptobiosis: inmune (fase immune) o recibe x2 daño (fase vulnerable)
            if (_cryptobiosisActive)
            {
                if (!_isCryptobiosisVulnerable) return;   // inmune
                amount *= 2f;                              // vulnerable: daño doble
            }
            else
            {
                _recentDamage      += amount;
                _recentDamageTimer  = cryptobioisTriggerWindow;

                if (_recentDamage >= cryptobioisTriggerDamage)
                    StartCoroutine(CryptobiosisRoutine());
            }

            base.TakeDamage(amount);
        }

        private IEnumerator CryptobiosisRoutine()
        {
            _cryptobiosisActive = true;
            _isCryptobiosisVulnerable = false;
            _recentDamage  = 0f;
            _tardiState    = TardiState.Cryptobiosis;
            _feedTarget    = null;

            Debug.Log("[Tardígrado] ¡CRIPTOBIOSIS activada! Inmune durante 8s.");

            // Cambio visual: encogido / tonalidad azul-gris
            if (_sr != null) _sr.color = new Color(0.5f, 0.65f, 0.85f, 0.9f);

            yield return new WaitForSeconds(cryptobiosisImmuneDuration);

            // Fase vulnerable
            _isCryptobiosisVulnerable = true;
            if (_sr != null) _sr.color = new Color(1f, 0.7f, 0.2f, 0.9f);
            Debug.Log("[Tardígrado] Criptobiosis: fase VULNERABLE (3s, daño ×2).");

            yield return new WaitForSeconds(cryptobiosisVulnerableDuration);

            // Recuperación
            _cryptobiosisActive       = false;
            _isCryptobiosisVulnerable = false;
            if (_sr != null) _sr.color = Color.white;
            _tardiState = TardiState.Wandering;
            Debug.Log("[Tardígrado] Criptobiosis terminada — vuelve a moverse.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Percepción — override para gigante

        protected override void LayerPerception()
        {
            // El Tardígrado no "percibe" ni busca objetivos activamente;
            // el contacto físico (trigger) activa Feeding.
            // LayerPerception no hace nada — el wander es autónomo.
        }

        protected override void LayerAction()
        {
            // Gestionado en Update() según TardiState.
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Muerte

        protected override void OnDeathEffects()
        {
            var rm = ResourceManager.Instance;
            if (rm != null)
            {
                rm.AddResource(ResourceType.Biomass,       biomassDrop);
                rm.AddResource(ResourceType.GenomicPoints, genomicPointsDrop);
                Debug.Log($"[Tardígrado] Muerto — +{biomassDrop} Biomasa, +{genomicPointsDrop} GenomicPoints.");
            }
        }

        #endregion
    }
}
