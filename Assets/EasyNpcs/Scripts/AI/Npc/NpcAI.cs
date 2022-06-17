using System;
using UnityEngine;
using UnityEngine.AI;
using DayandNight;

namespace Npc_AI
{
    public class NpcAI : NpcData, IDestructible
    {
        Animator anim;

        public NavMeshAgent agent { get; private set; }
        public float movementSpeed;
        public float scaredRunningSpeed;
        public float runningDistance;
        public float runningTime;

        [HideInInspector]
        public TextMesh Text;

        DayAndNightControl dayAndNightControl;
        public Behaviour workScript;

        void Start()
        {
            anim = GetComponentInChildren<Animator>();
            agent = GetComponent<NavMeshAgent>();
            Text = GetComponentInChildren<TextMesh>();
            DayAndNightCycle_Initialize();
        }

        void DayAndNightCycle_Initialize()
        {
            dayAndNightControl = FindObjectOfType<DayAndNightControl>();

            if (dayAndNightControl != null)
            {
                dayAndNightControl.OnMorningHandler += GoToWork;
                dayAndNightControl.OnEveningHandler += GoHome;
            }
            else
            {
                Debug.Log("Add in dayAndNight control to scene for use of npc's life cycle");
            }
        }

        void Update()
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
            WatchEnvironment();
        }

        GameObject Attacker;

        void WatchEnvironment()
        {
            Attacker = SenseSurroundings.Sense_Nearby_Attacker(transform.position, VisionRange, VisionLayers);
            if (Attacker != null)
            {
                ChangeState(NpcStates.Scared);
            }
            else
            {
                TriggerConversation(SenseSurroundings.Sense_Nearby_Npc(transform.position, VisionRange, VisionLayers));
            }
        }

        public void ChangeState(NpcStates newState)
        {
            if (currentState == newState)
                return;

            NpcStates prevState = currentState;
            currentState = newState;

            OnStateChanged(prevState, newState);
        }

        private void OnStateChanged(NpcStates prevState, NpcStates newState)
        {
            TurnOffBehaviour(prevState);
            switch (newState)
            {
                case NpcStates.Scared:
                    OnScared();
                    break;

                case NpcStates.GoingHome:
                    GoHome();
                    break;

                case NpcStates.GoingToWork:
                    break;

                case NpcStates.Idle:
                    OnIdle();
                    break;

                case NpcStates.Talking:
                    agent.SetDestination(transform.position);
                    break;

                case NpcStates.Working:
                    if (workScript == null)
                        agent.SetDestination(work.position);
                    else
                        workScript.enabled = true;
                    break;

                default: break;
            }
        }

        void TurnOffBehaviour(NpcStates prevState)
        {
            switch (prevState)
            {
                case NpcStates.Scared:
                    break;
                case NpcStates.GoingToWork:
                    Destroy(GetComponent<LifeCycle>());
                    break;
                case NpcStates.GoingHome:
                    Destroy(GetComponent<LifeCycle>());
                    break;
                case NpcStates.Working:
                    if (workScript != null)
                        workScript.enabled = false;
                    break;
                case NpcStates.Talking:
                    EndConversation();
                    break;
                default:
                    break;
            }
        }

        public void OnAttack(GameObject attacker, Attack attack)
        {
            if (this.enabled == false)
                return;

            Attacker = attacker;
            ChangeState(NpcStates.Scared);
        }

        void OnScared()
        {
            gameObject.AddComponent(typeof(RunAway));
            StartCoroutine(GetComponent<RunAway>().Run(Attacker));
        }

        void OnIdle()
        {
            float time = dayAndNightControl.currentTime;
            if (time > .3f && time < .7f)
            {
                GoToWork();
            }
            else
            {
                GoHome();
            }
        }

        void GoToWork()
        {
            if (!enabled)
                return;

            if (currentState == NpcStates.GoingToWork || currentState == NpcStates.Talking || currentState == NpcStates.Scared)
                return;

            ChangeState(NpcStates.GoingToWork);

            LifeCycle lifeCycle = gameObject.AddComponent<LifeCycle>();
            lifeCycle.Set(this);
            lifeCycle.Start_GOTOWork();
        }

        void GoHome()
        {
            if (!enabled)
                return;

            if (currentState == NpcStates.GoingHome || currentState == NpcStates.Talking || currentState == NpcStates.Scared)
                return;

            ChangeState(NpcStates.GoingHome);

            LifeCycle lifeCycle = gameObject.AddComponent<LifeCycle>();
            lifeCycle.Set(this);
            lifeCycle.Start_GOTOHome();
        }

        [Range(0, 10000)]
        public int converChoose = 0;

        void TriggerConversation(NpcAI npc)
        {
            if (currentState != NpcStates.Scared && npc.currentState != NpcStates.Scared)
            {
                if (UnityEngine.Random.Range(0, 10000) < converChoose) 
                {
                    //Each script has it's own ID. We can use these so one of the npc scripts is more prioritized
                    if (GetInstanceID() > npc.GetInstanceID())
                    {
                        if (GetComponent<RunConversation>() == null)
                        {
                            RunConversation runConversation = gameObject.AddComponent<RunConversation>();
                            runConversation.Set(true, this, npc, null);
                            runConversation.StartConversation();

                            ChangeState(NpcStates.Talking);
                        }
                    }
                }
            }
        }

        public void EndConversation()
        {
            Destroy(GetComponent<RunConversation>());
            GetComponentInChildren<TextMesh>().text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().job.ToString().ToLower();
        }

        private void OnEnable()
        {
            ChangeState(NpcStates.Idle);
        }

        public void OnDestruction(GameObject destroyer)
        {
            enabled = false;
        }

        void OnDisable()
        {
            TurnOffBehaviour(currentState);
            anim.SetFloat("Speed", 0);
        }

        public void EnableCombat()
        {
            this.enabled = false;
        }
    }
}