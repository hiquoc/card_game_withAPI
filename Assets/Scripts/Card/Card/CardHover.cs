using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool canHover = true;
    private Vector2 originalPosition;
    private Vector3 originalRotation;
    private RectTransform rt;
    private int orinalIndex;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CardDrag.isDragging || !canHover) return;
        /*Debug.Log(1);*/
        rt = GetComponent<RectTransform>();
        originalPosition = rt.anchoredPosition;
        originalRotation = rt.localEulerAngles;
        orinalIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        rt.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + 45f);
        rt.localEulerAngles = Vector3.zero;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (CardDrag.isDragging || !canHover) return;
        /*Debug.Log("x");*/
        rt.anchoredPosition = originalPosition;
        rt.localEulerAngles = originalRotation;
        transform.SetSiblingIndex(orinalIndex);
    }
    public void ResetSiblingIndex()
    {
        /*Debug.Log(orinalIndex);*/
        transform.SetSiblingIndex(orinalIndex);
    }
}
