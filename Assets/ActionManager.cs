using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using static Node;

public class ActionManager : MonoBehaviour
{
    public static ActionManager aM;

    private void Awake()
    {
        aM = this;
    }

    private void Update()
    {
        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            var adjustedCurAction = currentActions[i];
            adjustedCurAction.timer -= Time.deltaTime;
            currentActions[i] = adjustedCurAction;

            if (currentActions[i].actionType == ActionType.DM)
            {
                currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (dMActionLength - currentActions[i].timer) / dMActionLength));

                if (currentActions[i].timer < 0f)
                {
                    currentActions[i].actingNode.performingAction = false;
                    currentActions[i].receivingNode.DMResult();
                    Destroy(currentActions[i].actionLine);
                    currentActions.Remove(currentActions[i]);
                    continue;
                }
            }

            if (currentActions[i].actionType == ActionType.ClimateChange)
            {
                currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                Debug.Log((educationActionLength - currentActions[i].timer) / educationActionLength);
                if (currentActions[i].timer < 0f)
                {
                    currentActions[i].actingNode.performingAction = false;
                    currentActions[i].receivingNode.ClimateChangeEducationResult();
                    Destroy(currentActions[i].actionLine.gameObject);
                    currentActions.Remove(currentActions[i]);
                    continue;
                }
            }

            if (currentActions[i].actionType == ActionType.MinorityRights)
            {
                currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                Debug.Log((educationActionLength - currentActions[i].timer) / educationActionLength);
                if (currentActions[i].timer < 0f)
                {
                    currentActions[i].actingNode.performingAction = false;
                    currentActions[i].receivingNode.MinorityRightsEducationResult();
                    Destroy(currentActions[i].actionLine.gameObject);
                    currentActions.Remove(currentActions[i]);
                    continue;
                }
            }

            if (currentActions[i].actionType == ActionType.WealthInequality)
            {
                currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                Debug.Log((educationActionLength - currentActions[i].timer) / educationActionLength);
                if (currentActions[i].timer < 0f)
                {
                    currentActions[i].actingNode.performingAction = false;
                    currentActions[i].receivingNode.WealthInequalityEducationResult();
                    Destroy(currentActions[i].actionLine.gameObject);
                    currentActions.Remove(currentActions[i]);
                    continue;
                }
            }
        }
    }



    public void PerformButtonAction(UserButton buttonInfo)
    {
        Node receivingNode = buttonInfo.relatedNode;

        if (buttonInfo.type != ActionType.Accuse)
        {
            Node actingNode = null;

            foreach (Node connectedNode in receivingNode.connectedNodes)
            {
                connectedNode.nodePrio = 0;

                if (connectedNode.performingAction)
                {
                    connectedNode.nodePrio -= 1000;
                }

                connectedNode.nodePrio += receivingNode.connectedNodes.IndexOf(connectedNode);

                if (connectedNode.cCHidden != true && connectedNode.beliefClimateChange == BeliefStates.Believes)
                {
                    connectedNode.nodePrio += 10;
                }

                if (connectedNode.mRHidden != true && connectedNode.beliefMinorityRights == BeliefStates.Believes)
                {
                    connectedNode.nodePrio += 10;
                }

                if (connectedNode.wEHidden != true && connectedNode.beliefWealthInequality == BeliefStates.Believes)
                {
                    connectedNode.nodePrio += 10;
                }
            }

            foreach (Node connectedNode in receivingNode.connectedNodes)
            {
                if(connectedNode.nodePrio < 0)
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

            actingNode.performingAction = true;

            if (buttonInfo.type == ActionType.DM)
            {
                CurrentAction newCurrentAction;
                newCurrentAction.actionType = ActionType.DM;
                newCurrentAction.actingNode = actingNode;
                newCurrentAction.receivingNode = receivingNode;
                newCurrentAction.timer = dMActionLength;
                newCurrentAction.actionLine = Instantiate<GameObject>(actionLineObj).GetComponent<LineRenderer>();
                newCurrentAction.actionLine.SetPosition(0, actingNode.transform.position);
                newCurrentAction.actionLine.SetPosition(1, actingNode.transform.position);
                currentActions.Add(newCurrentAction);
            }
            
            if(buttonInfo.type == ActionType.Educate)
            {
                CurrentAction newCurrentAction;
                newCurrentAction.actionType = ActionType.DM;
                newCurrentAction.actingNode = actingNode;
                newCurrentAction.receivingNode = receivingNode;
                newCurrentAction.timer = educationActionLength;
                newCurrentAction.actionLine = Instantiate<GameObject>(actionLineObj).GetComponent<LineRenderer>();
                newCurrentAction.actionLine.SetPosition(0, actingNode.transform.position);
                newCurrentAction.actionLine.SetPosition(1, actingNode.transform.position);
                currentActions.Add(newCurrentAction);
            }
        }
    }

    public void PerformAIAction(int NumOfActions)
    {
        Node[] actingNodes = new Node[NumOfActions];

        foreach (Node node in NodeManager.nM.nodes)
        {
            node.nodePrio = 0;

            if (node.performingAction || node.isPlayer)
            {
                node.nodePrio -= 1000;
            }

            if(node.misinformerClimateChange || node.misinformerMinorityRights || node.misinformerWealthInequality)
            {
                node.nodePrio += 1000;
            }

            if (node.beliefClimateChange != BeliefStates.Unsure)
            {
                node.nodePrio += 10;
            }
            if (node.beliefClimateChange != BeliefStates.Denies)
            {
                node.nodePrio += 5;
            }

            if (node.beliefMinorityRights != BeliefStates.Unsure)
            {
                node.nodePrio += 10;
            }

            if (node.beliefMinorityRights != BeliefStates.Denies)
            {
                node.nodePrio += 15;
            }

            if (node.beliefWealthInequality != BeliefStates.Unsure)
            {
                node.nodePrio += 10;
            }

            if (node.beliefWealthInequality != BeliefStates.Denies)
            {
                node.nodePrio += 10;
            }


            node.nodePrio += node.connectedNodes.Count;

            node.nodePrio += Random.Range(0, 15);
        }

        for (int i = 0; i < NumOfActions; i++)
        {
            foreach (Node node in NodeManager.nM.nodes)
            {
                if (node.nodePrio < 0 || node.performingAction)
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

                actingNodes[i].performingAction = true;
            }
        }

        Node[] receivingNodes = new Node[NumOfActions];


        for (int i = 0; i < NumOfActions; i++)
        {
            foreach (Node node in actingNodes[i].connectedNodes)
            {
                node.nodePrio = 0;

                if (node.isPlayer)
                {
                    node.nodePrio -= 1000;
                }

                if (node.beliefClimateChange != actingNodes[i].beliefClimateChange)
                {
                    node.nodePrio += 15;
                }

                if (node.beliefMinorityRights != actingNodes[i].beliefMinorityRights)
                {
                    node.nodePrio += 15;
                }

                if (node.beliefWealthInequality != actingNodes[i].beliefWealthInequality)
                {
                    node.nodePrio += 15;
                }

                node.nodePrio += node.connectedNodes.Count;

                node.nodePrio += Random.Range(-5, 5);
            }
        }

        for (int i = 0; i < NumOfActions; i++)
        {
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

        for (int i = 0; i < NumOfActions; i++)
        {
            CurrentAction newCurrentAction;
            newCurrentAction.actionType = ActionType.ClimateChange;
            newCurrentAction.actingNode = actingNodes[i];
            newCurrentAction.receivingNode = receivingNodes[i];
            newCurrentAction.timer = educationActionLength;
            newCurrentAction.actionLine = Instantiate<GameObject>(actionLineObj).GetComponent<LineRenderer>();
            newCurrentAction.actionLine.colorGradient = neutralGradient;
            newCurrentAction.actionLine.SetPosition(0, actingNodes[i].transform.position);
            newCurrentAction.actionLine.SetPosition(1, actingNodes[i].transform.position);
            currentActions.Add(newCurrentAction);
        }
    }

    [SerializeField] public List<CurrentAction> currentActions = new List<CurrentAction>();

    [SerializeField] float dMActionLength;
    [SerializeField] float educationActionLength;

    [SerializeField] GameObject actionLineObj;
    [SerializeField] Gradient neutralGradient;
}

public enum ActionType
{ 
    DM,
    Educate,
    Accuse,
    ClimateChange,
    MinorityRights,
    WealthInequality
}

[System.Serializable]
public struct CurrentAction
{
    public ActionType actionType;
    public Node actingNode;
    public Node receivingNode;
    public float timer;
    public LineRenderer actionLine;
}

