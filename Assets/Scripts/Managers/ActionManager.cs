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
                currentActions[i].receivingNodeGroup.ActionResult(currentActions[i].actionType, currentActions[i].actingNodeGroup.groupFaction, currentActions[i].actingNodeGroup, currentActions[i].actionLayer, currentActions[i].bleat);
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

    public void PerfromGroupButtonAction(ActionType aT, NodeGroup receivingNodeGroup)
    {
        NodeGroup actingNodeGroup = null;

        if (aT != ActionType.Connect)
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

                if (aT == ActionType.Left || aT == ActionType.DoubleLeft)
                {
                    nodeGroup.prio += Mathf.Abs(Mathf.RoundToInt(receivingNodeGroup.groupBelief.x - nodeGroup.groupBelief.x) * 10);
                }

                if (aT == ActionType.Right || aT == ActionType.DoubleRight)
                {
                    nodeGroup.prio += Mathf.Abs(Mathf.RoundToInt(receivingNodeGroup.groupBelief.x - nodeGroup.groupBelief.x) * 10);
                }

                if (aT == ActionType.Up || aT == ActionType.DoubleUp)
                {
                    nodeGroup.prio += Mathf.Abs(Mathf.RoundToInt(receivingNodeGroup.groupBelief.y - nodeGroup.groupBelief.y) * 10);
                }

                if (aT == ActionType.Down || aT == ActionType.DoubleDown)
                {
                    nodeGroup.prio += Mathf.Abs(Mathf.RoundToInt(receivingNodeGroup.groupBelief.y - nodeGroup.groupBelief.y) * 10);
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

        MakeNewAction(buttonInfo.type, LayerManager.lM.activeLayer, actingNode, receivingNode);

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

            if (possibleActingNodes.Count == 0)
            {
                Debug.Log("Failed to find nodes to act");
            }

            //Decision Making

            Vector2 averagePos = Vector2.zero;

            foreach (Node node in possibleActingNodes)
            {
                averagePos += node.userInformation.beliefs;
            }

            averagePos /= possibleActingNodes.Count;
            float ideologicalDistance = 0;


            ideologicalDistance = Vector2.Distance(averagePos, LevelManager.lM.levelFactions[faction].mainPosition);

            //Acting on "Inoculate"
            if (ideologicalDistance > 100) //Ignoring this for now
            {
                Debug.Log("Performing Inoculate action for " + faction);
                foreach (Node node in possibleActingNodes)
                {
                    if (node.performingAction || node.userInformation.faction == LevelManager.lM.playerAllyFaction || node.isBanned)
                    {
                        continue;
                    }

                    foreach (Node connectedNode in node.userInformation.connectedNodes.Keys)
                    {
                        if (connectedNode.userInformation.faction != node.userInformation.faction)
                        {
                            continue;
                        }

                        float proximityValue = (0.1f / Vector2.Distance(node.userInformation.beliefs, connectedNode.userInformation.beliefs)) + 0.1f;
                        float strategicValue = 0;

                        if (node.userInformation.beliefs.y > connectedNode.userInformation.beliefs.y && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.up))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Up;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;
                            newPossibleAction.score += proximityValue;

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;

                                    //if(Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.up, cNConnectedNode.userInformation.beliefs) < 1.1f)
                                    //{
                                    //    strategicValue -= 4f;
                                    //}
                                }
                            }

                            //Temp version of "Value add for move also pulling node towards factions ideological center"
                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.up, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
                            {
                                strategicValue += 2f;
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.y < connectedNode.userInformation.beliefs.y && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.down))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Down;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;
                            newPossibleAction.score += proximityValue;

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;

                                    //if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.down, cNConnectedNode.userInformation.beliefs) < 1.1f)
                                    //{
                                    //    strategicValue -= 4f;
                                    //}
                                }
                            }

                            //Temp version of "Value add for move also pulling node towards factions ideological center"
                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.down, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
                            {
                                strategicValue += 2f;
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x > connectedNode.userInformation.beliefs.x && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.right))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Right;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;
                            newPossibleAction.score += proximityValue;

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;

                                    //if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.right, cNConnectedNode.userInformation.beliefs) < 1.1f)
                                    //{
                                    //    strategicValue -= 4f;
                                    //}
                                }
                            }

                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.right, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
                            {
                                strategicValue += 2f;
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x < connectedNode.userInformation.beliefs.x && LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + Vector2.left))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Left;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;
                            newPossibleAction.score += proximityValue;

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.userInformation.connectedNodes.Keys)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;

                                    //if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.left, cNConnectedNode.userInformation.beliefs) < 1.1f)
                                    //{
                                    //    strategicValue -= 4f;
                                    //}
                                }
                            }

                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.left, LevelManager.lM.levelFactions[faction].mainPosition) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].mainPosition))
                            {
                                strategicValue += 2f;
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                    }
                }
            }

            //Acting on "Expand"
            else
            {
                Vector2 movement = new Vector2(12f, 12f);
                Debug.Log("Performing Expand action for " + faction);
                foreach (Node node in possibleActingNodes)
                {
                    if (node.performingAction || node.userInformation.faction == LevelManager.lM.playerAllyFaction || node.isBanned)
                    {
                        continue;
                    }

                    foreach (Node connectedNode in node.userInformation.connectedNodes.Keys)
                    {
                        if (node.userInformation.connectedNodes[connectedNode].type == connectionType.influencedBy) //Don't perfom an action going the wrong way down an influence line
                        {
                            continue;
                        }

                        if (node.userInformation.faction == Faction.Neutral && connectedNode.userInformation.isPlayer) //Don't perfornm a neutral action on the player
                        {
                            continue;
                        }

                        if(node.userInformation.faction == connectedNode.userInformation.faction) //You can't "expand" to nodes of the same faction
                        {
                            continue;
                        }

                        if (LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + (Vector2.up * movement)))
                        {
                            if(node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.online || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Up, connectionLayer.online, node, connectedNode));
                            }
                            if (node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.offline || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Up, connectionLayer.offline, node, connectedNode));
                            }
                        }

                        if (LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + (Vector2.down * movement)))
                        {
                            if (node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.online || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Down, connectionLayer.online, node, connectedNode));
                            }
                            if (node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.offline || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Down, connectionLayer.offline, node, connectedNode));
                            }
                        }

                        if (LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + (Vector2.right * movement)))
                        {
                            if (node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.online || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Right, connectionLayer.online, node, connectedNode));
                            }
                            if (node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.offline || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Right, connectionLayer.offline, node, connectedNode));
                            }
                        }

                        if (LevelManager.lM.CheckValidSpace(connectedNode.userInformation.beliefs + (Vector2.left * movement)))
                        {
                            if (node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.online || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Left, connectionLayer.online, node, connectedNode));
                            }
                            if (node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.offline || node.userInformation.connectedNodes[connectedNode].layer == connectionLayer.onlineOffline)
                            {
                                possibleActions.Add(MakePossibleAction(ActionType.Left, connectionLayer.offline, node, connectedNode));
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
                    MakeNewAction(possibleAction.actionType, possibleAction.actionLayer, possibleAction.actingNode, possibleAction.receivingNode);
                    break;
                }
            }
        }
    }

    private void MakeNewAction(ActionType newActionType, connectionLayer actionLayer, Node actingNode, Node receivingNode)
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
        //
        //Debug.Log("Performing action type " + newActionType + " for faction " + newCurrentAction.faction + ", from " + actingNode + " to " + receivingNode);
    }

    private void MakeNewGroupAction(ActionType newActionType, connectionLayer actionLayer, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup)
    {
        CurrentAction newCurrentAction;
        newCurrentAction.actionType = newActionType;
        newCurrentAction.actingNodeGroup = actingNodeGroup;
        actingNodeGroup.performingActions++;
        newCurrentAction.receivingNodeGroup = receivingNodeGroup;
        receivingNodeGroup.receivingActions++;
        newCurrentAction.actionLayer = actionLayer;
        newCurrentAction.timer = actionLayer == connectionLayer.online ? actionInformation[newActionType].actionLength.x : actionInformation[newActionType].actionLength.y;
        newCurrentAction.timerMax = newCurrentAction.timer;
        newCurrentAction.faction = actingNodeGroup.groupFaction;
        newCurrentAction.actionLine = Instantiate<GameObject>(LevelManager.lM.levelFactions[newCurrentAction.faction].actionLine, this.transform).GetComponent<ActionLine>();
        newCurrentAction.actionLine.transform.position = Vector3.Lerp(actingNodeGroup.transform.position, receivingNodeGroup.transform.position, 0.5f);
        newCurrentAction.actionLine.SyncLine(0, actingNodeGroup.transform.position, receivingNodeGroup.transform.position, actionLayer != LayerManager.lM.activeLayer);
        newCurrentAction.actionRing = Instantiate<GameObject>(actionRing, this.transform);
        newCurrentAction.actionRing.transform.position = receivingNodeGroup.transform.position;
        newCurrentAction.bleat = TweetManager.tM.PublishTweet(LevelManager.lM.tweetsForActions[newActionType], actingNodeGroup.nodesInGroup[Random.Range(0, actingNodeGroup.nodesInGroup.Count - 1)], receivingNodeGroup.nodesInGroup[Random.Range(0, receivingNodeGroup.nodesInGroup.Count - 1)], newCurrentAction.faction);
        SetActionRing(newCurrentAction.actionRing, 0f, newCurrentAction.faction, receivingNodeGroup.transform.position);
        currentActions.Add(newCurrentAction);
        
        Debug.Log("Performing action type " + newActionType + " for faction " + newCurrentAction.faction + ", from " + actingNodeGroup + " to " + receivingNodeGroup);
    }

    private PossibleAction MakePossibleAction(ActionType actionType, connectionLayer actionLayer, Node actingNode, Node receivingNode)
    {
        PossibleAction newPossibleAction = new PossibleAction();
        newPossibleAction.actionType = actionType;
        newPossibleAction.actingNode = actingNode;
        newPossibleAction.receivingNode = receivingNode;
        newPossibleAction.actionLayer = actionLayer;

        Vector2 factionPos = LevelManager.lM.levelFactions[actingNode.userInformation.faction].mainPosition;
        factionPos.x = factionPos.x == 3 ? receivingNode.userInformation.beliefs.x : factionPos.x;
        factionPos.y = factionPos.y == 3 ? receivingNode.userInformation.beliefs.y : factionPos.y;

        newPossibleAction.score += actingNode.userInformation.faction == receivingNode.userInformation.faction ? 0f : 2.5f; //If node is in another faction, more valuable to grab

        if (actionLayer == connectionLayer.offline)
        {
            newPossibleAction.score -= 1f;

            if (LevelManager.lM.CheckValidSpace(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f)))
            {
                if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), actingNode.userInformation.beliefs) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs)) //If distance between acting and receiving nodes is closer due to action
                {
                    newPossibleAction.score += 5f;
                }

                if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), factionPos) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, factionPos)) //If distance between receiving node and faction center is closer due to action
                {
                    newPossibleAction.score += 3f;
                }

                if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), actingNode.userInformation.beliefs) < 12.1f)
                {
                    newPossibleAction.score += 5f;
                }

                foreach (Node cNConnectedNode in receivingNode.userInformation.connectedNodes.Keys)
                {
                    if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), cNConnectedNode.userInformation.beliefs) < 12.1f)
                    {
                        if (cNConnectedNode.userInformation.faction == Faction.Neutral)
                        {
                            newPossibleAction.score += 2f;
                        }
                        else
                        {
                            newPossibleAction.score -= 2f;
                        }
                    }

                    if (cNConnectedNode.userInformation.faction == receivingNode.userInformation.faction)
                    {
                        newPossibleAction.score += 2f;
                    }
                }
            }
            else
            {
                newPossibleAction.score -= 10f;
            }
        }
        else
        {
            if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + actionInformation[actionType].actionPosition, actingNode.userInformation.beliefs) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs)) //If distance between acting and receiving nodes is closer due to action
            {
                newPossibleAction.score += 4f;
            }

            if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + actionInformation[actionType].actionPosition, factionPos) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, factionPos)) //If distance between receiving node and faction center is closer due to action
            {
                newPossibleAction.score += 2f;
            }

            if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition), actingNode.userInformation.beliefs) < 12.1f)
            {
                newPossibleAction.score += 4f;
            }

            foreach (Node cNConnectedNode in receivingNode.userInformation.connectedNodes.Keys)
            {
                if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition), cNConnectedNode.userInformation.beliefs) < 12.1f)
                {
                    if (cNConnectedNode.userInformation.faction == Faction.Neutral)
                    {
                        newPossibleAction.score += 2f;
                    }
                    else
                    {
                        newPossibleAction.score -= 2f;
                    }
                }

                if (cNConnectedNode.userInformation.faction == receivingNode.userInformation.faction)
                {
                    newPossibleAction.score += 2f;


                    if (cNConnectedNode.userInformation.faction == receivingNode.userInformation.faction)
                    {
                        newPossibleAction.score += 2f;
                    }
                }
            }
        }

        newPossibleAction.score += Random.Range(0f, 1f);
        return newPossibleAction;
    }


    private float MinDistanceBetweenTwoVector2sOnMap(Vector2 startingPos, Vector2 endingPos) //Move to LevelManager
    {
        Vector2 movement = new Vector2(12f, 12f);
        List<Vector2> positionsToCheck = new List<Vector2>();
        Dictionary<Vector2, List<Vector2>> possiblePaths = new Dictionary<Vector2, List<Vector2>>();

        positionsToCheck.Add(startingPos);

        List<Vector2> startingPath = new List<Vector2>();
        startingPath.Add(startingPos);
        possiblePaths.Add(startingPos, startingPath);

        //Debug.Log("Checking for path from " + startingPos + " to " + endingPos);

        for (int i = 0; i < positionsToCheck.Count; i++) //Check list of Vector2s that are valid
        {
            Vector2 positionToCheck = positionsToCheck[i];

            List<Vector2> positionsToAdd = new List<Vector2>();

            //Check all adjacent Vector2, if they are available, we should look at them
            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.up * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.up * movement));
            }

            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.down * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.down * movement));
            }

            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.right * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.right * movement));
            }

            if (LevelManager.lM.CheckValidSpace(positionToCheck + (Vector2.left * movement)))
            {
                positionsToAdd.Add(positionToCheck + (Vector2.left * movement));
            }

            foreach (Vector2 positionToAdd in positionsToAdd)
            {
                //We're gonna see what a path to this new Vector2 looks like
                List<Vector2> newPossiblePath = new List<Vector2>();

                //Have we been to our Vector2 before? If so, grab the current path there now
                if (possiblePaths.ContainsKey(positionToCheck))
                {
                    //Debug.Log("Possible paths contains " + positionToCheck);
                    foreach (Vector2 pathPos in possiblePaths[positionToCheck])
                    {
                        newPossiblePath.Add(pathPos);
                    }
                    newPossiblePath.Add(positionToAdd);
                }
                else //Else, we have no record of how to get here, which probably means it's very close
                {
                    //Debug.Log("Possible paths does not contain " + positionToCheck);
                    newPossiblePath.Add(positionToCheck);
                    newPossiblePath.Add(positionToAdd);
                }

                //We have established a path to this new vector2 we're checking
                //Debug.Log("Possible Path between " + newPossiblePath[0] + " and " + positionToAdd + " is " + newPossiblePath.Count);

                //We've never been to this Vector2 before! Whatever path we've developed to it is currently our best one, so let's add that path and check what's around it in future
                if (!possiblePaths.ContainsKey(positionToAdd))
                {
                    //Debug.Log("Possible paths does not contain " + positionToAdd);
                    possiblePaths.Add(positionToAdd, newPossiblePath);
                    positionsToCheck.Add(positionToAdd);
                }
                else //We have been to this vector 2 before, so check how long it previously took to get here vs our new path. Save whichever one is shorter
                {
                    //Debug.Log("Possible paths does contain " + positionToAdd + "with length " + possiblePaths[positionToAdd].Count);
                    if (newPossiblePath.Count < possiblePaths[positionToAdd].Count)
                    {
                        //Debug.Log(newPossiblePath.Count + " is smaller than " + possiblePaths[positionToAdd].Count);
                        possiblePaths[positionToAdd].Clear();

                        foreach (Vector2 pathPos in newPossiblePath)
                        {
                            possiblePaths[positionToAdd].Add(pathPos);
                        }

                        positionsToCheck.Add(positionToAdd);
                    }
                }

                //Debug.Log("New smallest path between " + startingPos + " and " + positionToAdd + " is " + possiblePaths[positionToAdd].Count);
            }
        }

        //At this stage, we should have checked essentially every point on the board, and in that process found the shortest path to our ideal end point, so return that
        //Debug.Log("Shortest Path between " + startingPos + " and " + endingPos + " is " + possiblePaths[endingPos].Count);

        return possiblePaths[endingPos].Count;
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
    public ActionType actionType;
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
    public ActionType actionType;
    public Node actingNode;
    public Node receivingNode;
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
