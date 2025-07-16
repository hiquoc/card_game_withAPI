using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

public class CardManager : MonoBehaviour
{
    public static CardManager Instance;
    public GameObject cardPrefab;
    public Deck playerDeck, enemyDeck;
    public Queue<GameObject> playerCards, enemyCards;
    public List<GameObject> playerHand, enemyHand;
    public RectTransform playerHandPosition, playerDeckPosition;
    public RectTransform enemyHandPosition, enemyDeckPosition;
    public SplineContainer splineContainer;

    [SerializeField]
    int maxHandSize;

    public RectTransform animationLayer;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        playerDeck = new();
        enemyDeck = new();
        playerCards = new();
        enemyCards = new();

        InitDeck();
    }
    private void Start()
    {

    }
    public void DrawInitHand()
    {
        StartCoroutine(DrawInitHandCoroutine());

    }
    IEnumerator DrawInitHandCoroutine()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.5f);
            DrawCard(0, playerCards, playerHand);
            DrawCard(1, enemyCards, enemyHand);
        }
    }
    public void DrawPlayerCard() => DrawCard(0, playerCards, playerHand);
    public void DrawEnemyCard() => DrawCard(1, enemyCards, enemyHand);

    private void InitDeck()
    {
        InitSideDeck(playerDeck, playerCards, playerDeckPosition, true);
        playerDeck.Setup();

        InitSideDeck(enemyDeck, enemyCards, enemyDeckPosition, false);
        enemyDeck.Setup();
    }
    private void InitSideDeck(Deck deck, Queue<GameObject> cardQueue, Transform deckPosition, bool isPlayer)
    {
        for (int i = 0; i < 30; i++)
        {
            Card card;
            if (i > 2)
            {
                card = new Minion(3, 2, null)
                {
                    type = Card.CardType.minion,
                    mana = 3,
                };
                CardEffect cardEffect1 = new(2, CardEffect.Type.Damage, CardEffect.Target.ChosenTarget);
                card.onPlay.Add(cardEffect1);

                CardEffect cardEffect2 = new(1, CardEffect.Type.Damage, CardEffect.Target.AllEnemyMinions);
                card.onDeath.Add(cardEffect2);
            }
            else
            {
                card = new SpellCard
                {
                    type = Card.CardType.spell,
                    mana = 2
                };

                CardEffect cardEffect1 = new(2, CardEffect.Type.Buff, CardEffect.Target.Self, CardEffect.BuffType.DealDamageOverTime, 3, false, "Arrow");
                card.onPlay.Add(cardEffect1);
            }

            deck.list.Add(card);

            GameObject cardObj = Instantiate(cardPrefab);
            CardDisplay cardDisplay = cardObj.GetComponentInChildren<CardDisplay>();
            cardDisplay.SetupCard(card);
            CardDrag cardDrag = cardObj.GetComponentInChildren<CardDrag>();

            cardDrag.isDraggable = isPlayer;
            cardDrag.isMinionCard = card.type == Card.CardType.minion;
            cardObj.transform.SetParent(deckPosition);
            cardQueue.Enqueue(cardObj);
        }
    }
    public void DrawCard(int turn, Queue<GameObject> cards, List<GameObject> hand)
    {
        GameObject cardObj = cards.Dequeue();
        hand.Add(cardObj);
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        rt.SetParent(animationLayer, true);
        UpdateCardPosition(turn, hand, turn == 0 ? playerHandPosition : enemyHandPosition);
    }
    public void UpdateCardPosition(int turn, List<GameObject> hand, RectTransform handPosition)
    {
        if (hand.Count == 0) return;

        float spacingScale = Mathf.Min(1.3f, 8f / hand.Count);
        float cardSpacing = 1f * spacingScale / maxHandSize;
        float firstCardPos = 0.5f - (hand.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Splines[turn];

        for (int i = 0; i < hand.Count; i++)
        {
            float p = firstCardPos + i * cardSpacing;
            Vector3 splinePosition = spline.EvaluatePosition(p);
            Vector3 forward = spline.EvaluateTangent(p);

            float angle = Mathf.Atan2(forward.y, forward.x) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            GameObject card = hand[i];
            GameObject currentCard = card;

            Sequence seq = DOTween.Sequence();
            seq.Append(currentCard.transform.DOMove(splinePosition, 0.5f));
            seq.Join(currentCard.transform.DORotate(new Vector3(0, 0, angle), 0.5f));
            if (turn == 0)
                seq.Join(currentCard.transform.DOScale(new Vector3(1.3f, 1.3f, 1.3f), 0.2f));
            seq.OnComplete(() =>
            {
                if (turn == 0)
                    currentCard.GetComponent<FlipCard>().Flip();
                if (card.transform.parent != handPosition)
                    card.transform.SetParent(handPosition);
            });
        }
    }
}
