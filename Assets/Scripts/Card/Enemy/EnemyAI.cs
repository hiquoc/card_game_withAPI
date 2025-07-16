using System;
using System.Collections.Generic;
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
        /*bm = BattleManager.Instance;*/
        /*vz = ValidZone.Instance;*/
    }
    /*private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            GetTarget(CardEffect.Target.ChosenTarget);
    }*/
    public void PlayMinionCard()
    {
        /*GameObject cardObj = enemyHand.Find(c => c.GetComponent<CardDisplay>().card.type == Card.CardType.minion);*/
        isDone = false;
        GameObject cardObj = GetCard();
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
        Debug.Log("AI");
        CardDrag cardDrag = cardObj.GetComponent<CardDrag>();
        CardDisplay cardDisplay = cardDrag.GetComponent<CardDisplay>();
        if (cardDisplay.card.type == Card.CardType.minion)
        {
            GameObject minionHolderObj = Instantiate(rm.minionHolderPrefab, rm.enemyMinionPosition);
            cardDrag.minionHolderObj = minionHolderObj;
            RectTransform minionHolderRT = minionHolderObj.GetComponent<RectTransform>();
            cardDrag.minionHolderRT = minionHolderRT;
            Debug.Log("MinionCard");
            /*bm.PlayCard(cardDisplay.gameObject);*/
            /*cardDrag.lastMinionHolderPos = minionHolderRT.position;*/
        }
        else
        {
            Debug.Log("SpellCard");
        }
        StartCoroutine(rm.bm.PlayCard(cardDisplay.gameObject));
        /*cm.UpdateCardPosition(0, cm.playerHand, cm.playerHandPosition);*/

    }
    public GameObject GetCard()
    {
        GameObject cardObj = enemyHand[0];
        return cardObj;
    }
    public GameObject GetTarget(CardEffect.Target targetType)
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
}