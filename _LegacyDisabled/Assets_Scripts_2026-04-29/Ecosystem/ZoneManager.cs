using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Ecosystem
{
    /// <summary>
    /// Define y gestiona las 5 zonas de la gota (GDD v3 §6.1).
    ///
    /// Cada zona tiene condiciones ambientales propias y modifica la experiencia
    /// del jugador al entrar en ella. Las zonas se superponen; un organismo puede
    /// estar en zona Pelágica y también tocar una Microcavidad.
    ///
    /// Uso: añadir este componente al mismo GameObject que los colliders de zona.
    /// Alternativamente, usar BiomeZone para zonas individuales más detalladas.
    /// </summary>
    public enum ZoneType
    {
        /// <summary>Alta luz, O2 abundante, fotótrofos dominantes.
        /// Fantasía: superficie viva, destellos, microburbujas.</summary>
        Fotica,

        /// <summary>Columna central, luz media, depredación cruzada.
        /// Fantasía: estelas y partículas en suspensión.</summary>
        Pelagica,

        /// <summary>Fondo, poca luz, detrito, anoxia localizada.
        /// Fantasía: sedimento, sombras, descomposición.</summary>
        Bentonica,

        /// <summary>Materia orgánica concentrada, biofilm, contagio.
        /// Fantasía: manchas densas, colonias enzimáticas.</summary>
        Debris,

        /// <summary>Refugios entre restos y estructuras.
        /// Fantasía: huecos con refracción distinta, staging de emboscadas.</summary>
        Microcavidad
    }

    /// <summary>Propiedades de una zona del mapa.</summary>
    [System.Serializable]
    public class ZoneProperties
    {
        public ZoneType type;
        public string   displayName;

        [Header("Modificadores ambientales")]
        [Range(0f, 1f)]  public float oxygenLevel     = 1f;
        [Range(0f, 14f)] public float phLevel         = 7f;
        [Range(0f, 100f)]public float temperature     = 37f;
        [Range(0f, 2f)]  public float lightIntensity  = 1f;

        [Header("Modificadores de gameplay")]
        public float atpProductionMult  = 1f;
        public float movementSpeedMult  = 1f;
        public float visibilityRange    = 8f;  // radio de visión del jugador en esta zona
        public bool  hasAnoxicRisk      = false;

        [Header("Visual")]
        public Color  ambientTint       = Color.white;
        public float  particleDensity   = 1f;
    }

    /// <summary>
    /// Zona de la gota. Attach a un GameObject con Collider2D trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class ZoneRegion : MonoBehaviour
    {
        [SerializeField] public ZoneProperties properties;

        // Cuántos jugadores/entidades están actualmente en esta zona
        private int _occupants = 0;

        private void Awake()
        {
            var col = GetComponent<Collider2D>();
            col.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!IsPlayerOrUnit(other)) return;
            _occupants++;

            if (other.CompareTag("AllyCAP") || other.CompareTag("Membrane"))
            {
                ApplyZoneEffects();
                NotifySystemsEnter();
                EventBus.TriggerBiomeEntered(properties.type.ToString());
                Debug.Log($"[Zone] Entrando en zona: {properties.displayName}");
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!IsPlayerOrUnit(other)) return;
            _occupants = Mathf.Max(0, _occupants - 1);

            if (_occupants == 0 && (other.CompareTag("AllyCAP") || other.CompareTag("Membrane")))
                NotifySystemsExit();
        }

        private void NotifySystemsEnter()
        {
            // Zona béntica — más fría, disipa calor extra
            if (properties.type == ZoneType.Bentonica)
            {
                var mhs = Core.MetabolicHeatSystem.Instance;
                if (mhs != null) mhs.IsInBenthicZone = true;
            }

            // BalanceSystem registra cambio de zona para resetear FarmingDecay
            Core.BalanceSystem.Instance?.NotifyZoneChanged(properties.type.ToString());
        }

        private void NotifySystemsExit()
        {
            if (properties.type == ZoneType.Bentonica)
            {
                var mhs = Core.MetabolicHeatSystem.Instance;
                if (mhs != null) mhs.IsInBenthicZone = false;
            }
        }

        private void ApplyZoneEffects()
        {
            var em = EnvironmentManager.Instance;
            if (em != null)
                em.ApplyScenarioEnvironment(properties.phLevel, properties.oxygenLevel, properties.temperature);
        }

        private bool IsPlayerOrUnit(Collider2D col)
            => col.CompareTag("AllyCAP")    || col.CompareTag("Membrane") ||
               col.CompareTag("AllyUnit")   || col.CompareTag("AllyOrganelle");

        private void OnDrawGizmos()
        {
            if (properties == null) return;
            Gizmos.color = new Color(properties.ambientTint.r,
                                     properties.ambientTint.g,
                                     properties.ambientTint.b, 0.12f);

            var col = GetComponent<Collider2D>();
            if (col is CircleCollider2D circle)
                Gizmos.DrawSphere(transform.position, circle.radius);
            else if (col is BoxCollider2D box)
                Gizmos.DrawCube(transform.position, box.size);
        }
    }
}
