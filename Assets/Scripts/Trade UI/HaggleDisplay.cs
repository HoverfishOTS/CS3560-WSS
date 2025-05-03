using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HaggleDisplay : MonoBehaviour
{
    [Header("Displays")]
    [SerializeField] private HaggleBar haggleBar;
    [SerializeField] private TextMeshProUGUI goldAmount;
    [SerializeField] private TextMeshProUGUI goldEffect;

    [Header("Gold Effects")]
    [SerializeField] private Color positive;
    [SerializeField] private Color negative;

    [Header("Sprites")]
    public Image playerImage;
    public Image traderImage;
    public Sprite wess, mess, gess, less, yess;

    private Trader trader;

    public async Task<int> StartHaggling(Trader trader, TradeOffer offer)
    {
        this.trader = trader;
        UpdateSprites();

        goldAmount.text = offer.goldToTrader.ToString();

        // You can adjust the speed however you want
        string result = await haggleBar.StartHaggle(trader);

        int goodBonus = 0;
        int perfectBonus = 0;

        switch (trader.traderType)
        {
            case "normal":
                goodBonus = 1;
                perfectBonus = 2;
                break;
            case "generous":
                goodBonus = 2;
                perfectBonus = 3;
                break;
            case "stingy":
                goodBonus = 0;
                perfectBonus = 1;
                break;
        }

        int netGold = 0;

        switch (result)
        {
            case "perfect":
                Debug.Log("Perfect haggle! Great bonus.");
                netGold = Mathf.Min(perfectBonus, offer.goldToTrader - 1);
                break;

            case "good":
                Debug.Log("Good haggle. Some bonus.");
                netGold = Mathf.Min(goodBonus, offer.goldToTrader - 1);
                break;

            case "fail":
                Debug.Log("Haggle failed. No bonus.");
                netGold = 0;
                break;
        }

        goldEffect.text = "-" + netGold;
        goldEffect.color = netGold > 0 ? positive : negative;

        haggleBar.gameObject.SetActive(false);
        goldEffect.gameObject.SetActive(true);

        return netGold;
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
}
