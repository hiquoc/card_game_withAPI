using DG.Tweening;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Splines;
using UnityEngine.UI;
using static LoginHandler;
using static UnityEngine.EventSystems.EventTrigger;

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

    public int loadedImg;
    public bool isLoaded;

    public GameObject errorPanel;

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
        loadedImg = 0;
        isLoaded=false;
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
    public IEnumerator DrawCard(int turn)
    {
        Queue<GameObject> cards = turn == 0 ? playerCards : enemyCards;
        List<GameObject> hand = turn == 0 ? playerHand : enemyHand;

        if (cards.Count == 0)
        {
            rm.textHelper.ShowText(turn==0?"You":"Enemy"+" have ran out of card!");
            yield break;
        }
        CardDrag.canDrag = false;
        CardHover.canHover = false;
        GameObject cardObj = cards.Dequeue();

        if (hand.Count == 10)
        {
            GameObject frontObj = cardObj.transform.Find("Front").gameObject;
            rm.textHelper.ShowText(turn == 0 ? "You" : "Enemy" + " hand is full!");
            yield return StartCoroutine(cardObj.GetComponent<CardDrag>().MoveSpellCard(turn == 0));
            DeathExplosionUI deathExplosion = frontObj.GetComponent<DeathExplosionUI>();
            deathExplosion.canvasTransform = rm.canvas.transform as RectTransform;
            /*yield return new WaitForSeconds(1f);*/
            deathExplosion.Explode(frontObj.GetComponent<Image>());
            Destroy(cardObj);
            yield break;
        }

        hand.Add(cardObj);
        if(cardObj.TryGetComponent(out CardDisplay component) && component.card is MinionCard minion)
        {
            /*if (turn == 0)
            {
                Debug.Log(minion.id);
                Debug.Log(minion.minionImg);
            }*/
            PoolManager.Instance.SetUpNewSprite(minion.id, minion.minionImg);
        }
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
                seq.Join(currentCard.transform.DOScale(new Vector3(1.5f, 1.5f, 1.5f), 0.2f));
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
        {
            Debug.Log("null");
            yield break;
        }

        string url = DataFetcher.address + "deck";
        if(!isPlayer)
            url+= "/" + SceneLoader.Instance.enemyId;
        using UnityWebRequest request = UnityWebRequest.Get(url);
        /*Debug.Log(url);*/
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("url: " + DataFetcher.address + "/deck");
            Debug.Log("Error fetching deck data: " + request.error);
            if (request.responseCode == 403)
            {
                ShowErrorPanel("Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (403). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if (!errorPanel.activeInHierarchy)
            {
                ShowErrorPanel();
            }
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
        loadedImg++;
        if (loadedImg == 60)
            isLoaded = true;
    }
    public List<int> stageRewardId;
    public IEnumerator GiveRewardCard(int cardId)
    {
        SceneLoader sceneLoader=SceneLoader.Instance;
        string giveCardUrl = $"{DataFetcher.address}inventory/{sceneLoader.userId}/cards/system";
        
        string jsonBody = JsonUtility.ToJson(new CardRequest(cardId));
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        using UnityWebRequest request =new (giveCardUrl, "POST");

        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + sceneLoader.token);
        Debug.Log($"URL: {giveCardUrl}");
        Debug.Log($"Token: {sceneLoader.token}");
        Debug.Log($"JSON Body: {jsonBody}");
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error giving reward: " + request.error);
            if (request.responseCode == 403)
            {
                ShowErrorPanel("Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (403). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if (!errorPanel.activeInHierarchy)
            {
                ShowErrorPanel();
            }
            yield break;
        }

    }
    [Serializable]
    public class CardRequest
    {
        public int cardId;

        public CardRequest(int cardId)
        {
            this.cardId = cardId;
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
            "Buff"=>new BuffEffect(effectData.value,GetTargetByString(effectData.target), effectData.animationId, GetBuffTypeByString(effectData.buffType),effectData.duration,effectData.isStartOfTurn),
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
        {
            return result;
        }
            

        return BuffEffect.BuffType.None;
    }
    public Texture2D CropTransparent(Texture2D texture)
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
    public void ShowErrorPanel(string text = "Something went wrong\nPlease try again later")
    {
        errorPanel.GetComponentInChildren<TMP_Text>().text = text;
        errorPanel.SetActive(true);
    }

}