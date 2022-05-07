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
        public bool isAttacked;
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
            //Decrease run time after hit
            if (runTimeLeft > 0)
                runTimeLeft -= Time.deltaTime;
            anim.SetFloat("Speed", agent.velocity.magnitude);

            WatchEnvironment();
        }

        void WatchEnvironment()
        {
            Collider[] cols = Physics.OverlapSphere(transform.position, VisionRange, VisionLayers);

            foreach (Collider col in cols)
            {
                if (col.gameObject.GetComponent<NPC>()) 
                {
                    NPC npc = col.gameObject.GetComponent<NPC>();
                    if (npc.isAttacked)
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
                    EndConversation(false);
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

        void StopGoingHome()
        {
            if (agent.isActiveAndEnabled)
                agent.ResetPath();
            StopCoroutine(GoHomeCoroutine());
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

        private void OnDestroy()
        {
            DayAndNightControl control = FindObjectOfType<DayAndNightControl>();
            control.OnMorningHandler -= GoToWork;
            control.OnEveningHandler -= GoHome;
        }

        public void StartConversation(bool isFirst, GameObject talker, Tuple<List<string>, List<string>> forcedConverstion = null)
        {
            Conversation(new object[] { isFirst, talker, forcedConverstion });
        }

        public void StartConversation(bool isFirst, GameObject talker)
        {
            Conversation(new object[] { isFirst, talker });
        }

        void Conversation(object[] isFirst_talker_forcedConversation)
        {
            bool IsFirstInConversation = (bool)isFirst_talker_forcedConversation[0];
            conversationBuddy = (GameObject)isFirst_talker_forcedConversation[1];

            Tuple<List<string>, List<string>> conversationToSpeak = null;
            if (ForcedConversationExists(isFirst_talker_forcedConversation))
            {
                conversationToSpeak = (Tuple<List<string>, List<string>>)isFirst_talker_forcedConversation[2];
            }

            if (IsFirstInConversation)
            {
                StartCoroutine(FirstStartOfConversation(isFirst_talker_forcedConversation, conversationToSpeak));
            }
        }

        bool ForcedConversationExists(object[] isFirst_talker_forcedConversation)
        {
            if (isFirst_talker_forcedConversation.Length > 2)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        IEnumerator FirstStartOfConversation(object[] isFirst_talker_forcedConversation, Tuple<List<string>, List<string>> conversationToSpeak)
        {
            var componentOfBuddy = conversationBuddy.GetComponent<NPC>();

            // If text is not given, get text from the Text Loader script 
            if (!ForcedConversationExists(isFirst_talker_forcedConversation))
            {
                conversationToSpeak = ChooseConversation(componentOfBuddy);
            }
            if (conversationToSpeak == null)
            {
                yield break;
            }

            ChangeState(NpcStates.Talking);
            agent.SetDestination(componentOfBuddy.transform.position);

            componentOfBuddy.conversationBuddy = gameObject;
            componentOfBuddy.ChangeState(NpcStates.Talking);

            StartSpeaking(conversationToSpeak.Item1); //1st speaks then 2nd npc speaks after 4secs
            componentOfBuddy.Text.text = null;
            yield return new WaitForSeconds(4);
            componentOfBuddy.StartSpeaking(conversationToSpeak.Item2);
        }

        Tuple<List<string>, List<string>> ChooseConversation(NPC componentOfBuddy)
        {
            Job[] jobs = { job, componentOfBuddy.job };
            Gender[] genders = { Gender, componentOfBuddy.Gender };

            return TextLoader.GetDialgoue(genders, jobs);
        }

        public void StartSpeaking(List<string> text, int waitSeconds = 0)
        {
            StartCoroutine(nameof(Talk), new object[] { text, waitSeconds });
        }

        IEnumerator Talk(object[] parameters)
        {
            List<string> text = (List<string>)parameters[0];
            int waitSeconds = 0;

            if (parameters.Length > 1)
                waitSeconds = (int)parameters[1];

            for (int i = 0; i < text.Count; i++)
            {
                if (!text[i].StartsWith(" ")) //Displays sentece for 4 secs
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

            yield return new WaitForSeconds(waitSeconds);
            ChangeState(NpcStates.Idle);
        }

        //Stops conversation and removes all behaviours from it
        public void EndConversation(bool changeState = true)
        {
            agent.isStopped = false;
            StopCoroutine(nameof(Conversation));
            StopCoroutine(nameof(RotateTo));
            StopCoroutine(nameof(Talk));

            NPC npc = conversationBuddy.GetComponent<NPC>();
            if (changeState == true)
                npc.ChangeState(NpcStates.Idle);
            conversationBuddy = null;

            GetComponentInChildren<TextMesh>().text = GetComponentInChildren<NpcData>().NpcName + "\nThe " + GetComponentInChildren<NpcData>().Job.ToString().ToLower();
        }

        //Start NPC-NPC interaction with nearby NPCs with 
        void TriggerConversation(NPC npc)
        {
            if (!enabled)
                return;

            if (conv == false)
                return;

            // States that are more prioritized
            if (currentState == NpcStates.Scared || currentState == NpcStates.Talking)
                return;

            NPC NPCscript = npc;

            if (NPCscript.enabled == false)
                return;

            //Checks if the talker's state does not have a higher priority
            if (NPCscript.currentState == NpcStates.Scared || NPCscript.currentState == NpcStates.Talking)
                return;

            if (UnityEngine.Random.Range(0, 1000) < converChoose) //At a chance starts a conversation
            {
                //Each script has it's own ID. We can use these so one of the npc scripts is more prioritized
                //Can stop bug for when both scripts decide to have a conversation at the same time

                if (GetInstanceID() > NPCscript.GetInstanceID())
                {
                    StartConversation(true, NPCscript.gameObject);
                    NPCscript.StartConversation(false, gameObject);
                }
            }
        }

        [Range(0, 1000)]
        public int converChoose = 0;
        bool conv = true;

        IEnumerator WaitTillNextConv()
        {
            conv = false;
            yield return new WaitForSeconds(60);
            conv = true;
        }

        //Called when NPC is attacked
        public void OnAttack(GameObject attacker, Attack attack)
        {
            if (this.enabled == false)
                return;

            Attacker = attacker;
            ChangeState(NpcStates.Scared);
            StartCoroutine(nameof(Attacked));
        }

        //Method WatchEnvironment() uses "IsAttacked" boolean to check if NPC is attacked
        IEnumerator Attacked()
        {
            isAttacked = true;
            yield return new WaitForSeconds(1f);
            isAttacked = false;
        }

        //Run from "attacker" in opposite direction
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
                        ChangeState(NpcStates.Idle);
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