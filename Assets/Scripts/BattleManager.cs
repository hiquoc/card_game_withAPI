using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public bool isWaiting = false;
    public int turn = 0;
    int turnNum = 0;
    public CharacterDisplay playerDisplay, enemyDisplay;
    public Character player, enemy;
    Character tmpChar => turn == 0 ? player : enemy;
    Character reversedTmpChar => turn == 0 ? enemy : player;

    public List<GameObject> playerMinionList, enemyMinionList;
    List<GameObject> tmpMinionList => turn == 0 ? playerMinionList : enemyMinionList;
    List<GameObject> reversedTmpMinionList => turn == 1 ? playerMinionList : enemyMinionList;

    List<ActiveEffect> playerActiveEffectList, enemyActiveEffectList;
    List<ActiveEffect> tmpActiveEffectList => turn == 0 ? playerActiveEffectList : enemyActiveEffectList;

    public Button endTurnBtn;


    ReferenceManager rm;

    PendingEffect pendingEffect;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        SetupCharacter();

        rm = ReferenceManager.Instance;
        playerMinionList = new List<GameObject>();
        enemyMinionList = new List<GameObject>();
        playerActiveEffectList = new List<ActiveEffect>();
        enemyActiveEffectList = new List<ActiveEffect>();
        endTurnBtn.onClick.AddListener(() => StartCoroutine(EndPlayerTurn()));

    }
    private void Start()
    {
        endTurnBtn.interactable = true;
        CardManager.Instance.DrawInitHand();
        Invoke(nameof(StartPlayerTurn), 2f);
    }
    //Goi api lay du lieu nhan vat
    void SetupCharacter()
    {
        player = playerDisplay.character;
        playerDisplay.SetUp(30, "Mother");

        enemy = enemyDisplay.character;
        enemyDisplay.SetUp(30, "Thug");
    }

    //Turn logic
    private void StartPlayerTurn()
    {
        player.currentMana = turnNum <= 10 ? turnNum : 10;
        UpdateMana();
        rm.cm.DrawPlayerCard();
        ResetAttack();
        /*if (player.GetAttack() > 0 || playerMinionList.Count > 0)*/
        rm.am.drawable = true;
    }
    IEnumerator EndPlayerTurn()
    {
        Debug.Log(rm.aq.isPlaying);
        yield return new WaitUntil(() => rm.aq.isPlaying == false);// cho animation
        endTurnBtn.interactable = false;
        rm.am.drawable = false;
        CallActiveEffect(false);
        turn = turn == 0 ? 1 : 0;
        StartEnemyTurn();
    }

    private void StartEnemyTurn()
    {
        enemy.currentMana = turnNum <= 10 ? turnNum : 10;
        UpdateMana();
        rm.cm.DrawEnemyCard();
        ResetAttack();
        StartCoroutine(EnemyTurnCoroutine());
    }
    IEnumerator EnemyTurnCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        rm.ai.PlayMinionCard();
        yield return new WaitUntil(() => rm.ai.isDone == true); //cho chon muc tieu
        yield return new WaitUntil(() => rm.aq.isPlaying == false);// cho animation
        Debug.Log("isDone");
        yield return new WaitForSeconds(1f);
        CallActiveEffect(false);
        //Start Player Turn
        turn = turn == 0 ? 1 : 0;
        turnNum++;
        StartPlayerTurn();
        endTurnBtn.interactable = true;
    }
    void UpdateMana()
    {
        int currentMaxMana = Mathf.Min(turnNum, tmpChar.maxMana);
        if (turn == 0)
        {
            /*player.UpdateMana($"{player.currentMana}/{currentMaxMana}");*/
            player.SetMana(player.currentMana);
        }
        else
        {
            /*enemy.UpdateMana($"{player.currentMana}/{currentMaxMana}");*/
            enemy.SetMana(enemy.currentMana);
        }
    }
    void ResetAttack()
    {
        tmpChar.SetCanAttack(true);
        foreach (GameObject minionObj in tmpMinionList)
        {
            MinionDisplay minionDisplay = minionObj.GetComponent<MinionDisplay>();
            minionDisplay.minion.SetCanAttack(true);
        }
    }

    //Card logic
    public IEnumerator PlayCard(GameObject cardObj)
    {
        if (isWaiting)
        {
            Debug.Log("You cant do that right now");
            yield break;
        }
        if (turn == 0)
            rm.cm.playerHand.Remove(cardObj);
        else
            rm.cm.enemyHand.Remove(cardObj);

        yield return null;
        /*Debug.Log("PlayCard Start");*/
        Card card = cardObj.GetComponent<CardDisplay>().card;
        if (card is SpellCard spellCard)
        {
            Debug.Log("This is a spell with " + spellCard.mana + " mana cost.");
            tmpChar.DecreaseMana(spellCard.mana);
            yield return StartCoroutine(cardObj.GetComponent<CardDrag>().MoveSpellCard());
            yield return cardObj.transform.DOScale(0f, 0.1f).WaitForCompletion();
        }
        else if (card is Minion minionCard)
        {
            Debug.Log("This is a minon card with " + minionCard.currentAttack + " attack");
            tmpChar.DecreaseMana(minionCard.mana);

            yield return StartCoroutine(cardObj.GetComponent<CardDrag>().MoveMinionCard());
            yield return cardObj.transform.DOScale(0f, 0.2f).WaitForCompletion();

            GameObject minion = Instantiate(rm.minionPrefab);
            minion.GetComponent<MinionDisplay>().SetupMinion(minionCard);

            RectTransform minionRT = minion.GetComponent<RectTransform>();
            minionRT.SetParent(turn == 0 ? rm.playerMinionPosition : rm.enemyMinionPosition);
            tmpMinionList.Add(minion);
            Destroy(cardObj.GetComponent<CardDrag>().minionHolderObj);
            Canvas.ForceUpdateCanvases();
            /* minion.SetActive(false);


             minion.SetActive(true);*/
        }

        yield return new WaitForSeconds(0.1f);
        PlayOnPlayEffect(card);
        ReferenceManager.Instance.validZone.SetActive(false);


        if (cardObj != null)
        {
            Destroy(cardObj);
        }
        /*UpdateMana();*/
    }
    //Play effect
    void PlayOnPlayEffect(Card card)
    {
        foreach (CardEffect effect in card.onPlay)
        {
            if (effect.target == CardEffect.Target.ChosenMinion || effect.target == CardEffect.Target.ChosenTarget)
            {
                pendingEffect = new PendingEffect { pendingCardEffect = effect, pendingCard = card };
                if (turn == 0)
                    EnterSelectTarget(effect.target);
                else
                    EnemyEnterSelectTarget(effect.target);
            }
            else
            {
                ApplyEffect(card, effect, null);
            }
        }
    }
    void PlayOnDeathEffect(Card card)
    {
        foreach (CardEffect c in card.onDeath)
        {
            /*Debug.Log(c.target);*/
            /*if (c.target == CardEffect.Target.ChosenMinion || c.target == CardEffect.Target.ChosenTarget)
            {
                pendingEffect = new PendingEffect { pendingCardEffect = c, pendingCard = card };
                if (turn == 0)
                    EnterSelectTarget(c.target);
                else
                    EnemyEnterSelectTarget(c.target);
            }
            else
            {
                ApplyEffect(card, c, null, c.target);
            }*/
            ApplyEffect(card, c, null);
        }
    }

    public IEnumerator OnMinionDeath(GameObject minionObj, bool haveOnDeathEffect)
    {
        yield return new WaitUntil(() => !isWaiting);
        /*        Debug.Log("OnDeath");*/
        if (haveOnDeathEffect)
        {
            PlayOnDeathEffect(minionObj.GetComponent<MinionDisplay>().minion);
            yield return null;//Wait for effect animation later
        }
        if (playerMinionList.Contains(minionObj))
        {
            playerMinionList.Remove(minionObj);
        }
        else
            enemyMinionList.Remove(minionObj);
        Destroy(minionObj);
    }
    /////////Player selecting target
    private void EnterSelectTarget(CardEffect.Target targetType)
    {
        /*Debug.Log("Enter");*/
        List<GameObject> selectableTarget = new();
        if (targetType == CardEffect.Target.ChosenMinion)
        {
            selectableTarget.AddRange(playerMinionList);
            selectableTarget.AddRange(enemyMinionList);
        }
        else if (targetType == CardEffect.Target.ChosenTarget)
        {
            selectableTarget.Add(player.gameObject);
            selectableTarget.Add(enemy.gameObject);
            selectableTarget.AddRange(playerMinionList);
            selectableTarget.AddRange(enemyMinionList);
        }
        foreach (var target in selectableTarget)
        {
            if (target.TryGetComponent(out SelectableTarget currentTarget))
            {
                currentTarget.SetTargetSelectable(true);
                currentTarget.EnableHighlight();
            }
        }
        if (pendingEffect.pendingCard.type == 0)
            rm.am.ArrowForSelectingATarget(tmpChar.gameObject.GetComponent<RectTransform>());
        else
        {
            Minion minion = pendingEffect.pendingCard as Minion;
            rm.am.ArrowForSelectingATarget(minion.display.gameObject.GetComponent<RectTransform>());
        }
        CardDrag.canDrag = false;
        CardHover.canHover = false;
    }
    public void OnSelectTarget(GameObject selected)
    {
        /*Debug.Log("OnSelectTarget");*/
        if (pendingEffect == null) return;
        CardEffect cardEffect = pendingEffect.pendingCardEffect;
        Card card = pendingEffect.pendingCard;

        ITarget selectedTarget = null;
        if (selected.TryGetComponent(out CharacterDisplay character))
        {
            selectedTarget = character.character;
        }
        else if (selected.TryGetComponent(out MinionDisplay minion))
        {
            selectedTarget = minion.minion;
        }
        if (selectedTarget != null)
        {
            ApplyEffect(card, cardEffect, selectedTarget);
        }
        pendingEffect = null;
        ExitSelectTarget();
    }

    private void ExitSelectTarget()
    {
        /*Debug.Log("ExitSelectTarget");*/
        foreach (var target in FindObjectsByType<SelectableTarget>(FindObjectsSortMode.None))
        {
            target.SetTargetSelectable(false);
            target.DisableHighlight();
        }
        rm.am.StopDrawing();
        /*rm.am.isSelecting = false;*/
        CardDrag.canDrag = true;
        CardHover.canHover = true;
    }

    /////////Enemy AI selecting target
    private void EnemyEnterSelectTarget(CardEffect.Target targetType)
    {

        List<GameObject> selectableTarget = new();
        if (targetType == CardEffect.Target.ChosenMinion)
        {
            selectableTarget.AddRange(playerMinionList);
            selectableTarget.AddRange(enemyMinionList);
        }
        else if (targetType == CardEffect.Target.ChosenTarget)
        {
            selectableTarget.Add(player.gameObject);
            selectableTarget.Add(enemy.gameObject);
            selectableTarget.AddRange(playerMinionList);
            selectableTarget.AddRange(enemyMinionList);
        }
        foreach (var target in selectableTarget)
        {
            if (target.TryGetComponent(out SelectableTarget currentTarget))
            {
                currentTarget.SetTargetSelectable(true);
                currentTarget.EnableHighlight();
            }
        }
        rm.am.EnemyArrowForSelectingATarget(tmpChar.gameObject.GetComponent<RectTransform>(), targetType);
        CardDrag.canDrag = false;
        CardHover.canHover = false;
    }
    //Apply Effect Logic
    void ApplyEffect(Card card, CardEffect cardEffect, ITarget target)
    {
        /*Debug.Log("Apply");*/

        int value = cardEffect.value;
        switch (cardEffect.target)
        {
            case CardEffect.Target.Self:
                ApplyEffectToTarget(card, cardEffect, turn == 0 ? player : enemy, value);
                break;
            case CardEffect.Target.Enemy:
                ApplyEffectToTarget(card, cardEffect, turn == 1 ? player : enemy, value);
                break;
            case CardEffect.Target.ChosenTarget:
                ApplyEffectToTarget(card, cardEffect, target, value);
                break;
            case CardEffect.Target.ChosenMinion:
                ApplyEffectToTarget(card, cardEffect, target, value);
                break;
            case CardEffect.Target.All:
                ApplyEffectToAll(card, cardEffect, value);
                break;
            case CardEffect.Target.AllMinions:
                ApplyEffectToAllMinions(card, cardEffect, value);
                break;
            case CardEffect.Target.AllEnemyMinions:
                ApplyEffectToAllEnemyMinions(card, cardEffect, value);
                break;
            case CardEffect.Target.AllAllyMinions:
                ApplyEffectToAllAllyMinions(card, cardEffect, value);
                break;
            case CardEffect.Target.RandomAllyMinion:
                ApplyEffectToTarget(card, cardEffect, GetRandomAllyMinion(card), value);
                break;
            case CardEffect.Target.RandomEnemyMinion:
                ApplyEffectToTarget(card, cardEffect, GetRandomEnemyMinion(card), value);
                break;
        }
    }

    public void ApplyEffectToTarget(Card card, CardEffect effect, ITarget target, int value)
    {
        switch (effect.type)
        {
            case CardEffect.Type.Damage:
                target.TakeDamage(value);
                break;
            case CardEffect.Type.Heal:
                target.RestoreHealth(value);
                break;
            case CardEffect.Type.Buff:
                switch (effect.buffType)
                {
                    case CardEffect.BuffType.None:
                        break;
                    case CardEffect.BuffType.Attack:
                        target.IncreaseAttack(value); break;
                    case CardEffect.BuffType.Shield:
                        target.IncreaseShield(value); break;
                    case CardEffect.BuffType.HealOverTime:
                        /*target.IncreaseAttack(value); */
                        break;
                    case CardEffect.BuffType.DealDamageOverTime:
                        target.TakeDamage(value);
                        break;
                }
                if (effect.duration > 0 && !IsInActiveList(card, effect))
                {
                    tmpActiveEffectList.Add(new(card, effect, effect.duration, target));
                    Debug.Log("Added new active effect");
                }
                break;
            default:
                Debug.Log("Unchecked effect type: " + effect.type);
                break;
        }
        rm.ai.isDone = true; //Tam thoi de logic cho luot cua ai o day
    }

    private void ApplyEffectToAll(Card card, CardEffect effect, int value)
    {
        ApplyEffectToAllMinions(card, effect, value);
        ApplyEffectToTarget(card, effect, player, value);
        ApplyEffectToTarget(card, effect, enemy, value);
    }

    private void ApplyEffectToAllMinions(Card card, CardEffect effect, int value)
    {
        foreach (GameObject obj in playerMinionList)
        {
            ITarget target = obj.GetComponent<MinionDisplay>().minion;
            ApplyEffectToTarget(card, effect, target, value);
        }
        foreach (GameObject obj in enemyMinionList)
        {
            ITarget target = obj.GetComponent<MinionDisplay>().minion;
            ApplyEffectToTarget(card, effect, target, value);
        }
    }
    private void ApplyEffectToAllEnemyMinions(Card card, CardEffect effect, int value)
    {
        /*Debug.Log("ApplyEffectToAllEnemyMinions");*/
        /*Debug.Log(GetEnemyMinionListFromACard(card).Count);*/
        foreach (GameObject obj in GetEnemyMinionListFromACard(card))
        {
            ITarget target = obj.GetComponent<MinionDisplay>().minion;
            ApplyEffectToTarget(card, effect, target, value);
        }
    }
    private void ApplyEffectToAllAllyMinions(Card card, CardEffect effect, int value)
    {
        foreach (GameObject obj in GetAllyMinionListFromACard(card))
        {
            ITarget target = obj.GetComponent<MinionDisplay>().minion;
            ApplyEffectToTarget(card, effect, target, value);
        }
    }


    Minion GetRandomAllyMinion(Card card)
    {
        List<GameObject> list = GetAllyMinionListFromACard(card);
        return list[Random.Range(0, list.Count)].GetComponent<MinionDisplay>().minion;
    }
    Minion GetRandomEnemyMinion(Card card)
    {
        List<GameObject> list = GetEnemyMinionListFromACard(card);
        return list[Random.Range(0, list.Count)].GetComponent<MinionDisplay>().minion;
    }
    List<GameObject> GetAllyMinionListFromACard(Card card)
    {
        if (card is Minion minion)
            return playerMinionList.Contains(minion.GetGameObject()) ? playerMinionList : enemyMinionList;
        return turn == 0 ? playerMinionList : enemyMinionList;
    }
    List<GameObject> GetEnemyMinionListFromACard(Card card)
    {
        if (card is Minion minion)
        {
            return enemyMinionList.Contains(minion.GetGameObject()) ? playerMinionList : enemyMinionList;
        }
        return turn == 1 ? playerMinionList : enemyMinionList;
    }
    //Active Effect Logic
    void CallActiveEffect(bool onStart)
    {
        List<ActiveEffect> expiredEffects = new();
        foreach (ActiveEffect ae in tmpActiveEffectList)
        {
            if (ae.pendingTime > 0)
            {
                if ((onStart && ae.effect.isOnStartOfTurn) || (!onStart && !ae.effect.isOnStartOfTurn))
                {
                    /*Debug.Log("ActiveEffect");*/
                    PlayOnPlayEffect(ae.card);
                    ae.pendingTime -= 1;

                    if (ae.pendingTime <= 0)
                        expiredEffects.Add(ae);
                }
            }
        }
        foreach (var expired in expiredEffects)
            tmpActiveEffectList.Remove(expired);
    }













    ///////////////////////// Helper Function //////////////////////////////
    private bool IsInActiveList(Card card, CardEffect effect)
    {
        return tmpActiveEffectList.Exists(c => c.card == card && c.effect == effect);
    }
}