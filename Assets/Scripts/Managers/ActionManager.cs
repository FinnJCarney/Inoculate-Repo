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
            Destroy(currentActions[i].actionLine.gameObject);
            Destroy(currentActions[i].actionRing);
            currentActions.Remove(currentActions[i]);
        }

        numOfPlayerActions = 0;
        RoomManager.rM.AdjustDonutHolder(numOfPlayerActions);
    }

    private void Start()
    {
        tm = TimeManager.tM;
    }

    private void Update()
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            if (currentActions[i].faction != currentActions[i].actingNodeGroup.groupFaction)
            {
                if (currentActions[i].faction == LevelManager.lM.playerAllyFaction)
                {
                    TimeManager.tM.AddTimeScale(-0.75f);
                    numOfPlayerActions--;
                    RoomManager.rM.AdjustDonutHolder(numOfPlayerActions);
                }
                currentActions[i].actingNodeGroup.performingActions--; ;
                currentActions[i].receivingNodeGroup.receivingActions--;
                currentActions[i].bleat.CancelBleat();
                Destroy(currentActions[i].actionLine.gameObject);
                Destroy(currentActions[i].actionRing);
                currentActions.Remove(currentActions[i]);
                continue;
            }

            var adjustedCurAction = currentActions[i];
            adjustedCurAction.timer -= tm.adjustedDeltaTime;
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

                currentActions[i].actionLine.SyncLine(1f - (currentActions[i].timer / currentActions[i].timerMax), currentActions[i].actingNodeGroup.transform.position, currentActions[i].receivingNodeGroup.transform.position, false);
            }
            else
            {
                currentActions[i].actionLine.SyncLine(1f - (currentActions[i].timer / currentActions[i].timerMax), currentActions[i].actingNodeGroup.transform.position, currentActions[i].receivingNodeGroup.transform.position, true);
            }
            

            SetActionRing(currentActions[i].actionRing, (currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax, currentActions[i].faction, currentActions[i].receivingNodeGroup.transform.position);
            currentActions[i].receivingNodeGroup.SetActionAudio((currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax);

            if (currentActions[i].timer < 0f)
            {
                if (currentActions[i].faction == LevelManager.lM.playerAllyFaction)
                {
                    TimeManager.tM.AddTimeScale(-0.75f);
                    numOfPlayerActions--;
                    RoomManager.rM.AdjustDonutHolder(numOfPlayerActions);
                }
                currentActions[i].receivingNodeGroup.ActionResult(currentActions[i].action, currentActions[i].actingNodeGroup.groupFaction, currentActions[i].actingNodeGroup, currentActions[i].actionLayer, currentActions[i].bleat);
                currentActions[i].actingNodeGroup.performingActions--;
                currentActions[i].receivingNodeGroup.receivingActions--;
                Destroy(currentActions[i].actionLine.gameObject);
                Destroy(currentActions[i].actionRing);
                currentActions.Remove(currentActions[i]);
                continue;
            }
        }
    }

    private void SetActionRing(GameObject actionRing, float amountThrough, Faction faction, Vector3 nodePosition)
    {
        actionRing.transform.position = nodePosition;

        actionRing.transform.localScale = Vector3.Lerp(outerActionRingScale, new Vector3(0.1f, 0.1f, 0.1f), amountThrough);
        
        Color idealColor = Color.clear;

        idealColor = LevelManager.lM.levelFactions[faction].color;

        actionRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.clear, idealColor, amountThrough);
    }

    public void PerfromGroupButtonAction(AbstractAction aT, NodeGroup receivingNodeGroup)
    {
        NodeGroup actingNodeGroup = null;

        if (aT.GetComponent<Action_Movement>() != null)
        {
            foreach (NodeGroup nodeGroup in receivingNodeGroup.connectedNodes.Keys)
            {
                nodeGroup.prio = 100;

                if (nodeGroup.performingActions == nodeGroup.nodesInGroup.Count || nodeGroup.groupFaction != LevelManager.lM.playerAllyFaction)
                {
                    nodeGroup.prio -= 1000;
                }

                if (receivingNodeGroup.connectedNodes[nodeGroup].type == connectionType.influenceOn)
                {
                    nodeGroup.prio -= 1000;
                }

                foreach (NodeGroup connectedNodeGroup in nodeGroup.connectedNodes.Keys)
                {
                    if (nodeGroup.connectedNodes[connectedNodeGroup].type == connectionType.influencedBy)
                    {
                        continue;
                    }

                    if (connectedNodeGroup.groupFaction == nodeGroup.groupFaction)
                    {
                        nodeGroup.prio += 5;
                    }
                    else
                    {
                        nodeGroup.prio -= 5;
                    }

                }

                nodeGroup.prio += aT.ProvideActionScore(nodeGroup, receivingNodeGroup, LevelManager.lM.playerAllyFaction);
            }
        }

        foreach (NodeGroup nodeGroup in receivingNodeGroup.connectedNodes.Keys)
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

        TimeManager.tM.AddTimeScale(0.75f);

        numOfPlayerActions++;
        RoomManager.rM.AdjustDonutHolder(numOfPlayerActions);
    }

    public void PerformButtonAction(UserButton buttonInfo)
    {
        if(!buttonInfo.buttonEnabled)
        {
            return;
        }

        Node receivingNode = buttonInfo.relatedNode;

        Node actingNode = null;


        if (buttonInfo.type != ActionType.Connect)
        {
            foreach (Node connectedNode in receivingNode.userInformation.connectedNodes.Keys)
            {
                connectedNode.nodePrio = 100;

                if (connectedNode.performingAction || connectedNode.isBanned || connectedNode.userInformation.faction != LevelManager.lM.playerAllyFaction)
                {
                    connectedNode.nodePrio -= 1000;
                }

                if (receivingNode.userInformation.connectedNodes[connectedNode].type == connectionType.influenceOn)
                {
                    connectedNode.nodePrio -= 1000;
                }

                if (receivingNode.userInformation.connectedNodes[connectedNode].layer != connectionLayer.onlineOffline && receivingNode.userInformation.connectedNodes[connectedNode].layer != LayerManager.lM.activeLayer)
                {
                    connectedNode.nodePrio -= 1000;
                }

                foreach (Node cNCN in connectedNode.userInformation.connectedNodes.Keys)
                {
                    if (connectedNode.userInformation.connectedNodes[cNCN].type == connectionType.influencedBy)
                    {
                        continue;
                    }

                    if (cNCN.userInformation.faction == connectedNode.userInformation.faction)
                    {
                        connectedNode.nodePrio += 5;
                    }
                    else
                    {
                        connectedNode.nodePrio -= 5;
                    }

                }

                if (buttonInfo.type == ActionType.Left)
                {
                    connectedNode.nodePrio += Mathf.RoundToInt(receivingNode.userInformation.beliefs.x - connectedNode.userInformation.beliefs.x) * 10;
                }

                if (buttonInfo.type == ActionType.Right)
                {
                    connectedNode.nodePrio += Mathf.RoundToInt(connectedNode.userInformation.beliefs.x - receivingNode.userInformation.beliefs.x) * 10;
                }

                if (buttonInfo.type == ActionType.Up)
                {
                    connectedNode.nodePrio += Mathf.RoundToInt(connectedNode.userInformation.beliefs.y - receivingNode.userInformation.beliefs.y) * 10;
                }

                if (buttonInfo.type == ActionType.Down)
                {
                    connectedNode.nodePrio += Mathf.RoundToInt(receivingNode.userInformation.beliefs.y - connectedNode.userInformation.beliefs.y) * 10;
                }
            }

            foreach (Node connectedNode in receivingNode.userInformation.connectedNodes.Keys)
            {
                if (connectedNode.nodePrio < 0)
                {
                    continue;
                }

                if (actingNode == null)
                {
                    actingNode = connectedNode;
                }
                else if (actingNode.nodePrio < connectedNode.nodePrio)
                {
                    actingNode = connectedNode;
                }
            }
        }

        if (buttonInfo.type == ActionType.Connect)
        {
            Node closestCentristNode = null;
            float shortestDistance = Mathf.Infinity;

            foreach (Node centristNode in NodeManager.nM.centristNodes)
            {
                if(!receivingNode.userInformation.connectedNodes.Contains(centristNode))
                {
                    if(shortestDistance > Vector3.Distance(receivingNode.transform.position, centristNode.transform.position))
                    {
                        shortestDistance = Vector3.Distance(receivingNode.transform.position, centristNode.transform.position);
                        closestCentristNode = centristNode;
                    }
                }
            }

            actingNode = closestCentristNode;
        }

        if (actingNode == null)
        {
            Debug.Log("Unable to find acting node");
            return;
        }

        actingNode.performingAction = true;
        receivingNode.receivingActions++;

        //MakeNewAction(buttonInfo.type, LayerManager.lM.activeLayer, actingNode, receivingNode);

        TimeManager.tM.AddTimeScale(0.75f);

        numOfPlayerActions++;
        RoomManager.rM.AdjustDonutHolder(numOfPlayerActions);
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
                foreach(AbstractAction possibleAction in LevelManager.lM.levelFactions[faction].availableActions)
                {
                    int applicableActions = 0;
                    Vector3 availableActions = nodeGroup.CheckAvailableActions(faction);

                    if(possibleAction.costType == AbstractAction.ActionCostType.InternalAction)
                    {
                        applicableActions = ((int)availableActions.x);
                    }
                    else if (possibleAction.costType == AbstractAction.ActionCostType.ExternlAction)
                    {
                        applicableActions = ((int)availableActions.y);
                    }
                    else if (possibleAction.costType == AbstractAction.ActionCostType.ExternalGroupAction)
                    {
                        applicableActions = ((int)availableActions.z);
                    }

                    if(possibleAction.CheckActionAvailability(nodeGroup, applicableActions))
                    {
                        foreach(NodeGroup possibleActingNodeGroup in possibleAction.ProvidePossibleActingNodes(nodeGroup, faction))
                        {
                            possibleActions.Add(MakePossibleGroupAction(possibleAction, possibleActingNodeGroup, nodeGroup, faction));
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

    private void MakeNewAction(AbstractAction action, Node actingNode, Node receivingNode, Faction faction)
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

    private void MakeNewGroupAction(AbstractAction aT, connectionLayer actionLayer, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup)
    {
        CurrentAction newCurrentAction;
        newCurrentAction.action = aT;
        newCurrentAction.actingNodeGroup = actingNodeGroup;
        actingNodeGroup.performingActions++;
        newCurrentAction.receivingNodeGroup = receivingNodeGroup;
        receivingNodeGroup.receivingActions++;
        newCurrentAction.actionLayer = LayerManager.lM.activeLayer;
        newCurrentAction.timer = aT.timeToAct;
        newCurrentAction.timerMax = newCurrentAction.timer;
        newCurrentAction.faction = actingNodeGroup.groupFaction;
        newCurrentAction.actionLine = Instantiate<GameObject>(LevelManager.lM.levelFactions[newCurrentAction.faction].actionLine, this.transform).GetComponent<ActionLine>();
        newCurrentAction.actionLine.transform.position = Vector3.Lerp(actingNodeGroup.transform.position, receivingNodeGroup.transform.position, 0.5f);
        newCurrentAction.actionLine.SyncLine(0, actingNodeGroup.transform.position, receivingNodeGroup.transform.position, actionLayer != LayerManager.lM.activeLayer);
        newCurrentAction.actionRing = Instantiate<GameObject>(actionRing, this.transform);
        newCurrentAction.actionRing.transform.position = receivingNodeGroup.transform.position;
        newCurrentAction.bleat = TweetManager.tM.PublishTweet(LevelManager.lM.tweetsForActions[aT], actingNodeGroup.nodesInGroup[Random.Range(0, actingNodeGroup.nodesInGroup.Count - 1)], receivingNodeGroup.nodesInGroup[Random.Range(0, receivingNodeGroup.nodesInGroup.Count - 1)], newCurrentAction.faction);
        SetActionRing(newCurrentAction.actionRing, 0f, newCurrentAction.faction, receivingNodeGroup.transform.position);
        currentActions.Add(newCurrentAction);
        
        Debug.Log("Performing action type " + aT + " for faction " + newCurrentAction.faction + ", from " + actingNodeGroup + " to " + receivingNodeGroup);
    }

    private PossibleAction MakePossibleGroupAction(AbstractAction action, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction faction)
    {
        PossibleAction newPossibleAction = new PossibleAction();
        newPossibleAction.action = action;
        newPossibleAction.actingNodeGroup = actingNodeGroup;
        newPossibleAction.receivingNodeGroup = receivingNodeGroup;
        newPossibleAction.actionLayer = connectionLayer.online;
        newPossibleAction.faction = faction;

        newPossibleAction.score = action.ProvideActionScore(actingNodeGroup, receivingNodeGroup, faction);

        newPossibleAction.score += Random.Range(0f, 2f);
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
            }

            currentActions[i] = adjustedCurAction;
        }
    }

    public void PivotAction_Receiving(NodeGroup ogReceivingNodeGroup, NodeGroup newReceivingNodeGroup)
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            var adjustedCurAction = currentActions[i];

            if (adjustedCurAction.receivingNodeGroup == ogReceivingNodeGroup)
            {
                adjustedCurAction.receivingNodeGroup = newReceivingNodeGroup;
                ogReceivingNodeGroup.receivingActions--;
                newReceivingNodeGroup.receivingActions++;
            }

            currentActions[i] = adjustedCurAction;
        }
    }

    private int numOfPlayerActions = 0;

    [SerializeField] public List<CurrentAction> currentActions = new List<CurrentAction>();
    [SerializeField] public List<Node> previousNodes = new List<Node>();

    [SerializeField] GameObject actionRing;
    [SerializeField] Vector3 outerActionRingScale;

    [SerializeField] int actingNodeMemoryLength;

    [SerializeField] public SerializableDictionary<ActionType, ActionInformation> actionInformation = new SerializableDictionary<ActionType, ActionInformation>(); //Where x = online action time, and y = offline action time

    private TimeManager tm;
}

public enum ActionType
{ 
    DM,
    Ban,
    Left,
    Right,
    Up,
    Down,
    Connect,
    Inoculate,
    DoubleLeft,
    DoubleRight,
    DoubleUp,
    DoubleDown,
    None
}

[System.Serializable]
public struct CurrentAction
{
    public AbstractAction action;
    public NodeGroup actingNodeGroup;
    public NodeGroup receivingNodeGroup;
    public float timer;
    public float timerMax;
    public connectionLayer actionLayer;
    public ActionLine actionLine;
    public GameObject actionRing;
    public Faction faction;
    public Bleat bleat;
}

[System.Serializable]
public struct PossibleAction
{
    public AbstractAction action;
    public NodeGroup actingNodeGroup;
    public NodeGroup receivingNodeGroup;
    public Faction faction;
    public connectionLayer actionLayer;
    public float score;
}

[System.Serializable]
public struct ActionInformation
{
    public bool internalAction;
    public int actionCost;
    public Vector2 actionLength;
    public Vector2 actionPosition;
}
