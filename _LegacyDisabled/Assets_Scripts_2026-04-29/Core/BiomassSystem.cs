using System.Collections.Generic;
using UnityEngine;
using Protogenesis.Core;
using Protogenesis.Progression;
using Protogenesis.Slots;
using Protogenesis.Views;

namespace Protogenesis.Core
{
    /// <summary>
    /// BiomassSystem — Límite global de complejidad orgánica (GDD v4.3, Prompt 5.2 + Addendum v4.1).
    ///
    /// Calcula currentLoad = Σ(organelos × complejidad).
    /// maxBiomass varía por dominio: 100 (Eucariota) / 85 (Procariota) + vacuola+15 + consolidado+15.
    /// 4 niveles de eficiencia por ratio. Soft/hard cap de células integrado con CellDivisionSystem.
    /// </summary>
    public class BiomassSystem : MonoBehaviour
    {
        public static BiomassSystem Instance { get; private set; }

        // ── Estado ────────────────────────────────────────────────────────────────
        public float CurrentLoad  { get; private set; }
        public float MaxBiomass   { get; private set; } = 100f;
        public float Ratio        => MaxBiomass > 0f ? CurrentLoad / MaxBiomass : 0f;

        private float _evalTimer;
        private const float EvalInterval = 0.5f;

        // ── Complejidades por keyword (Addendum v4.1) ─────────────────────────────
        private static readonly (string key, float cost)[] Complexities =
        {
            ("nucleoide",             15f),
            ("nucleo",                15f),
            ("compartimento",         15f),
            ("mitocondria",            8f),
            ("motor",                  8f),
            ("cadena",                 8f),
            ("ribosoma",               6f),
            ("maquinaria",             6f),
            ("vacuola",                5f),
            ("peroxisoma",             6f),
            ("catalasa",               6f),
            ("flagelo bacteriano",     7f),
            ("flagelo bacter",         7f),
            ("peptidoglicano",         8f),
            ("plasmido",               5f),
            ("capsula",                7f),
            ("fimbria",                6f),
            ("flagelo eucariota",     10f),
            ("lisosoma",               9f),
            ("granulo",                9f),
            ("tilacoide",             11f),
            ("celulosa",               8f),
            ("hifa",                  12f),
            ("flagelo",               10f), // fallback
        };

        // ─────────────────────────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        private void OnEnable()  => EventBus.OnOrganelleBuilt += OnOrganelleBuilt;
        private void OnDisable() => EventBus.OnOrganelleBuilt -= OnOrganelleBuilt;

        private void Update()
        {
            _evalTimer += Time.deltaTime;
            if (_evalTimer < EvalInterval) return;
            _evalTimer = 0f;
            Evaluate();
        }

        // ─────────────────────────────────────────────────────────────────────────
        private void Evaluate()
        {
            var sm = SlotManager.Instance;
            if (sm == null) return;

            float load = 0f;
            bool hasVacuola = false;

            foreach (var slot in sm.GetAllInstalled())
            {
                string id = (slot.slotId ?? slot.displayName ?? "").ToLower();
                load += GetComplexity(id);
                if (id.Contains("vacuola")) hasVacuola = true;
            }

            CurrentLoad = load;

            // Max biomass por dominio
            var domain = CellProgressionPhase.Instance?.CurrentDomain ?? CellDomain.Undefined;
            float baseMax = domain == CellDomain.Prokaryote ? 85f : 100f;
            bool consolidated = SpecializationTracker.Instance?.IsConsolidated ?? false;

            MaxBiomass = baseMax
                + (hasVacuola  ? 15f : 0f)
                + (consolidated ? 15f : 0f);

            ApplyEffects();
        }

        private void ApplyEffects()
        {
            float r   = Ratio;
            var   rm  = ResourceManager.Instance;
            var   ss  = StressSystem.Instance;
            var   me  = MetabolismEngine.Instance;

            if (r < 0.70f)
            {
                me?.SetBiomassEfficiency(1.0f);
            }
            else if (r < 0.85f)
            {
                me?.SetBiomassEfficiency(0.90f);
                rm?.Consume(ResourceType.ATP, 0.5f * EvalInterval);
            }
            else if (r < 1.0f)
            {
                me?.SetBiomassEfficiency(0.75f);
                rm?.Consume(ResourceType.ATP, 1.5f * EvalInterval);
                ss?.AddStress(3f * EvalInterval);
            }
            else
            {
                me?.SetBiomassEfficiency(0f);
                ss?.AddStress(8f * EvalInterval);
            }
        }

        private void OnOrganelleBuilt(GameObject go, string type) => _evalTimer = EvalInterval; // force eval

        // ─────────────────────────────────────────────────────────────────────────
        public float GetComplexity(string structureName)
        {
            string key = structureName.ToLower();
            // Check multi-word keys first (longest match wins)
            foreach (var (k, cost) in Complexities)
                if (key.Contains(k)) return cost;
            return 8f;
        }

        // ─────────────────────────────────────────────────────────────────────────
        #region OnGUI — debug

        private void OnGUI()
        {
            if (!Application.isEditor && !Debug.isDebugBuild) return;
            if (ViewManager.Instance != null && !ViewManager.Instance.IsInteriorActive) return;

            float r = Ratio;
            Color col = r < 0.70f ? new Color(0.3f, 0.75f, 0.3f)
                      : r < 0.85f ? new Color(1f, 0.6f, 0.1f)
                      : r < 1.0f  ? new Color(1f, 0.35f, 0.1f)
                      :              new Color(1f, 0.1f, 0.1f);

            var style = new GUIStyle(GUI.skin.label) { fontSize = 11 };
            style.normal.textColor = col;
            GUI.Label(new Rect(10f, Screen.height - 80f, 200f, 18f),
                $"Biomasa: {CurrentLoad:F0}/{MaxBiomass:F0} ({r * 100f:F0}%)", style);
        }

        #endregion
    }
}
