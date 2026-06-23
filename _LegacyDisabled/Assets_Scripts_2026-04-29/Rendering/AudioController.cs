using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Views;
using Protogenesis.Progression;

namespace Protogenesis.Rendering
{
    /// <summary>
    /// AudioController — Audio reactivo al estado celular (Primordia, Prompt 6.2).
    ///
    /// Gestiona 5 canales de AudioSource independientes:
    ///   musicA / musicB  — crossfade de música ambiental (dual-buffer, 1.5 s fade)
    ///   ambient          — sonido de ambiente en loop (burbujas, zumbido, etc.)
    ///   sfx              — efectos one-shot (colectar, instalar, daño…)
    ///   stress           — latido/pulso cardíaco en estrés alto (loop, vol dinámico)
    ///
    /// Comportamiento reactivo:
    ///   · Zona/Vista     → crossfade a clip de música asignado
    ///   · StressLevel    → pitch global sube con estrés (1.0 → 1.25)
    ///                      canal stress activo en Critical/Catastrophic
    ///   · HP bajo        → pitch baja ligeramente (0.92 al 10% HP)
    ///   · Daño           → SFX de impacto + duck momentáneo de música (−40%)
    ///   · Acciones       → SFX específico por evento (instalar, colectar, consolidar…)
    ///
    /// Fallback procedural:
    ///   Si un clip no está asignado, GenerateBeep() produce un tono sinusoidal
    ///   de 0.12 s para que los eventos no queden en silencio total durante el
    ///   desarrollo. Desactivable con enableProceduralFallback = false.
    ///
    /// Todos los campos de clip son opcionales (null-safe).
    /// </summary>
    public class AudioController : MonoBehaviour
    {
        public static AudioController Instance { get; private set; }

        // ── Clips de música ───────────────────────────────────────────────────────
        [Header("Música — Exterior (asignar clips opcionales)")]
        [SerializeField] private AudioClip musicFotica;
        [SerializeField] private AudioClip musicPelagica;
        [SerializeField] private AudioClip musicBentica;
        [SerializeField] private AudioClip musicCombat;

        [Header("Música — Interior")]
        [SerializeField] private AudioClip musicInterior;

        [Header("Ambiente (loops)")]
        [SerializeField] private AudioClip ambientFotica;
        [SerializeField] private AudioClip ambientBentica;
        [SerializeField] private AudioClip ambientInterior;

        [Header("SFX")]
        [SerializeField] private AudioClip sfxCollect;
        [SerializeField] private AudioClip sfxInstall;
        [SerializeField] private AudioClip sfxUninstall;
        [SerializeField] private AudioClip sfxDamage;
        [SerializeField] private AudioClip sfxDeath;
        [SerializeField] private AudioClip sfxConsolidate;
        [SerializeField] private AudioClip sfxUnlock;
        [SerializeField] private AudioClip sfxChallengeStart;
        [SerializeField] private AudioClip sfxVictory;
        [SerializeField] private AudioClip sfxDefeat;
        [SerializeField] private AudioClip sfxViewSwitch;

        [Header("Estrés")]
        [SerializeField] private AudioClip sfxHeartbeat;

        [Header("Volúmenes base")]
        [SerializeField] [Range(0,1)] private float musicVolume   = 0.55f;
        [SerializeField] [Range(0,1)] private float ambientVolume = 0.30f;
        [SerializeField] [Range(0,1)] private float sfxVolume     = 0.75f;
        [SerializeField] [Range(0,1)] private float stressVolume  = 0.50f;

        [Header("Pitch")]
        [SerializeField] private float pitchStressMax = 1.22f;  // pitch a estrés 100%
        [SerializeField] private float pitchHPMin     = 0.92f;  // pitch a HP 10%
        [SerializeField] private float pitchLerpSpeed = 2.5f;

        [Header("Duck de música al recibir daño")]
        [SerializeField] private float duckAmount     = 0.40f;  // reducción de volumen
        [SerializeField] private float duckDuration   = 0.35f;

        [Header("Fallback procedural")]
        [SerializeField] private bool  enableProceduralFallback = true;

        // ── AudioSources ──────────────────────────────────────────────────────────
        private AudioSource _musicA;
        private AudioSource _musicB;
        private AudioSource _ambient;
        private AudioSource _sfxSrc;
        private AudioSource _stressSrc;

        private bool _musicOnA   = true;  // cuál buffer está activo
        private float _pitchTarget = 1f;
        private float _pitchCurrent= 1f;

        // Duck
        private float _duckTimer   = 0f;
        private bool  _isDucking   = false;

        // Estado
        private string   _currentBiome = "Pelagica";
        private ViewType _currentView  = ViewType.Exterior;
        private bool     _inCombat     = false;

