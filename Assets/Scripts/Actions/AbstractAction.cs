using NUnit.Framework;
using System;
using UnityEngine;

[System.Serializable]
public abstract class AbstractAction : MonoBehaviour
{
    [SerializeField] public int cost;
    [SerializeField] public int count;
    [SerializeField] public float timeToAct;
    [SerializeField] public ActionCostType costType;

    public virtual bool CheckActionAvailability(NodeGroup applicableNodeGroup, int availableActions)
    {
        return false;
    }

    public virtual NodeGroup[] ProvidePossibleActingNodes(NodeGroup applicableNodeGroup, Faction faction)
    {
        return null;
    }

    public virtual int ProvideActionScore(NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction actingFaction)
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
         ExternalGroupAction,
         ExternlAction,
         None
    }
}
