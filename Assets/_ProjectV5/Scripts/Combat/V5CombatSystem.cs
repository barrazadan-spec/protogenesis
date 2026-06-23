using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CombatSystem : MonoBehaviour
    {
        public bool ShowCombatNumbers = true;
        private float timer;
        private float nextCombatFeedback;

        private void Update()
        {
            timer += Time.deltaTime;
            if (timer < V5Balance.CombatTick) return;
            timer = 0f;
            TickCombat();
        }

        private void TickCombat()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity a = gm.PlayerCells[i];
                if (a == null) continue;
                for (int j = 0; j < gm.NonPlayerCells.Count; j++)
                {
                    V5CellEntity b = gm.NonPlayerCells[j];
                    if (b == null) continue;
                    ResolvePair(a, b);
                }
            }
        }

        private void ResolvePair(V5CellEntity a, V5CellEntity b)
        {
            float dist = Vector2.Distance(a.transform.position, b.transform.position);
            float contact = a.Stats.radius + b.Stats.radius + 0.15f;
            if (dist <= contact)
            {
                ResolveContact(a, b);
            }
            if (a.Stats.chemicalDamagePerSecond > 0f && dist < 3f + a.Stats.radius) ApplyDamage(a, b, a.Stats.chemicalDamagePerSecond * AttackDamageMultiplier(a) * V5Balance.CombatTick * (1f - dist / (3f + a.Stats.radius)), V5DamageKind.Chemical, "tox");
            if (b.Stats.chemicalDamagePerSecond > 0f && dist < 3f + b.Stats.radius) ApplyDamage(b, a, b.Stats.chemicalDamagePerSecond * AttackDamageMultiplier(b) * V5Balance.CombatTick * (1f - dist / (3f + b.Stats.radius)), V5DamageKind.Chemical, "tox");
            ResolveRouteSpecials(a, b, dist);
        }

        private void ResolveContact(V5CellEntity a, V5CellEntity b)
        {
            float aCoreSwarm = CoreFreeCellOverwhelmMultiplier(a, b);
            float bCoreSwarm = CoreFreeCellOverwhelmMultiplier(b, a);
            float aCoreBite = CoreEnemyOrganismBitePressureMultiplier(a, b);
            float bCoreBite = CoreEnemyOrganismBitePressureMultiplier(b, a);
            if (a.HasPhagocytosis && a.Stats.radius > b.Stats.radius * 1.35f)
            {
                ApplyDamage(a, b, 8f * V5Balance.CombatTick * a.Stats.physicalDamagePerSecond * AttackDamageMultiplier(a) * aCoreSwarm * aCoreBite, V5DamageKind.Physical, "digest");
                if (b.Stats.currentHp <= 0f)
                {
                    a.Resources.biomass += 12f;
                    PushFloating("absorb +12 bio", b.transform.position, new Color(0.8f, 1f, 0.6f, 1f));
                }
            }
            else ApplyDamage(a, b, a.Stats.physicalDamagePerSecond * AttackDamageMultiplier(a) * V5Balance.CombatTick * aCoreSwarm * aCoreBite, V5DamageKind.Physical, aCoreSwarm > 1f ? "swarm" : "hit");

            if (b.HasPhagocytosis && b.Stats.radius > a.Stats.radius * 1.35f)
            {
                ApplyDamage(b, a, 8f * V5Balance.CombatTick * b.Stats.physicalDamagePerSecond * AttackDamageMultiplier(b) * bCoreSwarm * bCoreBite, V5DamageKind.Physical, "digest");
                if (a.Stats.currentHp <= 0f)
                {
                    b.Resources.biomass += 12f;
                    PushFloating("absorb +12 bio", a.transform.position, new Color(0.8f, 1f, 0.6f, 1f));
                }
            }
            else ApplyDamage(b, a, b.Stats.physicalDamagePerSecond * AttackDamageMultiplier(b) * V5Balance.CombatTick * bCoreSwarm * bCoreBite, V5DamageKind.Physical, bCoreSwarm > 1f ? "swarm" : "hit");
        }

        private float CoreFreeCellOverwhelmMultiplier(V5CellEntity attacker, V5CellEntity target)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || attacker == null || target == null) return 1f;
            if (!attacker.IsPlayerOwned || target.IsPlayerOwned) return 1f;
            if (attacker.Role == V5CellRole.Mother || attacker.IsAttachedToBody || attacker.IsMorphedOrganism) return 1f;

            int contacters = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (cell == null || !cell.IsPlayerOwned || cell.Role == V5CellRole.Mother || cell.IsAttachedToBody || cell.IsMorphedOrganism) continue;
                float contact = cell.Stats.radius + target.Stats.radius + 0.45f;
                if (Vector2.SqrMagnitude((Vector2)cell.transform.position - (Vector2)target.transform.position) <= contact * contact) contacters++;
            }
            if (contacters < 6) return 1f + Mathf.Max(0, contacters - 1) * 0.10f;
            return Mathf.Min(3.25f, 1.45f + (contacters - 6) * 0.24f);
        }

        private float CoreEnemyOrganismBitePressureMultiplier(V5CellEntity attacker, V5CellEntity target)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || attacker == null || target == null) return 1f;
            if (attacker.IsPlayerOwned || !target.IsPlayerOwned) return 1f;
            if (!attacker.IsMorphedOrganism) return 1f;
            if (target.Role == V5CellRole.Mother || target.IsAttachedToBody || target.IsMorphedOrganism) return 1f;

            int contacters = 0;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (cell == null || !cell.IsPlayerOwned || cell.Role == V5CellRole.Mother || cell.IsAttachedToBody || cell.IsMorphedOrganism) continue;
                float contact = cell.Stats.radius + attacker.Stats.radius + 0.65f;
                if (Vector2.SqrMagnitude((Vector2)cell.transform.position - (Vector2)attacker.transform.position) <= contact * contact) contacters++;
            }

            if (contacters <= 1) return 1f;
            return Mathf.Clamp(1f / Mathf.Sqrt(contacters * 0.58f), 0.34f, 1f);
        }

        private float AttackDamageMultiplier(V5CellEntity attacker)
        {
            float mult = 1f;
            if (attacker != null) mult *= attacker.ModeDamageMultiplier();
            if (attacker != null && attacker.IsPlayerOwned && attacker.Role == V5CellRole.Mother) mult *= V5Balance.MotherCombatDamageMultiplier;
            if (attacker != null && attacker.IsPlayerOwned && V5GameManager.Instance != null && V5GameManager.Instance.LineageUpgrades != null)
                mult *= V5GameManager.Instance.LineageUpgrades.DamageMultiplier(attacker);
            return mult;
        }

        private void ResolveRouteSpecials(V5CellEntity a, V5CellEntity b, float dist)
        {
            ApplyStylet(a, b, dist);
            ApplyStylet(b, a, dist);
            ApplyCorona(a, b, dist);
            ApplyCorona(b, a, dist);
            ApplyMucilageOrHypha(a, b, dist);
            ApplyMucilageOrHypha(b, a, dist);
            ApplyDigestivePseudopods(a, b, dist);
            ApplyDigestivePseudopods(b, a, dist);
        }

        private void ApplyStylet(V5CellEntity attacker, V5CellEntity target, float dist)
        {
            if (attacker == null || target == null || !attacker.HasPiercingStylet) return;
            float reach = attacker.Stats.attackRange + attacker.Stats.radius + 0.25f;
            if (dist > reach) return;
            float damage = (1.6f + attacker.Stats.physicalDamagePerSecond * 0.55f) * AttackDamageMultiplier(attacker) * V5Balance.CombatTick;
            if (target.HasBiofilm || HasNetworkBody(target)) damage *= 1.45f;
            ApplyDamage(attacker, target, damage, V5DamageKind.Piercing, "pierce");
        }

        private void ApplyCorona(V5CellEntity attacker, V5CellEntity target, float dist)
        {
            if (attacker == null || target == null || !HasCiliaryFilter(attacker)) return;
            if (dist > 2.2f + attacker.Stats.radius) return;
            float smallTargetBonus = target.Stats.radius < attacker.Stats.radius * 0.85f ? 1.55f : 0.75f;
            float damage = (0.75f + attacker.Stats.physicalDamagePerSecond * 0.35f) * smallTargetBonus * AttackDamageMultiplier(attacker) * V5Balance.CombatTick;
            ApplyDamage(attacker, target, damage, V5DamageKind.Physical, "filter");
        }

        private void ApplyMucilageOrHypha(V5CellEntity attacker, V5CellEntity target, float dist)
        {
            if (attacker == null || target == null) return;
            bool hasNet = HasNetworkBody(attacker);
            if (!hasNet || dist > 2.8f + attacker.Stats.radius) return;
            float damage = (0.45f + attacker.Stats.chemicalDamagePerSecond * 0.25f) * AttackDamageMultiplier(attacker) * V5Balance.CombatTick;
            ApplyDamage(attacker, target, damage, V5DamageKind.Chemical, "net");
            target.Stats.stress = Mathf.Min(100f, target.Stats.stress + 0.08f);
        }

        private void ApplyDigestivePseudopods(V5CellEntity attacker, V5CellEntity target, float dist)
        {
            if (attacker == null || target == null) return;
            bool hasDigestiveArms = HasPlayerAdaptation(attacker, V5AdaptationId.Lysosome) && HasPlayerAdaptation(attacker, V5AdaptationId.Pseudopods);
            if (!hasDigestiveArms && !attacker.HasPhagocytosis) return;
            if (dist > attacker.Stats.radius + target.Stats.radius + 0.55f) return;

            float styleBonus = hasDigestiveArms ? 1.18f : 0.82f;
            float damage = (0.55f + attacker.Stats.physicalDamagePerSecond * 0.28f) * styleBonus * AttackDamageMultiplier(attacker) * V5Balance.CombatTick;
            ApplyDamage(attacker, target, damage, V5DamageKind.Physical, "phago");
            target.Stats.stress = Mathf.Min(100f, target.Stats.stress + 0.05f);
        }

        private bool HasCiliaryFilter(V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.HasStructure(V5StructureId.CoronaCilia) ||
                   cell.HasStructure(V5StructureId.Cilia) ||
                   cell.EvolutionPath == V5EvolutionPath.Ciliate ||
                   HasPlayerAdaptation(cell, V5AdaptationId.Cilia);
        }

        private bool HasNetworkBody(V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.HasMucilage ||
                   cell.HasStructure(V5StructureId.InvasiveHypha) ||
                   cell.HasStructure(V5StructureId.MucilageMatrix) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.FungalHypha) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.ExtracellularEnzymes) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.SlimePlasmodium) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.ColonialAdhesin);
        }

        private bool HasPlayerAdaptation(V5CellEntity cell, V5AdaptationId id)
        {
            if (cell == null || !cell.IsPlayerOwned) return false;
            V5GameManager gm = V5GameManager.Instance;
            return gm != null && gm.Adaptations != null && gm.Adaptations.Has(id);
        }

        private void ApplyDamage(V5CellEntity attacker, V5CellEntity target, float amount, V5DamageKind kind, string label)
        {
            if (attacker == null || target == null || amount <= 0f) return;
            bool aliveBefore = target.Stats.currentHp > 0f;
            V5GameManager gm = V5GameManager.Instance;

            bool routed = false;
            if (target.Role == V5CellRole.Mother && target.IsPlayerOwned && gm != null && gm.Body != null)
            {
                float leaked;
                if (gm.Body.TryAbsorbDamage(target, amount, kind, attacker.transform.position, out leaked))
                {
                    routed = true;
                    if (leaked > 0f) target.Damage(leaked, kind, attacker.transform.position);
                }
            }
            if (!routed) target.Damage(amount, kind, attacker.transform.position);

            bool killed = aliveBefore && target.Stats.currentHp <= 0f;
            if (gm != null && gm.AffinityLog != null) gm.AffinityLog.RecordCombat(attacker, label, killed);
            if (!ShowCombatNumbers) return;
            if (!attacker.IsPlayerOwned && !target.IsPlayerOwned) return;
            if (Time.unscaledTime < nextCombatFeedback) return;
            nextCombatFeedback = Time.unscaledTime + 0.08f;
            PushFloating(label + " " + Mathf.Max(0.1f, amount).ToString("0.0"), target.transform.position, DamageColor(kind, attacker.IsPlayerOwned));
        }

        private void PushFloating(string text, Vector2 world, Color color)
        {
            if (!ShowCombatNumbers) return;
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.PushFloating(text, world, color);
        }

        private Color DamageColor(V5DamageKind kind, bool playerAttack)
        {
            if (!playerAttack) return new Color(1f, 0.35f, 0.3f, 1f);
            if (kind == V5DamageKind.Chemical) return new Color(0.95f, 0.35f, 0.75f, 1f);
            if (kind == V5DamageKind.Piercing) return new Color(1f, 0.9f, 0.35f, 1f);
            return new Color(1f, 0.72f, 0.42f, 1f);
        }
    }
}
