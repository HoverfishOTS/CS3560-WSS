using System.Threading.Tasks;
using UnityEngine;

public class UserBrain : IBrain
{
    private Player player;
    private Map map;
    private Vision vision;

    public UserBrain(Player player, Map map, Vision vision)
    {
        this.player = player;
        this.map = map;
        this.vision = vision;
    }

    public async Task<Decision> GetDecisionAsync()
    {
        // Placeholder: Insert player input handling here
        Debug.Log("[UserBrain] Awaiting player input...");

        return await UserInputManager.Instance?.WaitForDecisionAsync();

        /*
        await Task.Delay(100); // Fake small delay to simulate frame check

        return new Decision { decisionType = DecisionType.Invalid };
        */
    }

}
