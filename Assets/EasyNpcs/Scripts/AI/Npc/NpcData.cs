using UnityEngine;
using UnityEngine.Events;

public class NpcData : MonoBehaviour
{
    [SerializeField]
    protected string npcName;
    public string NpcName { get { return npcName; } }

    public float VisionRange = 10;
    public LayerMask VisionLayers;

    [SerializeField]
    protected int age;
    public int Age { get { return age; } }

    public Job job;
    public Gender gender;

    [SerializeField]
    public Transform home;
    public Transform work;

    [SerializeField]
    private NpcStates _currentState;

    public NpcStates currentState
    {
        get
        {
            return _currentState;
        }
        protected set
        {
            _currentState = value;
        }
    }
    

    [HideInInspector]
    public UnityEvent OnNpcDataInspectorChanged;
    private void OnValidate()
    {
        if (OnNpcDataInspectorChanged == null)
            OnNpcDataInspectorChanged = new UnityEvent();
        OnNpcDataInspectorChanged.Invoke();
    }

   

}

