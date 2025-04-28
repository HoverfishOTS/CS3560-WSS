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
    }

    public void InitializeGame()
    {
        currentState = GameState.Playing;

        map = mapGenerator.GenerateMap();

        player = new Player();
        player.InitializePlayer(
            GameConfig.instance.playerConfig.maxFood,
            GameConfig.instance.playerConfig.maxWater,
            GameConfig.instance.playerConfig.maxEnergy
        );

        mapDisplay.DisplayMap(map);
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
}
