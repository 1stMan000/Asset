using UnityEngine;
using UnityEngine.Animations.Rigging;

public class ShopKeeper : MonoBehaviour
{
    public RigBuilder rigBuilder;
    public TwoBoneIKConstraint right;
    public TwoBoneIKConstraint left;

    public GameObject rightPlacement;
    public GameObject leftPlacement;

    void Start()
    {
        right.data.target = rightPlacement.transform;
        left.data.target = leftPlacement.transform;
        rigBuilder.Build();
    }

    void Update()
    {
        
    }
}
