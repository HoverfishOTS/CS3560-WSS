using UnityEngine;
using System;

public class CurrentGameState
{
    private Vision vision;
    private int currentFood;
    private int currentWater;
    private int currentEnergy;
    private int currentGold;
    private MapPosition currentMapPosition;

    public CurrentGameState(Vision vision, Player player)
    {
        this.vision = vision;
        this.currentFood = player.food;
        this.currentWater = player.water;
        this.currentEnergy = player.energy;
        this.currentGold = player.gold;
        this.currentMapPosition = player.mapPosition;
    }

    public string ToJson()
    {
        return JsonUtility.ToJson(this);
    }

}

[System.Serializable]
public struct MapPosition
{
    public int x;
    public int y;

    public MapPosition(int x, int y)
    {
        this.x = x;
        this.y = y;
    }

    public bool Equals(MapPosition other)
    {
        return x == other.x && y == other.y;
    }

    public override string ToString()
    {
        return "(" + x + ", " + y + ")";
    }
}
