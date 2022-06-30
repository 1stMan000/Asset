using UnityEngine;
using UnityEngine.AI;

public class NpcData : MonoBehaviour
{
    [HideInInspector]
    public NavMeshAgent agent;
    protected Animator anim;

    public float VisionRange = 10;
    public LayerMask VisionLayers;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();
    }

    protected virtual void Update()
    {
        anim.SetFloat("Speed", agent.velocity.magnitude);
    }
}

