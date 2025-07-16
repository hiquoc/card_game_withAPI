using DG.Tweening;
using TMPro;
using UnityEngine;

public class CharacterDisplay : MonoBehaviour
{
    public Character character;
    public TMP_Text characterNameText;
    public TMP_Text healthText;
    GameObject healthObj;
    public TMP_Text shieldText;
    GameObject shieldObj;
    public TMP_Text manaText;
    public TMP_Text attackText;
    GameObject attackObj;

    ReferenceManager rm;

    public TMP_Text skillNameText, skillDesText;
    private void Awake()
    {
        character = new();
        rm = ReferenceManager.Instance;
    }
    public void SetUp(int health, string name)
    {
        character.maxHealth = health;
        character.currentHealth = health;
        character.characterName = name;

        healthText.text = $"{health}";
        manaText.text = $"{character.currentMana}";
        characterNameText.text = $"{name}";

        healthObj = healthText.transform.parent.gameObject;
        shieldObj = shieldText.transform.parent.gameObject;
        attackObj = attackText.transform.parent.gameObject;

        character.display = this;
        //Thay doi image,skill cua nhan vat sau
    }
    public void UpdateAttack()
    {
        attackText.text = $"{character.currentAttack}";
        if (character.currentAttack > 0)
        {
            attackObj.SetActive(true);
        }
        else
        {
            attackObj.SetActive(false);
        }
    }
    public void UpdateHealth()
    {
        healthText.text = $"{character.currentHealth}";
    }

    public void UpdateShield()
    {
        shieldText.text = $"{character.currentShield}";
        if (character.currentShield > 0)
        {
            shieldObj.SetActive(true);
        }
        else
        {
            shieldObj.SetActive(false);
        }
    }
    public void UpdateMana()
    {
        manaText.text = character.currentMana.ToString();
    }
    public void PlayAttackAnimation(ITarget target, System.Action onComplete)
    {
        rm.bm.isWaiting = true;
        RectTransform attacker = GetComponent<RectTransform>();
        RectTransform targetRect = target.GetGameObject().GetComponent<RectTransform>();

        Vector2 originalPos = attacker.position;
        Vector2 targetPos = targetRect.position;
        Vector2 offset = new(0f, originalPos.y < targetPos.y ? -20f : 20f);

        int originalIndex = attacker.GetSiblingIndex();
        RectTransform aniLayer = rm.animationLayer;
        Transform originalParent = attacker.parent;
        attacker.SetParent(aniLayer);

        Sequence seq = DOTween.Sequence();
        seq.Append(attacker.transform.DOScale(1.3f, 0.3f));
        seq.Append(attacker.DOMove(targetPos + offset, 0.3f).SetEase(Ease.InOutQuad));
        seq.AppendCallback(() => onComplete?.Invoke());
        seq.Append(attacker.DOMove(originalPos, 0.5f));
        seq.Append(attacker.transform.DOScale(1f, 0.3f));
        seq.AppendCallback(() =>
        {
            attacker.SetParent(originalParent);
            character.SetHasAttackedThisTurn(true);
            rm.bm.isWaiting = false;
        });
    }

}