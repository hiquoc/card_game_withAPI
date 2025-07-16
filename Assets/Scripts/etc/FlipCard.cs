using DG.Tweening;
using UnityEngine;

public class FlipCard : MonoBehaviour
{
    GameObject front;
    GameObject back;

    float flipDuration = 0.5f;

    public bool flipped = false;
    private void Awake()
    {
        front = transform.Find("Front").gameObject;
        back = transform.Find("Back").gameObject;
    }

    public void Flip()
    {
        if (flipped) return;
        flipped = true;

        float currentZ = transform.eulerAngles.z;

        Sequence flipSeq = DOTween.Sequence();

        flipSeq.Append(transform.DORotate(new Vector3(0, 90, currentZ), flipDuration / 2).SetEase(Ease.InQuad))
               .AppendCallback(() =>
               {
                   front.SetActive(true);
                   back.SetActive(false);
                   // Keep Z, reset Y to -90
                   transform.rotation = Quaternion.Euler(0, -90, currentZ);
               })
               .Append(transform.DORotate(new Vector3(0, 0, currentZ), flipDuration / 2).SetEase(Ease.OutQuad));
    }
}
