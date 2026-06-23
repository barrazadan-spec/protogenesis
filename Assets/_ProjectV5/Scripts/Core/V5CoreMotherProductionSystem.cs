using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CoreMotherProductionSystem : MonoBehaviour
    {
        [Header("Core Mother Production")]
        public float MotherProductionInterval = 3.0f;
        public float MotherProductionIntervalMultiplier = 1.0f;
        public KeyCode ForceDivisionKey = KeyCode.D;
        public KeyCode DebugBatchKey = KeyCode.P;
        public int DebugBatchSize = 20;
        public string LastMessage = "Produccion madre lista.";

        [Header("Forced Division")]
        public float ForceDivisionBaseCostMultiplier = 1.35f;
        public float ForceDivisionHeatCostMultiplier = 0.72f;
        public float ForceDivisionHeatPerUse = 1f;
        public float ForceDivisionHeatDecayPerSecond = 0.18f;

        [Header("Core DNA Upgrades")]
        public int DnaPoints;
        public int SynthesisAccelerationLevel;
        public int BiofilmProtectorLevel;
        public int ColonialCapacityLevel;
        public int MaxSynthesisAccelerationLevel = 5;
        public int MaxBiofilmProtectorLevel = 3;
        public int MaxColonialCapacityLevel = 4;
        public float SynthesisIntervalMultiplierPerLevel = 0.82f;
        public int ColonialCapacityFreeCellBonusPerLevel = 15;
        public float BiofilmBaseRadius = 8f;
        public float BiofilmRadiusPerLevel = 2.5f;
        public float BiofilmSlowPerLevel = 0.16f;

        private float productionTimer;
        private float forceDivisionHeat;
        private SpriteRenderer biofilmAura;

        public float EffectiveMotherProductionInterval
        {
            get
            {
                float synth = Mathf.Pow(Mathf.Clamp(SynthesisIntervalMultiplierPerLevel, 0.45f, 0.98f), Mathf.Max(0, SynthesisAccelerationLevel));
                return Mathf.Max(0.25f, MotherProductionInterval * Mathf.Max(0.05f, MotherProductionIntervalMultiplier) * synth);
            }
        }

        public int FreeCellCapBonus { get { return Mathf.Max(0, ColonialCapacityLevel) * Mathf.Max(0, ColonialCapacityFreeCellBonusPerLevel); } }
        public float BiofilmRadius { get { return BiofilmProtectorLevel <= 0 ? 0f : BiofilmBaseRadius + BiofilmRadiusPerLevel * (BiofilmProtectorLevel - 1); } }

        public float ProductionProgress01
        {
            get { return Mathf.Clamp01(productionTimer / EffectiveMotherProductionInterval); }
        }

        public float SecondsUntilNext
        {
            get { return Mathf.Max(0f, EffectiveMotherProductionInterval - productionTimer); }
        }

        public float ForceDivisionHeat { get { return forceDivisionHeat; } }

        public float CurrentForceDivisionCost
        {
            get
            {
                V5GameManager gm = V5GameManager.Instance;
                return gm != null && gm.MotherCell != null ? ForceDivisionCost(gm.MotherCell) : 0f;
            }
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || gm.MotherCell == null) return;

            DecayForceDivisionHeat();
            if (Input.GetKeyDown(ForceDivisionKey)) TryForceDivision(gm);
            if (Input.GetKeyDown(DebugBatchKey)) SpawnDebugBatch(gm);
            UpdateBiofilmAura(gm);

            productionTimer += Time.deltaTime;
            if (productionTimer < EffectiveMotherProductionInterval) return;

            if (TryProduceOne(gm, false))
                productionTimer = 0f;
            else
                productionTimer = EffectiveMotherProductionInterval;
        }

        public void ResetForNewRun()
        {
            productionTimer = 0f;
            DnaPoints = 0;
            SynthesisAccelerationLevel = 0;
            BiofilmProtectorLevel = 0;
            ColonialCapacityLevel = 0;
            forceDivisionHeat = 0f;
            LastMessage = "Produccion madre lista.";
            if (biofilmAura != null) biofilmAura.enabled = false;
        }

        public void AddDna(int amount, Vector2 world, string reason)
        {
            int dna = Mathf.Max(0, amount);
            if (dna <= 0) return;
            DnaPoints += dna;
            LastMessage = reason + ": +" + dna + " ADN.";
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push(LastMessage, world, new Color(0.78f, 0.95f, 1f, 1f));
        }

        public int UpgradeCost(V5MotherUpgradeId id)
        {
            switch (id)
            {
                case V5MotherUpgradeId.SynthesisAcceleration: return 8 + SynthesisAccelerationLevel * 6 + SynthesisAccelerationLevel * SynthesisAccelerationLevel * 2;
                case V5MotherUpgradeId.BiofilmProtector: return 10 + BiofilmProtectorLevel * 9;
                case V5MotherUpgradeId.ColonialCapacity: return 12 + ColonialCapacityLevel * 8;
                default: return 999;
            }
        }

        public int UpgradeLevel(V5MotherUpgradeId id)
        {
            switch (id)
            {
                case V5MotherUpgradeId.SynthesisAcceleration: return SynthesisAccelerationLevel;
                case V5MotherUpgradeId.BiofilmProtector: return BiofilmProtectorLevel;
                case V5MotherUpgradeId.ColonialCapacity: return ColonialCapacityLevel;
                default: return 0;
            }
        }

        public int UpgradeMaxLevel(V5MotherUpgradeId id)
        {
            switch (id)
            {
                case V5MotherUpgradeId.SynthesisAcceleration: return Mathf.Max(1, MaxSynthesisAccelerationLevel);
                case V5MotherUpgradeId.BiofilmProtector: return Mathf.Max(1, MaxBiofilmProtectorLevel);
                case V5MotherUpgradeId.ColonialCapacity: return Mathf.Max(1, MaxColonialCapacityLevel);
                default: return 1;
            }
        }

        public bool CanBuyUpgrade(V5MotherUpgradeId id, out string reason)
        {
            int level = UpgradeLevel(id);
            int max = UpgradeMaxLevel(id);
            if (level >= max)
            {
                reason = "max";
                return false;
            }

            int cost = UpgradeCost(id);
            if (DnaPoints < cost)
            {
                reason = "falta " + (cost - DnaPoints) + " ADN";
                return false;
            }

            reason = "comprar";
            return true;
        }

        public bool BuyUpgrade(V5MotherUpgradeId id)
        {
            string reason;
            if (!CanBuyUpgrade(id, out reason))
            {
                LastMessage = UpgradeName(id) + ": " + reason + ".";
                return false;
            }

            int cost = UpgradeCost(id);
            DnaPoints -= cost;
            switch (id)
            {
                case V5MotherUpgradeId.SynthesisAcceleration: SynthesisAccelerationLevel++; break;
                case V5MotherUpgradeId.BiofilmProtector: BiofilmProtectorLevel++; break;
                case V5MotherUpgradeId.ColonialCapacity: ColonialCapacityLevel++; break;
            }

            LastMessage = "Mejora comprada: " + UpgradeName(id) + " nivel " + UpgradeLevel(id) + ".";
            V5GameManager gm = V5GameManager.Instance;
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null && gm != null && gm.MotherCell != null)
                feedback.Push(LastMessage, gm.MotherCell.transform.position, new Color(0.78f, 0.95f, 1f, 1f));
            return true;
        }

        public float EnemySpeedMultiplierAt(Vector2 world)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || gm.MotherCell == null || BiofilmProtectorLevel <= 0) return 1f;
            float radius = Mathf.Max(0.1f, BiofilmRadius);
            float distance = Vector2.Distance(world, gm.MotherCell.transform.position);
            if (distance >= radius) return 1f;
            float strength = 1f - Mathf.Clamp01(distance / radius);
            float slow = Mathf.Clamp01(BiofilmSlowPerLevel * BiofilmProtectorLevel);
            return Mathf.Clamp(1f - slow * strength, 0.38f, 1f);
        }

        public string UpgradeName(V5MotherUpgradeId id)
        {
            switch (id)
            {
                case V5MotherUpgradeId.SynthesisAcceleration: return "Sintesis acelerada";
                case V5MotherUpgradeId.BiofilmProtector: return "Biofilm protector";
                case V5MotherUpgradeId.ColonialCapacity: return "Capacidad colonial";
                default: return "Mejora";
            }
        }

        public int SeedStartingCells(V5GameManager gm, int count)
        {
            if (gm == null || gm.MotherCell == null) return 0;
            int made = 0;
            for (int i = 0; i < count; i++)
            {
                V5CellEntity child = SpawnMotherChild(gm, gm.MotherCell, i, count);
                if (child != null) made++;
            }
            LastMessage = "Inicio core: madre + " + made + " celulas.";
            return made;
        }

        public bool TryProduceOne(V5GameManager gm, bool ignoreCost)
        {
            if (gm == null || gm.MotherCell == null || gm.CellFactory == null) return false;
            V5CellEntity mother = gm.MotherCell;
            if (!gm.CanAddPlayerCellFrom(mother))
            {
                LastMessage = gm.PlayerTotalCellCount() >= V5Balance.CoreTotalPlayerCellHardCap
                    ? "Produccion pausada: cap total."
                    : "Produccion pausada: cap de libres.";
                return false;
            }

            float cost = V5Balance.DivisionCostBiomass(mother);
            if (!ignoreCost && mother.Resources.biomass < cost)
            {
                LastMessage = "Produccion esperando biomasa: falta " + Mathf.Ceil(cost - mother.Resources.biomass).ToString("0") + ".";
                return false;
            }

            if (!ignoreCost) mother.Resources.biomass -= cost;
            V5CellEntity child = SpawnMotherChild(gm, mother, gm.PlayerTotalCellCount(), Mathf.Max(1, gm.PlayerTotalCellCount() + 1));
            if (child == null)
            {
                LastMessage = "Produccion fallo al crear celula.";
                return false;
            }

            LastMessage = ignoreCost ? "Debug P: celula creada gratis." : "Madre produjo celula: -" + cost.ToString("0") + " biomasa.";
            return true;
        }

        public float ForceDivisionCost(V5CellEntity mother)
        {
            if (mother == null) return 0f;
            float baseCost = V5Balance.DivisionCostBiomass(mother);
            float multiplier = Mathf.Max(0.1f, ForceDivisionBaseCostMultiplier) + Mathf.Max(0f, forceDivisionHeat) * Mathf.Max(0f, ForceDivisionHeatCostMultiplier);
            return Mathf.Ceil(baseCost * multiplier);
        }

        public bool CanForceDivision(V5GameManager gm, out string reason)
        {
            reason = "lista";
            if (gm == null || gm.MotherCell == null || gm.CellFactory == null)
            {
                reason = "sin madre";
                return false;
            }

            if (!gm.CanAddPlayerCellFrom(gm.MotherCell))
            {
                reason = gm.PlayerTotalCellCount() >= V5Balance.CoreTotalPlayerCellHardCap ? "cap total" : "cap libres";
                return false;
            }

            float cost = ForceDivisionCost(gm.MotherCell);
            if (gm.MotherCell.Resources.biomass < cost)
            {
                reason = "falta " + Mathf.Ceil(cost - gm.MotherCell.Resources.biomass).ToString("0") + " bio";
                return false;
            }

            return true;
        }

        public bool TryForceDivision(V5GameManager gm)
        {
            string reason;
            if (!CanForceDivision(gm, out reason))
            {
                LastMessage = "Forzar division: " + reason + ".";
                V5FeedbackSystem blockedFeedback = FindFirstObjectByType<V5FeedbackSystem>();
                if (blockedFeedback != null && gm != null && gm.MotherCell != null)
                    blockedFeedback.Push(LastMessage, gm.MotherCell.transform.position, new Color(1f, 0.76f, 0.46f, 1f));
                return false;
            }

            V5CellEntity mother = gm.MotherCell;
            float cost = ForceDivisionCost(mother);
            mother.Resources.biomass = Mathf.Max(0f, mother.Resources.biomass - cost);
            V5CellEntity child = SpawnMotherChild(gm, mother, gm.PlayerTotalCellCount(), Mathf.Max(1, gm.PlayerTotalCellCount() + 1));
            if (child == null)
            {
                mother.Resources.biomass += cost;
                LastMessage = "Forzar division fallo al crear celula.";
                return false;
            }

            forceDivisionHeat += Mathf.Max(0.01f, ForceDivisionHeatPerUse);
            LastMessage = "Forzar division: +" + 1 + " celula, -" + cost.ToString("0") + " biomasa. Prox " + ForceDivisionCost(mother).ToString("0") + ".";
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null)
                feedback.Push(LastMessage, mother.transform.position, new Color(0.58f, 1f, 0.72f, 1f));
            return true;
        }

        private void DecayForceDivisionHeat()
        {
            if (forceDivisionHeat <= 0f) return;
            forceDivisionHeat = Mathf.Max(0f, forceDivisionHeat - Mathf.Max(0f, ForceDivisionHeatDecayPerSecond) * Time.deltaTime);
        }

        private void SpawnDebugBatch(V5GameManager gm)
        {
            int made = 0;
            for (int i = 0; i < DebugBatchSize; i++)
            {
                if (!gm.CanAddPlayerCellFrom(gm.MotherCell)) break;
                if (!TryProduceOne(gm, true)) break;
                made++;
            }
            LastMessage = "Debug P: +" + made + " celulas.";
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null && gm.MotherCell != null)
                feedback.Push(LastMessage, gm.MotherCell.transform.position, new Color(0.72f, 1f, 0.72f, 1f));
        }

        public int SpawnRewardBatch(V5GameManager gm, int count, Vector2 center, string reason)
        {
            if (gm == null || gm.MotherCell == null || gm.CellFactory == null) return 0;
            int made = 0;
            for (int i = 0; i < count; i++)
            {
                if (!gm.CanAddPlayerCellFrom(gm.MotherCell)) break;
                V5CellEntity child = SpawnMotherChildAt(gm, gm.MotherCell, center, i, count);
                if (child == null) break;
                made++;
            }

            LastMessage = reason + ": +" + made + " celulas.";
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push(LastMessage, center, new Color(0.72f, 1f, 0.72f, 1f));
            return made;
        }

        private V5CellEntity SpawnMotherChild(V5GameManager gm, V5CellEntity mother, int index, int total)
        {
            return SpawnMotherChildAt(gm, mother, mother != null ? (Vector2)mother.transform.position : Vector2.zero, index, total);
        }

        private V5CellEntity SpawnMotherChildAt(V5GameManager gm, V5CellEntity mother, Vector2 center, int index, int total)
        {
            if (gm == null || gm.CellFactory == null || mother == null) return null;
            float angle = index * 2.3999632f;
            float ring = 1.35f + Mathf.Sqrt(Mathf.Max(0, index)) * 0.16f;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * ring;
            V5CellEntity child = gm.CellFactory.SpawnPlayerCell(center + offset, V5CellRole.Daughter, mother);
            if (child != null)
            {
                child.Directive = V5Directive.FollowMother;
                child.DirectiveTarget = mother.transform.position;
                child.ApplyCellMode(V5CellModeId.FollowLineage);
            }
            return child;
        }

        private void UpdateBiofilmAura(V5GameManager gm)
        {
            if (gm == null || gm.MotherCell == null) return;
            if (biofilmAura == null)
            {
                GameObject go = new GameObject("V5_CoreMother_BiofilmAura");
                go.transform.SetParent(transform, false);
                biofilmAura = go.AddComponent<SpriteRenderer>();
                biofilmAura.sprite = V5ProceduralSprites.CreateRingSprite(160, 0.055f);
                biofilmAura.sortingOrder = 2;
            }

            bool active = BiofilmProtectorLevel > 0;
            biofilmAura.enabled = active;
            if (!active) return;

            biofilmAura.transform.position = gm.MotherCell.transform.position;
            float diameter = BiofilmRadius * 2f;
            biofilmAura.transform.localScale = Vector3.one * diameter;
            float pulse = 0.5f + Mathf.Sin(Time.time * 2.2f) * 0.5f;
            biofilmAura.color = new Color(0.32f, 1f, 0.62f, 0.22f + pulse * 0.08f);
        }
    }

    public enum V5MotherUpgradeId
    {
        SynthesisAcceleration,
        BiofilmProtector,
        ColonialCapacity
    }
}
