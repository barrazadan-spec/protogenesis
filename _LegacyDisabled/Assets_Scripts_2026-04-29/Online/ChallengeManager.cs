using System.Collections;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Views;

namespace Protogenesis.Online
{
    /// <summary>
    /// ChallengeManager — Detección y ciclo de vida del encuentro 1v1 (Primordia, Prompt 5.1).
    ///
    /// Máquina de estados:
    ///   Idle → Detecting → Pending → Active → Resolved
    ///
    ///   Idle       — sin oponente cercano.
    ///   Detecting  — oponente dentro del radio de detección; se muestra alerta.
    ///   Pending    — challenge enviado; esperando aceptación (timeout = acceptTimeout).
    ///                vs IA: auto-acepta en 0.5 s.
    ///                vs Jugador online: espera respuesta via NetworkChallengeMediator.
    ///   Active     — combate en curso; delega mecánicas al CombatMinigame (Prompt 5.2).
    ///   Resolved   — resultado determinado; pausa de resultadoDelay y vuelta a Idle.
    ///
    /// Detección:
    ///   · OverlapCircle cada detectionInterval segundos en el exterior.
    ///   · Busca objetos con tag "Enemy" o "RivalPlayer" dentro de challengeRadius.
    ///   · Solo activo cuando ViewManager.IsExteriorActive.
    ///
    /// Huida:
    ///   · El jugador puede pulsar F (FleeKey) en estado Pending o Active.
    ///   · Huir impone penalización: estrés +15, posición empujada en sentido opuesto.
    ///
    /// Integración online:
    ///   · Si NetworkChallengeMediator está presente, delega aceptación/rechazo por red.
    ///   · Si no, asume modo single-player vs IA (auto-acepta).
    /// </summary>
    public class ChallengeManager : MonoBehaviour
    {
        public static ChallengeManager Instance { get; private set; }

        // ── Configuración ─────────────────────────────────────────────────────────
        [Header("Detección")]
        [SerializeField] private float challengeRadius    = 2.5f;
        [SerializeField] private float detectionInterval  = 0.3f;  // segundos entre scans

        [Header("Tiempos")]
        [SerializeField] private float acceptTimeout      = 5.0f;  // máx. espera al oponente
        [SerializeField] private float aiAcceptDelay      = 0.5f;  // IA acepta tras este delay
        [SerializeField] private float resultDisplayTime  = 2.5f;  // tiempo mostrando resultado

        [Header("Penalización por huida")]
        [SerializeField] private float fleeStressPenalty  = 15f;
        [SerializeField] private float fleeKnockback      = 3.5f;  // unidades

        // Tecla leída de SettingsController en runtime

        // ── Estado ────────────────────────────────────────────────────────────────
        public enum State { Idle, Detecting, Pending, Active, Resolved }

        public State   CurrentState   { get; private set; } = State.Idle;
        public bool    IsChallengeActive => CurrentState == State.Active;

        private GameObject _opponent;
        private string     _opponentName = "";
        private bool       _opponentIsAI  = true;
        private float      _pendingTimer  = 0f;

        // Resultado provisional (se confirma al entrar en Resolved)
        private ChallengeResult _pendingResult;

        // ── Scan ──────────────────────────────────────────────────────────────────
        private float _scanTimer = 0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnCellDeath += OnCellDeath;
        }

        private void OnDisable()
        {
            EventBus.OnCellDeath -= OnCellDeath;
        }

