using UnityEngine;

public class NodeGroupButton : AbstractButtonClass
{
    private void Start()
    {
        myNodeGroup = GetComponentInParent<NodeGroup>();
    }

    public override void PerformAction()
    {
        myNodeGroup.PerformButtonAction(actionType);
    }


    NodeGroup myNodeGroup;
    [SerializeField] ActionType actionType;
}
