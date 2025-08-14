using DG.Tweening;
using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class NodeGroup : MonoBehaviour
{
    void Start()
    {
        groupBelief = new Vector2(this.transform.position.x, this.transform.position.z);

        menu.SetActive(false);
    }

    public void AddNodeToGroup(Node_UserInformation newNode)
    {
        if(!nodesInGroup.Contains(newNode))
        {
            nodesInGroup.Add(newNode);
        }

        ArrangeNodes();
        UpdateNodeInfo();
    }

    public void RemoveNodeFromGroup(Node_UserInformation newNode)
    {
        if (nodesInGroup.Contains(newNode))
        {
            nodesInGroup.Remove(newNode);
        }

        ArrangeNodes();
        UpdateNodeInfo();
    }

    private void ArrangeNodes()
    {
        foreach (Node_UserInformation node in nodesInGroup)
        {
            int i = nodesInGroup.IndexOf(node);
            float posY = (nodesInGroup.Count - 1 - i) * 1.5f;
            node.transform.DOKill();
            node.transform.DOMove(new Vector3(node.beliefs.x, posY, node.beliefs.y), 0.5f);
        }
    }

    private void Update()
    {
        UpdateNodeInfo();
        UpdatePlayerActions();
    }

    public void ShowMenu(bool show)
    {
        if (nodesInGroup.Count == 0)
        {
            menu.SetActive(false);
        }
        else
        {
            menu.SetActive(show);
            if (show)
            {
                NodeManager.nM.CloseAllNodeGroupMenus(this);
            }
        }
    }

    public void UpdateNodeInfo()
    {
        connectedNodes.Clear();

        foreach(Node_UserInformation nodeUser in nodesInGroup)
        {
            foreach(Node connectedNodeCore in nodeUser.connectedNodes.Keys) //Change to UserInformation when things are more implemented
            {
                Node_UserInformation connectedNode = connectedNodeCore.userInformation; // Should be able to get rid of this line
                NodeGroup connectedNodeGroup = LevelManager.lM.nodeGroups[connectedNode.beliefs];

                if (!connectedNodes.ContainsKey(connectedNodeGroup))
                {
                    connectedNodes.Add(connectedNodeGroup, nodeUser.connectedNodes[connectedNode.nodeCore]);
                }
                else
                {
                    connectedNodeInfo connectedNodeInfo;
                    connectedNodeInfo.layer = connectedNodes[connectedNodeGroup].layer;
                    connectedNodeInfo.type = connectedNodes[connectedNodeGroup].type;

                    if (connectedNodes[connectedNodeGroup].type != nodeUser.connectedNodes[connectedNode.nodeCore].type)
                    {
                        if(connectedNodes[connectedNodeGroup].type == connectionType.influencedBy && nodeUser.connectedNodes[connectedNode.nodeCore].type != connectionType.influencedBy)
                        {
                            connectedNodeInfo.type = nodeUser.connectedNodes[connectedNode.nodeCore].type;
                        }

                        if (connectedNodes[connectedNodeGroup].type == connectionType.influenceOn && nodeUser.connectedNodes[connectedNode.nodeCore].type == connectionType.mutual)
                        {
                            connectedNodeInfo.type = connectionType.mutual;
                        }
                    }

                    connectedNodes[connectedNode] = connectedNodeInfo;
                }
            }
        }
    }

    public void UpdatePlayerActions()
    {
        int possibleActions = 0;

        bool allyNeighbourAvail = false;

        bool leftActionAvail = false;
        bool rightActionAvail = false;
        bool upActionAvail = false;
        bool downActionAvail = false;
        bool inoculateActionAvail = false;

        if(nodesInGroup.Count == 0)
        {
            accessRing.color = Color.clear;
            allowanceRing.color = Color.clear;
            return;
        }
    
        foreach (NodeGroup connectedNodeGroup in connectedNodes.Keys)
        {
    
            if (connectedNodes[connectedNodeGroup].layer != connectionLayer.onlineOffline && connectedNodes[connectedNodeGroup].layer != LayerManager.lM.activeLayer)
            {
                continue;
            }
    
            if (connectedNodes[connectedNodeGroup].type == connectionType.influenceOn)
            {
                continue;
            }
    
            if (connectedNodeGroup.groupFaction == LevelManager.lM.playerAllyFaction)
            {
                possibleActions++;
                allyNeighbourAvail = true;
            }
        }
    
        if (allyNeighbourAvail)
        {
            Vector2 movement = new Vector2(12f, 12f);
            upActionAvail = LevelManager.lM.CheckValidSpace(groupBelief + (Vector2.up * movement));

            downActionAvail = LevelManager.lM.CheckValidSpace(groupBelief + (Vector2.down * movement));
    
            rightActionAvail = LevelManager.lM.CheckValidSpace(groupBelief + (Vector2.right * movement));
    
            leftActionAvail = LevelManager.lM.CheckValidSpace(groupBelief + (Vector2.left * movement));
        }

        if (nodesInGroup.Count > 1 && nodesInGroup.Count > performingActions && groupFaction == LevelManager.lM.playerAllyFaction)
        {
            inoculateActionAvail = true;
        }

        but_Left.gameObject.SetActive(leftActionAvail);
        but_Right.gameObject.SetActive(rightActionAvail);
        but_Up.gameObject.SetActive(upActionAvail);
        but_Down.gameObject.SetActive(downActionAvail);
        but_Inoculate.gameObject.SetActive(inoculateActionAvail);

    
        if (possibleActions > 0)
        {
            var factionColor = LevelManager.lM.levelFactions[LevelManager.lM.playerAllyFaction].color;
            float amountThrough = Mathf.Sqrt(Time.unscaledTime % 2f);
            allowanceRing.color = Color.Lerp(factionColor, Color.clear, amountThrough);
            allowanceRing.transform.position = Vector3.Lerp(accessRing.transform.position, accessRing.transform.position + Vector3.up, amountThrough);
            allowanceRing.transform.eulerAngles = new Vector3(-90, 0, 0);
            
        }
    
        else if (possibleActions == 0)
        {
            allowanceRing.color = Color.clear;
        }
    
        accessRing.color = LevelManager.lM.levelFactions[groupFaction].color;
    }

    public void PerformButtonAction(ActionType aT)
    {
        if (nodesInGroup.Count != 0)
        {
            ActionManager.aM.PerfromGroupButtonAction(aT, this);
        }
    }

    public Vector2 groupBelief;
    public Faction groupFaction = Faction.Neutral;

    [SerializeField] public List<Node_UserInformation> nodesInGroup = new List<Node_UserInformation>();
    public SerializableDictionary<NodeGroup, connectedNodeInfo> connectedNodes = new SerializableDictionary<NodeGroup, connectedNodeInfo>();

    public int prio;
    public int performingActions;
    public int receivingActions;

    [SerializeField] public SpriteRenderer nodeVisual;

    [SerializeField] private SpriteRenderer accessRing;
    [SerializeField] private SpriteRenderer allowanceRing;

    [SerializeField] GameObject menu;
    [SerializeField] TextMeshPro handleText;
    [SerializeField] GameObject bannedCover;

    [SerializeField] NodeGroupButton but_Left;
    [SerializeField] NodeGroupButton but_Right;
    [SerializeField] NodeGroupButton but_Up;
    [SerializeField] NodeGroupButton but_Down;
    [SerializeField] NodeGroupButton but_Inoculate;

    [SerializeField] ParticleSystem playerPS;
    [SerializeField] ParticleSystem aIPs;

    private AudioSource audioSource;
    [SerializeField] AudioClip actionComplete;
    [SerializeField] AudioClip actionReady;
    private float actionPitch;
}
