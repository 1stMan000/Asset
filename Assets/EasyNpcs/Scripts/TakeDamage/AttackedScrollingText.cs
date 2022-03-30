using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npc_Manager;

namespace Debug_Attack
{
    public class AttackedScrollingText : MonoBehaviour, IDestructible
    {
        public ScrollingText Text;
        public Color color;

        CharacterManager stats;

        void Start()
        {
            stats = GetComponent<CharacterManager>();
        }

        public void OnAttack(GameObject attacker, Attack attack)
        {
            var text = attack.Damage.ToString();

            var scrollingText = Instantiate(Text, transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
            scrollingText.SetText(text);
            scrollingText.SetColor(color);
        }

        public void OnDestruction(GameObject destroyer)
        {

        }
    }
}