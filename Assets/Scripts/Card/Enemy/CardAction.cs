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
    Character enemy=>BattleManager.Instance.enemy;
    Character player => BattleManager.Instance.player;

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
            if (enemyMinionCount == 6)
            {
                value -= 9999;
                return value;
            }
            if (enemyMinionCount > playerMinionCount)
                value += (enemyMinionCount - playerMinionCount) * 3;

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

            value += baseValue;

            switch (effect.target)
            {
                case CardEffect.Target.All:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        value += (playerMinionCount - enemyMinionCount)*Mathf.Min(5,effect.value);

                        if (playerMinionCount < 2 || enemyMinionCount>playerMinionCount||value>=enemy.GetHealth())
                            value -= 9999;
                    }
                    else if (effect.type == CardEffect.Type.Heal || effect.type == CardEffect.Type.Buff)
                    {
                        if (playerMinionCount > enemyMinionCount + 2)
                            value += (playerMinionCount - enemyMinionCount) * Mathf.Min(5, effect.value);
                        else
                            value -= 10;
                    }
                    break;
                case CardEffect.Target.AllMinions:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        value += (playerMinionCount - enemyMinionCount) * Mathf.Min(5, effect.value) ;

                        if (playerMinionCount < 2 || enemyMinionCount > playerMinionCount)
                            value -= 9999;
                    }
                    else if (effect.type == CardEffect.Type.Heal || effect.type == CardEffect.Type.Buff)
                    {
                        if (playerMinionCount > enemyMinionCount + 2)
                            value += (playerMinionCount - enemyMinionCount) * Mathf.Min(5, effect.value);
                        if (enemyMinionCount == 0 || playerMinionCount > enemyMinionCount + 1)
                            value -= 9999;
                    }
                    break;

                case CardEffect.Target.AllAlly:
                case CardEffect.Target.AllAllyMinions:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        value -=-5- 5 * enemyMinionCount;
                    }
                    else if (effect.type == CardEffect.Type.Heal)
                    {
                        int totalHealingPotential = 0;
                        foreach (GameObject g in BattleManager.Instance.enemyMinionList)
                        {
                            MinionCard minionCard = g.GetComponent<MinionDisplay>().minion;
                            int missing = minionCard.maxHealth - minionCard.GetHealth();
                            totalHealingPotential += Mathf.Clamp(missing, 0, effect.value) / 2;
                        }

                        if (totalHealingPotential <= 0)
                        {
                             return -9999;
                        }

                        value += totalHealingPotential;
                    }
                    else if (effect is BuffEffect buff)
                    {
                        if (buff.buffType is BuffType.Attack or BuffType.ActiveAttackBuff 
                            or BuffType.ActiveHealthBuff or BuffType.IncreaseMaxHealth)
                        {
                            int totalBuffPotential = 0;
                            foreach (GameObject g in BattleManager.Instance.enemyMinionList)
                            {
                                totalBuffPotential += effect.value / 2;
                            }

                            if (totalBuffPotential <= 0 )
                            {
                                return -9999;
                            }
                            value += totalBuffPotential;
                        }
                        

                    }
                     break;

                case CardEffect.Target.AllEnemy:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        int totalDamage = effect.value * playerMinionCount;
                        value += totalDamage;
                        if (player.GetHealth() <= value)
                            value += 9999;
                        if (playerMinionCount < 2)
                            value -= 5;
                    }
                    else if (effect is BuffEffect buff)
                    {
                        if (buff.buffType is BuffType.IncreaseMaxHealth or BuffType.ActiveAttackBuff or BuffType.ActiveHealthBuff or BuffType.Attack)
                        {
                            value -= 3 * playerMinionCount;
                        }
                        else
                        {
                            value += 3 * playerMinionCount;
                        }
                    }
                    break;
                case CardEffect.Target.AllEnemyMinions:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        int totalDamage = effect.value * playerMinionCount;
                        value += totalDamage;

                        if (playerMinionCount < 2)
                            value -= 5;
                    }
                    else if (effect.type == CardEffect.Type.Heal)
                        value -= 2 * playerMinionCount;
                    else if (effect is BuffEffect buff)
                    {
                        if (buff.buffType is BuffType.IncreaseMaxHealth or BuffType.ActiveAttackBuff or BuffType.ActiveHealthBuff or BuffType.Attack)
                        {
                            if (playerMinionCount > 1)
                                value -= 2 * playerMinionCount;
                            else
                                value -= 4 * playerMinionCount;
                        }
                        else
                        {
                            value += 2 * playerMinionCount;
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
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        value += effect.value;
                        if (effect.value >= enemy.currentHealth)
                            value += 1000;
                    }
                        
                    else if (effect.type == CardEffect.Type.Buff || effect.type == CardEffect.Type.Heal)
                        value -= 5;
                    break;
                case CardEffect.Target.ChosenTarget:
                    if (effect.type == CardEffect.Type.Damage)
                    {
                        value += effect.value;
                        if (effect.value >= enemy.currentHealth)
                            value += 1000;
                    }
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
            CardEffect.Type.Damage =>Mathf.Min(5,effect.value),
            CardEffect.Type.Heal =>
            BattleManager.Instance.playerMinionList.Exists(
                g => g.GetComponent<MinionDisplay>().minion.GetHealth()
                < g.GetComponent<MinionDisplay>().minion.maxHealth)
                ? effect.value : -10,
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
            BuffType.Taunt => enemy.GetHealth()<10?9999:3,
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
