using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    /// <summary>
    /// V5 0.9: active biological abilities for the RTS layer.
    /// Kept intentionally data-light: works on runtime cells without ScriptableObject setup.
    /// Experimental panel abilities. Direct Q/W/R hotkeys are off by default.
    /// </summary>
    public class V5AbilitySystem : MonoBehaviour, IV5RunResettable
    {
        private class TimedBuff
        {
            public V5CellEntity cell;
            public float endsAt;
            public float speedMultiplier;
            public float synthesisMultiplier;
            public float atpBonus;
        }

        public bool ShowPanel;
        public bool EnableDirectAbilityHotkeys;
        public float GlobalCooldown = 0.15f;
        public float SurgeCooldown = 9f;
        public float PulseCooldown = 7f;
        public float RepairCooldown = 10f;
        public float RouteFantasyCooldown = 12f;

        public float NextSurgeReady { get; private set; }
        public float NextPulseReady { get; private set; }
        public float NextRepairReady { get; private set; }
        public float NextRouteFantasyReady { get; private set; }
        public int RouteFantasyCastCount { get; private set; }
        public string LastAbilityMessage { get; private set; }
        public string LastRouteFantasyAbility { get; private set; }

        private readonly List<TimedBuff> activeBuffs = new List<TimedBuff>(32);
        private float nextInputAllowed;
        private GUIStyle panelStyle;
        private GUIStyle smallStyle;
        private GUIStyle titleStyle;
        private Texture2D panelTexture;

        private void Start()
        {
            ShowPanel = false;
            V5PanelRouter.Register("Habilidades", () => ShowPanel, v => ShowPanel = v);
        }

        public void ResetForNewRun()
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--) RemoveBuff(activeBuffs[i]);
            activeBuffs.Clear();
            NextSurgeReady = 0f;
            NextPulseReady = 0f;
            NextRepairReady = 0f;
            NextRouteFantasyReady = 0f;
            RouteFantasyCastCount = 0;
            LastAbilityMessage = "Habilidades listas.";
            LastRouteFantasyAbility = "Sin habilidad de ruta usada.";
            nextInputAllowed = 0f;
        }

        private void Update()
        {
            TickBuffs();
            V5GameManager gm = V5GameManager.Instance;
            if (Input.GetKeyDown(KeyCode.K) && (gm == null || !gm.CoreMode)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Habilidades"); ShowPanel = !ShowPanel; }
            if (!ShowPanel && !EnableDirectAbilityHotkeys) return;
            if (Time.unscaledTime < nextInputAllowed) return;

            if (Input.GetKeyDown(KeyCode.Q))
            {
                nextInputAllowed = Time.unscaledTime + GlobalCooldown;
                TryMetabolicSurge();
            }
            if (Input.GetKeyDown(KeyCode.W))
            {
                nextInputAllowed = Time.unscaledTime + GlobalCooldown;
                TryEcologicalPulse();
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                nextInputAllowed = Time.unscaledTime + GlobalCooldown;
                TryEmergencyRepair();
            }
            if (Input.GetKeyDown(KeyCode.E))
            {
                nextInputAllowed = Time.unscaledTime + GlobalCooldown;
                TryRouteFantasyAbility();
            }
        }

        private List<V5CellEntity> GetTargets()
        {
            List<V5CellEntity> result = new List<V5CellEntity>(16);
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return result;

            if (gm.Selection != null && gm.Selection.Selected.Count > 0)
            {
                for (int i = 0; i < gm.Selection.Selected.Count; i++)
                {
                    V5CellEntity c = gm.Selection.Selected[i];
                    if (c != null && c.IsPlayerOwned && !result.Contains(c)) result.Add(c);
                }
            }

            if (result.Count == 0 && gm.MotherCell != null) result.Add(gm.MotherCell);
            return result;
        }

        public bool TryMetabolicSurge()
        {
            if (Time.time < NextSurgeReady) { Notify("Metabolic Surge en cooldown"); return false; }
            List<V5CellEntity> targets = GetTargets();
            int applied = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                V5CellEntity cell = targets[i];
                if (cell == null || cell.Resources.atp < 14f) continue;
                cell.Resources.atp -= 14f;
                cell.Stats.stress = Mathf.Clamp(cell.Stats.stress + 4f, 0f, 100f);
                AddBuff(cell, 7.5f, 1.45f, 1.35f, 0.55f);
                applied++;
            }
            if (applied <= 0) { Notify("Falta ATP para Metabolic Surge"); return false; }
            NextSurgeReady = Time.time + SurgeCooldown;
            Notify("Metabolic Surge: velocidad y síntesis temporal ×" + applied);
            return true;
        }

        public bool TryEcologicalPulse()
        {
            if (Time.time < NextPulseReady) { Notify("Ecological Pulse en cooldown"); return false; }
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return false;
            List<V5CellEntity> targets = GetTargets();
            int applied = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                V5CellEntity cell = targets[i];
                if (cell == null || cell.Resources.atp < 18f) continue;
                cell.Resources.atp -= 18f;
                if (cell.Resources.aminoAcids > 3f) cell.Resources.aminoAcids -= 3f;

                ApplyPulseForCell(gm, cell);
                applied++;
            }
            if (applied <= 0) { Notify("Falta ATP para Ecological Pulse"); return false; }
            NextPulseReady = Time.time + PulseCooldown;
            Notify("Ecological Pulse liberado ×" + applied);
            return true;
        }

        private void ApplyPulseForCell(V5GameManager gm, V5CellEntity cell)
        {
            Vector2 pos = cell.transform.position;
            float radius = Mathf.Max(2.5f, 2.4f + cell.Stats.radius * 2f);

            bool toxicStyle = IsToxicStyle(gm, cell);
            bool photoStyle = IsPhotoStyle(gm, cell);
            bool acidStyle = IsExtremeStyle(gm, cell);
            bool digestStyle = IsDigestStyle(gm, cell);
            bool piercingStyle = IsPiercingStyle(gm, cell);
            bool flowStyle = IsFlowStyle(gm, cell);
            bool repairStyle = IsRepairStyle(gm, cell);

            if (photoStyle)
            {
                gm.Environment.ModifyArea(pos, radius + 1.2f, -0.01f, 0f, 0.12f, -0.04f, -0.01f, 0.03f, 0f);
                PushLabel("O₂ bloom", pos, new Color(0.55f, 0.95f, 1f, 1f));
                return;
            }

            if (acidStyle)
            {
                gm.Environment.ModifyArea(pos, radius, 0.02f, 0f, -0.02f, 0.04f, 0.12f, 0.025f, 0.02f);
                cell.Stats.toxinResistance += 0.015f;
                PushLabel("bolsa extrema", pos, new Color(0.85f, 0.65f, 0.35f, 1f));
                return;
            }

            if (digestStyle)
            {
                gm.Environment.ModifyArea(pos, radius, 0.015f, 0f, 0f, 0.015f, 0f, 0.01f, 0.035f);
                DamageEnemiesAround(gm, pos, radius + 0.8f, 10f + cell.Stats.physicalDamagePerSecond * 2f, V5DamageKind.Physical);
                PushLabel("digestión radial", pos, new Color(1f, 0.65f, 0.45f, 1f));
                return;
            }

            if (piercingStyle)
            {
                gm.Environment.ModifyArea(pos, radius, 0.0f, 0f, 0f, 0f, 0f, -0.025f, 0.012f);
                DamageEnemiesAround(gm, pos, radius + 0.5f, 12f + cell.Stats.physicalDamagePerSecond * 2.4f, V5DamageKind.Piercing);
                PushLabel("puncion", pos, new Color(0.95f, 0.86f, 0.55f, 1f));
                return;
            }

            if (flowStyle)
            {
                gm.Environment.ModifyArea(pos, radius + 0.8f, 0.005f, 0f, 0.03f, -0.025f, 0f, 0.012f, 0.015f);
                DamageEnemiesAround(gm, pos, radius + 1.1f, 6f + cell.Stats.physicalDamagePerSecond * 1.2f, V5DamageKind.Physical);
                PushLabel("corriente ciliada", pos, new Color(0.55f, 0.78f, 1f, 1f));
                return;
            }

            if (toxicStyle)
            {
                gm.Environment.ModifyArea(pos, radius, 0.0f, 0f, -0.03f, 0.13f, 0.04f, 0.015f, 0.01f);
                DamageEnemiesAround(gm, pos, radius, 7.5f + cell.Stats.chemicalDamagePerSecond * 2f, V5DamageKind.Chemical);
                PushLabel("toxina", pos, new Color(0.9f, 0.35f, 0.45f, 1f));
                return;
            }

            // Generic homeostatic pulse: detox and weak colonization.
            gm.Environment.ModifyArea(pos, radius, -0.005f, 0f, 0.02f, -0.08f, -0.025f, 0.018f, 0f);
            float stressRelief = repairStyle ? 14f : 8f;
            cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - stressRelief);
            PushLabel(repairStyle ? "homeostasis fuerte" : "homeostasis", pos, new Color(0.65f, 1f, 0.75f, 1f));
        }

        private bool IsToxicStyle(V5GameManager gm, V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.EvolutionPath == V5EvolutionPath.Bacteria ||
                   cell.EvolutionPath == V5EvolutionPath.Fungus ||
                   cell.EvolutionPath == V5EvolutionPath.SlimeMold ||
                   cell.HasMucilage ||
                   cell.HasStructure(V5StructureId.AzurophilicGranule) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.BacterialWall) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.FungalHypha) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.ExtracellularEnzymes) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.SlimePlasmodium);
        }

        private bool IsPhotoStyle(V5GameManager gm, V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.Metabolism == V5MetabolismType.Photosynthesis ||
                   cell.HasPhotosynthesis ||
                   cell.EvolutionPath == V5EvolutionPath.Cyanobacteria ||
                   HasPlayerAdaptation(cell, V5AdaptationId.ProkaryoticThylakoid) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.Chloroplast) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.CelluloseWall) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.SilicaFrustule);
        }

        private bool IsExtremeStyle(V5GameManager gm, V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.Metabolism == V5MetabolismType.Chemolithotrophy ||
                   cell.EvolutionPath == V5EvolutionPath.Archaea ||
                   HasPlayerAdaptation(cell, V5AdaptationId.ProtonPump) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.ExtremophileMembrane) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.CatalaseROS);
        }

        private bool IsDigestStyle(V5GameManager gm, V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.HasPhagocytosis ||
                   cell.EvolutionPath == V5EvolutionPath.Amoeba ||
                   cell.EvolutionPath == V5EvolutionPath.Rotifer ||
                   HasPlayerAdaptation(cell, V5AdaptationId.Lysosome) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.Pseudopods);
        }

        private bool IsPiercingStyle(V5GameManager gm, V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.HasPiercingStylet || cell.EvolutionPath == V5EvolutionPath.Nematode;
        }

        private bool IsFlowStyle(V5GameManager gm, V5CellEntity cell)
        {
            if (cell == null) return false;
            return cell.EvolutionPath == V5EvolutionPath.Ciliate ||
                   cell.HasStructure(V5StructureId.Cilia) ||
                   cell.HasStructure(V5StructureId.CoronaCilia) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.Cilia);
        }

        private bool IsRepairStyle(V5GameManager gm, V5CellEntity cell)
        {
            if (cell == null) return false;
            return HasPlayerAdaptation(cell, V5AdaptationId.ContractileVacuole) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.CatalaseROS) ||
                   HasPlayerAdaptation(cell, V5AdaptationId.BiologicalChampion);
        }

        private bool HasPlayerAdaptation(V5CellEntity cell, V5AdaptationId id)
        {
            if (cell == null || !cell.IsPlayerOwned) return false;
            V5GameManager gm = V5GameManager.Instance;
            return gm != null && gm.Adaptations != null && gm.Adaptations.Has(id);
        }

        public bool TryEmergencyRepair()
        {
            if (Time.time < NextRepairReady) { Notify("Emergency Repair en cooldown"); return false; }
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return false;

            List<V5CellEntity> targets = GetTargets();
            if (targets.Count == 1 && targets[0] == gm.MotherCell)
            {
                // Mother-only cast becomes local colony repair.
                targets.Clear();
                IReadOnlyList<V5CellEntity> allies = gm.PlayerCells;
                for (int i = 0; i < allies.Count; i++)
                {
                    if (allies[i] != null && Vector2.Distance(allies[i].transform.position, gm.MotherCell.transform.position) < 5.5f) targets.Add(allies[i]);
                }
            }

            int repaired = 0;
            for (int i = 0; i < targets.Count; i++)
            {
                V5CellEntity cell = targets[i];
                if (cell == null) continue;
                V5CellEntity payer = gm.MotherCell != null ? gm.MotherCell : cell;
                if (payer.Resources.atp < 8f || payer.Resources.lipids < 3f) continue;
                payer.Resources.atp -= 8f;
                payer.Resources.lipids -= 3f;
                float amount = 22f + cell.Stats.repairPerSecond * 6f;
                cell.Stats.currentHp = Mathf.Min(cell.Stats.maxHp, cell.Stats.currentHp + amount);
                cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 10f);
                repaired++;
                PushLabel("+reparación", cell.transform.position, new Color(0.5f, 1f, 0.65f, 1f));
            }
            if (repaired <= 0) { Notify("Faltan ATP/lípidos para reparar"); return false; }
            NextRepairReady = Time.time + RepairCooldown;
            Notify("Emergency Repair: " + repaired + " células estabilizadas");
            return true;
        }

        public bool TryRouteFantasyAbility()
        {
            if (Time.time < NextRouteFantasyReady) { Notify("Habilidad de ruta en cooldown"); return false; }
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return false;

            V5MvpRoute route = ActiveRoute(gm);
            if (route == V5MvpRoute.None) { Notify("Elige una ruta MVP para activar fantasia de ruta"); return false; }

            int stage = V5MvpCanon.BuildStage(route, gm.Adaptations);
            if (gm.RouteBuilds != null)
            {
                gm.RouteBuilds.RefreshSnapshot(gm);
                stage = Mathf.Max(stage, gm.RouteBuilds.ActiveStage);
            }
            if (stage < 2) { Notify("Completa 2 pasos de build para activar " + V5MvpCanon.DisplayName(route)); return false; }

            V5CellEntity caster = PrimaryCaster(gm);
            V5CellEntity payer = gm.MotherCell;
            V5ResourceWallet cost = RouteFantasyCost(route, stage);
            if (!payer.Resources.CanPay(cost))
            {
                Notify("Faltan recursos para fantasia " + V5MvpCanon.DisplayName(route));
                return false;
            }

            payer.Resources.Pay(cost);
            float mastery = gm.RouteMastery != null ? gm.RouteMastery.Mastery01(route) : 0f;
            float power = 1f + Mathf.Clamp(stage, 1, 6) * 0.12f + mastery * 0.35f;
            string label = ApplyRouteFantasy(route, gm, caster != null ? caster : payer, power);
            if (string.IsNullOrEmpty(label)) return false;

            RouteFantasyCastCount++;
            NextRouteFantasyReady = Time.time + Mathf.Max(4f, RouteFantasyCooldown - mastery * 2.5f);
            Vector2 castCenter = caster != null ? caster.transform.position : payer.transform.position;
            bool combo = gm.WorldEvents != null && gm.WorldEvents.RegisterRouteAbilityCast(route, castCenter, power, label, gm);
            bool counterplay = gm.RouteCounters != null && gm.RouteCounters.RegisterRouteAbilityCounterplay(route, castCenter, power, label, gm);
            bool branchSynergy = gm.RouteBranches != null && gm.RouteBranches.RegisterRouteAbilitySynergy(route, castCenter, power, label, gm);
            V5RouteBranchId branch = gm.RouteBranches != null ? gm.RouteBranches.BranchForRoute(route) : V5RouteBranchId.None;
            string branchName = branchSynergy && gm.RouteBranches != null ? gm.RouteBranches.BranchName(branch) : "";
            string doctrineName = branchSynergy && gm.RouteBranches != null && gm.RouteBranches.HasBranchDoctrine(branch) ? gm.RouteBranches.DoctrineNameForBranch(branch) : "";
            LastRouteFantasyAbility = label + " | " + V5MvpCanon.DisplayName(route) + " build " + stage + " power " + power.ToString("0.0") + (combo ? " | combo oportunidad" : "") + (counterplay ? " | counterplay" : "") + (branchSynergy ? " | rama " + branchName : "") + (!string.IsNullOrEmpty(doctrineName) ? " | doctrina " + doctrineName : "");

            if (gm.AffinityLog != null) gm.AffinityLog.AddEvent(RouteToAffinityPath(route), 6f + stage, label, "route_ability");
            if (gm.Codex != null) gm.Codex.Unlock("Habilidad de ruta: " + V5MvpCanon.DisplayName(route), LastRouteFantasyAbility);
            Notify(LastRouteFantasyAbility);
            return true;
        }

        public string RouteFantasyStatus(V5GameManager gm)
        {
            V5MvpRoute route = ActiveRoute(gm);
            if (route == V5MvpRoute.None) return "Ruta: sin habilidad activa.";
            int stage = gm != null ? V5MvpCanon.BuildStage(route, gm.Adaptations) : 0;
            float ready = Mathf.Max(0f, NextRouteFantasyReady - Time.time);
            string state = stage >= 2 ? (ready <= 0f ? "lista" : ready.ToString("0.0") + "s") : "requiere build 2";
            string doctrine = "";
            if (gm != null && gm.RouteBranches != null)
            {
                V5RouteBranchId branch = gm.RouteBranches.BranchForRoute(route);
                if (gm.RouteBranches.HasBranchDoctrine(branch)) doctrine = " | doctrina " + gm.RouteBranches.DoctrineNameForBranch(branch);
            }
            return "Habilidad " + V5MvpCanon.DisplayName(route) + ": " + RouteFantasyName(route) + " | " + state + doctrine;
        }

        private V5CellEntity PrimaryCaster(V5GameManager gm)
        {
            List<V5CellEntity> targets = GetTargets();
            if (targets.Count > 0) return targets[0];
            return gm != null ? gm.MotherCell : null;
        }

        private V5MvpRoute ActiveRoute(V5GameManager gm)
        {
            if (gm == null) return V5MvpRoute.None;
            if (gm.MvpIntent != null) return gm.MvpIntent.EffectiveRoute(gm);
            return V5MvpCanon.CurrentRoute(gm);
        }

        private V5ResourceWallet RouteFantasyCost(V5MvpRoute route, int stage)
        {
            float scale = Mathf.Clamp(stage, 2, 6) - 2f;
            switch (route)
            {
                case V5MvpRoute.Bacteria: return V5ResourceWallet.Cost(18f + scale * 2f, 8f, 0f, 0f, 0f, 0f);
                case V5MvpRoute.Amoeba: return V5ResourceWallet.Cost(20f + scale * 2f, 6f, 6f, 0f, 0f, 0f);
                case V5MvpRoute.PhotosyntheticProducer: return V5ResourceWallet.Cost(16f, 4f, 0f, 0f, 0f, 4f + scale);
                case V5MvpRoute.Volvox: return V5ResourceWallet.Cost(22f + scale * 2f, 8f, 0f, 5f, 3f, 0f);
                default: return V5ResourceWallet.Cost(18f, 6f, 0f, 0f, 0f, 0f);
            }
        }

        private string ApplyRouteFantasy(V5MvpRoute route, V5GameManager gm, V5CellEntity caster, float power)
        {
            if (gm == null || caster == null) return "";
            Vector2 pos = caster.transform.position;
            switch (route)
            {
                case V5MvpRoute.Bacteria:
                    return ApplyBacteriaFantasy(gm, caster, pos, power);
                case V5MvpRoute.Amoeba:
                    return ApplyAmoebaFantasy(gm, caster, pos, power);
                case V5MvpRoute.PhotosyntheticProducer:
                    return ApplyProducerFantasy(gm, caster, pos, power);
                case V5MvpRoute.Volvox:
                    return ApplyVolvoxFantasy(gm, caster, pos, power);
                default:
                    return "";
            }
        }

        private string ApplyBacteriaFantasy(V5GameManager gm, V5CellEntity caster, Vector2 pos, float power)
        {
            float radius = 4.2f + power;
            if (gm.Environment != null)
                gm.Environment.ModifyArea(pos, radius, 0.010f * power, 0f, 0.006f * power, -0.020f * power, 0f, 0.070f * power, 0.004f * power);
            caster.Resources.biomass += 10f * power;
            caster.Stats.stress = Mathf.Max(0f, caster.Stats.stress - 5f * power);
            PushLabel("marea de biofilm", pos, new Color(0.50f, 1f, 0.72f, 1f));
            return "Marea de Biofilm";
        }

        private string ApplyAmoebaFantasy(V5GameManager gm, V5CellEntity caster, Vector2 pos, float power)
        {
            float radius = 3.8f + power * 0.8f;
            DamageEnemiesAround(gm, pos, radius, 16f * power + caster.Stats.physicalDamagePerSecond * 2.5f, V5DamageKind.Physical);
            if (gm.Environment != null)
                gm.Environment.ModifyArea(pos, radius, 0.030f * power, 0f, 0.004f, -0.012f, 0f, 0.010f, 0.060f * power);
            caster.Resources.biomass += 8f * power;
            caster.Resources.aminoAcids += 12f * power;
            PushLabel("fagocitosis alfa", pos, new Color(1f, 0.68f, 0.88f, 1f));
            return "Fagocitosis Alfa";
        }

        private string ApplyProducerFantasy(V5GameManager gm, V5CellEntity caster, Vector2 pos, float power)
        {
            float radius = 6.2f + power;
            if (gm.Environment != null)
                gm.Environment.ModifyArea(pos, radius, 0.018f * power, 0.030f * power, 0.110f * power, -0.050f * power, -0.012f, 0.040f * power, 0f);
            caster.Resources.atp += 22f * power;
            caster.Resources.minerals += 5f * power;
            caster.Stats.synthesisRate += 0.025f * power;
            PushLabel("bloom fotosintetico", pos, new Color(0.78f, 1f, 0.42f, 1f));
            return "Bloom Fotosintetico";
        }

        private string ApplyVolvoxFantasy(V5GameManager gm, V5CellEntity caster, Vector2 pos, float power)
        {
            int affected = 0;
            IReadOnlyList<V5CellEntity> allies = gm.PlayerCells;
            for (int i = 0; i < allies.Count; i++)
            {
                V5CellEntity c = allies[i];
                if (c == null) continue;
                bool inBody = c.Role == V5CellRole.Mother || c.IsAttachedToBody || Vector2.Distance(c.transform.position, pos) <= 5.5f;
                if (!inBody) continue;
                c.Stats.currentHp = Mathf.Min(c.Stats.maxHp, c.Stats.currentHp + 12f * power);
                c.Stats.stress = Mathf.Max(0f, c.Stats.stress - 9f * power);
                if (c.Role != V5CellRole.Mother) c.Directive = V5Directive.Defend;
                affected++;
            }
            if (gm.Environment != null)
                gm.Environment.ModifyArea(pos, 5.5f + power, 0.010f, 0.006f * power, 0.025f * power, -0.018f * power, 0f, 0.040f * power, 0f);
            if (gm.Body != null) gm.Body.LastMessage = "Sincronia Volvox: " + affected + " celulas sincronizadas.";
            caster.Resources.lipids += 7f * power;
            caster.Resources.nucleotides += 5f * power;
            PushLabel("sincronia volvox", pos, new Color(0.62f, 0.90f, 1f, 1f));
            return "Sincronia Volvox";
        }

        private string RouteFantasyName(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return "Marea de Biofilm";
                case V5MvpRoute.Amoeba: return "Fagocitosis Alfa";
                case V5MvpRoute.PhotosyntheticProducer: return "Bloom Fotosintetico";
                case V5MvpRoute.Volvox: return "Sincronia Volvox";
                default: return "sin habilidad";
            }
        }

        private V5EvolutionPath RouteToAffinityPath(V5MvpRoute route)
        {
            switch (route)
            {
                case V5MvpRoute.Bacteria: return V5EvolutionPath.Bacteria;
                case V5MvpRoute.Amoeba: return V5EvolutionPath.Amoeba;
                case V5MvpRoute.PhotosyntheticProducer: return V5EvolutionPath.Cyanobacteria;
                case V5MvpRoute.Volvox: return V5EvolutionPath.Microalga;
                default: return V5EvolutionPath.Uncommitted;
            }
        }

        private void AddBuff(V5CellEntity cell, float duration, float speed, float synthesis, float atpBonus)
        {
            if (cell == null) return;
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                if (activeBuffs[i].cell == cell)
                {
                    RemoveBuff(activeBuffs[i]);
                    activeBuffs.RemoveAt(i);
                }
            }
            TimedBuff b = new TimedBuff();
            b.cell = cell;
            b.endsAt = Time.time + duration;
            b.speedMultiplier = speed;
            b.synthesisMultiplier = synthesis;
            b.atpBonus = atpBonus;
            cell.Stats.speed *= speed;
            cell.Stats.synthesisRate *= synthesis;
            cell.Stats.atpPerSecond += atpBonus;
            activeBuffs.Add(b);
            PushLabel("surge", cell.transform.position, new Color(1f, 0.86f, 0.35f, 1f));
        }

        private void TickBuffs()
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                TimedBuff b = activeBuffs[i];
                if (b.cell == null || Time.time >= b.endsAt)
                {
                    RemoveBuff(b);
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        private void RemoveBuff(TimedBuff b)
        {
            if (b == null || b.cell == null) return;
            b.cell.Stats.speed /= Mathf.Max(0.01f, b.speedMultiplier);
            b.cell.Stats.synthesisRate /= Mathf.Max(0.01f, b.synthesisMultiplier);
            b.cell.Stats.atpPerSecond -= b.atpBonus;
        }

        private void DamageEnemiesAround(V5GameManager gm, Vector2 center, float radius, float damage, V5DamageKind kind)
        {
            IReadOnlyList<V5CellEntity> enemies = gm.NonPlayerCells;
            for (int i = 0; i < enemies.Count; i++)
            {
                V5CellEntity e = enemies[i];
                if (e == null) continue;
                float d = Vector2.Distance(center, e.transform.position);
                if (d <= radius)
                {
                    float falloff = 1f - Mathf.Clamp01(d / Mathf.Max(0.01f, radius));
                    e.Damage(damage * (0.35f + falloff), kind, center);
                }
            }
        }

        private void Notify(string message)
        {
            LastAbilityMessage = message;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(message);
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null)
            {
                Vector2 p = gm != null && gm.MotherCell != null ? (Vector2)gm.MotherCell.transform.position : Vector2.zero;
                feedback.Push(message, p, new Color(0.85f, 0.95f, 1f, 1f));
                feedback.Ping("structure");
            }
        }

        private void PushLabel(string message, Vector2 world, Color color)
        {
            V5FeedbackSystem feedback = FindFirstObjectByType<V5FeedbackSystem>();
            if (feedback != null) feedback.Push(message, world, color);
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            float width = 430f;
            Rect r = new Rect(Screen.width - width - 16f, Screen.height - 326f, width, 180f);
            GUI.Box(r, GUIContent.none, panelStyle);
            GUILayout.BeginArea(new Rect(r.x + 12f, r.y + 8f, r.width - 24f, r.height - 16f));
            GUILayout.Label("CONDUCTAS BIOACTIVAS", titleStyle);
            GUILayout.Label("Impulso " + CooldownText(NextSurgeReady) + " | Senal " + CooldownText(NextPulseReady) + " | Reparar " + CooldownText(NextRepairReady) + " | Ruta " + CooldownText(NextRouteFantasyReady), smallStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Impulso")) TryMetabolicSurge();
            if (GUILayout.Button("Senal")) TryEcologicalPulse();
            if (GUILayout.Button("Reparar")) TryEmergencyRepair();
            if (GUILayout.Button("Ruta")) TryRouteFantasyAbility();
            GUILayout.EndHorizontal();
            GUILayout.Label(RouteFantasyStatus(V5GameManager.Instance), smallStyle);
            GUILayout.Label("Las conductas usan la identidad y adaptaciones instaladas; Q/W/R/E solo funcionan con este panel abierto salvo override debug.", smallStyle);
            GUILayout.Label("Usa la seleccion RTS; sin seleccion usa la madre. K cierra.", smallStyle);
            if (!string.IsNullOrEmpty(LastAbilityMessage)) GUILayout.Label(LastAbilityMessage, smallStyle);
            GUILayout.EndArea();
        }

        private string CooldownText(float readyAt)
        {
            float left = readyAt - Time.time;
            return left <= 0f ? "READY" : left.ToString("0.0") + "s";
        }

        private void EnsureStyles()
        {
            if (panelStyle != null) return;
            panelStyle = new GUIStyle(GUI.skin.box);
            panelTexture = MakeTexture(new Color(0.04f, 0.06f, 0.07f, 0.94f));
            panelStyle.normal.background = panelTexture;
            panelStyle.normal.textColor = Color.white;
            titleStyle = new GUIStyle(GUI.skin.label);
            titleStyle.fontStyle = FontStyle.Bold;
            titleStyle.normal.textColor = new Color(0.75f, 1f, 0.9f, 1f);
            smallStyle = new GUIStyle(GUI.skin.label);
            smallStyle.wordWrap = true;
            smallStyle.normal.textColor = new Color(0.9f, 0.96f, 1f, 1f);
        }

        private Texture2D MakeTexture(Color color)
        {
            Texture2D tex = new Texture2D(1, 1);
            tex.SetPixel(0, 0, color);
            tex.Apply();
            return tex;
        }
    }
}
