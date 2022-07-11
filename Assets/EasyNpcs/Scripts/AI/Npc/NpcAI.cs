using System;
using UnityEngine;
using DayandNight;
using Sense;

namespace Npc_AI
{
    public class NpcAI : NpcData, IDestructible
    {
        public Job job;
        public Gender gender;

        public float movementSpeed;
        public float scaredRunningSpeed;
        public float runningDistance;
        public float runningTime;

        [HideInInspector]
        public TextMesh Text;

        DayAndNightControl dayAndNightControl;
        public Behaviour workScript;
        public Transform home;
        public Transform work;

        private NpcState _currentState;

        public NpcState currentState
        {
            get
            {
                return _currentState;
            }
            protected set
            {
                _currentState = value;
            }
        }

        protected override void Start()
        {
            base.Start();
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

        protected override void Update()
        {
            base.Update();
            WatchEnvironment();
        }

        GameObject Attacker;

        void WatchEnvironment()
        {
            Attacker = SenseSurroundings.NPC_Sense_Attacker(transform.position, VisionRange, VisionLayers);
            if (Attacker != null)
            {
                ChangeState(NpcState.Scared);
            }
            else
            {
                TriggerConversation(SenseSurroundings.Sense_Nearby_Npc(transform.position, VisionRange, VisionLayers));
            }
        }

        public void ChangeState(NpcState newState)
        {
            if (currentState == newState)
                return;

            NpcState prevState = currentState;
            currentState = newState;

            OnStateChanged(prevState, newState);
        }

        private void OnStateChanged(NpcState prevState, NpcState newState)
        {
            TurnOffBehaviour(prevState);
            switch (newState)
            {
                case NpcState.Scared:
                    OnScared();
                    break;

                case NpcState.GoingHome:
                    GoHome();
                    break;

                case NpcState.GoingToWork:
                    break;

                case NpcState.Idle:
                    OnIdle();
                    break;

                case NpcState.Talking:
                    agent.SetDestination(transform.position);
                    break;

                case NpcState.Working:
                    if (workScript == null)
                        agent.SetDestination(work.position);
                    else
                        workScript.enabled = true;
                    break;

                default: break;
            }
        }

        void TurnOffBehaviour(NpcState prevState)
        {
            switch (prevState)
            {
                case NpcState.Scared:
                    break;
                case NpcState.GoingToWork:
                    Destroy(GetComponent<LifeCycle>());
                    break;
                case NpcState.GoingHome:
                    Destroy(GetComponent<LifeCycle>());
                    break;
                case NpcState.Working:
                    if (workScript != null)
                        workScript.enabled = false;
                    break;
                case NpcState.Talking:
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
            ChangeState(NpcState.Scared);
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

            if (currentState == NpcState.GoingToWork || currentState == NpcState.Talking || currentState == NpcState.Scared)
                return;

            ChangeState(NpcState.GoingToWork);
            Set_Cycle_Class().Start_GOTOWork();
        }

        void GoHome()
        {
            if (!enabled)
                return;

            if (currentState == NpcState.GoingHome || currentState == NpcState.Talking || currentState == NpcState.Scared)
                return;

            ChangeState(NpcState.GoingHome);
            Set_Cycle_Class().Start_GOTOHome();
        }

        LifeCycle Set_Cycle_Class()
        {
            LifeCycle lifeCycle = gameObject.AddComponent<LifeCycle>();
            lifeCycle.Set(this);

            return lifeCycle;
        }

        [Range(0, 10000)]
        public int converChoose = 0;

        void TriggerConversation(NpcAI npc)
        {
            if (currentState != NpcState.Scared && npc.currentState != NpcState.Scared)
            {
                if (UnityEngine.Random.Range(0, 10000) < converChoose) 
                {
                    //Each script has it's own ID. We can use these so one of the npc scripts is more prioritized
                    if (GetInstanceID() > npc.GetInstanceID())
                    {
                        if (GetComponent<RunConversation>() == null && npc.GetComponent<RunConversation>() == null)
                        {
                            RunConversation runConversation = gameObject.AddComponent<RunConversation>();
                            runConversation.Set(true, this, npc, null);
                            runConversation.StartConversation();

                            ChangeState(NpcState.Talking);
                        }
                    }
                }
            }
        }

        public void EndConversation()
        {
            Destroy(GetComponent<RunConversation>());
            GetComponentInChildren<TextMesh>().text = null;
        }

        private void OnEnable()
        {
            ChangeState(NpcState.Idle);
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