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
            manager = GetComponent<CharacterManager>();

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
            RotateToTarget();
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
            Transform target = CheckForTargets(); 
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
                ChangeState(EnemeyState.Patrol);
            }
        }

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
                ChangeState(EnemeyState.Idle);
                return false;
            }

            return true;
        }

        bool Check_If_Dead()
        {
            if (currentTarget.GetComponent<CharacterManager>().isDead == true)
            {
                currentTarget = null;
                ChangeState(EnemeyState.Idle);

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
                ChangeState(EnemeyState.Attack);
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

        void OnAttack()
        {
            agent.SetDestination(transform.position);

            if (currentTarget.GetComponent<CharacterManager>().isDead == true)
            {
                currentTarget = null;
                ChangeState(EnemeyState.Idle);
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
                    ChangeState(EnemeyState.Patrol);
                    agent.SetDestination(hit.position);
                    return;
                }
            }

            ChangeState(EnemeyState.Idle);
        }

        void RotateToTarget()
        {
            if (CurrentState == EnemeyState.Attack)
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
