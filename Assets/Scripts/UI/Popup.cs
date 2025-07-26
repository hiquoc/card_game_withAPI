using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;

public class Popup : MonoBehaviour
{
    public TMP_Text valueText;
    public CanvasGroup cvg;
    public IEnumerator Show(int value, Transform target, bool isHealing)
    {
        if (value == 0 || target == null) yield break;
        valueText.text = value.ToString();
        if (isHealing)
            valueText.text = "+" + valueText.text;
        gameObject.SetActive(true);
        cvg.alpha = 1f;
        /*transform.DOMoveY(transform.position.y + 30f, 1f);*/
        cvg.DOFade(0, 3f);
        float timer = 0f;
        while (timer < 3f)
        {
            if (target != null)
                transform.position = target.position;
            timer += Time.deltaTime;
            yield return null;
        }
        cvg.alpha = 0f;
        gameObject.SetActive(false);
    }
}