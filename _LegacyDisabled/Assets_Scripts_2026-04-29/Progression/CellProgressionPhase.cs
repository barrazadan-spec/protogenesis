using System;
using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Progression
{
    public enum ProgressionPhase
    {
        Protocell,      // Solo Compartimento Genético disponible (LUCA)
        BaseMetabolic,  // Motor Metabólico + Maquinaria de Síntesis
        Specializing,   // Catálogo por dominio
        Consolidated    // Todo disponible
    }

    public class CellProgressionPhase : MonoBehaviour
    {
        public static CellProgressionPhase Instance { get; private set; }

        public ProgressionPhase CurrentPhase  { get; private set; } = ProgressionPhase.Protocell;
        public CellDomain       CurrentDomain { get; private set; } = CellDomain.Undefined;

        // ── Eventos ───────────────────────────────────────────────────────────────
        public static event Action<ProgressionPhase> OnPhaseChanged;
        public static event Action<CellDomain>        OnDomainDefined;

        // ── Hint contextual ───────────────────────────────────────────────────────
        public string CurrentHint => CurrentPhase switch
        {
            ProgressionPhase.Protocell     => "Presioná G → instalá el COMPARTIMENTO GENÉTICO",
            ProgressionPhase.BaseMetabolic => "Instalá MOTOR METABÓLICO y MAQUINARIA DE SÍNTESIS",
            ProgressionPhase.Specializing  => CurrentDomain == CellDomain.Prokaryote
                                                ? "Dominio Procariota — elegí tus estructuras"
                                                : "Dominio Eucariota — elegí tus orgánulos",
            _                              => ""
        };

        private bool _hasGenetic  = false; // Compartimento Genético
        private bool _hasMotor    = false; // Motor Metabólico
        private bool _hasSynth    = false; // Maquinaria de Síntesis

        // ─────────────────────────────────────────────────────────────────────────
        #region Unity Lifecycle

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()
        {
            EventBus.OnOrganelleBuilt             += OnOrganelleBuilt;
            EventBus.OnSpecializationConsolidated += OnConsolidated;
        }

        private void OnDisable()
        {
            EventBus.OnOrganelleBuilt             -= OnOrganelleBuilt;
            EventBus.OnSpecializationConsolidated -= OnConsolidated;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Handlers

        private void OnOrganelleBuilt(GameObject go, string type)
        {
            string t = type?.ToLower() ?? "";
            if (t.Contains("compartimento") || t.Contains("nucleo") || t.Contains("nucleus") || t.Contains("nucleoide"))
                _hasGenetic = true;
            if (t.Contains("motor") || t.Contains("mitocondria") || t.Contains("cadena"))
                _hasMotor = true;
            if (t.Contains("maquinaria") || t.Contains("ribosoma"))
                _hasSynth = true;
            EvaluatePhase();
        }

        private void OnConsolidated(SpecializationType type, string displayName)
        {
            if (CurrentPhase < ProgressionPhase.Consolidated)
                AdvanceTo(ProgressionPhase.Consolidated);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Lógica de transición

        private void EvaluatePhase()
        {
            switch (CurrentPhase)
            {
                case ProgressionPhase.Protocell:
                    if (_hasGenetic)
                        AdvanceTo(ProgressionPhase.BaseMetabolic);
                    break;

                case ProgressionPhase.BaseMetabolic:
                    if (_hasMotor && _hasSynth)
                        AdvanceTo(ProgressionPhase.Specializing);
                    break;
            }
        }

        private void AdvanceTo(ProgressionPhase next)
        {
            CurrentPhase = next;
            Debug.Log($"[CellProgression] Fase: {next} — {CurrentHint}");
            OnPhaseChanged?.Invoke(next);
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region Dominio biológico

        /// <summary>
        /// Llamar desde GeneticRingSystem al elegir el gen del Anillo 1.
        /// geneIndex: 0=Respiración aeróbica, 1=Fotosíntesis, 2=Fermentación, 3=Quimiolitotrofía
        /// </summary>
        public void SetDomainFromRing1Gene(int geneIndex)
        {
            if (CurrentDomain != CellDomain.Undefined) return; // no-op si ya está definido

            CellDomain domain = geneIndex <= 1 ? CellDomain.Eukaryote : CellDomain.Prokaryote;
            CurrentDomain = domain;
            OnDomainDefined?.Invoke(domain);
            Debug.Log($"[CellProgression] Dominio: {domain}");
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────
        #region API pública — catálogo filtrado

        /// <summary>
        /// Retorna las keywords de estructuras permitidas en la fase y dominio actuales.
        /// Lista vacía = todos permitidos (Consolidated).
        /// Llamado por OrganelleInstaller para filtrar el catálogo visible.
        /// </summary>
        public List<string> GetAllowedOrganelles()
        {
            switch (CurrentPhase)
            {
                case ProgressionPhase.Protocell:
                    return new List<string> { "compartimento", "nucleo", "nucleus" };

                case ProgressionPhase.BaseMetabolic:
                    return new List<string> { "motor", "mitocondria", "maquinaria", "ribosoma", "cadena" };

                case ProgressionPhase.Specializing:
                    if (CurrentDomain == CellDomain.Prokaryote)
                        return new List<string>
                        {
                            "flagelo bacteriano", "peptidoglicano", "plasmido",
                            "capsula", "fimbrias", "tilacoide", "vacuola", "catalasa", "peroxisoma"
                        };
                    else
                        return new List<string>
                        {
                            "flagelo eucariota", "lisosoma", "vacuola",
                            "granulo", "tilacoide", "celulosa", "hifa", "peroxisoma"
                        };

                default: // Consolidated
                    return new List<string>();
            }
        }

        /// <summary>True si el organelleId está permitido en la fase/dominio actuales.</summary>
        public bool IsOrganelleAllowed(string organelleId)
        {
            var allowed = GetAllowedOrganelles();
            if (allowed.Count == 0) return true;
            string id = organelleId?.ToLower() ?? "";
            foreach (var kw in allowed)
                if (id.Contains(kw)) return true;
            return false;
        }

        #endregion
    }
}
