using NUnit.Framework.Internal;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemyAI : MonoBehaviour
{
    public static EnemyAI Instance;
    public bool isDone = false;
    public List<GameObject> enemyHand => CardManager.Instance.enemyHand;
    public object target;

    ReferenceManager rm;
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        rm = ReferenceManager.Instance;
    }
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            GetTarget(CardEffect.Target.ChosenTarget);
    }*/

    public List<GameObject> FindBestCardCombo(List<GameObject> currentHand, int startMana, int beamWidth = 3, int maxDepth = 5)
    {
        /*isDone = false;*/
        List<CardAction> allActions = new();
        foreach (GameObject cardObj in currentHand)
        {
            Card card = cardObj.GetComponent<CardDisplay>().card;
            allActions.Add(new CardAction(card, cardObj));
        }
        AIState initState = new()
        {
            remainingMana = startMana,
            remainingHand = new List<CardAction>(allActions),
            playedCards = new(),
            score = 0,
        };
        List<AIState> beam = new() { initState };

        for (int depth = 0; depth < maxDepth; depth++)
        {
            List<AIState> nextStates = new();
            foreach (AIState state in beam)
            {
                foreach (CardAction action in state.remainingHand)
                {
                    /*Debug.Log($"Trying to play {action.card.id}, mana: {action.card.mana}, remainingMana: {state.remainingMana}");*/
                    int cost = action.card.mana;
                    if (cost <= state.remainingMana)
                    {
                        AIState newState = state.Clone();
                        newState.remainingMana -= cost;
                        newState.playedCards.Add(action);
                        newState.remainingHand.Remove(action);
                        newState.score += action.estimatedValue;
                        nextStates.Add(newState);
                    }
                }
            }
            if (nextStates.Count == 0) break;
            beam = nextStates.OrderByDescending(state => state.score).Take(beamWidth).ToList();
            /*Debug.Log(beam.Count);*/
        }
        AIState best = beam.OrderByDescending(s => s.score).FirstOrDefault();
        List<GameObject> results = new();
        if (best != null && best.playedCards.Count > 0)
        {
            /*Debug.Log("Best combo found: ");*/
            foreach (CardAction action in best.playedCards)
            {
                /*Debug.Log($"Play {action.card.id} (value: {action.estimatedValue}, mana: {action.card.mana})");*/
                results.Add(action.cardObj);
            }
        }
        else
        {
            Debug.Log("Cant find combo: ");
        }
        /*isDone = true;*/
        return results;
    }

    public void PlayCard(GameObject cardObj)
    {
        /*GameObject cardObj = enemyHand.Find(c => c.GetComponent<CardDisplay>().card.type == Card.CardType.minion);*/
        isDone = false;
        /*GameObject cardObj = GetCard();*/
        if (!cardObj)
        {
            throw new Exception("Card khong ton tai");
        }
        else
        {
            /*Debug.Log(cardObj);*/
        }
        DropCardAI(cardObj);
    }
    public void DropCardAI(GameObject cardObj)
    {
        /*Debug.Log("AI");*/
        CardDrag cardDrag = cardObj.GetComponent<CardDrag>();
        CardDisplay cardDisplay = cardDrag.GetComponent<CardDisplay>();
        if (cardDisplay.card.type == Card.CardType.minion)
        {
            GameObject minionHolderObj = Instantiate(rm.minionHolderPrefab, rm.enemyMinionPosition);
            cardDrag.minionHolderObj = minionHolderObj;
            RectTransform minionHolderRT = minionHolderObj.GetComponent<RectTransform>();
            cardDrag.minionHolderRT = minionHolderRT;
        }
        else
        {
            /*Debug.Log("SpellCard");*/
        }
        StartCoroutine(rm.bm.PlayCard(cardDisplay.gameObject));


    }
    public GameObject GetTargetToDealDamage(List<GameObject> targets,int value,int attackerHP=0)
    {
        GameObject bestTarget = null;
        int bestScore = int.MinValue;

        if (targets.Contains(rm.bm.playerDisplay.gameObject) && value >= rm.bm.player.GetHealth())
            return rm.bm.playerDisplay.gameObject;

        int enemyMinionDamage = 0;
        foreach (GameObject playerMinion in rm.bm.playerMinionList) {
            enemyMinionDamage += playerMinion.GetComponent<MinionDisplay>().minion.GetAttack();
        }
        if(rm.bm.enemy.GetHealth() <= enemyMinionDamage)
            targets.Remove(rm.bm.playerDisplay.gameObject);
        foreach (GameObject targetObj in targets)
        {
            if (targetObj == null) continue;

            ITarget target = targetObj.TryGetComponent(out MinionDisplay md) ? md.minion : rm.bm.player;
            if (target == null) continue;

            int score = 0;
            if (attackerHP != 0)
            {
                int targetHP = target.GetHealth();
                int targetATK = target.GetAttack();

                if (value >= targetHP && targetATK < attackerHP)
                    score += 30; // strong trade
                else if (value >= targetHP)
                    score += 20; // still a kill
                else if (attackerHP > targetATK)
                    score += 10; // at least attacker survives
                else
                    score -= 50; // bad trade
                int effectiveDamage = Mathf.Min(value, targetHP);
                score += effectiveDamage * 3;
                int overkill = value - targetHP;
                if (overkill > 0)
                    score -= overkill * 2;
                score += target.GetAttack() * 3;
                score += target.GetHealth() * 2;
            }

            if (target is MinionCard minion)
            {
                if (minion.hasTaunt) score += 10;
                if (minion.onDeath.Count != 0) score -= 20;
                if (minion.onStartOfTurn.Count != 0) score += 20;
                if (minion.onEndOfTurn.Count != 0) score += 20;
                if (BuffManager.Instance.IsBuffing(minion)) score += 30;
            }
            else
            {
                score += 20;
            }

            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = targetObj;
            }
        }

        return bestTarget;
    }
    public GameObject GetTargetToBuff(List<GameObject> targets,CardEffect effect)
    {
        Debug.Log("Buff");
        GameObject bestTarget = null;
        int bestScore = int.MinValue;

        foreach (GameObject targetObj in targets)
        {
            if (targetObj == null) continue;

            ITarget target = targetObj.TryGetComponent(out MinionDisplay md) ? md.minion : rm.bm.enemy;
            if (target == null) continue;

            int score = 0;

            int targetHP = target.GetHealth();
            int targetATK = target.GetAttack();

            switch (effect)
            {
                case HealEffect:
                    if (targetHP == target.GetMaxHealth()) break; 
                    int missingHP = target.GetMaxHealth() - targetHP;
                    int actualHeal = Mathf.Min(effect.value, missingHP);
                    score += actualHeal * 5;
                    break;

                case BuffEffect statBuff: 
                    if(statBuff.buffType is BuffEffect.BuffType.Attack)       
                        score += effect.value * 3;
                    else if(statBuff.buffType is BuffEffect.BuffType.IncreaseMaxHealth)
                        score += effect.value * 2;

                    if (targetHP <= 3) 
                        score += 5;
                    if (targetATK <= 2)
                        score += 5;
                    break;

                default:
                    break;
            }
            if (target is MinionCard minion)
            {
                if (minion.hasTaunt) score += 30;
                if (minion.onStartOfTurn.Count > 0) score += 30;
                if (minion.onEndOfTurn.Count > 0) score += 30;
                if (minion.onDeath.Count > 0) score -= 20;
                if (BuffManager.Instance.IsBuffing(minion)) score += 40;
            }
            else
                score += 10;
            if (score > bestScore)
            {
                bestScore = score;
                bestTarget = targetObj;
            }
        }
        return bestTarget;
    }

    public GameObject GetTargetByTargetType(Card card, CardEffect cardEffect)
    {
        List<GameObject> selectableTarget = new();
        BattleManager bm = rm.bm;
        if (cardEffect.target == CardEffect.Target.ChosenTarget)
        {
            if (cardEffect is DamageEffect)
            {
                selectableTarget.Add(bm.player.gameObject);
                selectableTarget.AddRange(bm.playerMinionList);
            }
            else
            {
                selectableTarget.Add(bm.enemy.gameObject);
                selectableTarget.AddRange(bm.enemyMinionList);
            }
        }
        if (card is MinionCard minionCard)
            selectableTarget.Remove(minionCard.GetGameObject());
        else
            selectableTarget.Remove(bm.tmpChar.GetGameObject());
        //Thay doi selectedTarget thanh ham lay muc tieu dua theo loai effect
        GameObject selectedTarget;
        if (cardEffect is DamageEffect)
            selectedTarget = GetTargetToDealDamage(selectableTarget,cardEffect.value);
        else
            selectedTarget = GetTargetToBuff(selectableTarget, cardEffect);
            /*Debug.Log(selectableTarget.Count);*/
        return selectedTarget;
    }
    public IEnumerator DoAttackCoroutine()
    {
        List<GameObject> minionCopyList = new(rm.bm.enemyMinionList);
        foreach (GameObject minionObj in minionCopyList)
        {
            if (minionObj == null) continue;
            MinionCard minionCard = minionObj.GetComponent<MinionDisplay>().minion;
            if (minionCard != null && minionCard.CanAttack())
            {
                GameObject target = GetAttackTarget(minionCard);
                if (target == null)
                {
                    Debug.LogWarning("Cant get Target");
                    continue;
                }
                if (target.TryGetComponent(out MinionDisplay component))
                    minionCard.AttackTarget(component.minion);
                else
                {
                    if (rm.bm.player.GetHealth() <= 0)
                        yield break;
                    minionCard.AttackTarget(rm.bm.player);
                }      
                yield return null;
                while (rm.bm.isWaiting ||rm.bm.minionDying)
                {
                    yield return new WaitForSeconds(0.2f);
                }
                Debug.Log(rm.bm.isWaiting);
                Debug.Log(rm.bm.minionDying);

            }
        }
        if (rm.bm.enemy.CanAttack())
        {
            Character enemy = rm.bm.enemy;
            GameObject target = GetAttackTarget(enemy);
            if (target == null)
                Debug.LogWarning("Cant get Target");
            if (target.TryGetComponent(out MinionDisplay component))
                enemy.AttackTarget(component.minion);
            else
            {
                if (rm.bm.player.GetHealth() <= 0)
                    yield break;
                enemy.AttackTarget(rm.bm.player);
            }            
            yield return null;
            while (rm.bm.isWaiting || rm.bm.minionDying)
            {
                yield return new WaitForSeconds(0.2f);
            }
            /*Debug.Log(rm.bm.isWaiting);
            Debug.Log(rm.bm.minionDying);*/

        }
    }
    GameObject GetAttackTarget(ITarget attacker)
    {
        List<GameObject> list = new();
        bool hasTaunt = false;
        foreach (GameObject g in rm.bm.playerMinionList)
        {
            if (g == null) continue;
            MinionCard minion = g.GetComponent<MinionDisplay>().minion;
            if (minion == null ||minion.isDying) continue;
            if (minion.hasTaunt)
            {
                if (!hasTaunt)
                {
                    hasTaunt = true;
                    list.Clear();
                }
                list.Add(g);
            }
            else if (!hasTaunt)
                list.Add(g);
        }
        if (!hasTaunt)
            list.Add(rm.bm.player.GetGameObject());
        /*Debug.Log(list.Count);*/
        return GetTargetToDealDamage(list, attacker.GetAttack(),attacker.GetHealth());

    }
}