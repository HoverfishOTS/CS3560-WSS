using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    public static PlayerRenderer Instance;

    [SerializeField] private GameObject playerSprite;

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
        Map map = GameManager.Instance?.GetMap();
        if (map != null)
        {
            MapTerrain mapTerrain = map.GetTile(x, y);
            if (mapTerrain != null)
            {
                playerSprite.transform.position = mapTerrain.tile.transform.position;
            }
        }
    }

    public void SetPlayerSize(Vector2 size)
    {
        RectTransform rt = playerSprite.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.sizeDelta = size;
        }
    }
}