        // Cache de clips procedurales (frecuencia → clip)
        private readonly Dictionary<float, AudioClip> _proceduralCache = new();

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
            BuildAudioSources();
        }

        private void OnEnable()
        {
            EventBus.OnBiomeEntered           += OnBiomeEntered;
            EventBus.OnViewChanged            += OnViewChanged;
            EventBus.OnStressLevelChanged     += OnStressLevelChanged;
            EventBus.OnStressChanged          += OnStressChanged;
            EventBus.OnResourceChanged        += OnResourceCollected;
            EventBus.OnSlotInstalled          += OnSlotInstalled;
            EventBus.OnSlotUninstalled        += OnSlotUninstalled;
            EventBus.OnOrganelleInstalled     += OnOrganelleInstalled;
            EventBus.OnSpecializationConsolidated += OnSpecializationConsolidated;
            EventBus.OnUpgradeUnlocked        += OnUpgradeUnlocked;
            EventBus.OnChallengeStarted       += OnChallengeStarted;
            EventBus.OnChallengeEnded         += OnChallengeEnded;
            EventBus.OnCellDeath              += OnCellDeath;
        }

        private void OnDisable()
        {
            EventBus.OnBiomeEntered           -= OnBiomeEntered;
            EventBus.OnViewChanged            -= OnViewChanged;
            EventBus.OnStressLevelChanged     -= OnStressLevelChanged;
            EventBus.OnStressChanged          -= OnStressChanged;
            EventBus.OnResourceChanged        -= OnResourceCollected;
            EventBus.OnSlotInstalled          -= OnSlotInstalled;
            EventBus.OnSlotUninstalled        -= OnSlotUninstalled;
            EventBus.OnOrganelleInstalled     -= OnOrganelleInstalled;
            EventBus.OnSpecializationConsolidated -= OnSpecializationConsolidated;
            EventBus.OnUpgradeUnlocked        -= OnUpgradeUnlocked;
            EventBus.OnChallengeStarted       -= OnChallengeStarted;
            EventBus.OnChallengeEnded         -= OnChallengeEnded;
            EventBus.OnCellDeath              -= OnCellDeath;
        }

        private void Update()
        {
            TickPitch();
            TickDuck();
            TickHeartbeat();
        }

