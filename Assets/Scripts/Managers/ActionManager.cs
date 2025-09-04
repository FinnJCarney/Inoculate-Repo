using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class ActionManager : MonoBehaviour
{
    public static ActionManager aM;

    private void Awake()
    {
        if(aM != null)
        {
            Destroy(this.gameObject);
        }
        else
        {
            aM = this;
        }
    }

    private void OnDestroy()
    {
        aM = null;
    }

    public void FlushAllActions()
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            for (int j = currentActions[i].actionLines.Length - (1); j >= 0; j--)
            {
                Destroy(currentActions[i].actionLines[j].gameObject);
            }
            Destroy(currentActions[i].actionRing);
            currentActions.Remove(currentActions[i]);
        }

        numOfPlayerActions = 0;
        RoomManager.rM.AdjustDonutHolder(numOfPlayerActions);
    }

    private void Update()
    {
        numOfPlayerActions = 0;

        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            if (currentActions[i].faction != currentActions[i].actingNodeGroup.groupFaction)
            {
                DestroyCurrentAction(currentActions[i]);
                continue;
            }

            var adjustedCurAction = currentActions[i];
            adjustedCurAction.timer -= Time.deltaTime;
            currentActions[i] = adjustedCurAction;

            //Old functionality
            //if ((currentActions[i].faction == LevelManager.lM.playerAllyFaction || currentActions[i].receivingNode.userInformation.faction == LevelManager.lM.playerAllyFaction || currentActions[i].actingNode.showMenu || currentActions[i].receivingNode.showMenu) && LayerManager.lM.activeLayer == currentActions[i].actionLayer)
            //{
            //
            //    currentActions[i].actionLine.SyncLine(1f - (currentActions[i].timer / currentActions[i].timerMax), currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
            //}
            //else
            //{
            //    currentActions[i].actionLine.SyncLine(0, currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
            //}

     
            if (LayerManager.lM.activeLayer == currentActions[i].actionLayer)
            {
                for (int j = currentActions[i].actionLines.Length - (1); j >= 0; j--)
                {
                    currentActions[i].actionLines[j].SyncLine(1f - (currentActions[i].timer / currentActions[i].timerMax), currentActions[i].actingNodeGroup.transform.position, currentActions[i].receivingNodeGroup.transform.position, false);
                }
            }
            else
            {
                for (int j = currentActions[i].actionLines.Length - (1); j >= 0; j--)
                {
                    currentActions[i].actionLines[j].SyncLine(1f - (currentActions[i].timer / currentActions[i].timerMax), currentActions[i].actingNodeGroup.transform.position, currentActions[i].receivingNodeGroup.transform.position, true);
                }
            }
            

            SetActionRing(currentActions[i].actionRing, (currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax, currentActions[i].faction, currentActions[i].receivingNodeGroup.transform.position);
            currentActions[i].receivingNodeGroup.SetActionAudio((currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax);

            if (currentActions[i].timer < 0f)
            {
                currentActions[i].actingNodeGroup.performingActions -= currentActions[i].action.cost;
                currentActions[i].receivingNodeGroup.receivingActions--;
                currentActions[i].receivingNodeGroup.ActionResult(currentActions[i].action, currentActions[i].actingNodeGroup.groupFaction, currentActions[i].actingNodeGroup, currentActions[i].actionLayer, currentActions[i].bleat, currentActions[i].pastAction);

                for (int j = currentActions[i].actionLines.Length - (1); j >= 0; j--)
                {
                    Destroy(currentActions[i].actionLines[j].gameObject);
                }

                Destroy(currentActions[i].actionRing);
                currentActions.Remove(currentActions[i]);
                continue;
            }

            if(currentActions[i].faction == LevelManager.lM.playerAllyFaction)
            {
                numOfPlayerActions++;
            }
        }

        TimeManager.tM.SetTimeScale(numOfPlayerActions > 0 ? 1f : 0f);
        RoomManager.rM.AdjustDonutHolder(numOfPlayerActions);
    }

    private void SetActionRing(GameObject actionRing, float amountThrough, Faction faction, Vector3 nodePosition)
    {
        actionRing.transform.position = nodePosition;

        actionRing.transform.localScale = Vector3.Lerp(outerActionRingScale, new Vector3(0.1f, 0.1f, 0.1f), amountThrough);
        
        Color idealColor = Color.clear;

        idealColor = LevelManager.lM.levelFactions[faction].color;

        actionRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.clear, idealColor, amountThrough);
    }

    public void PerfromGroupButtonAction(Action aT, NodeGroup receivingNodeGroup)
    {
        NodeGroup actingNodeGroup = null;
        NodeGroup[] possibleActingNodeGroups = aT.ProvidePossibleActingNodes(receivingNodeGroup, LevelManager.lM.playerAllyFaction);

        foreach (NodeGroup nodeGroup in possibleActingNodeGroups)
        {
            nodeGroup.prio = aT.ProvideActionScore(nodeGroup, receivingNodeGroup, LevelManager.lM.playerAllyFaction);
        }

        if (aT.costType == Action.ActionCostType.InternalAction)
        {
            MakeNewGroupAction(aT, LayerManager.lM.activeLayer, receivingNodeGroup, receivingNodeGroup);
        }
        else
        {
            foreach (NodeGroup nodeGroup in possibleActingNodeGroups)
            {
                if (nodeGroup.prio < 0)
                {
                    continue;
                }

                if (actingNodeGroup == null)
                {
                    actingNodeGroup = nodeGroup;
                }
                else if (actingNodeGroup.prio < nodeGroup.prio)
                {
                    actingNodeGroup = nodeGroup;
                }
            }

            if (actingNodeGroup == null)
            {
                Debug.Log("Unable to find acting node group");
                return;
            }

            MakeNewGroupAction(aT, LayerManager.lM.activeLayer, actingNodeGroup, receivingNodeGroup);

        }
    }

    public void PerformAIAction(int NumOfActions, Faction faction)
    {
        for (int i = 0; i < NumOfActions; i++)
        {
            Node[] actingNodes = new Node[NumOfActions];

            List<PossibleAction> possibleActions = new List<PossibleAction>();
            List<Node> possibleActingNodes = null;

            possibleActingNodes = NodeManager.nM.nodeFactions[faction];

            List<NodeGroup> possibleReceivingNodeGroups = new List<NodeGroup>();

            foreach(NodeGroup nodeGroup in LevelManager.lM.nodeGroups.Values)
            {
                if(possibleReceivingNodeGroups.Contains(nodeGroup) || nodeGroup.nodesInGroup.Count == 0)
                {
                    continue;
                }

                if(nodeGroup.groupFaction == faction)
                {
                    possibleReceivingNodeGroups.Add(nodeGroup);

                    foreach (NodeGroup connectedNodeGroup in nodeGroup.connectedNodes.Keys)
                    {
                        if (!possibleReceivingNodeGroups.Contains(connectedNodeGroup))
                        {
                            possibleReceivingNodeGroups.Add(connectedNodeGroup);
                        }
                    }

                    continue;
                }
            }

            if (possibleReceivingNodeGroups.Count == 0)
            {
                Debug.Log("Failed to find receiving nodes");
            }

            //Decision Making

            //Vector2 averagePos = Vector2.zero;
            //
            //foreach (Node node in possibleActingNodes)
            //{
            //    averagePos += node.userInformation.beliefs;
            //}
            //
            //averagePos /= possibleActingNodes.Count;
            //float ideologicalDistance = 0;
            //
            //
            //ideologicalDistance = Vector2.Distance(averagePos, LevelManager.lM.levelFactions[faction].mainPosition);

            #region Old "Inoculate" decision code
            ////Acting on "Inoculate"
            //if (ideologicalDistance > 100) //Ignoring this for now
            //{
            //    Debug.Log("Performing Inoculate action for " + faction);
            //    foreach (Node node in possibleActingNodes)
            //    {
            //        if (node.performingAction || node.userInformation.faction == LevelManager.lM.playerAllyFaction || node.isBanned)
            //        {
            //            continue;
            //        }

            //        foreach (Node connectedNode in node.userInformation.connectedNodes.Keys)
            //        {
            //            if (connectedNode.userInformation.faction != node.userInformation.faction)
            //            {
            //                continue;
            //            }

            //            float proximityValue = (0.1f / Vector2.Distance(node.userInformation.beliefs, connectedNode.userInformation.beliefs)) + 0.1f;
            //            float strategicValue = 0;

            //            if (node.userInformation.beliefs.y > connectedNode.userInformation.beliefs.y && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.up))
            //            {
            //                PossibleAction newPossibleAction = new PossibleAction();
            //                newPossibleAction.actionType = ActionType.Up;
            //                newPossibleAction.actingNode = node;
            //                newPossibleAction.receivingNode = connectedNode;
            //                newPossibleAction.score += proximityValue;

            //                if (proximityValue < 1.5f)
            //                {
            //                    newPossibleAction.score += 2.5f;
            //                }

            //                foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
            //                {
            //                    if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
            //                    {
            //                        strategicValue += 2f;

            //                        //if(Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.up, cNConnectedNode.userInformation.beliefs) < 1.1f)
            //                        //{
            //                        //    strategicValue -= 4f;
            //                        //}
            //                    }
            //                }

            //                //Temp version of "Value add for move also pulling node towards factions ideological center"
            //                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.up, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
            //                {
            //                    strategicValue += 2f;
            //                }

            //                newPossibleAction.score += strategicValue;
            //                newPossibleAction.score += Random.Range(0f, 1f);
            //                possibleActions.Add(newPossibleAction);
            //            }

            //            if (node.userInformation.beliefs.y < connectedNode.userInformation.beliefs.y && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.down))
            //            {
            //                PossibleAction newPossibleAction = new PossibleAction();
            //                newPossibleAction.actionType = ActionType.Down;
            //                newPossibleAction.actingNode = node;
            //                newPossibleAction.receivingNode = connectedNode;
            //                newPossibleAction.score += proximityValue;

            //                if (proximityValue < 1.5f)
            //                {
            //                    newPossibleAction.score += 2.5f;
            //                }

            //                foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
            //                {
            //                    if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
            //                    {
            //                        strategicValue += 2f;

            //                        //if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.down, cNConnectedNode.userInformation.beliefs) < 1.1f)
            //                        //{
            //                        //    strategicValue -= 4f;
            //                        //}
            //                    }
            //                }

            //                //Temp version of "Value add for move also pulling node towards factions ideological center"
            //                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.down, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
            //                {
            //                    strategicValue += 2f;
            //                }

            //                newPossibleAction.score += strategicValue;
            //                newPossibleAction.score += Random.Range(0f, 1f);
            //                possibleActions.Add(newPossibleAction);
            //            }

            //            if (node.userInformation.beliefs.x > connectedNode.userInformation.beliefs.x && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.right))
            //            {
            //                PossibleAction newPossibleAction = new PossibleAction();
            //                newPossibleAction.actionType = ActionType.Right;
            //                newPossibleAction.actingNode = node;
            //                newPossibleAction.receivingNode = connectedNode;
            //                newPossibleAction.score += proximityValue;

            //                if (proximityValue < 1.5f)
            //                {
            //                    newPossibleAction.score += 2.5f;
            //                }

            //                foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
            //                {
            //                    if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
            //                    {
            //                        strategicValue += 2f;

            //                        //if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.right, cNConnectedNode.userInformation.beliefs) < 1.1f)
            //                        //{
            //                        //    strategicValue -= 4f;
            //                        //}
            //                    }
            //                }

            //                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.right, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
            //                {
            //                    strategicValue += 2f;
            //                }

            //                newPossibleAction.score += strategicValue;
            //                newPossibleAction.score += Random.Range(0f, 1f);
            //                possibleActions.Add(newPossibleAction);
            //            }

            //            if (node.userInformation.beliefs.x < connectedNode.userInformation.beliefs.x && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.left))
            //            {
            //                PossibleAction newPossibleAction = new PossibleAction();
            //                newPossibleAction.actionType = ActionType.Left;
            //                newPossibleAction.actingNode = node;
            //                newPossibleAction.receivingNode = connectedNode;
            //                newPossibleAction.score += proximityValue;

            //                if (proximityValue < 1.5f)
            //                {
            //                    newPossibleAction.score += 2.5f;
            //                }

            //                foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
            //                {
            //                    if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
            //                    {
            //                        strategicValue += 2f;

            //                        //if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.left, cNConnectedNode.userInformation.beliefs) < 1.1f)
            //                        //{
            //                        //    strategicValue -= 4f;
            //                        //}
            //                    }
            //                }

            //                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.left, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
            //                {
            //                    strategicValue += 2f;
            //                }

            //                newPossibleAction.score += strategicValue;
            //                newPossibleAction.score += Random.Range(0f, 1f);
            //                possibleActions.Add(newPossibleAction);
            //            }

            //        }
            //    }
            //} 
            #endregion

            //Acting on "Expand"
            Debug.Log("Performing Expand action for " + faction);

            foreach(NodeGroup nodeGroup in possibleReceivingNodeGroups)
            {
                foreach(Action possibleAction in LevelManager.lM.levelFactions[faction].availableActions)
                {
                    int applicableActions = 0;
                    Vector3 availableActions = nodeGroup.CheckAvailableActions(faction);

                    if (possibleAction.costType == Action.ActionCostType.InternalAction)
                    {
                        applicableActions = ((int)availableActions.x);

                        if (possibleAction.CheckNodeActionAvailability(nodeGroup, applicableActions))
                        {
                            possibleActions.Add(MakePossibleGroupAction(possibleAction, nodeGroup, nodeGroup, faction));
                        }
                    }
                    else
                    {
                        if (possibleAction.costType == Action.ActionCostType.ExternalAction)
                        {
                            applicableActions = ((int)availableActions.y);
                        }
                        
                        if (possibleAction.costType == Action.ActionCostType.ExternalGroupAction)
                        {
                            applicableActions = ((int)availableActions.z);
                        }

                        if (possibleAction.CheckNodeActionAvailability(nodeGroup, applicableActions))
                        {
                            foreach (NodeGroup possibleActingNodeGroup in possibleAction.ProvidePossibleActingNodes(nodeGroup, faction))
                            {
                                possibleActions.Add(MakePossibleGroupAction(possibleAction, possibleActingNodeGroup, nodeGroup, faction));
                            }
                        }
                    }
                }
            }

            float maxPossibleActionValue = 0;

            foreach (PossibleAction possibleAction in possibleActions)
            {
                //Debug.Log("Evaluating " + possibleAction.actionType + " action for " + faction + " faction, with acting node " + possibleAction.actingNode + ", receiving node " + possibleAction.receivingNode + ", and score of " + possibleAction.score);
                if (possibleAction.score > maxPossibleActionValue)
                {
                    maxPossibleActionValue = possibleAction.score;
                }
            }

            foreach (PossibleAction possibleAction in possibleActions)
            {
                if (possibleAction.score == maxPossibleActionValue)
                {
                    //Debug.Log("Performing " + possibleAction.actionType + " action for " + faction + " faction, with acting node " + possibleAction.actingNode);
                    MakeNewGroupAction(possibleAction.action, possibleAction.actionLayer, possibleAction.actingNodeGroup, possibleAction.receivingNodeGroup);
                    break;
                }
            }
        }
    }

    private void MakeNewAction(Action action, Node actingNode, Node receivingNode, Faction faction)
    {
        //CurrentAction newCurrentAction;
        //newCurrentAction.actionType = newActionType;
        //newCurrentAction.actingNode = actingNode;
        //actingNode.performingAction = true;
        //newCurrentAction.receivingNode = receivingNode;
        //newCurrentAction.actionLayer = actionLayer;
        //newCurrentAction.timer = actionLayer == connectionLayer.online ? actionInformation[newActionType].actionLength.x : actionInformation[newActionType].actionLength.y;
        //newCurrentAction.timerMax = newCurrentAction.timer;
        //newCurrentAction.faction = actingNode.userInformation.faction;
        //newCurrentAction.actionLine = Instantiate<GameObject>(LevelManager.lM.levelFactions[newCurrentAction.faction].actionLine, this.transform).GetComponent<ActionLine>();
        //newCurrentAction.actionLine.transform.position = Vector3.Lerp(actingNode.transform.position, receivingNode.transform.position, 0.5f);
        //newCurrentAction.actionLine.SyncLine(0, actingNode.transform.position, receivingNode.transform.position, actionLayer != LayerManager.lM.activeLayer);
        //newCurrentAction.actionRing = Instantiate<GameObject>(actionRing, this.transform);
        //newCurrentAction.actionRing.transform.position = receivingNode.transform.position;
        //newCurrentAction.bleat = TweetManager.tM.PublishTweet(LevelManager.lM.tweetsForActions[newActionType], actingNode.userInformation, receivingNode.userInformation, newCurrentAction.faction);
        //SetActionRing(newCurrentAction.actionRing, 0f, newCurrentAction.faction, receivingNode.transform.position);
        //currentActions.Add(newCurrentAction);
        
        //Debug.Log("Performing action type " + newActionType + " for faction " + newCurrentAction.faction + ", from " + actingNode + " to " + receivingNode);
    }

    private CurrentAction MakeNewGroupAction(Action aT, connectionLayer actionLayer, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup)
    {
        CurrentAction newCurrentAction;
        newCurrentAction.action = aT;
        newCurrentAction.actingNodeGroup = actingNodeGroup;
        actingNodeGroup.performingActions += aT.cost;
        newCurrentAction.receivingNodeGroup = receivingNodeGroup;
        receivingNodeGroup.receivingActions++;
        newCurrentAction.actionLayer = LayerManager.lM.activeLayer;
        newCurrentAction.timer = aT.timeToAct;
        newCurrentAction.timerMax = newCurrentAction.timer;
        newCurrentAction.faction = actingNodeGroup.groupFaction;
        newCurrentAction.actionLines = new ActionLine[aT.cost];
        for (int i = 0; i < aT.cost; i++)
        {
            newCurrentAction.actionLines[i] = Instantiate<GameObject>(LevelManager.lM.levelFactions[newCurrentAction.faction].actionLine, this.transform).GetComponent<ActionLine>();
            newCurrentAction.actionLines[i].transform.position = Vector3.Lerp(actingNodeGroup.transform.position, receivingNodeGroup.transform.position, 0.5f);
            newCurrentAction.actionLines[i].SyncLine(0, actingNodeGroup.transform.position, receivingNodeGroup.transform.position, actionLayer != LayerManager.lM.activeLayer);
        }
        newCurrentAction.actionRing = Instantiate<GameObject>(actionRing, this.transform);
        newCurrentAction.actionRing.transform.position = receivingNodeGroup.transform.position;
        newCurrentAction.bleat = TweetManager.tM.PublishTweet(LevelManager.lM.tweetsForActions[aT], actingNodeGroup.nodesInGroup[Random.Range(0, actingNodeGroup.nodesInGroup.Count - 1)], receivingNodeGroup.nodesInGroup[Random.Range(0, receivingNodeGroup.nodesInGroup.Count - 1)], newCurrentAction.faction);
        SetActionRing(newCurrentAction.actionRing, 0f, newCurrentAction.faction, receivingNodeGroup.transform.position);
        newCurrentAction.pastAction = RegisterPastAction(aT, actingNodeGroup, receivingNodeGroup, null);
        currentActions.Add(newCurrentAction);
        return currentActions[currentActions.IndexOf(newCurrentAction)];
        Debug.Log("Performing action type " + aT + " for faction " + newCurrentAction.faction + ", from " + actingNodeGroup + " to " + receivingNodeGroup);
    }

    private PossibleAction MakePossibleGroupAction(Action action, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction faction)
    {
        PossibleAction newPossibleAction = new PossibleAction();
        newPossibleAction.action = action;
        newPossibleAction.actingNodeGroup = actingNodeGroup;
        newPossibleAction.receivingNodeGroup = receivingNodeGroup;
        newPossibleAction.actionLayer = connectionLayer.online;
        newPossibleAction.faction = faction;

        newPossibleAction.score = action.ProvideActionScore(actingNodeGroup, receivingNodeGroup, faction);
        return newPossibleAction;


        //if (actionLayer == connectionLayer.offline)
        //{
        //    newPossibleAction.score -= 1f;

        //    if (LevelManager.lM.CheckValidSpace(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f)))
        //    {
        //        if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), actingNode.userInformation.beliefs) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs)) //If distance between acting and receiving nodes is closer due to action
        //        {
        //            newPossibleAction.score += 5f;
        //        }

        //        if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), factionPos) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, factionPos)) //If distance between receiving node and faction center is closer due to action
        //        {
        //            newPossibleAction.score += 3f;
        //        }

        //        if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), actingNode.userInformation.beliefs) < 12.1f)
        //        {
        //            newPossibleAction.score += 5f;
        //        }

        //        foreach (Node cNConnectedNode in receivingNode.userInformation.connectedNodes.Keys)
        //        {
        //            if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), cNConnectedNode.userInformation.beliefs) < 12.1f)
        //            {
        //                if (cNConnectedNode.userInformation.faction == Faction.Neutral)
        //                {
        //                    newPossibleAction.score += 2f;
        //                }
        //                else
        //                {
        //                    newPossibleAction.score -= 2f;
        //                }
        //            }

        //            if (cNConnectedNode.userInformation.faction == receivingNode.userInformation.faction)
        //            {
        //                newPossibleAction.score += 2f;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        newPossibleAction.score -= 10f;
        //    }
        //}
        //else

    }

    public PastAction RegisterPastAction(Action aA, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Node_UserInformation receivingNode)
    {
        PastAction newPastAction;
        newPastAction.action = aA;
        newPastAction.actingNodeGroups = new List<NodeGroupTarget>();
        newPastAction.receivingNodeGroups = new List<NodeGroupTarget>();
        newPastAction.receivingNode = receivingNode;
        newPastAction.timeStarted = TimeManager.tM.gameTimeElapsed;
        newPastAction.timeCompleted = -999f;

        NodeGroupTarget actingNodeTarget = new NodeGroupTarget();
        actingNodeTarget.nodeGroupTarget = actingNodeGroup;
        actingNodeTarget.timeOfTarget = TimeManager.tM.gameTimeElapsed;
        newPastAction.actingNodeGroups.Add(actingNodeTarget);

        NodeGroupTarget receivingNodeTarget = new NodeGroupTarget();
        receivingNodeTarget.nodeGroupTarget = receivingNodeGroup;
        receivingNodeTarget.timeOfTarget = TimeManager.tM.gameTimeElapsed;
        newPastAction.receivingNodeGroups.Add(receivingNodeTarget);
       

        pastActions.Add(newPastAction);
        return pastActions[pastActions.IndexOf(newPastAction)];
    }

    public void PivotPastAction(PastAction pA, NodeGroup newNodeGroup, bool acting)
    {
        PastAction adjustedPastAction = pastActions[pastActions.IndexOf(pA)];

        if (acting)
        {
            adjustedPastAction.actingNodeGroups.Add(ActionConverters.ProvideNodeGroupTarget(newNodeGroup));
        }
        else
        {
            adjustedPastAction.receivingNodeGroups.Add(ActionConverters.ProvideNodeGroupTarget(newNodeGroup));
        }

        pastActions[pastActions.IndexOf(pA)] = adjustedPastAction;
    }

    public void CompletePastAction(PastAction pA, Node_UserInformation receivingNode, bool completed)
    {
        PastAction adjustedPastAction = pastActions[pastActions.IndexOf(pA)];
        adjustedPastAction.timeCompleted = completed ? TimeManager.tM.gameTimeElapsed : -999f;
        adjustedPastAction.receivingNode = completed ? receivingNode : null;
        pastActions[pastActions.IndexOf(pA)] = adjustedPastAction;
    }

    public void RecreatePastAction(PastAction pA, float timerVal)
    {
        var recreatedAction = MakeNewGroupAction(pA.action, connectionLayer.online, pA.actingNodeGroups[pA.actingNodeGroups.Count - 1].nodeGroupTarget, pA.receivingNodeGroups[pA.receivingNodeGroups.Count - 1].nodeGroupTarget);
        int actionIndex = currentActions.IndexOf(recreatedAction);
        pastActions.Remove(recreatedAction.pastAction);
        recreatedAction.timer = timerVal;
        recreatedAction.pastAction = pA;
        currentActions[actionIndex] = recreatedAction;
    }

    //private PossibleAction MakePossibleAction(ActionType actionType, connectionLayer actionLayer, Node actingNode, Node receivingNode)
    //{
    //    PossibleAction newPossibleAction = new PossibleAction();
    //    newPossibleAction.actionType = actionType;
    //    newPossibleAction.actingNode = actingNode;
    //    newPossibleAction.receivingNode = receivingNode;
    //    newPossibleAction.actionLayer = actionLayer;

    //    Vector2 factionPos = LevelManager.lM.levelFactions[actingNode.userInformation.faction].mainPosition;
    //    factionPos.x = factionPos.x == 3 ? receivingNode.userInformation.beliefs.x : factionPos.x;
    //    factionPos.y = factionPos.y == 3 ? receivingNode.userInformation.beliefs.y : factionPos.y;

    //    newPossibleAction.score += actingNode.userInformation.faction == receivingNode.userInformation.faction ? 0f : 2.5f; //If node is in another faction, more valuable to grab

    //    if (actionLayer == connectionLayer.offline)
    //    {
    //        newPossibleAction.score -= 1f;

    //        if (LevelManager.lM.CheckValidSpace(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f)))
    //        {
    //            if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), actingNode.userInformation.beliefs) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs)) //If distance between acting and receiving nodes is closer due to action
    //            {
    //                newPossibleAction.score += 5f;
    //            }

    //            if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), factionPos) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, factionPos)) //If distance between receiving node and faction center is closer due to action
    //            {
    //                newPossibleAction.score += 3f;
    //            }

    //            if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), actingNode.userInformation.beliefs) < 12.1f)
    //            {
    //                newPossibleAction.score += 5f;
    //            }

    //            foreach (Node cNConnectedNode in receivingNode.userInformation.connectedNodes.Keys)
    //            {
    //                if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), cNConnectedNode.userInformation.beliefs) < 12.1f)
    //                {
    //                    if (cNConnectedNode.userInformation.faction == Faction.Neutral)
    //                    {
    //                        newPossibleAction.score += 2f;
    //                    }
    //                    else
    //                    {
    //                        newPossibleAction.score -= 2f;
    //                    }
    //                }

    //                if (cNConnectedNode.userInformation.faction == receivingNode.userInformation.faction)
    //                {
    //                    newPossibleAction.score += 2f;
    //                }
    //            }
    //        }
    //        else
    //        {
    //            newPossibleAction.score -= 10f;
    //        }
    //    }
    //    else
    //    {
    //        if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + actionInformation[actionType].actionPosition, actingNode.userInformation.beliefs) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs)) //If distance between acting and receiving nodes is closer due to action
    //        {
    //            newPossibleAction.score += 4f;
    //        }

    //        if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + actionInformation[actionType].actionPosition, factionPos) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, factionPos)) //If distance between receiving node and faction center is closer due to action
    //        {
    //            newPossibleAction.score += 2f;
    //        }

    //        if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition), actingNode.userInformation.beliefs) < 12.1f)
    //        {
    //            newPossibleAction.score += 4f;
    //        }

    //        foreach (Node cNConnectedNode in receivingNode.userInformation.connectedNodes.Keys)
    //        {
    //            if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition), cNConnectedNode.userInformation.beliefs) < 12.1f)
    //            {
    //                if (cNConnectedNode.userInformation.faction == Faction.Neutral)
    //                {
    //                    newPossibleAction.score += 2f;
    //                }
    //                else
    //                {
    //                    newPossibleAction.score -= 2f;
    //                }
    //            }

    //            if (cNConnectedNode.userInformation.faction == receivingNode.userInformation.faction)
    //            {
    //                newPossibleAction.score += 2f;


    //                if (cNConnectedNode.userInformation.faction == receivingNode.userInformation.faction)
    //                {
    //                    newPossibleAction.score += 2f;
    //                }
    //            }
    //        }
    //    }

    //    newPossibleAction.score += Random.Range(0f, 1f);
    //    return newPossibleAction;
    //}

    public void DestroyCurrentAction(CurrentAction currentAction)
    {
        currentAction.actingNodeGroup.performingActions -= currentAction.action.cost;
        currentAction.receivingNodeGroup.receivingActions--;
        Destroy(currentAction.bleat.gameObject);
        for (int j = currentAction.actionLines.Length - (1); j >= 0; j--)
        {
            Destroy(currentAction.actionLines[j].gameObject);
        }
        Destroy(currentAction.actionRing);
        currentActions.Remove(currentAction);
    }



    public void PivotAction_Performing(NodeGroup ogActingNodeGroup, NodeGroup newActingNodeGroup)
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            var adjustedCurAction = currentActions[i];

            if(adjustedCurAction.actingNodeGroup == ogActingNodeGroup)
            {
                adjustedCurAction.actingNodeGroup = newActingNodeGroup;
                ogActingNodeGroup.performingActions--;
                newActingNodeGroup.performingActions++;
                PivotPastAction(pastActions[pastActions.IndexOf(currentActions[i].pastAction)], newActingNodeGroup, true);
            }

            currentActions[i] = adjustedCurAction;
        }
    }

    public void PivotAction_Receiving(NodeGroup ogReceivingNodeGroup, NodeGroup newReceivingNodeGroup)
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            if (currentActions[i].timer < 0f)
            {
                continue;
            }

            var adjustedCurAction = currentActions[i];

            if (adjustedCurAction.receivingNodeGroup == ogReceivingNodeGroup)
            {
                adjustedCurAction.receivingNodeGroup = newReceivingNodeGroup;
                ogReceivingNodeGroup.receivingActions--;
                newReceivingNodeGroup.receivingActions++;
                PivotPastAction(pastActions[pastActions.IndexOf(currentActions[i].pastAction)], newReceivingNodeGroup, false);
            }

            currentActions[i] = adjustedCurAction;
        }
    }

    private int numOfPlayerActions = 0;

    [SerializeField] public List<CurrentAction> currentActions = new List<CurrentAction>();
    [SerializeField] public List<PastAction> pastActions = new List<PastAction>();
    [SerializeField] public List<Node> previousNodes = new List<Node>();

    [SerializeField] GameObject actionRing;
    [SerializeField] Vector3 outerActionRingScale;

    [SerializeField] int actingNodeMemoryLength;
}

[System.Serializable]
public struct CurrentAction
{
    public Action action;
    public NodeGroup actingNodeGroup;
    public NodeGroup receivingNodeGroup;
    public float timer;
    public float timerMax;
    public connectionLayer actionLayer;
    public ActionLine[] actionLines;
    public GameObject actionRing;
    public Faction faction;
    public Bleat bleat;
    public PastAction pastAction; 
}

[System.Serializable]
public struct PossibleAction
{
    public Action action;
    public NodeGroup actingNodeGroup;
    public NodeGroup receivingNodeGroup;
    public Faction faction;
    public connectionLayer actionLayer;
    public float score;
}

[System.Serializable]
public struct PastAction
{
    public Action action;
    public List<NodeGroupTarget> actingNodeGroups;
    public Node_UserInformation receivingNode;
    public List<NodeGroupTarget> receivingNodeGroups;
    public float timeStarted;
    public float timeCompleted;
}

[System.Serializable]
public struct NodeGroupTarget
{
    public NodeGroup nodeGroupTarget;
    public float timeOfTarget;
}

