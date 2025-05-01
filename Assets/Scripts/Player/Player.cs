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

    public void Move(string direction)
    {
        Debug.Log($"[Player] Moving {direction} (placeholder)");
        // TODO: Deduct costs, update map position here
    }

    public void Rest()
    {
        Debug.Log("[Player] Resting this turn (placeholder)");
        // TODO: Increase energy, consume small food/water
    }

    public void AttemptTrade(MapTerrain tile)
    {
        Debug.Log("[Player] Attempting to trade (placeholder)");
        // TODO: Check for trader on tile and perform trade
    }


    /// <summary>
    /// Officially starts trade interaction.
    /// </summary>
    private void InitializeTrade(Trader trader, string input, int inputCount, string output, int outputCount)
    {
        trader.MakeTrade(this, input, inputCount, output, outputCount);
    }

    private void ApplyCost(string type, int cost)
    {

    }

    private void ApplyBonus(string type, int bonus)
    {

    }
}
