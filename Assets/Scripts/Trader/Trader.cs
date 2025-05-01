using System;
using UnityEngine;

public abstract class Trader
{
    public string traderType { get; protected set; }
    protected int foodStock, waterStock, goldStock;

    /// <summary>
    /// Evaluation decision is made in derived classes.
    /// </summary>
    /// <returns>True, if trader accepts offer.</returns>
    protected abstract bool EvaluateTrade(Player player, string input, int inputCount, string output, int outputCount);

    /// <summary>
    /// Main function called after trade is decided by player brain
    /// </summary>
    public void MakeTrade(Player player, string input, int inputCount, string output, int outputCount)
    {
        if (EvaluateTrade(player, input, inputCount, output, outputCount))
        {
            ModifyStock(input, -inputCount);
            ModifyStock(output, outputCount);

            // Give player stuff 
        }
        else
        {
            MakeCounter(input, inputCount, output, outputCount);
        }
    }

    /// <summary>
    /// Implement based on trader type. Sends offer to the player brain 
    /// </summary>
    /// <param name="input">What the trader receives</param>
    /// <param name="output">What the trader loses</param>
    protected abstract void MakeCounter(string input, int inputCount, string output, int outputCount);

    private void ModifyStock(string item, int count)
    {
        switch (item.ToLower())
        {
            case "food":
                foodStock += count;
                break;
            case "water":
                waterStock += count;
                break;
            case "gold":
                goldStock += count;
                break;
            default:
                Debug.LogError("Unknown item: " + item);
                break;
        }
    }

    protected int GetStock(string item)
    {
        switch (item.ToLower())
        {
            case "food":
                return foodStock;
            case "water":
                return waterStock;
            case "gold":
                return goldStock;
            default:
                Debug.LogError("Unknown item: " + item);
                break;
        }
        return 0;
    }
}
