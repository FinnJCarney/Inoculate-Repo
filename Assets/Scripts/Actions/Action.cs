using NUnit.Framework;
using System;
using UnityEngine;

[System.Serializable]
public abstract class Action : MonoBehaviour
{
    [SerializeField] public int cost;
    [SerializeField] public float timeToAct;
    [SerializeField] public float actionSpeed;
    [SerializeField] public ActionCostType costType;

    public virtual bool CheckNodeActionAvailability(NodeGroup applicableNodeGroup, int availableActions)
    {
        return false;
    }

    public virtual NodeGroup[] ProvidePossibleActingNodes(NodeGroup applicableNodeGroup, Faction faction)
    {
        return null;
    }

    public virtual float ProvideActionScore(NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction actingFaction)
    {
        return -1000f;
    }

    public virtual bool PerformActionOnNode(Node_UserInformation nodeToActOn)
    {
        return false;
    }

    public virtual bool PerformActionOnNodeGroup(NodeGroup nodeGroupToActOn)
    {
        return false;
    }

    public virtual void UndoNodeAction(Node_UserInformation nodeToActOn) { }

    public enum ActionCostType
    { 
         InternalAction,
         ExternalGroupAction,
         ExternalAction,
         UserAction,
         None
    }
}
