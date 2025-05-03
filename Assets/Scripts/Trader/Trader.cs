using System;
using UnityEngine;

public class Trader
{
    public string traderType { get; protected set; }
    private float profitMargin;
    public int foodStock, waterStock, goldStock;

    private Tuple<int, int> easyResourceRange = new Tuple<int, int>(0, 5);
    private Tuple<int, int> mediumResourceRange = new Tuple<int, int>(1, 7);
    private Tuple<int, int> hardResourceRange = new Tuple<int, int>(2, 10);

    public Trader(string type) 
    {
        traderType = type.ToLower();
        switch(traderType)
        {
            case "generous":
                profitMargin = 0.75f;
                break;
            case "stingy":
                profitMargin = 1.5f;
                break;
            default:
                profitMargin = 1.0f;
                break;
        }

        Tuple<int, int> resourceRange;
        switch (GameConfig.instance.mapConfig.difficulty)
        {
            case Difficulty.Easy:
                resourceRange = easyResourceRange; break;
            case Difficulty.Medium:
                resourceRange = mediumResourceRange; break;
            case Difficulty.Hard:
                resourceRange = hardResourceRange; break;
            default:
                resourceRange = mediumResourceRange; break;
        }
        foodStock = UnityEngine.Random.Range(resourceRange.Item1, resourceRange.Item2);
        waterStock = UnityEngine.Random.Range(resourceRange.Item1, resourceRange.Item2);
        goldStock = UnityEngine.Random.Range(resourceRange.Item1, resourceRange.Item2);

    }

    /// <summary>
    /// Trader wants equal value or higher.
    /// </summary>
    /// <returns>True, if trader accepts offer.</returns>
    public bool EvaluateTrade(TradeOffer offer)
    {
        if (offer.foodToTrader == 0 && offer.waterToTrader == 0 && offer.goldToTrader == 0)
        {
            return false;
        }
        // No stock for offer
        if (offer.foodToPlayer > foodStock || offer.waterToPlayer > waterStock || offer.goldToPlayer > goldStock)
        {
            return false;
        }
        if (offer.GetPlayerValue() * profitMargin <= offer.GetTraderValue())
        {
            return false;
        }
        return false;
    }

    /// <summary>
    /// Implement based on trader type. Sends offer to the player brain 
    /// </summary>
    public TradeOffer CreateCounterOffer(TradeOffer offer)
    {
        int newFoodOffer = Mathf.Min(foodStock, offer.foodToPlayer);
        int newWaterOffer = Mathf.Min(waterStock, offer.waterToPlayer);
        int newGoldCost = Mathf.Max(0, (int)(newFoodOffer + newWaterOffer * profitMargin));
        return new TradeOffer(newGoldCost, newFoodOffer, newWaterOffer);
    }

    public void ModifyStock(TradeOffer offer)
    {
        foodStock -= offer.foodToPlayer;
        waterStock -= offer.waterToPlayer;
        goldStock -= offer.goldToPlayer;
    }
}
