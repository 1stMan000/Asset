using UnityEngine;
using UnityEngine.UI;
using Npc_Manager;
using Npc_AI;
using Enemy_AI;
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

            if (isInteracting && Input.GetMouseButtonUp(0))
            {
                if (Npc_Dialogue.currentSentence.nextSentence != null)
                {
                    Npc_Dialogue.currentSentence = Npc_Dialogue.currentSentence.nextSentence;
                    textAndButtons.text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
                }
                else if (Npc_Dialogue.currentSentence.choices != null)
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
                else
                {
                    isInteracting = false;
                    dialogueWindow.SetActive(false);

                    FirstPersonAIO firstPersonAIO = GetComponent<FirstPersonAIO>();
                    firstPersonAIO.enabled = true;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    Npc_Dialogue.enabled = false;
                    Npc_Dialogue.gameObject.GetComponent<NPC>().enabled = true;
                }
            }

            if (!isInteracting && Input.GetKeyDown(InventoryButton))
            {
                inventory.SetActive(true);
            }
        }

        void Attack()
        {
            if (!isInteracting)
            {
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

                if (Input.GetMouseButtonUp(0))
                {
                    int layerMask = LayerMask.GetMask("Player");
                    layerMask = ~layerMask;
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                    {
                        GameObject attackable = hit.collider.gameObject;
                        AttackTarget(attackable);
                    }
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
                    if (npc.GetComponentInParent<CharacterManager>() != null)
                    {
                        if (!npc.GetComponentInParent<CharacterManager>().isDead)
                        {
                            StartDialogue(npc);
                        }
                    }
                }
            }
        }

        void StartDialogue(GameObject npc)
        {
            if (npc.GetComponentInParent<DialogueManager>() != null)
            {
                if (Is_Npc_Type(npc))
                {
                    Npc_Dialogue = npc.GetComponentInParent<DialogueManager>();

                    isInteracting = true;
                    dialogueWindow.SetActive(true);

                    FirstPersonAIO firstPersonAIO = GetComponent<FirstPersonAIO>();
                    firstPersonAIO.enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;

                    Npc_Dialogue.enabled = true;
                    dialogueWindow.GetComponent<TextAndButtons>().text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
                }
            }
        }

        bool Is_Npc_Type(GameObject npc)
        {
            if (npc.GetComponentInParent<NPC>() != null)
            {
                NPC npcAI = npc.GetComponentInParent<NPC>();
                if (npcAI.enabled)
                {
                    return State_NotScared(npcAI);
                }
                else
                {
                    return false;
                }
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
