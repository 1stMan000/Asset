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
        public GameObject tradeInventory;

        public KeyCode InteractButton = KeyCode.E;
        public KeyCode InventoryButton = KeyCode.Tab;

        TextAndButtons textAndButtons;

        public LayerMask mask;

        enum PlayerState { Normal, Dialogue, Inventory, Trade }
        PlayerState playerState;

        Close_Open_TradeInven inventoryActions;

        public int totalCoins;

        private void Start()
        {
            playerState = PlayerState.Normal;
            if (dialogueWindow != null)
            {
                textAndButtons = dialogueWindow.GetComponent<TextAndButtons>();
            }

            inventory = FindObjectOfType<Inven_Initialation>().gameObject;
            tradeInventory = inventory.transform.GetChild(1).gameObject;
            inventoryActions = gameObject.AddComponent<Close_Open_TradeInven>();
        }

        void Update()
        {
            if (playerState == PlayerState.Normal)
            {
                Attack();
                Interact();
                On_InventoryButton_Down(true);
            }
            else
            {
                if (playerState == PlayerState.Dialogue)
                {
                    On_Dialgue_Sequence();
                }
                else
                {
                    On_InventoryButton_Down(false);
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
                    if (CheckState.Check_CharacterManager(chosenObject))
                    {
                        NpcInteract(chosenObject);
                    }
                    else
                    {
                        inventory.transform.GetChild(0).GetComponent<SizeInventoryExample>().inventory.TryAdd(chosenObject.GetComponent<Item>().ItemDefinition.CreateInstance());
                        Destroy(chosenObject);
                    }
                }
            }
        }

        void NpcInteract(GameObject npc)
        {
            if (npc.GetComponentInParent<DialogueManager>().currentSentence != null)
            {
                StartDialogue(npc);
            }
            else
            {
                playerState = PlayerState.Trade;
                inventoryActions.Activate_Trade();
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

        void On_InventoryButton_Down(bool on)
        {
            if (Input.GetKeyDown(InventoryButton))
            {
                Enable_Inventory(on);
            }
        }

        public void Enable_Inventory(bool on)
        {
            ChangeState_To_Inventory(on);
            Set_Character_Script(on);
            inventoryActions.Activate_Inventory(on);
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
