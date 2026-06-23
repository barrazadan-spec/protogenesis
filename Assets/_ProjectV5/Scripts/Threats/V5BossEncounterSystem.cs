using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5BossType { RotiferHunter, PhageCloud, AcidMatriarch, BloomLeviathan }

    public class V5BossEncounterSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public bool BossActive { get { return activeBoss != null; } }
        public int BossesDefeated;
        public string LastMessage = "Sin miniboss activo. Pulsa . para abrir panel.";
        public V5BossType CurrentBossType;
        private V5CellEntity activeBoss;
        private float bossTimer;
        private float pulseTimer;
        private float nextAutoSpawn = 520f;
        private bool rewardPending;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Period)) ShowPanel = !ShowPanel;
            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.Period)) SpawnRecommendedBoss(true);
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;
            if (activeBoss == null)
            {
                if (rewardPending) RewardDefeat();
                if (gm.ElapsedSeconds > nextAutoSpawn && gm.PlayerCellCount() >= 5)
                {
                    SpawnRecommendedBoss(false);
                    nextAutoSpawn = gm.ElapsedSeconds + Random.Range(520f, 760f);
                }
                return;
            }
            bossTimer += Time.deltaTime;
            pulseTimer += Time.deltaTime;
            if (pulseTimer >= 4.2f) { pulseTimer = 0f; BossPulse(); }
        }

        public void SpawnRecommendedBoss(bool debug)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.CellFactory == null || gm.Environment == null) return;
            if (activeBoss != null) { LastMessage = "Ya hay un miniboss activo."; return; }
            float oxygen = gm.Environment.AverageOxygen();
            float toxin = gm.Environment.AverageToxins();
            float acidity = gm.Environment.AverageAcidity();
            if (toxin > 0.22f) CurrentBossType = V5BossType.PhageCloud;
            else if (acidity > 0.62f) CurrentBossType = V5BossType.AcidMatriarch;
            else if (oxygen > 0.42f || gm.Environment.AverageColonization() > 0.18f) CurrentBossType = V5BossType.BloomLeviathan;
            else CurrentBossType = V5BossType.RotiferHunter;
            Vector2 dir = Random.insideUnitCircle.normalized;
            if (dir.sqrMagnitude < 0.1f) dir = Vector2.right;
            Vector2 pos = dir * gm.Environment.MapRadius * Random.Range(0.56f, 0.82f);
            V5EvolutionPath path = CurrentBossType == V5BossType.BloomLeviathan ? V5EvolutionPath.Cyanobacteria : V5EvolutionPath.Amoeba;
            if (CurrentBossType == V5BossType.AcidMatriarch) path = V5EvolutionPath.Archaea;
            if (CurrentBossType == V5BossType.PhageCloud) path = V5EvolutionPath.Bacteria;
            activeBoss = gm.CellFactory.SpawnNeutral(pos, path);
            activeBoss.name = "V5_BOSS_" + CurrentBossType;
            activeBoss.Stats.maxHp = debug ? 220f : Mathf.Lerp(220f, 430f, Mathf.Clamp01(gm.ElapsedSeconds / 1600f));
            activeBoss.Stats.currentHp = activeBoss.Stats.maxHp;
            activeBoss.Stats.radius *= 2.3f;
            activeBoss.Stats.speed *= CurrentBossType == V5BossType.RotiferHunter ? 1.18f : 0.82f;
            activeBoss.Stats.sensorRange += 7f;
            activeBoss.Stats.attackRange += 1.5f;
            activeBoss.Stats.physicalDamagePerSecond += CurrentBossType == V5BossType.RotiferHunter ? 6f : 2f;
            activeBoss.Stats.chemicalDamagePerSecond += CurrentBossType == V5BossType.PhageCloud ? 5.5f : 2.5f;
            activeBoss.Directive = V5Directive.Attack;
            if (activeBoss.GetComponent<V5EnemyBrain>() == null) activeBoss.gameObject.AddComponent<V5EnemyBrain>();
            bossTimer = 0f; pulseTimer = 0f; rewardPending = true;
            LastMessage = "Miniboss emergió: " + CurrentBossType;
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
            if (gm.Codex != null) gm.Codex.Unlock("Miniboss microscópico", "Encuentros de presión que fuerzan adaptación ecológica.");
        }

        private void BossPulse()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null || activeBoss == null) return;
            Vector2 p = activeBoss.transform.position;
            if (CurrentBossType == V5BossType.RotiferHunter)
            {
                for (int i = 0; i < gm.PlayerCells.Count; i++)
                {
                    V5CellEntity c = gm.PlayerCells[i]; if (c == null) continue;
                    float d = Vector2.Distance(c.transform.position, p);
                    if (d < 4.3f) c.Damage((4.3f - d) * 2.1f, V5DamageKind.Physical, p);
                }
                LastMessage = "Rotifer Hunter hizo barrido físico.";
            }
            else if (CurrentBossType == V5BossType.PhageCloud)
            {
                gm.Environment.ModifyArea(p, 6.5f, -0.02f, 0f, -0.03f, 0.16f, 0.02f, -0.03f, 0.04f);
                LastMessage = "Phage Cloud liberó toxinas.";
            }
            else if (CurrentBossType == V5BossType.AcidMatriarch)
            {
                gm.Environment.ModifyArea(p, 7.5f, -0.01f, -0.01f, -0.02f, 0.08f, 0.18f, -0.02f, 0.06f);
                LastMessage = "Acid Matriarch expandió frente ácido.";
            }
            else
            {
                gm.Environment.ModifyArea(p, 8f, 0.06f, 0.08f, 0.12f, -0.02f, -0.01f, 0.04f, 0.02f);
                if (Random.value < 0.35f && gm.CellFactory != null)
                {
                    V5CellEntity minion = gm.CellFactory.SpawnNeutral(p + Random.insideUnitCircle * 3f, V5EvolutionPath.Cyanobacteria);
                    if (minion != null) minion.gameObject.AddComponent<V5EnemyBrain>();
                }
                LastMessage = "Bloom Leviathan generó presión de enjambre.";
            }
        }

        private void RewardDefeat()
        {
            rewardPending = false; BossesDefeated++;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.MotherCell != null)
            {
                gm.MotherCell.Resources.atp += 95f; gm.MotherCell.Resources.biomass += 55f; gm.MotherCell.Resources.aminoAcids += 30f;
                gm.MotherCell.Resources.lipids += 24f; gm.MotherCell.Resources.nucleotides += 16f; gm.MotherCell.Resources.minerals += 12f;
                gm.MotherCell.Stats.stress = Mathf.Max(0f, gm.MotherCell.Stats.stress - 18f);
            }
            LastMessage = "Miniboss derrotado. Recompensa entregada.";
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        private void OnGUI()
        {
            if (!ShowPanel) return; EnsureStyles();
            Rect r = new Rect(Screen.width - 410f, 196f, 392f, 250f); GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("ENCUENTROS MINIBOSS 1.1  [.]", title); GUILayout.Label(LastMessage, body); GUILayout.Space(6f);
            GUILayout.Label("Activo: " + (activeBoss != null ? CurrentBossType.ToString() : "ninguno"));
            if (activeBoss != null)
            {
                GUILayout.Label("HP: " + activeBoss.Stats.currentHp.ToString("0") + " / " + activeBoss.Stats.maxHp.ToString("0") + " | Tiempo: " + bossTimer.ToString("0") + "s");
                if (GUILayout.Button("Enviar colonia a atacar")) CommandAttackBoss();
            }
            else if (GUILayout.Button("Invocar miniboss recomendado")) SpawnRecommendedBoss(true);
            GUILayout.Label("Derrotados: " + BossesDefeated); GUILayout.Label("Ctrl + . invoca rápido para test.", body);
            GUILayout.EndArea();
        }

        private void CommandAttackBoss()
        {
            if (activeBoss == null || V5GameManager.Instance == null) return;
            V5GameManager gm = V5GameManager.Instance;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i]; if (cell == null || cell.Role == V5CellRole.Mother) continue;
                cell.Directive = V5Directive.Attack; cell.AttackTarget = activeBoss; cell.DirectiveTarget = activeBoss.transform.position;
            }
            LastMessage = "Colonia enviada contra miniboss.";
        }
        private void EnsureStyles() { if (panel != null) return; panel = new GUIStyle(GUI.skin.box); title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(1f, 0.82f, 0.55f, 1f); body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white; }
    }
}
