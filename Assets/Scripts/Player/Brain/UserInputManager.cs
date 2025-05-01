using System.Threading.Tasks;
using UnityEngine;

public class UserInputManager : MonoBehaviour
{
    public static UserInputManager Instance;

    private TaskCompletionSource<Decision> decisionSource;

    private void Awake()
    {
        Instance = this;
    }

    public Task<Decision> WaitForDecisionAsync()
    {
        decisionSource = new TaskCompletionSource<Decision>();
        return decisionSource.Task;
    }

    public void MakeDecision(Decision decision)
    {
        if (decisionSource != null && !decisionSource.Task.IsCompleted)
        {
            decisionSource.SetResult(decision);
        }
    }
}
