using UnityEngine;

namespace Protogenesis.V5
{
    public class V5IdentityRecognizer : MonoBehaviour, IV5RunResettable
    {
        public V5IdentityId Identity = V5IdentityId.LUCA;
        public V5EvolutionPath EvolutionPath = V5EvolutionPath.Uncommitted;
        public string DisplayName = "LUCA plastica";
        public string Summary = "Sin identidad consolidada.";
        public string Reason = "Inicio de run.";
        public V5AdaptationId SuggestedNext = V5AdaptationId.BasicAdhesin;

        public void ResetForNewRun()
        {
            Identity = V5IdentityId.LUCA;
            EvolutionPath = V5EvolutionPath.Uncommitted;
            DisplayName = "LUCA plastica";
            Summary = "Sin identidad consolidada.";
            Reason = "Inicio de run.";
            SuggestedNext = V5AdaptationId.BasicAdhesin;
        }

        public void Recalculate(string source)
        {
            V5GameManager gm = V5GameManager.Instance;
            V5AdaptationSystem a = gm != null ? gm.Adaptations : null;
            if (a == null)
            {
                ResetForNewRun();
                return;
            }

            V5IdentityId next = V5IdentityId.LUCA;
            V5EvolutionPath path = V5EvolutionPath.Uncommitted;
            string name = "LUCA plastica";
            string reason = "Aun no hay patron dominante.";
            V5AdaptationId suggested = V5AdaptationId.BasicAdhesin;

            if (a.Has(V5AdaptationId.Chloroplast) && a.Has(V5AdaptationId.PersistentAdhesion) && a.Has(V5AdaptationId.CellDifferentiation))
            {
                next = V5IdentityId.VolvoxComplete;
                path = V5EvolutionPath.Microalga;
                name = "Volvox temprano";
                reason = "Fotosintesis + adhesion persistente + diferenciacion.";
                suggested = V5AdaptationId.SignalingCommunication;
            }
            else if (a.Has(V5AdaptationId.Chloroplast) && a.Has(V5AdaptationId.ColonialAdhesin))
            {
                next = V5IdentityId.VolvoxEarly;
                path = V5EvolutionPath.Microalga;
                name = "Colonia fotosintetica";
                reason = "Cloroplasto + adesina colonial.";
                suggested = V5AdaptationId.PersistentAdhesion;
            }
            else if (a.Has(V5AdaptationId.ChemicalMemory) && a.Has(V5AdaptationId.SlimePlasmodium))
            {
                next = V5IdentityId.SlimeMold;
                path = V5EvolutionPath.SlimeMold;
                name = "Moho mucilaginoso";
                reason = "Plasmodio movil + memoria quimica.";
                suggested = V5AdaptationId.SignalingCommunication;
            }
            else if (a.Has(V5AdaptationId.FungalHypha) && a.Has(V5AdaptationId.ExtracellularEnzymes))
            {
                next = V5IdentityId.Fungus;
                path = V5EvolutionPath.Fungus;
                name = "Hongo saprofito";
                reason = "Hifa fungica + enzimas extracelulares.";
                suggested = V5AdaptationId.SignalingCommunication;
            }
            else if (a.Has(V5AdaptationId.FungalHypha))
            {
                next = V5IdentityId.ProtistBase;
                path = V5EvolutionPath.Uncommitted;
                name = "Red hifal inmadura";
                reason = "Hifa sin digestion externa; falta Enzimas.";
                suggested = V5AdaptationId.ExtracellularEnzymes;
            }
            else if (a.Has(V5AdaptationId.SilicaFrustule))
            {
                next = V5IdentityId.Diatom;
                path = V5EvolutionPath.Microalga;
                name = "Diatomea";
                reason = "Cloroplasto + frustula de silice.";
                suggested = V5AdaptationId.ColonialAdhesin;
            }
            else if (a.Has(V5AdaptationId.CelluloseWall) && a.Has(V5AdaptationId.Chloroplast))
            {
                next = V5IdentityId.Microalga;
                path = V5EvolutionPath.Microalga;
                name = "Microalga";
                reason = "Cloroplasto + pared de celulosa.";
                suggested = V5AdaptationId.ColonialAdhesin;
            }
            else if (a.Has(V5AdaptationId.Cilia))
            {
                next = V5IdentityId.Ciliate;
                path = V5EvolutionPath.Ciliate;
                name = "Ciliado";
                reason = "Cilios para control de flujo.";
                suggested = V5AdaptationId.ContractileVacuole;
            }
            else if (a.Has(V5AdaptationId.EukaryoticFlagellum))
            {
                next = V5IdentityId.Flagellate;
                path = V5EvolutionPath.Flagellate;
                name = "Flagelado";
                reason = "Flagelo eucariota.";
                suggested = V5AdaptationId.Lysosome;
            }
            else if (a.Has(V5AdaptationId.Lysosome) && a.Has(V5AdaptationId.Pseudopods))
            {
                next = V5IdentityId.Amoeba;
                path = V5EvolutionPath.Amoeba;
                name = "Ameba";
                reason = "Lisosoma + seudopodos.";
                suggested = V5AdaptationId.ColonialAdhesin;
            }
            else if (a.Has(V5AdaptationId.Nucleus) && a.Has(V5AdaptationId.Mitochondria))
            {
                next = V5IdentityId.ProtistBase;
                path = V5EvolutionPath.Uncommitted;
                name = "Protista base";
                reason = "Nucleo + mitocondria.";
                suggested = V5AdaptationId.Lysosome;
            }
            else if (a.Has(V5AdaptationId.ProkaryoticThylakoid))
            {
                next = V5IdentityId.Cyanobacteria;
                path = V5EvolutionPath.Cyanobacteria;
                name = "Cianobacteria";
                reason = "Tilacoide procariota.";
                suggested = V5AdaptationId.CatalaseROS;
            }
            else if (a.Has(V5AdaptationId.ProtonPump) || a.Has(V5AdaptationId.ExtremophileMembrane))
            {
                next = V5IdentityId.Archaea;
                path = V5EvolutionPath.Archaea;
                name = "Arquea";
                reason = "Membrana extrema o bomba de protones.";
                suggested = V5AdaptationId.CatalaseROS;
            }
            else if (a.Has(V5AdaptationId.BacterialWall) && a.Has(V5AdaptationId.PolysaccharideCapsule) && a.Has(V5AdaptationId.BasicAdhesin))
            {
                next = V5IdentityId.Biofilm;
                path = V5EvolutionPath.Bacteria;
                name = "Biofilm bacteriano";
                reason = "Pared + capsula + adesina.";
                suggested = V5AdaptationId.ColonialAdhesin;
            }
            else if (a.Has(V5AdaptationId.BacterialWall) && (a.Has(V5AdaptationId.BacterialFlagellum) || a.Has(V5AdaptationId.RapidDivision)))
            {
                next = V5IdentityId.BacteriaSwarm;
                path = V5EvolutionPath.Bacteria;
                name = "Bacteria swarm";
                reason = "Pared bacteriana + movilidad o division.";
                suggested = V5AdaptationId.BasicAdhesin;
            }

            bool changed = next != Identity;
            Identity = next;
            EvolutionPath = path;
            DisplayName = name;
            Reason = reason + " (" + source + ")";
            Summary = DisplayName + " | " + Reason;
            SuggestedNext = suggested;

            V5CellEntity mother = gm != null ? gm.MotherCell : null;
            if (mother != null && path != V5EvolutionPath.Uncommitted && mother.EvolutionPath != path)
            {
                mother.ApplyPath(path, true);
            }
            if (changed && gm != null && gm.Codex != null && Identity != V5IdentityId.LUCA)
            {
                gm.Codex.ObserveIdentity(this);
            }
        }
    }
}
