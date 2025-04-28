using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private GameObject tilePrefab;

    private Map currentMap;

    public void DisplayMap(Map map)
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        float tileSpacing = rectTransform.sizeDelta.x / map.width;
        Vector2 tileDimensions = Vector2.one * tileSpacing;

        float xOffset = (map.width - 1) * tileSpacing / 2f;
        float yOffset = (map.height - 1) * tileSpacing / 2f;

        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                MapTerrain terrain = map.GetTile(x, y);

                Vector3 position = new Vector3(
                    (x * tileSpacing) - xOffset,
                    (y * tileSpacing) - yOffset,
                    0f
                );

                GameObject tileObj = Instantiate(tilePrefab, Vector3.zero, Quaternion.identity, transform);
                tileObj.transform.localPosition = new Vector3(
                    (x * tileSpacing) - xOffset,
                    (y * tileSpacing) - yOffset,
                    0f
                );


                TileDisplay tileDisplay = tileObj.GetComponent<TileDisplay>();
                if (tileDisplay != null)
                {
                    tileDisplay.Initialize(terrain, tileDimensions);
                }
            }
        }

        currentMap = map;
    }

    public void RegenerateCurrentMap()
    {
        // destroy all children
        foreach(Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // display
        DisplayMap(currentMap);
    }
}
