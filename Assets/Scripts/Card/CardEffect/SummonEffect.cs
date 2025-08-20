public class SummonEffect : CardEffect
{
    public int minionId;
    public SummonEffect(int duplicate, Target target, string animationId, int minionId)
        : base(duplicate, target, animationId)
    {
        this.type = Type.Summon;
        this.minionId = minionId;
    }
}
