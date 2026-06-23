using System.Collections.Generic;
using UnityEngine;

namespace Protogenesis.V5
{
    public class V5ScenarioDefinition
    {
        public V5ScenarioId id;
        public string displayName;
        public string description;
        public string primaryObjective;
        public string failureText;
        public int mapWidth;
        public int mapHeight;
        public float tileSize;
        public int startingResources;
        public int neutralCells;
        public float colonizationTarget;
        public float survivalSeconds;
        public float minimumDiscovered;
        public float startingToxinBias;
        public float startingAcidityBias;
        public float startingLightBias;
        public bool requireApex;
        public bool requireEnemyClear;
        public bool spawnEnemies;

        public float MapRadius
        {
            get { return Mathf.Min(mapWidth, mapHeight) * tileSize * 0.47f; }
        }
    }

    public static class V5ScenarioLibrary
    {
        private static readonly Dictionary<V5ScenarioId, V5ScenarioDefinition> defs = new Dictionary<V5ScenarioId, V5ScenarioDefinition>();

        static V5ScenarioLibrary()
        {
            Add(new V5ScenarioDefinition
            {
                id = V5ScenarioId.FirstDrop,
                displayName = "La primera gota",
                description = "Escenario tutorial. Construye una colonia estable y domina la gota sin presión extrema.",
                primaryObjective = "Coloniza 40% de la gota o estabiliza el ecosistema.",
                failureText = "la gota colapsó antes de estabilizarse",
                mapWidth = 96,
                mapHeight = 96,
                tileSize = 0.85f,
                startingResources = 90,
                neutralCells = 2,
                colonizationTarget = 0.40f,
                survivalSeconds = 35f * 60f,
                minimumDiscovered = 0.10f,
                spawnEnemies = true
            });

            Add(new V5ScenarioDefinition
            {
                id = V5ScenarioId.OxygenWar,
                displayName = "Guerra del oxígeno",
                description = "El mapa premia fotosíntesis y respiración. Anaerobios sufren si no se refugian.",
                primaryObjective = "Eleva el oxígeno medio y coloniza 35% antes del cierre.",
                failureText = "el frente de oxígeno destruyó tu nicho",
                mapWidth = 104,
                mapHeight = 104,
                tileSize = 0.82f,
                startingResources = 82,
                neutralCells = 6,
                colonizationTarget = 0.35f,
                survivalSeconds = 30f * 60f,
                minimumDiscovered = 0.18f,
                startingLightBias = 0.12f,
                spawnEnemies = true
            });

            Add(new V5ScenarioDefinition
            {
                id = V5ScenarioId.AcidFrontier,
                displayName = "Frontera ácida",
                description = "Un escenario hostil para probar arqueas, catalasa y control de pH.",
                primaryObjective = "Sobrevive 25 minutos y coloniza 30% del mapa ácido.",
                failureText = "el pH extremo rompió la colonia",
                mapWidth = 92,
                mapHeight = 92,
                tileSize = 0.90f,
                startingResources = 76,
                neutralCells = 5,
                colonizationTarget = 0.30f,
                survivalSeconds = 25f * 60f,
                minimumDiscovered = 0.12f,
                startingAcidityBias = 0.22f,
                startingToxinBias = 0.06f,
                spawnEnemies = true
            });

            Add(new V5ScenarioDefinition
            {
                id = V5ScenarioId.PredatorBloom,
                displayName = "Bloom depredador",
                description = "Oleadas agresivas. El objetivo no es solo colonizar: debes limpiar amenazas.",
                primaryObjective = "Coloniza 32%, descubre 25% del mapa y elimina la presión depredadora.",
                failureText = "la migración depredadora devoró la colonia",
                mapWidth = 108,
                mapHeight = 108,
                tileSize = 0.80f,
                startingResources = 96,
                neutralCells = 12,
                colonizationTarget = 0.32f,
                survivalSeconds = 32f * 60f,
                minimumDiscovered = 0.25f,
                requireEnemyClear = true,
                spawnEnemies = true
            });

            Add(new V5ScenarioDefinition
            {
                id = V5ScenarioId.Freeplay,
                displayName = "Freeplay / Sandbox",
                description = "Mapa grande para experimentar rutas, apex y world building sin derrota estricta.",
                primaryObjective = "Experimenta: coloniza 50%, invoca apex o sobrevive 45 minutos.",
                failureText = "la colonia colapsó",
                mapWidth = 128,
                mapHeight = 128,
                tileSize = 0.80f,
                startingResources = 130,
                neutralCells = 3,
                colonizationTarget = 0.50f,
                survivalSeconds = 45f * 60f,
                minimumDiscovered = 0.20f,
                requireApex = false,
                spawnEnemies = true
            });
        }

        private static void Add(V5ScenarioDefinition def)
        {
            defs[def.id] = def;
        }

        public static V5ScenarioDefinition Get(V5ScenarioId id)
        {
            V5ScenarioDefinition def;
            if (defs.TryGetValue(id, out def)) return def;
            return defs[V5ScenarioId.FirstDrop];
        }

        public static IEnumerable<V5ScenarioDefinition> All()
        {
            return defs.Values;
        }
    }
}
