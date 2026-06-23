using UnityEngine;

namespace Protogenesis.Rendering
{
    /// <summary>
    /// Visualización procedural del fondo del mapa dividido por zonas biológicas (GDD §6.1).
    ///
    /// Crea tres bandas de fondo y partículas ambiente características de cada zona:
    ///   Fótica   (y  4…12)  #E8F4FD — azul claro     + burbujas ascendentes
    ///   Pelágica (y -4… 4)  #1A4A7A — azul medio     + partículas en suspensión
    ///   Béntica  (y-12…-4)  #0A1428 — azul muy oscuro + sedimento descendente
    ///
    /// Todo es procedural: no requiere prefabs ni assets.
    /// Partículas gestionadas como pool de SpriteRenderers (65 GOs en total).
    /// </summary>
    public class MapVisualizer : MonoBehaviour
    {
        // ── Bounds del mapa (coordinadas de mundo) ────────────────────────────────
        private const float MapHalfW     = 25f;   // anchura visible = 50 unidades
        private const float ZoneH        = 8f;    // altura de cada banda
        private const float FoticaMinY   =  4f;   // Fótica: y ∈ [4, 12]
        private const float PelagicaMinY = -4f;   // Pelágica: y ∈ [-4, 4]
        private const float BenticaMinY  = -12f;  // Béntica: y ∈ [-12, -4]

        // ── Colores de fondo ──────────────────────────────────────────────────────
        private static readonly Color BgFotica   = new Color(0.91f, 0.96f, 0.99f); // #E8F4FD
        private static readonly Color BgPelagica = new Color(0.10f, 0.29f, 0.48f); // #1A4A7A
        private static readonly Color BgBentica  = new Color(0.04f, 0.08f, 0.16f); // #0A1428

        // ── Orden de capas ────────────────────────────────────────────────────────
        private const int SortBg         = -50;   // fondo de zona
        private const int SortParticle   = -30;   // delante de fondos, detrás del juego

        // ─────────────────────────────────────────────────────────────────────────
        // Datos de cada partícula: transforma + velocidad.
        // El SpriteRenderer se guarda para actualizaciones futuras (fade, etc.).
        private struct Particle
        {
            public Transform      tr;
            public SpriteRenderer sr;
            public Vector2        vel;
        }

        private Particle[] _bubbles;      // 20 — Fótica, suben
        private Particle[] _suspension;  // 25 — Pelágica, derivan
        private Particle[] _sediment;    // 20 — Béntica, caen

