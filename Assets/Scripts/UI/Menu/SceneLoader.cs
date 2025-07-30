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
    public string username;
    public string token;

    string loginUrl = "http://172.20.10.9:8080/auth/token";
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(Instance);
    }



    public void LoadNextScene(string sceneName)
    {
        StartCoroutine(LoadNextSceneCoroutine(sceneName));
    }
    private IEnumerator LoadNextSceneCoroutine(string sceneName)
    {
        yield return StartCoroutine(AnimateTransitionIn());
        AsyncOperation scene = SceneManager.LoadSceneAsync(sceneName);
        scene.allowSceneActivation = false;

        /*slider.gameObject.SetActive(true);*/
        while (scene.progress < 0.9f)
        {
            /*slider.value = Mathf.Clamp01(scene.progress / 0.9f);*/
            yield return null;
        }
        /*slider.value = 1;*/

        scene.allowSceneActivation = true;
        while (!scene.isDone)
        {
            yield return null;
        }
        /*slider.gameObject.SetActive(false);*/
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
}