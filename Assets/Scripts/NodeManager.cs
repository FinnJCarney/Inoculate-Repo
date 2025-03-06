using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
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
        if(!drawnLines)
        {
            DrawNodeConnectionLines();
            drawnLines = true;
        }

        int totalMisinformers = 0;
        int totalBanned = 0;

        foreach(Node node in nodes)
        {
            if(node.isBanned)
            {
                totalBanned++;
                if (!node.misinformerClimateChange && !node.misinformerMinorityRights && !node.misinformerWealthInequality)
                {
                    StateManager.sM.GameOver(false);
                    return;
                }
            }

            if (node.misinformerClimateChange || node.misinformerMinorityRights || node.misinformerWealthInequality)
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
        if(lines.Count > 0)
        {
            for (int i = lines.Count - 1; i >= 0; i--)
            {
                Destroy(lines[i].lineR.gameObject);
                lines.Remove(lines[i]);
            }
        }
        foreach(Node node in nodes)
        {
            foreach(Node connectedNode in node.connectedNodes)
            {
                bool alreadyConnected = false;
                
                if(node.isBanned || connectedNode.isBanned)
                {
                    continue;
                }

                foreach (Line line in lines)
                {
                    if (line.connectedNodes.Contains(node) && line.connectedNodes.Contains(connectedNode))
                    {
                        alreadyConnected = true;
                        line.lineR.material = twoWayLine;
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

    private bool drawnLines = false;

    private int totalBanned;

    [SerializeField] private Material twoWayLine;
}

[System.Serializable]
public struct Line
{
    public LineRenderer lineR;
    public List<Node> connectedNodes;
}

