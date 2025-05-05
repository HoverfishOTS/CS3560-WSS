using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SummaryManager : MonoBehaviour
{
    public static SummaryManager Instance;

    [SerializeField] private GameObject summaryScreen;

    [Header("Score Weights")]
    [SerializeField] private int turnsTakenWeight;
    [SerializeField] private int movesMadeWeight;
    [SerializeField] private int tradesMadeWeight;
    [SerializeField] private int restsMadeWeight;
    [SerializeField] private int foodCollectedWeight;
    [SerializeField] private int waterCollectedWeight;
    [SerializeField] private int goldCollectedWeight;
    [SerializeField] private int energyGainedWeight;
    [SerializeField] private int foodSpentWeight;
    [SerializeField] private int waterSpentWeight;
    [SerializeField] private int goldSpentWeight;
    [SerializeField] private int energySpentWeight;

    [Header("Text Displays")]
    [SerializeField] private TextMeshProUGUI turnsTakenText;
    [SerializeField] private TextMeshProUGUI movesMadeText;
    [SerializeField] private TextMeshProUGUI tradesMadeText;
    [SerializeField] private TextMeshProUGUI restsMadeText;
    [SerializeField] private TextMeshProUGUI foodCollectedText;
    [SerializeField] private TextMeshProUGUI waterCollectedText;
    [SerializeField] private TextMeshProUGUI goldCollectedText;
    [SerializeField] private TextMeshProUGUI energyGainedText;
    [SerializeField] private TextMeshProUGUI foodSpentText;
    [SerializeField] private TextMeshProUGUI waterSpentText;
    [SerializeField] private TextMeshProUGUI goldSpentText;
    [SerializeField] private TextMeshProUGUI energySpentText;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI titleText;

    [Header("Metrics")]
    public int turnsTaken;
    public int movesMade;
    public int tradesMade;
    public int restsMade;
    public int foodCollected;
    public int waterCollected;
    public int goldCollected;
    public int energyGained;
    public int foodSpent;
    public int waterSpent;
    public int goldSpent;
    public int energySpent;
    [SerializeField] private int score;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if(summaryScreen != null) summaryScreen.SetActive(false);
    }

    private int CalculateScore()
    {
        int result = 0;
        List<int> weights = new List<int> { 
            turnsTakenWeight, 
            movesMadeWeight, 
            tradesMadeWeight, 
            restsMadeWeight, 
            foodCollectedWeight, 
            waterCollectedWeight, 
            goldCollectedWeight, 
            energyGainedWeight, 
            foodSpentWeight, 
            waterSpentWeight, 
            goldSpentWeight, 
            energySpentWeight, 
        };
        List<int> metrics = new List<int> { 
            turnsTaken, 
            movesMade, 
            tradesMade, 
            restsMade, 
            foodCollected, 
            waterCollected, 
            goldCollected, 
            energyGained, 
            foodSpent, 
            waterSpent, 
            goldSpent, 
            energySpent, 
        };

        for (int i = 0; i < weights.Count; i++)
        {
            result += weights[i] * metrics[i] * 100;
        }

        return result;
    }

    public void DisplayScore(bool won)
    {
        score = Mathf.Max(0, CalculateScore());

        List<TextMeshProUGUI> texts = new List<TextMeshProUGUI> {
            turnsTakenText, 
            movesMadeText, 
            tradesMadeText, 
            restsMadeText, 
            foodCollectedText, 
            waterCollectedText, 
            goldCollectedText, 
            energyGainedText, 
            foodSpentText, 
            waterSpentText, 
            goldSpentText, 
            energySpentText, 
            scoreText
        };
        List<int> metrics = new List<int> {
            turnsTaken,
            movesMade,
            tradesMade,
            restsMade,
            foodCollected,
            waterCollected,
            goldCollected,
            energyGained,
            foodSpent,
            waterSpent,
            goldSpent,
            energySpent,
            score,
        };

        for(int i = 0;i < metrics.Count;i++)
        {
            if (texts[i] != null) texts[i].text = metrics[i].ToString();
        }

        if (titleText != null)
        {
            titleText.text = won ? "Victory" : "Game Over";
        }

        if(summaryScreen != null)
        {
            summaryScreen.SetActive(true);
        }
    }

    public void Restart()
    {
        // reload this scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void MainMenu()
    {
        // return to main menu
        SceneManager.LoadScene(0);
    }
}
