using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5AdaptationSystem : MonoBehaviour, IV5RunResettable
    {
        public const int BaseActiveCap = 14;
        public const int ApexActiveCap = 16;
        public string LastMessage = "Adaptaciones listas.";

        private readonly List<V5AdaptationId> installed = new List<V5AdaptationId>(32);

        public IReadOnlyList<V5AdaptationId> Installed { get { return installed; } }

        public int ActiveCap
        {
            get
            {
                V5GameManager gm = V5GameManager.Instance;
                bool apexOnline = Has(V5AdaptationId.BiologicalChampion) || (gm != null && gm.Apex != null && gm.Apex.ApexSpawned);
                return apexOnline ? ApexActiveCap : BaseActiveCap;
            }
        }

        public void ResetForNewRun()
        {
            installed.Clear();
            LastMessage = "Adaptaciones reiniciadas.";
        }

        public void RestoreInstalled(List<V5AdaptationId> saved)
        {
            installed.Clear();
            if (saved != null)
            {
                for (int i = 0; i < saved.Count; i++)
                {
                    V5AdaptationDefinition def = V5AdaptationLibrary.Get(saved[i]);
                    if (def != null && !installed.Contains(saved[i])) installed.Add(saved[i]);
                }
            }
            LastMessage = "Adaptaciones restauradas: " + installed.Count + ".";
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Identity != null) gm.Identity.Recalculate("quicksave");
        }

        public bool Has(V5AdaptationId id)
        {
            return installed.Contains(id);
        }

        public int ActiveCount()
        {
            int count = 0;
            for (int i = 0; i < installed.Count; i++)
            {
                V5AdaptationDefinition def = V5AdaptationLibrary.Get(installed[i]);
                if (def != null && def.countsTowardCap) count++;
            }
            return count;
        }

        public int CountInTier(V5AdaptationTier tier)
        {
            int count = 0;
            for (int i = 0; i < installed.Count; i++)
            {
                V5AdaptationDefinition def = V5AdaptationLibrary.Get(installed[i]);
                if (def != null && def.tier == tier) count++;
            }
            return count;
        }

        public bool CanInstall(V5AdaptationId id, V5CellEntity cell, out string reason)
        {
            reason = "";
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            if (def == null || id == V5AdaptationId.None)
            {
                reason = "Adaptacion desconocida.";
                return false;
            }
            if (cell == null)
            {
                reason = "No hay celula madre.";
                return false;
            }
            if (Has(id))
            {
                reason = "Ya instalada.";
                return false;
            }
            if (def.countsTowardCap && ActiveCount() >= ActiveCap)
            {
                reason = "Cap activo lleno: " + ActiveCount() + "/" + ActiveCap + ".";
                return false;
            }
            for (int i = 0; i < def.prerequisites.Length; i++)
            {
                if (!Has(def.prerequisites[i]))
                {
                    V5AdaptationDefinition missing = V5AdaptationLibrary.Get(def.prerequisites[i]);
                    reason = "Requiere " + (missing != null ? missing.shortName : def.prerequisites[i].ToString()) + ".";
                    return false;
                }
            }
            if (!SpecialGate(def, out reason)) return false;
            if (!cell.Resources.CanPay(def.cost))
            {
                reason = "Faltan recursos: " + CostText(def.cost) + ".";
                return false;
            }
            reason = "Lista.";
            return true;
        }

        public bool Install(V5AdaptationId id, V5CellEntity cell)
        {
            string reason;
            if (!CanInstall(id, cell, out reason))
            {
                LastMessage = reason;
                Toast(reason);
                V5GameManager blockedGm = V5GameManager.Instance;
                if (blockedGm != null && blockedGm.Telemetry != null)
                    blockedGm.Telemetry.RecordAdaptationInstallFailed(id, reason);
                return false;
            }

            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            cell.Resources.Pay(def.cost);
            installed.Add(id);
            ApplyToMother(def, cell);
            LastMessage = "Adaptacion instalada: " + def.displayName + ".";
            Toast(LastMessage);

            V5GameManager gm = V5GameManager.Instance;
            if (gm != null)
            {
                if (gm.Codex != null) gm.Codex.ObserveAdaptation(def);
                if (gm.Identity != null) gm.Identity.Recalculate("adaptacion " + def.shortName);
                if (gm.Telemetry != null) gm.Telemetry.RecordAdaptationInstalled(def, ActiveCount(), ActiveCap);
            }
            return true;
        }

        public float DivisionCostMultiplier(V5CellEntity cell)
        {
            return Has(V5AdaptationId.RapidDivision) ? 0.72f : 1f;
        }

        public float InheritanceChance(V5CellEntity cell, float fallback)
        {
            if (Has(V5AdaptationId.CellDifferentiation)) return Mathf.Max(fallback, V5Balance.StrongInheritanceChance);
            if (Has(V5AdaptationId.PersistentAdhesion)) return Mathf.Max(fallback, 0.52f);
            return fallback;
        }

        public float ColonizationMultiplier
        {
            get
            {
                float value = 1f;
                if (Has(V5AdaptationId.BasicAdhesin)) value += 0.18f;
                if (Has(V5AdaptationId.PiliFimbriae)) value += 0.18f;
                if (Has(V5AdaptationId.ColonialAdhesin)) value += 0.32f;
                if (Has(V5AdaptationId.SignalingCommunication)) value += 0.18f;
                return value;
            }
        }

        public float DeathRecycleMultiplier()
        {
            float value = 1f;
            if (Has(V5AdaptationId.FungalHypha)) value += 0.20f;
            if (Has(V5AdaptationId.ExtracellularEnzymes)) value += 0.25f;
            if (Has(V5AdaptationId.SlimePlasmodium)) value += 0.15f;
            return value;
        }

        public string Summary()
        {
            return "Adaptaciones " + installed.Count + " total | activas " + ActiveCount() + "/" + ActiveCap;
        }

        public string CostText(V5ResourceWallet cost)
        {
            return cost.atp.ToString("0") + " ATP, " + cost.biomass.ToString("0") + " Bio";
        }

        public V5AdaptationId FirstMissingPrerequisite(V5AdaptationId id)
        {
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            if (def == null || def.prerequisites == null) return V5AdaptationId.None;
            for (int i = 0; i < def.prerequisites.Length; i++)
                if (!Has(def.prerequisites[i])) return def.prerequisites[i];
            return V5AdaptationId.None;
        }

        public string PrerequisiteChecklist(V5AdaptationId id)
        {
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            if (def == null || def.prerequisites == null || def.prerequisites.Length == 0) return "sin prerequisitos";
            string result = "";
            for (int i = 0; i < def.prerequisites.Length; i++)
            {
                V5AdaptationDefinition prerequisite = V5AdaptationLibrary.Get(def.prerequisites[i]);
                if (result.Length > 0) result += " | ";
                result += (Has(def.prerequisites[i]) ? "[x] " : "[ ] ") + (prerequisite != null ? prerequisite.shortName : def.prerequisites[i].ToString());
            }
            return result;
        }

        public string ResourceChecklist(V5AdaptationId id, V5CellEntity cell)
        {
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            if (def == null) return "adaptacion desconocida";
            if (cell == null) return "sin madre";
            string result = "";
            AppendResourceState(ref result, "ATP", cell.Resources.atp, def.cost.atp);
            AppendResourceState(ref result, "Bio", cell.Resources.biomass, def.cost.biomass);
            return result.Length > 0 ? result : "sin costo";
        }

        public string MissingResourceSummary(V5AdaptationId id, V5CellEntity cell)
        {
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            if (def == null) return "";
            if (cell == null) return "madre";
            string result = "";
            AppendMissingResource(ref result, "ATP", cell.Resources.atp, def.cost.atp);
            AppendMissingResource(ref result, "Bio", cell.Resources.biomass, def.cost.biomass);
            return result;
        }

        public string NextStepFor(V5AdaptationId id, V5CellEntity cell)
        {
            V5AdaptationDefinition def = V5AdaptationLibrary.Get(id);
            if (def == null || id == V5AdaptationId.None) return "Elige una adaptacion valida.";
            if (cell == null) return "No hay celula madre.";
            if (Has(id)) return "Ya instalada.";
            if (def.countsTowardCap && ActiveCount() >= ActiveCap) return "Cap activo lleno: prioriza la ruta antes de instalar extras.";

            V5AdaptationId missing = FirstMissingPrerequisite(id);
            if (missing != V5AdaptationId.None)
            {
                V5AdaptationDefinition missingDef = V5AdaptationLibrary.Get(missing);
                return "Instala primero " + (missingDef != null ? missingDef.shortName : missing.ToString()) + ".";
            }

            string gateReason;
            if (!SpecialGate(def, out gateReason)) return gateReason;

            string resources = MissingResourceSummary(id, cell);
            if (!string.IsNullOrEmpty(resources)) return "Farmea " + resources + ".";
            return "Lista para instalar.";
        }

        public string ExplainInstall(V5AdaptationId id, V5CellEntity cell)
        {
            string reason;
            bool can = CanInstall(id, cell, out reason);
            string state = can ? "Lista." : reason;
            return "Estado: " + state + " | Requisitos: " + PrerequisiteChecklist(id) +
                   " | Recursos: " + ResourceChecklist(id, cell) + " | Paso: " + NextStepFor(id, cell);
        }

        public string InstalledNames()
        {
            if (installed.Count == 0) return "Ninguna.";
            string result = "";
            for (int i = 0; i < installed.Count; i++)
            {
                V5AdaptationDefinition def = V5AdaptationLibrary.Get(installed[i]);
                if (def == null) continue;
                if (result.Length > 0) result += ", ";
                result += def.shortName;
            }
            return result;
        }

        private void AppendResourceState(ref string result, string label, float current, float required)
        {
            if (required <= 0f) return;
            if (result.Length > 0) result += " | ";
            result += (current >= required ? "[x] " : "[ ] ") + label + " " + current.ToString("0") + "/" + required.ToString("0");
        }

        private void AppendMissingResource(ref string result, string label, float current, float required)
        {
            float missing = required - current;
            if (missing <= 0f) return;
            if (result.Length > 0) result += ", ";
            result += label + " +" + missing.ToString("0");
        }

        private bool SpecialGate(V5AdaptationDefinition def, out string reason)
        {
            reason = "";
            if (def.id == V5AdaptationId.Nucleus && CountInTier(V5AdaptationTier.T1Prokaryote) < 2)
            {
                reason = "Requiere 2 adaptaciones tempranas antes del Nucleo.";
                return false;
            }
            if (def.id == V5AdaptationId.BiologicalChampion)
            {
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.Apex != null && gm.Apex.ApexSpawned)
                {
                    reason = "Ya existe una forma apex.";
                    return false;
                }
            }
            return true;
        }

        private void ApplyToMother(V5AdaptationDefinition def, V5CellEntity cell)
        {
            for (int i = 0; i < def.legacyStructures.Length; i++)
            {
                if (!cell.HasStructure(def.legacyStructures[i])) cell.ForceInstallStructure(def.legacyStructures[i]);
            }

            if (def.legacyMetabolism != V5MetabolismType.None) cell.ApplyMetabolism(def.legacyMetabolism);

            ApplyDirectStats(def.id, cell);

            if (def.id == V5AdaptationId.Nucleus)
            {
                cell.Domain = V5CellDomain.Eukaryote;
            }
            else if (def.id == V5AdaptationId.ExtremophileMembrane || def.id == V5AdaptationId.ProtonPump ||
                     def.id == V5AdaptationId.BacterialWall || def.id == V5AdaptationId.BacterialFlagellum ||
                     def.id == V5AdaptationId.ProkaryoticThylakoid)
            {
                if (cell.Domain == V5CellDomain.LUCA) cell.Domain = V5CellDomain.Prokaryote;
            }

            if (def.id == V5AdaptationId.BiologicalChampion)
            {
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.Apex != null) gm.Apex.SpawnRecommended();
            }
        }

        private void ApplyDirectStats(V5AdaptationId id, V5CellEntity cell)
        {
            if (id == V5AdaptationId.BasicAdhesin)
            {
                cell.Stats.colonizationPower += 0.20f;
            }
            else if (id == V5AdaptationId.RapidDivision)
            {
                cell.Stats.divisionEfficiency = Mathf.Max(cell.Stats.divisionEfficiency, 1.20f);
            }
            else if (id == V5AdaptationId.ExtremophileMembrane)
            {
                cell.Stats.thermalResistance += 0.26f;
                cell.Stats.phTolerance = Mathf.Clamp01(cell.Stats.phTolerance + 0.14f);
                cell.Stats.toxinResistance += 0.08f;
            }
            else if (id == V5AdaptationId.ProtonPump)
            {
                cell.Stats.atpPerSecond += 0.25f;
                cell.Stats.phTolerance = Mathf.Clamp01(cell.Stats.phTolerance + 0.08f);
            }
            else if (id == V5AdaptationId.Nucleus)
            {
                cell.Stats.sensorRange += 0.8f;
                cell.Stats.maxBiomassLoad += 12f;
            }
            else if (id == V5AdaptationId.Pseudopods)
            {
                cell.Stats.physicalDamagePerSecond += 0.55f;
                cell.Stats.attackRange = Mathf.Max(cell.Stats.attackRange, 1.35f);
                cell.Stats.speed *= 0.96f;
            }
            else if (id == V5AdaptationId.ContractileVacuole)
            {
                cell.Stats.phTolerance = Mathf.Clamp01(cell.Stats.phTolerance + 0.10f);
                cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 8f);
            }
            else if (id == V5AdaptationId.SilicaFrustule)
            {
                cell.Stats.maxHp += 34f;
                cell.Stats.currentHp += 34f;
                cell.Stats.toxinResistance += 0.12f;
                cell.Stats.speed *= 0.82f;
            }
            else if (id == V5AdaptationId.ColonialAdhesin)
            {
                cell.Stats.colonizationPower += 0.35f;
                cell.Stats.maxHp += 10f;
                cell.Stats.currentHp += 10f;
            }
            else if (id == V5AdaptationId.ExtracellularEnzymes)
            {
                cell.Stats.synthesisRate *= 1.16f;
                cell.Stats.colonizationPower += 0.16f;
                cell.Stats.chemicalDamagePerSecond += 0.15f;
            }
            else if (id == V5AdaptationId.ChemicalMemory)
            {
                cell.Stats.sensorRange += 4f;
                cell.Stats.colonizationPower += 0.18f;
            }
            else if (id == V5AdaptationId.PersistentAdhesion)
            {
                cell.Stats.maxHp += 18f;
                cell.Stats.currentHp += 18f;
                cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 6f);
            }
            else if (id == V5AdaptationId.CellDifferentiation)
            {
                cell.Stats.divisionEfficiency = Mathf.Max(cell.Stats.divisionEfficiency, 1.25f);
                cell.Stats.sensorRange += 1.2f;
            }
            else if (id == V5AdaptationId.SignalingCommunication)
            {
                cell.Stats.sensorRange += 2.5f;
                cell.Stats.repairPerSecond += 0.25f;
            }
        }

        private void Toast(string message)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(message);
        }
    }
}
