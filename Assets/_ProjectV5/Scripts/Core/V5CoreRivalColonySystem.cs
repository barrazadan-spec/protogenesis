using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CoreRivalColonySystem : MonoBehaviour, IV5RunResettable
    {
        [Header("Rival Colony")]
        public bool EnableRivalColony = true;
        public bool EnableCoreMotherDefeat = true;
        public float RivalCoreHp = 2400f;
        public float RivalCoreRadius = 4.1f;
        public float SpawnRadiusFraction = 0.78f;
        public int TardigradeDefenders = 1;
        public int HarasserDefenders = 1;
        public int VolvoxDefenders = 1;
        public int AnchorDefenders = 1;
        public int InterdictorDefenders = 0;
        public string LastMessage = "Colonia rival esperando.";

        private V5CellEntity rivalCore;
        private bool rivalSpawned;
        private bool victoryClaimed;
        private readonly List<V5OrganismMorph> defenders = new List<V5OrganismMorph>(4);

        public V5CellEntity RivalCore { get { return rivalCore; } }
        public bool RivalSpawned { get { return rivalSpawned && rivalCore != null; } }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || !gm.CoreMode || !EnableRivalColony) return;
            if (!rivalSpawned) TrySpawn(gm);
            if (!rivalSpawned || victoryClaimed) return;

            if (rivalCore == null || rivalCore.Stats.currentHp <= 0f)
            {
                victoryClaimed = true;
                LastMessage = "Nucleo rival destruido.";
                V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                Vector2 world = rivalCore != null ? (Vector2)rivalCore.transform.position : Vector2.zero;
                if (feedback != null) feedback.Push("VICTORIA: nucleo rival destruido", world, new Color(0.7f, 1f, 0.45f, 1f));
                gm.Win("nucleo rival destruido");
            }
        }

        public void ResetForNewRun()
        {
            rivalCore = null;
            rivalSpawned = false;
            victoryClaimed = false;
            defenders.Clear();
            LastMessage = "Colonia rival esperando.";
        }

        public bool ShouldAllowCoreMotherDefeat()
        {
            return EnableRivalColony && EnableCoreMotherDefeat;
        }

        private void TrySpawn(V5GameManager gm)
        {
            if (gm == null || gm.Environment == null || gm.CellFactory == null || gm.OrganismMorph == null || gm.MotherCell == null) return;

            Vector2 spawn = RivalSpawnPosition(gm);
            rivalCore = gm.CellFactory.SpawnNeutral(spawn, V5EvolutionPath.Bacteria);
            if (rivalCore == null) return;

            rivalCore.name = "V5_RivalCore";
            rivalCore.Stats.maxHp = Mathf.Max(500f, RivalCoreHp);
            rivalCore.Stats.currentHp = rivalCore.Stats.maxHp;
            rivalCore.Stats.radius = Mathf.Max(1.5f, RivalCoreRadius);
            rivalCore.Stats.speed = 0f;
            rivalCore.Stats.sensorRange = 0f;
            rivalCore.Stats.attackRange = 0f;
            rivalCore.Stats.physicalDamagePerSecond = 0f;
            rivalCore.Stats.chemicalDamagePerSecond = 0f;
            rivalCore.Directive = V5Directive.Idle;
            rivalCore.DirectiveTarget = spawn;
            rivalCore.transform.localScale = Vector3.one * (rivalCore.Stats.radius * 2f);

            V5CoreRivalNucleusView view = rivalCore.gameObject.AddComponent<V5CoreRivalNucleusView>();
            view.Initialize(this, spawn);

            SpawnDefenders(gm, spawn);
            rivalSpawned = true;
            LastMessage = "Nucleo rival detectado: destruilo para ganar.";

            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push(LastMessage, spawn, new Color(1f, 0.32f, 0.24f, 1f));
        }

        private Vector2 RivalSpawnPosition(V5GameManager gm)
        {
            Vector2 away = Vector2.right;
            if (gm != null && gm.MotherCell != null)
            {
                Vector2 mother = gm.MotherCell.transform.position;
                if (mother.sqrMagnitude > 0.01f) away = -mother.normalized;
            }

            float radius = gm != null && gm.Environment != null ? gm.Environment.MapRadius * Mathf.Clamp(SpawnRadiusFraction, 0.35f, 0.92f) : 34f;
            return away.normalized * radius;
        }

        private void SpawnDefenders(V5GameManager gm, Vector2 center)
        {
            defenders.Clear();
            int spawned = 0;
            for (int i = 0; i < Mathf.Max(0, TardigradeDefenders); i++)
            {
                Vector2 p = center + RingOffset(spawned++, 7.6f);
                AddDefender(gm.OrganismMorph.SpawnEnemyOrganism(V5OrganismBlueprintKind.Tardigrade, p, 10));
            }

            for (int i = 0; i < Mathf.Max(0, HarasserDefenders); i++)
            {
                Vector2 p = center + RingOffset(spawned++, 8.4f);
                AddDefender(gm.OrganismMorph.SpawnEnemyOrganism(V5OrganismBlueprintKind.Harasser, p, 6));
            }

            for (int i = 0; i < Mathf.Max(0, VolvoxDefenders); i++)
            {
                Vector2 p = center + RingOffset(spawned++, 9.4f);
                AddDefender(gm.OrganismMorph.SpawnEnemyOrganism(V5OrganismBlueprintKind.Volvox, p, 8));
            }

            for (int i = 0; i < Mathf.Max(0, AnchorDefenders); i++)
            {
                Vector2 p = center + RingOffset(spawned++, 10.2f);
                AddDefender(gm.OrganismMorph.SpawnEnemyOrganism(V5OrganismBlueprintKind.Anchor, p, 9));
            }

            for (int i = 0; i < Mathf.Max(0, InterdictorDefenders); i++)
            {
                Vector2 p = center + RingOffset(spawned++, 10.8f);
                AddDefender(gm.OrganismMorph.SpawnEnemyOrganism(V5OrganismBlueprintKind.Interdictor, p, 8));
            }
        }

        private void AddDefender(V5OrganismMorph organism)
        {
            if (organism == null || organism.NucleusCell == null) return;
            defenders.Add(organism);
            organism.NucleusCell.Directive = V5Directive.Defend;
            organism.NucleusCell.DirectiveTarget = organism.NucleusCell.transform.position;
            V5EnemyBrain brain = organism.NucleusCell.GetComponent<V5EnemyBrain>();
            if (brain != null) brain.enabled = false;
        }

        private Vector2 RingOffset(int index, float radius)
        {
            float angle = index * 2.3999632f + 0.55f;
            return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
        }
    }

    [RequireComponent(typeof(V5CellEntity))]
    public class V5CoreRivalNucleusView : MonoBehaviour
    {
        private V5CoreRivalColonySystem owner;
        private V5CellEntity cell;
        private SpriteRenderer body;
        private SpriteRenderer ring;
        private Vector2 anchor;
        private GUIStyle label;

        public void Initialize(V5CoreRivalColonySystem system, Vector2 fixedPosition)
        {
            owner = system;
            anchor = fixedPosition;
        }

        private void Awake()
        {
            cell = GetComponent<V5CellEntity>();
            body = GetComponent<SpriteRenderer>();
            GameObject go = new GameObject("RivalCoreRing");
            go.transform.SetParent(transform, false);
            ring = go.AddComponent<SpriteRenderer>();
            ring.sprite = V5ProceduralSprites.CreateRingSprite(128, 0.18f);
            ring.sortingOrder = 20;
            ring.color = new Color(1f, 0.02f, 0.02f, 0.92f);
        }

        private void LateUpdate()
        {
            if (cell == null) return;
            transform.position = anchor;
            cell.Directive = V5Directive.Idle;
            cell.DirectiveTarget = anchor;

            float pulse = 0.5f + Mathf.Sin(Time.time * 3.4f) * 0.5f;
            if (body != null)
            {
                body.color = Color.Lerp(new Color(0.62f, 0.02f, 0.02f, 1f), new Color(1f, 0.16f, 0.1f, 1f), 0.22f + pulse * 0.18f);
                body.sortingOrder = 14;
            }

            if (ring != null)
            {
                ring.enabled = cell.Stats.currentHp > 0f;
                ring.transform.localScale = Vector3.one * (1.18f + pulse * 0.08f);
                ring.color = new Color(1f, 0.04f, 0.03f, 0.72f + pulse * 0.16f);
            }
        }

        private void OnGUI()
        {
            if (cell == null || cell.Stats.maxHp <= 0f || Camera.main == null) return;
            Vector3 screen = Camera.main.WorldToScreenPoint((Vector2)transform.position + Vector2.up * (cell.Stats.radius + 0.85f));
            if (screen.z < 0f) return;

            EnsureStyle();
            float width = Mathf.Clamp(112f + cell.Stats.radius * 14f, 136f, 190f);
            float height = 12f;
            float x = screen.x - width * 0.5f;
            float y = Screen.height - screen.y;
            Rect back = new Rect(x, y, width, height);
            Rect fill = new Rect(x + 1f, y + 1f, (width - 2f) * Mathf.Clamp01(cell.Stats.currentHp / cell.Stats.maxHp), height - 2f);

            Color old = GUI.color;
            GUI.color = new Color(0.08f, 0f, 0f, 0.88f);
            GUI.DrawTexture(back, Texture2D.whiteTexture);
            GUI.color = new Color(1f, 0.05f, 0.02f, 0.96f);
            GUI.DrawTexture(fill, Texture2D.whiteTexture);
            GUI.color = old;

            GUI.Label(new Rect(x - 18f, y - 19f, width + 36f, 18f), "NUCLEO RIVAL", label);
        }

        private void EnsureStyle()
        {
            if (label != null) return;
            label = new GUIStyle(GUI.skin.label);
            label.alignment = TextAnchor.MiddleCenter;
            label.fontStyle = FontStyle.Bold;
            label.fontSize = 11;
            label.normal.textColor = new Color(1f, 0.25f, 0.18f, 1f);
        }
    }
}
