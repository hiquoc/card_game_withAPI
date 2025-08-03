using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Splines;
using UnityEngine.UI;

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

    public bool doneLoading = false;

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
        LoadDeckData(playerDeck, playerCards, playerDeckPosition, true);
        LoadDeckData(enemyDeck, enemyCards, enemyDeckPosition, false);
        /*InitSideDeck(playerDeck, playerCards, playerDeckPosition, true);
        InitSideDeck(enemyDeck, enemyCards, enemyDeckPosition, false);*/
    }
    private void InitSideDeck(Deck deck, Queue<GameObject> cardQueue, Transform deckPosition, bool isPlayer)
    {
        for (int i = 0; i < 30; i++)
        {
            Card card;
            if (i > 0 && i <= 1)
            {
                card = new MinionCard(i, 1, 2, null)
                {
                    type = Card.CardType.minion,
                    mana = 3,
                };
                /*DamageEffect cardEffect1 = new(2, CardEffect.Target.ChosenTarget, "arrow");
                card.onPlay.Add(cardEffect1);*/

                /*BuffEffect cardEffect2 = new(0, CardEffect.Target.CurrentMinion, "", BuffEffect.BuffType.Taunt);
                card.onPlay.Add(cardEffect2);*/

                BuffEffect cardEffect2 = new(01, CardEffect.Target.ChosenTarget, "attack", BuffEffect.BuffType.Attack,1,false);
                card.onPlay.Add(cardEffect2);
                /*BuffEffect cardEffect3 = new(2, CardEffect.Target.AllAlly, "heal", BuffEffect.BuffType.IncreaseMaxHealth);
                card.onPlay.Add(cardEffect3);*/
            }
            else if (i > 1 && i < 5)
            {
                card = new MinionCard(i, 1, 2, null)
                {
                    type = Card.CardType.minion,
                    mana = 3,
                };
                DamageEffect cardEffect2 = new(1, CardEffect.Target.RandomEnemyMinion, "fireball");
                card.onEndOfTurn.Add(cardEffect2);
                /*card.onEndOfTurn.Add(cardEffect2);
                card.onEndOfTurn.Add(cardEffect2);*/

                BuffEffect cardEffect1 = new(2, CardEffect.Target.RandomAllyMinion, "attack", BuffEffect.BuffType.Attack);
                card.onPlay.Add(cardEffect1);
                /*BuffEffect cardEffect4 = new(1, CardEffect.Target.AllAllyMinions, "", BuffEffect.BuffType.ActiveHealthBuff);
                card.onPlay.Add(cardEffect4);
                DrawEffect cardEffect3 = new();
                card.onEndOfTurn.Add(cardEffect3);
                BuffEffect cardEffect9 = new(2, CardEffect.Target.RandomAllyMinion, "heal", BuffEffect.BuffType.HealOverTime,1,true);
                card.onPlay.Add(cardEffect9);*/
                HealEffect cardEffect = new(3, CardEffect.Target.AllAllyMinions, "heal");
                card.onPlay.Add(cardEffect);
                DamageEffect cardEffect3 = new(3, CardEffect.Target.AllEnemy, "explosion");
                card.onDeath.Add(cardEffect3);
            }
            else if (i > 5)
            {
                card = new MinionCard(i, 1, 2, null)
                {
                    type = Card.CardType.minion,
                    mana = 3,
                };
                
                /*DamageEffect cardEffect2 = new(1, CardEffect.Target.RandomEnemyMinion, "arrow");
                card.onStartOfTurn.Add(cardEffect2);
                BuffEffect cardEffect1 = new(2, CardEffect.Target.RandomAllyMinion, "attack", BuffEffect.BuffType.Attack);
                card.onPlay.Add(cardEffect1);*/
                /*DrawEffect cardEffect3 = new();
                card.onEndOfTurn.Add(cardEffect3);*/
                HealEffect cardEffect = new(3, CardEffect.Target.AllAllyMinions, "heal");
                card.onDeath.Add(cardEffect);
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

                BuffEffect cardEffect1 = new(2, CardEffect.Target.AllAlly, "attack", BuffEffect.BuffType.Attack,2, false);
                card.onPlay.Add(cardEffect1);

                BuffEffect cardEffect3 = new(2, CardEffect.Target.Self, "shield", BuffEffect.BuffType.Shield);
                card.onPlay.Add(cardEffect3);

                HealEffect cardEffect2 = new(1, CardEffect.Target.AllAlly, "heal");
                card.onPlay.Add(cardEffect2);
                /*DamageEffect cardEffect3 = new(3, CardEffect.Target.AllEnemy, "explosion");
                card.onPlay.Add(cardEffect3);*/
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

        if (cards.Count == 0)
        {
            rm.textHelper.ShowText("You have ran out of card!");
            yield break;
        }
        CardDrag.canDrag = false;
        CardHover.canHover = false;
        GameObject cardObj = cards.Dequeue();

        if (hand.Count == 10)
        {
            GameObject frontObj = cardObj.transform.Find("Front").gameObject;
            rm.textHelper.ShowText("You hand is full!");
            yield return StartCoroutine(cardObj.GetComponent<CardDrag>().MoveSpellCard(turn == 0));
            DeathExplosionUI deathExplosion = frontObj.GetComponent<DeathExplosionUI>();
            deathExplosion.canvasTransform = rm.canvas.transform as RectTransform;
            /*yield return new WaitForSeconds(1f);*/
            deathExplosion.Explode(frontObj.GetComponent<Image>());
            Destroy(cardObj);
            yield break;
        }

        hand.Add(cardObj);
        RectTransform rt = cardObj.GetComponent<RectTransform>();
        rt.SetParent(animationLayer, true);
        rm.sm.Play("cardDrawn");
        Tween tween= UpdateCardPosition(turn);
        if (tween != null) {
            yield return tween.WaitForCompletion();
        }
        CardDrag.canDrag = true;
        CardHover.canHover = true;
        yield return null;
        
    }

    public Tween UpdateCardPosition(int turn)
    {
        List<GameObject> hand = turn == 0 ? playerHand : enemyHand;
        RectTransform handPosition = turn == 0 ? playerHandPosition : enemyHandPosition;
        if (hand.Count == 0) return null;
        float spacingScale = Mathf.Min(1.3f, 8f / hand.Count);
        float cardSpacing = 1f * spacingScale / maxHandSize;
        float firstCardPos = 0.5f - (hand.Count - 1) * cardSpacing / 2;
        Spline spline = splineContainer.Splines[turn];

        Tween tween = null;
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
                {
                    currentCard.GetComponent<FlipCard>().Flip();
                    /*currentCard.GetComponent<CardHover>().enabled = false;
                    currentCard.GetComponent<CardDrag>().enabled = false;*/
                }

                if (card.transform.parent != handPosition)
                    card.transform.SetParent(handPosition);
                if (i == hand.Count - 1)
                    tween = seq;
            });
        }
        return tween;
    }











    public void LoadDeckData(Deck deck, Queue<GameObject> cardQueue, Transform deckPosition, bool isPlayer)
    {
        StartCoroutine(GetDeckDataCoroutine(deck,cardQueue, deckPosition, isPlayer));
    }
    public IEnumerator GetDeckDataCoroutine(Deck deck, Queue<GameObject> cardQueue, Transform deckPosition, bool isPlayer)
    {
        if (SceneLoader.Instance.token == null)
            Debug.Log("null");
        using UnityWebRequest request = UnityWebRequest.Get(DataFetcher.address+"deck");
        Debug.Log(SceneLoader.Instance.token);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);//add SceneLoader.Instance.enemyId if isPlayer==false
        
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("url: " + DataFetcher.address + "/deck");
            Debug.Log("Error fetching deck data: " + request.error);
            yield break;
        }

        try
        {
            DeckResult response = JsonConvert.DeserializeObject<DeckResult>(request.downloadHandler.text);

            if (response?.data?.cards == null)
            {
                Debug.LogWarning("Dữ liệu cards null hoặc parse lỗi ở deck.");
                yield break;
            }
            else
            {
                Debug.Log($"Deck loaded: {response.data.cards.Count} cards / Total: {response.data.totalCards}");
            }

            ProcessDeckData(response.data, deck, cardQueue, deckPosition, isPlayer);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception occurred:\n" + e);
        }
    }

    [System.Serializable]
    public class DeckResult
    {
        public int code;
        public string message;
        public DeckData data;
    }

    [System.Serializable]
    public class DeckData
    {
        public int userId;
        public string username;
        public List<CardData> cards;
        public int totalCards;
    }

    [System.Serializable]
    public class CardData
    {
        public int inventoryId;
        public int cardId;
        public string name;
        public string type;
        public string rarity;
        public int mana;
        public int? attack;
        public int? health;
        public string image;
        public string mainImg;
        public string description;
        public List<EffectData> effects;
    }
    #nullable enable
    [System.Serializable]
    public class EffectData
    {
        public int effectId;
        public string type;
        public int value;
        public string target;
        public string animationId;
        public string? buffType;
        public int duration;
        public bool isStartOfTurn;
        public string? summonMinionIds;
        public string triggerType;
    }

    private void ProcessDeckData(DeckData data, Deck deck, Queue<GameObject> cardQueue, Transform deckPosition, bool isPlayer)
    { 
        foreach (var card in data.cards)
        {
            Card newCard;
            if (card.type == "MINION")
            {
                newCard = new MinionCard(card.cardId, card.attack.Value, card.health.Value, null)
                {
                    id = card.cardId,
                    type = Card.CardType.minion,
                    mana = card.mana,
                    minionImg = card.image,
                    image = card.mainImg,
                };
            }
            else
            {
                newCard = new SpellCard
                {
                    id = card.cardId,
                    mana = card.mana,
                    type = 0,
                    image = card.mainImg,
                };
            }
            foreach(EffectData effectData in card.effects  ){
                switch (effectData.triggerType)
                {
                    case "ON_PLAY":
                        newCard.onPlay.Add(GetCardEffectByType(effectData));
                        break;
                    case "ON_DEATH":
                        newCard.onDeath.Add(GetCardEffectByType(effectData));
                        break;
                    case "ON_START_TURN":
                        newCard.onStartOfTurn.Add(GetCardEffectByType(effectData));
                        break;
                    case "ON_END_TURN":
                        newCard.onEndOfTurn.Add(GetCardEffectByType(effectData));
                        break;
                }
            }

            deck.list.Add(newCard);
        }

        deck.Setup();

        foreach (var newCard in deck.list)
        {
            GameObject cardObj = Instantiate(cardPrefab);

            Image image = cardObj.transform.Find("Front").GetComponent<Image>();
            StartCoroutine(LoadImageFromURLCoroutine(newCard.image, image));

            CardDisplay cardDisplay = cardObj.GetComponentInChildren<CardDisplay>();
            cardDisplay.SetupCard(newCard);

            CardDrag cardDrag = cardObj.GetComponentInChildren<CardDrag>();
            cardDrag.isDraggable = isPlayer;
            cardDrag.isMinionCard = newCard.type == Card.CardType.minion;

            cardObj.GetComponent<CardHover>().isEnemyCard = !isPlayer;

            cardObj.transform.SetParent(deckPosition);
            cardQueue.Enqueue(cardObj);
        }
    }

    IEnumerator LoadImageFromURLCoroutine(string url, Image image)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Texture2D cropped = CropTransparent(texture);

            Sprite sprite = Sprite.Create(cropped, new Rect(0, 0, cropped.width, cropped.height), new Vector2(0.5f, 0.5f), 1f);
            image.sprite = sprite;
        }
    }
    

 












    /// //////////////////////////////////////////////////////

    CardEffect GetCardEffectByType(EffectData effectData)
    {
        return effectData.type switch
        {
            "Damage" => new DamageEffect(effectData.value, GetTargetByString(effectData.target), effectData.animationId),
            "Heal" => new HealEffect(effectData.value, GetTargetByString(effectData.target), effectData.animationId),
            "Draw" => new DrawEffect(effectData.value, GetTargetByString(effectData.target), ""),
            /*"Summon" => new SummonEffect(effectData.value, GetTargetByString(effectData.target)),*/
            "Buff"=>new BuffEffect(effectData.value,GetTargetByString(effectData.target), "",GetBuffTypeByString(effectData.buffType),effectData.duration,effectData.isStartOfTurn),
        };
    }
     CardEffect.Target GetTargetByString(string targetString)
    {
        return targetString switch
        {
            "None" => CardEffect.Target.None,
            "Self" => CardEffect.Target.Self,
            "Enemy" => CardEffect.Target.Enemy,
            "CurrentMinion" => CardEffect.Target.CurrentMinion,
            "All" => CardEffect.Target.All,
            "AllAlly" => CardEffect.Target.AllAlly,
            "AllEnemy" => CardEffect.Target.AllEnemy,
            "AllMinions" => CardEffect.Target.AllMinions,
            "AllEnemyMinions" => CardEffect.Target.AllEnemyMinions,
            "AllAllyMinions" => CardEffect.Target.AllAllyMinions,
            "RandomAllyMinion" => CardEffect.Target.RandomAllyMinion,
            "RandomEnemyMinion" => CardEffect.Target.RandomEnemyMinion,
            "ChosenTarget" => CardEffect.Target.ChosenTarget,
            _ => CardEffect.Target.None,
        };
    }
     List<String> GetListOfMinionIds(string text)
    {
        List<String> list = new();
        string currentStr="";
        foreach(char c in text)
        {
            if (c != ',' || c != ' ')
                currentStr += c;
            else
            {
                list.Add(currentStr);
                currentStr = "";
            }
        }
        return list;
    }
    BuffEffect.BuffType GetBuffTypeByString(string text)
    {
        if (Enum.TryParse(text, out BuffEffect.BuffType result))
            return result;

        throw new ArgumentException($"Invalid buff type: {text}");
    }
    Texture2D CropTransparent(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;

        int minX = width;
        int minY = height;
        int maxX = 0;
        int maxY = 0;

        Color32[] pixels = texture.GetPixels32();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color32 pixel = pixels[y * width + x];
                if (pixel.a > 5)
                {
                    if (x < minX) minX = x;
                    if (y < minY) minY = y;
                    if (x > maxX) maxX = x;
                    if (y > maxY) maxY = y;
                }
            }
        }

        int croppedWidth = maxX - minX + 1;
        int croppedHeight = maxY - minY + 1;

        if (croppedWidth <= 0 || croppedHeight <= 0)
            return texture; // empty image, skip crop

        Texture2D newTex = new(croppedWidth, croppedHeight);
        newTex.SetPixels(texture.GetPixels(minX, minY, croppedWidth, croppedHeight));
        newTex.Apply();

        return newTex;
    }
}