using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardInDeck : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int cardId;
    public string cardName;
    public int cardMana;
    GameObject copyObj;
    public bool isDropOnValidZone = false;
    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        //Add check if can drag this card

        copyObj = Instantiate(gameObject, ReferenceManager.Instance.animationLayer);
        RectTransform copyRT = copyObj.GetComponent<RectTransform>();
        RectTransform originalRT = GetComponent<RectTransform>();
        copyRT.sizeDelta = originalRT.rect.size;
        copyObj.transform.GetChild(0).gameObject.SetActive(false);
        cardId = 1;
        cardMana = 4;
        cardName = "Testing Card";
        copyObj.GetComponent<CanvasGroup>().blocksRaycasts = false;
        ReferenceManager.Instance.deckValidZone.SetActive(true);
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (copyObj == null) return;
        copyObj.transform.position = eventData.position;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (copyObj == null) return;
        StartCoroutine(OnEndDragCoroutine());

    }
    IEnumerator OnEndDragCoroutine()
    {
        yield return null;
        ReferenceManager.Instance.deckValidZone.SetActive(false);
        Destroy(copyObj);
    }
}
