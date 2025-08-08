using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;
using Button = UnityEngine.UI.Button;

public class DeckManager : MonoBehaviour
{
    public static DeckManager Instance;
    public Button reloadBtn;
    public Button showAllCardBtn;
    public Button hideAllCardBtn;
    public Button submitBtn;
    public Button sortByManaBtn,sortByRarityBtn;
    public GameObject errorPanel;
    public TMP_Text numOfCard;
    public GameObject loadingText;
    

    public Dictionary<int, int> manaDict = new();
    public Dictionary<int, Sprite> imgDict = new();
    public Dictionary<int, InventoryCardData> inventoryDict = new();
    public Dictionary<int,GameObject> inventoryObjDict = new();
    int itemCount;

    public Dictionary<int, int> deckManaDict = new();
    public List<int> deck;
    
    ReferenceManager rm;

    public GameObject cardInInventoryPre, selectedCardPre;
     

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
        reloadBtn.onClick.AddListener(ReloadItem);
        submitBtn.onClick.AddListener(SubmitDeck);
        showAllCardBtn.onClick.AddListener(LoadCollection);
        hideAllCardBtn.onClick.AddListener(ReloadInventory);
        sortByManaBtn.onClick.AddListener(SortItemByMana);
        sortByRarityBtn.onClick.AddListener(SortItemByRarity);
    }
    public void ReloadItem()
    {
        Debug.Log("Reloaded");
        if (showAllCardBtn.gameObject.activeInHierarchy)
        {
            ReloadInventory();     
        }       
        else
            LoadCollection();
        if (sortByManaBtn.gameObject.activeInHierarchy)
        {
            sortByManaBtn.gameObject.SetActive(false);
            sortByRarityBtn.gameObject.SetActive(true);
        }
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
        string inventoryUrl = DataFetcher.address + "inventory";
        using UnityWebRequest request = UnityWebRequest.Get(inventoryUrl);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching collection data: " + request.error);
            if(request.responseCode==401)
            {
                ShowErrorPanel(false, "Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (401). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if(!errorPanel.activeInHierarchy)
            {
                ShowErrorPanel(true);
            }                         
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
            ShowErrorPanel(true);
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
        loadingText.SetActive(true);
        manaDict.Clear();
        imgDict.Clear();
        inventoryDict.Clear();
        inventoryObjDict.Clear();
        itemCount=result.data.Count;
        Transform allCardPanel = rm.allCardPanel;
        foreach (InventoryData card in result.data)
        {
            itemCount--;
            /*Debug.Log(itemCount);*/
            if (inventoryDict.ContainsKey(card.cardId))
            {
                inventoryDict.TryGetValue(card.cardId,out InventoryCardData cardData);
                cardData.inventoryId.Add(card.inventoryId);
                if (card.forSale)
                    cardData.onSale += 1;
                else
                    cardData.quantity += 1;
                continue;
            }
            inventoryDict[card.cardId] = new(card.forSale?null:card.inventoryId, card.cardId, card.rarity, card.cardName, card.mainImg,
                card.mana, card.attack, card.health, card.forSale ? 0 : 1, card.forSale ? 1 : 0);

            GameObject g = Instantiate(cardInInventoryPre, allCardPanel);
            g.SetActive(false);

            Image imageComponent = g.transform.Find("Image").GetComponent<Image>();
            StartCoroutine(AddSpriteToDict(card, g, imageComponent));
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

            if(!manaDict.ContainsKey(card.cardId))
                manaDict[card.cardId] = card.mana;
        }
        foreach(int cardId in inventoryObjDict.Keys)
        {
            InventoryCardData icd = inventoryDict[cardId];
            CardInInventory cid = inventoryObjDict[cardId].GetComponent<CardInInventory>();
            GameObject g = inventoryObjDict[cardId];
            GameObject countObj = g.transform.Find("Count").gameObject;
            if (icd.quantity > 0)
            {
                /*Debug.Log(icd.quantity);*/
                countObj.SetActive(true);
                countObj.GetComponent<TMP_Text>().text = "x " + icd.quantity.ToString();
                cid.quantity= icd.quantity;
                cid.inventoryId = inventoryDict[cardId].inventoryId.First();
            }
            else
            {
                /*Debug.Log(icd.quantity);*/
                countObj.SetActive(false);
                CanvasGroup cvg = g.GetComponent<CanvasGroup>();
                cvg.alpha = 0.6f;
                cid.inventoryId = -1;
            }

            if (icd.onSale == 0) continue;
            GameObject onSaleObj = g.transform.Find("OnSale").gameObject;
            onSaleObj.SetActive(true);
            onSaleObj.GetComponent<TMP_Text>().text = "s " + icd.onSale.ToString();
        }
    }
    IEnumerator AddSpriteToDict(InventoryData card, GameObject g, Image imageComponent)
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
                /*Debug.Log(card.cardName);
                Debug.Log(card.cardId);*/
                imgDict[card.cardId] = sprite;
                inventoryObjDict[card.cardId].SetActive(true);
                if(itemCount==0)
                {
                    loadingText.SetActive(false);
                    SortItemByMana();
                }
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
        {            
            Debug.Log("null");
            yield break;
        }
        string deckUrl = DataFetcher.address + "deck";
        using UnityWebRequest request = UnityWebRequest.Get(deckUrl);
        Debug.Log(SceneLoader.Instance.token);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching deck data: " + request.error);
            if (request.responseCode == 401)
            {
                ShowErrorPanel(false, "Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (401). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if (!errorPanel.activeInHierarchy)
            {
                ShowErrorPanel(true);
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

            ProcessDeckData(response.data);
        }
        catch (Exception e)
        {
            ShowErrorPanel(true);
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
        for (int i = selectedCardPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(selectedCardPanel.GetChild(i).gameObject);
        }
        deck.Clear();
        foreach (var card in data.cards)
        {
            GameObject g = Instantiate(selectedCardPre, selectedCardPanel);
            SelectedCard selectedCard = g.GetComponent<SelectedCard>();
            selectedCard.inventoryId = card.inventoryId;
            selectedCard.cardId = card.cardId;
            selectedCard.cardName = card.name;
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
            /*Debug.Log(card.inventoryId);*/
            deckManaDict[card.inventoryId] = card.mana;
        }
        numOfCard.text = data.cards.Count.ToString() + "/" + "30";
        SortDeckItem();
    }

    private void SubmitDeck()
    {
        if (deck.Count < 30)
        {
            ShowErrorPanel(false, "You don't have enough cards\n" + deck.Count.ToString() + "/" + "30");
            return;
        }
        StartCoroutine(SubmitDeckCoroutine());
    }

    private IEnumerator SubmitDeckCoroutine()
    {
        if (SceneLoader.Instance.token == null)
        {
            Debug.Log("null");
            yield break;
        }

        string jsonBody = JsonConvert.SerializeObject(new SubmitDeckRequest(deck));
        Debug.Log(jsonBody);
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        string deckUrl = DataFetcher.address + "deck";
        using UnityWebRequest request = new(deckUrl, "PUT")
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
            if (request.responseCode == 401)
            {
                ShowErrorPanel(false, "Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (401). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if (!errorPanel.activeInHierarchy)
            {
                ShowErrorPanel(true);
            }
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
            ShowErrorPanel(true,"Can't save your deck\nPlease try again later");
            Debug.LogError("Exception occurred:\n" + e);
        }
        ShowErrorPanel(false, "Save sucessfully");
        LoadDeckData();
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

    void LoadCollection()
    {
        sortByManaBtn.gameObject.SetActive(false);
        sortByRarityBtn.gameObject.SetActive(true);
        StartCoroutine(LoadCollectionDataCoroutine());
    }
    IEnumerator LoadCollectionDataCoroutine()
    {
        if (SceneLoader.Instance.token == null)
        {
            Debug.Log("null");
            yield break;
        }
        string collectionUrl = DataFetcher.address + "deck/collection";
        using UnityWebRequest request = new(collectionUrl, "GET");
        request.SetRequestHeader("Authorization","Bearer "+SceneLoader.Instance.token);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();
        if(request.result==UnityWebRequest.Result.ConnectionError||
            request.result==UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching collection data: " + request.error);
            if (request.responseCode == 401)
            {
                ShowErrorPanel(false, "Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (401). Possibly due to an invalid or expired token.");
                SceneLoader.Instance.BackToMenuAndLogout();
            }
            else if (!errorPanel.activeInHierarchy)
            {
                ShowErrorPanel(true);
            }
            yield break;
        }
        try
        {
            CollectionResult response = JsonConvert.DeserializeObject<CollectionResult>(request.downloadHandler.text);

            if (response == null)
            {
                Debug.LogError("Deserialized response is null!");
                yield break;
            }
            if (response.data == null)
            {
                Debug.LogError("response.data is null!");
                yield break;
            }
            if (response.data.cards == null)
            {
                Debug.LogError("response.data.cards is null!");
                yield break;
            }

            ProcessCollectionData(response);
        }
        catch (Exception e)
        {
            ShowErrorPanel(true);
            Debug.LogError("Exception occurred:\n" + e);
        }

    }

    [Serializable]
    public class CollectionResult
    {
        public int code;
        public string message;
        public CollectionDataResult data;
    }

    [Serializable]
    public class CollectionDataResult
    {
        public int userId;
        public string username;
        public List<CollectionCardData> cards;
        public int totalCards;
        public int ownedCards;
        public float completionPercentage;
    }

    [Serializable]
    public class CollectionCardData
    {
        public int cardId;
        public string name;           
        public string type;          
        public string rarity;
        public int mana;
        public int attack;
        public int health;
        public string image;
        public bool owned;
        public int quantity;         
    }


    void ProcessCollectionData(CollectionResult result)
    {
        Transform allCardPanel = rm.allCardPanel;

        for (int i = allCardPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(allCardPanel.GetChild(i).gameObject);
        }

        loadingText.SetActive(true);
        /*manaDict.Clear();*/
        /*imgDict.Clear();*/
        inventoryDict.Clear();
        inventoryObjDict.Clear();
        itemCount=result.data.totalCards;
        foreach (CollectionCardData card in result.data.cards)
        {
            itemCount--;
            /*Debug.Log(itemCount);*/
            if (inventoryDict.ContainsKey(card.cardId))
            {
                /*inventoryDict.TryGetValue(card.cardId, out InventoryCardData cardData);
                if (card.forSale)
                    cardData.onSale += 1;
                else
                    cardData.quantity += 1;
                */
                continue;
            }
            inventoryDict[card.cardId] = new(null, card.cardId, card.rarity, card.name, card.image,
                card.mana, card.attack, card.health, card.quantity, 0);

            GameObject g = Instantiate(cardInInventoryPre, allCardPanel);
            Image imageComponent = g.transform.Find("Image").GetComponent<Image>();
            StartCoroutine(AddSpriteToDict(card, g, imageComponent));
            g.SetActive(false);
            CardInInventory cid = g.GetComponent<CardInInventory>();
            cid.inventoryId = -1;
            cid.cardId = card.cardId;
            cid.rarity = card.rarity;
            cid.cardName = card.name;
            cid.cardMana = card.mana;
            cid.quantity = 1;


            GameObject manaObj = g.transform.Find("ManaText").gameObject;
            manaObj.GetComponent<TMP_Text>().text = card.mana.ToString();
            inventoryObjDict[card.cardId] = g;

            if (!card.owned)
            {
                CanvasGroup cvg = g.GetComponent<CanvasGroup>();
                cvg.alpha = 0.6f;
            }

            if (card.attack == 0 && card.health == 0) continue;
            GameObject attackObj = g.transform.Find("AttackImg").gameObject;
            attackObj.SetActive(true);
            attackObj.GetComponentInChildren<TMP_Text>().text = card.attack.ToString();
            GameObject healthObj = g.transform.Find("HealthImg").gameObject;
            healthObj.SetActive(true);
            healthObj.GetComponentInChildren<TMP_Text>().text = card.health.ToString();

                      
        }
        IEnumerator AddSpriteToDict(CollectionCardData card, GameObject g, Image imageComponent)
        {
            yield return StartCoroutine(LoadImageFromURLCoroutine(card, imageComponent));

        }
        IEnumerator LoadImageFromURLCoroutine(CollectionCardData card, Image image)
        {
            using UnityWebRequest request = UnityWebRequestTexture.GetTexture(card.image);
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
                if(image!=null)
                    image.sprite = sprite;

                inventoryObjDict[card.cardId].SetActive(true);
                if (itemCount == 0)
                {
                    loadingText.SetActive(false);
                    SortItemByMana();
                }
            }
        }
    }


    public void ShowErrorPanel(bool reload = false,string text="Something went wrong\nPlease try again later")
    {
        errorPanel.GetComponentInChildren<TMP_Text>().text = text;
        errorPanel.SetActive(true);
/*        if (reload)
        {
            ReloadInventory();
            showAllCardBtn.gameObject.SetActive(true) ;
            hideAllCardBtn.gameObject.SetActive(false) ;
        }*/
    }
    public int GetInventoryId(int cardId)
    {
        if (!inventoryDict.ContainsKey(cardId)) return -1;

        inventoryDict.TryGetValue(cardId, out InventoryCardData data);
        /*Debug.Log(data.inventoryId.Count);*/
        for (int i = 0; i < data.inventoryId.Count; i++) {
            
            /*Debug.Log(data.inventoryId[i]);*/
            int inventoryId=data.inventoryId[i];
            if(!deck.Contains(inventoryId)) return inventoryId;
        }
        /*Debug.Log("2");*/
        return inventoryDict[cardId].inventoryId.First();
    }





    ////////////////////////////////////////////////
    void SortItemByMana()
    {
        List<GameObject>list=inventoryObjDict.Values.ToList();
        list.Sort((a,b)=>a.GetComponent<CardInInventory>().cardMana.CompareTo(b.GetComponent<CardInInventory>().cardMana));
        for (int i = 0; i < list.Count; i++)
        {
            list[i].transform.SetSiblingIndex(i);
        }
    }
    void SortItemByRarity()
    {
        Dictionary<string, int> dic = new()
        {
            { "COMMON", 0 },
            { "RARE", 1 },
            { "EPIC", 2 },
            { "LEGENDARY", 3 }
        };

        List<GameObject> list = inventoryObjDict.Values.ToList();
        list.Sort((a, b) => {
            CardInInventory cardA = a.GetComponent<CardInInventory>();
            CardInInventory cardB = b.GetComponent<CardInInventory>();
            dic.TryGetValue(cardA.rarity,out int valueA);
            dic.TryGetValue(cardB.rarity, out int valueB);
            if(valueA == valueB)
            {
                return cardA.cardMana.CompareTo(cardB.cardMana);
            }
            return valueA.CompareTo(valueB); 
        });
        for (int i = 0; i < list.Count; i++)
        {
            list[i].transform.SetSiblingIndex(i);
        }
    }
    public void SortDeckItem()
    {
        List<Transform> list = new();
        Transform selectCardPanel = rm.selectedCardPanel;

        for (int i = 0; i < selectCardPanel.childCount; i++) { 
            list.Add(selectCardPanel.GetChild(i));
        }
        list.Sort((a, b) =>
        {
            SelectedCard cardA = a.GetComponent<SelectedCard>();
            SelectedCard cardB = b.GetComponent<SelectedCard>();

            int manaCompare = cardA.mana.CompareTo(cardB.mana);
            if (manaCompare == 0)
            {
                return string.Compare(cardA.cardName, cardB.cardName, StringComparison.OrdinalIgnoreCase);
            }
            return manaCompare;
        });
        for(int i=0; i < list.Count; i++)
        {
            list[i].SetSiblingIndex(i);
        }
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