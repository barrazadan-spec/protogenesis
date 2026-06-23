using System.Collections;
using UnityEngine;
using Unity.Netcode;
using TMPro;
using Protogenesis.Core;

namespace Protogenesis.Online
{
    /// <summary>
    /// Sincroniza el estado de juego entre host y clientes.
    ///
    /// - NetworkVariables replican ATP y Glucosa del servidor a todos los clientes (100ms)
    /// - ServerRpc para validar construcciones y spawns antes de confirmarlos
    /// - Modo autopilot (60 seg) cuando un jugador se desconecta
    ///
    /// REGLA: todo código que modifica estado corre SOLO en el servidor.
    /// Los clientes envían inputs via ServerRpc y reciben confirmación via ClientRpc.
    /// </summary>
    public class NetworkGameManager : NetworkBehaviour
    {
        public static NetworkGameManager Instance { get; private set; }

        // ── NetworkVariables (server → all clients, read-only para clientes) ────
        /// <summary>ATP del jugador local sincronizado desde el servidor.</summary>
        public NetworkVariable<float> NetworkATP =
            new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        /// <summary>Glucosa del jugador local sincronizada desde el servidor.</summary>
        public NetworkVariable<float> NetworkGlucose =
            new NetworkVariable<float>(0f, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

        // ── Config ────────────────────────────────────────────────────────────────
        [Header("Sync")]
        [Tooltip("Intervalo en segundos entre sincronizaciones de recursos (0.1 = 100ms)")]
        [SerializeField] private float syncInterval = 0.1f;

        [Header("Autopilot")]
        [Tooltip("Segundos en autopilot antes de rendirse definitivamente.")]
        [SerializeField] private float autopilotDuration = 60f;

        // ── Estado interno ────────────────────────────────────────────────────────
        private Coroutine _syncCoroutine;

        // ── UI de feedback ────────────────────────────────────────────────────────
        [Header("UI")]
        [SerializeField] private TMP_Text buildFeedbackText;

        // ─────────────────────────────────────────────────────────────────────────
        #region NetworkBehaviour Lifecycle

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this) { return; }
            Instance = this;

            if (IsServer)
            {
                _syncCoroutine = StartCoroutine(SyncResourcesLoop());
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            }

            // Clientes escuchan cambios en el ATP de red
            NetworkATP.OnValueChanged   += OnNetworkATPChanged;
            NetworkGlucose.OnValueChanged += OnNetworkGlucoseChanged;
        }

