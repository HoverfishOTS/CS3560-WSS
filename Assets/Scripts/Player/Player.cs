using System;
using UnityEngine;

public class Player
{
    private int food, water, energy, gold;
    private int maxFood, maxWater, maxEnergy;

    private Vision vision;

    private Tuple<int, int> mapPosition;

    public void InitializePlayer(int maxFood, int maxWater, int maxEnergy)
    {
        this.maxFood = maxFood;
        this.maxWater = maxWater;
        this.maxEnergy = maxEnergy;

        this.food = maxFood;
        this.water = maxWater;
        this.energy = maxEnergy;
    }

    public void SetMapPosition(int x, int y)
    {
        mapPosition = new Tuple<int, int>(x, y);

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
