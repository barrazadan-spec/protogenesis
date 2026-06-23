using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Protogenesis.Core;
using Protogenesis.Player;
using Protogenesis.Units;

namespace Protogenesis.Online
{
    /// <summary>
    /// Sistema de sabotaje PvP biológicamente canónico.
    ///
    /// Los 4 sabotajes disponibles:
    ///   a) IngenieriaPlasimido  — convierte unidad aliada en agente infiltrado (costo: 15 proteínas, requiere ERA3)
    ///   b) RedirigirPatogeno    — redirige patógeno neutral al territorio rival (costo: 8 ATP)
    ///   c) ShockOsmotico        — ArchaeaHalo cruza territorio y activa choque osmótico (costo: 7 ATP + 1 ArchaeaHalo)
    ///   d) CascadaCitoquinas    — tormenta de citoquinas en territorio rival (costo: 20 ATP)
    ///
    /// REGLA: toda validación y modificación de estado corre en el servidor.
    /// Los clientes solo envían requests via ServerRpc.
    /// </summary>
    public class PvPSabotageSystem : NetworkBehaviour
    {
        public static PvPSabotageSystem Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject cytokineStormPrefab;

        [Header("Config — Osmotico")]
        [SerializeField] private float osmoticShockRadius   = 4f;
        [SerializeField] private float archaeaHaloLifetime  = 15f;

        [Header("Config — Citoquinas")]
        [SerializeField] private float cytokineRadius       = 5f;
        [SerializeField] private float cytokineDamagePerSec = 8f;
        [SerializeField] private float cytokineDuration     = 15f;

        public override void OnNetworkSpawn()
        {
            if (Instance != null && Instance != this) return;
            Instance = this;
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region a) Ingeniería Plásmido

        /// <summary>
        /// Costo: 15 proteínas. Requiere nodo ERA3 desbloqueado.
        /// Convierte una unidad ProcarioteBasic aliada aleatoria en agente infiltrado.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void IngenieriaPlasimidoServerRpc(ulong rivalClientId, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (ResourceManager.Instance == null) return;

            // Verificar proteínas y era
            bool hasProteins = ResourceManager.Instance.CanAfford(ResourceType.Biomass, 15f);
            bool hasEra3     = GameManager.Instance != null && GameManager.Instance.CurrentEra >= 3;