        public override void OnNetworkDespawn()
        {
            NetworkATP.OnValueChanged   -= OnNetworkATPChanged;
            NetworkGlucose.OnValueChanged -= OnNetworkGlucoseChanged;

            if (IsServer && NetworkManager.Singleton != null)
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;

            if (_syncCoroutine != null) StopCoroutine(_syncCoroutine);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Sincronización de recursos (100ms)

        /// <summary>Solo corre en el servidor. Lee ResourceManager local y actualiza NetworkVars.</summary>
        private IEnumerator SyncResourcesLoop()
        {
            var wait = new WaitForSeconds(syncInterval);
            while (true)
            {
                yield return wait;

                if (ResourceManager.Instance != null)
                {
                    NetworkATP.Value    = ResourceManager.Instance.GetResource(ResourceType.ATP);
                    NetworkGlucose.Value = ResourceManager.Instance.GetResource(ResourceType.Glucose);
                }
            }
        }

        private void OnNetworkATPChanged(float previous, float current)
        {
            // Los clientes reciben aquí la actualización; el HUD se actualiza via EventBus
            if (!IsServer)
                EventBus.TriggerATPChanged(current, current - previous);
        }

        private void OnNetworkGlucoseChanged(float previous, float current)
        {
            if (!IsServer)
                EventBus.TriggerResourceChanged(ResourceType.Glucose, current, current - previous);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Validación de construcción

        /// <summary>
        /// El cliente solicita construir un orgánulo. El servidor verifica recursos
        /// y confirma o rechaza.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ValidateBuildServerRpc(Vector2 position, string organelleType, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            float cost = GetOrganelleCost(organelleType);

            if (ResourceManager.Instance != null && ResourceManager.Instance.CanAfford(ResourceType.ATP, cost))
            {
                ResourceManager.Instance.ConsumeResource(ResourceType.ATP, cost);
                ConfirmBuildClientRpc(position, organelleType, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                Debug.Log($"[NetworkGM] Build confirmado: {organelleType} en {position} para cliente {senderId}");
            }
            else
            {
                DenyBuildClientRpc($"ATP insuficiente para construir {organelleType} (costo: {cost})", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                Debug.Log($"[NetworkGM] Build rechazado: {organelleType} — ATP insuficiente.");
            }
        }

        [ClientRpc]
        private void ConfirmBuildClientRpc(Vector2 position, string organelleType, ClientRpcParams clientRpcParams = default)
        {
            ShowFeedback($"Construyendo {organelleType}...", Color.green);
            // El OrganelleBuilder local puede instanciar el prefab aquí
            Organelles.OrganelleBuilder.Instance?.BuildFromNetwork(organelleType, position);
        }

        [ClientRpc]
        private void DenyBuildClientRpc(string reason, ClientRpcParams clientRpcParams = default)
        {
            ShowFeedback(reason, Color.red);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Validación de spawn de unidades

        /// <summary>
        /// El cliente solicita producir una unidad. El servidor verifica ATP antes de confirmar.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ValidateSpawnServerRpc(string unitType, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            float cost = GetUnitCost(unitType);

            if (ResourceManager.Instance != null && ResourceManager.Instance.CanAfford(ResourceType.ATP, cost))
            {
                ResourceManager.Instance.ConsumeResource(ResourceType.ATP, cost);
                ConfirmSpawnClientRpc(unitType, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                Debug.Log($"[NetworkGM] Spawn confirmado: {unitType} para cliente {senderId}");
            }
            else
            {
                DenySpawnClientRpc($"ATP insuficiente para producir {unitType} (costo: {cost})", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
            }
        }

        [ClientRpc]
        private void ConfirmSpawnClientRpc(string unitType, ClientRpcParams clientRpcParams = default)
        {
            ShowFeedback($"Produciendo {unitType}...", Color.green);
            Units.UnitSpawner.Instance?.SpawnUnit(unitType, Vector2.zero);
        }

        [ClientRpc]
        private void DenySpawnClientRpc(string reason, ClientRpcParams clientRpcParams = default)
        {
            ShowFeedback(reason, Color.red);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Desconexión — Autopilot

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.Log($"[NetworkGM] Cliente {clientId} desconectado. Iniciando autopilot ({autopilotDuration}s)...");
            StartCoroutine(AutopilotRoutine(clientId));
        }

        private IEnumerator AutopilotRoutine(ulong clientId)
        {
            // Notificar a los demás clientes
            NotifyDisconnectClientRpc(clientId);

            yield return new WaitForSeconds(autopilotDuration);

            // Después de autopilotDuration segundos, el jugador se rinde
            Debug.Log($"[NetworkGM] Autopilot expirado para cliente {clientId}. La célula se rinde.");
            SurrenderClientRpc(clientId);
        }

        [ClientRpc]
        private void NotifyDisconnectClientRpc(ulong disconnectedClientId)
        {
            ShowFeedback($"Jugador {disconnectedClientId} desconectado. Autopilot activo ({autopilotDuration}s).", Color.yellow);
        }

        [ClientRpc]
        private void SurrenderClientRpc(ulong disconnectedClientId)
        {
            Debug.Log($"[NetworkGM] Célula del jugador {disconnectedClientId} eliminada por desconexión.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Costos (tabla de referencia)

        private static float GetOrganelleCost(string organelleType) => organelleType switch
        {
            "Mitocondria" => 30f,
            "Ribosoma"    => 20f,
            "Lisosoma"    => 25f,
            _             => 15f
        };

        private static float GetUnitCost(string unitType) => unitType switch
        {
            "ProcaryoteBasic"    => 2f,
            "ProcaryoteScout"    => 3f,
            "ArchaeaShield"      => 4f,
            "ArchaeaHalo"        => 5f,
            _                    => 2f
        };

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region UI helpers

        private Coroutine _feedbackCoroutine;

        /// <summary>Muestra el resultado de un intento de sabotaje en el HUD.</summary>
        public void ShowSabotageResult(string message, bool success)
            => ShowFeedback(message, success ? Color.green : Color.red);

        private void ShowFeedback(string msg, Color color)
        {
            if (buildFeedbackText == null) return;
            buildFeedbackText.text  = msg;
            buildFeedbackText.color = color;

            if (_feedbackCoroutine != null) StopCoroutine(_feedbackCoroutine);
            _feedbackCoroutine = StartCoroutine(ClearFeedback(3f));
        }

        private IEnumerator ClearFeedback(float delay)
        {
            yield return new WaitForSeconds(delay);
            if (buildFeedbackText != null) buildFeedbackText.text = "";
        }

        #endregion
    }
}
