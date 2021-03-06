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
        public Transform attackPoint; //Npc attacks enemies while going to area 

        public bool VisualiseAgentActions;

        public LayerMask VisionMask;
        public float VisionRange;
        public LayerMask WhatCanThisEnemyAttack;
        [TagSelector] public List<string> Tags;
        public List<string> Protects;

        public EnemyState CurrentState;
        public Transform currentTarget;

        public float AttackDistance;

        public int maximumAmountofAttackers = 1;

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

            if (CurrentState == EnemyState.Attacking)
                RotateTo(currentTarget.gameObject);

            #region Editor Only
#if UNITY_EDITOR
            if (VisualiseAgentActions)
            {
                DebugSphere.position = agent.destination;
            }
#endif
            #endregion
        }

        void ManageState() //Checks which is the current state and makes the Ai do the chosen behaviours every Update
        {
            switch (CurrentState)
            {
                case EnemyState.Patroling:
                    if (currentTarget == null)
                    {
                        Transform target = CheckForTargets(); //Find new target and start chasing it, else patrol
                        if (target != null)
                        {
                            currentTarget = target;
                            ChangeState(EnemyState.Chasing);

                            return;
                        }

                        if (agent.remainingDistance <= agent.stoppingDistance)
                        {
                            ChangeState(EnemyState.Idle);
                        }
                    }
                    else
                    {
                        ChangeState(EnemyState.Chasing);
                    }
                    break;

                case EnemyState.Idle:
                    agent.speed = 2;
                    if (attackPoint == null) //AttackPoint is the point will go to while attacking any enemies along the way.
                    {
                        PatrolToAnotherSpot();
                    }
                    else
                    {
                        ChangeState(EnemyState.Patroling);
                    }
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
                    if ((currentTarget.position - transform.position).magnitude <= AttackDistance && hit.transform == currentTarget)
                    {
                        ChangeState(EnemyState.Attacking);
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
                        RotateTo(currentTarget.gameObject);

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

        Transform CheckForTargets()
        {
            List<Collider> possibleTargets = new List<Collider>();

            //Return all attackable target colliders in sphere
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, WhatCanThisEnemyAttack);

            foreach (Collider col in cols)
            {
                if (VisualiseAgentActions)
                    Debug.DrawRay(transform.position, (col.transform.position - transform.position).normalized * VisionRange, Color.red);

                //Make sure the collider is not owned by this Ai
                if (col.transform == this.transform)
                    continue;

                //Check if AI can see the target 
                if (Physics.Linecast(transform.position, col.transform.position, out RaycastHit hit, VisionMask))
                {
                    if (hit.collider != col)
                        continue;
                }
                else
                    continue;

                //Check if collider has attackable tag
                for (int i = 0; i < Tags.Capacity; i++)
                {
                    if (col.gameObject.CompareTag(Tags[i]))
                    {
                        possibleTargets.Add(col);
                        break;
                    }
                }
            }

            if (possibleTargets.Count > 0)
            {
                //Find the nearest target
                Collider nearestTarget = possibleTargets[0];
                for (int i = 1; i < possibleTargets.Count; i++)
                {
                    if (Vector3.Distance(possibleTargets[i].transform.position, transform.position)
                        < Vector3.Distance(nearestTarget.transform.position, transform.position))
                        nearestTarget = possibleTargets[i];
                }

                //Checks if maximum amout of targets are filled per target
                EnemyAI[] combatBases = GameObject.FindObjectsOfType<EnemyAI>();
                for (int i = 0; i < combatBases.Length; i++)
                {
                    if (GetComponent<EnemyAI>() == combatBases[i] || combatBases[i].enabled == false)
                    {
                        for (int a = i; a < combatBases.Length - 1; a++)
                        {
                            combatBases[a] = combatBases[a + 1]; //Moving elements downwards, to fill the gap at [index]
                        }
                        System.Array.Resize(ref combatBases, combatBases.Length - 1);
                    }
                }

                int howmanyTarget = 0;
                for (int i = 0; i < combatBases.Length; i++)
                {
                    if (combatBases[i].currentTarget == nearestTarget.transform)
                    {
                        howmanyTarget++;
                    }
                }

                if (howmanyTarget < maximumAmountofAttackers)
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
                case EnemyState.Patroling:
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
                    ChangeState(EnemyState.Patroling);
                    agent.SetDestination(hit.position);
                    return;
                }
            }

            ChangeState(EnemyState.Idle);
        }

        void OnDrawGizmosSelected()
        {
            if (VisualiseAgentActions)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(transform.position, VisionRange);
            }
        }

        public void WhenAttacking(GameObject attacker)
        {
            AttackTarget(attacker);
        }

        public void OnAttack(GameObject attacker, Attack attack)
        {
            currentTarget = attacker.transform;
        }

        public void OnDestruction(GameObject destroyer)
        {
            enabled = false;
        }

        //Check environment to protect if another is being attacked
        private void WatchEnvironment()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange);

            foreach (Collider col in cols)
            {
                NPC npc = col.transform.root.GetComponent<NPC>();
                if (npc != null)
                {
                    if (npc.isAttacked)
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

        void RotateTo(GameObject target)
        {
            Vector3 direction = new Vector3(target.transform.position.x - transform.position.x, 0f, target.transform.position.z - transform.position.z);
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2 / (Quaternion.Angle(transform.rotation, lookRotation) / agent.angularSpeed));
        }
    }
}
