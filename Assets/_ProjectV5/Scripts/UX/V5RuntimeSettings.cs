using UnityEngine;

namespace Protogenesis.V5
{
    public class V5RuntimeSettings : MonoBehaviour
    {
        public float SimulationSpeed = 1f;
        public bool MinimalOverlayMode;
        public string LastMessage = "";

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space)) TogglePause();
            if (Input.GetKeyDown(KeyCode.Minus) || Input.GetKeyDown(KeyCode.KeypadMinus)) SetSpeed(Mathf.Max(0.25f, SimulationSpeed - 0.25f));
            if (Input.GetKeyDown(KeyCode.Equals) || Input.GetKeyDown(KeyCode.KeypadPlus)) SetSpeed(Mathf.Min(3f, SimulationSpeed + 0.25f));
            if (Input.GetKeyDown(KeyCode.BackQuote)) MinimalOverlayMode = !MinimalOverlayMode;
        }

        public void TogglePause()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            gm.Paused = !gm.Paused;
            Time.timeScale = gm.Paused ? 0f : SimulationSpeed;
            LastMessage = gm.Paused ? "Pausa" : "Simulación reanudada";
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        public void SetSpeed(float speed)
        {
            SimulationSpeed = Mathf.Clamp(speed, 0.25f, 3f);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && !gm.Paused) Time.timeScale = SimulationSpeed;
            LastMessage = "Velocidad x" + SimulationSpeed.ToString("0.00");
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
        }
    }
}
