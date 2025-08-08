using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static bool canHover = true;
    bool hasEnter = false;
    public bool isEnemyCard = false;
    private Vector2 originalPosition;
    private Vector3 originalRotation;
    private RectTransform rt;
    private int originalIndex;
    private GameObject cardObj;
    Coroutine coroutine;
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (CardDrag.isDragging || !canHover||isEnemyCard) return;
        /*Debug.Log(1);*/
        hasEnter = true;
        originalPosition = rt.anchoredPosition;
        originalRotation = rt.localEulerAngles;
        originalIndex = transform.GetSiblingIndex();
        transform.SetAsLastSibling();
        rt.anchoredPosition = new Vector2(originalPosition.x, originalPosition.y + 45f);
        rt.localEulerAngles = Vector3.zero;

        //Phong to card
        if (cardObj != null) return;
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        coroutine = StartCoroutine(ShowBaseCardCoroutine());

    }
    IEnumerator ShowBaseCardCoroutine()
    {
        yield return new WaitForSeconds(1f);
        cardObj = Instantiate(rt.gameObject, ReferenceManager.Instance.blurPanel);
        RectTransform rectTransform = cardObj.GetComponent<RectTransform>();
        rectTransform.position = new Vector3(350, 560);
        rectTransform.localScale = new Vector3(2f, 2f, 2f);
        coroutine = null;
        ReferenceManager.Instance.blurPanel.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ReferenceManager.Instance.blurPanel.gameObject.SetActive(false);
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }
        if (cardObj != null)
            Destroy(cardObj);
        if (CardDrag.isDragging || !canHover || !hasEnter || rt == null) return;
        /*Debug.Log(2);*/
        rt.anchoredPosition = originalPosition;
        rt.localEulerAngles = originalRotation;
        transform.SetSiblingIndex(originalIndex);
    }
    public void ResetSiblingIndex()
    {
        /*Debug.Log(orinalIndex);*/
        transform.SetSiblingIndex(originalIndex);
    }
}
