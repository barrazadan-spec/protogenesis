using UnityEngine;
using Protogenesis.Progression;

namespace Protogenesis.Core
{
    /// <summary>
    /// LegacySystem — Prompt 4.4 / GDD v3 Primordia
    ///
    /// Adaptación del sistema de Legado para el mini-juego 1v1 de Primordia.
    /// En Primordia no hay colonias ni eras: el Legado es la MEMORIA BIOLÓGICA
    /// de la especialización anterior que la célula porta al consolidar una nueva.
    ///
    /// TRIGGER (v3):
    ///   EventBus.OnSpecializationConsolidated — al consolidar un nuevo tipo,
    ///   el tipo anterior (si existía) queda registrado como Legado.
    ///   Solo puede existir 1 capa de Legado simultáneamente (célula única).
    ///
    /// EFICIENCIA:
    ///   · Se calcula por afinidad biológica entre el tipo Legado y el tipo activo.
    ///   · Grupos de afinidad (misma familia → mayor eficiencia):
    ///       Prokariota temprana:  Bacteria, Arquea, Cianobacteria
    ///       Eucariota micro:      Hongo, Ameba, Flagelado, Ciliado
    ///       Inmune:               Neutrofilo, Macrofago, CelulaNK, CelulaB
    ///       Flex:                 CelulaMadre
    ///   · Mismo grupo: eficiencia 0.70.
    ///   · Distinto grupo: eficiencia 0.35.
    ///   · Mismo tipo exacto (raro, por cambio y vuelta): eficiencia 1.00.
    ///
    /// BONUS PASIVO:
    ///   · Mientras hay Legado activo, el sistema añade ATP pasivo por segundo
    ///     = legacyATPRate × eficiencia.
    ///   · Coste de inestabilidad mantenido por compatibilidad (reducido en v3).
    ///
    /// DISOLUCIÓN:
    ///   · Dissolve() devuelve AminoAcids a ResourceManager (proteínas recicladas).
    ///   · CollapseOnDeath() elimina el Legado sin devolución (pérdida total).
    ///
    /// MUERTE (Prompt 4.3):
    ///   · DeathSystem llama CollapseOnDeath() al ejecutar la regresión.
    /// </summary>
    public class LegacySystem : MonoBehaviour
    {
        public static LegacySystem Instance { get; private set; }

        // ─── Config ───────────────────────────────────────────────────────────────
        [Header("Bonus pasivo")]
        [Tooltip("ATP por segundo que aporta el Legado (multiplicado por eficiencia).")]
        [SerializeField] private float legacyATPRate         = 2.5f;
        [Tooltip("Inestabilidad por segundo mientras el Legado está activo.")]
        [SerializeField] private float instabilityPerSecond  = 0.04f;

        [Header("Disolución")]
        [Tooltip("AminoAcids devueltos al disolver el Legado.")]
        [SerializeField] private float dissolveAminoReturn   = 15f;

        // ─── Estado ───────────────────────────────────────────────────────────────
        private SpecializationType _legacyType    = SpecializationType.None;
        private SpecializationType _currentType   = SpecializationType.None;
        private bool               _hasLegacy     = false;

        // ─── Propiedades públicas ─────────────────────────────────────────────────
        public bool               HasLegacy    => _hasLegacy;
        public SpecializationType LegacyType   => _legacyType;
        public float              Efficiency   => _hasLegacy
                                                  ? CalculateEfficiency(_legacyType, _currentType)
                                                  : 0f;

        // ─── Tabla de grupos de afinidad biológica ────────────────────────────────
        private static int AffinityGroup(SpecializationType t) => t switch
        {
            SpecializationType.Bacteria      => 0,   // Prokariota temprana
            SpecializationType.Arquea        => 0,
            SpecializationType.Cianobacteria => 0,

            SpecializationType.Hongo         => 1,   // Eucariota micro
            // TODO v4.2 — Microalga eliminada como especialización (ahora unidad Premium)
            SpecializationType.Ameba         => 1,
            SpecializationType.Flagelado     => 1,
            SpecializationType.Ciliado       => 1,

            SpecializationType.Neutrofilo    => 2,   // Inmune
            SpecializationType.Macrofago     => 2,
            SpecializationType.CelulaNK      => 2,
            SpecializationType.CelulaB       => 2,

            SpecializationType.CelulaMadre   => 3,   // Flex
            // TODO v4.2 — Tardigrado/Volvox/Paramecio eliminados como especializaciones (ahora unidades Premium)

            _                                => -1,  // None / desconocido
        };

        private static float CalculateEfficiency(SpecializationType legacy,
                                                  SpecializationType current)
        {
            if (legacy == SpecializationType.None || current == SpecializationType.None)
                return 0f;
            if (legacy == current)                                    return 1.00f;
            if (AffinityGroup(legacy) == AffinityGroup(current))      return 0.70f;
            return 0.35f;
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnSpecializationConsolidated += OnConsolidated;
        }

