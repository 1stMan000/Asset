using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npc_Manager;

public class WhenAttacking : MonoBehaviour
{
    public virtual void AttackTarget(GameObject target) //decides and creates attack on target
    {
        Attack attack = new Attack(10);

        var attackables = target.GetComponentsInChildren(typeof(IDestructible)); //IAttackable has OnAttack() when executed player's attack
        foreach (IDestructible attackable in attackables)
        {
            attackable.OnAttack(gameObject, attack);
        }
    }

    protected virtual Attack CreateAttack(CharacterManager attacker, CharacterManager defender)
    {
        float baseDamage = attacker.GetDamage().GetValue();

        if (defender != null)
            baseDamage -= defender.GetArmor().GetValue();

        if (baseDamage < 0)
            baseDamage = 0;
        return new Attack((int)baseDamage);
    }
}
