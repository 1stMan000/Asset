using System.Collections;
using UnityEngine.AI;
using UnityEngine;

namespace Rotation
{
    public class Rotate : MonoBehaviour
    {
        public void RotateTo(GameObject target)
        {
            StartCoroutine(RotateToObject(target));
        }

        IEnumerator RotateToObject(GameObject target)
        {
            Quaternion lookRotation;
            do
            {
                Vector3 direction = (target.transform.position - transform.position).normalized;
                lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime / (Quaternion.Angle(transform.rotation, lookRotation) / GetComponent<NavMeshAgent>().angularSpeed));
                yield return new WaitForFixedUpdate();
            } while (true);
        }
    }
}