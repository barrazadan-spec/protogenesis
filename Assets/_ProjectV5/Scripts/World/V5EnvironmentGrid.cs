using UnityEngine;

namespace Protogenesis.V5
{
    public class V5EnvironmentGrid : MonoBehaviour
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public float TileSize { get; private set; }
        public float MapRadius { get; private set; }
        public float[,] nutrients;
        public float[,] lightLevel;
        public float[,] oxygen;
        public float[,] toxins;
        public float[,] acidity;
        public float[,] colonization;
        public float[,] temperature;
        public float[,] detritus;

        private float tickTimer;

        public void Initialize(int width, int height, float tileSize, float mapRadius)
        {
            Width = Mathf.Max(16, width);
            Height = Mathf.Max(16, height);
            TileSize = Mathf.Max(0.25f, tileSize);
            MapRadius = Mathf.Max(8f, mapRadius);
            nutrients = new float[Width, Height];
            lightLevel = new float[Width, Height];
            oxygen = new float[Width, Height];
            toxins = new float[Width, Height];
            acidity = new float[Width, Height];
            colonization = new float[Width, Height];
            temperature = new float[Width, Height];
            detritus = new float[Width, Height];

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector2 wp = TileCenterWorld(x, y);
                    float d = wp.magnitude / MapRadius;
                    float noise = Mathf.PerlinNoise(x * 0.09f, y * 0.09f);
                    nutrients[x, y] = Mathf.Clamp01(0.35f + noise * 0.65f - d * 0.18f);
                    lightLevel[x, y] = Mathf.Clamp01((float)y / (float)(Height - 1) * 0.85f + Mathf.PerlinNoise(x * 0.04f + 20f, y * 0.04f) * 0.25f);
                    oxygen[x, y] = Mathf.Clamp01(0.18f + lightLevel[x, y] * 0.15f);
                    toxins[x, y] = Mathf.Clamp01(Mathf.PerlinNoise(x * 0.06f + 100f, y * 0.06f) * 0.12f);
                    acidity[x, y] = Mathf.Clamp01(0.45f + Mathf.PerlinNoise(x * 0.04f + 200f, y * 0.04f) * 0.18f);
                    colonization[x, y] = 0f;
                    temperature[x, y] = Mathf.Clamp01(0.45f + Mathf.PerlinNoise(x * 0.035f + 300f, y * 0.035f) * 0.22f);
                    detritus[x, y] = 0f;
                }
            }
        }

        private void Update()
        {
            tickTimer += Time.deltaTime;
            if (tickTimer >= V5Balance.EnvironmentTick)
            {
                tickTimer = 0f;
                SimulateEnvironmentTick();
            }
        }

        private void SimulateEnvironmentTick()
        {
            if (nutrients == null) return;
            for (int x = 1; x < Width - 1; x += 2)
            {
                for (int y = 1; y < Height - 1; y += 2)
                {
                    oxygen[x, y] = Mathf.Clamp01(oxygen[x, y] + lightLevel[x, y] * 0.002f - toxins[x, y] * 0.0008f);
                    toxins[x, y] = Mathf.Clamp01(toxins[x, y] * 0.997f + detritus[x, y] * 0.0012f);
                    detritus[x, y] = Mathf.Clamp01(detritus[x, y] * 0.996f);
                    nutrients[x, y] = Mathf.Clamp01(nutrients[x, y] + detritus[x, y] * 0.001f - colonization[x, y] * 0.0005f);
                    colonization[x, y] = Mathf.Clamp01(colonization[x, y] * 0.999f);
                }
            }
        }

        public Vector2 TileCenterWorld(int x, int y)
        {
            return new Vector2((x - Width * 0.5f) * TileSize, (y - Height * 0.5f) * TileSize);
        }

        public bool WorldToTile(Vector2 world, out int x, out int y)
        {
            x = Mathf.FloorToInt(world.x / TileSize + Width * 0.5f);
            y = Mathf.FloorToInt(world.y / TileSize + Height * 0.5f);
            bool inside = x >= 0 && y >= 0 && x < Width && y < Height && world.magnitude <= MapRadius;
            x = Mathf.Clamp(x, 0, Width - 1);
            y = Mathf.Clamp(y, 0, Height - 1);
            return inside;
        }

        public float Sample(V5OverlayMode channel, Vector2 world)
        {
            int x, y;
            WorldToTile(world, out x, out y);
            switch (channel)
            {
                case V5OverlayMode.Nutrients: return nutrients[x, y];
                case V5OverlayMode.Light: return lightLevel[x, y];
                case V5OverlayMode.Oxygen: return oxygen[x, y];
                case V5OverlayMode.Toxins: return toxins[x, y];
                case V5OverlayMode.Acidity: return acidity[x, y];
                case V5OverlayMode.Colonization: return colonization[x, y];
                case V5OverlayMode.Temperature: return temperature[x, y];
                default: return 0f;
            }
        }

        public void ModifyArea(Vector2 world, float radius, float nutrientDelta, float lightDelta, float oxygenDelta, float toxinDelta, float acidityDelta, float colonizationDelta, float detritusDelta)
        {
            int cx, cy;
            WorldToTile(world, out cx, out cy);
            int r = Mathf.CeilToInt(radius / TileSize);
            for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(Width - 1, cx + r); x++)
            {
                for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(Height - 1, cy + r); y++)
                {
                    float dist = Vector2.Distance(world, TileCenterWorld(x, y));
                    if (dist > radius) continue;
                    float f = 1f - (dist / Mathf.Max(0.001f, radius));
                    nutrients[x, y] = Mathf.Clamp01(nutrients[x, y] + nutrientDelta * f);
                    lightLevel[x, y] = Mathf.Clamp01(lightLevel[x, y] + lightDelta * f);
                    oxygen[x, y] = Mathf.Clamp01(oxygen[x, y] + oxygenDelta * f);
                    toxins[x, y] = Mathf.Clamp01(toxins[x, y] + toxinDelta * f);
                    acidity[x, y] = Mathf.Clamp01(acidity[x, y] + acidityDelta * f);
                    colonization[x, y] = Mathf.Clamp01(colonization[x, y] + colonizationDelta * f);
                    detritus[x, y] = Mathf.Clamp01(detritus[x, y] + detritusDelta * f);
                }
            }
        }

        public void RaiseLightOasis(Vector2 world, float radius, float centerLight = 0.95f, float edgeLight = 0.58f)
        {
            if (lightLevel == null) return;
            int cx, cy;
            WorldToTile(world, out cx, out cy);
            int r = Mathf.CeilToInt(radius / TileSize);
            for (int x = Mathf.Max(0, cx - r); x <= Mathf.Min(Width - 1, cx + r); x++)
            {
                for (int y = Mathf.Max(0, cy - r); y <= Mathf.Min(Height - 1, cy + r); y++)
                {
                    float distance = Vector2.Distance(world, TileCenterWorld(x, y));
                    if (distance > radius) continue;
                    float falloff = 1f - distance / Mathf.Max(0.001f, radius);
                    float shaped = falloff * falloff * (3f - 2f * falloff);
                    float oasisLight = Mathf.Lerp(edgeLight, centerLight, shaped);
                    lightLevel[x, y] = Mathf.Max(lightLevel[x, y], oasisLight);
                }
            }
        }

        public void ApplyScenarioBias(V5ScenarioDefinition def)
        {
            if (def == null || nutrients == null) return;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    Vector2 p = TileCenterWorld(x, y);
                    if (p.magnitude > MapRadius) continue;
                    float radial = 1f - Mathf.Clamp01(p.magnitude / Mathf.Max(0.01f, MapRadius));
                    toxins[x, y] = Mathf.Clamp01(toxins[x, y] + def.startingToxinBias * (0.55f + radial * 0.45f));
                    acidity[x, y] = Mathf.Clamp01(acidity[x, y] + def.startingAcidityBias * (0.35f + Mathf.PerlinNoise(x * 0.10f + 500f, y * 0.10f + 500f) * 0.65f));
                    lightLevel[x, y] = Mathf.Clamp01(lightLevel[x, y] + def.startingLightBias * ((float)y / Mathf.Max(1f, Height - 1f)));
                    nutrients[x, y] = Mathf.Clamp01(nutrients[x, y] + (def.id == V5ScenarioId.Freeplay ? 0.08f : 0f));
                }
            }
        }

        public float AverageOxygen()
        {
            return AverageChannel(oxygen);
        }

        public float AverageToxins()
        {
            return AverageChannel(toxins);
        }

        public float AverageAcidity()
        {
            return AverageChannel(acidity);
        }

        private float AverageChannel(float[,] channel)
        {
            if (channel == null) return 0f;
            float total = 0f;
            int count = 0;
            for (int x = 0; x < Width; x += 3)
            {
                for (int y = 0; y < Height; y += 3)
                {
                    Vector2 p = TileCenterWorld(x, y);
                    if (p.magnitude <= MapRadius)
                    {
                        total += channel[x, y];
                        count++;
                    }
                }
            }
            return count > 0 ? total / count : 0f;
        }

        public float AverageColonization()
        {
            if (colonization == null) return 0f;
            float total = 0f;
            int count = 0;
            for (int x = 0; x < Width; x += 3)
            {
                for (int y = 0; y < Height; y += 3)
                {
                    Vector2 p = TileCenterWorld(x, y);
                    if (p.magnitude <= MapRadius)
                    {
                        total += colonization[x, y];
                        count++;
                    }
                }
            }
            return count > 0 ? total / count : 0f;
        }

        public V5EnvironmentSnapshot CreateSnapshot()
        {
            V5EnvironmentSnapshot snapshot = new V5EnvironmentSnapshot();
            snapshot.width = Width;
            snapshot.height = Height;
            snapshot.tileSize = TileSize;
            snapshot.mapRadius = MapRadius;
            CopyChannelToList(nutrients, snapshot.nutrients);
            CopyChannelToList(lightLevel, snapshot.light);
            CopyChannelToList(oxygen, snapshot.oxygen);
            CopyChannelToList(toxins, snapshot.toxins);
            CopyChannelToList(acidity, snapshot.acidity);
            CopyChannelToList(colonization, snapshot.colonization);
            CopyChannelToList(temperature, snapshot.temperature);
            CopyChannelToList(detritus, snapshot.detritus);
            return snapshot;
        }

        public void ApplySnapshot(V5EnvironmentSnapshot snapshot)
        {
            if (snapshot == null || snapshot.width <= 0 || snapshot.height <= 0) return;
            Initialize(snapshot.width, snapshot.height, snapshot.tileSize, snapshot.mapRadius);
            CopyListToChannel(snapshot.nutrients, nutrients);
            CopyListToChannel(snapshot.light, lightLevel);
            CopyListToChannel(snapshot.oxygen, oxygen);
            CopyListToChannel(snapshot.toxins, toxins);
            CopyListToChannel(snapshot.acidity, acidity);
            CopyListToChannel(snapshot.colonization, colonization);
            CopyListToChannel(snapshot.temperature, temperature);
            CopyListToChannel(snapshot.detritus, detritus);
        }

        private void CopyChannelToList(float[,] source, System.Collections.Generic.List<float> target)
        {
            target.Clear();
            if (source == null) return;
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    target.Add(source[x, y]);
        }

        private void CopyListToChannel(System.Collections.Generic.List<float> source, float[,] target)
        {
            if (source == null || target == null) return;
            int index = 0;
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    if (index >= source.Count) return;
                    target[x, y] = Mathf.Clamp01(source[index]);
                    index++;
                }
            }
        }
    }
}
