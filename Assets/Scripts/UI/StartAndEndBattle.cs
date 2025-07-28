using DG.Tweening;
using System.Collections;
using UnityEngine;

public class StartAndEndBattle : MonoBehaviour
{
    public IEnumerator PlayStartBattle()
    {
        RectTransform blurPanel = ReferenceManager.Instance.blurPanel;
        blurPanel.gameObject.SetActive(true);
        Transform startImage = blurPanel.transform.GetChild(0);
        startImage.gameObject.SetActive(true);
        startImage.localScale = Vector3.zero;
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(startImage.DOScale(Vector3.one, 0.5f));
        seq.AppendInterval(1.5f);
        seq.Append(startImage.DOScale(Vector3.zero, 0.2f));
        seq.AppendInterval(0.2f);
        seq.AppendCallback(() =>
        {
            startImage.gameObject.SetActive(false);
            blurPanel.gameObject.SetActive(false);
        });
        yield return seq.WaitForCompletion();
    }
    public void PlayEndBattle(bool isWin)
    {
        StartCoroutine(PlayeEndBattleCoroutine(isWin));
    }
    public IEnumerator PlayeEndBattleCoroutine(bool isWin)
    {
        RectTransform endGamePanel = ReferenceManager.Instance.endGamePanel;
        endGamePanel.localScale = Vector3.zero;
        endGamePanel.gameObject.SetActive(true);
        if (isWin)
            endGamePanel.gameObject.transform.Find("winImg").gameObject.SetActive(true);
        else
            endGamePanel.gameObject.transform.Find("loseImg").gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(endGamePanel.DOScale(Vector3.one, 0.5f));

        yield return seq.WaitForCompletion();
    }
}
