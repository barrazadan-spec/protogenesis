using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Rendering;

namespace Protogenesis.Core
{
    /// <summary>
    /// SettingsController — Menú de pausa y opciones del juego (Primordia, Prompt 6.3).
    ///
    /// Gestiona:
    ///   · Pausa/reanudación   — Time.timeScale, notifica OnPauseChanged.
    ///   · Audio               — MasterVolume, MusicVolume, SFXVolume → AudioController.
    ///   · Gráficos            — Nivel de calidad (QualitySettings), pantalla completa.
    ///   · Controles           — Rebinding de 9 acciones de juego → guardado en PlayerPrefs.
    ///   · Persistencia        — Carga en Awake, guarda en cada cambio y en OnApplicationQuit.
    ///
    /// Panel OnGUI con 3 pestañas (Audio / Gráficos / Controles):
    ///   Tecla Escape  → toggle pausa (excepto cuando otro sistema tiene el foco).
    ///   Botón Resetear → restaura los defaults hardcodeados.
    ///
    /// API estática:
    ///   SettingsController.GetKey(GameAction)  — devuelve la KeyCode configurada.
    ///   SettingsController.IsPaused            — los sistemas de input comprueban esto.
    /// </summary>
    public enum GameAction
    {
        ToggleView       = 0,
        NucleusPanel     = 1,
        SpecializationUI = 2,
        SpecializationTree= 3,
        Flee             = 4,
        BurstAttack      = 5,
        Defend           = 6,
        ToxinAttack      = 7,
        Pause            = 8,
    }

    public class SettingsController : MonoBehaviour
    {
        public static SettingsController Instance { get; private set; }

        private static readonly KeyCode[] DefaultKeys =
        {
            KeyCode.Tab,    // ToggleView
            KeyCode.N,      // NucleusPanel
            KeyCode.E,      // SpecializationUI
            KeyCode.T,      // SpecializationTree
            KeyCode.F,      // Flee
            KeyCode.Space,  // BurstAttack
            KeyCode.Q,      // Defend
            KeyCode.E,      // ToxinAttack  (mismo que SpecUI — contextos exclusivos)
            KeyCode.Escape, // Pause
        };

        private static readonly string[] ActionNames =
        {
            "Cambiar Vista",
            "Panel Núcleo",
            "Árbol Espec.",
            "Árbol Desbloqueos",
            "Huir del combate",
            "Ráfaga ATP",
            "Defensa",
            "Toxina",
            "Pausa",
        };

        // ── Estado ────────────────────────────────────────────────────────────────
        /// <summary>Delega a GameManager para que haya una única fuente de verdad.</summary>
        public static bool IsPaused => GameManager.Instance != null ? GameManager.Instance.IsPaused : false;

        // Volúmenes (0-1)
        private float _masterVol = 1.0f;
        private float _musicVol  = 0.55f;
        private float _sfxVol    = 0.75f;

        // Gráficos
        private int  _qualityLevel = 3;  // Unity quality index (0-5)
        private bool _fullscreen   = true;

        // Key bindings
        private readonly KeyCode[] _keys = new KeyCode[9];

        // ── Rebind en curso ───────────────────────────────────────────────────────
        private bool   _rebinding     = false;
        private int    _rebindTarget  = -1;

        // ── OnGUI ─────────────────────────────────────────────────────────────────
        private bool   _open          = false;
        private int    _activeTab     = 0;   // 0=Audio 1=Gráficos 2=Controles
        private Vector2 _scrollCtrl  = Vector2.zero;

        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _tabActive;
        private GUIStyle _tabInactive;
        private GUIStyle _labelStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _btnRed;
        private GUIStyle _btnBinding;
        private GUIStyle _sliderLabelStyle;
        private bool     _stylesReady = false;

        // Nombres de niveles de calidad
        private static readonly string[] QualityNames =
            { "Muy bajo", "Bajo", "Medio", "Alto", "Muy alto", "Ultra" };

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            LoadFromPrefs();
        }

        private void Update()
        {
            // Rebinding en curso: captura la próxima tecla presionada
            if (_rebinding)
            {
                CaptureRebindKey();
                return;
            }

            // Tecla de pausa (solo si no hay otro panel abierto en primer plano)
            if (Input.GetKeyDown(GetKey(GameAction.Pause)))
                TogglePause();
        }

