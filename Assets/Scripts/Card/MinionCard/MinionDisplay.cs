using DG.Tweening;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MinionDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public MinionCard minion;
    public GameObject cardPreviewPrefab;
    private GameObject cardPreviewObj;
    public Image image;
    public TMP_Text attackText;
    public TMP_Text healthText;
    Coroutine coroutine;
    Popup activePopup;
    ReferenceManager rm;

    private void Awake()
    {
        rm = ReferenceManager.Instance;
    }
    public void SetupMinion(MinionCard card, GameObject prefab)
    {
        StartCoroutine(LoadImageFromURLCoroutine(card.minionImg, image));
        minion = card;
        attackText.text = card.currentAttack.ToString();
        healthText.text = card.currentHealth.ToString();
        card.display = this;
        cardPreviewPrefab = Instantiate(prefab);
        cardPreviewPrefab.SetActive(false);
        minion.SetCanAttack(false);
    }
    IEnumerator LoadImageFromURLCoroutine(string url, Image image)
    {
        using UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load image: " + request.error);
        }
        else
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
            image.sprite = sprite;
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        coroutine = StartCoroutine(ShowBaseCardCoroutine());

    }
    IEnumerator ShowBaseCardCoroutine()
    {
        yield return new WaitForSeconds(1f);
        cardPreviewObj = Instantiate(cardPreviewPrefab, rm.animationLayer);
        cardPreviewObj.GetComponent<CardDisplay>().SetupCard(minion);
        cardPreviewObj.transform.Find("Front").gameObject.SetActive(true);
        cardPreviewObj.transform.Find("Back").gameObject.SetActive(false);
        cardPreviewObj.transform.SetParent(ReferenceManager.Instance.blurPanel);
        cardPreviewObj.transform.position = new Vector3(350, 560);
        cardPreviewObj.transform.localScale = new(2f, 2f, 2f);
        cardPreviewObj.SetActive(true);
        ReferenceManager.Instance.blurPanel.gameObject.SetActive(true);
        coroutine = null;
    }
    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        ReferenceManager.Instance.blurPanel.gameObject.SetActive(false);
        if (coroutine != null)
        {
            StopCoroutine(coroutine);
            coroutine = null;
        }

        if (cardPreviewObj != null)
        {
            Destroy(cardPreviewObj);
        }
    }
    public void UpdateAttack()
    {
        attackText.text = $"{minion.currentAttack}";
        if (attackText != null)
        {
            attackText.transform.parent.TryGetComponent(out Animator component);
            component.Play("Move");
        }
    }
    public void UpdateHealth()
    {
        healthText.text = minion.currentHealth.ToString();
    }
    public void ShowReadyToAttack()
    {
        gameObject.GetComponent<SelectableTarget>().ReadyToAttackHighlight();
    }
    public void HideReadyToAttack()
    {
        gameObject.GetComponent<SelectableTarget>().DisableHighlight();
    }

    public void Die()
    {
        /*Debug.Log($"{transform.position} Died");*/
        rm.bm.EqueueMinionDeath(gameObject);
    }

    public void PlayAttackAnimation(ITarget target, System.Action onComplete)
    {
        rm.bm.isWaiting = true;
        RectTransform attackerRect = GetComponent<RectTransform>();
        RectTransform targetRect = target.GetGameObject().GetComponent<RectTransform>();

        Vector2 originalPos = attackerRect.position;
        Vector2 targetPos = targetRect.position;
        Vector2 offset = new(0f, originalPos.y < targetPos.y ? -70f : 70f);

        int originalIndex = attackerRect.GetSiblingIndex();
        RectTransform aniLayer = rm.animationLayer;
        Transform originalParent = attackerRect.parent;
        attackerRect.SetParent(aniLayer);

        GameObject holder = new("MinionHolder");
        holder.AddComponent<RectTransform>();
        holder.transform.SetParent(originalParent);
        holder.transform.SetSiblingIndex(originalIndex);
        holder.GetComponent<RectTransform>().sizeDelta = attackerRect.sizeDelta;
        Canvas.ForceUpdateCanvases();

        SoundManager.Instance.Play("attacked");
        Sequence seq = DOTween.Sequence();
        seq.Append(attackerRect.transform.DOScale(1.3f, 0.3f));
        seq.Append(attackerRect.DOMove(targetPos + offset, 0.3f).SetEase(Ease.InOutQuad));
        seq.AppendCallback(() => onComplete?.Invoke());
        seq.Append(attackerRect.DOMove(originalPos, 0.5f));
        seq.Append(attackerRect.transform.DOScale(1f, 0.3f));
        seq.AppendCallback(() =>
        {
            attackerRect.SetParent(originalParent);
            attackerRect.SetSiblingIndex(originalIndex);
            Destroy(holder);
            rm.bm.isWaiting = false;
        });
    }
    public void AddTaunt()
    {
        Image minionImage = GetComponent<Image>();
        minionImage.sprite = rm.tauntMinionSprite;
    }
    public void HaveAttackBuff(bool v)
    {
        if (v)
            attackText.color = Color.green;
        else
            attackText.color = Color.white;
    }
    public void HaveHealthBuff(bool v)
    {
        if (v)
            healthText.color = Color.green;
        else
            healthText.color = Color.white;
    }
    private void OnDestroy()
    {
        if (cardPreviewPrefab != null)
        {
            Destroy(cardPreviewPrefab);
        }
        if (activePopup != null)
        {
            activePopup.StopAndReturnPopup();
        }
    }
    public void ShowPopup(int value)
    {
        if (gameObject == null) return;
        if (rm.bm.dyingMinions.Contains(gameObject)) return;
        if (activePopup == null)
        {
            GameObject popupGO = PoolManager.Instance.GetPopup();
            activePopup = popupGO.GetComponent<Popup>();
        }      
        /*Debug.Log("Minion "+value);*/
        StartCoroutine(ShowPopupCoroutine(value));
    }
    IEnumerator ShowPopupCoroutine(int value)
    {
        yield return null;
        activePopup.AddValue(value, transform);
    }
}