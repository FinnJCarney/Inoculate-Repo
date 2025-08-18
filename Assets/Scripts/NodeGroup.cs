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
        audioSource = GetComponent<AudioSource>();

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

                    connectedNodes[connectedNodeGroup] = connectedNodeInfo;
                }
            }
        }
    }

    public void UpdatePlayerActions()
    {
        if(nodesInGroup.Count == 0)
        {
            accessRing.color = Color.clear;
            allowanceRing.color = Color.clear;
            return;
        }

        int internalPossibleActions = nodesInGroup.Count - performingActions;
        int externalPossibleActions = 0;

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
                externalPossibleActions += (connectedNodeGroup.nodesInGroup.Count - connectedNodeGroup.performingActions);
            }
        }

        if (menu.activeInHierarchy)
        {
            foreach (NodeGroupButton nGB in buttonList)
            {
                nGB.gameObject.SetActive(nGB.CheckActionAbility(internalPossibleActions, externalPossibleActions));
            }
        }


        if (externalPossibleActions > 0 || (internalPossibleActions > 0 && groupFaction == LevelManager.lM.playerAllyFaction))
        {
            var factionColor = LevelManager.lM.levelFactions[LevelManager.lM.playerAllyFaction].color;
            float amountThrough = Mathf.Sqrt(Time.unscaledTime % 2f);
            allowanceRing.color = Color.Lerp(factionColor, Color.clear, amountThrough);
            allowanceRing.transform.position = Vector3.Lerp(accessRing.transform.position, accessRing.transform.position + Vector3.up, amountThrough);
            allowanceRing.transform.eulerAngles = new Vector3(-90, 0, 0);
            
        }
    
        else
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

    public void ActionResult(ActionType aT, Faction actingFaction, NodeGroup actingNodeGroup, connectionLayer actingLayer, Bleat bleat)
    {

        if (nodesInGroup.Count == 0)
        {
            return;
        }


        bool actionSuccessful = false;

        //Are we inoculating rn? If so, do stuff for that, exit out of the rest of this

        Node_UserInformation nodeToActOn = nodesInGroup[nodesInGroup.Count - 1];

        //if (aT == ActionType.DM)
        //{
        //
        //}
        //
        //if (aT == ActionType.Ban)
        //{
        //
        //}
        //
        //if (aT == ActionType.Connect)
        //{
        //
        //}

        if (aT == ActionType.Left || aT == ActionType.Right || aT == ActionType.Up || aT == ActionType.Down || aT == ActionType.DoubleLeft || aT == ActionType.DoubleRight || aT == ActionType.DoubleUp || aT == ActionType.DoubleDown)
        {
            Vector2 originalBeliefs = nodeToActOn.beliefs;
            Vector2 actionMovement = ActionManager.aM.actionInformation[aT].actionPosition;

            if (LevelManager.lM.CheckValidSpace(originalBeliefs + actionMovement))
            {
                nodeToActOn.beliefs += actionMovement;
                actionSuccessful = true;
            }

            if (nodeToActOn.beliefs != originalBeliefs)
            {
                LevelManager.lM.nodeGroups[originalBeliefs].RemoveNodeFromGroup(nodeToActOn);
                LevelManager.lM.nodeGroups[nodeToActOn.beliefs].AddNodeToGroup(nodeToActOn);
            }
        }
        

        var particleSystemMain = playerPS.main;
        particleSystemMain.startColor = LevelManager.lM.levelFactions[actingFaction].color;
        playerPS.GetComponent<ParticleSystemRenderer>().material = LevelManager.lM.levelFactions[actingFaction].particleMaterial;
        playerPS.GetComponent<ParticleSystemRenderer>().trailMaterial = LevelManager.lM.levelFactions[actingFaction].particleMaterial;
        playerPS.Play();

        if (audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (actionSuccessful)
        {
            bleat.CreateResponse(nodeToActOn);
        }

        audioSource.volume = 0.5f;
        audioSource.loop = false;
        audioSource.pitch = Random.Range(0.85f, 1.15f);
        audioSource.clip = actionComplete;
        audioSource.Play();
    }

    public void SetActionAudio(float amountThrough)
    {
        audioSource.pitch = Mathf.Lerp(actionPitch - 0.75f, actionPitch, amountThrough);
        audioSource.volume = Mathf.Lerp(0f, 0.5f, amountThrough);
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

    [SerializeField] List<NodeGroupButton> buttonList = new List<NodeGroupButton>();

    [SerializeField] ParticleSystem playerPS;
    [SerializeField] ParticleSystem aIPs;

    private AudioSource audioSource;
    [SerializeField] AudioClip actionComplete;
    [SerializeField] AudioClip actionReady;
    private float actionPitch;
}
