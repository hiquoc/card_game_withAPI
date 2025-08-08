using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class StageManager:MonoBehaviour
{

    public List<Button> stageList;
    public GameObject errorPanel;
    private void Start()
    {
        for(int i = 1; i < stageList.Count; i++)
        {
            stageList[i].interactable=false;
        }
        StartCoroutine(GetUserStage());
    }
    IEnumerator GetUserStage()
    {
        string stageUrl = DataFetcher.address + "users/me";
        using UnityWebRequest request= UnityWebRequest.Get(stageUrl);
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.ConnectionError ||
            request.result == UnityWebRequest.Result.ProtocolError)
        {
            Debug.Log("Error fetching user data: " + request.error);
            if (request.responseCode == 401)
            {
                ShowErrorPanel("Expired session\nPlease re-login");
                Debug.LogWarning("Access forbidden (401). Possibly due to an invalid or expired token.");
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
            UserResult response = JsonConvert.DeserializeObject<UserResult>(request.downloadHandler.text);

            if (response?.data == null)
            {
                Debug.LogWarning("Dữ liệu data null hoặc parse lỗi ở user.");
                yield break;
            }

            ProcessUserData(response);
        }
        catch (Exception e)
        {
            Debug.LogError("Exception occurred:\n" + e);
        }
    }


    [Serializable]
    class UserResult
    {
        public int code;
        public string message;
        public UserData data;
    }
    [Serializable]
    class UserData
    {
        public int userId;
        public string username;
        public string email;
        public string fullname;
        public long phone;
        public string gender;
        public float? balance;
        public string? image;
        public string dob;
        public int? stage;
        public string createdAt;
        public string updatedAt;
        public bool locked;
        public List<string> roles;
    }
    private void ProcessUserData(UserResult result)
    {
        /*Debug.Log(result.data.stage);*/
        if (!result.data.stage.HasValue)
        {
            SceneLoader.Instance.finishedStage = 0;
            SceneLoader.Instance.userId = result.data.userId;
            return;
        }
        int stage= result.data.stage.Value;
        SceneLoader.Instance.finishedStage = stage;
        SceneLoader.Instance.userId = result.data.userId;
        for (int i = 0; i < stageList.Count; i++) { 
            if(i<=stage)
                stageList[i].interactable = true;
        }
    }














    public void ShowErrorPanel(string text = "Something went wrong\nPlease try again later")
    {
        errorPanel.GetComponentInChildren<TMP_Text>().text = text;
        errorPanel.SetActive(true);
    }
}
