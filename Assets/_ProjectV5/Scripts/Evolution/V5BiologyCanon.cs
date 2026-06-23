using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5StructureTag
    {
        Core,
        Metabolic,
        Locomotion,
        Defense,
        Predation,
        Network,
        Sensor,
        Phototrophy,
        Microfauna,
        LatentApex
    }

    public static class V5BiologyCanon
    {
        public static V5GeneId[] GenesForRoute(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Bacteria:
                    return Genes(V5GeneId.Fermentation, V5GeneId.RapidDivision, V5GeneId.Secretion, V5GeneId.Adhesion);
                case V5EvolutionPath.Archaea:
                    return Genes(V5GeneId.Chemolithotrophy, V5GeneId.TotalReabsorption, V5GeneId.StrongInheritance);
                case V5EvolutionPath.Cyanobacteria:
                    return Genes(V5GeneId.Photosynthesis, V5GeneId.Adhesion, V5GeneId.Symbiosis);
                case V5EvolutionPath.Amoeba:
                    return Genes(V5GeneId.Recognition, V5GeneId.StrongInheritance, V5GeneId.Secretion);
                case V5EvolutionPath.Flagellate:
                    return Genes(V5GeneId.Motility, V5GeneId.Autonomy, V5GeneId.Respiration);
                case V5EvolutionPath.Ciliate:
                    return Genes(V5GeneId.Motility, V5GeneId.Recognition, V5GeneId.Autonomy);
                case V5EvolutionPath.Microalga:
                    return Genes(V5GeneId.Photosynthesis, V5GeneId.Symbiosis, V5GeneId.StrongInheritance);
                case V5EvolutionPath.Fungus:
                    return Genes(V5GeneId.Adhesion, V5GeneId.Secretion, V5GeneId.StrongInheritance);
                case V5EvolutionPath.SlimeMold:
                    return Genes(V5GeneId.Adhesion, V5GeneId.TotalReabsorption, V5GeneId.Autonomy);
                case V5EvolutionPath.Rotifer:
                    return Genes(V5GeneId.Motility, V5GeneId.Recognition, V5GeneId.ApexMaturation);
                case V5EvolutionPath.Nematode:
                    return Genes(V5GeneId.Motility, V5GeneId.Recognition, V5GeneId.ApexMaturation);
                default:
                    return Genes();
            }
        }

        public static V5StructureId[] StructuresForRoute(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Bacteria:
                    return Structures(V5StructureId.Fimbriae, V5StructureId.Plasmid, V5StructureId.BacterialFlagellum, V5StructureId.PeptidoglycanWall);
                case V5EvolutionPath.Archaea:
                    return Structures(V5StructureId.Capsule, V5StructureId.Catalase, V5StructureId.CryptobiosisTun);
                case V5EvolutionPath.Cyanobacteria:
                    return Structures(V5StructureId.Thylakoid, V5StructureId.PeptidoglycanWall, V5StructureId.Catalase, V5StructureId.Fimbriae);
                case V5EvolutionPath.Amoeba:
                    return Structures(V5StructureId.Lysosome, V5StructureId.StorageVacuole, V5StructureId.Catalase);
                case V5EvolutionPath.Flagellate:
                    return Structures(V5StructureId.EukaryoticFlagellum, V5StructureId.MetabolicEngine, V5StructureId.Catalase);
                case V5EvolutionPath.Ciliate:
                    return Structures(V5StructureId.Cilia, V5StructureId.StorageVacuole, V5StructureId.Lysosome);
                case V5EvolutionPath.Microalga:
                    return Structures(V5StructureId.MicroalgalChloroplast, V5StructureId.CelluloseWall, V5StructureId.Thylakoid);
                case V5EvolutionPath.Fungus:
                    return Structures(V5StructureId.InvasiveHypha, V5StructureId.CelluloseWall, V5StructureId.SecretoryVesicle);
                case V5EvolutionPath.SlimeMold:
                    return Structures(V5StructureId.MucilageMatrix, V5StructureId.StorageVacuole, V5StructureId.SecretoryVesicle);
                case V5EvolutionPath.Rotifer:
                    return Structures(V5StructureId.CoronaCilia, V5StructureId.Cuticle, V5StructureId.Lysosome);
                case V5EvolutionPath.Nematode:
                    return Structures(V5StructureId.PiercingStylet, V5StructureId.Cuticle, V5StructureId.EukaryoticFlagellum);
                case V5EvolutionPath.Tardigrade:
                    return Structures(V5StructureId.CryptobiosisTun, V5StructureId.Cuticle, V5StructureId.Catalase);
                default:
                    return Structures(V5StructureId.GeneticCompartment, V5StructureId.MetabolicEngine, V5StructureId.SynthesisMachinery);
            }
        }

        public static V5AdaptationId[] AdaptationsForRoute(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Bacteria:
                    return Adaptations(V5AdaptationId.BacterialWall, V5AdaptationId.BacterialFlagellum, V5AdaptationId.RapidDivision, V5AdaptationId.BasicAdhesin);
                case V5EvolutionPath.Archaea:
                    return Adaptations(V5AdaptationId.ExtremophileMembrane, V5AdaptationId.ProtonPump, V5AdaptationId.CatalaseROS);
                case V5EvolutionPath.Cyanobacteria:
                    return Adaptations(V5AdaptationId.BacterialWall, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.CatalaseROS, V5AdaptationId.BasicAdhesin);
                case V5EvolutionPath.Amoeba:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods, V5AdaptationId.ContractileVacuole);
                case V5EvolutionPath.Flagellate:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.EukaryoticFlagellum);
                case V5EvolutionPath.Ciliate:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.Cilia, V5AdaptationId.ContractileVacuole);
                case V5EvolutionPath.Microalga:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Chloroplast, V5AdaptationId.CelluloseWall, V5AdaptationId.ColonialAdhesin);
                case V5EvolutionPath.Fungus:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.ColonialAdhesin, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes);
                case V5EvolutionPath.SlimeMold:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.ColonialAdhesin, V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory);
                case V5EvolutionPath.Rotifer:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.Cilia, V5AdaptationId.PersistentAdhesion, V5AdaptationId.CellDifferentiation, V5AdaptationId.SignalingCommunication);
                case V5EvolutionPath.Nematode:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria, V5AdaptationId.PersistentAdhesion, V5AdaptationId.CellDifferentiation, V5AdaptationId.SignalingCommunication);
                case V5EvolutionPath.Tardigrade:
                    return Adaptations(V5AdaptationId.ExtremophileMembrane, V5AdaptationId.CatalaseROS, V5AdaptationId.BiologicalChampion);
                default:
                    return Adaptations();
            }
        }

        public static V5AdaptationId[] AdaptationsForIdentity(V5IdentityId identity)
        {
            switch (identity)
            {
                case V5IdentityId.BacteriaSwarm:
                    return AdaptationsForRoute(V5EvolutionPath.Bacteria);
                case V5IdentityId.Biofilm:
                    return Adaptations(V5AdaptationId.BacterialWall, V5AdaptationId.BasicAdhesin, V5AdaptationId.PiliFimbriae, V5AdaptationId.PolysaccharideCapsule);
                case V5IdentityId.Archaea:
                    return AdaptationsForRoute(V5EvolutionPath.Archaea);
                case V5IdentityId.Cyanobacteria:
                    return AdaptationsForRoute(V5EvolutionPath.Cyanobacteria);
                case V5IdentityId.ProtistBase:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Mitochondria);
                case V5IdentityId.Amoeba:
                    return AdaptationsForRoute(V5EvolutionPath.Amoeba);
                case V5IdentityId.Flagellate:
                    return AdaptationsForRoute(V5EvolutionPath.Flagellate);
                case V5IdentityId.Ciliate:
                    return AdaptationsForRoute(V5EvolutionPath.Ciliate);
                case V5IdentityId.Microalga:
                    return AdaptationsForRoute(V5EvolutionPath.Microalga);
                case V5IdentityId.Diatom:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Chloroplast, V5AdaptationId.SilicaFrustule);
                case V5IdentityId.Fungus:
                    return AdaptationsForRoute(V5EvolutionPath.Fungus);
                case V5IdentityId.SlimeMold:
                    return AdaptationsForRoute(V5EvolutionPath.SlimeMold);
                case V5IdentityId.VolvoxEarly:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Chloroplast, V5AdaptationId.ColonialAdhesin, V5AdaptationId.PersistentAdhesion);
                case V5IdentityId.VolvoxComplete:
                    return Adaptations(V5AdaptationId.Nucleus, V5AdaptationId.Chloroplast, V5AdaptationId.ColonialAdhesin, V5AdaptationId.PersistentAdhesion, V5AdaptationId.CellDifferentiation, V5AdaptationId.SignalingCommunication);
                default:
                    return Adaptations();
            }
        }

        public static float RouteAdaptationScore01(V5EvolutionPath path, V5AdaptationSystem adaptations)
        {
            V5AdaptationId[] canon = AdaptationsForRoute(path);
            if (adaptations == null || canon.Length == 0) return 0f;

            int installed = 0;
            for (int i = 0; i < canon.Length; i++)
            {
                if (adaptations.Has(canon[i])) installed++;
            }

            return Mathf.Clamp01((float)installed / canon.Length);
        }

        public static string AdaptationListText(V5AdaptationId[] adaptations)
        {
            if (adaptations == null || adaptations.Length == 0) return "ninguna";

            string result = string.Empty;
            for (int i = 0; i < adaptations.Length; i++)
            {
                V5AdaptationDefinition def = V5AdaptationLibrary.Get(adaptations[i]);
                string name = def != null && !string.IsNullOrEmpty(def.shortName) ? def.shortName : adaptations[i].ToString();
                result += i == 0 ? name : ", " + name;
            }

            return result;
        }

        public static V5StructureId[] StructuresUnlockedByGene(V5GeneId gene, V5CellDomain domain, V5EvolutionPath currentPath)
        {
            switch (gene)
            {
                case V5GeneId.Respiration:
                    return Structures(V5StructureId.MetabolicEngine, V5StructureId.Catalase);
                case V5GeneId.Photosynthesis:
                    return domain == V5CellDomain.Eukaryote || currentPath == V5EvolutionPath.Microalga
                        ? Structures(V5StructureId.MicroalgalChloroplast, V5StructureId.Thylakoid)
                        : Structures(V5StructureId.Thylakoid);
                case V5GeneId.Fermentation:
                    return Structures(V5StructureId.Plasmid, V5StructureId.Fimbriae);
                case V5GeneId.Chemolithotrophy:
                    return Structures(V5StructureId.Catalase, V5StructureId.Capsule);
                case V5GeneId.Motility:
                    if (domain == V5CellDomain.Prokaryote) return Structures(V5StructureId.BacterialFlagellum);
                    if (currentPath == V5EvolutionPath.Ciliate) return Structures(V5StructureId.Cilia, V5StructureId.EukaryoticFlagellum);
                    if (currentPath == V5EvolutionPath.Rotifer) return Structures(V5StructureId.CoronaCilia, V5StructureId.Cuticle);
                    return Structures(V5StructureId.EukaryoticFlagellum, V5StructureId.Cilia);
                case V5GeneId.Secretion:
                    return domain == V5CellDomain.Prokaryote ? Structures(V5StructureId.Plasmid) : Structures(V5StructureId.SecretoryVesicle);
                case V5GeneId.Recognition:
                    return Structures(V5StructureId.RecognitionReceptors, V5StructureId.PiercingStylet, V5StructureId.CoronaCilia);
                case V5GeneId.Adhesion:
                    if (domain == V5CellDomain.Prokaryote) return Structures(V5StructureId.Fimbriae, V5StructureId.Capsule);
                    if (currentPath == V5EvolutionPath.SlimeMold) return Structures(V5StructureId.MucilageMatrix, V5StructureId.StorageVacuole);
                    return Structures(V5StructureId.InvasiveHypha, V5StructureId.MucilageMatrix);
                case V5GeneId.StrongInheritance:
                    return Structures(V5StructureId.GeneticCompartment, V5StructureId.StemPlasticity);
                case V5GeneId.TotalReabsorption:
                    return Structures(V5StructureId.StorageVacuole, V5StructureId.MucilageMatrix);
                case V5GeneId.Symbiosis:
                    return Structures(V5StructureId.Fimbriae, V5StructureId.MicroalgalChloroplast, V5StructureId.MucilageMatrix);
                case V5GeneId.ApexMaturation:
                    return Structures(V5StructureId.CryptobiosisTun, V5StructureId.Cuticle, V5StructureId.CoronaCilia);
                default:
                    return Structures();
            }
        }

        public static V5GerminalCasteId[] NaturalPhenotypesForRoute(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Bacteria:
                    return Phenotypes(V5GerminalCasteId.BacterialSymbiont, V5GerminalCasteId.LineageGatherer, V5GerminalCasteId.LineageScout);
                case V5EvolutionPath.Archaea:
                    return Phenotypes(V5GerminalCasteId.LineageDefender, V5GerminalCasteId.LineageGatherer, V5GerminalCasteId.PlasticDaughter);
                case V5EvolutionPath.Cyanobacteria:
                    return Phenotypes(V5GerminalCasteId.MicroalgaSupport, V5GerminalCasteId.LineageGatherer, V5GerminalCasteId.BacterialSymbiont);
                case V5EvolutionPath.Amoeba:
                    return Phenotypes(V5GerminalCasteId.AmoeboidGuard, V5GerminalCasteId.LineageRaider, V5GerminalCasteId.LineageDefender);
                case V5EvolutionPath.Flagellate:
                    return Phenotypes(V5GerminalCasteId.LineageScout, V5GerminalCasteId.LineageRaider, V5GerminalCasteId.PlasticDaughter);
                case V5EvolutionPath.Ciliate:
                    return Phenotypes(V5GerminalCasteId.CiliateController, V5GerminalCasteId.LineageDefender, V5GerminalCasteId.LineageScout);
                case V5EvolutionPath.Microalga:
                    return Phenotypes(V5GerminalCasteId.MicroalgaSupport, V5GerminalCasteId.LineageGatherer, V5GerminalCasteId.LineageDefender);
                case V5EvolutionPath.Fungus:
                    return Phenotypes(V5GerminalCasteId.LineageDefender, V5GerminalCasteId.LineageGatherer, V5GerminalCasteId.BacterialSymbiont);
                case V5EvolutionPath.SlimeMold:
                    return Phenotypes(V5GerminalCasteId.LineageGatherer, V5GerminalCasteId.LineageScout, V5GerminalCasteId.BacterialSymbiont);
                case V5EvolutionPath.Rotifer:
                    return Phenotypes(V5GerminalCasteId.CiliateController, V5GerminalCasteId.AmoeboidGuard, V5GerminalCasteId.LineageDefender);
                case V5EvolutionPath.Nematode:
                    return Phenotypes(V5GerminalCasteId.LineageRaider, V5GerminalCasteId.LineageScout, V5GerminalCasteId.AmoeboidGuard);
                default:
                    return Phenotypes(V5GerminalCasteId.PlasticDaughter, V5GerminalCasteId.LineageGatherer, V5GerminalCasteId.LineageDefender);
            }
        }

        public static V5GerminalCasteId[] NaturalPhenotypesForIdentity(V5IdentityId identity)
        {
            switch (identity)
            {
                case V5IdentityId.BacteriaSwarm:
                case V5IdentityId.Biofilm:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Bacteria);
                case V5IdentityId.Archaea:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Archaea);
                case V5IdentityId.Cyanobacteria:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Cyanobacteria);
                case V5IdentityId.Amoeba:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Amoeba);
                case V5IdentityId.Flagellate:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Flagellate);
                case V5IdentityId.Ciliate:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Ciliate);
                case V5IdentityId.Microalga:
                case V5IdentityId.Diatom:
                case V5IdentityId.VolvoxEarly:
                case V5IdentityId.VolvoxComplete:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Microalga);
                case V5IdentityId.Fungus:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Fungus);
                case V5IdentityId.SlimeMold:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.SlimeMold);
                default:
                    return NaturalPhenotypesForRoute(V5EvolutionPath.Uncommitted);
            }
        }

        public static V5BodySlotRole[] BodyBiasForRoute(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Bacteria:
                    return BodyRoles(V5BodySlotRole.Connector, V5BodySlotRole.Producer, V5BodySlotRole.Armor);
                case V5EvolutionPath.Archaea:
                    return BodyRoles(V5BodySlotRole.Armor, V5BodySlotRole.Reserve, V5BodySlotRole.Producer);
                case V5EvolutionPath.Cyanobacteria:
                    return BodyRoles(V5BodySlotRole.Producer, V5BodySlotRole.Connector, V5BodySlotRole.Armor);
                case V5EvolutionPath.Amoeba:
                    return BodyRoles(V5BodySlotRole.Mouth, V5BodySlotRole.Armor, V5BodySlotRole.Reserve);
                case V5EvolutionPath.Flagellate:
                    return BodyRoles(V5BodySlotRole.Motor, V5BodySlotRole.Sensor, V5BodySlotRole.Mouth);
                case V5EvolutionPath.Ciliate:
                    return BodyRoles(V5BodySlotRole.Mouth, V5BodySlotRole.Sensor, V5BodySlotRole.Motor);
                case V5EvolutionPath.Microalga:
                    return BodyRoles(V5BodySlotRole.Producer, V5BodySlotRole.Reserve, V5BodySlotRole.Connector);
                case V5EvolutionPath.Fungus:
                    return BodyRoles(V5BodySlotRole.Armor, V5BodySlotRole.Connector, V5BodySlotRole.Producer);
                case V5EvolutionPath.SlimeMold:
                    return BodyRoles(V5BodySlotRole.Connector, V5BodySlotRole.Motor, V5BodySlotRole.Producer);
                case V5EvolutionPath.Rotifer:
                    return BodyRoles(V5BodySlotRole.Mouth, V5BodySlotRole.Sensor, V5BodySlotRole.Armor);
                case V5EvolutionPath.Nematode:
                    return BodyRoles(V5BodySlotRole.Mouth, V5BodySlotRole.Motor, V5BodySlotRole.Sensor);
                default:
                    return BodyRoles(V5BodySlotRole.Connector, V5BodySlotRole.Reserve);
            }
        }

        public static bool IsStructureRecommendedForRoute(V5StructureId structure, V5EvolutionPath path)
        {
            V5StructureId[] structures = StructuresForRoute(path);
            for (int i = 0; i < structures.Length; i++)
                if (structures[i] == structure) return true;
            return false;
        }

        public static bool IsStructureRecommendedForCell(V5StructureId structure, V5CellEntity cell)
        {
            if (cell == null) return false;
            V5EvolutionPath path = cell.EvolutionPath;
            if (path == V5EvolutionPath.Uncommitted)
            {
                V5GameManager gm = V5GameManager.Instance;
                if (gm != null && gm.RouteLifecycle != null && gm.RouteLifecycle.BestScore01 >= V5Balance.RouteEmergenceAffinityThreshold)
                    path = gm.RouteLifecycle.BestRoute;
            }
            return IsStructureRecommendedForRoute(structure, path);
        }

        public static float StructureInstallCostMultiplier(V5StructureId structure, V5CellEntity cell, V5GeneSystem genes)
        {
            bool recommended = IsStructureRecommendedForCell(structure, cell);
            V5AdaptationSystem adaptations = null;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null) adaptations = gm.Adaptations;
            if (adaptations != null)
            {
                bool adaptationLicensed = IsStructureLicensedByAdaptations(structure, adaptations);
                if (recommended && adaptationLicensed) return 0.70f;
                if (adaptationLicensed) return 0.85f;
                if (recommended) return 0.95f;
                return 1.28f;
            }

            bool licensed = IsStructureLicensedByGenes(structure, genes, cell);
            if (recommended && licensed) return 0.75f;
            if (recommended) return 0.88f;
            if (licensed) return 0.90f;
            return 1.20f;
        }

        public static V5ResourceWallet EffectiveStructureCost(V5StructureId structure, V5ResourceWallet baseCost, V5CellEntity cell, V5GeneSystem genes)
        {
            return ScaleCost(baseCost, StructureInstallCostMultiplier(structure, cell, genes));
        }

        public static string StructureInstallCostNote(V5StructureId structure, V5CellEntity cell, V5GeneSystem genes)
        {
            float mult = StructureInstallCostMultiplier(structure, cell, genes);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Adaptations != null)
            {
                if (mult < 0.80f) return "canon adaptativo x" + mult.ToString("0.00");
                if (mult < 0.95f) return "adaptacion afin x" + mult.ToString("0.00");
                if (mult > 1.05f) return "estructura exploratoria x" + mult.ToString("0.00");
                return "costo base";
            }

            if (mult < 0.80f) return "costo canonico x" + mult.ToString("0.00");
            if (mult < 0.95f) return "costo afin x" + mult.ToString("0.00");
            if (mult > 1.05f) return "exploratoria x" + mult.ToString("0.00");
            return "costo base";
        }

        public static bool IsStructureLicensedByAdaptations(V5StructureId structure, V5AdaptationSystem adaptations)
        {
            if (structure == V5StructureId.GeneticCompartment ||
                structure == V5StructureId.MetabolicEngine ||
                structure == V5StructureId.SynthesisMachinery ||
                structure == V5StructureId.StorageVacuole ||
                structure == V5StructureId.Catalase)
                return true;
            if (adaptations == null) return false;

            switch (structure)
            {
                case V5StructureId.BacterialFlagellum:
                    return adaptations.Has(V5AdaptationId.BacterialFlagellum);
                case V5StructureId.EukaryoticFlagellum:
                    return adaptations.Has(V5AdaptationId.EukaryoticFlagellum);
                case V5StructureId.Cilia:
                case V5StructureId.CoronaCilia:
                    return adaptations.Has(V5AdaptationId.Cilia);
                case V5StructureId.PeptidoglycanWall:
                    return adaptations.Has(V5AdaptationId.BacterialWall);
                case V5StructureId.CelluloseWall:
                    return adaptations.Has(V5AdaptationId.CelluloseWall);
                case V5StructureId.Capsule:
                    return adaptations.Has(V5AdaptationId.PolysaccharideCapsule) || adaptations.Has(V5AdaptationId.ExtremophileMembrane);
                case V5StructureId.Fimbriae:
                    return adaptations.Has(V5AdaptationId.PiliFimbriae) || adaptations.Has(V5AdaptationId.BasicAdhesin);
                case V5StructureId.Thylakoid:
                    return adaptations.Has(V5AdaptationId.ProkaryoticThylakoid);
                case V5StructureId.MicroalgalChloroplast:
                    return adaptations.Has(V5AdaptationId.Chloroplast);
                case V5StructureId.Lysosome:
                    return adaptations.Has(V5AdaptationId.Lysosome);
                case V5StructureId.SecretoryVesicle:
                    return adaptations.Has(V5AdaptationId.ExtracellularEnzymes);
                case V5StructureId.InvasiveHypha:
                    return adaptations.Has(V5AdaptationId.FungalHypha);
                case V5StructureId.MucilageMatrix:
                    return adaptations.Has(V5AdaptationId.SlimePlasmodium) || adaptations.Has(V5AdaptationId.ColonialAdhesin);
                case V5StructureId.RecognitionReceptors:
                    return adaptations.Has(V5AdaptationId.SignalingCommunication);
                case V5StructureId.StemPlasticity:
                    return adaptations.Has(V5AdaptationId.CellDifferentiation);
                case V5StructureId.CryptobiosisTun:
                case V5StructureId.Cuticle:
                    return adaptations.Has(V5AdaptationId.ExtremophileMembrane) || adaptations.Has(V5AdaptationId.BiologicalChampion);
                case V5StructureId.PiercingStylet:
                    return adaptations.Has(V5AdaptationId.Lysosome) && adaptations.Has(V5AdaptationId.BiologicalChampion);
                default:
                    return false;
            }
        }

        public static bool IsStructureLicensedByGenes(V5StructureId structure, V5GeneSystem genes, V5CellEntity cell)
        {
            if (structure == V5StructureId.GeneticCompartment ||
                structure == V5StructureId.MetabolicEngine ||
                structure == V5StructureId.SynthesisMachinery ||
                structure == V5StructureId.StorageVacuole ||
                structure == V5StructureId.Catalase)
                return true;
            if (genes == null || cell == null) return false;

            V5GeneId[] routeGenes = GenesForRoute(cell.EvolutionPath);
            for (int i = 0; i < routeGenes.Length; i++)
            {
                if (!genes.HasGene(routeGenes[i])) continue;
                V5StructureId[] unlocked = StructuresUnlockedByGene(routeGenes[i], cell.Domain, cell.EvolutionPath);
                for (int j = 0; j < unlocked.Length; j++)
                    if (unlocked[j] == structure) return true;
            }

            return false;
        }

        public static V5StructureTag[] TagsForStructure(V5StructureId structure)
        {
            switch (structure)
            {
                case V5StructureId.GeneticCompartment:
                case V5StructureId.SynthesisMachinery:
                case V5StructureId.StorageVacuole:
                case V5StructureId.StemPlasticity:
                    return Tags(V5StructureTag.Core);
                case V5StructureId.MetabolicEngine:
                case V5StructureId.Catalase:
                case V5StructureId.Plasmid:
                    return Tags(V5StructureTag.Core, V5StructureTag.Metabolic);
                case V5StructureId.Thylakoid:
                case V5StructureId.MicroalgalChloroplast:
                    return Tags(V5StructureTag.Phototrophy, V5StructureTag.Metabolic);
                case V5StructureId.BacterialFlagellum:
                case V5StructureId.EukaryoticFlagellum:
                case V5StructureId.Cilia:
                    return Tags(V5StructureTag.Locomotion);
                case V5StructureId.PeptidoglycanWall:
                case V5StructureId.CelluloseWall:
                case V5StructureId.Capsule:
                case V5StructureId.Cuticle:
                    return Tags(V5StructureTag.Defense);
                case V5StructureId.Fimbriae:
                case V5StructureId.InvasiveHypha:
                case V5StructureId.MucilageMatrix:
                    return Tags(V5StructureTag.Network);
                case V5StructureId.Lysosome:
                case V5StructureId.SecretoryVesicle:
                case V5StructureId.AzurophilicGranule:
                case V5StructureId.PiercingStylet:
                    return Tags(V5StructureTag.Predation);
                case V5StructureId.RecognitionReceptors:
                    return Tags(V5StructureTag.Sensor);
                case V5StructureId.CoronaCilia:
                    return Tags(V5StructureTag.Microfauna, V5StructureTag.Predation);
                case V5StructureId.CryptobiosisTun:
                    return Tags(V5StructureTag.LatentApex, V5StructureTag.Defense);
                default:
                    return Tags(V5StructureTag.Core);
            }
        }

        public static string RouteDesignNote(V5EvolutionPath path)
        {
            switch (path)
            {
                case V5EvolutionPath.Fungus: return "Red fija: defensa, hifas y digestion territorial.";
                case V5EvolutionPath.SlimeMold: return "Red movil: mucilago, detritus y memoria quimica.";
                case V5EvolutionPath.Ciliate: return "Unicelular de corrientes: control de flujo local.";
                case V5EvolutionPath.Rotifer: return "Microfauna filtradora: anti-swarm experimental.";
                case V5EvolutionPath.Tardigrade: return "Neutral/apex defensivo; no ruta primaria.";
                case V5EvolutionPath.Archaea: return "Requiere futura S-layer; evitar peptidoglicano como canon.";
                default: return V5EvolutionLibrary.GetPath(path).fantasy;
            }
        }

        private static V5GeneId[] Genes(params V5GeneId[] genes) { return genes; }
        private static V5StructureId[] Structures(params V5StructureId[] structures) { return structures; }
        private static V5AdaptationId[] Adaptations(params V5AdaptationId[] adaptations) { return adaptations; }
        private static V5GerminalCasteId[] Phenotypes(params V5GerminalCasteId[] phenotypes) { return phenotypes; }
        private static V5BodySlotRole[] BodyRoles(params V5BodySlotRole[] roles) { return roles; }
        private static V5StructureTag[] Tags(params V5StructureTag[] tags) { return tags; }

        private static V5ResourceWallet ScaleCost(V5ResourceWallet cost, float multiplier)
        {
            cost.atp *= multiplier;
            cost.biomass *= multiplier;
            cost.aminoAcids *= multiplier;
            cost.lipids *= multiplier;
            cost.nucleotides *= multiplier;
            cost.minerals *= multiplier;
            return cost;
        }
    }
}
