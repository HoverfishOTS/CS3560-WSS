using UnityEngine;

public class GameController : MonoBehaviour
{
    public Brain brain = new Brain();

    async void Start()
    {
        string decision = await brain.GetDecisionAsync(10, 5, 15, "food EAST, trader SOUTH");
        Debug.Log("AI Decision: " + decision);
    }
}
