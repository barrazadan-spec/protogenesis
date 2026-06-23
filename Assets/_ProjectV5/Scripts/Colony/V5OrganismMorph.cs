using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5MorphPartRole { Body, Legs, Mouth }
    public enum V5OrganismBlueprintKind { Tardigrade, Volvox, Collector, Harasser, Anchor, Interdictor, Lacrymaria, Fighter }

    [System.Serializable]
    public class V5OrganismBlueprintDefinition
    {
        public V5OrganismBlueprintKind Kind = V5OrganismBlueprintKind.Tardigrade;
        public string Name = "Tardigrado";
        public int RequiredFreeCells = V5OrganismMorph.TardigradeRequiredFreeCells;
        public Sprite SilhouetteSprite;
        public Texture2D SilhouetteTexture;
        public bool UseTemporaryVolvoxSilhouette;
        public bool UseTemporaryCollectorSilhouette;
        public bool UseTemporaryHarasserSilhouette;
        public bool UseTemporaryAnchorSilhouette;
        public bool UseTemporaryInterdictorSilhouette;
        public bool UseTemporaryLacrymariaSilhouette;
        public bool UseTemporaryFighterSilhouette;
        public bool ForceAllBodyRoles;
        public float FormationRadiusX = 3.25f;
        public float FormationRadiusY = 1.55f;
        public float BaseMoveSpeed = 1.35f;
        public float LegMoveSpeedPerCell = 0.26f;
        public float MaxMoveSpeed = 6.2f;
        public float BodyHpPerCell = 8f;
        public float BodyResistancePerCell = 0.012f;

        public float PassiveBiomassPerCellPerSecond;
        public float EngulfRateMultiplier = 1f;
        public float ResourceSearchRange;
        public float CombatDamageMultiplier = 1f;
        public float MemberMaxHpMultiplier = 1f;
        public float AttackRange = 1.1f;
        public float RangedDamagePerSecond;
        public V5DamageKind DamageKind = V5DamageKind.Physical;
        [Range(0f, 1f)] public float Armor;
    }

    public class V5OrganismMorph : MonoBehaviour, IV5RunResettable
    {
        public const int DefaultMaxActiveOrganisms = 12;
        public const int CollectorRequiredFreeCells = 6;
        public const int HarasserRequiredFreeCells = 8;
        public const int FighterRequiredFreeCells = 10;
        public const int InterdictorRequiredFreeCells = 12;
        public const int AnchorRequiredFreeCells = 14;
        public const int LacrymariaRequiredFreeCells = 20;
        public const int VolvoxRequiredFreeCells = 22;
        public const int TardigradeRequiredFreeCells = 50;
        private const float EngulfCooldown = 0.6f;

        private enum PlayerOrderState { None, Move, Attack, AttackMove, Farm, Hold }

        [Header("Blueprint")]
        public string BlueprintName = "Tardigrado";
        public int RequiredFreeCells = TardigradeRequiredFreeCells;
        public List<V5OrganismBlueprintDefinition> Blueprints = new List<V5OrganismBlueprintDefinition>();
        public int ActiveBlueprintIndex;
        public bool TardigradeUnlocked = true;
        public int MaxActiveOrganisms = DefaultMaxActiveOrganisms;
        [Range(0f, 1f)] public float RevertDeathFraction = 0.30f;

        [Header("Silhouette Blueprint")]
        // TEMP ART REPLACEMENT: assign a transparent PNG Sprite/Texture here for the Tardigrado blueprint.
        // When both fields are empty, GenerateTemporaryTardigradeSilhouetteTexture() builds the rough code silhouette below.
        public Sprite BlueprintSilhouetteSprite;
        public Texture2D BlueprintSilhouetteTexture;
        [Range(0.01f, 0.95f)] public float SilhouetteAlphaThreshold = 0.18f;
        [Range(32, 256)] public int SilhouetteSampleResolutionX = 160;

        [Header("Silhouette Feel")]
        public float FormationRadiusX = 3.25f;
        public float FormationRadiusY = 1.55f;
        public float FormationFollowSpeed = 12f;
        public float MorphFlowSeconds = 0.75f;
        public float RotationLerpSpeed = 7.5f;
        public float RevertScatterSpeed = 3.2f;

        [Header("Part Functions")]
        public float BaseOrganismMoveSpeed = 1.35f;
        public float LegMoveSpeedPerCell = 0.26f;
        public float MaxOrganismMoveSpeed = 6.2f;
        public float EngulfRange = 2.4f;
        public float MouthEngulfRatePerCell = 8f;
        [Range(0f, 1f)] public float BaseEngulfRateMouthFraction = 0.20f;
        public float MaxEngulfRate = 72f;
        public float BodyHpPerCell = 8f;
        public float BodyResistancePerCell = 0.012f;

        [Header("Interdictor")]
        public float InterdictorToxicDamagePerSecond = 2.5f;

        [Header("Runtime State")]
        [Range(0.05f, 0.6f)] public float CollapseLiveCellFraction = 0.20f;
        public int CollapseMinimumLiveCells = 2;

        public bool IsMorphed { get { return isRuntimeInstance ? isMorphed : ActiveOrganismCount > 0; } private set { isMorphed = value; } }
        public string LastMessage { get; private set; }
        public IReadOnlyList<V5CellEntity> Members { get { return members; } }
        public IReadOnlyList<V5OrganismMorph> ActiveOrganisms { get { PruneOrganisms(); return activeOrganisms; } }
        public int ActiveOrganismCount { get { PruneOrganisms(); return activeOrganisms.Count; } }
        public bool UsingTemporarySilhouette { get; private set; }
        public int LastLegSlotCount { get; private set; }
        public int LastMouthSlotCount { get; private set; }
        public int LastBodySlotCount { get; private set; }
        public int LiveLegCount { get; private set; }
        public int LiveMouthCount { get; private set; }
        public int LiveBodyCount { get; private set; }
        public float CurrentOrganismMoveSpeed { get; private set; }
        public float CurrentEngulfRate { get; private set; }
        public float CurrentBodyStructureHp { get; private set; }
        public float CurrentBodyResistance01 { get; private set; }
        public float CurrentPassiveBiomassPerSecond { get; private set; }
        public float MorphFlow01 { get; private set; }
        public float OrganismAngleDegrees { get { return organismAngle; } }
        public float Health01 { get { return OrganismMaxHp() <= 0f ? 0f : Mathf.Clamp01(OrganismCurrentHp() / OrganismMaxHp()); } }
        public float EffectiveCombatDamagePerSecond
        {
            get
            {
                V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
                float mouthPressure = CurrentEngulfRate * 0.22f;
                float bodyPressure = Mathf.Max(0f, LiveBodyCount * 0.08f + LiveMouthCount * 0.36f);
                if (blueprint.RangedDamagePerSecond > 0f) return blueprint.RangedDamagePerSecond;
                return (5f + mouthPressure + bodyPressure) * Mathf.Max(0.05f, blueprint.CombatDamageMultiplier);
            }
        }
        public Texture2D ActiveSilhouetteTexture
        {
            get
            {
                V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
                Sprite sprite = blueprint.SilhouetteSprite != null ? blueprint.SilhouetteSprite : BlueprintSilhouetteSprite;
                if (sprite != null) return sprite.texture;
                Texture2D texture = blueprint.SilhouetteTexture != null ? blueprint.SilhouetteTexture : BlueprintSilhouetteTexture;
                if (texture != null) return texture;
                if (blueprint.UseTemporaryHarasserSilhouette) return GenerateTemporaryHarasserSilhouetteTexture();
                if (blueprint.UseTemporaryFighterSilhouette) return GenerateTemporaryFighterSilhouetteTexture();
                if (blueprint.UseTemporaryInterdictorSilhouette) return GenerateTemporaryInterdictorSilhouetteTexture();
                if (blueprint.UseTemporaryLacrymariaSilhouette) return GenerateTemporaryLacrymariaSilhouetteTexture();
                if (blueprint.UseTemporaryAnchorSilhouette) return GenerateTemporaryAnchorSilhouetteTexture();
                if (blueprint.UseTemporaryCollectorSilhouette) return GenerateTemporaryCollectorSilhouetteTexture();
                if (blueprint.UseTemporaryVolvoxSilhouette) return GenerateTemporaryVolvoxSilhouetteTexture();
                return GenerateTemporaryTardigradeSilhouetteTexture();
            }
        }
        public string ActiveBlueprintName { get { return GetActiveBlueprint().Name; } }
        public V5OrganismBlueprintKind ActiveBlueprintKind { get { return GetActiveBlueprint().Kind; } }
        public int ActiveRequiredFreeCells { get { return GetActiveBlueprint().RequiredFreeCells; } }
        public V5DamageKind ActiveDamageKind { get { return GetActiveBlueprint().DamageKind; } }
        public float ActiveArmor { get { return Mathf.Clamp01(GetActiveBlueprint().Armor); } }
        public float ActiveAttackRange { get { return Mathf.Max(0.1f, GetActiveBlueprint().AttackRange); } }
        public bool IsTardigradeUnlocked { get { return true; } }
        public V5CellEntity NucleusCell { get { return nucleusCell; } }
        public bool IsEnemyOrganism { get { return isEnemyOrganism; } }
        public bool IsNeutralOrganism { get { return isNeutralOrganism; } }
        public bool IsAlive { get { return IsMorphed && members.Count > 0; } }

        private readonly List<V5OrganismMorph> activeOrganisms = new List<V5OrganismMorph>(DefaultMaxActiveOrganisms);
        private readonly List<V5OrganismMorph> activeEnemyOrganisms = new List<V5OrganismMorph>(4);
        private readonly List<V5OrganismMorph> activeNeutralOrganisms = new List<V5OrganismMorph>(8);
        private readonly List<V5CellEntity> members = new List<V5CellEntity>(64);
        private readonly List<Vector2> slotOffsets = new List<Vector2>(64);
        private readonly List<Vector2> startOffsets = new List<Vector2>(64);
        private readonly List<V5MorphPartRole> slotRoles = new List<V5MorphPartRole>(64);
        private readonly List<float> originalMaxHp = new List<float>(64);
        private readonly List<float> originalPhysicalArmor = new List<float>(64);
        private readonly List<float> originalAttackRange = new List<float>(64);
        private Vector2 moveTarget;
        private bool hasMoveTarget;
        private Vector2 holdPosition;
        private bool hasHoldPosition;
        private PlayerOrderState playerOrder = PlayerOrderState.None;
        private V5CellEntity playerAttackTarget;
        private Vector2 playerAttackMoveTarget;
        private bool hasRangedStandoff;
        private Vector2 rangedStandoffPosition;
        private float morphStartedAt;
        private float organismAngle;
        private float lastEngulfTime = float.NegativeInfinity;
        private V5CellEntity nucleusCell;
        private bool isRuntimeInstance;
        private bool isEnemyOrganism;
        private bool isNeutralOrganism;
        private bool hasNeutralRoamCenter;
        private Vector2 neutralRoamCenter;
        private float neutralRoamRadius = 7f;
        private float nextNeutralWanderAt;
        private bool enemyRewardIssued;
        private int enemyRewardCells;
        private bool isMorphed;
        private V5OrganismMorph manager;
        private Texture2D temporaryTardigradeSilhouette;
        private Texture2D temporaryVolvoxSilhouette;
        private Texture2D temporaryCollectorSilhouette;
        private Texture2D temporaryHarasserSilhouette;
        private Texture2D temporaryFighterSilhouette;
        private Texture2D temporaryAnchorSilhouette;
        private Texture2D temporaryInterdictorSilhouette;
        private Texture2D temporaryLacrymariaSilhouette;

        private void OnValidate()
        {
            if (MaxActiveOrganisms < DefaultMaxActiveOrganisms) MaxActiveOrganisms = DefaultMaxActiveOrganisms;
        }

        private struct SilhouetteSample
        {
            public Vector2 normalized;
            public V5MorphPartRole role;

            public SilhouetteSample(Vector2 normalized, V5MorphPartRole role)
            {
                this.normalized = normalized;
                this.role = role;
            }
        }

        private struct MorphSlot
        {
            public Vector2 offset;
            public V5MorphPartRole role;

            public MorphSlot(Vector2 offset, V5MorphPartRole role)
            {
                this.offset = offset;
                this.role = role;
            }
        }

        private void EnsureBlueprints()
        {
            TardigradeUnlocked = true;
            if (Blueprints == null) Blueprints = new List<V5OrganismBlueprintDefinition>();
            if (!HasBlueprint(V5OrganismBlueprintKind.Tardigrade)) Blueprints.Insert(0, CreateTardigradeBlueprint());
            if (!HasBlueprint(V5OrganismBlueprintKind.Collector)) Blueprints.Add(CreateCollectorBlueprint());
            if (!HasBlueprint(V5OrganismBlueprintKind.Harasser)) Blueprints.Add(CreateHarasserBlueprint());
            if (!HasBlueprint(V5OrganismBlueprintKind.Fighter)) Blueprints.Add(CreateFighterBlueprint());
            if (!HasBlueprint(V5OrganismBlueprintKind.Interdictor)) Blueprints.Add(CreateInterdictorBlueprint());
            if (!HasBlueprint(V5OrganismBlueprintKind.Lacrymaria)) Blueprints.Add(CreateLacrymariaBlueprint());
            if (!HasBlueprint(V5OrganismBlueprintKind.Anchor)) Blueprints.Add(CreateAnchorBlueprint());
            if (!HasBlueprint(V5OrganismBlueprintKind.Volvox)) Blueprints.Add(CreateVolvoxBlueprint());
            ApplyBlueprintCostBalance();
            ActiveBlueprintIndex = Mathf.Clamp(ActiveBlueprintIndex, 0, Blueprints.Count - 1);
            SelectFirstUnlockedIfCurrentLocked();
            SyncLegacyBlueprintFields();
        }

        private bool HasBlueprint(V5OrganismBlueprintKind kind)
        {
            for (int i = 0; i < Blueprints.Count; i++)
                if (Blueprints[i] != null && Blueprints[i].Kind == kind) return true;
            return false;
        }

        private V5OrganismBlueprintDefinition GetActiveBlueprint()
        {
            EnsureBlueprintListOnly();
            if (Blueprints.Count == 0) Blueprints.Add(CreateTardigradeBlueprint());
            ActiveBlueprintIndex = Mathf.Clamp(ActiveBlueprintIndex, 0, Blueprints.Count - 1);
            SelectFirstUnlockedIfCurrentLocked();
            V5OrganismBlueprintDefinition blueprint = Blueprints[ActiveBlueprintIndex];
            if (blueprint == null)
            {
                blueprint = CreateTardigradeBlueprint();
                Blueprints[ActiveBlueprintIndex] = blueprint;
            }
            return blueprint;
        }

        private void SelectFirstUnlockedIfCurrentLocked()
        {
            if (Blueprints == null || Blueprints.Count == 0) return;
            V5OrganismBlueprintDefinition active = Blueprints[Mathf.Clamp(ActiveBlueprintIndex, 0, Blueprints.Count - 1)];
            if (IsBlueprintUnlocked(active)) return;

            for (int i = 0; i < Blueprints.Count; i++)
            {
                if (!IsBlueprintUnlocked(Blueprints[i])) continue;
                ActiveBlueprintIndex = i;
                return;
            }
        }

        private bool IsBlueprintUnlocked(V5OrganismBlueprintDefinition blueprint)
        {
            if (blueprint == null) return false;
            if (isEnemyOrganism || isNeutralOrganism) return true;
            if (blueprint.Kind == V5OrganismBlueprintKind.Tardigrade) return true;
            return true;
        }

        private void EnsureBlueprintListOnly()
        {
            if (Blueprints == null) Blueprints = new List<V5OrganismBlueprintDefinition>();
            if (Blueprints.Count == 0)
            {
                Blueprints.Add(CreateTardigradeBlueprint());
                Blueprints.Add(CreateCollectorBlueprint());
                Blueprints.Add(CreateHarasserBlueprint());
                Blueprints.Add(CreateFighterBlueprint());
                Blueprints.Add(CreateInterdictorBlueprint());
                Blueprints.Add(CreateLacrymariaBlueprint());
                Blueprints.Add(CreateAnchorBlueprint());
                Blueprints.Add(CreateVolvoxBlueprint());
            }
            ApplyBlueprintCostBalance();
        }

        private void ApplyBlueprintCostBalance()
        {
            if (Blueprints == null) return;
            for (int i = 0; i < Blueprints.Count; i++)
            {
                V5OrganismBlueprintDefinition blueprint = Blueprints[i];
                if (blueprint == null) continue;
                switch (blueprint.Kind)
                {
                    case V5OrganismBlueprintKind.Collector:
                        blueprint.RequiredFreeCells = CollectorRequiredFreeCells;
                        break;
                    case V5OrganismBlueprintKind.Harasser:
                        blueprint.RequiredFreeCells = HarasserRequiredFreeCells;
                        break;
                    case V5OrganismBlueprintKind.Fighter:
                        blueprint.RequiredFreeCells = FighterRequiredFreeCells;
                        break;
                    case V5OrganismBlueprintKind.Interdictor:
                        blueprint.RequiredFreeCells = InterdictorRequiredFreeCells;
                        break;
                    case V5OrganismBlueprintKind.Anchor:
                        blueprint.RequiredFreeCells = AnchorRequiredFreeCells;
                        break;
                    case V5OrganismBlueprintKind.Lacrymaria:
                        blueprint.RequiredFreeCells = LacrymariaRequiredFreeCells;
                        break;
                    case V5OrganismBlueprintKind.Volvox:
                        blueprint.RequiredFreeCells = VolvoxRequiredFreeCells;
                        break;
                    case V5OrganismBlueprintKind.Tardigrade:
                        blueprint.RequiredFreeCells = TardigradeRequiredFreeCells;
                        break;
                }
            }
        }

        private V5OrganismBlueprintDefinition CreateTardigradeBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Tardigrade;
            blueprint.Name = string.IsNullOrEmpty(BlueprintName) ? "Tardigrado" : BlueprintName;
            blueprint.RequiredFreeCells = TardigradeRequiredFreeCells;
            blueprint.SilhouetteSprite = BlueprintSilhouetteSprite;
            blueprint.SilhouetteTexture = BlueprintSilhouetteTexture;
            blueprint.UseTemporaryVolvoxSilhouette = false;
            blueprint.UseTemporaryCollectorSilhouette = false;
            blueprint.UseTemporaryHarasserSilhouette = false;
            blueprint.UseTemporaryAnchorSilhouette = false;
            blueprint.UseTemporaryInterdictorSilhouette = false;
            blueprint.ForceAllBodyRoles = false;
            blueprint.FormationRadiusX = FormationRadiusX;
            blueprint.FormationRadiusY = FormationRadiusY;
            blueprint.BaseMoveSpeed = BaseOrganismMoveSpeed;
            blueprint.LegMoveSpeedPerCell = LegMoveSpeedPerCell;
            blueprint.MaxMoveSpeed = MaxOrganismMoveSpeed;
            blueprint.BodyHpPerCell = BodyHpPerCell;
            blueprint.BodyResistancePerCell = BodyResistancePerCell;
            blueprint.PassiveBiomassPerCellPerSecond = 0f;
            blueprint.EngulfRateMultiplier = 1f;
            blueprint.ResourceSearchRange = 0f;
            blueprint.CombatDamageMultiplier = 1f;
            blueprint.MemberMaxHpMultiplier = 1f;
            blueprint.DamageKind = V5DamageKind.Piercing;
            blueprint.Armor = 0.20f;
            return blueprint;
        }

        private V5OrganismBlueprintDefinition CreateVolvoxBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Volvox;
            blueprint.Name = "Volvox";
            blueprint.RequiredFreeCells = VolvoxRequiredFreeCells;
            blueprint.UseTemporaryVolvoxSilhouette = true;
            blueprint.UseTemporaryCollectorSilhouette = false;
            blueprint.UseTemporaryHarasserSilhouette = false;
            blueprint.UseTemporaryAnchorSilhouette = false;
            blueprint.UseTemporaryInterdictorSilhouette = false;
            blueprint.ForceAllBodyRoles = true;
            blueprint.FormationRadiusX = 2.1f;
            blueprint.FormationRadiusY = 2.1f;
            blueprint.BaseMoveSpeed = 1.1f;
            blueprint.LegMoveSpeedPerCell = 0f;
            blueprint.MaxMoveSpeed = 1.7f;
            blueprint.BodyHpPerCell = 30f;
            blueprint.BodyResistancePerCell = 0.025f;
            blueprint.PassiveBiomassPerCellPerSecond = 0.045f;
            blueprint.EngulfRateMultiplier = 0.85f;
            blueprint.ResourceSearchRange = 0f;
            blueprint.CombatDamageMultiplier = 0.75f;
            blueprint.MemberMaxHpMultiplier = 1f;
            blueprint.DamageKind = V5DamageKind.Physical;
            blueprint.Armor = 0.30f;
            return blueprint;
        }

        private V5OrganismBlueprintDefinition CreateCollectorBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Collector;
            blueprint.Name = "Recolector";
            blueprint.RequiredFreeCells = CollectorRequiredFreeCells;
            blueprint.UseTemporaryVolvoxSilhouette = false;
            blueprint.UseTemporaryCollectorSilhouette = true;
            blueprint.UseTemporaryHarasserSilhouette = false;
            blueprint.UseTemporaryAnchorSilhouette = false;
            blueprint.UseTemporaryInterdictorSilhouette = false;
            blueprint.ForceAllBodyRoles = false;
            blueprint.FormationRadiusX = 1.75f;
            blueprint.FormationRadiusY = 0.48f;
            blueprint.BaseMoveSpeed = 1.95f;
            blueprint.LegMoveSpeedPerCell = 0.42f;
            blueprint.MaxMoveSpeed = 4.2f;
            blueprint.BodyHpPerCell = 0f;
            blueprint.BodyResistancePerCell = 0f;
            blueprint.PassiveBiomassPerCellPerSecond = 0f;
            blueprint.EngulfRateMultiplier = 2.8f;
            blueprint.ResourceSearchRange = 64f;
            blueprint.CombatDamageMultiplier = 0.22f;
            blueprint.MemberMaxHpMultiplier = 0.55f;
            blueprint.DamageKind = V5DamageKind.Physical;
            blueprint.Armor = 0f;
            return blueprint;
        }

        private V5OrganismBlueprintDefinition CreateHarasserBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Harasser;
            blueprint.Name = "Hostigador";
            blueprint.RequiredFreeCells = HarasserRequiredFreeCells;
            blueprint.UseTemporaryVolvoxSilhouette = false;
            blueprint.UseTemporaryCollectorSilhouette = false;
            blueprint.UseTemporaryHarasserSilhouette = true;
            blueprint.UseTemporaryAnchorSilhouette = false;
            blueprint.UseTemporaryInterdictorSilhouette = false;
            blueprint.ForceAllBodyRoles = false;
            blueprint.FormationRadiusX = 1.42f;
            blueprint.FormationRadiusY = 0.82f;
            blueprint.BaseMoveSpeed = 2.75f;
            blueprint.LegMoveSpeedPerCell = 0.78f;
            blueprint.MaxMoveSpeed = 7.4f;
            blueprint.BodyHpPerCell = 1.5f;
            blueprint.BodyResistancePerCell = 0.002f;
            blueprint.PassiveBiomassPerCellPerSecond = 0f;
            blueprint.EngulfRateMultiplier = 0.65f;
            blueprint.ResourceSearchRange = 0f;
            blueprint.CombatDamageMultiplier = 0.92f;
            blueprint.MemberMaxHpMultiplier = 0.58f;
            blueprint.DamageKind = V5DamageKind.Physical;
            blueprint.Armor = 0.05f;
            return blueprint;
        }

        private V5OrganismBlueprintDefinition CreateInterdictorBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Interdictor;
            blueprint.Name = "Dinoflagelado";
            blueprint.RequiredFreeCells = InterdictorRequiredFreeCells;
            blueprint.UseTemporaryVolvoxSilhouette = false;
            blueprint.UseTemporaryCollectorSilhouette = false;
            blueprint.UseTemporaryHarasserSilhouette = false;
            blueprint.UseTemporaryAnchorSilhouette = false;
            blueprint.UseTemporaryInterdictorSilhouette = true;
            blueprint.ForceAllBodyRoles = false;
            blueprint.FormationRadiusX = 1.55f;
            blueprint.FormationRadiusY = 0.72f;
            blueprint.BaseMoveSpeed = 2.2f;
            blueprint.LegMoveSpeedPerCell = 0.42f;
            blueprint.MaxMoveSpeed = 4.6f;
            blueprint.BodyHpPerCell = 3f;
            blueprint.BodyResistancePerCell = 0.006f;
            blueprint.PassiveBiomassPerCellPerSecond = 0f;
            blueprint.EngulfRateMultiplier = 0.45f;
            blueprint.ResourceSearchRange = 0f;
            blueprint.CombatDamageMultiplier = 0.48f;
            blueprint.MemberMaxHpMultiplier = 0.78f;
            blueprint.DamageKind = V5DamageKind.Chemical;
            blueprint.Armor = 0f;
            return blueprint;
        }

        private V5OrganismBlueprintDefinition CreateFighterBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Fighter;
            blueprint.Name = "Peleador";
            blueprint.RequiredFreeCells = FighterRequiredFreeCells;
            blueprint.UseTemporaryFighterSilhouette = true;
            blueprint.ForceAllBodyRoles = true;
            blueprint.FormationRadiusX = 1.35f;
            blueprint.FormationRadiusY = 1.15f;
            blueprint.BaseMoveSpeed = 1.1f;
            blueprint.LegMoveSpeedPerCell = 0f;
            blueprint.MaxMoveSpeed = 1.9f;
            blueprint.BodyHpPerCell = 7f;
            blueprint.BodyResistancePerCell = 0.008f;
            blueprint.PassiveBiomassPerCellPerSecond = 0f;
            blueprint.EngulfRateMultiplier = 0.55f;
            blueprint.ResourceSearchRange = 0f;
            blueprint.CombatDamageMultiplier = 1.4f;
            blueprint.MemberMaxHpMultiplier = 1f;
            blueprint.AttackRange = 1.1f;
            blueprint.RangedDamagePerSecond = 0f;
            blueprint.DamageKind = V5DamageKind.Physical;
            blueprint.Armor = 0.10f;
            return blueprint;
        }

        private V5OrganismBlueprintDefinition CreateAnchorBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Anchor;
            blueprint.Name = "Foramin\u00edfero";
            blueprint.RequiredFreeCells = AnchorRequiredFreeCells;
            blueprint.UseTemporaryVolvoxSilhouette = false;
            blueprint.UseTemporaryCollectorSilhouette = false;
            blueprint.UseTemporaryHarasserSilhouette = false;
            blueprint.UseTemporaryAnchorSilhouette = true;
            blueprint.UseTemporaryInterdictorSilhouette = false;
            blueprint.ForceAllBodyRoles = true;
            blueprint.FormationRadiusX = 1.35f;
            blueprint.FormationRadiusY = 1.35f;
            blueprint.BaseMoveSpeed = 1.25f;
            blueprint.LegMoveSpeedPerCell = 0f;
            blueprint.MaxMoveSpeed = 2.0f;
            blueprint.BodyHpPerCell = 20f;
            blueprint.BodyResistancePerCell = 0.030f;
            blueprint.PassiveBiomassPerCellPerSecond = 0f;
            blueprint.EngulfRateMultiplier = 0.35f;
            blueprint.ResourceSearchRange = 0f;
            blueprint.CombatDamageMultiplier = 0.62f;
            blueprint.MemberMaxHpMultiplier = 1.12f;
            blueprint.DamageKind = V5DamageKind.Physical;
            blueprint.Armor = 0.45f;
            return blueprint;
        }

        private V5OrganismBlueprintDefinition CreateLacrymariaBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = new V5OrganismBlueprintDefinition();
            blueprint.Kind = V5OrganismBlueprintKind.Lacrymaria;
            blueprint.Name = "Lacrymaria";
            blueprint.RequiredFreeCells = LacrymariaRequiredFreeCells;
            blueprint.UseTemporaryLacrymariaSilhouette = true;
            blueprint.ForceAllBodyRoles = true;
            blueprint.FormationRadiusX = 2.8f;
            blueprint.FormationRadiusY = 1.15f;
            blueprint.BaseMoveSpeed = 1.3f;
            blueprint.LegMoveSpeedPerCell = 0f;
            blueprint.MaxMoveSpeed = 2.5f;
            blueprint.BodyHpPerCell = 5f;
            blueprint.BodyResistancePerCell = 0.004f;
            blueprint.PassiveBiomassPerCellPerSecond = 0f;
            blueprint.EngulfRateMultiplier = 0.20f;
            blueprint.ResourceSearchRange = 0f;
            blueprint.CombatDamageMultiplier = 0.35f;
            blueprint.MemberMaxHpMultiplier = 0.75f;
            blueprint.AttackRange = 7f;
            blueprint.RangedDamagePerSecond = 6f;
            blueprint.DamageKind = V5DamageKind.Piercing;
            blueprint.Armor = 0f;
            return blueprint;
        }

        private void SyncLegacyBlueprintFields()
        {
            if (Blueprints == null || Blueprints.Count == 0) return;
            ActiveBlueprintIndex = Mathf.Clamp(ActiveBlueprintIndex, 0, Blueprints.Count - 1);
            V5OrganismBlueprintDefinition blueprint = Blueprints[ActiveBlueprintIndex];
            if (blueprint == null) return;
            BlueprintName = string.IsNullOrEmpty(blueprint.Name) ? "Organismo" : blueprint.Name;
            RequiredFreeCells = Mathf.Max(1, blueprint.RequiredFreeCells);
        }

        public void CycleBlueprint()
        {
            if (isRuntimeInstance && manager != null)
            {
                manager.CycleBlueprint();
                return;
            }

            EnsureBlueprints();
            int start = ActiveBlueprintIndex;
            for (int i = 1; i <= Blueprints.Count; i++)
            {
                int next = (start + i) % Blueprints.Count;
                if (!IsBlueprintUnlocked(Blueprints[next])) continue;
                ActiveBlueprintIndex = next;
                break;
            }
            SyncLegacyBlueprintFields();
            LastMessage = "Blueprint activo: " + BlueprintName + " (" + RequiredFreeCells + " celulas).";
        }

        public bool UnlockTardigrade()
        {
            if (isRuntimeInstance && manager != null) return manager.UnlockTardigrade();
            EnsureBlueprints();
            if (TardigradeUnlocked) return false;
            TardigradeUnlocked = true;
            LastMessage = "¡Evolución desbloqueada: Tardígrado!";
            return true;
        }

        public string UnlockedBlueprintsLabel()
        {
            EnsureBlueprints();
            string label = "";
            for (int i = 0; i < Blueprints.Count; i++)
            {
                if (!IsBlueprintUnlocked(Blueprints[i])) continue;
                if (label.Length > 0) label += ", ";
                label += Blueprints[i].Name;
            }
            return label.Length > 0 ? label : "ninguno";
        }

        public string PendingEvolutionLabel()
        {
            TardigradeUnlocked = true;
            return TardigradeUnlocked ? "Tardígrado desbloqueado" : "Derrotá al depredador para evolucionar al Tardígrado";
        }

        public bool IsBlueprintUnlockedFor(V5OrganismBlueprintKind kind)
        {
            if (isRuntimeInstance && manager != null) return manager.IsBlueprintUnlockedFor(kind);
            EnsureBlueprints();
            int index = IndexOfBlueprint(kind);
            return index >= 0 && IsBlueprintUnlocked(Blueprints[index]);
        }

        public string BlueprintNameFor(V5OrganismBlueprintKind kind)
        {
            if (isRuntimeInstance && manager != null) return manager.BlueprintNameFor(kind);
            EnsureBlueprints();
            int index = IndexOfBlueprint(kind);
            if (index < 0 || Blueprints[index] == null) return kind.ToString();
            return string.IsNullOrEmpty(Blueprints[index].Name) ? kind.ToString() : Blueprints[index].Name;
        }

        public int RequiredFreeCellsFor(V5OrganismBlueprintKind kind)
        {
            if (isRuntimeInstance && manager != null) return manager.RequiredFreeCellsFor(kind);
            EnsureBlueprints();
            int index = IndexOfBlueprint(kind);
            if (index < 0 || Blueprints[index] == null) return 0;
            return Mathf.Max(1, Blueprints[index].RequiredFreeCells);
        }

        public bool CanCreateOrganism(V5OrganismBlueprintKind kind, out string reason)
        {
            if (isRuntimeInstance && manager != null) return manager.CanCreateOrganism(kind, out reason);
            EnsureBlueprints();
            int index = IndexOfBlueprint(kind);
            if (index < 0 || Blueprints[index] == null)
            {
                reason = "sin blueprint";
                return false;
            }

            V5OrganismBlueprintDefinition blueprint = Blueprints[index];
            if (!IsBlueprintUnlocked(blueprint))
            {
                reason = "bloqueado";
                return false;
            }

            int limit = Mathf.Max(1, MaxActiveOrganisms);
            if (ActiveOrganismCount >= limit)
            {
                reason = "limite " + ActiveOrganismCount + "/" + limit;
                return false;
            }

            int missing = Mathf.Max(1, blueprint.RequiredFreeCells) - FreeCellCount();
            if (missing > 0)
            {
                reason = "faltan " + missing;
                return false;
            }

            reason = "listo";
            return true;
        }

        public bool CreateOrganism(V5OrganismBlueprintKind kind)
        {
            if (isRuntimeInstance && manager != null) return manager.CreateOrganism(kind);

            string reason;
            if (!CanCreateOrganism(kind, out reason))
            {
                LastMessage = BlueprintNameFor(kind) + ": " + reason + ".";
                return false;
            }

            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (gm == null || mother == null || !gm.CoreMode)
            {
                LastMessage = "Produccion solo disponible en Modo Nucleo.";
                return false;
            }

            int index = IndexOfBlueprint(kind);
            V5OrganismBlueprintDefinition blueprint = Blueprints[index];
            List<V5CellEntity> candidates = GatherFreeCandidates(gm, mother.transform.position);
            if (candidates.Count < Mathf.Max(1, blueprint.RequiredFreeCells))
            {
                LastMessage = blueprint.Name + ": faltan " + (Mathf.Max(1, blueprint.RequiredFreeCells) - candidates.Count) + " celulas.";
                return false;
            }

            V5OrganismMorph organism = CreateRuntimeOrganism();
            organism.ActiveBlueprintIndex = index;
            organism.SyncLegacyBlueprintFields();
            bool ok = organism.TryMorphFromCandidates(gm, mother, candidates);
            if (!ok)
            {
                activeOrganisms.Remove(organism);
                Destroy(organism.gameObject);
                LastMessage = organism.LastMessage;
                return false;
            }

            organism.name = "V5_Organism_" + organism.ActiveBlueprintName + "_" + ActiveOrganismCount;
            LastMessage = organism.ActiveBlueprintName + " creado desde la madre. Organismos activos: " + ActiveOrganismCount + "/" + Mathf.Max(1, MaxActiveOrganisms) + ".";
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null && organism.NucleusCell != null)
                feedback.Push("Unidad creada: " + organism.ActiveBlueprintName, organism.NucleusCell.transform.position, new Color(0.62f, 1f, 0.74f, 1f));
            return true;
        }

        public void ResetForNewRun()
        {
            if (isRuntimeInstance)
            {
                ForceClear();
                return;
            }

            for (int i = activeOrganisms.Count - 1; i >= 0; i--)
                if (activeOrganisms[i] != null) activeOrganisms[i].ForceClear();
            for (int i = activeEnemyOrganisms.Count - 1; i >= 0; i--)
                if (activeEnemyOrganisms[i] != null)
                {
                    activeEnemyOrganisms[i].enemyRewardIssued = true;
                    activeEnemyOrganisms[i].ForceClear();
                }
            for (int i = activeNeutralOrganisms.Count - 1; i >= 0; i--)
                if (activeNeutralOrganisms[i] != null)
                {
                    activeNeutralOrganisms[i].enemyRewardIssued = true;
                    activeNeutralOrganisms[i].ForceClear();
                }
            activeOrganisms.Clear();
            activeEnemyOrganisms.Clear();
            activeNeutralOrganisms.Clear();
            IsMorphed = false;
            hasMoveTarget = false;
            ResetPartCounters();
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode) return;

            if (!isRuntimeInstance)
            {
                EnsureBlueprints();
                PruneOrganisms();
                PruneEnemyOrganisms();
                PruneNeutralOrganisms();
                return;
            }

            if (IsMorphed) UpdateMorphedOrganism(gm);
        }

        public void ToggleMorph()
        {
            if (isRuntimeInstance)
            {
                Revert();
                return;
            }

            V5GameManager gm = V5GameManager.Instance;
            V5OrganismMorph selected = FindOrganismInSelection(gm != null && gm.Selection != null ? gm.Selection.Selected : null);
            if (selected != null)
            {
                selected.Revert();
                LastMessage = selected.LastMessage;
                PruneOrganisms();
                return;
            }

            TryMorph();
        }

        public bool CanMorph()
        {
            EnsureBlueprints();
            if (!isRuntimeInstance && ActiveOrganismCount >= Mathf.Max(1, MaxActiveOrganisms)) return false;
            return FreeCellCount() >= RequiredFreeCells;
        }

        public int FreeCellCount()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return 0;
            int count = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
                if (IsFreeMorphCandidate(gm.PlayerCells[i])) count++;
            return count;
        }

        public string HudStatus()
        {
            EnsureBlueprints();
            if (!isRuntimeInstance && ActiveOrganismCount >= Mathf.Max(1, MaxActiveOrganisms)) return "limite " + ActiveOrganismCount + "/" + Mathf.Max(1, MaxActiveOrganisms);
            int missing = RequiredFreeCells - FreeCellCount();
            string prefix = !isRuntimeInstance && ActiveOrganismCount > 0 ? ActiveOrganismCount + "/" + Mathf.Max(1, MaxActiveOrganisms) + " activos - " : "";
            return prefix + (missing <= 0 ? "listo" : "falta " + missing);
        }

        public void IssueMove(Vector2 target)
        {
            if (!isRuntimeInstance)
            {
                V5GameManager gm = V5GameManager.Instance;
                List<V5OrganismMorph> selected = FindOrganismsInSelection(gm != null && gm.Selection != null ? gm.Selection.Selected : null);
                if (selected.Count == 0 && ActiveOrganismCount == 1) selected.Add(activeOrganisms[0]);
                for (int i = 0; i < selected.Count; i++)
                    if (selected[i] != null) selected[i].IssueMove(target);
                return;
            }

            if (!IsMorphed) return;
            moveTarget = target;
            hasMoveTarget = true;
            hasHoldPosition = false;
            playerOrder = PlayerOrderState.Move;
            playerAttackTarget = null;
            hasRangedStandoff = false;

            V5CellEntity anchor = EnsureNucleus();
            if (anchor != null)
            {
                anchor.Directive = V5Directive.Move;
                anchor.DirectiveTarget = target;
            }
        }

        public void IssueAttack(V5CellEntity target)
        {
            if (!isRuntimeInstance)
            {
                V5GameManager gm = V5GameManager.Instance;
                List<V5OrganismMorph> selected = FindOrganismsInSelection(gm != null && gm.Selection != null ? gm.Selection.Selected : null);
                if (selected.Count == 0 && ActiveOrganismCount == 1) selected.Add(activeOrganisms[0]);
                for (int i = 0; i < selected.Count; i++)
                    if (selected[i] != null) selected[i].IssueAttack(target);
                return;
            }

            if (!IsMorphed || target == null) return;
            if (playerAttackTarget != target) hasRangedStandoff = false;
            playerOrder = PlayerOrderState.Attack;
            playerAttackTarget = target;
            moveTarget = target.transform.position;
            hasMoveTarget = true;
            hasHoldPosition = false;

            V5CellEntity anchor = EnsureNucleus();
            if (anchor != null)
            {
                anchor.Directive = V5Directive.Attack;
                anchor.AttackTarget = target;
                anchor.DirectiveTarget = target.transform.position;
            }
        }

        public void IssueAttackMove(Vector2 target)
        {
            if (!isRuntimeInstance)
            {
                V5GameManager gm = V5GameManager.Instance;
                List<V5OrganismMorph> selected = FindOrganismsInSelection(gm != null && gm.Selection != null ? gm.Selection.Selected : null);
                if (selected.Count == 0 && ActiveOrganismCount == 1) selected.Add(activeOrganisms[0]);
                for (int i = 0; i < selected.Count; i++)
                    if (selected[i] != null) selected[i].IssueAttackMove(target);
                return;
            }

            if (!IsMorphed) return;
            playerOrder = PlayerOrderState.AttackMove;
            playerAttackTarget = null;
            playerAttackMoveTarget = target;
            hasRangedStandoff = false;
            moveTarget = target;
            hasMoveTarget = true;
            hasHoldPosition = false;

            V5CellEntity anchor = EnsureNucleus();
            if (anchor != null)
            {
                anchor.AttackTarget = null;
                anchor.Directive = V5Directive.Move;
                anchor.DirectiveTarget = target;
            }
        }

        public void IssueFarm()
        {
            if (!isRuntimeInstance)
            {
                V5GameManager gm = V5GameManager.Instance;
                List<V5OrganismMorph> selected = FindOrganismsInSelection(gm != null && gm.Selection != null ? gm.Selection.Selected : null);
                if (selected.Count == 0 && ActiveOrganismCount == 1) selected.Add(activeOrganisms[0]);
                for (int i = 0; i < selected.Count; i++)
                    if (selected[i] != null) selected[i].IssueFarm();
                return;
            }

            if (!IsMorphed) return;
            playerOrder = PlayerOrderState.Farm;
            playerAttackTarget = null;
            hasRangedStandoff = false;
            hasMoveTarget = false;
            hasHoldPosition = false;

            V5CellEntity anchor = EnsureNucleus();
            if (anchor != null)
            {
                anchor.AttackTarget = null;
                anchor.Directive = V5Directive.Farm;
                anchor.DirectiveTarget = anchor.transform.position;
            }
        }

        private void HoldAtCurrentPosition(V5CellEntity anchor)
        {
            playerOrder = PlayerOrderState.Hold;
            playerAttackTarget = null;
            hasRangedStandoff = false;
            hasMoveTarget = false;
            if (anchor == null) return;
            holdPosition = ClampOrganismInsideMap(anchor.transform.position);
            hasHoldPosition = true;
            moveTarget = holdPosition;
            anchor.transform.position = holdPosition;
            anchor.AttackTarget = null;
            anchor.Directive = V5Directive.Move;
            anchor.DirectiveTarget = holdPosition;
        }

        public bool IsMember(V5CellEntity cell)
        {
            return cell != null && members.Contains(cell);
        }

        public bool IsMotherAnchor(V5CellEntity cell)
        {
            return false;
        }

        public bool IsOrganismCell(V5CellEntity cell)
        {
            if (!isRuntimeInstance)
                return FindOrganismForCell(cell) != null;
            return IsMember(cell);
        }

        public void NotifyCellUnavailable(V5CellEntity cell)
        {
            if (!isRuntimeInstance)
            {
                V5OrganismMorph organism = FindOrganismForCell(cell);
                if (organism != null) organism.NotifyCellUnavailable(cell);
                PruneOrganisms();
                return;
            }

            if (cell == null) return;
            if (cell == nucleusCell) nucleusCell = null;
            int index = members.IndexOf(cell);
            if (index >= 0) RemoveMemberAt(index);
            if (IsMorphed && members.Count == 0) ForceClear();
            else if (IsMorphed) EnsureNucleus();
        }

        private void AssignInitialNucleus()
        {
            if (members.Count == 0) return;
            int best = FindBestNucleusIndex();
            if (best < 0) return;

            nucleusCell = members[best];
            RecenterSlotsOnNucleus(best);
        }

        private void RecenterSlotsOnNucleus(int index)
        {
            if (index < 0 || index >= members.Count) return;
            V5CellEntity nucleus = members[index];
            Vector2 nucleusWorld = nucleus != null ? nucleus.transform.position : Vector2.zero;
            Vector2 nucleusSlot = index < slotOffsets.Count ? slotOffsets[index] : Vector2.zero;
            for (int i = 0; i < members.Count; i++)
            {
                if (members[i] != null && i < startOffsets.Count)
                    startOffsets[i] = (Vector2)members[i].transform.position - nucleusWorld;
                if (i < slotOffsets.Count) slotOffsets[i] -= nucleusSlot;
            }
        }

        private V5CellEntity EnsureNucleus()
        {
            if (!IsMorphed) return null;
            if (nucleusCell != null && nucleusCell.Stats.currentHp > 0f && members.Contains(nucleusCell)) return nucleusCell;

            int index = FindBestNucleusIndex();
            nucleusCell = index >= 0 ? members[index] : null;
            if (nucleusCell != null) RecenterSlotsOnNucleus(index);
            if (nucleusCell == null && members.Count == 0) ForceClear();
            return nucleusCell;
        }

        private int FindBestNucleusIndex()
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < members.Count; i++)
            {
                V5CellEntity cell = members[i];
                if (cell == null || cell.Stats.currentHp <= 0f) continue;
                Vector2 offset = i < slotOffsets.Count ? slotOffsets[i] : Vector2.zero;
                float d = offset.sqrMagnitude;
                if (d >= bestDistance) continue;
                bestDistance = d;
                best = i;
            }
            return best;
        }

        public bool SelectionContainsOrganism(List<V5CellEntity> selection)
        {
            if (!isRuntimeInstance) return FindOrganismInSelection(selection) != null;
            if (!IsMorphed || selection == null) return false;
            for (int i = 0; i < selection.Count; i++)
                if (IsOrganismCell(selection[i])) return true;
            return false;
        }

        public void SelectOrganism(bool additive)
        {
            if (!isRuntimeInstance)
            {
                V5GameManager currentGm = V5GameManager.Instance;
                V5OrganismMorph selected = FindOrganismInSelection(currentGm != null && currentGm.Selection != null ? currentGm.Selection.Selected : null);
                if (selected == null && ActiveOrganismCount == 1) selected = activeOrganisms[0];
                if (selected != null) selected.SelectOrganism(additive);
                return;
            }

            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Selection == null) return;
            if (!additive) gm.Selection.ClearSelection();
            for (int i = 0; i < members.Count; i++)
                if (members[i] != null) gm.Selection.AddSelection(members[i]);
        }

        public void SelectOrganismForCell(V5CellEntity cell, bool additive)
        {
            V5OrganismMorph organism = isRuntimeInstance ? (IsMember(cell) ? this : null) : FindOrganismForCell(cell);
            if (organism != null) organism.SelectOrganism(additive);
        }

        public int CountActiveOrganismsByKind(V5OrganismBlueprintKind kind)
        {
            if (isRuntimeInstance) return IsMorphed && ActiveBlueprintKind == kind ? 1 : 0;
            PruneOrganisms();
            int count = 0;
            for (int i = 0; i < activeOrganisms.Count; i++)
                if (activeOrganisms[i] != null && activeOrganisms[i].ActiveBlueprintKind == kind) count++;
            return count;
        }

        public void SelectOrganismsByKind(V5OrganismBlueprintKind kind, bool additive)
        {
            if (isRuntimeInstance)
            {
                if (ActiveBlueprintKind == kind) SelectOrganism(additive);
                return;
            }

            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Selection == null) return;
            if (!additive) gm.Selection.ClearSelection();
            PruneOrganisms();
            for (int i = 0; i < activeOrganisms.Count; i++)
            {
                V5OrganismMorph organism = activeOrganisms[i];
                if (organism == null || organism.ActiveBlueprintKind != kind) continue;
                for (int m = 0; m < organism.members.Count; m++)
                    if (organism.members[m] != null) gm.Selection.AddSelection(organism.members[m]);
            }
        }

        public float PlayerInterdictionSpeedMultiplierAt(Vector2 world)
        {
            if (isRuntimeInstance)
            {
                if (!IsMorphed || ActiveBlueprintKind != V5OrganismBlueprintKind.Interdictor || isEnemyOrganism || isNeutralOrganism) return 1f;
                V5CellEntity anchor = EnsureNucleus();
                if (anchor == null) return 1f;
                float radius = InterdictorAuraRadius();
                float distance = Vector2.Distance(world, anchor.transform.position);
                if (distance >= radius) return 1f;
                float strength = 1f - Mathf.Clamp01(distance / radius);
                return Mathf.Clamp(1f - 0.42f * strength, 0.52f, 1f);
            }

            PruneOrganisms();
            float multiplier = 1f;
            for (int i = 0; i < activeOrganisms.Count; i++)
            {
                V5OrganismMorph organism = activeOrganisms[i];
                if (organism == null) continue;
                multiplier = Mathf.Min(multiplier, organism.PlayerInterdictionSpeedMultiplierAt(world));
            }
            return multiplier;
        }

        public V5OrganismMorph SpawnEnemyOrganism(V5OrganismBlueprintKind kind, Vector2 center, int rewardCells)
        {
            if (isRuntimeInstance && manager != null) return manager.SpawnEnemyOrganism(kind, center, rewardCells);
            return SpawnNonPlayerOrganism(kind, center, rewardCells, false);
        }

        public V5OrganismMorph SpawnNeutralOrganism(V5OrganismBlueprintKind kind, Vector2 center, int rewardDna)
        {
            if (isRuntimeInstance && manager != null) return manager.SpawnNeutralOrganism(kind, center, rewardDna);
            return SpawnNonPlayerOrganism(kind, center, rewardDna, true);
        }

        public void ConfigureNeutralRoam(Vector2 center, float radius)
        {
            if (!isRuntimeInstance || !isNeutralOrganism) return;
            neutralRoamCenter = ClampOrganismInsideMap(center);
            neutralRoamRadius = Mathf.Clamp(radius, 6f, 8f);
            hasNeutralRoamCenter = true;
            nextNeutralWanderAt = Time.time + Random.Range(2.5f, 5.5f);
        }

        private V5OrganismMorph SpawnNonPlayerOrganism(V5OrganismBlueprintKind kind, Vector2 center, int rewardValue, bool neutral)
        {
            EnsureBlueprints();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null) return null;

            int blueprintIndex = IndexOfBlueprint(kind);
            if (blueprintIndex < 0) return null;
            V5OrganismBlueprintDefinition blueprint = Blueprints[blueprintIndex];
            int count = Mathf.Max(1, blueprint.RequiredFreeCells);
            List<V5CellEntity> candidates = new List<V5CellEntity>(count);
            for (int i = 0; i < count; i++)
            {
                float angle = i * 2.3999632f;
                float ring = Mathf.Sqrt(i + 0.5f) * 0.22f;
                Vector2 pos = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ring;
                V5CellEntity cell = gm.CellFactory.SpawnNeutral(pos, V5EvolutionPath.Tardigrade);
                if (cell == null) continue;
                cell.name = (neutral ? "V5_NeutralOrganism_" : "V5_EnemyOrganism_") + blueprint.Name + "_Cell";
                cell.IsCoreNeutral = neutral;
                cell.Stats.maxHp = Mathf.Max(8f, cell.Stats.maxHp);
                cell.Stats.currentHp = cell.Stats.maxHp;
                cell.Stats.radius = 0.48f;
                cell.Stats.speed = Mathf.Max(1.1f, cell.Stats.speed);
                cell.Stats.sensorRange = neutral ? 7f : (gm.Environment != null ? gm.Environment.MapRadius * 2.2f : 70f);
                cell.Stats.attackRange = 1.1f;
                cell.Stats.physicalDamagePerSecond = Mathf.Max(cell.Stats.physicalDamagePerSecond, 1.0f);
                candidates.Add(cell);
            }

            if (candidates.Count < count) return null;
            V5OrganismMorph organism = CreateRuntimeOrganism(!neutral, neutral);
            organism.ActiveBlueprintIndex = blueprintIndex;
            organism.SyncLegacyBlueprintFields();
            organism.enemyRewardCells = Mathf.Max(0, rewardValue);
            bool ok = organism.TryMorphFromCandidates(gm, null, candidates, center);
            if (!ok)
            {
                activeEnemyOrganisms.Remove(organism);
                activeNeutralOrganisms.Remove(organism);
                Destroy(organism.gameObject);
                return null;
            }

            organism.name = (neutral ? "V5_NeutralOrganism_" : "V5_EnemyOrganism_") + organism.ActiveBlueprintName;
            organism.LastMessage = (neutral ? "Organismo neutral formado: " : "Organismo enemigo formado: ") + organism.ActiveBlueprintName + ".";
            if (neutral) organism.ConfigureNeutralRoam(center, 7f);
            if (!neutral) organism.EnsureEnemyBrain();
            return organism;
        }

        public V5OrganismMorph FirstActiveEnemyOrganism()
        {
            if (isRuntimeInstance && manager != null) return manager.FirstActiveEnemyOrganism();
            PruneEnemyOrganisms();
            return activeEnemyOrganisms.Count > 0 ? activeEnemyOrganisms[0] : null;
        }

        public V5OrganismMorph EnemyOrganismForCell(V5CellEntity cell)
        {
            if (isRuntimeInstance && manager != null) return manager.EnemyOrganismForCell(cell);
            if (cell == null) return null;
            PruneEnemyOrganisms();
            for (int i = 0; i < activeEnemyOrganisms.Count; i++)
                if (activeEnemyOrganisms[i] != null && activeEnemyOrganisms[i].IsMember(cell)) return activeEnemyOrganisms[i];
            PruneNeutralOrganisms();
            for (int i = 0; i < activeNeutralOrganisms.Count; i++)
                if (activeNeutralOrganisms[i] != null && activeNeutralOrganisms[i].IsMember(cell)) return activeNeutralOrganisms[i];
            return null;
        }

        private V5OrganismMorph CreateRuntimeOrganism()
        {
            return CreateRuntimeOrganism(false);
        }

        private V5OrganismMorph CreateRuntimeOrganism(bool enemy)
        {
            return CreateRuntimeOrganism(enemy, false);
        }

        private V5OrganismMorph CreateRuntimeOrganism(bool enemy, bool neutral)
        {
            GameObject go = new GameObject(enemy ? "V5_EnemyOrganism_" + ActiveBlueprintName : (neutral ? "V5_NeutralOrganism_" + ActiveBlueprintName : "V5_Organism_" + ActiveBlueprintName + "_" + (ActiveOrganismCount + 1)));
            go.transform.SetParent(transform.parent, true);
            V5OrganismMorph organism = go.AddComponent<V5OrganismMorph>();
            organism.CopyRuntimeConfigFrom(this);
            organism.isRuntimeInstance = true;
            organism.isEnemyOrganism = enemy;
            organism.isNeutralOrganism = neutral;
            organism.manager = this;
            if (enemy) activeEnemyOrganisms.Add(organism);
            else if (neutral) activeNeutralOrganisms.Add(organism);
            else activeOrganisms.Add(organism);
            return organism;
        }

        private int IndexOfBlueprint(V5OrganismBlueprintKind kind)
        {
            EnsureBlueprints();
            for (int i = 0; i < Blueprints.Count; i++)
                if (Blueprints[i] != null && Blueprints[i].Kind == kind) return i;
            return -1;
        }

        private void CopyRuntimeConfigFrom(V5OrganismMorph source)
        {
            BlueprintName = source.BlueprintName;
            RequiredFreeCells = source.RequiredFreeCells;
            Blueprints = source.Blueprints;
            ActiveBlueprintIndex = source.ActiveBlueprintIndex;
            TardigradeUnlocked = source.TardigradeUnlocked;
            MaxActiveOrganisms = source.MaxActiveOrganisms;
            RevertDeathFraction = source.RevertDeathFraction;
            BlueprintSilhouetteSprite = source.BlueprintSilhouetteSprite;
            BlueprintSilhouetteTexture = source.BlueprintSilhouetteTexture;
            SilhouetteAlphaThreshold = source.SilhouetteAlphaThreshold;
            SilhouetteSampleResolutionX = source.SilhouetteSampleResolutionX;
            FormationRadiusX = source.FormationRadiusX;
            FormationRadiusY = source.FormationRadiusY;
            FormationFollowSpeed = source.FormationFollowSpeed;
            MorphFlowSeconds = source.MorphFlowSeconds;
            RotationLerpSpeed = source.RotationLerpSpeed;
            RevertScatterSpeed = source.RevertScatterSpeed;
            BaseOrganismMoveSpeed = source.BaseOrganismMoveSpeed;
            LegMoveSpeedPerCell = source.LegMoveSpeedPerCell;
            MaxOrganismMoveSpeed = source.MaxOrganismMoveSpeed;
            EngulfRange = source.EngulfRange;
            MouthEngulfRatePerCell = source.MouthEngulfRatePerCell;
            BaseEngulfRateMouthFraction = source.BaseEngulfRateMouthFraction;
            MaxEngulfRate = source.MaxEngulfRate;
            BodyHpPerCell = source.BodyHpPerCell;
            BodyResistancePerCell = source.BodyResistancePerCell;
            InterdictorToxicDamagePerSecond = source.InterdictorToxicDamagePerSecond;
            CollapseLiveCellFraction = source.CollapseLiveCellFraction;
            CollapseMinimumLiveCells = source.CollapseMinimumLiveCells;
            SyncLegacyBlueprintFields();
        }

        private void PruneOrganisms()
        {
            for (int i = activeOrganisms.Count - 1; i >= 0; i--)
            {
                V5OrganismMorph organism = activeOrganisms[i];
                if (organism == null || !organism.isRuntimeInstance || !organism.IsMorphed)
                    activeOrganisms.RemoveAt(i);
            }
        }

        private void PruneEnemyOrganisms()
        {
            for (int i = activeEnemyOrganisms.Count - 1; i >= 0; i--)
            {
                V5OrganismMorph organism = activeEnemyOrganisms[i];
                if (organism == null || !organism.isRuntimeInstance || !organism.IsMorphed)
                    activeEnemyOrganisms.RemoveAt(i);
            }
        }

        private void PruneNeutralOrganisms()
        {
            for (int i = activeNeutralOrganisms.Count - 1; i >= 0; i--)
            {
                V5OrganismMorph organism = activeNeutralOrganisms[i];
                if (organism == null || !organism.isRuntimeInstance || !organism.IsMorphed)
                    activeNeutralOrganisms.RemoveAt(i);
            }
        }

        private V5OrganismMorph FindOrganismForCell(V5CellEntity cell)
        {
            if (cell == null) return null;
            PruneOrganisms();
            for (int i = 0; i < activeOrganisms.Count; i++)
                if (activeOrganisms[i] != null && activeOrganisms[i].IsMember(cell)) return activeOrganisms[i];
            return null;
        }

        private V5OrganismMorph FindPlayerOrganismForCell(V5CellEntity cell)
        {
            V5OrganismMorph root = isRuntimeInstance && manager != null ? manager : this;
            return root.FindOrganismForCell(cell);
        }

        private V5OrganismMorph FindOrganismInSelection(List<V5CellEntity> selection)
        {
            if (selection == null) return null;
            for (int i = 0; i < selection.Count; i++)
            {
                V5OrganismMorph organism = FindOrganismForCell(selection[i]);
                if (organism != null) return organism;
            }
            return null;
        }

        private List<V5OrganismMorph> FindOrganismsInSelection(List<V5CellEntity> selection)
        {
            List<V5OrganismMorph> result = new List<V5OrganismMorph>(4);
            if (selection == null) return result;
            for (int i = 0; i < selection.Count; i++)
            {
                V5OrganismMorph organism = FindOrganismForCell(selection[i]);
                if (organism != null && !result.Contains(organism)) result.Add(organism);
            }
            return result;
        }

        public bool TryMorph()
        {
            EnsureBlueprints();
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (gm == null || mother == null || !gm.CoreMode) return false;

            if (!isRuntimeInstance)
            {
                if (ActiveOrganismCount >= Mathf.Max(1, MaxActiveOrganisms))
                {
                    LastMessage = "Limite de organismos activos: " + ActiveOrganismCount + "/" + Mathf.Max(1, MaxActiveOrganisms) + ".";
                    return false;
                }

                List<V5CellEntity> selectedCandidates = GatherSelectedFreeCandidates(gm);
                List<V5CellEntity> morphCandidates = selectedCandidates.Count > 0 ? selectedCandidates : GatherFreeCandidates(gm, mother.transform.position);
                if (morphCandidates.Count < RequiredFreeCells)
                {
                    LastMessage = "Morfar requiere " + RequiredFreeCells + " celulas libres.";
                    return false;
                }

                V5OrganismMorph organism = CreateRuntimeOrganism();
                bool ok = organism.TryMorphFromCandidates(gm, mother, morphCandidates);
                if (!ok)
                {
                    activeOrganisms.Remove(organism);
                    Destroy(organism.gameObject);
                    LastMessage = organism.LastMessage;
                    return false;
                }

                LastMessage = organism.LastMessage + " Organismos activos: " + ActiveOrganismCount + "/" + Mathf.Max(1, MaxActiveOrganisms) + ".";
                return true;
            }

            List<V5CellEntity> candidates = GatherFreeCandidates(gm, mother.transform.position);
            return TryMorphFromCandidates(gm, mother, candidates);
        }

        private bool TryMorphFromCandidates(V5GameManager gm, V5CellEntity mother, List<V5CellEntity> candidates)
        {
            Vector2 anchor = mother != null ? (Vector2)mother.transform.position : Vector2.zero;
            return TryMorphFromCandidates(gm, mother, candidates, anchor);
        }

        private bool TryMorphFromCandidates(V5GameManager gm, V5CellEntity mother, List<V5CellEntity> candidates, Vector2 anchor)
        {
            if (candidates.Count < RequiredFreeCells)
            {
                LastMessage = "Morfar requiere " + RequiredFreeCells + " celulas libres.";
                return false;
            }

            members.Clear();
            slotOffsets.Clear();
            startOffsets.Clear();
            slotRoles.Clear();
            originalMaxHp.Clear();
            originalPhysicalArmor.Clear();
            originalAttackRange.Clear();
            nucleusCell = null;
            playerOrder = PlayerOrderState.None;
            playerAttackTarget = null;
            ResetPartCounters();
            MorphFlow01 = 0f;
            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            List<MorphSlot> silhouetteSlots = BuildSilhouetteSlots(RequiredFreeCells);
            bool[] usedSlots = new bool[silhouetteSlots.Count];
            for (int i = 0; i < RequiredFreeCells; i++)
            {
                V5CellEntity cell = candidates[i];
                Vector2 localStart = (Vector2)cell.transform.position - anchor;
                int slotIndex = PickNearestUnusedSlot(silhouetteSlots, usedSlots, localStart);
                usedSlots[slotIndex] = true;
                MorphSlot slot = silhouetteSlots[slotIndex];

                members.Add(cell);
                startOffsets.Add(localStart);
                slotOffsets.Add(slot.offset);
                slotRoles.Add(slot.role);
                originalMaxHp.Add(cell.Stats.maxHp);
                originalPhysicalArmor.Add(cell.Stats.physicalArmor);
                originalAttackRange.Add(cell.Stats.attackRange);
                CountInitialSlot(slot.role);
                cell.Mother = mother != null ? mother : cell;
                cell.SetOrganismMorphSlot(i, slot.role);
                ApplyBlueprintSlotStats(cell, slot.role, blueprint);
            }
            AssignInitialNucleus();
            holdPosition = nucleusCell != null ? ClampOrganismInsideMap(nucleusCell.transform.position) : ClampOrganismInsideMap(anchor);
            hasHoldPosition = true;

            IsMorphed = true;
            hasMoveTarget = false;
            morphStartedAt = Time.time;
            organismAngle = 0f;
            RefreshLivePartState();
            LastMessage = BlueprintName + " formado con silueta " + (UsingTemporarySilhouette ? "temporal" : "PNG") + ": " + members.Count + " celulas.";
            if (!isEnemyOrganism) SelectOrganism(false);
            return true;
        }

        public void Revert()
        {
            if (!isRuntimeInstance)
            {
                V5GameManager gmManager = V5GameManager.Instance;
                V5OrganismMorph selected = FindOrganismInSelection(gmManager != null && gmManager.Selection != null ? gmManager.Selection.Selected : null);
                if (selected != null) selected.Revert();
                return;
            }

            if (!IsMorphed) return;

            V5GameManager gm = V5GameManager.Instance;
            Vector2 center = nucleusCell != null ? nucleusCell.transform.position : transform.position;
            int deadCount = Mathf.Clamp(Mathf.RoundToInt(members.Count * RevertDeathFraction), 0, members.Count);
            int survivorCount = members.Count - deadCount;

            for (int i = 0; i < members.Count; i++)
            {
                V5CellEntity cell = members[i];
                if (cell == null) continue;

                RestoreBlueprintSlotStats(cell, i);
                Vector2 outward = ((Vector2)cell.transform.position - center);
                if (outward.sqrMagnitude < 0.01f) outward = Random.insideUnitCircle;
                outward.Normalize();

                cell.ClearOrganismMorphSlot();
                cell.SetSelected(false);
                cell.transform.position += (Vector3)(outward * Random.Range(0.25f, 0.75f));

                if (i >= survivorCount)
                {
                    V5MorphFadeOut fade = cell.gameObject.AddComponent<V5MorphFadeOut>();
                    fade.Begin(outward * RevertScatterSpeed, 0.95f);
                }
                else
                {
                    cell.ApplyCellMode(V5CellModeId.FollowLineage);
                    cell.DirectiveTarget = (Vector2)cell.transform.position + outward * Random.Range(1.5f, 2.4f);
                }
            }

            members.Clear();
            slotOffsets.Clear();
            startOffsets.Clear();
            slotRoles.Clear();
            originalMaxHp.Clear();
            originalPhysicalArmor.Clear();
            originalAttackRange.Clear();
            nucleusCell = null;
            IsMorphed = false;
            hasMoveTarget = false;
            playerOrder = PlayerOrderState.None;
            playerAttackTarget = null;
            MorphFlow01 = 0f;
            ResetPartCounters();
            LastMessage = BlueprintName + " revertido: " + survivorCount + " libres, " + deadCount + " perdidas.";
            if (gm != null && gm.Selection != null) gm.Selection.ClearSelection();
            DisposeRuntimeInstance();
        }

        private void UpdateMorphedOrganism(V5GameManager gm)
        {
            V5CellEntity nucleus = EnsureNucleus();
            V5CellEntity bank = (isEnemyOrganism || isNeutralOrganism) ? nucleus : (gm != null ? gm.MotherCell : null);
            if (nucleus == null || bank == null)
            {
                ForceClear();
                return;
            }

            RefreshLivePartState();
            if (members.Count == 0)
            {
                ForceClear();
                return;
            }

            nucleus = EnsureNucleus();
            if (nucleus == null)
            {
                ForceClear();
                return;
            }
            if (isEnemyOrganism || isNeutralOrganism)
            {
                bank = nucleus;
                if (isEnemyOrganism) EnsureEnemyBrain();
            }

            if (!isEnemyOrganism && !isNeutralOrganism && playerOrder == PlayerOrderState.None)
            {
                hasMoveTarget = false;
                if (!hasHoldPosition)
                {
                    holdPosition = ClampOrganismInsideMap(nucleus.transform.position);
                    hasHoldPosition = true;
                }
                holdPosition = ClampOrganismInsideMap(holdPosition);
                nucleus.transform.position = holdPosition;
                moveTarget = holdPosition;
                nucleus.AttackTarget = null;
                nucleus.Directive = V5Directive.Move;
                nucleus.DirectiveTarget = holdPosition;
            }
            else if (playerOrder == PlayerOrderState.Attack)
            {
                if (playerAttackTarget == null || playerAttackTarget.Stats.currentHp <= 0f)
                {
                    HoldAtCurrentPosition(nucleus);
                }
                else
                {
                    moveTarget = playerAttackTarget.transform.position;
                    hasMoveTarget = true;
                    nucleus.Directive = V5Directive.Attack;
                    nucleus.AttackTarget = playerAttackTarget;
                    nucleus.DirectiveTarget = moveTarget;
                }
            }
            else if (playerOrder == PlayerOrderState.Hold)
            {
                hasMoveTarget = false;
                if (!hasHoldPosition)
                {
                    holdPosition = ClampOrganismInsideMap(nucleus.transform.position);
                    hasHoldPosition = true;
                }
                holdPosition = ClampOrganismInsideMap(holdPosition);
                nucleus.transform.position = holdPosition;
                nucleus.Directive = V5Directive.Move;
                nucleus.DirectiveTarget = holdPosition;
            }
            else if (playerOrder == PlayerOrderState.AttackMove)
            {
                if (playerAttackTarget == null || playerAttackTarget.Stats.currentHp <= 0f)
                {
                    hasRangedStandoff = false;
                    playerAttackTarget = FindAttackMoveTarget(gm, nucleus, playerAttackMoveTarget);
                }

                if (playerAttackTarget != null && playerAttackTarget.Stats.currentHp > 0f)
                {
                    moveTarget = playerAttackTarget.transform.position;
                    hasMoveTarget = true;
                    nucleus.Directive = V5Directive.Attack;
                    nucleus.AttackTarget = playerAttackTarget;
                    nucleus.DirectiveTarget = moveTarget;
                }
                else
                {
                    moveTarget = playerAttackMoveTarget;
                    hasMoveTarget = true;
                    nucleus.AttackTarget = null;
                    nucleus.Directive = V5Directive.Move;
                    nucleus.DirectiveTarget = moveTarget;
                }
            }
            else if (playerOrder == PlayerOrderState.Farm)
            {
                TickCollectorAutofarm(gm, nucleus, true);
            }

            if (IsRangedBlueprint() && hasRangedStandoff && IsValidRangedTarget(playerAttackTarget))
            {
                float range = ActiveAttackRange;
                if (Vector2.SqrMagnitude((Vector2)playerAttackTarget.transform.position - rangedStandoffPosition) <= range * range)
                {
                    nucleus.transform.position = rangedStandoffPosition;
                    hasMoveTarget = false;
                    nucleus.Directive = V5Directive.Attack;
                    nucleus.AttackTarget = playerAttackTarget;
                    nucleus.DirectiveTarget = playerAttackTarget.transform.position;
                }
                else
                {
                    hasRangedStandoff = false;
                }
            }

            TickNeutralRoaming(nucleus);

            if (hasMoveTarget)
            {
                Vector2 pos = nucleus.transform.position;
                Vector2 desired = moveTarget - pos;
                float arrivalRadius = (playerOrder == PlayerOrderState.Attack || playerOrder == PlayerOrderState.AttackMove) && playerAttackTarget != null
                    ? AttackArrivalRadius(playerAttackTarget)
                    : Mathf.Clamp(OrganismSizeRadius() * 0.22f, 0.35f, 1.05f);
                if (desired.sqrMagnitude > arrivalRadius * arrivalRadius)
                {
                    float desiredAngle = Mathf.Atan2(desired.y, desired.x) * Mathf.Rad2Deg;
                    organismAngle = Mathf.LerpAngle(organismAngle, desiredAngle, Mathf.Clamp01(RotationLerpSpeed * Time.deltaTime));
                    float moveSpeed = Mathf.Max(CurrentOrganismMoveSpeed, ActiveBaseMoveSpeed());
                    if (isEnemyOrganism && gm != null && gm.CoreMotherProduction != null)
                        moveSpeed *= gm.CoreMotherProduction.EnemySpeedMultiplierAt(pos);
                    if ((isEnemyOrganism || isNeutralOrganism) && gm != null && gm.OrganismMorph != null)
                        moveSpeed *= gm.OrganismMorph.PlayerInterdictionSpeedMultiplierAt(pos);
                    Vector2 step = desired.normalized * moveSpeed * Time.deltaTime;
                    if (step.sqrMagnitude > desired.sqrMagnitude) step = desired;
                    Vector2 nextPos = ClampOrganismInsideMap((Vector2)nucleus.transform.position + step);
                    nucleus.transform.position = nextPos;
                    if (Vector2.SqrMagnitude(nextPos - moveTarget) < arrivalRadius * arrivalRadius || Vector2.SqrMagnitude(nextPos - pos) < 0.0001f)
                        moveTarget = nextPos;
                    nucleus.Directive = V5Directive.Move;
                    nucleus.DirectiveTarget = moveTarget;
                }
                else
                {
                    if (playerOrder == PlayerOrderState.Move) HoldAtCurrentPosition(nucleus);
                    else if (playerOrder == PlayerOrderState.Attack && playerAttackTarget != null)
                    {
                        PlantRangedStandoff(nucleus);
                        nucleus.Directive = V5Directive.Attack;
                        nucleus.AttackTarget = playerAttackTarget;
                        nucleus.DirectiveTarget = playerAttackTarget.transform.position;
                    }
                    else if (playerOrder == PlayerOrderState.AttackMove)
                    {
                        if (playerAttackTarget != null && playerAttackTarget.Stats.currentHp > 0f)
                        {
                            PlantRangedStandoff(nucleus);
                            nucleus.Directive = V5Directive.Attack;
                            nucleus.AttackTarget = playerAttackTarget;
                            nucleus.DirectiveTarget = playerAttackTarget.transform.position;
                        }
                        else
                        {
                            HoldAtCurrentPosition(nucleus);
                        }
                    }
                    else
                    {
                        hasMoveTarget = false;
                        nucleus.DirectiveTarget = nucleus.transform.position;
                    }
                }
            }

            if (hasRangedStandoff) nucleus.transform.position = rangedStandoffPosition;
            nucleus.transform.position = ClampOrganismInsideMap(nucleus.transform.position);
            Vector2 anchor = nucleus.transform.position;
            nucleus.transform.rotation = Quaternion.Euler(0f, 0f, organismAngle);
            MorphFlow01 = Mathf.Clamp01((Time.time - morphStartedAt) / Mathf.Max(0.05f, MorphFlowSeconds));
            float flow = MorphFlow01 * MorphFlow01 * (3f - 2f * MorphFlow01);
            for (int i = members.Count - 1; i >= 0; i--)
            {
                V5CellEntity cell = members[i];
                if (cell == null) continue;

                int safeIndex = Mathf.Min(i, slotOffsets.Count - 1);
                Vector2 localOffset = Vector2.Lerp(startOffsets[safeIndex], slotOffsets[safeIndex], flow);
                Vector2 slot = anchor + Rotate(localOffset, organismAngle);
                cell.MoveMorphedToSlot(slot, FormationFollowSpeed);
                cell.SetSelected(cell.Selected);
            }

            TickCollectorAutofarm(gm, nucleus, false);
            ApplyPassiveBlueprintProduction(bank);
            ApplyInterdictorToxicAura(gm, nucleus);
            ApplyRangedAttack(gm, nucleus);
            EngulfTouchingBiomass(gm, nucleus, bank);
            EngulfTouchingEnemies(gm, nucleus, bank);
            if (members.Count == 0) ForceClear();
        }

        private void TickNeutralRoaming(V5CellEntity nucleus)
        {
            if (!isNeutralOrganism || nucleus == null) return;
            if (!hasNeutralRoamCenter) ConfigureNeutralRoam(nucleus.transform.position, 7f);

            neutralRoamCenter = ClampOrganismInsideMap(neutralRoamCenter);
            Vector2 position = nucleus.transform.position;
            Vector2 fromCenter = position - neutralRoamCenter;
            float roamRadius = Mathf.Clamp(neutralRoamRadius, 6f, 8f);
            if (fromCenter.sqrMagnitude > roamRadius * roamRadius)
            {
                playerOrder = PlayerOrderState.Move;
                playerAttackTarget = null;
                moveTarget = neutralRoamCenter;
                hasMoveTarget = true;
                nucleus.AttackTarget = null;
                nucleus.Directive = V5Directive.Move;
                nucleus.DirectiveTarget = moveTarget;
                return;
            }

            if ((playerOrder == PlayerOrderState.None || playerOrder == PlayerOrderState.Hold) && Time.time >= nextNeutralWanderAt)
            {
                nextNeutralWanderAt = Time.time + Random.Range(4f, 7f);
                playerOrder = PlayerOrderState.Move;
                moveTarget = ClampOrganismInsideMap(neutralRoamCenter + Random.insideUnitCircle * roamRadius);
                hasMoveTarget = true;
                nucleus.AttackTarget = null;
                nucleus.Directive = V5Directive.Move;
                nucleus.DirectiveTarget = moveTarget;
            }

            if (!hasMoveTarget) return;
            Vector2 targetOffset = moveTarget - neutralRoamCenter;
            if (targetOffset.sqrMagnitude <= roamRadius * roamRadius) return;
            moveTarget = neutralRoamCenter + targetOffset.normalized * roamRadius;
            moveTarget = ClampOrganismInsideMap(moveTarget);
            nucleus.DirectiveTarget = moveTarget;
        }

        private Vector2 ClampOrganismInsideMap(Vector2 position)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5EnvironmentGrid env = gm != null ? gm.Environment : null;
            if (env == null) return position;

            float allowedRadius = Mathf.Max(0.5f, env.MapRadius - Mathf.Max(0.15f, OrganismSizeRadius() * 0.35f));
            if (position.sqrMagnitude <= allowedRadius * allowedRadius) return position;
            if (position.sqrMagnitude <= 0.0001f) return position;
            return position.normalized * allowedRadius;
        }

        private void OnGUI()
        {
            if (!isRuntimeInstance || !IsMorphed || members.Count == 0) return;
            V5CellEntity nucleus = EnsureNucleus();
            if (nucleus == null) return;
            Camera cam = Camera.main;
            if (cam == null) return;

            Vector3 world = nucleus.transform.position + Vector3.up * (OrganismSizeRadius() + 0.85f);
            Vector3 screen = cam.WorldToScreenPoint(world);
            if (screen.z <= 0f) return;

            float width = Mathf.Clamp(34f + members.Count * 0.45f, 38f, 74f);
            float height = 6f;
            Rect bg = new Rect(screen.x - width * 0.5f, Screen.height - screen.y, width, height);
            float hp01 = Health01;
            Color previous = GUI.color;
            GUI.color = new Color(0.02f, 0.03f, 0.03f, 0.82f);
            GUI.DrawTexture(bg, Texture2D.whiteTexture);
            GUI.color = Color.Lerp(new Color(1f, 0.18f, 0.12f, 0.95f), new Color(0.34f, 1f, 0.42f, 0.95f), hp01);
            GUI.DrawTexture(new Rect(bg.x + 1f, bg.y + 1f, Mathf.Max(0f, (bg.width - 2f) * hp01), bg.height - 2f), Texture2D.whiteTexture);
            GUI.color = new Color(0.86f, 1f, 0.9f, 0.95f);
            GUI.Label(new Rect(bg.x - 8f, bg.y - 14f, bg.width + 16f, 14f), ActiveBlueprintName + " " + Mathf.RoundToInt(hp01 * 100f) + "%");
            GUI.color = previous;
        }

        private float OrganismCurrentHp()
        {
            float hp = 0f;
            for (int i = 0; i < members.Count; i++)
                if (members[i] != null && members[i].Stats.currentHp > 0f) hp += members[i].Stats.currentHp;
            return hp;
        }

        private float OrganismMaxHp()
        {
            float hp = 0f;
            for (int i = 0; i < members.Count; i++)
                if (members[i] != null) hp += Mathf.Max(1f, members[i].Stats.maxHp);
            return hp;
        }

        private void ForceClear()
        {
            if (isEnemyOrganism || isNeutralOrganism)
            {
                RewardEnemyOrganismIfNeeded();
                for (int i = 0; i < members.Count; i++)
                {
                    V5CellEntity cell = members[i];
                    if (cell == null) continue;
                    RestoreBlueprintSlotStats(cell, i);
                    cell.ClearOrganismMorphSlot();
                    Destroy(cell.gameObject);
                }
                members.Clear();
                slotOffsets.Clear();
                startOffsets.Clear();
                slotRoles.Clear();
                originalMaxHp.Clear();
                originalPhysicalArmor.Clear();
                originalAttackRange.Clear();
                nucleusCell = null;
                IsMorphed = false;
                hasMoveTarget = false;
                playerOrder = PlayerOrderState.None;
                playerAttackTarget = null;
                MorphFlow01 = 0f;
                CurrentPassiveBiomassPerSecond = 0f;
                ResetPartCounters();
                DisposeRuntimeInstance();
                return;
            }

            for (int i = 0; i < members.Count; i++)
                if (members[i] != null)
                {
                    RestoreBlueprintSlotStats(members[i], i);
                    members[i].ClearOrganismMorphSlot();
                }
            members.Clear();
            slotOffsets.Clear();
            startOffsets.Clear();
            slotRoles.Clear();
            originalMaxHp.Clear();
            originalPhysicalArmor.Clear();
            originalAttackRange.Clear();
            nucleusCell = null;
            IsMorphed = false;
            hasMoveTarget = false;
            playerOrder = PlayerOrderState.None;
            playerAttackTarget = null;
            MorphFlow01 = 0f;
            CurrentPassiveBiomassPerSecond = 0f;
            ResetPartCounters();
            DisposeRuntimeInstance();
        }

        private void DisposeRuntimeInstance()
        {
            if (!isRuntimeInstance) return;
            if (manager != null)
            {
                manager.activeOrganisms.Remove(this);
                manager.activeEnemyOrganisms.Remove(this);
                manager.activeNeutralOrganisms.Remove(this);
            }
            Destroy(gameObject);
        }

        private void RewardEnemyOrganismIfNeeded()
        {
            if ((!isEnemyOrganism && !isNeutralOrganism) || enemyRewardIssued) return;
            enemyRewardIssued = true;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode) return;

            Vector2 center = nucleusCell != null ? (Vector2)nucleusCell.transform.position : (Vector2)transform.position;
            V5CoreMotherProductionSystem production = gm.CoreMotherProduction != null ? gm.CoreMotherProduction : FindFirstObjectByType<V5CoreMotherProductionSystem>();
            int rewardCells = isNeutralOrganism ? 0 : Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(0, enemyRewardCells) * 0.25f), 0, 4);
            int made = production != null && rewardCells > 0 ? production.SpawnRewardBatch(gm, rewardCells, center, "Brote menor por digestion") : 0;
            int dna = Mathf.Max(isNeutralOrganism ? 4 : 6, Mathf.RoundToInt(OrganismMaxHp() / 75f + OrganismSizeRadius() * 2.2f + ActiveRequiredFreeCells * (isNeutralOrganism ? 0.10f : 0.15f)));
            if (production != null) production.AddDna(dna, center, isNeutralOrganism ? "ADN por organismo neutral" : "ADN por organismo enemigo");
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push((isNeutralOrganism ? "Organismo neutral cazado: +" : "Organismo enemigo digerido: +") + dna + " ADN" + (made > 0 ? " +" + made + " celulas" : ""), center, new Color(0.58f, 0.94f, 1f, 1f));
        }

        private void EnsureEnemyBrain()
        {
            if (!isEnemyOrganism || nucleusCell == null) return;
            if (nucleusCell.GetComponent<V5EnemyBrain>() == null) nucleusCell.gameObject.AddComponent<V5EnemyBrain>();
        }

        private void RefreshLivePartState()
        {
            LiveLegCount = 0;
            LiveMouthCount = 0;
            LiveBodyCount = 0;

            if (slotRoles.Count == 0)
            {
                CurrentOrganismMoveSpeed = ActiveBaseMoveSpeed();
                CurrentEngulfRate = 0f;
                CurrentBodyStructureHp = 0f;
                CurrentBodyResistance01 = 0f;
                CurrentPassiveBiomassPerSecond = 0f;
                return;
            }

            for (int i = members.Count - 1; i >= 0; i--)
            {
                V5CellEntity cell = members[i];
                if (cell == null || cell.Stats.currentHp <= 0f)
                {
                    RemoveMemberAt(i);
                    continue;
                }

                V5MorphPartRole role = slotRoles[Mathf.Min(i, slotRoles.Count - 1)];
                if (role == V5MorphPartRole.Legs) LiveLegCount++;
                else if (role == V5MorphPartRole.Mouth) LiveMouthCount++;
                else LiveBodyCount++;
            }

            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            float baseSpeed = Mathf.Max(0.05f, blueprint.BaseMoveSpeed);
            float legSpeed = Mathf.Max(0f, blueprint.LegMoveSpeedPerCell);
            float maxSpeed = Mathf.Max(baseSpeed, blueprint.MaxMoveSpeed);
            float bodyHp = Mathf.Max(0f, blueprint.BodyHpPerCell);
            float bodyResistance = Mathf.Max(0f, blueprint.BodyResistancePerCell);

            CurrentOrganismMoveSpeed = Mathf.Min(maxSpeed, baseSpeed + legSpeed * LiveLegCount);
            float baseEngulfRate = MouthEngulfRatePerCell * Mathf.Clamp01(BaseEngulfRateMouthFraction);
            float engulfMultiplier = Mathf.Max(0.05f, blueprint.EngulfRateMultiplier);
            CurrentEngulfRate = Mathf.Min(MaxEngulfRate * engulfMultiplier, (baseEngulfRate + MouthEngulfRatePerCell * LiveMouthCount) * engulfMultiplier);
            CurrentBodyStructureHp = LiveBodyCount * bodyHp;
            CurrentBodyResistance01 = Mathf.Clamp01(LiveBodyCount * bodyResistance);
            CurrentPassiveBiomassPerSecond = Mathf.Max(0f, blueprint.PassiveBiomassPerCellPerSecond) * members.Count;
            EnsureNucleus();
            CollapseIfTooDamaged();
        }

        private void CollapseIfTooDamaged()
        {
            if (!isRuntimeInstance || !IsMorphed || members.Count == 0) return;
            int threshold = Mathf.Max(CollapseMinimumLiveCells, Mathf.CeilToInt(ActiveRequiredFreeCells * Mathf.Clamp01(CollapseLiveCellFraction)));
            if (members.Count >= threshold) return;

            LastMessage = BlueprintName + " colapso: quedan " + members.Count + "/" + ActiveRequiredFreeCells + " celulas.";
            if (isEnemyOrganism || isNeutralOrganism) ForceClear();
            else Revert();
        }

        private void TickCollectorAutofarm(V5GameManager gm, V5CellEntity anchorCell, bool playerDirected)
        {
            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            if (blueprint.Kind != V5OrganismBlueprintKind.Collector && !playerDirected) return;
            if (!playerDirected) return;
            if (gm == null || gm.Resources == null || anchorCell == null) return;
            if (hasMoveTarget)
            {
                float arrivalRadius = Mathf.Clamp(OrganismSizeRadius() * 0.28f, 0.38f, 1.1f);
                if (Vector2.SqrMagnitude((Vector2)anchorCell.transform.position - moveTarget) > arrivalRadius * arrivalRadius) return;
            }

            float range = Mathf.Max(24f, blueprint.ResourceSearchRange);
            V5ResourceNode node = FindNearestBiomassNode(gm, anchorCell.transform.position, range);
            if (node == null && gm.Environment != null) node = FindNearestBiomassNode(gm, anchorCell.transform.position, gm.Environment.MapRadius * 2f);
            if (node == null || node.depleted || node.kind != V5ResourceKind.Biomass) return;

            moveTarget = node.transform.position;
            hasMoveTarget = true;
            anchorCell.AttackTarget = null;
            anchorCell.Directive = V5Directive.Move;
            anchorCell.DirectiveTarget = moveTarget;
        }

        private V5ResourceNode FindNearestBiomassNode(V5GameManager gm, Vector2 from, float maxRange)
        {
            if (gm == null || gm.Resources == null) return null;
            float best = maxRange * maxRange;
            V5ResourceNode bestNode = null;
            List<V5ResourceNode> nodes = gm.Resources.Nodes;
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                V5ResourceNode node = nodes[i];
                if (node == null)
                {
                    nodes.RemoveAt(i);
                    continue;
                }
                if (node.depleted || node.kind != V5ResourceKind.Biomass) continue;
                float d = Vector2.SqrMagnitude((Vector2)node.transform.position - from);
                if (d >= best) continue;
                best = d;
                bestNode = node;
            }
            return bestNode;
        }

        private void ApplyPassiveBlueprintProduction(V5CellEntity bank)
        {
            if (bank == null || CurrentPassiveBiomassPerSecond <= 0f) return;
            bank.Resources.biomass += CurrentPassiveBiomassPerSecond * Time.deltaTime;
        }

        private void ApplyBlueprintSlotStats(V5CellEntity cell, V5MorphPartRole role, V5OrganismBlueprintDefinition blueprint)
        {
            if (cell == null || blueprint == null) return;
            cell.Stats.physicalArmor = Mathf.Clamp01(blueprint.Armor);
            cell.Stats.attackRange = Mathf.Max(0.1f, blueprint.AttackRange);
            if (blueprint.Kind == V5OrganismBlueprintKind.Tardigrade) return;

            if (blueprint.MemberMaxHpMultiplier > 0f && Mathf.Abs(blueprint.MemberMaxHpMultiplier - 1f) > 0.001f)
            {
                cell.Stats.maxHp = Mathf.Max(6f, cell.Stats.maxHp * blueprint.MemberMaxHpMultiplier);
                cell.Stats.currentHp = Mathf.Min(cell.Stats.currentHp, cell.Stats.maxHp);
            }

            if (role != V5MorphPartRole.Body) return;
            float hpBonus = Mathf.Max(0f, blueprint.BodyHpPerCell);
            if (hpBonus > 0f)
            {
                cell.Stats.maxHp += hpBonus;
                cell.Stats.currentHp = Mathf.Min(cell.Stats.maxHp, cell.Stats.currentHp + hpBonus);
            }
        }

        private void RestoreBlueprintSlotStats(V5CellEntity cell, int index)
        {
            if (cell == null || index < 0 || index >= originalMaxHp.Count) return;
            if (index < originalPhysicalArmor.Count)
                cell.Stats.physicalArmor = Mathf.Clamp01(originalPhysicalArmor[index]);
            if (index < originalAttackRange.Count)
                cell.Stats.attackRange = Mathf.Max(0.1f, originalAttackRange[index]);

            float restoredMaxHp = Mathf.Max(1f, originalMaxHp[index]);
            if (cell.Stats.maxHp <= restoredMaxHp + 0.01f) return;
            cell.Stats.maxHp = restoredMaxHp;
            cell.Stats.currentHp = Mathf.Min(cell.Stats.currentHp, cell.Stats.maxHp);
        }

        private void RemoveMemberAt(int index)
        {
            if (index >= 0 && index < members.Count && members[index] != null)
            {
                if (members[index] == nucleusCell) nucleusCell = null;
                RestoreBlueprintSlotStats(members[index], index);
                members[index].ClearOrganismMorphSlot();
                members[index].SetSelected(false);
            }
            members.RemoveAt(index);
            if (index < slotOffsets.Count) slotOffsets.RemoveAt(index);
            if (index < startOffsets.Count) startOffsets.RemoveAt(index);
            if (index < slotRoles.Count) slotRoles.RemoveAt(index);
            if (index < originalMaxHp.Count) originalMaxHp.RemoveAt(index);
            if (index < originalPhysicalArmor.Count) originalPhysicalArmor.RemoveAt(index);
            if (index < originalAttackRange.Count) originalAttackRange.RemoveAt(index);
        }

        private void EngulfTouchingBiomass(V5GameManager gm, V5CellEntity anchorCell, V5CellEntity bank)
        {
            if (gm == null || gm.Resources == null || anchorCell == null || bank == null || CurrentEngulfRate <= 0f) return;

            List<V5ResourceNode> nodes = gm.Resources.Nodes;
            float request = CurrentEngulfRate * Time.deltaTime;
            for (int i = nodes.Count - 1; i >= 0; i--)
            {
                V5ResourceNode node = nodes[i];
                if (node == null)
                {
                    nodes.RemoveAt(i);
                    continue;
                }
                if (node.depleted || node.kind != V5ResourceKind.Biomass) continue;
                if (!IsNodeTouchingMouth(node, anchorCell)) continue;

                float taken = node.Harvest(request);
                if (taken > 0f) bank.Resources.Add(node.kind, taken);
            }
        }

        private bool IsNodeTouchingMouth(V5ResourceNode node, V5CellEntity anchorCell)
        {
            if (slotRoles.Count == 0) return false;

            float nodeRadius = Mathf.Max(0.3f, node.transform.localScale.x * 0.5f);
            float reach = EngulfRange + nodeRadius;
            float reachSqr = reach * reach;
            for (int i = 0; i < members.Count; i++)
            {
                if (slotRoles[Mathf.Min(i, slotRoles.Count - 1)] != V5MorphPartRole.Mouth) continue;
                V5CellEntity cell = members[i];
                if (cell == null) continue;
                if (Vector2.SqrMagnitude((Vector2)cell.transform.position - (Vector2)node.transform.position) <= reachSqr) return true;
            }

            return IsNodeInsideFrontalEngulfZone(node, anchorCell, nodeRadius);
        }

        private bool IsNodeInsideFrontalEngulfZone(V5ResourceNode node, V5CellEntity anchorCell, float nodeRadius)
        {
            if (node == null || anchorCell == null) return false;

            Vector2 anchor = anchorCell.transform.position;
            Vector2 frontCenter = anchor + Rotate(Vector2.right * (ActiveFormationRadiusX() * 0.72f), organismAngle);
            float frontRadius = EngulfRange + ActiveFormationRadiusY() * 0.65f + nodeRadius;
            return Vector2.SqrMagnitude((Vector2)node.transform.position - frontCenter) <= frontRadius * frontRadius;
        }

        private void EngulfTouchingEnemies(V5GameManager gm, V5CellEntity anchorCell, V5CellEntity bank)
        {
            if (gm == null || anchorCell == null || bank == null) return;
            if (isNeutralOrganism && playerOrder != PlayerOrderState.Attack) return;

            float organismRadius = OrganismSizeRadius();
            IReadOnlyList<V5CellEntity> targets = (isEnemyOrganism || isNeutralOrganism) ? gm.PlayerCells : gm.NonPlayerCells;
            if (targets == null) return;

            List<V5OrganismMorph> handledOrganisms = new List<V5OrganismMorph>(4);
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                V5CellEntity enemy = targets[i];
                if (enemy == null || enemy.Stats.currentHp <= 0f) continue;
                if (!IsEnemyTouchingOrganism(enemy, anchorCell, organismRadius)) continue;

                V5OrganismMorph targetOrganism = (isEnemyOrganism || isNeutralOrganism) ? FindPlayerOrganismForCell(enemy) : EnemyOrganismForCell(enemy);
                if (targetOrganism != null)
                {
                    if (handledOrganisms.Contains(targetOrganism)) continue;
                    handledOrganisms.Add(targetOrganism);
                    FightComparableOrganism(targetOrganism, anchorCell);
                    continue;
                }

                if (CanEngulfEnemy(enemy, organismRadius) && Time.time >= lastEngulfTime + EngulfCooldown)
                {
                    enemy.Damage(enemy.Stats.maxHp + enemy.Stats.currentHp + 20f, V5DamageKind.Physical, anchorCell.transform.position);
                    lastEngulfTime = Time.time;
                    if (!isEnemyOrganism && !isNeutralOrganism) bank.Resources.biomass += 18f;
                    V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                    if (feedback != null) feedback.PushFloating("engullido +18 bio", enemy.transform.position, new Color(1f, 0.55f, 0.38f, 1f));
                }
                else
                {
                    FightComparableEnemy(enemy, anchorCell);
                }
            }
        }

        private bool CanEngulfEnemy(V5CellEntity enemy, float organismRadius)
        {
            V5OrganismBlueprintKind kind = GetActiveBlueprint().Kind;
            if (kind == V5OrganismBlueprintKind.Collector || kind == V5OrganismBlueprintKind.Lacrymaria) return false;
            return enemy != null && organismRadius >= enemy.Stats.radius * 1.55f;
        }

        private void FightComparableOrganism(V5OrganismMorph targetOrganism, V5CellEntity anchorCell)
        {
            if (targetOrganism == null || anchorCell == null) return;
            float combatMultiplier = Mathf.Max(0.05f, GetActiveBlueprint().CombatDamageMultiplier);
            float mouthPressure = CurrentEngulfRate * 0.22f;
            float bodyPressure = Mathf.Max(0f, LiveBodyCount * 0.08f + LiveMouthCount * 0.36f);
            float damage = (5f + mouthPressure + bodyPressure) * combatMultiplier * Time.deltaTime;
            targetOrganism.ReceiveOrganismDamage(damage, GetActiveBlueprint().DamageKind, anchorCell.transform.position);
        }

        private void ReceiveOrganismDamage(float damage, V5DamageKind damageKind, Vector2 source)
        {
            V5CellEntity target = EnsureNucleus();
            if (target == null && members.Count > 0) target = members[0];
            if (target != null) target.Damage(damage, damageKind, source);
        }

        private void FightComparableEnemy(V5CellEntity enemy, V5CellEntity anchorCell)
        {
            if (enemy == null || anchorCell == null) return;
            float combatMultiplier = Mathf.Max(0.05f, GetActiveBlueprint().CombatDamageMultiplier);
            float mouthPressure = CurrentEngulfRate * 0.34f;
            float bodyPressure = Mathf.Max(0f, LiveBodyCount * 0.09f + LiveMouthCount * 0.42f);
            float damage = (8f + mouthPressure + bodyPressure) * combatMultiplier * Time.deltaTime;
            enemy.Damage(damage, GetActiveBlueprint().DamageKind, anchorCell.transform.position);

            if (Random.value < Time.deltaTime * 3.0f)
            {
                V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                if (feedback != null) feedback.PushFloating("mordida " + damage.ToString("0.0"), enemy.transform.position, new Color(1f, 0.48f, 0.30f, 1f));
            }
        }

        private bool IsEnemyTouchingOrganism(V5CellEntity enemy, V5CellEntity anchorCell, float organismRadius)
        {
            if (enemy == null || anchorCell == null) return false;
            float envelope = organismRadius + enemy.Stats.radius + 0.18f;
            return Vector2.SqrMagnitude((Vector2)enemy.transform.position - (Vector2)anchorCell.transform.position) <= envelope * envelope;
        }

        private float AttackArrivalRadius(V5CellEntity target)
        {
            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            if (blueprint.RangedDamagePerSecond > 0f && blueprint.AttackRange > 1.5f)
                return Mathf.Max(1.5f, blueprint.AttackRange * 0.92f);
            if (target == null) return Mathf.Clamp(OrganismSizeRadius() * 0.18f, 0.25f, 0.8f);
            float contactRadius = OrganismSizeRadius() + Mathf.Max(0.1f, target.Stats.radius) + 0.18f;
            return Mathf.Max(0.18f, contactRadius * 0.72f);
        }

        private bool IsRangedBlueprint()
        {
            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            return blueprint.RangedDamagePerSecond > 0f && blueprint.AttackRange > 1.5f;
        }

        private void PlantRangedStandoff(V5CellEntity nucleus)
        {
            if (!IsRangedBlueprint() || nucleus == null || !IsValidRangedTarget(playerAttackTarget)) return;
            rangedStandoffPosition = ClampOrganismInsideMap(nucleus.transform.position);
            hasRangedStandoff = true;
            hasMoveTarget = false;
        }

        private void ApplyRangedAttack(V5GameManager gm, V5CellEntity nucleus)
        {
            if (gm == null || nucleus == null) return;
            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            float range = Mathf.Max(0.1f, blueprint.AttackRange);
            float damagePerSecond = Mathf.Max(0f, blueprint.RangedDamagePerSecond);
            if (range <= 1.5f || damagePerSecond <= 0f) return;

            V5CellEntity target = IsValidRangedTarget(playerAttackTarget) ? playerAttackTarget : null;
            if (target == null && IsValidRangedTarget(nucleus.AttackTarget)) target = nucleus.AttackTarget;
            if (target == null || Vector2.SqrMagnitude((Vector2)target.transform.position - (Vector2)nucleus.transform.position) > range * range)
                target = FindNearestRangedTarget(gm, nucleus.transform.position, range);
            if (target == null) return;

            Vector2 source = nucleus.transform.position;
            if (Vector2.SqrMagnitude((Vector2)target.transform.position - source) > range * range) return;
            target.Damage(damagePerSecond * Time.deltaTime, blueprint.DamageKind, source);
        }

        private bool IsValidRangedTarget(V5CellEntity target)
        {
            if (target == null || target.Stats.currentHp <= 0f) return false;
            return (isEnemyOrganism || isNeutralOrganism) ? target.IsPlayerOwned : !target.IsPlayerOwned;
        }

        private V5CellEntity FindNearestRangedTarget(V5GameManager gm, Vector2 from, float range)
        {
            IReadOnlyList<V5CellEntity> targets = (isEnemyOrganism || isNeutralOrganism) ? gm.PlayerCells : gm.NonPlayerCells;
            if (targets == null) return null;

            float best = range * range;
            V5CellEntity nearest = null;
            for (int i = 0; i < targets.Count; i++)
            {
                V5CellEntity target = targets[i];
                if (!IsValidRangedTarget(target)) continue;
                float distance = Vector2.SqrMagnitude((Vector2)target.transform.position - from);
                if (distance > best) continue;
                best = distance;
                nearest = target;
            }
            return nearest;
        }

        private V5CellEntity FindAttackMoveTarget(V5GameManager gm, V5CellEntity anchorCell, Vector2 destination)
        {
            if (gm == null || anchorCell == null) return null;
            IReadOnlyList<V5CellEntity> targets = (isEnemyOrganism || isNeutralOrganism) ? gm.PlayerCells : gm.NonPlayerCells;
            if (targets == null) return null;

            Vector2 from = anchorCell.transform.position;
            float acquireRange = Mathf.Max(11f, OrganismSizeRadius() + 5.0f);
            float destinationRange = Mathf.Max(6f, OrganismSizeRadius() + 1.5f);
            float best = float.MaxValue;
            V5CellEntity bestCell = null;
            for (int i = 0; i < targets.Count; i++)
            {
                V5CellEntity target = targets[i];
                if (target == null || target.Stats.currentHp <= 0f) continue;
                float fromDistance = Vector2.SqrMagnitude((Vector2)target.transform.position - from);
                float destinationDistance = Vector2.SqrMagnitude((Vector2)target.transform.position - destination);
                if (fromDistance > acquireRange * acquireRange && destinationDistance > destinationRange * destinationRange) continue;
                if (fromDistance >= best) continue;
                best = fromDistance;
                bestCell = target;
            }
            return bestCell;
        }

        private float OrganismSizeRadius()
        {
            return Mathf.Max(ActiveFormationRadiusX(), ActiveFormationRadiusY()) + 0.45f;
        }

        private float InterdictorAuraRadius()
        {
            return Mathf.Max(4.5f, OrganismSizeRadius() + 3.8f);
        }

        private void ApplyInterdictorToxicAura(V5GameManager gm, V5CellEntity nucleus)
        {
            if (gm == null || nucleus == null || ActiveBlueprintKind != V5OrganismBlueprintKind.Interdictor) return;

            float damage = Mathf.Max(0f, InterdictorToxicDamagePerSecond) * Time.deltaTime;
            if (damage <= 0f) return;

            IReadOnlyList<V5CellEntity> targets = (isEnemyOrganism || isNeutralOrganism) ? gm.PlayerCells : gm.NonPlayerCells;
            if (targets == null) return;

            Vector2 source = nucleus.transform.position;
            float radius = InterdictorAuraRadius();
            float radiusSqr = radius * radius;
            for (int i = targets.Count - 1; i >= 0; i--)
            {
                V5CellEntity target = targets[i];
                if (target == null || target.Stats.currentHp <= 0f) continue;
                if (Vector2.SqrMagnitude((Vector2)target.transform.position - source) > radiusSqr) continue;
                target.Damage(damage, V5DamageKind.Chemical, source);
            }
        }

        private float ActiveFormationRadiusX()
        {
            return Mathf.Max(0.1f, GetActiveBlueprint().FormationRadiusX);
        }

        private float ActiveFormationRadiusY()
        {
            return Mathf.Max(0.1f, GetActiveBlueprint().FormationRadiusY);
        }

        private float ActiveBaseMoveSpeed()
        {
            return Mathf.Max(0.05f, GetActiveBlueprint().BaseMoveSpeed);
        }

        private void ResetPartCounters()
        {
            LastLegSlotCount = 0;
            LastMouthSlotCount = 0;
            LastBodySlotCount = 0;
            LiveLegCount = 0;
            LiveMouthCount = 0;
            LiveBodyCount = 0;
            CurrentOrganismMoveSpeed = 0f;
            CurrentEngulfRate = 0f;
            CurrentBodyStructureHp = 0f;
            CurrentBodyResistance01 = 0f;
            CurrentPassiveBiomassPerSecond = 0f;
        }

        private void CountInitialSlot(V5MorphPartRole role)
        {
            if (role == V5MorphPartRole.Legs) LastLegSlotCount++;
            else if (role == V5MorphPartRole.Mouth) LastMouthSlotCount++;
            else LastBodySlotCount++;
        }

        private List<V5CellEntity> GatherFreeCandidates(V5GameManager gm, Vector2 motherPos)
        {
            List<V5CellEntity> candidates = new List<V5CellEntity>(gm.PlayerCells.Count);
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (IsFreeMorphCandidate(cell)) candidates.Add(cell);
            }

            candidates.Sort((a, b) =>
                Vector2.SqrMagnitude((Vector2)a.transform.position - motherPos)
                    .CompareTo(Vector2.SqrMagnitude((Vector2)b.transform.position - motherPos)));
            return candidates;
        }

        private List<V5CellEntity> GatherSelectedFreeCandidates(V5GameManager gm)
        {
            List<V5CellEntity> candidates = new List<V5CellEntity>(RequiredFreeCells);
            if (gm == null || gm.Selection == null) return candidates;

            Vector2 center = Vector2.zero;
            for (int i = 0; i < gm.Selection.Selected.Count; i++)
            {
                V5CellEntity cell = gm.Selection.Selected[i];
                if (!IsFreeMorphCandidate(cell)) continue;
                candidates.Add(cell);
                center += (Vector2)cell.transform.position;
            }
            if (candidates.Count == 0) return candidates;

            center /= candidates.Count;
            candidates.Sort((a, b) =>
                Vector2.SqrMagnitude((Vector2)a.transform.position - center)
                    .CompareTo(Vector2.SqrMagnitude((Vector2)b.transform.position - center)));
            return candidates;
        }

        private bool IsFreeMorphCandidate(V5CellEntity cell)
        {
            return cell != null &&
                   cell.IsPlayerOwned &&
                   cell.Role != V5CellRole.Mother &&
                   cell.Stats.currentHp > 0f &&
                   !cell.IsAttachedToBody &&
                   !cell.IsMorphedOrganism;
        }

        private List<MorphSlot> BuildSilhouetteSlots(int total)
        {
            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            float radiusX = Mathf.Max(0.1f, blueprint.FormationRadiusX);
            float radiusY = Mathf.Max(0.1f, blueprint.FormationRadiusY);
            List<SilhouetteSample> samples = CollectSilhouetteSamples();
            List<MorphSlot> slots = new List<MorphSlot>(total);
            if (samples.Count == 0)
            {
                for (int i = 0; i < total; i++) slots.Add(new MorphSlot(FallbackFormationOffset(i, total), V5MorphPartRole.Body));
                return slots;
            }

            List<int> selected = SelectDistributedSamples(samples, total);
            for (int i = 0; i < selected.Count; i++)
            {
                SilhouetteSample sample = samples[selected[i]];
                Vector2 offset = new Vector2(sample.normalized.x * radiusX, sample.normalized.y * radiusY);
                slots.Add(new MorphSlot(offset, sample.role));
            }
            while (slots.Count < total) slots.Add(new MorphSlot(FallbackFormationOffset(slots.Count, total), V5MorphPartRole.Body));
            return slots;
        }

        private List<SilhouetteSample> CollectSilhouetteSamples()
        {
            V5OrganismBlueprintDefinition blueprint = GetActiveBlueprint();
            List<SilhouetteSample> samples = new List<SilhouetteSample>(4096);
            UsingTemporarySilhouette = false;

            Sprite sprite = blueprint.SilhouetteSprite != null ? blueprint.SilhouetteSprite : BlueprintSilhouetteSprite;
            Texture2D texture = blueprint.SilhouetteTexture != null ? blueprint.SilhouetteTexture : BlueprintSilhouetteTexture;

            if (sprite != null)
            {
                if (TryCollectOpaqueSamples(sprite.texture, sprite.textureRect, samples))
                {
                    if (blueprint.ForceAllBodyRoles) ForceSamplesToBody(samples);
                    return samples;
                }
                samples.Clear();
            }

            if (texture != null)
            {
                Rect rect = new Rect(0f, 0f, texture.width, texture.height);
                if (TryCollectOpaqueSamples(texture, rect, samples))
                {
                    if (blueprint.ForceAllBodyRoles) ForceSamplesToBody(samples);
                    return samples;
                }
                samples.Clear();
            }

            UsingTemporarySilhouette = true;
            Texture2D temporary = blueprint.UseTemporaryLacrymariaSilhouette
                ? GenerateTemporaryLacrymariaSilhouetteTexture()
                : (blueprint.UseTemporaryHarasserSilhouette
                    ? GenerateTemporaryHarasserSilhouetteTexture()
                    : (blueprint.UseTemporaryFighterSilhouette
                        ? GenerateTemporaryFighterSilhouetteTexture()
                        : (blueprint.UseTemporaryInterdictorSilhouette
                            ? GenerateTemporaryInterdictorSilhouetteTexture()
                            : (blueprint.UseTemporaryAnchorSilhouette
                                ? GenerateTemporaryAnchorSilhouetteTexture()
                                : (blueprint.UseTemporaryCollectorSilhouette
                                    ? GenerateTemporaryCollectorSilhouetteTexture()
                                    : (blueprint.UseTemporaryVolvoxSilhouette
                                        ? GenerateTemporaryVolvoxSilhouetteTexture()
                                        : GenerateTemporaryTardigradeSilhouetteTexture()))))));
            TryCollectOpaqueSamples(temporary, new Rect(0f, 0f, temporary.width, temporary.height), samples);
            if (blueprint.ForceAllBodyRoles) ForceSamplesToBody(samples);
            return samples;
        }

        private void ForceSamplesToBody(List<SilhouetteSample> samples)
        {
            if (samples == null) return;
            for (int i = 0; i < samples.Count; i++)
                samples[i] = new SilhouetteSample(samples[i].normalized, V5MorphPartRole.Body);
        }

        private bool TryCollectOpaqueSamples(Texture2D texture, Rect pixelRect, List<SilhouetteSample> samples)
        {
            if (texture == null || samples == null) return false;
            int sampleX = Mathf.Clamp(SilhouetteSampleResolutionX, 32, 256);
            int sampleY = Mathf.Clamp(Mathf.RoundToInt(sampleX * pixelRect.height / Mathf.Max(1f, pixelRect.width)), 18, 256);

            try
            {
                for (int y = 0; y < sampleY; y++)
                {
                    float vy = (y + 0.5f) / sampleY;
                    int py = Mathf.Clamp(Mathf.RoundToInt(pixelRect.yMin + vy * pixelRect.height), 0, texture.height - 1);
                    for (int x = 0; x < sampleX; x++)
                    {
                        float vx = (x + 0.5f) / sampleX;
                        int px = Mathf.Clamp(Mathf.RoundToInt(pixelRect.xMin + vx * pixelRect.width), 0, texture.width - 1);
                        Color pixel = texture.GetPixel(px, py);
                        if (pixel.a < SilhouetteAlphaThreshold) continue;

                        Vector2 normalized = new Vector2(vx * 2f - 1f, vy * 2f - 1f);
                        samples.Add(new SilhouetteSample(normalized, RoleFromPixel(pixel, normalized)));
                    }
                }
            }
            catch (UnityException)
            {
                return false;
            }

            return samples.Count > 0;
        }

        private List<int> SelectDistributedSamples(List<SilhouetteSample> samples, int total)
        {
            List<int> selected = new List<int>(total);
            bool[] used = new bool[samples.Count];
            float[] nearest = new float[samples.Count];

            AddSeedSample(samples, selected, used, FindClosestSample(samples, Vector2.zero), total);
            AddSeedSample(samples, selected, used, FindRoleExtremeSample(samples, V5MorphPartRole.Mouth, new Vector2(1f, 0f)), total);
            AddSeedSample(samples, selected, used, FindRoleExtremeSample(samples, V5MorphPartRole.Legs, new Vector2(0f, 1f)), total);
            AddSeedSample(samples, selected, used, FindRoleExtremeSample(samples, V5MorphPartRole.Legs, new Vector2(0f, -1f)), total);
            AddSeedSample(samples, selected, used, FindExtremeSample(samples, new Vector2(1f, 0f)), total);
            AddSeedSample(samples, selected, used, FindExtremeSample(samples, new Vector2(-1f, 0f)), total);
            AddSeedSample(samples, selected, used, FindExtremeSample(samples, new Vector2(0f, 1f)), total);
            AddSeedSample(samples, selected, used, FindExtremeSample(samples, new Vector2(0f, -1f)), total);
            RefreshNearestSamples(samples, selected, nearest);

            while (selected.Count < total && selected.Count < samples.Count)
            {
                int best = -1;
                float bestScore = -1f;
                for (int i = 0; i < samples.Count; i++)
                {
                    if (used[i]) continue;
                    float score = nearest[i] * RoleDistributionWeight(samples[i].role);
                    if (score > bestScore)
                    {
                        bestScore = score;
                        best = i;
                    }
                }
                if (best < 0) break;
                used[best] = true;
                selected.Add(best);
                for (int i = 0; i < samples.Count; i++)
                {
                    float d = Vector2.SqrMagnitude(samples[i].normalized - samples[best].normalized);
                    if (d < nearest[i]) nearest[i] = d;
                }
            }

            return selected;
        }

        private float RoleDistributionWeight(V5MorphPartRole role)
        {
            if (role == V5MorphPartRole.Mouth) return 1.22f;
            if (role == V5MorphPartRole.Legs) return 1.12f;
            return 1f;
        }

        private void AddSeedSample(List<SilhouetteSample> samples, List<int> selected, bool[] used, int index, int total)
        {
            if (index < 0 || selected.Count >= total || used[index]) return;
            used[index] = true;
            selected.Add(index);
        }

        private void RefreshNearestSamples(List<SilhouetteSample> samples, List<int> selected, float[] nearest)
        {
            for (int i = 0; i < samples.Count; i++)
            {
                float best = float.MaxValue;
                for (int j = 0; j < selected.Count; j++)
                {
                    float d = Vector2.SqrMagnitude(samples[i].normalized - samples[selected[j]].normalized);
                    if (d < best) best = d;
                }
                nearest[i] = best;
            }
        }

        private int FindClosestSample(List<SilhouetteSample> samples, Vector2 target)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < samples.Count; i++)
            {
                float d = Vector2.SqrMagnitude(samples[i].normalized - target);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }
            return best;
        }

        private int FindExtremeSample(List<SilhouetteSample> samples, Vector2 direction)
        {
            int best = -1;
            float bestDot = float.MinValue;
            for (int i = 0; i < samples.Count; i++)
            {
                float dot = Vector2.Dot(samples[i].normalized, direction);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = i;
                }
            }
            return best;
        }

        private int FindRoleExtremeSample(List<SilhouetteSample> samples, V5MorphPartRole role, Vector2 direction)
        {
            int best = -1;
            float bestDot = float.MinValue;
            for (int i = 0; i < samples.Count; i++)
            {
                if (samples[i].role != role) continue;
                float dot = Vector2.Dot(samples[i].normalized, direction);
                if (dot > bestDot)
                {
                    bestDot = dot;
                    best = i;
                }
            }
            return best;
        }

        private int PickNearestUnusedSlot(List<MorphSlot> slots, bool[] usedSlots, Vector2 localStart)
        {
            int best = -1;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < slots.Count; i++)
            {
                if (usedSlots[i]) continue;
                float d = Vector2.SqrMagnitude(slots[i].offset - localStart);
                if (d < bestDistance)
                {
                    bestDistance = d;
                    best = i;
                }
            }
            return best >= 0 ? best : 0;
        }

        private V5MorphPartRole RoleFromPixel(Color pixel, Vector2 normalized)
        {
            if (pixel.r > pixel.g + 0.14f && pixel.r > pixel.b + 0.14f) return V5MorphPartRole.Mouth;
            if (pixel.b > pixel.r + 0.14f && pixel.b > pixel.g + 0.14f) return V5MorphPartRole.Legs;

            bool uncoloredWhite = Mathf.Abs(pixel.r - pixel.g) < 0.08f &&
                                  Mathf.Abs(pixel.r - pixel.b) < 0.08f &&
                                  pixel.r > 0.82f;
            if (uncoloredWhite && IsLikelyLegSample(normalized)) return V5MorphPartRole.Legs;
            return V5MorphPartRole.Body;
        }

        private bool IsLikelyLegSample(Vector2 normalized)
        {
            if (Mathf.Abs(normalized.y) < 0.36f) return false;
            float[] legCenters = { -0.60f, -0.28f, 0.08f, 0.44f };
            for (int i = 0; i < legCenters.Length; i++)
                if (Mathf.Abs(normalized.x - legCenters[i]) < 0.18f) return true;
            return Mathf.Abs(normalized.y) > 0.58f && Mathf.Abs(normalized.x) < 0.78f;
        }

        private Texture2D GenerateTemporaryTardigradeSilhouetteTexture()
        {
            if (temporaryTardigradeSilhouette != null) return temporaryTardigradeSilhouette;

            const int width = 192;
            const int height = 96;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyColor = new Color(0.58f, 0.62f, 0.65f, 1f);
            Color legColor = new Color(0.12f, 0.38f, 1f, 1f);
            Color mouthColor = new Color(1f, 0.16f, 0.12f, 1f);

            for (int y = 0; y < height; y++)
            {
                float ny = ((y + 0.5f) / height - 0.5f) * 2f;
                for (int x = 0; x < width; x++)
                {
                    float nx = ((x + 0.5f) / width - 0.5f) * 2f;
                    if (TemporaryTardigradeMouth(nx, ny)) tex.SetPixel(x, y, mouthColor);
                    else if (TemporaryTardigradeLegs(nx, ny)) tex.SetPixel(x, y, legColor);
                    else if (TemporaryTardigradeBody(nx, ny)) tex.SetPixel(x, y, bodyColor);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryTardigradeSilhouette = tex;
            return temporaryTardigradeSilhouette;
        }

        private bool TemporaryTardigradeBody(float x, float y)
        {
            float[] centers = { -0.70f, -0.46f, -0.20f, 0.06f, 0.32f, 0.58f };
            float[] heights = { 0.30f, 0.36f, 0.40f, 0.40f, 0.36f, 0.30f };
            for (int i = 0; i < centers.Length; i++)
                if (InEllipse(x, y, centers[i], 0f, 0.27f, heights[i])) return true;
            return InEllipse(x, y, 0.78f, 0f, 0.18f, 0.22f) || InEllipse(x, y, -0.84f, 0f, 0.13f, 0.20f);
        }

        private bool TemporaryTardigradeMouth(float x, float y)
        {
            return InEllipse(x, y, 0.90f, 0f, 0.075f, 0.105f) ||
                   (x > 0.80f && InEllipse(x, y, 0.78f, 0f, 0.18f, 0.22f));
        }

        private bool TemporaryTardigradeLegs(float x, float y)
        {
            float[] centers = { -0.60f, -0.28f, 0.08f, 0.44f };
            for (int i = 0; i < centers.Length; i++)
            {
                for (int side = -1; side <= 1; side += 2)
                {
                    float cx = centers[i];
                    float cy = side * 0.48f;
                    if (InRotatedEllipse(x, y, cx, cy, 0.11f, 0.24f, side * 13f)) return true;
                    if (InEllipse(x, y, cx + 0.03f, side * 0.70f, 0.10f, 0.07f)) return true;
                }
            }
            return false;
        }

        private Texture2D GenerateTemporaryVolvoxSilhouetteTexture()
        {
            if (temporaryVolvoxSilhouette != null) return temporaryVolvoxSilhouette;

            const int size = 144;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyCore = new Color(0.50f, 0.78f, 0.66f, 1f);
            Color bodyEdge = new Color(0.35f, 0.62f, 0.55f, 1f);

            for (int y = 0; y < size; y++)
            {
                float ny = ((y + 0.5f) / size - 0.5f) * 2f;
                for (int x = 0; x < size; x++)
                {
                    float nx = ((x + 0.5f) / size - 0.5f) * 2f;
                    float d = Mathf.Sqrt(nx * nx + ny * ny);
                    if (d <= 0.92f) tex.SetPixel(x, y, Color.Lerp(bodyCore, bodyEdge, Mathf.Clamp01(d)));
                    else tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryVolvoxSilhouette = tex;
            return temporaryVolvoxSilhouette;
        }

        private Texture2D GenerateTemporaryCollectorSilhouetteTexture()
        {
            if (temporaryCollectorSilhouette != null) return temporaryCollectorSilhouette;

            const int width = 176;
            const int height = 64;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyColor = new Color(0.56f, 0.82f, 0.52f, 1f);
            Color mouthColor = new Color(1f, 0.18f, 0.12f, 1f);
            Color flagellumColor = new Color(0.12f, 0.42f, 1f, 1f);

            for (int y = 0; y < height; y++)
            {
                float ny = ((y + 0.5f) / height - 0.5f) * 2f;
                for (int x = 0; x < width; x++)
                {
                    float nx = ((x + 0.5f) / width - 0.5f) * 2f;
                    if (TemporaryCollectorMouth(nx, ny)) tex.SetPixel(x, y, mouthColor);
                    else if (TemporaryCollectorFlagellum(nx, ny)) tex.SetPixel(x, y, flagellumColor);
                    else if (TemporaryCollectorBody(nx, ny)) tex.SetPixel(x, y, bodyColor);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryCollectorSilhouette = tex;
            return temporaryCollectorSilhouette;
        }

        private Texture2D GenerateTemporaryHarasserSilhouetteTexture()
        {
            if (temporaryHarasserSilhouette != null) return temporaryHarasserSilhouette;

            const int width = 176;
            const int height = 92;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyColor = new Color(0.70f, 0.78f, 0.82f, 1f);
            Color legColor = new Color(0.10f, 0.44f, 1f, 1f);
            Color mouthColor = new Color(1f, 0.18f, 0.10f, 1f);

            for (int y = 0; y < height; y++)
            {
                float ny = ((y + 0.5f) / height - 0.5f) * 2f;
                for (int x = 0; x < width; x++)
                {
                    float nx = ((x + 0.5f) / width - 0.5f) * 2f;
                    if (TemporaryHarasserMouth(nx, ny)) tex.SetPixel(x, y, mouthColor);
                    else if (TemporaryHarasserMobilityRidge(nx, ny)) tex.SetPixel(x, y, legColor);
                    else if (TemporaryHarasserBody(nx, ny)) tex.SetPixel(x, y, bodyColor);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryHarasserSilhouette = tex;
            return temporaryHarasserSilhouette;
        }

        private bool TemporaryHarasserBody(float x, float y)
        {
            if (x < -0.92f || x > 0.84f) return false;
            float t = Mathf.InverseLerp(-0.92f, 0.84f, x);
            float wave = Mathf.Sin(t * Mathf.PI * 4.7f) * 0.34f;
            float taper = Mathf.Lerp(0.14f, 0.08f, Mathf.Abs(t - 0.5f) * 2f);
            return Mathf.Abs(y - wave) < taper ||
                   InEllipse(x, y, -0.88f, Mathf.Sin(0f) * 0.34f, 0.13f, 0.16f) ||
                   InEllipse(x, y, 0.86f, Mathf.Sin(Mathf.PI * 4.7f) * 0.34f, 0.12f, 0.13f);
        }

        private bool TemporaryHarasserMobilityRidge(float x, float y)
        {
            if (x < -0.86f || x > 0.76f) return false;
            float t = Mathf.InverseLerp(-0.86f, 0.76f, x);
            float wave = Mathf.Sin(t * Mathf.PI * 4.7f) * 0.34f;
            float ridge = Mathf.Cos(t * Mathf.PI * 4.7f) >= 0f ? 0.10f : -0.10f;
            return Mathf.Abs(y - (wave + ridge)) < 0.052f;
        }

        private bool TemporaryHarasserMouth(float x, float y)
        {
            if (x < 0.66f) return false;
            float t = Mathf.InverseLerp(-0.92f, 0.84f, Mathf.Clamp(x, -0.92f, 0.84f));
            float wave = Mathf.Sin(t * Mathf.PI * 4.7f) * 0.34f;
            return InEllipse(x, y, 0.82f, wave, 0.12f, 0.12f);
        }

        private Texture2D GenerateTemporaryAnchorSilhouetteTexture()
        {
            if (temporaryAnchorSilhouette != null) return temporaryAnchorSilhouette;

            const int size = 136;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyColor = new Color(0.54f, 0.68f, 0.74f, 1f);
            Color bodyRim = new Color(0.38f, 0.52f, 0.60f, 1f);

            Vector2[] chambers =
            {
                new Vector2(-0.10f, 0.02f),
                new Vector2(0.08f, 0.11f),
                new Vector2(0.25f, 0.02f),
                new Vector2(0.32f, -0.18f),
                new Vector2(0.16f, -0.39f),
                new Vector2(-0.14f, -0.42f),
                new Vector2(-0.43f, -0.21f),
                new Vector2(-0.50f, 0.15f)
            };
            float[] radii = { 0.15f, 0.19f, 0.24f, 0.30f, 0.37f, 0.44f, 0.50f, 0.58f };

            for (int y = 0; y < size; y++)
            {
                float ny = ((y + 0.5f) / size - 0.5f) * 2f;
                for (int x = 0; x < size; x++)
                {
                    float nx = ((x + 0.5f) / size - 0.5f) * 2f;
                    int chamberIndex = -1;
                    float rim = 1f;
                    for (int i = chambers.Length - 1; i >= 0; i--)
                    {
                        float rx = radii[i] * 0.92f;
                        float ry = radii[i] * 0.82f;
                        if (!InEllipse(nx, ny, chambers[i].x, chambers[i].y, rx, ry)) continue;

                        chamberIndex = i;
                        float dx = (nx - chambers[i].x) / Mathf.Max(0.001f, rx);
                        float dy = (ny - chambers[i].y) / Mathf.Max(0.001f, ry);
                        rim = Mathf.Sqrt(dx * dx + dy * dy);
                        break;
                    }

                    bool aperture = InEllipse(nx, ny, -0.73f, 0.28f, 0.18f, 0.12f);
                    if (chamberIndex >= 0 || aperture)
                    {
                        float edge = Mathf.SmoothStep(0.70f, 1f, rim);
                        float chamberTint = chamberIndex >= 0 ? chamberIndex / 7f * 0.12f : 0.06f;
                        tex.SetPixel(x, y, Color.Lerp(bodyColor, bodyRim, Mathf.Clamp01(edge + chamberTint)));
                    }
                    else tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryAnchorSilhouette = tex;
            return temporaryAnchorSilhouette;
        }

        private Texture2D GenerateTemporaryFighterSilhouetteTexture()
        {
            if (temporaryFighterSilhouette != null) return temporaryFighterSilhouette;

            const int width = 120;
            const int height = 110;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyColor = new Color(0.54f, 0.76f, 0.82f, 1f);
            Color highlight = new Color(0.70f, 0.88f, 0.90f, 1f);
            Vector2[] centers =
            {
                new Vector2(-0.58f, 0.22f), new Vector2(-0.27f, 0.43f), new Vector2(0.08f, 0.40f),
                new Vector2(0.40f, 0.25f), new Vector2(0.62f, -0.02f), new Vector2(0.32f, -0.22f),
                new Vector2(0.05f, 0.02f), new Vector2(-0.30f, 0.06f), new Vector2(-0.58f, -0.18f),
                new Vector2(-0.38f, -0.45f), new Vector2(-0.02f, -0.42f), new Vector2(0.34f, -0.48f)
            };
            float[] radii = { 0.25f, 0.27f, 0.25f, 0.27f, 0.23f, 0.28f, 0.31f, 0.30f, 0.24f, 0.25f, 0.28f, 0.23f };

            for (int y = 0; y < height; y++)
            {
                float ny = ((y + 0.5f) / height - 0.5f) * 2f;
                for (int x = 0; x < width; x++)
                {
                    float nx = ((x + 0.5f) / width - 0.5f) * 2f;
                    int circle = -1;
                    float normalizedDistance = float.MaxValue;
                    for (int i = 0; i < centers.Length; i++)
                    {
                        float distance = Vector2.Distance(new Vector2(nx, ny), centers[i]) / radii[i];
                        if (distance > 1f || distance >= normalizedDistance) continue;
                        normalizedDistance = distance;
                        circle = i;
                    }

                    if (circle < 0)
                    {
                        tex.SetPixel(x, y, Color.clear);
                        continue;
                    }

                    float rim = Mathf.SmoothStep(0.64f, 1f, normalizedDistance);
                    float variation = (circle % 3) * 0.045f;
                    tex.SetPixel(x, y, Color.Lerp(highlight, bodyColor, Mathf.Clamp01(rim + variation)));
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryFighterSilhouette = tex;
            return temporaryFighterSilhouette;
        }

        private Texture2D GenerateTemporaryLacrymariaSilhouetteTexture()
        {
            if (temporaryLacrymariaSilhouette != null) return temporaryLacrymariaSilhouette;

            const int width = 180;
            const int height = 100;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyColor = new Color(0.50f, 0.78f, 0.84f, 1f);
            Color neckColor = new Color(0.68f, 0.91f, 0.94f, 1f);

            for (int y = 0; y < height; y++)
            {
                float ny = ((y + 0.5f) / height - 0.5f) * 2f;
                for (int x = 0; x < width; x++)
                {
                    float nx = ((x + 0.5f) / width - 0.5f) * 2f;
                    float bodyT = Mathf.InverseLerp(-0.96f, -0.08f, nx);
                    float bodyHalfHeight = 0.08f + Mathf.Pow(Mathf.Sin(bodyT * Mathf.PI), 0.62f) * 0.48f;
                    bool dropletBody = nx >= -0.96f && nx <= -0.08f && Mathf.Abs(ny + 0.02f) <= bodyHalfHeight;

                    float neckT = Mathf.InverseLerp(-0.12f, 0.83f, nx);
                    float neckCenter = Mathf.Sin(neckT * Mathf.PI * 1.15f) * 0.045f;
                    float neckHalfHeight = Mathf.Lerp(0.13f, 0.045f, neckT);
                    bool neck = nx >= -0.12f && nx <= 0.83f && Mathf.Abs(ny - neckCenter) <= neckHalfHeight;
                    bool neckTip = InEllipse(nx, ny, 0.84f, neckCenter, 0.13f, 0.105f);

                    if (neck || neckTip) tex.SetPixel(x, y, neckColor);
                    else if (dropletBody) tex.SetPixel(x, y, bodyColor);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryLacrymariaSilhouette = tex;
            return temporaryLacrymariaSilhouette;
        }

        private Texture2D GenerateTemporaryInterdictorSilhouetteTexture()
        {
            if (temporaryInterdictorSilhouette != null) return temporaryInterdictorSilhouette;

            const int width = 156;
            const int height = 92;
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            Color bodyColor = new Color(0.58f, 0.72f, 0.80f, 1f);
            Color legColor = new Color(0.12f, 0.52f, 1f, 1f);

            for (int y = 0; y < height; y++)
            {
                float ny = ((y + 0.5f) / height - 0.5f) * 2f;
                for (int x = 0; x < width; x++)
                {
                    float nx = ((x + 0.5f) / width - 0.5f) * 2f;
                    bool body = InEllipse(nx, ny, -0.08f, 0.22f, 0.58f, 0.42f) ||
                                InEllipse(nx, ny, -0.06f, -0.23f, 0.62f, 0.42f) ||
                                InEllipse(nx, ny, -0.11f, 0f, 0.66f, 0.68f);
                    bool groove = body && Mathf.Abs(ny - Mathf.Sin((nx + 0.2f) * Mathf.PI * 1.6f) * 0.035f) < 0.055f && nx > -0.66f && nx < 0.58f;

                    float tLong = Mathf.InverseLerp(0.42f, 1.04f, nx);
                    float longWave = -0.06f - tLong * 0.54f + Mathf.Sin(tLong * Mathf.PI * 1.7f) * 0.08f;
                    bool longitudinalFlagellum = nx > 0.42f && nx < 1.04f && Mathf.Abs(ny - longWave) < 0.040f;

                    float tTrans = Mathf.InverseLerp(-0.86f, 0.80f, nx);
                    float transWave = Mathf.Sin(tTrans * Mathf.PI * 2.2f) * 0.045f;
                    bool transverseFlagellum = nx > -0.86f && nx < 0.80f && Mathf.Abs(ny - transWave) < 0.036f;

                    if (longitudinalFlagellum || transverseFlagellum) tex.SetPixel(x, y, legColor);
                    else if (body && !groove) tex.SetPixel(x, y, bodyColor);
                    else tex.SetPixel(x, y, Color.clear);
                }
            }

            tex.Apply(false);
            tex.hideFlags = HideFlags.HideAndDontSave;
            temporaryInterdictorSilhouette = tex;
            return temporaryInterdictorSilhouette;
        }

        private bool TemporaryCollectorBody(float x, float y)
        {
            if (x < -0.62f || x > 0.62f) return false;
            return Mathf.Abs(y) < 0.24f || InEllipse(x, y, -0.62f, 0f, 0.23f, 0.24f) || InEllipse(x, y, 0.62f, 0f, 0.23f, 0.24f);
        }

        private bool TemporaryCollectorMouth(float x, float y)
        {
            return x > 0.46f && InEllipse(x, y, 0.66f, 0f, 0.22f, 0.22f);
        }

        private bool TemporaryCollectorFlagellum(float x, float y)
        {
            if (x > -0.58f) return false;
            float t = Mathf.InverseLerp(-1.0f, -0.58f, x);
            float wave = Mathf.Sin(t * Mathf.PI * 2.4f) * 0.12f;
            return Mathf.Abs(y - wave) < 0.035f || InEllipse(x, y, -0.94f, wave, 0.035f, 0.055f);
        }

        private bool InEllipse(float x, float y, float cx, float cy, float rx, float ry)
        {
            float dx = (x - cx) / Mathf.Max(0.001f, rx);
            float dy = (y - cy) / Mathf.Max(0.001f, ry);
            return dx * dx + dy * dy <= 1f;
        }

        private bool InRotatedEllipse(float x, float y, float cx, float cy, float rx, float ry, float degrees)
        {
            float radians = -degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            float dx = x - cx;
            float dy = y - cy;
            float lx = dx * cos - dy * sin;
            float ly = dx * sin + dy * cos;
            return InEllipse(lx, ly, 0f, 0f, rx, ry);
        }

        private Vector2 Rotate(Vector2 value, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2(value.x * cos - value.y * sin, value.x * sin + value.y * cos);
        }

        private Vector2 FallbackFormationOffset(int index, int total)
        {
            if (total <= 1) return Vector2.zero;
            float t = (index + 0.5f) / total;
            float r = Mathf.Sqrt(t);
            float angle = index * 2.3999632f;
            return new Vector2(Mathf.Cos(angle) * r * ActiveFormationRadiusX(), Mathf.Sin(angle) * r * ActiveFormationRadiusY());
        }
    }

    public class V5MorphFadeOut : MonoBehaviour
    {
        private V5CellEntity cell;
        private SpriteRenderer[] renderers;
        private Color[] startColors;
        private Vector2 velocity;
        private float duration = 0.9f;
        private float age;

        public void Begin(Vector2 pushVelocity, float fadeSeconds)
        {
            velocity = pushVelocity;
            duration = Mathf.Max(0.1f, fadeSeconds);
            cell = GetComponent<V5CellEntity>();
            renderers = GetComponentsInChildren<SpriteRenderer>();
            startColors = new Color[renderers.Length];
            for (int i = 0; i < renderers.Length; i++) startColors[i] = renderers[i].color;

            if (cell != null)
            {
                cell.SetSelected(false);
                cell.Stats.currentHp = 0f;
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null) gm.UnregisterCell(cell);
                cell.enabled = false;
            }

            Collider2D[] colliders = GetComponents<Collider2D>();
            for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = false;
        }

        private void Update()
        {
            age += Time.deltaTime;
            float t = Mathf.Clamp01(age / duration);
            transform.position += (Vector3)(velocity * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.one * 0.08f, t);

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Color c = startColors[i];
                c = Color.Lerp(c, new Color(0.16f, 0.22f, 0.25f, 0f), t);
                renderers[i].color = c;
            }

            if (age >= duration) Destroy(gameObject);
        }
    }
}
