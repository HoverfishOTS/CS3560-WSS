using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TooltipManager : MonoBehaviour
{
    public static TooltipManager Instance;

    [Header("Tooltip UI Elements")]
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private GameObject tooltip;
    [SerializeField] private GameObject screenDetector;
    [SerializeField] private GameObject detectorBorder;

    private bool trackCursor = false;

    private RectTransform rectTransform;
    private Tooltip tooltipScript;

    private Vector2 localOffset;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if(tooltipObject != null)
        {
            rectTransform = tooltipObject.GetComponent<RectTransform>();
            tooltipScript = tooltipObject.GetComponent<Tooltip>();

            localOffset = tooltip.transform.localPosition;
        }

        HideTooltip();
    }

    private void Update()
    {
        if (trackCursor)
        {
            Vector2 cursorPositon = Input.mousePosition;
            rectTransform.position = cursorPositon;

            // keep the tooltip on screen
            if(screenDetector != null)
            {
                Vector2 newOffset = Vector2.zero;
                newOffset = localOffset * ((screenDetector.transform.position.x > detectorBorder.transform.position.x) ? Vector2.left : Vector2.right);
                newOffset += localOffset * ((screenDetector.transform.position.y < detectorBorder.transform.position.y) ? Vector2.down : Vector2.up);

                tooltip.transform.localPosition = newOffset;
            }
        }
    }

    public void ShowTooltip(MapTerrain terrain)
    {
        tooltipScript.UpdateInfo(terrain);

        trackCursor = true;

        if (tooltipObject != null)
            tooltipObject.SetActive(true);
    }

    public void HideTooltip()
    {
        if (tooltipObject != null)
            tooltipObject.SetActive(false);

        trackCursor = false;
    }
}
