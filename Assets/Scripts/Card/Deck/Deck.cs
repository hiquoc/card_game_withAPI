using System.Collections.Generic;
using UnityEngine;

public class Deck
{
    public List<Card> list = new();

    public void Setup()
    {
        for (int i = list.Count - 1; i >= 0; i--)
        {
            int rand = Random.Range(0, i + 1);
            (list[rand], list[i]) = (list[i], list[rand]);
        }
    }
}
