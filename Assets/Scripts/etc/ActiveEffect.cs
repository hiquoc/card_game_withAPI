public class ActiveEffect
{
    public Card card;
    public CardEffect effect;
    public int pendingTime;
    public ITarget target;
    public ActiveEffect(Card card, CardEffect effect, int pendingTime, ITarget target)
    {
        this.card = card;
        this.effect = effect;
        this.pendingTime = pendingTime;
        this.target = target;
    }
}
