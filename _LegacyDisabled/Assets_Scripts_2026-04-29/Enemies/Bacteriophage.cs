using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Player;

namespace Protogenesis.Enemies
{
    /// <summary>
    /// Bacteriófago — Enemigo principal de las primeras oleadas.
    ///
    /// FSM específico:
    ///   SeekingMembrane → Approaching → Anchoring (5s, vulnerable)
    ///   → Injecting → Retreating → [repite]
    ///
    /// Tras inyectar, infecta el Ribosoma más cercano: produce enemigos 30 seg.
    ///
    /// Al morir ANTES de inyectar: explosión ácida (AcidExplosion) radio 2u, 20 daño.
    ///
    /// Variantes configurables vía ScriptableObject (BacteriophageStats):
    ///   - Normal:   AnchorTime=5s, HP=100%
    ///   - Rápido:   AnchorTime=2s, HP=60%
    ///   - Silencioso: invisible hasta llegar a membrana
    ///
    /// VARIACIÓN ANTIGÉNICA (oleada 8+):
    ///   Al recibir daño, el fago registra el tipo de fuente. Tras acumular
    ///   3 golpes del mismo tipo en 10 seg, desarrolla resistencia temporal
    ///   (−70% daño de ese tipo por 20 seg). Obliga al jugador a diversificar.
    /// </summary>
    public class Bacteriophage : EnemyBase
    {
        // ── Configuración (sobreescribible desde BacteriophageStats SO) ───────────
        [Header("Bacteriófago")]
        [SerializeField] private float anchorTime          = 5f;
        [SerializeField] private bool  isSilent            = false;
        [SerializeField] private bool  antigenicVariation  = false;

        [Header("Efectos")]
        [SerializeField] private GameObject acidExplosionPrefab;
        [SerializeField] private GameObject injectionFXPrefab;

        // ── Tamaño de mundo (GDD v4.6): 0.1 µm ──────────────────────────────────
        public override float WorldSize            => 0.0001f;
        public override float PerceptionMultiplier => 10f;   // radio = 0.001 unidades

        // ── Estado del ciclo ──────────────────────────────────────────────────────
        private enum PhageState
        {
            SeekingMembrane,
            Approaching,
            Anchoring,
            Injecting,
            Retreating
        }
        private PhageState _phageState = PhageState.SeekingMembrane;

        private Transform _membraneTarget;
        private bool      _injected       = false;
        private Coroutine _anchorCoroutine;

        // ── Variación Antigénica ──────────────────────────────────────────────────
        private readonly Dictionary<string, int>   _damageHits     = new Dictionary<string, int>();
        private readonly Dictionary<string, float> _hitTimers      = new Dictionary<string, float>();
        private readonly HashSet<string>           _resistances    = new HashSet<string>();
        private const int   ResistanceHitThreshold  = 3;
        private const float ResistanceWindow        = 10f;
        private const float ResistanceDuration      = 20f;
        private const float ResistanceReduction     = 0.70f;   // −70% daño

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            enemyName        = "Bacteriophage";
            maxHP            = 90f;   // GDD v4.6
            moveSpeed        = 3.0f;
            damage           = 12f;
            fleeHPThreshold  = 0f;    // Los fagos no huyen — son suicidas
            CurrentHP        = maxHP;
        }

        private void Start()
        {
            if (isSilent && _sr != null)
                _sr.color = new Color(1f, 1f, 1f, 0.15f);   // Semi-invisible
        }

        protected override void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            if (!IsAlive) return;

            TickAntigenicVariation();
            UpdatePhageFSM();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region FSM del fago

        private void UpdatePhageFSM()
        {
            switch (_phageState)
            {
                case PhageState.SeekingMembrane:
                    SeekMembrane();
                    break;

                case PhageState.Approaching:
                    if (_membraneTarget == null)
                    { _phageState = PhageState.SeekingMembrane; break; }

                    MoveTowards(_membraneTarget.position);

                    if (Vector2.Distance(transform.position, _membraneTarget.position) <= 0.5f)
                    {
                        _phageState      = PhageState.Anchoring;
                        _anchorCoroutine = StartCoroutine(AnchorRoutine());
                    }
                    break;

                case PhageState.Anchoring:
                case PhageState.Injecting:
                case PhageState.Retreating:
                    // Controlado por coroutines
                    break;
            }
        }

        private void SeekMembrane()
        {
            GameObject[] membranes = GameObject.FindGameObjectsWithTag("Membrane");
            _membraneTarget = FindNearest(membranes);

            // Fallback: si no hay membrana, apuntar directamente a la CAP
            // TODO: Primordia — if (_membraneTarget == null && Player.CAP.Instance != null)
                // TODO: Primordia — _membraneTarget = Player.CAP.Instance.transform;

            if (_membraneTarget != null)
            {
                _phageState = PhageState.Approaching;

                // Al encontrar objetivo, revelar el fago silencioso
                if (isSilent && _sr != null)
                    _sr.color = new Color(1f, 1f, 1f, 1f);
            }
        }

