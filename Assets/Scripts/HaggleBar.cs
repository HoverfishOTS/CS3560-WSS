using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Handles the haggle minigame UI logic, including handle movement and result detection.
/// </summary>
public class HaggleBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject leftWidthReference;  // World position marking the left edge of the bar
    [SerializeField] private GameObject rightWidthReference; // World position marking the right edge of the bar
    [SerializeField] private GameObject handle;              // The moving handle the player tries to stop
    [SerializeField] private GameObject goodFill;            // The "good" target region
    [SerializeField] private GameObject perfectFill;         // The "perfect" target region
    [SerializeField] private GameObject haggleButton;        // Button to stop the handle
    [SerializeField] private GameObject actionButtons;       // UI buttons to show when haggling ends

    [Header("Result Thresholds")]
    [SerializeField] private float goodChance = 0.3f;        // Percentage of bar width considered "good"
    [SerializeField] private float perfectChance = 0.05f;    // Percentage of bar width considered "perfect"

    private bool haggling = false;                           // Whether the handle is currently moving
    private float speed;                                     // Current handle speed (units/sec)
    private bool movingRight = true;                         // Direction of handle movement
    private TaskCompletionSource<string> resultSource;       // Task used to return haggle result asynchronously

    // World-space x positions for movement and scoring
    private float leftX;
    private float rightX;
    private float goodLeftX;
    private float goodRightX;
    private float perfectLeftX;
    private float perfectRightX;

    /// <summary>
    /// Called automatically when the object is enabled. Initializes bar dimensions and fill areas.
    /// </summary>
    private void OnEnable()
    {
        // Get the left and right bounds of the haggle bar in world space
        float leftX = leftWidthReference.transform.position.x;
        float rightX = rightWidthReference.transform.position.x;
        float totalWidth = rightX - leftX;
        float centerX = (leftX + rightX) / 2f;

        // Set the width of the good fill based on percentage
        float goodWidth = totalWidth * goodChance;
        RectTransform goodRT = goodFill.GetComponent<RectTransform>();
        goodRT.sizeDelta = new Vector2(goodWidth, goodRT.sizeDelta.y);

        // Position the good fill centered in the bar
        Vector3 goodLocal = goodRT.transform.parent.InverseTransformPoint(new Vector3(centerX, 0f, 0f));
        goodRT.localPosition = new Vector3(goodLocal.x, goodRT.localPosition.y, goodRT.localPosition.z);

        // Store world-space bounds for scoring
        goodLeftX = centerX - goodWidth / 2f;
        goodRightX = centerX + goodWidth / 2f;

        // Repeat the same for the perfect fill
        float perfectWidth = totalWidth * perfectChance;
        RectTransform perfectRT = perfectFill.GetComponent<RectTransform>();
        perfectRT.sizeDelta = new Vector2(perfectWidth, perfectRT.sizeDelta.y);

        Vector3 perfectLocal = perfectRT.transform.parent.InverseTransformPoint(new Vector3(centerX, 0f, 0f));
        perfectRT.localPosition = new Vector3(perfectLocal.x, perfectRT.localPosition.y, perfectLocal.z);

        perfectLeftX = centerX - perfectWidth / 2f;
        perfectRightX = centerX + perfectWidth / 2f;

        // Save the movement limits
        this.leftX = leftX;
        this.rightX = rightX;
    }

    /// <summary>
    /// Begins the haggle game and returns a Task that completes when the player presses the haggle button.
    /// </summary>
    public async Task<string> StartHaggle(Trader trader)
    {
        // Calculate how fast the handle should move based on bar width
        float barWidth = rightWidthReference.transform.position.x - leftWidthReference.transform.position.x;
        float baseTimeToCross = 1.5f; // Time in seconds for the handle to go across the bar
        speed = barWidth / baseTimeToCross;

        // Adjust difficulty based on trader type
        switch (trader.traderType)
        {
            case "stingy":
                speed *= 1.5f; // Faster = harder
                break;
            case "normal":
                speed *= 1.1f;
                break;
            case "generous":
                speed *= 0.8f; // Slower = easier
                break;
        }

        // Start haggling loop
        haggling = true;
        resultSource = new TaskCompletionSource<string>();

        // Show UI
        gameObject.SetActive(true);
        haggleButton.SetActive(true);
        actionButtons.SetActive(false);

        // Wait until StopHaggling() is called and returns a result
        return await resultSource.Task;
    }

    /// <summary>
    /// Called when the player presses the haggle button to stop the handle and evaluate the result.
    /// </summary>
    public void StopHaggling()
    {
        haggling = false;

        // Get the x position of the handle in world space
        float handleX = handle.transform.position.x;

        // Small tolerance to prevent floating point rounding errors from failing borderline results
        const float tolerance = 0.001f;

        // Evaluate result by comparing handle position to perfect/good fill bounds
        if (handleX >= perfectLeftX - tolerance && handleX <= perfectRightX + tolerance)
            resultSource.SetResult("perfect");
        else if (handleX >= goodLeftX - tolerance && handleX <= goodRightX + tolerance)
            resultSource.SetResult("good");
        else
            resultSource.SetResult("fail");

        // Debug log for visual calibration
        Debug.Log($"Handle X: {handleX:F4}");
        Debug.Log($"Perfect range: {perfectLeftX:F4} to {perfectRightX:F4}");
        Debug.Log($"Good range: {goodLeftX:F4} to {goodRightX:F4}");

        // Hide haggle UI and show other buttons
        gameObject.SetActive(false);
        haggleButton.SetActive(false);
        actionButtons.SetActive(true);
    }

    /// <summary>
    /// Moves the handle back and forth while haggling is active.
    /// </summary>
    private void Update()
    {
        if (haggling)
        {
            // Move the handle
            Vector3 pos = handle.transform.position;
            float step = speed * Time.deltaTime;

            if (movingRight)
            {
                pos.x += step;

                // Reverse direction if it hits the right bound
                if (pos.x >= rightX)
                {
                    pos.x = rightX;
                    movingRight = false;
                }
            }
            else
            {
                pos.x -= step;

                // Reverse direction if it hits the left bound
                if (pos.x <= leftX)
                {
                    pos.x = leftX;
                    movingRight = true;
                }
            }

            // Apply new position
            handle.transform.position = pos;
        }
    }
}
