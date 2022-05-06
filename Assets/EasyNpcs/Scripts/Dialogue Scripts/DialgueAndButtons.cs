using UnityEngine;
using UnityEngine.UI;

public class DialgueAndButtons : MonoBehaviour
{
    [HideInInspector]
    public Text npcName;
    [HideInInspector]
    public Text text;

    [HideInInspector]
    public Button[] button;

    private void Start()
    {
        npcName = transform.GetChild(0).GetComponent<Text>();
        text = transform.GetChild(1).GetComponent<Text>();

        button = new Button[4];
        button[0] = transform.GetChild(2).GetComponent<Button>();
        button[1] = transform.GetChild(3).GetComponent<Button>();
        button[2] = transform.GetChild(4).GetComponent<Button>();
        button[3] = transform.GetChild(5).GetComponent<Button>();
    }
}