        private static Sprite _dotSprite;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            _dotSprite = BuildDotSprite();
            CreateBackground();
            InitBubbles();
            InitSuspension();
            InitSediment();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            UpdateBubbles(dt);
            UpdateSuspension(dt);
            UpdateSediment(dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Fondos por zona

        private void CreateBackground()
        {
            // Fótica — tercio superior
            MakeBand("BG_Fotica",
                     centerY: FoticaMinY + ZoneH * 0.5f,
                     color:   BgFotica,
                     order:   SortBg);

            // Pelágica — franja central
            MakeBand("BG_Pelagica",
                     centerY: 0f,
                     color:   BgPelagica,
                     order:   SortBg + 1);

            // Béntica — tercio inferior
            MakeBand("BG_Bentica",
                     centerY: BenticaMinY + ZoneH * 0.5f,
                     color:   BgBentica,
                     order:   SortBg + 2);
        }

        private void MakeBand(string goName, float centerY, Color color, int order)
        {
            var go = new GameObject(goName);
            go.transform.SetParent(transform);
            go.transform.position   = new Vector3(0f, centerY, 0f);
            go.transform.localScale = new Vector3(MapHalfW * 2f, ZoneH, 1f);

            var tex = new Texture2D(1, 1, TextureFormat.RGB24, false);
            tex.SetPixel(0, 0, Color.white);
            tex.Apply();
            var spr = Sprite.Create(tex, new Rect(0, 0, 1, 1), Vector2.one * 0.5f, 1f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = spr;
            sr.color        = color;
            sr.sortingOrder = order;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Burbujas — Fótica (ascienden)

        private void InitBubbles()
        {
            _bubbles = new Particle[20];
            for (int i = 0; i < _bubbles.Length; i++)
            {
                float alpha = Random.Range(0.12f, 0.36f);
                float size  = Random.Range(0.04f, 0.15f);
                Color col   = new Color(0.85f, 0.95f, 1.00f, alpha);

                var go = MakeParticleGO("Bubble", col, size);
                go.transform.position = RandPosFotica();

                _bubbles[i] = new Particle
                {
                    tr  = go.transform,
                    sr  = go.GetComponent<SpriteRenderer>(),
                    vel = new Vector2(Random.Range(-0.12f, 0.12f),
                                     Random.Range(0.40f,  1.10f))
                };
            }
        }

        private void UpdateBubbles(float dt)
        {
            for (int i = 0; i < _bubbles.Length; i++)
            {
                ref var p   = ref _bubbles[i];
                p.tr.position += (Vector3)(p.vel * dt);

                // Cuando sale por arriba → reaparece en el suelo de la zona Fótica
                if (p.tr.position.y > FoticaMinY + ZoneH + 0.2f)
                    p.tr.position = new Vector3(
                        Random.Range(-MapHalfW * 0.9f, MapHalfW * 0.9f),
                        FoticaMinY, 0f);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Suspensión — Pelágica (deriva lenta)

        private void InitSuspension()
        {
            _suspension = new Particle[25];
            for (int i = 0; i < _suspension.Length; i++)
            {
                float alpha = Random.Range(0.06f, 0.22f);
                float size  = Random.Range(0.03f, 0.09f);
                Color col   = new Color(0.55f, 0.75f, 0.95f, alpha);

                var go = MakeParticleGO("Suspension", col, size);
                go.transform.position = RandPosPelagica();

                _suspension[i] = new Particle
                {
                    tr  = go.transform,
                    sr  = go.GetComponent<SpriteRenderer>(),
                    vel = Random.insideUnitCircle * Random.Range(0.05f, 0.18f)
                };
            }
        }

        private void UpdateSuspension(float dt)
        {
            for (int i = 0; i < _suspension.Length; i++)
            {
                ref var p   = ref _suspension[i];
                p.tr.position += (Vector3)(p.vel * dt);

                // Micro-perturbación browniana esporádica
                if (Random.value < 0.008f)
                    p.vel = Random.insideUnitCircle * Random.Range(0.05f, 0.18f);

                // Rebotar en los límites verticales de la zona
                var pos = p.tr.position;
                if (pos.y > FoticaMinY   - 0.1f) p.vel.y = -Mathf.Abs(p.vel.y);
                if (pos.y < PelagicaMinY + 0.1f) p.vel.y =  Mathf.Abs(p.vel.y);
                if (pos.x >  MapHalfW * 0.95f)   p.vel.x = -Mathf.Abs(p.vel.x);
                if (pos.x < -MapHalfW * 0.95f)   p.vel.x =  Mathf.Abs(p.vel.x);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Sedimento — Béntica (cae)

        private void InitSediment()
        {
            _sediment = new Particle[20];
            for (int i = 0; i < _sediment.Length; i++)
            {
                float alpha = Random.Range(0.10f, 0.30f);
                float size  = Random.Range(0.03f, 0.10f);
                Color col   = new Color(0.45f, 0.38f, 0.28f, alpha);  // marrón arena tenue

                var go = MakeParticleGO("Sediment", col, size);
                // Distribuir inicialmente por toda la altura de la zona
                go.transform.position = RandPosBentica();

                _sediment[i] = new Particle
                {
                    tr  = go.transform,
                    sr  = go.GetComponent<SpriteRenderer>(),
                    vel = new Vector2(Random.Range(-0.06f, 0.06f),
                                     -Random.Range(0.18f, 0.65f))   // cae hacia abajo
                };
            }
        }

        private void UpdateSediment(float dt)
        {
            for (int i = 0; i < _sediment.Length; i++)
            {
                ref var p   = ref _sediment[i];
                p.tr.position += (Vector3)(p.vel * dt);

                // Cuando toca el fondo → reaparece en el techo de la zona Béntica
                if (p.tr.position.y < BenticaMinY - 0.2f)
                    p.tr.position = new Vector3(
                        Random.Range(-MapHalfW * 0.9f, MapHalfW * 0.9f),
                        PelagicaMinY - 0.05f, 0f);   // justo bajo la línea Pelágica/Béntica
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private GameObject MakeParticleGO(string label, Color color, float worldSize)
        {
            var go = new GameObject($"_P_{label}");
            go.transform.SetParent(transform);
            go.transform.localScale = Vector3.one * worldSize;

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite       = _dotSprite;
            sr.color        = color;
            sr.sortingOrder = SortParticle;
            return go;
        }

        private static Vector3 RandPosFotica()
            => new Vector3(Random.Range(-MapHalfW * 0.9f, MapHalfW * 0.9f),
                           Random.Range(FoticaMinY, FoticaMinY + ZoneH), 0f);

        private static Vector3 RandPosPelagica()
            => new Vector3(Random.Range(-MapHalfW * 0.9f, MapHalfW * 0.9f),
                           Random.Range(PelagicaMinY, FoticaMinY), 0f);

        private static Vector3 RandPosBentica()
            => new Vector3(Random.Range(-MapHalfW * 0.9f, MapHalfW * 0.9f),
                           Random.Range(BenticaMinY, PelagicaMinY), 0f);

        /// <summary>Sprite circular suavizado (sin assets externos).</summary>
        private static Sprite BuildDotSprite()
        {
            const int Res = 16;
            var tex = new Texture2D(Res, Res, TextureFormat.RGBA32, false);
            tex.filterMode = UnityEngine.FilterMode.Bilinear;
            float c = Res * 0.5f, r = Res * 0.46f;
            for (int x = 0; x < Res; x++)
                for (int y = 0; y < Res; y++)
                {
                    float d    = Vector2.Distance(new Vector2(x, y), new Vector2(c, c));
                    float edge = Mathf.Clamp01((r - d) / (Res * 0.10f));
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, edge));
                }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, Res, Res), Vector2.one * 0.5f, Res);
        }

        #endregion
    }
}
