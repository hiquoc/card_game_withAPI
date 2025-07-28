using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DataFetcher : MonoBehaviour
{
    string deckUrl = "";

    public IEnumerator GetDeckDataCoroutine()
    {
        using UnityWebRequest request = new(deckUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching deck data: " + request.error);
            yield break;
        }
        string jsonString = request.downloadHandler.text;
        Debug.Log("Raw JSON: " + jsonString);

        try
        {
            processJsonData(jsonString);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error processing JSON: {e.Message}");
        }
    }

    private void processJsonData(string jsonString)
    {
        Debug.Log("Inspect the raw JSON above to determine its structure. It may contain 'cards', 'effects', and image URLs.");

    }
}