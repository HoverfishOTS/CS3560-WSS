using UnityEngine;

public class StingyTrader : Trader
{
    private const float ProfitMargin = 1.5f;

    public StingyTrader()
    {
        traderType = "StingyTrader";
    }

    /// <summary>
    /// Stingy trader wants high value trades (Receives a percentage more than offered).
    /// </summary>
    /// <returns>True, if trader accepts offer.</returns>
    protected override bool EvaluateTrade(Player player, TradeOffer offer)
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
        if (offer.GetPlayerValue() * ProfitMargin <= offer.GetTraderValue())
        {
            return false;
        }
        return true;
    }

    protected override void MakeCounter(TradeOffer offer)
    {
        int newFoodOffer = Mathf.Min(foodStock, offer.foodToPlayer);
        int newWaterOffer = Mathf.Min(waterStock, offer.waterToPlayer);
        int newGoldCost = (int) (newFoodOffer + newWaterOffer * ProfitMargin);
        TradeOffer counter = new TradeOffer(newGoldCost, newFoodOffer, newWaterOffer);
        // Send offer to brain
    }
}
