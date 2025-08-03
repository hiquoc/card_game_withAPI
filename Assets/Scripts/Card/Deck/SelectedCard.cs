using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectedCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,IPointerEnterHandler,IPointerExitHandler
{
    int originalIndex;
    public bool isDroppedOnValidZone = false;
    public int inventoryId;
    public int cardId;
    public int mana;
    public int attack, health;
    public GameObject cardInCollectionPre;
    bool isDragging=false;
    Coroutine coroutine;
    GameObject cardObj;
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
        isDragging = true;
        if (coroutine != null)
            coroutine = null;
        if(cardObj!=null)
            Destroy(cardObj);
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        StartCoroutine(OnEndDragCoroutine());
    }

    private IEnumerator OnEndDragCoroutine()
    {
        yield return null;
        ReferenceManager.Instance.removeFromDeckValidZone.SetActive(false);
        if (!isDroppedOnValidZone)
        {
            transform.SetParent(ReferenceManager.Instance.selectedCardPanel);
            transform.SetSiblingIndex(originalIndex);
            GetComponent<CanvasGroup>().blocksRaycasts = true;

        }
        else
        {
            Destroy(gameObject);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if(!isDragging)
            coroutine??=StartCoroutine(OnPointerEnterCoroutine());
    }
    IEnumerator OnPointerEnterCoroutine()
    {
        yield return new WaitForSeconds(1f);

        if (!RectTransformUtility.RectangleContainsScreenPoint(
            GetComponent<RectTransform>(),
            Input.mousePosition,
            null))
        {
            coroutine = null;
            yield break;
        }
        Debug.Log(cardId);
        if (!DeckManager.Instance.imgDict.ContainsKey(cardId)) yield break;
        Debug.Log(1);
        cardObj = Instantiate(cardInCollectionPre, ReferenceManager.Instance.animationLayer);
        cardObj.transform.localScale = new Vector3(2f, 2f, 2f);
        Destroy(cardObj.GetComponent<CardInInventory>());
        Image image = cardObj.transform.Find("Image").GetComponent<Image>();
        if (DeckManager.Instance.imgDict.TryGetValue(cardId, out Sprite sprite))
        {
            image.sprite = sprite;
        }
        if (attack >0||health>0)
        {
            Debug.Log(2);
            GameObject attackObj = cardObj.transform.Find("AttackImg").gameObject;
            attackObj.SetActive(true);
            attackObj.GetComponentInChildren<TMP_Text>().text = attack.ToString();
            GameObject healthObj = cardObj.transform.Find("HealthImg").gameObject;
            healthObj.SetActive(true);
            healthObj.GetComponentInChildren<TMP_Text>().text = health.ToString();
        }

        coroutine = null;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        if (cardObj != null)
        {
            Destroy(cardObj);
            cardObj = null;
        }
    }
}