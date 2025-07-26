public class TauntEffect : CardEffect
{
    public TauntEffect(int value = 0, Target target = Target.CurrentMinion, string animationId = "") : base(value, target, animationId)
    {
        this.type = Type.Taunt;
    }
}