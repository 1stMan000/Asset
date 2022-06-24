using UnityEngine;
using UnityEngine.AI;
using Enemy_AI;

public static class CalculatePatrol
{
    public static bool CalculateSpots(EnemyAI me, int iterationLimit, out Vector3 dest)
    {
        NavMeshAgent agent = me.GetComponent<NavMeshAgent>();

        //iteration limit to avoid stack overflow
        for (int i = 0; i < iterationLimit; i++)
        {
            if (me.patrolArea == null)
            {
                //Pick spot within X4 VisionRange 
                dest = new Vector3(
                    Random.Range(me.transform.position.x - me.VisionRange * 2, me.transform.position.x + me.VisionRange * 2),
                    (me.transform.position.y),
                    Random.Range(me.transform.position.z - me.VisionRange * 2, me.transform.position.z + me.VisionRange * 2)
                    );
            }
            else
            {
                //Pick spot within Patrol Area collider
                dest = new Vector3(
                    Random.Range(me.patrolArea.bounds.min.x, me.patrolArea.bounds.max.x),
                    0,
                    Random.Range(me.patrolArea.bounds.min.z, me.patrolArea.bounds.max.z)
                    );
            }

            if (NavMesh.SamplePosition(dest, out NavMeshHit hit, me.VisionRange, agent.areaMask))
            {
                dest = hit.position;
                return true;
            }
        }

        dest = new Vector3();
        return false;
    }
}
