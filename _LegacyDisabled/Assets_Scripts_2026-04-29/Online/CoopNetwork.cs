using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Protogenesis.Core;
using Protogenesis.Organelles;
using Protogenesis.UI;

namespace Protogenesis.Online
{
    /// <summary>
    /// Modo Cooperativo en red.
    ///
    /// Funcionalidades:
    ///   - SignalingConnection: enlace entre 2 células aliadas a menos de 10u
    ///   - TransferATP: transfiere hasta 100 ATP entre aliados conectados
    ///   - SharedVision: comparte niebla de guerra con aliados conectados
    ///   - DonateOrganelle: dona un orgánulo al aliado (costo: 20% del ATP del orgánulo)
    ///   - BreachEvent: alerta + redistribución de enemigos cuando un aliado muere
    /// </summary>
    public class CoopNetwork : NetworkBehaviour
    {
        public static CoopNetwork Instance { get; private set; }

        [Header("Signaling")]
        [SerializeField] private float connectionRange = 10f;

        [Header("Transfer")]
        [SerializeField] private float maxATPTransfer = 100f;

        // ── Estado de conexiones ──────────────────────────────────────────────────
        /// <summary>ClientIds de aliados actualmente conectados a este jugador.</summary>
        private readonly HashSet<ulong> _connectedAllies = new HashSet<ulong>();

        // ── Referencia a la posición de la CAP local ──────────────────────────────
        [SerializeField] private Transform capTransform;

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this) return;
            Instance = this;
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region Signaling Connection

