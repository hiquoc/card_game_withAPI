using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class Popup : MonoBehaviour
{
    public TMP_Text valueText;
    public CanvasGroup cvg;

    private int currentValue = 0;
    private Coroutine showCoroutine;
    private Transform target;

    public void ShowPopup(int value, Transform target)
    {
        currentValue = value;
        this.target = target;

        UpdateDisplay();

        if (showCoroutine != null)
        {
            StopCoroutine(showCoroutine);
        }
        showCoroutine = StartCoroutine(ShowRoutine());
    }

    private void UpdateDisplay()
    {
        /*Debug.Log(valueText);*/
        /*Debug.Log(currentValue);*/
        valueText.text = (currentValue > 0 ? "+" : "") + currentValue;
        /*valueText.color = currentValue > 0 ? Color.green : Color.red;*/
        cvg.DOKill();
        cvg.alpha = 1f;
        transform.position = target.position;
        gameObject.SetActive(true);
    }
    private IEnumerator ShowRoutine()
    {
        float duration = 1f;
        float fadeTime = 1f;
        float timer = 0f;

        while (timer < duration)
        {
            if (target != null)
                transform.position = target.position;
            timer += Time.deltaTime;
            yield return null;
        }

        cvg.DOFade(0, fadeTime);

        timer = 0f;
        while (timer < fadeTime)
        {
            if (target != null)
                transform.position = target.position;
            timer += Time.deltaTime;
            yield return null;
        }

        currentValue = 0;
        showCoroutine = null;
        /*Debug.Log("null");*/
        PoolManager.Instance.ReturnPopup(gameObject);
    }
    public void StopAndReturnPopup()
    {
        currentValue = 0;
        showCoroutine = null;
        PoolManager.Instance.ReturnPopup(gameObject);
    }

}
