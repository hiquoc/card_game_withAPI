using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SlicePieceUI : MonoBehaviour
{
    public float lifetime = 1.5f;
    public float fadeDuration = 0.5f;

    private Image img;
    private CanvasGroup group;
    private Vector2 velocity;

    private void Awake()
    {
        img = GetComponentInChildren<Image>();
        group = GetComponent<CanvasGroup>();
    }

    public void Initialize(Sprite sprite, Vector2 direction, float speed)
    {
        img.sprite = sprite;
        velocity = direction.normalized * speed;
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;
        float fadeStart = lifetime - fadeDuration;

        while (time < lifetime)
        {
            transform.position += (Vector3)(velocity * Time.deltaTime);

            if (time >= fadeStart)
                group.alpha = 1f - (time - fadeStart) / fadeDuration;

            time += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
