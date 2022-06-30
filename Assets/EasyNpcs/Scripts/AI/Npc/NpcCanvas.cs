using UnityEngine.Events;
using UnityEngine;


public class NpcCanvas : MonoBehaviour
{
    public TextMesh text;

    public Canvas canvas;
    public Camera PlayerCam;

    private void Awake()
    {
        if(text == null)
            text = GetComponentInChildren<TextMesh>();
        if(canvas == null)
            canvas = GetComponent<Canvas>();
        if (PlayerCam == null)
            PlayerCam = Camera.main;
     
    }

    private void Start()
    {
        var parent = GetComponentInParent<NpcData>();
        if (parent == null)
        {
            enabled = false;
        }
    }

    private void Update()
    {
        canvas.transform.LookAt(PlayerCam.transform.position);
    }
}
