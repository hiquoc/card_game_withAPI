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
        if (cardDrag != null && cardDrag.isDraggable && !rm.bm.isWaiting)
        {
            if (rm.bm.turn == 1)
            {
                Debug.Log("Is not your turn");
                return;
            }
            cardDrag.isDroppedValidZone = true;
            cardDrag.GetComponent<CardHover>().enabled = false;

            CardDisplay cardDisplay = cardDrag.GetComponent<CardDisplay>();
            if (cardDisplay.card.type == Card.CardType.minion)
            {
                /*bm.PlayCard(cardDisplay.gameObject);*/
                /*cardDrag.lastMinionHolderPos = cardDrag.minionHolderRT.position;*/
            }

            else
            {
                /*Debug.Log("SpellCard");*/

            }

            StartCoroutine(rm.bm.PlayCard(cardDisplay.gameObject));
            /*cm.UpdateCardPosition(0, cm.playerHand, cm.playerHandPosition);*/
        }
    }

}
