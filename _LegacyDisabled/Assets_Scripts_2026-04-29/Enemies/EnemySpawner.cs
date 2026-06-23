using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Ecosystem;

namespace Protogenesis.Enemies
{
    /// <summary>
    /// EnemySpawner — Sincroniza el mundo físico con la simulación Lotka-Volterra.
    ///
    /// Cada syncInterval segundos, lee las poblaciones lógicas de EcosystemManager
    /// y ajusta el número de GameObjects enemigos activos para que representen
    /// un populationRatio (30%) de la población simulada, capped por maxPerSpecies.
    ///
    /// Especies soportadas: Paramecio_NPC, Didinium_NPC, Bacteriophage.
    /// Hidra y Tardígrado se activarán cuando sus clases estén disponibles.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        public static EnemySpawner Instance { get; private set; }

        [Header("Sincronización")]
        [SerializeField] private float syncInterval   = 5f;
        [SerializeField] private float spawnRadius    = 18f;

        [Tooltip("Fracción de la población lógica a materializar como GameObjects.")]
        [SerializeField] [Range(0f, 1f)]
        private float populationRatio = 0.30f;

        [Header("Límites por especie")]
        [SerializeField] private int maxParamecios    = 6;
        [SerializeField] private int maxDidiniums     = 2;
        [SerializeField] private int maxBacteriophages = 4;

        // ── Instancias activas por especie ────────────────────────────────────────
        private readonly Dictionary<string, List<GameObject>> _active
            = new Dictionary<string, List<GameObject>>();

        // ── Catálogo de especies ──────────────────────────────────────────────────
        private struct SpeciesEntry
        {
            public string      speciesId;
            public System.Type componentType;
            public int         maxInstances;
            public int         minEra;
            public Color       color;
            public int         spriteResolution;
        }

        private SpeciesEntry[] _species;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            _species = new SpeciesEntry[]
            {
                new SpeciesEntry
                {
                    speciesId       = "paramecio",
                    componentType   = typeof(Paramecio_NPC),
                    maxInstances    = maxParamecios,
                    minEra          = 0,
                    color           = new Color(0.56f, 0.27f, 0.68f),
                    spriteResolution = 20
                },
                new SpeciesEntry
                {
                    speciesId       = "didinium",
                    componentType   = typeof(Didinium_NPC),
                    maxInstances    = maxDidiniums,
                    minEra          = 1,
                    color           = new Color(0.80f, 0.23f, 0.23f),
                    spriteResolution = 24
                },
                new SpeciesEntry
                {
                    speciesId       = "bacteriophage",
                    componentType   = typeof(Bacteriophage),
                    maxInstances    = maxBacteriophages,
                    minEra          = 0,
                    color           = new Color(0.75f, 0.22f, 0.17f),
                    spriteResolution = 16
                },
            };

            foreach (var s in _species)
            {
                _active[s.speciesId] = new List<GameObject>();
            }
        }

        private void Start()
        {
            StartCoroutine(SyncLoop());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Sync Loop

        private IEnumerator SyncLoop()
        {
            // Delay inicial para asegurar que EcosystemManager haya procesado
            // el primer tick de Lotka-Volterra.
            yield return new WaitForSeconds(2f);

            while (true)
            {
                if (GameManager.Instance == null ||
                   (!GameManager.Instance.IsGameOver && !GameManager.Instance.IsPaused))
                    SyncAll();

                yield return new WaitForSeconds(syncInterval);
            }
        }

        private void SyncAll()
        {
            var em  = EcosystemManager.Instance;
            int era = GameManager.Instance != null ? GameManager.Instance.CurrentEra : 0;

            foreach (var entry in _species)
            {
                if (era < entry.minEra) continue;

                float logicalPop = em != null ? em.GetPopulation(entry.speciesId) : 0f;
                int   target     = Mathf.Clamp(
                    Mathf.RoundToInt(logicalPop * populationRatio),
                    0, entry.maxInstances);

                SyncSpecies(entry, target);
            }
        }

        private void SyncSpecies(SpeciesEntry entry, int targetCount)
        {
            var list = _active[entry.speciesId];

            // Limpiar referencias nulas (enemigos destruidos externamente)
            list.RemoveAll(go => go == null || !go.activeSelf);

            int current = list.Count;

            // Spawnar los que faltan
            for (int i = current; i < targetCount; i++)
                SpawnEnemy(entry);

            // Destruir el exceso
            for (int i = list.Count - 1; i >= targetCount; i--)
            {
                if (list[i] != null)
                    Destroy(list[i]);
                list.RemoveAt(i);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Spawn

        private void SpawnEnemy(SpeciesEntry entry)
        {
            // TODO: Primordia — Vector2 origin = Player.CAP.Instance != null
            //                  ? (Vector2)Player.CAP.Instance.transform.position
            //                  : Vector2.zero;
            Vector2 origin = Vector2.zero; // Primordia stub

            float angle  = Random.Range(0f, Mathf.PI * 2f);
            float radius = spawnRadius + Random.Range(-3f, 3f);
            Vector2 pos  = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;

            var go = new GameObject($"{entry.speciesId}_spawned");
            go.transform.position = pos;
            go.tag = "Enemy";

            var rb = go.AddComponent<Rigidbody2D>();
            rb.bodyType       = RigidbodyType2D.Kinematic;
            rb.gravityScale   = 0f;
            rb.freezeRotation = true;

            go.AddComponent<CircleCollider2D>();

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = CreateCircleSprite(entry.spriteResolution);
            sr.color        = entry.color;
            sr.sortingOrder = 5;

            go.AddComponent(entry.componentType);

            _active[entry.speciesId].Add(go);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Notifica que un enemigo fue destruido para retirarle de la lista de activos.
        /// Si no se llama, el loop de sync limpiará referencias nulas cada syncInterval.
        /// </summary>
        public void NotifyEnemyDied(GameObject go)
        {
            foreach (var list in _active.Values)
                list.Remove(go);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private static Sprite CreateCircleSprite(int resolution)
        {
            var tex    = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            float center = resolution / 2f;
            float radius = resolution / 2f - 1f;

            for (int x = 0; x < resolution; x++)
            for (int y = 0; y < resolution; y++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                tex.SetPixel(x, y, d <= radius ? Color.white : Color.clear);
            }
            tex.Apply();
            return Sprite.Create(tex,
                                 new Rect(0, 0, resolution, resolution),
                                 new Vector2(0.5f, 0.5f),
                                 resolution);
        }

        #endregion
    }
}
