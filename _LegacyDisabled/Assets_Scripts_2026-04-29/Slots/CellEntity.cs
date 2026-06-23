using UnityEngine;
using Protogenesis.Archetypes;

namespace Protogenesis.Slots
{
    /// <summary>
    /// Proxy centralizado de estadísticas de la célula (GDD v3 §7, §11).
    ///
    /// CellEntity es el único punto donde slots, arquetipos y el entorno
    /// escriben modificadores. Los sistemas existentes (CAP, abilities, IA)
    /// leen de aquí. Principio de integración: "adición, no sustitución".
    ///
    /// Todos los modificadores son aditivos sobre la base de CAPStats:
    ///   DamageMultiplier = 1.0 + Σ bonos
    /// </summary>
    public class CellEntity : MonoBehaviour
    {
        public static CellEntity Instance { get; private set; }

        // ── COMBATE ───────────────────────────────────────────────────────────────
        public float DamageMultiplier      { get; private set; } = 1f;
        public float AttackSpeedMultiplier { get; private set; } = 1f;

        // ── MOVILIDAD ─────────────────────────────────────────────────────────────
        public float MoveSpeedMultiplier   { get; private set; } = 1f;

        // ── INTEGRIDAD (HP) ───────────────────────────────────────────────────────
        public float MaxHPBonus            { get; private set; } = 0f;
        public float HPRegenPerSecond      { get; private set; } = 0f;
        public float DamageReduction       { get; private set; } = 0f;  // 0-1

        // ── ENERGÍA (ATP) ─────────────────────────────────────────────────────────
        public float ATPProductionMultiplier   { get; private set; } = 1f;
        public float FermentationEffBonus      { get; private set; } = 0f;

        // ── ALIMENTACIÓN (BIOMASA) ────────────────────────────────────────────────
        public float BiomassGainMultiplier { get; private set; } = 1f;
        public float FagocytosisRange      { get; private set; } = 0f;  // radio extra
        public float FiltrationRate        { get; private set; } = 0f;  // partículas/s extra

        // ── REPRODUCCIÓN ──────────────────────────────────────────────────────────
        public float ReproductionRateBonus { get; private set; } = 0f;  // % más rápido
        public int   MaxColonySize         { get; private set; } = 0;   // unidades adicionales

        // ── SOCIAL ────────────────────────────────────────────────────────────────
        public float QuorumSignalRadius    { get; private set; } = 0f;  // radio extra de señal
        public float SocialSignalStrength  { get; private set; } = 0f;  // intensidad de señal

        // ── SENSORIAL ─────────────────────────────────────────────────────────────
        public float PerceptionRadius      { get; private set; } = 0f;  // radio extra
        public bool  HasChemoreception     { get; private set; } = false;
        public bool  HasPhotoreception     { get; private set; } = false;

        // ── FACCIÓN (GDD v5.2 — MVP PvP) ─────────────────────────────────────────
        /// <summary>
        /// 0 = Neutral/NPC  |  1 = Jugador 1  |  2 = Jugador 2  |  3+ = otros jugadores.
        /// EcosystemManager es la fuente de verdad; este valor se asigna al inicio.
        /// </summary>
        public int FactionId { get; private set; } = 1;

        public void SetFaction(int factionId) => FactionId = factionId;

        // ── ARQUETIPO ACTIVO ──────────────────────────────────────────────────────
        public ArchetypeType CurrentArchetype { get; private set; } = ArchetypeType.None;

        // ── DEUDA DE ADAPTACIÓN (GDD v3 §7.3) ────────────────────────────────────
        /// <summary>
        /// 0 = sin deuda (100% eficiencia).
        /// 1 = deuda total (0% eficiencia, máxima vulnerabilidad).
        /// </summary>
        public float AdaptationDebt { get; private set; } = 0f;

        // ── Acumuladores internos ─────────────────────────────────────────────────
        private float _damAdd, _atkSpdAdd, _movSpdAdd, _atpAdd, _bioAdd, _reproAdd;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()  => Core.EventBus.OnArchetypeChanged += HandleArchetypeChanged;
        private void OnDisable() => Core.EventBus.OnArchetypeChanged -= HandleArchetypeChanged;

