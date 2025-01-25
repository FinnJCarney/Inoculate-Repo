using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static Node;

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

    private void Start()
    {
        DrawNodeConnectionLines();
    }

    private void DrawNodeConnectionLines()
    {
        foreach(Node node in nodes)
        {

            foreach(Node connectedNode in node.connectedNodes)
            {
                bool alreadyConnected = false;

                foreach(Line line in lines)
                {
                    if(line.connectedNodes.Contains(node) && line.connectedNodes.Contains(connectedNode))
                    {
                        alreadyConnected = true;
                    }
                }

                if (!alreadyConnected)
                {

                    var newLineObj = Instantiate<GameObject>(lineObj);
                    newLineObj.transform.parent = this.transform;
                    var newLineR = newLineObj.GetComponent<LineRenderer>();
                    newLineR.positionCount = 2;
                    newLineR.SetPosition(0, node.transform.position);
                    newLineR.SetPosition(1, connectedNode.transform.position);

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

    public void CloseAllNodeMenus()
    {
        foreach (Node node in nodes)
        {
            node.ShowMenu(false);
        }
    }


    [SerializeField] public List<Node> nodes = new List<Node>();
    [SerializeField] List<Line> lines = new List<Line>();
    [SerializeField] private GameObject lineObj;
}

[System.Serializable]
public struct Line
{
    public LineRenderer lineR;
    public List<Node> connectedNodes;
}

