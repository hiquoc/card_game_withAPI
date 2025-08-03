using UnityEngine;
using UnityEngine.EventSystems;

public class RemoveFromDeckValidZone : MonoBehaviour, IDropHandler
{
    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        GameObject eventObj = eventData.pointerDrag;
        if (eventObj.TryGetComponent(out SelectedCard selectedCard))
        {
            selectedCard.isDroppedOnValidZone = true;
            DeckManager dm = DeckManager.Instance;
            Debug.Log(selectedCard.inventoryId);
            dm.deck.Remove(selectedCard.inventoryId);
            dm.manaDict[selectedCard.inventoryId] = selectedCard.mana;
            dm.numOfCard.text = DeckManager.Instance.deck.Count.ToString() + "/30";
        }
    }
}