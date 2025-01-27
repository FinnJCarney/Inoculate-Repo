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


            if (currentActions[i].playerActivated || currentActions[i].actingNode.showMenu || currentActions[i].receivingNode.showMenu)
            {
                //if (!currentActions[i].actionLine.GetComponent<AudioSource>().isPlaying)
                //{
                //    currentActions[i].actionLine.GetComponent<AudioSource>().Play();
                //}

                if (currentActions[i].actionType == ActionType.DM)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (dMActionLength - currentActions[i].timer) / dMActionLength));
                }

                if (currentActions[i].actionType == ActionType.Educate_ClimateChange)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                }

                if (currentActions[i].actionType == ActionType.Educate_MinorityRights)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                }

                if (currentActions[i].actionType == ActionType.Educate_WealthInequality)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                }

                if (currentActions[i].actionType == ActionType.Disinform_ClimateChange)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                }

                if (currentActions[i].actionType == ActionType.Disinform_MinorityRights)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                }

                if (currentActions[i].actionType == ActionType.Disinform_WealthInequality)
                {
                    currentActions[i].actionLine.SetPosition(1, Vector3.Lerp(currentActions[i].actingNode.transform.position, currentActions[i].receivingNode.transform.position, (educationActionLength - currentActions[i].timer) / educationActionLength));
                }
            }
            else
            {
                currentActions[i].actionLine.SetPosition(1, currentActions[i].actingNode.transform.position);
                //currentActions[i].actionLine.GetComponent<AudioSource>().Pause();
            }

            if (currentActions[i].actionType == ActionType.DM)
            {
                currentActions[i].receivingNode.SetActionRing(currentActions[i].playerActivated, (dMActionLength - currentActions[i].timer) / dMActionLength);
            }

            if (currentActions[i].actionType == ActionType.Educate_ClimateChange)
            {
                currentActions[i].receivingNode.SetActionRing(currentActions[i].playerActivated, (educationActionLength - currentActions[i].timer) / educationActionLength);
            }

            if (currentActions[i].actionType == ActionType.Educate_MinorityRights)
            {
                currentActions[i].receivingNode.SetActionRing(currentActions[i].playerActivated, (educationActionLength - currentActions[i].timer) / educationActionLength);
            }

            if (currentActions[i].actionType == ActionType.Educate_WealthInequality)
            {
                currentActions[i].receivingNode.SetActionRing(currentActions[i].playerActivated, (educationActionLength - currentActions[i].timer) / educationActionLength);
            }

            if (currentActions[i].actionType == ActionType.Disinform_ClimateChange)
            {
                currentActions[i].receivingNode.SetActionRing(currentActions[i].playerActivated, (educationActionLength - currentActions[i].timer) / educationActionLength);
            }

            if (currentActions[i].actionType == ActionType.Disinform_MinorityRights)
            {
                currentActions[i].receivingNode.SetActionRing(currentActions[i].playerActivated, (educationActionLength - currentActions[i].timer) / educationActionLength);
            }

            if (currentActions[i].actionType == ActionType.Disinform_WealthInequality)
            {
                currentActions[i].receivingNode.SetActionRing(currentActions[i].playerActivated, (educationActionLength - currentActions[i].timer) / educationActionLength);
            }

            if (currentActions[i].timer < 0f)
            {
                //currentActions[i].actionLine.GetComponent<AudioSource>().Stop();
                currentActions[i].receivingNode.ActionResult(currentActions[i].actionType, currentActions[i].playerActivated);
                currentActions[i].actingNode.performingAction = false;
                Destroy(currentActions[i].actionLine.gameObject);
                currentActions.Remove(currentActions[i]);
                continue;
            }
        }
    }



    public void PerformButtonAction(UserButton buttonInfo)
    {
        if(!buttonInfo.buttonEnabled)
        {
            return;
        }

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
                newCurrentAction.actionLine = Instantiate<GameObject>(playerLineObj).GetComponent<LineRenderer>();
                newCurrentAction.actionLine.SetPosition(0, actingNode.transform.position);
                newCurrentAction.actionLine.SetPosition(1, actingNode.transform.position);
                newCurrentAction.playerActivated = true;
                currentActions.Add(newCurrentAction);
            }
            
            if(buttonInfo.type == ActionType.Educate_ClimateChange)
            {
                CurrentAction newCurrentAction;
                newCurrentAction.actionType = ActionType.Educate_ClimateChange;
                newCurrentAction.actingNode = actingNode;
                newCurrentAction.receivingNode = receivingNode;
                newCurrentAction.timer = educationActionLength;
                newCurrentAction.actionLine = Instantiate<GameObject>(playerLineObj).GetComponent<LineRenderer>();
                newCurrentAction.actionLine.SetPosition(0, actingNode.transform.position);
                newCurrentAction.actionLine.SetPosition(1, actingNode.transform.position);
                newCurrentAction.playerActivated = true;
                currentActions.Add(newCurrentAction);
            }

            if (buttonInfo.type == ActionType.Educate_MinorityRights)
            {
                CurrentAction newCurrentAction;
                newCurrentAction.actionType = ActionType.Educate_MinorityRights;
                newCurrentAction.actingNode = actingNode;
                newCurrentAction.receivingNode = receivingNode;
                newCurrentAction.timer = educationActionLength;
                newCurrentAction.actionLine = Instantiate<GameObject>(playerLineObj).GetComponent<LineRenderer>();
                newCurrentAction.actionLine.SetPosition(0, actingNode.transform.position);
                newCurrentAction.actionLine.SetPosition(1, actingNode.transform.position);
                newCurrentAction.playerActivated = true;
                currentActions.Add(newCurrentAction);
            }

            if (buttonInfo.type == ActionType.Educate_WealthInequality)
            {
                CurrentAction newCurrentAction;
                newCurrentAction.actionType = ActionType.Educate_WealthInequality;
                newCurrentAction.actingNode = actingNode;
                newCurrentAction.receivingNode = receivingNode;
                newCurrentAction.timer = educationActionLength;
                newCurrentAction.actionLine = Instantiate<GameObject>(playerLineObj).GetComponent<LineRenderer>();
                newCurrentAction.actionLine.transform.position = Vector3.Lerp(actingNode.transform.position, receivingNode.transform.position, 0.5f);
                newCurrentAction.actionLine.SetPosition(0, actingNode.transform.position);
                newCurrentAction.actionLine.SetPosition(1, actingNode.transform.position);
                newCurrentAction.playerActivated = true;
                currentActions.Add(newCurrentAction);
            }
        }
    }

    public void PerformAIAction(int NumOfActions)
    {
        Node[] actingNodes = new Node[NumOfActions];

        int educateCC = 0;
        int educateMR = 0;
        int educateWE = 0;

        foreach (Node node in NodeManager.nM.nodes)
        {
            node.nodePrio = 0;

            if (node.performingAction || node.isPlayer || node.showMenu)
            {
                node.nodePrio -= 1000;
            }

            if (previousNodes.Contains(node))
            {
                node.nodePrio -= 25;
            }

            if(node.misinformerClimateChange)
            {
                node.nodePrio += 25;
                educateCC += 15;
            }

            if (node.misinformerMinorityRights)
            {
                node.nodePrio += 25;
                educateMR += 15;
            }

            if (node.misinformerWealthInequality)
            {
                node.nodePrio += 25;
                educateWE += 15;
            }

            if (node.beliefClimateChange != BeliefStates.Unsure)
            {
                node.nodePrio += 10;
                educateCC += 10;
            }
            else
            {
                educateCC -= 50;
            }

            if (node.beliefClimateChange == BeliefStates.Denies)
            {
                node.nodePrio += 10;
                educateCC += 10;
            }

            if (node.beliefMinorityRights != BeliefStates.Unsure)
            {
                node.nodePrio += 10;
                educateMR += 10;
            }
            else
            {
                educateMR -= 50;
            }

            if (node.beliefMinorityRights == BeliefStates.Denies)
            {
                node.nodePrio += 10;
                educateMR += 10;
            }

            if (node.beliefWealthInequality != BeliefStates.Unsure)
            {
                node.nodePrio += 10;
                educateWE += 10;
            }
            else
            {
                educateWE -= 50;
            }

            if (node.beliefWealthInequality == BeliefStates.Denies)
            {
                node.nodePrio += 10;
                educateWE += 10;
            }


            node.nodePrio += node.connectedNodes.Count * 3;

            node.nodePrio += Random.Range(0, 20);

            educateCC += Random.Range(0, 15);
            educateMR += Random.Range(0, 15);
            educateWE += Random.Range(0, 15);
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

                actingNodes[i].performingAction = true;
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

                if (node.isPlayer)
                {
                    node.nodePrio -= 1000;
                }

                if (node.beliefClimateChange != actingNodes[i].beliefClimateChange)
                {
                    node.nodePrio += 15;
                    educateCC += 15;
                }

                if (node.beliefMinorityRights != actingNodes[i].beliefMinorityRights)
                {
                    node.nodePrio += 15;
                    educateMR += 15;
                }

                if (node.beliefWealthInequality != actingNodes[i].beliefWealthInequality)
                {
                    node.nodePrio += 15;
                    educateWE += 15;
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

        if(educateCC == educateMR || educateMR == educateWE || educateCC == educateWE)
        {
            educateCC += Random.Range(0, 10);
            educateMR += Random.Range(0, 10);
            educateWE += Random.Range(0, 10);
        }

        for (int i = 0; i < actingNodes.Length; i++)
        {
            if (actingNodes[i] == null)
            {
                continue;
            }

            CurrentAction newCurrentAction;

            newCurrentAction.actionType = ActionType.None;

            if(educateCC > educateMR && educateCC > educateWE)
            {
                if (actingNodes[i].beliefClimateChange == BeliefStates.Believes)
                {
                    newCurrentAction.actionType = ActionType.Educate_ClimateChange;
                }
                else
                {
                    newCurrentAction.actionType = ActionType.Disinform_ClimateChange;
                }
            }

            if (educateMR > educateCC && educateMR > educateWE)
            {
                if (actingNodes[i].beliefMinorityRights == BeliefStates.Believes)
                {
                    newCurrentAction.actionType = ActionType.Educate_MinorityRights;
                }
                else
                {
                    newCurrentAction.actionType = ActionType.Disinform_MinorityRights;
                }
            }

            if (educateWE > educateCC && educateWE > educateMR)
            {
                if (actingNodes[i].beliefWealthInequality == BeliefStates.Believes)
                {
                    newCurrentAction.actionType = ActionType.Educate_WealthInequality;
                }
                else
                {
                    newCurrentAction.actionType = ActionType.Disinform_WealthInequality;
                }
            }

            previousNodes.Add(actingNodes[i]);

            if(previousNodes.Count > 3)
            {
                previousNodes.RemoveAt(0);
            }

            receivingNodes[i].PlayActionAudio();

            newCurrentAction.actingNode = actingNodes[i];
            newCurrentAction.receivingNode = receivingNodes[i];
            newCurrentAction.timer = educationActionLength;
            newCurrentAction.actionLine = Instantiate<GameObject>(actionLineObj).GetComponent<LineRenderer>();
            newCurrentAction.actionLine.colorGradient = neutralGradient;
            newCurrentAction.actionLine.SetPosition(0, actingNodes[i].transform.position);
            newCurrentAction.actionLine.SetPosition(1, actingNodes[i].transform.position);
            newCurrentAction.playerActivated = false;
            currentActions.Add(newCurrentAction);
        }
    }

    [SerializeField] public List<CurrentAction> currentActions = new List<CurrentAction>();
    [SerializeField] public List<Node> previousNodes = new List<Node>();

    [SerializeField] float dMActionLength;
    [SerializeField] float educationActionLength;

    [SerializeField] GameObject playerLineObj;
    [SerializeField] GameObject actionLineObj;
    [SerializeField] Gradient neutralGradient;
}

public enum ActionType
{ 
    DM,
    Educate,
    Accuse,
    Educate_ClimateChange,
    Educate_MinorityRights,
    Educate_WealthInequality,
    Disinform_ClimateChange,
    Disinform_MinorityRights,
    Disinform_WealthInequality,
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
}

