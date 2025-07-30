using DG.Tweening;
using TMPro;
using UnityEngine;

public class TextHelper : MonoBehaviour
{
    public TMP_Text text;
    public CanvasGroup cvg;
    Sequence seq;
    public void ShowText(string t)
    {
        text.text = t;
        gameObject.SetActive(true);
        seq?.Kill();
        cvg.alpha = 1f;

        seq = DOTween.Sequence();
        seq.AppendInterval(3f);
        seq.Append(cvg.DOFade(0f, 1f));
        seq.OnComplete(()=>gameObject.SetActive(false));
    }
}