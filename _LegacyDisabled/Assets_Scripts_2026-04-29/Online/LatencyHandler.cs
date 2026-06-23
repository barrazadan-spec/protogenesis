using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

namespace Protogenesis.Online
{
    /// <summary>
    /// Monitorea el ping del jugador local y muestra indicadores de latencia en UI.
    ///
    /// Umbrales:
    ///   RTT &lt; 150ms  → Sin indicador (conexión buena)
    ///   RTT &gt; 150ms  → Indicador amarillo  "Latencia alta"
    ///   RTT &gt; 200ms  → Indicador rojo      "Latencia crítica — puede afectar gameplay"
    ///   RTT &gt; 500ms  → Advertencia de posible desconexión
    /// </summary>
    public class LatencyHandler : NetworkBehaviour
    {
        [Header("UI")]
        [SerializeField] private GameObject latencyIndicator;    // Contenedor del indicador
        [SerializeField] private TMP_Text   latencyLabel;        // Texto descriptivo
        [SerializeField] private TMP_Text   pingValueText;       // Muestra el valor en ms
        [SerializeField] private Image      latencyIcon;         // Icono con color dinámico

        [Header("Umbrales (ms)")]
        [SerializeField] private float thresholdHigh     = 150f;
        [SerializeField] private float thresholdCritical = 200f;
        [SerializeField] private float thresholdDisconnect = 500f;

        [Header("Config")]
        [SerializeField] private float checkInterval = 2f; // Segundos entre checks

        // ── Colores ───────────────────────────────────────────────────────────────
        private static readonly Color ColorGood       = new Color(0f,  0.66f, 0.42f); // #00A86B (aliados)
        private static readonly Color ColorHigh       = new Color(1f,  0.85f, 0f);    // Amarillo
        private static readonly Color ColorCritical   = new Color(0.75f, 0.22f, 0.17f); // #C0392B (enemigos)
        private static readonly Color ColorDisconnect = new Color(1f,  0f,   0f);      // Rojo brillante

        // ── Estado ────────────────────────────────────────────────────────────────
        private Coroutine _monitorCoroutine;
        private float     _lastRTT = 0f;

        public override void OnNetworkSpawn()
        {
            if (!IsOwner) return; // Solo el owner monitorea su propio ping

            if (latencyIndicator != null) latencyIndicator.SetActive(false);
            _monitorCoroutine = StartCoroutine(MonitorLatencyLoop());
        }

        public override void OnNetworkDespawn()
        {
            if (_monitorCoroutine != null) StopCoroutine(_monitorCoroutine);
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region Monitor loop

        private IEnumerator MonitorLatencyLoop()
        {
            var wait = new WaitForSeconds(checkInterval);

            while (true)
            {
                yield return wait;
                CheckLatency();
            }
        }

        private void CheckLatency()
        {
            if (NetworkManager.Singleton == null || NetworkManager.Singleton.LocalClient == null)
                return;

            // RTT via UnityTransport (NGO 2.x — LocalClient.RTT no existe)
            float rttMs = 0f;
            var transport = NetworkManager.Singleton.NetworkConfig?.NetworkTransport
                            as Unity.Netcode.Transports.UTP.UnityTransport;
            if (transport != null)
                rttMs = transport.GetCurrentRtt(NetworkManager.ServerClientId) * 1000f;

            _lastRTT = rttMs;
            UpdateUI(rttMs);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region UI

        private void UpdateUI(float rttMs)
        {
            if (pingValueText != null)
                pingValueText.text = $"{rttMs:F0} ms";

            if (rttMs <= thresholdHigh)
            {
                // Conexión buena — ocultar indicador
                SetIndicator(false, "", ColorGood);
            }
            else if (rttMs <= thresholdCritical)
            {
                // Latencia alta
                SetIndicator(true, "Latencia alta", ColorHigh);
            }
            else if (rttMs <= thresholdDisconnect)
            {
                // Latencia crítica
                SetIndicator(true, "Latencia crítica — puede afectar gameplay", ColorCritical);
            }
            else
            {
                // Advertencia de desconexión inminente
                SetIndicator(true, "Advertencia: posible desconexión", ColorDisconnect);
                Debug.LogWarning($"[LatencyHandler] RTT crítico: {rttMs:F0}ms. Posible desconexión inminente.");
            }
        }

        private void SetIndicator(bool visible, string message, Color color)
        {
            if (latencyIndicator != null)
                latencyIndicator.SetActive(visible);

            if (latencyLabel != null)
                latencyLabel.text = message;

            if (latencyIcon != null)
                latencyIcon.color = color;

            if (latencyLabel != null)
                latencyLabel.color = color;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        public float CurrentRTTms => _lastRTT;
        public bool  IsHighLatency     => _lastRTT > thresholdHigh;
        public bool  IsCriticalLatency => _lastRTT > thresholdCritical;

        #endregion
    }
}
