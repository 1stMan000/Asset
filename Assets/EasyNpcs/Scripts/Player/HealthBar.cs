using UnityEngine;
using UnityEngine.UI;
using Npc_Manager;

namespace Debug_Health
{
    public class HealthBar : MonoBehaviour
    {
        [SerializeField] private CharacterManager character;
        [SerializeField] private Image healthImage;

        void Start()
        {
            character.OnHealthValueChanged += HandleHealthValueChanged;

            HandleHealthValueChanged();
        }

        private void HandleHealthValueChanged()
        {
            Debug.Log($"Health: {character.GetCurrentHealth().GetValue()}/{character.GetMaxHealth().GetValue()}");

            healthImage.fillAmount = character.GetCurrentHealth().GetValue() / character.GetMaxHealth().GetValue();
        }
    }
}