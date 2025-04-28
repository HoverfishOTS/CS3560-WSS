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

                // costs
                TerrainCostSettings costSettings = GetCostSettingsForBiome(settings, biome);

                int movementCost = Random.Range(costSettings.movementCostRange.x, costSettings.movementCostRange.y + 1);
                int waterCost = Random.Range(costSettings.waterCostRange.x, costSettings.waterCostRange.y + 1);
                int foodCost = Random.Range(costSettings.foodCostRange.x, costSettings.foodCostRange.y + 1);


                // bonuses
                ResourceSpawnSettings resourceSettings = GetResourceSettingsForBiome(settings, biome);

                Trader trader = Roll(resourceSettings.traderChance) ? new Trader() : null;

                float resourceNoise = Mathf.PerlinNoise((x + resourceOffsetX) * resourceNoiseScale, (y + resourceOffsetY) * resourceNoiseScale);

                bool hasFoodBonus = resourceNoise < resourceSettings.foodChance;
                bool foodBonusRepeating = hasFoodBonus && resourceNoise < resourceSettings.repeatingFoodChance;
                bool hasWaterBonus = resourceNoise < resourceSettings.waterChance;
                bool waterBonusRepeating = hasWaterBonus && resourceNoise < resourceSettings.repeatingWaterChance;
                bool hasGoldBonus = resourceNoise < resourceSettings.goldChance;

                int foodBonusAmount = 0;
                int waterBonusAmount = 0;
                int goldBonusAmount = 0;

                if (hasFoodBonus)
                    foodBonusAmount = Random.Range(resourceSettings.foodBonusRange.x, resourceSettings.foodBonusRange.y + 1);

                if (hasWaterBonus)
                    waterBonusAmount = Random.Range(resourceSettings.waterBonusRange.x, resourceSettings.waterBonusRange.y + 1);

                if (hasGoldBonus)
                    goldBonusAmount = Random.Range(resourceSettings.goldBonusRange.x, resourceSettings.goldBonusRange.y + 1);


                MapTerrain tile = new MapTerrain(
                    movementCost, waterCost, foodCost, biome,
                    trader,
                    hasFoodBonus, foodBonusRepeating, foodBonusAmount,
                    hasWaterBonus, waterBonusRepeating, waterBonusAmount,
                    hasGoldBonus, goldBonusAmount
                );

                mapMatrix[y][x] = tile;
            }
        }

        Map map = new Map(width, height, (int)difficulty, mapMatrix);
        //DebugDrawMap(map);
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

    private TerrainCostSettings GetCostSettingsForBiome(DifficultySettings settings, Biome biome)
    {
        return biome switch
        {
            Biome.Plains => settings.plainsCostSettings,
            Biome.Desert => settings.desertCostSettings,
            Biome.Mountains => settings.mountainCostSettings,
            Biome.Forest => settings.forestCostSettings,
            Biome.Jungle => settings.jungleCostSettings,
            Biome.Swamp => settings.swampCostSettings,
            _ => settings.plainsCostSettings
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

    public TerrainCostSettings plainsCostSettings;
    public TerrainCostSettings desertCostSettings;
    public TerrainCostSettings mountainCostSettings;
    public TerrainCostSettings forestCostSettings;
    public TerrainCostSettings jungleCostSettings;
    public TerrainCostSettings swampCostSettings;
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

    public Vector2Int foodBonusRange = new Vector2Int(2, 5);
    public Vector2Int waterBonusRange = new Vector2Int(2, 5);
    public Vector2Int goldBonusRange = new Vector2Int(1, 3);
}

[System.Serializable]
public class TerrainCostSettings
{
    public Vector2Int movementCostRange;
    public Vector2Int waterCostRange;
    public Vector2Int foodCostRange;
}
