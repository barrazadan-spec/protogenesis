using System.Collections;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Views;
using Protogenesis.Slots;
using Protogenesis.Progression;

namespace Protogenesis.Online
{
    /// <summary>
    /// CombatMinigame — Mecánicas del encuentro 1v1 en tiempo real (Primordia, Prompt 5.2).
    ///
    /// Se activa al recibir OnChallengeStarted y termina llamando a
    /// ChallengeManager.ResolveChallenge() cuando se cumple una condición.
    ///
    /// Estructura del combate (30 segundos máximo):
    ///   · Ambos lados se atacan automáticamente cada autoAttackInterval segundos.
    ///   · El jugador dispone de 3 acciones activas con cooldown:
    ///       [Space] Ráfaga ATP  — ×3 DPS durante burstDuration, cuesta burstCost ATP
    ///       [Q]     Defensa     — −50% daño entrante durante defendDuration, cuesta defendCost ATP
    ///       [E]     Toxina      — envenena al oponente, cuesta toxinCost AminoAcids
    ///
    ///   · Especializaciones aplican bonificadores automáticos al daño y defensa.
    ///   · El oponente IA alterna acciones básicas según su HP restante.
    ///
    /// Condiciones de victoria:
    ///   HP oponente ≤ 0            → ChallengeResult.Victory
    ///   HP jugador ≤ 0             → ChallengeResult.Defeat   (CellState ya dispara OnCellDeath)
    ///   Timer agotado              → ChallengeResult.Draw
    ///
    /// El daño al jugador se aplica a CellState (HP global compartido con ambas vistas).
    /// El HP del oponente es local a este sistema (no persiste).
    /// </summary>
    public class CombatMinigame : MonoBehaviour
    {
        public static CombatMinigame Instance { get; private set; }

        // ── Configuración ─────────────────────────────────────────────────────────
        [Header("Duración")]
        [SerializeField] private float combatDuration      = 30f;

        [Header("Auto-ataque")]
        [SerializeField] private float autoAttackInterval  = 1.0f;
        [SerializeField] private float playerBaseDPS       = 5f;
        [SerializeField] private float opponentBaseDPS     = 4f;

        [Header("Oponente IA")]
        [SerializeField] private float opponentBaseHP      = 60f;
        [Tooltip("HP del oponente escala con la era actual.")]
        [SerializeField] private float opponentHPPerEra    = 20f;

        [Header("Acciones — Ráfaga ATP")]
        [SerializeField] private float burstCost           = 20f;
        [SerializeField] private float burstMultiplier     = 3f;
        [SerializeField] private float burstDuration       = 1.2f;
        [SerializeField] private float burstCooldown       = 4f;

        [Header("Acciones — Defensa")]
        [SerializeField] private float defendCost          = 10f;
        [SerializeField] private float defendDamageReduct  = 0.50f;
        [SerializeField] private float defendDuration      = 2.0f;
        [SerializeField] private float defendCooldown      = 5f;

        [Header("Acciones — Toxina")]
        [SerializeField] private float toxinCost           = 15f;
        [SerializeField] private float toxinDPS            = 3f;
        [SerializeField] private float toxinDuration       = 4f;
        [SerializeField] private float toxinCooldown       = 7f;

        // ── Estado del combate ────────────────────────────────────────────────────
        public bool IsActive { get; private set; } = false;

        private float _timer          = 0f;  // tiempo restante
        private float _opponentHP     = 0f;
        private float _opponentMaxHP  = 0f;

        // Acciones del jugador
        private bool  _isBursting     = false;
        private float _burstTimer     = 0f;
        private float _burstCDTimer   = 0f;

        private bool  _isDefending    = false;
        private float _defendTimer    = 0f;
        private float _defendCDTimer  = 0f;

        private bool  _toxinActive    = false;
        private float _toxinTimer     = 0f;
        private float _toxinCDTimer   = 0f;

        // Auto-ataque
        private float _autoAttackTimer = 0f;

        // IA del oponente
        private float _aiActionTimer   = 0f;
        private const float AIActionInterval = 3.5f;
        private bool  _aiDefending     = false;
        private float _aiDefendTimer   = 0f;

        // Multiplicadores de especialización (se calculan al inicio)
        private float _specDamageBonus = 1f;
        private float _specDefBonus    = 0f;

        // ── OnGUI styles ──────────────────────────────────────────────────────────
        private GUIStyle _panelStyle;
        private GUIStyle _titleStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _btnStyle;
        private GUIStyle _btnCoolStyle;
        private bool     _stylesReady = false;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnChallengeStarted += OnChallengeStarted;
            EventBus.OnCellDeath        += OnCellDeath;
        }

        private void OnDisable()
        {
            EventBus.OnChallengeStarted -= OnChallengeStarted;
            EventBus.OnCellDeath        -= OnCellDeath;
        }

