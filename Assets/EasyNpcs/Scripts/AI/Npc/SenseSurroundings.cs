using System.Collections.Generic;
using UnityEngine;
using Npc_AI;

public static class SenseSurroundings 
{
    public static GameObject Sense_Nearby_Attacker(Vector3 position, float VisionRange, LayerMask VisionLayers)
    {
        Collider[] cols = Physics.OverlapSphere(position, VisionRange, VisionLayers);

        foreach (Collider col in cols)
        {
            if (col.gameObject.GetComponent<RunAway>())
            {
                return col.gameObject.GetComponent<RunAway>().Attacker;
            }
        }

        return null;
    }

    public static Transform Sense_Nearby_Attacked_Npc(Vector3 position, float VisionRange, LayerMask VisionLayers, List<string> tags)
    {
        Collider[] cols = Physics.OverlapSphere(position, VisionRange, VisionLayers);

        foreach (Collider col in cols)
        {
            if (col.gameObject.GetComponent<RunAway>())
            {
                NpcAI npcAI = col.gameObject.GetComponent<NpcAI>();
                return CheckTag(npcAI, tags);
            } 
        }

        return null;
    }

    static Transform CheckTag(NpcAI npc, List<string> tags)
    {
        foreach (string tag in tags)
        {
            if (npc.tag == tag)
            {
                return npc.GetComponent<RunAway>().Attacker.transform;
            }
        }

        return null;
    }

    public static NpcAI Sense_Nearby_Npc(Vector3 position, float VisionRange, LayerMask VisionLayers)
    {
        Collider[] cols = Physics.OverlapSphere(position, VisionRange, VisionLayers);

        foreach (Collider col in cols)
        {
            if (col.gameObject.GetComponent<NpcAI>())
            {
                NpcAI npc = col.gameObject.GetComponent<NpcAI>();
                return npc;
            }
        }

        return null;
    }
}
