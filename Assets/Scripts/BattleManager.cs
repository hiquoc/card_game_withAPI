using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using static UnityEngine.Rendering.DebugUI;
using Random = UnityEngine.Random;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance;
    public bool isWaiting = false;

    public int turn = 0;
    int turnNum = 3;//////////////////////////////////////////////////////////////////////////
    public CharacterDisplay playerDisplay, enemyDisplay;
    public Character player, enemy;
    public Character tmpChar => turn == 0 ? player : enemy;
    Character reversedTmpChar => turn == 0 ? enemy : player;

    public List<GameObject> playerMinionList, enemyMinionList;
    List<GameObject> tmpMinionList => turn == 0 ? playerMinionList : enemyMinionList;
    List<GameObject> reversedTmpMinionList => turn == 1 ? playerMinionList : enemyMinionList;

    List<ActiveEffect> playerActiveEffectList, enemyActiveEffectList;
    List<ActiveEffect> tmpActiveEffectList => turn == 0 ? playerActiveEffectList : enemyActiveEffectList;

    public bool minionDying = false;
    Queue<GameObject> minionDeathQueue = new();
    public HashSet<GameObject> dyingMinions = new();
    public UnityEngine.UI.Button endTurnBtn;


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
        

        rm = ReferenceManager.Instance;
        playerMinionList = new List<GameObject>();
        enemyMinionList = new List<GameObject>();
        playerActiveEffectList = new List<ActiveEffect>();
        enemyActiveEffectList = new List<ActiveEffect>();
        endTurnBtn.onClick.AddListener(() => StartCoroutine(EndPlayerTurn()));
        endTurnBtn.interactable = false;
    }
    private void Start()
    {
        SetupCharacter();
        
        CardManager.Instance.DrawInitHand();
        StartCoroutine(StartPlayerTurnAfterDelay());
    }
    private IEnumerator StartPlayerTurnAfterDelay()
    {
        yield return StartCoroutine(GetComponent<StartAndEndBattle>().PlayStartBattle());
        yield return StartCoroutine(SetupEnemyBuff());
        endTurnBtn.interactable = true;
        /*yield return new WaitForSeconds(2.5f);*/
        yield return StartCoroutine(StartPlayerTurn());
    }
    //Goi api lay du lieu nhan vat
    void SetupCharacter()
    {
        player = playerDisplay.character;
        playerDisplay.SetUp(30, "Mother");

        enemy = enemyDisplay.character;
        enemyDisplay.SetUp(30, "Thug");
    }
    IEnumerator SetupEnemyBuff()
    {
        yield return null;
        /*SpellCard spellCard = new()
        {
            type = Card.CardType.spell,
            mana = 2
        };*/
        /*BuffEffect cardEffect = new(1, CardEffect.Target.Enemy, "attack", BuffEffect.BuffType.Attack);
        spellCard.onPlay.Add(cardEffect);
        ApplyEffect(spellCard, cardEffect, null);
        enemy.SetHasAttackedThisTurn(true);
        while (rm.anim.isPlaying)
        {
            yield return new WaitForSeconds(0.5f);
        }*/

        /*BuffEffect cardEffect = new(1, CardEffect.Target.Enemy, "heal", BuffEffect.BuffType.HealOverTime, int.MaxValue, true);
        spellCard.onPlay.Add(cardEffect);
        enemyActiveEffectList.Add(new(spellCard, cardEffect, cardEffect.duration, enemy));
        while (rm.anim.isPlaying)
        {
            yield return new WaitForSeconds(0.5f);
        }*/

        /*enemy.maxHealth = 40;
        BuffEffect cardEffect = new(1, CardEffect.Target.Enemy, "attack", BuffEffect.BuffType.Attack);
        spellCard.onPlay.Add(cardEffect);
        ApplyEffect(spellCard, cardEffect, null);
        enemy.SetHasAttackedThisTurn(true);
        while (rm.anim.isPlaying)
        {
            yield return new WaitForSeconds(0.2f);
        }

        BuffEffect cardEffect1 = new(1, CardEffect.Target.Enemy, "heal", BuffEffect.BuffType.HealOverTime, int.MaxValue, true);
        spellCard.onPlay.Add(cardEffect1);
        enemyActiveEffectList.Add(new(spellCard, cardEffect, cardEffect.duration, enemy));
        while (rm.anim.isPlaying)
        {
            yield return new WaitForSeconds(0.2f);
        }*/
        
    }

    //Turn logic
    private IEnumerator StartPlayerTurn()
    {
        isWaiting = true;
        yield return StartCoroutine(PlayOnStartOfTurnEffect());
        CallActiveEffect(true);
        isWaiting = false;
        player.currentMana = turnNum <= 10 ? turnNum : 10;
        UpdateMana();
        rm.cm.DrawPlayerCard();
        ResetAttack(true);
        /*if (player.GetAttack() > 0 || playerMinionList.Count > 0)*/
        rm.am.drawable = true;
        CardDrag.canDrag = true;
    }
    IEnumerator EndPlayerTurn()
    {
        /*Debug.Log(isWaiting);*/
        if (isWaiting) yield break;
        endTurnBtn.interactable = false;
        rm.am.drawable = false;
        CardDrag.canDrag = false;
        rm.sm.Play("btnClick");
        yield return StartCoroutine(PlayOnEndOfTurnEffect());
        /*Debug.Log(rm.anim.isPlaying);*/
        yield return new WaitUntil(() => rm.anim.isPlaying == false);// cho animation

        CallActiveEffect(false);
        ResetAttack(false);
        turn = turn == 0 ? 1 : 0;
        StartCoroutine(StartEnemyTurn());
    }

    private IEnumerator StartEnemyTurn()
    {
        while (isWaiting)
        {
            yield return new WaitForSeconds(0.5f);
        }
        yield return StartCoroutine(PlayOnStartOfTurnEffect());
        CallActiveEffect(true);
        enemy.currentMana = turnNum <= 10 ? turnNum : 10;
        UpdateMana();
        rm.cm.DrawEnemyCard();
        ResetAttack(true);
        yield return StartCoroutine(EnemyTurnCoroutine());
    }
    IEnumerator EnemyTurnCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        /*rm.ai.PlayMinionCard();*/
        foreach (GameObject cardObj in rm.ai.FindBestCardCombo(rm.cm.enemyHand, enemy.currentMana))
        {
            rm.ai.PlayCard(cardObj);
            yield return new WaitUntil(() => rm.ai.isDone); //cho chon muc tieu
            /*Debug.Log("isDone");*/
            while (rm.anim.isPlaying)
                yield return new WaitForSeconds(0.2f);// cho animation
        }
        /*yield return new WaitForSeconds(0.5f);*/
        yield return StartCoroutine(rm.ai.DoAttackCoroutine());
        yield return new WaitForSeconds(0.5f);
        /*Debug.Log(rm.ai.isDone);*/
        //Debug.Log(rm.anim.isPlaying);
        /*yield return new WaitForSeconds(1f);*/

        /*Debug.Log(rm.anim.isPlaying);*/
        yield return StartCoroutine(PlayOnEndOfTurnEffect());
        yield return new WaitForSeconds(1f);
        while (isWaiting)
        {
            yield return new WaitForSeconds(0.5f);
        }
        CallActiveEffect(false);
        ResetAttack(false);
        //Start Player Turn
        turn = turn == 0 ? 1 : 0;
        /*Debug.Log("isDone");*/
        turnNum++;
        StartCoroutine(StartPlayerTurn());
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
    void ResetAttack(bool v)
    {
        tmpChar.SetCanAttack(v);// se hien thi mau xanh khi set==true
        if (v)
            tmpChar.SetHasAttackedThisTurn(false);
        if (tmpChar.GetAttack() == 0)
        {
            tmpChar.GetGameObject().GetComponent<SelectableTarget>().DisableHighlight();
        }
        foreach (GameObject minionObj in tmpMinionList)
        {
            MinionDisplay minionDisplay = minionObj.GetComponent<MinionDisplay>();
            minionDisplay.minion.SetCanAttack(v);
        }
    }
    public void ShowReadyToAttack()
    {
        if (tmpChar.CanAttack())
            tmpChar.display.ShowReadyToAttack();
        foreach (GameObject minionObj in tmpMinionList)
        {
            MinionDisplay minionDisplay = minionObj.GetComponent<MinionDisplay>();
            if (minionDisplay.minion.CanAttack())
                minionDisplay.ShowReadyToAttack();
        }
    }

    //Card logic
    public IEnumerator PlayCard(GameObject cardObj)
    {
        if (isWaiting)
        {
            rm.textHelper.ShowText("You can't do that right now!");
            yield break;
        }
        Card card = cardObj.GetComponent<CardDisplay>().card;
        if (card.mana>tmpChar.currentMana) {
            rm.textHelper.ShowText("You don't have enough mana!");
            if(card is MinionCard &&cardObj.TryGetComponent(out CardDrag cd))
            {
                if (cd.minionHolderObj != null)
                {
                    Destroy(cd.minionHolderObj);
                    cd.minionHolderObj = null;
                }                    
                cd.minionHolderRT=null;
            }
            yield break;
        } 
        isWaiting = true;
        if (turn == 0)
            rm.cm.playerHand.Remove(cardObj);
        else
            rm.cm.enemyHand.Remove(cardObj);
        yield return null;
        /*Debug.Log("PlayCard Start");*/
        /*Debug.Log(turn);*/
        
        if (card is SpellCard spellCard)
        {
            Debug.Log("This is a spell with " + spellCard.mana + " mana cost.");
            tmpChar.DecreaseMana(spellCard.mana);
            rm.sm.Play("cardPlay");
            yield return StartCoroutine(cardObj.GetComponent<CardDrag>().MoveSpellCard(turn==1));

            yield return cardObj.transform.DOScale(0f, 0.1f).WaitForCompletion();
        }
        else if (card is MinionCard minionCard)
        {
            Debug.Log("This is a minon card with " + minionCard.currentAttack + " attack");
            tmpChar.DecreaseMana(minionCard.mana);

            yield return StartCoroutine(cardObj.GetComponent<CardDrag>().MoveMinionCard(turn==1));
            rm.sm.Play("cardDrop");
            yield return cardObj.transform.DOScale(0f, 0.2f).WaitForCompletion();
            /*Debug.Log("After move card");*/
            GameObject minion = Instantiate(rm.minionPrefab);
            minion.GetComponent<MinionDisplay>().SetupMinion(minionCard, cardObj);

            RectTransform minionRT = minion.GetComponent<RectTransform>();
            minionRT.SetParent(turn == 0 ? rm.playerMinionPosition : rm.enemyMinionPosition);
            tmpMinionList.Add(minion);
            rm.buffm.OnNewMinion(minionCard, turn);

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
        isWaiting = false;
    }
    //Play effect
    void PlayOnPlayEffect(Card card)
    {
        foreach (CardEffect effect in card.onPlay)
        {
            if (effect.target == CardEffect.Target.ChosenTarget)
            {
                pendingEffect = new PendingEffect { pendingCardEffect = effect, pendingCard = card };
                if (turn == 0)
                    EnterSelectTarget(card, effect);
                else
                    EnemyEnterSelectTarget(card, effect);
            }
            else
            {
                ApplyEffect(card, effect, null);
            }
        }
        if (card.onPlay.Count == 0)
        {
            /* Debug.Log("isDone at count=0");*/
            rm.ai.isDone = true; //Tam thoi de logic cho luot cua ai o day 
        }

    }
    void PlayOnDeathEffect(Card card)
    {
        foreach (CardEffect c in card.onDeath)
        {
            /*Debug.Log(c.type.ToString());*/
            ApplyEffect(card, c, null);
        }
    }
    IEnumerator PlayOnStartOfTurnEffect()
    {
        foreach (GameObject minionObj in tmpMinionList)
        {
            MinionCard minionCard = minionObj.GetComponent<MinionDisplay>().minion;
            foreach (CardEffect c in minionCard.onStartOfTurn)
            {
                ApplyEffect(minionCard, c, null);
                yield return null;
                yield return new WaitUntil(() => !rm.anim.isPlaying);
                yield return new WaitForSeconds(0.5f);
                while (minionDying)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
        }

    }
    IEnumerator PlayOnEndOfTurnEffect()
    {
        List<GameObject> copy = new(tmpMinionList);
        foreach (GameObject minionObj in copy)
        {
            if (minionObj == null) continue;
            if (!tmpMinionList.Contains(minionObj)) continue;
            MinionCard minionCard = minionObj.GetComponent<MinionDisplay>().minion;
            foreach (CardEffect c in minionCard.onEndOfTurn)
            {
                /*Debug.Log("before ani");*/
                while (minionDying)
                    yield return new WaitForSeconds(0.2f);
                if (!tmpMinionList.Contains(minionObj)) break;
                ApplyEffect(minionCard, c, null);
                yield return null;
                yield return new WaitUntil(() => !rm.anim.isPlaying);
                yield return new WaitForSeconds(0.5f);


                /*Debug.Log("after ani");*/
                if (!tmpMinionList.Contains(minionObj)) break;
            }
        }
    }

    public void EqueueMinionDeath(GameObject minionObj)
    {

        if (!dyingMinions.Contains(minionObj))
        {
            Debug.Log(minionObj);
            dyingMinions.Add(minionObj);
            minionObj.GetComponent<MinionDisplay>().minion.isDying = true;
            minionDeathQueue.Enqueue(minionObj);
        }
        if (!minionDying)
        {
            StartCoroutine(ProcessMinionDeathQueue());
        }

    }
    IEnumerator ProcessMinionDeathQueue()
    {
        minionDying = true;
        while (minionDeathQueue.Count > 0)
        {
            GameObject minionObj = minionDeathQueue.Dequeue();
            if (minionObj == null) continue;
            MinionCard minionCard = minionObj.GetComponent<MinionDisplay>().minion;
            bool hasEffect = minionCard.onDeath.Count > 0;
            yield return new WaitUntil(() => !isWaiting);

            if (hasEffect)
            {
                yield return new WaitUntil(() => !rm.anim.isPlaying);
                PlayOnDeathEffect(minionCard);
                yield return null;

                while (rm.anim.isPlaying)
                {
                    yield return new WaitForSeconds(0.2f);
                }
            }
            rm.buffm.RemoveBuff(minionCard);
            if (playerMinionList.Contains(minionObj))
                playerMinionList.Remove(minionObj);
            else
                enemyMinionList.Remove(minionObj);
            minionObj.transform.localScale = Vector3.zero;
            minionObj.transform.SetParent(rm.canvas.transform, true);
            dyingMinions.Remove(minionObj);
            Destroy(minionObj, 5f);
        }
        minionDying = false;
    }
    /////////Player selecting target
    private void EnterSelectTarget(Card card, CardEffect effect)
    {
        /*Debug.Log("Enter");*/
        endTurnBtn.interactable = false;
        isWaiting = true;
        List<GameObject> selectableTarget = new();
        switch (effect)
        {
            case DamageEffect:
                selectableTarget.Add(enemyDisplay.gameObject);
                selectableTarget.AddRange(enemyMinionList);
                break;
            default:
                selectableTarget.Add(playerDisplay.gameObject);
                selectableTarget.AddRange(playerMinionList);
                break;
        }
        
        //Loai bo ban than khoi muc tieu co the chon
        if (card is MinionCard minionCard)
        {
            selectableTarget.Remove(minionCard.GetGameObject());
        }
        else
            selectableTarget.Remove(tmpChar.GetGameObject());

        foreach (var target in selectableTarget)
        {
            /*Debug.Log(target);*/
            if (target.TryGetComponent(out SelectableTarget currentTarget))
            {
                currentTarget.SetTargetSelectable(true);
                currentTarget.EnableHighlight();
            }
        }
        if (pendingEffect.pendingCard is SpellCard)
            rm.am.ArrowForSelectingATarget(tmpChar.gameObject.GetComponent<RectTransform>());
        else
        {
            MinionCard minion = pendingEffect.pendingCard as MinionCard;
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
        ShowReadyToAttack();//hien thi target co the tan cong
        isWaiting = false;
        endTurnBtn.interactable = true;
    }

    /////////Enemy AI selecting target
    private void EnemyEnterSelectTarget(Card card, CardEffect cardEffect)
    {

        List<GameObject> selectableTarget = new();
        if (turn == 0)
            selectableTarget.Add(enemyDisplay.gameObject);
        else
            selectableTarget.Add(playerDisplay.gameObject);
        selectableTarget.AddRange(playerMinionList);
        selectableTarget.AddRange(enemyMinionList);
        //Loai bo ban than khoi muc tieu co the chon
        if (card is MinionCard minionCard)
        {
            selectableTarget.Remove(minionCard.GetGameObject());
            /*Debug.Log(result);*/
        }

        else
            selectableTarget.Remove(tmpChar.GetGameObject());
        foreach (var target in selectableTarget)
        {
            if (target.TryGetComponent(out SelectableTarget currentTarget))
            {
                currentTarget.SetTargetSelectable(true);
                currentTarget.EnableHighlight();
            }
        }
        if (pendingEffect.pendingCard is SpellCard)
            rm.am.EnemyArrowForSelectingATarget(card, tmpChar.gameObject.GetComponent<RectTransform>(), cardEffect);
        else
        {
            MinionCard minion = pendingEffect.pendingCard as MinionCard;
            rm.am.EnemyArrowForSelectingATarget(card, minion.display.gameObject.GetComponent<RectTransform>(), cardEffect);
        }
        CardDrag.canDrag = false;
        CardHover.canHover = false;
    }
    //Apply Effect Logic
    void ApplyEffect(Card card, CardEffect effect, ITarget target)
    {
        /*Debug.Log(effect.type);*/
        int value = effect.value;
        switch (effect.target)
        {
            case CardEffect.Target.Self:
                ApplyEffectToTarget(card, effect, GetCharacterFromACard(card), value);
                break;
            case CardEffect.Target.Enemy:
                ApplyEffectToTarget(card, effect, GetCharacterFromACard(card) == player ? enemy : player, value);
                break;
            case CardEffect.Target.CurrentMinion:
                ApplyEffectToTarget(card, effect, GetMinionFromACard(card), value);
                break;
            case CardEffect.Target.ChosenTarget:
                ApplyEffectToTarget(card, effect, GetSource(card), value, false, target);
                break;
            /*case CardEffect.Target.ChosenMinion:
                ApplyEffectToTarget(card, effect, GetSource(card), value, false, target);
                break;*/
            case CardEffect.Target.All:
                ApplyEffectToAll(card, effect, value);
                break;
            case CardEffect.Target.AllAlly:
                ApplyEffectToAllAlly(card, effect, value);
                break;
            case CardEffect.Target.AllEnemy:
                ApplyEffectToAllEnemy(card, effect, value);
                break;
            case CardEffect.Target.AllMinions:
                ApplyEffectToAllMinions(card, effect, value);
                break;
            case CardEffect.Target.AllEnemyMinions:
                ApplyEffectToAllEnemyMinions(card, effect, value);
                break;
            case CardEffect.Target.AllAllyMinions:
                ApplyEffectToAllAllyMinions(card, effect, value);
                break;
            case CardEffect.Target.RandomAllyMinion:
                ApplyEffectToTarget(card, effect, GetSource(card), value, false, GetRandomAllyMinion(card), true);
                break;
            case CardEffect.Target.RandomEnemyMinion:
                ApplyEffectToTarget(card, effect, GetSource(card), value, false, GetRandomEnemyMinion(card), true);
                break;
        }
    }

    public void ApplyEffectToTarget(Card card, CardEffect effect, ITarget source, int value, bool hasPlayedAni = false, ITarget target = null, bool triedToGetTarget = false)
    {
        switch (effect.type)
        {
            case CardEffect.Type.Damage:
                if (triedToGetTarget && target == null)
                {
                    /*Debug.Log("Apply2");*/
                    break;
                }
                if (target != null && !hasPlayedAni)
                {
                    if (target is MinionCard minion && !minion.isDying || target is Character)
                        rm.anim.EnqueueAnimation(new(effect.animationId, source.GetPosition(), target.GetPosition(), () =>
                             target.TakeDamage(value)
                        ));
                    /* Debug.Log("Apply3");*/
                }
                else
                {
                    if (source is MinionCard minion && !minion.isDying || source is Character)
                    {
                        /*Debug.Log(source is MinionCard minion1 && !minion1.isDying);*/
                        source.TakeDamage(value);
                    }

                }
                /*Debug.Log("Apply4");*/
                break;
            case CardEffect.Type.Heal:
                /*Debug.Log("Heal");*/
                if (triedToGetTarget && target == null)
                {
                    break;
                }
                if( (target != null && !hasPlayedAni && triedToGetTarget)|| target != null)
                {
                    if (target is MinionCard minion && !minion.isDying || target is Character)
                        rm.anim.EnqueueAnimation(new(effect.animationId, target.GetPosition(), Vector3.zero,
                        () => { target.RestoreHealth(value); }));
                }
                else
                {
                    if (source is MinionCard minion && !minion.isDying || source is Character)
                        rm.anim.PlayInstanceAnimation(new(effect.animationId, source.GetPosition(), Vector3.zero,
                       () => { source.RestoreHealth(value); }));
                }
                break;

            case CardEffect.Type.Draw:
                if (turn == 0)
                    rm.cm.DrawPlayerCard();
                else
                    rm.cm.DrawEnemyCard();
                break;
            case CardEffect.Type.Buff:
                if (effect is not BuffEffect buffEffect) break;
                switch (buffEffect.buffType)
                {
                    case BuffEffect.BuffType.None:
                        break;
                    case BuffEffect.BuffType.Attack:
                        if (triedToGetTarget && target == null)
                        {
                            break;
                        }
                        if ((target != null && !hasPlayedAni && triedToGetTarget) || target != null)
                        {
                            if (target is MinionCard minion1 && !minion1.isDying || target is Character)
                                rm.anim.PlayInstanceAnimation(new(effect.animationId, target.GetPosition(), Vector3.zero,
                                () => { target.IncreaseAttack(value); }));
                        }
                        else
                        {
                            if (source is MinionCard minion1 && !minion1.isDying || source is Character)
                                rm.anim.PlayInstanceAnimation(new(effect.animationId, source.GetPosition(), Vector3.zero,
                               () => { source.IncreaseAttack(value); }));
                        }
                        break;
                    case BuffEffect.BuffType.IncreaseMaxHealth:
                        if (triedToGetTarget && target == null)
                        {
                            break;
                        }
                        if ((target != null && !hasPlayedAni && triedToGetTarget) || target != null)
                        {
                            if (target is MinionCard minion1 && !minion1.isDying || target is Character)
                                rm.anim.PlayInstanceAnimation(new(effect.animationId, target.GetPosition(), Vector3.zero,
                                () => { target.IncreaseHealth(value); }));
                        }
                        else
                        {
                            if (source is MinionCard minion1 && !minion1.isDying || source is Character)
                                rm.anim.PlayInstanceAnimation(new(effect.animationId, source.GetPosition(), Vector3.zero,
                               () => { source.IncreaseHealth(value); }));
                        }
                        break;
                    /*rm.anim.PlayInstanceAnimation(new(effect.animationId, source.GetPosition(), Vector3.zero, () => source.IncreaseAttack(value)));
                    break;*/
                    case BuffEffect.BuffType.Shield:
                        rm.anim.PlayInstanceAnimation(new(effect.animationId, source.GetPosition(), Vector3.zero, () => source.IncreaseShield(value)));
                        break;
                    case BuffEffect.BuffType.Taunt:
                        MinionCard minion = source as MinionCard;
                        minion.AddTaunt();
                        break;
                    case BuffEffect.BuffType.HealOverTime:
                        /*target.IncreaseAttack(value); */
                        break;
                    case BuffEffect.BuffType.DealDamageOverTime:
                        /*target.TakeDamage(value);*/
                        break;
                    case BuffEffect.BuffType.ActiveAttackBuff:
                    case BuffEffect.BuffType.ActiveHealthBuff:
                    case BuffEffect.BuffType.ActiveAttackDebuff:
                    case BuffEffect.BuffType.ActiveHealthDebuff:
                        /*Debug.Log(buffEffect.buffType);*/
                        BuffManager.Instance.ApplyBuff(card, effect as BuffEffect, source, turn);
                        break;

                }
                /*Debug.Log(buffEffect.duration);
                Debug.Log(!IsInActiveList(card, effect));*/
                if (buffEffect.duration > 0 && !IsInActiveList(card, effect))
                {
                    tmpActiveEffectList.Add(new(card, effect, buffEffect.duration, target ?? source));
                    Debug.Log("Added new active effect");
                }
                break;
            default:
                Debug.Log("Unchecked effect type: " + effect.type);
                break;
        }
        /*Debug.Log("is done at applyeffecttotarget");*/
        rm.ai.isDone = true; //Tam thoi de logic cho luot cua ai o day 
    }

    private void ApplyEffectToAll(Card card, CardEffect effect, int value)
    {
        bool willPlayOne = isOneTimeAnimation(effect.animationId);
        if (willPlayOne)
        {
            rm.anim.EnqueueAnimation(new(effect.animationId, /*GetSource(card).GetPosition()*/ Vector3.zero, Vector3.zero, () =>
            {
                foreach (GameObject obj in playerMinionList)
                {
                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, willPlayOne);
                }
                foreach (GameObject obj in enemyMinionList)
                {
                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, willPlayOne);
                }
                ApplyEffectToTarget(card, effect, player, value, willPlayOne);
                ApplyEffectToTarget(card, effect, enemy, value, willPlayOne);
            }, true));
        }
        else
        {
            foreach (GameObject obj in playerMinionList)
            {
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
            foreach (GameObject obj in enemyMinionList)
            {
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
            ApplyEffectToTarget(card, effect, player, value);
            ApplyEffectToTarget(card, effect, enemy, value);
        }
    }
    private void ApplyEffectToAllAlly(Card card, CardEffect effect, int value)
    {
        bool willPlayOne = isOneTimeAnimation(effect.animationId);
        if (willPlayOne)
        {
            rm.anim.EnqueueAnimation(new(effect.animationId, /*GetSource(card).GetPosition()*/GetCharacterFromACard(card)==player?new Vector3(0,-200,0):new Vector3(0,200,0), Vector3.zero, () =>
            {
                foreach (GameObject obj in GetAllyMinionListFromACard(card))
                {
                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, true);
                }
                ApplyEffectToTarget(card, effect, GetCharacterFromACard(card), value, true);
            }, true));
        }
        else
        {
            foreach (GameObject obj in GetAllyMinionListFromACard(card))
            {
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
            ApplyEffectToTarget(card, effect, GetCharacterFromACard(card), value);
        }
    }

    private void ApplyEffectToAllEnemy(Card card, CardEffect effect, int value)
    {
        bool willPlayOne = isOneTimeAnimation(effect.animationId);
        if (willPlayOne)
        {
            rm.anim.EnqueueAnimation(new(effect.animationId, /*GetSource(card).GetPosition()*/GetCharacterFromACard(card) == player ? new Vector3(0, 200, 0) : new Vector3(0, -200, 0), Vector3.zero, () =>
            {
                foreach (GameObject obj in GetEnemyMinionListFromACard(card))
                {
                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, true);
                }
                ITarget enemyChar = GetCharacterFromACard(card) == player ? enemy : player;

                /*Debug.Log(enemyChar.GetGameObject().name);*/


                ApplyEffectToTarget(card, effect, enemyChar, value, true);
            }, true));
        }
        else
        {
            foreach (GameObject obj in GetEnemyMinionListFromACard(card))
            {
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
            ITarget enemyChar = GetCharacterFromACard(card) == player ? enemy : player;
            ApplyEffectToTarget(card, effect, enemyChar, value);
        }
    }

    private void ApplyEffectToAllMinions(Card card, CardEffect effect, int value)
    {
        bool willPlayOne = isOneTimeAnimation(effect.animationId);
        if (willPlayOne)
        {
            rm.anim.EnqueueAnimation(new(effect.animationId,/* GetSource(card).GetPosition()*/Vector3.zero, Vector3.zero, () =>
            {
                foreach (GameObject obj in playerMinionList)
                {
                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, true);
                }
                foreach (GameObject obj in enemyMinionList)
                {
                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, true);
                }
            }, true));
        }
        else
        {
            foreach (GameObject obj in playerMinionList)
            {
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
            foreach (GameObject obj in enemyMinionList)
            {
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
        }
    }

    private void ApplyEffectToAllEnemyMinions(Card card, CardEffect effect, int value)
    {

        bool willPlayOne = isOneTimeAnimation(effect.animationId);
        if (willPlayOne)
        {
            rm.anim.EnqueueAnimation(new(effect.animationId, /*GetSource(card).GetPosition()*/GetCharacterFromACard(card) == player ? new Vector3(0, 200, 0) : new Vector3(0, -200, 0), Vector3.zero, () =>
            {
                /*Debug.Log(GetEnemyMinionListFromACard(card).Count);*/
                foreach (GameObject obj in GetEnemyMinionListFromACard(card))
                {

                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, true);
                }
            }, true));
        }
        else
        {
            foreach (GameObject obj in GetEnemyMinionListFromACard(card))
            {
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
        }
    }

    private void ApplyEffectToAllAllyMinions(Card card, CardEffect effect, int value)
    {
        bool willPlayOne = isOneTimeAnimation(effect.animationId);
        if (willPlayOne)
        {
            rm.anim.EnqueueAnimation(new(effect.animationId, /*GetSource(card).GetPosition()*/GetCharacterFromACard(card) == player ? new Vector3(0, -200, 0) : new Vector3(0, 200, 0), Vector3.zero, () =>
            {
                foreach (GameObject obj in GetAllyMinionListFromACard(card))
                {

                    ITarget source = obj.GetComponent<MinionDisplay>().minion;
                    ApplyEffectToTarget(card, effect, source, value, true);
                }
            },true));
        }
        else
        {
            foreach (GameObject obj in GetAllyMinionListFromACard(card))
            {
                /*Debug.Log(obj.name);*/
                ITarget source = obj.GetComponent<MinionDisplay>().minion;
                ApplyEffectToTarget(card, effect, source, value);
            }
        }
    }


    MinionCard GetRandomAllyMinion(Card card)
    {
        List<GameObject> list = GetAllyMinionListFromACard(card);
        if (list.Count == 0) return null;
        return list[Random.Range(0, list.Count)].GetComponent<MinionDisplay>().minion;
    }
    MinionCard GetRandomEnemyMinion(Card card)
    {
        List<GameObject> list = GetEnemyMinionListFromACard(card);
        if (list.Count == 0) return null;
        return list[Random.Range(0, list.Count)].GetComponent<MinionDisplay>().minion;
    }

    //Active Effect Logic
    void CallActiveEffect(bool onStart)
    {
        List<ActiveEffect> expiredEffects = new();
        foreach (ActiveEffect ae in tmpActiveEffectList)
        {
            if (ae.pendingTime > 0 && ae.effect is BuffEffect buffEffect)
            {
                if ((onStart && buffEffect.isStartOfTurn) || (!onStart && !buffEffect.isStartOfTurn))
                {
                    /*Debug.Log(buffEffect.buffType);*/
                    switch (buffEffect.buffType)
                    {
                        case BuffEffect.BuffType.Attack:
                            if (ae.pendingTime == 1)
                            {
                                ae.target.DecreaseAttack(ae.effect.value);
                                /*Debug.Log(ae.pendingTime);*/
                            }
                                
                            break;
                        case BuffEffect.BuffType.DealDamageOverTime:
                            ae.target.TakeDamage(ae.effect.value);
                            break;
                        case BuffEffect.BuffType.HealOverTime:
                            rm.anim.EnqueueAnimation(new(ae.effect.animationId, ae.target.GetPosition(), Vector3.zero,
                        () => { ae.target.RestoreHealth(ae.effect.value); }));
                            
                            break;
                        default:
                            Debug.Log("Unchecked BuffType");
                            break;
                    }

                    ae.pendingTime -= 1;
                    /*Debug.Log(ae.pendingTime);*/
                    if (ae.pendingTime <= 0)
                        expiredEffects.Add(ae);
                }
            }
        }
        foreach (var expired in expiredEffects)
            tmpActiveEffectList.Remove(expired);
    }
    /* void ActiveBuffTarget(Card card, BuffEffect buffEffect, bool isIncreasing)
     {
         List<ITarget> targets = new();
         switch (buffEffect.target)
         {
             case BuffEffect.Target.AllAllyMinions:
                 foreach (GameObject minionObj in GetAllyMinionListFromACard(card))
                 {
                     targets.Add(minionObj.GetComponent<MinionDisplay>().minion);
                 }

                 break;
             case BuffEffect.Target.AllEnemyMinions:
                 foreach (GameObject minionObj in GetEnemyMinionListFromACard(card))
                 {
                     targets.Add(minionObj.GetComponent<MinionDisplay>().minion);
                 }
                 break;
         }
         switch (buffEffect.buffType)
         {
             case BuffEffect.BuffType.ActiveAttackBuff:
                 foreach (var target in targets)
                 {
                     if (target is MinionCard minionCard)
                     {
                         minionCard.IncreaseAttack(buffEffect.value);
                     }
                     *//*else if (target is Character character)
                     {
                         // Apply buff to player or enemy
                     }*//*
                 }
                 break;
             case BuffEffect.BuffType.ActiveAttackDebuff:
                 foreach (var target in targets)
                 {
                     if (target is MinionCard minionCard)
                     {
                         minionCard.DecreaseAttack(buffEffect.value);
                     }
                     *//*else if (target is Character character)
                     {
                         // Apply buff to player or enemy
                     }*//*
                 }
                 break;
             case BuffEffect.BuffType.ActiveHealthBuff:
                 foreach (var target in targets)
                 {
                     if (target is MinionCard minionCard)
                     {
                         minionCard.IncreaseHealth(buffEffect.value);
                     }
                     *//*else if (target is Character character)
                     {
                         // Apply buff to player or enemy
                     }*//*
                 }
                 break;
             case BuffEffect.BuffType.ActiveHealthDebuff:
                 foreach (var target in targets)
                 {
                     if (target is MinionCard minionCard)
                     {
                         minionCard.DecreaseHealth(buffEffect.value);
                     }
                     *//*else if (target is Character character)
                     {
                         // Apply buff to player or enemy
                     }*//*
                 }
                 break;
         }

     }
 */

    public void PlayEndAnimation()
    {
        isWaiting = true;
        bool isWin = player.GetHealth() > 0;
        StartCoroutine(PlayEndAnimationCoroutine(isWin));
    }
    IEnumerator PlayEndAnimationCoroutine(bool isWin)
    {
        yield return new WaitForSeconds(2f);
        GetComponent<StartAndEndBattle>().PlayEndBattle(isWin);
    }




    ///////////////////////// Helper Function //////////////////////////////
    private bool IsInActiveList(Card card, CardEffect effect)
    {
        return tmpActiveEffectList.Exists(c => c.card == card && c.effect == effect);
    }
    ITarget GetSource(Card card)
    {
        return card switch
        {
            SpellCard => tmpChar,
            MinionCard m => m,
            _ => null
        };
    }
    List<GameObject> GetAllyMinionListFromACard(Card card)
    {
        if (card is MinionCard minion)
            return playerMinionList.Contains(minion.GetGameObject()) ? playerMinionList : enemyMinionList;
        return turn == 0 ? playerMinionList : enemyMinionList;
    }
    List<GameObject> GetEnemyMinionListFromACard(Card card)
    {
        if (card is MinionCard minion)
        {
            return enemyMinionList.Contains(minion.GetGameObject()) ? playerMinionList : enemyMinionList;
        }
        return turn == 1 ? playerMinionList : enemyMinionList;
    }
    Character GetCharacterFromACard(Card card)
    {
        if (card is MinionCard minion)
        {
            return playerMinionList.Contains(minion.GetGameObject()) ? player : enemy;
        }
        else
        {
            return tmpChar;
        }
    }
    MinionCard GetMinionFromACard(Card card)
    {
        return card as MinionCard;
    }
    public bool CanAttackTarget(ITarget attacker, ITarget target)
    {
        List<GameObject> minionList;
        if (attacker is MinionCard minionCard)
            minionList = GetEnemyMinionListFromACard(minionCard);
        else if (attacker is Character character)
            minionList = character == player ? enemyMinionList : playerMinionList;
        else
            return false;
        bool hasTaunt = minionList.Any(minionObj => minionObj.GetComponent<MinionDisplay>().minion.hasTaunt);
        if (hasTaunt)
            return target is MinionCard minion && minion.hasTaunt;
        return true;
    }
    public void ShowAttackableTarget()
    {
        List<GameObject> selectableTarget = new();

        foreach (GameObject minionObj in enemyMinionList)
        {
            MinionCard mc = minionObj.GetComponent<MinionDisplay>().minion;
            if (mc.hasTaunt)
                selectableTarget.Add(minionObj);
        }
        if (selectableTarget.Count == 0)
        {
            selectableTarget.AddRange(enemyMinionList);
            selectableTarget.Add(enemyDisplay.gameObject);
        }

        foreach (GameObject target in selectableTarget)
        {
            if (target.TryGetComponent(out SelectableTarget currentTarget))
            {
                currentTarget.SetTargetSelectable(true);
                currentTarget.EnableHighlight();
            }
        }
    }
    public void HideAttackableTarget()
    {
        foreach (GameObject target in enemyMinionList)
        {
            if (target.TryGetComponent(out SelectableTarget currentTarget))
            {
                currentTarget.DisableHighlight();
            }
        }
        if (enemyDisplay.TryGetComponent(out SelectableTarget enemyTarget))
        {
            enemyTarget.DisableHighlight();
        }
    }
    bool isOneTimeAnimation(string name)
    {
        return name switch
        {
            "explosion" => true,
            _ => false
        };
    }
}