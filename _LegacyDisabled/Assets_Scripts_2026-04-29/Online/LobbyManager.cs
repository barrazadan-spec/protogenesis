using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;

namespace Protogenesis.Online
{
    /// <summary>
    /// Gestiona la creación y unión a salas de juego usando Unity Lobby + Unity Relay.
    /// Genera códigos de sala de 6 caracteres. Solo el host puede iniciar la partida.
    /// </summary>
    public class LobbyManager : MonoBehaviour
    {
        public static LobbyManager Instance { get; private set; }

        // ── UI ────────────────────────────────────────────────────────────────────
        [Header("UI — Lobby")]
        [SerializeField] private GameObject  lobbyPanel;
        [SerializeField] private TMP_InputField codeInputField;
        [SerializeField] private Button      btnCreate;
        [SerializeField] private Button      btnJoin;
        [SerializeField] private Button      btnStart;        // Solo visible para el host
        [SerializeField] private TMP_Text    lobbyCodeDisplay;
        [SerializeField] private TMP_Text    statusText;
        [SerializeField] private Transform   playerListContainer;
        [SerializeField] private TMP_Text    playerListItemPrefab;

        [Header("Config")]
        [SerializeField] private int   maxPlayers    = 4;
        [SerializeField] private float heartbeatInterval = 15f; // Unity Lobby requiere heartbeat

