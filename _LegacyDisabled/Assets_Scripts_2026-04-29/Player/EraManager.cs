using UnityEngine;
using Protogenesis.Core;

namespace Protogenesis.Player
{
    /// <summary>
    /// EraManager — stub de migración Primordia.
    /// En Protogenesis v5.2 este script aplicaba CAPStats por era.
    /// En Primordia las eras son reemplazadas por el sistema de Especialización.
    /// TODO: Primordia Fase 4.2 — reemplazar con SpecializationTracker.
    /// </summary>
    public class EraManager : MonoBehaviour
    {
        private void Start()
        {
            EventBus.OnEraChanged += OnEraChanged;
        }

        private void OnDestroy()
        {
            EventBus.OnEraChanged -= OnEraChanged;
        }

        private void OnEraChanged(int previous, int next)
        {
            Debug.Log($"[EraManager] Era changed {previous} → {next}. (Primordia stub — stats application pending)");
        }

        [ContextMenu("Debug: Advance to Next Era")]
        public void DebugAdvanceEra()
        {
            if (GameManager.Instance == null) return;
            int next = GameManager.Instance.CurrentEra + 1;
            if (next <= 4) GameManager.Instance.AdvanceEra(next);
        }
    }
}
