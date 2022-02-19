using System;
using UnityEngine;

public class CharacterStats : MonoBehaviour, IDestructible
{
    public Stat maxHealth;
    public Stat currentHealth { get; private set; }

    public Stat Damage;
    public Stat Armor;

    public bool isDead = false;

    public event Action OnHealthValueChanged;

    protected virtual void Start()
    {
        currentHealth = new Stat();
        currentHealth.SetValue(maxHealth.GetValue());
    }

    public void TakeDamage(GameObject attacker, float damage)
    {
        Debug.Log("attacked" + damage + this.gameObject);
        if (damage <= 0f) return;
        currentHealth.SetValue(currentHealth.GetValue() - damage);

        OnHealthValueChanged?.Invoke();
    }

    public Stat GetArmor()
    {
        return Armor;
    }

    public Stat GetDamage()
    {
        return Damage;
    }

    public Stat GetCurrentHealth()
    {
        Debug.Log(currentHealth.GetValue());
        return currentHealth;
    }

    public Stat GetMaxHealth()
    {
        return maxHealth;
    }

    public void OnDestruction(GameObject destroyer)
    {
        isDead = true;
    }
}
