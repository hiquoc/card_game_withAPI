using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Networking;


public class DeckHandler : MonoBehaviour
{
    public GameObject cardInDeckPre;
    string deckUrl = DataFetcher.address + "deck";

    ReferenceManager rm;
    private void Start()
    {
        rm = ReferenceManager.Instance;
        StartCoroutine(GetDeckDataCoroutine());
    }
    public void ReloadDeck()
    {
        StartCoroutine(GetDeckDataCoroutine());
    }
    public IEnumerator GetDeckDataCoroutine()
    {
        if (SceneLoader.Instance.token == null)
            Debug.Log("null");
        using UnityWebRequest request = UnityWebRequest.Get(deckUrl);
        Debug.Log(SceneLoader.Instance.token);
/*        string tmpToken = "eyJhbGciOiJIUzI1NiJ9.eyJyb2xlcyI6WyJVU0VSIl0sInVzZXJJZCI6NSwic3ViIjoidXNlcjQ2NyIsImlhdCI6MTc1Mzc3NzY2NywiZXhwIjoxNzUzODY0MDY3fQ.skULc_6x2GqGpP7VDNBh1fZNaUJ24sTqZawpka4Wubg";
*/      request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
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
                Debug.LogWarning("Dữ liệu cards null hoặc parse lỗi.");
            }
            else
            {
                Debug.Log($"Deck loaded: {response.data.cards.Count} cards / Total: {response.data.totalCards}");
            }

            ProcessDeckData(response.data);
        }
        catch (Exception e)
        {
            Debug.LogError("Failed to parse JSON: " + e.Message);
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
        // Bạn nên thêm đúng các trường mà API trả về trong "effects" nếu có.
    }

    private void ProcessDeckData(DeckData data)
    {
        Transform allCardPanel = rm.allCardPanel;
        foreach (var card in data.cards)
        {
            GameObject g = Instantiate(cardInDeckPre, allCardPanel);
            
        }
    }
}