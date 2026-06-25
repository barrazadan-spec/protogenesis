using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5ResourceEcology
    {
        Debris,
        MicroPlankton,
        Algae
    }

    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(CircleCollider2D))]
    public class V5ResourceNode : MonoBehaviour
    {
        public V5ResourceKind kind = V5ResourceKind.Biomass;
        public float amount = 40f;
        public float maxAmount = 40f;
        public float regenPerSecond = 0.5f;
        public bool depleted;
        public bool living;
        public V5ResourceEcology Ecology = V5ResourceEcology.Debris;
        public V5ResourceSystem Owner;
        public float VisualScaleMultiplier = 1f;
        private SpriteRenderer sr;
        private float pickupFeedbackAt;
        private Vector2 driftDirection;
        private Vector2 driftTargetDirection;
        private float driftSpeed;
        private float nextDriftChangeAt;
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
            if (living) TickLivingDrift();
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

        public void SetLiving(bool value)
        {
            SetEcology(value ? V5ResourceEcology.MicroPlankton : V5ResourceEcology.Debris);
        }

        public void SetEcology(V5ResourceEcology ecology)
        {
            Ecology = ecology;
            living = ecology != V5ResourceEcology.Debris;
            if (living) ResetLivingDrift();
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

        private void ResetLivingDrift()
        {
            driftDirection = Random.insideUnitCircle.normalized;
            if (driftDirection.sqrMagnitude < 0.01f) driftDirection = Vector2.right;
            V5GameManager manager = V5GameManager.Instance;
            V5EnvironmentGrid environment = manager != null ? manager.Environment : null;
            if (Ecology == V5ResourceEcology.Algae && environment != null)
                driftDirection = ChoosePhototaxisDirection(environment, transform.position);
            driftTargetDirection = driftDirection;
            driftSpeed = Random.Range(0.3f, 0.6f);
            nextDriftChangeAt = Time.time + Random.Range(2f, 3f);
        }

        private void TickLivingDrift()
        {
            if (driftDirection.sqrMagnitude < 0.01f) ResetLivingDrift();
            if (Time.time >= nextDriftChangeAt)
            {
                V5GameManager manager = V5GameManager.Instance;
                V5EnvironmentGrid driftEnvironment = manager != null ? manager.Environment : null;
                if (Ecology == V5ResourceEcology.Algae && driftEnvironment != null)
                    driftTargetDirection = ChoosePhototaxisDirection(driftEnvironment, transform.position);
                else
                    driftTargetDirection = (driftDirection + Random.insideUnitCircle * 0.85f).normalized;
                if (driftTargetDirection.sqrMagnitude < 0.01f) driftTargetDirection = driftDirection;
                driftSpeed = Random.Range(0.3f, 0.6f);
                nextDriftChangeAt = Time.time + Random.Range(2f, 3f);
            }

            float steeringSpeed = Ecology == V5ResourceEcology.Algae ? 2.2f : 0.9f;
            driftDirection = Vector2.Lerp(driftDirection, driftTargetDirection, Mathf.Clamp01(Time.deltaTime * steeringSpeed)).normalized;
            Vector2 next = (Vector2)transform.position + driftDirection * driftSpeed * Time.deltaTime;
            V5GameManager gm = V5GameManager.Instance;
            V5EnvironmentGrid environment = gm != null ? gm.Environment : null;
            if (environment != null)
            {
                float margin = Mathf.Max(0.2f, transform.localScale.x * 0.5f);
                float allowedRadius = Mathf.Max(0.5f, environment.MapRadius - margin);
                if (next.sqrMagnitude > allowedRadius * allowedRadius)
                {
                    next = next.normalized * allowedRadius;
                    Vector2 tangent = new Vector2(-next.y, next.x).normalized * Random.Range(-0.35f, 0.35f);
                    driftTargetDirection = (-next.normalized + tangent).normalized;
                    driftDirection = driftTargetDirection;
                    nextDriftChangeAt = Time.time + Random.Range(2f, 3f);
                }
            }
            transform.position = next;
        }

        private Vector2 ChoosePhototaxisDirection(V5EnvironmentGrid environment, Vector2 position)
        {
            Vector2 bestDirection = driftDirection.sqrMagnitude > 0.01f ? driftDirection.normalized : Vector2.right;
            float bestScore = environment.Sample(V5OverlayMode.Light, position);
            float phase = Random.Range(0f, Mathf.PI * 2f);
            const int samples = 12;
            for (int i = 0; i < samples; i++)
            {
                float angle = phase + i * Mathf.PI * 2f / samples;
                Vector2 direction = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
                Vector2 nearPoint = position + direction * 4.5f;
                Vector2 farPoint = position + direction * 11f;
                int x, y;
                if (!environment.WorldToTile(farPoint, out x, out y)) continue;
                float score = environment.Sample(V5OverlayMode.Light, nearPoint) * 0.62f
                    + environment.Sample(V5OverlayMode.Light, farPoint) * 0.38f
                    + Vector2.Dot(direction, bestDirection) * 0.012f;
                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = direction;
                }
            }

            Vector2 wander = Random.insideUnitCircle * 0.12f;
            return (bestDirection + wander).normalized;
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
            if (Ecology == V5ResourceEcology.Algae) c = Color.Lerp(c, new Color(0.05f, 1f, 0.18f, 1f), 0.62f);
            else if (living) c = Color.Lerp(c, new Color(0.30f, 0.92f, 0.72f, 1f), 0.32f);
            c.a = depleted ? 0.18f : (living ? 0.88f : 0.68f);
            sr.color = c;
            float typeScale = Ecology == V5ResourceEcology.Algae ? 0.94f : (living ? 0.84f : 1.08f);
            float s = Mathf.Lerp(0.25f, 0.8f, maxAmount <= 0f ? 0f : amount / maxAmount) * VisualScaleMultiplier * typeScale;
            transform.localScale = Vector3.one * s;
        }
    }
}
