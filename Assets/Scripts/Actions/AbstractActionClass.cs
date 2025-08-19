using UnityEngine;

public abstract class AbstractActionClass : MonoBehaviour
{
    [SerializeField] public ActionType actionType;
    [SerializeField] public int cost; 
    [SerializeField] public int count;
    [SerializeField] public ActionCostType costType;

    public virtual bool CheckActionAvailability(NodeGroup applicableNodeGroup, int availableActions)
    {
        return false;
    }

    public virtual int ProvideActionScore(NodeGroup applicableNodeGroup, NodeGroup receivingNodeGroup)
    {
        return -1000;
    }

    public virtual bool PerformAction(Node_UserInformation nodeToActOn)
    {
        return false;
    }

    public enum ActionCostType
    { 
         InternalAction,
         InternalGroupAction,
         ExternlAction,
         None
    }
}
