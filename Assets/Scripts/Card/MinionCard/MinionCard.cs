using UnityEngine;

public class MinionCard : Card, ITarget
{
    public MinionDisplay display;

    public int currentHealth, baseHealth, maxHealth;
    public int currentAttack, baseAttack;

    public bool hasTaunt = false;
    bool canAttack = false;
    public GameObject minionPrefab;
    public bool isDying = false;
    public MinionCard(int id, int health, int attack, GameObject prefab)
    {
        this.id = id;
        maxHealth = health;
        currentHealth = maxHealth;
        baseHealth = currentHealth;
        currentAttack = attack;
        baseAttack = attack;
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
        OnHeal(value);
        display.UpdateHealth();
    }
    public void IncreaseHealth(int value)
    {
        maxHealth += value;
        RestoreHealth(value);
        OnHeal(value);
        display.HaveHealthBuff(currentHealth > baseHealth);
    }
    public void DecreaseHealth(int value)
    {
        maxHealth -= value;
        currentHealth -= value;
        display.HaveHealthBuff(currentHealth > baseHealth);
        OnTakeDamage(value);
        display.UpdateHealth();
        if (currentHealth <= 0)
        {
            display.Die();
        }
    }
    public void TakeDamage(int value)
    {
        currentHealth -= value;
        display.HaveHealthBuff(currentHealth > baseHealth);
        OnTakeDamage(value);
        display.UpdateHealth();
        if (currentHealth <= 0)
        {
            display.Die();
        }
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
        return canAttack;
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
        currentAttack += value;
        display.UpdateAttack();
        display.HaveAttackBuff(currentAttack > baseAttack);
    }
    public void DecreaseAttack(int value)
    {
        currentAttack -= value;
        display.UpdateAttack();
        display.HaveAttackBuff(currentAttack > baseAttack);
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
    public Vector3 GetPosition()
    {
        return display.gameObject.transform.position;
    }
    void OnHeal(int value)
    {
        PoolManager pp = PoolManager.Instance;
        GameObject popup2 = pp.GetPopup();
        pp.Show(popup2, value, GetGameObject().transform, true);
    }
    void OnTakeDamage(int value)
    {
        PoolManager pp = PoolManager.Instance;
        GameObject popup2 = pp.GetPopup();
        pp.Show(popup2, value, GetGameObject().transform, false);
    }
    public void AddTaunt()
    {
        hasTaunt = true;
        display.AddTaunt();
    }
}