        /// <summary>
        /// Intenta establecer conexión de señalización con otro jugador.
        /// Solo es posible si están a menos de <see cref="connectionRange"/> unidades.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RequestConnectionServerRpc(ulong targetClientId, ServerRpcParams rpcParams = default)
        {
            ulong requesterId = rpcParams.Receive.SenderClientId;

            // Verificar distancia entre las dos CAPs
            float distance = GetDistanceBetweenClients(requesterId, targetClientId);

            if (distance > connectionRange)
            {
                ConnectionDeniedClientRpc($"Demasiado lejos para conectar (distancia: {distance:F1}u, máx: {connectionRange}u)", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { requesterId } }
                });
                return;
            }

            // Confirmar conexión en ambos clientes
            _connectedAllies.Add(targetClientId);
            ConnectionEstablishedClientRpc(requesterId, targetClientId);

            Debug.Log($"[Coop] Conexión establecida entre cliente {requesterId} y {targetClientId}");
        }

        [ClientRpc]
        private void ConnectionEstablishedClientRpc(ulong clientA, ulong clientB)
        {
            ulong localId = NetworkManager.Singleton.LocalClientId;
            if (localId == clientA || localId == clientB)
            {
                ulong allyId = localId == clientA ? clientB : clientA;
                _connectedAllies.Add(allyId);
                Debug.Log($"[Coop] Conectado con aliado {allyId}. Visión compartida activa.");
                // Activar shared vision para este aliado
                EnableSharedVision(allyId);
            }
        }

        [ClientRpc]
        private void ConnectionDeniedClientRpc(string reason, ClientRpcParams clientRpcParams = default)
        {
            Debug.LogWarning($"[Coop] Conexión rechazada: {reason}");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Transfer ATP

        /// <summary>
        /// Transfiere ATP del emisor al receptor. Máximo 100 ATP por transferencia.
        /// Solo funciona si están conectados.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void TransferATPServerRpc(float amount, ulong targetClientId, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            amount = Mathf.Clamp(amount, 0f, maxATPTransfer);

            if (!_connectedAllies.Contains(targetClientId))
            {
                TransferDeniedClientRpc("No estás conectado con ese aliado.", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            if (ResourceManager.Instance == null || !ResourceManager.Instance.CanAfford(ResourceType.ATP, amount))
            {
                TransferDeniedClientRpc($"ATP insuficiente para transferir {amount}.", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            // Descontar del emisor
            ResourceManager.Instance.ConsumeResource(ResourceType.ATP, amount);

            // Dar al receptor
            ATPReceivedClientRpc(amount, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { targetClientId } }
            });

            TransferConfirmedClientRpc(amount, targetClientId, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
            });

            Debug.Log($"[Coop] TransferATP: {amount} ATP de cliente {senderId} → {targetClientId}");
        }

        [ClientRpc]
        private void ATPReceivedClientRpc(float amount, ClientRpcParams clientRpcParams = default)
        {
            ResourceManager.Instance?.AddResource(ResourceType.ATP, amount);
            Debug.Log($"[Coop] Recibiste {amount} ATP de un aliado.");
        }

        [ClientRpc]
        private void TransferConfirmedClientRpc(float amount, ulong targetId, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log($"[Coop] Transferiste {amount} ATP al aliado {targetId}.");
        }

        [ClientRpc]
        private void TransferDeniedClientRpc(string reason, ClientRpcParams clientRpcParams = default)
        {
            Debug.LogWarning($"[Coop] Transferencia denegada: {reason}");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Shared Vision

        private void EnableSharedVision(ulong allyId)
        {
            // Activar la cámara/niebla de guerra compartida.
            // La implementación concreta depende del sistema de Fog of War del proyecto.
            // Este método es el hook para conectar con dicho sistema.
            Debug.Log($"[Coop] SharedVision habilitado con aliado {allyId}.");
        }

        /// <summary>
        /// Envía la posición visible del jugador local a sus aliados conectados.
        /// Llamar periódicamente desde Update o una coroutine.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void SyncVisionServerRpc(Vector2 visibleCenter, float visibleRadius, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            foreach (ulong allyId in _connectedAllies)
            {
                ReceiveVisionDataClientRpc(visibleCenter, visibleRadius, senderId, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { allyId } }
                });
            }
        }

        [ClientRpc]
        private void ReceiveVisionDataClientRpc(Vector2 center, float radius, ulong fromClientId, ClientRpcParams clientRpcParams = default)
        {
            // Hook para el sistema de Fog of War: revelar el área visible del aliado
            Debug.Log($"[Coop] Visión compartida recibida de aliado {fromClientId}: centro {center}, radio {radius}u");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Donate Organelle

        /// <summary>
        /// Destruye el orgánulo del donante y lo instancia en el mapa del receptor.
        /// Costo: 20% del ATP del orgánulo donado.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void DonateOrganelleServerRpc(string organelleType, ulong targetClientId, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (!_connectedAllies.Contains(targetClientId))
            {
                DonateDeniedClientRpc("No estás conectado con ese aliado.", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            float atpCost = GetOrganelleATPCost(organelleType) * 0.20f;

            if (ResourceManager.Instance == null || !ResourceManager.Instance.CanAfford(ResourceType.ATP, atpCost))
            {
                DonateDeniedClientRpc($"ATP insuficiente (costo donación: {atpCost:F0} ATP)", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            ResourceManager.Instance.ConsumeResource(ResourceType.ATP, atpCost);

            // Destruir orgánulo en el donante
            DestroyOrganelleClientRpc(organelleType, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
            });

            // Instanciar orgánulo en el receptor
            ReceiveOrganelleClientRpc(organelleType, new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { targetClientId } }
            });

            Debug.Log($"[Coop] DonateOrganelle: {organelleType} de cliente {senderId} → {targetClientId} (costo: {atpCost} ATP)");
        }

        [ClientRpc]
        private void DestroyOrganelleClientRpc(string organelleType, ClientRpcParams clientRpcParams = default)
        {
            // Destruir el primer orgánulo del tipo especificado en la escena local
            var organelles = FindObjectsByType<OrganelleBase>(FindObjectsSortMode.None);
            foreach (var org in organelles)
            {
                if (org.gameObject.name.Contains(organelleType))
                {
                    Debug.Log($"[Coop] Orgánulo {organelleType} donado y destruido localmente.");
                    Destroy(org.gameObject);
                    return;
                }
            }
        }

        [ClientRpc]
        private void ReceiveOrganelleClientRpc(string organelleType, ClientRpcParams clientRpcParams = default)
        {
            // Instanciar el orgánulo recibido en el mapa del receptor (posición cerca del núcleo)
            Debug.Log($"[Coop] Orgánulo {organelleType} recibido de aliado.");
            OrganelleBuilder.Instance?.BuildFromNetwork(organelleType, Vector2.zero);
        }

        [ClientRpc]
        private void DonateDeniedClientRpc(string reason, ClientRpcParams clientRpcParams = default)
        {
            Debug.LogWarning($"[Coop] Donación rechazada: {reason}");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Breach Event

        /// <summary>
        /// Llamar cuando la CAP de un aliado es destruida definitivamente.
        /// Notifica a los demás aliados y redistribuye los enemigos hacia ellos.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void NotifyBreachServerRpc(Vector2 breachPosition, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            // Notificar a todos los clientes conectados
            NotifyBreachClientRpc(breachPosition, senderId);

            // Los enemigos del jugador muerto ahora atacan a los demás
            RedirectEnemiesAfterBreach(breachPosition);

            Debug.Log($"[Coop] BreachEvent: aliado {senderId} caído en {breachPosition}. Enemigos redirigidos.");
        }

        [ClientRpc]
        private void NotifyBreachClientRpc(Vector2 breachPosition, ulong fallenClientId)
        {
            _connectedAllies.Remove(fallenClientId);
            Debug.LogWarning($"[Coop] ¡BRECHA! Aliado {fallenClientId} eliminado en {breachPosition}. ¡Sus enemigos vienen hacia ti!");

            // Disparar alerta en el HUD
            UI.AlertSystem.Instance?.ShowAlert(
                $"⚠ ¡BRECHA! Aliado {fallenClientId} eliminado — enemigos redirigidos",
                new Color(0.9f, 0.1f, 0.1f), UI.AlertPriority.Critical);
        }

        private void RedirectEnemiesAfterBreach(Vector2 breachPos)
        {
            // Buscar enemigos cerca de la posición de brecha y redirigirlos
            Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(breachPos, 15f);
            foreach (var col in nearbyColliders)
            {
                if (col.CompareTag("Enemy"))
                {
                    var enemy = col.GetComponent<Enemies.EnemyBase>();
                    if (enemy != null)
                    {
                        // Redirigir al aliado más cercano vivo
                        Vector2 newTarget = FindNearestAliveAllyPosition(breachPos);
                        enemy.SetNewTarget(newTarget);
                    }
                }
            }
        }

        private static Vector2 FindNearestAliveAllyPosition(Vector2 from)
        {
            // Buscar el objeto con tag "AllyCAP" más cercano
            GameObject[] caps = GameObject.FindGameObjectsWithTag("AllyCAP");
            float minDist = float.MaxValue;
            Vector2 nearest = from + Vector2.right * 10f;

            foreach (var cap in caps)
            {
                // TODO: Primordia — float d = Vector2.Distance(from, cap.transform.position);
                // TODO: Primordia — if (d < minDist) { minDist = d; nearest = cap.transform.position; }
            }
            return nearest;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Utilidades

        private float GetDistanceBetweenClients(ulong clientA, ulong clientB)
        {
            // En implementación completa, registrar posiciones de CAP por clientId
            // Por ahora retorna distancia aproximada
            return Mathf.Abs((float)(clientA - clientB)) * 5f;
        }

        private static float GetOrganelleATPCost(string organelleType) => organelleType switch
        {
            "Mitocondria" => 30f,
            "Ribosoma"    => 20f,
            "Lisosoma"    => 25f,
            _             => 15f
        };

        public bool IsConnectedWith(ulong clientId) => _connectedAllies.Contains(clientId);

        #endregion
    }
}
