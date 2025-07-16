using System.Collections.Generic;
using UnityEngine.UI;

public abstract class Card
{
    public Image image;
    public int id;
    public CardType type;//0:spell,1:minion
    public string name;
    public int mana;
    public string description;

    public List<CardEffect> onPlay = new();
    public List<CardEffect> onDeath = new();

    public enum CardType
    {
        spell = 0,
        minion = 1,
    }
}
