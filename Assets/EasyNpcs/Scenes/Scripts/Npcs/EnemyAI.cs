using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using Npc_Manager;
using Npc_AI;

namespace Enemy_AI
{
    public class EnemyAI : WhenAttacking, IDestructible
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

            ManageState();
            Check_To_Protect();
            RotateToTarget();
        }

        void ManageState()
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

        /// <summary>
        /// Protect functions
        /// </summary>
        private void Check_To_Protect()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange);

            foreach (Collider col in cols)
            {
                if (col.transform.root.GetComponent<NPC>() != null)
                {
                    NPC npc = col.transform.root.GetComponent<NPC>();
                    if (npc.broadcastAttacked)
                    {
                        CheckTag(npc);
                    }
                }
            }
        }

        void CheckTag(NPC npc)
        {
            foreach (string protect in Protects)
            {
                if (npc.tag == protect && currentTarget == null)
                {
                    currentTarget = npc.Attacker.transform;
                    return;
                }
            }
        }

        /// <summary>
        /// Sense target functions
        /// </summary>
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
            List<Collider> possibleTargets = PossibleTargets();
            if (possibleTargets.Count > 0)
            {
                Collider nearestTarget = NearestTarget(possibleTargets);
                
                EnemyAI[] enemyAiScripts = Return_All_Valid_EnemyAi_Scripts();
                int howmanyTarget = How_Many_Enemies_Are_Facing_Target(enemyAiScripts, nearestTarget);

                return Check_If_Maximum_Enemies_Are_Facing_Target(nearestTarget, howmanyTarget);
            }
            else
            {
                return null;
            }
        }

        List<Collider> PossibleTargets()
        {
            List<Collider> toReturn = new List<Collider>();

            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, VisionMask);
            foreach (Collider col in cols)
            {
                if (col.transform != this.transform)
                {
                    if (Physics.Linecast(transform.position + Vector3.up * 1.7f, col.transform.position + Vector3.up * 1.7f, out RaycastHit hit, VisionMask))
                    {
                        Check_Tags(col, ref toReturn);
                    }
                }
            }

            return toReturn;
        }

        void Check_Tags(Collider col, ref List<Collider> toReturn)
        {
            for (int i = 0; i < Tags.Capacity; i++)
            {
                if (col.gameObject.CompareTag(Tags[i]))
                {
                    toReturn.Add(col);
                    break;
                }
            }
        }

        Collider NearestTarget(List<Collider> possibleTargets)
        {
            Collider nearestTarget = possibleTargets[0];
            for (int i = 1; i < possibleTargets.Count; i++)
            {
                if (Vector3.Distance(possibleTargets[i].transform.position, transform.position)
                    < Vector3.Distance(nearestTarget.transform.position, transform.position))
                    nearestTarget = possibleTargets[i];
            }

            return nearestTarget;
        }

        EnemyAI[] Return_All_Valid_EnemyAi_Scripts()
        {
            EnemyAI[] enemyAiScripts = GameObject.FindObjectsOfType<EnemyAI>();
            Remove_Disabled_Enemy_Scripts(ref enemyAiScripts);

            return enemyAiScripts;
        }

        void Remove_Disabled_Enemy_Scripts(ref EnemyAI[] enemyAiScripts)
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

        int How_Many_Enemies_Are_Facing_Target(EnemyAI[] enemyAiScripts, Collider nearestTarget)
        {
            int howmanyTarget = 0;
            for (int i = 0; i < enemyAiScripts.Length; i++)
            {
                if (enemyAiScripts[i].currentTarget == nearestTarget.transform)
                {
                    howmanyTarget++;
                }
            }

            return howmanyTarget;
        }

        Transform Check_If_Maximum_Enemies_Are_Facing_Target(Collider target, int how_Many_Enemeies)
        {
            if (how_Many_Enemeies < maximumAttackers)
            {
                return target.transform;
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
            AttackTarget(attacker);
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
