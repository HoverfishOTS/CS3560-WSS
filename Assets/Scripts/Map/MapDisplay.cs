using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    [Header("Tile Settings")]
    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private float tileSpacing = 1.1f; // Slight spacing so tiles don't overlap

    public void DisplayMap(Map map)
    {
        for (int y = 0; y < map.height; y++)
        {
            for (int x = 0; x < map.width; x++)
            {
                MapTerrain terrain = map.GetTile(x, y);

                Vector3 position = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                GameObject tileObj = Instantiate(tilePrefab, position, Quaternion.identity, transform);

                TileDisplay tileDisplay = tileObj.GetComponent<TileDisplay>();
                if (tileDisplay != null)
                {
                    tileDisplay.Initialize(terrain);
                }
            }
        }
    }
}
