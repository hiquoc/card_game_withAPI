using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    public Slider slider;
    public Image circle;
    public string token;
    public int userId;
    public string username;
    public int enemyId;
    public int finishedStage;
    public int selectedStage;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(Instance);
        DataFetcher.LoadConfig();
    }

    private void Start()
    {
        
    }

    public void LoadNextScene(string sceneName)
    {
        if (sceneName == "BattleScene")
            StartCoroutine(LoadBattleSceneCoroutine(sceneName));
        else
            StartCoroutine(LoadNextSceneCoroutine(sceneName));
    }
    private IEnumerator LoadNextSceneCoroutine(string sceneName)
    {
        yield return StartCoroutine(AnimateTransitionIn());
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        while (scene.progress < 0.9f)
            yield return null;

        /*slider.gameObject.SetActive(true);
        while (CardManager.Instance == null)
        {
            yield return null;
        }
        CardManager cm = CardManager.Instance;
        while (cm.loadedImg < 60)
        {
            slider.value = Mathf.Clamp01(cm.loadedImg / 60f);
            yield return null;
        }
        slider.value = 1;*/

        scene.allowSceneActivation = true;
        while (!scene.isDone)
        {
            yield return null;
        }
        /*slider.gameObject.SetActive(false);*/
        yield return StartCoroutine(AnimateTransitionOut());
    }
    private IEnumerator LoadBattleSceneCoroutine(string sceneName)
    {
        yield return StartCoroutine(AnimateTransitionIn());
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        while (scene.progress < 0.9f)
            yield return null;
        scene.allowSceneActivation = true;
        while (!scene.isDone)
        {
            yield return null;
        }
        slider.gameObject.SetActive(true);
        while(CardManager.Instance == null)
        {   
            yield return null;
        }
        CardManager cm=CardManager.Instance;
        while (!cm.isLoaded)
        {
            slider.value = Mathf.Clamp01(cm.loadedImg / 60f);
            yield return null;
        }
        slider.value = 1;
        yield return new WaitForSeconds(1f);
        slider.gameObject.SetActive(false);
        yield return StartCoroutine(AnimateTransitionOut());
    }
    public IEnumerator AnimateTransitionIn()
    {
        circle.rectTransform.position = new Vector2(-1500f, 500f);
        circle.gameObject.SetActive(true);
        yield return circle.rectTransform.DOMoveX(1000, 1f).WaitForCompletion();
    }
    public IEnumerator AnimateTransitionOut()
    {
        yield return circle.rectTransform.DOMoveX(3500f, 1f).WaitForCompletion();
        circle.gameObject.SetActive(false);
    }
    public void BackToMenuAndLogout()
    {
        StartCoroutine(BackToMenuAndLogoutCoroutine());
    }
    IEnumerator BackToMenuAndLogoutCoroutine()
    {
        yield return new WaitForSeconds(2f);
        yield return StartCoroutine(AnimateTransitionIn());
        AsyncOperation scene = SceneManager.LoadSceneAsync("StartScene");
        scene.allowSceneActivation = false;

        while (scene.progress < 0.9f)
            yield return null;

        scene.allowSceneActivation = true;
        while (!scene.isDone)
        {
            yield return null;
        }
        GameObject canvas = GameObject.Find("Canvas");
        if (canvas != null)
        {
            canvas.TryGetComponent(out MainMenu menu);
            StartCoroutine(menu.SendLogoutRequest());
        }

        yield return StartCoroutine(AnimateTransitionOut());

    }
}