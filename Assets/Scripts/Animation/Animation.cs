public class Animation
{
    public string animationId;
    public ITarget source;
    public ITarget target;
    public float duration;
    public Animation(string animationId, ITarget source, ITarget target, float duration)
    {
        this.animationId = animationId;
        this.source = source;
        this.target = target;
        this.duration = duration;
    }
}
