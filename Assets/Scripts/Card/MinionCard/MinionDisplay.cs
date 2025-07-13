using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinionDisplay : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Minion minion;
    /*public GameObject cardPreviewPrefab;
    private GameObject cardPreviewObj;*/
    public TMP_Text attackText;
    public TMP_Text healthText;

    ReferenceManager rm;

    private void Awake()
    {
        rm = ReferenceManager.Instance;
    }
    public void SetupMinion(Minion card)
    {
        minion = card;
        minion.SetCanAttack(false);
        attackText.text = card.currentAttack.ToString();
        healthText.text = card.currentHealth.ToString();
        card.display = this;
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        /*Debug.Log("Enter");
        cardPreviewObj = Instantiate(cardPreviewPrefab);
        cardPreviewObj.GetComponent<CardDisplay>().SetupCard(card);
        cardPreviewObj.transform.Find("Front").gameObject.SetActive(true);
        cardPreviewObj.transform.Find("Back").gameObject.SetActive(false);
        cardPreviewObj.transform.SetParent(ReferenceManager.Instance.canvas.transform);
        cardPreviewObj.transform.position = eventData.position;*/
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        /*if (cardPreviewObj != null)
            Destroy(cardPreviewObj);*/
    }

    public void UpdateHealth()
    {
        healthText.text = minion.currentHealth.ToString();
    }

    public void Die()
    {
        Debug.Log($"{minion.name} Died");
        StartCoroutine(rm.bm.OnMinionDeath(gameObject));
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

}