            if (!hasProteins || !hasEra3)
            {
                string reason = !hasProteins ? "Proteínas insuficientes (costo: 15)" : "Requiere ERA III desbloqueada";
                SabotageDeniedClientRpc(reason, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            ResourceManager.Instance.ConsumeResource(ResourceType.Biomass, 15f);

            // Buscar una unidad ProcaryoteBasic aliada aleatoria
            var allies = GameObject.FindGameObjectsWithTag("AllyUnit");
            GameObject target = null;

            foreach (var ally in allies)
            {
                if (ally.name.Contains("ProcaryoteBasic"))
                {
                    target = ally;
                    break;
                }
            }

            if (target == null)
            {
                SabotageDeniedClientRpc("No hay ProcaryoteBasic aliado disponible.", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            // Convertir en agente infiltrado
            target.tag = "NeutralUnit";
            target.name = "InfiltrationAgent";

            // Obtener posición del núcleo rival (aproximada)
            Vector2 rivalNucleus = GetRivalNucleusPosition(rivalClientId);

            var unit = target.GetComponent<UnitBase>();
            if (unit != null)
                StartCoroutine(InfiltrationRoutine(target, rivalNucleus, 30f));

            SabotageConfirmedClientRpc("Ingeniería Plásmido activada: agente infiltrado enviado.", new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
            });

            Debug.Log($"[PvP] IngenieriaPlasimido: {target.name} convertido en agente infiltrado.");
        }

        private IEnumerator InfiltrationRoutine(GameObject agent, Vector2 rivalTarget, float attackDuration)
        {
            float elapsed = 0f;
            var rb = agent.GetComponent<Rigidbody2D>();

            // Mover hacia el núcleo rival
            while (agent != null && Vector2.Distance(agent.transform.position, rivalTarget) > 1.5f)
            {
                if (rb != null)
                {
                    Vector2 dir = (rivalTarget - (Vector2)agent.transform.position).normalized;
                    rb.MovePosition(rb.position + dir * 3f * Time.deltaTime);
                }
                yield return null;
            }

            // Atacar durante attackDuration segundos
            while (agent != null && elapsed < attackDuration)
            {
                // El agente ataca estructuras rivales en el área
                Collider2D[] hits = Physics2D.OverlapCircleAll(agent.transform.position, 1.5f);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("RivalOrganelle"))
                        hit.GetComponent<IDamageable>()?.TakeDamage(5f * Time.deltaTime);
                }
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Devolver al pool
            if (agent != null) agent.tag = "AllyUnit";
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region b) Redirigir Patógeno

        /// <summary>
        /// Costo: 8 ATP. Requiere CelulaDendritica en campo.
        /// Redirige el patógeno neutral más cercano al territorio rival.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void RedirigirPatogenoServerRpc(ulong rivalClientId, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (ResourceManager.Instance == null) return;

            bool hasATP          = ResourceManager.Instance.CanAfford(ResourceType.ATP, 8f);
            bool hasDendritica   = GameObject.FindGameObjectWithTag("CelulaDendritica") != null;

            if (!hasATP || !hasDendritica)
            {
                string reason = !hasATP
                    ? "ATP insuficiente (costo: 8)"
                    : "Requiere Célula Dendrítica en campo";
                SabotageDeniedClientRpc(reason, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            ResourceManager.Instance.ConsumeResource(ResourceType.ATP, 8f);

            // Buscar el patógeno neutral más cercano
            GameObject[] neutralEnemies = GameObject.FindGameObjectsWithTag("NeutralEnemy");
            if (neutralEnemies.Length == 0)
            {
                SabotageDeniedClientRpc("No hay patógenos neutrales en el campo.", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            GameObject nearest  = null;
            float minDist       = float.MaxValue;
            Vector2 myPos       = Vector2.zero; // Posición del host como referencia

            foreach (var enemy in neutralEnemies)
            {
                float d = Vector2.Distance(enemy.transform.position, myPos);
                if (d < minDist) { minDist = d; nearest = enemy; }
            }

            if (nearest != null)
            {
                Vector2 rivalPos = GetRivalNucleusPosition(rivalClientId);
                // SetNewTarget es un método en EnemyBase
                var enemyBase = nearest.GetComponent<Enemies.EnemyBase>();
                enemyBase?.SetNewTarget(rivalPos);

                SabotageConfirmedClientRpc("Patógeno redirigido al territorio rival.", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                Debug.Log($"[PvP] RedirigirPatogeno: {nearest.name} redirigido a {rivalPos}");
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region c) Shock Osmótico

        /// <summary>
        /// Costo: 7 ATP + consume 1 ArchaeaHalo aliada.
        /// La ArchaeaHalo cruza al territorio rival y activa OsmoticShock en radio 4u.
        /// La ArchaeaHalo muere después de 15 segundos.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void ShockOsmoticServerRpc(ulong rivalClientId, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (ResourceManager.Instance == null) return;

            bool hasATP      = ResourceManager.Instance.CanAfford(ResourceType.ATP, 7f);
            GameObject halo  = FindArchaeaHalo();

            if (!hasATP || halo == null)
            {
                string reason = !hasATP ? "ATP insuficiente (costo: 7)" : "Requiere ArchaeaHalo aliada en campo";
                SabotageDeniedClientRpc(reason, new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            ResourceManager.Instance.ConsumeResource(ResourceType.ATP, 7f);

            Vector2 rivalPos = GetRivalNucleusPosition(rivalClientId);
            StartCoroutine(OsmoticShockRoutine(halo, rivalPos));

            SabotageConfirmedClientRpc("Shock Osmótico activado. ArchaeaHalo en camino.", new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
            });
        }

        private IEnumerator OsmoticShockRoutine(GameObject archaeaHalo, Vector2 rivalPos)
        {
            // Mover hacia el territorio rival
            var rb = archaeaHalo.GetComponent<Rigidbody2D>();
            while (archaeaHalo != null && Vector2.Distance(archaeaHalo.transform.position, rivalPos) > 1f)
            {
                if (rb != null)
                {
                    Vector2 dir = (rivalPos - (Vector2)archaeaHalo.transform.position).normalized;
                    rb.MovePosition(rb.position + dir * 3.5f * Time.deltaTime);
                }
                yield return null;
            }

            // Activar Osmotico en radio 4u — dañar unidades rivales
            if (archaeaHalo != null)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(archaeaHalo.transform.position, osmoticShockRadius);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("RivalUnit") || hit.CompareTag("RivalOrganelle"))
                    {
                        var damageable = hit.GetComponent<IDamageable>();
                        damageable?.TakeDamage(20f);

                        var stunnable = hit.GetComponent<IStunnable>();
                        stunnable?.Stun(3f);
                    }
                }

                Debug.Log($"[PvP] ShockOsmotico activado en {archaeaHalo.transform.position}. Dañando en radio {osmoticShockRadius}u.");
            }

            // Esperar y destruir la ArchaeaHalo
            yield return new WaitForSeconds(archaeaHaloLifetime);

            if (archaeaHalo != null)
            {
                UnitSpawner.Instance?.ReturnToPool(archaeaHalo.GetComponent<UnitBase>());
                Debug.Log("[PvP] ArchaeaHalo murió tras Shock Osmótico.");
            }
        }

        private static GameObject FindArchaeaHalo()
        {
            GameObject[] allies = GameObject.FindGameObjectsWithTag("AllyUnit");
            foreach (var ally in allies)
            {
                if (ally.name.Contains("ArchaeaHalo")) return ally;
            }
            return null;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region d) Cascada Citoquinas

        /// <summary>
        /// Costo: 20 ATP.
        /// Instancia CytokineStorm en el territorio rival. Hace 8 daño/seg
        /// a toda unidad y orgánulo rival en radio 5u durante 15 seg.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void CascadaCitoquinasServerRpc(ulong rivalClientId, ServerRpcParams rpcParams = default)
        {
            ulong senderId = rpcParams.Receive.SenderClientId;

            if (ResourceManager.Instance == null) return;

            if (!ResourceManager.Instance.CanAfford(ResourceType.ATP, 20f))
            {
                SabotageDeniedClientRpc("ATP insuficiente (costo: 20)", new ClientRpcParams
                {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
                });
                return;
            }

            ResourceManager.Instance.ConsumeResource(ResourceType.ATP, 20f);

            Vector2 rivalPos = GetRivalNucleusPosition(rivalClientId);

            // Instanciar CytokineStorm en el cliente rival
            CytokineStormClientRpc(rivalPos, rivalClientId);

            SabotageConfirmedClientRpc("Cascada de Citoquinas lanzada al territorio rival.", new ClientRpcParams
            {
                Send = new ClientRpcSendParams { TargetClientIds = new[] { senderId } }
            });

            Debug.Log($"[PvP] CascadaCitoquinas enviada a cliente {rivalClientId} en posición {rivalPos}");
        }

        [ClientRpc]
        private void CytokineStormClientRpc(Vector2 position, ulong targetClientId)
        {
            // Solo el cliente objetivo procesa el daño
            if (NetworkManager.Singleton.LocalClientId != targetClientId) return;

            if (cytokineStormPrefab != null)
            {
                var storm = Instantiate(cytokineStormPrefab, position, Quaternion.identity);
                StartCoroutine(CytokineStormRoutine(storm, position));
            }
            else
            {
                // Sin prefab: aplicar daño directo
                StartCoroutine(CytokineStormRoutine(null, position));
            }
        }

        private IEnumerator CytokineStormRoutine(GameObject stormObj, Vector2 center)
        {
            float elapsed = 0f;
            Debug.Log($"[PvP] Tormenta de Citoquinas activa en {center} — {cytokineDamagePerSec} daño/seg durante {cytokineDuration}s");

            while (elapsed < cytokineDuration)
            {
                Collider2D[] hits = Physics2D.OverlapCircleAll(center, cytokineRadius);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("AllyUnit") || hit.CompareTag("AllyOrganelle"))
                        hit.GetComponent<IDamageable>()?.TakeDamage(cytokineDamagePerSec * Time.deltaTime);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            if (stormObj != null) Destroy(stormObj);
            Debug.Log("[PvP] Tormenta de Citoquinas expirada.");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region ClientRpc — Feedback genérico

        [ClientRpc]
        private void SabotageConfirmedClientRpc(string message, ClientRpcParams clientRpcParams = default)
        {
            Debug.Log($"[PvP] {message}");
            NetworkGameManager.Instance?.ShowSabotageResult(message, true);
        }

        [ClientRpc]
        private void SabotageDeniedClientRpc(string reason, ClientRpcParams clientRpcParams = default)
        {
            Debug.LogWarning($"[PvP] Sabotaje rechazado: {reason}");
            NetworkGameManager.Instance?.ShowSabotageResult($"Sabotaje fallido: {reason}", false);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Utilidades

        /// <summary>Devuelve una posición aproximada del núcleo del rival (placeholder — conectar con el sistema de celdas).</summary>
        private static Vector2 GetRivalNucleusPosition(ulong rivalClientId)
        {
            // En implementación completa, esto consultaría el registro de posiciones de CAP por cliente.
            // Por ahora usamos un offset predeterminado para testing.
            return new Vector2(rivalClientId % 2 == 0 ? 20f : -20f, 0f);
        }

        #endregion
    }
}
