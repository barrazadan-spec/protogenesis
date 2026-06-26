using UnityEngine;

namespace Protogenesis.V5
{
    public class V5CellFactory : MonoBehaviour
    {
        public V5CellEntity SpawnMother(Vector2 position)
        {
            V5CellEntity cell = CreateCell("MotherCell", position);
            cell.Initialize(V5CellRole.Mother, true, V5EvolutionPath.Uncommitted, position);
            cell.IsNexus = true;
            if (V5GameManager.Instance != null && V5GameManager.Instance.CoreMode)
            {
                cell.Stats.maxHp = 1200f;
                cell.Stats.currentHp = 1200f;
                cell.Stats.speed = Mathf.Max(cell.Stats.speed, 3.35f);
            }
            cell.Mother = cell;
            V5GameManager.Instance.MotherCell = cell;
            cell.ForceInstallStructure(V5StructureId.GeneticCompartment);
            if (V5GameManager.Instance.CameraController != null) V5GameManager.Instance.CameraController.FollowTarget = cell.transform;
            return cell;
        }

        public V5CellEntity SpawnPlayerCell(Vector2 position, V5CellRole role, V5CellEntity parent)
        {
            V5CellEntity cell = CreateCell(role.ToString(), position);
            cell.Initialize(role, true, parent.EvolutionPath, position);
            cell.Generation = parent.Generation + 1;
            float inheritance = parent.HasStructure(V5StructureId.StemPlasticity) ? V5Balance.StrongInheritanceChance : V5Balance.DaughterInheritanceChance;
            if (V5GameManager.Instance != null && V5GameManager.Instance.Genes != null) inheritance = V5GameManager.Instance.Genes.InheritanceChance(parent, inheritance);
            if (V5GameManager.Instance != null && V5GameManager.Instance.Adaptations != null) inheritance = V5GameManager.Instance.Adaptations.InheritanceChance(parent, inheritance);
            cell.InheritFrom(parent, inheritance);
            if (parent.Role == V5CellRole.Mother)
            {
                cell.SetFunctionalCaste(V5FunctionalCasteId.Hybrid);
            }
            else
            {
                cell.PhenotypeLabel = parent.PhenotypeLabel;
                cell.PhenotypeCaste = parent.PhenotypeCaste;
                cell.PhenotypeRecipeCode = parent.PhenotypeRecipeCode;
                cell.PhenotypeRecipeSummary = parent.PhenotypeRecipeSummary;
                cell.SetFunctionalCaste(parent.FunctionalCaste);
            }
            cell.Mother = parent.Role == V5CellRole.Mother ? parent : parent.Mother;
            return cell;
        }

        public V5CellEntity SpawnGerminalCell(Vector2 position, V5CellEntity mother, V5GerminalCasteDefinition def)
        {
            return SpawnGerminalCell(position, mother, def, V5CellDeploymentMode.FreeSquad);
        }

        public V5CellEntity SpawnGerminalCell(Vector2 position, V5CellEntity mother, V5GerminalCasteDefinition def, V5CellDeploymentMode deploymentMode)
        {
            if (mother == null || def == null) return null;
            V5GerminalProductionSystem germinal = V5GameManager.Instance != null ? V5GameManager.Instance.Germinal : null;
            V5EvolutionPath path = germinal != null ? germinal.ResolveTargetPath(def, mother) : (def.targetPath == V5EvolutionPath.Uncommitted ? mother.EvolutionPath : def.targetPath);
            V5CellEntity cell = CreateCell("Germinal_" + def.id, position);
            cell.Initialize(V5CellRole.Daughter, true, path, position);
            cell.Generation = 1;
            float inheritance = mother.HasStructure(V5StructureId.StemPlasticity) ? V5Balance.StrongInheritanceChance : V5Balance.DaughterInheritanceChance;
            if (V5GameManager.Instance != null && V5GameManager.Instance.Genes != null) inheritance = V5GameManager.Instance.Genes.InheritanceChance(mother, inheritance);
            if (V5GameManager.Instance != null && V5GameManager.Instance.Adaptations != null) inheritance = V5GameManager.Instance.Adaptations.InheritanceChance(mother, inheritance);
            cell.InheritFrom(mother, inheritance);
            cell.ApplyPath(path, true);
            for (int i = 0; i < def.grantedStructures.Length; i++)
                if (!cell.HasStructure(def.grantedStructures[i])) cell.ForceInstallStructure(def.grantedStructures[i]);
            cell.ApplyPath(path, true);
            cell.Mother = mother;
            cell.LineageRole = def.lineageRole;
            cell.Directive = def.defaultDirective;
            cell.SyncModeFromDirective();
            cell.PhenotypeLabel = def.displayName;
            ApplyGerminalStats(cell, def);
            V5PhenotypeRecipeLibrary.ApplyToCell(cell, def, mother, path);
            if (deploymentMode == V5CellDeploymentMode.AttachedBody)
            {
                cell.Directive = V5Directive.FollowMother;
                cell.SyncModeFromDirective();
            }
            return cell;
        }

