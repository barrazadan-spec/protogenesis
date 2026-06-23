using System.Collections;
using UnityEngine;
using Protogenesis.Slots;
using Protogenesis.Player;

namespace Protogenesis.Core
{
    /// <summary>
    /// MetabolismEngine — Evalúa rutas metabólicas cada 0.25 s (GDD v4.6 §Fase 1).
    ///
    /// Lee los slots instalados (SlotManager) y las condiciones de zona
    /// (EnvironmentManager) para determinar qué ruta metabólica está activa
    /// y aplicar sus modificadores al ResourceManager.
    ///
    /// Rutas disponibles:
    ///   Respiracion     — O2 >= umbral, slot Energia activo → ATP x1.0 (base)
    ///   Fermentacion    — O2 bajo umbral → ATP x0.4, genera Biomasa x0.5
    ///   Fotosintesis    — Fenotipo MicroalgaFototrofica + O2 disponible → +O2, +Glucosa
    ///   Quimiolitotrofia— Nitrógeno disponible + sin O2 → ATP x0.7, consume Nitrogen
    ///   Mixotrofia      — Energia + Alimentacion slots → ATP x1.15, consume Glucosa
    /// </summary>
    public class MetabolismEngine : MonoBehaviour
    {
        public static MetabolismEngine Instance { get; private set; }

        [Header("Intervalos")]
        [Tooltip("Segundos entre evaluaciones metabólicas.")]
        [SerializeField] private float evalInterval = 0.25f;

        [Header("Umbrales")]
        [SerializeField] private float o2FermentationThreshold  = 0.20f;  // O2 normalizado
        [SerializeField] private float glucoseMixoConsume       = 0.5f;   // Glucosa/tick mixotrofia
        [SerializeField] private float nitrogenLithoConsume     = 0.3f;   // Nitrógeno/tick quimiolito

        // ── Ruta activa ───────────────────────────────────────────────────────────
        public string ActiveRoute { get; private set; } = "Respiracion";

        // ── Modificadores de producción aplicados ─────────────────────────────────
        /// <summary>Multiplicador de producción de ATP de la ruta activa.</summary>
        public float ATPRouteMultiplier { get; private set; } = 1.0f;

        private float _biomassEfficiency = 1.0f;

        /// <summary>Llamado por BiomassSystem para degradar la eficiencia metabólica.</summary>
        public void SetBiomassEfficiency(float efficiency)
        {
            _biomassEfficiency = Mathf.Clamp01(efficiency);
            var rm = ResourceManager.Instance;
            if (rm != null)
                rm.SetProductionMultiplier(ResourceType.ATP,
                    ATPRouteMultiplier * _biomassEfficiency);
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void Start()
        {
            StartCoroutine(MetabolismLoop());
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Loop principal

        private IEnumerator MetabolismLoop()
        {
            var wait = new WaitForSeconds(evalInterval);
            while (true)
            {
                yield return wait;

                if (GameManager.Instance != null &&
                   (GameManager.Instance.IsGameOver || GameManager.Instance.IsPaused))
                    continue;

                EvaluateRoute();
            }
        }

        private void EvaluateRoute()
        {
            var rm  = ResourceManager.Instance;
            var env = EnvironmentManager.Instance;
            var sm  = SlotManager.Instance;
            // TODO: Primordia — var ps  = PhenotypeSystem.Instance;
            object ps = null; // Primordia migration stub

            if (rm == null) return;

            float o2Normalized = env != null
                ? env.CurrentO2   // ya viene en 0-1 desde EnvironmentManager
                : 1.0f;

            bool hasEnergiaSlot    = sm != null && sm.GetUsedSlots(SlotType.Energia)    > 0;
            bool hasAlimSlot       = sm != null && sm.GetUsedSlots(SlotType.Alimentacion) > 0;
            bool isFotoFenotipo    = false; // Primordia stub
            bool lowO2             = o2Normalized < o2FermentationThreshold;
            float nitrogen         = rm.GetResource(ResourceType.Nitrogen);
            float glucose          = rm.GetResource(ResourceType.Glucose);

            string newRoute;
            float  newATPMult;

            if (isFotoFenotipo && !lowO2)
            {
                // Fotosíntesis: produce O2 y Glucosa
                newRoute   = "Fotosintesis";
                newATPMult = 0.8f + Progression.GeneticFlags.PhotoATPConversionBonus;
                rm.AddResource(ResourceType.O2,      (1.5f + Progression.GeneticFlags.PhotosynthesisO2Bonus) * evalInterval);
                rm.AddResource(ResourceType.Glucose, 1.0f * evalInterval);
            }
            else if (!lowO2 && hasEnergiaSlot && hasAlimSlot && glucose > 1f)
            {
                // Mixotrofia: combina luz y heterotrofia
                newRoute   = "Mixotrofia";
                newATPMult = 1.15f;
                float glucoseConsume = glucoseMixoConsume * (1f - Progression.GeneticFlags.MixotrophyGlucoseDiscount);
                rm.ConsumeResource(ResourceType.Glucose, glucoseConsume * evalInterval);
            }
            else if (lowO2 && nitrogen > 1f && hasEnergiaSlot)
            {
                // Quimiolitotrofia: oxida compuestos inorgánicos (Nitrógeno)
                newRoute   = "Quimiolitotrofia";
                newATPMult = 0.7f;
                float nitrogenConsume = nitrogenLithoConsume * (1f - Progression.GeneticFlags.LithotrophyNitrogenDiscount);
                rm.ConsumeResource(ResourceType.Nitrogen, nitrogenConsume * evalInterval);
            }
            else if (lowO2)
            {
                // Fermentación: sin O2, ineficiente
                newRoute   = "Fermentacion";
                newATPMult = 0.4f;
                rm.AddResource(ResourceType.Biomass, (0.5f + Progression.GeneticFlags.FermentationBiomassBonus) * evalInterval);
            }
            else
            {
                // Respiración aerobia estándar
                newRoute   = "Respiracion";
                newATPMult = 1.0f;
            }

            // Aplicar multiplicador al ResourceManager (era el eslabón que faltaba)
            rm.SetProductionMultiplier(ResourceType.ATP, newATPMult);
            ATPRouteMultiplier = newATPMult;

            if (newRoute != ActiveRoute)
            {
                ActiveRoute = newRoute;
                Debug.Log($"[MetabolismEngine] Ruta → {newRoute} (ATPx{newATPMult})");
                EventBus.TriggerMetabolicRouteChanged(newRoute);
            }
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API

        /// <summary>True si la célula está en modo anaerobio (fermentación o quimiolito).</summary>
        public bool IsAnaerobic => ActiveRoute == "Fermentacion" || ActiveRoute == "Quimiolitotrofia";

        #endregion
    }
}
