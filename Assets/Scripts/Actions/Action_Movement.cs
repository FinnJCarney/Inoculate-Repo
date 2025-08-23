using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Action_Movement : AbstractAction
{
    public Vector2 movement = new Vector2(33f, 33f);

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
        if (cost > availableActions)
        {
            return false;
        }

        bool returnValue = LevelManager.lM.CheckValidSpace(applicableNodeGroup.groupBelief + movement);

        return returnValue;
    }

    public override NodeGroup[] ProvidePossibleActingNodes(NodeGroup applicableNodeGroup, Faction faction)
    {
        //Under assumption this only works as an external action
        List<NodeGroup> possibleActingNodeGroupList = new List<NodeGroup>();
        
        foreach(NodeGroup connectedNodeGroup in applicableNodeGroup.connectedNodes.Keys)
        {
            int availableActions = 0;

            if (connectedNodeGroup.connectedNodes[applicableNodeGroup].type == connectionType.influencedBy)
            {
                continue;
            }

            if(connectedNodeGroup.groupFaction == faction)
            {
                availableActions += (connectedNodeGroup.nodesInGroup.Count - connectedNodeGroup.performingActions);
            }

            if(availableActions >= cost)
            {
                possibleActingNodeGroupList.Add(connectedNodeGroup);
            }
        }
        

        NodeGroup[] possibleActingNodeGroupArray = possibleActingNodeGroupList.ToArray();

        return possibleActingNodeGroupArray;
    }

    public override int ProvideActionScore(NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction actingFaction)
    {
        float scoreToReturn = 0;
        
        if (LevelManager.lM.MinDistanceBetweenTwoVector2sOnMap(receivingNodeGroup.groupBelief + movement, actingNodeGroup.groupBelief) < LevelManager.lM.MinDistanceBetweenTwoVector2sOnMap(receivingNodeGroup.groupBelief, actingNodeGroup.groupBelief)) //If distance between acting and receiving nodes is closer due to action
        {
            scoreToReturn += 2f;
        }

        if (LevelManager.lM.MinDistanceBetweenTwoVector2sOnMap(receivingNodeGroup.groupBelief + movement, LevelManager.lM.levelFactions[actingFaction].mainPosition) < LevelManager.lM.MinDistanceBetweenTwoVector2sOnMap(receivingNodeGroup.groupBelief, LevelManager.lM.levelFactions[actingFaction].mainPosition)) //If distance between receiving node and faction center is closer due to action
        {
            scoreToReturn += 4f;
        }

        if (Vector2.Distance(receivingNodeGroup.groupBelief + movement, actingNodeGroup.groupBelief) < 12.1f)
        {
            scoreToReturn += 6f;
        }

        foreach (NodeGroup cNConnectedNode in receivingNodeGroup.connectedNodes.Keys)
        {
            if (Vector2.Distance(receivingNodeGroup.groupBelief + movement, cNConnectedNode.groupBelief) < 12.1f)
            {
                if (cNConnectedNode.groupFaction == Faction.Neutral)
                {
                    scoreToReturn += 3f;
                }
                else
                {
                    scoreToReturn -= 3f;
                }
            }
        }

        return Mathf.RoundToInt(scoreToReturn);
    }
}

