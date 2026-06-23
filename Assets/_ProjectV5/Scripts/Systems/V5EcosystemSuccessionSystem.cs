using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5SuccessionStage
    {
        PrimordialSoup,
        FirstBiofilm,
        MetabolicBloom,
        EngineeredMicrobiome,
        PredatorWeb,
        StableBiosphere,
        CollapseRisk
    }

    /// <summary>
    /// Reads the world state and turns the sandbox into an ecological succession arc.
    /// Toggle panel with PageDown.
    /// </summary>
    public class V5EcosystemSuccessionSystem : MonoBehaviour
    {
        public V5SuccessionStage Stage = V5SuccessionStage.PrimordialSoup;
        public float StabilityScore;
        public float DiversityScore;
        public float ProductivityScore;
        public float HostilityScore;
        public string LastTransition = "Sucesion inicializada.";
        public bool ShowPanel;

        private float tickTimer;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.PageDown)) ShowPanel = !ShowPanel;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Paused) return;

            tickTimer += Time.deltaTime;
            if (tickTimer >= 2.0f)
            {
                tickTimer = 0f;
                Evaluate(gm);
            }
        }

        private void Evaluate(V5GameManager gm)
        {
            if (gm.Environment == null) return;

            float colonization = gm.Environment.AverageColonization();
            float oxygen = gm.Environment.AverageOxygen();
            float toxins = gm.Environment.AverageToxins();
            float acidity = gm.Environment.AverageAcidity();
            float acidStress = Mathf.Abs(acidity - 0.5f) * 2f;
            float threatPressure = gm.ThreatEcology != null ? gm.ThreatEcology.ThreatPressure : 0f;

            DiversityScore = CalculateDiversity(gm);
            float adaptationProductivity = AdaptationProductivity(gm);
            ProductivityScore = Mathf.Clamp01(colonization * 0.50f + oxygen * 0.20f + DiversityScore * 0.18f + adaptationProductivity * 0.18f);
            HostilityScore = Mathf.Clamp01(toxins * 0.50f + acidStress * 0.26f + Mathf.Clamp01(gm.NonPlayerCells.Count / 18f) * 0.14f + threatPressure * 0.18f);
            StabilityScore = Mathf.Clamp01(ProductivityScore * 0.62f + DiversityScore * 0.28f - HostilityScore * 0.45f + Mathf.Clamp01(gm.PlayerCellCount() / 12f) * 0.12f);

            bool engineered = (gm.Adaptations != null && gm.Adaptations.ActiveCount() >= 5 && gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA) ||
                              (gm.Genes != null && gm.Genes.UnlockedCount >= 3);

            V5SuccessionStage next = Stage;
            if (HostilityScore > 0.78f && StabilityScore < 0.42f) next = V5SuccessionStage.CollapseRisk;
            else if (StabilityScore > 0.78f && colonization > 0.34f && DiversityScore > 0.35f) next = V5SuccessionStage.StableBiosphere;
            else if (gm.NonPlayerCells.Count > 10 && gm.PlayerCellCount() > 7) next = V5SuccessionStage.PredatorWeb;
            else if (colonization > 0.24f && engineered) next = V5SuccessionStage.EngineeredMicrobiome;
            else if (oxygen > 0.38f || colonization > 0.16f || adaptationProductivity > 0.34f) next = V5SuccessionStage.MetabolicBloom;
            else if (colonization > 0.06f || gm.PlayerCellCount() >= 4) next = V5SuccessionStage.FirstBiofilm;
            else next = V5SuccessionStage.PrimordialSoup;

            if (next != Stage)
            {
                Stage = next;
                LastTransition = "Nueva etapa ecologica: " + Stage;
                if (gm.Hud != null) gm.Hud.Toast(LastTransition);
                if (gm.Codex != null) gm.Codex.Unlock("Sucesion: " + Stage, "El ecosistema cambio de estado por adaptaciones, identidad, metabolismo y presion ambiental.");
                ApplyStagePulse(gm);
            }

            ApplyPassiveWorldEffects(gm);
            CheckSuccessionVictory(gm);
        }

        private float CalculateDiversity(V5GameManager gm)
        {
            bool prok = false, euk = false, microfauna = false, photo = false, predator = false, colonizer = false, recycler = false;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i];
                if (c == null) continue;
                if (c.Domain == V5CellDomain.Prokaryote) prok = true;
                if (c.Domain == V5CellDomain.Eukaryote) euk = true;
                if (c.Domain == V5CellDomain.Multicellular) microfauna = true;
                if (c.HasPhotosynthesis || c.EvolutionPath == V5EvolutionPath.Cyanobacteria || c.EvolutionPath == V5EvolutionPath.Microalga) photo = true;
                if (c.HasPhagocytosis || c.EvolutionPath == V5EvolutionPath.Amoeba || c.EvolutionPath == V5EvolutionPath.Rotifer || c.EvolutionPath == V5EvolutionPath.Nematode) predator = true;
                if (c.Directive == V5Directive.Colonize || c.LineageRole == V5LineageRole.Colonizer || c.HasBiofilm) colonizer = true;
                if (c.LineageRole == V5LineageRole.Recycler || c.EvolutionPath == V5EvolutionPath.Fungus || c.EvolutionPath == V5EvolutionPath.SlimeMold) recycler = true;
            }

            V5AdaptationSystem a = gm.Adaptations;
            if (a != null)
            {
                if (HasAny(a, V5AdaptationId.BacterialWall, V5AdaptationId.ProtonPump, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.ProkaryoticThylakoid)) prok = true;
                if (HasAny(a, V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.Chloroplast)) euk = true;
                if (HasAny(a, V5AdaptationId.PersistentAdhesion, V5AdaptationId.CellDifferentiation, V5AdaptationId.SignalingCommunication, V5AdaptationId.BiologicalChampion)) microfauna = true;
                if (HasAny(a, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule)) photo = true;
                if (HasAny(a, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods, V5AdaptationId.Cilia)) predator = true;
                if (HasAny(a, V5AdaptationId.BasicAdhesin, V5AdaptationId.PiliFimbriae, V5AdaptationId.ColonialAdhesin, V5AdaptationId.PersistentAdhesion)) colonizer = true;
                if (HasAny(a, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory)) recycler = true;
            }

            int count = 0;
            if (prok) count++;
            if (euk) count++;
            if (microfauna) count++;
            if (photo) count++;
            if (predator) count++;
            if (colonizer) count++;
            if (recycler) count++;
            return Mathf.Clamp01(count / 7f);
        }

        private float AdaptationProductivity(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            V5AdaptationSystem a = gm.Adaptations;
            float score = 0f;
            if (HasAny(a, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast)) score += 0.28f;
            if (HasAny(a, V5AdaptationId.ProtonPump, V5AdaptationId.Mitochondria)) score += 0.22f;
            if (HasAny(a, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.SlimePlasmodium)) score += 0.18f;
            if (HasAny(a, V5AdaptationId.BasicAdhesin, V5AdaptationId.ColonialAdhesin, V5AdaptationId.PersistentAdhesion)) score += 0.16f;
            if (HasAny(a, V5AdaptationId.SignalingCommunication, V5AdaptationId.CellDifferentiation)) score += 0.16f;
            return Mathf.Clamp01(score);
        }

        private void ApplyStagePulse(V5GameManager gm)
        {
            V5CellEntity mother = gm.MotherCell;
            if (mother == null || gm.Environment == null) return;
            Vector2 pos = mother.transform.position;
            if (Stage == V5SuccessionStage.FirstBiofilm)
                gm.Environment.ModifyArea(pos, 5.5f, -0.01f, 0f, 0.02f, -0.02f, 0f, 0.12f, 0f);
            else if (Stage == V5SuccessionStage.MetabolicBloom)
                gm.Environment.ModifyArea(pos, 8f, 0.02f, 0.02f, 0.08f, -0.01f, 0f, 0.04f, 0.02f);
            else if (Stage == V5SuccessionStage.EngineeredMicrobiome)
                gm.Environment.ModifyArea(pos, 10f, 0.03f, 0.01f, 0.04f, -0.05f, -0.015f, 0.08f, 0.01f);
            else if (Stage == V5SuccessionStage.PredatorWeb)
                gm.Environment.ModifyArea(pos, 9f, 0.01f, 0f, 0.01f, 0.02f, 0f, 0.03f, 0.04f);
            else if (Stage == V5SuccessionStage.StableBiosphere)
                gm.Environment.ModifyArea(pos, 12f, 0.05f, 0.02f, 0.06f, -0.08f, -0.02f, 0.10f, 0.02f);
            else if (Stage == V5SuccessionStage.CollapseRisk)
                gm.Environment.ModifyArea(pos, 10f, -0.03f, 0f, -0.05f, 0.08f, 0.03f, -0.05f, 0.06f);
        }

        private void ApplyPassiveWorldEffects(V5GameManager gm)
        {
            if (gm.Environment == null || gm.MotherCell == null) return;
            Vector2 pos = gm.MotherCell.transform.position;
            if (Stage == V5SuccessionStage.StableBiosphere)
            {
                for (int i = 0; i < gm.PlayerCells.Count; i++)
                {
                    V5CellEntity c = gm.PlayerCells[i];
                    if (c != null) c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 0.05f);
                }
                gm.Environment.ModifyArea(pos, 5f, 0.004f, 0f, 0.006f, -0.005f, 0f, 0.004f, 0.001f);
            }
            else if (Stage == V5SuccessionStage.CollapseRisk)
            {
                for (int i = 0; i < gm.PlayerCells.Count; i++)
                {
                    V5CellEntity c = gm.PlayerCells[i];
                    if (c != null) c.Stats.stress = Mathf.Min(100f, c.Stats.stress + 0.08f);
                }
            }
        }

        private void CheckSuccessionVictory(V5GameManager gm)
        {
            if (gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;
            if (Stage == V5SuccessionStage.StableBiosphere && StabilityScore >= 0.84f && gm.Environment.AverageColonization() >= 0.40f)
                gm.Win("biosfera microscopica estabilizada");
        }

        private bool HasAny(V5AdaptationSystem adaptations, params V5AdaptationId[] ids)
        {
            if (adaptations == null || ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
                if (adaptations.Has(ids[i])) return true;
            return false;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            Rect r = new Rect(Screen.width - 455f, 82f, 435f, 235f);
            GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 10f, r.width - 24f, r.height - 20f));
            GUILayout.Label("SUCESION ECOLOGICA 1.4  [PageDown]", title);
            GUILayout.Label("Etapa: " + Stage, body);
            GUILayout.Label("Estabilidad " + Percent(StabilityScore) + " | Diversidad " + Percent(DiversityScore) + " | Productividad " + Percent(ProductivityScore) + " | Hostilidad " + Percent(HostilityScore), body);
            if (gm != null && gm.Environment != null)
                GUILayout.Label("Colonizacion " + Percent(gm.Environment.AverageColonization()) + " | O2 " + Percent(gm.Environment.AverageOxygen()) + " | Toxinas " + Percent(gm.Environment.AverageToxins()) + " | pH relativo " + Percent(gm.Environment.AverageAcidity()), body);
            GUILayout.Label(LastTransition, body);
            GUILayout.Space(6f);
            GUILayout.Label("Uso: estabiliza diversidad + colonizacion para ganar por biosfera estable. Si la hostilidad sube, entra riesgo de colapso.", body);
            if (GUILayout.Button("Cerrar")) ShowPanel = false;
            GUILayout.EndArea();
        }

        private string Percent(float v) { return (Mathf.Clamp01(v) * 100f).ToString("0") + "%"; }

        private void EnsureStyles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box);
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 16; title.normal.textColor = new Color(0.84f, 1f, 0.92f, 1f);
            body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white;
        }
    }
}
