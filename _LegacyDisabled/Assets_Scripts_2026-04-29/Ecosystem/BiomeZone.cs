using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Ecosystem
{
    /// <summary>
    /// Zona de bioma en el mundo. Modifica las variables ambientales cuando
    /// la CAP entra en ella y dispara el evento OnBiomeEntered.
    ///
    /// Configurar en el Inspector: asignar un CircleCollider2D como trigger
    /// en el mismo GameObject.
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    public class BiomeZone : MonoBehaviour
    {
        [Header("Identidad")]
        public string biomeId;
        public string displayName;
        public Color  zoneColor = new Color(0.2f, 0.8f, 0.4f, 0.15f);

        [Header("Modificadores ambientales al entrar")]
        [Tooltip("Valor de pH que se fuerza al entrar en la zona.")]
        public float targetPH          = 7f;
        [Tooltip("Nivel de O2 que se fuerza al entrar.")]
        public float targetO2          = 1f;
        [Tooltip("Temperatura que se fuerza al entrar.")]
        public float targetTemperature = 37f;

        [Header("Modificadores de recursos")]
        [Tooltip("Multiplicador de producción de Glucosa dentro de la zona.")]
        public float glucoseMultiplier = 1f;
        [Tooltip("Multiplicador de producción de ATP dentro de la zona.")]
        public float atpMultiplier     = 1f;

        private CircleCollider2D _collider;
        private bool             _playerInside = false;

        private void Awake()
        {
            _collider = GetComponent<CircleCollider2D>();
            _collider.isTrigger = true;
        }

        private void OnDrawGizmos()
        {
            var col = GetComponent<CircleCollider2D>();
            if (col == null) return;
            Gizmos.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, col.radius);
            Gizmos.color = zoneColor;
            Gizmos.DrawWireSphere(transform.position, col.radius);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("AllyCAP") && !other.CompareTag("Membrane")) return;
            if (_playerInside) return;

            _playerInside = true;

            var env = EnvironmentManager.Instance;
            if (env != null)
                env.ApplyScenarioEnvironment(targetPH, targetO2, targetTemperature);

            EventBus.TriggerBiomeEntered(biomeId);
            Debug.Log($"[BiomeZone] Entrando en bioma: {displayName}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("AllyCAP") && !other.CompareTag("Membrane")) return;
            _playerInside = false;
        }
    }
}
