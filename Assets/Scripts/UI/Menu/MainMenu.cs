using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class MainMenu : MonoBehaviour
{
    public GameObject loginBtn, logoutBtn, signupBtn, userBtn,resultPanel,popupPanel;
    private void Start()
    {
        if (loginBtn == null) return;
        if (SceneLoader.Instance.token == "")
        {
            loginBtn.SetActive(true);
            signupBtn.SetActive(true);
        }
        else
        {
            userBtn.GetComponentInChildren<TMP_Text>().text =SceneLoader.Instance.username;
            userBtn.SetActive(true);
            logoutBtn.SetActive(true);
        }
    }
    public void OnAISelection()
    {
        if (SceneLoader.Instance.token == "")
        {
            StartCoroutine(ShowPopupPanel("You haven't logged in yet!"));
            return;
        }
        SceneLoader.Instance.LoadNextScene("AISelectionScene");
    }
    public void OnAIButton(int stageId)
    {
        SceneLoader.Instance.enemyId = stageId==1?10:stageId==2?11:12;
        SceneLoader.Instance.selectedStage=stageId;
        SceneLoader.Instance.LoadNextScene("BattleScene");
    }
    public void OnReloadAIButton()
    {
        SceneLoader.Instance.LoadNextScene("BattleScene");
    }
    public void OnNextAIButton()
    {
        SceneLoader.Instance.enemyId += 1;
        SceneLoader.Instance.selectedStage += 1;
        SceneLoader.Instance.LoadNextScene("BattleScene");
    }
    public void OnDeckButton()
    {
        if (SceneLoader.Instance.token == "")
        {
            StartCoroutine(ShowPopupPanel("You haven't logged in yet!"));
            return;
        }
        SceneLoader.Instance.LoadNextScene("DeckScene");
    }
    public void BackToMainMenu()
    {
        SceneLoader.Instance.LoadNextScene("StartScene");
    }
    IEnumerator ShowPopupPanel(string t)
    {
        TMP_Text text = popupPanel.GetComponentInChildren<TMP_Text>();
        text.text = t;
        popupPanel.SetActive(true);
        yield return new WaitForSeconds(2f);
        popupPanel.SetActive(false);
    }

    
    public void LogoutBtnClicked()
    {
        if (SceneLoader.Instance.token == "") return;
        StartCoroutine(SendLogoutRequest());
    }

    public IEnumerator SendLogoutRequest()
    {
        string logoutUrl = DataFetcher.address + "auth/logout";
        using UnityWebRequest request = UnityWebRequest.PostWwwForm(logoutUrl, "");
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SocketClient.Instance.CloseConnection();
            SceneLoader.Instance.token = "";
            loginBtn.SetActive(true);
            Transform loginPanel = loginBtn.transform.parent.Find("LoginPanel");
            TMP_InputField userText= loginPanel.Find("Username").GetComponent<TMP_InputField>();
            TMP_InputField passText= loginPanel.Find("Password").GetComponent<TMP_InputField>();
            userText.text = "";
            passText.text = "";
            signupBtn.SetActive(true);
            logoutBtn.SetActive(false);
            userBtn.SetActive(false);
            resultPanel.SetActive(false);
            /*StartCoroutine( ShowPopupPanel("Logout successfully"));*/
        }
        else
        {
            Debug.LogWarning("Logout failed: " + request.error);
        }
    }
    public void OpenWeb()
    {
        Application.OpenURL(DataFetcher.GetReplacedPortUrl(3000));
    }
    public void OpenUserWeb()
    {
        Application.OpenURL(DataFetcher.GetReplacedPortUrl(3000) + "profile");
    }
    public void OpenSignUpWeb()
    {
        Application.OpenURL(DataFetcher.GetReplacedPortUrl(3000)+"register");
    }
    public void OpenEventWeb()
    {
        Application.OpenURL(DataFetcher.GetReplacedPortUrl(3000)+"event");
    }
    public void ExitGame()
    {
        Application.Quit();
    }
}