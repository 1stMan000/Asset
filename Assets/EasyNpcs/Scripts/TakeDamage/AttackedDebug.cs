using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedDebug : MonoBehaviour, IAttackable
{
    public void OnAttack(GameObject attacker, Attack attack, bool bashAttack = false)
    {
        if (attack.isCritical)
            Debug.Log("Critical Damage");
        Debug.LogFormat("damage");
    }
}
