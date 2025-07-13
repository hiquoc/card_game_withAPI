using UnityEngine;

public interface ITarget
{
    public bool CanAttack();
    public void SetCanAttack(bool v);
    public GameObject GetGameObject();
    public void TakeDamage(int value);
    public int GetHealth();
    public void RestoreHealth(int value);
    public int GetAttack();
    public void AttackTarget(ITarget target);
}