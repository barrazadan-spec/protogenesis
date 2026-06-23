using UnityEngine;

namespace Protogenesis.V5
{
    public class V5LiveCoachSystem : MonoBehaviour, IV5RunResettable
    {
        public bool Enabled = true;
        public int NotificationsShown { get; private set; }
        public string LastCoachMessage { get; private set; } = "Sin intervencion.";
        public string LastCoachAction { get; private set; } = "Sin accion.";
        public string LastStatus { get; private set; } = "Sin diagnostico.";
        public V5AdaptationId LastSuggestedAdaptation { get; private set; } = V5AdaptationId.None;
        public string LastSuggestedAdaptationLabel { get; private set; } = "Sin adaptacion sugerida.";
        public string LastSuggestedAdaptationStatus { get; private set; } = "Sin estado.";
        public string Summary { get; private set; } = "Coach vivo esperando senales.";

        private float nextScan;
        private float lastToastTime = -999f;
        private int lastWarningCount;
        private int lastSeverityBand;
        private string lastActionKey = "";

        public void ResetForNewRun()
        {
            NotificationsShown = 0;
            LastCoachMessage = "Sin intervencion.";
            LastCoachAction = "Sin accion.";
            LastStatus = "Sin diagnostico.";
            LastSuggestedAdaptation = V5AdaptationId.None;
            LastSuggestedAdaptationLabel = "Sin adaptacion sugerida.";
            LastSuggestedAdaptationStatus = "Sin estado.";
            Summary = "Coach vivo esperando senales.";
            nextScan = 0f;
            lastToastTime = -999f;
            lastWarningCount = 0;
            lastSeverityBand = 0;
            lastActionKey = "";
        }

        private void Update()
        {
            if (!Enabled) return;
            if (Time.unscaledTime < nextScan) return;
            nextScan = Time.unscaledTime + 3f;
            EvaluateNow(false);
        }

        public bool EvaluateNow(bool forceToast)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Hud == null || gm.Diagnostics == null || gm.Advisor == null) return false;
            if (gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return false;

            gm.Diagnostics.Scan();
            gm.Advisor.Recalculate();

            if (!gm.Advisor.UsingDiagnosticAdvice)
            {
                LastStatus = gm.Diagnostics.ShortStatus;
                Summary = "Coach vivo: sin alerta accionable | " + LastStatus;
                return false;
            }

            int severity = SeverityBand(gm.Diagnostics.Score, gm.Diagnostics.WarningCount);
            if (severity <= 0)
            {
                LastStatus = gm.Diagnostics.ShortStatus;
                Summary = "Coach vivo: diagnostico leve | " + LastStatus;
                return false;
            }

            LastCoachMessage = gm.Diagnostics.CoachAdvice;
            LastCoachAction = gm.Diagnostics.CoachAction;
            LastStatus = gm.Diagnostics.ShortStatus;
            LastSuggestedAdaptation = gm.Diagnostics.CoachAdaptation;
            LastSuggestedAdaptationLabel = gm.Diagnostics.CoachAdaptationLabel;
            LastSuggestedAdaptationStatus = gm.Diagnostics.CoachAdaptationStatus;
            Summary = "Coach vivo: " + LastStatus + " | " + LastCoachAction;
            if (LastSuggestedAdaptation != V5AdaptationId.None)
                Summary += " | Genoma: " + LastSuggestedAdaptationLabel;

            bool actionChanged = LastCoachAction != lastActionKey;
            bool severityRose = severity > lastSeverityBand;
            bool warningsRose = gm.Diagnostics.WarningCount > lastWarningCount;
            bool cooldownReady = Time.unscaledTime - lastToastTime >= 35f;
            bool shouldToast = forceToast || (cooldownReady && (actionChanged || severityRose || warningsRose));

            lastSeverityBand = severity;
            lastWarningCount = gm.Diagnostics.WarningCount;
            lastActionKey = LastCoachAction;

            if (!shouldToast) return false;

            NotificationsShown++;
            lastToastTime = Time.unscaledTime;
            gm.Hud.Toast("Consejo: " + LastCoachAction);
            return true;
        }

        private int SeverityBand(int score, int warnings)
        {
            if (warnings <= 0) return 0;
            if (score < 50) return 3;
            if (score < 70) return 2;
            if (score < 95) return 1;
            return warnings >= 2 ? 1 : 0;
        }
    }
}
