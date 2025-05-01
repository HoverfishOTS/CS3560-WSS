using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RestButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MapTerrain terrain;

    public void OnRestButtonClicked()
    {
        UserInputManager.Instance.MakeDecision(new Decision
        {
            decisionType = DecisionType.Rest
        });
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StatEffectManager.Instance.ReflectMapTerrain(terrain);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StatEffectManager.Instance.ClearEffect("all");
    }
}
