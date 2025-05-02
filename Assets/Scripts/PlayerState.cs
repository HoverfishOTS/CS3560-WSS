using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class PlayerState
{
    public int food;
    public int water;
    public int energy;
    public int gold;

    public int mapWidth;
    public int mapHeight;
    public MapPosition currentPosition;

    public TileData[][] visibleTerrain; // 5x3 matrix of tiles

    public PlayerState(Player player, Map map, Vision vision)
    {
        this.food = player.food;
        this.water = player.water;
        this.energy = player.energy;
        this.gold = player.gold;

        this.mapWidth = map.width;
        this.mapHeight = map.height;
        this.currentPosition = player.mapPosition;

        MapTerrain[][] field = vision.GetField();
        visibleTerrain = new TileData[5][];
        for (int y = 0; y < 5; y++)
        {
            visibleTerrain[y] = new TileData[3];
            for (int x = 0; x < 3; x++)
            {
                MapTerrain terrain = field[y][x];
                if (terrain != null)
                {
                    List<string> items = new List<string>();
                    if (terrain.hasFoodBonus) items.Add("Food Bonus");
                    if (terrain.hasWaterBonus) items.Add("Water Bonus");
                    if (terrain.hasGoldBonus) items.Add("Gold Bonus");
                    if (terrain.hasTrader) items.Add("Trader");

                    visibleTerrain[y][x] = new TileData
                    {
                        terrain = terrain.biome.ToString(),
                        move_cost = terrain.movementCost,
                        food_cost = terrain.foodCost,
                        water_cost = terrain.waterCost,
                        items = items.ToArray(),

                        food_bonus = terrain.foodBonus,
                        food_repeating = terrain.foodBonusRepeating,

                        water_bonus = terrain.waterBonus,
                        water_repeating = terrain.waterBonusRepeating,

                        gold_bonus = terrain.goldBonus,
                        has_trader = terrain.hasTrader
                    };
                }
                else
                {
                    visibleTerrain[y][x] = null;
                }
            }
        }
    }
}

[System.Serializable]
public class TileData
{
    public string terrain;
    public int move_cost;
    public int food_cost;
    public int water_cost;
    public string[] items;

    // Bonus values
    public int food_bonus;
    public bool food_repeating;

    public int water_bonus;
    public bool water_repeating;

    public int gold_bonus;
    public bool has_trader;
}
