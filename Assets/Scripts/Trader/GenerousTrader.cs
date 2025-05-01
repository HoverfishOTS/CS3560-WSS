using UnityEngine;

public class GenerousTrader : Trader
{
    public GenerousTrader()
    {
        traderType = "GenerousTrader";
    }

    /// <summary>
    /// Generous trader just wants to make trades (Receives equal to or more items than offered).
    /// </summary>
    /// <returns>True, if trader accepts offer.</returns>
    protected override bool EvaluateTrade(Player player, string input, int inputCount, string output, int outputCount)
    {
        if (inputCount >= outputCount)
        {
            return true;
        }
        return false;
    }

    protected override void MakeCounter(string input, int inputCount, string output, int outputCount)
    {
        int stockedInput = GetStock(input);
        if (stockedInput < inputCount && stockedInput > 0)
        {
            // No stock for current offer:
            // ==>> Offers same trade but with amount of items in stock
            // await Brain.GetTradeDecisionAsync(output, stockedInput, input, int stockedInput, traderType);
        }
        else if (foodStock > 0)
        {
            // Offers food for same input (1:1)
            // await Brain.GetTradeDecisionAsync("food", GetStock("food"), input, GetStock("food"), traderType);
        }
        else if (waterStock > 0)
        {
            // Offers water for same input (1:1)
            // await Brain.GetTradeDecisionAsync("water", GetStock("water"), input, GetStock("water"_, traderType);
        }
        else if (goldStock > 0)
        {
            // Offers gold for same input (1:1)
            // await Brain.GetTradeDecisionAsync("gold", GetStock("gold"), input, (GetStock("gold"), traderType);
        }

        // Read decision:
        //  Cancel trade ==>> pass back to main game
        //  Brain counter offer ==>> MakeTrade(decision.input, decision.inputCount ... );
    }
}
