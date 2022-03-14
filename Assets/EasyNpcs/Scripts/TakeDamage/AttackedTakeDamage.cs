﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterStats))]
public class AttackedTakeDamage : MonoBehaviour, IAttackable
{
    private CharacterStats stats;
    public bool RagdollOnDeath = true;

    void Awake()
    {
        stats = GetComponent<CharacterStats>();
    }

    public void OnAttack(GameObject attacker, Attack attack, bool bashAttack = false)
    {
        stats.TakeDamage(attacker, attack.Damage);

        if (stats.GetCurrentHealth().GetValue() <= 0)
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
}