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

    private void OnDestroy()
    {
        nM = null;
    }

    public void FlushAllLinesAndNodes()
    {
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            Destroy(lines[i].lineR.gameObject);
            lines.Remove(lines[i]);
        }

        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            nodes.Remove(nodes[i]);
        }
    }

    private void Update()
    {
        if(StateManager.sM == null || LevelManager.lM == null)
        {
            return;
        }

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

            foreach (Node connectedNode in node.connectedNodes)
            {
                bool alreadyConnected = false;

                for (int i = lines.Count - 1; i >= 0; i--)
                {
                    var line = lines[i];

                    if (line.connectedNodes.Contains(node) && line.connectedNodes.Contains(connectedNode))
                    {
                        alreadyConnected = true;

                        if(node.isBanned || connectedNode.isBanned)
                        {
                            Destroy(line.lineR.gameObject);
                            lines.Remove(line);
                        }

                        if (node.userInformation.faction == connectedNode.userInformation.faction)
                        {
                            if (node.userInformation.faction == Faction.DownRight)
                            {
                                line.lineR.material = yellowLine;
                            }

                            else if (node.userInformation.faction == Faction.UpRight)
                            {
                                line.lineR.material = redLine;
                            }

                            else if (node.userInformation.faction == Faction.DownLeft)
                            {
                                line.lineR.material = greenLine;
                            }

                            else if (node.userInformation.faction == Faction.UpLeft)
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
                    if(node.isBanned || connectedNode.isBanned)
                    {
                        continue;
                    }

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
            if(node.userInformation.faction != LevelManager.lM.playerAllyFaction && node.userInformation.beliefs.x == 0 && node.userInformation.beliefs.y == 0)
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
        blueNodes.Clear();
        redNodes.Clear();
        yellowNodes.Clear();
        greenNodes.Clear();
        neutralNodes.Clear();

        foreach(Node node in nodes)
        {
            if(node.userInformation.instigator != Faction.None)
            {
                if (node.userInformation.instigator == Faction.DownRight)
                {
                    node.userInformation.faction = Faction.DownRight;

                    yellowNodes.Add(node);
                }

                if (node.userInformation.instigator == Faction.UpRight)
                {
                    node.userInformation.faction = Faction.UpRight;

                    redNodes.Add(node);
                }

                if (node.userInformation.instigator == Faction.DownLeft)
                {
                    node.userInformation.faction = Faction.DownLeft;

                    greenNodes.Add(node);
                }

                if (node.userInformation.instigator == Faction.UpLeft)
                {
                    node.userInformation.faction = Faction.UpLeft;

                    blueNodes.Add(node);
                }

                continue;
            }

            bool yellowAlly = false;
            bool redAlly = false;
            bool greenAlly = false;
            bool blueAlly = false;

            int allyNum = 0;

            if(node.isBanned)
            {
                node.userInformation.faction = Faction.Neutral;
                continue;
            }


            foreach (Node connectedNode in node.connectedNodes)
            {
                if (Mathf.Abs(connectedNode.userInformation.beliefs.x - node.userInformation.beliefs.x) + Mathf.Abs(connectedNode.userInformation.beliefs.y - node.userInformation.beliefs.y) < 1.1f)
                {
                    if (connectedNode.userInformation.faction == Faction.DownRight)
                    {
                        if (yellowAlly == false)
                        {
                            yellowAlly = true;
                            allyNum++;
                        }
                    }

                    else if(connectedNode.userInformation.faction == Faction.UpRight)
                    {
                        if (redAlly == false)
                        {
                            redAlly = true;
                            allyNum++;
                        }
                    }

                    else if(connectedNode.userInformation.faction == Faction.DownLeft)
                    {
                        if (greenAlly == false)
                        {
                            greenAlly = true;
                            allyNum++;
                        }
                    }

                    else if (connectedNode.userInformation.faction == Faction.UpLeft)
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
                node.userInformation.faction = Faction.Neutral;
                neutralNodes.Add(node);
            }

            else
            {
                if (yellowAlly)
                {
                    node.userInformation.faction = Faction.DownRight;

                    yellowNodes.Add(node);
                }

                else if (redAlly)
                {
                    node.userInformation.faction = Faction.UpRight;

                    redNodes.Add(node);
                }

                if (greenAlly)
                {
                    node.userInformation.faction = Faction.DownLeft;

                    greenNodes.Add(node);
                }

                if (blueAlly)
                {
                    node.userInformation.faction = Faction.UpLeft;

                    blueNodes.Add(node);
                }
            }
        }
    }


    [SerializeField] public List<Node> nodes = new List<Node>();
    [SerializeField] public List<Node> yellowNodes = new List<Node>();
    [SerializeField] public List<Node> redNodes = new List<Node>();
    [SerializeField] public List<Node> greenNodes = new List<Node>();
    [SerializeField] public List<Node> blueNodes = new List<Node>();
    [SerializeField] public List<Node> neutralNodes = new List<Node>();
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

