using UnityEngine;
using System.Threading.Tasks;
// using Unity.VisualScripting; // Uncomment if needed
using TMPro;
using System.Collections; // Added for Coroutine if needed elsewhere

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
    [SerializeField] private StatEffectManager statEffectManager;

    [Header("Player Stats UI")]
    [SerializeField] private TextMeshProUGUI foodDisplay;
    [SerializeField] private TextMeshProUGUI waterDisplay;
    [SerializeField] private TextMeshProUGUI energyDisplay;
    [SerializeField] private TextMeshProUGUI goldDisplay;
    [SerializeField] private GameObject tradeButton;
    [SerializeField] private RestButton restButton;

    [Header("Debug Options")]
    [Tooltip("If true and AI lands on a trader tile, force initiate trade for testing.")]
    [SerializeField] private bool forceAITradeOnTraderTile = true; // Toggle this for testing

    // --- Private State ---
    private Map map;
    private Player player;
    private Vision vision;
    private IBrain brain;
    private GameState currentState;

    private void Awake()
    {
        Application.runInBackground = true;
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (GameConfig.instance == null) {
             Debug.LogError("[GameManager] CRITICAL: GameConfig instance not found!");
        }
         if (statEffectManager == null) {
              statEffectManager = FindFirstObjectByType<StatEffectManager>(); // Use newer API
              if (statEffectManager == null) Debug.LogError("[GameManager] StatEffectManager reference missing!");
         }
    }

    private void Start()
    {
        if (GameConfig.instance == null)
        {
             Debug.LogError("[GameManager] Cannot Start game - GameConfig is missing!");
             return;
        }

        InitializeGame();

        PlayerConfig playerConfig = GameConfig.instance.playerConfig;
        if (playerConfig.brainType == BrainType.CPU)
        {
             brain = new AIBrain(player, map, vision);
             Debug.Log("[GameManager] AIBrain created.");
             StartCPUGame();
        }
        else
        {
              brain = new UserBrain(player, map, vision);
              Debug.Log("[GameManager] UserBrain created.");
              StartUserGame();
        }
    }

    public void InitializeGame()
    {
        currentState = GameState.Playing;

        if (mapGenerator == null) { Debug.LogError("[GameManager] MapGenerator reference missing!"); return; }
        map = mapGenerator.GenerateMap();
        if (map == null) { Debug.LogError("[GameManager] Map generation failed!"); return; }

        player = new Player(map);
        PlayerConfig playerConfig = GameConfig.instance.playerConfig;
        player.InitializePlayer(
            playerConfig.maxFood, playerConfig.maxWater, playerConfig.maxEnergy, playerConfig.visionType
        );

        if (statEffectManager != null) statEffectManager.SetPlayer(player);
        else Debug.LogWarning("[GameManager] StatEffectManager not found.");

        if (mapDisplay == null) { Debug.LogError("[GameManager] MapDisplay reference missing!"); return; }
        mapDisplay.DisplayMap(map);

        if (playerRenderer == null) { Debug.LogError("[GameManager] PlayerRenderer reference missing!"); return; }
        playerRenderer.SetMap(map);
        player.SetMapPosition(0, Mathf.RoundToInt((map.height - 1) / 2f));

        switch (playerConfig.visionType)
        {
            case VisionType.Cautious: vision = new CautiousVision(player, map); break;
            case VisionType.Focused: vision = new FocusedVision(player, map); break;
            case VisionType.Keeneyed: vision = new KeeneyedVision(player, map); break;
            case VisionType.Farsighted: vision = new FarsightedVision(player, map); break;
            default:
                Debug.LogWarning($"[GameManager] Unknown vision type '{playerConfig.visionType}'. Defaulting to Focused.");
                vision = new FocusedVision(player, map);
                break;
        }
        vision.GenerateField();

        UpdateDisplay();
        Debug.Log("[GameManager] Game Initialized.");
    }

    // --- Modified AI Game Loop ---
    private async void StartCPUGame()
    {
        Debug.Log("[GameManager] Starting CPU Game Loop...");
        if (brain is AIBrain aiBrain)
        {
            await aiBrain.ResetMemoryAsync();
        }

        while (currentState == GameState.Playing && this != null && isActiveAndEnabled)
        {
            if (brain == null) { Debug.LogError("[GameManager] AI Brain is null in game loop!"); break; }

            // --- Get AI Decision ---
            Decision decision = await brain.GetDecisionAsync();

            // --- Forced Trade Logic (for Debugging) ---
            bool tradeForced = false;
            if (forceAITradeOnTraderTile) // Check the debug flag
            {
                MapTerrain currentTerrain = player?.GetCurrentMapTerrain();
                if (currentTerrain != null && currentTerrain.hasTrader)
                {
                    // If on a trader tile, override the AI's decision and force a trade attempt
                    Debug.LogWarning($"[GameManager] DEBUG: Forcing AI Trade on tile {player.mapPosition}. Original decision was: {decision?.decisionType.ToString() ?? "Null"}");
                    decision = new Decision { decisionType = DecisionType.Trade }; // Create a trade decision
                    tradeForced = true;
                }
            }
            // --- End Forced Trade Logic ---

            // --- Apply Decision ---
            ApplyDecision(decision); // Apply the (potentially overridden) decision

            // --- Post-Action Updates ---
            if (CheckGameEndConditions()) break;

            // Only update vision/display *after* the action is fully processed
            // (TradeManager handles its own async flow, so don't delay here if trade was initiated)
            if (!tradeForced || decision.decisionType != DecisionType.Trade) // Avoid double updates if trade manager is running
            {
                 vision?.GenerateField();
                 UpdateDisplay();
            }

            // --- Delay ---
            // Add a delay unless a trade was just initiated (TradeManager has its own flow)
            if (!tradeForced || decision.decisionType != DecisionType.Trade)
            {
                 await Task.Delay(500); // Small delay between non-trade AI turns
            } else {
                 await Task.Yield(); // If trade started, just yield briefly to let TradeManager start
            }
        }
        Debug.Log("[GameManager] CPU Game Loop ended.");
    }

    private async void StartUserGame()
    {
        Debug.Log("[GameManager] Starting User Game Loop...");
        while (currentState == GameState.Playing && this != null && isActiveAndEnabled)
        {
             if (brain == null) { Debug.LogError("[GameManager] User Brain is null in game loop!"); break; }

             PrepareUserDecision();
             Decision decision = await brain.GetDecisionAsync();
             ApplyDecision(decision);

             if (CheckGameEndConditions()) break;

             vision?.GenerateField();
             UpdateDisplay();

             await Task.Yield();
        }
         Debug.Log("[GameManager] User Game Loop ended.");
    }

    private void PrepareUserDecision()
    {
        if (player == null || map == null) return;
        MapPosition mapPosition = player.mapPosition;
        MapTerrain currentTile = map.GetTile(mapPosition.x, mapPosition.y);
        int[,] neighbors = { { 0, 0 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 1 }, { 1, -1 }, { -1, -1 }, { -1, 1 } };

        for (int i = 0; i < neighbors.GetLength(0); i++)
        {
            MapTerrain tile = map.GetTile(mapPosition.x + neighbors[i, 0], mapPosition.y + neighbors[i, 1]);
            if (tile?.tile != null) tile.tile.SetSelectionFrameActive(true);
        }
        if (restButton != null) restButton.terrain = currentTile;
    }

    private void DisableUserDecisionSelection()
    {
         if (player == null || map == null) return;
         MapPosition mapPosition = player.mapPosition;
         int[,] neighbors = { { 0, 0 }, { 1, 0 }, { 0, 1 }, { -1, 0 }, { 0, -1 }, { 1, 1 }, { 1, -1 }, { -1, -1 }, { -1, 1 } };

         for (int i = 0; i < neighbors.GetLength(0); i++)
         {
             MapTerrain tile = map.GetTile(mapPosition.x + neighbors[i, 0], mapPosition.y + neighbors[i, 1]);
             if (tile?.tile != null) tile.tile.SetSelectionFrameActive(false);
         }
    }


    private void ApplyDecision(Decision decision)
    {
        if (player == null) { Debug.LogError("[GameManager] Cannot apply decision, player is null!"); return; }
        if (decision == null || decision.decisionType == DecisionType.Invalid)
        {
            Debug.LogWarning("[GameManager] Invalid or null decision received. Turn skipped?");
            return;
        }

        if (!IsAIPlayer()) DisableUserDecisionSelection(); // Disable UI for human after click

        switch (decision.decisionType)
        {
            case DecisionType.Move:
                Debug.Log($"[GameManager] Applying Move: {decision.direction}");
                player.Move(decision.direction);
                break;
            case DecisionType.Rest:
                Debug.Log("[GameManager] Applying Rest.");
                player.Rest();
                break;
            case DecisionType.Trade:
                Debug.Log("[GameManager] Applying Trade Decision (calling Player.AttemptTrade).");
                player.AttemptTrade(); // This calls TradeManager.StartTrade
                break;
        }
    }

    private bool CheckGameEndConditions()
    {
        if (player == null || map == null) return true;

        if (player.food <= 0) { GameOver("ran out of food"); return true; }
        if (player.water <= 0) { GameOver("ran out of water"); return true; }
        if (player.energy <= 0) { GameOver("ran out of energy"); return true; }
        if (player.mapPosition.x >= map.width - 1) { GameWin(); return true; }

        return false;
    }

    public void UpdateDisplay()
    {
        if (player != null)
        {
            foodDisplay.text = player.food.ToString();
            waterDisplay.text = player.water.ToString();
            energyDisplay.text = player.energy.ToString();
            goldDisplay.text = player.gold.ToString();

            MapTerrain currentTerrain = player.GetCurrentMapTerrain();
            if (currentTerrain != null)
            {
                tradeButton?.SetActive(currentTerrain.hasTrader); // Show if trader present
                if (restButton != null) restButton.terrain = currentTerrain;
            }
            else
            {
                tradeButton?.SetActive(false);
                if (restButton != null) restButton.terrain = null;
            }
        }
        else
        {
            foodDisplay.text = "-"; waterDisplay.text = "-"; energyDisplay.text = "-"; goldDisplay.text = "-";
            tradeButton?.SetActive(false);
            if (restButton != null) restButton.terrain = null;
        }
    }

    public void GameOver(string reason = "unknown causes")
    {
        if (currentState == GameState.GameOver) return;
        currentState = GameState.GameOver;
        Debug.Log($"--- GAME OVER --- Player died from {reason}.");
        // TODO: Implement game over screen, etc.
    }

     public void GameWin()
    {
        if (currentState == GameState.GameOver) return;
        currentState = GameState.GameOver;
        Debug.Log($"--- GAME WON --- Player reached the east edge!");
        // TODO: Implement win screen, etc.
    }


    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        currentState = GameState.GameOver;
    }

    private void OnApplicationQuit()
    {
        currentState = GameState.GameOver;
    }

    public void StopGame()
    {
        currentState = GameState.GameOver;
        Debug.Log("[GameManager] StopGame called.");
    }


    // -------------------------------------------------- //
    // --- TradeManager Helper Functions ---              //
    // -------------------------------------------------- //
    public Player GetPlayerReference()
    {
        if (player == null) Debug.LogError("[GameManager] GetPlayerReference called before player was initialized!");
        return player;
    }
    public bool IsAIPlayer()
    {
        if (GameConfig.instance != null) return GameConfig.instance.playerConfig.brainType == BrainType.CPU;
        Debug.LogError("[GameManager] IsAIPlayer check failed: GameConfig instance not found!");
        return false;
    }
    public AIBrain GetAIBrainReference()
    {
        if (IsAIPlayer())
        {
            if (brain != null && brain is AIBrain aiBrain) return aiBrain;
            Debug.LogError($"[GameManager] GetAIBrainReference: Should be AI, but brain instance is null or wrong type! Type: {(brain?.GetType().Name ?? "null")}");
            return null;
        }
        return null;
    }
    // -------------------------------------------------- //
    // --- End TradeManager Helper Functions ---          //
    // -------------------------------------------------- //

}
