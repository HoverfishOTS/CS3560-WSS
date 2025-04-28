using UnityEngine;
using TMPro;

public class Tooltip : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI biome;
    [SerializeField] private TextMeshProUGUI foodCost;
    [SerializeField] private TextMeshProUGUI waterCost;
    [SerializeField] private TextMeshProUGUI movementCost;
    [SerializeField] private TextMeshProUGUI foodBonus;
    [SerializeField] private TextMeshProUGUI waterBonus;
    [SerializeField] private TextMeshProUGUI goldBonus;
    [SerializeField] private TextMeshProUGUI trader;

    public void UpdateInfo(MapTerrain mapTerrain)
    {
        biome.text = mapTerrain.biome.ToString();
        foodCost.text = "=" + mapTerrain.foodCost.ToString();
        waterCost.text = "=" + mapTerrain.waterCost.ToString();
        movementCost.text = "=" + mapTerrain.movementCost.ToString();
        foodBonus.text = "=" + mapTerrain.foodBonus.ToString();
        waterBonus.text = "=" + mapTerrain.waterBonus.ToString();
        goldBonus.text = "=" + mapTerrain.goldBonus.ToString();

        // trader info
    }
}
