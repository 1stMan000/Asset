using System.Collections.Generic;
using UnityEngine;
using Npc_AI;

public static class SenseSurroundings 
{
    public static GameObject NPC_Sense_Attacker(Vector3 position, float VisionRange, LayerMask VisionLayers)
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

    public static Transform BattleAI_Sense_Friendly_Attacked(Vector3 position, float VisionRange, LayerMask VisionLayers, List<string> tags)
    {
        Collider[] cols = Physics.OverlapSphere(position, VisionRange, VisionLayers);

        foreach (Collider col in cols)
        {
            if (col.gameObject.GetComponent<RunAway>())
            {
                GameObject npc = CheckTag(col.gameObject, tags);
                if (npc != null)
                {
                    return npc.GetComponent<RunAway>().Attacker.transform;
                }

                return null;
            } 
        }

        return null;
    }

    static GameObject CheckTag(GameObject npc, List<string> tags)
    {
        foreach (string tag in tags)
        {
            if (npc.tag == tag)
            {
                return npc;
            }
        }

        return null;
    }

    public static List<Collider> PossibleTargets(Vector3 position, float VisionRange, LayerMask VisionLayers, List<string> tags, GameObject me)
    {
        List<Collider> posssibleTargets = new List<Collider>();

        Collider[] cols = Physics.OverlapSphere(position, VisionRange, VisionLayers);
        foreach (Collider col in cols)
        {
            if (col.transform.parent != me.transform)
            {
                if (Physics.Linecast(position + Vector3.up * 1.7f, col.transform.position + Vector3.up * 1.7f, out RaycastHit hit, VisionLayers))
                {
                    posssibleTargets.Add(CheckTag(col, tags));
                }
            }
        }

        return posssibleTargets;
    }

    static Collider CheckTag(Collider col, List<string> tags)
    {
        Collider collider = new Collider();

        for (int i = 0; i < tags.Capacity; i++)
        {
            if (col.gameObject.CompareTag(tags[i]))
            {
                collider = col;
                break;
            }
        }

        return collider;
    }
}
