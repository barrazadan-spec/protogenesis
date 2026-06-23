using System;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Views;

namespace Protogenesis.Progression
{
    /// <summary>
    /// SpecializationTracker — 3 fuentes de puntos + consolidación por dominio (GDD v4.3, Prompt 3.3).
    ///
    /// Fuentes:
    ///   35% — Genes activos (+1.5 pts/seg por gen, leído cada 1s)
    ///   35% — Comportamiento en mapa (movimiento, colecta, combate, inactividad)
    ///   30% — Estructuras instaladas (tabla por dominio)
    ///
    /// Umbral: 50 pts procariota / 60 pts eucariota.
    /// </summary>
    public class SpecializationTracker : MonoBehaviour
    {
        public static SpecializationTracker Instance { get; private set; }

        private readonly float[] _scores = new float[13]; // None(0) + 12 tipos

        private bool               _consolidated;
        private SpecializationType _consolidatedType = SpecializationType.None;

        // Timers para fuentes
        private float _geneTimer;
        private float _behaviorTimer;
        private float _idleTimer;
        private float _lastCombatTime = -999f;

        private Vector2 _lastPos;
        private bool    _hasLastPos;
        private float   _distLastSecond;
        private float   _distAccum;
        private float   _distTimer;

        private float _broadcastTimer;
        private const float BroadcastInterval = 0.5f;

        // ─────────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnOrganelleBuilt += OnOrganelleBuilt;
            EventBus.OnCellDeath      += OnCellDeath;
        }

        private void OnDisable()
        {
            EventBus.OnOrganelleBuilt -= OnOrganelleBuilt;
            EventBus.OnCellDeath      -= OnCellDeath;
        }

