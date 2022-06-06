using UnityEngine;
using UnityEngine.UI;
using Npc_Manager;
using Npc_AI;
using PlayerController;

namespace Player_Actions
{
    public class PlayerActions : WhenAttacking
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

        // Update is called once per frame
        void Update()
        {
            Attack();
            OpenOrCloseDialogue();
            On_Dialgue_Sequence();

            if (!isInteracting && Input.GetKeyDown(InventoryButton))
            {
                inventory.SetActive(true);
            }
        }

        void Attack()
        {
            if (!isInteracting && Input.GetMouseButtonUp(0))
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                int layerMask = LayerMask.GetMask("Player");
                layerMask = ~layerMask;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                {
                    GameObject attackable = hit.collider.gameObject;
                    AttackTarget(attackable);
                }
            }
        }

        void OpenOrCloseDialogue()
        {
            if (Input.GetKeyDown(InteractButton) && !isInteracting)
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1))
                {
                    GameObject npc = hit.transform.gameObject;
                    if (Check_CharacterManager(npc))
                    {
                        StartDialogue(npc);
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
            if (npc.GetComponentInParent<NPC>() != null)
            {
                NPC npcAI = npc.GetComponentInParent<NPC>();
                if (npcAI.enabled)
                {
                    return State_NotScared(npcAI);
                }

                return false;
            }
            else
            {
                return false;
            }
        }

        bool State_NotScared(NPC npcAI)
        {
            if (npcAI.currentState == NpcStates.Scared)
            {
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
            if (isInteracting && Input.GetMouseButtonUp(0))
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
                button.SetActive(true);
                button.GetComponentInChildren<Text>().text = Npc_Dialogue.currentSentence.choices[choiceNum].playerText;
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
            Npc_Dialogue.gameObject.GetComponent<NPC>().enabled = !on_Off_Switch;
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

        public void PressButton0()
        {
            foreach (GameObject button in textAndButtons.buttons)
            {
                button.SetActive(false);
            }
            textAndButtons.text.SetActive(true);

            Npc_Dialogue.currentSentence = Npc_Dialogue.currentSentence.choices[0];
            textAndButtons.text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
        }
    }
}
