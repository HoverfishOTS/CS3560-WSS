using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;
using System; // Added for TimeSpan

public class AIBrain : IBrain
{
    // Keep the client static for performance (reuses connections)
    static readonly HttpClient client = new HttpClient();

    private Player player;
    private Map map;
    private Vision vision;

    // Static constructor to configure the client once
    static AIBrain()
    {
        // Set a longer timeout (e.g., 60 seconds)
        // Default is often 100 seconds, but let's be explicit.
        client.Timeout = TimeSpan.FromSeconds(60);
        // You might have already set ExpectContinue = false, keep it if needed
        client.DefaultRequestHeaders.ExpectContinue = false;
        Debug.Log("[AIBrain] HttpClient configured with 60 second timeout.");
    }


    public AIBrain(Player player, Map map, Vision vision)
    {
        this.player = player;
        this.map = map;
        this.vision = vision;
        // Configuration now happens in the static constructor
    }

    public async Task<Decision> GetDecisionAsync()
    {
        // Ensure player/map/vision are not null before proceeding
        if (player == null || map == null || vision == null)
        {
            Debug.LogError("[AIBrain] Cannot get decision: Player, Map, or Vision is null.");
            return new Decision { decisionType = DecisionType.Invalid }; // Return invalid decision
        }

        Debug.Log("[AIBrain] Sending decision request...");

        // Prepare payload
        PlayerState playerState = new PlayerState(player, map, vision);
        string jsonPayload = "";
        try
        {
             // Use Newtonsoft.Json for serialization consistency if PlayerState uses it
             // If PlayerState is simple and uses Unity's JsonUtility, keep that.
             // Assuming Newtonsoft for consistency with TradeManager examples:
             jsonPayload = JsonConvert.SerializeObject(playerState, Formatting.None, // Use None for smaller payload
                 new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }); // Ignore nulls
        }
        catch (Exception e)
        {
             Debug.LogError($"[AIBrain] Failed to serialize PlayerState: {e.Message}");
             return new Decision { decisionType = DecisionType.Invalid }; // Return invalid if serialization fails
        }

        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = null;
        string result = "";
        try
        {
            // Make the POST request
            // The timeout set in the static constructor applies here
            response = await client.PostAsync("http://localhost:5000/decide", content);

            // Read the response content
            result = await response.Content.ReadAsStringAsync();

            // Check for successful status code AFTER reading content (in case of error messages in body)
            response.EnsureSuccessStatusCode(); // Throws HttpRequestException for non-2xx status codes

            Debug.Log($"[AIBrain] Received response: {result}");

            // Deserialize response
            AIResponse responseData = JsonConvert.DeserializeObject<AIResponse>(result); // Use Newtonsoft
            if (responseData == null || string.IsNullOrEmpty(responseData.decision))
            {
                 Debug.LogError($"[AIBrain] Failed to parse decision from response or decision was empty: {result}");
                 return ParseDecision("REST"); // Default to REST on parse error
            }

            return ParseDecision(responseData.decision);
        }
        // Catch specific exception for timeout/cancellation
        catch (TaskCanceledException e)
        {
             // Check if it was due to timeout vs. explicit cancellation
             if (e.CancellationToken.IsCancellationRequested) {
                  Debug.LogWarning("[AIBrain] Decision request was canceled.");
             } else {
                  Debug.LogError($"[AIBrain] Decision request timed out ({client.Timeout.TotalSeconds} seconds). Is the Python server/LLM responding fast enough?\nError: {e.Message}");
             }
             return ParseDecision("REST"); // Default to REST on timeout
        }
        // Catch exceptions from EnsureSuccessStatusCode or network issues
        catch (HttpRequestException e)
        {
            Debug.LogError($"[AIBrain] Decision request failed: {response?.StatusCode} ({e.Message})\nResponse Body: {result}"); // Log body if available
            return ParseDecision("REST"); // Default to REST on HTTP error
        }
        // Catch JSON parsing errors
        catch (JsonException e)
        {
             Debug.LogError($"[AIBrain] Failed to parse JSON response: {e.Message}\nResponse Body: {result}");
             return ParseDecision("REST"); // Default to REST on JSON error
        }
        // Catch any other unexpected errors
        catch (Exception e)
        {
             Debug.LogError($"[AIBrain] Unexpected error during decision request: {e.Message}\n{e.StackTrace}");
             return ParseDecision("REST"); // Default to REST on other errors
        }
    }


    private Decision ParseDecision(string raw)
    {
        if (string.IsNullOrEmpty(raw))
        {
             Debug.LogWarning("[AIBrain] ParseDecision received null or empty string, defaulting to REST.");
             return new Decision { decisionType = DecisionType.Rest }; // Default to Rest if parsing empty
        }


        string upperRaw = raw.Trim().ToUpperInvariant(); // Trim whitespace and convert to upper

        if (upperRaw.StartsWith("MOVE ")) // Check for space after MOVE
        {
            string[] parts = upperRaw.Split(new[] { ' ' }, 2, StringSplitOptions.RemoveEmptyEntries); // Split only once
            if (parts.Length == 2 && !string.IsNullOrEmpty(parts[1]))
            {
                // Basic validation for direction? Could add check against known directions.
                return new Decision { decisionType = DecisionType.Move, direction = parts[1] };
            }
            else
            {
                 Debug.LogWarning($"[AIBrain] Invalid MOVE format received: '{raw}'. Defaulting to REST.");
                 return new Decision { decisionType = DecisionType.Rest };
            }
        }
        else if (upperRaw == "REST")
        {
            return new Decision { decisionType = DecisionType.Rest };
        }
        else if (upperRaw == "TRADE")
        {
            return new Decision { decisionType = DecisionType.Trade };
        }
        else // Unrecognized action
        {
            Debug.LogWarning($"[AIBrain] Unrecognized decision string: '{raw}'. Defaulting to REST.");
            return new Decision { decisionType = DecisionType.Rest }; // Default to Rest for safety
        }
    }

    // Keep using Newtonsoft for consistency if PlayerState uses it
    [System.Serializable]
    private class AIResponse
    {
        public string decision;
        // Add error field if Python might send structured errors
        // public string error;
    }

    public async Task ResetMemoryAsync()
    {
        try
        {
            // Use a separate timeout for reset? Or rely on the default? Default is fine usually.
            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/reset", null); // No content needed
            if (response.IsSuccessStatusCode)
            {
                Debug.Log("[AIBrain] AI memory reset successfully.");
            }
            else
            {
                string errorBody = await response.Content.ReadAsStringAsync();
                Debug.LogWarning($"[AIBrain] Memory reset request failed: {response.StatusCode}\nResponse: {errorBody}");
            }
        }
        catch (TaskCanceledException e) // Handle timeout specifically
        {
             Debug.LogError($"[AIBrain] Memory reset request timed out or canceled: {e.Message}");
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"[AIBrain] Could not contact AI server for memory reset: {e.Message}");
        }
         catch (Exception e) // Catch unexpected errors
        {
             Debug.LogError($"[AIBrain] Unexpected error during memory reset: {e.Message}\n{e.StackTrace}");
        }
    }

    // --- GetTradeDecisionAsync method from previous steps should be here ---
    // Make sure it also uses the static client instance configured with the timeout
    public async Task<string> GetTradeDecisionAsync(Player player, Trader trader, TradeOffer currentOffer)
    {
        Debug.Log("[AIBrain] Requesting trade decision...");
        if (player == null || trader == null) {
             Debug.LogError("[AIBrain] GetTradeDecisionAsync: Player or Trader is null.");
             return "REJECT";
        }

        // Prepare payload (using classes defined previously or inline anonymous types)
        var playerStats = new { player_food = player.food, player_water = player.water, player_gold = player.gold, player_max_food = player.maxFood, player_max_water = player.maxWater };
        var traderInfo = new { trader_type = trader.traderType, trader_food_stock = trader.foodStock, trader_water_stock = trader.waterStock };
        var payload = new { player_stats = playerStats, trader_info = traderInfo, current_offer = currentOffer }; // Pass currentOffer (can be null)

        string jsonPayload = JsonConvert.SerializeObject(payload, Formatting.None, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Include });
        Debug.Log($"[AIBrain] Sending Trade Payload:\n{jsonPayload}");
        var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = null;
        string result = "";
        try
        {
            // Use the same static client with the configured timeout
            response = await client.PostAsync("http://localhost:5000/trade_decide", content);
            result = await response.Content.ReadAsStringAsync();
            response.EnsureSuccessStatusCode(); // Check for HTTP errors

            Debug.Log($"[AIBrain] Received Trade Response: {result}");
            TradeResponse responseData = JsonConvert.DeserializeObject<TradeResponse>(result);

            if (responseData != null && !string.IsNullOrEmpty(responseData.trade_action))
            {
                return responseData.trade_action; // Return the action string (ACCEPT/REJECT/COUNTER...)
            }
            else if (responseData != null && !string.IsNullOrEmpty(responseData.error)) {
                 Debug.LogError($"[AIBrain] Trade decision error from Python: {responseData.error}");
                 return "REJECT";
            } else {
                 Debug.LogError($"[AIBrain] Failed to parse trade response JSON or action missing: {result}");
                 return "REJECT";
            }
        }
        catch (TaskCanceledException e) { Debug.LogError($"[AIBrain] Trade decision request timed out or canceled ({client.Timeout.TotalSeconds}s): {e.Message}"); return "REJECT"; }
        catch (HttpRequestException e) { Debug.LogError($"[AIBrain] Trade decision request failed: {response?.StatusCode} ({e.Message})\nResponse: {result}"); return "REJECT"; }
        catch (JsonException e) { Debug.LogError($"[AIBrain] Failed to parse trade JSON response: {e.Message}\nResponse: {result}"); return "REJECT"; }
        catch (Exception e) { Debug.LogError($"[AIBrain] Unexpected error during trade decision request: {e.Message}\n{e.StackTrace}"); return "REJECT"; }
    }

    // --- Helper classes for JSON serialization/deserialization ---
    // (Keep the PlayerTradeStats, TraderTradeInfo, TradeRequestPayload, TradeResponse classes here if defined inline previously)
    // Example:
    [System.Serializable]
    private class TradeResponse
    {
        public string trade_action;
        public string error;
    }

}
