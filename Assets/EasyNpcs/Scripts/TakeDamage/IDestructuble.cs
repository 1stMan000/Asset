using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDestructible
{
    void OnAttack(GameObject attacker, Attack attack);
    void OnDestruction(GameObject destroyer);
}
