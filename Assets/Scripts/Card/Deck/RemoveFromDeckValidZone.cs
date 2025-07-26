using UnityEngine;
using UnityEngine.EventSystems;

public class RemoveFromDeckValidZone : MonoBehaviour, IDropHandler
{
    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        GameObject eventObj = eventData.pointerDrag;
        if (eventObj.TryGetComponent(out SelectedCard selectedCard))
        {
            selectedCard.GetComponent<SelectedCard>().isDroppedOnValidZone = true;

            DeckManager.Instance.numOfCard.text = DeckManager.Instance.deck.Count.ToString() + "/30";
        }
    }
}