using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Rendering;

public class NodeManager : MonoBehaviour
{
    public static NodeManager nM;

    private void Awake()
    {
        if(nM == null)
        {
            nM = this;
        }
    }

    private void Update()
    {

        DrawNodeConnectionLines();
        CheckNodeConnections();
        CheckNodeAbilities();

        int totalMisinformers = 0;
        int totalBanned = 0;

        foreach(Node node in nodes)
        {
            if(node.isBanned)
            {
                totalBanned++;
                if (!node.userInformation.misinformerHori && !node.userInformation.misinformerVert)
                {
                    StateManager.sM.GameOver(false);
                    return;
                }
            }

            if (node.userInformation.misinformerHori || node.userInformation.misinformerVert)
            {
                totalMisinformers++;
            }
        }

        if(totalBanned == totalMisinformers)
        {
            StateManager.sM.GameOver(true);
        }
    }

   
    public void DrawNodeConnectionLines()
    {
        foreach(Node node in nodes)
        {
            if (node.isBanned)
            {
                continue;
            }

            foreach (Node connectedNode in node.connectedNodes)
            {
                bool alreadyConnected = false;
                
                if(connectedNode.isBanned)
                {
                    continue;
                }

                foreach (Line line in lines)
                {
                    if (line.connectedNodes.Contains(node) && line.connectedNodes.Contains(connectedNode))
                    {
                        alreadyConnected = true;

                        if (node.userInformation.allyStatus == connectedNode.userInformation.allyStatus)
                        {
                            if (node.userInformation.allyStatus == AllyStatus.Yellow)
                            {
                                line.lineR.material = yellowLine;
                            }

                            else if (node.userInformation.allyStatus == AllyStatus.Red)
                            {
                                line.lineR.material = redLine;
                            }

                            else if (node.userInformation.allyStatus == AllyStatus.Green)
                            {
                                line.lineR.material = greenLine;
                            }

                            else if (node.userInformation.allyStatus == AllyStatus.Blue)
                            {
                                line.lineR.material = blueLine;
                            }
                        }
                        else
                        {
                            line.lineR.material = neutralLine;
                        }
                    }
                }

                if (!alreadyConnected)
                {

                    var newLineObj = Instantiate<GameObject>(lineObj);
                    newLineObj.transform.parent = this.transform;
                    var newLineR = newLineObj.GetComponent<LineRenderer>();
                    newLineR.positionCount = 2;
                    newLineR.SetPosition(0, node.transform.position - (Vector3.up * 0.1f));
                    newLineR.SetPosition(1, connectedNode.transform.position - (Vector3.up * 0.1f));

                    List<Node> connectedNodes = new List<Node>();
                    connectedNodes.Add(node);
                    connectedNodes.Add(connectedNode);

                    Line newLine;
                    newLine.lineR = newLineR;
                    newLine.connectedNodes = connectedNodes;
                    lines.Add(newLine);
                }
            }
        }
    }

    public void AddNodeToList(Node node)
    {
        nodes.Add(node);
    }

    public void CloseAllNodeMenus(Node exceptionNode)
    {
        foreach(Node node in nodes)
        {
            if(exceptionNode != null && node == exceptionNode)
            {
                continue;
            }

            node.ShowMenu(false);
        }

        if (exceptionNode == null)
        {
            HUDManager.i.SyncMenu(null);
        }
    }

    private void CheckNodeAbilities()
    {
        foreach (Node node in nodes)
        {
            if(node.userInformation.allyStatus != LevelManager.lM.playerAllyFaction && node.userInformation.beliefs.x == 0 && node.userInformation.beliefs.y == 0)
            {
                if(!centristNodes.Contains(node))
                {
                    centristNodes.Add(node);
                }
            }
            else
            {
                if (centristNodes.Contains(node))
                {
                    centristNodes.Remove(node);
                }
            }
        }
    }

    public void CheckNodeConnections()
    {
        foreach(Node node in nodes)
        {
            if(node.userInformation.instigator != AllyStatus.None)
            {
                if (node.userInformation.instigator == AllyStatus.Yellow)
                {
                    node.userInformation.allyStatus = AllyStatus.Yellow;
                }

                if (node.userInformation.instigator == AllyStatus.Red)
                {
                    node.userInformation.allyStatus = AllyStatus.Red;
                }

                if (node.userInformation.instigator == AllyStatus.Green)
                {
                    node.userInformation.allyStatus = AllyStatus.Green;
                }

                if (node.userInformation.instigator == AllyStatus.Blue)
                {
                    node.userInformation.allyStatus = AllyStatus.Blue;
                }

                continue;
            }

            bool yellowAlly = false;
            bool redAlly = false;
            bool greenAlly = false;
            bool blueAlly = false;

            int allyNum = 0;

            foreach (Node connectedNode in node.connectedNodes)
            {
                if (Mathf.Abs(connectedNode.userInformation.beliefs.x - node.userInformation.beliefs.x) + Mathf.Abs(connectedNode.userInformation.beliefs.y - node.userInformation.beliefs.y) < 1.1f)
                {
                    if (connectedNode.userInformation.allyStatus == AllyStatus.Yellow)
                    {
                        if (yellowAlly == false)
                        {
                            yellowAlly = true;
                            allyNum++;
                        }
                    }

                    else if(connectedNode.userInformation.allyStatus == AllyStatus.Red)
                    {
                        if (redAlly == false)
                        {
                            redAlly = true;
                            allyNum++;
                        }
                    }

                    else if(connectedNode.userInformation.allyStatus == AllyStatus.Green)
                    {
                        if (greenAlly == false)
                        {
                            greenAlly = true;
                            allyNum++;
                        }
                    }

                    else if (connectedNode.userInformation.allyStatus == AllyStatus.Blue)
                    {
                        if (blueAlly == false)
                        {
                            blueAlly = true;
                            allyNum++;
                        }
                    }
                }
            }

            if(allyNum == 0 || allyNum > 1)
            {
                node.userInformation.allyStatus = AllyStatus.Neutral;
            }

            else
            {
                if (yellowAlly)
                {
                    node.userInformation.allyStatus = AllyStatus.Yellow;
                }

                if (redAlly)
                {
                    node.userInformation.allyStatus = AllyStatus.Red;
                }

                if (greenAlly)
                {
                    node.userInformation.allyStatus = AllyStatus.Green;
                }

                if (blueAlly)
                {
                    node.userInformation.allyStatus = AllyStatus.Blue;
                }
            }
        }
    }


    [SerializeField] public List<Node> nodes = new List<Node>();
    [SerializeField] List<Line> lines = new List<Line>();
    [SerializeField] private GameObject lineObj;

    private int totalBanned;

    [SerializeField] private Material neutralLine;
    [SerializeField] private Material blueLine;
    [SerializeField] private Material yellowLine;
    [SerializeField] private Material redLine;
    [SerializeField] private Material greenLine;

    [SerializeField] public List<Node> centristNodes = new List<Node>();
}

[System.Serializable]
public struct Line
{
    public LineRenderer lineR;
    public List<Node> connectedNodes;
}

