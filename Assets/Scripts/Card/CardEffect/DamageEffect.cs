public class DamageEffect : CardEffect
{
    public DamageEffect(int value, Target target, string animationId = "")
        : base(value, target, animationId)
    {
        this.type = Type.Damage;
    }
}
