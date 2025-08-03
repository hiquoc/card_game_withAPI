using System.Collections.Generic;

public class SummonEffect : CardEffect
{
    public SummonEffect(int minionId, Target target, string animationId = "")
        : base(minionId, target, animationId)
    {
        this.type = Type.Summon;
    }
}
