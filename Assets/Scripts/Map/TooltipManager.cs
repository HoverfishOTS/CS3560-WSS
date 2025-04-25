using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Tooltip UI Elements")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private TextMeshProUGUI tooltipText;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        HideTooltip();
    }

    public void ShowTooltip(MapTerrain terrain)
    {
        if (tooltipPanel == null || tooltipText == null) return;

        tooltipPanel.SetActive(true);

        string info = $"Biome: {terrain.biome}\n" +
                      $"Movement Cost: {terrain.movementCost}\n" +
                      $"Water Cost: {terrain.waterCost}\n" +
                      $"Food Cost: {terrain.foodCost}\n";

        if (terrain.hasFoodBonus)
            info += "\nFood Bonus" + (terrain.foodBonusRepeating ? " (Repeating)" : "");
        if (terrain.hasWaterBonus)
            info += "\nWater Bonus" + (terrain.waterBonusRepeating ? " (Repeating)" : "");
        if (terrain.hasGoldBonus)
            info += "\nGold Bonus";
        if (terrain.hasTrader)
            info += "\nTrader Present";

        tooltipText.text = info;
    }

    public void HideTooltip()
    {
        if (tooltipPanel != null)
            tooltipPanel.SetActive(false);
    }
}
