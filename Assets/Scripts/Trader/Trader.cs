using System;
using UnityEngine;

public class Trader
{
    public string traderType { get; protected set; }
    public int foodStock, waterStock, goldStock;

    public Trader() 
    {
        traderType = "Trader";
    }

    /// <summary>
    /// Trader wants equal value or higher.
    /// </summary>
    /// <returns>True, if trader accepts offer.</returns>
    protected virtual bool EvaluateTrade(Player player, TradeOffer offer)
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
        if (offer.GetPlayerValue() <= offer.GetTraderValue())
        {
            return false;
        }
        return false;
    }

    /// <summary>
    /// Main function called after trade is decided by brain
    /// </summary>
    public void MakeTrade(Player player, TradeOffer offer)
    {
        if (EvaluateTrade(player, offer))
        {
            ModifyStock(offer);

            // Give player stuff 
        }
        else
        {
            MakeCounter(offer);
        }
    }

    /// <summary>
    /// Implement based on trader type. Sends offer to the player brain 
    /// </summary>
    protected virtual void MakeCounter(TradeOffer offer)
    {
        int newFoodOffer = Mathf.Min(foodStock, offer.foodToPlayer);
        int newWaterOffer = Mathf.Min(waterStock, offer.waterToPlayer);
        int newGoldCost = newFoodOffer + newWaterOffer;
        TradeOffer counter = new TradeOffer(newGoldCost, newFoodOffer, newWaterOffer);
        // Send offer to brain
    }

    private void ModifyStock(TradeOffer offer)
    {
        foodStock -= offer.foodToPlayer;
        waterStock -= offer.waterToPlayer;
        goldStock -= offer.goldToPlayer;
    }
}
