using UnityEngine;
using UnityEngine.EventSystems;

public class ValidZone : MonoBehaviour, IDropHandler
{
    public static ValidZone Instance;
    ReferenceManager rm;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rm = ReferenceManager.Instance;
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        CardDrag cardDrag = eventData.pointerDrag.GetComponent<CardDrag>();
        if (cardDrag != null && cardDrag.isDraggable && CardDrag.canDrag && !rm.bm.isWaiting)
        {
            if (rm.bm.turn == 1)
            {
                Debug.Log("Is not your turn");
                return;
            }
            CardDisplay cardDisplay = cardDrag.GetComponent<CardDisplay>();
            if (cardDisplay.card is MinionCard && rm.bm.playerMinionList.Count > 5)
            {
                Debug.Log("Your board is full");
                return;
            }
            cardDrag.isDroppedValidZone = true;
            cardDrag.GetComponent<CardHover>().enabled = false;


            StartCoroutine(rm.bm.PlayCard(cardDisplay.gameObject));
            /*cm.UpdateCardPosition(0, cm.playerHand, cm.playerHandPosition);*/
        }
    }

}
