using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Npc_Manager;
using Sense;
using Rotation;

namespace Enemy_AI
{
    public class EnemyAI : MonoBehaviour, IDestructible
    {
        private NavMeshAgent agent = null;
        protected Animator anim;

        [Tooltip("The collider representing the area in which the enemy preffer to stay. " +
                "It can still be lured out of the area by npcs and the player. " +
                "This is an optional field")]
        public Collider patrolArea;
        public Transform attackPoint; 

        public LayerMask VisionMask;
        public float VisionRange;

        [TagSelector] public List<string> Tags;
        public List<string> Protects;

        public EnemeyState CurrentState;
        public Transform currentTarget;

        public float AttackDistance;

        public int maximumAttackers = 1;

        public enum Weapon { melee, ranged};
        public Weapon assignedWeapon; 
        public Projectile projectile;
        public float launchHight;

        #region Editor Only

#if UNITY_EDITOR
        Transform DebugSphere;
#endif
        #endregion

        void Start()
        {
            anim = GetComponentInChildren<Animator>();
            agent = GetComponent<NavMeshAgent>();

            ChangeState(EnemeyState.Idle);

            if (VisionRange <= 0)
            {
                Debug.Log("Please put the vision range of enemy AI to something bigger than 0");
            }
        }

        void Update()
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);

            State_On_Update();
            Protect();
        }

        void State_On_Update()
        {
            switch (CurrentState)
            {
                case EnemeyState.Patrol:
                    OnPatrol();
                    break;

                case EnemeyState.Idle:
                    OnIdle();
                    break;

                case EnemeyState.Chase:
                    OnChase();
                    break;

                case EnemeyState.Attack:
                    OnAttack();
                    break;

                default:
                    break;
            }
        }

        private void Protect()
        {
            if (currentTarget == null)
            {
                currentTarget = SenseSurroundings.BattleAI_Sense_Friendly_Attacked(transform.position, VisionRange, VisionMask, Protects);
            }
        }

        void OnPatrol()
        {
            if (currentTarget == null)
            {
                TryToFindTarget();
            }
            else
            {
                ChangeState(EnemeyState.Chase);
            }
        }

        void TryToFindTarget()
        {
            Transform target = SenseSurroundings.CheckForTargets(gameObject); 
            if (target != null)
            {
                currentTarget = target;
                ChangeState(EnemeyState.Chase);

                return;
            }

            No_Target_Available();
        }

        void No_Target_Available()
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                ChangeState(EnemeyState.Idle);
            }
        }

        public float walkSpeed = 2;

        void OnIdle()
        {
            agent.speed = walkSpeed;
            if (attackPoint == null)
            {
                PatrolToAnotherSpot();
            }
            else
            {
                ChangeState(EnemeyState.Patrol);
            }
        }

        public float runSpeed = 4;

        void OnChase()
        {
            agent.speed = runSpeed;
            
            if (currentTarget.GetComponent<CharacterManager>().isDead == false)
            {
                if (SenseSurroundings.Check_Target_Distance_And_Raycast(transform, currentTarget, AttackDistance))
                {
                    ChangeState(EnemeyState.Attack);
                }
                else
                {
                    Chase(currentTarget);
                }
            }
        }

        void Chase(Transform target)
        {
            if (agent.destination != currentTarget.position)
            {
                currentTarget = target;
                agent.SetDestination(target.position);
            }
        }

        void OnAttack()
        {
            agent.SetDestination(transform.position);

            if (currentTarget.GetComponent<CharacterManager>().isDead == true)
            {
                currentTarget = null;
                ChangeState(EnemeyState.Idle);

                return;
            }

            if (SenseSurroundings.Check_Target_Distance_And_Raycast(transform, currentTarget, AttackDistance))
            {
                Attack(currentTarget.gameObject);
            }
            else
            {
                ChangeState(EnemeyState.Chase);
            }
        }

        void Attack(GameObject target)
        {
            anim.SetTrigger("Attack");
        }

        public void Attack_Anim_Finished(GameObject attacker)
        {
            AttackManager.AttackTarget(gameObject, attacker);
        }

        void ChangeState(EnemeyState newState)
        {
            if (CurrentState == newState)
                return;

            TurnOffBehaviour(CurrentState);
            OnStateChange(CurrentState, newState);
            CurrentState = newState;
        }

        void OnStateChange(EnemeyState oldState, EnemeyState newState)
        {
            switch (newState)
            {
                case EnemeyState.Attack:
                    Rotate rotate = gameObject.AddComponent<Rotate>();
                    rotate.RotateTo(currentTarget.gameObject);
                    break;
                case EnemeyState.Chase:
                    break;
                case EnemeyState.Idle:
                    break;
                case EnemeyState.Patrol:
                    To_Attack_Point();
                    break;
            }
        }

        void TurnOffBehaviour(EnemeyState prevState)
        {
            switch (prevState)
            {
                case EnemeyState.Attack:
                    Destroy(GetComponent<Rotate>());
                    break;
            }
        }

        void To_Attack_Point()
        {
            if (attackPoint)
            {
                agent.SetDestination(attackPoint.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)));
            }
        }

        //Pick random spot and start moving there
        void PatrolToAnotherSpot()
        {
            Vector3 dest;
            if (CalculatePatrol.CalculateSpots(this, 25, out dest))
            {
                ChangeState(EnemeyState.Patrol);
                agent.SetDestination(dest);
            }
            else
            {
                ChangeState(EnemeyState.Idle);
            }
        }

        public void OnAttack(GameObject attacker, Attack attack)
        {
            currentTarget = attacker.transform;
        }

        public void OnDestruction(GameObject destroyer)
        {
            enabled = false;
        }
    }
}