using UnityEngine;
using System.Threading.Tasks;

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
    private Vision vision;
    private IBrain brain; // Interface type

    private GameState currentState;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        InitializeGame();

        if (GameConfig.instance.playerConfig.brainType == BrainType.CPU)
        {
            StartCPUGame();
        }
        else
        {
            StartUserGame();
        }
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
        playerRenderer.SetMap(map);

        player.SetMapPosition(0, Mathf.RoundToInt(map.height / 2f));

        // Create Vision object
        switch (GameConfig.instance.playerConfig.visionType)
        {
            case VisionType.Cautious:
                vision = new CautiousVision(player, map);
                break;
            case VisionType.Focused:
                vision = new FocusedVision(player, map);
                break;
            case VisionType.Keeneyed:
                vision = new KeeneyedVision(player, map);
                break;
            case VisionType.Farsighted:
                vision = new FarsightedVision(player, map);
                break;
            default:
                Debug.LogWarning("Unknown vision type; defaulting to Cautious.");
                vision = new CautiousVision(player, map);
                break;
        }
    }

    private async void StartCPUGame()
    {
        brain = new AIBrain(player, map, vision);

        if (brain is AIBrain aiBrain)
        {
            await aiBrain.ResetMemoryAsync();
        }

    while (currentState == GameState.Playing && this != null && isActiveAndEnabled)
        {
            Decision decision = await brain.GetDecisionAsync();
            ApplyDecision(decision);

            if (CheckGameEndConditions())
                break;

            await Task.Delay(500);
        }
    }

    private async void StartUserGame()
    {
        brain = new UserBrain(player, map, vision);

        while (currentState == GameState.Playing)
        {
            Decision decision = await brain.GetDecisionAsync();
            ApplyDecision(decision);

            if (CheckGameEndConditions())
                break;

            await Task.Delay(1); // No artificial delay for user input
        }
    }

    private void ApplyDecision(Decision decision)
    {
        if (decision == null || decision.decisionType == DecisionType.Invalid)
        {
            Debug.LogWarning("Invalid decision received.");
            return;
        }

        switch (decision.decisionType)
        {
            case DecisionType.Move:
                Debug.Log($"[GameManager] Move {decision.direction}");
                player.Move(decision.direction); // Placeholder
                break;
            case DecisionType.Rest:
                Debug.Log("[GameManager] Rest");
                player.Rest(); // Placeholder
                break;
            case DecisionType.Trade:
                Debug.Log("[GameManager] Trade");
                player.AttemptTrade(map.GetTile(player.mapPosition.x, player.mapPosition.y)); // Placeholder
                break;
        }
    }

    private bool CheckGameEndConditions()
    {
        if (player.food <= 0 || player.water <= 0 || player.energy <= 0)
        {
            GameOver();
            return true;
        }

        if (player.mapPosition.x >= map.width)
        {
            GameOver();
            return true;
        }

        return false;
    }

    public void GameOver()
    {
        currentState = GameState.GameOver;
        Debug.Log("Game Over!");
        // TODO: Show results, reset option
    }

    private void OnDestroy()
    {
        currentState = GameState.GameOver;
    }

    private void OnApplicationQuit()
    {
        currentState = GameState.GameOver;
    }

    public void StopGame()
    {
        currentState = GameState.GameOver;
    }
}
