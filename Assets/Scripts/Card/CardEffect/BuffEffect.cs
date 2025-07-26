public class BuffEffect : CardEffect
{
    public BuffType buffType;
    public int duration = 0;
    public bool isStartOfTurn = false;

    public enum BuffType
    {
        None,
        Attack,
        Shield,
        Taunt,
        IncreaseMaxHealth,
        HealOverTime,
        DealDamageOverTime,
        ActiveAttackBuff,
        ActiveHealthBuff,
        ActiveAttackDebuff,
        ActiveHealthDebuff,
    }

    public BuffEffect(int value, Target target, string animationId, BuffType buffType, int duration = 0, bool isStartOfTurn = false)
        : base(value, target, animationId)
    {
        this.buffType = buffType;
        this.duration = duration;
        this.isStartOfTurn = isStartOfTurn;
        this.type = Type.Buff;
    }
}
