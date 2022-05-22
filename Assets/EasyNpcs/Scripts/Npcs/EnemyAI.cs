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
        [HideInInspector]
        public NavMeshAgent agent = null;

        protected Animator anim;

        [HideInInspector]
        public CharacterManager manager;

        [Tooltip("The collider representing the area in which the enemy preffer to stay. " +
                "It can still be lured out of the area by npcs and the player. " +
                "This is an optional field")]
        public Collider PatrolArea;
        [HideInInspector]
        public Transform attackPoint; 

        public bool VisualiseAgentActions;

        public LayerMask VisionMask;
        public float VisionRange;

        [TagSelector] public List<string> Tags;
        public List<string> Protects;

        public EnemyState CurrentState;
        public Transform currentTarget;

        public float AttackDistance;

        public int maximumAttackers = 1;

        [HideInInspector]
        public bool changingState = true;

        public enum weapon { melee, ranged};
        public weapon assignedWeapon; 
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

            #region Editor Only
#if UNITY_EDITOR
            if (VisualiseAgentActions)
            {
                GameObject debugsphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                Destroy(debugsphere.GetComponent<Collider>());
                debugsphere.name = "Target Debugger for " + transform.name;
                GameObject debugparent = GameObject.Find("Debugger");
                if (debugparent == null)
                    debugparent = new GameObject("Debugger");
                debugsphere.transform.SetParent(debugparent.transform);
                DebugSphere = debugsphere.transform;
            }
#endif
            #endregion

            ChangeState(EnemyState.Idle);

            if (VisionRange == 0)
            {
                Debug.Log("Please put the vision range of enemy AI to something bigger than 0");
            }
        }

        void Update()
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);

            ManageState();
            WatchEnvironment();
            RotateToTarget_WhenAttacking();

            #region Editor Only
#if UNITY_EDITOR
            if (VisualiseAgentActions)
            {
                DebugSphere.position = agent.destination;
            }
#endif
            #endregion
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

                case EnemyState.Chasing:
                    agent.speed = 4;
                    if (currentTarget == null) //If target is null, switch to idle state
                    {
                        ChangeState(EnemyState.Idle);
                        return;
                    }
                    else if (currentTarget.GetComponent<CharacterManager>().isDead == true)
                    {
                        currentTarget = null;
                        ChangeState(EnemyState.Idle);
                    }
                    
                    RaycastHit hit;
                    Physics.Raycast(transform.position + new Vector3(0, 1), currentTarget.transform.position - transform.position, out hit, Mathf.Infinity, VisionMask);
                    if ((currentTarget.position - transform.position).magnitude <= AttackDistance)
                    {
                        if (hit.transform == currentTarget)
                        {
                            ChangeState(EnemyState.Attacking);
                        }
                        else
                        {
                            StartCoroutine(nameof(RotateTo), currentTarget.gameObject);
                        }
                    }
                    else
                    {
                        Chase(currentTarget);
                    }
                    break;

                case EnemyState.Attacking:
                    agent.SetDestination(transform.position);

                    if (currentTarget == null)
                    {
                        ChangeState(EnemyState.Idle);
                    }
                    else if (currentTarget.GetComponent<CharacterManager>().isDead == true)
                    {
                        currentTarget = null;
                        ChangeState(EnemyState.Idle);
                    }
                    else
                    {
                        RaycastHit hit1;
                        Physics.Raycast(transform.position + new Vector3(0, 1), currentTarget.transform.position - transform.position, out hit1, Mathf.Infinity, VisionMask);
                        if ((currentTarget.position - transform.position).magnitude <= AttackDistance && hit1.transform == currentTarget)
                        {
                            Attack(currentTarget.gameObject);
                        }
                        else
                        {
                            ChangeState(EnemyState.Chasing);
                        }
                    }
                    break;

                default:
                    break;
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
                ChangeState(EnemyState.Chasing);
            }
        }

        void TryToFindTarget()
        {
            Transform target = CheckForTargets(); 
            if (target != null)
            {
                currentTarget = target;
                ChangeState(EnemyState.Chasing);

                return;
            }

            OnCatchingTarget();
        }

        void OnCatchingTarget()
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

                if (howmanyTarget < maximumAttackers)
                {
                    return nearestTarget.transform;
                }
                else
                {
                    return null;
                }
            }
            else
                return null;
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
                        for (int i = 0; i < Tags.Capacity; i++)
                        {
                            if (col.gameObject.CompareTag(Tags[i]))
                            {
                                toReturn.Add(col);
                                break;
                            }
                        }
                    }
                }
            }

            return toReturn;
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

            return enemyAiScripts;
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

        void OnIdle()
        {
            agent.speed = 2;
            if (attackPoint == null)
            {
                PatrolToAnotherSpot();
            }
            else
            {
                ChangeState(EnemyState.Patrol);
            }
        }

        void Chase(Transform target)
        {
            if (currentTarget == null)
            {
                ChangeState(EnemyState.Idle);
                return;
            }

            if (agent.destination != currentTarget.position)
            {
                currentTarget = target;
                agent.SetDestination(target.position);
            }
        }

        void Attack(GameObject target)
        {
            anim.SetTrigger("Attack");
        }

        public void WhenAttacking(GameObject attacker)
        {
            AttackTarget(attacker);
        }

        void ChangeState(EnemyState state)
        {
            if (CurrentState == state)
                return;

            ManageStateChange(CurrentState, state);
            CurrentState = state;
        }

        void ManageStateChange(EnemyState oldState, EnemyState newState)
        {
            switch (oldState)
            {
                case EnemyState.Attacking:
                    changingState = false;
                    break;
            }

            switch (newState)
            {
                case EnemyState.Attacking:
                    break;
                case EnemyState.Chasing:
                    break;
                case EnemyState.Idle:
                    break;
                case EnemyState.Patrol:
                    if (attackPoint)
                    {
                        agent.SetDestination(attackPoint.position + new Vector3(Random.Range(-10, 10), 0, Random.Range(-10, 10)));
                    }
                    break;
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

        //Check environment to protect if another is being attacked
        private void WatchEnvironment()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange);

            foreach (Collider col in cols)
            {
                if (col.transform.root.GetComponent<NPC>() != null)
                {
                    NPC npc = col.transform.root.GetComponent<NPC>();
                    if (npc.broadcastAttacked)
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
                }
            }
        }

        void RotateToTarget_WhenAttacking()
        {
            if (CurrentState == EnemyState.Attacking)
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

        void OnDrawGizmosSelected()
        {
            if (VisualiseAgentActions)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, VisionRange);
            }
        }
    }
}
