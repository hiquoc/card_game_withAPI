using TMPro;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public GameObject loginBtn, logoutBtn, signupBtn, userBtn;
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
    public void OnAIButton()
    {
        SceneLoader.Instance.LoadNextScene("BattleScene");
    }
    public void OnDeckButton()
    {
        SceneLoader.Instance.LoadNextScene("DeckScene");
    }
    public void BackToMainMenu()
    {
        SceneLoader.Instance.LoadNextScene("StartScene");
    }
}