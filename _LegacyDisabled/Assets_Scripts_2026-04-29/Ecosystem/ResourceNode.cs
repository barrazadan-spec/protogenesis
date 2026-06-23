using System.Collections;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Ecosystem
{
    /// <summary>
    /// Nodo de recurso coleccionable en el mundo (GDD §6.1).
    ///
    /// El jugador lo recoge pasando sobre él (trigger).
    /// Se desactiva al recolectarse y reaparece tras <see cref="respawnTime"/> segundos.
    /// Si es de zona Debris, <c>respawnTime = 0</c> y no reaparece.
    ///
    /// Colores canónicos:
    ///   Glucosa  → #F0C030  parpadeo suave
    ///   Nitrógeno→ #3090B0  estático
    ///   Debris   → #C08030  más grande, evento único
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class ResourceNode : MonoBehaviour
    {
        // ── Configuración pública ─────────────────────────────────────────────────
        public ResourceType resourceType  = ResourceType.Glucose;
        public float        amount        = 1.0f;
        public float        respawnTime   = 15f;   // 0 → no reaparece (Debris)
        public ZoneType     zone          = ZoneType.Fotica;

        // ── Colores por tipo ──────────────────────────────────────────────────────
        // Primordia: ATP, AminoAcids, Nucleotides, Lipids
        private static readonly Color ColATP         = new Color(0.29f, 0.56f, 0.85f);  // azul
        private static readonly Color ColAminoAcids  = new Color(0.94f, 0.75f, 0.19f);  // amarillo
        private static readonly Color ColNucleotides = new Color(0.19f, 0.80f, 0.44f);  // verde
        private static readonly Color ColLipids      = new Color(0.56f, 0.27f, 0.68f);  // morado
        // Legacy (mantener para compilación)
        private static readonly Color ColGlucose  = new Color(0.94f, 0.75f, 0.19f);  // #F0C030
        private static readonly Color ColNitrogen = new Color(0.19f, 0.56f, 0.69f);  // #3090B0
        private static readonly Color ColDebris   = new Color(0.75f, 0.50f, 0.19f);  // #C08030

        // ── Referencias ───────────────────────────────────────────────────────────
        private SpriteRenderer _sr;
        private CircleCollider2D _col;
        private Color _baseColor;
        private float _pulseOffset;
        private bool  _active = true;

        // ── Constantes ────────────────────────────────────────────────────────────
        private const float PulseSpeed     = 2.2f;
        private const float PulseAmpAlpha  = 0.22f;   // glucosa parpadea ±22% alpha
        private const float FlashDuration  = 0.18f;
        private const float FloatDuration  = 0.75f;
        private const float FloatSpeed     = 2.8f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            _sr  = GetComponent<SpriteRenderer>();
            _col = GetComponent<CircleCollider2D>();

            _col.isTrigger = true;

            _sr.sprite       = CreateCircleSprite(32);
            _sr.sortingOrder = 5;

            _pulseOffset = Random.Range(0f, Mathf.PI * 2f); // fases distintas entre nodos
        }

        private void Start()
        {
            Configure();
        }

        private void Update()
        {
            if (!_active) return;

            // ATP y AminoAcids parpadean; Nucleotides/Lipids son estáticos
            bool pulses = resourceType == ResourceType.ATP
                       || resourceType == ResourceType.AminoAcids
                       || resourceType == ResourceType.Glucose; // legacy
            if (pulses)
            {
                float alpha = Mathf.Clamp01(
                    (_baseColor.a - PulseAmpAlpha) +
                    (PulseAmpAlpha * 2f) * (0.5f + 0.5f * Mathf.Sin(Time.time * PulseSpeed + _pulseOffset))
                );
                _sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, alpha);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Configuración por tipo / zona

        /// <summary>
        /// Aplica color, tamaño y collider según <see cref="zone"/> y <see cref="resourceType"/>.
        /// Puede llamarse de nuevo si se reasignan las propiedades en runtime.
        /// </summary>
        public void Configure()
        {
            // Color según tipo de recurso (Primordia)
            _baseColor = resourceType switch
            {
                ResourceType.ATP         => ColATP,
                ResourceType.AminoAcids  => ColAminoAcids,
                ResourceType.Nucleotides => ColNucleotides,
                ResourceType.Lipids      => ColLipids,
                ResourceType.Nitrogen    => ColNitrogen,
                _                        => ColGlucose
            };

            if (zone == ZoneType.Debris)
            {
                _baseColor = ColDebris;
                transform.localScale = Vector3.one * 0.70f;
                _col.radius = 0.55f;
            }
            else
            {
                float scale = zone switch
                {
                    ZoneType.Bentonica => 0.40f,
                    ZoneType.Pelagica  => 0.28f,
                    _                  => 0.35f    // Fotica
                };
                float rad = zone switch
                {
                    ZoneType.Bentonica => 0.35f,
                    ZoneType.Pelagica  => 0.25f,
                    _                  => 0.30f
                };
                transform.localScale = Vector3.one * scale;
                _col.radius = rad;
            }

            _sr.color = _baseColor;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Recolección

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_active) return;
            if (!other.CompareTag("Player")) return;

            Collect(other.transform);
        }

        /// <summary>Recolecta el nodo desde código externo (ej: PlayerCellExterior).</summary>
        public void Collect() => Collect(null);

        private void Collect(Transform collector)
        {
            _active = false;
            _sr.enabled  = false;
            _col.enabled = false;

            // Entregar recurso
            ResourceManager.Instance?.AddResource(resourceType, amount);

            // Flash de color en el CAP
            StartCoroutine(FlashCollector(collector));

            // Partícula flotante sobre el nodo
            StartCoroutine(FloatingParticle());

            // Respawn o destruir
            if (respawnTime > 0f)
                StartCoroutine(RespawnAfter(respawnTime));
            // else: no reaparece (Debris)
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Efectos visuales

        private IEnumerator FlashCollector(Transform collector)
        {
            var sr = collector?.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            Color original = sr.color;
            sr.color = _baseColor;
            yield return new WaitForSeconds(FlashDuration);
            sr.color = original;
        }

        private IEnumerator FloatingParticle()
        {
            // Crea un pequeño punto que sube y se desvanece
            var go  = new GameObject("_NodePickupFX");
            go.transform.position = transform.position;

            var sr  = go.AddComponent<SpriteRenderer>();
            sr.sprite       = CreateCircleSprite(8);
            sr.sortingOrder = 10;

            float elapsed = 0f;
            while (elapsed < FloatDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / FloatDuration;

                go.transform.position += Vector3.up * FloatSpeed * Time.deltaTime;
                sr.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 1f - t);

                // Escala crece y luego encoge
                float s = Mathf.Sin(t * Mathf.PI) * 0.25f + 0.05f;
                go.transform.localScale = Vector3.one * s;

                yield return null;
            }

            Destroy(go);
        }

        private IEnumerator RespawnAfter(float delay)
        {
            yield return new WaitForSeconds(delay);

            // Mover posición ligeramente aleatoria dentro de la misma zona
            transform.position = ShufflePositionInZone();

            _sr.enabled  = true;
            _col.enabled = true;
            _sr.color    = _baseColor;
            _active      = true;
        }

        /// <summary>
        /// Mueve el nodo a una posición nueva aleatoria dentro de los límites de su zona
        /// para evitar que siempre reaparezca exactamente en el mismo punto.
        /// </summary>
        private Vector2 ShufflePositionInZone()
        {
            float x = Random.Range(-13f, 13f);
            float y = zone switch
            {
                ZoneType.Fotica   => Random.Range(5f,  11f),
                ZoneType.Pelagica => Random.Range(-3f,  3f),
                ZoneType.Bentonica => Random.Range(-11f, -5f),
                _                 => transform.position.y
            };
            return new Vector2(x, y);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Sprite procedural

        private static Sprite CreateCircleSprite(int resolution)
        {
            var tex  = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float c  = resolution * 0.5f;
            float r  = resolution * 0.46f;

            for (int x = 0; x < resolution; x++)
                for (int y = 0; y < resolution; y++)
                {
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    if (dist <= r)
                    {
                        // Borde suavizado
                        float edge  = Mathf.Clamp01((r - dist) / (resolution * 0.08f));
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, edge));
                    }
                    else
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                }

            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, resolution, resolution),
                                 Vector2.one * 0.5f, resolution);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública (para ResourceNodeSpawner)

        /// <summary>
        /// Inicializa el nodo directamente desde código en lugar de usar propiedades
        /// serializadas. Equivale a asignar campos + llamar Configure().
        /// </summary>
        public void Init(ResourceType type, float resourceAmount, float respawn, ZoneType zoneType)
        {
            resourceType = type;
            amount       = resourceAmount;
            respawnTime  = respawn;
            zone         = zoneType;
            Configure();
        }

        #endregion
    }
}
