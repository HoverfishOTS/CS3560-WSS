using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Difficulty Settings")]
    [SerializeField] private DifficultySettings easySettings;
    [SerializeField] private DifficultySettings mediumSettings;
    [SerializeField] private DifficultySettings hardSettings;

    [Header("Noise Settings")]
    [SerializeField] private float biomeNoiseScale = 0.1f;
    [SerializeField] private float resourceNoiseScale = 0.2f;

    public Map GenerateMap()
    {
        MapConfig config = GameConfig.instance.mapConfig;
        int width = config.width;
        int height = config.height;
        Difficulty difficulty = config.difficulty;

        DifficultySettings settings = GetSettingsForDifficulty(difficulty);

        MapTerrain[][] mapMatrix = new MapTerrain[height][];

        float biomeOffsetX = Random.Range(0f, 1000f);
        float biomeOffsetY = Random.Range(0f, 1000f);
        float resourceOffsetX = Random.Range(0f, 1000f);
        float resourceOffsetY = Random.Range(0f, 1000f);

        for (int y = 0; y < height; y++)
        {
            mapMatrix[y] = new MapTerrain[width];
            for (int x = 0; x < width; x++)
            {
                float biomeNoise = Mathf.PerlinNoise((x + biomeOffsetX) * biomeNoiseScale, (y + biomeOffsetY) * biomeNoiseScale);
                Biome biome = GetBiomeFromNoise(biomeNoise, settings.biomeSettings);
                (int movementCost, int waterCost, int foodCost) = GetTerrainCosts(biome);

                ResourceSpawnSettings resourceSettings = GetResourceSettingsForBiome(settings, biome);

                Trader trader = Roll(resourceSettings.traderChance) ? new Trader() : null;

                float resourceNoise = Mathf.PerlinNoise((x + resourceOffsetX) * resourceNoiseScale, (y + resourceOffsetY) * resourceNoiseScale);

                bool hasFoodBonus = resourceNoise < resourceSettings.foodChance;
                bool foodBonusRepeating = hasFoodBonus && resourceNoise < resourceSettings.repeatingFoodChance;
                bool hasWaterBonus = resourceNoise < resourceSettings.waterChance;
                bool waterBonusRepeating = hasWaterBonus && resourceNoise < resourceSettings.repeatingWaterChance;
                bool hasGoldBonus = resourceNoise < resourceSettings.goldChance;

                MapTerrain tile = new MapTerrain(
                    movementCost, waterCost, foodCost, biome,
                    trader,
                    hasFoodBonus, foodBonusRepeating,
                    hasWaterBonus, waterBonusRepeating,
                    hasGoldBonus
                );

                mapMatrix[y][x] = tile;
            }
        }

        Map map = new Map(width, height, (int)difficulty, mapMatrix);
        DebugDrawMap(map);
        return map;
    }

    private void DebugDrawMap(Map map)
    {
        for (int y = 0; y < map.height; y++)
        {
            string row = "";
            for (int x = 0; x < map.width; x++)
            {
                MapTerrain tile = map.GetTile(x, y);
                switch (tile.biome)
                {
                    case Biome.Plains: row += "P"; break;
                    case Biome.Desert: row += "D"; break;
                    case Biome.Mountains: row += "M"; break;
                    case Biome.Forest: row += "F"; break;
                    case Biome.Jungle: row += "J"; break;
                    case Biome.Swamp: row += "S"; break;
                }
            }
            Debug.Log(row);
        }
    }

    private DifficultySettings GetSettingsForDifficulty(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Easy => easySettings,
            Difficulty.Medium => mediumSettings,
            Difficulty.Hard => hardSettings,
            _ => easySettings
        };
    }

    private Biome GetBiomeFromNoise(float noiseValue, BiomeSettings biomeSettings)
    {
        float total = biomeSettings.plainsChance + biomeSettings.desertChance + biomeSettings.mountainsChance + biomeSettings.forestChance + biomeSettings.jungleChance + biomeSettings.swampChance;
        float cumulative = 0;

        noiseValue *= total;

        cumulative += biomeSettings.plainsChance;
        if (noiseValue < cumulative) return Biome.Plains;
        cumulative += biomeSettings.desertChance;
        if (noiseValue < cumulative) return Biome.Desert;
        cumulative += biomeSettings.mountainsChance;
        if (noiseValue < cumulative) return Biome.Mountains;
        cumulative += biomeSettings.forestChance;
        if (noiseValue < cumulative) return Biome.Forest;
        cumulative += biomeSettings.jungleChance;
        if (noiseValue < cumulative) return Biome.Jungle;

        return Biome.Swamp;
    }

    private (int movementCost, int waterCost, int foodCost) GetTerrainCosts(Biome biome)
    {
        return biome switch
        {
            Biome.Plains => (1, 1, 1),
            Biome.Desert => (2, 3, 2),
            Biome.Mountains => (4, 2, 3),
            Biome.Forest => (2, 2, 2),
            Biome.Jungle => (3, 3, 2),
            Biome.Swamp => (3, 2, 3),
            _ => (1, 1, 1)
        };
    }

    private ResourceSpawnSettings GetResourceSettingsForBiome(DifficultySettings settings, Biome biome)
    {
        return biome switch
        {
            Biome.Plains => settings.plainsResourceSettings,
            Biome.Desert => settings.desertResourceSettings,
            Biome.Mountains => settings.mountainResourceSettings,
            Biome.Forest => settings.forestResourceSettings,
            Biome.Jungle => settings.jungleResourceSettings,
            Biome.Swamp => settings.swampResourceSettings,
            _ => settings.plainsResourceSettings
        };
    }

    private bool Roll(float chance)
    {
        return Random.Range(0f, 1f) <= chance;
    }
}

[System.Serializable]
public class DifficultySettings
{
    public BiomeSettings biomeSettings;
    public ResourceSpawnSettings plainsResourceSettings;
    public ResourceSpawnSettings desertResourceSettings;
    public ResourceSpawnSettings mountainResourceSettings;
    public ResourceSpawnSettings forestResourceSettings;
    public ResourceSpawnSettings jungleResourceSettings;
    public ResourceSpawnSettings swampResourceSettings;
}

[System.Serializable]
public class BiomeSettings
{
    public float plainsChance;
    public float desertChance;
    public float mountainsChance;
    public float forestChance;
    public float jungleChance;
    public float swampChance;
}

[System.Serializable]
public class ResourceSpawnSettings
{
    public float traderChance;
    public float foodChance;
    public float waterChance;
    public float goldChance;
    public float repeatingFoodChance;
    public float repeatingWaterChance;
}
