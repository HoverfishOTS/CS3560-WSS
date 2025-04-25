using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image tileImage;
    private MapTerrain terrain;

    private static readonly Color plainsColor = new Color(0.6f, 0.8f, 0.4f);
    private static readonly Color desertColor = new Color(0.9f, 0.8f, 0.4f);
    private static readonly Color mountainsColor = new Color(0.6f, 0.6f, 0.6f);
    private static readonly Color forestColor = new Color(0.2f, 0.5f, 0.2f);
    private static readonly Color jungleColor = new Color(0.1f, 0.4f, 0.1f);
    private static readonly Color swampColor = new Color(0.4f, 0.3f, 0.2f);

    public void Initialize(MapTerrain terrain)
    {
        this.terrain = terrain;

        if (tileImage == null)
            tileImage = GetComponent<Image>();

        if (tileImage != null)
        {
            tileImage.color = GetColorForBiome(terrain.biome);
        }
    }

    private Color GetColorForBiome(Biome biome)
    {
        return biome switch
        {
            Biome.Plains => plainsColor,
            Biome.Desert => desertColor,
            Biome.Mountains => mountainsColor,
            Biome.Forest => forestColor,
            Biome.Jungle => jungleColor,
            Biome.Swamp => swampColor,
            _ => Color.white
        };
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        TooltipManager.Instance.ShowTooltip(terrain);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TooltipManager.Instance.HideTooltip();
    }
}
