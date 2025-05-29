using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using static Node;

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
            currentActions.Remove(currentActions[i]);
        }
    }

    private void Start()
    {
        tm = TimeManager.i;
    }

    private void Update()
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            var adjustedCurAction = currentActions[i];
            adjustedCurAction.timer -= tm.adjustedDeltaTime;
            currentActions[i] = adjustedCurAction;

            if (currentActions[i].playerActivated || currentActions[i].actingNode.showMenu || currentActions[i].receivingNode.showMenu)
            {
                if (currentActions[i].actionType == ActionType.DM)
                {
                    currentActions[i].actionLine.SyncLine(1f - (currentActions[i].timer / dMActionLength), currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
                }

                if (currentActions[i].actionType == ActionType.Ban || currentActions[i].actionType == ActionType.Connect)
                {
                    currentActions[i].actionLine.SyncLine(1f - (currentActions[i].timer / longOnlineActionLength), currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
                }

                if (currentActions[i].actionType == ActionType.Up || currentActions[i].actionType == ActionType.Down || currentActions[i].actionType == ActionType.Right || currentActions[i].actionType == ActionType.Left)
                {
                    currentActions[i].actionLine.SyncLine(1f - (currentActions[i].timer / mediumOnlineActionLength), currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
                }
            }
            else
            {
                currentActions[i].actionLine.SyncLine(0, currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position);
            }

            if (currentActions[i].actionType == ActionType.DM)
            {
                SetActionRing(currentActions[i].actionRing, currentActions[i].playerActivated, (dMActionLength - currentActions[i].timer) / dMActionLength, currentActions[i].faction);
                currentActions[i].receivingNode.SetActionAudio(currentActions[i].playerActivated, (dMActionLength - currentActions[i].timer) / dMActionLength);
            }

            if (currentActions[i].actionType == ActionType.Ban)
            {
                SetActionRing(currentActions[i].actionRing, currentActions[i].playerActivated, (longOnlineActionLength - currentActions[i].timer) / longOnlineActionLength, currentActions[i].faction);
                currentActions[i].receivingNode.SetActionAudio(currentActions[i].playerActivated, (longOnlineActionLength - currentActions[i].timer) / longOnlineActionLength);
            }

            if (currentActions[i].actionType == ActionType.Up || currentActions[i].actionType == ActionType.Down || currentActions[i].actionType == ActionType.Right || currentActions[i].actionType == ActionType.Left)
            {
                SetActionRing(currentActions[i].actionRing, currentActions[i].playerActivated, (mediumOnlineActionLength - currentActions[i].timer) / mediumOnlineActionLength, currentActions[i].faction);
                currentActions[i].receivingNode.SetActionAudio(currentActions[i].playerActivated, (mediumOnlineActionLength - currentActions[i].timer) / mediumOnlineActionLength);
            }

            if (currentActions[i].actionType == ActionType.Connect)
            {
                SetActionRing(currentActions[i].actionRing, currentActions[i].playerActivated, (longOnlineActionLength - currentActions[i].timer) / longOnlineActionLength, currentActions[i].faction);
                currentActions[i].receivingNode.SetActionAudio(currentActions[i].playerActivated, (longOnlineActionLength - currentActions[i].timer) / longOnlineActionLength);
            }

            if (currentActions[i].timer < 0f)
            {
                currentActions[i].receivingNode.ActionResult(currentActions[i].actionType, currentActions[i].playerActivated, currentActions[i].actingNode);
                currentActions[i].actingNode.performingAction = false;
                currentActions[i].receivingNode.receivingActions--;
                Destroy(currentActions[i].actionLine.gameObject);
                Destroy(currentActions[i].actionRing);
                currentActions.Remove(currentActions[i]);
                continue;
            }
        }
    }

    private void SetActionRing(GameObject actionRing, bool playerActivated, float amountThrough, Faction faction)
    {
        actionRing.transform.localScale = Vector3.Lerp(outerActionRingScale, new Vector3(0.1f, 0.1f, 0.1f), amountThrough);
        
        Color idealColor = Color.clear;
        
        if(faction == Faction.UpRight)
        {
            idealColor = Color.red;
        }
        else if(faction == Faction.DownRight)
        {
            idealColor = Color.yellow;
        }
        else if (faction == Faction.UpLeft)
        {
            idealColor = Color.blue;
        }
        else if (faction == Faction.DownLeft)
        {
            idealColor = Color.green;
        }
        else
        {
            idealColor = Color.white;
        }

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
            foreach (Node connectedNode in receivingNode.connectedNodes)
            {
                connectedNode.nodePrio = 100;

                if (connectedNode.performingAction || connectedNode.isBanned || connectedNode.userInformation.faction != LevelManager.lM.playerAllyFaction)
                {
                    connectedNode.nodePrio -= 1000;
                }

                connectedNode.nodePrio -= receivingNode.connectedNodes.IndexOf(connectedNode);
                connectedNode.nodePrio -= connectedNode.connectedNodes.Count * 5;

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

            foreach (Node connectedNode in receivingNode.connectedNodes)
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
                if(!receivingNode.connectedNodes.Contains(centristNode))
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

        if (buttonInfo.type == ActionType.DM)
        {
            MakeNewAction(ActionType.DM, shortOnlineActionLength, actingNode, receivingNode);
        }

        if (buttonInfo.type == ActionType.Ban)
        {
            MakeNewAction(ActionType.Ban, longOnlineActionLength, actingNode, receivingNode);
        }

        if (buttonInfo.type == ActionType.Left)
        {
            MakeNewAction(ActionType.Left, mediumOnlineActionLength, actingNode, receivingNode);
        }

        if (buttonInfo.type == ActionType.Right)
        {
            MakeNewAction(ActionType.Right, mediumOnlineActionLength, actingNode, receivingNode);
        }

        if (buttonInfo.type == ActionType.Up)
        {
            MakeNewAction(ActionType.Up, mediumOnlineActionLength, actingNode, receivingNode);
        }

        if (buttonInfo.type == ActionType.Down)
        {
            MakeNewAction(ActionType.Down, mediumOnlineActionLength, actingNode, receivingNode);
        }

        if (buttonInfo.type == ActionType.Connect)
        {
            MakeNewAction(ActionType.Connect, longOnlineActionLength, actingNode, receivingNode);
        }
    }

    private void MakeNewAction(ActionType newActionType, float actionLength, Node actingNode, Node receivingNode)
    {
        CurrentAction newCurrentAction;
        newCurrentAction.actionType = newActionType;
        newCurrentAction.actingNode = actingNode;
        newCurrentAction.receivingNode = receivingNode;
        newCurrentAction.timer = actionLength;
        newCurrentAction.faction = actingNode.userInformation.faction;

        if (newCurrentAction.faction == Faction.UpRight)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Red).GetComponent<ActionLine>();
        }
        else if (newCurrentAction.faction == Faction.DownRight)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Yellow).GetComponent<ActionLine>();
        }
        else if (newCurrentAction.faction == Faction.DownLeft)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Green).GetComponent<ActionLine>();
        }
        else if (newCurrentAction.faction == Faction.UpLeft)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Blue).GetComponent<ActionLine>();
        }
        else
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Neutral).GetComponent<ActionLine>();
        }

        newCurrentAction.actionLine.transform.position = Vector3.Lerp(actingNode.transform.position, receivingNode.transform.position, 0.5f);
        newCurrentAction.actionLine.SyncLine(0, actingNode.transform.position, receivingNode.transform.position);
        newCurrentAction.playerActivated = true;
        newCurrentAction.actionRing = Instantiate<GameObject>(actionRing);
        newCurrentAction.actionRing.transform.position = receivingNode.transform.position;
        SetActionRing(newCurrentAction.actionRing, newCurrentAction.playerActivated, 0f, newCurrentAction.faction);
        currentActions.Add(newCurrentAction);
    }

    public void PerformAIAction(int NumOfActions, Faction faction)
    {
        for (int i = 0; i < NumOfActions; i++)
        {
            Node[] actingNodes = new Node[NumOfActions];

            List<PossibleAction> possibleActions = new List<PossibleAction>();
            List<Node> possibleActingNodes = null;

            if (faction == Faction.UpRight)
            {
                possibleActingNodes = NodeManager.nM.redNodes;
            }
            else if (faction == Faction.DownRight)
            {
                possibleActingNodes = NodeManager.nM.yellowNodes;
            }
            else if (faction == Faction.UpLeft)
            {
                possibleActingNodes = NodeManager.nM.blueNodes;
            }
            else if (faction == Faction.DownLeft)
            {
                possibleActingNodes = NodeManager.nM.greenNodes;
            }
            else if (faction == Faction.Neutral)
            {
                possibleActingNodes = NodeManager.nM.neutralNodes;
            }

            if(possibleActingNodes.Count == 0)
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


            //REWORK THIS to be a count of current factions and their noted ideological centers
            if (faction == Faction.UpRight)
            {
                ideologicalDistance = Vector2.Distance(averagePos, new Vector2(2, 2));
            }
            else if (faction == Faction.DownRight)
            {
                ideologicalDistance = Vector2.Distance(averagePos, new Vector2(2, -2));
            }
            else if (faction == Faction.UpLeft)
            {
                ideologicalDistance = Vector2.Distance(averagePos, new Vector2(-2, 2));
            }
            else if (faction == Faction.DownLeft)
            {
                ideologicalDistance = Vector2.Distance(averagePos, new Vector2(-2, -2));
            }

            //Acting on "Inoculate"
            if (ideologicalDistance > 2)
            {
                Debug.Log("Performing Inoculate action for " + faction);
                foreach (Node node in possibleActingNodes)
                {
                    if (node.performingAction || node.userInformation.faction == LevelManager.lM.playerAllyFaction || node.isBanned)
                    {
                        continue;
                    }

                    foreach (Node connectedNode in node.connectedNodes)
                    {
                        if (connectedNode.userInformation.faction != node.userInformation.faction)
                        {
                            continue;
                        }

                        float proximityValue = (0.1f / Vector2.Distance(node.userInformation.beliefs, connectedNode.userInformation.beliefs)) + 0.1f;
                        float strategicValue = 0;

                        if (node.userInformation.beliefs.y > connectedNode.userInformation.beliefs.y && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.up))
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

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;
                                }
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.y < connectedNode.userInformation.beliefs.y && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.down))
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

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;
                                }
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x > connectedNode.userInformation.beliefs.x && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.right))
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

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;
                                }
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x < connectedNode.userInformation.beliefs.x && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.left))
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

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (cNConnectedNode.userInformation.faction != node.userInformation.faction)
                                {
                                    strategicValue += 2f;
                                }
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

                    foreach (Node connectedNode in node.connectedNodes)
                    {
                        float proximityValue = (0.1f / Vector2.Distance(node.userInformation.beliefs, connectedNode.userInformation.beliefs)) + 0.1f;
                        float factionValue = node.userInformation.faction == connectedNode.userInformation.faction ? 0f : 2.5f;
                        float strategicValue = 0;

                        if (node.userInformation.faction == Faction.Neutral && connectedNode.userInformation.isPlayer)
                        {
                            factionValue = -100f;
                        }


                        if (node.userInformation.beliefs.y > connectedNode.userInformation.beliefs.y && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.up))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Up;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;

                            newPossibleAction.score += proximityValue;
                            newPossibleAction.score += (0.1f / Mathf.Abs(node.userInformation.beliefs.y - connectedNode.userInformation.beliefs.y));

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.up, cNConnectedNode.userInformation.beliefs) < 1.5f)
                                {
                                    if (cNConnectedNode.userInformation.faction == Faction.Neutral)
                                    {
                                        strategicValue += 2f;
                                    }
                                    else
                                    {
                                        strategicValue -= 2f;
                                    }
                                }
                            }

                            newPossibleAction.score += factionValue;
                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.y < connectedNode.userInformation.beliefs.y && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.down))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Down;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;
                            newPossibleAction.score += proximityValue;
                            newPossibleAction.score += (0.1f / Mathf.Abs(node.userInformation.beliefs.y - connectedNode.userInformation.beliefs.y));

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.down, cNConnectedNode.userInformation.beliefs) < 1.5f)
                                {
                                    if (cNConnectedNode.userInformation.faction == Faction.Neutral)
                                    {
                                        strategicValue += 2f;
                                    }
                                    else
                                    {
                                        strategicValue -= 2f;
                                    }
                                }
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += factionValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x > connectedNode.userInformation.beliefs.x && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.right))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Right;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;
                            newPossibleAction.score += proximityValue;
                            newPossibleAction.score += (0.1f / Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.x));

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.right, cNConnectedNode.userInformation.beliefs) < 1.5f)
                                {
                                    if (cNConnectedNode.userInformation.faction == Faction.Neutral)
                                    {
                                        strategicValue += 2f;
                                    }
                                    else
                                    {
                                        strategicValue -= 2f;
                                    }
                                }
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += factionValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }

                        if (node.userInformation.beliefs.x < connectedNode.userInformation.beliefs.x && HUDManager.i.IsSpaceValid(connectedNode.userInformation.beliefs + Vector2.left))
                        {
                            PossibleAction newPossibleAction = new PossibleAction();
                            newPossibleAction.actionType = ActionType.Left;
                            newPossibleAction.actingNode = node;
                            newPossibleAction.receivingNode = connectedNode;
                            newPossibleAction.score += proximityValue;
                            newPossibleAction.score += (0.1f / Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.x));

                            if (proximityValue < 1.5f)
                            {
                                newPossibleAction.score += 2.5f;
                            }

                            foreach (Node cNConnectedNode in connectedNode.connectedNodes)
                            {
                                if (Vector2.Distance(connectedNode.userInformation.beliefs + Vector2.left, cNConnectedNode.userInformation.beliefs) < 1.5f)
                                {
                                    if (cNConnectedNode.userInformation.faction == Faction.Neutral)
                                    {
                                        strategicValue += 2f;
                                    }
                                    else
                                    {
                                        strategicValue -= 2f;
                                    }
                                }
                            }

                            newPossibleAction.score += strategicValue;
                            newPossibleAction.score += factionValue;
                            newPossibleAction.score += Random.Range(0f, 1f);
                            possibleActions.Add(newPossibleAction);
                        }
                    }
                }
            }

            float maxPossibleActionValue = 0;

            foreach (PossibleAction possibleAction in possibleActions)
            {
                if (possibleAction.score > maxPossibleActionValue)
                {
                    maxPossibleActionValue = possibleAction.score;
                }
            }

            foreach (PossibleAction possibleAction in possibleActions)
            {
                if (possibleAction.score == maxPossibleActionValue)
                {
                    CurrentAction newCurrentAction;

                    newCurrentAction.actionType = possibleAction.actionType;
                    newCurrentAction.actingNode = possibleAction.actingNode;
                    newCurrentAction.actingNode.performingAction = true;
                    newCurrentAction.receivingNode = possibleAction.receivingNode;
                    newCurrentAction.receivingNode.PlayActionAudio();
                    newCurrentAction.receivingNode.receivingActions++;
                    newCurrentAction.timer = mediumOnlineActionLength;
                    newCurrentAction.faction = newCurrentAction.actingNode.userInformation.faction;

                    if (newCurrentAction.faction == Faction.UpRight)
                    {
                        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Red).GetComponent<ActionLine>();
                    }
                    else if (newCurrentAction.faction == Faction.DownRight)
                    {
                        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Yellow).GetComponent<ActionLine>();
                    }
                    else if (newCurrentAction.faction == Faction.DownLeft)
                    {
                        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Green).GetComponent<ActionLine>();
                    }
                    else if (newCurrentAction.faction == Faction.UpLeft)
                    {
                        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Blue).GetComponent<ActionLine>();
                    }
                    else
                    {
                        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Neutral).GetComponent<ActionLine>();
                    }

                    newCurrentAction.actionLine.SyncLine(0, newCurrentAction.actingNode.transform.position, newCurrentAction.receivingNode.transform.position);
                    newCurrentAction.playerActivated = false;
                    newCurrentAction.actionRing = Instantiate<GameObject>(actionRing);
                    newCurrentAction.actionRing.transform.position = newCurrentAction.receivingNode.transform.position;
                    SetActionRing(newCurrentAction.actionRing, newCurrentAction.playerActivated, 0f, newCurrentAction.faction);
                    currentActions.Add(newCurrentAction);
                }
            }
        }


        //    }
        //
        //    foreach (Node connectedNode in node.connectedNodes)
        //    {
        //        node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.x));
        //        node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.y));
        //
        //        if (node.userInformation.beliefs.x == connectedNode.userInformation.beliefs.x && node.userInformation.beliefs.x == connectedNode.userInformation.beliefs.y)
        //        {
        //            node.nodePrio -= 10;
        //        }
        //    }
        //
        //    if (previousNodes.Contains(node))
        //    {
        //        node.nodePrio -= 100;
        //    }
        //
        //    if(node.userInformation.misinformerHori)
        //    {
        //        node.nodePrio += 25;
        //    }
        //
        //    if(node.userInformation.misinformerVert)
        //    {
        //        node.nodePrio += 25;
        //    }
        //
        //    foreach(Node connectedNode in node.connectedNodes)
        //    {
        //        node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.x));
        //        node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.y));
        //
        //        if (node.userInformation.beliefs.x == connectedNode.userInformation.beliefs.x && node.userInformation.beliefs.x == connectedNode.userInformation.beliefs.y)
        //        {
        //            node.nodePrio -= 10;
        //        }
        //    }
        //
        //    node.nodePrio += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x)) * 10;
        //    node.nodePrio += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.y)) * 10;
        //    
        //    horiAction += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x)) * 10;
        //    vertAction += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.y)) * 10;
        //
        //    node.nodePrio += Random.Range(0, 10);          
        //}
        //
        //for (int i = 0; i < NumOfActions; i++)
        //{
        //    foreach (Node node in NodeManager.nM.nodes)
        //    {
        //        if (node.nodePrio < 0)
        //        {
        //            continue;
        //        }
        //
        //        if (actingNodes[i] == null)
        //        {
        //            actingNodes[i] = node;
        //        }
        //        else if (actingNodes[i].nodePrio < node.nodePrio)
        //        {
        //            actingNodes[i] = node;
        //        }
        //    }
        //}
        //
        //Node[] receivingNodes = new Node[NumOfActions];
        //
        //for (int i = 0; i < actingNodes.Length; i++)
        //{
        //    if (actingNodes[i] == null)
        //    {
        //        continue;
        //    }
        //
        //    foreach (Node node in actingNodes[i].connectedNodes)
        //    {
        //        node.nodePrio = 0;
        //
        //        if (node.isBanned)
        //        {
        //            node.nodePrio -= 1000;
        //        }
        //
        //        node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.x));
        //        node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.y));
        //
        //        horiAction += 20 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.x));
        //        vertAction += 20 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.y));
        //
        //        if(node.userInformation.beliefs.x == actingNodes[i].userInformation.beliefs.x)
        //        {
        //            horiAction = 0;
        //        }
        //
        //        if (node.userInformation.beliefs.y == actingNodes[i].userInformation.beliefs.y)
        //        {
        //            vertAction = 0;
        //        }
        //
        //        if(horiAction == 0 && vertAction == 0)
        //        {
        //            node.nodePrio -= 1000;
        //        }
        //
        //        node.nodePrio += node.connectedNodes.Count;
        //
        //        node.nodePrio += Random.Range(-5, 5);
        //    }
        //}
        //
        //for (int i = 0; i < actingNodes.Length; i++)
        //{
        //    if (actingNodes[i] == null)
        //    {
        //        continue;
        //    }
        //
        //    foreach (Node node in actingNodes[i].connectedNodes)
        //    {
        //        if (node.nodePrio < 0)
        //        {
        //            continue;
        //        }
        //
        //        if (receivingNodes[i] == null)
        //        {
        //            receivingNodes[i] = node;
        //        }
        //        else if (receivingNodes[i].nodePrio < node.nodePrio)
        //        {
        //            receivingNodes[i] = node;
        //        }
        //    }
        //}
        //
        //if (vertAction == horiAction)
        //{
        //    vertAction += Random.Range(-10, 10);
        //}
        //
        //for (int i = 0; i < actingNodes.Length; i++)
        //{
        //    if (actingNodes[i] == null || receivingNodes[i] == null)
        //    {
        //        Debug.LogWarning("Failed to execute AI action");
        //        continue;
        //    }
        //
        //    CurrentAction newCurrentAction;
        //
        //    newCurrentAction.actionType = ActionType.None;
        //
        //    if(horiAction > vertAction)
        //    {
        //        if (actingNodes[i].userInformation.beliefs.x > receivingNodes[i].userInformation.beliefs.x)
        //        {
        //            newCurrentAction.actionType = ActionType.Right;
        //        }
        //        else
        //        {
        //            newCurrentAction.actionType = ActionType.Left;
        //        }
        //    }
        //
        //    else
        //    {
        //        if (actingNodes[i].userInformation.beliefs.y > receivingNodes[i].userInformation.beliefs.y)
        //        {
        //            newCurrentAction.actionType = ActionType.Up;
        //        }
        //        else
        //        {
        //            newCurrentAction.actionType = ActionType.Down;
        //        }
        //    }
        //
        //    previousNodes.Add(actingNodes[i]);
        //
        //    if(previousNodes.Count > actingNodeMemoryLength)
        //    {
        //        previousNodes.RemoveAt(0);
        //    }
        //
        //    receivingNodes[i].PlayActionAudio();
        //
        //    newCurrentAction.actingNode = actingNodes[i];
        //    newCurrentAction.actingNode.performingAction = true;
        //    newCurrentAction.receivingNode = receivingNodes[i];
        //    newCurrentAction.receivingNode.receivingActions++;
        //    newCurrentAction.timer = mediumOnlineActionLength;
        //    newCurrentAction.faction = actingNodes[i].userInformation.allyStatus;
        //
        //    if (newCurrentAction.faction == AllyStatus.Red)
        //    {
        //        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Red).GetComponent<ActionLine>();
        //    }
        //    else if (newCurrentAction.faction == AllyStatus.Yellow)
        //    {
        //        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Yellow).GetComponent<ActionLine>();
        //    }
        //    else if (newCurrentAction.faction == AllyStatus.Green)
        //    {
        //        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Green).GetComponent<ActionLine>();
        //    }
        //    else if (newCurrentAction.faction == AllyStatus.Blue)
        //    {
        //        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Blue).GetComponent<ActionLine>();
        //    }
        //    else
        //    {
        //        newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Neutral).GetComponent<ActionLine>();
        //    }
        //
        //    newCurrentAction.actionLine.SyncLine(0, newCurrentAction.actingNode.transform.position, newCurrentAction.receivingNode.transform.position);
        //    newCurrentAction.playerActivated = false;
        //    newCurrentAction.actionRing = Instantiate<GameObject>(actionRing);
        //    newCurrentAction.actionRing.transform.position = receivingNodes[i].transform.position;
        //    SetActionRing(newCurrentAction.actionRing, newCurrentAction.playerActivated, 0f, newCurrentAction.faction);
        //    currentActions.Add(newCurrentAction);
        //}
    }

    [SerializeField] public List<CurrentAction> currentActions = new List<CurrentAction>();
    [SerializeField] public List<Node> previousNodes = new List<Node>();

    [SerializeField] float dMActionLength;
    [SerializeField] float educationActionLength;

    [SerializeField] float shortOnlineActionLength;
    [SerializeField] float mediumOnlineActionLength;
    [SerializeField] float longOnlineActionLength;

    [SerializeField] GameObject ActionLineObj_Blue;
    [SerializeField] GameObject ActionLineObj_Yellow;
    [SerializeField] GameObject ActionLineObj_Red;
    [SerializeField] GameObject ActionLineObj_Green;
    [SerializeField] GameObject ActionLineObj_Neutral;

    [SerializeField] GameObject actionRing;
    [SerializeField] Vector3 outerActionRingScale;

    [SerializeField] int actingNodeMemoryLength;

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
    public ActionLine actionLine;
    public bool playerActivated;
    public GameObject actionRing;
    public Faction faction;
}

[System.Serializable]
public struct PossibleAction
{
    public ActionType actionType;
    public Node actingNode;
    public Node receivingNode;
    public float score;
}