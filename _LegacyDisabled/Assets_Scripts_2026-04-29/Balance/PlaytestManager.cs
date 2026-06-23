using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Online;
using Protogenesis.Victory;
using Protogenesis.Views;

namespace Protogenesis.Balance
{
    /// <summary>
    /// PlaytestManager — Prompt 5.5 / GDD v3 Primordia
    ///
    /// Herramienta de balance y playtest para el mini-juego 1v1.
    /// NO forma parte del build de producción — solo activo en UNITY_EDITOR o
    /// cuando DEBUG_PLAYTEST está definido.
    ///
    /// RESPONSABILIDADES:
    ///
    ///   1. VALIDACIÓN DE SISTEMAS (Start)
    ///      Comprueba que todos los singletons críticos del mini-juego están
    ///      presentes en escena. Emite advertencias por consola si alguno falta.
    ///
    ///   2. MÉTRICAS DE SESIÓN
    ///      Acumula por cada combate: resultado, duración, daño infligido/recibido,
    ///      ATP gastado. Al final de la sesión genera un resumen de balance.
    ///
    ///   3. HUD DE DEBUG (tecla F1)
    ///      Panel overlay en tiempo real con stats del jugador, rival, sistemas
    ///      activos y alertas de balance. Sólo visible cuando _hudVisible = true.
    ///
    ///   4. ANÁLISIS DE BALANCE AUTOMÁTICO
    ///      Tras cada combate compara métricas contra rangos esperados y emite
    ///      recomendaciones de ajuste al log (sin pausar el juego).
    ///
    /// TECLAS:
    ///   F1  — togglear HUD de debug
    ///   F2  — volcar resumen de sesión al log ahora
    /// </summary>
    public class PlaytestManager : MonoBehaviour
    {
        public static PlaytestManager Instance { get; private set; }

        // ── Rangos de balance esperados ───────────────────────────────────────────
        [Header("Rangos de balance esperados")]
        [Tooltip("Duración mínima esperada de un combate normal (segundos).")]
        [SerializeField] private float expectedMinDuration    = 8f;
        [Tooltip("Duración máxima esperada de un combate normal (segundos).")]
        [SerializeField] private float expectedMaxDuration    = 45f;
        [Tooltip("Daño al rival mínimo esperado en una victoria por eliminación (%).")]
        [SerializeField] private float expectedMinDamageDealt = 40f;
        [Tooltip("HP perdido máximo esperado en una victoria (%).")]
        [SerializeField] private float expectedMaxHPLostOnWin = 70f;

        // ── Estado de sesión ──────────────────────────────────────────────────────
        private float       _sessionStart;
        private int         _challengesTotal;
        private int         _challengesWon;
        private int         _challengesLost;
        private int         _challengesFled;
        private float       _totalDamageDealtPct;   // suma de % de HP del rival eliminado
        private float       _totalHPLostPct;        // suma de % de HP propio perdido
        private float       _totalATPSpentPct;
        private float       _totalDuration;
        private float       _peakATP;
        private float       _lowestHPSeen = 1f;

        // Combate actual (snapshot al inicio para calcular deltas)
        private float _matchStartPlayerHP;
        private float _matchStartPlayerATP;
        private float _matchStartTime;
        private bool  _inMatch;

        // Log de combates
        private readonly List<MatchRecord> _matchLog = new List<MatchRecord>();

        // HUD
        private bool  _hudVisible   = false;
        private bool  _stylesReady  = false;
        private GUIStyle _hudBg, _hudLabel, _hudAlert, _hudTitle, _hudOk;

        // ── Advertencias de validación ────────────────────────────────────────────
        private readonly List<string> _validationWarnings = new List<string>();

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start()
        {
            _sessionStart = Time.time;
            ValidateSystems();
        }

        private void OnEnable()
        {
            EventBus.OnChallengeStarted += OnChallengeStarted;
            EventBus.OnChallengeEnded   += OnChallengeEnded;
        }

