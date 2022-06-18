using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Npc_Manager;
using Npc_AI;

namespace Enemy_AI
{
    public class EnemyAI : MonoBehaviour, IDestructible
    {
        private CharacterManager manager;
        private NavMeshAgent agent = null;
        protected Animator anim;

        [Tooltip("The collider representing the area in which the enemy preffer to stay. " +
                "It can still be lured out of the area by npcs and the player. " +
                "This is an optional field")]
        public Collider PatrolArea;
        public Transform attackPoint; 

        public LayerMask VisionMask;
        public float VisionRange;

        [TagSelector] public List<string> Tags;
        public List<string> Protects;

        public EnemyState CurrentState;
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
            manager = GetComponent<CharacterManager>();

            ChangeState(EnemyState.Idle);

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
            RotateToTarget();
        }

        void State_On_Update()
        {
            switch (CurrentState)
            {
                case EnemyState.Patrol:
                    OnPatrol();
                    break;

                case EnemyState.Idle:
                    OnIdle();
                    break;

                case EnemyState.Chase:
                    OnChase();
                    break;

                case EnemyState.Attack:
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
                ChangeState(EnemyState.Chase);
            }
        }

        void TryToFindTarget()
        {
            Transform target = CheckForTargets(); 
            if (target != null)
            {
                currentTarget = target;
                ChangeState(EnemyState.Chase);

                return;
            }

            No_Target_Available();
        }

        void No_Target_Available()
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                ChangeState(EnemyState.Idle);
            }
        }

        Transform CheckForTargets()
        {
            List<Collider> possibleTargets = SenseSurroundings.PossibleTargets(transform.position, VisionRange, VisionMask, Tags, gameObject);
            if (possibleTargets.Count > 0)
            {
                Collider nearestTarget = SenseSurroundings.NearestTarget(possibleTargets, transform.position);
                return SenseSurroundings.Check_If_Maximum_Enemies_Are_Facing_Target(nearestTarget, maximumAttackers);
            }
            else
            {
                return null;
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
                ChangeState(EnemyState.Patrol);
            }
        }

        /// <summary>
        /// Chasing target functions
        /// </summary>
        public float runSpeed = 4;

        void OnChase()
        {
            agent.speed = runSpeed;
            
            if (Check_Conditions_For_Chase())
            {
                if ((currentTarget.position - transform.position).magnitude <= AttackDistance)
                {
                    Check_If_Facing_Target();
                }
                else
                {
                    Chase(currentTarget);
                }
            }
        }

        bool Check_Conditions_For_Chase()
        {
            if (!When_Target_Is_Null() || !Check_If_Dead())
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        bool When_Target_Is_Null()
        {
            if (currentTarget == null)
            {
                ChangeState(EnemyState.Idle);
                return false;
            }

            return true;
        }

        bool Check_If_Dead()
        {
            if (currentTarget.GetComponent<CharacterManager>().isDead == true)
            {
                currentTarget = null;
                ChangeState(EnemyState.Idle);

                return false;
            }

            return true;
        }

        void Check_If_Facing_Target()
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, 1), currentTarget.transform.position - transform.position, out hit, Mathf.Infinity, VisionMask);
            if (hit.transform == currentTarget)
            {
                ChangeState(EnemyState.Attack);
            }
            else
            {
                StartCoroutine(nameof(RotateTo), currentTarget.gameObject);
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

        /// <summary>
        /// Attack functions
        /// </summary>
        void OnAttack()
        {
            agent.SetDestination(transform.position);

            if (currentTarget.GetComponent<CharacterManager>().isDead == true)
            {
                currentTarget = null;
                ChangeState(EnemyState.Idle);
            }
            else
            {
                Check_Target_Distance_And_Raycast();
            }
        }

        void Check_Target_Distance_And_Raycast()
        {
            RaycastHit hit;
            Physics.Raycast(transform.position + new Vector3(0, 1), currentTarget.transform.position - transform.position, out hit, Mathf.Infinity, VisionMask);
            if ((currentTarget.position - transform.position).magnitude <= AttackDistance && hit.transform == currentTarget)
            {
                Attack(currentTarget.gameObject);
            }
            else
            {
                ChangeState(EnemyState.Chase);
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

        /// <summary>
        /// State maintanance functions
        /// </summary>
        /// <param name="state"></param>
        void ChangeState(EnemyState state)
        {
            if (CurrentState == state)
                return;

            ManageStateChange(CurrentState, state);
            CurrentState = state;
        }

        void ManageStateChange(EnemyState oldState, EnemyState newState)
        {
            switch (newState)
            {
                case EnemyState.Attack:
                    break;
                case EnemyState.Chase:
                    break;
                case EnemyState.Idle:
                    break;
                case EnemyState.Patrol:
                    To_Attack_Point();
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
            const int IterationLimit = 25;
            Vector3 dest;
            //iteration limit to avoid stack overflow
            for (int i = 0; i < IterationLimit; i++)
            {
                if (PatrolArea == null)
                {
                    //Pick spot within X4 VisionRange 
                    dest = new Vector3(
                        Random.Range(transform.position.x - VisionRange * 2, transform.position.x + VisionRange * 2),
                        (transform.position.y),
                        Random.Range(transform.position.z - VisionRange * 2, transform.position.z + VisionRange * 2)
                        );
                }
                else
                {
                    //Pick spot within Patrol Area collider
                    dest = new Vector3(
                        Random.Range(PatrolArea.bounds.min.x, PatrolArea.bounds.max.x),
                        0,
                        Random.Range(PatrolArea.bounds.min.z, PatrolArea.bounds.max.z)
                        );
                }
                if (NavMesh.SamplePosition(dest, out NavMeshHit hit, VisionRange, agent.areaMask))
                {
                    ChangeState(EnemyState.Patrol);
                    agent.SetDestination(hit.position);
                    return;
                }
            }

            ChangeState(EnemyState.Idle);
        }

        void RotateToTarget()
        {
            if (CurrentState == EnemyState.Attack)
            {
                StartCoroutine(nameof(RotateTo), currentTarget.gameObject);
            }
        }

        public IEnumerator RotateTo(GameObject target)
        {
            Quaternion lookRotation;
            do
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / GetComponent<NavMeshAgent>().angularSpeed));
                yield return new WaitForEndOfFrame();
            } while (true);
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
