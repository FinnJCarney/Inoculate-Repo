using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.ShaderData;

public class NodeManager : MonoBehaviour
{
    public static NodeManager nM;

    private void Awake()
    {
        if (nM == null)
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
            Destroy(lines[i].gameObject);
            lines.Remove(lines[i]);
        }

        for (int i = nodes.Count - 1; i >= 0; i--)
        {
            nodes.Remove(nodes[i]);
        }

        nodeFactions.Clear();

        Debug.Log("Node factions count = " + nodeFactions.Count);
    }

    private void Update()
    {
        if (LevelManager.lM == null)
        {
            return;
        }

        //CheckNodeConnections();
        CheckNodeGroupFactions();
        //UpdateLinePositions();
        DrawNodeGroupConnectionLines();
        ManageGameMode(LevelManager.lM.gameMode);
    }

    public void ManageGameMode(GameMode gameMode)
    {
        if (nodes.Count < 1)
        {
            return;
        }

        //Removing instigator check
        /*
        if (Vector2.Distance(LevelManager.lM.playerNode.userInformation.beliefs, LevelManager.lM.levelFactions[LevelManager.lM.playerAllyFaction].mainPosition) > 12f)
        {
            StateManager.sM.GameOver(false);
            return;
        }
        */

        if (LevelManager.lM.gameMode == GameMode.MisinformerHunt)
        {
            int totalMisinformers = 0;
            int totalBanned = 0;

            foreach (Node_UserInformation node in nodes)
            {
                if (node.isBanned)
                {
                    totalBanned++;
                    if (!node.misinformerHori && !node.misinformerVert)
                    {
                        StateManager.sM.GameOver(false);
                        return;
                    }
                }

                if (node.misinformerHori || node.misinformerVert)
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

            foreach (Node_UserInformation node in nodes)
            {
                if (node.faction == LevelManager.lM.playerAllyFaction)
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

            foreach (Node_UserInformation node in nodes)
            {
                if (node.toCapture)
                {
                    nodesRequired++;

                    if (node.faction == LevelManager.lM.playerAllyFaction)
                    {
                        nodesCaptured++;
                    }
                }
            }

            if (nodesRequired == nodesCaptured)
            {
                StateManager.sM.GameOver(true);
            }
        }
    }


    //public void DrawNodeConnectionLines()
    //{
    //    foreach (Node node in nodes)
    //    {
    //        foreach (Node connectedNode in node.userInformation.connectedNodes.Keys)
    //        {
    //            bool alreadyConnected = false;

    //            for (int i = lines.Count - 1; i >= 0; i--)
    //            {
    //                var line = lines[i];

    //                if (line.connectedNodes.Contains(node) && line.connectedNodes.Contains(connectedNode))
    //                {
    //                    alreadyConnected = true;

    //                    if (node.isBanned || connectedNode.isBanned || (node.userInformation.connectedNodes[connectedNode].layer != connectionLayer.onlineOffline && LayerManager.lM.activeLayer != line.connectionLayer))
    //                    {
    //                        for (int j = line.arrows.Count - 1; j >= 0; j--)
    //                        {
    //                            Destroy(line.arrows[j]);
    //                            line.arrows.RemoveAt(j);
    //                        }
    //                        line.arrows.Clear();
    //                        Destroy(line.lineR.gameObject);
    //                        lines.Remove(line);
    //                        continue;
    //                    }

    //                    if (node.userInformation.faction == connectedNode.userInformation.faction)
    //                    {
    //                        if (line.lineFaction != node.userInformation.faction)
    //                        {
    //                            line.lineFaction = node.userInformation.faction;
    //                            line.lineR.material = LevelManager.lM.GiveLineMaterial(line.lineFaction);
    //                            foreach (GameObject arrow in line.arrows)
    //                            {
    //                                arrow.GetComponentInChildren<SpriteRenderer>().color = LevelManager.lM.levelFactions[line.lineFaction].color;
    //                            }
    //                            HUDManager.hM.SyncPoliticalAxes();
    //                        }

    //                    }
    //                    else
    //                    {
    //                        if (line.lineFaction != Faction.Neutral)
    //                        {
    //                            line.lineR.material = neutralLine;
    //                            line.lineFaction = Faction.Neutral;
    //                            foreach (GameObject arrow in line.arrows)
    //                            {
    //                                arrow.GetComponentInChildren<SpriteRenderer>().color = LevelManager.lM.levelFactions[line.lineFaction].color;
    //                            }
    //                            HUDManager.hM.SyncPoliticalAxes();
    //                        }
    //                    }
    //                }

    //                lines[i] = line;
    //            }

    //            if (!alreadyConnected)
    //            {
    //                if (node.isBanned || connectedNode.isBanned)
    //                {
    //                    continue;
    //                }

    //                if (node.userInformation.connectedNodes[connectedNode].layer != connectionLayer.onlineOffline && node.userInformation.connectedNodes[connectedNode].layer != LayerManager.lM.activeLayer)
    //                {
    //                    continue;
    //                }

    //                var newLineObj = Instantiate<GameObject>(lineObj);
    //                newLineObj.transform.parent = this.transform;
    //                var newLineR = newLineObj.GetComponent<LineRenderer>();
    //                newLineR.positionCount = 2;
    //                newLineR.SetPosition(0, node.transform.position - (Vector3.up * 0.25f));
    //                newLineR.SetPosition(1, connectedNode.transform.position - (Vector3.up * 0.25f));

    //                List<Node> connectedNodes = new List<Node>();
    //                connectedNodes.Add(node);
    //                connectedNodes.Add(connectedNode);

    //                Line newLine;
    //                newLine.lineR = newLineR;
    //                newLine.connectedNodes = connectedNodes;
    //                newLine.arrows = new List<GameObject>();

    //                if (node.userInformation.faction == connectedNode.userInformation.faction && node.userInformation.faction != Faction.Neutral)
    //                {
    //                    newLine.lineR.material = LevelManager.lM.GiveLineMaterial(node.userInformation.faction);
    //                    newLine.lineFaction = node.userInformation.faction;
    //                }
    //                else
    //                {
    //                    newLine.lineR.material = neutralLine;
    //                    newLine.lineFaction = Faction.Neutral;
    //                }

    //                bool influencerConnection = node.userInformation.connectedNodes[connectedNode].type == connectionType.influencedBy || node.userInformation.connectedNodes[connectedNode].type == connectionType.influenceOn;

    //                if (influencerConnection)
    //                {
    //                    float lengthOfLine = Vector3.Distance(newLineR.GetPosition(0), newLineR.GetPosition(1));
    //                    int numberOfArrows = Mathf.FloorToInt(lengthOfLine * arrowsPerUnit);
    //                    Vector3 midPoint = (newLineR.GetPosition(0) + newLineR.GetPosition(1)) / 2;

    //                    for (int i = 0; i < numberOfArrows; i++)
    //                    {
    //                        float middleProximity = (((i + 1) / (float)numberOfArrows) - 0.5f);
    //                        var newArrow = Instantiate<GameObject>(arrow);
    //                        newArrow.transform.parent = this.transform;
    //                        newArrow.transform.position = Vector3.MoveTowards(midPoint, newLineR.GetPosition(0), middleProximity * lengthOfLine);

    //                        if (node.userInformation.connectedNodes[connectedNode].type == connectionType.influencedBy)
    //                        {
    //                            newArrow.transform.LookAt(newLineR.GetPosition(0));
    //                        }
    //                        else
    //                        {
    //                            newArrow.transform.LookAt(newLineR.GetPosition(1));
    //                        }

    //                        newLine.arrows.Add(newArrow);

    //                    }
    //                }

    //                newLine.connectionLayer = node.userInformation.connectedNodes[connectedNode].layer;

    //                lines.Add(newLine);
    //            }
    //        }
    //    }
    //}

    //private void UpdateLinePositions()
    //{
    //    foreach (Line line in lines)
    //    {
    //        line.lineR.SetPosition(0, line.connectedNodes[0].transform.position - (Vector3.up * 0.25f));
    //        line.lineR.SetPosition(1, line.connectedNodes[1].transform.position - (Vector3.up * 0.25f));
    //    }
    //}

    public void AddNodeToList(Node_UserInformation node)
    {
        nodes.Add(node);
        Sprite newSprite = faceSprites[Mathf.RoundToInt(Random.Range(0, faceSprites.Length - 1))];
        node.nodeCore.nodeVisual.sprite = newSprite;
        node.NodeImage = newSprite;

        if(!nodeFactions.ContainsKey(node.faction))
        {
            NodeManager.nM.AddNewNodeFaction(node.faction);
        }

        nodeFactions[node.faction].Add(node);
    }

    public void CloseAllNodeMenus(Node exceptionNode)
    {
        foreach (Node_UserInformation node in nodes)
        {
            if (exceptionNode != null && node == exceptionNode)
            {
                continue;
            }

            node.nodeCore.ShowMenu(false);
        }

        if (exceptionNode == null)
        {
            HUDManager.hM.SyncMenu(null);
        }
    }

    public void CloseAllNodeGroupMenus(NodeGroup exceptionNodeGroup)
    {
        foreach (NodeGroup nodeGroup in LevelManager.lM.nodeGroups.Values)
        {
            if (exceptionNodeGroup != null && exceptionNodeGroup == nodeGroup)
            {
                continue;
            }

            nodeGroup.ShowMenu(false);
        }
    }

    public void CheckNodeConnections()
    {
        foreach (Faction faction in nodeFactions.Keys)
        {
            nodeFactions[faction].Clear();
        }

        foreach (Node_UserInformation node in nodes)
        {
            if (node.isBanned)
            {
                node.faction = Faction.Neutral;
                continue;
            }

            foreach (Node_UserInformation connectedNode in node.connectedNodes.Keys)
            {
                if (!connectedNode.connectedNodes.ContainsKey(node))
                {
                    Debug.LogWarning(node + "is not properly connected to " + connectedNode);
                    continue;
                }

                if (connectedNode.connectedNodes[node].type != node.connectedNodes[connectedNode].type)
                {
                    if (!(connectedNode.connectedNodes[node].type == connectionType.influencedBy && node.connectedNodes[connectedNode].type == connectionType.influenceOn || connectedNode.connectedNodes[node].type == connectionType.influenceOn && node.connectedNodes[connectedNode].type == connectionType.influencedBy))
                    {
                        Debug.LogWarning(node + " has a connection type discrepancy with " + connectedNode);
                        continue;
                    }
                }
            }

            List<Faction> possibleAlliedFactions = new List<Faction>();

            foreach (Faction lFac in LevelManager.lM.levelFactions.Keys)
            {
                if (lFac == Faction.Neutral)
                { 
                    continue;
                }
                
                foreach(Vector2 factionPosition in LevelManager.lM.levelFactions[lFac].positions)
                {
                    if(Vector2.Distance(node.beliefs, factionPosition) < 12.1f)
                    {
                        possibleAlliedFactions.Add(lFac);
                        break;
                    }
                }
            }

            Faction possibleAlliedFaction = Faction.Neutral;

            if (possibleAlliedFactions.Count == 1)
            {
                possibleAlliedFaction = possibleAlliedFactions[0];
            }


            //else if (possibleAlliedFaction != Faction.Neutral)
            //{
            //    if (!CheckIfConnectedToInstigator(node, possibleAlliedFaction))
            //    {
            //        possibleAlliedFaction = Faction.Neutral;
            //    }
            //}

            if(node.faction != possibleAlliedFaction)
            {
                RemoveNodePosition(node.faction, node.beliefs);
            }

            node.faction = possibleAlliedFaction;
            nodeFactions[possibleAlliedFaction].Add(node);
            AddNodePosition(possibleAlliedFaction, node.beliefs);
        }
    }

    public void CheckNodeGroupFactions()
    {
        foreach (Faction faction in nodeFactions.Keys)
        {
            nodeFactions[faction].Clear();
        }

        for (int i = 0; i < 3; i++)
        {
            Dictionary<NodeGroup, List<Faction>> possibleAlliedFactionsPerNode = new Dictionary<NodeGroup, List<Faction>>();

            foreach (NodeGroup nodeGroup in LevelManager.lM.nodeGroups.Values)
            {
                if (nodeGroup.nodesInGroup.Count == 0)
                {
                    nodeGroup.groupFaction = Faction.Neutral;
                    AddNodePosition(Faction.Neutral, nodeGroup.groupBelief);
                    continue;
                }

                List<NodeGroup> neighbouringNodeGroups = LevelManager.lM.ProvideNeighbouringNodeGroups(nodeGroup.groupBelief);

                List<Faction> possibleFactions = new List<Faction>();

                foreach (Faction lFac in LevelManager.lM.levelFactions.Keys)
                {
                    if (lFac == Faction.Neutral)
                    {
                        continue;
                    }

                    if (Vector2.Distance(nodeGroup.groupBelief, LevelManager.lM.levelFactions[lFac].mainPosition) < 13f)
                    {
                        if (!possibleFactions.Contains(lFac))
                        {
                            possibleFactions.Add(lFac);
                        }
                        break;
                    }

                    else
                    {

                        foreach (NodeGroup neighbouringNodeGroup in neighbouringNodeGroups)
                        {
                            if (neighbouringNodeGroup.groupFaction == lFac)
                            {
                                possibleFactions.Add(lFac);
                                break;
                            }
                        }
                    }
                }

                possibleAlliedFactionsPerNode.Add(nodeGroup, possibleFactions);
                
            }

            foreach (NodeGroup nodeGroup in possibleAlliedFactionsPerNode.Keys)
            {
                Faction possibleAlliedFaction = Faction.Neutral;

                if (possibleAlliedFactionsPerNode[nodeGroup].Count == 1)
                {
                    possibleAlliedFaction = possibleAlliedFactionsPerNode[nodeGroup].ToArray()[0];
                }

                if (nodeGroup.groupFaction != possibleAlliedFaction)
                {
                    RemoveNodePosition(nodeGroup.groupFaction, nodeGroup.groupBelief);
                }

                nodeGroup.groupFaction = possibleAlliedFaction;
                AddNodePosition(possibleAlliedFaction, nodeGroup.groupBelief);

                foreach (Node_UserInformation node in nodeGroup.nodesInGroup)
                {
                    node.faction = possibleAlliedFaction;
                    nodeFactions[possibleAlliedFaction].Add(node);
                }
            }
        }
    }
            
    

    public void DrawNodeGroupConnectionLines()
    {
        for (int i = lines.Count - 1; i >= 0; i--)
        {
            if (lines[i].connectedNodeGroups[0].nodesInGroup.Count == 0 || lines[i].connectedNodeGroups[1].nodesInGroup.Count == 0)
            {
                lines[i].Adjust(false, Faction.Neutral);
                lines.RemoveAt(i);
            }
        } 

        foreach (NodeGroup nodeGroup in LevelManager.lM.nodeGroups.Values)
        { 
            if(nodeGroup.nodesInGroup.Count == 0)
            {
                continue;
            }

            foreach (NodeGroup connectedNodeGroup in nodeGroup.connectedNodes.Keys)
            {
                if (connectedNodeGroup.nodesInGroup.Count == 0)
                {
                    continue;
                }

                bool alreadyConnected = false;

                foreach (ConnectionLine line in lines)
                {   
                    if (line.connectedNodeGroups.Contains(nodeGroup) && line.connectedNodeGroups.Contains(connectedNodeGroup))
                    {
                        alreadyConnected = true; //If lines are connected, make necessary adjustments
                        if (nodeGroup.groupFaction == connectedNodeGroup.groupFaction)
                        {
                            line.Adjust(true, nodeGroup.groupFaction);
                        }

                        else
                        {
                            line.Adjust(true, Faction.Neutral);
                        }
                    }
                }

                if (!alreadyConnected) //If lines arn't connected, make a new line and set it up
                {
                    var newLineObj = Instantiate<GameObject>(lineObj, this.transform);
                    ConnectionLine newLine = newLineObj.GetComponent<ConnectionLine>();
                    lines.Add(newLine);
                    
                    if(nodeGroup.groupFaction == connectedNodeGroup.groupFaction)
                    {
                        newLine.Setup(nodeGroup.groupFaction, nodeGroup, connectedNodeGroup);
                    }

                    else
                    {
                        newLine.Setup(Faction.Neutral, nodeGroup, connectedNodeGroup);
                    }

                }
            }
        }
    }

    private void AddNodePosition(Faction faction, Vector2 position)
    {
        if (!LevelManager.lM.levelFactions[faction].positions.Contains(position))
        {
            LevelManager.lM.levelFactions[faction].positions.Add(position);
            LevelManager.lM.CheckFactionSpaces();
            LevelManager.lM.UpdateFactionGrid();
        }
    }

    private void RemoveNodePosition(Faction faction, Vector2 position)
    {
        if (LevelManager.lM.levelFactions[faction].positions.Contains(position))
        {
            LevelManager.lM.levelFactions[faction].positions.Add(position);
            LevelManager.lM.CheckFactionSpaces();
            LevelManager.lM.UpdateFactionGrid();
        }
    }

    public void AddNodeFactions()
    {
        if (nodeFactions.Count == 0)
        {
            Debug.Log("Node Factions = 0");
            foreach (Faction faction in LevelManager.lM.levelFactions.Keys)
            {
                nodeFactions.Add(faction, null);
                nodeFactions[faction] = new List<Node_UserInformation>();
                Debug.Log("Adding faction " + faction);
            }
        }
    }

    public void AddNewNodeFaction(Faction faction)
    {
        nodeFactions.Add(faction, null);
        nodeFactions[faction] = new List<Node_UserInformation>();
        Debug.Log("Adding faction " + faction);
    }


    [SerializeField] public List<Node_UserInformation> nodes = new List<Node_UserInformation>();

    [SerializeField] public SerializableDictionary<Faction, List<Node_UserInformation>> nodeFactions = new SerializableDictionary<Faction, List<Node_UserInformation>>();

    [SerializeField] public List<ConnectionLine> lines = new List<ConnectionLine>();
    [SerializeField] private GameObject lineObj;

    private int totalBanned;

    [SerializeField] private Material neutralLine;

    [SerializeField] public List<Node> centristNodes = new List<Node>();

    [SerializeField] private Sprite[] faceSprites;

    [SerializeField] GameObject arrow;
    [SerializeField] float arrowsPerUnit;
}

[System.Serializable]
public struct Line
{
    public LineRenderer lineR;
    public Faction lineFaction;
    public List<Node> connectedNodes;
    public List<GameObject> arrows;
}

