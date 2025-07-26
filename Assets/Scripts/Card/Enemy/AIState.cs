using System.Collections.Generic;

public class AIState
{
    public List<CardAction> playedCards = new();
    public int remainingMana;
    public int score;
    public List<CardAction> remainingHand = new();

    public AIState Clone()
    {
        return new AIState
        {
            playedCards = new List<CardAction>(playedCards),
            remainingMana = remainingMana,
            score = score,
            remainingHand = new List<CardAction>(remainingHand)
        };
    }

}