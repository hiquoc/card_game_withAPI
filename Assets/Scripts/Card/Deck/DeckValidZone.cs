using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DeckValidZone : MonoBehaviour, IDropHandler
{
    public static DeckValidZone Instance;
    public GameObject selectedCardPre;
    public RectTransform selectedCardPanel;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void IDropHandler.OnDrop(PointerEventData eventData)
    {
        DeckManager dm = DeckManager.Instance;
        if (dm.deck.Count == 30)
        {
            Debug.LogWarning("You cant add more to the deck");
            return;
        }
        GameObject eventObj = eventData.pointerDrag;
        if (eventObj.TryGetComponent(out CardInDeck cardInDeck))
        {
            GameObject selectedCardObj = Instantiate(selectedCardPre);
            Transform manaTF = selectedCardObj.transform.Find("Mana");
            if (manaTF != null)
                manaTF.GetComponentInChildren<TMP_Text>().text = cardInDeck.cardMana.ToString();
            Transform nameTR = selectedCardObj.transform.Find("NameText");
            nameTR.GetComponent<TMP_Text>().text = cardInDeck.cardName;
            selectedCardObj.transform.SetParent(selectedCardPanel);
            dm.deck.Add(cardInDeck.cardId);
            dm.numOfCard.text = dm.deck.Count.ToString() + "/30";
        }
    }
}