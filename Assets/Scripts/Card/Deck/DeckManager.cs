using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public Button reloadBtn;
    public Button submitBtn;
    public TMP_Text numOfCard;
    public List<int> deck;

    public Dictionary<int, int> manaDict = new();
    public Dictionary<int, Sprite> imgDict = new();
    public Dictionary<int, InventoryCardData> inventoryDict = new();
    public Dictionary<int,GameObject> inventoryObjDict = new();

    ReferenceManager rm;

    public GameObject cardInInventoryPre, selectedCardPre;
    readonly string deckUrl = DataFetcher.address + "deck";
    readonly string inventoryUrl = DataFetcher.address + "inventory";

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rm=ReferenceManager.Instance;
        ReloadInventory();
        LoadDeckData();
        reloadBtn.onClick.AddListener(ReloadInventory);
        submitBtn.onClick.AddListener(SubmitDeck);
    }

    public void ReloadInventory()
    {
        if (ReferenceManager.Instance == null)
        {
            Debug.LogError("ReferenceManager.Instance is null");
            return;
        }
        if (ReferenceManager.Instance.allCardPanel == null)
        {
            Debug.LogError("ReferenceManager.Instance.allCardPanel is null");
            return;
        }

        Transform allCardPanel = rm.allCardPanel;

        for (int i = allCardPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(allCardPanel.GetChild(i).gameObject);
        }
        StartCoroutine(GetInventoryDataCoroutine());
    }
    public IEnumerator GetInventoryDataCoroutine()
    {
        if (SceneLoader.Instance.token == null)
            Debug.Log("null");
        using UnityWebRequest request = UnityWebRequest.Get(inventoryUrl);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching collection data: " + request.error);
            yield break;
        }

        try
        {
            InventoryResult response = JsonConvert.DeserializeObject<InventoryResult>(request.downloadHandler.text);

            if (response?.data == null)
            {
                Debug.LogWarning("Dữ liệu data null hoặc parse lỗi ở inventory.");
                yield break;
            }

            ProcessInventoryData(response);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception occurred:\n" + e);
        }
    }
    [Serializable]
    class InventoryResult
    {
        public int code;
        public string message;
        public List<InventoryData> data;
    }
    [Serializable]
    class InventoryData
    {
        public int inventoryId;
        public int cardId;
        public string rarity;
        public string cardName;
        public string image;
        public string mainImg;
        public int mana;
        public int attack;
        public int health;
        public float? salePrice;
        public string acquiredAt;
        public bool onDeck;
        public bool forSale;
    }
    void ProcessInventoryData(InventoryResult result)
    {
        Transform allCardPanel = rm.allCardPanel;
        foreach (InventoryData card in result.data)
        {
            if (inventoryDict.ContainsKey(card.cardId))
            {
                inventoryDict.TryGetValue(card.cardId,out InventoryCardData cardData);               
                if (card.forSale)
                    cardData.onSale += 1;
                else
                    cardData.quantity += 1;
                continue;
            }
            inventoryDict[card.cardId] = new(card.forSale?null:card.inventoryId, card.cardId, card.rarity, card.cardName, card.mainImg,
                card.mana, card.attack, card.health, card.forSale ? 0 : 1, card.forSale ? 1 : 0);

            GameObject g = Instantiate(cardInInventoryPre, allCardPanel);

            Image imageComponent = g.transform.Find("Image").GetComponent<Image>();
            StartCoroutine(AddGameObjectToDict(card, g, imageComponent));
            CardInInventory cid = g.GetComponent<CardInInventory>();
            cid.cardId = card.cardId;
            cid.rarity = card.rarity;
            cid.cardName = card.cardName;
            cid.cardMana = card.mana;
            cid.quantity = 1;//Sua lai sau

            GameObject manaObj = g.transform.Find("ManaText").gameObject;
            manaObj.GetComponent<TMP_Text>().text = card.mana.ToString();
            inventoryObjDict[card.cardId] = g;

            if (card.attack==0 && card.health==0) continue;
            cid.attack = card.attack;
            cid.health = card.health;
            GameObject attackObj = g.transform.Find("AttackImg").gameObject;
            attackObj.SetActive(true);
            attackObj.GetComponentInChildren<TMP_Text>().text = card.attack.ToString();
            GameObject healthObj = g.transform.Find("HealthImg").gameObject;
            healthObj.SetActive(true);
            healthObj.GetComponentInChildren<TMP_Text>().text = card.health.ToString();
            

        }
        foreach(int cardId in inventoryObjDict.Keys)
        {
            InventoryCardData icd = inventoryDict[cardId];
            CardInInventory cid = inventoryObjDict[cardId].GetComponent<CardInInventory>();
            GameObject g = inventoryObjDict[cardId];
            GameObject countObj = g.transform.Find("Count").gameObject;
            if (icd.quantity > 0)
            {
                Debug.Log(icd.quantity);
                countObj.SetActive(true);
                countObj.GetComponent<TMP_Text>().text = "x " + icd.quantity.ToString();
                cid.quantity= icd.quantity;
            }
            else
            {
                Debug.Log(icd.quantity);
                countObj.SetActive(false);
                CanvasGroup cvg = g.GetComponent<CanvasGroup>();
                cvg.alpha = 0.6f;
                Destroy(cid);
            }

            if (icd.onSale == 0) continue;
            GameObject onSaleObj = g.transform.Find("OnSale").gameObject;
            onSaleObj.SetActive(true);
            onSaleObj.GetComponent<TMP_Text>().text = "s " + icd.onSale.ToString();
        }
    }
    IEnumerator AddGameObjectToDict(InventoryData card, GameObject g, Image imageComponent)
    {
        yield return StartCoroutine(LoadImageFromURLCoroutine(card, imageComponent));

    }
    IEnumerator LoadImageFromURLCoroutine(InventoryData card, Image image)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(card.mainImg);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            texture.filterMode = FilterMode.Point;
            texture.Apply();

            Texture2D cropped = CropTransparent(texture);

            Sprite sprite = Sprite.Create(cropped, new Rect(0, 0, cropped.width, cropped.height), new Vector2(0.5f, 0.5f), 1f);
            image.sprite = sprite;
            if (!imgDict.ContainsKey(card.cardId))
            {
                Debug.Log(card.cardName);
                Debug.Log(card.cardId);
                imgDict[card.cardId] = sprite;
            }
                
        }
    }

    public void LoadDeckData()
    {
        StartCoroutine(GetDeckDataCoroutine());
    }
    public IEnumerator GetDeckDataCoroutine()
    {
        if (SceneLoader.Instance.token == null)
        {            Debug.Log("null");
            yield break;
        }
            
        using UnityWebRequest request = UnityWebRequest.Get(deckUrl);
        Debug.Log(SceneLoader.Instance.token);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
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

            ProcessDeckData(response.data);
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
        public string description;
        public List<EffectData> effects;
    }

    [System.Serializable]
    public class EffectData
    {

    }

    private void ProcessDeckData(DeckData data)
    {
        Transform selectedCardPanel = rm.selectedCardPanel;
        foreach (var card in data.cards)
        {
            GameObject g = Instantiate(selectedCardPre, selectedCardPanel);
            SelectedCard selectedCard = g.GetComponent<SelectedCard>();
            selectedCard.inventoryId = card.inventoryId;
            selectedCard.cardId = card.cardId;
            selectedCard.mana = card.mana;
            if (card.type == "MINION")
            {
                selectedCard.attack = card.attack.Value;
                selectedCard.health = card.health.Value;
            }

            GameObject manaTxt = g.transform.Find("Mana").gameObject;
            GameObject nameTxt = g.transform.Find("NameText").gameObject;

            manaTxt.GetComponentInChildren<TMP_Text>().text = card.mana.ToString();
            nameTxt.GetComponent<TMP_Text>().text = card.name;
            deck.Add(card.inventoryId);
            Debug.Log(card.inventoryId);
/*            Debug.Log(card.cardId);*/
            manaDict[card.inventoryId] = card.mana;
        }
        numOfCard.text = data.cards.Count.ToString() + "/" + "30";
    }

    private void SubmitDeck()
    {
        StartCoroutine(SubmitDeckCoroutine());
    }

    private IEnumerator SubmitDeckCoroutine()
    {
        if (SceneLoader.Instance.token == null)
        {
            Debug.Log("null");
            yield break;
        }

        Debug.Log(SceneLoader.Instance.token);
        string jsonBody = JsonUtility.ToJson(new SubmitDeckRequest(deck));
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        using UnityWebRequest request = new(deckUrl, "POST")
        {
            uploadHandler = new UploadHandlerRaw(bodyRaw),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching deck data: " + request.error);
            yield break;
        }

        try
        {
            SubmitDeckResult response = JsonConvert.DeserializeObject<SubmitDeckResult>(request.downloadHandler.text);

            if (response?.data == null)
            {
                Debug.LogWarning("Dữ liệu cards null hoặc parse lỗi ở deck.");
                yield break;
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Exception occurred:\n" + e);
        }
    }
    [System.Serializable]
    public class SubmitDeckRequest
    {
        public List<int> inventoryIds;

        public SubmitDeckRequest(List<int> ids)
        {
            inventoryIds = ids;
        }
    }
    [Serializable]
    public class SubmitDeckResult
    {
        public int code;
        public string message;
        public DeckData data;
    }













    ////////////////////////////////////////////////
    void SortInventoryByMana()
    {
        
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