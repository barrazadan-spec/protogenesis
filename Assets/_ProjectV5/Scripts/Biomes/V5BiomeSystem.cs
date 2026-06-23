using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public enum V5BiomeType
    {
        NeutralBroth,
        PhoticShelf,
        OxygenBloom,
        ToxicBloom,
        AcidFrontier,
        DetritusField,
        ColonizedMatrix,
        ThermalPocket
    }

    public struct V5BiomeSample
    {
        public V5BiomeType biome;
        public float nutrients;
        public float light;
        public float oxygen;
        public float toxins;
        public float acidity;
        public float colonization;
        public float temperature;
        public float detritus;

        public override string ToString()
        {
            return biome + " | Nut " + nutrients.ToString("0.00") + " Luz " + light.ToString("0.00") + " O2 " + oxygen.ToString("0.00") + " Tox " + toxins.ToString("0.00") + " pH " + acidity.ToString("0.00") + " Col " + colonization.ToString("0.00");
        }
    }

    public class V5BiomeSystem : MonoBehaviour
    {
        public bool ShowPanel;
        public bool ApplyBiomeEffects = true;
        public string LastMessage = "B: mostrar microzonas. Las microzonas nacen de luz, O2, toxinas, acidez, detritus y colonización.";
        public V5BiomeType MotherBiome = V5BiomeType.NeutralBroth;
        public float AverageHostility;
        public float AverageProductivity;

        private readonly Dictionary<V5BiomeType, int> counts = new Dictionary<V5BiomeType, int>();
        private readonly Dictionary<V5CellEntity, float> nextCellTick = new Dictionary<V5CellEntity, float>();
        private GUIStyle box;
        private GUIStyle title;
        private GUIStyle button;
        private float mapSampleTimer;
        private float effectTimer;

        private void Start() => V5PanelRouter.Register("Biomas", () => ShowPanel, v => ShowPanel = v);

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Backslash)) { if (!ShowPanel) V5PanelRouter.CloseOthers("Biomas"); ShowPanel = !ShowPanel; }

            mapSampleTimer += Time.deltaTime;
            if (mapSampleTimer >= 1.25f)
            {
                mapSampleTimer = 0f;
                RecalculateMapSummary();
            }

            effectTimer += Time.deltaTime;
            if (effectTimer >= 0.35f)
            {
                effectTimer = 0f;
                if (ApplyBiomeEffects) ApplyEffectsToCells();
            }
        }

        public V5BiomeSample SampleAt(Vector2 world)
        {
            V5BiomeSample s = new V5BiomeSample();
            V5GameManager gm = V5GameManager.Instance;
            V5EnvironmentGrid env = gm != null ? gm.Environment : null;
            if (env == null) return s;
            int x, y;
            env.WorldToTile(world, out x, out y);
            s.nutrients = env.nutrients[x, y];
            s.light = env.lightLevel[x, y];
            s.oxygen = env.oxygen[x, y];
            s.toxins = env.toxins[x, y];
            s.acidity = env.acidity[x, y];
            s.colonization = env.colonization[x, y];
            s.temperature = env.temperature[x, y];
            s.detritus = env.detritus[x, y];
            s.biome = Classify(s);
            return s;
        }

        public V5BiomeType Classify(V5BiomeSample s)
        {
            if (s.colonization >= 0.55f) return V5BiomeType.ColonizedMatrix;
            if (s.toxins >= 0.52f) return V5BiomeType.ToxicBloom;
            if (s.acidity >= 0.72f || s.acidity <= 0.18f) return V5BiomeType.AcidFrontier;
            if (s.detritus >= 0.42f) return V5BiomeType.DetritusField;
            if (s.temperature >= 0.72f || s.temperature <= 0.18f) return V5BiomeType.ThermalPocket;
            if (s.light >= 0.60f && s.nutrients >= 0.36f) return V5BiomeType.PhoticShelf;
            if (s.oxygen >= 0.58f) return V5BiomeType.OxygenBloom;
            return V5BiomeType.NeutralBroth;
        }

        public float ProductivityMultiplier(V5CellEntity cell)
        {
            if (cell == null) return 1f;
            V5BiomeSample s = SampleAt(cell.transform.position);
            switch (s.biome)
            {
                case V5BiomeType.PhoticShelf:
                    return (cell.Metabolism == V5MetabolismType.Photosynthesis || cell.HasPhotosynthesis) ? 1.35f : 1.05f;
                case V5BiomeType.OxygenBloom:
                    return cell.Metabolism == V5MetabolismType.Respiration ? 1.25f : 0.95f;
                case V5BiomeType.DetritusField:
                    return cell.LineageRole == V5LineageRole.Recycler || cell.EvolutionPath == V5EvolutionPath.Fungus || cell.EvolutionPath == V5EvolutionPath.SlimeMold ? 1.35f : 1.08f;
                case V5BiomeType.ColonizedMatrix:
                    return cell.IsPlayerOwned ? 1.12f : 0.92f;
                case V5BiomeType.ToxicBloom:
                    return Mathf.Lerp(0.70f, 1.05f, Mathf.Clamp01(cell.Stats.toxinResistance));
                case V5BiomeType.AcidFrontier:
                    return cell.EvolutionPath == V5EvolutionPath.Archaea || cell.Metabolism == V5MetabolismType.Chemolithotrophy ? 1.22f : 0.78f;
                case V5BiomeType.ThermalPocket:
                    return cell.EvolutionPath == V5EvolutionPath.Archaea ? 1.18f : 0.86f;
                default:
                    return 1f;
            }
        }

        public float HostilityAt(Vector2 world)
        {
            V5BiomeSample s = SampleAt(world);
            float phBad = Mathf.Abs(s.acidity - 0.5f) * 1.2f;
            float thermalBad = Mathf.Abs(s.temperature - 0.5f) * 0.8f;
            return Mathf.Clamp01(s.toxins * 0.65f + phBad + thermalBad - s.colonization * 0.15f);
        }

        private void ApplyEffectsToCells()
        {
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            ApplyList(gm.PlayerCells);
            ApplyList(gm.NonPlayerCells);
        }

        private void ApplyList(IReadOnlyList<V5CellEntity> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                V5CellEntity cell = cells[i];
                if (cell == null) continue;
                float next;
                if (nextCellTick.TryGetValue(cell, out next) && Time.time < next) continue;
                nextCellTick[cell] = Time.time + 0.9f + Random.value * 0.25f;
                ApplyBiomeEffect(cell);
            }
        }

        private void ApplyBiomeEffect(V5CellEntity cell)
        {
            V5BiomeSample s = SampleAt(cell.transform.position);
            float prod = ProductivityMultiplier(cell);
            if (prod > 1f)
            {
                cell.Resources.atp += (prod - 1f) * 1.6f;
                if (s.biome == V5BiomeType.DetritusField) cell.Resources.biomass += 0.25f;
            }
            else if (prod < 0.95f)
            {
                cell.Stats.stress += (0.95f - prod) * 2.2f;
            }

            if (s.biome == V5BiomeType.ColonizedMatrix && cell.IsPlayerOwned)
            {
                cell.Stats.stress = Mathf.Max(0f, cell.Stats.stress - 0.25f);
            }

            if (s.biome == V5BiomeType.ToxicBloom && cell.Stats.toxinResistance < 0.35f)
            {
                cell.Damage(0.45f, V5DamageKind.Chemical, cell.transform.position);
            }
        }

        private void RecalculateMapSummary()
        {
            counts.Clear();
            V5GameManager gm = V5GameManager.Instance;
            V5EnvironmentGrid env = gm != null ? gm.Environment : null;
            if (env == null) return;

            float hostility = 0f;
            float productivity = 0f;
            int n = 0;
            for (int x = 1; x < env.Width - 1; x += 5)
            {
                for (int y = 1; y < env.Height - 1; y += 5)
                {
                    Vector2 p = env.TileCenterWorld(x, y);
                    if (p.magnitude > env.MapRadius) continue;
                    V5BiomeSample s = SampleAt(p);
                    if (!counts.ContainsKey(s.biome)) counts[s.biome] = 0;
                    counts[s.biome]++;
                    hostility += HostilityAt(p);
                    productivity += Mathf.Clamp01(s.nutrients * 0.45f + s.light * 0.2f + s.oxygen * 0.2f + s.detritus * 0.15f - s.toxins * 0.25f);
                    n++;
                }
            }
            AverageHostility = n > 0 ? hostility / n : 0f;
            AverageProductivity = n > 0 ? productivity / n : 0f;
            if (gm.MotherCell != null) MotherBiome = SampleAt(gm.MotherCell.transform.position).biome;
        }

        private void OnGUI()
        {
            if (!ShowPanel) return;
            EnsureStyles();
            V5GameManager gm = V5GameManager.Instance;
            if (gm == null) return;
            Rect r = new Rect(10, 212, 420, 360);
            GUI.Box(r, "", box);
            GUI.Label(new Rect(r.x + 12, r.y + 10, r.width - 24, 24), "MICROZONAS / BIOMAS CELULARES", title);
            GUI.Label(new Rect(r.x + 12, r.y + 38, r.width - 24, 22), "Madre: " + MotherBiome + " | Hostilidad global " + (AverageHostility * 100f).ToString("0") + "% | Productividad " + (AverageProductivity * 100f).ToString("0") + "%");

            V5CellEntity selected = gm.Selection != null && gm.Selection.Selected.Count > 0 ? gm.Selection.Selected[0] : gm.MotherCell;
            if (selected != null)
            {
                V5BiomeSample s = SampleAt(selected.transform.position);
                GUI.Label(new Rect(r.x + 12, r.y + 66, r.width - 24, 44), "Selección: " + s.ToString());
                GUI.Label(new Rect(r.x + 12, r.y + 112, r.width - 24, 22), "Multiplicador de productividad: x" + ProductivityMultiplier(selected).ToString("0.00") + " | Hostilidad local " + (HostilityAt(selected.transform.position) * 100f).ToString("0") + "%");
            }

            float y = r.y + 146;
            GUI.Label(new Rect(r.x + 12, y, 390, 22), "Distribución del mapa:");
            y += 24;
            foreach (V5BiomeType type in System.Enum.GetValues(typeof(V5BiomeType)))
            {
                int c = counts.ContainsKey(type) ? counts[type] : 0;
                GUI.Label(new Rect(r.x + 24, y, 240, 20), type.ToString());
                GUI.Label(new Rect(r.x + 265, y, 120, 20), c.ToString());
                y += 20;
            }
            y += 6;
            ApplyBiomeEffects = GUI.Toggle(new Rect(r.x + 12, y, 280, 24), ApplyBiomeEffects, "Aplicar efectos sistémicos");
            if (GUI.Button(new Rect(r.x + 300, y, 96, 26), "Recalcular", button)) RecalculateMapSummary();
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
        }
    }
}
