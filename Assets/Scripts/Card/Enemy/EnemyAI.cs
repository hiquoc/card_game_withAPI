using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public GameObject GetTargetToDealDamage(List<GameObject> targets, int value)
    {
        GameObject bestTarget = null;
        int bestScore = int.MaxValue;
        foreach (GameObject targetObj in targets)
        {
            ITarget itarget = null;
            if (targetObj.TryGetComponent(out MinionDisplay component))
                itarget = component.minion;
            else
                itarget = rm.bm.player;
            /*Debug.Log(itarget.GetGameObject().name);*/
            if (itarget == null) continue;
            int currentScore = Mathf.Abs(value - itarget.GetHealth());

            if (currentScore < bestScore)
            {
                bestScore = currentScore;
                bestTarget = targetObj;
            }
        }
        return bestTarget;
    }
    public GameObject GetTargetByTargetType(Card card, CardEffect.Target targetType)
    {
        List<GameObject> selectableTarget = new();
        BattleManager bm = rm.bm;
        if (targetType == CardEffect.Target.ChosenMinion)
        {
            selectableTarget.AddRange(bm.playerMinionList);
            selectableTarget.AddRange(bm.enemyMinionList);
        }
        else if (targetType == CardEffect.Target.ChosenTarget)
        {
            selectableTarget.Add(bm.player.gameObject);//0
            selectableTarget.Add(bm.enemy.gameObject);//1
            selectableTarget.AddRange(bm.playerMinionList);//(2:playerMinionList.Count+1]
            selectableTarget.AddRange(bm.enemyMinionList);//(playerMinionList.Count+2:enemyMinionList.Count+playerMinionList.Count+1]
        }
        if (card is MinionCard minionCard)
            selectableTarget.Remove(minionCard.GetGameObject());
        else
            selectableTarget.Remove(bm.tmpChar.GetGameObject());
        //Tam thoi chon ngau nhien
        /*for (int i = 0; i < 10; i++)
        {
            
            GameObject selectedTarget = selectableTarget[rand];
            Debug.Log(selectedTarget.name);
        }*/
        int rand = Random.Range(0, selectableTarget.Count);
        GameObject selectedTarget = selectableTarget[rand];
        /*if (rand == 0)
            target = bm.player;
        else if (rand == 1)
            target = bm.enemy;
        else if (rand > 1 && rand <= bm.playerMinionList.Count + 1)
            target = bm.playerMinionList[rand - 2].GetComponent<MinionDisplay>().card;
        else
            target = bm.enemyMinionList[rand - 2 - bm.playerMinionList.Count].GetComponent<MinionDisplay>().card;*/
        /*Debug.Log(selectableTarget.Count);*/
        return selectedTarget;
    }
    public IEnumerator DoAttackCoroutine()
    {
        /*if (rm.bm.enemyMinionList.Count == 0) yield break;*/
        /*Debug.Log(rm.bm.enemyMinionList.Count);*/
        /*List<GameObject> attackerList = new(rm.bm.enemyMinionList);*/
        if (rm.bm.enemy.CanAttack())
        {
            Character enemy = rm.bm.enemy;
            int attackDamage = enemy.GetAttack();
            GameObject target = GetAttackTarget(attackDamage);
            if (target == null)
                Debug.LogWarning("Cant get Target");
            if (target.TryGetComponent(out MinionDisplay component))
                enemy.AttackTarget(component.minion);
            else
                enemy.AttackTarget(rm.bm.player);
            yield return null;
            yield return new WaitUntil(() => !rm.bm.isWaiting);

        }
        List<GameObject> minionCopyList = new(rm.bm.enemyMinionList);
        foreach (GameObject minionObj in minionCopyList)
        {
            if (minionObj == null) continue;
            MinionCard minionCard = minionObj.GetComponent<MinionDisplay>().minion;
            if (minionCard != null && minionCard.CanAttack())
            {
                int attackDamage = minionCard.GetAttack();
                GameObject target = GetAttackTarget(attackDamage);
                if (target == null)
                    Debug.LogWarning("Cant get Target");
                if (target.TryGetComponent(out MinionDisplay component))
                    minionCard.AttackTarget(component.minion);
                else
                    minionCard.AttackTarget(rm.bm.player);
                yield return null;
                yield return new WaitUntil(() => !rm.bm.isWaiting);
            }
        }

    }
    GameObject GetAttackTarget(int value)
    {
        List<GameObject> list = new();
        bool hasTaunt = false;
        foreach (GameObject g in rm.bm.playerMinionList)
        {
            if (g == null) continue;
            MinionCard minion = g.GetComponent<MinionDisplay>().minion;
            if (minion == null) continue;
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
        return GetTargetToDealDamage(list, value);

    }
}