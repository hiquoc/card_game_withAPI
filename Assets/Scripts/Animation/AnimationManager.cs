using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public static AnimationManager Instance;
    private Queue<Animation> animationQueue = new();
    public bool isPlaying = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(Instance);
            return;
        }
        Instance = this;
    }
    public void EnqueueAnimation(Animation animation)
    {
        animationQueue.Enqueue(animation);
        if (!isPlaying)
            StartCoroutine(PlayAnimations());
    }
    IEnumerator PlayAnimations()
    {
        isPlaying = true;
        while (animationQueue.Count > 0)
        {
            Animation animation = animationQueue.Dequeue();
            yield return StartCoroutine(PlaySingleAnimation(animation));
        }
        isPlaying = false;
    }
    IEnumerator PlaySingleAnimation(Animation animation)
    {
        Debug.Log("Playing animtion " + animation.animationId);
        yield return new WaitForSeconds(animation.duration);
    }
}