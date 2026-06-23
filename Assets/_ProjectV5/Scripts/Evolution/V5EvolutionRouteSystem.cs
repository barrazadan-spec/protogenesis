using UnityEngine;

namespace Protogenesis.V5
{
    public class V5EvolutionRouteSystem : MonoBehaviour, IV5RunResettable
    {
        public V5EvolutionPath BestRoute = V5EvolutionPath.Uncommitted;
        public float BestScore01;
        public V5EvolutionPath ConsolidatedRoute = V5EvolutionPath.Uncommitted;
        public bool ApexReady;
        public string LastMessage = "Afinidad evolutiva lista.";

        private float tick;
        private V5EvolutionPath lastEmergingRoute = V5EvolutionPath.Uncommitted;
        private V5EvolutionPath lastApexReadyRoute = V5EvolutionPath.Uncommitted;

        public string BestPercentLabel { get { return (Mathf.Clamp01(BestScore01) * 100f).ToString("0") + "%"; } }

        public string Summary
        {
            get
            {
                string route = BestRoute == V5EvolutionPath.Uncommitted ? "sin ruta" : BestRoute.ToString();
                string state = BestScore01 >= V5Balance.RouteApexAffinityThreshold ? "apex maduro" :
                    BestScore01 >= V5Balance.RouteConsolidationAffinityThreshold ? "consolidada" :
                    BestScore01 >= V5Balance.RouteEmergenceAffinityThreshold ? "emergente" : "plastica";
                return route + " " + BestPercentLabel + " / " + state;
            }
        }

        public string ApexSummary
        {
            get
            {
                float need = V5Balance.RouteApexAffinityThreshold * 100f;
                return ApexReady ? "Apex listo por afinidad" : "Apex requiere " + need.ToString("0") + "% afinidad";
            }
        }

        public void ResetForNewRun()
        {
            BestRoute = V5EvolutionPath.Uncommitted;
            BestScore01 = 0f;
            ConsolidatedRoute = V5EvolutionPath.Uncommitted;
            ApexReady = false;
            lastEmergingRoute = V5EvolutionPath.Uncommitted;
            lastApexReadyRoute = V5EvolutionPath.Uncommitted;
            LastMessage = "Afinidad evolutiva lista.";
        }

        private void Update()
        {
            tick += Time.deltaTime;
            if (tick < 1f) return;
            tick = 0f;
            EvaluateAndApply("tick", true);
        }

        public void EvaluateNow()
        {
            EvaluateAndApply("manual", false);
        }

        public bool TryConsolidateNow(string source)
        {
            return EvaluateAndApply(source, true);
        }

        public bool IsApexReadyFor(V5EvolutionPath path)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null || path == V5EvolutionPath.Uncommitted) return false;
            if (mother.EvolutionPath != path) return false;
            V5EvolutionAffinityResult result = V5EvolutionAffinitySystem.Evaluate(mother, path);
            return result.Score01 >= V5Balance.RouteApexAffinityThreshold;
        }

        private bool EvaluateAndApply(string source, bool allowToast)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother == null) return false;

            V5EvolutionAffinityResult best = V5EvolutionAffinitySystem.BestRoute(mother);
            BestRoute = best.path;
            BestScore01 = best.Score01;
            if (mother.EvolutionPath != V5EvolutionPath.Uncommitted) ConsolidatedRoute = mother.EvolutionPath;

            if (best.path == V5EvolutionPath.Uncommitted || !V5RosterBalance.IsPlayablePath(best.path))
            {
                ApexReady = false;
                LastMessage = "Sin ruta dominante.";
                return false;
            }

            if (best.Score01 >= V5Balance.RouteEmergenceAffinityThreshold && best.Score01 < V5Balance.RouteConsolidationAffinityThreshold && lastEmergingRoute != best.path)
            {
                lastEmergingRoute = best.path;
                LastMessage = "Ruta emergente: " + best.path + " (" + best.PercentLabel + ")";
                if (allowToast && gm.Hud != null) gm.Hud.Toast(LastMessage);
            }

            bool consolidated = false;
            if (best.Score01 >= V5Balance.RouteConsolidationAffinityThreshold && mother.EvolutionPath != best.path)
            {
                mother.ApplyPath(best.path, true);
                ConsolidatedRoute = best.path;
                consolidated = true;
                LastMessage = "Ruta consolidada: " + best.path + " (" + best.PercentLabel + ")";
                if (gm.Codex != null) gm.Codex.Unlock("Ruta consolidada: " + best.path, "La colonia consolido identidad por afinidad: " + best.reasons);
                if (allowToast && gm.Hud != null) gm.Hud.Toast(LastMessage);
                V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
                if (feedback != null) feedback.PushFloating(LastMessage, mother.transform.position, new Color(0.75f, 1f, 0.95f, 1f));
            }

            ApexReady = mother.EvolutionPath == best.path && best.Score01 >= V5Balance.RouteApexAffinityThreshold;
            if (ApexReady && lastApexReadyRoute != best.path)
            {
                lastApexReadyRoute = best.path;
                LastMessage = "Madurez apex alcanzada: " + best.path + " (" + best.PercentLabel + ")";
                if (gm.Codex != null) gm.Codex.Unlock("Madurez apex: " + best.path, "La ruta alcanzo afinidad apex. Requiere gen ApexMaturation, tiempo y recursos.");
                if (allowToast && gm.Hud != null) gm.Hud.Toast(LastMessage);
            }
            else if (!consolidated && best.Score01 >= V5Balance.RouteConsolidationAffinityThreshold)
            {
                LastMessage = "Ruta estable: " + best.path + " (" + best.PercentLabel + ")";
            }

            return consolidated;
        }
    }
}
