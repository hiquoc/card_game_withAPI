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
        if (!isDraggable || !canDrag||rm.bm.turn==1) return;

        canvasGroup.blocksRaycasts = false;
        isDragging = true;
        ReferenceManager.Instance.validZone.SetActive(true);
        originalPosition = transform.position;
        originalRotation = rt.localEulerAngles;
        rt.localEulerAngles = Vector3.zero;
        originalIndex = transform.GetSiblingIndex();

        originalParent = transform.parent;

        transform.SetAsLastSibling();
        if (isMinionCard && rm.bm.playerMinionList.Count < 6)
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

        rm.cm.UpdateCardPosition(0);

    }

    //Di chuyen the bai
    public IEnumerator MoveSpellCard(bool canHover)
    {
        canDrag = false;
        CardHover.canHover = canHover;
        Vector2 screenCenter = new((Screen.width + 100f) / 2f, Screen.height / 2f);
        ReferenceManager rm = ReferenceManager.Instance;
        transform.SetParent(rm.animationLayer);
        rt.localEulerAngles = new Vector2(0f, 0f);
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOMove(screenCenter, 0.8f));
        seq.Join(rt.DOScale(new Vector3(2f, 2f, 2f), 0.2f));
        gameObject.TryGetComponent(out FlipCard flipCard);
        if (flipCard != null && !flipCard.flipped)
        {
            seq.AppendCallback(() => flipCard.Flip());
            seq.AppendInterval(1f);
        }

        rm.cm.UpdateCardPosition(rm.bm.turn);

        yield return seq.WaitForCompletion();
        yield return new WaitForSeconds(1f);
        canDrag = true;
        CardHover.canHover = true;
    }
    public IEnumerator MoveMinionCard(bool canHover)
    {
        /*Debug.Log("StartMove" +canHover);*/
        canDrag = false;
        CardHover.canHover = canHover;
        ReferenceManager rm = ReferenceManager.Instance;
        transform.SetParent(rm.animationLayer);
        rt.localEulerAngles = new Vector2(0f, 0f);
        Sequence seq = DOTween.Sequence();
        seq.Append(rt.DOMove(minionHolderRT.position, 0.8f));
        seq.Join(rt.DOScale(new Vector3(2f, 2f, 2f), 0.2f));
        gameObject.TryGetComponent(out FlipCard flipCard);

        rm.cm.UpdateCardPosition(rm.bm.turn);

        if (flipCard != null && !flipCard.flipped)
        {
            seq.AppendCallback(() => flipCard.Flip());
            seq.AppendInterval(1f);
        }
        yield return seq.WaitForCompletion();
        yield return new WaitForSeconds(1f);
        canDrag = true;
        CardHover.canHover = true;
        /*Debug.Log("EndMove");*/
    }

}