        // ── Estado interno ────────────────────────────────────────────────────────
        private Lobby  _currentLobby;
        private string _localPlayerId;
        private bool   _isHost = false;
        private Coroutine _heartbeatCoroutine;
        private Coroutine _pollCoroutine;

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private async void Start()
        {
            await InitializeServicesAsync();
            SetupUI();
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
            _ = LeaveLobbyAsync();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Inicialización

        private async Task InitializeServicesAsync()
        {
            try
            {
                await UnityServices.InitializeAsync();

                if (!AuthenticationService.Instance.IsSignedIn)
                    await AuthenticationService.Instance.SignInAnonymouslyAsync();

                _localPlayerId = AuthenticationService.Instance.PlayerId;
                SetStatus("Listo. Crea o únete a una sala.");
                Debug.Log($"[LobbyManager] Autenticado. PlayerID: {_localPlayerId}");
            }
            catch (Exception e)
            {
                SetStatus($"Error de conexión: {e.Message}");
                Debug.LogError($"[LobbyManager] Error inicializando servicios: {e}");
            }
        }

        private void SetupUI()
        {
            btnCreate?.onClick.AddListener(() => _ = CreateLobbyAsync());
            btnJoin?.onClick.AddListener(()   => _ = JoinLobbyByCodeAsync(codeInputField?.text));
            btnStart?.onClick.AddListener(()  => _ = StartGameAsync());

            if (btnStart != null) btnStart.gameObject.SetActive(false);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Crear sala

        /// <summary>
        /// Crea una sala en Unity Lobby, genera código de 6 caracteres
        /// y activa Unity Relay para obtener el JoinCode.
        /// </summary>
        public async Task CreateLobbyAsync(string lobbyName = null)
        {
            try
            {
                SetStatus("Creando sala...");

                string name = lobbyName ?? $"ProtoCell_{GenerateShortCode()}";

                // Crear asignación Relay para el host
                Allocation hostAllocation = await RelayService.Instance.CreateAllocationAsync(maxPlayers - 1);
                string relayJoinCode = await RelayService.Instance.GetJoinCodeAsync(hostAllocation.AllocationId);

                // Opciones de lobby
                var options = new CreateLobbyOptions
                {
                    IsPrivate = false,
                    Data = new Dictionary<string, DataObject>
                    {
                        { "RelayJoinCode", new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                    }
                };

                _currentLobby = await LobbyService.Instance.CreateLobbyAsync(name, maxPlayers, options);
                _isHost        = true;

                // Configurar Netcode con Relay
                var relayServerData = new RelayServerData(hostAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartHost();

                string displayCode = _currentLobby.LobbyCode;
                if (lobbyCodeDisplay != null) lobbyCodeDisplay.text = $"Código: {displayCode}";
                if (btnStart != null) btnStart.gameObject.SetActive(true);

                SetStatus($"Sala creada. Código: {displayCode}");
                Debug.Log($"[LobbyManager] Sala creada: {_currentLobby.Id} | Código: {displayCode}");

                _heartbeatCoroutine = StartCoroutine(HeartbeatLoop());
                _pollCoroutine      = StartCoroutine(PollLobbyLoop());

                RefreshPlayerList();
            }
            catch (Exception e)
            {
                SetStatus($"Error al crear sala: {e.Message}");
                Debug.LogError($"[LobbyManager] CreateLobby error: {e}");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Unirse a sala

        /// <summary>
        /// Une al jugador local a una sala existente usando el código de 6 caracteres.
        /// </summary>
        public async Task JoinLobbyByCodeAsync(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                SetStatus("Ingresa un código de sala válido.");
                return;
            }

            try
            {
                SetStatus($"Uniéndose a sala {code.ToUpper()}...");

                _currentLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(code.ToUpper());
                _isHost        = false;

                // Obtener Relay join code del lobby
                string relayJoinCode = _currentLobby.Data["RelayJoinCode"].Value;

                JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayJoinCode);

                var relayServerData = new RelayServerData(joinAllocation, "dtls");
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
                NetworkManager.Singleton.StartClient();

                if (btnStart != null) btnStart.gameObject.SetActive(false);

                SetStatus($"Conectado a sala: {_currentLobby.Name}");
                Debug.Log($"[LobbyManager] Unido a sala: {_currentLobby.Id}");

                _pollCoroutine = StartCoroutine(PollLobbyLoop());
                RefreshPlayerList();
            }
            catch (LobbyServiceException e)
            {
                SetStatus($"Sala no encontrada o llena: {e.Message}");
                Debug.LogError($"[LobbyManager] JoinLobby error: {e}");
            }
            catch (Exception e)
            {
                SetStatus($"Error de conexión: {e.Message}");
                Debug.LogError($"[LobbyManager] JoinLobby error: {e}");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Iniciar partida

        /// <summary>
        /// Solo el host puede llamar esto. Carga la escena de juego en todos los clientes.
        /// </summary>
        public async Task StartGameAsync()
        {
            if (!_isHost)
            {
                SetStatus("Solo el host puede iniciar la partida.");
                return;
            }

            if (_currentLobby == null)
            {
                SetStatus("No hay sala activa.");
                return;
            }

            try
            {
                SetStatus("Iniciando partida...");

                // Marcar lobby como en juego para que no entren más jugadores
                await LobbyService.Instance.UpdateLobbyAsync(_currentLobby.Id, new UpdateLobbyOptions
                {
                    Data = new Dictionary<string, DataObject>
                    {
                        { "GameStarted", new DataObject(DataObject.VisibilityOptions.Public, "true") }
                    }
                });

                // Cargar escena ERA_0 en todos los clientes via Netcode
                NetworkManager.Singleton.SceneManager.LoadScene("ERA_0", UnityEngine.SceneManagement.LoadSceneMode.Single);

                if (lobbyPanel != null) lobbyPanel.SetActive(false);

                Debug.Log("[LobbyManager] Partida iniciada. Cargando ERA_0...");
            }
            catch (Exception e)
            {
                SetStatus($"Error al iniciar: {e.Message}");
                Debug.LogError($"[LobbyManager] StartGame error: {e}");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Salir de sala

        public async Task LeaveLobbyAsync()
        {
            if (_currentLobby == null) return;

            try
            {
                if (_heartbeatCoroutine != null) StopCoroutine(_heartbeatCoroutine);
                if (_pollCoroutine      != null) StopCoroutine(_pollCoroutine);

                if (_isHost)
                    await LobbyService.Instance.DeleteLobbyAsync(_currentLobby.Id);
                else
                    await LobbyService.Instance.RemovePlayerAsync(_currentLobby.Id, _localPlayerId);

                _currentLobby = null;
                _isHost       = false;
                Debug.Log("[LobbyManager] Sala abandonada.");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[LobbyManager] Error al salir de sala: {e.Message}");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Heartbeat & Polling

        /// <summary>El host debe enviar heartbeat para mantener el lobby vivo.</summary>
        private IEnumerator HeartbeatLoop()
        {
            while (_currentLobby != null && _isHost)
            {
                yield return new WaitForSeconds(heartbeatInterval);
                try
                {
                    _ = LobbyService.Instance.SendHeartbeatPingAsync(_currentLobby.Id);
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LobbyManager] Heartbeat error: {e.Message}");
                }
            }
        }

        /// <summary>Actualiza la lista de jugadores periódicamente.</summary>
        private IEnumerator PollLobbyLoop()
        {
            while (_currentLobby != null)
            {
                yield return new WaitForSeconds(2f);
                try
                {
                    _ = PollLobbyAsync();
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[LobbyManager] Poll error: {e.Message}");
                }
            }
        }

        private async Task PollLobbyAsync()
        {
            if (_currentLobby == null) return;
            _currentLobby = await LobbyService.Instance.GetLobbyAsync(_currentLobby.Id);
            RefreshPlayerList();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region UI helpers

        private void RefreshPlayerList()
        {
            if (playerListContainer == null || _currentLobby == null) return;

            // Limpiar lista existente
            foreach (Transform child in playerListContainer)
                Destroy(child.gameObject);

            // Repoblar
            foreach (var player in _currentLobby.Players)
            {
                if (playerListItemPrefab != null)
                {
                    var item = Instantiate(playerListItemPrefab, playerListContainer);
                    bool isMe   = player.Id == _localPlayerId;
                    bool isHost = player.Id == _currentLobby.HostId;
                    item.text = $"{player.Id[..6]}  {(isHost ? "[HOST]" : "")} {(isMe ? "(Tú)" : "")}";
                }
            }
        }

        private void SetStatus(string msg)
        {
            if (statusText != null) statusText.text = msg;
            Debug.Log($"[LobbyManager] {msg}");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Utilidades

        /// <summary>Genera un código de sala de 6 caracteres alfanuméricos en mayúsculas.</summary>
        private static string GenerateShortCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var result = new System.Text.StringBuilder(6);
            var rng    = new System.Random();
            for (int i = 0; i < 6; i++)
                result.Append(chars[rng.Next(chars.Length)]);
            return result.ToString();
        }

        public Lobby  CurrentLobby => _currentLobby;
        public bool   IsHost       => _isHost;
        public string LocalPlayerId => _localPlayerId;

        #endregion
    }
}
