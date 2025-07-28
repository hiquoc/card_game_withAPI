using UnityEngine;

public class MainMenu : MonoBehaviour
{
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