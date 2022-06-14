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
        if (enemyAI.currentTarget != null)
        {
            if (enemyAI.assignedWeapon == EnemyAI.Weapon.melee)
                enemyAI.Attack_Anim_Finished(enemyAI.currentTarget.gameObject);
            else
            {
                Projectile projectile = Instantiate(enemyAI.projectile, thisNpc.transform.position + thisNpc.transform.forward * 1 + new Vector3(0, enemyAI.launchHight, 0), enemyAI.transform.rotation);
                projectile.Fire(thisNpc, enemyAI.currentTarget.gameObject, enemyAI.currentTarget.rotation, 10, 10);
            }
        }
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
