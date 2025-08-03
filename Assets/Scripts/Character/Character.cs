using System.Collections.Generic;
using UnityEngine;
using Image = UnityEngine.UI.Image;

public class Character : ITarget
{
    public CharacterDisplay display;
    public GameObject gameObject => display.gameObject;
    public string characterName;
    public Image characterImage;
    public int currentHealth, baseHealth, maxHealth;
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
        OnTakeDamage(value);
        /*Debug.Log("show");*/
        if (currentShield > 0)
        {
            int absorbed = Mathf.Min(currentShield, value);
            DecreaseShield(absorbed);
            value -= absorbed;
            if (value <= 0) return;
        }
        currentHealth -= value;
        display.HaveHealthBuff(currentHealth > baseHealth);
        display.UpdateHealth();
        if (currentHealth <= 0)
        {
            Debug.Log(characterName + " Died");
            display.PlayDeathAnimation();
        }

    }

    public int GetAttack()
    {
        return currentAttack;
    }
    public int GetMaxHealth()
    {
        return maxHealth;
    }
    public int GetHealth()
    {
        return currentHealth;
    }
    public void RestoreHealth(int value)
    {
        int healed = Mathf.Min(value, maxHealth - currentHealth);
        currentHealth += healed;

        OnHeal(healed);
        display.UpdateHealth();
    }

    public void IncreaseHealth(int value)
    {
        maxHealth += value;
        RestoreHealth(value);
        display.HaveHealthBuff(currentHealth > baseHealth);
    }
    public void DecreaseHealth(int value)
    {
        maxHealth -= value;
        currentHealth -= value;
        display.HaveHealthBuff(currentHealth > baseHealth);
        display.UpdateHealth();
        OnTakeDamage(value);
        /*Debug.Log("show");*/
        if (currentHealth <= 0)
            Debug.Log(characterName + " Died");
    }
    public void SetCanAttack(bool v)
    {
        canAttack = v;
        if (canAttack)
            display.ShowReadyToAttack();
        else
            display.HideReadyToAttack();
    }
    public bool CanAttack()
    {
        return canAttack && GetAttack() > 0 && !hasAttackedThisTurn;
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
            TakeDamage(target.GetAttack());

            SetCanAttack(false);
        });
    }
    public void IncreaseAttack(int value)
    {
        currentAttack += value; /*Debug.Log(value);*/
        if (!HasAttackedThisTurn())
            SetCanAttack(true);
        display.UpdateAttack();
    }
    public void DecreaseAttack(int value)
    {
        currentAttack = Mathf.Max(0, currentAttack - value);
        /*Debug.Log(currentAttack);*/
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
    public int GetShield()
    {
        return currentShield;
    }
    public GameObject GetGameObject()
    {
        return display.gameObject;
    }
    public Vector3 GetPosition()
    {
        return display.gameObject.transform.position;
    }

    void OnHeal(int value)
    {
        if (value == 0) return;
        display.ShowPopup(value);
    }
    void OnTakeDamage(int value)
    {
        if (value == 0) return;
        display.ShowPopup(-value);
    }
}
