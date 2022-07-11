using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Npc_AI;

public class LifeCycle : MonoBehaviour
{
    NpcAI npcAI;

    public void Set(NpcAI npc)
    {
        npcAI = npc;
    }

    public void Start_GOTOWork()
    {
        StartCoroutine(GoToWorkCoroutine());
    }
 
    IEnumerator GoToWorkCoroutine()
    {
        npcAI.agent.speed = npcAI.movementSpeed;
        npcAI.agent.SetDestination(npcAI.work.position);
        yield return new WaitUntil(() => Vector3.Distance(transform.position, npcAI.work.position) <= 0.1f);

        npcAI.ChangeState(NpcState.Working);
    }

    public void Start_GOTOHome()
    {
        StartCoroutine(GoHomeCoroutine());
    }

    IEnumerator GoHomeCoroutine()
    {
        npcAI.agent.speed = npcAI.movementSpeed;
        npcAI.agent.SetDestination(npcAI.home.position);

        yield return new WaitUntil(() => npcAI.agent.remainingDistance <= 0.1f && !npcAI.agent.pathPending);

        npcAI.ChangeState(NpcState.Idle);
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }
}
