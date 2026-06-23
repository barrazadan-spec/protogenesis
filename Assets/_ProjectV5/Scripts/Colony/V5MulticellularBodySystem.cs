using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5MulticellularBodySystem : MonoBehaviour, IV5RunResettable
    {
        private const float AttachedContactOverlap = 0.88f;
        private const float AttachedFollowSpeed = 18f;

        private class BodySlot
        {
            public V5BodySlotDefinition definition;
            public V5CellEntity occupant;
            public float integrity01 = 1f;

            public BodySlot(V5BodySlotDefinition definition)
            {
                this.definition = definition;
            }
        }

        public V5BodyState CurrentState { get; private set; }
        public int OccupiedSlots { get; private set; }
        public int MaxSlots { get { EnsureSlots(); return slots.Count; } }
        public string LastMessage = "Cuerpo multicelular listo.";
        public string Summary { get; private set; }
        public float MovementMultiplier { get; private set; }
        public float PhysicalDamageReduction01 { get; private set; }
        public float AtpPerSecondFromBody { get; private set; }
        public float BiomassPerSecondFromBody { get; private set; }
        public float StressReductionPerSecond { get; private set; }
        public bool ChampionActive => championActive && Time.time < championUntil;
        public bool HuskDropOnCooldown => Time.time < lastHuskDropTime + 30f;
        public float ChampionTimeRemaining => championActive ? Mathf.Max(0f, championUntil - Time.time) : 0f;
        public bool AttachmentUnlocked
        {
            get
            {
                V5GameManager gm = V5GameManager.Instance;
                return HasAdhesiveBond(gm != null ? gm.MotherCell : null);
            }
        }

        private readonly List<BodySlot> slots = new List<BodySlot>(6);
        private float passiveTick;
        private float bodyFullSince = -1f;
        private float lastHuskDropTime = -999f;
        private bool championActive = false;
        private float championUntil = 0f;

        private void Update()
        {
            EnsureSlots();
            CleanupSlots();
            UpdateStateAndBuffs();
            UpdateAttachedCellPositions();
            ApplyPassiveSupport();
        }

        public void ResetForNewRun()
        {
            EnsureSlots();
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].occupant != null) slots[i].occupant.ClearBodyAttachment(false);
                slots[i].occupant = null;
                slots[i].integrity01 = 1f;
            }
            bodyFullSince = -1f;
            lastHuskDropTime = -999f;
            championActive = false;
            championUntil = 0f;
            LastMessage = "Cuerpo reiniciado: 0/" + slots.Count + " slots.";
            UpdateStateAndBuffs();
        }

        public bool TryAttach(V5CellEntity cell, V5BodySlotRole preferredRole)
        {
            EnsureSlots();
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null || cell == null) return false;
            if (!cell.IsPlayerOwned || cell.Role == V5CellRole.Mother || cell.Role == V5CellRole.Apex) return false;
            if (cell.IsAttachedToBody) return false;
            if (cell.AttachmentState == V5AttachmentState.Cooldown && Time.time < cell.AttachmentCooldownUntil) return false;
            if (!HasAdhesiveBond(mother))
            {
                LastMessage = AdhesionRequirementText();
                Toast();
                return false;
            }

            float maxDistance = 3.5f + mother.Stats.radius + cell.Stats.radius;
            if (Vector2.Distance(cell.transform.position, mother.transform.position) > maxDistance)
            {
                LastMessage = cell.PhenotypeLabel + " esta demasiado lejos para adherirse.";
                Toast();
                return false;
            }

            if (preferredRole == V5BodySlotRole.None) preferredRole = V5PhenotypeRecipeLibrary.RecommendedBodyRole(cell.PhenotypeCaste);
            int slotIndex = BestOpenSlot(preferredRole);
            if (slotIndex < 0)
            {
                LastMessage = "No hay slot corporal libre.";
                Toast();
                return false;
            }

            slots[slotIndex].occupant = cell;
            slots[slotIndex].integrity01 = 1f;
            cell.SetBodyAttachment(slotIndex);
            cell.Mother = mother;
            cell.SnapAttachedToBodySlot(SlotTargetPosition(mother, cell, slots[slotIndex].definition));
            mother.Stats.stress = Mathf.Min(100f, mother.Stats.stress + 1f);
            UpdateStateAndBuffs();
            LastMessage = "Adherida " + cell.PhenotypeLabel + " a slot " + slotIndex + " (" + slots[slotIndex].definition.preferredRole + ").";
            Toast();
            return true;
        }

        public bool CanUseBodyAttachment(V5CellEntity mother, out string reason)
        {
            if (HasAdhesiveBond(mother))
            {
                reason = "Adesina activa: el cuerpo puede recibir hijas.";
                return true;
            }
            reason = AdhesionRequirementText();
            return false;
        }

        public string AdhesionRequirementText()
        {
            return "Requiere Adesina basica: abre Genoma (G) y activa esa adaptacion antes de pegar hijas al cuerpo.";
        }

        private bool HasAdhesiveBond(V5CellEntity mother)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Adaptations != null)
            {
                return gm.Adaptations.Has(V5AdaptationId.BasicAdhesin) ||
                       gm.Adaptations.Has(V5AdaptationId.ColonialAdhesin) ||
                       gm.Adaptations.Has(V5AdaptationId.PersistentAdhesion);
            }
            if (gm != null && gm.Genes != null && gm.Genes.HasGene(V5GeneId.Adhesion)) return true;
            if (gm != null && gm.Genes != null) return false;
            if (mother == null) return false;
            return mother.HasStructure(V5StructureId.Fimbriae) ||
                   mother.HasStructure(V5StructureId.InvasiveHypha) ||
                   mother.HasStructure(V5StructureId.MucilageMatrix);
        }

        public bool Detach(V5CellEntity cell, bool voluntary)
        {
            if (cell == null || !cell.IsAttachedToBody) return false;
            int slotIndex = cell.BodySlotIndex;
            if (slotIndex >= 0 && slotIndex < slots.Count && slots[slotIndex].occupant == cell)
            {
                slots[slotIndex].occupant = null;
                slots[slotIndex].integrity01 = 1f;
            }

            V5GameManager gm = V5GameManager.Instance;
            if (voluntary && gm != null && gm.MotherCell != null) gm.MotherCell.Stats.stress = Mathf.Min(100f, gm.MotherCell.Stats.stress + 2f);
            Vector2 outward = Random.insideUnitCircle.normalized;
            if (gm != null && gm.MotherCell != null)
            {
                outward = ((Vector2)cell.transform.position - (Vector2)gm.MotherCell.transform.position).normalized;
                if (outward.sqrMagnitude < 0.01f) outward = Random.insideUnitCircle.normalized;
            }
            cell.ClearBodyAttachment(voluntary);
            cell.transform.position = (Vector2)cell.transform.position + outward * Mathf.Max(0.6f, cell.Stats.radius + 0.35f);
            UpdateStateAndBuffs();
            LastMessage = "Celula desprendida del cuerpo.";
            Toast();
            return true;
        }

        public void NotifyAttachedCellDied(V5CellEntity cell)
        {
            if (cell == null) return;
            int slotIndex = cell.BodySlotIndex;
            if (slotIndex >= 0 && slotIndex < slots.Count && slots[slotIndex].occupant == cell)
            {
                slots[slotIndex].occupant = null;
                slots[slotIndex].integrity01 = 0f;
            }

            V5BodyState prevState = CurrentState;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.MotherCell != null)
            {
                V5BodySlotRole role = V5PhenotypeRecipeLibrary.RecommendedBodyRole(cell.PhenotypeCaste);
                float stressPenalty = role == V5BodySlotRole.Connector ? 5f : 3f;
                gm.MotherCell.Stats.stress = Mathf.Min(100f, gm.MotherCell.Stats.stress + stressPenalty);
            }
            UpdateStateAndBuffs();
            LastMessage = prevState == V5BodyState.Complete && CurrentState == V5BodyState.Partial
                ? "Cuerpo fracturado."
                : "Slot corporal perdido.";
            Toast();
            if (CurrentState == V5BodyState.Exposed) CheckChampionActivation();
        }

        public bool TryHuskDrop()
        {
            EnsureSlots();
            if (OccupiedSlots == 0)
            {
                LastMessage = "No hay celulas adheridas para soltar.";
                Toast();
                return false;
            }
            for (int i = slots.Count - 1; i >= 0; i--)
            {
                if (slots[i].occupant != null) Detach(slots[i].occupant, true);
            }
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.MotherCell != null)
                gm.MotherCell.Stats.stress = Mathf.Min(100f, gm.MotherCell.Stats.stress + 12f);
            lastHuskDropTime = Time.time;
            bodyFullSince = -1f;
            LastMessage = "Husk Drop: cuerpo liberado. Champion bloqueado 30s.";
            Toast();
            return true;
        }

        public bool HasOpenSlot(V5BodySlotRole preferredRole)
        {
            EnsureSlots();
            return BestOpenSlot(preferredRole) >= 0;
        }

        public int BestOpenSlotIndex(V5BodySlotRole preferredRole)
        {
            EnsureSlots();
            return BestOpenSlot(preferredRole);
        }

        public int OpenSlotCount()
        {
            EnsureSlots();
            int count = 0;
            for (int i = 0; i < slots.Count; i++)
                if (slots[i].occupant == null) count++;
            return count;
        }

        public V5CellEntity GetSlotOccupant(int index)
        {
            EnsureSlots();
            if (index < 0 || index >= slots.Count) return null;
            return slots[index].occupant;
        }

        public V5BodySlotDefinition GetSlotDefinition(int index)
        {
            EnsureSlots();
            if (index < 0 || index >= slots.Count) return new V5BodySlotDefinition(-1, V5BodyRing.Inner, 0f, 0f, V5BodySlotRole.None);
            return slots[index].definition;
        }

        public string SlotLabel(int index)
        {
            V5CellEntity cell = GetSlotOccupant(index);
            if (cell == null) return "--";
            return ShortRole(V5PhenotypeRecipeLibrary.RecommendedBodyRole(cell.PhenotypeCaste)) + ":" + cell.PhenotypeRecipeCode;
        }

        public float AttachedTargetDistance(V5CellEntity cell)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null || cell == null) return 0f;
            int slotIndex = cell.BodySlotIndex;
            V5BodySlotDefinition def = slotIndex >= 0 && slotIndex < slots.Count
                ? slots[slotIndex].definition
                : new V5BodySlotDefinition(-1, V5BodyRing.Inner, 0f, 0f, V5BodySlotRole.None);
            return SlotOrbitDistance(mother, cell, def);
        }

        public string BodyStateLabel()
        {
            switch (CurrentState)
            {
                case V5BodyState.Exposed: return "Nucleo expuesto";
                case V5BodyState.Partial: return "Cuerpo parcial";
                case V5BodyState.Complete: return "Cuerpo completo";
                case V5BodyState.Overloaded: return "Cuerpo sobrecargado";
                default: return "Sin cuerpo";
            }
        }

        public bool TryAbsorbDamage(V5CellEntity nominalTarget, float amount, V5DamageKind kind, Vector2 source, out float leakedToMother)
        {
            leakedToMother = amount;
            EnsureSlots();
            if (ChampionActive && (CurrentState == V5BodyState.Exposed || OccupiedSlots == 0))
            {
                leakedToMother = amount * 0.45f;
                return true;
            }
            if (CurrentState == V5BodyState.Exposed || OccupiedSlots == 0) return false;

            float leakPct = CurrentState == V5BodyState.Partial ? 0.12f : 0.06f;
            if (kind == V5DamageKind.Piercing) leakPct = Mathf.Min(1f, leakPct + 0.10f);
            bool isChemical = kind == V5DamageKind.Chemical || kind == V5DamageKind.Oxidative || kind == V5DamageKind.Acid;
            if (isChemical) leakPct *= 0.5f;

            V5CellEntity absorber = PickAbsorberSlot(source);
            if (absorber == null) return false;

            leakedToMother = amount * leakPct;
            absorber.Damage(amount * (1f - leakPct), kind, source);
            return true;
        }

        private V5CellEntity PickAbsorberSlot(Vector2 source)
        {
            EnsureSlots();
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return null;

            Vector2 motherPos = mother.transform.position;
            Vector2 dir = source - motherPos;
            Vector2 toAttacker = dir.sqrMagnitude > 0.01f ? dir.normalized : Vector2.right;

            V5CellEntity best = null;
            float bestDot = -2f;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].occupant == null) continue;
                float rad = slots[i].definition.angleDegrees * Mathf.Deg2Rad;
                Vector2 slotDir = new Vector2(Mathf.Cos(rad), Mathf.Sin(rad));
                float dot = Vector2.Dot(slotDir, toAttacker);
                if (dot > bestDot) { bestDot = dot; best = slots[i].occupant; }
            }
            return best;
        }

        private void EnsureSlots()
        {
            if (slots.Count > 0) return;
            AddSlot(0, 90f, V5BodySlotRole.Connector);
            AddSlot(1, 30f, V5BodySlotRole.Armor);
            AddSlot(2, -30f, V5BodySlotRole.Mouth);
            AddSlot(3, -90f, V5BodySlotRole.Producer);
            AddSlot(4, -150f, V5BodySlotRole.Armor);
            AddSlot(5, 150f, V5BodySlotRole.Motor);
        }

        private void AddSlot(int index, float angle, V5BodySlotRole role)
        {
            slots.Add(new BodySlot(new V5BodySlotDefinition(index, V5BodyRing.Inner, angle, 0f, role)));
        }

        private int BestOpenSlot(V5BodySlotRole preferredRole)
        {
            int fallback = -1;
            for (int i = 0; i < slots.Count; i++)
            {
                if (slots[i].occupant != null) continue;
                if (fallback < 0) fallback = i;
                if (slots[i].definition.preferredRole == preferredRole) return i;
            }
            return fallback;
        }

        private void CleanupSlots()
        {
            for (int i = 0; i < slots.Count; i++)
            {
                V5CellEntity cell = slots[i].occupant;
                if (cell == null)
                {
                    slots[i].occupant = null;
                    continue;
                }

                if (!cell.IsAttachedToBody || cell.BodySlotIndex != i)
                {
                    slots[i].occupant = null;
                }
            }
        }

        private void UpdateStateAndBuffs()
        {
            OccupiedSlots = 0;
            int armor = 0;
            int motor = 0;
            int producer = 0;
            int connector = 0;
            for (int i = 0; i < slots.Count; i++)
            {
                V5CellEntity cell = slots[i].occupant;
                if (cell == null) continue;
                OccupiedSlots++;
                V5BodySlotRole role = V5PhenotypeRecipeLibrary.RecommendedBodyRole(cell.PhenotypeCaste);
                if (role == V5BodySlotRole.Armor) armor++;
                else if (role == V5BodySlotRole.Motor || role == V5BodySlotRole.Sensor) motor++;
                else if (role == V5BodySlotRole.Producer) producer++;
                else if (role == V5BodySlotRole.Connector || role == V5BodySlotRole.Reserve) connector++;
            }

            if (OccupiedSlots <= 2) CurrentState = V5BodyState.Exposed;
            else if (OccupiedSlots <= 5) CurrentState = V5BodyState.Partial;
            else if (OccupiedSlots <= 12) CurrentState = V5BodyState.Complete;
            else CurrentState = V5BodyState.Overloaded;

            float stateMove = 1.15f;
            if (CurrentState == V5BodyState.Partial) stateMove = 0.95f;
            else if (CurrentState == V5BodyState.Complete) stateMove = 0.78f;
            else if (CurrentState == V5BodyState.Overloaded) stateMove = 0.62f;

            MovementMultiplier = Mathf.Clamp(stateMove + motor * 0.07f - Mathf.Max(0, OccupiedSlots - 6) * 0.025f, 0.5f, 1.25f);
            PhysicalDamageReduction01 = Mathf.Min(0.30f, armor * 0.08f);
            AtpPerSecondFromBody = producer * 0.18f;
            BiomassPerSecondFromBody = producer * 0.10f;
            StressReductionPerSecond = connector * 0.12f;
            if (OccupiedSlots >= 6 && bodyFullSince < 0f) bodyFullSince = Time.time;
            else if (OccupiedSlots < 6) bodyFullSince = -1f;

            if (ChampionActive)
            {
                MovementMultiplier = Mathf.Min(1.60f, MovementMultiplier + 0.35f);
                PhysicalDamageReduction01 = Mathf.Min(0.50f, PhysicalDamageReduction01 + 0.25f);
            }

            string champPrefix = ChampionActive ? "[CAMPEON " + ChampionTimeRemaining.ToString("0") + "s] " : "";
            Summary = champPrefix + BodyStateLabel() + " | slots " + OccupiedSlots + "/" + slots.Count + " | mov x" + MovementMultiplier.ToString("0.00") + " | coraza " + (PhysicalDamageReduction01 * 100f).ToString("0") + "%";
        }

        private void UpdateAttachedCellPositions()
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;

            for (int i = 0; i < slots.Count; i++)
            {
                V5CellEntity cell = slots[i].occupant;
                if (cell == null) continue;
                V5BodySlotDefinition def = slots[i].definition;
                Vector2 target = SlotTargetPosition(mother, cell, def);
                cell.MoveAttachedToBodySlot(target, AttachedFollowSpeed);
            }
        }

        private Vector2 SlotTargetPosition(V5CellEntity mother, V5CellEntity cell, V5BodySlotDefinition def)
        {
            float radians = def.angleDegrees * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            return (Vector2)mother.transform.position + dir * SlotOrbitDistance(mother, cell, def);
        }

        private float SlotOrbitDistance(V5CellEntity mother, V5CellEntity cell, V5BodySlotDefinition def)
        {
            float contactDistance = mother.Stats.radius + cell.Stats.radius;
            return Mathf.Max(0.18f, contactDistance * AttachedContactOverlap + Mathf.Max(0f, def.radius));
        }

        private void ApplyPassiveSupport()
        {
            passiveTick += Time.deltaTime;
            if (passiveTick < 0.5f) return;
            float dt = passiveTick;
            passiveTick = 0f;

            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother != null)
            {
                mother.Resources.atp += AtpPerSecondFromBody * dt;
                mother.Resources.biomass += BiomassPerSecondFromBody * dt;
                mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - StressReductionPerSecond * dt);
                if (CurrentState == V5BodyState.Exposed && OccupiedSlots <= 2)
                    mother.Stats.stress = Mathf.Min(100f, mother.Stats.stress + 0.18f * dt);
                if (ChampionActive)
                {
                    mother.Stats.currentHp = Mathf.Min(mother.Stats.maxHp, mother.Stats.currentHp + 8f * dt);
                    mother.Stats.stress = Mathf.Max(0f, mother.Stats.stress - 2f * dt);
                }
            }
            if (championActive && Time.time >= championUntil)
            {
                championActive = false;
                LastMessage = "Champion expirado. Reconstruye el cuerpo.";
                Toast();
            }
        }

        private void CheckChampionActivation()
        {
            if (championActive) return;
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return;
            if (Time.time < lastHuskDropTime + 30f) return;
            if (bodyFullSince < 0f || Time.time - bodyFullSince < 45f) return;
            if (mother.Stats.currentHp < mother.Stats.maxHp * 0.20f) return;
            if (mother.Stats.stress >= 95f) return;

            championActive = true;
            championUntil = Time.time + 25f;
            LastMessage = "MADRE CAMPEON: emergencia activada por 25s.";
            Toast();
            if (gm != null && gm.Codex != null) gm.Codex.Unlock("Madre Campeon", "La madre sobrevivio el colapso del cuerpo y entro en modo emergencia.");
        }

        private string ShortRole(V5BodySlotRole role)
        {
            switch (role)
            {
                case V5BodySlotRole.Armor: return "AR";
                case V5BodySlotRole.Motor: return "MO";
                case V5BodySlotRole.Producer: return "PR";
                case V5BodySlotRole.Mouth: return "BO";
                case V5BodySlotRole.Connector: return "CO";
                case V5BodySlotRole.Sensor: return "SE";
                case V5BodySlotRole.Reserve: return "RS";
                default: return "??";
            }
        }

        private void Toast()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
        }
    }
}
