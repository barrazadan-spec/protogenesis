using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CampaignFlowSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public int CampaignStep;
        public string CurrentChapter = "Capítulo 1 — La primera gota";
        public string LastMessage = "Campaña interna lista. Pulsa ] para abrir.";
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Awake()
        {
            CampaignStep = PlayerPrefs.GetInt("ProtogenesisV5_CampaignStep", 0);
            RefreshChapter();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightBracket)) ShowPanel = !ShowPanel;
            CheckProgression();
        }

        private void CheckProgression()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            int targetStep = CampaignStep;
            if (gm.Environment != null && gm.Environment.AverageColonization() >= 0.08f) targetStep = Mathf.Max(targetStep, 1);
            if (gm.Genes != null && gm.Genes.UnlockedCount >= 2) targetStep = Mathf.Max(targetStep, 2);
            if (gm.Apex != null && gm.Apex.ApexSpawned) targetStep = Mathf.Max(targetStep, 3);
            V5BossEncounterSystem bosses = FindFirstObjectByType<V5BossEncounterSystem>();
            if (bosses != null && bosses.BossesDefeated >= 1) targetStep = Mathf.Max(targetStep, 4);
            if (gm.Phase == V5GamePhase.Victory) targetStep = Mathf.Max(targetStep, 5);
            if (targetStep != CampaignStep)
            {
                CampaignStep = targetStep;
                PlayerPrefs.SetInt("ProtogenesisV5_CampaignStep", CampaignStep);
                PlayerPrefs.Save();
                RefreshChapter();
                if (gm.Hud != null) gm.Hud.Toast("Campaña: " + CurrentChapter);
            }
        }

        private void RefreshChapter()
        {
            if (CampaignStep <= 0) CurrentChapter = "Capítulo 1 — La primera gota";
            else if (CampaignStep == 1) CurrentChapter = "Capítulo 2 — Matriz colonial";
            else if (CampaignStep == 2) CurrentChapter = "Capítulo 3 — Dominio metabólico";
            else if (CampaignStep == 3) CurrentChapter = "Capítulo 4 — Forma apex";
            else if (CampaignStep == 4) CurrentChapter = "Capítulo 5 — Depredadores microscópicos";
            else CurrentChapter = "Capítulo 6 — Demo completa";
            LastMessage = "Progreso de campaña: " + CurrentChapter;
        }

        public V5ScenarioId RecommendedScenario()
        {
            if (CampaignStep <= 0) return V5ScenarioId.FirstDrop;
            if (CampaignStep == 1) return V5ScenarioId.OxygenWar;
            if (CampaignStep == 2) return V5ScenarioId.AcidFrontier;
            if (CampaignStep == 3) return V5ScenarioId.PredatorBloom;
            return V5ScenarioId.Freeplay;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return; EnsureStyles();
            Rect r = new Rect(540f, 192f, 420f, 260f); GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 10f, r.width - 28f, r.height - 20f));
            GUILayout.Label("CAMPAÑA INTERNA 1.1  []", title); GUILayout.Label(CurrentChapter, title); GUILayout.Label(LastMessage, body);
            GUILayout.Space(8f); GUILayout.Label("Siguiente escenario sugerido: " + RecommendedScenario());
            if (GUILayout.Button("Cargar escenario sugerido")) { V5GameManager gm = V5GameManager.Instance; if (gm != null && gm.RunReset != null) gm.RunReset.RestartScenario(RecommendedScenario()); }
            if (GUILayout.Button("Reset progreso campaña")) { CampaignStep = 0; PlayerPrefs.SetInt("ProtogenesisV5_CampaignStep", 0); PlayerPrefs.Save(); RefreshChapter(); }
            GUILayout.Label("Desbloqueos: colonización 8%, 2 genes, apex, miniboss derrotado y victoria.", body);
            GUILayout.EndArea();
        }
        private void EnsureStyles() { if (panel != null) return; panel = new GUIStyle(GUI.skin.box); title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.78f, 0.95f, 1f, 1f); body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white; }
    }
}
