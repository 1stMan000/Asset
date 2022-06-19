using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Npc_AI;

public class RunAway : MonoBehaviour
{
    NavMeshAgent agent;
    NpcAI npc;

    public GameObject Attacker;

    private float runTimeLeft = 10;

    private void Update()
    {
        runTimeLeft -= Time.deltaTime;
    }

    public IEnumerator Run(GameObject attacker)
    {
        Attacker = attacker;

        agent = GetComponent<NavMeshAgent>();
        npc = GetComponent<NpcAI>();

        agent.speed = npc.scaredRunningSpeed;
        runTimeLeft = npc.runningTime;
        agent.ResetPath();

        while (runTimeLeft > 0)
        {
            Vector3 goal;
            bool isPathValid;
            NavMeshPath path = new NavMeshPath();

            //Get the angle between "attacker" and NPC
            Vector3 distanceIn3D = attacker.transform.position - transform.position;
            float magnitude = new Vector2(distanceIn3D.x, distanceIn3D.z).magnitude;
            Vector2 distance = new Vector2(distanceIn3D.x / magnitude, distanceIn3D.z / magnitude);
            double angleX = Math.Acos(distance.x);
            double angleY = Math.Asin(distance.y);

            //Loop has iteration limit to avoid errors
            int index = 0;
            const int limit = 13;

            //Loop tries to find further point from "attacker" in boundaries of a circle of "runningDistance" radius
            do
            {
                //Rotate point in the circle by (PI / 6 * index)
                angleX += index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                angleY -= index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                distance = new Vector2((float)Math.Cos(angleX), (float)Math.Sin(angleY));
                goal = new Vector3(transform.position.x - distance.x * npc.runningDistance, transform.position.y, transform.position.z - distance.y * npc.runningDistance);

                //Check if NPC can reach this point
                bool samplePosition = NavMesh.SamplePosition(goal, out NavMeshHit hit, npc.runningDistance / 5, agent.areaMask);
                //Calculate path if the point is reachable
                if (samplePosition)
                {
                    agent.CalculatePath(hit.position, path);
                    yield return new WaitUntil(() => path.status != NavMeshPathStatus.PathInvalid);
                    agent.path = path;
                }

                isPathValid = (samplePosition &&
                               path.status != NavMeshPathStatus.PathPartial &&
                               agent.remainingDistance <= npc.runningDistance);

                //Stop loop if it is impossible to find way after "limit" iterations
                if (++index > limit)
                {
                    agent.destination = this.transform.position;
                    break;
                }
            } while (!isPathValid);

            yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= npc.runningDistance / 1.2);
        }

        npc.ChangeState(NpcState.Idle);
        Destroy(this);
    }
}
