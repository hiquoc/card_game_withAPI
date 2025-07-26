using DG.Tweening;
using System.Collections;
using UnityEngine;

public class StartAndEndBattle : MonoBehaviour
{
    public RectTransform startImage;
    public RectTransform endImage;


    public IEnumerator PlayStartBattle()
    {
        RectTransform blurPanel = ReferenceManager.Instance.blurPanel;
        blurPanel.gameObject.SetActive(true);
        startImage.gameObject.SetActive(true);
        startImage.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(startImage.DOScale(Vector3.one, 0.5f));
        seq.AppendInterval(1.5f);
        seq.Append(startImage.DOScale(Vector3.zero, 0.2f));
        seq.AppendInterval(0.5f);
        seq.AppendCallback(() =>
        {
            startImage.gameObject.SetActive(false);
            blurPanel.gameObject.SetActive(false);
        });
        yield return seq.WaitForCompletion();
    }
    /*public void PlayEndBattle()
    {
        RectTransform blurPanel = ReferenceManager.Instance.blurPanel;
        blurPanel.gameObject.SetActive(true);
        startImage.gameObject.SetActive(true);
        Sequence seq = DOTween.Sequence();
        seq.Append(startImage.DOScale(new Vector3(1f, 1f, 1f), 1f));
        seq.AppendInterval(1f);
        seq.AppendCallback(() =>
        {
            startImage.gameObject.SetActive(false);
            blurPanel.gameObject.SetActive(false);
        });
    }*/
}
