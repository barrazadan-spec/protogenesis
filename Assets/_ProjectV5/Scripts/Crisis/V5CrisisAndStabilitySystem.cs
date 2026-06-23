using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5CrisisType { None, OsmoticShock, ToxicRunoff, UVPulse, ResourceCrash, PredatorMigration, AcidStorm }
    public enum V5LiabilityType { None, FragileMembrane, MetabolicLeak, HeredityNoise, AutoimmuneDrift, ResourceHunger }

    /// <summary>
    /// Adds long-run pressure. Crises are external events; liabilities are internal negative mutations
    /// caused by over-specialization and unchecked complexity.
    /// Toggle panel with LeftBracket ([).
    /// </summary>
    public class V5CrisisAndStabilitySystem : MonoBehaviour
    {
        public bool ShowPanel;
        public V5CrisisType ActiveCrisis = V5CrisisType.None;
        public float CrisisTimeRemaining;
        public float NextCrisisIn = 165f;
        public float AdaptationDebt;
        public string LastMessage = "Estabilidad colonial lista. Pulsa [ para abrir.";
        public readonly List<V5LiabilityType> ActiveLiabilities = new List<V5LiabilityType>(6);

        private float tickTimer;
        private float liabilityTimer;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;

        private void Start() => V5PanelRouter.Register("Crisis", () => ShowPanel, v => ShowPanel = v);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftBracket)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Crisis"); ShowPanel = !ShowPanel; }
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Phase == V5GamePhase.Victory || gm.Phase == V5GamePhase.Defeat) return;

            tickTimer += Time.deltaTime;
            if (tickTimer >= 0.50f)
            {
                tickTimer = 0f;
                TickDebt(gm);
                TickCrisis(gm, 0.50f);
                TickLiabilities(gm, 0.50f);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.LeftBracket)) StartRandomCrisis(true);
        }

        private void TickDebt(V5GameManager gm)
        {
            V5CellEntity mother = gm.MotherCell;
            if (mother == null) return;
            float structureLoad = V5Balance.BiomassLoadRatio(mother);
            if (gm.Adaptations != null) structureLoad = Mathf.Max(structureLoad, Mathf.Clamp01(gm.Adaptations.ActiveCount() / Mathf.Max(1f, gm.Adaptations.ActiveCap)));
            float monoculture = FindFirstObjectByType<V5EcologicalRelationsSystem>() != null ? FindFirstObjectByType<V5EcologicalRelationsSystem>().CompetitionScore : 0f;
            float populationPressure = Mathf.Max(0f, gm.PlayerCellCount() - V5Balance.SoftControllableEntityCount) * 0.10f;
            float stressPressure = mother.Stats.stress / 140f;
            float adaptationPressure = AdaptationPressure(gm);
            AdaptationDebt += (structureLoad * 0.14f + monoculture * 0.10f + populationPressure + stressPressure + adaptationPressure * 0.16f) * 0.5f;

            if (mother.HasStructure(V5StructureId.Catalase)) AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 0.16f);
            if (mother.EvolutionPath == V5EvolutionPath.StemCell || mother.HasStructure(V5StructureId.StemPlasticity)) AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 0.22f);
            if (gm.Adaptations != null)
            {
                if (gm.Adaptations.Has(V5AdaptationId.CatalaseROS)) AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 0.18f);
                if (gm.Adaptations.Has(V5AdaptationId.ContractileVacuole)) AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 0.10f);
                if (gm.Adaptations.Has(V5AdaptationId.SignalingCommunication)) AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 0.16f);
                if (gm.Adaptations.Has(V5AdaptationId.BiologicalChampion)) AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 0.20f);
            }
            AdaptationDebt = Mathf.Clamp(AdaptationDebt, 0f, 100f);

            liabilityTimer += 0.5f;
            if (AdaptationDebt >= 35f + ActiveLiabilities.Count * 18f && liabilityTimer > 18f)
            {
                liabilityTimer = 0f;
                AddRandomLiability();
            }
        }

        private void TickCrisis(V5GameManager gm, float dt)
        {
            if (ActiveCrisis == V5CrisisType.None)
            {
                NextCrisisIn -= dt;
                float threat = gm.Director != null ? gm.Director.ThreatLevel : 0.25f;
                if (NextCrisisIn <= 0f) StartRandomCrisis(false);
                else if (threat > 0.92f && Random.value < 0.02f) StartRandomCrisis(false);
                return;
            }

            CrisisTimeRemaining -= dt;
            ApplyCrisisEffect(gm, dt);
            if (CrisisTimeRemaining <= 0f)
            {
                LastMessage = "Crisis terminada: " + ActiveCrisis;
                ActiveCrisis = V5CrisisType.None;
                CrisisTimeRemaining = 0f;
                NextCrisisIn = Random.Range(135f, 230f);
                Toast(LastMessage);
            }
        }

        private void ApplyCrisisEffect(V5GameManager gm, float dt)
        {
            V5EnvironmentGrid env = gm.Environment;
            if (env == null) return;
            Vector2 center = gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;
            if (ActiveCrisis == V5CrisisType.OsmoticShock)
            {
                env.ModifyArea(center, 9f, 0f, 0f, -0.002f, 0.002f, 0.002f, -0.0015f, 0f);
                DamageColony(gm, 0.035f, V5DamageKind.Osmotic, false);
            }
            else if (ActiveCrisis == V5CrisisType.ToxicRunoff)
            {
                env.ModifyArea(Random.insideUnitCircle * env.MapRadius * 0.60f, 11f, -0.0012f, 0f, -0.001f, 0.012f, 0.0008f, -0.0008f, 0.001f);
                DamageColony(gm, 0.028f, V5DamageKind.Chemical, true);
            }
            else if (ActiveCrisis == V5CrisisType.UVPulse)
            {
                env.ModifyArea(new Vector2(0f, env.MapRadius * 0.35f), 14f, 0f, 0.006f, 0.002f, 0.003f, 0f, 0f, 0f);
                DamagePhotosensitive(gm, 0.042f);
            }
            else if (ActiveCrisis == V5CrisisType.ResourceCrash)
            {
                env.ModifyArea(Vector2.zero, env.MapRadius * 0.75f, -0.004f, 0f, 0f, 0f, 0f, -0.0007f, -0.002f);
                if (gm.MotherCell != null) gm.MotherCell.Resources.atp = Mathf.Max(0f, gm.MotherCell.Resources.atp - 0.12f);
            }
            else if (ActiveCrisis == V5CrisisType.PredatorMigration)
            {
                if (Random.value < 0.10f && gm.CellFactory != null) gm.CellFactory.SpawnNeutral(Random.insideUnitCircle * env.MapRadius * 0.88f, RandomPredatorPath());
            }
            else if (ActiveCrisis == V5CrisisType.AcidStorm)
            {
                env.ModifyArea(Random.insideUnitCircle * env.MapRadius * 0.50f, 13f, -0.001f, 0f, -0.001f, 0.002f, 0.014f, -0.0008f, 0.0004f);
                DamageNonAcidophiles(gm, 0.036f);
            }
        }

        public void StartRandomCrisis(bool forced)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5CrisisType chosen = PickContextualCrisis(gm);
            StartCrisis(chosen, forced);
        }

        public void StartCrisis(V5CrisisType type, bool forced)
        {
            if (type == V5CrisisType.None) return;
            ActiveCrisis = type;
            CrisisTimeRemaining = forced ? 55f : Random.Range(40f, 78f);
            NextCrisisIn = 999f;
            LastMessage = "Crisis activa: " + type + " (" + CrisisTimeRemaining.ToString("0") + "s)";
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null) LastMessage += " | causa: " + CrisisCause(gm, type);
            Toast(LastMessage);
        }

        private void TickLiabilities(V5GameManager gm, float dt)
        {
            if (ActiveLiabilities.Count == 0) return;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i]; if (c == null) continue;
                if (ActiveLiabilities.Contains(V5LiabilityType.FragileMembrane)) c.Stats.currentHp = Mathf.Max(1f, c.Stats.currentHp - 0.025f * dt);
                if (ActiveLiabilities.Contains(V5LiabilityType.MetabolicLeak)) c.Resources.atp = Mathf.Max(0f, c.Resources.atp - 0.08f * dt);
                if (ActiveLiabilities.Contains(V5LiabilityType.ResourceHunger)) c.Resources.biomass = Mathf.Max(0f, c.Resources.biomass - 0.045f * dt);
                if (ActiveLiabilities.Contains(V5LiabilityType.AutoimmuneDrift) && c.IsPlayerOwned && c.Role != V5CellRole.Mother && Random.value < 0.002f) c.Stats.stress += 1.2f;
                if (ActiveLiabilities.Contains(V5LiabilityType.HeredityNoise) && c.Generation >= 1) c.Stats.stress += 0.015f * dt;
            }
        }

        private void AddRandomLiability()
        {
            V5LiabilityType preferred = PickContextualLiability();
            if (preferred != V5LiabilityType.None && !ActiveLiabilities.Contains(preferred))
            {
                ActiveLiabilities.Add(preferred);
                AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 18f);
                LastMessage = "Mutacion negativa emergente: " + preferred;
                Toast(LastMessage);
                return;
            }

            V5LiabilityType[] values = (V5LiabilityType[])System.Enum.GetValues(typeof(V5LiabilityType));
            int guard = 0;
            while (guard < 24)
            {
                guard++;
                V5LiabilityType t = values[Random.Range(1, values.Length)];
                if (!ActiveLiabilities.Contains(t))
                {
                    ActiveLiabilities.Add(t);
                    AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 18f);
                    LastMessage = "Mutacion negativa emergente: " + t;
                    Toast(LastMessage);
                    return;
                }
            }
        }

        public void StabilizeColony()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return;
            V5ResourceWallet cost = V5ResourceWallet.Cost(35f, 18f, 16f, 18f, 12f, 8f);
            if (!gm.MotherCell.Resources.CanPay(cost)) { Toast("Faltan recursos para estabilizar la colonia."); return; }
            gm.MotherCell.Resources.Pay(cost);
            AdaptationDebt = Mathf.Max(0f, AdaptationDebt - 26f);
            if (ActiveLiabilities.Count > 0) ActiveLiabilities.RemoveAt(ActiveLiabilities.Count - 1);
            for (int i = 0; i < gm.PlayerCells.Count; i++) if (gm.PlayerCells[i] != null) gm.PlayerCells[i].Stats.stress = Mathf.Max(0f, gm.PlayerCells[i].Stats.stress - 8f);
            LastMessage = "Homeostasis colonial aplicada.";
            Toast(LastMessage);
        }

        private void DamageColony(V5GameManager gm, float amount, V5DamageKind kind, bool respectCatalase)
        {
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i]; if (c == null) continue;
                if (respectCatalase && HasCrisisAdaptation(gm, c, V5AdaptationId.CatalaseROS)) continue;
                if (respectCatalase && c.HasStructure(V5StructureId.Catalase)) continue;
                c.Damage(amount * CrisisDamageMultiplier(gm, c, kind), kind, c.transform.position);
            }
        }

        private void DamagePhotosensitive(V5GameManager gm, float amount)
        {
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i]; if (c == null) continue;
                if (c.HasStructure(V5StructureId.Catalase) || c.EvolutionPath == V5EvolutionPath.Cyanobacteria || HasCrisisAdaptation(gm, c, V5AdaptationId.CatalaseROS) || HasCrisisAdaptation(gm, c, V5AdaptationId.ProkaryoticThylakoid)) c.Stats.stress += 0.06f;
                else c.Damage(amount * CrisisDamageMultiplier(gm, c, V5DamageKind.Thermal), V5DamageKind.Thermal, c.transform.position);
            }
        }

        private void DamageNonAcidophiles(V5GameManager gm, float amount)
        {
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity c = gm.PlayerCells[i]; if (c == null) continue;
                if (c.EvolutionPath == V5EvolutionPath.Archaea || c.Metabolism == V5MetabolismType.Chemolithotrophy || HasCrisisAdaptation(gm, c, V5AdaptationId.ExtremophileMembrane) || HasCrisisAdaptation(gm, c, V5AdaptationId.ProtonPump)) c.Resources.minerals += 0.05f;
                else c.Damage(amount * CrisisDamageMultiplier(gm, c, V5DamageKind.Chemical), V5DamageKind.Chemical, c.transform.position);
            }
        }

        private float AdaptationPressure(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            float active = Mathf.Clamp01(gm.Adaptations.ActiveCount() / Mathf.Max(1f, gm.Adaptations.ActiveCap));
            float advanced = Mathf.Clamp01((gm.Adaptations.CountInTier(V5AdaptationTier.T3Specialization) + gm.Adaptations.CountInTier(V5AdaptationTier.T4ColonialBody) * 1.4f) / 8f);
            float identity = gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA ? 0.10f : 0f;
            return Mathf.Clamp01(active * 0.55f + advanced * 0.35f + identity);
        }

        private V5CrisisType PickContextualCrisis(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return RandomFallbackCrisis();
            V5AdaptationSystem a = gm.Adaptations;
            if (HasAny(a, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast, V5AdaptationId.SilicaFrustule) && Random.value < 0.56f) return V5CrisisType.UVPulse;
            if (HasAny(a, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.SlimePlasmodium) && Random.value < 0.56f) return V5CrisisType.ResourceCrash;
            if (HasAny(a, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods, V5AdaptationId.Cilia) && Random.value < 0.56f) return V5CrisisType.PredatorMigration;
            if (HasAny(a, V5AdaptationId.ProtonPump, V5AdaptationId.ExtremophileMembrane) && Random.value < 0.48f) return V5CrisisType.AcidStorm;
            if (HasAny(a, V5AdaptationId.BacterialWall, V5AdaptationId.PolysaccharideCapsule, V5AdaptationId.BasicAdhesin) && Random.value < 0.46f) return V5CrisisType.OsmoticShock;
            if (AdaptationDebt > 55f) return Random.value < 0.5f ? V5CrisisType.ToxicRunoff : V5CrisisType.ResourceCrash;
            return RandomFallbackCrisis();
        }

        private V5CrisisType RandomFallbackCrisis()
        {
            V5CrisisType[] values = (V5CrisisType[])System.Enum.GetValues(typeof(V5CrisisType));
            V5CrisisType chosen = V5CrisisType.None;
            int guard = 0;
            while (chosen == V5CrisisType.None && guard < 16)
            {
                guard++;
                chosen = values[Random.Range(0, values.Length)];
            }
            return chosen == V5CrisisType.None ? V5CrisisType.OsmoticShock : chosen;
        }

        private V5LiabilityType PickContextualLiability()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Adaptations == null) return V5LiabilityType.None;
            V5AdaptationSystem a = gm.Adaptations;
            if (a.ActiveCount() >= 8 && !HasAny(a, V5AdaptationId.SignalingCommunication, V5AdaptationId.CellDifferentiation)) return V5LiabilityType.HeredityNoise;
            if (HasAny(a, V5AdaptationId.Mitochondria, V5AdaptationId.ProtonPump, V5AdaptationId.Chloroplast) && !a.Has(V5AdaptationId.CatalaseROS)) return V5LiabilityType.MetabolicLeak;
            if (!HasAny(a, V5AdaptationId.BacterialWall, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule, V5AdaptationId.PersistentAdhesion)) return V5LiabilityType.FragileMembrane;
            if (gm.PlayerCellCount() > V5Balance.SoftControllableEntityCount + 3 && !HasAny(a, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast, V5AdaptationId.ProtonPump)) return V5LiabilityType.ResourceHunger;
            if (HasAny(a, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods) && !a.Has(V5AdaptationId.SignalingCommunication)) return V5LiabilityType.AutoimmuneDrift;
            return V5LiabilityType.None;
        }

        private string CrisisCause(V5GameManager gm, V5CrisisType type)
        {
            if (gm == null) return "ecosistema";
            if (gm.Identity != null && gm.Identity.Identity != V5IdentityId.LUCA) return gm.Identity.DisplayName + " / deuda " + AdaptationDebt.ToString("0");
            return "deuda " + AdaptationDebt.ToString("0");
        }

        private float CrisisDamageMultiplier(V5GameManager gm, V5CellEntity cell, V5DamageKind kind)
        {
            float mult = 1f;
            if (kind == V5DamageKind.Osmotic && HasCrisisAdaptation(gm, cell, V5AdaptationId.ContractileVacuole)) mult *= 0.55f;
            if ((kind == V5DamageKind.Chemical || kind == V5DamageKind.Thermal) && HasCrisisAdaptation(gm, cell, V5AdaptationId.CatalaseROS)) mult *= 0.62f;
            if (kind == V5DamageKind.Chemical && HasCrisisAdaptation(gm, cell, V5AdaptationId.ExtremophileMembrane)) mult *= 0.58f;
            if (HasCrisisAdaptation(gm, cell, V5AdaptationId.BiologicalChampion)) mult *= 0.72f;
            return mult;
        }

        private bool HasCrisisAdaptation(V5GameManager gm, V5CellEntity cell, V5AdaptationId id)
        {
            if (gm == null || gm.Adaptations == null || cell == null || !cell.IsPlayerOwned) return false;
            return gm.Adaptations.Has(id);
        }

        private bool HasAny(V5AdaptationSystem adaptations, params V5AdaptationId[] ids)
        {
            if (adaptations == null || ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
                if (adaptations.Has(ids[i])) return true;
            return false;
        }

        private V5EvolutionPath RandomPredatorPath()
        {
            float r = Random.value;
            if (r < 0.30f) return V5EvolutionPath.Amoeba;
            if (r < 0.52f) return V5EvolutionPath.Flagellate;
            if (r < 0.72f) return V5EvolutionPath.Rotifer;
            if (r < 0.88f) return V5EvolutionPath.Nematode;
            return V5EvolutionPath.SlimeMold;
        }

        private void Toast(string msg)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(msg);
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            Rect r = new Rect(Screen.width - 535f, 220f, 515f, 390f);
            GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 12f, r.width - 28f, r.height - 24f));
            GUILayout.Label("CRISIS + ESTABILIDAD 1.2  [[ ]", title);
            GUILayout.Label(LastMessage, body);
            GUILayout.Label("Crisis: " + ActiveCrisis + " | tiempo " + CrisisTimeRemaining.ToString("0") + "s | próxima " + NextCrisisIn.ToString("0") + "s", body);
            GUILayout.Label("Deuda adaptativa: " + AdaptationDebt.ToString("0.0") + " / 100", body);
            Rect bar = GUILayoutUtility.GetRect(470f, 18f); GUI.Box(bar, ""); GUI.Box(new Rect(bar.x + 2f, bar.y + 2f, (bar.width - 4f) * Mathf.Clamp01(AdaptationDebt / 100f), bar.height - 4f), "");
            GUILayout.Space(8f);
            GUILayout.Label("Mutaciones negativas activas:", title);
            if (ActiveLiabilities.Count == 0) GUILayout.Label("ninguna", body);
            for (int i = 0; i < ActiveLiabilities.Count; i++) GUILayout.Label("- " + ActiveLiabilities[i], body);
            GUILayout.Space(8f);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Forzar crisis")) StartRandomCrisis(true);
            if (GUILayout.Button("Estabilizar colonia")) StabilizeColony();
            GUILayout.EndHorizontal();
            GUILayout.Label("Ctrl + [ fuerza una crisis. La deuda sube con complejidad, monocultivo, stress y exceso de poblacion; baja con CatalasaROS, Vacuola, Comunicacion y homeostasis.", body);
            GUILayout.EndArea();
        }

        private void EnsureStyles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box);
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(1f, 0.72f, 0.55f, 1f);
            body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white;
        }
    }
}
