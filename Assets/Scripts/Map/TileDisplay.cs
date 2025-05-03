using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TileDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image tileImage;
    [SerializeField] private Color undiscoveredColor;
    [SerializeField] private GameObject selectionFrame;
    private MapTerrain terrain;

    private bool discovered = false;
    private bool selectable = false;

    private static readonly Color plainsColor = new Color(0.6f, 0.8f, 0.4f);
    private static readonly Color desertColor = new Color(0.9f, 0.8f, 0.4f);
    private static readonly Color mountainsColor = new Color(0.6f, 0.6f, 0.6f);
    private static readonly Color forestColor = new Color(0.2f, 0.5f, 0.2f);
    private static readonly Color jungleColor = new Color(0.1f, 0.4f, 0.1f);
    private static readonly Color swampColor = new Color(0.4f, 0.3f, 0.2f);

    [SerializeField] private GameObject cosmetics;

    [Header("Trader Display")]
    [SerializeField] private GameObject traderObject;
    [SerializeField] private Image traderImage;
    [SerializeField] private Sprite stingyTrader;
    [SerializeField] private Sprite normalTrader;
    [SerializeField] private Sprite generousTrader;
    [SerializeField] private Vector2 traderPositionMinMax = new Vector2(-15f, 15f);

    public void Initialize(MapTerrain terrain, Vector2 dimensions)
    {
        this.terrain = terrain;

        if (tileImage == null)
            tileImage = GetComponent<Image>();

        if (tileImage != null)
        {
            tileImage.color = GetColorForBiome(terrain.biome);
        }

        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = dimensions;

        RectTransform selectionFrameRT = selectionFrame.GetComponent<RectTransform>();
        selectionFrameRT.sizeDelta = dimensions;

        if(cosmetics != null)
        {
            cosmetics.SetActive(false);
            cosmetics.transform.localScale = Vector3.one * dimensions.x / 50f;
            // update trader display
            if (terrain.hasTrader)
            {
                if(terrain.trader.traderType == "stingy")
                {
                    traderImage.sprite = stingyTrader;
                }
                else if (terrain.trader.traderType == "generous")
                {
                    traderImage.sprite = generousTrader;
                }
                else
                {
                    traderImage.sprite = normalTrader;
                }
                traderObject.transform.localPosition = new Vector2(Random.Range(traderPositionMinMax.x, traderPositionMinMax.y), Random.Range(traderPositionMinMax.x, traderPositionMinMax.y));
                traderObject.SetActive(true);
            }
            else
            {
                traderObject.SetActive(false);
            }

            // TODO: Spawn cosmetics on tile based on Biome
        }
    }

    public void DiscoverTile()
    {
        discovered = true;
        if (tileImage != null)
        {
            tileImage.color = GetColorForBiome(terrain.biome);
        }
        if (cosmetics != null)
        {
            cosmetics.SetActive(true);
        }
    }

    private Color GetColorForBiome(Biome biome)
    {
        if (!discovered) return undiscoveredColor;

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
        if(!discovered) return;
        TooltipManager.Instance.ShowTooltip(terrain);

        if (selectable)
        {
            StatEffectManager.Instance.ReflectMapTerrain(terrain);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!discovered) return;
        TooltipManager.Instance.HideTooltip();

        if (selectable)
        {
            StatEffectManager.Instance.ClearEffect("all");
        }
    }

    public void OnTileClicked()
    {
        if (!selectable) return;

        string dir = CalculateDirection();

        if(dir != "STAY" && dir != "INVALID")
        {
            UserInputManager.Instance.MakeDecision(new Decision
            {
                decisionType = DecisionType.Move,
                direction = dir,
            });

            // Update tooltip
            TooltipManager.Instance.ShowTooltip(terrain);
            StatEffectManager.Instance.ReflectMapTerrain(terrain);
        }
        else if (dir == "STAY")
        {
            UserInputManager.Instance.MakeDecision(new Decision
            {
                decisionType = DecisionType.Rest,
            });
        }
    }

    public void SetSelectionFrameActive(bool active)
    {
        // if (!discovered) return;
        if(selectionFrame != null)
        {
            selectionFrame.SetActive(active);
        }
        selectable = active;

    }

    private string CalculateDirection()
    {
        MapPosition thisPosition = terrain.coordinate;
        MapPosition playerPosition = PlayerRenderer.Instance.position;

        int dX = thisPosition.x - playerPosition.x;
        int dY = thisPosition.y - playerPosition.y;

        //Debug.Log($"This Tile: {thisPosition}, Player : {playerPosition}");

        if (dX == 1 && dY == 1) return "NORTHEAST";
        if (dX == 1 && dY == -1) return "SOUTHEAST";
        if (dX == -1 && dY == -1) return "SOUTHWEST";
        if (dX == -1 && dY == 1) return "NORTHWEST";

        switch (dX)
        {
            case 1:
                return "EAST";
            case -1:
                return "WEST";
        }

        switch(dY)
        {
            case -1:
                return "SOUTH";
            case 1:
                return "NORTH";
        }

        if (dX == 0 && dY == 0) return "STAY";
        return "INVALID";
    }

    public void HideTrader()
    {
        traderObject.SetActive(false);
    }
}
