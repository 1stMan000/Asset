using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DayandNight;
using Text_Loader;

namespace Npc_AI
{
    public class NPC : NpcData, IDestructible
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
        private float runTimeLeft;

        //NPC-NPC interaction
        GameObject conversationBuddy;
        private TextMesh Text;

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
            Idle_Walk_Run_BlendAnim();
            WatchEnvironment();
        }

        void Idle_Walk_Run_BlendAnim()
        {
            if (runTimeLeft > 0)
                runTimeLeft -= Time.deltaTime;
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        void WatchEnvironment()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, VisionLayers);

            foreach (Collider col in cols)
            {
                if (col.gameObject.GetComponent<NPC>()) 
                {
                    NPC npc = col.gameObject.GetComponent<NPC>();
                    if (npc.broadcastAttacked)
                    {
                        Attacker = npc.Attacker;
                        ChangeState(NpcStates.Scared);
                    }
                    else
                    {
                        TriggerConversation(npc);
                    }
                }
            }
        }

        private void ChangeState(NpcStates newState)
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
                    StartCoroutine(Run(Attacker));
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
                        SetMoveTarget(work);
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

        private void SetMoveTarget(Transform target)
        {
            agent.ResetPath();
            agent.SetDestination(target.position);
        }

        void GoToWork()
        {
            if (!enabled)
                return;

            ChangeState(NpcStates.GoingToWork);
            StartCoroutine(GoToWorkCoroutine());
        }

        //Set agent destination to work position, and change state to "Working" as it is reached
        IEnumerator GoToWorkCoroutine()
        {
            agent.speed = movementSpeed;
            SetMoveTarget(work);
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

            SetMoveTarget(home);

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

        //Conversation Functions
        public void StartConversation(bool isFirst, GameObject talker, Tuple<List<string>, List<string>> forcedConverstion = null)
        {
            Ready_For_Conversation(new object[] { isFirst, talker, forcedConverstion });
        }

        void Ready_For_Conversation(object[] isFirst_talker_forcedConversation)
        {
            bool IsFirstInConversation = (bool)isFirst_talker_forcedConversation[0];
            conversationBuddy = (GameObject)isFirst_talker_forcedConversation[1];

            Tuple<List<string>, List<string>> conversationToSpeak = null;
            if (IsFirstInConversation)
            {
                Initialize_Or_Find_Conversation(isFirst_talker_forcedConversation, conversationToSpeak);
            }
        }

        void Initialize_Or_Find_Conversation(object[] isFirst_talker_forcedConversation, Tuple<List<string>, List<string>> conversationToSpeak)
        {
            conversationToSpeak = Check_ForcedConversation_And_If_Not_Choose(isFirst_talker_forcedConversation);
            if (conversationToSpeak != null)
            {
                var componentOfBuddy = conversationBuddy.GetComponent<NPC>();
                Change_Buddy_toTalkingState(componentOfBuddy);

                object[] componentOfBuddy_conversationToSpeak = { componentOfBuddy, conversationToSpeak };
                StartCoroutine(nameof(Speak_First_Lines), componentOfBuddy_conversationToSpeak);
            }
        }

        Tuple<List<string>, List<string>> Check_ForcedConversation_And_If_Not_Choose(object[] isFirst_talker_forcedConversation)
        {
            if (!ForcedConversationExists(isFirst_talker_forcedConversation))
            {
                return ChooseConversation(conversationBuddy.GetComponent<NPC>());
            }
            else
            {
                return (Tuple<List<string>, List<string>>)isFirst_talker_forcedConversation[2];
            }
        }

        bool ForcedConversationExists(object[] isFirst_talker_forcedConversation)
        {
            if (isFirst_talker_forcedConversation[2] != null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        Tuple<List<string>, List<string>> ChooseConversation(NPC componentOfBuddy)
        {
            Job[] jobs = { job, componentOfBuddy.job };
            Gender[] genders = { Gender, componentOfBuddy.Gender };

            return TextLoader.GetDialgoue(genders, jobs);
        }

        void Change_Buddy_toTalkingState(NPC componentOfBuddy)
        {
            ChangeState(NpcStates.Talking);
            agent.SetDestination(componentOfBuddy.transform.position);

            componentOfBuddy.conversationBuddy = gameObject;
            componentOfBuddy.ChangeState(NpcStates.Talking);
        }

        IEnumerator Speak_First_Lines(object[] componentOfBuddy_conversationToSpeak)
        {
            Tuple<List<string>, List<string>> conversationToSpeak = (Tuple<List<string>, List<string>>)componentOfBuddy_conversationToSpeak[1];
            ExecuteLine(conversationToSpeak.Item1); 
            yield return new WaitForSeconds(4);

            NPC componentOfBuddy = (NPC)componentOfBuddy_conversationToSpeak[0];
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

            GetComponentInChildren<TextMesh>().text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
        }

        [Range(0, 10000)]
        public int converChoose = 0;

        void TriggerConversation(NPC npc)
        {
            if (Check_Conversation_Requirments(this) && Check_Conversation_Requirments(npc))
            {
                if (UnityEngine.Random.Range(0, 10000) < converChoose) 
                {
                    //Each script has it's own ID. We can use these so one of the npc scripts is more prioritized
                    if (GetInstanceID() > npc.GetInstanceID())
                    {
                        StartConversation(true, npc.gameObject);
                        npc.StartConversation(false, gameObject);
                    }
                }
            }
        }

        bool Check_Conversation_Requirments(NPC npcScript)
        {
            bool state = Check_State_For_Conversation(npcScript);
            if (!npcScript.enabled || !state)
            {
                return false;
            }

            return true;
        }

        bool Check_State_For_Conversation(NPC npcScript)
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

        public IEnumerator Run(GameObject attacker)
        {
            agent.speed = scaredRunningSpeed;
            runTimeLeft = runningTime;
            agent.ResetPath();

            while (runTimeLeft > 0)
            {
                Vector3 goal;
                bool isPathValid;
                NavMeshPath path = new NavMeshPath();

                //Get the angle between "attacker" and NPC
                Vector3 distanceIn3D = attacker.transform.position - transform.position;
                float magnitude = new Vector2(distanceIn3D.x, distanceIn3D.z).magnitude;
                Vector2 distance = new Vector2(distanceIn3D.x / magnitude, distanceIn3D.z / magnitude);
                double angleX = Math.Acos(distance.x);
                double angleY = Math.Asin(distance.y);

                //Loop has iteration limit to avoid errors
                int index = 0;
                const int limit = 13;

                //Loop tries to find further point from "attacker" in boundaries of a circle of "runningDistance" radius
                do
                {
                    //Rotate point in the circle by (PI / 6 * index)
                    angleX += index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                    angleY -= index * Math.Pow(-1.0f, index) * Math.PI / 6.0f;
                    distance = new Vector2((float)Math.Cos(angleX), (float)Math.Sin(angleY));
                    goal = new Vector3(transform.position.x - distance.x * runningDistance, transform.position.y, transform.position.z - distance.y * runningDistance);

                    //Check if NPC can reach this point
                    bool samplePosition = NavMesh.SamplePosition(goal, out NavMeshHit hit, runningDistance / 5, agent.areaMask);
                    //Calculate path if the point is reachable
                    if (samplePosition)
                    {
                        agent.CalculatePath(hit.position, path);
                        yield return new WaitUntil(() => path.status != NavMeshPathStatus.PathInvalid);
                        agent.path = path;
                    }

                    isPathValid = (samplePosition &&
                                   path.status != NavMeshPathStatus.PathPartial &&
                                   agent.remainingDistance <= runningDistance);

                    //Stop loop if it is impossible to find way after "limit" iterations
                    if (++index > limit)
                    {
                        agent.destination = this.transform.position;
                        break;
                    }
                } while (!isPathValid);

                yield return new WaitUntil(() => Vector3.Distance(agent.destination, transform.position) <= runningDistance / 1.2);
            }

            ChangeState(NpcStates.Idle);
        }

        void StopRunning()
        {
            StopCoroutine(nameof(Run));
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