using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5GeneDefinition
    {
        public V5GeneId id;
        public V5GeneRing ring;
        public string name;
        public string description;
        public V5ResourceWallet cost;
        public float minTime;
        public V5CellDomain requiredDomain;

        public V5GeneDefinition(V5GeneId id, V5GeneRing ring, string name, string description, V5ResourceWallet cost, float minTime, V5CellDomain requiredDomain)
        {
            this.id = id;
            this.ring = ring;
            this.name = name;
            this.description = description;
            this.cost = cost;
            this.minTime = minTime;
            this.requiredDomain = requiredDomain;
        }
    }

    public class V5GeneSystem : MonoBehaviour, IV5RunResettable
    {
        public V5GeneId Ring1 = V5GeneId.None;
        public V5GeneId Ring2 = V5GeneId.None;
        public V5GeneId Ring3 = V5GeneId.None;
        public V5GeneId Ring4 = V5GeneId.None;
        public string LastMessage = "";
        public int UnlockedCount { get { int c = 0; if (Ring1 != V5GeneId.None) c++; if (Ring2 != V5GeneId.None) c++; if (Ring3 != V5GeneId.None) c++; if (Ring4 != V5GeneId.None) c++; return c; } }

        private readonly Dictionary<V5GeneId, V5GeneDefinition> defs = new Dictionary<V5GeneId, V5GeneDefinition>();

        public float ColonizationMultiplier
        {
            get
            {
                float value = 1f;
                if (Ring2 == V5GeneId.Adhesion) value += 0.35f;
                if (Ring4 == V5GeneId.Symbiosis) value += 0.30f;
                return value;
            }
        }

        private void Awake()
        {
            BuildDefinitions();
        }

        public void ResetRun()
        {
            Ring1 = V5GeneId.None;
            Ring2 = V5GeneId.None;
            Ring3 = V5GeneId.None;
            Ring4 = V5GeneId.None;
            LastMessage = "Anillos genicos reiniciados.";
        }

        void IV5RunResettable.ResetForNewRun() => ResetRun();

        private void BuildDefinitions()
        {
            if (defs.Count > 0) return;
            Add(V5GeneId.Respiration, V5GeneRing.Ring1Metabolism, "Respiracion aerobica", "Eucariota. Alto ATP si hay oxigeno.", V5ResourceWallet.Cost(20, 8, 3, 2, 6, 0), 15f, V5CellDomain.LUCA);
            Add(V5GeneId.Photosynthesis, V5GeneRing.Ring1Metabolism, "Fotosintesis", "Eucariota fotosintetico. Produce ATP y oxigeno en luz.", V5ResourceWallet.Cost(24, 10, 4, 6, 4, 2), 15f, V5CellDomain.LUCA);
            Add(V5GeneId.Fermentation, V5GeneRing.Ring1Metabolism, "Fermentacion", "Procariota. ATP rapido, mas stress y toxinas locales.", V5ResourceWallet.Cost(16, 6, 2, 1, 4, 0), 15f, V5CellDomain.LUCA);
            Add(V5GeneId.Chemolithotrophy, V5GeneRing.Ring1Metabolism, "Quimiolitotrofia", "Procariota extremo. Genera ATP y acidifica la zona.", V5ResourceWallet.Cost(18, 7, 2, 1, 4, 5), 15f, V5CellDomain.LUCA);

            Add(V5GeneId.Motility, V5GeneRing.Ring2Function, "Motilidad activa", "Instala el flagelo del dominio y aumenta sensor.", V5ResourceWallet.Cost(28, 12, 10, 3, 0, 0), 120f, V5CellDomain.LUCA);
            Add(V5GeneId.Secretion, V5GeneRing.Ring2Function, "Secrecion pasiva", "Habilita toxinas, plasmidos o vesiculas secretoras.", V5ResourceWallet.Cost(25, 12, 9, 6, 3, 2), 120f, V5CellDomain.LUCA);
            Add(V5GeneId.Recognition, V5GeneRing.Ring2Function, "Reconocimiento", "Detecta firmas biologicas, presas y amenazas; habilita control selectivo.", V5ResourceWallet.Cost(24, 10, 8, 3, 6, 0), 120f, V5CellDomain.Eukaryote);
            Add(V5GeneId.Adhesion, V5GeneRing.Ring2Function, "Adesina / adhesion", "Primer puente corporal: permite pegar hijas al cuerpo y mejora colonizacion, biofilm, mucilago e hifas.", V5ResourceWallet.Cost(6, 3, 1, 1, 0, 0), 15f, V5CellDomain.LUCA);

            Add(V5GeneId.RapidDivision, V5GeneRing.Ring3Division, "Division rapida", "Costo de division -35%. Ideal para swarm.", V5ResourceWallet.Cost(40, 18, 12, 8, 8, 4), 420f, V5CellDomain.LUCA);
            Add(V5GeneId.StrongInheritance, V5GeneRing.Ring3Division, "Herencia fuerte", "Hijas heredan estructuras con mas probabilidad.", V5ResourceWallet.Cost(42, 18, 14, 8, 10, 3), 420f, V5CellDomain.LUCA);
            Add(V5GeneId.Autonomy, V5GeneRing.Ring3Division, "Autonomia celular", "Hijas exploran y atacan mejor. Mas rango sensorial.", V5ResourceWallet.Cost(38, 16, 10, 6, 8, 5), 420f, V5CellDomain.LUCA);
            Add(V5GeneId.TotalReabsorption, V5GeneRing.Ring3Division, "Reabsorcion total", "Colonias reciclan mas recursos al morir.", V5ResourceWallet.Cost(36, 15, 10, 8, 8, 4), 420f, V5CellDomain.LUCA);

            Add(V5GeneId.Symbiosis, V5GeneRing.Ring4Ecosystem, "Simbiosis colonial", "Mas colonizacion y estabilidad del ecosistema.", V5ResourceWallet.Cost(65, 35, 20, 16, 15, 8), 650f, V5CellDomain.LUCA);
            Add(V5GeneId.ApexMaturation, V5GeneRing.Ring4Ecosystem, "Maduracion apex", "Permite invocar una forma premium del linaje.", V5ResourceWallet.Cost(70, 40, 24, 16, 18, 10), 650f, V5CellDomain.LUCA);
        }

        private void Add(V5GeneId id, V5GeneRing ring, string name, string description, V5ResourceWallet cost, float minTime, V5CellDomain requiredDomain)
        {
            defs[id] = new V5GeneDefinition(id, ring, name, description, cost, minTime, requiredDomain);
        }

        public IEnumerable<V5GeneDefinition> AllDefinitions()
        {
            BuildDefinitions();
            return defs.Values;
        }

        public V5GeneDefinition Get(V5GeneId id)
        {
            BuildDefinitions();
            return defs[id];
        }

        public bool HasGene(V5GeneId id)
        {
            return Ring1 == id || Ring2 == id || Ring3 == id || Ring4 == id;
        }

        public bool CanUnlock(V5GeneId id, V5CellEntity cell)
        {
            if (id == V5GeneId.None || cell == null) return false;
            BuildDefinitions();
            if (!defs.ContainsKey(id)) return false;
            V5GeneDefinition d = defs[id];
            if (SlotTaken(d.ring)) return false;
            if (V5GameManager.Instance != null && V5GameManager.Instance.ElapsedSeconds < d.minTime) return false;
            if (d.ring != V5GeneRing.Ring1Metabolism && Ring1 == V5GeneId.None) return false;
            if (d.ring == V5GeneRing.Ring3Division && Ring2 == V5GeneId.None) return false;
            if (d.ring == V5GeneRing.Ring4Ecosystem && Ring3 == V5GeneId.None) return false;
            if (d.requiredDomain != V5CellDomain.LUCA && cell.Domain != d.requiredDomain && !(cell.Domain == V5CellDomain.Multicellular && d.requiredDomain == V5CellDomain.Eukaryote)) return false;
            return cell.Resources.CanPay(d.cost);
        }

        private bool SlotTaken(V5GeneRing ring)
        {
            if (ring == V5GeneRing.Ring1Metabolism) return Ring1 != V5GeneId.None;
            if (ring == V5GeneRing.Ring2Function) return Ring2 != V5GeneId.None;
            if (ring == V5GeneRing.Ring3Division) return Ring3 != V5GeneId.None;
            if (ring == V5GeneRing.Ring4Ecosystem) return Ring4 != V5GeneId.None;
            return true;
        }

        public bool Unlock(V5GeneId id, V5CellEntity cell)
        {
            if (!CanUnlock(id, cell))
            {
                LastMessage = "No se puede activar " + id + ". Revisa recursos, dominio o anillo previo.";
                return false;
            }

            V5GeneDefinition d = Get(id);
            cell.Resources.Pay(d.cost);

            if (d.ring == V5GeneRing.Ring1Metabolism) Ring1 = id;
            else if (d.ring == V5GeneRing.Ring2Function) Ring2 = id;
            else if (d.ring == V5GeneRing.Ring3Division) Ring3 = id;
            else if (d.ring == V5GeneRing.Ring4Ecosystem) Ring4 = id;

            ApplyImmediateEffect(id, cell);
            if (V5GameManager.Instance != null && V5GameManager.Instance.AffinityLog != null) V5GameManager.Instance.AffinityLog.RecordGene(id);
            LastMessage = "Gen activado: " + d.name;
            if (V5GameManager.Instance != null && V5GameManager.Instance.Codex != null) V5GameManager.Instance.Codex.Unlock("Gen: " + d.name, d.description);
            return true;
        }

        private void ApplyImmediateEffect(V5GeneId id, V5CellEntity cell)
        {
            if (id == V5GeneId.Respiration) cell.ApplyMetabolism(V5MetabolismType.Respiration);
            else if (id == V5GeneId.Photosynthesis) cell.ApplyMetabolism(V5MetabolismType.Photosynthesis);
            else if (id == V5GeneId.Fermentation) cell.ApplyMetabolism(V5MetabolismType.Fermentation);
            else if (id == V5GeneId.Chemolithotrophy) cell.ApplyMetabolism(V5MetabolismType.Chemolithotrophy);
            else if (id == V5GeneId.Motility)
            {
                if (cell.Domain == V5CellDomain.Prokaryote) cell.ForceInstallStructure(V5StructureId.BacterialFlagellum);
                else if (cell.Domain == V5CellDomain.Eukaryote || cell.Domain == V5CellDomain.Multicellular) cell.ForceInstallStructure(V5StructureId.EukaryoticFlagellum);
                cell.Stats.sensorRange += 2f;
            }
            else if (id == V5GeneId.Secretion)
            {
                if (cell.Domain == V5CellDomain.Prokaryote) cell.ForceInstallStructure(V5StructureId.Plasmid);
                else if (cell.Domain == V5CellDomain.Eukaryote || cell.Domain == V5CellDomain.Multicellular) cell.ForceInstallStructure(V5StructureId.SecretoryVesicle);
                cell.Stats.chemicalDamagePerSecond += 0.5f;
            }
            else if (id == V5GeneId.Recognition)
            {
                cell.ForceInstallStructure(V5StructureId.RecognitionReceptors);
                cell.Stats.sensorRange += 3f;
            }
            else if (id == V5GeneId.Adhesion)
            {
                if (cell.Domain == V5CellDomain.Prokaryote) cell.ForceInstallStructure(V5StructureId.Fimbriae);
                else if (cell.Domain == V5CellDomain.Eukaryote || cell.Domain == V5CellDomain.Multicellular) cell.ForceInstallStructure(V5StructureId.InvasiveHypha);
                cell.Stats.colonizationPower += 0.4f;
            }
            else if (id == V5GeneId.Autonomy) cell.Stats.sensorRange += 2f;
            else if (id == V5GeneId.Symbiosis)
            {
                cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 20f);
                cell.Stats.colonizationPower += 0.5f;
            }
        }

        public float DivisionCostMultiplier(V5CellEntity cell)
        {
            if (Ring3 == V5GeneId.RapidDivision) return 0.65f;
            return 1f;
        }

        public float InheritanceChance(V5CellEntity cell, float fallback)
        {
            if (Ring3 == V5GeneId.StrongInheritance) return Mathf.Max(fallback, V5Balance.StrongInheritanceChance);
            return fallback;
        }

        public float DeathRecycleMultiplier()
        {
            if (Ring3 == V5GeneId.TotalReabsorption) return 1.45f;
            return 1f;
        }

        public string Summary()
        {
            return "R1 " + Ring1 + " | R2 " + Ring2 + " | R3 " + Ring3 + " | R4 " + Ring4;
        }
    }
}
