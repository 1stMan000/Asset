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
        updateText();
     
    }

    private void Start()
    {
        var parent = GetComponentInParent<NpcData>();
        if (parent == null)
        {
            enabled = false;
        }  
        else
        {
            var data = parent.OnNpcDataInspectorChanged;
            if (data == null)
                data = new UnityEvent();

            data.AddListener(updateText);
        }
    }

    private void Update()
    {
        canvas.transform.LookAt(PlayerCam.transform.position);
    }

    private void updateText()
    {
        NpcData data = GetComponentInParent<NpcData>();
        if (data != null)
            text.text = data.NpcName + "\nThe " + data.job.ToString().ToLower();
    }
}
