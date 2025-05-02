using UnityEngine;
using UnityEngine.EventSystems; // Keep this if you might add hover effects later

// No funny business here, just a simple button handler.
public class TradeButton : MonoBehaviour //, IPointerEnterHandler, IPointerExitHandler // Interfaces commented out for now
{
    // We might need a reference to the current terrain later if we add hover effects,
    // but for just triggering the trade decision, it's not needed yet.
    // public MapTerrain terrain;

    /// <summary>
    /// Called when the Trade button is clicked in the UI.
    /// Tells the UserInputManager to register a Trade decision.
    /// </summary>
    public void OnTradeButtonClicked()
    {
        // Check if the UserInputManager exists, just in case. Belt and suspenders.
        if (UserInputManager.Instance != null)
        {
            Debug.Log("[TradeButton] Trade button clicked, sending Trade decision.");
            // Create a new Decision object with the Trade type
            UserInputManager.Instance.MakeDecision(new Decision
            {
                decisionType = DecisionType.Trade
                // No direction needed for Trade
            });
        }
        else
        {
            Debug.LogError("[TradeButton] UserInputManager instance not found!");
        }
    }

    // --- Hover Effect Logic (Optional - Currently Commented Out) ---
    // If you want to show potential trade outcomes on hover later,
    // you'd uncomment these methods and the interfaces above,
    // similar to RestButton.cs. You'd need a reference to the
    // current MapTerrain and potentially the Player object.

    /*
    public void OnPointerEnter(PointerEventData eventData)
    {
        // Example: Show potential trade outcome if trader is present
        // if (terrain != null && terrain.hasTrader && StatEffectManager.Instance != null)
        // {
        //     // You'd need logic in StatEffectManager to display "+1 Food, +1 Water, -2 Gold"
        //     // StatEffectManager.Instance.ReflectTradeOffer(terrain);
        // }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Clear the trade offer preview
        // if (StatEffectManager.Instance != null)
        // {
        //     StatEffectManager.Instance.ClearEffect("all"); // Or a specific "trade" effect type
        // }
    }
    */
}
