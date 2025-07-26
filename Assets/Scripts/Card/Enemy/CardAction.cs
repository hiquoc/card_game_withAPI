using System.Collections.Generic;
using UnityEngine;
using static BuffEffect;

public class CardAction
{
    public static int nextInstanceId = 0;
    public int instanceId;
    public Card card;
    public GameObject cardObj;
    public int estimatedValue;

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
            value += GetEffectValue(effect);
        }
        return value;

    }
    int GetEffectValue(CardEffect effect)
    {
        return effect.type switch
        {
            CardEffect.Type.Damage => effect.value,
            CardEffect.Type.Heal => effect.value / 2,
            CardEffect.Type.Draw => 2,
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
