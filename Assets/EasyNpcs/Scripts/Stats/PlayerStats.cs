using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : CharacterManager
{
    public Slider slider;

    public float experienceForLevelup = 0;

    public void AddExperience()
    {
        experienceForLevelup = experienceForLevelup + 0.05f;
        slider.value = experienceForLevelup;
    }
}
