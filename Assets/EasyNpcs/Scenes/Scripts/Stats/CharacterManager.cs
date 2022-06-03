using System;
using UnityEngine;

namespace Npc_Manager
{
    public class CharacterManager : MonoBehaviour, IDestructible
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

        public void OnAttack(GameObject attacker, Attack attack)
        {
            TakeDamage(attacker, attack.Damage);
            OnDeath(attacker);
        }

        void TakeDamage(GameObject attacker, float damage)
        {
            if (damage <= 0f) return;
            currentHealth.SetValue(currentHealth.GetValue() - damage);

            OnHealthValueChanged?.Invoke();
        }

        void OnDeath(GameObject attacker)
        {
            if (GetCurrentHealth().GetValue() <= 0)
            {
                if (gameObject.layer == 8)
                {
                    IDestructible[] destructibles = GetComponents<IDestructible>();
                    foreach (IDestructible destructible in destructibles)
                    {
                        destructible.OnDestruction(attacker);
                    }
                }
            }
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
}

