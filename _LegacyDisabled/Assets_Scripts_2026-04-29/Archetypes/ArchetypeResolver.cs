using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Archetypes
{
    /// <summary>
    /// Evalúa el comportamiento del jugador cada <see cref="evaluationInterval"/> segundos
    /// y determina qué arquetipo emergente corresponde a su estilo de juego actual.
    ///
    /// El arquetipo se selecciona por puntuación: cada ArchetypeDefinition tiene
    /// condiciones de activación; la que más puntos acumule (max 100) se activa.
    /// Si el puntaje máximo cae por debajo de 30, el arquetipo vuelve a None.
    /// </summary>
    public class ArchetypeResolver : MonoBehaviour
    {
        public static ArchetypeResolver Instance { get; private set; }

        [Header("Configuración")]
        [SerializeField] private float evaluationInterval  = 10f;
        [SerializeField] private float minimumScoreToSet   = 30f;

        [Header("Definiciones de arquetipos")]
        [SerializeField] private ArchetypeDefinition[] definitions;

        // ── Historial de comportamiento ───────────────────────────────────────────
        private int   _killsLast30s   = 0;
        private float _killWindow     = 30f;
        private readonly Queue<float> _killTimestamps = new Queue<float>();

        private float _evalTimer = 0f;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnUnitDied += OnUnitDied;
        }

        private void OnDisable()
        {
            EventBus.OnUnitDied -= OnUnitDied;
        }

        private void Update()
        {
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            // Mantener ventana de 30s para kills
            while (_killTimestamps.Count > 0 &&
                   Time.time - _killTimestamps.Peek() > _killWindow)
                _killTimestamps.Dequeue();
            _killsLast30s = _killTimestamps.Count;

            _evalTimer += Time.deltaTime;
            if (_evalTimer >= evaluationInterval)
            {
                _evalTimer = 0f;
                Evaluate();
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Evaluación

        private void Evaluate()
        {
            // Usar ArchetypeLibrary como fallback si no hay definitions asignadas en inspector
            var defs = (definitions != null && definitions.Length > 0)
                ? definitions
                : (ArchetypeLibrary.Instance != null ? ArchetypeLibrary.Instance.All : null);

            if (defs == null || defs.Length == 0) return;

            // Reasignar para que el resto del método use la variable local
            definitions = defs;

            ArchetypeDefinition best = null;
            float bestScore = 0f;

            foreach (var def in definitions)
            {
                if (def == null) continue;
                float score = ScoreDefinition(def);
                if (score > bestScore)
                {
                    bestScore = score;
                    best      = def;
                }
            }

            var gm = GameManager.Instance;
            ArchetypeType current = gm != null ? gm.CurrentArchetype : ArchetypeType.None;

            if (bestScore >= minimumScoreToSet && best != null && best.archetypeType != current)
            {
                ArchetypeType previous = current;
                if (gm != null) gm.SetArchetype(best.archetypeType);
                EventBus.TriggerArchetypeChanged(previous, best.archetypeType);
                Debug.Log($"[ArchetypeResolver] Arquetipo → {best.displayName} (score: {bestScore:F0})");
            }
            else if (bestScore < minimumScoreToSet && current != ArchetypeType.None)
            {
                if (gm != null) gm.SetArchetype(ArchetypeType.None);
                EventBus.TriggerArchetypeChanged(current, ArchetypeType.None);
                Debug.Log("[ArchetypeResolver] Arquetipo → None (comportamiento no definido)");
            }
        }

        private float ScoreDefinition(ArchetypeDefinition def)
        {
            float score = 0f;
            var   rm    = ResourceManager.Instance;
            var   em    = EnvironmentManager.Instance;

            // ATP net rate en rango
            if (rm != null)
            {
                float netATP = rm.GetNetRate(ResourceType.ATP);
                if (netATP >= def.minATPNetRate && netATP <= def.maxATPNetRate)
                    score += 25f;
            }

            // Fermentación
            if (def.requiresFermentation)
            {
                if (em != null && em.IsFermentationActive) score += 20f;
            }
            else score += 10f; // Sin requisito da puntos base

            // Aliados cercanos
            if (def.minAlliesNearby > 0)
            {
                int allies = CountNearbyAllies(8f);
                if (allies >= def.minAlliesNearby) score += 25f;
                else score += 10f * (allies / (float)def.minAlliesNearby);
            }
            else score += 15f;

            // Kills recientes
            if (def.minRecentKills > 0)
            {
                if (_killsLast30s >= def.minRecentKills) score += 25f;
                else score += 20f * (_killsLast30s / (float)def.minRecentKills);
            }
            else score += 15f;

            // Condición de O2 bajo
            if (def.scoreIfLowO2 && em != null && em.CurrentO2 < 0.2f)
                score += 15f;

            return Mathf.Clamp(score, 0f, 100f);
        }

        private int CountNearbyAllies(float radius)
        {
            // TODO: Primordia — if (Player.CAP.Instance == null) return 0;
            // TODO: Primordia — var origin = Player.CAP.Instance.transform.position;
            Vector2 origin = Vector2.zero; // Primordia stub
            int count = 0;
            var cols = Physics2D.OverlapCircleAll(origin, radius);
            foreach (var col in cols)
            {
                if (col.CompareTag("AllyUnit") || col.CompareTag("AllyOrganelle"))
                    count++;
            }
            return count;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Handlers

        private void OnUnitDied(GameObject go, string type)
        {
            // Solo contamos muertes de enemigos
            if (go != null && (go.CompareTag("Enemy") || go.CompareTag("SmallEnemy")))
                _killTimestamps.Enqueue(Time.time);
        }

        #endregion
    }
}
