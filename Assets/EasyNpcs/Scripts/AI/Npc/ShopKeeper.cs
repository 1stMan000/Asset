using UnityEngine;
using UnityEngine.Animations.Rigging;
using Rotation;

public class ShopKeeper : MonoBehaviour, IWork
{
    public RigBuilder rigBuilder;
    public TwoBoneIKConstraint right;
    public TwoBoneIKConstraint left;

    public GameObject rightPlacement;
    public GameObject leftPlacement;
    public GameObject shop;

    Rotate rotate;

    private void OnEnable()
    {
        rotate = gameObject.AddComponent<Rotate>();
        rotate.RotateTo(shop);

        right.data.target = rightPlacement.transform;
        left.data.target = leftPlacement.transform;
        rigBuilder.Build();
    }

    private void OnDisable()
    {
        right.data.target = null;
        left.data.target = null;
        rigBuilder.Build();

        Destroy(rotate);
    }

    public Behaviour GetScript()
    {
        return this;
    }
}
