using System.Collections;
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

    public static NpcAI Sense_Nearby_Attacked_Npc(Vector3 position, float VisionRange, LayerMask VisionLayers)
    {
        Collider[] cols = Physics.OverlapSphere(position, VisionRange, VisionLayers);

        foreach (Collider col in cols)
        {
            if (col.gameObject.GetComponent<RunAway>())
            {
                return col.gameObject.GetComponent<NpcAI>();
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
