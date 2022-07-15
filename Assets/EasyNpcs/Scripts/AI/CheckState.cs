using UnityEngine;
using Npc_AI;

namespace Npc_Manager
{
    public static class CheckState
    {
        public static bool Check_CharacterManager(GameObject npc)
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

        public static bool Check_State(GameObject npc)
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

        static bool State_NotScared(NpcAI npcAI)
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
    }
}