        private void Update()
        {
            // La deuda de adaptación se resuelve sola con el tiempo
            // El rate real lo controla AdaptationDebtHandler en SlotManager
            if (AdaptationDebt > 0f)
                AdaptationDebt = Mathf.Max(0f, AdaptationDebt - Time.deltaTime * 0.02f);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Combate

        public void AddDamageMultiplier(float v)     { _damAdd += v;    DamageMultiplier      = 1f + _damAdd; }
        public void RemoveDamageMultiplier(float v)  { _damAdd -= v;    DamageMultiplier      = 1f + _damAdd; }
        public void AddAttackSpeedMult(float v)      { _atkSpdAdd += v; AttackSpeedMultiplier = 1f + _atkSpdAdd; }
        public void RemoveAttackSpeedMult(float v)   { _atkSpdAdd -= v; AttackSpeedMultiplier = 1f + _atkSpdAdd; }

        public void AddDamageReduction(float v)      => DamageReduction = Mathf.Clamp01(DamageReduction + v);
        public void RemoveDamageReduction(float v)   => DamageReduction = Mathf.Clamp01(DamageReduction - v);

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Movilidad

        public void AddMoveSpeedMult(float v)        { _movSpdAdd += v; MoveSpeedMultiplier = 1f + _movSpdAdd; }
        public void RemoveMoveSpeedMult(float v)     { _movSpdAdd -= v; MoveSpeedMultiplier = 1f + _movSpdAdd; }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Integridad

        public void AddMaxHPBonus(float v)           => MaxHPBonus      += v;
        public void RemoveMaxHPBonus(float v)        => MaxHPBonus      -= v;
        public void AddHPRegen(float v)              => HPRegenPerSecond += v;
        public void RemoveHPRegen(float v)           => HPRegenPerSecond -= v;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Energía

        public void AddATPMult(float v)              { _atpAdd += v;    ATPProductionMultiplier = 1f + _atpAdd; }
        public void RemoveATPMult(float v)           { _atpAdd -= v;    ATPProductionMultiplier = 1f + _atpAdd; }
        public void AddFermentationBonus(float v)    => FermentationEffBonus += v;
        public void RemoveFermentationBonus(float v) => FermentationEffBonus -= v;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Alimentación

        public void AddBiomassMult(float v)          { _bioAdd += v;    BiomassGainMultiplier = 1f + _bioAdd; }
        public void RemoveBiomassMult(float v)       { _bioAdd -= v;    BiomassGainMultiplier = 1f + _bioAdd; }
        public void AddFagocytosisRange(float v)     => FagocytosisRange += v;
        public void RemoveFagocytosisRange(float v)  => FagocytosisRange  = Mathf.Max(0f, FagocytosisRange - v);
        public void AddFiltrationRate(float v)       => FiltrationRate   += v;
        public void RemoveFiltrationRate(float v)    => FiltrationRate    = Mathf.Max(0f, FiltrationRate - v);

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Reproducción

        public void AddReproductionRate(float v)     { _reproAdd += v; ReproductionRateBonus = _reproAdd; }
        public void RemoveReproductionRate(float v)  { _reproAdd -= v; ReproductionRateBonus = _reproAdd; }
        public void AddColonySize(int v)             => MaxColonySize += v;
        public void RemoveColonySize(int v)          => MaxColonySize  = Mathf.Max(0, MaxColonySize - v);

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Social

        public void AddQuorumRadius(float v)         => QuorumSignalRadius  += v;
        public void RemoveQuorumRadius(float v)      => QuorumSignalRadius   = Mathf.Max(0f, QuorumSignalRadius - v);
        public void AddSocialSignal(float v)         => SocialSignalStrength += v;
        public void RemoveSocialSignal(float v)      => SocialSignalStrength  = Mathf.Max(0f, SocialSignalStrength - v);

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Sensorial

        public void AddPerceptionRadius(float v)     => PerceptionRadius += v;
        public void RemovePerceptionRadius(float v)  => PerceptionRadius  = Mathf.Max(0f, PerceptionRadius - v);
        public void EnableChemoreception()           => HasChemoreception = true;
        public void DisableChemoreception()          => HasChemoreception = false;
        public void EnablePhotoreception()           => HasPhotoreception = true;
        public void DisablePhotoreception()          => HasPhotoreception = false;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API — Deuda de adaptación

        /// <summary>
        /// Añade deuda de adaptación al instalar una estructura de cierto nivel.
        /// La deuda se resuelve gradualmente en Update().
        /// </summary>
        public void AddAdaptationDebt(float debt)
        {
            AdaptationDebt = Mathf.Clamp01(AdaptationDebt + debt);
        }

        /// <summary>
        /// Modificador de eficiencia global derivado de la deuda de adaptación.
        /// 0 deuda = 1.0 eficiencia; 1.0 deuda = 0.0 eficiencia.
        /// </summary>
        public float EfficiencyFromDebt => 1f - AdaptationDebt;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Arquetipo

        private void HandleArchetypeChanged(ArchetypeType prev, ArchetypeType next)
            => CurrentArchetype = next;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Reset (para regresión evolutiva)

        /// <summary>
        /// Reinicia todos los modificadores a sus valores base.
        /// Llamar por DeathSystem al hacer regresión evolutiva.
        /// </summary>
        public void ResetToBase()
        {
            _damAdd = _atkSpdAdd = _movSpdAdd = _atpAdd = _bioAdd = _reproAdd = 0f;
            DamageMultiplier        = 1f;
            AttackSpeedMultiplier   = 1f;
            MoveSpeedMultiplier     = 1f;
            MaxHPBonus              = 0f;
            HPRegenPerSecond        = 0f;
            DamageReduction         = 0f;
            ATPProductionMultiplier = 1f;
            FermentationEffBonus    = 0f;
            BiomassGainMultiplier   = 1f;
            FagocytosisRange        = 0f;
            FiltrationRate          = 0f;
            ReproductionRateBonus   = 0f;
            MaxColonySize           = 0;
            QuorumSignalRadius      = 0f;
            SocialSignalStrength    = 0f;
            PerceptionRadius        = 0f;
            HasChemoreception       = false;
            HasPhotoreception       = false;
            AdaptationDebt          = 0f;
            CurrentArchetype        = ArchetypeType.None;
        }

        #endregion
    }
}
