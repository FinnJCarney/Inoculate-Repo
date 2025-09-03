using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Action_TagAdder : Action
{
    public string tagToAdd = "ThatWhichResemblesTheGrave";
    public float tagTimer;

    public override bool PerformActionOnNodeGroup(NodeGroup nodeGroupToActOn)
    {
        if (!nodeGroupToActOn.tags.Contains(tagToAdd))
        {
            nodeGroupToActOn.AddTag(tagToAdd, tagTimer);
        }
        else
        {
            nodeGroupToActOn.tags[tagToAdd] = tagTimer;
        }
        
        return true;
    }

    public override bool CheckNodeActionAvailability(NodeGroup applicableNodeGroup, int availableActions)
    {
        if (cost > availableActions)
        {
            return false;
        }

        return true;
    }

    public override NodeGroup[] ProvidePossibleActingNodes(NodeGroup applicableNodeGroup, Faction faction)
    {
        NodeGroup[] nodeGroupToReturn = new NodeGroup[1];
        nodeGroupToReturn[0] = applicableNodeGroup;

        return nodeGroupToReturn;
    }

    public override int ProvideActionScore(NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction actingFaction)
    {
        float scoreToReturn = 0;
        
        //Check how much actingNodeGroup has been attacked in the last minute
        //Look at how vunerable it is to attack
        //Return that number + some extra value

        return Mathf.RoundToInt(scoreToReturn);
    }
}