        private void Update()
        {
            if (_consolidated) return;

            float dt = Time.deltaTime;

            // Distancia acumulada por segundo
            TrackMovement(dt);

            // Genes: cada 1s
            _geneTimer += dt;
            if (_geneTimer >= 1f) { _geneTimer = 0f; AccumulateGenes(); }

            // Comportamiento: cada 0.5s
            _behaviorTimer += dt;
            if (_behaviorTimer >= 0.5f) { _behaviorTimer = 0f; AccumulateBehavior(); }

            CheckConsolidation();

            _broadcastTimer += dt;
            if (_broadcastTimer >= BroadcastInterval) { _broadcastTimer = 0f; BroadcastLeaders(); }
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region Fuente 1 — Genes

        private void AccumulateGenes()
        {
            var grs = GeneticRingSystem.Instance;
            if (grs == null) return;

            AddGenePoints(GeneticRing.Ring1, grs.ActiveRing1Gene);
            AddGenePoints(GeneticRing.Ring2, grs.ActiveRing2Gene);
            AddGenePoints(GeneticRing.Ring3, grs.ActiveRing3Gene);
        }

        private void AddGenePoints(GeneticRing ring, int geneIndex)
        {
            if (geneIndex < 0) return;

            switch (ring)
            {
                case GeneticRing.Ring1:
                    // 0=Respiración→Eucariota, 1=Fotosíntesis→Eucariota, 2=Fermentación→Procariota, 3=Quimiolitotrofía→Procariota
                    if (geneIndex == 0) { Add(SpecializationType.Ciliado, 1.5f); Add(SpecializationType.Ameba, 0.5f); }
                    else if (geneIndex == 1) { Add(SpecializationType.Cianobacteria, 1.5f); }
                    else if (geneIndex == 2) { Add(SpecializationType.Bacteria, 1.5f); Add(SpecializationType.Arquea, 0.5f); }
                    else if (geneIndex == 3) { Add(SpecializationType.Arquea, 1.5f); }
                    break;

                case GeneticRing.Ring2:
                    if (geneIndex == 0) { Add(SpecializationType.Flagelado, 1.5f); }
                    else if (geneIndex == 1) { Add(SpecializationType.Bacteria, 1.5f); }
                    else if (geneIndex == 2) { Add(SpecializationType.CelulaNK, 1.5f); Add(SpecializationType.CelulaB, 0.5f); }
                    else if (geneIndex == 3) { Add(SpecializationType.Arquea, 1.5f); }
                    break;

                case GeneticRing.Ring3:
                    if (geneIndex == 0) { Add(SpecializationType.Bacteria, 1.5f); }
                    else if (geneIndex == 1) { Add(SpecializationType.CelulaMadre, 1.5f); }
                    else if (geneIndex == 2) { Add(SpecializationType.CelulaMadre, 1.0f); Add(SpecializationType.Flagelado, 0.5f); }
                    else if (geneIndex == 3) { Add(SpecializationType.Macrofago, 1.5f); }
                    break;
            }
        }

        // Llamado por GeneticRingSystem al activar un gen
        public void OnGeneActivated(GeneticRing ring, int geneIndex) => AddGenePoints(ring, geneIndex);

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Fuente 2 — Comportamiento

        private void TrackMovement(float dt)
        {
            var player = PlayerCellExterior.Instance;
            if (player == null) return;

            Vector2 pos = player.transform.position;
            if (!_hasLastPos) { _lastPos = pos; _hasLastPos = true; return; }

            float dist = Vector2.Distance(pos, _lastPos);
            _lastPos    = pos;
            _distAccum += dist;
            _distTimer += dt;

            if (_distTimer >= 1f)
            {
                _distLastSecond = _distAccum;
                _distAccum      = 0f;
                _distTimer      = 0f;
            }

            if (dist < 0.01f)
                _idleTimer += dt;
            else
                _idleTimer = 0f;
        }

        private void AccumulateBehavior()
        {
            // Movimiento > 2u en el último segundo
            if (_distLastSecond > 2f)
            {
                Add(SpecializationType.Flagelado, 0.8f * 0.5f); // ×0.5 por evaluar cada 0.5s
                Add(SpecializationType.Bacteria,  0.4f * 0.5f);
            }

            // Sin movimiento > 5s
            if (_idleTimer > 5f)
            {
                Add(SpecializationType.Arquea,  1.0f * 0.5f);
                Add(SpecializationType.Hongo,   0.8f * 0.5f);
            }

            // Combate reciente (rival contact tracked externally via OnRivalContact)
            // Resource node collected sin combate reciente — se trackea en AccumulateResourceCollect
        }

        /// <summary>Llamar al recolectar un nodo de recurso.</summary>
        public void OnResourceNodeCollected()
        {
            bool noCombatRecent = (Time.time - _lastCombatTime) > 10f;
            if (noCombatRecent)
            {
                Add(SpecializationType.Cianobacteria, 1.0f);
                Add(SpecializationType.Ciliado,       0.6f);
            }
        }

        /// <summary>Llamar al contacto con célula rival.</summary>
        public void OnRivalContact()
        {
            _lastCombatTime = Time.time;
            Add(SpecializationType.Ameba,     1.2f);
            Add(SpecializationType.Macrofago,  0.8f);
            Add(SpecializationType.Neutrofilo, 0.8f);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Fuente 3 — Estructuras instaladas

        private void OnOrganelleBuilt(GameObject go, string type)
        {
            string t = type?.ToLower() ?? "";
            var domain = CellProgressionPhase.Instance?.CurrentDomain ?? CellDomain.Undefined;

            if (domain == CellDomain.Prokaryote)
                ApplyProkaryotePoints(t);
            else
                ApplyEukaryotePoints(t);
        }

        private void ApplyProkaryotePoints(string t)
        {
            if (t.Contains("flagelo") && t.Contains("bacter"))
            { Add(SpecializationType.Bacteria, 12f); Add(SpecializationType.Cianobacteria, 3f); }

            if (t.Contains("peptidoglicano") || (t.Contains("pared") && !t.Contains("celulosa")))
            { Add(SpecializationType.Bacteria, 10f); Add(SpecializationType.Arquea, 8f); }

            if (t.Contains("plasmido"))
            { Add(SpecializationType.Bacteria, 14f); }

            if (t.Contains("capsula"))
            { Add(SpecializationType.Arquea, 14f); Add(SpecializationType.Bacteria, 4f); }

            if (t.Contains("fimbria"))
            { Add(SpecializationType.Bacteria, 12f); }

            if (t.Contains("tilacoide"))
            { Add(SpecializationType.Cianobacteria, 15f); }
        }

        private void ApplyEukaryotePoints(string t)
        {
            if (t.Contains("flagelo") && !t.Contains("bacter"))
            { Add(SpecializationType.Flagelado, 10f); Add(SpecializationType.Bacteria, 3f); }

            if (t.Contains("lisosoma"))
            { Add(SpecializationType.Ameba, 10f); Add(SpecializationType.Macrofago, 7f); }

            if (t.Contains("vacuola"))
            { Add(SpecializationType.Ciliado, 6f); Add(SpecializationType.Ameba, 3f); }

            if (t.Contains("granulo"))
            { Add(SpecializationType.Neutrofilo, 14f); }

            if (t.Contains("tilacoide"))
            { Add(SpecializationType.Cianobacteria, 12f); }

            if (t.Contains("celulosa") || (t.Contains("pared") && !t.Contains("peptido")))
            { Add(SpecializationType.Hongo, 10f); }

            if (t.Contains("hifa"))
            { Add(SpecializationType.Hongo, 14f); }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Consolidación

        private float GetConsolidationThreshold()
        {
            var domain = CellProgressionPhase.Instance?.CurrentDomain ?? CellDomain.Undefined;
            return domain == CellDomain.Prokaryote ? 50f : 60f;
        }

        private void CheckConsolidation()
        {
            float threshold = GetConsolidationThreshold();
            var leader      = GetLeader(out float leadScore);
            if (leader != SpecializationType.None && leadScore >= threshold)
                Consolidate(leader);
        }

        private void Consolidate(SpecializationType type)
        {
            _consolidated     = true;
            _consolidatedType = type;

            var cs = CellState.Instance;
            if (cs != null) cs.ConsolidatedSpecialization = type;

            string displayName = GetDisplayName(type);
            EventBus.TriggerSpecializationConsolidated(type, displayName);
            EventBus.TriggerPowerMoment("consolidation");
            Debug.Log($"[SpecializationTracker] Consolidada: {displayName}");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        public float GetScore(SpecializationType type)
            => type == SpecializationType.None ? 0f : _scores[(int)type];

        public float GetConsolidationProgress(SpecializationType type)
            => Mathf.Clamp01(GetScore(type) / GetConsolidationThreshold());

        public bool               IsConsolidated => _consolidated;
        public SpecializationType ConsolidatedType => _consolidatedType;

        public SpecializationType LeadingSpecialization => GetLeader(out _);

        public float LeadingScore { get { GetLeader(out float s); return s; } }

        public float ConsolidationThreshold => GetConsolidationThreshold();

        public SpecializationType GetLeader(out float score)
        {
            int best = 0; float max = 0f;
            for (int i = 1; i < _scores.Length; i++)
                if (_scores[i] > max) { max = _scores[i]; best = i; }
            score = max;
            return best == 0 ? SpecializationType.None : (SpecializationType)best;
        }

        public float[] AllScores
        {
            get
            {
                var copy = new float[_scores.Length];
                Array.Copy(_scores, copy, _scores.Length);
                return copy;
            }
        }

        public static string GetDisplayName(SpecializationType type) => type switch
        {
            SpecializationType.None         => "Sin especialización",
            SpecializationType.Bacteria     => "Bacteria",
            SpecializationType.Arquea       => "Arquea",
            SpecializationType.Cianobacteria=> "Cianobacteria",
            SpecializationType.Hongo        => "Hongo",
            SpecializationType.Ameba        => "Ameba",
            SpecializationType.Flagelado    => "Flagelado",
            SpecializationType.Ciliado      => "Ciliado",
            SpecializationType.Neutrofilo   => "Neutrófilo",
            SpecializationType.Macrofago    => "Macrófago",
            SpecializationType.CelulaNK     => "Célula NK",
            SpecializationType.CelulaB      => "Célula B",
            SpecializationType.CelulaMadre  => "Célula Madre",
            _                               => type.ToString()
        };

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private void Add(SpecializationType type, float amount)
        {
            if (type == SpecializationType.None || amount <= 0f) return;
            int idx = (int)type;
            if (idx < 0 || idx >= _scores.Length) return;

            float prev = _scores[idx];
            _scores[idx] = Mathf.Min(prev + amount, 100f);
            if (!Mathf.Approximately(_scores[idx], prev))
                EventBus.TriggerSpecializationScoreChanged(type, _scores[idx]);
        }

        private void BroadcastLeaders()
        {
            float[] sorted = new float[13];
            Array.Copy(_scores, sorted, 13);
            Array.Sort(sorted);
            float threshold = sorted[10];

            for (int i = 1; i < 13; i++)
                if (_scores[i] >= threshold && _scores[i] > 0f)
                    EventBus.TriggerSpecializationProgress((SpecializationType)i, _scores[i]);
        }

        private void OnCellDeath()
        {
            if (!_consolidated) Array.Clear(_scores, 0, _scores.Length);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI debug

        private void OnGUI()
        {
            if (!Application.isEditor && !Debug.isDebugBuild) return;

            float threshold = GetConsolidationThreshold();
            float x = Screen.width - 200f;
            float y = 10f;

            if (_consolidated)
            {
                GUI.Label(new Rect(x, y, 190f, 20f), $"ESPEC: {GetDisplayName(_consolidatedType)}");
                return;
            }

            var leader = GetLeader(out float score);
            GUI.Label(new Rect(x, y, 190f, 20f),
                $"Espec → {GetDisplayName(leader)}: {score:F0}/{threshold:F0}");
        }

        #endregion
    }
}
