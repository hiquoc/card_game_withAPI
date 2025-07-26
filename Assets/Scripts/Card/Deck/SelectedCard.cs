using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectedCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    int originalIndex;
    public bool isDroppedOnValidZone = false;
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        originalIndex = gameObject.transform.GetSiblingIndex();
        transform.SetParent(ReferenceManager.Instance.animationLayer);

        GetComponent<CanvasGroup>().blocksRaycasts = false;
        ReferenceManager.Instance.removeFromDeckValidZone.SetActive(true);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(OnEndDragCoroutine());
    }

    private IEnumerator OnEndDragCoroutine()
    {
        yield return null;
        ReferenceManager.Instance.removeFromDeckValidZone.SetActive(false);
        if (!isDroppedOnValidZone)
        {
            transform.SetParent(DeckValidZone.Instance.selectedCardPanel);
            transform.SetSiblingIndex(originalIndex);
            GetComponent<CanvasGroup>().blocksRaycasts = true;

        }
        else
        {
            Destroy(gameObject);
        }
    }
}