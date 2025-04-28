using System;
using UnityEngine;

public class Player
{
    public int food { get; private set; }
    public int water { get; private set; }
    public int energy { get; private set; }
    public int gold { get; private set; }

    private int maxFood, maxWater, maxEnergy;

    public VisionType visionType { get; private set; }
    private Vision vision;

    public MapPosition mapPosition { get; private set; }

    public void InitializePlayer(int maxFood, int maxWater, int maxEnergy, VisionType visionType)
    {
        this.maxFood = maxFood;
        this.maxWater = maxWater;
        this.maxEnergy = maxEnergy;

        this.food = maxFood;
        this.water = maxWater;
        this.energy = maxEnergy;

        this.visionType = visionType;
    }

    public void SetMapPosition(int x, int y)
    {
        mapPosition = new MapPosition(x, y);
        PlayerRenderer.Instance?.UpdatePosition(x, y);
    }

    public void SetMapPosition(MapPosition position)
    {
        mapPosition = position;
    }

    private void InitializeTrade(Trader trader, string input, int inputCount, string output, int outputCount)
    {

    }

    private void ApplyCost(string type, int cost)
    {

    }

    private void ApplyBonus(string type, int bonus)
    {

    }
}
