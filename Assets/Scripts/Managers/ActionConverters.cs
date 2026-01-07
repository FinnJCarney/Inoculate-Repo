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

    public static Vector3 Vector2ToVector3(Vector2 inputVector)
    {
        return new Vector3(inputVector.x, 0f, inputVector.y);
    }

}
