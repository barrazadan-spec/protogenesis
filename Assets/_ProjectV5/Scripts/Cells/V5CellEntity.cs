using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class V5CellEntity : MonoBehaviour
    {
        public V5CellDomain Domain = V5CellDomain.LUCA;
        public V5CellRole Role = V5CellRole.Mother;
        public V5EvolutionPath EvolutionPath = V5EvolutionPath.Uncommitted;
        public V5MetabolismType Metabolism = V5MetabolismType.None;
        public V5Directive Directive = V5Directive.Idle;
        public V5CellModeId CellMode = V5CellModeId.FollowLineage;
        public V5CellStats Stats;
        public V5ResourceWallet Resources;
        public readonly List<V5StructureId> Structures = new List<V5StructureId>(16);
        public V5MembraneSegment[] Membrane = new V5MembraneSegment[6];
        public bool IsPlayerOwned = true;
        public bool IsCoreNeutral;
        public int Generation;
        public V5LineageRole LineageRole = V5LineageRole.Generalist;
        public Vector2 DirectiveTarget;
        public V5CellEntity Mother;
        public V5CellEntity AttackTarget;
        public float CarryAmount;
        public V5ResourceKind CarryKind;
        public string PhenotypeLabel = "Linaje";
        public V5GerminalCasteId PhenotypeCaste = V5GerminalCasteId.PlasticDaughter;
        public string PhenotypeRecipeCode = "LIN-00";
        public string PhenotypeRecipeSummary = "Receta basal de linaje";
        public V5FunctionalCasteId FunctionalCaste = V5FunctionalCasteId.Hybrid;
        public V5AttachmentState AttachmentState = V5AttachmentState.Free;
        public int BodySlotIndex = -1;
        public int MorphSlotIndex = -1;
        public V5MorphPartRole MorphPartRole = V5MorphPartRole.Body;
        public bool MorphSlotIsLeg { get { return MorphPartRole == V5MorphPartRole.Legs; } }
        public float AttachmentCooldownUntil;
        public float LastModeChangeTime { get; private set; }
        public string CellModeLabel { get { return V5CellModeLibrary.Get(CellMode).displayName; } }
        public string FunctionalCasteLabel { get { return V5CasteLibrary.Get(FunctionalCaste).displayName; } }
        public bool IsAttachedToBody { get { return AttachmentState == V5AttachmentState.Attached; } }
        public bool IsMorphedOrganism { get { return MorphSlotIndex >= 0; } }
        public int LastInheritedStructureCount { get; private set; }
        public int LastInheritanceCandidateCount { get; private set; }
        public float LastInheritanceChance { get; private set; }
        public bool Selected { get; private set; }
        public int ActiveStructureCount { get { return Structures.Count; } }
        public bool HasPhagocytosis { get; private set; }
        public bool HasPhotosynthesis { get; private set; }
        public bool HasBiofilm { get; private set; }
        public bool HasRecognition { get; private set; }
        public bool HasMucilage { get; private set; }
        public bool HasPiercingStylet { get; private set; }
        public bool HasCryptobiosis { get; private set; }

        private SpriteRenderer spriteRenderer;
        private SpriteRenderer selectionRingRenderer;
        private SpriteRenderer motherBodyRenderer;
        private SpriteRenderer motherHaloRenderer;
        private SpriteRenderer motherCoreRenderer;
        private SpriteRenderer casteHaloRenderer;
        private SpriteRenderer roleMarkerRenderer;
        private readonly List<SpriteRenderer> swarmDotRenderers = new List<SpriteRenderer>(12);
        private CircleCollider2D circleCollider;
        private Vector2 velocity;
        private Vector2 exploreDirection;
        private float aiTimer;
        private float metabolismTimer;
        private float affinityEnvironmentTimer;
        private float visualPhase;
        private float deathTimer;
        private V5Directive lastObservedDirective;
        private bool hasPlayerOrder;
        private bool playerOrderIsAttack;
        private bool playerOrderIsFarm;
        private bool playerOrderIsAttackMove;
        private Vector2 playerMoveTarget;
        private V5CellEntity playerAttackTarget;
        private static Sprite circleSprite;
        private static Sprite ringSprite;
        private const float CoreGuardRadius = 5.6f;
        private const float MotherVisualScale = 1.7f;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            circleCollider = GetComponent<CircleCollider2D>();
            if (circleSprite == null) circleSprite = V5ProceduralSprites.CreateCircleSprite(64);
            if (ringSprite == null) ringSprite = V5ProceduralSprites.CreateRingSprite(96, 0.18f);
            spriteRenderer.sprite = circleSprite;
            spriteRenderer.sortingOrder = 10;
            motherBodyRenderer = CreateVisualChild("MotherBodyVisual", circleSprite, 12);
            motherHaloRenderer = CreateVisualChild("MotherHalo", ringSprite, 8);
            casteHaloRenderer = CreateVisualChild("CasteHalo", ringSprite, 9);
            selectionRingRenderer = CreateVisualChild("SelectionRing", ringSprite, 14);
            motherCoreRenderer = CreateVisualChild("MotherCore", circleSprite, 15);
            roleMarkerRenderer = CreateVisualChild("RoleMarker", circleSprite, 16);
            roleMarkerRenderer.transform.localPosition = new Vector3(0.34f, 0.34f, 0f);
            exploreDirection = Random.insideUnitCircle.normalized;
            visualPhase = Random.Range(0f, 99f);
        }

        private SpriteRenderer CreateVisualChild(string childName, Sprite sprite, int sortingOrder)
        {
            GameObject child = new GameObject(childName);
            child.transform.SetParent(transform, false);
            child.transform.localPosition = Vector3.zero;
            SpriteRenderer renderer = child.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;
            renderer.enabled = false;
            return renderer;
        }

        public void Initialize(V5CellRole role, bool playerOwned, V5EvolutionPath path, Vector2 position)
        {
            Role = role;
            IsPlayerOwned = playerOwned;
            EvolutionPath = path;
            Stats = V5CellStats.MotherDefaults();
            Resources = V5ResourceWallet.Starter();
            transform.position = position;
            if (role == V5CellRole.Daughter) { Stats.maxHp = V5Balance.DaughterHp; Stats.currentHp = Stats.maxHp; Resources = V5ResourceWallet.Cost(20, 16, 8, 7, 4, 2); Generation = 1; }
            if (role == V5CellRole.Granddaughter) { Stats.maxHp = V5Balance.GranddaughterHp; Stats.currentHp = Stats.maxHp; Resources = V5ResourceWallet.Cost(12, 9, 4, 4, 2, 1); Generation = 2; }
            if (!playerOwned) { Role = V5CellRole.Enemy; Resources = V5ResourceWallet.Cost(25, 12, 8, 6, 3, 2); }
            ApplyPath(path, false);
            for (int i = 0; i < Membrane.Length; i++) { Membrane[i].maxHp = Stats.maxHp / 6f; Membrane[i].hp = Membrane[i].maxHp; }
            SyncModeFromDirective(false);
            RefreshVisuals();
            if (V5GameManager.Instance != null) V5GameManager.Instance.RegisterCell(this);
        }

        private void Update()
        {
            if (AttachmentState == V5AttachmentState.Cooldown && Time.time >= AttachmentCooldownUntil)
            {
                AttachmentState = V5AttachmentState.Free;
            }

            if (Stats.currentHp <= 0f)
            {
                deathTimer += Time.deltaTime;
                if (deathTimer >= V5Balance.MotherDeathDelay) Die();
                return;
            }
            ObserveDirectiveChange();
            TickMetabolism();
            TickStress();
            TickRepair();
            if (IsAttachedToBody)
            {
                velocity = Vector2.zero;
                RefreshVisuals();
                return;
            }
            if (IsMorphedOrganism)
            {
                velocity = Vector2.zero;
                RefreshVisuals();
                return;
            }
            aiTimer += Time.deltaTime;
            if (aiTimer >= V5Balance.AiTick)
            {
                aiTimer = 0f;
                ThinkDirective();
            }
            MoveStep();
            RefreshVisuals();
        }

        public void SetSelected(bool value)
        {
            Selected = value;
            RefreshVisuals();
        }

        public void SetFunctionalCaste(V5FunctionalCasteId caste)
        {
            FunctionalCaste = caste;
            RefreshVisuals();
        }

        public void SetBodyAttachment(int slotIndex)
        {
            AttachmentState = V5AttachmentState.Attached;
            BodySlotIndex = slotIndex;
            AttackTarget = null;
            CarryAmount = 0f;
            velocity = Vector2.zero;
            if (V5GameManager.Instance != null && V5GameManager.Instance.MotherCell != null) Mother = V5GameManager.Instance.MotherCell;
            DirectiveTarget = Mother != null ? (Vector2)Mother.transform.position : (Vector2)transform.position;
        }

        public void ClearBodyAttachment(bool voluntary)
        {
            BodySlotIndex = -1;
            AttachmentState = voluntary ? V5AttachmentState.Cooldown : V5AttachmentState.Free;
            AttachmentCooldownUntil = voluntary ? Time.time + 1.5f : 0f;
            velocity = Vector2.zero;
            if (Directive == V5Directive.Idle || Directive == V5Directive.FollowMother) ApplyCellMode(V5CellModeId.FollowLineage);
        }

        public void MoveAttachedToBodySlot(Vector2 target, float followSpeed)
        {
            if (!IsAttachedToBody) return;
            velocity = Vector2.zero;
            DirectiveTarget = target;
            transform.position = Vector2.Lerp(transform.position, target, Mathf.Clamp01(followSpeed * Time.deltaTime));
        }

        public void SnapAttachedToBodySlot(Vector2 target)
        {
            if (!IsAttachedToBody) return;
            velocity = Vector2.zero;
            DirectiveTarget = target;
            transform.position = target;
        }

        public void SetOrganismMorphSlot(int slotIndex)
        {
            SetOrganismMorphSlot(slotIndex, V5MorphPartRole.Body);
        }

        public void SetOrganismMorphSlot(int slotIndex, bool legSlot)
        {
            SetOrganismMorphSlot(slotIndex, legSlot ? V5MorphPartRole.Legs : V5MorphPartRole.Body);
        }

        public void SetOrganismMorphSlot(int slotIndex, V5MorphPartRole partRole)
        {
            MorphSlotIndex = slotIndex;
            MorphPartRole = partRole;
            AttackTarget = null;
            ClearPlayerOrder();
            CarryAmount = 0f;
            velocity = Vector2.zero;
            Directive = V5Directive.Idle;
            DirectiveTarget = transform.position;
            lastObservedDirective = Directive;
        }

        public void ClearOrganismMorphSlot()
        {
            MorphSlotIndex = -1;
            MorphPartRole = V5MorphPartRole.Body;
            velocity = Vector2.zero;
            if (Directive == V5Directive.Idle) ApplyCellMode(V5CellModeId.FollowLineage);
        }

        public void MoveMorphedToSlot(Vector2 target, float followSpeed)
        {
            if (!IsMorphedOrganism) return;
            velocity = Vector2.zero;
            DirectiveTarget = target;
            transform.position = Vector2.Lerp(transform.position, target, Mathf.Clamp01(followSpeed * Time.deltaTime));
        }

        public void ApplyCellMode(V5CellModeId mode)
        {
            ClearPlayerOrder();
            if (mode == V5CellModeId.RouteSpecial) mode = V5CellModeLibrary.ResolveRouteSpecial(this);
            V5CellModeDefinition def = V5CellModeLibrary.Get(mode);
            CellMode = mode;
            Directive = def.directive;
            if (Role != V5CellRole.Mother) LineageRole = def.lineageRole;
            if ((Directive == V5Directive.FollowMother || Directive == V5Directive.ReturnHome) && V5GameManager.Instance != null && V5GameManager.Instance.MotherCell != null)
                Mother = V5GameManager.Instance.MotherCell;
            if (Directive == V5Directive.Colonize) DirectiveTarget = transform.position;
            lastObservedDirective = Directive;
            LastModeChangeTime = Time.time;
            if (IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.AffinityLog != null)
                V5GameManager.Instance.AffinityLog.RecordCellMode(this, mode);
        }

        public void SetPlayerMoveOrder(Vector2 target)
        {
            hasPlayerOrder = true;
            playerOrderIsAttack = false;
            playerOrderIsFarm = false;
            playerOrderIsAttackMove = false;
            playerMoveTarget = target;
            playerAttackTarget = null;
            AttackTarget = null;
            CarryAmount = 0f;
            Directive = V5Directive.Move;
            DirectiveTarget = target;
            lastObservedDirective = Directive;
        }

        public void SetPlayerAttackOrder(V5CellEntity target)
        {
            if (target == null)
            {
                HoldPosition();
                return;
            }

            hasPlayerOrder = true;
            playerOrderIsAttack = true;
            playerOrderIsFarm = false;
            playerOrderIsAttackMove = false;
            playerAttackTarget = target;
            AttackTarget = target;
            CarryAmount = 0f;
            Directive = V5Directive.Attack;
            DirectiveTarget = target.transform.position;
            SyncModeFromDirective(false);
        }

        public void SetPlayerAttackMoveOrder(Vector2 target)
        {
            hasPlayerOrder = true;
            playerOrderIsAttack = false;
            playerOrderIsFarm = false;
            playerOrderIsAttackMove = true;
            playerMoveTarget = target;
            playerAttackTarget = null;
            AttackTarget = null;
            CarryAmount = 0f;
            Directive = V5Directive.Move;
            DirectiveTarget = target;
            lastObservedDirective = Directive;
        }

        public void SetPlayerFarmOrder()
        {
            hasPlayerOrder = true;
            playerOrderIsAttack = false;
            playerOrderIsFarm = true;
            playerOrderIsAttackMove = false;
            playerAttackTarget = null;
            AttackTarget = null;
            CarryAmount = 0f;
            Directive = V5Directive.Farm;
            DirectiveTarget = transform.position;
            SyncModeFromDirective(false);
        }

        public void HoldPosition()
        {
            hasPlayerOrder = true;
            playerOrderIsAttack = false;
            playerOrderIsFarm = false;
            playerOrderIsAttackMove = false;
            playerMoveTarget = transform.position;
            playerAttackTarget = null;
            AttackTarget = null;
            CarryAmount = 0f;
            Directive = V5Directive.Move;
            DirectiveTarget = transform.position;
            lastObservedDirective = Directive;
        }

        public void ClearPlayerOrder()
        {
            hasPlayerOrder = false;
            playerOrderIsAttack = false;
            playerOrderIsFarm = false;
            playerOrderIsAttackMove = false;
            playerAttackTarget = null;
        }

        public void SyncModeFromDirective()
        {
            SyncModeFromDirective(true);
        }

        private void SyncModeFromDirective(bool markChangeTime)
        {
            if (Directive != V5Directive.Move) CellMode = V5CellModeLibrary.ModeForDirective(Directive);
            lastObservedDirective = Directive;
            if (markChangeTime) LastModeChangeTime = Time.time;
        }

        private void ObserveDirectiveChange()
        {
            if (Directive != lastObservedDirective) SyncModeFromDirective(false);
        }

        public float ModeDamageMultiplier()
        {
            return V5CellModeLibrary.Get(CellMode).damageMultiplier * V5CasteLibrary.DamageMultiplier(FunctionalCaste, CellMode, IsAttachedToBody);
        }

        private float ModeSpeedMultiplier()
        {
            return V5CellModeLibrary.Get(CellMode).speedMultiplier * V5CasteLibrary.SpeedMultiplier(FunctionalCaste, IsAttachedToBody);
        }

        private float ModeSynthesisMultiplier()
        {
            return V5CellModeLibrary.Get(CellMode).synthesisMultiplier * V5CasteLibrary.SynthesisMultiplier(FunctionalCaste, CellMode, IsAttachedToBody);
        }

        private float ModeDamageTakenMultiplier()
        {
            return V5CellModeLibrary.Get(CellMode).damageTakenMultiplier * V5CasteLibrary.DamageTakenMultiplier(FunctionalCaste, IsAttachedToBody);
        }

        private float ModeColonizationMultiplier()
        {
            return V5CellModeLibrary.Get(CellMode).colonizationMultiplier * V5CasteLibrary.ColonizationMultiplier(FunctionalCaste, IsAttachedToBody);
        }

        private float ModeRepairMultiplier()
        {
            return V5CellModeLibrary.Get(CellMode).repairMultiplier * V5CasteLibrary.RepairMultiplier(FunctionalCaste, IsAttachedToBody);
        }

        public bool HasStructure(V5StructureId id)
        {
            return Structures.Contains(id);
        }

        public bool CanInstall(V5StructureId id)
        {
            if (HasStructure(id)) return false;
            V5StructureDefinition def = V5EvolutionLibrary.GetStructure(id);
            if (!DomainAllows(def.domain)) return false;
            V5ResourceWallet effectiveCost = EffectiveInstallCost(id, def.cost);
            return CanPayInstallCost(effectiveCost) && V5Balance.BiomassLoadRatio(this) < 1.05f;
        }

        private bool DomainAllows(V5CellDomain required)
        {
            if (required == V5CellDomain.LUCA) return true;
            if (required == Domain) return true;
            return Domain == V5CellDomain.Multicellular && required == V5CellDomain.Eukaryote;
        }

        public bool InstallStructure(V5StructureId id)
        {
            if (!CanInstall(id)) return false;
            V5StructureDefinition def = V5EvolutionLibrary.GetStructure(id);
            V5ResourceWallet effectiveCost = EffectiveInstallCost(id, def.cost);
            V5CellEntity payer = InstallCostPayer(effectiveCost);
            payer.Resources.Pay(effectiveCost);
            Structures.Add(id);
            ApplyStructure(def);
            if (V5GameManager.Instance != null && V5GameManager.Instance.Codex != null) V5GameManager.Instance.Codex.ObserveStructure(id);
            if (V5GameManager.Instance != null && V5GameManager.Instance.AffinityLog != null) V5GameManager.Instance.AffinityLog.RecordStructure(this, id);
            ResolveEvolutionFromStructures();
            RefreshVisuals();
            if (V5GameManager.Instance != null && V5GameManager.Instance.Hud != null && payer != this)
                V5GameManager.Instance.Hud.Toast("Madre pago " + def.displayName + " en " + ShortName());
            return true;
        }

        public V5ResourceWallet EffectiveInstallCost(V5StructureId id)
        {
            V5StructureDefinition def = V5EvolutionLibrary.GetStructure(id);
            return EffectiveInstallCost(id, def.cost);
        }

        private V5ResourceWallet EffectiveInstallCost(V5StructureId id, V5ResourceWallet baseCost)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5GeneSystem genes = gm != null ? gm.Genes : null;
            return V5BiologyCanon.EffectiveStructureCost(id, baseCost, this, genes);
        }

        private bool CanPayInstallCost(V5ResourceWallet cost)
        {
            if (Resources.CanPay(cost)) return true;
            V5CellEntity payer = ColonyBank();
            return payer != null && payer != this && payer.Resources.CanPay(cost);
        }

        private V5CellEntity InstallCostPayer(V5ResourceWallet cost)
        {
            if (Resources.CanPay(cost)) return this;
            V5CellEntity payer = ColonyBank();
            return payer != null && payer.Resources.CanPay(cost) ? payer : this;
        }

        private V5CellEntity ColonyBank()
        {
            if (!IsPlayerOwned || Role == V5CellRole.Mother) return null;
            if (Mother != null) return Mother;
            return V5GameManager.Instance != null ? V5GameManager.Instance.MotherCell : null;
        }


        public void ForceInstallStructure(V5StructureId id)
        {
            if (HasStructure(id)) return;
            V5StructureDefinition def = V5EvolutionLibrary.GetStructure(id);
            Structures.Add(id);
            ApplyStructure(def);
            ResolveEvolutionFromStructures();
            RefreshVisuals();
        }

        public void ApplyMetabolism(V5MetabolismType type)
        {
            V5MetabolismType previous = Metabolism;
            Metabolism = type;
            if (type == V5MetabolismType.Fermentation || type == V5MetabolismType.Chemolithotrophy) Domain = V5CellDomain.Prokaryote;
            if (type == V5MetabolismType.Respiration) Domain = V5CellDomain.Eukaryote;
            if (type == V5MetabolismType.Photosynthesis && Domain == V5CellDomain.LUCA) Domain = V5CellDomain.Prokaryote;
            if (type == V5MetabolismType.Photosynthesis && !HasStructure(V5StructureId.Thylakoid) && !HasStructure(V5StructureId.MicroalgalChloroplast)) InstallStructure(V5StructureId.Thylakoid);
            if (previous != type && V5GameManager.Instance != null && V5GameManager.Instance.AffinityLog != null) V5GameManager.Instance.AffinityLog.RecordMetabolism(this, type);
            ResolveEvolutionFromStructures();
            if (V5GameManager.Instance != null && V5GameManager.Instance.Codex != null) V5GameManager.Instance.Codex.ObserveCell(this);
        }

        public void InheritFrom(V5CellEntity parent, float chance)
        {
            Domain = parent.Domain;
            EvolutionPath = parent.EvolutionPath;
            Metabolism = parent.Metabolism;
            Mother = parent.Role == V5CellRole.Mother ? parent : parent.Mother;
            LastInheritedStructureCount = 0;
            LastInheritanceCandidateCount = parent.Structures.Count;
            LastInheritanceChance = chance;
            for (int i = 0; i < parent.Structures.Count; i++)
            {
                if (Random.value <= chance && !HasStructure(parent.Structures[i]))
                {
                    Structures.Add(parent.Structures[i]);
                    ApplyStructure(V5EvolutionLibrary.GetStructure(parent.Structures[i]));
                    LastInheritedStructureCount++;
                }
            }
            ApplyPath(EvolutionPath, true);
        }

        public void RestoreSavedStructures(List<V5StructureId> savedStructures)
        {
            Structures.Clear();
            HasPhagocytosis = false;
            HasPhotosynthesis = false;
            HasBiofilm = false;
            HasRecognition = false;
            HasMucilage = false;
            HasPiercingStylet = false;
            HasCryptobiosis = false;
            if (savedStructures != null)
            {
                for (int i = 0; i < savedStructures.Count; i++)
                {
                    if (Structures.Contains(savedStructures[i])) continue;
                    Structures.Add(savedStructures[i]);
                    V5StructureDefinition def = V5EvolutionLibrary.GetStructure(savedStructures[i]);
                    HasPhotosynthesis |= def.enablesPhotosynthesis;
                    HasPhagocytosis |= def.enablesPhagocytosis;
                    HasBiofilm |= def.enablesBiofilm;
                    HasRecognition |= def.enablesRecognition;
                    ApplyStructureFlags(savedStructures[i]);
                }
            }
            RefreshVisuals();
        }

        private void ApplyStructure(V5StructureDefinition def)
        {
            Stats.maxHp += def.hpBonus;
            Stats.currentHp += def.hpBonus;
            Stats.speed *= Mathf.Max(0.15f, def.speedMultiplier);
            Stats.atpPerSecond += def.atpBonus;
            Stats.synthesisRate *= Mathf.Max(0.1f, def.synthesisMultiplier);
            Stats.toxinResistance += def.toxinResist;
            Stats.chemicalDamagePerSecond += def.chemicalDamage;
            Stats.physicalDamagePerSecond += def.physicalDamage;
            Stats.colonizationPower += def.colonization;
            HasPhotosynthesis |= def.enablesPhotosynthesis;
            HasPhagocytosis |= def.enablesPhagocytosis;
            HasBiofilm |= def.enablesBiofilm;
            HasRecognition |= def.enablesRecognition;
            ApplyStructureFlags(def.id);
            ApplyStructureSpecialStats(def.id);
        }

        private void ApplyStructureFlags(V5StructureId id)
        {
            if (id == V5StructureId.MicroalgalChloroplast) HasPhotosynthesis = true;
            if (id == V5StructureId.MucilageMatrix) { HasMucilage = true; HasBiofilm = true; }
            if (id == V5StructureId.PiercingStylet) { HasPiercingStylet = true; HasRecognition = true; }
            if (id == V5StructureId.CoronaCilia) HasPhagocytosis = true;
            if (id == V5StructureId.CryptobiosisTun) HasCryptobiosis = true;
        }

        private void ApplyStructureSpecialStats(V5StructureId id)
        {
            if (id == V5StructureId.PiercingStylet) Stats.attackRange = Mathf.Max(Stats.attackRange, 1.8f);
            if (id == V5StructureId.Cuticle)
            {
                Stats.thermalResistance += 0.22f;
                Stats.phTolerance = Mathf.Clamp01(Stats.phTolerance + 0.08f);
            }
            if (id == V5StructureId.CryptobiosisTun)
            {
                Stats.thermalResistance += 0.35f;
                Stats.phTolerance = 0.62f;
            }
        }

        public void ApplyPath(V5EvolutionPath path, bool keepResources)
        {
            EvolutionPath = path;
            V5PathDefinition p = V5EvolutionLibrary.GetPath(path);
            if (path != V5EvolutionPath.Uncommitted) Domain = p.domain;
            Stats.radius = p.worldSize * RoleSizeMultiplier();
            circleCollider.radius = Stats.radius;
            transform.localScale = Vector3.one * (Stats.radius * 2f);
            if (!keepResources && Role == V5CellRole.Enemy)
            {
                for (int i = 0; i < p.keyStructures.Length && i < 2; i++)
                {
                    if (!HasStructure(p.keyStructures[i]))
                    {
                        Structures.Add(p.keyStructures[i]);
                        ApplyStructure(V5EvolutionLibrary.GetStructure(p.keyStructures[i]));
                    }
                }
            }
        }

        private float RoleSizeMultiplier()
        {
            if (!IsPlayerOwned) return 1f;
            if (Role == V5CellRole.Mother) return 1.20f;
            if (Role == V5CellRole.Daughter) return 0.88f;
            if (Role == V5CellRole.Granddaughter) return 0.76f;
            if (Role == V5CellRole.Apex) return 1.15f;
            return 1f;
        }

        private void ResolveEvolutionFromStructures()
        {
            if (IsPlayerOwned && Role == V5CellRole.Mother)
            {
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.RouteLifecycle != null)
                {
                    gm.RouteLifecycle.TryConsolidateNow("mother");
                    return;
                }

                V5EvolutionAffinityResult bestAffinity = V5EvolutionAffinitySystem.BestRoute(this);
                if (bestAffinity.path != V5EvolutionPath.Uncommitted && bestAffinity.path != EvolutionPath && bestAffinity.Score01 >= V5Balance.RouteConsolidationAffinityThreshold)
                    ApplyPath(bestAffinity.path, true);
                return;
            }

            int bacteria = Score(V5StructureId.BacterialFlagellum) + Score(V5StructureId.Plasmid) + Score(V5StructureId.Fimbriae);
            int archaea = Score(V5StructureId.Capsule) + Score(V5StructureId.Catalase) + (Metabolism == V5MetabolismType.Chemolithotrophy ? 2 : 0);
            int cyano = Score(V5StructureId.Thylakoid) * 2 + (Metabolism == V5MetabolismType.Photosynthesis ? 2 : 0);
            int fungus = Score(V5StructureId.InvasiveHypha) * 2 + Score(V5StructureId.CelluloseWall);
            int amoeba = Score(V5StructureId.Lysosome) * 2 + Score(V5StructureId.StorageVacuole);
            int flag = Score(V5StructureId.EukaryoticFlagellum) * 2;
            int ciliate = Score(V5StructureId.Cilia) * 2 + Score(V5StructureId.StorageVacuole);
            int microalga = Score(V5StructureId.MicroalgalChloroplast) * 3 + Score(V5StructureId.CelluloseWall) + (Metabolism == V5MetabolismType.Photosynthesis && Domain == V5CellDomain.Eukaryote ? 1 : 0);
            int slime = Score(V5StructureId.MucilageMatrix) * 3 + Score(V5StructureId.SecretoryVesicle) + Score(V5StructureId.StorageVacuole);
            int rotifer = Score(V5StructureId.CoronaCilia) * 3 + Score(V5StructureId.Lysosome) + Score(V5StructureId.Cuticle);
            int nematode = Score(V5StructureId.PiercingStylet) * 3 + Score(V5StructureId.Cuticle) + Score(V5StructureId.EukaryoticFlagellum);
            int tardigrade = Score(V5StructureId.CryptobiosisTun) * 3 + Score(V5StructureId.Cuticle) + Score(V5StructureId.Catalase);
            int stem = Score(V5StructureId.StemPlasticity) * 3;

            V5EvolutionPath best = EvolutionPath;
            int bestScore = 2;
            Consider(V5EvolutionPath.Bacteria, bacteria, ref best, ref bestScore);
            Consider(V5EvolutionPath.Archaea, archaea, ref best, ref bestScore);
            Consider(V5EvolutionPath.Cyanobacteria, cyano, ref best, ref bestScore);
            Consider(V5EvolutionPath.Fungus, fungus, ref best, ref bestScore);
            Consider(V5EvolutionPath.Amoeba, amoeba, ref best, ref bestScore);
            Consider(V5EvolutionPath.Flagellate, flag, ref best, ref bestScore);
            Consider(V5EvolutionPath.Ciliate, ciliate, ref best, ref bestScore);
            Consider(V5EvolutionPath.Microalga, microalga, ref best, ref bestScore);
            Consider(V5EvolutionPath.SlimeMold, slime, ref best, ref bestScore);
            Consider(V5EvolutionPath.Rotifer, rotifer, ref best, ref bestScore);
            Consider(V5EvolutionPath.Nematode, nematode, ref best, ref bestScore);
            if (!IsPlayerOwned) Consider(V5EvolutionPath.Tardigrade, tardigrade, ref best, ref bestScore);
            Consider(V5EvolutionPath.StemCell, stem, ref best, ref bestScore);
            if (best != EvolutionPath) ApplyPath(best, true);
        }

        private int Score(V5StructureId id) { return HasStructure(id) ? 1 : 0; }
        private void Consider(V5EvolutionPath p, int score, ref V5EvolutionPath best, ref int bestScore) { if (score > bestScore) { bestScore = score; best = p; } }

        private void TickMetabolism()
        {
            metabolismTimer += Time.deltaTime;
            V5EnvironmentGrid env = V5GameManager.Instance != null ? V5GameManager.Instance.Environment : null;
            if (env == null) return;
            int tx, ty;
            env.WorldToTile(transform.position, out tx, out ty);
            float light = env.lightLevel[tx, ty];
            float oxygen = env.oxygen[tx, ty];
            float tox = env.toxins[tx, ty];
            float acid = env.acidity[tx, ty];

            float biomassForAtp = Mathf.Max(0f, Resources.biomass - 6f);
            float biomassUse = Mathf.Min(biomassForAtp, 0.20f * Stats.synthesisRate * ModeSynthesisMultiplier() * Time.deltaTime);
            if (biomassUse > 0f)
            {
                Resources.biomass -= biomassUse;
                Resources.atp += biomassUse * 4.0f;
            }

            Resources.atp += Stats.atpPerSecond * Time.deltaTime;
            if (Metabolism == V5MetabolismType.Respiration)
            {
                Resources.atp += (1.6f + oxygen * 1.6f) * Time.deltaTime;
                env.ModifyArea(transform.position, 1.2f, 0f, 0f, -0.004f, 0.002f, 0f, 0f, 0f);
            }
            else if (Metabolism == V5MetabolismType.Fermentation)
            {
                Resources.atp += 1.55f * Time.deltaTime;
                Stats.stress += 0.18f * Time.deltaTime;
                env.ModifyArea(transform.position, 1.25f, 0f, 0f, 0f, 0.006f, 0.001f, 0f, 0f);
            }
            else if (Metabolism == V5MetabolismType.Photosynthesis || HasPhotosynthesis)
            {
                Resources.atp += light * 2.8f * Time.deltaTime;
                env.ModifyArea(transform.position, 1.8f, 0f, 0f, 0.009f, 0f, 0f, 0.0008f, 0f);
            }
            else if (Metabolism == V5MetabolismType.Chemolithotrophy)
            {
                Resources.atp += 1.35f * Time.deltaTime;
                env.ModifyArea(transform.position, 1.6f, -0.0004f, 0f, 0f, 0.004f, 0.006f, 0.0005f, 0f);
            }
            if (HasStructure(V5StructureId.MicroalgalChloroplast))
            {
                Resources.atp += light * 1.1f * Time.deltaTime;
                env.ModifyArea(transform.position, 2.1f, 0f, 0.001f, 0.006f, -0.001f, 0f, 0.0008f, 0f);
            }
            if (HasMucilage)
            {
                float detritusGain = env.detritus[tx, ty] * 0.14f * Time.deltaTime;
                Resources.biomass += detritusGain;
                env.ModifyArea(transform.position, 2.4f, 0.0008f, 0f, 0f, 0.0018f, 0f, 0.004f, -0.0035f);
            }
            if (HasStructure(V5StructureId.CoronaCilia))
            {
                Resources.biomass += env.detritus[tx, ty] * 0.05f * Time.deltaTime;
                env.ModifyArea(transform.position, 1.9f, 0.002f, 0f, 0f, -0.0012f, 0f, 0.0007f, -0.004f);
            }
            if (HasCryptobiosis && (Stats.currentHp < Stats.maxHp * 0.45f || Stats.stress > 72f))
            {
                Resources.atp += 0.18f * Time.deltaTime;
                Stats.stress = Mathf.Max(0f, Stats.stress - 0.75f * Time.deltaTime);
                env.ModifyArea(transform.position, 1.6f, 0f, 0f, 0f, -0.0015f, 0f, 0.002f, 0f);
            }
            if (tox > Stats.toxinResistance + 0.15f) Damage((tox - Stats.toxinResistance) * 0.7f * Time.deltaTime, V5DamageKind.Chemical, transform.position);
            if (Mathf.Abs(acid - Stats.phTolerance) > 0.45f + Stats.toxinResistance) Stats.stress += 0.8f * Time.deltaTime;
            affinityEnvironmentTimer += Time.deltaTime;
            if (affinityEnvironmentTimer >= 8f)
            {
                affinityEnvironmentTimer = 0f;
                if (IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.AffinityLog != null)
                    V5GameManager.Instance.AffinityLog.RecordEnvironment(this, light, oxygen, tox, acid, env.detritus[tx, ty]);
            }
        }

        private void TickStress()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.CoreMode)
            {
                Stats.stress = 0f;
                return;
            }

            if (gm != null && IsPlayerOwned) Stats.stress += V5Balance.StressFromPopulationLoad(gm.PlayerPopulationLoad()) * Time.deltaTime;
            float load = V5Balance.BiomassLoadRatio(this);
            if (load > 0.85f) Stats.stress += (load - 0.85f) * 2.5f * Time.deltaTime;
            Stats.stress += V5CellModeLibrary.Get(CellMode).stressPerSecond * Time.deltaTime;
            if (IsPlayerOwned && Stats.stress > 0f)
                Stats.stress = Mathf.Max(0f, Stats.stress - 0.55f * Time.deltaTime);
            Stats.stress = Mathf.Clamp(Stats.stress, 0f, 100f);
            if (Stats.stress > 95f) Damage(1.2f * Time.deltaTime, V5DamageKind.Osmotic, transform.position);
        }

        private void TickRepair()
        {
            if (Resources.atp <= 0f) return;
            float repair = Stats.repairPerSecond * Stats.synthesisRate * ModeRepairMultiplier() * Time.deltaTime;
            if (Stats.currentHp < Stats.maxHp)
            {
                float spent = Mathf.Min(repair, Resources.atp * 2f, Stats.maxHp - Stats.currentHp);
                Stats.currentHp += spent;
                Resources.atp -= spent * 0.08f;
            }
        }

        private void ThinkDirective()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (!IsPlayerOwned && GetComponent<V5EnemyBrain>() != null) return;
            if (ApplyPlayerOrderOverride()) return;
            if (ApplyCorePlayerGuard(gm)) return;
            if (Directive == V5Directive.Idle && Role != V5CellRole.Mother) { Directive = V5Directive.FollowMother; SyncModeFromDirective(false); }
            if (Directive == V5Directive.FollowMother && Mother != null) DirectiveTarget = Mother.transform.position;
            bool returningToCoreTerritory = TryReturnToCoreTerritory(gm);
            if (Directive == V5Directive.ReturnHome && Mother != null)
            {
                DirectiveTarget = Mother.transform.position;
                if (Vector2.Distance(transform.position, Mother.transform.position) < V5Balance.ResourceDepositRadius)
                {
                    if (CarryAmount > 0.01f)
                    {
                        Mother.Resources.Add(CarryKind, CarryAmount);
                        CarryAmount = 0f;
                    }
                    Stats.stress = Mathf.Max(0f, Stats.stress - 0.8f * V5Balance.AiTick);
                }
            }
            if (Directive == V5Directive.Defend && Mother != null)
            {
                Vector2 offset = ((Vector2)transform.position - (Vector2)Mother.transform.position).normalized * 3.2f;
                if (offset.sqrMagnitude < 0.1f) offset = Random.insideUnitCircle.normalized * 3.2f;
                DirectiveTarget = (Vector2)Mother.transform.position + offset;
            }
            if (Directive == V5Directive.Explore && !returningToCoreTerritory)
            {
                if (Random.value < 0.08f) exploreDirection = (exploreDirection + Random.insideUnitCircle * 0.8f).normalized;
                V5CoreTerritorySystem territory = CoreTerritoryForAutonomy(gm);
                DirectiveTarget = territory != null
                    ? territory.ScoutTarget(transform.position, exploreDirection, 7f)
                    : (Vector2)transform.position + exploreDirection * 7f;
            }
            if (Directive == V5Directive.Colonize)
            {
                V5EnvironmentGrid env = gm != null ? gm.Environment : null;
                if (env != null)
                {
                    float geneColonization = (gm.Genes != null && IsPlayerOwned) ? gm.Genes.ColonizationMultiplier : 1f;
                    float adaptationColonization = (gm.Adaptations != null && IsPlayerOwned) ? gm.Adaptations.ColonizationMultiplier : 1f;
                    env.ModifyArea(transform.position, 2.0f + Stats.colonizationPower, -0.001f, 0f, 0.001f, -0.001f, 0f, 0.006f * Stats.colonizationPower * geneColonization * adaptationColonization * ModeColonizationMultiplier() * ((gm.LineageUpgrades != null && IsPlayerOwned) ? gm.LineageUpgrades.ColonizationMultiplier(this) : 1f), 0f);
                    if (IsPlayerOwned && V5GameManager.Instance.AffinityLog != null) V5GameManager.Instance.AffinityLog.RecordColonization(this);
                }
            }
            if (Directive == V5Directive.Farm && !returningToCoreTerritory) FarmStep();
            if (Directive == V5Directive.Attack) AcquireAttackTarget();
            else if (!returningToCoreTerritory && Role != V5CellRole.Mother && Directive != V5Directive.ReturnHome && !IsCoreNeutral) ReactToNearbyThreat();
        }

        private bool ApplyCorePlayerGuard(V5GameManager gm)
        {
            if (gm == null || !gm.CoreMode || !IsPlayerOwned || IsCoreNeutral || Role == V5CellRole.Mother || IsAttachedToBody || IsMorphedOrganism || hasPlayerOrder)
                return false;

            V5CellEntity coreMother = gm.MotherCell != null ? gm.MotherCell : Mother;
            AttackTarget = null;
            CarryAmount = 0f;
            Directive = V5Directive.Move;
            CellMode = V5CellModeId.FollowLineage;
            lastObservedDirective = Directive;

            if (coreMother == null)
            {
                DirectiveTarget = transform.position;
                velocity = Vector2.zero;
                return true;
            }

            Mother = coreMother;
            int guardIndex = 0;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell == null || !cell.IsPlayerOwned || cell.IsCoreNeutral || cell.Role == V5CellRole.Mother || cell.IsAttachedToBody || cell.IsMorphedOrganism || cell.hasPlayerOrder)
                    continue;
                if (cell == this) break;
                guardIndex++;
            }

            float angle = guardIndex * 2.39996323f;
            float radius = Mathf.Min(CoreGuardRadius - 0.35f, 1.4f + Mathf.Sqrt(guardIndex) * 0.44f);
            Vector2 guardTarget = (Vector2)coreMother.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            float motherDistance = Vector2.Distance(transform.position, coreMother.transform.position);
            if (motherDistance > CoreGuardRadius || Vector2.SqrMagnitude((Vector2)transform.position - guardTarget) > 0.45f * 0.45f)
                DirectiveTarget = guardTarget;
            else
                DirectiveTarget = transform.position;
            return true;
        }

        private bool ApplyPlayerOrderOverride()
        {
            if (!hasPlayerOrder) return false;

            if (playerOrderIsAttackMove)
            {
                if (playerAttackTarget == null || playerAttackTarget.Stats.currentHp <= 0f)
                    playerAttackTarget = FindAttackMoveTarget(playerMoveTarget);

                if (playerAttackTarget != null && playerAttackTarget.Stats.currentHp > 0f)
                {
                    Directive = V5Directive.Attack;
                    AttackTarget = playerAttackTarget;
                    DirectiveTarget = playerAttackTarget.transform.position;
                    CarryAmount = 0f;
                    return true;
                }

                Directive = V5Directive.Move;
                AttackTarget = null;
                CarryAmount = 0f;
                if (Vector2.SqrMagnitude((Vector2)transform.position - playerMoveTarget) <= 0.18f * 0.18f)
                    HoldPosition();
                else
                    DirectiveTarget = playerMoveTarget;
                return true;
            }

            if (playerOrderIsFarm)
            {
                Directive = V5Directive.Farm;
                AttackTarget = null;
                FarmStep();
                return true;
            }

            if (playerOrderIsAttack)
            {
                if (playerAttackTarget == null || playerAttackTarget.Stats.currentHp <= 0f)
                {
                    HoldPosition();
                    return true;
                }

                Directive = V5Directive.Attack;
                AttackTarget = playerAttackTarget;
                DirectiveTarget = playerAttackTarget.transform.position;
                CarryAmount = 0f;
                return true;
            }

            Directive = V5Directive.Move;
            AttackTarget = null;
            CarryAmount = 0f;
            if (Vector2.SqrMagnitude((Vector2)transform.position - playerMoveTarget) <= 0.18f * 0.18f)
                playerMoveTarget = transform.position;
            DirectiveTarget = playerMoveTarget;
            return true;
        }

        private V5CellEntity FindAttackMoveTarget(Vector2 destination)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return null;
            IReadOnlyList<V5CellEntity> targets = IsPlayerOwned ? gm.NonPlayerCells : gm.PlayerCells;
            if (targets == null) return null;

            Vector2 from = transform.position;
            float acquireRange = Mathf.Max(11f, Stats.sensorRange + 1.5f);
            float destinationRange = 6f;
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

        private bool TryReturnToCoreTerritory(V5GameManager gm)
        {
            V5CoreTerritorySystem territory = CoreTerritoryForAutonomy(gm);
            if (territory == null || territory.IsInside(transform.position)) return false;
            AttackTarget = null;
            CarryAmount = 0f;
            DirectiveTarget = territory.ReturnTarget(transform.position);
            return true;
        }

        private V5CoreTerritorySystem CoreTerritoryForAutonomy(V5GameManager gm)
        {
            if (gm == null || !gm.CoreMode || gm.CoreTerritory == null) return null;
            if (!IsPlayerOwned || Role == V5CellRole.Mother) return null;
            if (hasPlayerOrder) return null;
            if (Directive != V5Directive.Farm && Directive != V5Directive.Explore) return null;
            return gm.CoreTerritory;
        }

        private void ReactToNearbyThreat()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> enemies = IsPlayerOwned ? gm.NonPlayerCells : gm.PlayerCells;
            float reach = Stats.attackRange + Stats.radius + 2.5f;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null) continue;
                if (Vector2.Distance(transform.position, enemies[i].transform.position) <= reach)
                {
                    AttackTarget = enemies[i];
                    DirectiveTarget = enemies[i].transform.position;
                    return;
                }
            }
            AttackTarget = null;
        }

        private void MoveStep()
        {
            if (IsAttachedToBody) return;
            if (Directive == V5Directive.Idle || Directive == V5Directive.Colonize) { ApplySeparation(); return; }
            Vector2 pos = transform.position;
            Vector2 target = DirectiveTarget;
            if (Directive == V5Directive.Attack && AttackTarget != null) target = AttackTarget.transform.position;
            Vector2 desired = target - pos;
            if (desired.magnitude > 0.25f)
            {
                Vector2 dir = desired.normalized;
                float moveMult = ModeSpeedMultiplier();
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.LineageUpgrades != null && IsPlayerOwned) moveMult *= gm.LineageUpgrades.MoveMultiplier(this);
                if (Role == V5CellRole.Mother && gm != null && gm.Body != null) moveMult *= gm.Body.MovementMultiplier;
                if (!IsPlayerOwned && gm != null && gm.CoreMotherProduction != null) moveMult *= gm.CoreMotherProduction.EnemySpeedMultiplierAt(pos);
                if (!IsPlayerOwned && gm != null && gm.OrganismMorph != null) moveMult *= gm.OrganismMorph.PlayerInterdictionSpeedMultiplierAt(pos);
                velocity += dir * Stats.speed * moveMult * 7f * Time.deltaTime;
            }
            ApplySeparation();
            float capMult = ModeSpeedMultiplier();
            V5GameManager capGm = V5GameManager.Instance;
            if (capGm != null && capGm.LineageUpgrades != null && IsPlayerOwned) capMult *= capGm.LineageUpgrades.MoveMultiplier(this);
            if (Role == V5CellRole.Mother && capGm != null && capGm.Body != null) capMult *= capGm.Body.MovementMultiplier;
            if (!IsPlayerOwned && capGm != null && capGm.CoreMotherProduction != null) capMult *= capGm.CoreMotherProduction.EnemySpeedMultiplierAt(transform.position);
            if (!IsPlayerOwned && capGm != null && capGm.OrganismMorph != null) capMult *= capGm.OrganismMorph.PlayerInterdictionSpeedMultiplierAt(transform.position);
            velocity = Vector2.ClampMagnitude(velocity, Stats.speed * capMult);
            transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity = Vector2.Lerp(velocity, Vector2.zero, 2.8f * Time.deltaTime);
            V5EnvironmentGrid env = V5GameManager.Instance != null ? V5GameManager.Instance.Environment : null;
            if (env != null && ((Vector2)transform.position).magnitude > env.MapRadius)
            {
                Vector2 clamped = ((Vector2)transform.position).normalized * env.MapRadius;
                transform.position = clamped;
                velocity *= -0.25f;
            }
        }

        private void ApplySeparation()
        {
            if (V5GameManager.Instance == null) return;
            IReadOnlyList<V5CellEntity> list = IsPlayerOwned ? V5GameManager.Instance.PlayerCells : V5GameManager.Instance.NonPlayerCells;
            Vector2 pos = transform.position;
            Vector2 push = Vector2.zero;
            for (int i = 0; i < list.Count; i++)
            {
                V5CellEntity other = list[i];
                if (other == null || other == this) continue;
                if (IsAttachedToBody || other.IsAttachedToBody || IsMorphedOrganism || other.IsMorphedOrganism) continue;
                Vector2 d = pos - (Vector2)other.transform.position;
                float min = (Stats.radius + other.Stats.radius) * V5Balance.CellSeparationRadius;
                if (d.sqrMagnitude < min * min && d.sqrMagnitude > 0.001f)
                {
                    push += d.normalized * (min - d.magnitude);
                }
            }
            velocity += push * V5Balance.CellSeparationStrength * Time.deltaTime;
        }

        private void FarmStep()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Resources == null) return;
            CarryAmount = 0f;

            V5CoreTerritorySystem territory = CoreTerritoryForAutonomy(gm);
            if (territory != null && !territory.IsInside(transform.position))
            {
                DirectiveTarget = territory.ReturnTarget(transform.position);
                return;
            }

            V5ResourceNode node = territory != null
                ? territory.FindNearestNode(transform.position, 18f)
                : gm.Resources.FindNearestNode(transform.position, 18f);
            if (node == null)
            {
                float globalSearchRange = gm.Environment != null ? gm.Environment.MapRadius * 2f : V5Balance.DefaultMapRadius * 2f;
                node = territory != null
                    ? territory.FindNearestNode(transform.position, Mathf.Max(32f, globalSearchRange))
                    : gm.Resources.FindNearestNode(transform.position, Mathf.Max(32f, globalSearchRange));
            }

            if (node != null)
            {
                DirectiveTarget = node.transform.position;
                if (Vector2.Distance(transform.position, node.transform.position) < V5Balance.ResourcePickupRadius)
                {
                    float taken = node.Harvest(14f * Stats.synthesisRate * ModeSynthesisMultiplier() * V5Balance.AiTick * ((V5GameManager.Instance != null && V5GameManager.Instance.LineageUpgrades != null && IsPlayerOwned) ? V5GameManager.Instance.LineageUpgrades.FarmMultiplier(this) : 1f));
                    CarryKind = node.kind;
                    if (taken > 0f)
                    {
                        V5CellEntity bank = ColonyBank();
                        if (bank == null) bank = Mother != null ? Mother : this;
                        bank.Resources.Add(CarryKind, taken);
                    }
                    CarryAmount = 0f;
                }
            }
            else
            {
                DirectiveTarget = territory != null ? territory.ClampInside(transform.position) : (Vector2)transform.position;
            }
        }

        private void AcquireAttackTarget()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> enemies = IsPlayerOwned ? gm.NonPlayerCells : gm.PlayerCells;
            float best = 9999f;
            V5CellEntity target = null;
            for (int i = 0; i < enemies.Count; i++)
            {
                if (enemies[i] == null) continue;
                float d = Vector2.Distance(transform.position, enemies[i].transform.position);
                if (d < best && d < Stats.sensorRange)
                {
                    best = d;
                    target = enemies[i];
                }
            }
            AttackTarget = target;
            if (target != null) DirectiveTarget = target.transform.position;
        }

        public void Damage(float amount, V5DamageKind kind, Vector2 source)
        {
            float mitigated = amount;
            if (kind == V5DamageKind.Physical) mitigated *= Mathf.Clamp01(1f - Stats.physicalArmor);
            if (kind == V5DamageKind.Chemical) mitigated *= Mathf.Clamp01(1f - Stats.toxinResistance);
            if (kind == V5DamageKind.Thermal) mitigated *= Mathf.Clamp01(1f - Stats.thermalResistance);
            if (kind == V5DamageKind.Oxidative) mitigated *= Mathf.Clamp01(1f - Stats.toxinResistance * 0.7f);
            if (kind == V5DamageKind.Acid) mitigated *= Mathf.Clamp01(0.55f + Mathf.Abs(Stats.phTolerance - 0.5f));
            if (HasCryptobiosis && (Stats.currentHp < Stats.maxHp * 0.45f || Stats.stress > 72f)) mitigated *= 0.48f;
            if (IsPlayerOwned && Role == V5CellRole.Mother)
            {
                mitigated *= V5Balance.MotherCombatIncomingDamageMultiplier;
                Stats.stress += mitigated * 0.22f;
            }
            V5SquadTacticsSystem squad = V5GameManager.Instance != null ? V5GameManager.Instance.Squads : null;
            if (squad == null) squad = FindFirstObjectByType<V5SquadTacticsSystem>();
            if (squad != null && IsPlayerOwned) mitigated *= squad.DamageTakenMultiplier(this, kind);
            if (IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.LineageUpgrades != null) mitigated *= V5GameManager.Instance.LineageUpgrades.DamageTakenMultiplier(this);
            mitigated *= ModeDamageTakenMultiplier();
            Stats.currentHp -= Mathf.Max(0f, mitigated);
            float stressMult = IsPlayerOwned && Role == V5CellRole.Mother ? V5Balance.MotherCombatStressMultiplier : 1f;
            Stats.stress += mitigated * 0.35f * stressMult;
            V5NeutralCampMember neutralCamp = IsCoreNeutral ? GetComponent<V5NeutralCampMember>() : null;
            if (neutralCamp != null) neutralCamp.NotifyDamaged(source);
        }

        public bool CanDivide()
        {
            if (Role == V5CellRole.Apex) return false;
            if (IsMorphedOrganism) return false;
            if (Role != V5CellRole.Mother && Generation >= 2) return false;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.CoreMode && Role != V5CellRole.Mother) return false;
            if (gm != null && IsPlayerOwned && !gm.CanAddPlayerCellFrom(this)) return false;
            if (gm != null && gm.CoreMode)
                return Resources.biomass >= V5Balance.DivisionCostBiomass(this) && Stats.currentHp > Stats.maxHp * 0.35f;
            return Resources.atp >= V5Balance.DivisionCostATP(this) && Resources.biomass >= V5Balance.DivisionCostBiomass(this) && Stats.stress < 92f && Stats.currentHp > Stats.maxHp * 0.35f;
        }

        public V5CellEntity Divide()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null || !CanDivide()) return null;
            float atpCost = V5Balance.DivisionCostATP(this);
            float bioCost = V5Balance.DivisionCostBiomass(this);
            if (!gm.CoreMode) Resources.atp -= atpCost;
            Resources.biomass -= bioCost;
            if (!gm.CoreMode) Stats.stress += Domain == V5CellDomain.Prokaryote ? 5f : 9f;
            V5CellRole childRole = Generation == 0 ? V5CellRole.Daughter : V5CellRole.Granddaughter;
            Vector2 spawn = (Vector2)transform.position + Random.insideUnitCircle.normalized * (Stats.radius + 0.8f);
            V5CellEntity child = gm.CellFactory.SpawnPlayerCell(spawn, childRole, this);
            child.LineageRole = Role == V5CellRole.Mother ? V5LineageRole.Generalist : LineageRole;
            child.Directive = Role == V5CellRole.Mother ? V5Directive.FollowMother : Directive;
            if (child.Directive == V5Directive.Idle) child.Directive = V5Directive.FollowMother;
            child.ApplyCellMode(Role == V5CellRole.Mother ? V5CellModeId.FollowLineage : CellMode);
            if (child.IsPlayerOwned)
            {
                string message = "Division: hija G" + child.Generation + " heredo " + child.LastInheritedStructureCount + "/" + child.LastInheritanceCandidateCount + " estructuras (" + (child.LastInheritanceChance * 100f).ToString("0") + "%)";
                if (gm.Hud != null) gm.Hud.Toast(message);
                V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                if (feedback != null) feedback.PushFloating(message, child.transform.position, new Color(0.75f, 1f, 0.85f, 1f));
            }
            return child;
        }

        private void Die()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.CoreMode && Role == V5CellRole.Mother && IsPlayerOwned)
            {
                V5CoreRivalColonySystem rival = FindFirstObjectByType<V5CoreRivalColonySystem>();
                if (rival == null || !rival.ShouldAllowCoreMotherDefeat())
                {
                    Stats.currentHp = Mathf.Max(Stats.maxHp * 0.5f, 1f);
                    Stats.stress = 0f;
                    deathTimer = 0f;
                    Debug.LogWarning("[V5Core] Mother death ignored in sandbox core mode.");
                    return;
                }
            }

            if (gm != null)
            {
                float recycleMultiplier = (gm.Genes != null && IsPlayerOwned) ? gm.Genes.DeathRecycleMultiplier() : 1f;
                if (gm.Adaptations != null && IsPlayerOwned) recycleMultiplier *= gm.Adaptations.DeathRecycleMultiplier();
                if (gm.Environment != null) gm.Environment.ModifyArea(transform.position, 2.5f, 0.04f * recycleMultiplier, 0f, 0f, 0.02f, 0f, 0f, 0.12f * recycleMultiplier);
                if (IsAttachedToBody && gm.Body != null) gm.Body.NotifyAttachedCellDied(this);
                bool continuityHandled = false;
                if (Role == V5CellRole.Mother && IsPlayerOwned)
                {
                    V5ColonialContinuitySystem continuity = gm.Continuity != null ? gm.Continuity : FindFirstObjectByType<V5ColonialContinuitySystem>();
                    continuityHandled = continuity != null && continuity.TryHandleMotherDeath(this);
                }
                gm.UnregisterCell(this);
                if (Role == V5CellRole.Mother && IsPlayerOwned && !continuityHandled) gm.Lose("murió la célula madre");
            }
            Destroy(gameObject);
        }

        private void RefreshVisuals()
        {
            if (spriteRenderer == null) return;
            bool isMother = IsPlayerOwned && Role == V5CellRole.Mother;
            Color c = V5EvolutionLibrary.ColorForPath(EvolutionPath);
            if (EvolutionPath == V5EvolutionPath.Uncommitted) c = V5Colors.LUCA;
            if (IsPlayerOwned && !isMother) c = Color.Lerp(c, V5CasteLibrary.Get(FunctionalCaste).primaryColor, 0.52f);
            if (isMother) c = Color.Lerp(c, new Color(0.35f, 1f, 0.92f, 1f), 0.35f);
            if (!IsPlayerOwned) c = IsCoreNeutral ? Color.Lerp(c, new Color(0.92f, 0.78f, 0.36f, 1f), 0.42f) : Color.Lerp(c, Color.red, 0.35f);
            if (Selected) c = Color.Lerp(c, V5Colors.Selected, 0.45f);
            if (IsAttachedToBody) c = Color.Lerp(c, new Color(0.78f, 1f, 0.95f, 1f), 0.25f);
            if (IsMorphedOrganism)
                c = Color.Lerp(c, MorphRoleTint(), MorphPartRole == V5MorphPartRole.Body ? 0.46f : 0.64f);
            if (!IsPlayerOwned && IsMorphedOrganism) c = IsCoreNeutral ? Color.Lerp(c, new Color(1f, 0.78f, 0.22f, 1f), 0.58f) : Color.Lerp(c, new Color(1f, 0.05f, 0.03f, 1f), 0.58f);
            if (Stats.currentHp < Stats.maxHp * 0.3f) c = Color.Lerp(c, Color.red, 0.55f);
            float pulse = 1f + Mathf.Sin(Time.time * 2.3f + visualPhase) * 0.035f;
            spriteRenderer.color = isMother ? new Color(c.r, c.g, c.b, 0f) : c;
            spriteRenderer.sortingOrder = isMother ? 12 : 10;
            if (motherBodyRenderer != null)
            {
                motherBodyRenderer.enabled = isMother;
                motherBodyRenderer.color = c;
                motherBodyRenderer.transform.localScale = Vector3.one * MotherVisualScale;
            }
            transform.localScale = Vector3.one * (Stats.radius * 2f * pulse);
            circleCollider.radius = IsPlayerOwned && !isMother ? 0.68f : 0.5f;
            RefreshRoleMarkers(isMother);
            RefreshSwarmVisuals(c, isMother);
        }

        private Color MorphRoleTint()
        {
            if (MorphPartRole == V5MorphPartRole.Legs) return new Color(0.22f, 0.62f, 1f, 1f);
            if (MorphPartRole == V5MorphPartRole.Mouth) return new Color(1f, 0.30f, 0.24f, 1f);
            return new Color(0.70f, 0.82f, 0.86f, 1f);
        }

        private void RefreshRoleMarkers(bool isMother)
        {
            if (motherHaloRenderer != null)
            {
                motherHaloRenderer.enabled = isMother;
                motherHaloRenderer.color = new Color(1f, 0.75f, 0.2f, 0.55f);
                float haloPulse = MotherVisualScale * (1.9f + Mathf.Sin(Time.time * 3.1f + visualPhase) * 0.2f);
                motherHaloRenderer.transform.localScale = Vector3.one * haloPulse;
            }

            if (motherCoreRenderer != null)
            {
                motherCoreRenderer.enabled = isMother;
                motherCoreRenderer.color = new Color(1f, 0.92f, 0.28f, 0.92f);
                motherCoreRenderer.transform.localScale = Vector3.one * (MotherVisualScale * 0.46f);
            }

            if (selectionRingRenderer != null)
            {
                selectionRingRenderer.enabled = Selected;
                selectionRingRenderer.color = isMother ? new Color(1f, 0.94f, 0.18f, 0.95f) : new Color(1f, 0.86f, 0.2f, 0.86f);
                selectionRingRenderer.transform.localScale = Vector3.one * (isMother ? MotherVisualScale * 1.42f : 1.24f);
            }

            V5FunctionalCasteDefinition caste = V5CasteLibrary.Get(FunctionalCaste);
            if (casteHaloRenderer != null)
            {
                casteHaloRenderer.enabled = false;
                casteHaloRenderer.color = new Color(caste.primaryColor.r, caste.primaryColor.g, caste.primaryColor.b, IsAttachedToBody ? 0.56f : 0.40f);
                float castePulse = 1.08f + Mathf.Sin(Time.time * 2.7f + visualPhase) * 0.035f;
                casteHaloRenderer.transform.localScale = Vector3.one * (IsAttachedToBody ? 1.18f : castePulse);
            }

            if (roleMarkerRenderer != null)
            {
                roleMarkerRenderer.enabled = IsPlayerOwned && !isMother;
                roleMarkerRenderer.color = Color.Lerp(caste.primaryColor, V5CellModeLibrary.Get(CellMode).color, 0.35f);
                roleMarkerRenderer.transform.localScale = Vector3.one * (FunctionalCaste == V5FunctionalCasteId.Structural ? 0.28f : 0.22f);
            }
        }

        private void RefreshSwarmVisuals(Color baseColor, bool isMother)
        {
            int dotCount = (!isMother && V5RosterBalance.RepresentsMicrocolony(EvolutionPath)) ? Mathf.Clamp(V5RosterBalance.VisualOrganismMax(EvolutionPath) - 1, 1, 11) : 0;
            EnsureSwarmDotCount(dotCount);
            for (int i = 0; i < swarmDotRenderers.Count; i++)
            {
                SpriteRenderer dot = swarmDotRenderers[i];
                bool visible = i < dotCount;
                dot.enabled = visible;
                if (!visible) continue;

                float t = Time.time * 0.65f + visualPhase + i * 2.17f;
                float ring = 0.26f + (i % 4) * 0.055f;
                dot.transform.localPosition = new Vector3(Mathf.Cos(t) * ring, Mathf.Sin(t * 0.83f) * ring, 0f);
                dot.transform.localScale = Vector3.one * (0.16f + (i % 3) * 0.018f);
                dot.color = Color.Lerp(baseColor, Color.white, 0.22f);
                dot.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder + 1 : 11;
            }
        }

        private void EnsureSwarmDotCount(int count)
        {
            while (swarmDotRenderers.Count < count)
            {
                SpriteRenderer dot = CreateVisualChild("SwarmDot" + swarmDotRenderers.Count, circleSprite, 11);
                swarmDotRenderers.Add(dot);
            }
        }

        private string ShortName()
        {
            if (Role == V5CellRole.Mother) return "Madre";
            return Role + " G" + Generation;
        }
    }
}
