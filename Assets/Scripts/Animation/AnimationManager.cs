using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public GameObject arrowPre;
    public GameObject fireballPre;
    public GameObject explosionPre;
    public GameObject healPre;
    public static AnimationManager Instance;
    private Queue<Animation> animationQueue = new();
    public bool isPlaying = false;
    private bool isPlayingMultiple = false;

    ReferenceManager rm;
    RectTransform animationLayer;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }
        Instance = this;
        rm = ReferenceManager.Instance;
        animationLayer = ReferenceManager.Instance.animationLayer;
    }
    public void EnqueueAnimation(Animation animation)
    {
        animationQueue.Enqueue(animation);
        Debug.Log($"Animation enqueued: {animation.animationId}");

        /*Debug.Log("AnimationManager: isPlaying = true");*/
        StartCoroutine(PlayAnimations());
    }
    public void PlayInstanceAnimation(Animation animation)
    {
        StartCoroutine(PlayInstanceAnimationCoroutine(animation));
    }
    IEnumerator PlayInstanceAnimationCoroutine(Animation animation)
    {
        StartCoroutine(PlaySingleAnimation(animation));
        if (!isPlayingMultiple)
        {
            isPlayingMultiple = true;
            isPlaying = true;
            yield return new WaitForSeconds(1f);
            isPlaying = false;
            isPlayingMultiple = false;
        }
    }
    IEnumerator PlayAnimations()
    {
        while (isPlaying && animationQueue.Count == 0)
        {
            yield return new WaitForSeconds(0.2f);
        }
        isPlaying = true;
        while (animationQueue.Count > 0)
        {
            Animation animation = animationQueue.Dequeue();
            yield return StartCoroutine(PlaySingleAnimation(animation));
        }
        /*Debug.Log("isPlaying");*/
        isPlaying = false;
    }
    public IEnumerator PlaySingleAnimation(Animation animation)
    {
        Debug.Log("Playing animation " + animation.animationId);
        Sequence seq = DOTween.Sequence();
        GameObject aniObj = null;
        RectTransform aniRT = null;
        switch (animation.animationId)
        {
            case "arrow":
                aniObj = Instantiate(arrowPre, animationLayer);
                aniRT = aniObj.GetComponent<RectTransform>();
                break;
            case "fireball":
                aniObj = Instantiate(fireballPre, animationLayer);
                aniRT = aniObj.GetComponent<RectTransform>();
                break;
            case "explosion":
                aniObj = Instantiate(explosionPre, animationLayer);
                aniRT = aniObj.GetComponent<RectTransform>();
                break;

            case "heal":
                aniObj = Instantiate(healPre, animationLayer);
                aniRT = aniObj.GetComponent<RectTransform>();
                break;

            default:
                Debug.Log("Unknown animation " + animation.animationId);
                break;
        }


        if (animation.target != Vector3.zero)
        {
            Vector3 sourcePos = animation.source;
            Vector3 targetPos = animation.target;

            /*Debug.Log(sourcePos);
            Debug.Log(targetPos);*/
            /*bool yLarger = sourcePos.y < targetPos.y;
            bool xLarget = sourcePos.x < targetPos.x;
            targetPos += new Vector3(xLarget ? -20f : 20f, yLarger ? -20f : 20f, 0f);*/
            Vector3 direction = (targetPos - sourcePos).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

            aniRT.transform.position = sourcePos;
            aniRT.rotation = Quaternion.Euler(0f, 0f, angle);

            aniRT.position = sourcePos;
            aniRT.rotation = Quaternion.Euler(0f, 0f, angle);

            // Animator logic
            Animator ani = aniObj.GetComponent<Animator>();
            ani.Play("Start");
            yield return new WaitForSeconds(0.5f);

            ani.Play("Move");
            float distance = Vector3.Distance(sourcePos, targetPos);
            float speed = 1000f;
            float duration = Mathf.Clamp(distance / speed, 0.1f, 1f);

            Tween moveTween = aniRT.DOMove(targetPos, duration);
            rm.sm.Play(animation.animationId);
            yield return moveTween.WaitForCompletion();



            ani.Play("End");
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            if (aniRT != null)
            {
                /*Debug.Log(aniObj);*/
                aniRT.position = animation.source;
                if (aniObj.TryGetComponent(out Animator ani))
                    ani.Play("Move");
            }

            rm.sm.Play(animation.animationId);
            yield return new WaitForSeconds(1f);
        }
        if (aniObj != null)
            Destroy(aniObj);
        animation.callback?.Invoke();
    }

}