using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class Action_Movement : Action
{
    public Vector2 movement = new Vector2(33f, 33f);

    public override bool PerformActionOnNode(Node_UserInformation nodeToActOn)
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

    public override bool CheckNodeActionAvailability(NodeGroup applicableNodeGroup, int availableActions)
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
        List<NodeGroup> possibleActingNodeGroupList = new List<NodeGroup>();
        
        foreach(NodeGroup connectedNodeGroup in applicableNodeGroup.connectedNodes.Keys)
        {
            int availableActions = 0;

            if(!connectedNodeGroup.connectedNodes.ContainsKey(applicableNodeGroup))
            {
                continue;
            }

            if (connectedNodeGroup.connectedNodes[applicableNodeGroup].type == connectionType.influencedBy)
            {
                continue;
            }

            if (connectedNodeGroup.groupFaction != faction)
            {
                continue;
            }

            availableActions += (connectedNodeGroup.nodesInGroup.Count - connectedNodeGroup.performingActions);

            if(availableActions >= cost)
            {
                possibleActingNodeGroupList.Add(connectedNodeGroup);
            }
        }
        

        NodeGroup[] possibleActingNodeGroupArray = possibleActingNodeGroupList.ToArray();

        return possibleActingNodeGroupArray;
    }

    public override float ProvideActionScore(NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction actingFaction)
    {
        float scoreToReturn = 0;

        Vector2 otherActionMovement = Vector2.zero;

        foreach(CurrentAction factionActions in ActionManager.aM.currentActions)
        {
            if(factionActions.faction == actingFaction)
            {
                if(factionActions.action.GetComponent<Action_Movement>())
                {
                    otherActionMovement += factionActions.action.GetComponent<Action_Movement>().movement;
                }
            }
        }
        
        if (Vector2.Distance(receivingNodeGroup.groupBelief + otherActionMovement + movement, actingNodeGroup.groupBelief) < Vector2.Distance(receivingNodeGroup.groupBelief + otherActionMovement, actingNodeGroup.groupBelief)) //If distance between acting and receiving nodes is closer due to action
        {
            scoreToReturn += 2f;
        }

        if (Vector2.Distance(receivingNodeGroup.groupBelief + otherActionMovement + movement, LevelManager.lM.levelFactions[actingFaction].mainPosition) < Vector2.Distance(receivingNodeGroup.groupBelief + otherActionMovement, LevelManager.lM.levelFactions[actingFaction].mainPosition)) //If distance between receiving node and faction center is closer due to action
        {
            scoreToReturn += 6f;
        }
        else
        {
            scoreToReturn -= 6f;
        }

        if(receivingNodeGroup.groupFaction == actingFaction)
        {
            scoreToReturn -= 4f;
        }

        List<NodeGroup> oldNeighbouringNodes = LevelManager.lM.ProvideNeighbouringNodeGroups(receivingNodeGroup.groupBelief + otherActionMovement);
        bool oldPosConnectsWithFaction = false;

        foreach(NodeGroup connectedNG in oldNeighbouringNodes)
        {
            if (connectedNG.groupFaction == actingFaction)
            {
                oldPosConnectsWithFaction = true;
            }

            if (Vector2.Distance(receivingNodeGroup.groupBelief + otherActionMovement + movement, connectedNG.groupBelief) < 12.1f)
            {
                if (connectedNG.groupFaction != Faction.Neutral)
                {
                    scoreToReturn += 3f;
                }
            }
        }

        List<NodeGroup> newNeighbouringNodes = LevelManager.lM.ProvideNeighbouringNodeGroups(receivingNodeGroup.groupBelief + otherActionMovement + movement);
        bool newPosConnectsWithFaction = false;

        foreach (NodeGroup connectedNG in newNeighbouringNodes)
        {
            if(connectedNG.groupFaction == actingFaction)
            {
                newPosConnectsWithFaction = true;
            }

            if (Vector2.Distance(receivingNodeGroup.groupBelief + otherActionMovement + movement, connectedNG.groupBelief) < 12.1f)
            {
                if (connectedNG.groupFaction == Faction.Neutral)
                {
                    scoreToReturn += 3f;
                }
            }
        }

        if(newPosConnectsWithFaction && !oldPosConnectsWithFaction)
        {
            scoreToReturn += 6f;
        }

        else if(!newPosConnectsWithFaction && oldPosConnectsWithFaction)
        {
            scoreToReturn -= 12f;    
        }

        Debug.Log("For Faction:" + actingFaction + ", Movement:" + movement + " on " + receivingNodeGroup + ", score = " + scoreToReturn);

        float speedBonus = Mathf.Min(Vector2.Distance(actingNodeGroup.groupBelief, receivingNodeGroup.groupBelief), 64f) / 64f;
        //scoreToReturn += (2f - (speedBonus * 2f));

        return Mathf.RoundToInt(scoreToReturn);
    }

    public override void UndoNodeAction(Node_UserInformation nodeToActOn)
    {
        Vector2 originalBeliefs = nodeToActOn.beliefs;

        if (!LevelManager.lM.CheckValidSpace(originalBeliefs - movement))
        {
            return;
        }

        nodeToActOn.beliefs -= movement;

        LevelManager.lM.nodeGroups[originalBeliefs].RemoveNodeFromGroup(nodeToActOn);
        LevelManager.lM.nodeGroups[nodeToActOn.beliefs].AddNodeToGroup(nodeToActOn);
    }
}

