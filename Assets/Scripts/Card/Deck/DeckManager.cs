using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public Button reloadBtn;
    public Button submitBtn;
    public TMP_Text numOfCard;
    public List<int> deck;

    public List<Card> cards = new();
    public Dictionary<int, Image> imageDict = new();

    ReferenceManager rm;

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rm = ReferenceManager.Instance;
        LoadAllCards();
        reloadBtn.onClick.AddListener(LoadAllCards);
        submitBtn.onClick.AddListener(SubmitDeck);
    }


    private void LoadAllCards()
    {
        //Get cards and store into cards
        ////////////
        foreach (Card card in cards)
        {
            imageDict[card.id] = card.image;
        }
        LoadDeck();
    }
    private void LoadDeck()
    {
        /*load deck then add to Deck list*/

        numOfCard.text = deck.Count.ToString() + "/30";

    }
    private void SubmitDeck()
    {

    }
}