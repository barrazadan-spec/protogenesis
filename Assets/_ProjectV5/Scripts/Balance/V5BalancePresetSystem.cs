using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5BalancePreset { Tutorial, Easy, Standard, Hard, Sandbox }

    public class V5BalancePresetSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public V5BalancePreset CurrentPreset = V5BalancePreset.Standard;
        public string LastMessage = "Presets listos. Pulsa ' para abrir.";
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Quote)) ShowPanel = !ShowPanel;
        }

        public void ApplyPreset(V5BalancePreset preset)
        {
            CurrentPreset = preset;
            V5GameManager gm = V5GameManager.Instance;
            V5BalanceProfileSystem profileSystem = gm != null ? gm.BalanceProfile : FindFirstObjectByType<V5BalanceProfileSystem>();
            if (profileSystem == null) return;
            if (profileSystem.Profile == null) profileSystem.Profile = new V5BalanceProfile();
            V5BalanceProfile p = profileSystem.Profile;
            p.version = 1.1f;
            if (preset == V5BalancePreset.Tutorial)
            {
                p.playerResourceMultiplier = 1.75f; p.divisionCostMultiplier = 0.65f; p.enemySpawnMultiplier = 0.35f; p.biomeEffectMultiplier = 0.70f; p.lineageUpgradeCostMultiplier = 0.70f;
            }
            else if (preset == V5BalancePreset.Easy)
            {
                p.playerResourceMultiplier = 1.35f; p.divisionCostMultiplier = 0.80f; p.enemySpawnMultiplier = 0.65f; p.biomeEffectMultiplier = 0.85f; p.lineageUpgradeCostMultiplier = 0.85f;
            }
            else if (preset == V5BalancePreset.Hard)
            {
                p.playerResourceMultiplier = 0.88f; p.divisionCostMultiplier = 1.18f; p.enemySpawnMultiplier = 1.45f; p.biomeEffectMultiplier = 1.18f; p.lineageUpgradeCostMultiplier = 1.12f;
            }
            else if (preset == V5BalancePreset.Sandbox)
            {
                p.playerResourceMultiplier = 2.75f; p.divisionCostMultiplier = 0.45f; p.enemySpawnMultiplier = 0.15f; p.biomeEffectMultiplier = 0.65f; p.lineageUpgradeCostMultiplier = 0.50f;
            }
            else
            {
                p.playerResourceMultiplier = 1f; p.divisionCostMultiplier = 1f; p.enemySpawnMultiplier = 1f; p.biomeEffectMultiplier = 1f; p.lineageUpgradeCostMultiplier = 1f;
            }
            LastMessage = "Preset aplicado: " + preset;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        private void OnGUI()
        {
            if (!ShowPanel) return; EnsureStyles();
            Rect r = new Rect(Screen.width - 430f, Screen.height - 340f, 410f, 220f); GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 10f, r.width - 28f, r.height - 20f));
            GUILayout.Label("PRESETS DE BALANCE 1.1  [']", title); GUILayout.Label(LastMessage, body); GUILayout.Label("Actual: " + CurrentPreset);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Tutorial")) ApplyPreset(V5BalancePreset.Tutorial);
            if (GUILayout.Button("Easy")) ApplyPreset(V5BalancePreset.Easy);
            if (GUILayout.Button("Standard")) ApplyPreset(V5BalancePreset.Standard);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Hard")) ApplyPreset(V5BalancePreset.Hard);
            if (GUILayout.Button("Sandbox")) ApplyPreset(V5BalancePreset.Sandbox);
            GUILayout.EndHorizontal();
            GUILayout.Label("J/F6/F8 sigue disponible para JSON runtime.", body);
            GUILayout.EndArea();
        }
        private void EnsureStyles() { if (panel != null) return; panel = new GUIStyle(GUI.skin.box); title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.9f, 1f, 0.8f, 1f); body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white; }
    }
}
