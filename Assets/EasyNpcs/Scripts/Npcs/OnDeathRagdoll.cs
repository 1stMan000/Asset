using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Ragdoll
{
    public class OnDeathRagdoll : MonoBehaviour, IDestructible
    {
        NavMeshAgent agent;
        Rigidbody[] rig;
        SkinnedMeshRenderer[] skin;

        void Start()
        {
            agent = GetComponent<NavMeshAgent>();
            skin = GetComponentsInChildren<SkinnedMeshRenderer>();

            foreach (SkinnedMeshRenderer skinned in skin)
            {
                skinned.updateWhenOffscreen = false; //has to be enabled when ragdoll is in. Otherwise the character sometimes does not render
            }

            rig = GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rigidbody in rig)
            {
                Rigidbody ownRigidbody = GetComponent<Rigidbody>();
                if (rigidbody != ownRigidbody)
                {
                    rigidbody.GetComponent<Collider>().enabled = true;
                    rigidbody.isKinematic = true;
                }
            }
            GetComponent<CapsuleCollider>().enabled = true;
        }

        public void OnAttack(GameObject attacker, Attack attack)
        {

        }

        public void OnDestruction(GameObject destoyer)
        {
            foreach (SkinnedMeshRenderer skinned in skin)
            {
                skinned.updateWhenOffscreen = true; //Stops character from disrendering
            }

            foreach (SkinnedMeshRenderer skinned in GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                skinned.updateWhenOffscreen = true; //Stops character from disrendering
            }

            GetComponentInChildren<Animator>().enabled = false;
            GetComponent<NavMeshAgent>().enabled = false;
            Destroy(GetComponent<CapsuleCollider>());
            Destroy(GetComponent<Rigidbody>());

            foreach (Rigidbody rigidbody in GetComponentsInChildren<Rigidbody>())
            {
                if (rigidbody != this.GetComponent<Rigidbody>())
                {
                    rigidbody.GetComponent<Collider>().enabled = true;
                    rigidbody.isKinematic = false;
                }
            }
        }
    }
}