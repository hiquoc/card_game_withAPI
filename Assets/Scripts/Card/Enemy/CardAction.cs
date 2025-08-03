using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static BuffEffect;

public class CardAction
{
    public static int nextInstanceId = 0;
    public int instanceId;
    public Card card;
    public GameObject cardObj;
    public int estimatedValue;

    public int playerMinionCount => BattleManager.Instance.playerMinionList.Count;
    public int enemyMinionCount => BattleManager.Instance.enemyMinionList.Count;

    public CardAction(Card card, GameObject cardObj)
    {
        this.card = card;
        this.cardObj = cardObj;
        this.estimatedValue = EstimateCardValue(card);
        instanceId = nextInstanceId++;
    }
    int EstimateCardValue(Card card)
    {
        int value = 0;
        value += SumEffectValue(card.onPlay);
        if (card is MinionCard)
        {
            int minionCount = enemyMinionCount;
            if (minionCount == 6)
            {
                value -= 9999;
                return value;
            }
            if(minionCount> playerMinionCount)
            {
                value -= (minionCount - playerMinionCount) * 2;
            }
            value += SumEffectValue(card.onDeath);
            value += SumEffectValue(card.onStartOfTurn);
            value += SumEffectValue(card.onEndOfTurn);
        }
        return value;
    }
    int SumEffectValue(List<CardEffect> effects)
    {
        int value = 0;

        foreach (CardEffect effect in effects)
        {
            int baseValue = GetEffectValue(effect);

            // Start with base value
            value += baseValue;

            switch (effect.target)
            {
                case CardEffect.Target.All:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        if (playerMinionCount < 2)
                            value -= 9999;
                        else if (playerMinionCount > enemyMinionCount + 1)
                            value += 2*(playerMinionCount-enemyMinionCount);
                    }
                    else if (effect.type == CardEffect.Type.Heal || effect.type == CardEffect.Type.Buff)
                    {
                        if(playerMinionCount > enemyMinionCount + 2)
                            value += (playerMinionCount - enemyMinionCount);
                    }
                    break;

                case CardEffect.Target.AllAlly:
                case CardEffect.Target.AllAllyMinions:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        value -= 30*enemyMinionCount;
                    }
                    else if (effect.type == CardEffect.Type.Heal || effect.type == CardEffect.Type.Buff)
                    {
                        value += enemyMinionCount * 2;
                    }
                    break;

                case CardEffect.Target.AllEnemy:
                case CardEffect.Target.AllEnemyMinions:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        if (playerMinionCount < 2)
                            value -= 5;
                        else if (playerMinionCount > enemyMinionCount + 1)
                            value += 2 * (playerMinionCount - enemyMinionCount);
                    }
                    else if (effect.type == CardEffect.Type.Heal)
                        value -= 20 * playerMinionCount;
                    else if (effect.type == CardEffect.Type.Buff)
                    {
                        if (effect is BuffEffect buff)
                        {
                            if (buff.buffType is BuffType.IncreaseMaxHealth or BuffType.ActiveAttackBuff or BuffType.ActiveHealthBuff or BuffType.Attack)
                            {
                                if (playerMinionCount > 1)
                                    value -= 2 * playerMinionCount;
                                else
                                    value -= 40 * playerMinionCount;
                            }
                            else
                            {
                                value += 2 * playerMinionCount;
                            }
                        }
                    }
                    break;

                /*// -------------------- SUMMON --------------------
                case CardEffect.Target.None:
                    if (effect.type == CardEffect.Type.Summon)
                    {
                        int space = 7 - playerMinionCount;
                        if (space <= 0)
                            value -= 9999; // No room to summon
                        else
                            value += space * 2;
                    }
                    break;*/

                case CardEffect.Target.Self:
                    if (effect.type == CardEffect.Type.Buff || effect.type == CardEffect.Type.Heal)
                        value += 5;
                    break;

                case CardEffect.Target.Enemy:
                    if (effect.type == CardEffect.Type.Buff || effect.type == CardEffect.Type.Heal)
                        value -= 5;
                    break;
                default:
                    break;
            }
        }

        return value;
    }

    int GetEffectValue(CardEffect effect)
    {
        return effect.type switch
        {
            CardEffect.Type.Damage => effect.value,
            CardEffect.Type.Heal => BattleManager.Instance.enemyMinionList.Count==0 && BattleManager.Instance.enemy.currentHealth <= effect.value ? effect.value / 2 : -9999,
            CardEffect.Type.Draw => 10 - CardManager.Instance.enemyHand.Count > effect.value ? 2 : -9999,
            CardEffect.Type.Buff => GetBuffEffectValue(effect as BuffEffect),
            _ => 0,
        };
    }
    int GetBuffEffectValue(BuffEffect effect)
    {
        return effect.buffType switch
        {
            BuffType.Attack => effect.value + 2,
            BuffType.Shield => 2,
            BuffType.Taunt => 3,
            BuffType.ActiveAttackBuff => 4,
            BuffType.ActiveHealthBuff => 3,
            _ => 0,
        };
    }


    public override bool Equals(object obj)
    {
        if (obj is not CardAction other) return false;
        return instanceId == other.instanceId;
    }

    public override int GetHashCode()
    {
        return instanceId;
    }

}
