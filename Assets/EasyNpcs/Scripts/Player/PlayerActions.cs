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

        enum PlayerState { Normal, Dialogue, Inventory }
        PlayerState playerState;

        private void Start()
        {
            playerState = PlayerState.Normal;
            textAndButtons = dialogueWindow.GetComponent<TextAndButtons>();
        }

        void Update()
        {
            if (playerState == PlayerState.Normal)
            {
                Attack();
                Interact();
                Switch_To_Inventory(true);
            }
            else
            {
                if (playerState == PlayerState.Dialogue)
                {
                    On_Dialgue_Sequence();
                }
                else
                {
                    Switch_To_Inventory(false);
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

        enum InventoryState { Default, Trade}
        InventoryState inventoryState;

        void Interact()
        {
            if (Input.GetKeyDown(InteractButton))
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1))
                {
                    GameObject chosenObject = hit.transform.gameObject;
                    if (CheckState.Check_CharacterManager(chosenObject))
                    {
                        if (chosenObject.GetComponentInParent<DialogueManager>().currentSentence != null)
                        {
                            StartDialogue(chosenObject);
                        }
                        else
                        {
                            inventoryState = InventoryState.Trade;
                            ChangeState_To_Inventory(true);
                            Set_Character_Script(true);
                            Activate_Inventory(true);
                        }
                    }
                    else
                    {
                        inventory.transform.GetChild(0).GetComponent<SizeInventoryExample>().inventory.TryAdd(chosenObject.GetComponent<Item>().ItemDefinition.CreateInstance());
                        Destroy(chosenObject);
                    }
                }
            }
        }

        DialogueManager Npc_Dialogue;

        void StartDialogue(GameObject npc)
        {
            if (npc.GetComponentInParent<DialogueManager>() != null)
            {
                Npc_Dialogue = npc.GetComponentInParent<DialogueManager>();
                if (CheckState.Check_State(npc))
                {
                    Switch_To_DialogueState(true);
                    dialogueWindow.GetComponent<TextAndButtons>().text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
                }
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
                Switch_To_DialogueState(false);
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

        void Switch_To_DialogueState(bool on)
        {
            if (on)
                playerState = PlayerState.Dialogue;
            else
                playerState = PlayerState.Normal;
            Set_Character_Script(on);
            dialogueWindow.SetActive(on);

            Npc_Dialogue.enabled = on;
            Npc_Dialogue.GetComponent<NpcAI>().enabled = !on;
        }

        void Switch_To_Inventory(bool on)
        {
            if (Input.GetKeyDown(InventoryButton))
            {
                ChangeState_To_Inventory(on);
                Set_Character_Script(on);
                Activate_Inventory(on);
            }
        }

        void ChangeState_To_Inventory(bool on)
        {
            if (on)
                playerState = PlayerState.Inventory;
            else
                playerState = PlayerState.Normal;
        }

        void Set_Character_Script(bool on)
        {
            GetComponent<FirstPersonAIO>().enabled = !on;
            CursorManager.SetCursor(on);
        }

        public InventoryInitialation inventoryInitialation;

        void Activate_Inventory(bool on)
        {
            inventory.SetActive(on);
            inventoryInitialation.Inventory_Initialization();
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
