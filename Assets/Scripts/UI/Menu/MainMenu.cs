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
    public void OnAIButton(int aiId)
    {
        SceneLoader.Instance.enemyId = aiId;
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

    string logoutUrl = DataFetcher.address + "auth/logout";
    public void LogoutBtnClicked()
    {
        if (SceneLoader.Instance.token == "") return;
        StartCoroutine(SendLogoutRequest());
    }

    private IEnumerator SendLogoutRequest()
    {
        using UnityWebRequest request = UnityWebRequest.PostWwwForm(logoutUrl, "");
        request.SetRequestHeader("Authorization", "Bearer " + SceneLoader.Instance.token);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            SceneLoader.Instance.token = "";
            loginBtn.SetActive(true);
            TMP_InputField userText=loginBtn.transform.Find("Username").GetComponent<TMP_InputField>();
            TMP_InputField passText=loginBtn.transform.Find("Password").GetComponent<TMP_InputField>();
            userText.text = "";
            passText.text = "";
            signupBtn.SetActive(true);
            logoutBtn.SetActive(false);
            userBtn.SetActive(false);
            resultPanel.SetActive(false );
            StartCoroutine( ShowPopupPanel("Logout successfully"));
            SceneLoader.Instance.token = "";
        }
        else
        {
            Debug.LogWarning("Logout failed: " + request.error);
        }
    }
}