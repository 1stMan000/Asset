using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;
using PlayerController;
using Player_Actions;

public class DialogueManager : MonoBehaviour
{
    Camera dialogueCamera;

    GameObject player;
    FirstPersonAIO firstPersonAIO;
    PlayerActions playerActions;

    // Start is called before the first frame update
    void Start()
    {
        dialogueCamera = GetComponentInChildren<Camera>();

        player = GameObject.FindWithTag("Player");
        firstPersonAIO = player.GetComponent<FirstPersonAIO>();
        playerActions = player.GetComponent<PlayerActions>();

        rotateTo = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (rotateTo)
        {
            RotateTo(player);
        }
    }
    
    bool rotateTo = false;

    public void ActivateDialogue()
    {
        rotateTo = true;
    }

    void RotateTo(GameObject target)
    {
        Vector3 direction = new Vector3(target.transform.position.x - transform.position.x, 0f, target.transform.position.z - transform.position.z);
        Quaternion lookRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 2 / (Quaternion.Angle(transform.rotation, lookRotation) / 180));
    }
}
