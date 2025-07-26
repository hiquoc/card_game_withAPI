using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BuffManager : MonoBehaviour
{
    public static BuffManager Instance;
    private Dictionary<Card, List<BuffInstance>> activeBuffMap = new();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void ApplyBuff(Card card, BuffEffect effect, ITarget target, int character)
    {
        if (!activeBuffMap.TryGetValue(card, out var buffList))
        {
            buffList = new List<BuffInstance>();
            activeBuffMap[card] = buffList;
        }

        var existingInstance = buffList.FirstOrDefault(i => i.effect == effect);

        if (existingInstance == null)
        {
            var newInstance = new BuffInstance(effect, character);
            ApplyBuffEffect(target, newInstance);
            newInstance.affectedTargets.Add(target);
            buffList.Add(newInstance);
        }
        else if (!existingInstance.affectedTargets.Contains(target))
        {
            ApplyBuffEffect(target, existingInstance);
            existingInstance.affectedTargets.Add(target);
        }
    }

    public void RemoveBuff(Card card)
    {
        /*Debug.Log("RemoveBuff");*/
        if (!activeBuffMap.ContainsKey(card)) return;
        foreach (BuffInstance instance in activeBuffMap[card])
        {
            foreach (ITarget target in instance.affectedTargets)
            {
                if (target != null)
                    RemoveBuffEffect(target, instance);
            }
        }
        foreach (List<BuffInstance> list in activeBuffMap.Values)
        {
            foreach (BuffInstance instance in list)
            {
                instance.affectedTargets.Remove(card as ITarget);
            }
        }
        activeBuffMap.Remove(card);

    }
    void ApplyBuffEffect(ITarget target, BuffInstance buffInstance)
    {
        Debug.Log(buffInstance.effect.buffType);
        switch (buffInstance.effect.buffType)
        {
            case BuffEffect.BuffType.ActiveAttackBuff:
                target.IncreaseAttack(buffInstance.effect.value);
                break;
            case BuffEffect.BuffType.ActiveAttackDebuff:
                target.DecreaseAttack(buffInstance.effect.value);
                break;
            case BuffEffect.BuffType.ActiveHealthBuff:
                target.IncreaseHealth(buffInstance.effect.value);
                break;
            case BuffEffect.BuffType.ActiveHealthDebuff:
                target.DecreaseHealth(buffInstance.effect.value);
                break;
        }
    }
    void RemoveBuffEffect(ITarget target, BuffInstance buffInstance)
    {
        /*Debug.Log("RemoveBuffEffect");*/
        switch (buffInstance.effect.buffType)
        {
            case BuffEffect.BuffType.ActiveAttackBuff:
                target.DecreaseAttack(buffInstance.effect.value);
                break;
            case BuffEffect.BuffType.ActiveAttackDebuff:
                target.IncreaseAttack(buffInstance.effect.value);
                break;
            case BuffEffect.BuffType.ActiveHealthBuff:
                target.DecreaseHealth(buffInstance.effect.value);
                break;
            case BuffEffect.BuffType.ActiveHealthDebuff:
                target.IncreaseHealth(buffInstance.effect.value);
                break;
        }
    }

    public void OnNewMinion(ITarget target, int character)
    {
        foreach (List<BuffInstance> value in activeBuffMap.Values)
        {
            foreach (BuffInstance instance in value)
            {
                if (character == instance.character && !instance.affectedTargets.Contains(target))
                {
                    Debug.Log("OnNewMinion");
                    ApplyBuffEffect(target, instance);
                    instance.affectedTargets.Add(target);
                }
            }
        }
    }


}