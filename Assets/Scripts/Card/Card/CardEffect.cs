public class CardEffect
{
    public int value;
    public Type type;
    public Target target;
    public int duration;
    public BuffType buffType;
    public bool isOnStartOfTurn;
    public string animationId;
    public enum Type
    {
        Damage,
        Heal,
        Draw,
        Summon,
        Taunt,
        Buff,
        /*Silence,
        Destroy,
        Transform,*/
    }
    public enum Target
    {
        None,
        Self,
        Enemy,
        All,
        AllMinions,
        AllEnemyMinions,
        AllAllyMinions,
        RandomAllyMinion,
        RandomEnemyMinion,
        ChosenMinion,
        ChosenTarget,
    }
    public enum BuffType
    {
        None,
        Attack,
        Shield,
        IncreaseMaxHealth,
        HealOverTime,
        DealDamageOverTime,
    }
    public CardEffect(int value, Type type, Target target, BuffType buffType = BuffType.None, int duration = 0, bool isOnStartOfTurn = false, string animationId = "")
    {
        this.value = value;
        this.type = type;
        this.target = target;
        this.buffType = buffType;
        this.duration = duration;
        this.isOnStartOfTurn = isOnStartOfTurn;
        this.animationId = animationId;
    }

}