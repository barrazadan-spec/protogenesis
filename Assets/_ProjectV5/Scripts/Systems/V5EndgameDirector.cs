using UnityEngine;

namespace Protogenesis.V5
{
    public class V5EndgameDirector : MonoBehaviour, IV5RunResettable
    {
        public bool Enabled = true;
        public string Status = "";
        private float tick;
        private float nextWarning;
        private float dominanceSustainedSeconds;
        private float ecologicalHoldSeconds;
        private float metabolicCollapseSeconds;

        private void Update()
        {
            if (!Enabled) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;
            tick += Time.deltaTime;
            if (tick < 1.0f) return;
            tick = 0f;
            Evaluate(gm);
        }

        private void Evaluate(V5GameManager gm)
        {
            if (gm.MotherCell == null)
            {
                gm.Lose("la célula madre se perdió");
                return;
            }

            float col = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            int cells = gm.PlayerCellCount();
            int enemies = gm.NonPlayerCells != null ? gm.NonPlayerCells.Count : 0;
            bool apex = gm.Apex != null && gm.Apex.ApexSpawned;
            bool stableChemistry = gm.Environment != null && gm.Environment.AverageToxins() < 0.32f && Mathf.Abs(gm.Environment.AverageAcidity() - 0.5f) < 0.28f;

            if (gm.RouteClimax != null)
            {
                gm.RouteClimax.RefreshNow(gm);
                if (gm.RouteClimax.ClaimVictory(gm))
                {
                    PlayEndCue(true);
                    return;
                }
            }

            // MVP Win 1: Dominancia Colonial — cuerpo estable 90s
            bool dominanceActive = cells >= 8 &&
                gm.Body != null && gm.Body.OccupiedSlots >= 6 &&
                gm.Continuity != null && gm.Continuity.LastScore >= 70f;
            dominanceSustainedSeconds = dominanceActive ? dominanceSustainedSeconds + 1f : 0f;
            if (dominanceSustainedSeconds >= 90f)
            {
                gm.Win("dominancia colonial: cuerpo estable sostenido 90s");
                PlayEndCue(true);
                return;
            }

            // MVP Win 2: Control Ecologico — colonizacion sostenida 120s
            ecologicalHoldSeconds = col >= 0.45f ? ecologicalHoldSeconds + 1f : 0f;
            if (ecologicalHoldSeconds >= 120f)
            {
                gm.Win("control ecologico: colonizacion sostenida 120s");
                PlayEndCue(true);
                return;
            }

            // Victorias heredadas (fallback)
            if (gm.ElapsedSeconds > 1500f && col >= 0.25f && stableChemistry)
            {
                gm.Win("ecosistema estable sostenido");
                PlayEndCue(true);
                return;
            }
            if (gm.ElapsedSeconds > 900f && enemies == 0 && cells >= 5)
            {
                gm.Win("amenazas biologicas eliminadas");
                PlayEndCue(true);
                return;
            }
            if (apex && col >= 0.30f)
            {
                gm.Win("forma apex consolido el ecosistema");
                PlayEndCue(true);
                return;
            }

            // Derrota: madre muerta
            if (gm.MotherCell.Stats.currentHp <= 1f)
            {
                gm.Lose("lisis de celula madre");
                PlayEndCue(false);
                return;
            }

            // Derrota: colapso metabolico — sin recursos ni unidades por 60s
            bool metabolicCrisis = gm.MotherCell.Resources.atp < 5f &&
                gm.MotherCell.Resources.biomass < 5f && cells <= 2;
            metabolicCollapseSeconds = metabolicCrisis ? metabolicCollapseSeconds + 1f : 0f;
            if (metabolicCollapseSeconds >= 60f)
            {
                gm.Lose("colapso metabolico: recursos agotados");
                PlayEndCue(false);
                return;
            }

            // Aviso de stress critico
            if (gm.MotherCell.Stats.stress > 92f && Time.time > nextWarning)
            {
                nextWarning = Time.time + 8f;
                Status = "Riesgo de colapso: stress sobre 92.";
                if (gm.Hud != null) gm.Hud.Toast(Status);
            }
            else
            {
                string domProg = dominanceSustainedSeconds > 0f ? " | dom " + dominanceSustainedSeconds.ToString("0") + "/90s" : "";
                string ecoProg = ecologicalHoldSeconds > 0f ? " | eco " + ecologicalHoldSeconds.ToString("0") + "/120s" : "";
                string climax = gm.RouteClimax != null ? " | " + gm.RouteClimax.Summary : "";
                Status = "Col " + (col * 100f).ToString("0") + "% | cel " + cells + " | ene " + enemies + domProg + ecoProg + climax;
            }
        }

        public void ResetForNewRun()
        {
            dominanceSustainedSeconds = 0f;
            ecologicalHoldSeconds = 0f;
            metabolicCollapseSeconds = 0f;
            nextWarning = 0f;
            Status = "";
        }

        private void PlayEndCue(bool victory)
        {
            V5AudioFeedback audio = FindFirstObjectByType<V5AudioFeedback>();
            if (audio != null) audio.PlayCue(victory ? "victory" : "defeat");
        }
    }
}
