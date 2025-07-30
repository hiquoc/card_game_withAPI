using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StagePicker : MonoBehaviour
{
    public  List<GameObject> stageList;
    string levelUrl =DataFetcher.address+ "";
    private void Start()
    {
        StartCoroutine(GetPlayerPlayedLevel());
    }
    IEnumerator GetPlayerPlayedLevel()
    {
        if (SceneLoader.Instance.token == null)
            Debug.Log("null");
        using UnityWebRequest request = UnityWebRequest.Get(levelUrl);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError ||
           request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching stage data: " + request.error);
            yield break;
        }
        try
        {
            LevelResult respone = JsonConvert.DeserializeObject<LevelResult>(request.downloadHandler.text);
            Debug.Log($"Stage completed: {respone.stage}");
            UpdateStageStatus(respone.stage);
        }
        catch (Exception e)
        {
            Debug.LogError("Error when parse to JSON in stage data " + e.Message);
        }
    }

    private void UpdateStageStatus(int stageCompleted)
    {
        for(int i = 0;i<stageCompleted;i++)
        {
            GameObject stageObj = stageList[i];  
            Button button = stageObj.GetComponent<Button>();
            CanvasGroup cvg= stageObj.GetComponentInChildren<CanvasGroup>();
            button.interactable = true;
            cvg.alpha = 1;
        }
    }

    [Serializable]
    private class LevelResult
    {
        public int stage;
    }
}
