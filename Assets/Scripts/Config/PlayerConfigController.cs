using TMPro;
using UnityEngine;

public class PlayerConfigController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI visionSelection;
    [SerializeField] private TextMeshProUGUI brainSelection;
    [SerializeField] private TextMeshProUGUI maxFoodSelection;
    [SerializeField] private TextMeshProUGUI maxWaterSelection;
    [SerializeField] private TextMeshProUGUI maxEnergySelection;

    private void Start()
    {
        visionSelection.text = GameConfig.instance.playerConfig.visionType.ToString();
        brainSelection.text = GameConfig.instance.playerConfig.brainType.ToString();
        maxFoodSelection.text = GameConfig.instance.playerConfig.maxFood.ToString();
        maxWaterSelection.text = GameConfig.instance.playerConfig.maxWater.ToString();
        maxEnergySelection.text = GameConfig.instance.playerConfig.maxEnergy.ToString();
    }

    public void IncrementVision()
    {
        visionSelection.text = GameConfig.instance.playerConfig.IncrementVisionType();
    }

    public void DecrementVision()
    {
        visionSelection.text = GameConfig.instance.playerConfig.DecrementVisionType();
    }

    public void IncrementBrain()
    {
        brainSelection.text = GameConfig.instance.playerConfig.IncrementBrainType();
    }

    public void DecrementBrain()
    {
        brainSelection.text = GameConfig.instance.playerConfig.DecrementBrainType();
    }

    public void IncrementMaxFood()
    {
        maxFoodSelection.text = GameConfig.instance.playerConfig.IncrementMaxFood();
    }

    public void DecrementMaxFood()
    {
        maxFoodSelection.text = GameConfig.instance.playerConfig.DecrementMaxFood();
    }

    public void IncrementMaxWater()
    {
        maxWaterSelection.text = GameConfig.instance.playerConfig.IncrementMaxWater();
    }

    public void DecrementMaxWater()
    {
        maxWaterSelection.text = GameConfig.instance.playerConfig.DecrementMaxWater();
    }

    public void IncrementMaxEnergy()
    {
        maxEnergySelection.text = GameConfig.instance.playerConfig.IncrementMaxEnergy();
    }

    public void DecrementMaxEnergy()
    {
        maxEnergySelection.text = GameConfig.instance.playerConfig.DecrementMaxEnergy();
    }
}
