using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DayandNight;
using Text_Loader;

namespace Npc_AI
{
    public class NpcAI : NpcData, IDestructible
    {
        Animator anim;

        public NavMeshAgent agent { get; private set; }
        public float movementSpeed;

        [HideInInspector]
        public GameObject Attacker;
        [HideInInspector]
        public bool broadcastAttacked;
        public float scaredRunningSpeed;
        public float runningDistance;
        public float runningTime;

        //NPC-NPC interaction
        GameObject conversationBuddy;
        public TextMesh Text;

        DayAndNightControl dayAndNightControl;
        public Behaviour workScript;

        void Start()
        {
            anim = GetComponentInChildren<Animator>();
            agent = GetComponent<NavMeshAgent>();
            Text = GetComponentInChildren<TextMesh>();

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
                    gameObject.AddComponent(typeof(RunAway_Script));
                    StartCoroutine(GetComponent<RunAway_Script>().Run(Attacker));
                    break;

                case NpcStates.GoingHome:
                    GoHome();
                    break;

                case NpcStates.GoingToWork:
                    break;

                case NpcStates.Idle:
                    float time = dayAndNightControl.currentTime;
                    if (time > .3f && time < .7f)
                    {
                        GoToWork();
                    }
                    else
                    {
                        GoHome();
                    }
                    break;

                case NpcStates.Talking:
                    StartCoroutine(nameof(RotateTo), conversationBuddy);
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
                    StopRunning();
                    break;
                case NpcStates.GoingHome:
                    StopGoingHome();
                    break;
                case NpcStates.GoingToWork:
                    StopGoingToWork();
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

        void GoToWork()
        {
            if (!enabled)
                return;

            ChangeState(NpcStates.GoingToWork);
            StartCoroutine(GoToWorkCoroutine());
        }

        IEnumerator GoToWorkCoroutine()
        {
            agent.speed = movementSpeed;
            agent.SetDestination(work.position);
            yield return new WaitUntil(() => Vector3.Distance(transform.position, work.position) <= agent.stoppingDistance);

            if (enabled)
            {
                if (currentState != NpcStates.Working)
                    ChangeState(NpcStates.Working);
            }
        }

        void StopGoingToWork()
        {
            if (agent.isActiveAndEnabled)
                agent.ResetPath();
            StopCoroutine(GoToWorkCoroutine());
        }

        void GoHome()
        {
            if (!enabled)
                return;

            //States that are more prioritized than 'GoingHome' state
            if (currentState == NpcStates.GoingHome || currentState == NpcStates.Talking || currentState == NpcStates.Scared)
                return;
            StartCoroutine(GoHomeCoroutine());
        }

        IEnumerator GoHomeCoroutine()
        {
            agent.speed = movementSpeed;
            ChangeState(NpcStates.GoingHome);

            agent.SetDestination(home.position);

            yield return new WaitUntil(() => agent.remainingDistance <= 0.1f && !agent.pathPending);
            if (currentState == NpcStates.GoingHome)
                ChangeState(NpcStates.Idle);
        }

        void StopGoingHome()
        {
            if (agent.isActiveAndEnabled)
                agent.ResetPath();
            StopCoroutine(GoHomeCoroutine());
        }

        public void ChangeTo_Talking(GameObject gameObject)
        {
            conversationBuddy = gameObject;
            ChangeState(NpcStates.Talking);
        }

        IEnumerator Speak_First_Lines(object[] componentOfBuddy_conversationToSpeak)
        {
            Tuple<List<string>, List<string>> conversationToSpeak = (Tuple<List<string>, List<string>>)componentOfBuddy_conversationToSpeak[1];
            ExecuteLine(conversationToSpeak.Item1); 
            yield return new WaitForSeconds(4);

            NpcAI componentOfBuddy = (NpcAI)componentOfBuddy_conversationToSpeak[0];
            componentOfBuddy.ExecuteLine(conversationToSpeak.Item2);
        }

        public void ExecuteLine(List<string> text, int waitSeconds = 0)
        {
            StartCoroutine(nameof(Talk), text);
        }

        IEnumerator Talk(List<string> text)
        {
            for (int i = 0; i < text.Count; i++)
            {
                if (!text[i].StartsWith(" ")) 
                {
                    Text.text = text[i];
                    yield return new WaitForSeconds(4);
                    if (i != text.Count - 1)
                    {
                        Text.text = null;
                        yield return new WaitForSeconds(4);
                    }
                }
            }

            ChangeState(NpcStates.Idle);
        }

        public void EndConversation()
        {
            agent.isStopped = false;
            StopCoroutine(nameof(RotateTo));
            StopCoroutine(nameof(Speak_First_Lines));
            StopCoroutine(nameof(Talk));

            conversationBuddy = null;

            GetComponentInChildren<TextMesh>().text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().job.ToString().ToLower();
        }

        [Range(0, 10000)]
        public int converChoose = 0;

        void TriggerConversation(NpcAI npc)
        {
            if (Check_Conversation_Requirments(this) && Check_Conversation_Requirments(npc))
            {
                if (UnityEngine.Random.Range(0, 10000) < converChoose) 
                {
                    //Each script has it's own ID. We can use these so one of the npc scripts is more prioritized
                    if (GetInstanceID() > npc.GetInstanceID())
                    {
                        if (GetComponent<RunConversation>() == null)
                        {
                            RunConversation runConversation = gameObject.AddComponent<RunConversation>();
                            runConversation.first = true;
                            runConversation.me = this;
                            runConversation.partner = npc;
                            runConversation.conversation = null;
                            runConversation.StartConversation();
                        }
                    }
                }
            }
        }

        bool Check_Conversation_Requirments(NpcAI npcScript)
        {
            bool state = Check_State_For_Conversation(npcScript);
            if (!npcScript.enabled || !state)
            {
                return false;
            }

            return true;
        }

        bool Check_State_For_Conversation(NpcAI npcScript)
        {
            if (npcScript.currentState == NpcStates.Scared || npcScript.currentState == NpcStates.Talking)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void OnAttack(GameObject attacker, Attack attack)
        {
            if (this.enabled == false)
                return;

            Attacker = attacker;
            ChangeState(NpcStates.Scared);
            StartCoroutine(nameof(BroadcastAttacked_Courountine));
        }

        IEnumerator BroadcastAttacked_Courountine()
        {
            broadcastAttacked = true;
            yield return new WaitForSeconds(1f);
            broadcastAttacked = false;
        }

        void StopRunning()
        {
            if (GetComponent<RunAway_Script>() != null)
                Destroy(GetComponent<RunAway_Script>());
        }

        //Rotate to the target
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

        void OnDestroy()
        {
            DayAndNightControl control = FindObjectOfType<DayAndNightControl>();
            control.OnMorningHandler -= GoToWork;
            control.OnEveningHandler -= GoHome;
        }

        void OnDisable()
        {
            TurnOffBehaviour(currentState);
            anim.SetFloat("Speed", 0);
        }

        private void OnEnable()
        {
            ChangeState(NpcStates.Idle);
        }

        public void OnDestruction(GameObject destroyer)
        {
            enabled = false;
        }

        public void EnableCombat()
        {
            this.enabled = false;
        }
    }
}