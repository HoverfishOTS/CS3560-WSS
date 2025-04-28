using UnityEngine;

public enum GameState
{
    MainMenu,
    Playing,
    GameOver
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("References")]
    [SerializeField] private MapGenerator mapGenerator;
    [SerializeField] private MapDisplay mapDisplay;
    [SerializeField] private PlayerRenderer playerRenderer;

    private Map map;
    private Player player;
    private GameState currentState;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        //DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        InitializeGame();
        StartGame();
    }

    public void InitializeGame()
    {
        currentState = GameState.Playing;

        map = mapGenerator.GenerateMap();

        player = new Player();
        player.InitializePlayer(
            GameConfig.instance.playerConfig.maxFood,
            GameConfig.instance.playerConfig.maxWater,
            GameConfig.instance.playerConfig.maxEnergy,
            GameConfig.instance.playerConfig.visionType
        );

        mapDisplay.DisplayMap(map);

        player.SetMapPosition(0, Mathf.RoundToInt(map.height / 2f));
    }

    private void StartGame()
    {
        // get vision
        MapTerrain[][] vision = GetVision();
        CurrentGameState currentGameState = new CurrentGameState(new Vision(vision), player);

        // get decision or await decision


        // execute decision

    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over!");
        // Additional Game Over logic can be added here
    }

    public GameState GetGameState()
    {
        return currentState;
    }

    public Map GetMap()
    {
        return map;
    }

    public Player GetPlayer()
    {
        return player;
    }

    private MapTerrain[][] GetVision()
    {
        bool[] inVision = null;
        Debug.Log("Vision Type: " + player.visionType.ToString());
        switch (player.visionType)
        {
            case VisionType.Focused:
                inVision = new bool[15]{
                    false, false, false,
                    false, true, false,
                    true, true, false,
                    false, true, false,
                    false, false, false
                };
                break;
            case VisionType.Cautious:
                inVision = new bool[15]{
                    false, false, false,
                    true, false, false,
                    true, true, false,
                    true, false, false,
                    false, false, false
                };
                break;
            case VisionType.Keeneyed:
                inVision = new bool[15]{
                    false, false, false,
                    true, true, false,
                    true, true, true,
                    true, true, false,
                    false, false, false
                };
                break;
            case VisionType.Farsighted:
                inVision = new bool[15]{
                    true, true, false,
                    true, true, true,
                    true, true, true,
                    true, true, true,
                    true, true, false
                };
                break;
        }

        if (inVision == null) return null;

        MapTerrain[] area = new MapTerrain[inVision.Length];
        int index = 0;
        for(int y = -2; y <= 2; y++)
        {
            for(int x = 0; x <= 2; x++)
            {
                area[index] = map.GetTile(player.mapPosition.x + x, player.mapPosition.y + y);
                index++;
            }
        }

        MapTerrain[][] result = new MapTerrain[5][];
        for (int i = 0; i < 5; i++)
        {
            result[i] = new MapTerrain[3];
        }
        for (int i = 0; i < index; i++)
        {
            result[i / 3][ i % 3] = inVision[i] ? area[i] : null;
        }

        // update the display of each MapTerrain in vision
        for(int i = 0; i < result.Length; i++)
        {
            for(int j = 0; j < result[i].Length; j++)
            {
                if(result[i][j] != null)
                {
                    result[i][j].tile.DiscoverTile();
                }
            }
        }

        return result;
    }
}
