using DG.Tweening;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static bool canDrag = true; //For making card undraggable during target selection
    public static bool isDragging = false;
    public bool isMinionCard = false;
    public bool isDraggable = true;
    RectTransform rt;
    Transform originalParent;
    Vector3 originalPosition;
    Vector3 originalRotation;
    int originalIndex;

    //public static int lastMinionIndex = -1;
    public GameObject minionHolderObj;
    public RectTransform minionHolderRT;
    /*public Vector3 lastMinionHolderPos;*/

    CardHover cardHover;
    CanvasGroup canvasGroup;
    ReferenceManager rm;

    public bool isDroppedValidZone = false;
    private void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        cardHover = GetComponent<CardHover>();
        rm = ReferenceManager.Instance;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        /*Debug.Log(2);*/
        if (!isDraggable || !canDrag) return;

        canvasGroup.blocksRaycasts = false;
        isDragging = true;
        ReferenceManager.Instance.validZone.SetActive(true);
        originalPosition = transform.position;
        originalRotation = rt.localEulerAngles;
        rt.localEulerAngles = Vector3.zero;
        originalIndex = transform.GetSiblingIndex();

        originalParent = transform.parent;

        transform.SetAsLastSibling();
        if (isMinionCard)
        {
            minionHolderObj = Instantiate(rm.minionHolderPrefab, rm.playerMinionPosition);
            minionHolderRT = minionHolderObj.GetComponent<RectTransform>();
        }
    }


    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        /*Debug.Log(3);*/
        if (!isDragging) return;
        rt.position = eventData.position;

    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        /*Debug.Log(3.1);*/
        if (!isDragging) return;
        /*Debug.Log(3.2);*/
        isDragging = false;
        cardHover.ResetSiblingIndex();
        canvasGroup.blocksRaycasts = true;
        StartCoroutine(OnEndDragCoroutine());

    }
    IEnumerator OnEndDragCoroutine()
    {
        yield return null;

        /*Debug.Log($"[OnEndDrag] isDroppedValidZone = {isDroppedValidZone}");*/

        if (!isDroppedValidZone)
        {
            /*Debug.Log(4);*/
            /*Debug.Log("[OnEndDrag] Card dropped in invalid zone, resetting.");*/
            rt.SetParent(originalParent, false);
            transform.position = originalPosition;
            rt.localEulerAngles = originalRotation;
            transform.SetSiblingIndex(originalIndex);
            Destroy(minionHolderObj);
        }
        else
        {
            /*Debug.Log("[OnEndDrag] Card successfully dropped on valid zone.");*/
            /*isDragging = false;
            isDroppedValidZone = false;
            ReferenceManager.Instance.validZone.SetActive(false);*/
        }

        yield return null;
        isDragging = false;
        isDroppedValidZone = false;
        cardHover.ResetSiblingIndex();

        CardManager cm = CardManager.Instance;
        cm.UpdateCardPosition(0, cm.playerHand, cm.playerHandPosition);

    }

    //Di chuyen the bai
    public IEnumerator MoveSpellCard()
    {
        Vector2 screenCenter = new((Screen.width + 100f) / 2f, Screen.height / 2f);
        ReferenceManager rm = ReferenceManager.Instance;
        transform.SetParent(rm.animationLayer);
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOMove(screenCenter, 0.8f));
        seq.Join(rt.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f));
        yield return seq.WaitForCompletion();
    }
    public IEnumerator MoveMinionCard()
    {
        ReferenceManager rm = ReferenceManager.Instance;
        transform.SetParent(rm.animationLayer);
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOMove(minionHolderRT.position, 0.8f));
        seq.Join(rt.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f));
        yield return seq.WaitForCompletion();
    }

}