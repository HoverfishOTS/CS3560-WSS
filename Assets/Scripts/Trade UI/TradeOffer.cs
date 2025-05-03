
[System.Serializable]
public class TradeOffer
{
    public int goldToPlayer;    // May not use
    public int foodToPlayer;
    public int waterToPlayer;
    public int goldToTrader;
    public int foodToTrader;    // May not use
    public int waterToTrader;   // May not use

    /// <summary>
    /// Basic trade for resources
    /// </summary>
    public TradeOffer(int goldToTrader, int foodToPlayer, int waterToPlayer)
    {
        this.goldToPlayer = 0;  // Unused
        this.foodToPlayer = foodToPlayer; 
        this.waterToPlayer = waterToPlayer; 
        this.goldToTrader = goldToTrader; 
        this.foodToTrader = 0;  // Unused
        this.waterToTrader = 0; // Unused
    }

    /// <summary>
    /// Gets the total amount of all resources the player receives 
    /// </summary>
    public int GetPlayerValue()
    {
        return goldToPlayer + foodToPlayer + waterToPlayer;
    }

    /// <summary>
    /// Gets the total amount of all resources the trader receives 
    /// </summary>
    public int GetTraderValue()
    {
        return goldToTrader + foodToTrader + waterToTrader;
    }

    public bool Equals(TradeOffer other)
    {
        return goldToPlayer == other.goldToPlayer && 
            foodToPlayer == other.foodToPlayer &&
            waterToPlayer == other.waterToPlayer &&
            goldToTrader == other.goldToTrader &&
            foodToTrader == other.foodToTrader &&
            waterToTrader == other.waterToTrader;
    }

    public override string ToString()
    {
        return $"(Player receives: {goldToPlayer}, {foodToPlayer}, {waterToPlayer}) (Trader receives: {goldToTrader}, {foodToTrader}, {waterToTrader})";
    }
}
