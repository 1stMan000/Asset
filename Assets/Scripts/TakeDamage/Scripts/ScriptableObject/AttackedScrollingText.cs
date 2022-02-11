﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackedScrollingText : MonoBehaviour, IAttackable
{
    public ScrollingText Text;
    public Color color;

    CharacterStats stats;

    void Start()
    {
        stats = GetComponent<CharacterStats>();
    }

    public void OnAttack(GameObject attacker, Attack attack, bool bashAttack = false)
    {
        var text = attack.Damage.ToString();

        var scrollingText = Instantiate(Text, transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
        scrollingText.SetText(text);
        scrollingText.SetColor(color);
    }
}
