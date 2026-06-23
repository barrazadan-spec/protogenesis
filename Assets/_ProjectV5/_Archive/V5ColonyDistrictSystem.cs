using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5DistrictType
    {
        Nursery,
        MetabolicReactor,
        DetoxMatrix,
        DefensiveBastion,
        PhoticShelf,
        HuntingGround
    }

    [Serializable]
    public class V5DistrictSaveEntry
    {
        public int index;
        public bool claimed;
        public V5DistrictType type;
    }

    [Serializable]
    public class V5DistrictSaveFile
    {
        public string version = "2.0";
        public List<V5DistrictSaveEntry> districts = new List<V5DistrictSaveEntry>();
    }

    /// <summary>
    /// World-builder layer for V5 2.0. It slices the droplet into macro districts
    /// that can be claimed by colonization and assigned a colony function.
    /// </summary>
    public class V5ColonyDistrictSystem : MonoBehaviour
    {
        public int Columns = 6;
        public int Rows = 6;
        public float ClaimThreshold = 0.18f;
        public float StrongClaimThreshold = 0.34f;
        public float DistrictTickSeconds = 1.25f;
        public string LastMessage = "District system waiting for environment.";
        public int ClaimedCount { get; private set; }
        public float NetworkEfficiency { get; private set; }
        public float SupplyCapacity { get; private set; }
        public float SupplyUsage { get; private set; }
        public float SupplyPressure { get; private set; }

        private readonly List<District> districts = new List<District>(64);
        private float tickTimer;
        private bool showPanel;
        private int selectedIndex;
        private Vector2 scroll;
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;
        private GUIStyle small;

        private string SavePath
        {
            get { return Path.Combine(Application.persistentDataPath, "protogenesis_v5_districts_2_0.json"); }
        }

        private void Start() => V5PanelRouter.Register("Distritos", () => showPanel, v => showPanel = v);

        private void Update()
        {
            // Equals removed because RuntimeSettings uses = for speed; open via Paneles.
            if (Input.GetKeyDown(KeyCode.KeypadPlus)) AutoDesignDistricts();
            if (Input.GetKeyDown(KeyCode.KeypadMinus)) ExportDistricts();
            if (Input.GetKeyDown(KeyCode.KeypadMultiply)) ImportDistricts();

            tickTimer += Time.deltaTime;
            if (tickTimer >= DistrictTickSeconds)
            {
                tickTimer = 0f;
                EnsureDistricts();
                ScanAndClaimDistricts();
                ApplyDistrictEffects();
                UpdateSupply();
            }
        }

        private void OnGUI()
        {
            if (!showPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;

            Rect r = new Rect(16, 190, 520, Screen.height - 245);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "DISTRITOS COLONIALES - RED ADAPTATIVA", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 20), string.Format("Claimed {0}/{1} | Network {2:0}% | Supply {3:0}/{4:0} | Pressure {5:0}%", ClaimedCount, districts.Count, NetworkEfficiency * 100f, SupplyUsage, SupplyCapacity, SupplyPressure * 100f));
            GUI.Label(new Rect(r.x + 12, r.y + 60, r.width - 24, 20), LastMessage);

            if (GUI.Button(new Rect(r.x + 12, r.y + 86, 118, 28), "Auto-design", button)) AutoDesignDistricts();
            if (GUI.Button(new Rect(r.x + 136, r.y + 86, 118, 28), "Export JSON", button)) ExportDistricts();
            if (GUI.Button(new Rect(r.x + 260, r.y + 86, 118, 28), "Import JSON", button)) ImportDistricts();
            if (GUI.Button(new Rect(r.x + 384, r.y + 86, 118, 28), "Select mother", button)) SelectNearestToMother();

            DrawDistrictGrid(new Rect(r.x + 12, r.y + 124, 240, 240));
            DrawSelectedDistrict(new Rect(r.x + 264, r.y + 124, r.width - 276, 240));
            DrawDistrictList(new Rect(r.x + 12, r.y + 374, r.width - 24, r.height - 386));
        }

        private void EnsureStyles()
        {
            if (box != null) return;
            box = new GUIStyle(GUI.skin.box); box.alignment = TextAnchor.UpperLeft; box.normal.textColor = Color.white; box.fontSize = 12;
            title = new GUIStyle(GUI.skin.label); title.fontStyle = FontStyle.Bold; title.fontSize = 15; title.normal.textColor = new Color(0.82f, 1f, 0.92f, 1f);
            button = new GUIStyle(GUI.skin.button); button.fontSize = 11; button.wordWrap = true;
            small = new GUIStyle(GUI.skin.label); small.fontSize = 11; small.normal.textColor = Color.white;
        }

        private void EnsureDistricts()
        {
            V5EnvironmentGrid env = V5GameManager.Instance != null ? V5GameManager.Instance.Environment : null;
            if (env == null || env.nutrients == null) return;
            int target = Mathf.Max(1, Columns) * Mathf.Max(1, Rows);
            if (districts.Count == target) return;
            districts.Clear();
            selectedIndex = 0;
            float diameter = env.MapRadius * 2f;
            float w = diameter / Columns;
            float h = diameter / Rows;
            int index = 0;
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    Vector2 center = new Vector2(-env.MapRadius + w * (x + 0.5f), -env.MapRadius + h * (y + 0.5f));
                    District d = new District();
                    d.index = index++;
                    d.gridX = x;
                    d.gridY = y;
                    d.center = center;
                    d.size = new Vector2(w, h);
                    d.type = V5DistrictType.Nursery;
                    d.existsInsideMap = center.magnitude <= env.MapRadius * 1.05f;
                    districts.Add(d);
                }
            }
            LastMessage = "Districts initialized. Colonize sectors to claim them.";
        }

        private void ScanAndClaimDistricts()
        {
            V5EnvironmentGrid env = V5GameManager.Instance != null ? V5GameManager.Instance.Environment : null;
            if (env == null || districts.Count == 0) return;
            ClaimedCount = 0;
            for (int i = 0; i < districts.Count; i++)
            {
                District d = districts[i];
                if (!d.existsInsideMap) continue;
                SampleDistrict(env, d);
                if (!d.claimed && d.avgColonization >= ClaimThreshold)
                {
                    d.claimed = true;
                    d.type = RecommendType(d);
                    LastMessage = "Claimed district " + d.Label + " as " + d.type + ".";
                    Toast(LastMessage);
                }
                if (d.claimed) ClaimedCount++;
            }
            NetworkEfficiency = districts.Count > 0 ? Mathf.Clamp01((ClaimedCount / Mathf.Max(1f, districts.Count * 0.55f)) * (1f - SupplyPressure * 0.35f)) : 0f;
        }

        private void SampleDistrict(V5EnvironmentGrid env, District d)
        {
            float colonization = 0f;
            float nutrients = 0f;
            float light = 0f;
            float oxygen = 0f;
            float toxins = 0f;
            float acidity = 0f;
            float detritus = 0f;
            int count = 0;
            int samples = 4;
            for (int sx = 0; sx < samples; sx++)
            {
                for (int sy = 0; sy < samples; sy++)
                {
                    Vector2 p = new Vector2(
                        d.center.x - d.size.x * 0.42f + d.size.x * (sx / (float)(samples - 1)) * 0.84f,
                        d.center.y - d.size.y * 0.42f + d.size.y * (sy / (float)(samples - 1)) * 0.84f);
                    if (p.magnitude > env.MapRadius) continue;
                    int tx, ty;
                    env.WorldToTile(p, out tx, out ty);
                    colonization += env.colonization[tx, ty];
                    nutrients += env.nutrients[tx, ty];
                    light += env.lightLevel[tx, ty];
                    oxygen += env.oxygen[tx, ty];
                    toxins += env.toxins[tx, ty];
                    acidity += env.acidity[tx, ty];
                    detritus += env.detritus[tx, ty];
                    count++;
                }
            }
            count = Mathf.Max(1, count);
            d.avgColonization = colonization / count;
            d.avgNutrients = nutrients / count;
            d.avgLight = light / count;
            d.avgOxygen = oxygen / count;
            d.avgToxins = toxins / count;
            d.avgAcidity = acidity / count;
            d.avgDetritus = detritus / count;
            d.quality = Mathf.Clamp01(d.avgColonization * 0.45f + d.avgNutrients * 0.18f + d.avgOxygen * 0.12f + d.avgLight * 0.08f + (1f - d.avgToxins) * 0.12f + (1f - Mathf.Abs(d.avgAcidity - 0.5f) * 2f) * 0.05f);
        }

        private V5DistrictType RecommendType(District d)
        {
            V5GameManager gm = V5GameManager.Instance;
            bool hasDetox = HasAnyAdaptation(gm, V5AdaptationId.CatalaseROS, V5AdaptationId.ContractileVacuole, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.ProtonPump);
            bool hasPhoto = HasAnyAdaptation(gm, V5AdaptationId.ProkaryoticThylakoid, V5AdaptationId.Chloroplast, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule);
            bool hasRecycler = HasAnyAdaptation(gm, V5AdaptationId.FungalHypha, V5AdaptationId.ExtracellularEnzymes, V5AdaptationId.SlimePlasmodium, V5AdaptationId.ChemicalMemory);
            bool hasPredator = HasAnyAdaptation(gm, V5AdaptationId.Lysosome, V5AdaptationId.Pseudopods, V5AdaptationId.Cilia, V5AdaptationId.EukaryoticFlagellum);
            bool hasDefense = HasAnyAdaptation(gm, V5AdaptationId.BacterialWall, V5AdaptationId.PolysaccharideCapsule, V5AdaptationId.ExtremophileMembrane, V5AdaptationId.CelluloseWall, V5AdaptationId.SilicaFrustule, V5AdaptationId.PersistentAdhesion);

            if ((d.avgToxins > 0.22f || Mathf.Abs(d.avgAcidity - 0.5f) > 0.20f) && hasDetox) return V5DistrictType.DetoxMatrix;
            if (d.avgLight > 0.54f && hasPhoto) return V5DistrictType.PhoticShelf;
            if (d.avgDetritus > 0.20f && hasRecycler) return V5DistrictType.MetabolicReactor;
            if (d.avgDetritus > 0.22f && hasPredator) return V5DistrictType.HuntingGround;
            if (d.avgColonization > StrongClaimThreshold && hasDefense) return V5DistrictType.DefensiveBastion;

            if (d.avgToxins > 0.28f || Mathf.Abs(d.avgAcidity - 0.5f) > 0.26f) return V5DistrictType.DetoxMatrix;
            if (d.avgLight > 0.62f) return V5DistrictType.PhoticShelf;
            if (d.avgDetritus > 0.30f) return hasRecycler ? V5DistrictType.MetabolicReactor : V5DistrictType.HuntingGround;
            if (d.avgOxygen > 0.45f || d.avgNutrients > 0.55f) return V5DistrictType.MetabolicReactor;
            if (d.avgColonization > StrongClaimThreshold) return V5DistrictType.DefensiveBastion;
            return V5DistrictType.Nursery;
        }

        private void ApplyDistrictEffects()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null || gm.Environment == null || gm.MotherCell == null) return;
            for (int i = 0; i < districts.Count; i++)
            {
                District d = districts[i];
                if (!d.claimed || !d.existsInsideMap) continue;
                float strength = Mathf.Clamp01(d.quality) * DistrictTickSeconds;
                strength *= DistrictAdaptationMultiplier(d.type, gm);
                switch (d.type)
                {
                    case V5DistrictType.Nursery:
                        gm.MotherCell.Resources.biomass += 0.08f * strength;
                        gm.MotherCell.Resources.aminoAcids += 0.05f * strength;
                        gm.Environment.ModifyArea(d.center, Mathf.Min(d.size.x, d.size.y) * 0.42f, 0.0008f * strength, 0f, 0f, -0.0004f * strength, 0f, 0.0008f * strength, 0f);
                        break;
                    case V5DistrictType.MetabolicReactor:
                        gm.MotherCell.Resources.atp += 0.18f * strength;
                        gm.MotherCell.Resources.minerals += 0.025f * strength;
                        gm.Environment.ModifyArea(d.center, Mathf.Min(d.size.x, d.size.y) * 0.40f, -0.0004f * strength, 0f, 0.0012f * strength, 0.0002f * strength, 0f, 0f, 0f);
                        break;
                    case V5DistrictType.DetoxMatrix:
                        gm.Environment.ModifyArea(d.center, Mathf.Min(d.size.x, d.size.y) * 0.48f, 0f, 0f, 0.0006f * strength, -0.0045f * strength, (0.5f - d.avgAcidity) * 0.0025f * strength, 0.0004f * strength, -0.0008f * strength);
                        gm.MotherCell.Stats.stress = Mathf.Max(0f, gm.MotherCell.Stats.stress - 0.018f * strength);
                        break;
                    case V5DistrictType.DefensiveBastion:
                        RepairOwnedCellsInDistrict(d, 0.12f * strength);
                        PushEnemiesInDistrict(d, 0.18f * strength);
                        gm.Environment.ModifyArea(d.center, Mathf.Min(d.size.x, d.size.y) * 0.44f, 0f, 0f, 0f, -0.0005f * strength, 0f, 0.0012f * strength, 0f);
                        break;
                    case V5DistrictType.PhoticShelf:
                        gm.MotherCell.Resources.atp += 0.08f * strength;
                        gm.Environment.ModifyArea(d.center, Mathf.Min(d.size.x, d.size.y) * 0.48f, 0f, 0.0018f * strength, 0.0028f * strength, -0.0002f * strength, 0f, 0.0005f * strength, 0f);
                        break;
                    case V5DistrictType.HuntingGround:
                        gm.MotherCell.Resources.biomass += 0.04f * strength;
                        gm.MotherCell.Resources.lipids += 0.025f * strength;
                        DamageEnemiesInDistrict(d, 0.20f * strength);
                        gm.Environment.ModifyArea(d.center, Mathf.Min(d.size.x, d.size.y) * 0.40f, 0.0012f * strength, 0f, 0f, -0.0002f * strength, 0f, 0.0002f * strength, -0.0010f * strength);
                        break;
                }
            }
        }

        private void UpdateSupply()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            int claimed = Mathf.Max(0, ClaimedCount);
            float roleBonus = 0f;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null) continue;
                if (c.LineageRole == V5LineageRole.Farmer || c.LineageRole == V5LineageRole.Colonizer || c.LineageRole == V5LineageRole.Recycler) roleBonus += 0.35f;
                if (c.HasStructure(V5StructureId.StorageVacuole)) roleBonus += 0.25f;
                if (c.HasStructure(V5StructureId.Fimbriae)) roleBonus += 0.25f;
            }
            roleBonus += SupplyAdaptationBonus(gm);
            SupplyCapacity = 6f + claimed * 2.25f + NetworkEfficiency * 5f + roleBonus;
            SupplyUsage = gm.PlayerCellCount();
            SupplyPressure = Mathf.Clamp01((SupplyUsage - SupplyCapacity) / Mathf.Max(1f, SupplyCapacity));
            if (SupplyPressure > 0.01f)
            {
                for (int i = 0; i < cells.Count; i++)
                {
                    V5CellEntity c = cells[i];
                    if (c != null) c.Stats.stress = Mathf.Clamp(c.Stats.stress + SupplyPressure * 0.065f * DistrictTickSeconds, 0f, 100f);
                }
            }
        }

        private float DistrictAdaptationMultiplier(V5DistrictType type, V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 1f;
            V5AdaptationSystem a = gm.Adaptations;
            float bonus = 0f;

            switch (type)
            {
                case V5DistrictType.Nursery:
                    if (a.Has(V5AdaptationId.RapidDivision)) bonus += 0.18f;
                    if (a.Has(V5AdaptationId.CellDifferentiation)) bonus += 0.16f;
                    if (a.Has(V5AdaptationId.SignalingCommunication)) bonus += 0.14f;
                    if (a.Has(V5AdaptationId.BasicAdhesin)) bonus += 0.08f;
                    break;
                case V5DistrictType.MetabolicReactor:
                    if (a.Has(V5AdaptationId.Mitochondria)) bonus += 0.18f;
                    if (a.Has(V5AdaptationId.ProtonPump)) bonus += 0.14f;
                    if (a.Has(V5AdaptationId.ProkaryoticThylakoid)) bonus += 0.14f;
                    if (a.Has(V5AdaptationId.Chloroplast)) bonus += 0.18f;
                    if (a.Has(V5AdaptationId.ExtracellularEnzymes)) bonus += 0.12f;
                    break;
                case V5DistrictType.DetoxMatrix:
                    if (a.Has(V5AdaptationId.CatalaseROS)) bonus += 0.22f;
                    if (a.Has(V5AdaptationId.ContractileVacuole)) bonus += 0.14f;
                    if (a.Has(V5AdaptationId.ExtremophileMembrane)) bonus += 0.16f;
                    if (a.Has(V5AdaptationId.ProtonPump)) bonus += 0.12f;
                    break;
                case V5DistrictType.DefensiveBastion:
                    if (a.Has(V5AdaptationId.BacterialWall)) bonus += 0.12f;
                    if (a.Has(V5AdaptationId.PolysaccharideCapsule)) bonus += 0.14f;
                    if (a.Has(V5AdaptationId.CelluloseWall)) bonus += 0.14f;
                    if (a.Has(V5AdaptationId.SilicaFrustule)) bonus += 0.20f;
                    if (a.Has(V5AdaptationId.PersistentAdhesion)) bonus += 0.16f;
                    break;
                case V5DistrictType.PhoticShelf:
                    if (a.Has(V5AdaptationId.ProkaryoticThylakoid)) bonus += 0.22f;
                    if (a.Has(V5AdaptationId.Chloroplast)) bonus += 0.22f;
                    if (a.Has(V5AdaptationId.CelluloseWall)) bonus += 0.10f;
                    if (a.Has(V5AdaptationId.SilicaFrustule)) bonus += 0.10f;
                    break;
                case V5DistrictType.HuntingGround:
                    if (a.Has(V5AdaptationId.Lysosome)) bonus += 0.20f;
                    if (a.Has(V5AdaptationId.Pseudopods)) bonus += 0.18f;
                    if (a.Has(V5AdaptationId.Cilia)) bonus += 0.14f;
                    if (a.Has(V5AdaptationId.EukaryoticFlagellum)) bonus += 0.12f;
                    if (a.Has(V5AdaptationId.ChemicalMemory)) bonus += 0.10f;
                    break;
            }

            return 1f + Mathf.Clamp(bonus, 0f, 0.60f);
        }

        private float SupplyAdaptationBonus(V5GameManager gm)
        {
            if (gm == null || gm.Adaptations == null) return 0f;
            V5AdaptationSystem a = gm.Adaptations;
            float bonus = 0f;
            if (a.Has(V5AdaptationId.BasicAdhesin)) bonus += 0.65f;
            if (a.Has(V5AdaptationId.PiliFimbriae)) bonus += 0.45f;
            if (a.Has(V5AdaptationId.ColonialAdhesin)) bonus += 0.95f;
            if (a.Has(V5AdaptationId.PersistentAdhesion)) bonus += 1.20f;
            if (a.Has(V5AdaptationId.SignalingCommunication)) bonus += 0.90f;
            if (a.Has(V5AdaptationId.CellDifferentiation)) bonus += 0.75f;
            if (a.Has(V5AdaptationId.ChemicalMemory)) bonus += 0.55f;
            return bonus;
        }

        private bool HasAnyAdaptation(V5GameManager gm, params V5AdaptationId[] ids)
        {
            if (gm == null || gm.Adaptations == null || ids == null) return false;
            for (int i = 0; i < ids.Length; i++)
            {
                if (gm.Adaptations.Has(ids[i])) return true;
            }
            return false;
        }

        private void RepairOwnedCellsInDistrict(District d, float amount)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> cells = gm.PlayerCells;
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity c = cells[i];
                if (c == null || !PointInDistrict(c.transform.position, d)) continue;
                c.Stats.currentHp = Mathf.Min(c.Stats.maxHp, c.Stats.currentHp + amount);
                c.Stats.stress = Mathf.Max(0f, c.Stats.stress - amount * 0.07f);
            }
        }

        private void DamageEnemiesInDistrict(District d, float amount)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> enemies = gm.NonPlayerCells;
            for (int i = 0; i < enemies.Count; i++)
            {
                V5CellEntity c = enemies[i];
                if (c != null && PointInDistrict(c.transform.position, d)) c.Damage(amount, V5DamageKind.Physical, d.center);
            }
        }

        private void PushEnemiesInDistrict(District d, float stress)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            IReadOnlyList<V5CellEntity> enemies = gm.NonPlayerCells;
            for (int i = 0; i < enemies.Count; i++)
            {
                V5CellEntity c = enemies[i];
                if (c != null && PointInDistrict(c.transform.position, d)) c.Stats.stress = Mathf.Clamp(c.Stats.stress + stress, 0f, 100f);
            }
        }

        private bool PointInDistrict(Vector2 p, District d)
        {
            return Mathf.Abs(p.x - d.center.x) <= d.size.x * 0.5f && Mathf.Abs(p.y - d.center.y) <= d.size.y * 0.5f;
        }

        private void DrawDistrictGrid(Rect r)
        {
            GUI.Box(r, "", box);
            float cellW = r.width / Mathf.Max(1, Columns);
            float cellH = r.height / Mathf.Max(1, Rows);
            for (int i = 0; i < districts.Count; i++)
            {
                District d = districts[i];
                Rect b = new Rect(r.x + d.gridX * cellW + 2, r.y + (Rows - 1 - d.gridY) * cellH + 2, cellW - 4, cellH - 4);
                Color old = GUI.color;
                GUI.color = d.claimed ? Color.Lerp(Color.white, ColorForType(d.type), 0.55f) : new Color(0.3f, 0.3f, 0.3f, 0.8f);
                if (GUI.Button(b, d.claimed ? ShortType(d.type) + "\n" + (d.avgColonization * 100f).ToString("0") + "%" : (d.avgColonization * 100f).ToString("0") + "%", button)) selectedIndex = d.index;
                GUI.color = old;
            }
        }

        private void DrawSelectedDistrict(Rect r)
        {
            GUI.Box(r, "", box);
            if (districts.Count == 0) return;
            selectedIndex = Mathf.Clamp(selectedIndex, 0, districts.Count - 1);
            District d = districts[selectedIndex];
            GUI.Label(new Rect(r.x + 10, r.y + 8, r.width - 20, 20), "District " + d.Label, title);
            GUI.Label(new Rect(r.x + 10, r.y + 34, r.width - 20, 80), string.Format("Claimed: {0}\nType: {1}\nQuality: {2:0}%\nColonization: {3:0}%\nTox/O2/Light: {4:0}% / {5:0}% / {6:0}%", d.claimed ? "yes" : "no", d.type, d.quality * 100f, d.avgColonization * 100f, d.avgToxins * 100f, d.avgOxygen * 100f, d.avgLight * 100f));
            GUI.enabled = d.claimed;
            float y = r.y + 120;
            if (GUI.Button(new Rect(r.x + 10, y, r.width - 20, 24), "Nursery", button)) d.type = V5DistrictType.Nursery;
            y += 27;
            if (GUI.Button(new Rect(r.x + 10, y, r.width - 20, 24), "Metabolic Reactor", button)) d.type = V5DistrictType.MetabolicReactor;
            y += 27;
            if (GUI.Button(new Rect(r.x + 10, y, r.width - 20, 24), "Detox Matrix", button)) d.type = V5DistrictType.DetoxMatrix;
            y += 27;
            if (GUI.Button(new Rect(r.x + 10, y, r.width - 20, 24), "Defensive Bastion", button)) d.type = V5DistrictType.DefensiveBastion;
            y += 27;
            if (GUI.Button(new Rect(r.x + 10, y, r.width - 20, 24), "Photic Shelf", button)) d.type = V5DistrictType.PhoticShelf;
            y += 27;
            if (GUI.Button(new Rect(r.x + 10, y, r.width - 20, 24), "Hunting Ground", button)) d.type = V5DistrictType.HuntingGround;
            GUI.enabled = true;
        }

        private void DrawDistrictList(Rect r)
        {
            GUI.Box(r, "", box);
            Rect content = new Rect(0, 0, r.width - 20, Mathf.Max(r.height, districts.Count * 24 + 10));
            scroll = GUI.BeginScrollView(r, scroll, content);
            float y = 4f;
            for (int i = 0; i < districts.Count; i++)
            {
                District d = districts[i];
                if (!d.existsInsideMap) continue;
                string label = string.Format("{0} | {1} | colon {2:0}% | q {3:0}% | tox {4:0}%", d.Label, d.claimed ? d.type.ToString() : "unclaimed", d.avgColonization * 100f, d.quality * 100f, d.avgToxins * 100f);
                if (GUI.Button(new Rect(4, y, content.width - 8, 22), label, button)) selectedIndex = d.index;
                y += 24f;
            }
            GUI.EndScrollView();
        }

        private void AutoDesignDistricts()
        {
            EnsureDistricts();
            int changed = 0;
            for (int i = 0; i < districts.Count; i++)
            {
                District d = districts[i];
                if (!d.claimed) continue;
                V5DistrictType recommended = RecommendType(d);
                if (d.type != recommended)
                {
                    d.type = recommended;
                    changed++;
                }
            }
            LastMessage = "Auto-designed " + changed + " claimed districts.";
            Toast(LastMessage);
        }

        private void SelectNearestToMother()
        {
            EnsureDistricts();
            V5CellEntity mother = V5GameManager.Instance != null ? V5GameManager.Instance.MotherCell : null;
            if (mother == null || districts.Count == 0) return;
            float best = float.MaxValue;
            int bestIndex = selectedIndex;
            for (int i = 0; i < districts.Count; i++)
            {
                District d = districts[i];
                if (!d.existsInsideMap) continue;
                float dist = Vector2.Distance(mother.transform.position, d.center);
                if (dist < best)
                {
                    best = dist;
                    bestIndex = d.index;
                }
            }
            selectedIndex = bestIndex;
        }

        private void ExportDistricts()
        {
            EnsureDistricts();
            V5DistrictSaveFile save = new V5DistrictSaveFile();
            for (int i = 0; i < districts.Count; i++)
            {
                District d = districts[i];
                V5DistrictSaveEntry e = new V5DistrictSaveEntry();
                e.index = d.index;
                e.claimed = d.claimed;
                e.type = d.type;
                save.districts.Add(e);
            }
            File.WriteAllText(SavePath, JsonUtility.ToJson(save, true));
            LastMessage = "District profile exported: " + SavePath;
            Toast("District profile exported.");
        }

        private void ImportDistricts()
        {
            EnsureDistricts();
            if (!File.Exists(SavePath))
            {
                LastMessage = "No district profile found.";
                Toast(LastMessage);
                return;
            }
            V5DistrictSaveFile save = JsonUtility.FromJson<V5DistrictSaveFile>(File.ReadAllText(SavePath));
            if (save == null || save.districts == null) return;
            for (int i = 0; i < save.districts.Count; i++)
            {
                V5DistrictSaveEntry e = save.districts[i];
                if (e.index >= 0 && e.index < districts.Count)
                {
                    districts[e.index].claimed = e.claimed;
                    districts[e.index].type = e.type;
                }
            }
            LastMessage = "District profile imported.";
            Toast(LastMessage);
        }

        private string ShortType(V5DistrictType type)
        {
            switch (type)
            {
                case V5DistrictType.Nursery: return "NUR";
                case V5DistrictType.MetabolicReactor: return "ATP";
                case V5DistrictType.DetoxMatrix: return "DET";
                case V5DistrictType.DefensiveBastion: return "DEF";
                case V5DistrictType.PhoticShelf: return "LUX";
                case V5DistrictType.HuntingGround: return "HNT";
                default: return "DST";
            }
        }

        private Color ColorForType(V5DistrictType type)
        {
            switch (type)
            {
                case V5DistrictType.Nursery: return new Color(0.5f, 1f, 0.55f, 1f);
                case V5DistrictType.MetabolicReactor: return new Color(1f, 0.86f, 0.25f, 1f);
                case V5DistrictType.DetoxMatrix: return new Color(0.45f, 0.9f, 1f, 1f);
                case V5DistrictType.DefensiveBastion: return new Color(1f, 0.55f, 0.35f, 1f);
                case V5DistrictType.PhoticShelf: return new Color(0.7f, 1f, 0.35f, 1f);
                case V5DistrictType.HuntingGround: return new Color(1f, 0.38f, 0.38f, 1f);
                default: return Color.white;
            }
        }

        private void Toast(string message)
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm != null && gm.Hud != null) gm.Hud.Toast(message);
        }

        private class District
        {
            public int index;
            public int gridX;
            public int gridY;
            public Vector2 center;
            public Vector2 size;
            public bool existsInsideMap;
            public bool claimed;
            public V5DistrictType type;
            public float avgColonization;
            public float avgNutrients;
            public float avgLight;
            public float avgOxygen;
            public float avgToxins;
            public float avgAcidity;
            public float avgDetritus;
            public float quality;
            public string Label { get { return ((char)('A' + gridY)).ToString() + (gridX + 1).ToString(); } }
        }
    }
}
