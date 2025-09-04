using UnityEngine;

public static class ActionConverters
{
    public static NodeGroupTarget ProvideNodeGroupTarget(NodeGroup nodeGroup)
    {
        NodeGroupTarget newNodeGroupTarget;
        newNodeGroupTarget.nodeGroupTarget = nodeGroup;
        newNodeGroupTarget.timeOfTarget = TimeManager.tM.gameTimeElapsed;
        return newNodeGroupTarget;
    }
}
