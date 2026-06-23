using System.Collections;
using UnityEngine;
using Protogenesis.Progression;
using Protogenesis.Views;

namespace Protogenesis.Core
{
    /// <summary>
    /// PowerMomentSystem — Momentos cinematográficos (GDD v4.3, Prompt 4.3).
    ///
    /// consolidation: glow radial + texto flotante con nombre de especialización.
    /// premium: pulsos radiales + zoom out 1.4× al emerger.
    /// elimination: flash + mensaje en pantalla 2.5s.
    /// </summary>
    public class PowerMomentSystem : MonoBehaviour
    {
        public static PowerMomentSystem Instance { get; private set; }

        // Estado de efectos activos
        private bool   _glowActive;
        private float  _glowRadius;
        private float  _glowAlpha;
        private Vector3 _glowPos;

        private string _floatingText;
        private Color  _floatingColor;
        private float  _floatingY;
        private float  _floatingTimer;

        private string _eliminationMsg;
        private float  _eliminationTimer;

        // ─────────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()  => EventBus.OnPowerMoment += OnPowerMoment;
        private void OnDisable() => EventBus.OnPowerMoment -= OnPowerMoment;

        private void Update()
        {
            // Glow radial
            if (_glowActive)
            {
                _glowRadius += Time.deltaTime * 4f; // 2u en 0.5s
                _glowAlpha   = Mathf.Max(0f, 0.6f - _glowRadius * 0.3f);
                if (_glowAlpha <= 0f) _glowActive = false;
            }

            // Texto flotante
            if (_floatingTimer > 0f)
            {
                _floatingTimer -= Time.deltaTime;
                _floatingY     -= Time.deltaTime * (1f / 2.5f); // sube 1u en 2.5s
            }

            // Mensaje eliminación
            if (_eliminationTimer > 0f)
                _eliminationTimer -= Time.deltaTime;
        }

        // ─────────────────────────────────────────────────────────────────────────
        private void OnPowerMoment(string type)
        {
            switch (type)
            {
                case "consolidation": TriggerConsolidation(); break;
                case "premium":       StartCoroutine(TriggerPremium()); break;
                case "elimination":   TriggerElimination();  break;
            }
        }

        private void TriggerConsolidation()
        {
            var player = Object.FindFirstObjectByType<PlayerCellExterior>();
            if (player == null) return;

            _glowActive = true;
            _glowRadius = 0f;
            _glowAlpha  = 0.6f;
            _glowPos    = player.transform.position;

            var tracker = SpecializationTracker.Instance;
            string specName = SpecializationTracker.GetDisplayName(
                tracker?.ConsolidatedType ?? SpecializationType.None);

            _floatingText   = specName;
            _floatingColor  = GetSpecColor(tracker?.ConsolidatedType ?? SpecializationType.None);
            _floatingY      = player.transform.position.y;
            _floatingTimer  = 2.5f;
        }

        private IEnumerator TriggerPremium()
        {
            Camera cam = Camera.main;
            if (cam == null) yield break;

            float origSize = cam.orthographicSize;
            float targetSize = origSize * 1.4f;

            // 8 pulsos de 1s antes del emerge
            for (int i = 0; i < 8; i++)
            {
                _glowActive = true;
                _glowRadius = 0f;
                _glowAlpha  = 0.5f;
                var player = Object.FindFirstObjectByType<PlayerCellExterior>();
                if (player != null) _glowPos = player.transform.position;
                yield return new WaitForSeconds(1f);
            }

            // Zoom out en 0.4s
            float elapsed = 0f;
            while (elapsed < 0.4f)
            {
                elapsed += Time.deltaTime;
                cam.orthographicSize = Mathf.Lerp(origSize, targetSize, elapsed / 0.4f);
                yield return null;
            }

            // Volver al zoom normal en 2s
            elapsed = 0f;
            while (elapsed < 2f)
            {
                elapsed += Time.deltaTime;
                cam.orthographicSize = Mathf.Lerp(targetSize, origSize, elapsed / 2f);
                yield return null;
            }
            cam.orthographicSize = origSize;
        }