        private void OnDestroy()
        {
            foreach (var clip in _proceduralCache.Values)
                if (clip != null) Destroy(clip);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Construcción de AudioSources

        private void BuildAudioSources()
        {
            _musicA   = AddSource("MusicA",   musicVolume,   true,  false);
            _musicB   = AddSource("MusicB",   0f,            true,  false);
            _ambient  = AddSource("Ambient",  ambientVolume, true,  false);
            _sfxSrc   = AddSource("SFX",      sfxVolume,     false, false);
            _stressSrc= AddSource("Stress",   0f,            true,  false);

            // Arrancar música inicial (Pelágica)
            CrossfadeTo(ResolveMusic(), 0f);  // sin fade al inicio
        }

        private AudioSource AddSource(string id, float vol, bool loop, bool playOnAwake)
        {
            var go = new GameObject($"_Audio_{id}");
            go.transform.SetParent(transform, false);
            var src         = go.AddComponent<AudioSource>();
            src.loop        = loop;
            src.volume      = vol;
            src.playOnAwake = playOnAwake;
            src.spatialBlend= 0f; // 2D
            return src;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Música / Crossfade

        private void CrossfadeTo(AudioClip clip, float duration = 1.5f)
        {
            if (clip == null && !enableProceduralFallback) return;

            var incoming = _musicOnA ? _musicB : _musicA;
            var outgoing = _musicOnA ? _musicA : _musicB;
            _musicOnA = !_musicOnA;

            incoming.clip   = clip ?? GetOrCreateBeep(220f, 0.5f);
            incoming.volume = 0f;
            incoming.Play();

            StartCoroutine(FadeMusicRoutine(outgoing, incoming, duration));
        }

        private IEnumerator FadeMusicRoutine(AudioSource from, AudioSource to, float duration)
        {
            float fromStart = from.volume;
            float elapsed   = 0f;

            if (duration <= 0f)
            {
                from.volume = 0f;
                from.Stop();
                to.volume = musicVolume;
                yield break;
            }

            while (elapsed < duration)
            {
                float t  = elapsed / duration;
                from.volume = Mathf.Lerp(fromStart, 0f,          t);
                to.volume   = Mathf.Lerp(0f,        musicVolume, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            from.volume = 0f;
            from.Stop();
            to.volume = musicVolume;
        }

        private AudioClip ResolveMusic()
        {
            if (_inCombat)        return musicCombat;
            if (_currentView == ViewType.Interior) return musicInterior;
            return _currentBiome switch
            {
                "Fotica"   => musicFotica,
                "Bentonica"=> musicBentica,
                _          => musicPelagica
            };
        }

        private void ResolveAmbient()
        {
            AudioClip target = _currentView == ViewType.Interior
                ? ambientInterior
                : _currentBiome == "Fotica"    ? ambientFotica
                : _currentBiome == "Bentonica" ? ambientBentica
                : null;

            if (target == null && !enableProceduralFallback)
            {
                _ambient.Stop();
                return;
            }

            var clip = target ?? GetOrCreateBeep(80f, 1.0f);
            if (_ambient.clip == clip) return;
            _ambient.clip = clip;
            _ambient.volume = ambientVolume;
            _ambient.Play();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region SFX

        private void PlaySFX(AudioClip clip, float pitchVariance = 0.05f)
        {
            if (clip == null && !enableProceduralFallback) return;
            var c = clip ?? GetOrCreateBeep(440f, 0.12f);
            _sfxSrc.pitch = 1f + Random.Range(-pitchVariance, pitchVariance);
            _sfxSrc.PlayOneShot(c, sfxVolume);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Tick

        private void TickPitch()
        {
            // Calcular pitch objetivo: estrés → sube, HP bajo → baja
            var ss = StressSystem.Instance;
            var cs = CellState.Instance;

            float stressN  = ss != null ? ss.StressNormalized : 0f;
            float hpN      = cs != null ? cs.HPNormalized     : 1f;

            float stressPitch = Mathf.Lerp(1f, pitchStressMax, stressN);
            float hpPitch     = hpN < 0.20f
                ? Mathf.Lerp(pitchHPMin, 1f, hpN / 0.20f)
                : 1f;

            _pitchTarget  = stressPitch * hpPitch;
            _pitchCurrent = Mathf.Lerp(_pitchCurrent, _pitchTarget, pitchLerpSpeed * Time.deltaTime);

            _musicA.pitch   = _pitchCurrent;
            _musicB.pitch   = _pitchCurrent;
            _ambient.pitch  = _pitchCurrent;
        }

        private void TickDuck()
        {
            if (!_isDucking) return;
            _duckTimer -= Time.deltaTime;
            if (_duckTimer <= 0f)
            {
                _isDucking = false;
                // Restaurar volumen suavemente
                StartCoroutine(RestoreMusicVolume());
            }
        }

        private IEnumerator RestoreMusicVolume()
        {
            var active = _musicOnA ? _musicA : _musicB;
            float start   = active.volume;
            float elapsed = 0f;
            float dur     = 0.4f;
            while (elapsed < dur)
            {
                active.volume = Mathf.Lerp(start, musicVolume, elapsed / dur);
                elapsed += Time.deltaTime;
                yield return null;
            }
            active.volume = musicVolume;
        }

        private float _heartbeatTimer = 0f;
        private void TickHeartbeat()
        {
            if (_stressSrc == null) return;

            var ss = StressSystem.Instance;
            if (ss == null || ss.CurrentLevel < StressLevel.Critical)
            {
                _stressSrc.volume = Mathf.MoveTowards(_stressSrc.volume, 0f, Time.deltaTime * 2f);
                return;
            }

            float targetVol = ss.CurrentLevel == StressLevel.Catastrophic
                ? stressVolume
                : stressVolume * 0.55f;
            _stressSrc.volume = Mathf.MoveTowards(_stressSrc.volume, targetVol, Time.deltaTime * 3f);

            // Pulso periódico si no hay clip de heartbeat asignado
            if (sfxHeartbeat == null && enableProceduralFallback)
            {
                float interval = ss.CurrentLevel == StressLevel.Catastrophic ? 0.5f : 0.8f;
                _heartbeatTimer -= Time.deltaTime;
                if (_heartbeatTimer <= 0f)
                {
                    _heartbeatTimer = interval;
                    _sfxSrc.pitch = 0.75f;
                    _sfxSrc.PlayOneShot(GetOrCreateBeep(60f, 0.08f), stressVolume * 0.6f);
                    _sfxSrc.pitch = 1f;
                }
            }
            else if (sfxHeartbeat != null && !_stressSrc.isPlaying)
            {
                _stressSrc.clip = sfxHeartbeat;
                _stressSrc.Play();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Handlers de eventos

        private void OnBiomeEntered(string biomeId)
        {
            _currentBiome = biomeId;
            if (!_inCombat) CrossfadeTo(ResolveMusic());
            ResolveAmbient();
        }

        private void OnViewChanged(ViewType prev, ViewType next)
        {
            _currentView = next;
            CrossfadeTo(ResolveMusic(), 0.8f);
            ResolveAmbient();
            PlaySFX(sfxViewSwitch, 0.02f);
        }

        private void OnStressLevelChanged(StressLevel prev, StressLevel next)
        {
            // Spike de pitch + SFX corto al subir de nivel
            if (next > prev)
            {
                _pitchCurrent = Mathf.Min(_pitchCurrent + 0.08f, pitchStressMax + 0.1f);
                if (next >= StressLevel.Critical)
                    PlaySFX(GetOrCreateBeep(180f, 0.06f), 0f);
            }
        }

        private void OnStressChanged(float value, float delta)
        {
            // Gestionado por TickPitch() cada frame
        }

        private void OnResourceCollected(ResourceType type, float current, float delta)
        {
            if (delta > 0.5f) // solo si ganó recursos (no pérdidas menores)
                PlaySFX(sfxCollect, 0.08f);
        }

        private void OnSlotInstalled(Slots.SlotType type, int index, string effectId)
            => PlaySFX(sfxInstall, 0.04f);

        private void OnSlotUninstalled(Slots.SlotType type, int index)
            => PlaySFX(sfxUninstall, 0.04f);

        private void OnOrganelleInstalled(int slotIndex, string organelleId)
            => PlaySFX(sfxInstall, 0.03f);

        private void OnSpecializationConsolidated(SpecializationType type, string displayName)
        {
            var clip = sfxConsolidate ?? GetOrCreateBeep(660f, 0.5f);
            _sfxSrc.PlayOneShot(clip, sfxVolume * 1.2f);
        }

        private void OnUpgradeUnlocked(string nodeId)
            => PlaySFX(sfxUnlock, 0.05f);

        private void OnChallengeStarted(string opponentName)
        {
            _inCombat = true;
            CrossfadeTo(musicCombat ?? GetOrCreateBeep(330f, 1.0f), 0.6f);
            PlaySFX(sfxChallengeStart, 0.02f);
        }

        private void OnChallengeEnded(ChallengeResult result)
        {
            _inCombat = false;
            CrossfadeTo(ResolveMusic(), 1.2f);

            switch (result)
            {
                case ChallengeResult.Victory:
                    PlaySFX(sfxVictory ?? GetOrCreateBeep(880f, 0.4f), 0f);
                    break;
                case ChallengeResult.Defeat:
                    PlaySFX(sfxDefeat ?? GetOrCreateBeep(110f, 0.5f), 0f);
                    break;
            }
        }

        private void OnCellDeath()
        {
            // Bajar toda la música rápidamente
            StartCoroutine(FadeMusicRoutine(
                _musicOnA ? _musicA : _musicB,
                _musicOnA ? _musicB : _musicA,   // silencio
                0.8f));
            PlaySFX(sfxDeath ?? GetOrCreateBeep(55f, 0.8f), 0f);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>Dispara el duck de música por daño y el SFX de impacto.</summary>
        public void TriggerDamageAudio()
        {
            PlaySFX(sfxDamage, 0.10f);

            // Duck de música
            var active = _musicOnA ? _musicA : _musicB;
            active.volume = musicVolume * (1f - duckAmount);
            _isDucking  = true;
            _duckTimer  = duckDuration;
        }

        /// <summary>Cambia el volumen maestro de música (0-1).</summary>
        public void SetMusicVolume(float v)
        {
            musicVolume = Mathf.Clamp01(v);
            var active = _musicOnA ? _musicA : _musicB;
            if (!_isDucking) active.volume = musicVolume;
        }

        /// <summary>Cambia el volumen de SFX (0-1).</summary>
        public void SetSFXVolume(float v) => sfxVolume = Mathf.Clamp01(v);

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Generación procedural de tonos

        /// <summary>
        /// Genera (o devuelve del cache) un clip sinusoidal de frecuencia y duración dadas.
        /// Útil como fallback cuando no hay clips de audio asignados.
        /// </summary>
        private AudioClip GetOrCreateBeep(float frequency, float duration)
        {
            float key = frequency * 1000f + duration;
            if (_proceduralCache.TryGetValue(key, out var cached) && cached != null)
                return cached;

            int sampleRate = 44100;
            int samples    = Mathf.CeilToInt(sampleRate * duration);
            var data       = new float[samples];

            float angularFreq = 2f * Mathf.PI * frequency;
            float fadeIn      = sampleRate * 0.01f; // 10 ms
            float fadeOut     = sampleRate * 0.05f; // 50 ms

            for (int i = 0; i < samples; i++)
            {
                float t       = (float)i / sampleRate;
                float amp     = 1f;
                if (i < fadeIn)  amp = i / fadeIn;
                if (i > samples - fadeOut) amp = (samples - i) / fadeOut;
                data[i] = Mathf.Sin(angularFreq * t) * amp * 0.35f;
            }

            var clip = AudioClip.Create($"beep_{frequency}Hz", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            _proceduralCache[key] = clip;
            return clip;
        }

        #endregion
    }
}
