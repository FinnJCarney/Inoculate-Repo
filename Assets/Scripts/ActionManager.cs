using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
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
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (dMActionLength - currentActions[i].timer) / dMActionLength));
                }

                if (currentActions[i].actionType == ActionType.Ban)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (longOnlineActionLength - currentActions[i].timer) / longOnlineActionLength));
                }

                if (currentActions[i].actionType == ActionType.Up || currentActions[i].actionType == ActionType.Down || currentActions[i].actionType == ActionType.Right || currentActions[i].actionType == ActionType.Left)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (mediumOnlineActionLength - currentActions[i].timer) / mediumOnlineActionLength));
                }

                if (currentActions[i].actionType == ActionType.Connect)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (longOnlineActionLength - currentActions[i].timer) / longOnlineActionLength));
                }
            }
            else
            {
                currentActions[i].actionLine.SetPosition(1, currentActions[i].actingNode.transform.position);
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

    private void SetActionRing(GameObject actionRing, bool playerActivated, float amountThrough, AllyStatus faction)
    {
        actionRing.transform.localScale = Vector3.Lerp(outerActionRingScale, new Vector3(0.1f, 0.1f, 0.1f), amountThrough);
        
        Color idealColor = Color.clear;
        
        if(faction == AllyStatus.Red)
        {
            idealColor = Color.red;
        }
        else if(faction == AllyStatus.Yellow)
        {
            idealColor = Color.yellow;
        }
        else if (faction == AllyStatus.Blue)
        {
            idealColor = Color.blue;
        }
        else if (faction == AllyStatus.Green)
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

                if (connectedNode.performingAction || connectedNode.isBanned || connectedNode.userInformation.allyStatus != LevelManager.lM.playerAllyFaction)
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
        newCurrentAction.faction = actingNode.userInformation.allyStatus;

        if (newCurrentAction.faction == AllyStatus.Red)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Red).GetComponent<LineRenderer>();
        }
        else if (newCurrentAction.faction == AllyStatus.Yellow)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Yellow).GetComponent<LineRenderer>();
        }
        else if (newCurrentAction.faction == AllyStatus.Green)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Green).GetComponent<LineRenderer>();
        }
        else if (newCurrentAction.faction == AllyStatus.Blue)
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Blue).GetComponent<LineRenderer>();
        }
        else
        {
            newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Neutral).GetComponent<LineRenderer>();
        }

        newCurrentAction.actionLine.transform.position = Vector3.Lerp(actingNode.transform.position, receivingNode.transform.position, 0.5f);
        newCurrentAction.actionLine.SetPosition(0, actingNode.transform.position);
        newCurrentAction.actionLine.SetPosition(1, actingNode.transform.position);
        newCurrentAction.playerActivated = true;
        newCurrentAction.actionRing = Instantiate<GameObject>(actionRing);
        newCurrentAction.actionRing.transform.position = receivingNode.transform.position;
        SetActionRing(newCurrentAction.actionRing, newCurrentAction.playerActivated, 0f, newCurrentAction.faction);
        currentActions.Add(newCurrentAction);
    }

    public void PerformAIAction(int NumOfActions)
    {
        Node[] actingNodes = new Node[NumOfActions];

        int vertAction = 0;
        int horiAction = 0;

        foreach (Node node in NodeManager.nM.nodes)
        {
            node.nodePrio = 0;

            if (node.performingAction || node.userInformation.allyStatus == LevelManager.lM.playerAllyFaction || node.isBanned)
            {
                node.nodePrio -= 1000;
            }

            if (previousNodes.Contains(node))
            {
                node.nodePrio -= 25;
            }

            if(node.userInformation.misinformerHori)
            {
                node.nodePrio += 25;
            }

            if(node.userInformation.misinformerVert)
            {
                node.nodePrio += 25;
            }

            foreach(Node connectedNode in node.connectedNodes)
            {
                node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.x));
                node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - connectedNode.userInformation.beliefs.y));

                if (node.userInformation.beliefs.x == connectedNode.userInformation.beliefs.x && node.userInformation.beliefs.x == connectedNode.userInformation.beliefs.y)
                {
                    node.nodePrio -= 10;
                }
            }

            node.nodePrio += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x)) * 10;
            node.nodePrio += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.y)) * 10;
            
            horiAction += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x)) * 10;
            vertAction += Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.y)) * 10;

            node.nodePrio += Random.Range(0, 10);          
        }

        for (int i = 0; i < NumOfActions; i++)
        {
            foreach (Node node in NodeManager.nM.nodes)
            {
                if (node.nodePrio < 0)
                {
                    continue;
                }

                if (actingNodes[i] == null)
                {
                    actingNodes[i] = node;
                }
                else if (actingNodes[i].nodePrio < node.nodePrio)
                {
                    actingNodes[i] = node;
                }
            }
        }

        Node[] receivingNodes = new Node[NumOfActions];

        for (int i = 0; i < actingNodes.Length; i++)
        {
            if (actingNodes[i] == null)
            {
                continue;
            }

            foreach (Node node in actingNodes[i].connectedNodes)
            {
                node.nodePrio = 0;

                if (node.isBanned)
                {
                    node.nodePrio -= 1000;
                }

                node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.x));
                node.nodePrio += 15 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.y));

                horiAction += 20 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.x));
                vertAction += 20 * Mathf.RoundToInt(Mathf.Abs(node.userInformation.beliefs.x - actingNodes[i].userInformation.beliefs.y));

                if(node.userInformation.beliefs.x == actingNodes[i].userInformation.beliefs.x)
                {
                    horiAction = 0;
                }

                if (node.userInformation.beliefs.y == actingNodes[i].userInformation.beliefs.y)
                {
                    vertAction = 0;
                }

                if(horiAction == 0 && vertAction == 0)
                {
                    node.nodePrio -= 1000;
                }

                node.nodePrio += node.connectedNodes.Count;

                node.nodePrio += Random.Range(-5, 5);
            }
        }

        for (int i = 0; i < actingNodes.Length; i++)
        {
            if (actingNodes[i] == null)
            {
                continue;
            }

            foreach (Node node in actingNodes[i].connectedNodes)
            {
                if (node.nodePrio < 0)
                {
                    continue;
                }

                if (receivingNodes[i] == null)
                {
                    receivingNodes[i] = node;
                }
                else if (receivingNodes[i].nodePrio < node.nodePrio)
                {
                    receivingNodes[i] = node;
                }
            }
        }

        if (vertAction == horiAction)
        {
            vertAction += Random.Range(-10, 10);
        }

        for (int i = 0; i < actingNodes.Length; i++)
        {
            if (actingNodes[i] == null || receivingNodes[i] == null)
            {
                Debug.LogWarning("Failed to execute AI action");
                continue;
            }

            CurrentAction newCurrentAction;

            newCurrentAction.actionType = ActionType.None;

            if(horiAction > vertAction)
            {
                if (actingNodes[i].userInformation.beliefs.x > receivingNodes[i].userInformation.beliefs.x)
                {
                    newCurrentAction.actionType = ActionType.Right;
                }
                else
                {
                    newCurrentAction.actionType = ActionType.Left;
                }
            }

            else
            {
                if (actingNodes[i].userInformation.beliefs.y > receivingNodes[i].userInformation.beliefs.y)
                {
                    newCurrentAction.actionType = ActionType.Up;
                }
                else
                {
                    newCurrentAction.actionType = ActionType.Down;
                }
            }

            previousNodes.Add(actingNodes[i]);

            if(previousNodes.Count > actingNodeMemoryLength)
            {
                previousNodes.RemoveAt(0);
            }

            receivingNodes[i].PlayActionAudio();

            newCurrentAction.actingNode = actingNodes[i];
            newCurrentAction.actingNode.performingAction = true;
            newCurrentAction.receivingNode = receivingNodes[i];
            newCurrentAction.receivingNode.receivingActions++;
            newCurrentAction.timer = mediumOnlineActionLength;
            newCurrentAction.faction = actingNodes[i].userInformation.allyStatus;

            if (newCurrentAction.faction == AllyStatus.Red)
            {
                newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Red).GetComponent<LineRenderer>();
            }
            else if (newCurrentAction.faction == AllyStatus.Yellow)
            {
                newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Yellow).GetComponent<LineRenderer>();
            }
            else if (newCurrentAction.faction == AllyStatus.Green)
            {
                newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Green).GetComponent<LineRenderer>();
            }
            else if (newCurrentAction.faction == AllyStatus.Blue)
            {
                newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Blue).GetComponent<LineRenderer>();
            }
            else
            {
                newCurrentAction.actionLine = Instantiate<GameObject>(ActionLineObj_Neutral).GetComponent<LineRenderer>();
            }

            newCurrentAction.actionLine.SetPosition(0, actingNodes[i].transform.position);
            newCurrentAction.actionLine.SetPosition(1, actingNodes[i].transform.position);
            newCurrentAction.playerActivated = false;
            newCurrentAction.actionRing = Instantiate<GameObject>(actionRing);
            newCurrentAction.actionRing.transform.position = receivingNodes[i].transform.position;
            SetActionRing(newCurrentAction.actionRing, newCurrentAction.playerActivated, 0f, newCurrentAction.faction);
            currentActions.Add(newCurrentAction);
        }
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
    public LineRenderer actionLine;
    public bool playerActivated;
    public GameObject actionRing;
    public AllyStatus faction;
}