        private void Update()
        {
            if (!IsActive) return;

            float dt = Time.deltaTime;

            TickTimer(dt);
            TickCooldowns(dt);
            TickBurst(dt);
            TickDefend(dt);
            TickToxin(dt);
            TickAutoAttack(dt);
            TickAI(dt);
            HandlePlayerInput();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Inicio / Fin

        private void OnChallengeStarted(string opponentName)
        {
            StartCombat();
        }

        private void StartCombat()
        {
            IsActive = true;
            _timer   = combatDuration;

            // HP del oponente escalado por era
            int era = GameManager.Instance != null ? GameManager.Instance.CurrentEra : 0;
            _opponentMaxHP = opponentBaseHP + opponentHPPerEra * era;
            _opponentHP    = _opponentMaxHP;

            // Resetear timers
            _burstCDTimer  = 0f; _defendCDTimer = 0f; _toxinCDTimer = 0f;
            _autoAttackTimer = 0f;
            _aiActionTimer   = AIActionInterval * 0.5f; // IA actúa a mitad del primer ciclo
            _isBursting = _isDefending = _toxinActive = false;
            _aiDefending = false;

            // Calcular bonificadores de especialización
            CalcSpecBonuses();

            Debug.Log($"[CombatMinigame] Combate iniciado. OpponentHP={_opponentMaxHP}");
        }

        private void EndCombat(ChallengeResult result)
        {
            if (!IsActive) return;
            IsActive = false;
            ChallengeManager.Instance?.ResolveChallenge(result);
            Debug.Log($"[CombatMinigame] Combate terminado: {result}");
        }

        private void OnCellDeath()
        {
            if (IsActive) EndCombat(ChallengeResult.Defeat);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Tick

        private void TickTimer(float dt)
        {
            _timer -= dt;
            if (_timer <= 0f) EndCombat(ChallengeResult.Draw);
        }

        private void TickCooldowns(float dt)
        {
            if (_burstCDTimer  > 0f) _burstCDTimer  -= dt;
            if (_defendCDTimer > 0f) _defendCDTimer -= dt;
            if (_toxinCDTimer  > 0f) _toxinCDTimer  -= dt;
        }

        private void TickBurst(float dt)
        {
            if (!_isBursting) return;
            _burstTimer -= dt;
            if (_burstTimer <= 0f) _isBursting = false;
        }

        private void TickDefend(float dt)
        {
            if (!_isDefending) return;
            _defendTimer -= dt;
            if (_defendTimer <= 0f) _isDefending = false;
        }

        private void TickToxin(float dt)
        {
            if (!_toxinActive) return;
            _toxinTimer -= dt;

            // Daño de toxina al oponente
            float dmg = toxinDPS * dt;
            DamageOpponent(dmg);

            if (_toxinTimer <= 0f) _toxinActive = false;
        }

        private void TickAutoAttack(float dt)
        {
            _autoAttackTimer -= dt;
            if (_autoAttackTimer > 0f) return;
            _autoAttackTimer = autoAttackInterval;

            // Ataque del jugador al oponente
            float playerDmg = playerBaseDPS * _specDamageBonus;
            if (_isBursting) playerDmg *= burstMultiplier;
            if (_aiDefending) playerDmg *= 0.5f;
            DamageOpponent(playerDmg);

            // Ataque del oponente al jugador
            float opponentDmg = opponentBaseDPS;
            float reduction   = _specDefBonus;
            if (_isDefending) reduction += defendDamageReduct;
            opponentDmg *= (1f - Mathf.Clamp01(reduction));
            DamagePlayer(opponentDmg);
        }

        private void TickAI(float dt)
        {
            if (_aiDefending)
            {
                _aiDefendTimer -= dt;
                if (_aiDefendTimer <= 0f) _aiDefending = false;
            }

            _aiActionTimer -= dt;
            if (_aiActionTimer > 0f) return;
            _aiActionTimer = AIActionInterval;

            float hpPct = _opponentHP / _opponentMaxHP;

            // Lógica simple: HP bajo → defender más; HP alto → atacar
            if (hpPct < 0.35f && !_aiDefending)
            {
                _aiDefending   = true;
                _aiDefendTimer = defendDuration * 1.5f;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Input del jugador

        // DEPRECATED v4.6 — Combate PvP desactivado en GDD v3. Space/Q/E liberados.
        private void HandlePlayerInput()
        {
            // if (Input.GetKeyDown(Core.SettingsController.GetKey(Core.GameAction.BurstAttack)))  TryBurst();
            // if (Input.GetKeyDown(Core.SettingsController.GetKey(Core.GameAction.Defend)))       TryDefend();
            // if (Input.GetKeyDown(Core.SettingsController.GetKey(Core.GameAction.ToxinAttack)))  TryToxin();
        }

        private void TryBurst()
        {
            if (_isBursting || _burstCDTimer > 0f) return;
            var rm = ResourceManager.Instance;
            if (rm == null || !rm.Consume(ResourceType.ATP, burstCost)) return;

            _isBursting   = true;
            _burstTimer   = burstDuration;
            _burstCDTimer = burstCooldown;
        }

        private void TryDefend()
        {
            if (_isDefending || _defendCDTimer > 0f) return;
            var rm = ResourceManager.Instance;
            if (rm == null || !rm.Consume(ResourceType.ATP, defendCost)) return;

            _isDefending   = true;
            _defendTimer   = defendDuration;
            _defendCDTimer = defendCooldown;
        }

        private void TryToxin()
        {
            if (_toxinActive || _toxinCDTimer > 0f) return;
            var rm = ResourceManager.Instance;
            if (rm == null || !rm.Consume(ResourceType.AminoAcids, toxinCost)) return;

            _toxinActive  = true;
            _toxinTimer   = toxinDuration;
            _toxinCDTimer = toxinCooldown;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Daño

        private void DamageOpponent(float amount)
        {
            if (amount <= 0f) return;
            _opponentHP = Mathf.Max(0f, _opponentHP - amount);
            if (_opponentHP <= 0f) EndCombat(ChallengeResult.Victory);
        }

        private void DamagePlayer(float amount)
        {
            if (amount <= 0f) return;
            // CellState.TakeDamage dispara OnCellDeath si HP llega a 0
            CellState.Instance?.TakeDamage(amount);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Especialización

        private void CalcSpecBonuses()
        {
            _specDamageBonus = 1f;
            _specDefBonus    = 0f;

            var cs = CellState.Instance;
            if (cs == null || !cs.IsSpecializationConsolidated) return;

            switch (cs.ConsolidatedSpecialization)
            {
                // TODO v3: reemplazar con tipos biológicos del GDD v3
                // case SpecializationType.Fagotrofa:   → Ameba, Macrofago (+30% daño)
                // case SpecializationType.Flagelada:   → Flagelado (+30% daño)
                // case SpecializationType.CorazaQuitinosa: → Arquea (-25% daño recibido)
                // case SpecializationType.Enquistada:  → Tardigrado (-25% daño recibido)
                case SpecializationType.Ameba:
                case SpecializationType.Flagelado:
                    _specDamageBonus = 1.30f;   // +30% daño
                    break;
                case SpecializationType.Arquea:
                // TODO v4.2 — Tardigrado eliminado como especialización
                    _specDefBonus    = 0.25f;   // -25% daño recibido
                    break;
                case SpecializationType.Macrofago:
                    _specDamageBonus = 1.15f;
                    _specDefBonus    = 0.10f;
                    break;
                default:
                    break;
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>HP actual del oponente (0-OpponentMaxHP).</summary>
        public float OpponentHP    => _opponentHP;
        public float OpponentMaxHP => _opponentMaxHP;
        public float OpponentHPNormalized => _opponentMaxHP > 0f ? _opponentHP / _opponentMaxHP : 0f;

        /// <summary>Tiempo restante del combate (0-combatDuration).</summary>
        public float TimeRemaining => _timer;
        public float TimerNormalized => _timer / combatDuration;

        // Cooldowns normalizados (0 = listo, 1 = recién usado)
        public float BurstCDNorm  => _burstCDTimer  / burstCooldown;
        public float DefendCDNorm => _defendCDTimer / defendCooldown;
        public float ToxinCDNorm  => _toxinCDTimer  / toxinCooldown;

        public bool IsBursting   => _isBursting;
        public bool IsDefending  => _isDefending;
        public bool IsToxinActive=> _toxinActive;
        public bool AIDefending  => _aiDefending;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI

        private void OnGUI()
        {
            if (!IsActive) return;
            if (!_stylesReady) InitStyles();

            float sw = Screen.width;
            float sh = Screen.height;

            // ── Panel central (320 × 260) ───────────────────────────────────────
            float pw = 320f, ph = 260f;
            float px = (sw - pw) * 0.5f;
            float py = sh * 0.12f;

            GUILayout.BeginArea(new Rect(px, py, pw, ph), _panelStyle);

            // Timer
            float tPct = TimerNormalized;
            Color timerCol = tPct > 0.5f
                ? Color.white
                : tPct > 0.25f ? new Color(1f, 0.8f, 0.1f) : new Color(1f, 0.2f, 0.2f);
            DrawColorLabel($"⏱ {_timer:F1} s", timerCol, _titleStyle);

            GUILayout.Space(4f);

            // HP del oponente
            var cm = ChallengeManager.Instance;
            string oppName = cm != null ? cm.OpponentName : "Oponente";
            DrawColorLabel(oppName, new Color(0.9f, 0.4f, 0.4f), _labelStyle);
            DrawBar(OpponentHPNormalized, new Color(0.85f, 0.2f, 0.2f), 14f);
            GUILayout.Label($"  {_opponentHP:F0} / {_opponentMaxHP:F0} HP"
                           + (_aiDefending ? "  [DEFENDIENDO]" : ""), _labelStyle);

            GUILayout.Space(6f);

            // HP del jugador
            var cs = CellState.Instance;
            float pHP  = cs != null ? cs.CurrentHP  : 0f;
            float pMax = cs != null ? cs.MaxHP       : 100f;
            DrawColorLabel("Tu célula", new Color(0.3f, 0.8f, 0.4f), _labelStyle);
            DrawBar(pMax > 0f ? pHP / pMax : 0f, new Color(0.2f, 0.8f, 0.3f), 14f);
            GUILayout.Label($"  {pHP:F0} / {pMax:F0} HP", _labelStyle);

            GUILayout.Space(8f);

            // Acciones
            GUILayout.BeginHorizontal();
            DrawAction($"[{Core.SettingsController.GetKey(Core.GameAction.BurstAttack)}] Ráfaga\n({burstCost} ATP)",
                _isBursting, _burstCDTimer, burstCooldown,
                new Color(0.30f, 0.60f, 1.00f));

            DrawAction($"[{Core.SettingsController.GetKey(Core.GameAction.Defend)}] Defensa\n({defendCost} ATP)",
                _isDefending, _defendCDTimer, defendCooldown,
                new Color(0.20f, 0.85f, 0.45f));

            DrawAction($"[{Core.SettingsController.GetKey(Core.GameAction.ToxinAttack)}] Toxina\n({toxinCost} AA)",
                _toxinActive, _toxinCDTimer, toxinCooldown,
                new Color(0.75f, 0.30f, 0.80f));
            GUILayout.EndHorizontal();

            GUILayout.EndArea();
        }

        private void DrawAction(string label, bool active, float cdTimer, float cdMax, Color color)
        {
            bool onCD = cdTimer > 0f;
            float cdPct = Mathf.Clamp01(cdTimer / cdMax);

            Color prev = GUI.color;
            GUI.color = active ? color : onCD ? new Color(0.4f, 0.4f, 0.4f) : Color.white;
            GUILayout.BeginVertical(_btnCoolStyle, GUILayout.Width(90f));

            GUI.color = active ? color : onCD ? new Color(0.5f, 0.5f, 0.5f) : Color.white;
            GUILayout.Label(label, _labelStyle);

            // Barra de cooldown
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
                         GUILayout.Height(6f), GUILayout.ExpandWidth(true));
            DrawBarRect(r, 1f - cdPct, onCD ? new Color(0.5f, 0.5f, 0.5f) : color);

            if (active)
            {
                GUI.color = color;
                GUILayout.Label("ACTIVO", _labelStyle);
            }
            else if (onCD)
            {
                GUI.color = new Color(0.6f, 0.6f, 0.6f);
                GUILayout.Label($"{cdTimer:F1}s", _labelStyle);
            }

            GUILayout.EndVertical();
            GUI.color = prev;
        }

        private void DrawBar(float t, Color fill, float h)
        {
            Rect r = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none,
                         GUILayout.Height(h), GUILayout.ExpandWidth(true));
            DrawBarRect(r, t, fill);
        }

        private static void DrawBarRect(Rect r, float t, Color fill)
        {
            GUI.color = new Color(0.10f, 0.12f, 0.16f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            if (t > 0f)
            {
                Color c = fill; c.a = 0.85f;
                GUI.color = c;
                GUI.DrawTexture(new Rect(r.x, r.y, r.width * t, r.height), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
        }

        private void DrawColorLabel(string text, Color color, GUIStyle style)
        {
            Color prev = GUI.color;
            GUI.color = color;
            GUILayout.Label(text, style);
            GUI.color = prev;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Styles

        private void InitStyles()
        {
            _panelStyle = new GUIStyle(GUI.skin.box)
            {
                padding  = new RectOffset(10, 10, 8, 8),
                fontSize = 12
            };
            _panelStyle.normal.background = MakeTex(new Color(0.03f, 0.05f, 0.12f, 0.95f));

            _titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 14,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            _titleStyle.normal.textColor = Color.white;

            _labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            _labelStyle.normal.textColor = Color.white;

            _btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 11 };

            _btnCoolStyle = new GUIStyle(GUI.skin.box)
            {
                padding  = new RectOffset(4, 4, 4, 4),
                fontSize = 10
            };
            _btnCoolStyle.normal.background = MakeTex(new Color(0.08f, 0.10f, 0.18f, 0.9f));

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
    }
}
