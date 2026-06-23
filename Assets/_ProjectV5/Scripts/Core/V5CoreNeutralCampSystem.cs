using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CoreNeutralCampSystem : MonoBehaviour, IV5RunResettable
    {
        [Header("Neutral Camps")]
        public bool EnableNeutralCamps = true;
        public float RespawnSeconds = 55f;
        public float WanderRadius = 6.5f;
        public float WanderInterval = 5.0f;
        public int CampCount = 6;
        public string LastMessage = "Jungla neutral esperando.";

        private readonly List<NeutralCamp> camps = new List<NeutralCamp>(8);

        private class NeutralCamp
        {
            public Vector2 center;
            public V5OrganismBlueprintKind[] kinds;
            public readonly List<V5OrganismMorph> organisms = new List<V5OrganismMorph>(3);
            public float respawnAt;
            public float nextWanderAt;
            public V5CellEntity aggroTarget;
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || !EnableNeutralCamps || gm.OrganismMorph == null || gm.Environment == null || gm.MotherCell == null) return;
            if (camps.Count == 0) BuildCamps(gm);

            for (int i = 0; i < camps.Count; i++)
            {
                NeutralCamp camp = camps[i];
                PruneCamp(camp);
                if (camp.organisms.Count == 0)
                {
                    if (camp.respawnAt <= 0f) camp.respawnAt = Time.time + RespawnSeconds;
                    if (Time.time >= camp.respawnAt) SpawnCamp(gm, camp);
                    continue;
                }

                TickCampBehavior(gm, camp);
            }
        }

        public void ResetForNewRun()
        {
            camps.Clear();
            LastMessage = "Jungla neutral esperando.";
        }

        public void NotifyCampAttacked(V5NeutralCampMember member, Vector2 source)
        {
            if (member == null || member.CampIndex < 0 || member.CampIndex >= camps.Count) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;

            NeutralCamp camp = camps[member.CampIndex];
            V5CellEntity target = NearestPlayerCell(gm, source, 13f);
            if (target == null) target = gm.MotherCell;
            camp.aggroTarget = target;
            for (int i = 0; i < camp.organisms.Count; i++)
                if (camp.organisms[i] != null && camp.organisms[i].IsAlive) camp.organisms[i].IssueAttack(target);
        }

        private void BuildCamps(V5GameManager gm)
        {
            camps.Clear();
            Vector2 start = gm.MotherCell.transform.position;
            Vector2 end = RivalPositionOrFallback(gm);
            int count = Mathf.Clamp(CampCount, 3, 8);
            for (int i = 0; i < count; i++)
            {
                float t = (i + 1f) / (count + 1f);
                Vector2 spine = Vector2.Lerp(start, end, t);
                Vector2 side = Vector2.Perpendicular((end - start).sqrMagnitude > 0.1f ? (end - start).normalized : Vector2.right);
                float offset = ((i % 2 == 0) ? 1f : -1f) * Mathf.Lerp(5.5f, 15f, (i % 3) / 2f);

                NeutralCamp camp = new NeutralCamp();
                camp.center = ClampInsideMap(gm, spine + side * offset + Random.insideUnitCircle * 2.4f);
                camp.kinds = CampKindsForIndex(i);
                camps.Add(camp);
                SpawnCamp(gm, camp);
            }

            LastMessage = "Jungla neutral: " + camps.Count + " campamentos.";
        }

        private void SpawnCamp(V5GameManager gm, NeutralCamp camp)
        {
            camp.organisms.Clear();
            camp.aggroTarget = null;
            camp.respawnAt = 0f;
            camp.nextWanderAt = Time.time + Random.Range(1f, WanderInterval);

            int campIndex = camps.IndexOf(camp);
            for (int i = 0; i < camp.kinds.Length; i++)
            {
                Vector2 pos = ClampInsideMap(gm, camp.center + RingOffset(i, 2.8f + i * 1.8f));
                int reward = NeutralRewardFor(camp.kinds[i]);
                V5OrganismMorph organism = gm.OrganismMorph.SpawnNeutralOrganism(camp.kinds[i], pos, reward);
                if (organism == null) continue;
                organism.ConfigureNeutralRoam(camp.center, WanderRadius);
                camp.organisms.Add(organism);
                MarkMembers(organism, campIndex);
                organism.IssueMove(camp.center + Random.insideUnitCircle * WanderRadius * 0.35f);
            }
        }

        private void MarkMembers(V5OrganismMorph organism, int campIndex)
        {
            if (organism == null) return;
            for (int i = 0; i < organism.Members.Count; i++)
            {
                V5CellEntity cell = organism.Members[i];
                if (cell == null) continue;
                cell.IsCoreNeutral = true;
                V5NeutralCampMember member = cell.GetComponent<V5NeutralCampMember>();
                if (member == null) member = cell.gameObject.AddComponent<V5NeutralCampMember>();
                member.Initialize(this, campIndex);
            }
        }

        private void TickCampBehavior(V5GameManager gm, NeutralCamp camp)
        {
            if (camp.aggroTarget != null && camp.aggroTarget.Stats.currentHp > 0f)
            {
                float maxChase = Mathf.Max(12f, WanderRadius * 2.3f);
                if (Vector2.Distance(camp.center, camp.aggroTarget.transform.position) <= maxChase)
                {
                    for (int i = 0; i < camp.organisms.Count; i++)
                        if (camp.organisms[i] != null && camp.organisms[i].IsAlive) camp.organisms[i].IssueAttack(camp.aggroTarget);
                    return;
                }
            }

            camp.aggroTarget = null;
            if (Time.time < camp.nextWanderAt) return;
            camp.nextWanderAt = Time.time + WanderInterval + Random.Range(-1.2f, 1.6f);
            for (int i = 0; i < camp.organisms.Count; i++)
            {
                V5OrganismMorph organism = camp.organisms[i];
                if (organism == null || !organism.IsAlive) continue;
                organism.IssueMove(ClampInsideMap(gm, camp.center + Random.insideUnitCircle * WanderRadius));
            }
        }

        private void PruneCamp(NeutralCamp camp)
        {
            for (int i = camp.organisms.Count - 1; i >= 0; i--)
                if (camp.organisms[i] == null || !camp.organisms[i].IsAlive) camp.organisms.RemoveAt(i);
        }

        private V5CellEntity NearestPlayerCell(V5GameManager gm, Vector2 from, float maxRange)
        {
            V5CellEntity bestCell = null;
            float best = maxRange * maxRange;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (cell == null || cell.Stats.currentHp <= 0f) continue;
                float d = Vector2.SqrMagnitude((Vector2)cell.transform.position - from);
                if (d >= best) continue;
                best = d;
                bestCell = cell;
            }
            return bestCell;
        }

        private Vector2 RivalPositionOrFallback(V5GameManager gm)
        {
            V5CoreRivalColonySystem rival = FindFirstObjectByType<V5CoreRivalColonySystem>();
            if (rival != null && rival.RivalCore != null) return rival.RivalCore.transform.position;
            Vector2 start = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.left * 30f;
            float radius = gm != null && gm.Environment != null ? gm.Environment.MapRadius * 0.76f : 48f;
            return -start.normalized * radius;
        }

        private V5OrganismBlueprintKind[] CampKindsForIndex(int index)
        {
            switch (index)
            {
                case 0: return new[] { V5OrganismBlueprintKind.Collector };
                case 1: return new[] { V5OrganismBlueprintKind.Harasser };
                case 2: return new[] { V5OrganismBlueprintKind.Interdictor };
                case 3: return new[] { V5OrganismBlueprintKind.Anchor };
                case 4: return new[] { V5OrganismBlueprintKind.Volvox };
                default: return new[] { V5OrganismBlueprintKind.Tardigrade };
            }
        }

        private int NeutralRewardFor(V5OrganismBlueprintKind kind)
        {
            switch (kind)
            {
                case V5OrganismBlueprintKind.Collector: return 5;
                case V5OrganismBlueprintKind.Harasser: return 6;
                case V5OrganismBlueprintKind.Interdictor: return 8;
                case V5OrganismBlueprintKind.Anchor: return 10;
                case V5OrganismBlueprintKind.Volvox: return 14;
                case V5OrganismBlueprintKind.Tardigrade: return 22;
                default: return 6;
            }
        }

        private Vector2 RingOffset(int index, float radius)
        {
            float angle = index * 2.3999632f + 0.8f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }

        private Vector2 ClampInsideMap(V5GameManager gm, Vector2 position)
        {
            if (gm == null || gm.Environment == null) return position;
            float radius = gm.Environment.MapRadius * 0.90f;
            if (position.sqrMagnitude <= radius * radius) return position;
            return position.normalized * radius;
        }
    }

    public class V5NeutralCampMember : MonoBehaviour
    {
        public int CampIndex { get; private set; }
        private V5CoreNeutralCampSystem owner;

        public void Initialize(V5CoreNeutralCampSystem system, int campIndex)
        {
            owner = system;
            CampIndex = campIndex;
        }

        public void NotifyDamaged(Vector2 source)
        {
            if (owner == null) owner = FindFirstObjectByType<V5CoreNeutralCampSystem>();
            if (owner != null) owner.NotifyCampAttacked(this, source);
        }
    }
}
