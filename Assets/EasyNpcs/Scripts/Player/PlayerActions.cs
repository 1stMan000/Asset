using UnityEngine;
using UnityEngine.UI;
using Npc_Manager;
using Npc_AI;
using PlayerController;
using FarrokhGames.Inventory.Examples;

namespace Player_Actions
{
    public class PlayerActions : MonoBehaviour
    {
        public Camera playerCamera;
        public GameObject dialogueWindow;
        public GameObject inventory;

        public KeyCode InteractButton = KeyCode.E;
        public KeyCode InventoryButton = KeyCode.Tab;

        TextAndButtons textAndButtons;

        public LayerMask mask;

        bool isInteracting;

        DialogueManager Npc_Dialogue;

        private void Start()
        {
            isInteracting = false;
            textAndButtons = dialogueWindow.GetComponent<TextAndButtons>();
        }

        void Update()
        {
            if (!isInteracting)
            {
                Attack();
                Interact();
            }
            else
            {
                On_Dialgue_Sequence();
            }
            
            if (Input.GetKeyDown(InventoryButton))
            {
                if (!isInteracting)
                {
                    isInteracting = true;
                    GetComponent<FirstPersonAIO>().enabled = false;
                    Cursor_Lock_State(true);
                    Cursor.visible = true;

                    inventory.SetActive(true);
                }
                else
                {
                    isInteracting = false;
                    GetComponent<FirstPersonAIO>().enabled = true;
                    Cursor_Lock_State(false);
                    Cursor.visible = false;

                    inventory.SetActive(false);
                }
            }
        }

        void Attack()
        {
            if (Input.GetMouseButtonUp(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, ~LayerMask.GetMask("Player")))
                {
                    AttackManager.AttackTarget(gameObject, hit.collider.gameObject);
                }
            }
        }

        void Interact()
        {
            if (Input.GetKeyDown(InteractButton))
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1))
                {
                    GameObject chosenObject = hit.transform.gameObject;
                    if (Check_CharacterManager(chosenObject))
                    {
                        StartDialogue(chosenObject);
                    }
                    else
                    {
                        inventory.transform.GetChild(1).GetComponent<SizeInventoryExample>().inventory.TryAdd(chosenObject.GetComponent<Item>().ItemDefinition);
                    }
                }
            }
        }

        bool Check_CharacterManager(GameObject npc)
        {
            if (npc.GetComponent<CharacterManager>() != null)
            {
                if (!npc.GetComponentInParent<CharacterManager>().isDead)
                {
                    return true;
                }

                Debug.Log("Npc is dead");
                return false;
            }
            else
            {
                return false;
            }
        }

        void StartDialogue(GameObject npc)
        {
            if (npc.GetComponentInParent<DialogueManager>() != null)
            {
                Npc_Dialogue = npc.GetComponentInParent<DialogueManager>();
                if (Check_State(npc))
                {
                    Switch_PlayState_To_DialogueState(true);
                    dialogueWindow.GetComponent<TextAndButtons>().text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
                }
            }
        }

        bool Check_State(GameObject npc)
        {
            if (npc.GetComponentInParent<NpcAI>() != null)
            {
                NpcAI npcAI = npc.GetComponentInParent<NpcAI>();
                if (npcAI.enabled)
                {
                    return State_NotScared(npcAI);
                }

                Debug.Log("NpcAI of" + npc + "is not enabled");
                return false;
            }
            else
            {
                Debug.LogWarning(npc + "does not have NpcAi attached");
                return false;
            }
        }

        bool State_NotScared(NpcAI npcAI)
        {
            if (npcAI.currentState == NpcState.Scared)
            {
                Debug.Log("The npc's current state blocks interaction");
                return false;
            }
            else
            {
                npcAI.enabled = false;
                return true;
            }
        }

        void On_Dialgue_Sequence()
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
                Switch_PlayState_To_DialogueState(false);
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

        void Switch_PlayState_To_DialogueState(bool on_Off_Switch)
        {
            isInteracting = on_Off_Switch;
            dialogueWindow.SetActive(on_Off_Switch);

            GetComponent<FirstPersonAIO>().enabled = !on_Off_Switch;

            Cursor_Lock_State(on_Off_Switch);
            Cursor.visible = on_Off_Switch;

            Npc_Dialogue.enabled = on_Off_Switch;
            Npc_Dialogue.gameObject.GetComponent<NpcAI>().enabled = !on_Off_Switch;
        }

        void Cursor_Lock_State(bool on_Off_Switch)
        {
            if (on_Off_Switch)
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
            }
        }

        public void PressButton(int i)
        {
            Disable_Buttons();

            Npc_Dialogue.currentSentence = Npc_Dialogue.currentSentence.choices[i];
            textAndButtons.text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
        }

        void Disable_Buttons()
        {
            foreach (GameObject button in textAndButtons.buttons)
            {
                button.SetActive(false);
            }
            textAndButtons.text.SetActive(true);
        }
    }
}
