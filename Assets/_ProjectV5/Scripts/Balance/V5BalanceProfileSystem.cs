using System.IO;
using UnityEngine;

namespace Protogenesis.V5
{
    [System.Serializable]
    public class V5BalanceProfile
    {
        public float version = 1.1f;
        public float playerResourceMultiplier = 1f;
        public float divisionCostMultiplier = 1f;
        public float enemySpawnMultiplier = 1f;
        public float biomeEffectMultiplier = 1f;
        public float lineageUpgradeCostMultiplier = 1f;
    }

    public class V5BalanceProfileSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public V5BalanceProfile Profile = new V5BalanceProfile();
        public string LastMessage = "J: balance runtime. Exporta/importa JSON en persistentDataPath.";
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;
        private string PathName { get { return Path.Combine(Application.persistentDataPath, "protogenesis_v5_balance_1_1.json"); } }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.J)) ShowPanel = !ShowPanel;
            if (Input.GetKeyDown(KeyCode.F6)) ExportProfile();
            if (Input.GetKeyDown(KeyCode.F8)) ImportProfile();
        }

        public void ExportProfile()
        {
            try
            {
                File.WriteAllText(PathName, JsonUtility.ToJson(Profile, true));
                LastMessage = "Balance exportado: " + PathName;
                if (V5GameManager.Instance != null && V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Balance exportado F6");
            }
            catch (System.Exception ex)
            {
                LastMessage = "Error exportando balance: " + ex.Message;
            }
        }

        public void ImportProfile()
        {
            try
            {
                if (!File.Exists(PathName))
                {
                    ExportProfile();
                    LastMessage = "No existía JSON; se creó plantilla: " + PathName;
                    return;
                }
                Profile = JsonUtility.FromJson<V5BalanceProfile>(File.ReadAllText(PathName));
                if (Profile == null) Profile = new V5BalanceProfile();
                LastMessage = "Balance importado: " + PathName;
                if (V5GameManager.Instance != null && V5GameManager.Instance.Hud != null) V5GameManager.Instance.Hud.Toast("Balance importado F8");
            }
            catch (System.Exception ex)
            {
                LastMessage = "Error importando balance: " + ex.Message;
            }
        }

        public float DivisionCostMultiplier()
        {
            return Mathf.Clamp(Profile != null ? Profile.divisionCostMultiplier : 1f, 0.25f, 4f);
        }

        public float PlayerResourceMultiplier()
        {
            return Mathf.Clamp(Profile != null ? Profile.playerResourceMultiplier : 1f, 0.1f, 5f);
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 455, Screen.height - 410, 445, 300);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "BALANCE RUNTIME 1.1", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 40), "Edita estos valores en runtime y exporta/importa JSON. Ruta: " + PathName);
            float y = r.y + 88;
            Slider(r.x + 12, ref Profile.playerResourceMultiplier, "Recursos jugador", 0.25f, 3f, ref y);
            Slider(r.x + 12, ref Profile.divisionCostMultiplier, "Costo división", 0.35f, 2.5f, ref y);
            Slider(r.x + 12, ref Profile.enemySpawnMultiplier, "Spawn enemigos", 0.25f, 3f, ref y);
            Slider(r.x + 12, ref Profile.biomeEffectMultiplier, "Efecto biomas", 0.25f, 3f, ref y);
            Slider(r.x + 12, ref Profile.lineageUpgradeCostMultiplier, "Costo mejoras linaje", 0.35f, 2.5f, ref y);
            if (GUI.Button(new Rect(r.x + 12, y, 140, 30), "F6 Exportar", button)) ExportProfile();
            if (GUI.Button(new Rect(r.x + 164, y, 140, 30), "F8 Importar", button)) ImportProfile();
            if (GUI.Button(new Rect(r.x + 316, y, 110, 30), "Reset", button)) Profile = new V5BalanceProfile();
            GUI.Label(new Rect(r.x + 12, y + 38, r.width - 24, 42), LastMessage);
        }

        private void Slider(float x, ref float value, string label, float min, float max, ref float y)
        {
            GUI.Label(new Rect(x, y, 180, 20), label + ": x" + value.ToString("0.00"));
            value = GUI.HorizontalSlider(new Rect(x + 190, y + 4, 220, 20), value, min, max);
            y += 32f;
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.normal.background = Texture2D.whiteTexture;
            box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label);
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button);
        }
    }
}
