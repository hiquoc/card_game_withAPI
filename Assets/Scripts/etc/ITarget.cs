using UnityEngine;

public interface ITarget
{
    public bool CanAttack();
    public void SetCanAttack(bool v);
    public GameObject GetGameObject();
    public Vector3 GetPosition();
    public void TakeDamage(int value);
    public int GetMaxHealth();
    public int GetHealth();
    public void RestoreHealth(int value);
    public void IncreaseHealth(int value);
    public void DecreaseHealth(int value);
    public int GetAttack();
    public void IncreaseAttack(int value);
    public void DecreaseAttack(int value);
    public void IncreaseShield(int value);
    public void DecreaseShield(int value);
    public void AttackTarget(ITarget target);
}