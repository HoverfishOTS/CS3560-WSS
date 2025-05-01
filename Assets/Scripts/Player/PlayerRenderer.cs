using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    public static PlayerRenderer Instance;

    [SerializeField] private GameObject playerSprite;

    public MapPosition position { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    public void UpdatePosition(int x, int y)
    {
        if (map != null)
        {
            MapTerrain terrain = map.GetTile(x, y);
            if (terrain != null && terrain.tile != null)
            {
                playerSprite.transform.position = terrain.tile.transform.position;
            }
        }
        position = new MapPosition(x, y);
    }

    public void SetPlayerSize(Vector2 size)
    {
        RectTransform rt = playerSprite.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = size;
        }
    }

    private Map map;

    public void SetMap(Map map)
    {
        this.map = map;
    }

}
