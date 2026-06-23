using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.Ecosystem
{
    /// <summary>
    /// EcosystemManager — stub de migración Primordia.
    /// En Protogenesis v5.2 simulaba Lotka-Volterra con EcosystemPopulation.
    /// En Primordia el ecosistema es reemplazado por ExteriorMap + ResourceNodes.
    /// TODO: Primordia Fase 3.3 — reemplazar con ResourceNodeSpawner.
    /// </summary>
    public class EcosystemManager : MonoBehaviour
    {
        public static EcosystemManager Instance { get; private set; }

        private readonly Dictionary<string, float> _populations = new Dictionary<string, float>();

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(this); return; }
            Instance = this;
        }

        /// <summary>Devuelve la población actual de una especie por su ID (stub — siempre 0 en Primordia).</summary>
        public float GetPopulation(string speciesId)
        {
            _populations.TryGetValue(speciesId, out float val);
            return val;
        }

        /// <summary>Aplica un impacto a la población (stub en Primordia).</summary>
        public void ImpactPopulation(string speciesId, float delta)
        {
            if (!_populations.ContainsKey(speciesId)) _populations[speciesId] = 0f;
            _populations[speciesId] = Mathf.Max(0f, _populations[speciesId] + delta);
        }

        /// <summary>No-op en Primordia — EcosystemPopulation fue deprecado.</summary>
        public void RegisterPopulations(object[] pops)
        {
            // TODO: Primordia — sistema de poblaciones deprecado. Ver ExteriorMap + ResourceNodeSpawner.
        }
    }
}
