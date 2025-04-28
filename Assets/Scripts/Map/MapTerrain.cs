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

    public MapTerrain(
        int movementCost, int waterCost, int foodCost, Biome biome,
        Trader trader,
        bool hasFoodBonus, bool foodBonusRepeating, int foodBonusAmount,
        bool hasWaterBonus, bool waterBonusRepeating, int waterBonusAmount,
        bool hasGoldBonus, int goldBonusAmount
    )
    {
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
