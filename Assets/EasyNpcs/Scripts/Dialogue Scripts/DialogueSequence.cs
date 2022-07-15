using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Npc_AI;

public class DialogueSequence : MonoBehaviour
{
    DialogueManager Npc_Dialogue;
    TextAndButtons textAndButtons;

    public DialogueSequence(DialogueManager npc_Dialogue, TextAndButtons _textAndButtons, bool on)
    {
        Npc_Dialogue = npc_Dialogue;
        textAndButtons = _textAndButtons;   

        Npc_Dialogue.enabled = on;
        Npc_Dialogue.gameObject.GetComponent<NpcAI>().enabled = !on;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Change_State_Of_Dialogue();
        }
    }

    void Change_State_Of_Dialogue()
    {
        if (Npc_Dialogue.currentSentence.nextSentence != null)
        {
            Change_To_NextSentence();
        }
        else if (Npc_Dialogue.currentSentence.choices != null)
        {
            Activate_Choices_UI();
        }
        else
        {
            //Switch_To_DialogueState(false);
        }
    }

    void Change_To_NextSentence()
    {
        Npc_Dialogue.currentSentence = Npc_Dialogue.currentSentence.nextSentence;
        textAndButtons.text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
    }

    void Activate_Choices_UI()
    {
        textAndButtons.text.SetActive(false);

        int choiceNum = 0;
        foreach (GameObject button in textAndButtons.buttons)
        {
            if (Npc_Dialogue.currentSentence.choices.Count > choiceNum)
            {
                button.SetActive(true);
                button.GetComponentInChildren<Text>().text = Npc_Dialogue.currentSentence.choices[choiceNum].playerText;
            }
            else
            {
                break;
            }

            choiceNum++;
        }
    }
}
