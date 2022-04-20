using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enemy_AI;

public class OnAttackAnim : StateMachineBehaviour
{
    EnemyAI enemyAI;
    GameObject thisNpc;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
       enemyAI = animator.GetComponentInParent<EnemyAI>();
       thisNpc = enemyAI.gameObject;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (enemyAI.assignedWeapon == EnemyAI.weapon.melee)
            enemyAI.WhenAttacking(enemyAI.currentTarget.gameObject);
        else
        {
            Projectile projectile = Instantiate(enemyAI.projectile, thisNpc.transform.position + thisNpc.transform.forward * 1 + new Vector3(0, enemyAI.launchHight, 0), enemyAI.transform.rotation);
            projectile.Fire(thisNpc, enemyAI.currentTarget.gameObject, enemyAI.currentTarget.rotation, 10, 10);
        }
    }

    Quaternion TakeAim()
    {
        float dis = Vector3.Distance(thisNpc.transform.position, enemyAI.currentTarget.transform.position);
        float startRot = 2 * Mathf.Sqrt(dis * 2);
        float angle = Vector2.Angle(new Vector2(1, startRot), new Vector2(1, 0));

        Quaternion AimRotation = enemyAI.transform.rotation;

        // Get target position and rebase to make archer the origin
        Vector3 temp1 = enemyAI.currentTarget.transform.position;
        Vector3 temp2 = thisNpc.transform.position;
        temp1.y = 0;
        temp2.y = 0;

        float x = Vector3.Distance(temp1, temp2);
        float y = enemyAI.currentTarget.transform.position.y - (thisNpc.transform.position.y + enemyAI.launchHight);

        // Getting typeofAttack values
        float gravityFactor = 10;
        float speed = 10;
        float f1 = speed * speed;
        float f2 = Mathf.Sqrt((f1 * f1) - gravityFactor * (gravityFactor * (x * x) + 2 * y * f1));
        float AimOffset1 = Mathf.Atan((f1 + f2) / (gravityFactor * x));
        float AimOffset2 = Mathf.Atan((f1 - f2) / (gravityFactor * x));

        float AimOffset = Mathf.Abs(AimOffset1) > Mathf.Abs(AimOffset2) ? AimOffset2 : AimOffset1;
        AimOffset = Mathf.Asin(gravityFactor * x / (speed * speed)) / 2;

        AimRotation *= Quaternion.AngleAxis(AimOffset, enemyAI.transform.forward);

        return AimRotation;
    }

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    //override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that sets up animation IK (inverse kinematics)
    //}
}
