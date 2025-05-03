using UnityEngine;
using System.Threading.Tasks;

public class HaggleBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject leftWidthReference;
    [SerializeField] private GameObject rightWidthReference;
    [SerializeField] private GameObject handle;
    [SerializeField] private GameObject goodFill;
    [SerializeField] private GameObject perfectFill;
    [SerializeField] private GameObject haggleButton;
    [SerializeField] private GameObject actionButtons;

    [Header("Result Thresholds")]
    [SerializeField] private float goodChance = 0.3f;
    [SerializeField] private float perfectChance = 0.05f;

    private bool haggling = false;
    private float speed;
    private bool movingRight = true;
    private TaskCompletionSource<string> resultSource;

    private float leftX;
    private float rightX;
    private float goodLeftX;
    private float goodRightX;
    private float perfectLeftX;
    private float perfectRightX;

    private void OnEnable()
    {
        float leftX = leftWidthReference.transform.position.x;
        float rightX = rightWidthReference.transform.position.x;
        float totalWidth = rightX - leftX;
        float centerX = (leftX + rightX) / 2f;

        float goodWidth = totalWidth * goodChance;
        RectTransform goodRT = goodFill.GetComponent<RectTransform>();
        goodRT.sizeDelta = new Vector2(goodWidth, goodRT.sizeDelta.y);

        Vector3 goodLocal = goodRT.transform.parent.InverseTransformPoint(new Vector3(centerX, 0f, 0f));
        goodRT.localPosition = new Vector3(goodLocal.x, goodRT.localPosition.y, goodRT.localPosition.z);

        goodLeftX = centerX - goodWidth / 2f;
        goodRightX = centerX + goodWidth / 2f;

        float perfectWidth = totalWidth * perfectChance;
        RectTransform perfectRT = perfectFill.GetComponent<RectTransform>();
        perfectRT.sizeDelta = new Vector2(perfectWidth, perfectRT.sizeDelta.y);

        Vector3 perfectLocal = perfectRT.transform.parent.InverseTransformPoint(new Vector3(centerX, 0f, 0f));
        perfectRT.localPosition = new Vector3(perfectLocal.x, perfectRT.localPosition.y, perfectRT.localPosition.z);

        perfectLeftX = centerX - perfectWidth / 2f;
        perfectRightX = centerX + perfectWidth / 2f;

        // Save bounds for handle movement
        this.leftX = leftX;
        this.rightX = rightX;
    }

    public async Task<string> StartHaggle(Trader trader)
    {
        // Calculate handle speed based on bar width
        float barWidth = rightWidthReference.transform.position.x - leftWidthReference.transform.position.x;
        float baseTimeToCross = 1.5f; // seconds
        speed = barWidth / baseTimeToCross;

        // Modify speed based on trader type
        switch (trader.traderType)
        {
            case "stingy":
                speed *= 1.5f; // harder
                break;
            case "normal":
                speed *= 1.1f;
                break;
            case "generous":
                speed *= 0.8f; // easier
                break;
        }

        haggling = true;
        resultSource = new TaskCompletionSource<string>();

        gameObject.SetActive(true);
        haggleButton.SetActive(true);
        actionButtons.SetActive(false);

        return await resultSource.Task;
    }

    public void StopHaggling()
    {
        haggling = false;

        float handleX = handle.transform.position.x;
        const float tolerance = 0.001f;

        if (handleX >= perfectLeftX - tolerance && handleX <= perfectRightX + tolerance)
            resultSource.SetResult("perfect");
        else if (handleX >= goodLeftX - tolerance && handleX <= goodRightX + tolerance)
            resultSource.SetResult("good");
        else
            resultSource.SetResult("fail");


        Debug.Log($"Handle X: {handleX:F4}");
        Debug.Log($"Perfect range: {perfectLeftX:F4} to {perfectRightX:F4}");
        Debug.Log($"Good range: {goodLeftX:F4} to {goodRightX:F4}");

        gameObject.SetActive(false);
        haggleButton.SetActive(false);
        actionButtons.SetActive(true);
    }

    private void Update()
    {
        if (haggling)
        {
            Vector3 pos = handle.transform.position;
            float step = speed * Time.deltaTime;

            if (movingRight)
            {
                pos.x += step;
                if (pos.x >= rightX)
                {
                    pos.x = rightX;
                    movingRight = false;
                }
            }
            else
            {
                pos.x -= step;
                if (pos.x <= leftX)
                {
                    pos.x = leftX;
                    movingRight = true;
                }
            }

            handle.transform.position = pos;
        }
    }
}