        private void ApplyGerminalStats(V5CellEntity cell, V5GerminalCasteDefinition def)
        {
            cell.Stats.maxHp = Mathf.Max(8f, cell.Stats.maxHp * Mathf.Max(0.2f, def.hpMultiplier));
            cell.Stats.currentHp = cell.Stats.maxHp;
            cell.Stats.speed *= Mathf.Max(0.2f, def.speedMultiplier);
            cell.Stats.synthesisRate *= Mathf.Max(0.2f, def.synthesisMultiplier);
            cell.Stats.physicalDamagePerSecond = Mathf.Max(0f, cell.Stats.physicalDamagePerSecond + def.physicalBonus);
            cell.Stats.chemicalDamagePerSecond = Mathf.Max(0f, cell.Stats.chemicalDamagePerSecond + def.chemicalBonus);
            cell.Stats.sensorRange = Mathf.Max(1f, cell.Stats.sensorRange + def.sensorBonus);
            cell.Stats.colonizationPower = Mathf.Max(0f, cell.Stats.colonizationPower + def.colonizationBonus);
        }

        public V5CellEntity SpawnNeutral(Vector2 position, V5EvolutionPath path)
        {
            V5CellEntity cell = CreateCell("Neutral_" + path, position);
            cell.Initialize(V5CellRole.Enemy, false, path, position);
            cell.Resources.atp = 35f;
            cell.Resources.biomass = 20f;
            return cell;
        }

        public V5CellEntity SpawnApex(Vector2 position, V5ApexForm form, V5CellEntity mother)
        {
            V5EvolutionPath path = V5EvolutionPath.Amoeba;
            if (form == V5ApexForm.Volvox) path = V5EvolutionPath.Cyanobacteria;
            else if (form == V5ApexForm.Hydra) path = V5EvolutionPath.Fungus;
            else if (form == V5ApexForm.Tardigrade) path = V5EvolutionPath.Tardigrade;
            else if (form == V5ApexForm.Paramecium) path = V5EvolutionPath.Ciliate;

            V5CellEntity cell = CreateCell("Apex_" + form, position);
            cell.Initialize(V5CellRole.Apex, true, path, position);
            cell.Generation = 0;
            cell.Mother = mother != null ? mother : cell;
            cell.Domain = mother != null ? mother.Domain : cell.Domain;
            cell.Metabolism = mother != null ? mother.Metabolism : cell.Metabolism;
            cell.Stats.maxHp = 300f;
            cell.Stats.currentHp = 300f;
            cell.Stats.radius *= 1.75f;
            cell.Stats.speed *= form == V5ApexForm.Tardigrade ? 0.75f : 0.95f;
            cell.Stats.sensorRange += 3f;
            cell.Stats.physicalDamagePerSecond += form == V5ApexForm.Tardigrade ? 4f : 1.5f;
            cell.Stats.chemicalDamagePerSecond += form == V5ApexForm.Hydra ? 2.5f : 0.6f;
            cell.Stats.colonizationPower += form == V5ApexForm.Volvox ? 2.2f : 0.8f;
            if (form == V5ApexForm.Paramecium) cell.Stats.synthesisRate *= 1.45f;
            if (form == V5ApexForm.Hydra) cell.ForceInstallStructure(V5StructureId.InvasiveHypha);
            if (form == V5ApexForm.Volvox) cell.ForceInstallStructure(V5StructureId.Thylakoid);
            if (form == V5ApexForm.Tardigrade)
            {
                cell.ForceInstallStructure(V5StructureId.Catalase);
                cell.ForceInstallStructure(V5StructureId.Cuticle);
                cell.ForceInstallStructure(V5StructureId.CryptobiosisTun);
            }
            if (form == V5ApexForm.Paramecium) cell.ForceInstallStructure(V5StructureId.Cilia);
            cell.Directive = V5Directive.Defend;
            cell.SyncModeFromDirective();
            return cell;
        }

        public V5CellEntity SpawnFromSave(V5SavedCell saved)
        {
            if (saved == null) return null;
            Vector2 pos = new Vector2(saved.x, saved.y);
            V5CellEntity cell = CreateCell(saved.role.ToString() + "_Loaded", pos);
            cell.Initialize(saved.role, saved.player, saved.path, pos);
            cell.Domain = saved.domain;
            cell.EvolutionPath = saved.path;
            cell.Metabolism = saved.metabolism;
            cell.LineageRole = saved.lineageRole;
            cell.Resources = saved.resources;
            cell.Stats = saved.stats;
            cell.FunctionalCaste = saved.functionalCaste;
            cell.RestoreSavedStructures(saved.structures);
            cell.SyncModeFromDirective();
            if (saved.player && saved.role == V5CellRole.Mother)
            {
                V5GameManager.Instance.MotherCell = cell;
                cell.Mother = cell;
                cell.IsNexus = true;
                if (V5GameManager.Instance.CameraController != null) V5GameManager.Instance.CameraController.FollowTarget = cell.transform;
            }
            return cell;
        }

        private V5CellEntity CreateCell(string name, Vector2 position)
        {
            GameObject go = new GameObject("V5_" + name);
            go.transform.position = position;
            V5CellEntity cell = go.AddComponent<V5CellEntity>();
            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.linearDamping = 1.5f;
            rb.angularDamping = 4f;
            return cell;
        }
    }
}
