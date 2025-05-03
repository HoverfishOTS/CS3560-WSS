using UnityEngine;

[System.Serializable]
public class MapTerrain
{
    public MapPosition coordinate;
    public int movementCost { get; private set; }
    public int waterCost { get; private set; }
    public int foodCost { get; private set; }

    public bool hasTrader { get; private set; }
    public bool hasFoodBonus { get; private set; }
    public bool hasWaterBonus { get; private set; }
    public bool hasGoldBonus { get; private set; }

    public Trader trader { get; private set; }

    public int waterBonus { get; private set; }
    public int foodBonus { get; private set; }
    public int goldBonus { get; private set; }

    public bool waterBonusRepeating { get; private set; }
    public bool foodBonusRepeating { get; private set; }

    public Biome biome { get; private set; }

    [System.NonSerialized]
    public TileDisplay tile;

    public MapTerrain(
        MapPosition coordinate,
        int movementCost, int waterCost, int foodCost, Biome biome,
        Trader trader,
        bool hasFoodBonus, bool foodBonusRepeating, int foodBonusAmount,
        bool hasWaterBonus, bool waterBonusRepeating, int waterBonusAmount,
        bool hasGoldBonus, int goldBonusAmount
    )
    {
        this.coordinate = coordinate;
        this.movementCost = movementCost;
        this.waterCost = waterCost;
        this.foodCost = foodCost;
        this.biome = biome;

        this.trader = trader;
        this.hasTrader = trader != null;

        this.hasFoodBonus = hasFoodBonus;
        this.foodBonus = foodBonusAmount;
        this.foodBonusRepeating = foodBonusRepeating;

        this.hasWaterBonus = hasWaterBonus;
        this.waterBonus = waterBonusAmount;
        this.waterBonusRepeating = waterBonusRepeating;

        this.hasGoldBonus = hasGoldBonus;
        this.goldBonus = goldBonusAmount;
    }

    public void SetTileDisplay(TileDisplay tileDisplay)
    {
        tile = tileDisplay;
    }

    public void TakeBonus(string type)
    {
        switch (type.ToLower())
        {
            case "food":
                if (!foodBonusRepeating)
                {
                    foodBonus = 0; 
                    hasFoodBonus = false;
                    tile.HideFoodBonus();
                }
                break;
            case "water":
                if (!waterBonusRepeating)
                {
                    waterBonus = 0; 
                    hasWaterBonus = false;
                    tile.HideWaterBonus();
                }
                break;
            case "gold":
                goldBonus = 0; hasGoldBonus = false; break;
        }
    }

    public void ClearTrader()
    {
        hasTrader = false;
        trader = null;
        tile.HideTrader();
    }
}

public enum Biome
{
    Plains,
    Desert,
    Mountains,
    Forest,
    Jungle,
    Swamp,
}
