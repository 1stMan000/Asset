using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public Sentence currentSentence;
    GameObject player;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        RotateTo(player);
    }
    
    void RotateTo(GameObject target)
    {
        Vector3 direction = new Vector3(target.transform.position.x - transform.position.x, 0f, target.transform.position.z - transform.position.z);
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2 / (Quaternion.Angle(transform.rotation, lookRotation) / 180));
    }
}
