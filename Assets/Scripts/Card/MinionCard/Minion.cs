using UnityEngine;

public class Minion : Card, ITarget
{
    public MinionDisplay display;

    public int currentHealth, maxHealth;
    public int currentAttack;
    bool canAttack = false;
    public GameObject minionPrefab;

    public Minion(int health, int attack, GameObject prefab)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        currentAttack = attack;
        minionPrefab = prefab;
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
    }
    public void TakeDamage(int value)
    {
        currentHealth -= value;
        display.UpdateHealth();
        if (currentHealth <= 0)
        {
            display.Die();
        }
    }
    public void SetCanAttack(bool v)
    {
        canAttack = v;
    }
    public bool CanAttack()
    {
        return canAttack;
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
    }
    public void DecreaseAttack(int value)
    {
        currentAttack -= value;
    }
    public void IncreaseShield(int value)
    {
        return;
    }
    public void DecreaseShield(int value)
    {
        return;
    }
    public GameObject GetGameObject()
    {
        return display.gameObject;
    }
}