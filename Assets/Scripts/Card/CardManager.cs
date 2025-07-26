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

    ReferenceManager rm;
    RectTransform animationLayer;

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

        rm = ReferenceManager.Instance;
        animationLayer = rm.animationLayer;
        InitDeck();
    }
    public void DrawInitHand()
    {
        StartCoroutine(DrawInitHandCoroutine());

    }
    IEnumerator DrawInitHandCoroutine()
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(DrawCard(0));
            yield return new WaitForSeconds(0.2f);
            yield return StartCoroutine(DrawCard(1));

        }
    }

    public void DrawPlayerCard() => StartCoroutine(DrawCard(0));
    public void DrawEnemyCard() => StartCoroutine(DrawCard(1));

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
            if (i > 0 && i <= 1)
            {
                card = new MinionCard(i, 3, 2, null)
                {
                    type = Card.CardType.minion,
                    mana = 3,
                };
                DamageEffect cardEffect1 = new(2, CardEffect.Target.ChosenTarget, "arrow");
                card.onPlay.Add(cardEffect1);

                BuffEffect cardEffect2 = new(0, CardEffect.Target.CurrentMinion, "", BuffEffect.BuffType.Taunt);
                card.onPlay.Add(cardEffect2);
            }
            else if (i > 1)
            {
                card = new MinionCard(i, 3, 2, null)
                {
                    type = Card.CardType.minion,
                    mana = 3,
                };
                DamageEffect cardEffect2 = new(1, CardEffect.Target.RandomEnemyMinion, "fireball");
                card.onEndOfTurn.Add(cardEffect2);
                BuffEffect cardEffect1 = new(2, CardEffect.Target.RandomAllyMinion, "attack", BuffEffect.BuffType.Attack);
                card.onPlay.Add(cardEffect1);
                BuffEffect cardEffect4 = new(1, CardEffect.Target.AllAllyMinions, "", BuffEffect.BuffType.ActiveHealthBuff);
                card.onPlay.Add(cardEffect4);
                /*DrawEffect cardEffect3 = new();
                card.onEndOfTurn.Add(cardEffect3);*/
                HealEffect cardEffect = new(3, CardEffect.Target.AllAllyMinions, "heal");
                card.onDeath.Add(cardEffect);
                DamageEffect cardEffect3 = new(3, CardEffect.Target.AllEnemy, "explosion");
                card.onDeath.Add(cardEffect3);
            }
            else
            {
                card = new SpellCard
                {
                    id = i,
                    type = Card.CardType.spell,
                    mana = 2
                };

                /*DrawEffect cardEffect1 = new();
                card.onPlay.Add(cardEffect1);*/

                BuffEffect cardEffect1 = new(2, CardEffect.Target.Self, "attack", BuffEffect.BuffType.Attack, 1, false);
                card.onPlay.Add(cardEffect1);

                BuffEffect cardEffect3 = new(2, CardEffect.Target.Self, "shield", BuffEffect.BuffType.Shield, 1, false);
                card.onPlay.Add(cardEffect3);

                HealEffect cardEffect2 = new(1, CardEffect.Target.AllAlly, "heal");
                card.onPlay.Add(cardEffect2);
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
    public IEnumerator DrawCard(int turn)
    {
        Queue<GameObject> cards = turn == 0 ? playerCards : enemyCards;
        List<GameObject> hand = turn == 0 ? playerHand : enemyHand;
        GameObject cardObj = cards.Dequeue();
        hand.Add(cardObj);
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        rt.SetParent(animationLayer, true);
        UpdateCardPosition(turn);
        rm.sm.Play("cardDrawn");
        yield return null;
    }

    public void UpdateCardPosition(int turn)
    {
        List<GameObject> hand = turn == 0 ? playerHand : enemyHand;
        RectTransform handPosition = turn == 0 ? playerHandPosition : enemyHandPosition;
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
