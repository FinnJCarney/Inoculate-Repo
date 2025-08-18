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

    public bool CheckActionAbility(int internalNodePossibleActions, int externalNodePossibleActions)
    {
        if(myNodeGroup == null)
        {
            myNodeGroup = GetComponentInParent<NodeGroup>();
        }

        ActionInformation actionInfo = ActionManager.aM.actionInformation[actionType];
        if(actionInfo.internalAction)
        {
            if (internalNodePossibleActions < actionInfo.actionCost)
            {
                return false;
            }
        }
        else if (externalNodePossibleActions < actionInfo.actionCost)
        {
            return false;
        }

        bool returnValue = LevelManager.lM.CheckValidSpace(myNodeGroup.groupBelief + actionInfo.actionPosition);

        return returnValue;
    }


    NodeGroup myNodeGroup;
    [SerializeField] ActionType actionType;
}
