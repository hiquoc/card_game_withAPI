public class HealEffect : CardEffect
{
    public HealEffect(int value, Target target, string animationId = "")
        : base(value, target, animationId)
    {
        this.type = Type.Heal;
    }
}
