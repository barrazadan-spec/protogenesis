using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5EcologyPolicy { Balanced, MutualisticMatrix, CompetitiveSelection, DefensiveSymbiosis, PredatoryWeb }
    public enum V5RelationKind { Neutral, Mutualism, Competition, Predation, Parasitism }

    /// <summary>
    /// Lightweight ecology layer: nearby cells now form a colony-level relationship web.
    /// This does not replace RTS control; it adds passive incentives for diverse, spatially coherent colonies.
    /// Toggle panel with Slash (/).
    /// </summary>
    public class V5EcologicalRelationsSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public V5EcologyPolicy Policy = V5EcologyPolicy.Balanced;
        public float MutualismScore;
        public float CompetitionScore;
        public float PredationScore;
        public float SymbiosisStability;
        public string LastMessage = "Red ecológica lista. Pulsa / para abrir.";

        private readonly Dictionary<V5EvolutionPath, int> pathCounts = new Dictionary<V5EvolutionPath, int>();
        private readonly Dictionary<V5LineageRole, int> roleCounts = new Dictionary<V5LineageRole, int>();
        private float tickTimer;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Start() => V5PanelRouter.Register("Ecología", () => ShowPanel, v => ShowPanel = v);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Slash)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Ecología"); ShowPanel = !ShowPanel; }
            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.85f)
            {
                tickTimer = 0f;
                TickRelations();
            }
        }

        private void TickRelations()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.PlayerCells == null || gm.PlayerCells.Count == 0) return;

            BuildComposition(gm);
            float diversity = DiversityScore();
            float roleCoverage = RoleCoverageScore();
            float colonization = gm.Environment != null ? gm.Environment.AverageColonization() : 0f;
            float toxins = gm.Environment != null ? gm.Environment.AverageToxins() : 0f;
            float oxygen = gm.Environment != null ? gm.Environment.AverageOxygen() : 0f;
            float adaptationDiversity = AdaptationDiversity(gm);
            float adaptationMutualism = AdaptationMutualismBonus(gm);

            MutualismScore = Mathf.Clamp01(diversity * 0.30f + adaptationDiversity * 0.22f + roleCoverage * 0.28f + colonization * 0.62f + CompatibleMetabolismBonus(gm) * 0.22f + adaptationMutualism * 0.24f);
            CompetitionScore = Mathf.Clamp01((gm.PlayerPopulationLoad() / V5Balance.HardPopulationLoad) * 0.38f + toxins * 0.42f + MonoculturePenalty() * 0.34f + AdaptationMonoculturePenalty(gm) * 0.24f);
            PredationScore = Mathf.Clamp01(Mathf.Max(PredatorRatio(), AdaptationPredatorRatio(gm)) * 0.75f + Mathf.Max(0f, gm.NonPlayerCells.Count - 8) * 0.025f);

            if (Policy == V5EcologyPolicy.MutualisticMatrix) { MutualismScore += 0.12f; CompetitionScore -= 0.06f; }
            else if (Policy == V5EcologyPolicy.CompetitiveSelection) { CompetitionScore += 0.08f; PredationScore += 0.05f; }
            else if (Policy == V5EcologyPolicy.DefensiveSymbiosis) { MutualismScore += 0.05f; PredationScore -= 0.06f; }
            else if (Policy == V5EcologyPolicy.PredatoryWeb) { PredationScore += 0.12f; MutualismScore -= 0.04f; }

            MutualismScore = Mathf.Clamp01(MutualismScore);
            CompetitionScore = Mathf.Clamp01(CompetitionScore);
            PredationScore = Mathf.Clamp01(PredationScore);
            SymbiosisStability = Mathf.Clamp01(0.50f + MutualismScore * 0.65f - CompetitionScore * 0.45f - PredationScore * 0.20f + oxygen * 0.08f);

            ApplyColonyEffects(gm);
        }

        private void BuildComposition(V5GameManager gm)
        {
            pathCounts.Clear(); roleCounts.Clear();
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                if (!pathCounts.ContainsKey(c.EvolutionPath)) pathCounts[c.EvolutionPath] = 0;
                pathCounts[c.EvolutionPath]++;
                if (!roleCounts.ContainsKey(c.LineageRole)) roleCounts[c.LineageRole] = 0;
                roleCounts[c.LineageRole]++;
            }
        }

        private float DiversityScore()
        {
            int nonZero = 0;
            foreach (KeyValuePair<V5EvolutionPath, int> kv in pathCounts)
            {
                if (kv.Key != V5EvolutionPath.Uncommitted && kv.Value > 0) nonZero++;
            }
            return Mathf.Clamp01(nonZero / 5f);
        }

        private float RoleCoverageScore()
        {
            int nonZero = 0;
            foreach (KeyValuePair<V5LineageRole, int> kv in roleCounts) if (kv.Value > 0) nonZero++;
            return Mathf.Clamp01(nonZero / 5f);
        }

        private float MonoculturePenalty()
        {
            int total = 0; int largest = 0;
            foreach (KeyValuePair<V5EvolutionPath, int> kv in pathCounts) { total += kv.Value; largest = Mathf.Max(largest, kv.Value); }
            if (total <= 2) return 0f;
            return Mathf.Clamp01((largest / Mathf.Max(1f, (float)total) - 0.55f) * 1.6f);
        }

        private float PredatorRatio()
        {
            int total = 0; int predators = 0;
            foreach (KeyValuePair<V5EvolutionPath, int> kv in pathCounts)
            {
                total += kv.Value;
                if (kv.Key == V5EvolutionPath.Amoeba || kv.Key == V5EvolutionPath.Flagellate || kv.Key == V5EvolutionPath.Rotifer || kv.Key == V5EvolutionPath.Nematode) predators += kv.Value;
            }
            return total > 0 ? Mathf.Clamp01((float)predators / total) : 0f;
        }

        private float CompatibleMetabolismBonus(V5GameManager gm)
        {
            bool photosynthetic = false; bool respirator = false; bool recycler = false; bool acidophile = false;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i]; if (c == null) continue;
                photosynthetic |= c.Metabolism == V5MetabolismType.Photosynthesis || c.HasPhotosynthesis || c.EvolutionPath == V5EvolutionPath.Cyanobacteria;
                respirator |= c.Metabolism == V5MetabolismType.Respiration;
                recycler |= c.LineageRole == V5LineageRole.Recycler || c.EvolutionPath == V5EvolutionPath.Fungus || c.EvolutionPath == V5EvolutionPath.SlimeMold;
                acidophile |= c.EvolutionPath == V5EvolutionPath.Archaea || c.Metabolism == V5MetabolismType.Chemolithotrophy;
            }
            V5AdaptationSystem a = gm.Adaptations;
            if (a != null)
            {
                photosynthetic |= HasAny(a, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast);
                respirator |= HasAny(a, V5AdaptationId.Mitochondria);
                recycler |= HasAny(a, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory);
                acidophile |= HasAny(a, V5AdaptationId.ProtonPump, V5AdaptationId.ExtremophileMembrane);
            }
            float score = 0f;
            if (photosynthetic && respirator) score += 0.45f;
            if (recycler && (photosynthetic || respirator)) score += 0.35f;
            if (acidophile && recycler) score += 0.20f;
            return Mathf.Clamp01(score);
        }

        private float AdaptationDiversity(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            V5AdaptationSystem a = gm.Adaptations;
            int count = 0;
            if (HasAny(a, V5AdaptationId.BacterialWall, V5AdaptationId.BacterialFlagellum, V5AdaptationId.PiliFimbriae, V5AdaptationId.BasicAdhesin)) count++;
            if (HasAny(a, V5AdaptationId.ProtonPump, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.CatalaseROS)) count++;
            if (HasAny(a, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule)) count++;
            if (HasAny(a, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods, V5AdaptationId.Cilia, V5AdaptationId.EukaryoticFlagellum)) count++;
            if (HasAny(a, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory)) count++;
            if (HasAny(a, V5AdaptationId.PersistentAdhesion, V5AdaptationId.CellDifferentiation, V5AdaptationId.SignalingCommunication)) count++;
            return Mathf.Clamp01(count / 6f);
        }

        private float AdaptationMutualismBonus(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            V5AdaptationSystem a = gm.Adaptations;
            float score = 0f;
            if (HasAny(a, V5AdaptationId.BasicAdhesin, V5AdaptationId.PiliFimbriae, V5AdaptationId.ColonialAdhesin, V5AdaptationId.PersistentAdhesion)) score += 0.30f;
            if (HasAny(a, V5AdaptationId.CellDifferentiation, V5AdaptationId.SignalingCommunication)) score += 0.32f;
            if (HasAny(a, V5AdaptationId.Chloroplast, V5AdaptationId.Mitochondria, V5AdaptationId.ProtonPump)) score += 0.20f;
            if (HasAny(a, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.ChemicalMemory)) score += 0.18f;
            return Mathf.Clamp01(score);
        }

        private float AdaptationMonoculturePenalty(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            float routeScore = 0f;
            V5EvolutionPath path = gm.Identity != null ? gm.Identity.EvolutionPath : V5EvolutionPath.Uncommitted;
            if (path != V5EvolutionPath.Uncommitted) routeScore = V5BiologyCanon.RouteAdaptationScore01(path, gm.Adaptations);
            float diversity = AdaptationDiversity(gm);
            return Mathf.Clamp01(routeScore * Mathf.Max(0f, 0.65f - diversity));
        }

        private float AdaptationPredatorRatio(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            V5AdaptationSystem a = gm.Adaptations;
            float score = 0f;
            if (HasAny(a, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods)) score += 0.45f;
            if (HasAny(a, V5AdaptationId.Cilia, V5AdaptationId.EukaryoticFlagellum, V5AdaptationId.BacterialFlagellum)) score += 0.25f;
            if (gm.Identity != null && (gm.Identity.EvolutionPath == V5EvolutionPath.Amoeba || gm.Identity.EvolutionPath == V5EvolutionPath.Ciliate || gm.Identity.EvolutionPath == V5EvolutionPath.Flagellate)) score += 0.25f;
            return Mathf.Clamp01(score);
        }

        private bool HasAny(V5AdaptationSystem adaptations, params V5AdaptationId[] ids)
        {
            if (adaptations == null || ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
                if (adaptations.Has(ids[i])) return true;
            return false;
        }

        private void ApplyColonyEffects(V5GameManager gm)
        {
            V5EnvironmentGrid env = gm.Environment;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i]; if (c == null) continue;
                if (SymbiosisStability > 0.62f)
                {
                    c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 0.18f);
                    c.Resources.atp += 0.05f + MutualismScore * 0.07f;
                }
                if (CompetitionScore > 0.62f && c.Role != V5CellRole.Mother)
                {
                    c.Stats.stress = Mathf.Min(100f, c.Stats.stress + CompetitionScore * 0.18f);
                    c.Resources.biomass = Mathf.Max(0f, c.Resources.biomass - 0.03f);
                }
                if (PredationScore > 0.55f && c.LineageRole == V5LineageRole.Predator)
                {
                    c.Stats.physicalDamagePerSecond += 0.002f;
                }
                if (env != null && (c.HasBiofilm || HasBiofilmAdaptations(gm)) && SymbiosisStability > 0.55f)
                {
                    env.ModifyArea(c.transform.position, 1.55f, 0f, 0f, 0.0008f, -0.0012f, 0f, 0.0016f, 0f);
                }
            }
        }

        private bool HasBiofilmAdaptations(V5GameManager gm)
        {
            return gm != null && gm.Adaptations != null && HasAny(gm.Adaptations, V5AdaptationId.BasicAdhesin, V5AdaptationId.PiliFimbriae, V5AdaptationId.ColonialAdhesin, V5AdaptationId.PersistentAdhesion);
        }

        private void SetPolicy(V5EcologyPolicy policy)
        {
            Policy = policy;
            LastMessage = "Política ecológica activa: " + policy;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(LastMessage);
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(22f, 220f, 500f, 390f);
            GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 12f, r.width - 28f, r.height - 24f));
            GUILayout.Label("RED ECOLÓGICA 1.2  [/]", title);
            GUILayout.Label(LastMessage, body);
            GUILayout.Space(6f);
            GUILayout.Label("Política: " + Policy + " | Estabilidad simbiótica: " + (SymbiosisStability * 100f).ToString("0") + "%", body);
            GUILayout.Label("Mutualismo " + Percent(MutualismScore) + "   Competencia " + Percent(CompetitionScore) + "   Depredación " + Percent(PredationScore), body);
            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Balance")) SetPolicy(V5EcologyPolicy.Balanced);
            if (GUILayout.Button("Mutualismo")) SetPolicy(V5EcologyPolicy.MutualisticMatrix);
            if (GUILayout.Button("Competición")) SetPolicy(V5EcologyPolicy.CompetitiveSelection);
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Defensiva")) SetPolicy(V5EcologyPolicy.DefensiveSymbiosis);
            if (GUILayout.Button("Red predadora")) SetPolicy(V5EcologyPolicy.PredatoryWeb);
            GUILayout.EndHorizontal();
            GUILayout.Space(10f);
            GUILayout.Label("Composición evolutiva", title);
            foreach (KeyValuePair<V5EvolutionPath, int> kv in pathCounts)
            {
                if (kv.Value > 0) GUILayout.Label(kv.Key + ": " + kv.Value, body);
            }
            GUILayout.Label("Regla jugable: la diversidad + roles + matriz colonizada reducen stress; monocultivos bajo toxina compiten y se vuelven frágiles.", body);
            GUILayout.EndArea();
        }

        private string Percent(float v) { return (v * 100f).ToString("0") + "%"; }
        private void EnsureStyles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box);
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.72f, 1f, 0.84f, 1f);
            body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white;
        }
    }
}
