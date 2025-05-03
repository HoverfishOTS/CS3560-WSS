using UnityEngine;

public class TradeManager : MonoBehaviour
{
    public static TradeManager Instance;

    private TradeState currentState = TradeState.None;
    private Trader currentTrader;
    private TradeOffer currentOffer;

    public GameObject tradeWindow;
    public TradeOfferDisplay createOfferDisplay;
    public TradeOfferDisplay counterOfferDisplay;
    public HaggleDisplay haggleDisplay;
    public TradeOfferDisplay tradeResultDisplay;

    private Player player;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayerReference();

        tradeWindow.SetActive(false);
    }

    public void StartTrade(Trader trader)
    {
        currentTrader = trader;
        currentState = TradeState.OfferMade;
        currentOffer = new TradeOffer(0, 0, 0); // Default blank offer
        
        createOfferDisplay.Initialize(currentOffer, trader);
        createOfferDisplay.gameObject.SetActive(true);

        counterOfferDisplay.gameObject.SetActive(false);
        haggleDisplay.gameObject.SetActive(false);
        tradeResultDisplay.gameObject.SetActive(false);

        tradeWindow.SetActive(true);
    }

    public void SubmitOffer()
    {
        if (currentOffer.GetPlayerValue() == 0 || currentOffer.GetTraderValue() == 0) return;

        bool accepted = currentTrader.EvaluateTrade(currentOffer);

        if (accepted)
        {
            EnterHaggling();
        }
        else
        {
            TradeOffer newOffer = currentTrader.CreateCounterOffer(currentOffer);
            if (!newOffer.Equals(currentOffer))
            {
                currentOffer = newOffer;
                currentState = TradeState.CounterReceived;
                counterOfferDisplay.Initialize(currentOffer, currentTrader);

                createOfferDisplay.gameObject.SetActive(false);
                counterOfferDisplay.gameObject.SetActive(true);
            }
            else
            {
                EnterHaggling();
            }
            
        }
    }

    public void AcceptCounterOffer()
    {
        if(currentOffer.goldToTrader <= player.gold)
        {
            EnterHaggling();
        }
    }

    public void RejectTrade()
    {
        currentState = TradeState.Rejected;
        CloseTradeWindow();
    }

    public void MakeCounterOffer()
    {
        currentState = TradeState.OfferMade;
        counterOfferDisplay.gameObject.SetActive(false);

        createOfferDisplay.Initialize(currentOffer, currentTrader);
        createOfferDisplay.gameObject.SetActive(true);
    }

    private async void EnterHaggling()
    {
        currentState = TradeState.Haggling;
        createOfferDisplay.gameObject.SetActive(false);
        counterOfferDisplay.gameObject.SetActive(false);

        haggleDisplay.gameObject.SetActive(true);

        int result = await haggleDisplay.StartHaggling(currentTrader, currentOffer);

        // Apply reward logic based on result
        ApplyHaggleResult(result);
    }

    public void EndTrade()
    {
        haggleDisplay.gameObject.SetActive(false);

        currentTrader.ModifyStock(currentOffer);
        player.ApplyTrade(currentOffer);

        tradeResultDisplay.Initialize(currentOffer, currentTrader);
        tradeResultDisplay.gameObject.SetActive(true);

        StatEffectManager.Instance.ClearEffect("all");

        GameManager.Instance.UpdateDisplay();

        currentState = TradeState.Completed;
    }

    public void CloseTradeWindow()
    {
        currentState = TradeState.None;
        currentTrader = null;
        currentOffer = new TradeOffer(0, 0, 0);

        tradeResultDisplay.gameObject.SetActive(false);
        tradeWindow.gameObject.SetActive(false);

        StatEffectManager.Instance.ClearEffect("all");
    }

    private void ApplyHaggleResult(int result)
    {
        currentOffer.goldToTrader -= result;

        StatEffectManager.Instance.ReflectTradeOffer(currentOffer);
    }


}

public enum TradeState
{
    None,
    OfferMade,
    CounterReceived,
    Haggling,
    Completed,
    Rejected
}

