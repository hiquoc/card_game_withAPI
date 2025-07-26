using System.Collections.Generic;

public class BuffInstance
{
    public BuffEffect effect;
    public int character;
    public List<ITarget> affectedTargets;
    public BuffInstance(BuffEffect effect, int character)
    {
        this.effect = effect;
        affectedTargets = new();
        this.character = character;
    }

}