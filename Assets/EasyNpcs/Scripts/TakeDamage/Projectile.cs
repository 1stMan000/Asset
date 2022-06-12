using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    private GameObject Caster;
    private GameObject target;

    Rigidbody rigidBody;

    [Range(20.0f, 75.0f)] public float LaunchAngle;

    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody>();
    }

    public void Fire(GameObject caster, GameObject getTarget, Quaternion tartgetRotation, float speed, float range)
    {
        Caster = caster;
        target = getTarget;

        Fly();
    }

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.LookRotation(rigidBody.velocity);
    }

    private void Fly()
    {
        // think of it as top-down view of vectors: 
        //   we don't care about the y-component(height) of the initial and target position.
        Vector3 projectileXZPos = new Vector3(transform.position.x, 1.7f, transform.position.z);
        Vector3 targetXZPos = new Vector3(target.transform.position.x, 0.0f, target.transform.position.z);
        targetXZPos += -0.5f * (projectileXZPos - targetXZPos).normalized;

        // rotate the object to face the target
        transform.LookAt(targetXZPos);

        // shorthands for the formula
        float R = Vector3.Distance(projectileXZPos, targetXZPos);
        float G = Physics.gravity.y;
        float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
        float H = target.transform.position.y - transform.position.y;

        // calculate the local space components of the velocity 
        // required to land the projectile on the target object 
        float Vz = Mathf.Sqrt(G * R * R / (2.0f * (H - R * tanAlpha)));
        float Vy = tanAlpha * Vz;

        // create the velocity vector in local space and get it in global space
        Vector3 localVelocity = new Vector3(0f, Vy, Vz);
        Vector3 globalVelocity = transform.TransformDirection(localVelocity);

        // launch the object by setting its initial velocity and flipping its state
        rigidBody.velocity = globalVelocity;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject == Caster || other.gameObject.transform.root.gameObject == Caster)
        {
            Destroy(gameObject);
            return;
        }

        if (other.gameObject.layer != 14)
        {
            Destroy(gameObject);
            return;
        }

        AttackManager.AttackTarget(gameObject, target);
        Destroy(gameObject);
    }
}
