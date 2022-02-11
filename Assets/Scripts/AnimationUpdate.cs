using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AnimationUpdate : MonoBehaviour
{
    AnimationController controller;
    NavMeshAgent agent;
    CharacterStats stats;

    private bool dontChange = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        controller = GetComponentInChildren<AnimationController>();
        stats = GetComponent<CharacterStats>();
    }

    private void OnEnable()
    {
        dontChange = false;
    }

    private void Update()
    {
        if (dontChange == false)
        {
            ManageAnimations();
        }
    }

    void ManageAnimations()
    {
        //Manage animations
        if (agent.velocity.magnitude == 0)
        {
            //Idle animation if npc isn't moving
            controller.ChangeAnimation(AnimationController.IDLE, AnimatorLayers.ALL);
        }
        else
        {
            if (agent.velocity.magnitude < 2.5f)
            {
                //Walk animation if npc is moving slow
                controller.ChangeAnimation(AnimationController.WALK, AnimatorLayers.ALL);
            }
            else
            {
                //Walk animation if npc is moving fast
                controller.ChangeAnimation(AnimationController.RUN, AnimatorLayers.ALL);
            }
        }

        StartCoroutine(WaitTillNext());
    }

    IEnumerator WaitTillNext()
    {
        dontChange = true;
        yield return new WaitForSeconds(0.2f);
        dontChange = false;
    }
}


