using UnityEngine;

namespace Protogenesis.Organelles
{
    /// <summary>
    /// Sistema de zonas del citoplasma.
    ///
    /// La célula está dividida en 3 anillos concéntricos desde el centro de la CAP:
    ///   Nuclear    (radio 0-3u): Núcleo, Retículo Endoplásmico, Ribosomas
    ///   Media      (radio 3-6u): Mitocondrias, Aparato de Golgi
    ///   Periférica (radio 6-9u): Lisosomas, Vacuolas, Peroxisomas, Defensas
    ///
    /// La célula puede crecer expandiendo su membrana con el método ExpandCell.
    /// Más membrana = más superficie a defender (trade-off estratégico).
    /// </summary>
    public class ZoneSystem : MonoBehaviour
    {
        public static ZoneSystem Instance { get; private set; }

        // ── Radios de zona (expandibles con mejoras) ──────────────────────────────
        [Header("Radios de zona (en unidades de mundo)")]
        [SerializeField] private float nuclearRadius     = 3f;
        [SerializeField] private float middleRadius      = 6f;
        [SerializeField] private float peripheralRadius  = 9f;

        public float NuclearRadius    => nuclearRadius;
        public float MiddleRadius     => middleRadius;
        public float PeripheralRadius => peripheralRadius;

        /// <summary>Centro de la célula (posición de la CAP).</summary>
        private Transform _cellCenter;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()
        {
            // Buscar el centro celular (la CAP)
            // TODO: Primordia — var cap = Player.CAP.Instance;
            object cap = null; // Primordia migration stub
            // TODO: Primordia — if (cap != null) _cellCenter = cap.transform; else
            _cellCenter = transform; // Primordia stub: cap always null
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Validación de placement

        /// <summary>
        /// Verifica si una posición del mundo está dentro de la zona requerida.
        /// </summary>
        public bool IsValidPlacement(Vector2 worldPosition, ZoneType required)
        {
            if (_cellCenter == null) return true;

            float dist = Vector2.Distance(worldPosition, _cellCenter.position);

            return required switch
            {
                ZoneType.Nuclear    => dist <= nuclearRadius,
                ZoneType.Middle     => dist > nuclearRadius && dist <= middleRadius,
                ZoneType.Peripheral => dist > middleRadius  && dist <= peripheralRadius,
                _                   => false
            };
        }

        /// <summary>
        /// Devuelve la zona a la que pertenece una posición del mundo.
        /// Retorna null si está fuera de la célula.
        /// </summary>
        public ZoneType? GetZoneAt(Vector2 worldPosition)
        {
            if (_cellCenter == null) return null;

            float dist = Vector2.Distance(worldPosition, _cellCenter.position);

            if (dist <= nuclearRadius)    return ZoneType.Nuclear;
            if (dist <= middleRadius)     return ZoneType.Middle;
            if (dist <= peripheralRadius) return ZoneType.Peripheral;
            return null;
        }

        /// <summary>Devuelve si una posición está dentro de la célula.</summary>
        public bool IsInsideCell(Vector2 worldPosition)
            => GetZoneAt(worldPosition) != null;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Expansión celular

        /// <summary>
        /// Expande todos los radios de zona en la cantidad dada.
        /// Llamar cuando el jugador construye más membrana o avanza de era.
        /// </summary>
        public void ExpandCell(float amount)
        {
            nuclearRadius    += amount;
            middleRadius     += amount;
            peripheralRadius += amount;
            Debug.Log($"[ZoneSystem] Célula expandida +{amount}u. Periferia → {peripheralRadius}u.");
        }

        /// <summary>Establece radios exactos (usado por ScriptableObjects de escenario).</summary>
        public void SetZoneRadii(float nuclear, float middle, float peripheral)
        {
            nuclearRadius    = nuclear;
            middleRadius     = middle;
            peripheralRadius = peripheral;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Gizmos

        private void OnDrawGizmosSelected()
        {
            Vector3 center = _cellCenter != null ? _cellCenter.position : transform.position;

            // Nuclear — azul
            Gizmos.color = new Color(0.2f, 0.4f, 1.0f, 0.15f);
            DrawCircle(center, nuclearRadius);
            Gizmos.color = new Color(0.2f, 0.4f, 1.0f, 0.6f);
            DrawCircleWire(center, nuclearRadius);

            // Media — verde
            Gizmos.color = new Color(0.0f, 0.66f, 0.42f, 0.10f);
            DrawCircle(center, middleRadius);
            Gizmos.color = new Color(0.0f, 0.66f, 0.42f, 0.5f);
            DrawCircleWire(center, middleRadius);

            // Periférica — amarillo
            Gizmos.color = new Color(0.8f, 0.65f, 0.0f, 0.08f);
            DrawCircle(center, peripheralRadius);
            Gizmos.color = new Color(0.8f, 0.65f, 0.0f, 0.4f);
            DrawCircleWire(center, peripheralRadius);
        }

        private void DrawCircle(Vector3 center, float radius)
        {
            // Simulamos un disco con una esfera plana (solo visual 2D)
            Gizmos.DrawSphere(center, radius * 0.01f); // Solo para marcar el centro
        }

        private void DrawCircleWire(Vector3 center, float radius)
        {
            const int segments = 36;
            float angleStep = 360f / segments;
            Vector3 prev = center + new Vector3(radius, 0f, 0f);
            for (int i = 1; i <= segments; i++)
            {
                float angle = i * angleStep * Mathf.Deg2Rad;
                Vector3 next = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }

        #endregion
    }
}
