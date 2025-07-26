public class DrawEffect : CardEffect
{
    public DrawEffect(int value = 0, Target target = Target.Self, string animationId = "")
        : base(value, target, animationId)
    {
        this.type = Type.Draw;
    }
}
