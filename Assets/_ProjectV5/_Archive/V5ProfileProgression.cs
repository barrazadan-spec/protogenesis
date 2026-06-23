using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ProfileProgression : MonoBehaviour
    {
        public int RunsStarted { get; private set; }
        public int RunsWon { get; private set; }
        public int ApexSpawned { get; private set; }
        public float BestColonization { get; private set; }
        public string LastProfileMessage = "";
        private bool countedRun;
        private bool countedWin;
        private bool countedApex;
        private float tick;

        private void Awake()
        {
            Load();
        }

        private void Update()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            if (!countedRun && gm.MotherCell != null)
            {
                countedRun = true;
                RunsStarted++;
                Save();
            }
            tick += Time.deltaTime;
            if (tick < 2f) return;
            tick = 0f;
            if (gm.Environment != null)
            {
                float c = gm.Environment.AverageColonization();
                if (c > BestColonization)
                {
                    BestColonization = c;
                    Save();
                }
            }
            if (!countedApex && gm.Apex != null && gm.Apex.ApexSpawned)
            {
                countedApex = true;
                ApexSpawned++;
                LastProfileMessage = "Meta progreso: primera forma apex de la run registrada.";
                Save();
            }
            if (!countedWin && gm.Phase == V5GamePhase.Victory)
            {
                countedWin = true;
                RunsWon++;
                LastProfileMessage = "Victoria guardada en perfil local.";
                Save();
            }
        }

        public void ResetProfile()
        {
            RunsStarted = 0;
            RunsWon = 0;
            ApexSpawned = 0;
            BestColonization = 0f;
            Save();
        }

        private void Load()
        {
            RunsStarted = PlayerPrefs.GetInt("V5_Profile_RunsStarted", 0);
            RunsWon = PlayerPrefs.GetInt("V5_Profile_RunsWon", 0);
            ApexSpawned = PlayerPrefs.GetInt("V5_Profile_ApexSpawned", 0);
            BestColonization = PlayerPrefs.GetFloat("V5_Profile_BestColonization", 0f);
        }

        private void Save()
        {
            PlayerPrefs.SetInt("V5_Profile_RunsStarted", RunsStarted);
            PlayerPrefs.SetInt("V5_Profile_RunsWon", RunsWon);
            PlayerPrefs.SetInt("V5_Profile_ApexSpawned", ApexSpawned);
            PlayerPrefs.SetFloat("V5_Profile_BestColonization", BestColonization);
            PlayerPrefs.Save();
        }
    }
}
