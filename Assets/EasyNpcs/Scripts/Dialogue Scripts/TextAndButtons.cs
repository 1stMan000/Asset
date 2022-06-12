using UnityEngine;

public class TextAndButtons : MonoBehaviour
{
    [HideInInspector]
    public GameObject text;

    [HideInInspector]
    public GameObject[] buttons;

    private void Awake()
    {
        text = transform.GetChild(0).gameObject;
        
        buttons = new GameObject[4];
        buttons[0] = transform.GetChild(1).gameObject;
        buttons[1] = transform.GetChild(2).gameObject;
        buttons[2] = transform.GetChild(3).gameObject;
        buttons[3] = transform.GetChild(4).gameObject;
    }
}
