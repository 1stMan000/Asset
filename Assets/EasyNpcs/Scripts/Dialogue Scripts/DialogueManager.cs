using UnityEngine;
using Player_Actions;

public class DialogueManager : MonoBehaviour
{
    GameObject player;
    PlayerActions playerActions;

    DialgueAndButtons Buttons_And_Dialogues;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindWithTag("Player");
        playerActions = player.GetComponent<PlayerActions>();
        Buttons_And_Dialogues = playerActions.dialogueWindow.GetComponent<DialgueAndButtons>();

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

    public void RotateToPlayer()
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
