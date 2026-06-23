using System.Collections;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Lisosoma — "El sistema digestivo de la célula".
    ///
    /// Daña enemigos en su radio de forma automática cada 0.5 seg.
    /// Nivel 3: puede activar modo móvil (se mueve hacia el enemigo más cercano).
    ///
    /// RIESGO: al ser destruido libera sus enzimas, dañando orgánulos aliados cercanos.
    ///
    /// Mitofagia activa:
    ///   Si hay una Mitocondria en ReadyForMitophagy cerca, el Lisosoma puede
    ///   iniciar la mitofagia en ella para recuperar recursos de forma controlada.
    /// </summary>
    public class Lisosoma : OrganelleBase
    {
        // ── Daño por nivel ────────────────────────────────────────────────────────
        private static readonly float[] DamagePerSec = { 10f, 25f, 50f };
        private static readonly float[] DamageRadius = { 1.5f, 2.5f, 4.0f };

        private const float DamageTickInterval = 0.5f;

        // ── Modo móvil (nivel 3) ──────────────────────────────────────────────────
        [Header("Modo móvil (nivel 3)")]
        [SerializeField] private float mobileSpeed = 2f;

        public bool IsMobile { get; private set; } = false;
        private float _mobilePenalty = 0f; // 10% eficiencia perdida al estar fuera de zona

        // ── Explosión de enzimas al ser destruido ─────────────────────────────────
        [Header("Explosión de enzimas")]
        [SerializeField] private float enzymeExplosionRadius  = 2f;
        [SerializeField] private float enzymeExplosionDPS     = 5f;
        [SerializeField] private float enzymeExplosionDuration = 3f;
        [SerializeField] private GameObject enzymeCloudPrefab;

        // ── Mitofagia ─────────────────────────────────────────────────────────────
        [Header("Mitofagia")]
        [SerializeField] private float mitophagyDetectionRadius = 4f;

        private Rigidbody2D _rb;
        private float       _damageTickTimer = 0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
        }

        private void Start()
        {
            EventBus.TriggerOrganelleBuilt(gameObject, OrganelleType);
        }

        protected override void Update()
        {
            base.Update();

            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            _damageTickTimer += Time.deltaTime;
            if (_damageTickTimer >= DamageTickInterval)
            {
                _damageTickTimer = 0f;
                DamageNearbyEnemies();
            }

            if (IsMobile && CurrentLevel == 3)
                MoveTowardsNearestEnemy();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Daño a enemigos

        private void DamageNearbyEnemies()
        {
            if (Efficiency <= 0f) return;

            float radius = DamageRadius[CurrentLevel - 1];
            float damage = DamagePerSec[CurrentLevel - 1] * DamageTickInterval * Efficiency;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (var hit in hits)
            {
                if (!hit.CompareTag("Enemy")) continue;
                var damageable = hit.GetComponent<Core.IDamageable>();
                damageable?.TakeDamage(damage);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Modo Móvil (nivel 3)

        /// <summary>Activa/desactiva el modo móvil del Lisosoma de nivel 3.</summary>
        public void SetMobileMode(bool active)
        {
            if (CurrentLevel < 3)
            {
                Debug.Log("[Lisosoma] Modo móvil requiere nivel 3.");
                return;
            }
            IsMobile = active;
            _mobilePenalty = active ? 0.1f : 0f;
        }

        private void MoveTowardsNearestEnemy()
        {
            GameObject nearest = FindNearestEnemy();
            if (nearest == null || _rb == null) return;

            Vector2 dir = ((Vector2)nearest.transform.position - (Vector2)transform.position).normalized;
            _rb.MovePosition(_rb.position + dir * mobileSpeed * Time.deltaTime);
        }

        private GameObject FindNearestEnemy()
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            GameObject nearest   = null;
            float minDist        = float.MaxValue;

            foreach (var e in enemies)
            {
                float d = Vector2.Distance(transform.position, e.transform.position);
                if (d < minDist) { minDist = d; nearest = e; }
            }
            return nearest;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Mitofagia activa

        /// <summary>
        /// Busca mitocondrias dañadas cercanas y ejecuta mitofagia en la más dañada.
        /// Llamar desde la UI cuando el jugador lo solicita.
        /// CANON: los lisosomas se fusionan con autofagosomas que contienen las
        /// mitocondrias dañadas, degradándolas y recuperando sus componentes.
        /// </summary>
        public bool TryInitiateMitophagy()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, mitophagyDetectionRadius);

            Mitocondria target  = null;
            float lowestHP     = float.MaxValue;

            foreach (var hit in hits)
            {
                var mito = hit.GetComponent<Mitocondria>();
                if (mito == null || !mito.ReadyForMitophagy) continue;

                float hp = mito.HPPercent;
                if (hp < lowestHP) { lowestHP = hp; target = mito; }
            }

            if (target == null)
            {
                Debug.Log("[Lisosoma] No hay mitocondrias listas para mitofagia en rango.");
                return false;
            }

            target.StartMitophagy();
            Debug.Log($"[Lisosoma] Mitofagia iniciada en {target.name}.");
            return true;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region OrganelleBase overrides

        protected override void OnLevelUp()
        {
            Debug.Log($"[Lisosoma] Nivel {CurrentLevel}: " +
                      $"{DamagePerSec[CurrentLevel - 1]} DPS, radio {DamageRadius[CurrentLevel - 1]}u");
            if (CurrentLevel == 3)
                Debug.Log("[Lisosoma] Nivel 3: modo móvil disponible.");
        }

        protected override void OnOrganelleDestroyed()
        {
            // Explosión de enzimas: daña orgánulos aliados en el área
            if (enzymeCloudPrefab != null)
            {
                var cloud = Instantiate(enzymeCloudPrefab, transform.position, Quaternion.identity);
                Destroy(cloud, enzymeExplosionDuration);
            }
            else
            {
                // Fallback sin prefab
                StartCoroutine(EnzymeExplosionFallback());
            }
            Debug.Log("[Lisosoma] DESTRUIDO — liberando enzimas lisosomales.");
        }

        private IEnumerator EnzymeExplosionFallback()
        {
            float elapsed = 0f;
            while (elapsed < enzymeExplosionDuration)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, enzymeExplosionRadius);
                foreach (var hit in hits)
                {
                    if (!hit.CompareTag("AllyOrganelle")) continue;
                    hit.GetComponent<OrganelleBase>()?.TakeDamage(enzymeExplosionDPS * 0.5f);
                }
                elapsed += 0.5f;
                yield return new WaitForSeconds(0.5f);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            if (CurrentLevel < 1) return;
            Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.3f);
            Gizmos.DrawSphere(transform.position, DamageRadius[CurrentLevel - 1]);
            Gizmos.color = new Color(0.8f, 0.2f, 0.2f, 0.8f);
            Gizmos.DrawWireSphere(transform.position, DamageRadius[CurrentLevel - 1]);

            // Radio de detección de mitofagia
            Gizmos.color = new Color(0.5f, 0.0f, 0.5f, 0.2f);
            Gizmos.DrawWireSphere(transform.position, mitophagyDetectionRadius);
        }

        #endregion
    }
}
