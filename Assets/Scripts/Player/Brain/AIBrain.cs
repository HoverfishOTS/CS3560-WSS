using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Newtonsoft.Json;

public class AIBrain : IBrain
{
    static readonly HttpClient client = new HttpClient();

    private Player player;
    private Map map;
    private Vision vision;

    public AIBrain(Player player, Map map, Vision vision)
    {
        this.player = player;
        this.map = map;
        this.vision = vision;
        client.DefaultRequestHeaders.ExpectContinue = false;
    }

    public async Task<Decision> GetDecisionAsync()
    {
        Debug.Log("[AIBrain] Sending decision request...");
        
        PlayerState playerState = new PlayerState(player, map, vision);
        string json = JsonConvert.SerializeObject(playerState);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync("http://localhost:5000/decide", content);
        string result = await response.Content.ReadAsStringAsync();

        Debug.Log("[AIBrain] Received response.");

        AIResponse responseData = JsonUtility.FromJson<AIResponse>(result);
        return ParseDecision(responseData.decision);
    }


    private Decision ParseDecision(string raw)
    {
        if (string.IsNullOrEmpty(raw))
            return new Decision { decisionType = DecisionType.Invalid };

        raw = raw.ToUpper();

        if (raw.StartsWith("MOVE"))
        {
            string[] parts = raw.Split(' ');
            if (parts.Length >= 2)
            {
                return new Decision { decisionType = DecisionType.Move, direction = parts[1] };
            }
        }
        else if (raw == "REST")
        {
            return new Decision { decisionType = DecisionType.Rest };
        }
        else if (raw == "TRADE")
        {
            return new Decision { decisionType = DecisionType.Trade };
        }

        return new Decision { decisionType = DecisionType.Invalid };
    }

    [System.Serializable]
    private class AIResponse
    {
        public string decision;
    }

    public async Task ResetMemoryAsync()
    {
        try
        {
            HttpResponseMessage response = await client.PostAsync("http://localhost:5000/reset", null);
            if (response.IsSuccessStatusCode)
            {
                Debug.Log("[AIBrain] AI memory reset successfully.");
            }
            else
            {
                Debug.LogWarning($"[AIBrain] Memory reset failed: {response.StatusCode}");
            }
        }
        catch (HttpRequestException e)
        {
            Debug.LogError($"[AIBrain] Could not contact AI server: {e.Message}");
        }
    }

}