        private IEnumerator AnchorRoutine()
        {
            Debug.Log($"[Bacteriophage] Anclándose en membrana... ({anchorTime}s vulnerable)");
            float elapsed = 0f;

            while (elapsed < anchorTime)
            {
                if (!IsAlive)
                {
                    OnDeathBeforeInject();
                    yield break;
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            _phageState = PhageState.Injecting;
            StartCoroutine(InjectRoutine());
        }

        private IEnumerator InjectRoutine()
        {
            if (injectionFXPrefab != null)
                Instantiate(injectionFXPrefab, transform.position, Quaternion.identity);

            // Buscar el Ribosoma más cercano e infectarlo
            var ribosomes = FindObjectsByType<Organelles.Ribosoma>(FindObjectsSortMode.None);
            Organelles.Ribosoma target = null;
            float minDist = float.MaxValue;

            foreach (var r in ribosomes)
            {
                float d = Vector2.Distance(transform.position, r.transform.position);
                if (d < minDist) { minDist = d; target = r; }
            }

            if (target != null)
            {
                target.GetInfected();
                _injected = true;
                Debug.Log($"[Bacteriophage] ¡Inyección exitosa en {target.name}!");

                if (GameManager.Instance != null)
                    GameManager.Instance.BacteriophagesKilled++;

                // Inyección exitosa → +4 inestabilidad, +1.5 calor en la CAP
                NotifyHitOnCAP();
                Core.InstabilitySystem.Instance?.AddInstability(4f);
            }

            yield return new WaitForSeconds(0.5f);

            _phageState = PhageState.Retreating;
            StartCoroutine(RetreatRoutine());
        }

        private IEnumerator RetreatRoutine()
        {
            // Alejarse de la membrana y buscar otro punto
            float elapsed = 0f;
            Vector2 retreatDir = ((Vector2)transform.position -
                                  (Vector2)(_membraneTarget != null
                                      ? _membraneTarget.position
                                      : transform.position)).normalized;

            while (elapsed < 2f)
            {
                _rb.MovePosition(_rb.position + retreatDir * moveSpeed * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Buscar nuevo objetivo de membrana
            _membraneTarget = null;
            _phageState     = PhageState.SeekingMembrane;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Muerte antes de inyectar

        private void OnDeathBeforeInject()
        {
            if (acidExplosionPrefab != null)
                Instantiate(acidExplosionPrefab, transform.position, Quaternion.identity);
            else
            {
                // Explosión ácida fallback
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 2f);
                foreach (var h in hits)
                {
                    if (!h.CompareTag("AllyUnit") && !h.CompareTag("AllyOrganelle")) continue;
                    h.GetComponent<IDamageable>()?.TakeDamage(20f);
                }
            }
        }

        protected override void OnDeathEffects()
        {
            if (!_injected)
                OnDeathBeforeInject();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Variación Antigénica

        /// <summary>
        /// Activa la Variación Antigénica en este fago.
        /// </summary>
        public void EnableAntigenicVariation() => antigenicVariation = true;

        public override void TakeDamage(float amount)
            => TakeDamage(amount, "Generic");

        public new void TakeDamage(float amount, string sourceType)
        {
            if (!IsAlive) return;

            // Reducir daño si hay resistencia activa a este tipo
            if (antigenicVariation && _resistances.Contains(sourceType))
            {
                amount *= (1f - ResistanceReduction);
                Debug.Log($"[Bacteriophage] Resistencia a '{sourceType}': daño reducido.");
            }

            base.TakeDamage(amount);

            if (antigenicVariation)
                RegisterDamageHit(sourceType);
        }

        private void RegisterDamageHit(string sourceType)
        {
            if (!_damageHits.ContainsKey(sourceType))
            {
                _damageHits[sourceType]  = 0;
                _hitTimers[sourceType]   = 0f;
            }

            _damageHits[sourceType]++;
            _hitTimers[sourceType] = ResistanceWindow;

            if (_damageHits[sourceType] >= ResistanceHitThreshold &&
                !_resistances.Contains(sourceType))
            {
                StartCoroutine(DevelopResistance(sourceType));
            }
        }

        private IEnumerator DevelopResistance(string sourceType)
        {
            _resistances.Add(sourceType);
            _damageHits[sourceType] = 0;

            // Cambio visual: tono amarillento (nueva capa proteica)
            if (_sr != null) _sr.color = new Color(1f, 0.85f, 0.2f);

            Debug.Log($"[Bacteriophage] ¡VARIACIÓN ANTIGÉNICA! Resistente a '{sourceType}' por {ResistanceDuration}s.");

            yield return new WaitForSeconds(ResistanceDuration);

            _resistances.Remove(sourceType);
            if (_sr != null) _sr.color = Color.white;
            Debug.Log($"[Bacteriophage] Resistencia a '{sourceType}' expirada.");
        }

        private void TickAntigenicVariation()
        {
            if (!antigenicVariation) return;

            List<string> toReset = new List<string>();
            foreach (var key in new List<string>(_hitTimers.Keys))
            {
                _hitTimers[key] -= Time.deltaTime;
                if (_hitTimers[key] <= 0f)
                {
                    toReset.Add(key);
                }
            }
            foreach (var key in toReset)
            {
                _damageHits.Remove(key);
                _hitTimers.Remove(key);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>Configura el fago como variante Rápida (anchorTime=2s, HP=60%).</summary>
        public void SetFastVariant()
        {
            anchorTime = 2f;
            maxHP      = maxHP * 0.6f;
            CurrentHP  = maxHP;
            moveSpeed  = 4f;
        }

        /// <summary>Configura el fago como variante Silenciosa.</summary>
        public void SetSilentVariant()
        {
            isSilent = true;
            if (_sr != null)
                _sr.color = new Color(1f, 1f, 1f, 0.15f);
        }

        #endregion
    }
}
