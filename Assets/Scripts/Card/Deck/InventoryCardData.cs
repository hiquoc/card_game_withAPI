using NUnit.Framework;
using System.Collections.Generic;

public class InventoryCardData
{
    public List<int> inventoryId=new();
    public int cardId;
    public string rarity;
    public string cardName;
    public string mainImg;
    public int mana;
    public int attack;
    public int health;
    public int quantity;
    public int onSale;

    public InventoryCardData(int? inventoryId, int cardId, string rarity, string cardName, string mainImg, int mana, int attack, int health, int quantity, int onSale)
    {
        if(inventoryId.HasValue)
            this.inventoryId.Add(inventoryId.Value);
        this.cardId = cardId;
        this.rarity = rarity;
        this.cardName = cardName;
        this.mainImg = mainImg;
        this.mana = mana;
        this.attack = attack;
        this.health = health;
        this.quantity = quantity;
        this.onSale = onSale;
    }
}