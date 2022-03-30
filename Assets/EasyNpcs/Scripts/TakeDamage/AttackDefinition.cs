using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npc_Manager;

namespace AttackDef
{
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

        public Attack CreateAttack(CharacterManager attacker, CharacterManager defender)
        {
            float baseDamage = attacker.GetDamage().GetValue();

            if (defender != null)
                baseDamage -= defender.GetArmor().GetValue();

            if (baseDamage < 0)
                baseDamage = 0;
            return new Attack((int)baseDamage);
        }
    }
}
