using NativeWebSocket;
using Newtonsoft.Json;
using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SocketClient : MonoBehaviour
{
    public static SocketClient Instance;
    WebSocket websocket;

    private float reloadDelay = 0.3f;
    private float reloadTimer = 0f;
    private bool shouldReloadDeck = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Persist across scenes
    }

    public async void SetUpSocket()
    {
        string newUrl = DataFetcher.address.Replace("http", "ws");
        websocket = new WebSocket(newUrl + "ws?token=" + SceneLoader.Instance.token);
        Debug.Log($"Connecting to: {newUrl}ws?token={SceneLoader.Instance.token}");

        websocket.OnOpen += () => {
            Debug.Log("WebSocket opened successfully");
        };

        websocket.OnMessage += OnWebSocketMessage;

        websocket.OnError += (e) => {
            Debug.LogError($"WebSocket Error: {e}");
        };

        websocket.OnClose += (e) => {
            Debug.Log($"WebSocket closed: {e}");
        };

        await websocket.Connect();
    }

    private void OnWebSocketMessage(byte[] bytes)
    {
        string msg = Encoding.UTF8.GetString(bytes);
        Debug.Log($"WebSocket message received: {msg}");
        shouldReloadDeck=true;
        reloadTimer = reloadDelay;
        /*if (msg.TrimStart().StartsWith("{"))
        {
            try
            {
                NotificationMessage notif = JsonUtility.FromJson<NotificationMessage>(msg);

                if (notif == null)
                {
                    Debug.LogWarning("Failed to parse notification message");
                    return;
                }
                Debug.Log($"Notification - Type: {notif.type}, UserId: {notif.userId}, Message: {notif.message}");
                *//*RouteNotification(notif);*//*
            }
            catch (Exception ex)
            {
                Debug.LogError($"Parse failed: {ex.Message}");
            }
        }
        else
        {
            Debug.Log($"Received plain message: {msg}");
        }      */ 
    }
    private void RouteNotification(NotificationMessage notification)
    {
        if (string.IsNullOrEmpty(notification.type))
        {
            Debug.LogWarning("Notification type is null or empty");
            return;
        }

        switch (notification.type.ToLower())
        {
            case "GACHA_RECEIVED":
                Debug.Log($"Update collection: {notification.type}");
                if (SceneManager.GetActiveScene().name == "DeckScene")
                    DeckManager.Instance.ReloadItem();
                break;


            default:
                Debug.Log($"Unknown notification type: {notification.type}");
                break;
        }
    }



    public async void CloseConnection()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
            Debug.Log("WebSocket closed manually");
        }
    }

    void Update()
    {
        websocket?.DispatchMessageQueue();
        if (shouldReloadDeck)
        {
            reloadTimer -= Time.deltaTime;
            if(reloadTimer<=0)
            {
                shouldReloadDeck = false;
                Debug.Log(SceneManager.GetActiveScene().name);
                if (SceneManager.GetActiveScene().name == "DeckScene")
                    DeckManager.Instance.ReloadItem();               
            }
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null)
        {
            await websocket.Close();
        }
    }


    [System.Serializable]
    public class NotificationMessage
    {
        public string userId;
        public string type;
        public string message;
        public long timestamp;
    }
}
