using UnityEngine;
using UnityEngine.UI;
using Npc_Manager;
using Npc_AI;
using Enemy_AI;
using PlayerController;

namespace Player_Actions
{
    public class PlayerActions : MonoBehaviour
    {
        public Camera playerCamera;
        public GameObject dialogueWindow;
        public KeyCode InteractButton = KeyCode.E;

        TextAndButtons textAndButtons;

        public LayerMask mask;

        [HideInInspector]
        public bool isInteracting;

        DialogueManager Npc_Dialogue;

        private void Start()
        {
            isInteracting = false;
            textAndButtons = dialogueWindow.GetComponent<TextAndButtons>();
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(InteractButton))
            {
                OpenOrCloseDialogue();
            }

            if (isInteracting && Input.GetMouseButtonDown(0))
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
            }
        }

        void OpenOrCloseDialogue()
        {
            if (!isInteracting)
            {
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, 1))
                {
                    GameObject npc = hit.transform.gameObject;
                    if (npc.GetComponentInParent<CharacterManager>() != null)
                    {
                        if (!npc.GetComponentInParent<CharacterManager>().isDead)
                        {
                            StartConversation(npc);
                        }
                    }
                }
            }
        }

        void StartConversation(GameObject npc)
        {
            if (npc.GetComponentInParent<DialogueManager>() == null)
            {
                return;
            }
            Npc_Dialogue = npc.GetComponentInParent<DialogueManager>();

            if (npc.GetComponentInParent<NPC>() != null)
            {
                NPC npcAI = npc.GetComponentInParent<NPC>();
                if (npcAI.enabled)
                {
                    if (npcAI.currentState == NpcStates.Scared)
                    {
                        return;
                    }
                    else
                    {
                        npcAI.enabled = false;
                    }
                }
            }

            if (npc.GetComponentInParent<EnemyAI>() != null)
            {
                if (npc.GetComponentInParent<EnemyAI>().enabled)
                {
                    return;
                }
            }

            isInteracting = true;
            dialogueWindow.SetActive(true);

            FirstPersonAIO firstPersonAIO = GetComponent<FirstPersonAIO>();
            firstPersonAIO.playerCanMove = false;
            firstPersonAIO.lockAndHideCursor = false;
            firstPersonAIO.enableCameraMovement = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            Npc_Dialogue.enabled = true;
            dialogueWindow.GetComponent<TextAndButtons>().text.GetComponent<Text>().text = Npc_Dialogue.currentSentence.npcText;
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
