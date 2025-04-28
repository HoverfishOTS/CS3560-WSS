using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Brain
{
    static readonly HttpClient client = new HttpClient();

    public async Task<string> GetDecisionAsync(int food, int water, int energy, string nearbyInfo)
    {
        string json = JsonUtility.ToJson(new GameState
        {
            food = food,
            water = water,
            energy = energy,
            nearby = nearbyInfo
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("http://localhost:5000/decide", content);
        string result = await response.Content.ReadAsStringAsync();

        AIResponse responseData = JsonUtility.FromJson<AIResponse>(result);
        return responseData.decision;
    }

    /// <summary>
    /// Called by traders when counter offer is proposed.
    /// </summary>
    /// <returns></returns>
    public async Task<string> GetTradeDecisionAsync(string input, int inputCount, string output, int outputCount, string traderType)
    {
        string json = JsonUtility.ToJson(new TradeCounterOffer
        {
            input = input,
            inputCount = inputCount,
            output = output,
            outputCount = outputCount,
            traderType = traderType
        });

        var content = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage response = await client.PostAsync("http://localhost:5000/trade", content);
        string result = await response.Content.ReadAsStringAsync();

        AIResponse responseData = JsonUtility.FromJson<AIResponse>(result);
        return responseData.decision;
    }

    [System.Serializable]
    public class GameState
    {
        public int food;
        public int water;
        public int energy;
        public string nearby;
    }

    [System.Serializable]
    public class TradeCounterOffer
    {
        public string input;
        public int inputCount;
        public string output;
        public int outputCount;
        public string traderType;
    }

    [System.Serializable]
    public class AIResponse
    {
        public string decision;
    }
}
