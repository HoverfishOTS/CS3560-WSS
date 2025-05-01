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
    protected override bool EvaluateTrade(Player player, string input, int inputCount, string output, int outputCount)
    {
        if (GetStock(input) >= inputCount)
        {
            return false;
        }
        if (inputCount >= outputCount * ProfitMargin)
        {
            return true;
        }
        return false;
    }

    // Issue: Can counter offer same item for same item type
    protected override void MakeCounter(string input, int inputCount, string output, int outputCount)
    {
        int stockedInput = GetStock(input);
        string decision = string.Empty;
        if (stockedInput < inputCount && stockedInput > 0)
        {
            // No stock for current offer:
            // ==>> Offers same trade but with amount of items in stock
            // decision = await Brain.GetTradeDecisionAsync(output, stockedInput, input, int (stockedInput * ProfitMargin), traderType);
        }
        else if (foodStock > 0)
        {
            // Offers food for same input
            // decision = await Brain.GetTradeDecisionAsync("food", GetStock("food"), input, int (GetStock("food") * ProfitMargin), traderType);
        }
        else if (waterStock > 0)
        {
            // Offers water for same input
            // decision = await Brain.GetTradeDecisionAsync("water", GetStock("water"), input, int (GetStock("water") * ProfitMargin), traderType);
        }
        else if (goldStock > 0)
        {
            // Offers gold for same input
            // decision = await Brain.GetTradeDecisionAsync("gold", GetStock("gold"), input, int (GetStock("gold") * ProfitMargin), traderType);
        }

        // Read decision:
        //  Cancel trade ==>> pass back to main game
        //  Brain counter offer ==>> MakeTrade(decision.input, decision.inputCount ... );
    }
}
