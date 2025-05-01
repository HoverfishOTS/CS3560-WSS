using TMPro;
using UnityEngine;

public class StatEffectManager : MonoBehaviour
{
    public static StatEffectManager Instance;

    private Player player;

    [Header("Stat Effects")]
    [SerializeField] private TextMeshProUGUI foodEffect;
    [SerializeField] private TextMeshProUGUI waterEffect;
    [SerializeField] private TextMeshProUGUI energyEffect;
    [SerializeField] private TextMeshProUGUI goldEffect;

    [Header("Colors")]
    [SerializeField] private Color positiveColor;
    [SerializeField] private Color negativeColor;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ClearEffect("all");
    }

    public void SetPlayer(Player player)
    {
        this.player = player;
    }

    public void ReflectMapTerrain(MapTerrain terrain)
    {
        bool rest = terrain.coordinate.Equals(player.mapPosition);
        int netFood = terrain.foodBonus - (!rest ? terrain.foodCost : Mathf.RoundToInt(Mathf.Ceil(terrain.foodCost / 2f)));
        int netWater = terrain.waterBonus - (!rest ? terrain.waterCost : Mathf.RoundToInt(Mathf.Ceil(terrain.waterCost / 2f)));

        if (netFood != 0) SetEffect("food", netFood > 0, Mathf.Abs(netFood));
        if (netWater != 0) SetEffect("water", netWater > 0, Mathf.Abs(netWater));
        if (!rest)
        {
            if (terrain.movementCost > 0) SetEffect("energy", false, terrain.movementCost);
        }
        else
        {
            int netEnergy = Mathf.Min(2, GameConfig.instance.playerConfig.maxEnergy - player.energy);
            if(netEnergy > 0) SetEffect("energy", true, netEnergy);
        }
        if (terrain.hasGoldBonus) SetEffect("gold", true, terrain.goldBonus);
    }

    private void SetEffect(string type, bool positive, int value)
    {
        switch (type.ToLower())
        {
            case "food":
                foodEffect.text = (positive ? "+" : "-") + value.ToString(); foodEffect.color = positive ? positiveColor : negativeColor; break;
            case "water":
                waterEffect.text = (positive ? "+" : "-") + value.ToString(); waterEffect.color = positive ? positiveColor : negativeColor; break;
            case "energy":
                energyEffect.text = (positive ? "+" : "-") + value.ToString(); energyEffect.color = positive ? positiveColor : negativeColor; break;
            case "gold":
                goldEffect.text = (positive ? "+" : "-") + value.ToString(); goldEffect.color = positive ? positiveColor : negativeColor; break;
        }
    }

    public void ClearEffect(string type)
    {
        switch (type.ToLower())
        {
            case "food":
                foodEffect.text = string.Empty; break;
            case "water":
                waterEffect.text = string.Empty; break;
            case "energy":
                energyEffect.text = string.Empty; break;
            case "gold":
                goldEffect.text = string.Empty; break;
            case "all":
                foodEffect.text = string.Empty;
                waterEffect.text = string.Empty;
                energyEffect.text = string.Empty;
                goldEffect.text = string.Empty;
                break;
        }
    }
}