        private void OnDisable()
        {
            EventBus.OnSpecializationConsolidated -= OnConsolidated;
        }

        private void Update()
        {
            if (!_hasLegacy) return;
            if (GameManager.Instance != null &&
               (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused)) return;

            float dt  = Time.deltaTime;
            float eff = Efficiency;

            // Bonus pasivo de ATP
            float atpBonus = legacyATPRate * eff * dt;
            ResourceManager.Instance?.Produce(ResourceType.ATP, atpBonus);

            // Coste de inestabilidad (reducido respecto a v5.2 — célula única)
            InstabilitySystem.Instance?.AddInstability(instabilityPerSecond * dt);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Handlers de evento

        private void OnConsolidated(SpecializationType newType, string displayName)
        {
            if (newType == SpecializationType.None) return;

            // El tipo activo anterior pasa a ser Legado
            if (_currentType != SpecializationType.None)
            {
                _legacyType = _currentType;
                _hasLegacy  = true;

                float eff = CalculateEfficiency(_legacyType, newType);
                Debug.Log($"[LegacySystem] Legado registrado: {_legacyType} → {newType} " +
                          $"| Eficiencia: {eff:P0} " +
                          $"| Grupo {AffinityGroup(_legacyType)} → {AffinityGroup(newType)}");

                EventBus.TriggerEcosystemEvent("legacy_activated",
                    $"Legado {_legacyType} activo. Eficiencia {eff:P0}.");
            }

            _currentType = newType;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública

        /// <summary>
        /// Disuelve el Legado voluntariamente.
        /// Devuelve AminoAcids × eficiencia a ResourceManager.
        /// </summary>
        public void Dissolve()
        {
            if (!_hasLegacy) return;

            float returned = dissolveAminoReturn * Efficiency;
            ResourceManager.Instance?.Produce(ResourceType.AminoAcids, returned);

            Debug.Log($"[LegacySystem] Legado {_legacyType} disuelto. " +
                      $"+{returned:F1} AminoAcids devueltos.");

            EventBus.TriggerEcosystemEvent("legacy_dissolved",
                $"Legado {_legacyType} disuelto. +{returned:F1} AminoAcids.");

            ClearLegacy();
        }

        /// <summary>
        /// Colapsa el Legado por muerte de la célula (sin devolución de recursos).
        /// Llamado por DeathSystem durante la regresión (Prompt 4.3).
        /// </summary>
        public void CollapseOnDeath()
        {
            if (!_hasLegacy) return;

            Debug.Log($"[LegacySystem] Legado {_legacyType} colapsado por muerte celular.");
            ClearLegacy();
        }

        /// <summary>
        /// Nombre de display del tipo de Legado actual (para HUDs).
        /// </summary>
        public string LegacyDisplayName => _hasLegacy
            ? _legacyType.ToString()
            : "—";

        /// <summary>
        /// Descripción del bonus activo para tooltips.
        /// </summary>
        public string BonusDescription => _hasLegacy
            ? $"+{legacyATPRate * Efficiency:F1} ATP/s ({LegacyType} · {Efficiency:P0})"
            : "Sin Legado activo";

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Compatibilidad con DeathSystem (Prompt 4.3)

        /// <summary>Alias mantenido por compatibilidad con llamadas del DeathSystem.</summary>
        public void CollapseOnCAPDeath() => CollapseOnDeath();

        // ─────────────────────────────────────────────────────────────────────────
        #region Compatibilidad con SlotInstallUI (legacy API de v5)

        /// <summary>
        /// Registra el estado de la célula antes de avanzar de era (stub — en Primordia no hay colonia).
        /// </summary>
        public void SnapshotCurrentColony(int era)
        {
            // Primordia v3: célula única, no hay colonia que registrar.
            Debug.Log($"[LegacySystem] SnapshotCurrentColony era={era} (no-op en Primordia).");
        }

        /// <summary>
        /// Número de "células legado" activas.
        /// En Primordia solo existe una célula, por lo que devuelve 0 ó 1.
        /// </summary>
        public int TotalLegacyCells => _hasLegacy ? 1 : 0;

        /// <summary>
        /// Factor de eficiencia del Legado para la era indicada (parámetro ignorado en v3).
        /// Rango [0,1].
        /// </summary>
        public float GetEfficiencyFactor(int era) => Efficiency;

        #endregion

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Helpers

        private void ClearLegacy()
        {
            _legacyType = SpecializationType.None;
            _hasLegacy  = false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Debug

        [ContextMenu("Debug: Estado del Legado")]
        private void DebugState()
        {
            if (!_hasLegacy)
                Debug.Log("[LegacySystem] Sin Legado activo.");
            else
                Debug.Log($"[LegacySystem] Legado: {_legacyType} " +
                          $"| Actual: {_currentType} " +
                          $"| Eficiencia: {Efficiency:P0} " +
                          $"| {BonusDescription}");
        }

        #endregion
    }
}
