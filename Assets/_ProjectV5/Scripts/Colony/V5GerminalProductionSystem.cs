using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5GerminalCasteDefinition
    {
        public V5GerminalCasteId id;
        public string displayName;
        public string category;
        public string plateText;
        public string tacticalRole;
        public V5EvolutionPath targetPath;
        public V5LineageRole lineageRole;
        public V5Directive defaultDirective;
        public V5ResourceWallet cost;
        public bool auxiliary;
        public float stressCost;
        public float hpMultiplier;
        public float speedMultiplier;
        public float synthesisMultiplier;
        public float physicalBonus;
        public float chemicalBonus;
        public float sensorBonus;
        public float colonizationBonus;
        public V5GeneId requiredGene;
        public V5StructureId[] requiredAnyStructure;
        public V5AdaptationId[] requiredAnyAdaptation;
        public V5AdaptationId[] requiredAllAdaptations;
        public V5StructureId[] grantedStructures;

        public V5GerminalCasteDefinition(V5GerminalCasteId id, string displayName, string category, string plateText, string tacticalRole, V5EvolutionPath targetPath, V5LineageRole lineageRole, V5Directive defaultDirective, V5ResourceWallet cost, bool auxiliary, float stressCost, float hpMultiplier, float speedMultiplier, float synthesisMultiplier, float physicalBonus, float chemicalBonus, float sensorBonus, float colonizationBonus, V5GeneId requiredGene, V5StructureId[] requiredAnyStructure, V5StructureId[] grantedStructures, V5AdaptationId[] requiredAnyAdaptation = null, V5AdaptationId[] requiredAllAdaptations = null)
        {
            this.id = id;
            this.displayName = displayName;
            this.category = category;
            this.plateText = plateText;
            this.tacticalRole = tacticalRole;
            this.targetPath = targetPath;
            this.lineageRole = lineageRole;
            this.defaultDirective = defaultDirective;
            this.cost = cost;
            this.auxiliary = auxiliary;
            this.stressCost = stressCost;
            this.hpMultiplier = hpMultiplier;
            this.speedMultiplier = speedMultiplier;
            this.synthesisMultiplier = synthesisMultiplier;
            this.physicalBonus = physicalBonus;
            this.chemicalBonus = chemicalBonus;
            this.sensorBonus = sensorBonus;
            this.colonizationBonus = colonizationBonus;
            this.requiredGene = requiredGene;
            this.requiredAnyStructure = requiredAnyStructure ?? new V5StructureId[0];
            this.requiredAnyAdaptation = requiredAnyAdaptation ?? new V5AdaptationId[0];
            this.requiredAllAdaptations = requiredAllAdaptations ?? new V5AdaptationId[0];
            this.grantedStructures = grantedStructures ?? new V5StructureId[0];
        }
    }

    public class V5GerminalProductionSystem : MonoBehaviour, IV5RunResettable
    {
        public bool ShowPanel;
        public V5GerminalCasteId SelectedCaste = V5GerminalCasteId.PlasticDaughter;
        public V5CellDeploymentMode SelectedDeploymentMode = V5CellDeploymentMode.Auto;
        public string LastMessage = "Camara germinal lista.";

        private readonly List<V5GerminalCasteDefinition> definitions = new List<V5GerminalCasteDefinition>(12);
        private Vector2 scroll;
        private GUIStyle panel;
        private GUIStyle title;
        private GUIStyle body;
        private GUIStyle small;
        private GUIStyle button;

        private void Awake()
        {
            EnsureDefinitions();
        }

        private void Start()
        {
            V5PanelRouter.Register("Germinal", () => ShowPanel, v => ShowPanel = v);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha8)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Germinal"); ShowPanel = !ShowPanel; }
        }

        public void ResetForNewRun()
        {
            SelectedCaste = V5GerminalCasteId.PlasticDaughter;
            SelectedDeploymentMode = V5CellDeploymentMode.Auto;
            LastMessage = "Camara germinal lista.";
        }

        public V5GerminalCasteDefinition Get(V5GerminalCasteId id)
        {
            EnsureDefinitions();
            for (int i = 0; i < definitions.Count; i++)
                if (definitions[i].id == id) return definitions[i];
            return definitions[0];
        }

        public bool TryProduce(V5GerminalCasteId id, bool toast)
        {
            return TryProduce(id, SelectedDeploymentMode, toast);
        }

        public bool TryProduce(V5GerminalCasteId id, V5CellDeploymentMode deploymentMode, bool toast)
        {
            V5GerminalCasteDefinition def = Get(id);
            string reason;
            if (!CanProduce(def, deploymentMode, out reason))
            {
                LastMessage = reason;
                if (toast) Toast(reason);
                return false;
            }

            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm.MotherCell;
            V5CellDeploymentMode resolvedDeployment = ResolveDeployment(def, mother, deploymentMode);
            mother.Resources.Pay(def.cost);
            V5EvolutionPath targetPath = ResolveTargetPath(def, mother);
            float recipeStress = Mathf.Max(0f, def.stressCost + V5PhenotypeRecipeLibrary.StressAdjustment(def, mother, targetPath));
            mother.Stats.stress = Mathf.Clamp(mother.Stats.stress + recipeStress, 0f, 100f);

            Vector2 spawn = (Vector2)mother.transform.position + Random.insideUnitCircle.normalized * (mother.Stats.radius + 1.0f);
            V5CellEntity child = gm.CellFactory.SpawnGerminalCell(spawn, mother, def, resolvedDeployment);
            bool attached = false;
            if (child != null && resolvedDeployment == V5CellDeploymentMode.AttachedBody && gm.Body != null)
                attached = gm.Body.TryAttach(child, V5PhenotypeRecipeLibrary.RecommendedBodyRole(def.id));
            if (child != null && gm.AffinityLog != null) gm.AffinityLog.RecordGerminal(ResolveTargetPath(def, mother), def.displayName);
            V5PhenotypeRecipeMaturity maturity = V5PhenotypeRecipeLibrary.EvaluateMaturity(def, mother, targetPath);
            string destination = attached ? "cuerpo" : "squad libre";
            LastMessage = child != null ? "Diferenciada: " + def.displayName + " -> " + destination + " (" + maturity.label + ")" : "No se pudo diferenciar.";
            if (toast) Toast(LastMessage);
            return child != null;
        }

        public bool CanProduce(V5GerminalCasteDefinition def, out string reason)
        {
            return CanProduce(def, V5CellDeploymentMode.Auto, out reason);
        }

        public bool CanProduce(V5GerminalCasteDefinition def, V5CellDeploymentMode deploymentMode, out string reason)
        {
            reason = "";
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null || gm.CellFactory == null)
            {
                reason = "No hay madre o fabrica celular.";
                return false;
            }

            V5CellEntity mother = gm.MotherCell;
            V5EvolutionPath targetPath = ResolveTargetPath(def, mother);
            if (!V5RosterBalance.IsPlayablePath(targetPath))
            {
                reason = "La ruta " + targetPath + " no es producible como casta.";
                return false;
            }
            if (gm.CoreMode)
            {
                if (!gm.CanAddPlayerCellFrom(mother))
                {
                    reason = gm.PlayerTotalCellCount() >= V5Balance.CoreTotalPlayerCellHardCap
                        ? "Cap total de celulas alcanzado."
                        : "Cap de celulas libres alcanzado.";
                    return false;
                }
            }
            else
            {
                if (gm.PlayerCellCount() >= V5Balance.HardControllableEntityCap)
                {
                    reason = "Cap de entidades alcanzado.";
                    return false;
                }
                float projected = V5RosterBalance.PopulationWeight(targetPath, V5CellRole.Daughter);
                if (gm.PlayerPopulationLoad() + projected > V5Balance.HardPopulationLoad + 0.001f)
                {
                    reason = "Carga biologica insuficiente.";
                    return false;
                }
                int routeCap = V5RosterBalance.RecommendedHardCap(targetPath);
                if (routeCap <= 0 || V5Balance.RouteCount(gm.PlayerCells, targetPath) >= routeCap)
                {
                    reason = "Cap de ruta alcanzado para " + targetPath + ".";
                    return false;
                }
            }
            if (def.auxiliary && !CanUseAuxiliary(def, mother, out reason)) return false;
            if (def.requiredGene != V5GeneId.None && (gm.Genes == null || !gm.Genes.HasGene(def.requiredGene)))
            {
                reason = "Requiere gen: " + def.requiredGene + ".";
                return false;
            }
            if (!HasAllRequiredAdaptations(def))
            {
                reason = "Requiere adaptaciones: " + V5BiologyCanon.AdaptationListText(def.requiredAllAdaptations) + ".";
                return false;
            }
            if (!HasAnyRequiredAdaptation(def))
            {
                reason = "Requiere alguna adaptacion: " + V5BiologyCanon.AdaptationListText(def.requiredAnyAdaptation) + ".";
                return false;
            }
            if (!HasAnyRequiredStructure(mother, def))
            {
                reason = "Requiere estructura compatible.";
                return false;
            }
            if (!EnvironmentAllows(def, mother, out reason)) return false;
            if (!mother.Resources.CanPay(def.cost))
            {
                reason = "Faltan recursos: " + CostText(def.cost);
                return false;
            }
            float recipeStress = Mathf.Max(0f, def.stressCost + V5PhenotypeRecipeLibrary.StressAdjustment(def, mother, targetPath));
            if (mother.Stats.stress + recipeStress > 96f)
            {
                reason = "Stress germinal demasiado alto.";
                return false;
            }
            if (deploymentMode == V5CellDeploymentMode.AttachedBody && !CanUseBodyDestination(def, mother, out reason)) return false;
            reason = "Disponible.";
            return true;
        }

        public V5EvolutionPath ResolveTargetPath(V5GerminalCasteDefinition def, V5CellEntity mother)
        {
            if (def == null) return V5EvolutionPath.Uncommitted;
            if (def.targetPath == V5EvolutionPath.Uncommitted)
                return mother != null ? mother.EvolutionPath : V5EvolutionPath.Uncommitted;
            return def.targetPath;
        }

        public V5CellDeploymentMode ResolveDeployment(V5GerminalCasteDefinition def, V5CellEntity mother, V5CellDeploymentMode requestedMode)
        {
            if (requestedMode == V5CellDeploymentMode.FreeSquad) return V5CellDeploymentMode.FreeSquad;
            if (requestedMode == V5CellDeploymentMode.AttachedBody) return V5CellDeploymentMode.AttachedBody;
            string reason;
            if (ShouldAutoAttach(def, mother) && CanUseBodyDestination(def, mother, out reason)) return V5CellDeploymentMode.AttachedBody;
            return V5CellDeploymentMode.FreeSquad;
        }

        private bool ShouldAutoAttach(V5GerminalCasteDefinition def, V5CellEntity mother)
        {
            if (def == null || mother == null) return false;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Body == null || gm.Body.OpenSlotCount() <= 0) return false;
            V5BodySlotRole role = V5PhenotypeRecipeLibrary.RecommendedBodyRole(def.id);
            if (role == V5BodySlotRole.Armor || role == V5BodySlotRole.Producer || role == V5BodySlotRole.Connector || role == V5BodySlotRole.Mouth) return true;
            return gm.Body.OccupiedSlots >= 2 && role == V5BodySlotRole.Motor;
        }

        private bool CanUseBodyDestination(V5GerminalCasteDefinition def, V5CellEntity mother, out string reason)
        {
            reason = "";
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Body == null)
            {
                reason = "No hay cuerpo multicelular instalado.";
                return false;
            }
            if (mother == null)
            {
                reason = "No hay madre para anclar el cuerpo.";
                return false;
            }
            if (!gm.Body.CanUseBodyAttachment(mother, out reason)) return false;
            V5BodySlotRole role = V5PhenotypeRecipeLibrary.RecommendedBodyRole(def != null ? def.id : V5GerminalCasteId.PlasticDaughter);
            if (!gm.Body.HasOpenSlot(role))
            {
                reason = "No hay slot corporal libre para " + BodyRoleLabel(role) + ".";
                return false;
            }
            reason = "Slot corporal disponible: " + BodyRoleLabel(role) + ".";
            return true;
        }

        private string DeploymentStatus(V5GerminalCasteDefinition def, V5CellEntity mother)
        {
            V5CellDeploymentMode resolved = ResolveDeployment(def, mother, SelectedDeploymentMode);
            string bodyReason;
            bool bodyOk = CanUseBodyDestination(def, mother, out bodyReason);
            if (SelectedDeploymentMode == V5CellDeploymentMode.AttachedBody && !bodyOk) return "Cuerpo bloqueado: " + bodyReason;
            if (SelectedDeploymentMode == V5CellDeploymentMode.Auto)
            {
                string suffix = bodyOk ? bodyReason : bodyReason;
                return "Auto -> " + DeploymentLabel(resolved) + ". " + suffix;
            }
            return DeploymentLabel(SelectedDeploymentMode) + ". " + (SelectedDeploymentMode == V5CellDeploymentMode.AttachedBody ? bodyReason : "Sale libre y controlable.");
        }

        private string DeploymentLabel(V5CellDeploymentMode mode)
        {
            switch (mode)
            {
                case V5CellDeploymentMode.AttachedBody: return "Cuerpo";
                case V5CellDeploymentMode.FreeSquad: return "Squad libre";
                default: return "Auto";
            }
        }

        private string BodyRoleLabel(V5BodySlotRole role)
        {
            switch (role)
            {
                case V5BodySlotRole.Armor: return "armadura";
                case V5BodySlotRole.Motor: return "motor";
                case V5BodySlotRole.Producer: return "productor";
                case V5BodySlotRole.Mouth: return "boca";
                case V5BodySlotRole.Connector: return "conector";
                case V5BodySlotRole.Sensor: return "sensor";
                case V5BodySlotRole.Reserve: return "reserva";
                default: return "general";
            }
        }

        public float CurrentAuxiliaryLoad()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return 0f;
            V5EvolutionPath primary = gm.MotherCell.EvolutionPath;
            float load = 0f;
            for (int i = 0; i < gm.PlayerCells.Count; i++)
            {
                V5CellEntity cell = gm.PlayerCells[i];
                if (cell == null || cell.Role == V5CellRole.Mother || cell.Role == V5CellRole.Apex) continue;
                if (cell.EvolutionPath != primary) load += V5RosterBalance.PopulationWeight(cell);
            }
            return load;
        }

        public float AuxiliaryLoadCap()
        {
            V5GameManager gm = V5GameManager.Instance;
            float ratio = V5Balance.MaxAuxiliaryPopulationLoadRatio;
            if (gm != null && gm.Genes != null && gm.Genes.HasGene(V5GeneId.Symbiosis)) ratio = V5Balance.SymbiosisAuxiliaryPopulationLoadRatio;
            if (gm != null && gm.Adaptations != null && (gm.Adaptations.Has(V5AdaptationId.CellDifferentiation) || gm.Adaptations.Has(V5AdaptationId.SignalingCommunication))) ratio = V5Balance.SymbiosisAuxiliaryPopulationLoadRatio;
            return V5Balance.HardPopulationLoad * ratio;
        }

        private bool CanUseAuxiliary(V5GerminalCasteDefinition def, V5CellEntity mother, out string reason)
        {
            reason = "";
            V5EvolutionPath primary = mother.EvolutionPath;
            V5EvolutionPath target = ResolveTargetPath(def, mother);
            if (primary == V5EvolutionPath.Uncommitted)
            {
                reason = "Consolida una identidad antes de castas auxiliares.";
                return false;
            }
            if (target == primary)
            {
                reason = "Casta natural de la ruta primaria.";
                return true;
            }

            V5GameManager gm = V5GameManager.Instance;
            float projected = V5RosterBalance.PopulationWeight(target, V5CellRole.Daughter);
            if (CurrentAuxiliaryLoad() + projected > AuxiliaryLoadCap() + 0.001f)
            {
                reason = "Cupo auxiliar completo (" + CurrentAuxiliaryLoad().ToString("0.0") + "/" + AuxiliaryLoadCap().ToString("0.0") + ").";
                return false;
            }

            bool symbiosis = gm != null && gm.Genes != null && gm.Genes.HasGene(V5GeneId.Symbiosis);
            symbiosis |= gm != null && gm.Adaptations != null && (gm.Adaptations.Has(V5AdaptationId.CellDifferentiation) || gm.Adaptations.Has(V5AdaptationId.SignalingCommunication));
            bool allowed = symbiosis;
            if (gm != null && gm.Adaptations != null)
            {
                allowed |= HasAuxiliaryAdaptationPath(target, gm.Adaptations);
                allowed |= V5BiologyCanon.RouteAdaptationScore01(target, gm.Adaptations) >= V5Balance.GerminalAuxiliaryAffinityThreshold;
            }
            if (target == V5EvolutionPath.Amoeba)
                allowed |= primary == V5EvolutionPath.Flagellate || primary == V5EvolutionPath.Ciliate || primary == V5EvolutionPath.Rotifer || mother.HasStructure(V5StructureId.Lysosome);
            if (target == V5EvolutionPath.Ciliate)
                allowed |= primary == V5EvolutionPath.Flagellate || primary == V5EvolutionPath.Amoeba || primary == V5EvolutionPath.Rotifer || mother.HasStructure(V5StructureId.Cilia);
            if (target == V5EvolutionPath.Bacteria)
                allowed |= mother.Domain == V5CellDomain.Prokaryote || primary == V5EvolutionPath.Cyanobacteria || mother.HasStructure(V5StructureId.Fimbriae);
            if (target == V5EvolutionPath.Microalga)
                allowed |= primary == V5EvolutionPath.Cyanobacteria || primary == V5EvolutionPath.SlimeMold || primary == V5EvolutionPath.Fungus || mother.HasStructure(V5StructureId.Thylakoid) || mother.HasStructure(V5StructureId.MicroalgalChloroplast);

            V5EvolutionAffinityResult affinity = V5EvolutionAffinitySystem.Evaluate(mother, target);
            allowed |= affinity.Score01 >= V5Balance.GerminalAuxiliaryAffinityThreshold;

            if (!allowed)
            {
                reason = "Afinidad insuficiente para casta " + target + " (" + affinity.PercentLabel + "): " + affinity.reasons;
                return false;
            }
            return true;
        }

        private bool HasAuxiliaryAdaptationPath(V5EvolutionPath target, V5AdaptationSystem adaptations)
        {
            if (adaptations == null) return false;
            switch (target)
            {
                case V5EvolutionPath.Amoeba:
                    return adaptations.Has(V5AdaptationId.Lysosome) && adaptations.Has(V5AdaptationId.Pseudopods);
                case V5EvolutionPath.Ciliate:
                    return adaptations.Has(V5AdaptationId.Cilia);
                case V5EvolutionPath.Bacteria:
                    return adaptations.Has(V5AdaptationId.BacterialWall) || adaptations.Has(V5AdaptationId.PiliFimbriae) || adaptations.Has(V5AdaptationId.BasicAdhesin);
                case V5EvolutionPath.Microalga:
                    return adaptations.Has(V5AdaptationId.ProkaryoticThylakoid) || adaptations.Has(V5AdaptationId.Chloroplast);
                default:
                    return false;
            }
        }

        private bool HasAnyRequiredStructure(V5CellEntity mother, V5GerminalCasteDefinition def)
        {
            if (def.requiredAnyStructure == null || def.requiredAnyStructure.Length == 0) return true;
            for (int i = 0; i < def.requiredAnyStructure.Length; i++)
                if (mother.HasStructure(def.requiredAnyStructure[i])) return true;
            return false;
        }

        private bool HasAnyRequiredAdaptation(V5GerminalCasteDefinition def)
        {
            if (def.requiredAnyAdaptation == null || def.requiredAnyAdaptation.Length == 0) return true;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Adaptations == null) return true;
            for (int i = 0; i < def.requiredAnyAdaptation.Length; i++)
                if (gm.Adaptations.Has(def.requiredAnyAdaptation[i])) return true;
            return false;
        }

        private bool HasAllRequiredAdaptations(V5GerminalCasteDefinition def)
        {
            if (def.requiredAllAdaptations == null || def.requiredAllAdaptations.Length == 0) return true;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Adaptations == null) return true;
            for (int i = 0; i < def.requiredAllAdaptations.Length; i++)
                if (!gm.Adaptations.Has(def.requiredAllAdaptations[i])) return false;
            return true;
        }

        private bool EnvironmentAllows(V5GerminalCasteDefinition def, V5CellEntity mother, out string reason)
        {
            reason = "";
            if (def.id != V5GerminalCasteId.MicroalgaSupport && def.id != V5GerminalCasteId.CiliateController) return true;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null) return true;
            int tx, ty;
            gm.Environment.WorldToTile(mother.transform.position, out tx, out ty);
            if (def.id == V5GerminalCasteId.MicroalgaSupport)
            {
                bool hasPlastidAdaptation = gm.Adaptations != null && (gm.Adaptations.Has(V5AdaptationId.ProkaryoticThylakoid) || gm.Adaptations.Has(V5AdaptationId.Chloroplast));
                bool lightOk = gm.Environment.lightLevel[tx, ty] > 0.38f || mother.HasStructure(V5StructureId.Thylakoid) || mother.HasStructure(V5StructureId.MicroalgalChloroplast) || hasPlastidAdaptation;
                if (!lightOk)
                {
                    reason = "Necesita luz o plastidos para soporte microalgal.";
                    return false;
                }
            }
            if (def.id == V5GerminalCasteId.CiliateController)
            {
                bool fluidOk = gm.Environment.nutrients[tx, ty] > 0.35f || gm.Environment.oxygen[tx, ty] > 0.22f;
                if (!fluidOk)
                {
                    reason = "Necesita zona fluida/nutritiva para control ciliado.";
                    return false;
                }
            }
            return true;
        }

        private void EnsureDefinitions()
        {
            if (definitions.Count > 0) return;
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.PlasticDaughter, "Hija plastica", "Nucleo del linaje", "Una celula hija simple, todavia flexible. En la placa se ve como una gota joven con nucleo brillante y membrana sin especializar.", "Refuerzo general y heredero viable.", V5EvolutionPath.Uncommitted, V5LineageRole.Generalist, V5Directive.FollowMother, V5ResourceWallet.Cost(30, 14, 4, 4, 2, 1), false, 4f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, V5GeneId.None, null, null));
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.LineageGatherer, "Recolectora de linaje", "Nucleo del linaje", "La misma identidad que la madre, afinada para absorber y transportar recursos. La placa muestra vacuolas grandes y membrana ligera.", "Economia movil.", V5EvolutionPath.Uncommitted, V5LineageRole.Farmer, V5Directive.Farm, V5ResourceWallet.Cost(34, 16, 5, 5, 2, 1), false, 5f, 0.92f, 1.06f, 1.28f, -0.35f, 0f, -0.5f, 0.10f, V5GeneId.None, null, new V5StructureId[] { V5StructureId.StorageVacuole }));
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.LineageScout, "Exploradora de linaje", "Nucleo del linaje", "Fenotipo delgado, con sensores extendidos y organelos de movimiento visibles en primer plano.", "Vision, exploracion y flanqueo.", V5EvolutionPath.Uncommitted, V5LineageRole.Scout, V5Directive.Explore, V5ResourceWallet.Cost(32, 13, 6, 4, 2, 1), false, 5f, 0.76f, 1.35f, 0.95f, -0.45f, 0f, 4f, 0f, V5GeneId.None, null, null));
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.LineageDefender, "Defensora de linaje", "Nucleo del linaje", "Membrana densa, matriz protectora y forma mas compacta. En la placa funciona como una muralla viva.", "Ancla defensiva de bajo riesgo.", V5EvolutionPath.Uncommitted, V5LineageRole.Defender, V5Directive.Defend, V5ResourceWallet.Cost(42, 20, 6, 8, 2, 2), false, 7f, 1.28f, 0.82f, 0.95f, 0.10f, 0f, 0f, 0.08f, V5GeneId.None, null, new V5StructureId[] { V5StructureId.Capsule }));
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.LineageRaider, "Raider de linaje", "Nucleo del linaje", "Fenotipo agresivo de la ruta dominante. La placa muestra la celula entrando en una corriente de ataque.", "Hostigar y perseguir objetivos vulnerables.", V5EvolutionPath.Uncommitted, V5LineageRole.Predator, V5Directive.Attack, V5ResourceWallet.Cost(44, 18, 8, 5, 3, 2), false, 9f, 0.92f, 1.20f, 1.0f, 1.15f, 0.15f, 1.2f, 0f, V5GeneId.None, null, null));

            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.AmoeboidGuard, "Ameboide defensiva", "Casta auxiliar", "Una masa lenta y flexible envuelve el borde de la placa; su rol no es correr, sino absorber presion.", "Tanque organico y fagocito local.", V5EvolutionPath.Amoeba, V5LineageRole.Defender, V5Directive.Defend, V5ResourceWallet.Cost(58, 30, 14, 8, 4, 2), true, 12f, 1.42f, 0.72f, 1.0f, 1.1f, 0f, 0f, 0.04f, V5GeneId.None, null, new V5StructureId[] { V5StructureId.Lysosome, V5StructureId.StorageVacuole }, null, new V5AdaptationId[] { V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods }));
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.CiliateController, "Ciliada de control", "Casta auxiliar", "En la placa se ven cilios como pestanas de corriente, arrastrando particulas hacia una boca celular.", "Control de zona, filtracion y anti-swarm.", V5EvolutionPath.Ciliate, V5LineageRole.Defender, V5Directive.Defend, V5ResourceWallet.Cost(52, 24, 12, 6, 4, 1), true, 10f, 1.02f, 1.03f, 1.08f, 0.35f, 0.35f, 2.2f, 0.10f, V5GeneId.None, null, new V5StructureId[] { V5StructureId.Cilia }, new V5AdaptationId[] { V5AdaptationId.Cilia }));
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.BacterialSymbiont, "Simbionte bacteriano", "Casta auxiliar", "La placa muestra una nube de microcuerpos alrededor de la celula madre: no son heroes, son infraestructura viva.", "Swarm barato, biofilm y colonizacion.", V5EvolutionPath.Bacteria, V5LineageRole.Colonizer, V5Directive.Colonize, V5ResourceWallet.Cost(28, 12, 4, 3, 2, 0), true, 6f, 0.74f, 1.18f, 1.05f, -0.15f, 0.25f, -0.2f, 0.42f, V5GeneId.None, null, new V5StructureId[] { V5StructureId.Fimbriae }, new V5AdaptationId[] { V5AdaptationId.BacterialWall, V5AdaptationId.PiliFimbriae, V5AdaptationId.BasicAdhesin }));
            definitions.Add(new V5GerminalCasteDefinition(V5GerminalCasteId.MicroalgaSupport, "Microalga soporte", "Casta auxiliar", "Una celula verde queda suspendida en una columna de luz; produce energia y oxigena el borde de la colonia.", "Soporte luminico y economia en zonas claras.", V5EvolutionPath.Microalga, V5LineageRole.Farmer, V5Directive.Farm, V5ResourceWallet.Cost(54, 24, 8, 8, 8, 3), true, 9f, 0.90f, 0.88f, 1.18f, -0.15f, 0f, 0.5f, 0.22f, V5GeneId.None, null, new V5StructureId[] { V5StructureId.MicroalgalChloroplast }, new V5AdaptationId[] { V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast }));
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            Rect r = new Rect(520f, 92f, 610f, 560f);
            GUI.Box(r, GUIContent.none, panel);
            GUILayout.BeginArea(new Rect(r.x + 14f, r.y + 12f, r.width - 28f, r.height - 24f));
            GUILayout.Label("CAMARA GERMINAL - PRODUCCION DE FENOTIPOS", title);
            if (mother == null)
            {
                GUILayout.Label("Sin madre viva.", body);
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label("Madre: " + mother.EvolutionPath + " | Aux " + CurrentAuxiliaryLoad().ToString("0.0") + "/" + AuxiliaryLoadCap().ToString("0.0") + " | " + LastMessage, body);
            GUILayout.BeginHorizontal();
            DrawCasteList(mother);
            DrawSelectedPlate(mother);
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }

        private void DrawCasteList(V5CellEntity mother)
        {
            GUILayout.BeginVertical(GUILayout.Width(250f));
            GUILayout.Label("Fenotipos", title);
            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Height(450f));
            string lastCategory = "";
            for (int i = 0; i < definitions.Count; i++)
            {
                V5GerminalCasteDefinition def = definitions[i];
                if (def.category != lastCategory)
                {
                    lastCategory = def.category;
                    GUILayout.Space(4f);
                    GUILayout.Label(lastCategory, small);
                }
                string reason;
                bool can = CanProduce(def, out reason);
                GUI.enabled = SelectedCaste != def.id;
                V5PhenotypeRecipeDefinition recipe = V5PhenotypeRecipeLibrary.Get(def.id);
                V5PhenotypeRecipeMaturity maturity = V5PhenotypeRecipeLibrary.EvaluateMaturity(def, mother, ResolveTargetPath(def, mother));
                string marker = maturity.required01 <= 0.001f ? " " : (maturity.mature ? "*" : "~");
                string natural = IsNaturalPhenotype(def.id, mother.EvolutionPath) ? " +" : "";
                V5FunctionalCasteDefinition functional = V5CasteLibrary.Get(V5CasteLibrary.FromGerminalCaste(def.id));
                if (GUILayout.Button((can ? "" : "[bloq] ") + marker + natural + " " + functional.shortName + " " + recipe.code + "  " + def.displayName, button, GUILayout.Height(38f))) SelectedCaste = def.id;
                GUI.enabled = true;
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawSelectedPlate(V5CellEntity mother)
        {
            V5GerminalCasteDefinition def = Get(SelectedCaste);
            string reason;
            bool can = CanProduce(def, SelectedDeploymentMode, out reason);
            V5EvolutionPath target = ResolveTargetPath(def, mother);
            V5PhenotypeRecipeDefinition recipe = V5PhenotypeRecipeLibrary.Get(def.id);
            V5PhenotypeRecipeMaturity maturity = V5PhenotypeRecipeLibrary.EvaluateMaturity(def, mother, target);
            V5FunctionalCasteDefinition functional = V5CasteLibrary.Get(V5CasteLibrary.FromGerminalCaste(def.id));

            GUILayout.BeginVertical(GUILayout.Width(325f));
            Rect plate = GUILayoutUtility.GetRect(320f, 160f);
            DrawPreviewPlate(plate, def, target);
            GUILayout.Label(def.displayName + " - " + target, title);
            GUILayout.Label("Casta funcional: " + functional.displayName + " | " + functional.effectSummary, body);
            GUILayout.Label("Receta " + recipe.code + ": " + recipe.bodyPlan, body);
            GUILayout.Label("Organelos: " + recipe.organellePlan, small);
            GUILayout.Label("Herencia: " + recipe.inheritancePlan, small);
            GUILayout.Label("Efecto: " + recipe.effectSummary, small);
            V5CellModeDefinition mode = V5CellModeLibrary.Get(recipe.defaultMode);
            GUILayout.Label("Modo default: " + mode.displayName + " (" + mode.effectSummary + ")", small);
            GUILayout.Label("Canon: " + PhenotypeCanonLabel(def.id, mother.EvolutionPath, target) + " | cuerpo " + BodyBiasText(target) + " | sesgo " + BodyRoleLabel(functional.preferredBodyRole), small);
            GUILayout.Label("Adaptaciones: " + AdaptationRequirementText(def), small);
            GUILayout.Label("Ruta: " + V5BiologyCanon.RouteDesignNote(target), small);
            GUILayout.Label("Madurez: " + maturity.label + " | " + maturity.reason, small);
            GUILayout.Label("Rol: " + def.tacticalRole, body);
            V5EvolutionAffinityResult affinity = V5EvolutionAffinitySystem.Evaluate(mother, target);
            GUILayout.Label("Afinidad " + target + ": " + affinity.PercentLabel + " | " + affinity.reasons, small);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.AffinityLog != null) GUILayout.Label("Historial: " + gm.AffinityLog.RouteSummary(target, 2), small);
            float recipeStress = Mathf.Max(0f, def.stressCost + V5PhenotypeRecipeLibrary.StressAdjustment(def, mother, target));
            GUILayout.Label("Costo: " + CostText(def.cost) + " | Stress +" + recipeStress.ToString("0"), small);
            DrawDeploymentSelector();
            GUILayout.Label("Destino: " + DeploymentStatus(def, mother), small);
            GUILayout.Label("Estado: " + reason, small);
            GUI.enabled = can;
            if (GUILayout.Button("Diferenciar -> " + DeploymentLabel(SelectedDeploymentMode), button, GUILayout.Height(38f))) TryProduce(def.id, true);
            GUI.enabled = true;
            GUILayout.Label("Regla: la madre define identidad; el entorno y las adaptaciones abren castas; la carga biologica limita la mezcla.", small);
            GUILayout.EndVertical();
        }

        private bool IsNaturalPhenotype(V5GerminalCasteId id, V5EvolutionPath path)
        {
            V5GerminalCasteId[] phenotypes = V5BiologyCanon.NaturalPhenotypesForRoute(path);
            for (int i = 0; i < phenotypes.Length; i++)
                if (phenotypes[i] == id) return true;
            return false;
        }

        private string PhenotypeCanonLabel(V5GerminalCasteId id, V5EvolutionPath primaryPath, V5EvolutionPath targetPath)
        {
            if (IsNaturalPhenotype(id, primaryPath)) return "natural del linaje";
            if (IsNaturalPhenotype(id, targetPath)) return "natural de " + targetPath;
            return "auxiliar/exploratorio";
        }

        private string AdaptationRequirementText(V5GerminalCasteDefinition def)
        {
            if (def == null) return "sin receta";
            string all = def.requiredAllAdaptations != null && def.requiredAllAdaptations.Length > 0
                ? "todas [" + V5BiologyCanon.AdaptationListText(def.requiredAllAdaptations) + "]"
                : "";
            string any = def.requiredAnyAdaptation != null && def.requiredAnyAdaptation.Length > 0
                ? "alguna [" + V5BiologyCanon.AdaptationListText(def.requiredAnyAdaptation) + "]"
                : "";
            if (string.IsNullOrEmpty(all) && string.IsNullOrEmpty(any)) return "sin candado duro; madura por afinidad/adaptaciones.";
            if (string.IsNullOrEmpty(all)) return any;
            if (string.IsNullOrEmpty(any)) return all;
            return all + " + " + any;
        }

        private string BodyBiasText(V5EvolutionPath path)
        {
            V5BodySlotRole[] roles = V5BiologyCanon.BodyBiasForRoute(path);
            if (roles == null || roles.Length == 0) return "sin sesgo";
            string s = "";
            for (int i = 0; i < roles.Length; i++)
            {
                s += BodyRoleLabel(roles[i]);
                if (i < roles.Length - 1) s += "/";
            }
            return s;
        }

        private void DrawDeploymentSelector()
        {
            GUILayout.BeginHorizontal();
            DrawDeploymentButton(V5CellDeploymentMode.Auto, "Auto");
            DrawDeploymentButton(V5CellDeploymentMode.AttachedBody, "Cuerpo");
            DrawDeploymentButton(V5CellDeploymentMode.FreeSquad, "Squad");
            GUILayout.EndHorizontal();
        }

        private void DrawDeploymentButton(V5CellDeploymentMode mode, string label)
        {
            bool old = GUI.enabled;
            bool selected = SelectedDeploymentMode == mode;
            GUI.enabled = old && !selected;
            if (GUILayout.Button((selected ? "[" + label + "]" : label), button, GUILayout.Height(24f))) SelectedDeploymentMode = mode;
            GUI.enabled = old;
        }

        private void DrawPreviewPlate(Rect rect, V5GerminalCasteDefinition def, V5EvolutionPath target)
        {
            Color old = GUI.color;
            V5PhenotypeRecipeDefinition recipe = V5PhenotypeRecipeLibrary.Get(def.id);
            V5FunctionalCasteDefinition functional = V5CasteLibrary.Get(V5CasteLibrary.FromGerminalCaste(def.id));
            GUI.color = new Color(0.03f, 0.08f, 0.10f, 0.96f);
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            Color path = Color.Lerp(Color.Lerp(V5EvolutionLibrary.ColorForPath(target), recipe.plateColor, 0.45f), functional.primaryColor, 0.35f);
            GUI.color = new Color(path.r, path.g, path.b, 0.30f);
            GUI.DrawTexture(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, rect.height - 16f), Texture2D.whiteTexture);
            GUI.color = path;
            float cx = rect.x + rect.width * 0.52f;
            float cy = rect.y + rect.height * 0.52f;
            GUI.DrawTexture(new Rect(cx - 34f, cy - 34f, 68f, 68f), Texture2D.whiteTexture);
            GUI.color = new Color(1f, 1f, 1f, 0.55f);
            GUI.DrawTexture(new Rect(cx - 11f, cy - 11f, 22f, 22f), Texture2D.whiteTexture);
            GUI.color = new Color(path.r, path.g, path.b, 0.80f);
            for (int i = 0; i < 5; i++)
            {
                float a = (Time.time * 0.35f + i * 1.25f);
                float rr = 54f + i * 4f;
                GUI.DrawTexture(new Rect(cx + Mathf.Cos(a) * rr - 5f, cy + Mathf.Sin(a * 0.8f) * 34f - 5f, 10f, 10f), Texture2D.whiteTexture);
            }
            GUI.color = Color.white;
            GUI.Label(new Rect(rect.x + 12f, rect.y + 10f, rect.width - 24f, 22f), def.category + " | " + functional.displayName + " | " + recipe.code);
            GUI.Label(new Rect(rect.x + 12f, rect.y + rect.height - 28f, rect.width - 24f, 22f), V5CellModeLibrary.Get(recipe.defaultMode).displayName + " - " + recipe.effectSummary);
            GUI.color = old;
        }

        private string CostText(V5ResourceWallet c)
        {
            return "ATP " + c.atp.ToString("0") + " Bio " + c.biomass.ToString("0");
        }

        private void EnsureStyles()
        {
            if (panel != null) return;
            panel = new GUIStyle(GUI.skin.box);
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.86f, 1f, 1f, 1f);
            body = new GUIStyle(GUI.skin.label); body.wordWrap = true; body.normal.textColor = Color.white;
            small = new GUIStyle(body); small.fontSize = 11;
            button = new GUIStyle(GUI.skin.button); button.wordWrap = true; button.fontSize = 12;
        }

        private void Toast(string message)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(message);
        }
    }
}
