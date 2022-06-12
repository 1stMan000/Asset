using UnityEngine;
using Npc_Manager;

public static class AttackManager
{
    public static void AttackTarget(GameObject attacker, GameObject target)
    {
        Attack attack = new Attack(10);

        var attackables = target.GetComponentsInChildren(typeof(IDestructible));
        foreach (IDestructible attackable in attackables)
        {
            attackable.OnAttack(attacker, attack);
        }
    }

    public static Attack CreateAttack(CharacterManager attacker, CharacterManager defender)
    {
        float baseDamage = attacker.GetDamage().GetValue();

        if (defender != null)
            baseDamage -= defender.GetArmor().GetValue();

        if (baseDamage < 0)
            baseDamage = 0;
        return new Attack((int)baseDamage);
    }
}
