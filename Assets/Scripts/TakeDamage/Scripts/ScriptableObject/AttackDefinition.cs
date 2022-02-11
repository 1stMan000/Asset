﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Attack.asset", menuName = "Attack/BaseAttack")]
public class AttackDefinition : ScriptableObject
{
    [Range(1.5f, 10f)]
    public float Cooldown;

    public float minDamage;
    public float maxDamage;
    public float criticalMultipliyer;
    public float criticalChance;
    public float Range;

    public Attack CreateAttack(CharacterStats attacker, CharacterStats defender, bool bashAttack = false)
    {
        float baseDamage = attacker.GetDamage().GetValue();
        if (!bashAttack)
            baseDamage += Random.Range(minDamage, maxDamage);
        else
            baseDamage += Random.Range(minDamage, maxDamage) * 2;
        bool isCritical = Random.value < criticalChance;
        if (isCritical)
            baseDamage *= criticalMultipliyer;

        if (defender != null)
            baseDamage -= defender.GetArmor().GetValue();

        if (baseDamage < 0)
            baseDamage = 0;
        return new Attack((int)baseDamage, isCritical);
    }
}
