using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5LineageUpgradeId
    {
        FarmerEnzymes,
        ScoutChemotaxis,
        DefenderMembrane,
        ColonizerMatrix,
        PredatorLysosomalBurst,
        RecyclerAutolysisLoop,
        GeneralistHomeostasis
    }

    public class V5LineageUpgradeSystem : MonoBehaviour, IV5RunResettable
    {
        public bool ShowPanel;
        public bool UseLegacyPanel;
        public string LastMessage = "V: mejoras de linaje. Mejoran roles sin crear nuevas especies.";
        private readonly HashSet<V5LineageUpgradeId> unlocked = new HashSet<V5LineageUpgradeId>();
        private readonly Dictionary<V5LineageUpgradeId, V5ResourceWallet> costs = new Dictionary<V5LineageUpgradeId, V5ResourceWallet>();
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;

        private void Awake()
        {
            costs[V5LineageUpgradeId.FarmerEnzymes] = V5ResourceWallet.Cost(38, 18, 16, 8, 4, 2);
            costs[V5LineageUpgradeId.ScoutChemotaxis] = V5ResourceWallet.Cost(32, 12, 10, 10, 5, 4);
            costs[V5LineageUpgradeId.DefenderMembrane] = V5ResourceWallet.Cost(42, 22, 12, 18, 5, 8);
            costs[V5LineageUpgradeId.ColonizerMatrix] = V5ResourceWallet.Cost(46, 20, 14, 18, 8, 8);
            costs[V5LineageUpgradeId.PredatorLysosomalBurst] = V5ResourceWallet.Cost(52, 24, 22, 10, 10, 4);
            costs[V5LineageUpgradeId.RecyclerAutolysisLoop] = V5ResourceWallet.Cost(36, 14, 12, 6, 4, 2);
            costs[V5LineageUpgradeId.GeneralistHomeostasis] = V5ResourceWallet.Cost(44, 18, 16, 14, 8, 6);
        }

        private void Update()
        {
            if (!Input.GetKeyDown(KeyCode.V)) return;
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Lineages != null)
            {
                gm.Lineages.TogglePanel(true);
                return;
            }
            if (UseLegacyPanel) ShowPanel = !ShowPanel;
        }

        public bool Has(V5LineageUpgradeId id)
        {
            return unlocked.Contains(id);
        }

        public List<V5LineageUpgradeId> GetUnlocked()
        {
            return new List<V5LineageUpgradeId>(unlocked);
        }

        public void RestoreUnlocked(List<V5LineageUpgradeId> ids)
        {
            unlocked.Clear();
            if (ids != null)
            {
                for (int i = 0; i < ids.Count; i++) unlocked.Add(ids[i]);
            }
            LastMessage = "Mejoras restauradas: " + unlocked.Count;
        }

        public void ClearRun()
        {
            unlocked.Clear();
            LastMessage = "Mejoras de linaje reiniciadas.";
        }

        void IV5RunResettable.ResetForNewRun() => ClearRun();

        public float MoveMultiplier(V5CellEntity cell)
        {
            if (cell == null) return 1f;
            float m = 1f;
            if (cell.LineageRole == V5LineageRole.Scout && Has(V5LineageUpgradeId.ScoutChemotaxis)) m *= 1.28f;
            if (cell.LineageRole == V5LineageRole.Predator && Has(V5LineageUpgradeId.PredatorLysosomalBurst)) m *= 1.10f;
            if (cell.LineageRole == V5LineageRole.Defender && Has(V5LineageUpgradeId.DefenderMembrane)) m *= 0.92f;
            if (cell.LineageRole == V5LineageRole.Generalist && Has(V5LineageUpgradeId.GeneralistHomeostasis)) m *= 1.08f;
            return m;
        }

        public float FarmMultiplier(V5CellEntity cell)
        {
            if (cell == null) return 1f;
            float m = 1f;
            if (cell.LineageRole == V5LineageRole.Farmer && Has(V5LineageUpgradeId.FarmerEnzymes)) m *= 1.45f;
            if (cell.LineageRole == V5LineageRole.Recycler && Has(V5LineageUpgradeId.RecyclerAutolysisLoop)) m *= 1.28f;
            if (cell.LineageRole == V5LineageRole.Generalist && Has(V5LineageUpgradeId.GeneralistHomeostasis)) m *= 1.10f;
            return m;
        }

        public float ColonizationMultiplier(V5CellEntity cell)
        {
            if (cell == null) return 1f;
            float m = 1f;
            if (cell.LineageRole == V5LineageRole.Colonizer && Has(V5LineageUpgradeId.ColonizerMatrix)) m *= 1.55f;
            if (cell.LineageRole == V5LineageRole.Generalist && Has(V5LineageUpgradeId.GeneralistHomeostasis)) m *= 1.08f;
            return m;
        }

        public float DamageMultiplier(V5CellEntity cell)
        {
            if (cell == null) return 1f;
            float m = 1f;
            if (cell.LineageRole == V5LineageRole.Predator && Has(V5LineageUpgradeId.PredatorLysosomalBurst)) m *= 1.30f;
            if (cell.LineageRole == V5LineageRole.Defender && Has(V5LineageUpgradeId.DefenderMembrane)) m *= 1.12f;
            return m;
        }

        public float DamageTakenMultiplier(V5CellEntity cell)
        {
            if (cell == null) return 1f;
            float m = 1f;
            if (cell.LineageRole == V5LineageRole.Defender && Has(V5LineageUpgradeId.DefenderMembrane)) m *= 0.75f;
            if (cell.LineageRole == V5LineageRole.Generalist && Has(V5LineageUpgradeId.GeneralistHomeostasis)) m *= 0.92f;
            return m;
        }

        public bool TryUnlock(V5LineageUpgradeId id)
        {
            if (unlocked.Contains(id)) return false;
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.MotherCell == null) return false;
            V5ResourceWallet cost = CostFor(id);
            if (!gm.MotherCell.Resources.CanPay(cost))
            {
                LastMessage = "Faltan recursos para " + id + ": " + CostText(cost);
                if (gm.Hud != null) gm.Hud.Toast(LastMessage);
                return false;
            }
            gm.MotherCell.Resources.Pay(cost);
            unlocked.Add(id);
            LastMessage = "Mejora desbloqueada: " + id;
            if (gm.Hud != null) gm.Hud.Toast(LastMessage);
            if (gm.Codex != null) gm.Codex.Unlock("Mejora de linaje - " + id, DescriptionFor(id));
            return true;
        }

        public V5ResourceWallet CostFor(V5LineageUpgradeId id)
        {
            V5ResourceWallet cost = costs.ContainsKey(id) ? costs[id] : V5ResourceWallet.Cost(40, 20, 10, 10, 5, 5);
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.BalanceProfile != null && gm.BalanceProfile.Profile != null)
            {
                float cm = Mathf.Clamp(gm.BalanceProfile.Profile.lineageUpgradeCostMultiplier, 0.35f, 2.5f);
                cost.atp *= cm; cost.biomass *= cm; cost.aminoAcids *= cm; cost.lipids *= cm; cost.nucleotides *= cm; cost.minerals *= cm;
            }
            return cost;
        }

        public string DescriptionFor(V5LineageUpgradeId id)
        {
            return Description(id);
        }

        public string CostLabel(V5ResourceWallet cost)
        {
            return CostText(cost);
        }

        public string Summary()
        {
            if (unlocked.Count == 0) return "sin mejoras";
            List<string> names = new List<string>();
            foreach (V5LineageUpgradeId id in unlocked) names.Add(id.ToString());
            return string.Join(", ", names.ToArray());
        }

        private void OnGUI()
        {
            if (!UseLegacyPanel || !ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            Rect r = new Rect(Screen.width - 500, 110, 490, 396);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "MEJORAS DE LINAJE", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 38), "Estas mejoras convierten roles RTS en ramas semipermanentes: farmers, scouts, defenders, colonizers, predators, recyclers y generalistas.");
            float y = r.y + 84;
            foreach (V5LineageUpgradeId id in System.Enum.GetValues(typeof(V5LineageUpgradeId)))
            {
                bool active = unlocked.Contains(id);
                V5ResourceWallet cost = CostFor(id);
                GUI.enabled = !active && gm.MotherCell != null && gm.MotherCell.Resources.CanPay(cost);
                string label = (active ? "[x] " : "") + id + "\n" + Description(id) + "\n" + CostText(cost);
                if (GUI.Button(new Rect(r.x + 12, y, r.width - 24, 58), label, button)) TryUnlock(id);
                GUI.enabled = true;
                y += 62f;
            }
            GUI.Label(new Rect(r.x + 12, r.y + r.height - 34, r.width - 24, 24), LastMessage);
        }

        private string Description(V5LineageUpgradeId id)
        {
            switch (id)
            {
                case V5LineageUpgradeId.FarmerEnzymes: return "Farmers recolectan +45%.";
                case V5LineageUpgradeId.ScoutChemotaxis: return "Scouts se mueven +28%.";
                case V5LineageUpgradeId.DefenderMembrane: return "Defenders reciben -25% dano.";
                case V5LineageUpgradeId.ColonizerMatrix: return "Colonizers colonizan +55%.";
                case V5LineageUpgradeId.PredatorLysosomalBurst: return "Predators hacen +30% dano.";
                case V5LineageUpgradeId.RecyclerAutolysisLoop: return "Recyclers recolectan cadaver/detritus mejor.";
                case V5LineageUpgradeId.GeneralistHomeostasis: return "Generalistas reciben pequenos bonos globales.";
                default: return "Mejora de linaje.";
            }
        }

        private string CostText(V5ResourceWallet c)
        {
            return string.Format("ATP {0:0} Bio {1:0}", c.atp, c.biomass);
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box);
            box.normal.background = Texture2D.whiteTexture;
            box.normal.textColor = Color.white;
            title = new GUIStyle(GUI.skin.label);
            title.fontStyle = FontStyle.Bold;
            title.normal.textColor = Color.white;
            button = new GUIStyle(GUI.skin.button);
            button.wordWrap = true;
            button.alignment = TextAnchor.MiddleLeft;
        }
    }
}
