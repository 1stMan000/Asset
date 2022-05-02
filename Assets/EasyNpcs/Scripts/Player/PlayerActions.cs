using System.Collections;
using UnityEngine;
using Npc_Manager;
using Npc_AI;
using Enemy_AI;

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

        private void Start()
        {
            isInteracting = false;
        }

        // Update is called once per frame
        void Update()
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

            currentNpc = npc;
            currentNpc.GetComponent<DialogueManager>().ActivateDialogue();
        }
    }
}
