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
        startImage.localScale = Vector3.zero;
        startImage.gameObject.SetActive(true);
        blurPanel.gameObject.SetActive(true);

        yield return new WaitForSeconds(1f);
        yield return startImage.DOScale(Vector3.one, 0.5f).WaitForCompletion();
        yield return new WaitForSeconds(2f);
        yield return startImage.DOScale(Vector3.zero, 0.2f).WaitForCompletion();
        yield return new WaitForSeconds(0.2f);

        startImage.gameObject.SetActive(false);
        blurPanel.gameObject.SetActive(false);
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
        if (BattleManager.Instance.player.GetHealth() <= 0 && BattleManager.Instance.enemy.GetHealth() <= 0)
        {
            endGamePanel.gameObject.transform.Find("drawImg").gameObject.SetActive(true);
        }
        else
        {
            if (isWin)
                endGamePanel.gameObject.transform.Find("winImg").gameObject.SetActive(true);
            else
                endGamePanel.gameObject.transform.Find("loseImg").gameObject.SetActive(true);
        }
        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(endGamePanel.DOScale(Vector3.one, 0.5f));

        yield return seq.WaitForCompletion();
    }
    public void PlayDraw()
    {        
        StartCoroutine(PlayDrawBattleCoroutine());
    }
    public IEnumerator PlayDrawBattleCoroutine()
    {
        RectTransform endGamePanel = ReferenceManager.Instance.endGamePanel;
        endGamePanel.localScale = Vector3.zero;
        endGamePanel.gameObject.SetActive(true);
        endGamePanel.gameObject.transform.Find("drawImg").gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.AppendInterval(1f);
        seq.Append(endGamePanel.DOScale(Vector3.one, 0.5f));

        yield return seq.WaitForCompletion();
    }
}
