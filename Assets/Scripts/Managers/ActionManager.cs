 using System.Collections;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.Analytics;

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
    
        if(StateManager.sM.gameState != GameState.Mission)
        {
            return;
        }

        CalculatePossiblePlayerActions();

        numOfPlayerActions = 0;

        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            //Debug.Log("Current Action Timer = " + currentActions[i].timer + " / " + currentActions[i].timerMax);
            if (currentActions[i].faction != currentActions[i].actingNodeGroup.groupFaction)
            {
                DestroyCurrentAction(currentActions[i]);
                continue;
            }

            //If action repivots to target itself, but isn't an internal action
            if (currentActions[i].actingNodeGroup == currentActions[i].receivingNodeGroup && currentActions[i].action.costType == Action.ActionCostType.InternalAction)
            {
                DestroyCurrentAction(currentActions[i]);
                continue;
            }

            if(!hitStun)
            {
                var adjustedCurAction = currentActions[i];
                adjustedCurAction.timer -= Time.deltaTime;
                currentActions[i] = adjustedCurAction;
            }

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

            for (int j = currentActions[i].actionLines.Length - (1); j >= 0; j--)
            {
                currentActions[i].actionLines[j].SyncLine(1f - (currentActions[i].timer / currentActions[i].timerMax), currentActions[i].actingNodeGroup.transform.position, currentActions[i].receivingNodeGroup.transform.position);
            }

            SetActionRing(currentActions[i].actionRing, (currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax, currentActions[i].faction, currentActions[i].receivingNodeGroup.transform.position);
            currentActions[i].receivingNodeGroup.SetActionAudio((currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax);

            if (currentActions[i].timer < 0f)
            {
                currentActions[i].actingNodeGroup.performingActions -= currentActions[i].action.cost;
                currentActions[i].receivingNodeGroup.receivingActions--;
                currentActions[i].receivingNodeGroup.ActionResult(currentActions[i].action, currentActions[i].actingNodeGroup.groupFaction, currentActions[i].actingNodeGroup, currentActions[i].bleat, currentActions[i].pastAction);
                CallHitStun();

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

        if (!hitStun)
        {
            TimeManager.tM.SetTimeScale(currentActions.Count > 0 ? 5f : 0.25f);
        }

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
        List<NodeGroup> possibleActingNodeGroups = new List<NodeGroup>();

        foreach(PossiblePlayerAction possibleAction in possiblePlayerActions)
        {
            if(possibleAction.action == aT && possibleAction.receivingNode == receivingNodeGroup)
            {
                possibleActingNodeGroups.Add(possibleAction.actingNode);
            }
        }

        foreach (NodeGroup nodeGroup in possibleActingNodeGroups)
        {
            nodeGroup.prio = aT.ProvideActionScore(nodeGroup, receivingNodeGroup, LevelManager.lM.playerAllyFaction);
        }

        actingNodeGroup = GiveTheorheticalActingNode(possibleActingNodeGroups);

        //Check if selectedActingNodeGroup can do action
        //If cannot, swap for one that can? 
        //Or, score actions based on ability to do actions
        //I.e, the more *specific* an action it is, the higher it's score
        //Lowest score wins???

        /*

        if (aT.costType == Action.ActionCostType.InternalAction)
        {
            possibleActingNodeGroups[0] = receivingNodeGroup;
        }
        else
        {
            possibleActingNodeGroups = aT.ProvidePossibleActingNodes(receivingNodeGroup, LevelManager.lM.playerAllyFaction);
        }

        */

        if (actingNodeGroup == null)
        {
            Debug.Log("Unable to find acting node group");
            return;
        }

        if(!CanNodeGroupPerformAction(aT, actingNodeGroup))
        {
            if(!TryToPivotPlannedActions(actingNodeGroup))
            {
                Debug.LogWarning("Failed to Pivot Action!");
                return;
            }
            else
            {
                Debug.Log("Oh my god it maybe worked");
            }
        }

        MakeNewPlannedAction(aT, actingNodeGroup, receivingNodeGroup);
    }

    private bool CanNodeGroupPerformAction(Action action, NodeGroup nodeToCheck)
    {
        return nodeToCheck.nodesInGroup.Count >= nodeToCheck.possiblePerformingActions + action.cost;
    }

    private NodeGroup GiveTheorheticalActingNode(List<NodeGroup> nodeGroups)
    {
        NodeGroup possibleActingNodeGroup = null;

        foreach (NodeGroup nodeGroup in nodeGroups)
        {
            if (nodeGroup.prio < 0)
            {
                continue;
            }

            if (possibleActingNodeGroup == null)
            {
                possibleActingNodeGroup = nodeGroup;
            }
            else if (possibleActingNodeGroup.prio < nodeGroup.prio)
            {
                possibleActingNodeGroup = nodeGroup;
            }
        }

        return possibleActingNodeGroup;
    }

    private void CalculatePossiblePlayerActions()
    {
        //While in Update
        possiblePlayerActions.Clear();

        Faction playerAllyFaction = LevelManager.lM.playerAllyFaction;

        List<Action> playerActions = LevelManager.lM.levelFactions[playerAllyFaction].availableActions;

        List<PossiblePlayerAction> actionsToCheck = new List<PossiblePlayerAction>();
        
        foreach(NodeGroup nodeGroup in LevelManager.lM.nodeGroups.Values)
        {
            if(nodeGroup.groupFaction != playerAllyFaction || nodeGroup.nodesInGroup.Count == 0)
            {
                //I.e whatever should inherently disqualify a node from offering options
                continue;
            }

            foreach (Action playerAction in playerActions)
            {
                if(playerAction.costType == Action.ActionCostType.InternalAction)
                {
                    PossiblePlayerAction newPossiblePlayerAction = new PossiblePlayerAction();
                    newPossiblePlayerAction.action = playerAction;
                    newPossiblePlayerAction.actingNode = nodeGroup;
                    newPossiblePlayerAction.receivingNode = nodeGroup;

                    if(nodeGroup.possiblePerformingActions + playerAction.cost > nodeGroup.nodesInGroup.Count)
                    {
                        actionsToCheck.Add(newPossiblePlayerAction);
                    }
                    else
                    {
                        possiblePlayerActions.Add(newPossiblePlayerAction);
                    }
                }
                else
                {
                    foreach(NodeGroup connectedNodeGroup in nodeGroup.connectedNodes.Keys)
                    {
                        if (nodeGroup.connectedNodes[connectedNodeGroup].type == connectionType.influencedBy)
                        {
                            continue;
                        }

                        PossiblePlayerAction newPossiblePlayerAction = new PossiblePlayerAction();
                        newPossiblePlayerAction.action = playerAction;
                        newPossiblePlayerAction.actingNode = nodeGroup;
                        newPossiblePlayerAction.receivingNode = connectedNodeGroup;

                        if (nodeGroup.possiblePerformingActions + playerAction.cost > nodeGroup.nodesInGroup.Count)
                        {
                            actionsToCheck.Add(newPossiblePlayerAction);
                        }
                        else
                        {
                            possiblePlayerActions.Add(newPossiblePlayerAction);
                        }
                    }
                }
            }

            foreach(PossiblePlayerAction actionToCheck in actionsToCheck)
            {
                if(CanNodePerformOtherActions(actionToCheck.actingNode))
                {
                    possiblePlayerActions.Add(actionToCheck);
                }
            }
        }
    }

    private bool CanNodePerformOtherActions(NodeGroup nodeGroupToCheck)
    {
        List<PlannedAction> actionsPerformedByNode = ProvidePlannedActionsCurrentlyPerformedByNode(nodeGroupToCheck);

        foreach(PlannedAction plannedAction in plannedActions)
        {
            if(CanOtherNodePerformAction(plannedAction))
            {
                return true;
            }
        }

        return false;
    }

    private List<PlannedAction> ProvidePlannedActionsCurrentlyPerformedByNode(NodeGroup nodeGroupToCheck)
    {
        List<PlannedAction> actionsPerformedByNode = new List<PlannedAction>();

        foreach(PlannedAction plannedAction in plannedActions)
        {
            if(plannedAction.curActingNodeGroup == nodeGroupToCheck)
            {
                actionsPerformedByNode.Add(plannedAction);
            }
        }

        return actionsPerformedByNode;
    }

    private bool CanOtherNodePerformAction(PlannedAction plannedAction)
    {
        foreach (PossiblePlayerAction possibleAction in possiblePlayerActions)
        {
            if(possibleAction.actingNode.possiblePerformingActions + plannedAction.action.cost > possibleAction.actingNode.nodesInGroup.Count)
            {
                continue;
            }

            if(possibleAction.action == plannedAction.action && possibleAction.receivingNode == plannedAction.receivingNodeGroup)
            {
                //Debug.Log("Acting Node confirming action as possible is " + possibleAction.actingNode + " with currentPlannedActions" + possibleAction.actingNode.possiblePerformingActions + " and nodes " + possibleAction.actingNode.nodesInGroup.Count);
                return true;
            }
        }
        
        return false;
    }

    private List<PossiblePlayerAction> ProvideAlternativesToPlannedAction(PlannedAction plannedAction)
    {
        List<PossiblePlayerAction> alternativeActions = new List<PossiblePlayerAction>();
        foreach (PossiblePlayerAction possibleAction in possiblePlayerActions)
        {
            if(possibleAction.actingNode == plannedAction.curActingNodeGroup)
            {
                continue;
            }

            if(possibleAction.actingNode.possiblePerformingActions + plannedAction.action.cost > possibleAction.actingNode.nodesInGroup.Count)
            {
                continue;
            }

            if(possibleAction.action == plannedAction.action && possibleAction.receivingNode == plannedAction.receivingNodeGroup)
            {
                Debug.Log("Acting Node confirming action as possible is " + possibleAction.actingNode + " with currentPlannedActions" + possibleAction.actingNode.possiblePerformingActions + " and nodes " + possibleAction.actingNode.nodesInGroup.Count);
                alternativeActions.Add(possibleAction);
            }
        }
        
        return alternativeActions;
    }

    private bool TryToPivotPlannedActions(NodeGroup nodeGroupToCheck)
    {
        List<PlannedAction> plannedActionsPerformedByNG = new List<PlannedAction>();

        foreach(PlannedAction plannedAction in plannedActions)
        {
            Debug.Log("Planned Action Node = " + plannedAction.curActingNodeGroup +  ", core Node Group we're checking is" + nodeGroupToCheck);
            if(plannedAction.curActingNodeGroup == nodeGroupToCheck)
            {
                plannedActionsPerformedByNG.Add(plannedAction);
            }
        }

        Dictionary<PlannedAction, List<PossiblePlayerAction>> alternativeActions = new Dictionary<PlannedAction, List<PossiblePlayerAction>>();

        foreach(PlannedAction plannedAction in plannedActionsPerformedByNG)
        {
            if(alternativeActions.ContainsKey(plannedAction))
            {
                continue;
            }
            alternativeActions.Add(plannedAction, ProvideAlternativesToPlannedAction(plannedAction));
        }

        int curHighestActionScore = 0;
        PlannedAction actionToSwapOut = new PlannedAction();
        PossiblePlayerAction actionToSwapIn = new PossiblePlayerAction();

        foreach(PlannedAction plannedAction in alternativeActions.Keys)
        {
            foreach(PossiblePlayerAction possibleAction in alternativeActions[plannedAction])
            {
                int actionScore =  possibleAction.action.ProvideActionScore(possibleAction.actingNode, possibleAction.receivingNode, possibleAction.actingNode.groupFaction);

                if(actionScore > curHighestActionScore)
                {
                    curHighestActionScore = actionScore;
                    actionToSwapOut = plannedAction;
                    actionToSwapIn = possibleAction;
                }
            }
        }

        if(curHighestActionScore > 0)
        {
            plannedActions.Remove(actionToSwapOut);
            MakeNewPlannedAction(actionToSwapIn.action, actionToSwapIn.actingNode, actionToSwapIn.receivingNode);
            return true;
        }

        return false;
    }

    public void PerformPlannedActions()
    {
        Debug.Log("Performing Planned Actions");
        foreach(PlannedAction plannedAction in plannedActions)
        {
            MakeNewGroupAction(plannedAction.action, plannedAction.curActingNodeGroup, plannedAction.receivingNodeGroup);
            plannedAction.curActingNodeGroup.possiblePerformingActions = 0;
        }

        plannedActions.Clear();

        foreach(Faction faction in LevelManager.lM.levelFactions.Keys)
        {
            if(faction == LevelManager.lM.playerAllyFaction || faction == Faction.Neutral)
            {
                continue;
            }

            PerformAIAction(LevelManager.lM.levelFactions[faction].positions.Count, faction);
        }
    }

        //Rework this to incorperate the idea of highest cost actions //Shouldn't be that hard now that actions have their own internal scoring? 
    public void PerformAIAction(int NumOfActions, Faction faction)
    {
        for (int i = 0; i < NumOfActions; i++)
        {
            Node[] actingNodes = new Node[NumOfActions];

            List<PossibleAction> possibleActions = new List<PossibleAction>();
            List<Node_UserInformation> possibleActingNodes = null;

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
                    MakeNewGroupAction(possibleAction.action, possibleAction.actingNodeGroup, possibleAction.receivingNodeGroup);
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

    private CurrentAction MakeNewGroupAction(Action aT, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup)
    {
        CurrentAction newCurrentAction;
        newCurrentAction.action = aT;
        newCurrentAction.actingNodeGroup = actingNodeGroup;
        actingNodeGroup.performingActions += aT.cost;
        receivingNodeGroup.receivingActions++;
        newCurrentAction.receivingNodeGroup = receivingNodeGroup;
        Debug.Log("Actor Pos = " +  actingNodeGroup.transform.position + "Receiver Pos = " +  receivingNodeGroup.transform.position + "Action Distance = " + Vector2.Distance(actingNodeGroup.transform.position, receivingNodeGroup.transform.position));
        newCurrentAction.timer = aT.actionSpeed == 0f ? 1f : Vector3.Distance(actingNodeGroup.transform.position, receivingNodeGroup.transform.position) / aT.actionSpeed;
        newCurrentAction.timerMax = newCurrentAction.timer;
        newCurrentAction.faction = actingNodeGroup.groupFaction;
        newCurrentAction.actionLines = new ActionLine[aT.cost];
        for (int i = 0; i < aT.cost; i++)
        {
            newCurrentAction.actionLines[i] = Instantiate<GameObject>(LevelManager.lM.levelFactions[newCurrentAction.faction].actionLine, this.transform).GetComponent<ActionLine>();
            newCurrentAction.actionLines[i].transform.position = Vector3.Lerp(actingNodeGroup.transform.position, receivingNodeGroup.transform.position, 0.5f);
            newCurrentAction.actionLines[i].SyncLine(0, actingNodeGroup.transform.position, receivingNodeGroup.transform.position);
        }
        newCurrentAction.actionRing = Instantiate<GameObject>(actionRing, this.transform);
        newCurrentAction.actionRing.transform.position = receivingNodeGroup.transform.position;
        newCurrentAction.bleat = TweetManager.tM.PublishTweet(LevelManager.lM.tweetsForActions[aT], actingNodeGroup.nodesInGroup[Random.Range(0, actingNodeGroup.nodesInGroup.Count - 1)], receivingNodeGroup.nodesInGroup[Random.Range(0, receivingNodeGroup.nodesInGroup.Count - 1)], newCurrentAction.faction);
        SetActionRing(newCurrentAction.actionRing, 0f, newCurrentAction.faction, receivingNodeGroup.transform.position);
        newCurrentAction.pastAction = RegisterPastAction(aT, actingNodeGroup, receivingNodeGroup, null);
        currentActions.Add(newCurrentAction);
        return currentActions[currentActions.IndexOf(newCurrentAction)];
        //Debug.Log("Performing action type " + aT + " for faction " + newCurrentAction.faction + ", from " + actingNodeGroup + " to " + receivingNodeGroup);
    }

    private PlannedAction MakeNewPlannedAction(Action aT, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup)
    {
        PlannedAction newPlannedAction = new PlannedAction();
        newPlannedAction.action = aT;
        newPlannedAction.curActingNodeGroup = actingNodeGroup;
        actingNodeGroup.possiblePerformingActions += aT.cost;
        newPlannedAction.receivingNodeGroup = receivingNodeGroup;
        newPlannedAction.timer = Vector3.Distance(actingNodeGroup.groupBelief, receivingNodeGroup.groupBelief) / aT.actionSpeed;
        plannedActions.Add(newPlannedAction);
        return newPlannedAction;
        //Debug.Log("Performing action type " + aT + " for faction " + newCurrentAction.faction + ", from " + actingNodeGroup + " to " + receivingNodeGroup);
    }

    private PossibleAction MakePossibleGroupAction(Action action, NodeGroup actingNodeGroup, NodeGroup receivingNodeGroup, Faction faction)
    {
        PossibleAction newPossibleAction = new PossibleAction();
        newPossibleAction.action = action;
        newPossibleAction.actingNodeGroup = actingNodeGroup;
        newPossibleAction.receivingNodeGroup = receivingNodeGroup;
        newPossibleAction.faction = faction;

        newPossibleAction.score = action.ProvideActionScore(actingNodeGroup, receivingNodeGroup, faction);
        return newPossibleAction;

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
        Debug.Log("Past Action Acting Count = " + (pA.actingNodeGroups.Count - 1) + "Past Acting Receiving Count = " + (pA.receivingNodeGroups.Count - 1));
        var recreatedAction = MakeNewGroupAction(pA.action, pA.actingNodeGroups[pA.actingNodeGroups.Count - 1].nodeGroupTarget, pA.receivingNodeGroups[pA.receivingNodeGroups.Count - 1].nodeGroupTarget);
        int actionIndex = currentActions.IndexOf(recreatedAction);
        pastActions.Remove(recreatedAction.pastAction);
        recreatedAction.timer = recreatedAction.timerMax - timerVal;
        recreatedAction.pastAction = pA;
        currentActions[actionIndex] = recreatedAction;
    }

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
        List<int> currentActionsByOGActingNode = new List<int>();

        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            if (currentActions[i].actingNodeGroup == ogActingNodeGroup)
            {
                currentActionsByOGActingNode.Add(i);
            }
        }

        float lowestAmountThrough = Mathf.Infinity;
        int actionToPivot = -1;

        foreach(int curActionInt in currentActionsByOGActingNode)
        {
            var curAction = currentActions[curActionInt];
            float amountThrough = 1f - (curAction.timer / curAction.timerMax);

            if(amountThrough < lowestAmountThrough)
            {
                actionToPivot = curActionInt;
                lowestAmountThrough = amountThrough;
            }
        }

        if(actionToPivot == -1)
        {
            Debug.LogWarning("Failed to Pivot");
            return;
        }

        if(newActingNodeGroup.groupFaction == ogActingNodeGroup.groupFaction)
        {
            var adjustedCurAction = currentActions[actionToPivot];
            adjustedCurAction.actingNodeGroup = newActingNodeGroup;
            ogActingNodeGroup.performingActions--;
            newActingNodeGroup.performingActions++;
            //PivotPastAction(pastActions[pastActions.IndexOf(currentActions[currentActionsByOGActingNode[actionToPivot]].pastAction)], newActingNodeGroup, true);
            currentActions[actionToPivot] = adjustedCurAction;
        }
        else
        {
            DestroyCurrentAction(currentActions[actionToPivot]);
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
                PivotPastAction(currentActions[i].pastAction, newReceivingNodeGroup, false);
            }

            currentActions[i] = adjustedCurAction;
        }
    }

    public void CallHitStun()
    {
        StartCoroutine(HitStun());
    }

    private IEnumerator HitStun()
    {
        float timer = 0f;

        while(timer < 0.333f)
        {
            hitStun = true;
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        hitStun = false;
    }

    private int numOfPlayerActions = 0;

    [SerializeField] public List<PlannedAction> plannedActions = new List<PlannedAction>();
    [SerializeField] public List<CurrentAction> currentActions = new List<CurrentAction>();
    [SerializeField] public List<PastAction> pastActions = new List<PastAction>();
    [SerializeField] public List<PossiblePlayerAction> possiblePlayerActions = new List<PossiblePlayerAction>();
    [SerializeField] public List<Node> previousNodes = new List<Node>();

    [SerializeField] GameObject actionRing;
    [SerializeField] Vector3 outerActionRingScale;

    [SerializeField] int actingNodeMemoryLength;

    private bool hitStun;
}

[System.Serializable]
public struct CurrentAction
{
    public Action action;
    public NodeGroup actingNodeGroup;
    public NodeGroup receivingNodeGroup;
    public float timer;
    public float timerMax;
    public ActionLine[] actionLines;
    public GameObject actionRing;
    public Faction faction;
    public Bleat bleat;
    public PastAction pastAction; 
}

[System.Serializable]
public struct PlannedAction
{
    public Action action;
    public NodeGroup curActingNodeGroup;
    public NodeGroup receivingNodeGroup;
    public float timer;
    public Faction faction;
}

[System.Serializable]
public struct PossibleAction
{
    public Action action;
    public NodeGroup actingNodeGroup;
    public NodeGroup receivingNodeGroup;
    public Faction faction;
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

[System.Serializable]
public struct PossiblePlayerAction
{
    public Action action;
    public NodeGroup actingNode;
    public NodeGroup receivingNode;
}


