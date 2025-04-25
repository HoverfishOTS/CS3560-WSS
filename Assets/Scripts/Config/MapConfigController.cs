using TMPro;
using UnityEngine;

public class MapConfigController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI difficultySelection;
    [SerializeField] private TextMeshProUGUI widthSelection;
    [SerializeField] private TextMeshProUGUI heightSelection;

    private void Start()
    {
        difficultySelection.text = GameConfig.instance.mapConfig.difficulty.ToString();
        widthSelection.text = GameConfig.instance.mapConfig.width.ToString();
        heightSelection.text = GameConfig.instance.mapConfig.height.ToString();
    }

    public void IncrementDifficulty()
    {
        difficultySelection.text = GameConfig.instance.mapConfig.IncrementDifficulty();
    }

    public void DecrementDifficulty()
    {
        difficultySelection.text = GameConfig.instance.mapConfig.DecrementDifficulty();
    }

    public void IncrementWidth()
    {
        widthSelection.text = GameConfig.instance.mapConfig.IncrementWidth();
    }

    public void DecrementWidth()
    {
        widthSelection.text = GameConfig.instance.mapConfig.DecrementWidth();
    }

    public void IncrementHeight()
    {
        heightSelection.text = GameConfig.instance.mapConfig.IncrementHeight();
    }

    public void DecrementHeight()
    {
        heightSelection.text = GameConfig.instance.mapConfig.DecrementHeight();
    }
}
