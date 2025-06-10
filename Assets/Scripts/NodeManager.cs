using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

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

        nodeFactions.Clear();
    }

    private void Update()
    {
        if(StateManager.sM == null || LevelManager.lM == null)
        {
            return;
        }

        if (nodeFactions.Count == 0)
        {
            foreach(Faction faction in LevelManager.lM.levelFactions.Keys)
            {
                nodeFactions.Add(faction, null);
                nodeFactions[faction] = new List<Node>();
            }
        }

        CheckNodeConnections();
        CheckNodeAbilities();
        DrawNodeConnectionLines();
        ManageGameMode(LevelManager.lM.gameMode);
    }

    public void ManageGameMode(GameMode gameMode)
    {

        if (Vector2.Distance(LevelManager.lM.playerNode.userInformation.beliefs, LevelManager.lM.levelFactions[LevelManager.lM.playerAllyFaction].position) > 1.5f)
        {
            StateManager.sM.GameOver(false);
            return;
        }

        if (LevelManager.lM.gameMode == GameMode.MisinformerHunt)
        {
            int totalMisinformers = 0;
            int totalBanned = 0;

            foreach (Node node in nodes)
            {
                if (node.isBanned)
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

            if (totalBanned == totalMisinformers)
            {
                StateManager.sM.GameOver(true);
            }
        }

        else if (LevelManager.lM.gameMode == GameMode.MapControl)
        {
            int nodesInPlayerFaction = 0;

            foreach (Node node in nodes)
            {
                if (node.userInformation.faction == LevelManager.lM.playerAllyFaction)
                {
                    nodesInPlayerFaction++;
                }
            }

            if (nodesInPlayerFaction > nodes.Count * LevelManager.lM.amountRequiredForControl)
            {
                StateManager.sM.GameOver(true);
            }
        }

        else if (LevelManager.lM.gameMode == GameMode.SpecificNodeCapture)
        {
            int nodesRequired = 0;
            int nodesCaptured = 0;

            foreach (Node node in nodes)
            {
                if (node.userInformation.toCapture)
                {
                    nodesRequired++;

                    if (node.userInformation.faction == LevelManager.lM.playerAllyFaction)
                    {
                        nodesCaptured++;
                    }
                }
            }

            if(nodesRequired == nodesCaptured)
            {
                StateManager.sM.GameOver(true);
            }
        }
    }


    public void DrawNodeConnectionLines()
    {
        foreach (Node node in nodes)
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

                        if (node.isBanned || connectedNode.isBanned)
                        {
                            Destroy(line.lineR.gameObject);
                            lines.Remove(line);
                            continue;
                        }

                        if (node.userInformation.faction == connectedNode.userInformation.faction && Vector2.Distance(node.userInformation.beliefs, connectedNode.userInformation.beliefs) < 1.1f)
                        {
                            if(line.lineFaction != node.userInformation.faction)
                            {
                                line.lineR.material = LevelManager.lM.GiveLineMaterial(node.userInformation.faction);
                                line.lineFaction = node.userInformation.faction;
                                HUDManager.i.SyncPoliticalAxes();
                            }
                            
                        }
                        else
                        {
                            if (line.lineFaction != Faction.Neutral)
                            {
                                line.lineR.material = neutralLine;
                                line.lineFaction = Faction.Neutral;
                                HUDManager.i.SyncPoliticalAxes();
                            }
                        }
                    }

                    lines[i] = line;
                }

                if (!alreadyConnected)
                {
                    if (node.isBanned || connectedNode.isBanned)
                    {
                        continue;
                    }

                    var newLineObj = Instantiate<GameObject>(lineObj);
                    newLineObj.transform.parent = this.transform;
                    var newLineR = newLineObj.GetComponent<LineRenderer>();
                    newLineR.positionCount = 2;
                    newLineR.SetPosition(0, node.transform.position - (Vector3.up * 0.25f));
                    newLineR.SetPosition(1, connectedNode.transform.position - (Vector3.up * 0.25f));

                    List<Node> connectedNodes = new List<Node>();
                    connectedNodes.Add(node);
                    connectedNodes.Add(connectedNode);

                    Line newLine;
                    newLine.lineR = newLineR;
                    newLine.connectedNodes = connectedNodes;

                    if (node.userInformation.faction == connectedNode.userInformation.faction && node.userInformation.faction != Faction.Neutral && Vector2.Distance(node.userInformation.beliefs, connectedNode.userInformation.beliefs) < 1.1f)
                    {
                        newLine.lineR.material = LevelManager.lM.GiveLineMaterial(node.userInformation.faction);
                        newLine.lineFaction = node.userInformation.faction;
                    }
                    else
                    {
                        newLine.lineR.material = neutralLine;
                        newLine.lineFaction = Faction.Neutral;
                    }

                    lines.Add(newLine);
                }
            }
        }
    }

    public void AddNodeToList(Node node)
    {
        nodes.Add(node);
        node.nodeVisual.sprite = faceSprites[Mathf.RoundToInt(Random.Range(0, faceSprites.Length - 1))];
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
        foreach (Faction faction in nodeFactions.Keys)
        {
            nodeFactions[faction].Clear();
        }

        foreach (Node node in nodes)
        {
            if (node.userInformation.instigator != Faction.None && nodeFactions.ContainsKey(node.userInformation.instigator))
            {
                if(Vector2.Distance(node.userInformation.beliefs, LevelManager.lM.levelFactions[node.userInformation.instigator].position) > 1.5f)
                {
                    node.userInformation.instigator = Faction.None;
                }
                else
                {
                    node.userInformation.faction = node.userInformation.instigator;
                    nodeFactions[node.userInformation.faction].Add(node);

                    continue;
                }
            }

            if (node.isBanned)
            {
                node.userInformation.faction = Faction.Neutral;
                continue;
            }

            List<Node> alliedNodes = new List<Node>();

            foreach (Node connectedNode in node.connectedNodes)
            {
                if (connectedNode.userInformation.faction != Faction.Neutral && Vector2.Distance(connectedNode.userInformation.beliefs, node.userInformation.beliefs) < 1.1f)
                {
                    alliedNodes.Add(connectedNode);
                }
            }

            Faction possibleAlliedFaction = Faction.None;

            if (alliedNodes.Count > 0)
            {
                foreach (Node alliedNode in alliedNodes)
                {
                    if (alliedNode.userInformation.faction == Faction.Neutral)
                    {
                        continue;
                    }

                    if (possibleAlliedFaction == Faction.None)
                    {
                        possibleAlliedFaction = alliedNode.userInformation.faction;
                        continue;
                    }

                    if(possibleAlliedFaction != alliedNode.userInformation.faction)
                    {
                        possibleAlliedFaction = Faction.Neutral;
                        break;
                    }
                }
            }

            if (possibleAlliedFaction == Faction.None)
            {
                possibleAlliedFaction = Faction.Neutral;
            }

            else if (possibleAlliedFaction != Faction.Neutral)
            {
                if (!CheckIfConnectedToInstigator(node, possibleAlliedFaction))
                {
                    possibleAlliedFaction = Faction.Neutral;
                }
            }

            node.userInformation.faction = possibleAlliedFaction;
            nodeFactions[possibleAlliedFaction].Add(node);

        }
    }

    public bool CheckIfConnectedToInstigator(Node node, Faction faction)
    {
        List<Node> nodesToCheck = new List<Node>();

        List<Node> nodesChecked = new List<Node>();

        nodesToCheck.Add(node);

        foreach(Node connectedNode in node.connectedNodes)
        {
            if (Vector2.Distance(node.userInformation.beliefs, connectedNode.userInformation.beliefs) < 1.1f)
            {
                nodesToCheck.Add(connectedNode);
            }
        }

        for (int i = 0; i < nodesToCheck.Count; i++)
        {
            if (nodesToCheck[i].userInformation.faction != faction)
            {
                continue;
            }

            if(nodesToCheck[i].userInformation.instigator == faction)
            {
                return true;
            }

            nodesChecked.Add(nodesToCheck[i]);

            foreach(Node nodeToAdd in nodesToCheck[i].connectedNodes)
            {
                if(!nodesChecked.Contains(nodeToAdd) && Vector2.Distance(nodesToCheck[i].userInformation.beliefs, nodeToAdd.userInformation.beliefs) < 1.1f)
                {
                    nodesToCheck.Add(nodeToAdd);
                }
            }
        }

        return false;
    }


    [SerializeField] public List<Node> nodes = new List<Node>();

    [SerializeField] public Dictionary<Faction, List<Node>> nodeFactions = new Dictionary<Faction, List<Node>>();

    [SerializeField] public List<Line> lines = new List<Line>();
    [SerializeField] private GameObject lineObj;

    private int totalBanned;

    [SerializeField] private Material neutralLine;

    [SerializeField] public List<Node> centristNodes = new List<Node>();

    [SerializeField] private Sprite[] faceSprites;
}

[System.Serializable]
public struct Line
{
    public LineRenderer lineR;
    public Faction lineFaction;
    public List<Node> connectedNodes;
}

