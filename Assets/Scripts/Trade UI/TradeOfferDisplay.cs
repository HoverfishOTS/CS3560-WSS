using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TradeOfferDisplay : MonoBehaviour
{
    [Header("Player")]
    public TextMeshProUGUI goldAmount;

    [Header("Trader")]
    public TextMeshProUGUI foodAmount;
    public TextMeshProUGUI waterAmount;
    public TextMeshProUGUI foodStock;
    public TextMeshProUGUI waterStock;

    [Header("Sprites")]
    public Image playerImage;
    public Image traderImage;
    public Sprite wess, mess, gess, less, yess;

    private TradeOffer tradeOffer;
    private Player player;
    private Trader trader;

    public void Initialize(TradeOffer tradeOffer, Trader trader)
    {
        this.tradeOffer = tradeOffer;
        player = GameManager.Instance.GetPlayerReference();
        this.trader = trader;

        foodStock.text = trader.foodStock.ToString();
        waterStock.text = trader.waterStock.ToString();

        UpdateSprites();

        UpdateDisplays();
    }

    private void UpdateSprites()
    {
        playerImage.sprite = GameConfig.instance.playerConfig.brainType == BrainType.Player ? wess : mess;

        Sprite newTraderSprite;
        switch (trader.traderType)
        {
            case "stingy":
                newTraderSprite = less;
                break;
            case "generous":
                newTraderSprite = yess;
                break;
            default:
                newTraderSprite = gess;
                break;
        }
        traderImage.sprite = newTraderSprite;
    }

    private void UpdateDisplays()
    {
        UpdateTradeDisplay();
        UpdateResourceEffects();
    }

    private void UpdateTradeDisplay()
    {
        goldAmount.text = tradeOffer.goldToTrader.ToString();
        foodAmount.text = tradeOffer.foodToPlayer.ToString();
        waterAmount.text = tradeOffer.waterToPlayer.ToString();
    }

    private void UpdateResourceEffects()
    {
        StatEffectManager.Instance.ReflectTradeOffer(tradeOffer);
    }

    public void IncrementValue(string type)
    {
        switch (type)
        {
            case "gold":
                if (tradeOffer.goldToTrader < player.gold) tradeOffer.goldToTrader++;
                break;
            case "food":
                if (tradeOffer.foodToPlayer < trader.foodStock && player.food + tradeOffer.foodToPlayer < player.maxFood) tradeOffer.foodToPlayer++;
                break;
            case "water":
                if (tradeOffer.waterToPlayer < trader.waterStock && player.water + tradeOffer.waterToPlayer < player.maxWater) tradeOffer.waterToPlayer++;
                break;
        }
        UpdateDisplays();
    }

    public void DecrementValue(string type)
    {
        switch (type)
        {
            case "gold":
                if (tradeOffer.goldToTrader > 0) tradeOffer.goldToTrader--;
                break;
            case "food":
                if (tradeOffer.foodToPlayer > 0) tradeOffer.foodToPlayer--;
                break;
            case "water":
                if (tradeOffer.waterToPlayer > 0) tradeOffer.waterToPlayer--;
                break;
        }
        UpdateDisplays();
    }
}
