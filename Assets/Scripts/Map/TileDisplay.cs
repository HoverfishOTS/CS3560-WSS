using System.Collections.Generic;
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
    [SerializeField] private GameObject mapCosmetics;

    [Header("Trader Display")]
    [SerializeField] private GameObject traderObject;
    [SerializeField] private Image traderImage;
    [SerializeField] private Sprite stingyTrader;
    [SerializeField] private Sprite normalTrader;
    [SerializeField] private Sprite generousTrader;
    [SerializeField] private Vector2 traderPositionMinMax = new Vector2(-15f, 15f);

    [Header("Map Cosmetics")]
    [SerializeField] private GameObject[] plainsCosmetics;
    [SerializeField] private GameObject[] forestCosmetics;
    [SerializeField] private GameObject[] mountainsCosmetics;
    [SerializeField] private GameObject[] desertCosmetics;
    [SerializeField] private GameObject[] jungleCosmetics;
    [SerializeField] private GameObject[] swampCosmetics;

    [Header("Map Consmetic Densities")]
    [SerializeField] private int plainsDensity = 8;
    [SerializeField] private int forestDensity = 4;
    [SerializeField] private int mountainsDensity = 3;
    [SerializeField] private int desertDensity = 3;
    [SerializeField] private int jungleDensity = 10;
    [SerializeField] private int swampDensity = 3;

    public void Initialize(MapTerrain terrain, Vector2 dimensions)
    {
        this.terrain = terrain;

        // Set base scale relative to 50x50 baseline
        float scaleFactor = dimensions.x / 50f;
        transform.localScale = Vector3.one * scaleFactor;

        // Set tile color
        if (tileImage == null)
            tileImage = GetComponent<Image>();

        if (tileImage != null)
            tileImage.color = GetColorForBiome(terrain.biome);

        // Reset sizeDelta for alignment purposes (optional if layout is fixed)
        GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);
        selectionFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(50f, 50f);

        // Hide cosmetics by default
        if (cosmetics != null)
            cosmetics.SetActive(false);

        // Setup trader
        if (terrain.hasTrader)
        {
            traderImage.sprite = terrain.trader.traderType switch
            {
                "stingy" => stingyTrader,
                "generous" => generousTrader,
                _ => normalTrader
            };

            traderObject.transform.localPosition = new Vector2(
                Random.Range(traderPositionMinMax.x, traderPositionMinMax.y),
                Random.Range(traderPositionMinMax.x, traderPositionMinMax.y)
            );

            traderObject.SetActive(true);
        }
        else
        {
            traderObject.SetActive(false);
        }

        // Spawn cosmetics
        if (mapCosmetics != null)
        {
            mapCosmetics.SetActive(false);

            (GameObject[] prefabSet, int density) = terrain.biome switch
            {
                Biome.Plains => (plainsCosmetics, plainsDensity),
                Biome.Forest => (forestCosmetics, forestDensity),
                Biome.Mountains => (mountainsCosmetics, mountainsDensity),
                Biome.Desert => (desertCosmetics, desertDensity),
                Biome.Jungle => (jungleCosmetics, jungleDensity),
                Biome.Swamp => (swampCosmetics, swampDensity),
                _ => (null, 0)
            };

            if (prefabSet == null || prefabSet.Length == 0 || density == 0)
                return;

            // Use fixed tile size (50) for local layout, scale handles visual size
            float padding = 5f; // fixed padding within 50x50 layout
            float paddedWidth = 50f - padding * 2f;
            float paddedHeight = 50f - padding * 2f;

            int gridCount = Mathf.CeilToInt(Mathf.Sqrt(density));
            float cellWidth = paddedWidth / gridCount;
            float cellHeight = paddedHeight / gridCount;

            float startX = -paddedWidth / 2f + cellWidth / 2f;
            float startY = -paddedHeight / 2f + cellHeight / 2f;

            // Generate grid
            List<Vector2> spawnPositions = new List<Vector2>();
            for (int x = 0; x < gridCount; x++)
            {
                for (int y = 0; y < gridCount; y++)
                {
                    float posX = startX + x * cellWidth;
                    float posY = startY + y * cellHeight;
                    spawnPositions.Add(new Vector2(posX, posY));
                }
            }

            // Shuffle
            for (int i = 0; i < spawnPositions.Count; i++)
            {
                int j = Random.Range(i, spawnPositions.Count);
                (spawnPositions[i], spawnPositions[j]) = (spawnPositions[j], spawnPositions[i]);
            }

            // Spawn
            int spawnCount = Mathf.Min(density, spawnPositions.Count);
            for (int i = 0; i < spawnCount; i++)
            {
                GameObject prefab = prefabSet[Random.Range(0, prefabSet.Length)];
                GameObject instance = Instantiate(prefab, mapCosmetics.transform);
                RectTransform cosmeticRT = instance.GetComponent<RectTransform>();

                float jitterX = Random.Range(-cellWidth * 0.3f, cellWidth * 0.3f);
                float jitterY = Random.Range(-cellHeight * 0.3f, cellHeight * 0.3f);

                cosmeticRT.anchoredPosition = spawnPositions[i] + new Vector2(jitterX, jitterY);
            }
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
        if (mapCosmetics != null)
        {
            mapCosmetics.SetActive(true);
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
