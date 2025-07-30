public abstract class CardEffect
{
    public int id;
    public int value;
    public Type type { get; protected set; }
    public Target target;
    public string animationId;
    public CardEffect(int value, Target target, string animationId = "")
    {
        this.value = value;
        this.target = target;
        this.animationId = animationId;
    }
    public enum Type
    {
        Damage,
        Heal,
        Draw,
        Summon,
        Taunt,
        Buff,
        // Silence, Destroy, Transform
    }

    public enum Target
    {
        None,
        Self,
        Enemy,
        CurrentMinion,
        All,
        AllAlly,
        AllEnemy,
        AllMinions,
        AllEnemyMinions,
        AllAllyMinions,
        RandomAllyMinion,
        RandomEnemyMinion,
        /*ChosenMinion,*/
        ChosenTarget,
    }
}
