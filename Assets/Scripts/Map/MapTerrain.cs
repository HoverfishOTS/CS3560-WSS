using UnityEngine;

public class MapTerrain
{
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

    public MapTerrain(int movementCost, int waterCost, int foodCost, Biome biome,
        Trader trader = null,
        bool hasFoodBonus = false, bool foodBonusRepeating = false,
        bool hasWaterBonus = false, bool waterBonusRepeating = false,
        bool hasGoldBonus = false)
    {
        this.movementCost = movementCost;
        this.waterCost = waterCost;
        this.foodCost = foodCost;
        this.biome = biome;

        this.trader = trader;
        this.hasTrader = trader != null;

        this.hasFoodBonus = hasFoodBonus;
        this.foodBonusRepeating = foodBonusRepeating;

        this.hasWaterBonus = hasWaterBonus;
        this.waterBonusRepeating = waterBonusRepeating;

        this.hasGoldBonus = hasGoldBonus;
    }


    private void refreshWater()
    {

    }

    private void refreshFood()
    {

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
