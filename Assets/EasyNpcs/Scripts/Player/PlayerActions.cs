using UnityEngine;
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

        public LayerMask mask;

        [HideInInspector]
        public bool isInteracting;

        GameObject currentNpc;
        DialogueManager Npc_Dialogue;

        private void Start()
        {
            isInteracting = false;
        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetKeyDown(InteractButton))
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
                else
                {
                    PressSpeakButton();
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

            currentNpc = npc;
            currentNpc.GetComponent<DialogueManager>().RotateToPlayer();
        }

        void PressSpeakButton()
        {

        }
    }
}
