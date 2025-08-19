using UnityEngine;

public class Movement : AbstractActionClass
{
    Vector2 movement;

    public override bool PerformAction(Node_UserInformation nodeToActOn)
    {
        Vector2 originalBeliefs = nodeToActOn.beliefs;

        if (!LevelManager.lM.CheckValidSpace(originalBeliefs + movement))
        {
            return false;
        }

        nodeToActOn.beliefs += movement;

        LevelManager.lM.nodeGroups[originalBeliefs].RemoveNodeFromGroup(nodeToActOn);
        LevelManager.lM.nodeGroups[nodeToActOn.beliefs].AddNodeToGroup(nodeToActOn);

        return true;
    }

    public override bool CheckActionAvailability(NodeGroup applicableNodeGroup, int availableActions)
    {
        ActionInformation actionInfo = ActionManager.aM.actionInformation[actionType];

        if (cost > availableActions)
        {
            return false;
        }

        bool returnValue = LevelManager.lM.CheckValidSpace(applicableNodeGroup.groupBelief + movement);

        return returnValue;
    }

    public override int ProvideActionScore(NodeGroup applicableNodeGroup, NodeGroup receivingNodeGroup)
    {
        return Mathf.Abs(Mathf.RoundToInt(receivingNodeGroup.groupBelief.x - receivingNodeGroup.groupBelief.x) * 10);
    }
}

