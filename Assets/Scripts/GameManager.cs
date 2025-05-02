using UnityEngine;
using System.Threading.Tasks;
using Unity.VisualScripting;
using TMPro;

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

    [Header("Player Stats")]
    [SerializeField] private TextMeshProUGUI foodDisplay;
    [SerializeField] private TextMeshProUGUI waterDisplay;
    [SerializeField] private TextMeshProUGUI energyDisplay;
    [SerializeField] private TextMeshProUGUI goldDisplay;
    [SerializeField] private GameObject tradeButton;
    [SerializeField] private RestButton restButton;

    private Map map;
    private Player player;
    private Vision vision;
    private IBrain brain; // Interface type

    private GameState currentState;

    private void Awake()
    {
        Application.runInBackground = true;
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

        player = new Player(map);
        player.InitializePlayer(
            GameConfig.instance.playerConfig.maxFood,
            GameConfig.instance.playerConfig.maxWater,
            GameConfig.instance.playerConfig.maxEnergy,
            GameConfig.instance.playerConfig.visionType
        );
        StatEffectManager.Instance.SetPlayer(player);

        mapDisplay.DisplayMap(map);
        playerRenderer.SetMap(map);

        player.SetMapPosition(0, Mathf.RoundToInt(map.height / 2f));

        UpdateDisplay();

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
            vision.GenerateField();

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
            PrepareUserDecision();

            Decision decision = await brain.GetDecisionAsync();
            ApplyDecision(decision);
            vision.GenerateField();

            if (CheckGameEndConditions())
                break;

            await Task.Delay(1); // No artificial delay for user input
        }
    }

    private void PrepareUserDecision()
    {
        // enable selectable on surrounding tiles
        MapPosition mapPosition = player.mapPosition;
        MapTerrain[] surroundingTiles = new MapTerrain[9] {
            map.GetTile(mapPosition.x + 1, mapPosition.y),   // east 
            map.GetTile(mapPosition.x, mapPosition.y + 1),   // north
            map.GetTile(mapPosition.x, mapPosition.y - 1),   // south
            map.GetTile(mapPosition.x - 1, mapPosition.y),   // west
            map.GetTile(mapPosition.x + 1, mapPosition.y + 1),   // northeast
            map.GetTile(mapPosition.x + 1, mapPosition.y - 1),   // southeast
            map.GetTile(mapPosition.x - 1, mapPosition.y - 1),   // southwest
            map.GetTile(mapPosition.x - 1, mapPosition.y + 1),   // northwest
            map.GetTile(mapPosition.x, mapPosition.y)        // current
        };
        for (int i = 0; i < surroundingTiles.Length; i++)
        {
            if (surroundingTiles[i] == null) continue;
            surroundingTiles[i].tile.SetSelectionFrameActive(true);
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
                player.AttemptTrade(); // Placeholder (needs to pass string input, int inputCount, string output, int outputCount)
                break;
        }
    }

    private bool CheckGameEndConditions()
    {
        UpdateDisplay();

        if (player.food <= 0 || player.water <= 0 || player.energy <= 0)
        {
            GameOver();
            return true;
        }

        if (player.mapPosition.x >= map.width-1)
        {
            GameOver();
            return true;
        }

        return false;
    }

    private void UpdateDisplay()
    {
        // resources
        foodDisplay.text = player.food.ToString();
        waterDisplay.text = player.water.ToString();
        energyDisplay.text = player.energy.ToString();
        goldDisplay.text = player.gold.ToString();

        // buttons
        MapTerrain currentTerrain = player.GetCurrentMapTerrain();
        if (currentTerrain != null)
        {
            tradeButton.SetActive(currentTerrain.hasTrader);
        }
        restButton.terrain = currentTerrain;
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
