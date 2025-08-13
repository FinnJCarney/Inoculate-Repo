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

        UpdateNodeInfo();
    }

    public void RemoveNodeFromGroup(Node_UserInformation newNode)
    {
        if (nodesInGroup.Contains(newNode))
        {
            nodesInGroup.Remove(newNode);
        }

        UpdateNodeInfo();
    }

    private void Update()
    {
        UpdateNodeInfo();
        UpdatePlayerActions();
    }

    public void ShowMenu(bool show)
    {
        menu.SetActive(show);
    }


    public void UpdateNodeInfo()
    {
        connectedNodes.Clear();

        foreach(Node_UserInformation nodeUser in nodesInGroup)
        {
            foreach(Node connectedNodeCore in nodeUser.connectedNodes.Keys) //Change to UserInformation when things are more implemented
            {
                Node_UserInformation connectedNode = connectedNodeCore.userInformation; // Should be able to get rid of this line

                if(!connectedNodes.ContainsKey(connectedNode))
                {
                    connectedNodes.Add(connectedNode, nodeUser.connectedNodes[connectedNode]);
                }
                else
                {
                    connectedNodeInfo connectedNodeInfo;
                    connectedNodeInfo.layer = connectedNodes[connectedNode].layer;
                    connectedNodeInfo.type = connectedNodes[connectedNode].type;

                    if (connectedNodes[connectedNode].type != nodeUser.connectedNodes[connectedNode.nodeCore].type)
                    {
                        if(connectedNodes[connectedNode].type == connectionType.influencedBy && nodeUser.connectedNodes[connectedNode.nodeCore].type != connectionType.influencedBy)
                        {
                            connectedNodeInfo.type = nodeUser.connectedNodes[connectedNode.nodeCore].type;
                        }

                        if (connectedNodes[connectedNode].type == connectionType.influenceOn && nodeUser.connectedNodes[connectedNode.nodeCore].type == connectionType.mutual)
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
    
        foreach (Node_UserInformation connectedNode in connectedNodes.Keys)
        {
            if (connectedNode.nodeCore.isBanned)
            {
                continue;
            }
    
            if (connectedNodes[connectedNode].layer != connectionLayer.onlineOffline && connectedNodes[connectedNode].layer != LayerManager.lM.activeLayer)
            {
                continue;
            }
    
            if (connectedNodes[connectedNode].type == connectionType.influenceOn || connectedNode.connectedNodes[connectedNode.nodeCore].type == connectionType.influencedBy)
            {
                continue;
            }
    
            if (connectedNode.faction == LevelManager.lM.playerAllyFaction)
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
            ActionManager.aM.NewPerformButtonAction(aT, nodesInGroup[nodesInGroup.Count - 1]);
        }
    }

    public Vector2 groupBelief;
    public Faction groupFaction = Faction.Neutral;

    [SerializeField] public List<Node_UserInformation> nodesInGroup = new List<Node_UserInformation>();
    public SerializableDictionary<Node_UserInformation, connectedNodeInfo> connectedNodes = new SerializableDictionary<Node_UserInformation, connectedNodeInfo>();

    public int nodeGroupPrio;
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
