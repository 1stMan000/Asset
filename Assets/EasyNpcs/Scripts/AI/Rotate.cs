using System.Collections;
using UnityEngine.AI;
using UnityEngine;

public class Rotate : MonoBehaviour
{
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
}
