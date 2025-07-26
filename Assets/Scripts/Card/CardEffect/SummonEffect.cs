using System.Collections.Generic;

public class SummonEffect : CardEffect
{
    public List<int> minionIdList;

    public SummonEffect(List<int> minionIdList, Target target, string animationId = "")
        : base(minionIdList.Count, target, animationId)
    {
        this.minionIdList = minionIdList;
        this.type = Type.Summon;
    }
}
