using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckValidZone : MonoBehaviour, IDropHandler
{
    public GameObject selectedCardPre;
    public RectTransform selectedCardPanel;


    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        DeckManager dm = DeckManager.Instance;
        if (dm.deck.Count == 30)
        {
            dm.ShowErrorPanel(false,"You cant add more to the deck");
            return;
        }
        GameObject eventObj = eventData.pointerDrag;
        if (eventObj.TryGetComponent(out CardInInventory cardInInventory))
        {
            int owned = cardInInventory.quantity;
            int inDeck = 0;
            for (int i = 0; i < selectedCardPanel.transform.childCount; i++) { 
                SelectedCard selected=selectedCardPanel.transform.GetChild(i).GetComponent<SelectedCard>();
                if(cardInInventory.cardId==selected.cardId)
                    inDeck++;
            }

           /* Debug.Log(dm.deck.Count);
            Debug.Log(owned);
            Debug.Log(inDeck);*/
            if (inDeck >= owned)
            {
                dm.ShowErrorPanel(false,"You don't have more copy of that card");
                return;
            }
            GameObject selectedCardObj = Instantiate(selectedCardPre);
            Transform manaTF = selectedCardObj.transform.Find("Mana");
            if (manaTF != null)
                manaTF.GetComponentInChildren<TMP_Text>().text = cardInInventory.cardMana.ToString();
            Transform nameTR = selectedCardObj.transform.Find("NameText");
            nameTR.GetComponent<TMP_Text>().text = cardInInventory.cardName;
            Debug.Log(cardInInventory.cardName);
            selectedCardObj.transform.SetParent(selectedCardPanel);
            SelectedCard selectedCard = selectedCardObj.GetComponent<SelectedCard>();
            if (cardInInventory.attack > 0 || cardInInventory.health > 0)
            {
                selectedCard.attack=cardInInventory.attack;
                selectedCard.health=cardInInventory.health;
            }
            selectedCard.mana = cardInInventory.cardMana;
            selectedCard.cardId= cardInInventory.cardId;
            selectedCard.inventoryId= cardInInventory.inventoryId;
            dm.deck.Add(dm.GetInventoryId(cardInInventory.cardId));
            dm.manaDict[cardInInventory.inventoryId] = cardInInventory.cardMana;
            dm.numOfCard.text = dm.deck.Count.ToString() + "/30";
            dm.SortDeckItem();
        }
    }
}