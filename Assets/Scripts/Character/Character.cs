using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class Character : ITarget
{
    public CharacterDisplay display;
    public GameObject gameObject => display.gameObject;
    public string characterName;
    public Image characterImage;
    public int currentHealth, maxHealth;
    public int currentShield = 0;
    public int currentMana = 0;
    public int maxMana = 10;
    public int currentAttack = 0;
    private bool canAttack = false;
    private bool hasAttackedThisTurn = false;

    public List<CardEffect> skill = new();
    public string skillName;
    public string skillDes;



    public void SetMana(int value)
    {
        currentMana = Mathf.Min(value, maxMana);
        display.UpdateMana();
    }
    public void IncreaseMana(int value)
    {
        currentMana = Mathf.Min(currentMana + value, maxMana);
        display.UpdateMana();
    }
    public void DecreaseMana(int value)
    {
        currentMana = Mathf.Max(0, currentMana - value);
        display.UpdateMana();
    }
    public void TakeDamage(int value)
    {
        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, value);
            DecreaseShield(absorbed);
            value -= absorbed;
            if (value <= 0) return;
        }
        currentHealth -= value;
        display.UpdateHealth();
        if (currentHealth <= 0)
            Debug.Log(characterName + " Died");
    }

    public int GetAttack()
    {
        return currentAttack;
    }
    public int GetHealth()
    {
        return currentHealth;
    }
    public void RestoreHealth(int value)
    {
        currentHealth = Mathf.Min(currentHealth + value, maxHealth);
        display.UpdateHealth();
    }

    public void SetCanAttack(bool v)
    {
        canAttack = v;
    }
    public bool CanAttack()
    {
        return canAttack && GetAttack() > 0;
    }
    public void SetHasAttackedThisTurn(bool v)
    {
        hasAttackedThisTurn = v;
    }
    public bool HasAttackedThisTurn()
    {
        return hasAttackedThisTurn;
    }
    public void AttackTarget(ITarget target)
    {
        display.PlayAttackAnimation(target, () =>
        {
            target.TakeDamage(currentAttack);
            SetCanAttack(false);
        });
    }
    public void IncreaseAttack(int value)
    {
        currentAttack += value;
        if (!HasAttackedThisTurn())
            SetCanAttack(true);
        display.UpdateAttack();
    }
    public void DecreaseAttack(int value)
    {
        currentAttack = Mathf.Max(0, currentAttack - value);
        display.UpdateAttack();
    }
    public void IncreaseShield(int value)
    {
        currentShield += value;
        display.UpdateShield();
    }
    public void DecreaseShield(int value)
    {
        currentShield = Mathf.Max(0, currentShield - value);
        display.UpdateShield();
    }
    public GameObject GetGameObject()
    {
        return display.gameObject;
    }
}
