using UnityEngine;

namespace Protogenesis.V5
{
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class V5ResourceNode : MonoBehaviour
    {
        public V5ResourceKind kind = V5ResourceKind.Biomass;
        public float amount = 40f;
        public float maxAmount = 40f;
        public float regenPerSecond = 0.5f;
        public bool depleted;
        public V5ResourceSystem Owner;
        public float VisualScaleMultiplier = 1f;
        private SpriteRenderer sr;
        private float pickupFeedbackAt;
        private static Sprite sprite;

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sprite == null) sprite = V5ProceduralSprites.CreateCircleSprite(32);
            sr.sprite = sprite;
            sr.sortingOrder = 2;
            CircleCollider2D col = GetComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.5f;
            Refresh();
        }

        private void Update()
        {
            if (IsCoreMode()) return;
            if (amount < maxAmount)
            {
                amount = Mathf.Min(maxAmount, amount + regenPerSecond * Time.deltaTime);
                depleted = amount <= 0.05f;
                Refresh();
            }
        }

        public void Setup(V5ResourceKind k, float a)
        {
            kind = k; amount = a; maxAmount = a; depleted = false; Refresh();
        }

        public void SetVisualScaleMultiplier(float multiplier)
        {
            VisualScaleMultiplier = Mathf.Max(0.2f, multiplier);
            Refresh();
        }

        public float Harvest(float request)
        {
            if (amount <= 0f) return 0f;
            float take = Mathf.Min(request, amount);
            amount -= take;
            depleted = amount <= 0.05f;
            Refresh();
            if (depleted && IsCoreMode()) HandleCoreDepleted();
            return take;
        }

        private void HandleCoreDepleted()
        {
            amount = 0f;
            V5ResourceSystem system = Owner != null ? Owner : (V5GameManager.Instance != null ? V5GameManager.Instance.Resources : null);
            if (system == null) system = FindFirstObjectByType<V5ResourceSystem>();
            if (system != null) system.NotifyNodeDepleted(this);
            else Destroy(gameObject);
        }

        private bool IsCoreMode()
        {
            V5GameManager gm = V5GameManager.Instance;
            return gm != null && gm.CoreMode;
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            if (depleted) return;
            V5CellEntity collector = other != null ? other.GetComponent<V5CellEntity>() : null;
            if (collector == null || !collector.IsPlayerOwned) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.CoreMode && gm.OrganismMorph != null && gm.OrganismMorph.IsMorphed && gm.OrganismMorph.IsOrganismCell(collector)) return;

            float casteHarvest = V5CasteLibrary.SynthesisMultiplier(collector.FunctionalCaste, collector.CellMode, collector.IsAttachedToBody);
            float take = Harvest((18f + collector.Stats.synthesisRate * 8f) * casteHarvest * Time.deltaTime);
            if (take <= 0f) return;

            V5CellEntity target = collector.Role == V5CellRole.Mother || collector.Mother == null ? collector : collector.Mother;
            target.Resources.Add(kind, take);

            if (Time.time >= pickupFeedbackAt)
            {
                pickupFeedbackAt = Time.time + 0.45f;
                V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                if (feedback != null) feedback.Push("+" + take.ToString("0") + " " + kind, transform.position, sr != null ? sr.color : Color.white);
            }
        }

        private void Refresh()
        {
            if (sr == null) return;
            Color c = Color.white;
            if (kind == V5ResourceKind.ATP) c = new Color(1f, 0.84f, 0.1f, 1f);
            else if (kind == V5ResourceKind.Biomass) c = new Color(0.25f, 0.8f, 0.35f, 1f);
            else if (kind == V5ResourceKind.AminoAcids) c = new Color(0.85f, 0.7f, 1f, 1f);
            else if (kind == V5ResourceKind.Lipids) c = new Color(0.55f, 0.85f, 1f, 1f);
            else if (kind == V5ResourceKind.Nucleotides) c = new Color(1f, 0.25f, 0.55f, 1f);
            else if (kind == V5ResourceKind.Minerals) c = new Color(0.75f, 0.65f, 0.5f, 1f);
            c.a = depleted ? 0.18f : 0.75f;
            sr.color = c;
            float s = Mathf.Lerp(0.25f, 0.8f, maxAmount <= 0f ? 0f : amount / maxAmount) * VisualScaleMultiplier;
            transform.localScale = Vector3.one * s;
        }
    }
}