        private void OnApplicationQuit()
        {
            SaveToPrefs();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Pausa

        public void TogglePause()
        {
            if (IsPaused) Resume(); else Pause();
        }

        public void Pause()
        {
            GameManager.Instance?.SetPause(true);
            _open = true;
        }

        public void Resume()
        {
            GameManager.Instance?.SetPause(false);
            _open      = false;
            _rebinding = false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Configuración de audio

        public void SetMasterVolume(float v)
        {
            _masterVol = Mathf.Clamp01(v);
            AudioListener.volume = _masterVol;
            SaveToPrefs();
        }

        public void SetMusicVolume(float v)
        {
            _musicVol = Mathf.Clamp01(v);
            AudioController.Instance?.SetMusicVolume(_musicVol);
            SaveToPrefs();
        }

        public void SetSFXVolume(float v)
        {
            _sfxVol = Mathf.Clamp01(v);
            AudioController.Instance?.SetSFXVolume(_sfxVol);
            SaveToPrefs();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Configuración de gráficos

        public void SetQuality(int level)
        {
            _qualityLevel = Mathf.Clamp(level, 0, QualitySettings.names.Length - 1);
            QualitySettings.SetQualityLevel(_qualityLevel, true);
            SaveToPrefs();
        }

        public void SetFullscreen(bool value)
        {
            _fullscreen  = value;
            Screen.fullScreen = value;
            SaveToPrefs();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Key bindings

        /// <summary>Devuelve la KeyCode configurada para la acción dada.</summary>
        public static KeyCode GetKey(GameAction action)
        {
            if (Instance == null) return DefaultKeys[(int)action];
            return Instance._keys[(int)action];
        }

        private void StartRebind(int actionIndex)
        {
            _rebinding    = true;
            _rebindTarget = actionIndex;
        }

        private void CaptureRebindKey()
        {
            // Detectar la primera tecla presionada (excepto modificadores solos)
            foreach (KeyCode kc in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (kc == KeyCode.None) continue;
                if (kc >= KeyCode.LeftShift && kc <= KeyCode.RightAlt) continue; // ignorar modificadores
                if (!Input.GetKeyDown(kc)) continue;

                if (kc == KeyCode.Escape)
                {
                    // Escape cancela el rebind sin guardar
                    _rebinding = false;
                    return;
                }

                _keys[_rebindTarget] = kc;
                _rebinding = false;
                SaveToPrefs();
                return;
            }
        }

        private void ResetDefaultKeys()
        {
            for (int i = 0; i < _keys.Length; i++)
                _keys[i] = DefaultKeys[i];
            SaveToPrefs();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Persistencia

        private const string PrefPrefix = "Primordia_";

        public void SaveToPrefs()
        {
            PlayerPrefs.SetFloat(PrefPrefix + "MasterVol",   _masterVol);
            PlayerPrefs.SetFloat(PrefPrefix + "MusicVol",    _musicVol);
            PlayerPrefs.SetFloat(PrefPrefix + "SFXVol",      _sfxVol);
            PlayerPrefs.SetInt  (PrefPrefix + "Quality",     _qualityLevel);
            PlayerPrefs.SetInt  (PrefPrefix + "Fullscreen",  _fullscreen ? 1 : 0);

            for (int i = 0; i < _keys.Length; i++)
                PlayerPrefs.SetInt(PrefPrefix + $"Key{i}", (int)_keys[i]);

            PlayerPrefs.Save();
        }

        public void LoadFromPrefs()
        {
            // Inicializar con defaults primero
            for (int i = 0; i < _keys.Length; i++)
                _keys[i] = DefaultKeys[i];

            _masterVol    = PlayerPrefs.GetFloat(PrefPrefix + "MasterVol",  1.0f);
            _musicVol     = PlayerPrefs.GetFloat(PrefPrefix + "MusicVol",   0.55f);
            _sfxVol       = PlayerPrefs.GetFloat(PrefPrefix + "SFXVol",     0.75f);
            _qualityLevel = PlayerPrefs.GetInt  (PrefPrefix + "Quality",    3);
            _fullscreen   = PlayerPrefs.GetInt  (PrefPrefix + "Fullscreen", 1) == 1;

            for (int i = 0; i < _keys.Length; i++)
            {
                int stored = PlayerPrefs.GetInt(PrefPrefix + $"Key{i}", -1);
                if (stored >= 0) _keys[i] = (KeyCode)stored;
            }

            // Aplicar configuración cargada
            AudioListener.volume = _masterVol;
            QualitySettings.SetQualityLevel(_qualityLevel, true);
            Screen.fullScreen = _fullscreen;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI

        private void OnGUI()
        {
            if (!_open) return;
            if (!_stylesReady) InitStyles();

            // Overlay oscuro
            GUI.color = new Color(0f, 0f, 0f, 0.60f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float pw = 440f;
            float ph = 420f;
            float px = (Screen.width  - pw) * 0.5f;
            float py = (Screen.height - ph) * 0.5f;

            GUILayout.BeginArea(new Rect(px, py, pw, ph), _panelStyle);

            // ── Título ──────────────────────────────────────────────────────────
            GUILayout.Label("OPCIONES  [Esc]", _titleStyle);
            GUILayout.Space(6f);

            // ── Pestañas ────────────────────────────────────────────────────────
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Audio",     _activeTab == 0 ? _tabActive : _tabInactive)) _activeTab = 0;
            if (GUILayout.Button("Gráficos",  _activeTab == 1 ? _tabActive : _tabInactive)) _activeTab = 1;
            if (GUILayout.Button("Controles", _activeTab == 2 ? _tabActive : _tabInactive)) _activeTab = 2;
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);

            // ── Contenido de pestaña ────────────────────────────────────────────
            switch (_activeTab)
            {
                case 0: DrawAudioTab();    break;
                case 1: DrawGraphicsTab(); break;
                case 2: DrawControlsTab(); break;
            }

            GUILayout.FlexibleSpace();
            GUILayout.Space(8f);

            // ── Botones inferiores ───────────────────────────────────────────────
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Resetear defaults", _btnRed))
            {
                ResetDefaultKeys();
                SetMasterVolume(1.0f);
                SetMusicVolume(0.55f);
                SetSFXVolume(0.75f);
                SetQuality(3);
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Reanudar  [Esc]", _btnStyle))
                Resume();
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        // ── Pestaña Audio ─────────────────────────────────────────────────────────
        private void DrawAudioTab()
        {
            DrawSlider("Volumen Maestro", _masterVol, v => SetMasterVolume(v));
            DrawSlider("Música",          _musicVol,  v => SetMusicVolume(v));
            DrawSlider("Efectos (SFX)",   _sfxVol,    v => SetSFXVolume(v));
        }

        private void DrawSlider(string label, float value, System.Action<float> onChange)
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label(label, _labelStyle, GUILayout.Width(160f));
            float newVal = GUILayout.HorizontalSlider(value, 0f, 1f, GUILayout.ExpandWidth(true));
            GUILayout.Label($"{value * 100f:F0}%", _sliderLabelStyle, GUILayout.Width(38f));
            GUILayout.EndHorizontal();
            GUILayout.Space(4f);

            // Solo disparar cambio si el valor se movió
            if (!Mathf.Approximately(newVal, value))
                onChange(newVal);
        }

        // ── Pestaña Gráficos ──────────────────────────────────────────────────────
        private void DrawGraphicsTab()
        {
            // Calidad
            GUILayout.Label("Calidad gráfica:", _labelStyle);
            GUILayout.BeginHorizontal();
            for (int i = 0; i < QualityNames.Length; i++)
            {
                bool active = _qualityLevel == i;
                if (GUILayout.Button(QualityNames[i], active ? _tabActive : _tabInactive))
                    SetQuality(i);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            // Pantalla completa
            GUILayout.BeginHorizontal();
            GUILayout.Label("Pantalla completa:", _labelStyle, GUILayout.Width(180f));
            if (GUILayout.Button(_fullscreen ? "ON" : "OFF",
                _fullscreen ? _tabActive : _tabInactive, GUILayout.Width(60f)))
            {
                SetFullscreen(!_fullscreen);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10f);

            // Resolución actual (informativa)
            GUILayout.Label($"Resolución: {Screen.width} × {Screen.height}", _labelStyle);
        }

        // ── Pestaña Controles ─────────────────────────────────────────────────────
        private void DrawControlsTab()
        {
            if (_rebinding)
            {
                GUILayout.Label($"Presiona una tecla para\n\"{ActionNames[_rebindTarget]}\"…\n[Esc] para cancelar",
                    _titleStyle);
                return;
            }

            _scrollCtrl = GUILayout.BeginScrollView(_scrollCtrl, GUILayout.Height(260f));

            for (int i = 0; i < _keys.Length; i++)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(ActionNames[i], _labelStyle, GUILayout.Width(170f));

                // Botón con la tecla actual
                string keyLabel = _keys[i].ToString();
                if (GUILayout.Button(keyLabel, _btnBinding, GUILayout.Width(120f)))
                    StartRebind(i);

                // Indicador de conflicto (misma tecla en otro slot, contextos diferentes)
                bool conflict = HasConflict(i);
                if (conflict)
                {
                    Color prev = GUI.color;
                    GUI.color = new Color(1f, 0.7f, 0.2f);
                    GUILayout.Label("⚠", _labelStyle, GUILayout.Width(20f));
                    GUI.color = prev;
                }

                GUILayout.EndHorizontal();
                GUILayout.Space(2f);
            }

            GUILayout.EndScrollView();

            GUILayout.Label("Haz clic en una tecla para reasignarla.  [Esc] cancela.",
                _sliderLabelStyle);
        }

        private bool HasConflict(int idx)
        {
            // Conflicto real: misma tecla en acciones que pueden activarse al mismo tiempo
            // (SpecializationUI [E] y ToxinAttack [E] son contextos mutuamente exclusivos — no conflicto)
            for (int j = 0; j < _keys.Length; j++)
            {
                if (j == idx) continue;
                if (_keys[j] != _keys[idx]) continue;

                // Pares E/ToxinAttack son aceptables
                bool isEPair = (idx == (int)GameAction.SpecializationUI && j == (int)GameAction.ToxinAttack)
                            || (idx == (int)GameAction.ToxinAttack && j == (int)GameAction.SpecializationUI);
                if (isEPair) continue;

                return true;
            }
            return false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Styles

        private void InitStyles()
        {
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding  = new RectOffset(16, 16, 12, 12),
                fontSize = 12
            };
            _panelStyle.normal.background = MakeTex(new Color(0.04f, 0.06f, 0.14f, 0.97f));

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 16,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                wordWrap  = true
            };
            _titleStyle.normal.textColor = new Color(0.75f, 0.92f, 1f);

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 12 };
            _labelStyle.normal.textColor = Color.white;

            _sliderLabelStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            _sliderLabelStyle.normal.textColor = new Color(0.70f, 0.75f, 0.80f);

            _btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 12 };

            _btnRed = new GUIStyle(_btnStyle);
            _btnRed.normal.textColor   = new Color(1f, 0.4f, 0.3f);
            _btnRed.hover.textColor    = new Color(1f, 0.3f, 0.2f);

            _tabActive = new GUIStyle(GUI.skin.button) { fontSize = 11, fontStyle = FontStyle.Bold };
            _tabActive.normal.textColor  = new Color(0.25f, 0.85f, 1f);

            _tabInactive = new GUIStyle(GUI.skin.button) { fontSize = 11 };
            _tabInactive.normal.textColor= new Color(0.60f, 0.65f, 0.70f);

            _btnBinding = new GUIStyle(GUI.skin.button) { fontSize = 12, alignment = TextAnchor.MiddleCenter };
            _btnBinding.normal.textColor = new Color(0.90f, 0.85f, 0.40f);

            _stylesReady = true;
        }

        private static Texture2D MakeTex(Color color)
        {
            var tex = new Texture2D(2, 2);
            tex.SetPixels(new[] { color, color, color, color });
            tex.Apply();
            return tex;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        public float MasterVolume  => _masterVol;
        public float MusicVolume   => _musicVol;
        public float SFXVolume     => _sfxVol;
        public int   QualityLevel  => _qualityLevel;
        public bool  Fullscreen    => _fullscreen;

        #endregion
    }
}