        private void OnDisable()
        {
            EventBus.OnChallengeStarted -= OnChallengeStarted;
            EventBus.OnChallengeEnded   -= OnChallengeEnded;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F1)) _hudVisible = !_hudVisible;
            if (Input.GetKeyDown(KeyCode.F2)) DumpSessionLog();

            // Rastrear pico de ATP y mínimo de HP en tiempo real
            if (ResourceManager.Instance != null)
            {
                float atp = ResourceManager.Instance.GetPercent(ResourceType.ATP);
                if (atp > _peakATP) _peakATP = atp;
            }
            if (CellState.Instance != null)
            {
                float hp = CellState.Instance.HPNormalized;
                if (hp < _lowestHPSeen && hp > 0f) _lowestHPSeen = hp;
            }
        }

        private void OnDestroy()
        {
            DumpSessionLog();
        }

        private void OnGUI()
        {
            if (!_hudVisible) return;
            if (!_stylesReady) InitStyles();
            DrawHUD();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Validación de sistemas

        private void ValidateSystems()
        {
            Check(PlayerCellExterior.Instance      != null, "PlayerCellExterior");
            Check(ChallengeManager.Instance        != null, "ChallengeManager");
            Check(VictoryManager.Instance          != null, "VictoryManager");
            Check(ResourceManager.Instance         != null, "ResourceManager");
            Check(CellState.Instance               != null, "CellState");
            Check(MembraneSegmentSystem.Instance   != null, "MembraneSegmentSystem");
            Check(ChemicalGradientSystem.Instance  != null, "ChemicalGradientSystem");
            Check(RivalCellController.AllRivals.Count > 0,  "RivalCellController (>0 en escena)");

            // FagocytosisSystem vive en el player
            if (PlayerCellExterior.Instance != null)
                Check(PlayerCellExterior.Instance.GetComponent<FagocytosisSystem>() != null,
                      "FagocytosisSystem (en PlayerCellExterior)");

            if (_validationWarnings.Count == 0)
            {
                Debug.Log("[PlaytestManager] ✓ Todos los sistemas del mini-juego validados.");
            }
            else
            {
                foreach (var w in _validationWarnings)
                    Debug.LogWarning($"[PlaytestManager] FALTA: {w}");
            }
        }

        private void Check(bool condition, string systemName)
        {
            if (!condition)
                _validationWarnings.Add(systemName);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Tracking de combate

        private void OnChallengeStarted(string opponentName)
        {
            _inMatch              = true;
            _matchStartTime       = Time.time;
            _matchStartPlayerHP   = CellState.Instance != null
                                    ? CellState.Instance.HPNormalized : 1f;
            _matchStartPlayerATP  = ResourceManager.Instance != null
                                    ? ResourceManager.Instance.GetPercent(ResourceType.ATP) : 1f;
        }

        private void OnChallengeEnded(ChallengeResult result)
        {
            if (!_inMatch) return;
            _inMatch = false;

            float duration   = Time.time - _matchStartTime;
            float playerHPNow  = CellState.Instance != null
                                 ? CellState.Instance.HPNormalized : 1f;
            float playerATPNow = ResourceManager.Instance != null
                                 ? ResourceManager.Instance.GetPercent(ResourceType.ATP) : 1f;

            float hpLostPct   = Mathf.Clamp01(_matchStartPlayerHP  - playerHPNow)   * 100f;
            float atpSpentPct = Mathf.Clamp01(_matchStartPlayerATP - playerATPNow) * 100f;

            // Daño al rival: leer de VictoryManager si rastreó al rival
            float damageDealtPct = 0f;
            var rival = VictoryManager.Instance?.TrackedRival;
            if (rival != null)
                damageDealtPct = Mathf.Clamp01(1f - rival.HPNormalized) * 100f;
            else if (result == ChallengeResult.Victory)
                damageDealtPct = 100f;

            // Acumular métricas
            _challengesTotal++;
            switch (result)
            {
                case ChallengeResult.Victory: _challengesWon++;  break;
                case ChallengeResult.Defeat:  _challengesLost++; break;
                case ChallengeResult.Fled:    _challengesFled++; break;
            }
            _totalDamageDealtPct += damageDealtPct;
            _totalHPLostPct      += hpLostPct;
            _totalATPSpentPct    += atpSpentPct;
            _totalDuration       += duration;

            var record = new MatchRecord
            {
                result        = result,
                duration      = duration,
                damageDealtPct = damageDealtPct,
                hpLostPct     = hpLostPct,
                atpSpentPct   = atpSpentPct,
            };
            _matchLog.Add(record);

            AnalyzeBalance(record);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Análisis de balance

        private void AnalyzeBalance(MatchRecord r)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[PlaytestManager] Combate #{_challengesTotal} — {r.result}  ({r.duration:F1}s)");

            bool ok = true;

            if (r.duration < expectedMinDuration)
            {
                sb.AppendLine($"  ⚠ Combate muy corto ({r.duration:F1}s < {expectedMinDuration}s). " +
                              "Considera aumentar HP del rival o reducir attackDamage de EnemyCellAI.");
                ok = false;
            }
            if (r.duration > expectedMaxDuration)
            {
                sb.AppendLine($"  ⚠ Combate muy largo ({r.duration:F1}s > {expectedMaxDuration}s). " +
                              "Considera reducir HP del rival o aumentar attackDamage del jugador.");
                ok = false;
            }

            if (r.result == ChallengeResult.Victory && r.damageDealtPct < expectedMinDamageDealt)
            {
                sb.AppendLine($"  ⚠ Victoria con poco daño al rival ({r.damageDealtPct:F0}%). " +
                              "Posible victoria por inanición muy rápida. Revisa starvationTime en VictoryManager.");
                ok = false;
            }

            if (r.result == ChallengeResult.Victory && r.hpLostPct > expectedMaxHPLostOnWin)
            {
                sb.AppendLine($"  ⚠ Victoria con mucho daño recibido ({r.hpLostPct:F0}%). " +
                              "Rival demasiado agresivo o counterDamage de EnemyCellAI muy alto.");
                ok = false;
            }

            if (r.result == ChallengeResult.Defeat && r.atpSpentPct < 20f)
            {
                sb.AppendLine($"  ⚠ Derrota con ATP casi intacto ({r.atpSpentPct:F0}% gastado). " +
                              "El jugador perdió sin gastar energía — posible desequilibrio en daño de membrana.");
                ok = false;
            }

            if (ok)
                sb.AppendLine("  ✓ Métricas dentro de rango esperado.");

            Debug.Log(sb.ToString());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Resumen de sesión

        private void DumpSessionLog()
        {
            if (_challengesTotal == 0)
            {
                Debug.Log("[PlaytestManager] Sesión sin combates registrados.");
                return;
            }

            float sessionDuration = Time.time - _sessionStart;
            float winRate         = _challengesTotal > 0
                                    ? _challengesWon * 100f / _challengesTotal : 0f;
            float avgDuration     = _totalDuration    / _challengesTotal;
            float avgDamage       = _totalDamageDealtPct / _challengesTotal;
            float avgHPLost       = _totalHPLostPct    / _challengesTotal;
            float avgATPSpent     = _totalATPSpentPct  / _challengesTotal;

            var sb = new StringBuilder();
            sb.AppendLine("══════════════════════════════════════════════════");
            sb.AppendLine("[PlaytestManager] RESUMEN DE SESIÓN");
            sb.AppendLine($"  Tiempo de sesión:    {sessionDuration / 60f:F1} min");
            sb.AppendLine($"  Combates totales:    {_challengesTotal}");
            sb.AppendLine($"  Win rate:            {winRate:F0}%  " +
                          $"(V:{_challengesWon}  D:{_challengesLost}  H:{_challengesFled})");
            sb.AppendLine($"  Duración promedio:   {avgDuration:F1}s");
            sb.AppendLine($"  Daño al rival prom.: {avgDamage:F0}%");
            sb.AppendLine($"  HP perdido prom.:    {avgHPLost:F0}%");
            sb.AppendLine($"  ATP gastado prom.:   {avgATPSpent:F0}%");
            sb.AppendLine($"  Pico ATP sesión:     {_peakATP * 100f:F0}%");
            sb.AppendLine($"  Mínimo HP sesión:    {_lowestHPSeen * 100f:F0}%");
            sb.AppendLine("──────────────────────────────────────────────────");

            // Recomendaciones globales
            if (winRate > 80f)
                sb.AppendLine("  ⚠ Win rate alto (>80%). El rival puede ser poco desafiante.");
            if (winRate < 30f)
                sb.AppendLine("  ⚠ Win rate bajo (<30%). El rival puede ser demasiado difícil.");
            if (avgDuration < expectedMinDuration)
                sb.AppendLine("  ⚠ Combates demasiado cortos en promedio. Revisar HP y daño.");
            if (_challengesFled > _challengesWon)
                sb.AppendLine("  ⚠ Más huidas que victorias. Evaluar si el flee penalty disuade suficiente.");

            if (_validationWarnings.Count > 0)
            {
                sb.AppendLine("  ⚠ Sistemas ausentes en validación inicial:");
                foreach (var w in _validationWarnings)
                    sb.AppendLine($"    - {w}");
            }

            sb.AppendLine("══════════════════════════════════════════════════");
            Debug.Log(sb.ToString());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region HUD de Debug (OnGUI)

        private void DrawHUD()
        {
            const float hudW = 280f;
            float hudH = 340f + _validationWarnings.Count * 16f;
            float x    = Screen.width - hudW - 8f;
            float y    = 8f;

            // Fondo
            GUI.color = new Color(0f, 0f, 0f, 0.75f);
            GUI.DrawTexture(new Rect(x, y, hudW, hudH), Texture2D.whiteTexture);
            GUI.color = new Color(0.3f, 0.8f, 0.4f, 0.5f);
            GUI.DrawTexture(new Rect(x, y, hudW, 1f), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float ly = y + 6f;

            Label(ref ly, x, hudW, "── PLAYTEST HUD  [F1] ──", _hudTitle);
            ly += 2f;

            // ── Player ───────────────────────────────────────────────────────────
            Label(ref ly, x, hudW, "JUGADOR", _hudTitle);
            var cs  = CellState.Instance;
            var rm  = ResourceManager.Instance;
            LabelPair(ref ly, x, hudW, "HP",
                      cs  != null ? $"{cs.HPNormalized * 100f:F0}%  ({cs.CurrentHP:F0}/{cs.MaxHP:F0})" : "—");
            LabelPair(ref ly, x, hudW, "ATP",
                      rm  != null ? $"{rm.GetPercent(ResourceType.ATP) * 100f:F0}%" : "—");
            LabelPair(ref ly, x, hudW, "ATP neto/s",
                      rm  != null ? $"{rm.GetNetRate(ResourceType.ATP):+0.0;-0.0}" : "—");

            var mss = MembraneSegmentSystem.Instance;
            if (mss != null)
                LabelPair(ref ly, x, hudW, "Membrana", $"{mss.TotalHP:F0} HP total");

            ly += 4f;

            // ── Challenge ────────────────────────────────────────────────────────
            Label(ref ly, x, hudW, "COMBATE", _hudTitle);
            var cm = ChallengeManager.Instance;
            LabelPair(ref ly, x, hudW, "Estado",     cm  != null ? cm.CurrentState.ToString() : "—");
            LabelPair(ref ly, x, hudW, "Oponente",   cm  != null ? (cm.OpponentName != "" ? cm.OpponentName : "—") : "—");

            var rival = VictoryManager.Instance?.TrackedRival;
            if (rival != null)
            {
                LabelPair(ref ly, x, hudW, "Rival HP",  $"{rival.HPNormalized * 100f:F0}%");
                LabelPair(ref ly, x, hudW, "Rival ATP", $"{rival.ATPNormalized * 100f:F0}%");
                LabelPair(ref ly, x, hudW, "Rival",     rival.BehaviorLabel);
            }

            ly += 4f;

            // ── Sesión ───────────────────────────────────────────────────────────
            Label(ref ly, x, hudW, "SESIÓN", _hudTitle);
            LabelPair(ref ly, x, hudW, "Combates",
                      $"{_challengesTotal}  (V:{_challengesWon} D:{_challengesLost} H:{_challengesFled})");
            if (_challengesTotal > 0)
                LabelPair(ref ly, x, hudW, "Win rate",
                          $"{_challengesWon * 100f / _challengesTotal:F0}%");
            LabelPair(ref ly, x, hudW, "Pico ATP",   $"{_peakATP * 100f:F0}%");
            LabelPair(ref ly, x, hudW, "HP mínimo",  $"{_lowestHPSeen * 100f:F0}%");

            ly += 4f;

            // ── Advertencias de validación ────────────────────────────────────────
            if (_validationWarnings.Count > 0)
            {
                Label(ref ly, x, hudW, "SISTEMAS AUSENTES", _hudTitle);
                foreach (var w in _validationWarnings)
                {
                    GUI.color = new Color(1f, 0.4f, 0.1f);
                    GUI.Label(new Rect(x + 6f, ly, hudW - 10f, 15f), $"✗ {w}", _hudAlert);
                    GUI.color = Color.white;
                    ly += 15f;
                }
            }
            else
            {
                GUI.color = new Color(0.3f, 0.9f, 0.4f);
                GUI.Label(new Rect(x + 6f, ly, hudW - 10f, 15f), "✓ Sistemas validados", _hudOk);
                GUI.color = Color.white;
                ly += 16f;
            }

            // Hint F2
            GUI.color = new Color(0.5f, 0.5f, 0.55f);
            GUI.Label(new Rect(x + 6f, ly + 2f, hudW - 10f, 13f),
                      "[F2] Volcar resumen de sesión al log", _hudLabel);
            GUI.color = Color.white;
        }

        // ── Helpers de layout ─────────────────────────────────────────────────────
        private void Label(ref float y, float x, float w, string text, GUIStyle style)
        {
            GUI.Label(new Rect(x + 6f, y, w - 10f, 16f), text, style);
            y += 16f;
        }

        private void LabelPair(ref float y, float x, float w, string key, string value)
        {
            GUI.Label(new Rect(x + 6f,        y, 95f,       14f), key,   _hudLabel);
            GUI.Label(new Rect(x + 6f + 95f,  y, w - 105f, 14f), value, _hudLabel);
            y += 14f;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Styles

        private void InitStyles()
        {
            _hudLabel = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            _hudLabel.normal.textColor = new Color(0.82f, 0.82f, 0.82f);

            _hudTitle = new GUIStyle(GUI.skin.label)
            {
                fontSize  = 11,
                fontStyle = FontStyle.Bold,
            };
            _hudTitle.normal.textColor = new Color(0.55f, 0.85f, 0.95f);

            _hudAlert = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            _hudAlert.normal.textColor = new Color(1f, 0.45f, 0.15f);

            _hudOk = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            _hudOk.normal.textColor = new Color(0.30f, 0.88f, 0.42f);

            _stylesReady = true;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Estructura interna

        private struct MatchRecord
        {
            public ChallengeResult result;
            public float           duration;
            public float           damageDealtPct;
            public float           hpLostPct;
            public float           atpSpentPct;
        }

        #endregion
    }
}