        private void Update()
        {
            // Solo activo en vista Exterior
            if (ViewManager.Instance != null && !ViewManager.Instance.IsExteriorActive) return;

            switch (CurrentState)
            {
                case State.Idle:
                    ScanForOpponent();
                    break;

                case State.Detecting:
                    // Verificar que el oponente sigue en rango
                    if (!IsOpponentInRange())
                        TransitionTo(State.Idle);
                    ScanForOpponent(); // puede actualizar al oponente más cercano
                    break;

                case State.Pending:
                    TickPending();
                    HandleFleeInput();
                    break;

                case State.Active:
                    HandleFleeInput();
                    // La lógica de combate vive en CombatMinigame (Prompt 5.2)
                    break;

                case State.Resolved:
                    // Esperando que termine la coroutine de resultados
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Detección

        private void ScanForOpponent()
        {
            _scanTimer -= Time.deltaTime;
            if (_scanTimer > 0f) return;
            _scanTimer = detectionInterval;

            var player = PlayerCellExterior.Instance;
            if (player == null) return;

            // Buscar enemigos IA
            var hits = Physics2D.OverlapCircleAll(player.transform.position, challengeRadius);
            foreach (var hit in hits)
            {
                if (hit.gameObject == player.gameObject) continue;
                if (hit.gameObject.tag != "Enemy" && hit.gameObject.tag != "RivalPlayer") continue;

                // Encontrado: oponente válido
                _opponent      = hit.gameObject;
                _opponentIsAI  = hit.gameObject.tag == "Enemy";
                _opponentName  = _opponentIsAI
                    ? (hit.GetComponent<Enemies.EnemyBase>()?.GetType().Name ?? "Enemigo")
                    : $"Jugador [{hit.gameObject.name}]";

                if (CurrentState == State.Idle)
                {
                    TransitionTo(State.Detecting);
                    EventBus.TriggerChallengeDetected(_opponent, _opponentName);
                }
                return;
            }

            // Nada en rango: si estábamos detectando, volver a Idle
            if (CurrentState == State.Detecting)
                TransitionTo(State.Idle);
        }

        private bool IsOpponentInRange()
        {
            if (_opponent == null) return false;
            var player = PlayerCellExterior.Instance;
            if (player == null) return false;
            return Vector2.Distance(player.transform.position, _opponent.transform.position)
                   <= challengeRadius * 1.3f; // margen de tolerancia
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Pending

        private void TickPending()
        {
            _pendingTimer -= Time.deltaTime;
            if (_pendingTimer <= 0f)
            {
                // Timeout: el oponente no aceptó → el jugador huyó por defecto
                ApplyFleePenalty();
                ResolveChallenge(ChallengeResult.Fled);
            }
        }

        /// <summary>
        /// Llamado por NetworkChallengeMediator (online) o automáticamente vs IA.
        /// </summary>
        public void AcceptChallenge()
        {
            if (CurrentState != State.Pending) return;
            TransitionTo(State.Active);
            EventBus.TriggerChallengeStarted(_opponentName);
        }

        /// <summary>
        /// El oponente rechazó el desafío (solo online).
        /// </summary>
        public void RejectChallenge()
        {
            if (CurrentState != State.Pending) return;
            TransitionTo(State.Idle);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Huida

        // DEPRECATED v4.6 — F liberado en GDD v3.
        private void HandleFleeInput()
        {
            // if (!Input.GetKeyDown(Core.SettingsController.GetKey(Core.GameAction.Flee))) return;
            // Flee();
        }

        public void Flee()
        {
            if (CurrentState != State.Pending && CurrentState != State.Active) return;
            ApplyFleePenalty();
            ResolveChallenge(ChallengeResult.Fled);
            EventBus.TriggerChallengeFled();
        }

        private void ApplyFleePenalty()
        {
            StressSystem.Instance?.AddStress(StressFactor.MechanicalDamage, fleeStressPenalty);

            // Knockback alejándose del oponente
            var player = PlayerCellExterior.Instance;
            if (player != null && _opponent != null)
            {
                Vector2 dir = ((Vector2)player.transform.position
                               - (Vector2)_opponent.transform.position).normalized;
                player.transform.position += (Vector3)(dir * fleeKnockback);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Resolución

        /// <summary>
        /// Llamado por CombatMinigame (Prompt 5.2) cuando el combate termina,
        /// o internamente por huida/timeout.
        /// </summary>
        public void ResolveChallenge(ChallengeResult result)
        {
            if (CurrentState == State.Resolved) return;
            _pendingResult = result;
            TransitionTo(State.Resolved);
            StartCoroutine(ResolvedRoutine(result));
        }

        private IEnumerator ResolvedRoutine(ChallengeResult result)
        {
            EventBus.TriggerChallengeEnded(result);

            // Aplicar consecuencias
            ApplyResultConsequences(result);

            yield return new WaitForSeconds(resultDisplayTime);

            _opponent     = null;
            _opponentName = "";
            TransitionTo(State.Idle);
        }

        private void ApplyResultConsequences(ChallengeResult result)
        {
            var rm = ResourceManager.Instance;
            var ss = StressSystem.Instance;

            switch (result)
            {
                case ChallengeResult.Victory:
                    // Recompensa: ATP + AminoAcids del oponente derrotado
                    rm?.Produce(ResourceType.ATP,        30f);
                    rm?.Produce(ResourceType.AminoAcids, 15f);
                    ss?.ReduceStress(10f);
                    break;

                case ChallengeResult.Defeat:
                    // Penalización: daño a HP de membrana + estrés
                    CellState.Instance?.TakeDamage(20f);
                    ss?.AddStress(StressFactor.MechanicalDamage, 20f);
                    break;

                case ChallengeResult.Draw:
                    // Ninguna parte gana recursos; estrés leve
                    ss?.AddStress(StressFactor.MechanicalDamage, 5f);
                    break;

                case ChallengeResult.Fled:
                    // Penalización ya aplicada en ApplyFleePenalty()
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Transiciones de estado

        private void TransitionTo(State next)
        {
            State prev = CurrentState;
            CurrentState = next;

            OnExitState(prev);
            OnEnterState(next);
        }

        private void OnEnterState(State s)
        {
            switch (s)
            {
                case State.Detecting:
                    break;

                case State.Pending:
                    _pendingTimer = acceptTimeout;
                    if (_opponentIsAI)
                        StartCoroutine(AIAcceptRoutine());
                    // Modo online: NetworkChallengeMediator enviará la invitación
                    break;

                case State.Active:
                    // CombatMinigame escucha OnChallengeStarted y se inicializa
                    break;

                case State.Resolved:
                    break;
            }
        }

        private void OnExitState(State s)
        {
            // Nada específico por ahora
        }

        private IEnumerator AIAcceptRoutine()
        {
            yield return new WaitForSeconds(aiAcceptDelay);
            if (CurrentState == State.Pending)
                AcceptChallenge();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Inicia la fase Pending para el oponente actual (llamado externamente
        /// por zona de encuentro o trigger de colisión).
        /// </summary>
        public void InitiateChallenge(GameObject opponent, bool isAI = true)
        {
            if (CurrentState != State.Idle && CurrentState != State.Detecting) return;

            _opponent     = opponent;
            _opponentIsAI = isAI;
            _opponentName = isAI
                ? (opponent.GetComponent<Enemies.EnemyBase>()?.GetType().Name ?? "Enemigo")
                : opponent.name;

            TransitionTo(State.Pending);
            EventBus.TriggerChallengeDetected(opponent, _opponentName);
        }

        /// <summary>Nombre del oponente actual (vacío si Idle).</summary>
        public string OpponentName  => _opponentName;

        /// <summary>Referencia al GameObject del oponente (null si Idle).</summary>
        public GameObject Opponent  => _opponent;

        /// <summary>True si el oponente es IA (false = jugador online).</summary>
        public bool OpponentIsAI    => _opponentIsAI;

        /// <summary>Progreso del timer de aceptación (0-1, solo en estado Pending).</summary>
        public float PendingProgress =>
            CurrentState == State.Pending ? _pendingTimer / acceptTimeout : 0f;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Eventos externos

        private void OnCellDeath()
        {
            // Si la célula muere durante un desafío, registrar derrota
            if (CurrentState == State.Active || CurrentState == State.Pending)
                ResolveChallenge(ChallengeResult.Defeat);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Gizmos debug

        private void OnDrawGizmosSelected()
        {
            var player = PlayerCellExterior.Instance;
            if (player == null) return;

            Gizmos.color = CurrentState switch
            {
                State.Idle      => new Color(0.3f, 0.3f, 0.8f, 0.2f),
                State.Detecting => new Color(0.9f, 0.8f, 0.1f, 0.3f),
                State.Pending   => new Color(0.9f, 0.5f, 0.1f, 0.3f),
                State.Active    => new Color(0.9f, 0.1f, 0.1f, 0.4f),
                _               => new Color(0.2f, 0.9f, 0.2f, 0.2f),
            };
            Gizmos.DrawWireSphere(player.transform.position, challengeRadius);
        }

        #endregion
    }
}
