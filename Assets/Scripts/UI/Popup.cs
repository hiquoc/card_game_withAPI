using DG.Tweening;
/*using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    private int currentValue = 0;
    private bool isHealingPopup = false;

    public TMP_Text valueText;
    public CanvasGroup cvg;

    public IEnumerator Show(Transform target)
    {
        if (target == null) yield break;
        yield return null;
        *//*Debug.Log(target.transform.position);
        Debug.Log($"Target active: {gameObject.activeSelf}, position: {transform.position}");*//*

        transform.position = target.position;
        gameObject.SetActive(true);

        valueText.text = (isHealingPopup ? "+" : "-") + currentValue;
        cvg.alpha = 1f;

        yield return new WaitForSeconds(1f);
        cvg.DOFade(0, 1f);

        float timer = 0f;
        while (timer < 1f)
        {
            if (target != null)
                transform.position = target.position;
            timer += Time.deltaTime;
            yield return null;
        }

        cvg.alpha = 0f;
        gameObject.SetActive(false);
        currentValue = 0;
    }


    public void UpdateValue(int newValue, bool isHealing)
    {
        currentValue = newValue;
        isHealingPopup = isHealing;
        valueText.text = (isHealing ? "+" : "-") + currentValue;
    }

}*/
using System.Collections;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public TMP_Text valueText;
    public CanvasGroup cvg;

    private int currentValue = 0;
    private Coroutine showCoroutine;
    private Transform target;

    public void AddValue(int value, Transform target)
    {
        currentValue += value;
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
        PoolManager.Instance.ReturnPopup(gameObject);
    }


}
