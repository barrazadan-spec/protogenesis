using System.Collections;
using UnityEngine;
using Protogenesis.Archetypes;
using Protogenesis.Views;

namespace Protogenesis.Core
{
    /// <summary>
    /// Singleton central del juego. Gestiona el estado global: era, Game Over, pausa
    /// y la secuencia de apoptosis cuando la CAP pierde toda la energía.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        // ── Singleton ────────────────────────────────────────────────────────────
        public static GameManager Instance { get; private set; }

        // ── Estado global ─────────────────────────────────────────────────────────
        /// <summary>Era evolutiva actual (0=Protocélula … 4=Eucariota).</summary>
        public int CurrentEra { get; private set; } = 0;

        public bool IsGameOver { get; private set; } = false;
        public bool IsPaused   { get; private set; } = false;

        // ── Apoptosis (countdown antes de Game Over definitivo) ───────────────────
        [Header("Apoptosis")]
        [Tooltip("Segundos de gracia antes de que la apoptosis sea definitiva.")]
        [SerializeField] private float apoptosisDuration = 30f;

        public bool  IsInApoptosis    { get; private set; } = false;
        /// <summary>Progreso normalizado 0-1 de la cuenta atrás (0 = inicio, 1 = fin).</summary>
        public float ApoptosisProgress { get; private set; } = 0f;
        /// <summary>Segundos restantes en la cuenta atrás de apoptosis.</summary>
        public float ApoptosisTimeLeft => Mathf.Max(0f, apoptosisDuration * (1f - ApoptosisProgress));
        private Coroutine _apoptosisCoroutine;

        // ── Stats de sesión (usados por meta-progresión) ──────────────────────────
        /// <summary>ATP total acumulado en esta run (no el actual, sino el histórico).</summary>
        public float TotalATPProduced { get; set; } = 0f;

        /// <summary>Tiempo de juego activo en segundos (no cuenta mientras está pausado).</summary>
        public float ActivePlayTime { get; private set; } = 0f;

        // ── Arquetipo activo (GDD v2) ─────────────────────────────────────────────
        /// <summary>Arquetipo emergente activo, evaluado por ArchetypeResolver.</summary>
        public ArchetypeType CurrentArchetype { get; private set; } = ArchetypeType.None;

        /// <summary>Establece el arquetipo actual (llamado por ArchetypeResolver).</summary>
        public void SetArchetype(ArchetypeType archetype) => CurrentArchetype = archetype;

        // ── Flags de logros in-run ────────────────────────────────────────────────
        public bool GameCompleted        { get; set; } = false;
        public int  BacteriophagesKilled { get; set; } = 0;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            // Patrón Singleton estricto
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Update()
        {
            if (Instance != this) return;
            if (!IsGameOver && !IsPaused)
                ActivePlayTime += Time.deltaTime;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Era

        /// <summary>
        /// Avanza a la nueva era, actualiza el estado global y notifica al sistema.
        /// </summary>
        /// <param name="newEra">Número de era destino (1-4).</param>
        public void AdvanceEra(int newEra)
        {
            if (newEra <= CurrentEra)
            {
                Debug.LogWarning($"[GameManager] Intento de avanzar a era {newEra} desde {CurrentEra}. Ignorado.");
                return;
            }

            int previousEra = CurrentEra;
            CurrentEra = newEra;

            // Legado: snapshot de la colonia antes de avanzar
            LegacySystem.Instance?.SnapshotCurrentColony(previousEra);

            Debug.Log($"[GameManager] ERA {previousEra} → ERA {newEra}");
            EventBus.TriggerEraChanged(previousEra, newEra);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Apoptosis & Game Over

        /// <summary>
        /// Inicia la cuenta regresiva de apoptosis. Si la CAP sobrevive los
        /// <see cref="apoptosisDuration"/> segundos, se recupera al 20% HP.
        /// Si es golpeada durante ese tiempo, es Game Over definitivo.
        /// </summary>
        public void StartApoptosis()
        {
            if (IsInApoptosis || IsGameOver) return;

            IsInApoptosis     = true;
            ApoptosisProgress = 0f;
            Debug.Log("[GameManager] ¡APOPTOSIS INICIADA! La célula tiene 30 segundos.");
            EventBus.TriggerApoptosisStart(apoptosisDuration);

            _apoptosisCoroutine = StartCoroutine(ApoptosisCountdown());
        }

        private IEnumerator ApoptosisCountdown()
        {
            float elapsed = 0f;
            while (elapsed < apoptosisDuration)
            {
                elapsed += Time.deltaTime;
                ApoptosisProgress = elapsed / apoptosisDuration;
                yield return null;
            }

            // Sobrevivió la apoptosis — restaurar al 20% HP
            IsInApoptosis    = false;
            ApoptosisProgress = 0f;
            var cs = Views.CellState.Instance;
            if (cs != null) cs.Heal(cs.MaxHP * 0.20f);
            Debug.Log("[GameManager] Apoptosis superada. HP restaurado al 20%.");
            EventBus.TriggerApoptosisEnd(survived: true);
        }

        /// <summary>
        /// Llama esto cuando la CAP recibe daño durante el estado de apoptosis.
        /// Desencadena el Game Over definitivo.
        /// </summary>
        public void TriggerGameOverDuringApoptosis()
        {
            if (_apoptosisCoroutine != null)
                StopCoroutine(_apoptosisCoroutine);

            IsInApoptosis     = false;
            ApoptosisProgress = 0f;
            GameOver();
        }

        /// <summary>Activa el estado de Game Over y notifica al sistema.</summary>
        public void GameOver()
        {
            if (IsGameOver) return;

            IsGameOver = true;
            Time.timeScale = 0f;
            Debug.Log("[GameManager] GAME OVER");
            EventBus.TriggerGameOver();
        }

        /// <summary>Cancela la apoptosis si el jugador recupera ATP a tiempo.</summary>
        public void CancelApoptosis()
        {
            if (!IsInApoptosis) return;

            if (_apoptosisCoroutine != null)
                StopCoroutine(_apoptosisCoroutine);

            IsInApoptosis     = false;
            ApoptosisProgress = 0f;
            Debug.Log("[GameManager] Apoptosis cancelada. ATP recuperado a tiempo.");
            EventBus.TriggerApoptosisEnd(survived: true);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Pausa

        /// <summary>Alterna el estado de pausa del juego.</summary>
        public void TogglePause()
        {
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
        }

        /// <summary>Establece el estado de pausa directamente.</summary>
        public void SetPause(bool paused)
        {
            IsPaused = paused;
            Time.timeScale = paused ? 0f : 1f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Reinicio

        /// <summary>Reinicia el estado de la sesión para un New Game / New Game+.</summary>
        public void ResetSession()
        {
            CurrentEra            = 0;
            IsGameOver            = false;
            IsPaused              = false;
            IsInApoptosis         = false;
            TotalATPProduced      = 0f;
            ActivePlayTime        = 0f;
            GameCompleted         = false;
            BacteriophagesKilled  = 0;
            CurrentArchetype      = ArchetypeType.None;
            Time.timeScale        = 1f;
        }

        #endregion
    }
}
