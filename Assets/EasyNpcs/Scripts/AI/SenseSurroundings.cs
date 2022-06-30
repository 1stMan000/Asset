using System.Collections.Generic;
using UnityEngine;
using Npc_AI;
using Enemy_AI;

namespace Sense
{
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

        public static Transform CheckForTargets(GameObject me)
        {
            EnemyAI enemyAI = me.GetComponent<EnemyAI>();
            List<Collider> possibleTargets = SenseSurroundings.PossibleTargets(me.transform.position, enemyAI.VisionRange, enemyAI.VisionLayers, enemyAI.Tags, me);
            if (possibleTargets.Count > 0)
            {
                Collider nearestTarget = SenseSurroundings.NearestTarget(possibleTargets, me.transform.position);
                return SenseSurroundings.Check_If_Maximum_Enemies_Are_Facing_Target(nearestTarget, enemyAI.maximumAttackers);
            }
            else
            {
                return null;
            }
        }

        static List<Collider> PossibleTargets(Vector3 position, float VisionRange, LayerMask VisionLayers, List<string> tags, GameObject me)
        {
            List<Collider> posssibleTargets = new List<Collider>();

            Collider[] cols = Physics.OverlapSphere(position, VisionRange, VisionLayers);
            foreach (Collider col in cols)
            {
                if (col.transform.parent != me.transform)
                {
                    if (Physics.Linecast(position + Vector3.up * 1.7f, col.transform.position + Vector3.up * 1.7f, out RaycastHit hit, VisionLayers))
                    {
                        if (CheckTag(col, tags))
                        {
                            posssibleTargets.Add(col);
                        }
                    }
                }
            }

            return posssibleTargets;
        }

        static bool CheckTag(Collider col, List<string> tags)
        {
            for (int i = 0; i < tags.Capacity; i++)
            {
                if (col.gameObject.CompareTag(tags[i]))
                {
                    return true;
                }
            }

            return false;
        }

        static Collider NearestTarget(List<Collider> possibleTargets, Vector3 position)
        {
            Collider nearestTarget = possibleTargets[0];
            if (possibleTargets.Count > 1)
            {
                for (int i = 1; i < possibleTargets.Count; i++)
                {
                    if (Vector3.Distance(possibleTargets[i].transform.position, position)
                        < Vector3.Distance(nearestTarget.transform.position, position))
                        nearestTarget = possibleTargets[i];
                }
            }
            
            return nearestTarget;
        }

        static Transform Check_If_Maximum_Enemies_Are_Facing_Target(Collider target, int maximumAttackers)
        {
            int number_Of_Enemies_On_Target = Number_Of_Enemies_On_Target(target);
            if (number_Of_Enemies_On_Target < maximumAttackers)
            {
                return target.transform;
            }
            else
            {
                return null;
            }
        }

        static int Number_Of_Enemies_On_Target(Collider target)
        {
            EnemyAI[] enemyAiScripts = Return_All_Valid_EnemyAi_Scripts();

            int enemies = 0;
            for (int i = 0; i < enemyAiScripts.Length; i++)
            {
                if (enemyAiScripts[i].currentTarget == target.transform)
                {
                    enemies++;
                }
            }

            return enemies;
        }

        static EnemyAI[] Return_All_Valid_EnemyAi_Scripts()
        {
            EnemyAI[] enemyAiScripts = GameObject.FindObjectsOfType<EnemyAI>();
            Remove_Disabled_Enemy_Scripts(ref enemyAiScripts);

            return enemyAiScripts;
        }

        static void Remove_Disabled_Enemy_Scripts(ref EnemyAI[] enemyAiScripts)
        {
            for (int i = 0; i < enemyAiScripts.Length; i++)
            {
                if (enemyAiScripts[i].enabled == false)
                {
                    for (int a = i; a < enemyAiScripts.Length - 1; a++)
                    {
                        enemyAiScripts[a] = enemyAiScripts[a + 1];
                    }

                    System.Array.Resize(ref enemyAiScripts, enemyAiScripts.Length - 1);
                }
            }
        }

        public static bool Check_Target_Distance_And_Raycast(Transform me, Transform target, float attackDistance)
        {
            RaycastHit hit;
            Physics.Raycast(me.position + new Vector3(0, 1), target.transform.position - me.position, out hit, Mathf.Infinity);
            if ((me.position - target.position).magnitude <= attackDistance && hit.transform == target)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}