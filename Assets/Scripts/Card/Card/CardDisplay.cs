using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDisplay : MonoBehaviour
{
    public Card card;
    [Header("UI References")]
    public TMP_Text manaText;

    [Header("Minion Stats")]
    public GameObject minionStatsPanel;
    public TMP_Text attackText;
    public TMP_Text healthText;
    public void SetupCard(Card card)
    {
        this.card = card;
        manaText.text = card.mana.ToString();
        if (card.type == Card.CardType.minion)
        {
            MinionCard minionCard = card as MinionCard;
            attackText.text = minionCard.currentAttack.ToString();
            healthText.text = minionCard.maxHealth.ToString();
            minionStatsPanel.SetActive(true);
        }
    }
}