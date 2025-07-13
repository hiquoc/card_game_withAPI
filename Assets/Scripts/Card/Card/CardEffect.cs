public class CardEffect
{
    public int value;
    public Type type;
    public Target target;

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
    public CardEffect(int value, Type type, Target target)
    {
        this.value = value;
        this.type = type;
        this.target = target;
    }
}