        private void TriggerElimination()
        {
            var tracker = SpecializationTracker.Instance;
            string mySpec = SpecializationTracker.GetDisplayName(
                tracker?.ConsolidatedType ?? SpecializationType.None);
            _eliminationMsg   = $"{mySpec} eliminó a un rival";
            _eliminationTimer = 2.5f;

            _glowActive = true;
            _glowRadius = 0f;
            _glowAlpha  = 0.6f;
            var player = Object.FindFirstObjectByType<PlayerCellExterior>();
            if (player != null) _glowPos = player.transform.position;
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI

        private Texture2D _glowTex;

        private void OnGUI()
        {
            if (ViewManager.Instance != null && !ViewManager.Instance.IsExteriorActive) return;

            Camera cam = Camera.main;
            if (cam == null) return;

            // ── Glow radial ───────────────────────────────────────────────────────
            if (_glowActive)
            {
                float ppu = Screen.height / (cam.orthographicSize * 2f);
                Vector3 sp = cam.WorldToScreenPoint(_glowPos);
                float cx = sp.x, cy = Screen.height - sp.y;
                float r = _glowRadius * ppu;

                GUI.color = new Color(1f, 1f, 1f, _glowAlpha);
                GUI.DrawTexture(new Rect(cx - r, cy - r, r * 2f, r * 2f),
                    GetGlowTex());
                GUI.color = Color.white;
            }

            // ── Texto flotante ────────────────────────────────────────────────────
            if (_floatingTimer > 0f)
            {
                Vector3 sp = cam.WorldToScreenPoint(new Vector3(_glowPos.x, _floatingY, _glowPos.z));
                float alpha = Mathf.Clamp01(_floatingTimer / 2.5f);
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 18,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                style.normal.textColor = new Color(_floatingColor.r, _floatingColor.g, _floatingColor.b, alpha);
                GUI.Label(new Rect(sp.x - 100f, Screen.height - sp.y - 12f, 200f, 24f),
                    _floatingText, style);
            }

            // ── Mensaje de eliminación ────────────────────────────────────────────
            if (_eliminationTimer > 0f)
            {
                float alpha = Mathf.Clamp01(_eliminationTimer / 2.5f);
                var style = new GUIStyle(GUI.skin.label)
                {
                    fontSize  = 14,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
                style.normal.textColor = new Color(1f, 0.35f, 0.35f, alpha);
                GUI.Label(new Rect(Screen.width * 0.5f - 150f, 20f, 300f, 22f),
                    _eliminationMsg, style);
            }
        }

        private Texture2D GetGlowTex()
        {
            if (_glowTex != null) return _glowTex;
            int sz = 64;
            _glowTex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);
            _glowTex.filterMode = FilterMode.Bilinear;
            Vector2 c = new Vector2(sz * 0.5f, sz * 0.5f);
            for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), c) / (sz * 0.5f);
                float a = d < 1f ? Mathf.Clamp01(1f - d * d) * 0.5f : 0f;
                _glowTex.SetPixel(x, y, new Color(1f, 1f, 1f, a));
            }
            _glowTex.Apply();
            return _glowTex;
        }

        private void OnDestroy() { if (_glowTex != null) Destroy(_glowTex); }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        private static Color GetSpecColor(SpecializationType t) => t switch
        {
            SpecializationType.Bacteria      => new Color(0.416f, 0.706f, 0.831f),
            SpecializationType.Arquea        => new Color(0.882f, 0.635f, 0.247f),
            SpecializationType.Cianobacteria => new Color(0.302f, 0.761f, 0.533f),
            SpecializationType.Hongo         => new Color(0.875f, 0.525f, 0.259f),
            SpecializationType.Ameba         => new Color(0.831f, 0.659f, 0.784f),
            SpecializationType.Flagelado     => new Color(0.302f, 0.714f, 0.675f),
            SpecializationType.Ciliado       => new Color(0.400f, 0.749f, 0.894f),
            SpecializationType.Neutrofilo    => new Color(0.953f, 0.875f, 0.427f),
            SpecializationType.Macrofago     => new Color(0.941f, 0.580f, 0.290f),
            SpecializationType.CelulaNK      => new Color(0.839f, 0.318f, 0.424f),
            SpecializationType.CelulaB       => new Color(0.443f, 0.639f, 0.894f),
            SpecializationType.CelulaMadre   => new Color(0.769f, 0.529f, 0.875f),
            _                                => Color.white,
        };
    }
}
