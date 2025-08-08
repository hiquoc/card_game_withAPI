using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance;
    ReferenceManager rm;
    [SerializeField] private GameObject popupPre;
    [SerializeField] private int poolSize = 5;
    Queue<GameObject> popups = new();
    public class SpriteData
    {
        public Sprite sprite;
        public Texture2D texture;

        public SpriteData(Texture2D tex, Sprite spr)
        {
            texture = tex;
            sprite = spr;
        }
    }

    private Dictionary<int, SpriteData> sprites = new();
    private Dictionary<int, int> spriteCounter = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rm = ReferenceManager.Instance;
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(popupPre, rm.canvas.transform);
            obj.SetActive(false);
            popups.Enqueue(obj);
        }
    }
    //Popup
    public GameObject GetPopup()
    {
        if (popups.Count > 0)
        {
            return popups.Dequeue();
        }
        else
        {
            GameObject obj = Instantiate(popupPre, rm.canvas.transform);
            obj.transform.position = Vector3.zero;
            obj.SetActive(false);
            return obj;
        }
    }
    public void ReturnPopup(GameObject popup)
    {
        popup.SetActive(false);
        popups.Enqueue(popup);
    }
    public void SetUpNewSprite(int cardId,string url)
    {
        if (!spriteCounter.ContainsKey(cardId))
            spriteCounter[cardId] = 0;
        spriteCounter[cardId]++;
        if (!sprites.ContainsKey(cardId))
            StartCoroutine(LoadImageFromURLCoroutine(cardId,url));
    }
    IEnumerator LoadImageFromURLCoroutine(int cardId, string url)
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
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 1f);
            sprites[cardId] = new SpriteData(texture, sprite);
        }
    }
    public Sprite GetSprite(int cardId)
    {
        return sprites.ContainsKey(cardId) ? sprites[cardId].sprite : null;
    }
    public void ReleaseSprite(int cardId)
    {
        if (!spriteCounter.ContainsKey(cardId))
            return;

        spriteCounter[cardId]--;

        if (spriteCounter[cardId] <= 0 && sprites.ContainsKey(cardId))
        {
            var data = sprites[cardId];
            if (data.texture != null) Destroy(data.texture);
            if (data.sprite != null) Destroy(data.sprite);

            sprites.Remove(cardId);
            spriteCounter.Remove(cardId);
        }
    }
}