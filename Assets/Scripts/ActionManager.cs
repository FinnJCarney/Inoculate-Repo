using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEditor.Experimental.GraphView;
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
    }

    private void Start()
    {
        tm = TimeManager.tM;
    }

    private void Update()
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            if (currentActions[i].faction != currentActions[i].actingNode.userInformation.faction)
            {
                currentActions[i].actingNode.performingAction = false;
                currentActions[i].receivingNode.receivingActions--;
                Destroy(currentActions[i].actionLine.gameObject);
                Destroy(currentActions[i].actionRing);
                currentActions.Remove(currentActions[i]);
                continue;
            }

            var adjustedCurAction = currentActions[i];
            adjustedCurAction.timer -= tm.adjustedDeltaTime;
            currentActions[i] = adjustedCurAction;

            if ((currentActions[i].faction == LevelManager.lM.playerAllyFaction || currentActions[i].receivingNode.userInformation.faction == LevelManager.lM.playerAllyFaction || currentActions[i].actingNode.showMenu || currentActions[i].receivingNode.showMenu) && LayerManager.lM.activeLayer == currentActions[i].actionLayer)
            {

                currentActions[i].actionLine.SyncLine(1f - (currentActions[i].timer / currentActions[i].timerMax), currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
            }
            else
            {
                currentActions[i].actionLine.SyncLine(0, currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
            }

            SetActionRing(currentActions[i].actionRing, (currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax, currentActions[i].faction);
            currentActions[i].receivingNode.SetActionAudio((currentActions[i].timerMax - currentActions[i].timer) / currentActions[i].timerMax);

            if (currentActions[i].timer < 0f)
            {
                currentActions[i].receivingNode.ActionResult(currentActions[i].actionType, currentActions[i].actingNode.userInformation.faction, currentActions[i].actingNode, currentActions[i].actionLayer);
                currentActions[i].actingNode.performingAction = false;
                currentActions[i].receivingNode.receivingActions--;
                Destroy(currentActions[i].actionLine.gameObject);
                Destroy(currentActions[i].actionRing);
                currentActions.Remove(currentActions[i]);
                continue;
            }
        }
    }

    private void SetActionRing(GameObject actionRing, float amountThrough, Faction faction)
    {
        actionRing.transform.localScale = Vector3.Lerp(outerActionRingScale, new Vector3(0.1f, 0.1f, 0.1f), amountThrough);
        
        Color idealColor = Color.clear;

        idealColor = LevelManager.lM.levelFactions[faction].color;

        actionRing.GetComponent<SpriteRenderer>().color = Color.Lerp(Color.clear, idealColor, amountThrough);
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

                connectedNode.nodePrio -= connectedNode.userInformation.connectedNodes.Count * 5;

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


            ideologicalDistance = Vector2.Distance(averagePos, LevelManager.lM.levelFactions[faction].position);

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

                        if (node.userInformation.beliefs.y > connectedNode.userInformation.beliefs.y && HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.up))
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
                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.up, LevelManager.lM.levelFactions[faction].position) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].position))
                            {
                                strategicValue += 2f;
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.y < connectedNode.userInformation.beliefs.y && HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.down))
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
                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.down, LevelManager.lM.levelFactions[faction].position) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].position))
                            {
                                strategicValue += 2f;
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x > connectedNode.userInformation.beliefs.x && HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.right))
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

                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.right, LevelManager.lM.levelFactions[faction].position) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].position))
                            {
                                strategicValue += 2f;
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x < connectedNode.userInformation.beliefs.x && HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.left))
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

                            if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.left, LevelManager.lM.levelFactions[faction].position) < Vector2.Distance(connectedNode.userInformation.beliefs, LevelManager.lM.levelFactions[faction].position))
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

                        if (HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.up))
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

                        if (HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.down))
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

                        if (HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.right))
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

                        if (HUDManager.hM.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.left))
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
        CurrentAction newCurrentAction;
        newCurrentAction.actionType = newActionType;
        newCurrentAction.actingNode = actingNode;
        actingNode.performingAction = true;
        newCurrentAction.receivingNode = receivingNode;
        newCurrentAction.actionLayer = actionLayer;
        newCurrentAction.timer = actionLayer == connectionLayer.online ? actionInformation[newActionType].actionLength.x : actionInformation[newActionType].actionLength.y;
        newCurrentAction.timerMax = newCurrentAction.timer;
        newCurrentAction.faction = actingNode.userInformation.faction;
        newCurrentAction.actionLine = Instantiate<GameObject>(LevelManager.lM.levelFactions[newCurrentAction.faction].actionLine, this.transform).GetComponent<ActionLine>();
        newCurrentAction.actionLine.transform.position = Vector3.Lerp(actingNode.transform.position, receivingNode.transform.position, 0.5f);
        newCurrentAction.actionLine.SyncLine(0, actingNode.transform.position, receivingNode.transform.position);
        newCurrentAction.actionRing = Instantiate<GameObject>(actionRing, this.transform);
        newCurrentAction.actionRing.transform.position = receivingNode.transform.position;
        SetActionRing(newCurrentAction.actionRing, 0f, newCurrentAction.faction);
        currentActions.Add(newCurrentAction);

        Debug.Log("Performing action type " + newActionType + " for faction " + newCurrentAction.faction + ", from " + actingNode + " to " + receivingNode);
    }

    private PossibleAction MakePossibleAction(ActionType actionType, connectionLayer actionLayer, Node actingNode, Node receivingNode)
    {
        PossibleAction newPossibleAction = new PossibleAction();
        newPossibleAction.actionType = actionType;
        newPossibleAction.actingNode = actingNode;
        newPossibleAction.receivingNode = receivingNode;
        newPossibleAction.actionLayer = actionLayer;

        Vector2 factionPos = LevelManager.lM.levelFactions[actingNode.userInformation.faction].position;
        factionPos.x = factionPos.x == 3 ? receivingNode.userInformation.beliefs.x : factionPos.x;
        factionPos.y = factionPos.y == 3 ? receivingNode.userInformation.beliefs.y : factionPos.y;

        newPossibleAction.score += actingNode.userInformation.faction == receivingNode.userInformation.faction ? 0f : 2.5f; //If node is in another faction, more valuable to grab

        if (actionLayer == connectionLayer.offline)
        {
            newPossibleAction.score -= 1f;

            if (HUDManager.hM.IsSpaceValid(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f)))
            {
                if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), actingNode.userInformation.beliefs) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs)) //If distance between acting and receiving nodes is closer due to action
                {
                    newPossibleAction.score += 5f;
                }

                if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), factionPos) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, factionPos)) //If distance between receiving node and faction center is closer due to action
                {
                    newPossibleAction.score += 3f;
                }

                foreach (Node cNConnectedNode in receivingNode.userInformation.connectedNodes.Keys)
                {
                    if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition * 2f), cNConnectedNode.userInformation.beliefs) < 1.45f)
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
            Debug.Log("For action type" + actionType + " with starting position " + receivingNode.userInformation.beliefs + ", distance with action " + MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + actionInformation[actionType].actionPosition, actingNode.userInformation.beliefs) + " vs distance without " + MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs));
            if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + actionInformation[actionType].actionPosition, actingNode.userInformation.beliefs) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, actingNode.userInformation.beliefs)) //If distance between acting and receiving nodes is closer due to action
            {
                newPossibleAction.score += 4f;
            }

            if (MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs + actionInformation[actionType].actionPosition, factionPos) < MinDistanceBetweenTwoVector2sOnMap(receivingNode.userInformation.beliefs, factionPos)) //If distance between receiving node and faction center is closer due to action
            {
                newPossibleAction.score += 2f;
            }

            foreach (Node cNConnectedNode in receivingNode.userInformation.connectedNodes.Keys)
            {
                if (Vector2.Distance(receivingNode.userInformation.beliefs + (actionInformation[actionType].actionPosition), cNConnectedNode.userInformation.beliefs) < 1.45f)
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


    private float MinDistanceBetweenTwoVector2sOnMap(Vector2 startingPos, Vector2 endingPos)
    {

        List<Vector2> positionsToCheck = new List<Vector2>();
        Dictionary<Vector2, List<Vector2>> possiblePaths = new Dictionary<Vector2, List<Vector2>>();

        positionsToCheck.Add(startingPos);

        List<Vector2> startingPath = new List<Vector2>();
        startingPath.Add(startingPos);
        possiblePaths.Add(startingPos, startingPath);

        //Debug.Log("Checking for path from " + startingPos + " to " + endingPos);

        for (int i = 0; i < positionsToCheck.Count; i++)
        {
            Vector2 positionToCheck = positionsToCheck[i];

            List<Vector2> positionsToAdd = new List<Vector2>();

            if (HUDManager.hM.IsSpaceValid(positionToCheck + Vector2.up))
            {
                positionsToAdd.Add(positionToCheck + Vector2.up);
            }

            if (HUDManager.hM.IsSpaceValid(positionToCheck + Vector2.down))
            {
                positionsToAdd.Add(positionToCheck + Vector2.down);
            }

            if (HUDManager.hM.IsSpaceValid(positionToCheck + Vector2.right))
            {
                positionsToAdd.Add(positionToCheck + Vector2.right);
            }

            if (HUDManager.hM.IsSpaceValid(positionToCheck + Vector2.left))
            {
                positionsToAdd.Add(positionToCheck + Vector2.left);
            }

            foreach (Vector2 positionToAdd in positionsToAdd)
            {
                List<Vector2> newPossiblePath = new List<Vector2>();

                if (possiblePaths.ContainsKey(positionToCheck))
                {
                    //Debug.Log("Possible paths contains " + positionToCheck);
                    foreach (Vector2 pathPos in possiblePaths[positionToCheck])
                    {
                        newPossiblePath.Add(pathPos);
                    }
                    newPossiblePath.Add(positionToAdd);
                }
                else
                {
                    //Debug.Log("Possible paths does not contain " + positionToCheck);
                    newPossiblePath.Add(positionToCheck);
                    newPossiblePath.Add(positionToAdd);
                }

                //Debug.Log("Possible Path between " + newPossiblePath[0] + " and " + positionToAdd + " is " + newPossiblePath.Count);

                if (!possiblePaths.ContainsKey(positionToAdd))
                {
                    //Debug.Log("Possible paths does not contain " + positionToAdd);
                    possiblePaths.Add(positionToAdd, newPossiblePath);
                    positionsToCheck.Add(positionToAdd);
                }
                else
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

        //Debug.Log("Shortest Path between " + startingPos + " and " + endingPos + " is " + possiblePaths[endingPos].Count);

        return possiblePaths[endingPos].Count;
    }


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
    None
}

[System.Serializable]
public struct CurrentAction
{
    public ActionType actionType;
    public Node actingNode;
    public Node receivingNode;
    public float timer;
    public float timerMax;
    public connectionLayer actionLayer;
    public ActionLine actionLine;
    public GameObject actionRing;
    public Faction faction;
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
    public Vector2 actionLength;
    public Vector2 actionPosition;
}
