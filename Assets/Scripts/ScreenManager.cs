using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenManager : MonoBehaviour
{
    [SerializeField] private GameObject[] screens;

    private int currentScreen = 0;

    private void Start()
    {
        // make sure the first screen is the only active screen
        for(int i = 0; i < screens.Length; i++)
        {
            switch (i)
            {
                case 0:
                    screens[i].SetActive(true);
                    break;
                default:
                    screens[i].SetActive(false);
                    break;
            }
        }
    }

    /// <summary>
    /// Sets the active screen to the specified index.
    /// </summary>
    /// <param name="index"></param>
    private void SetScreen(int index)
    {
        if (screens.Length == 0) return;

        if(index != currentScreen && currentScreen < screens.Length && index < screens.Length)
        {
            screens[currentScreen].SetActive(false);
            currentScreen = index;
            screens[currentScreen].SetActive(true);
        }
        else if (index == currentScreen)
        {
            // ensure the currentScreen is active
            screens[currentScreen].SetActive(true);
        }
    }

    /// <summary>
    /// Sets the active screen to the screen after the current screen.
    /// </summary>
    public void NextScreen()
    {
        if (currentScreen < screens.Length - 1)
        {
            SetScreen(currentScreen + 1);
        }
    }

    /// <summary>
    /// Sets the active screen to the screen before the current screen.
    /// </summary>
    public void PreviousScreen()
    {
        if (currentScreen > 0)
        {
            SetScreen(currentScreen - 1);
        }
    }

    public void Play()
    {
        SceneManager.LoadScene("Gameplay");

    }

}
