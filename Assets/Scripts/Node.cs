using System.Collections;
using System.Collections.Generic;
using System.Net;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Node : MonoBehaviour
{
    private void Start()
    {
        userInformation = GetComponent<Node_UserInformation>();

        nodeHandle = userInformation.NodeName != "" ? userInformation.NodeName : this.gameObject.name;

        NodeManager.nM.AddNodeToList(this);

        audioSource = GetComponent<AudioSource>();

        handleText.text = nodeHandle;

        menu.SetActive(false);
        bannedCover.SetActive(isBanned);

        actionPitch = Random.Range(1.25f, 1.5f);
    }

    private void Update()
    {
        nodeVisual.color = Color.Lerp(LevelManager.lM.GiveAverageColor(userInformation.beliefs), Color.white, 0.5f);

        int theorheticalActions = 0;
        int possibleActions = 0;

        bool hasLeftNeighbourAvail = false;
        bool hasRightNeighbourAvail = false;
        bool hasUpNeighbourAvail = false;
        bool hasDownNeighbourAvail = false;

        bool hasAllyNeighbourAvail = false;

        foreach (Node connectedNode in userInformation.connectedNodes.Keys)
        {
            if (connectedNode.isBanned)
            {
                continue;
            }

            if(userInformation.connectedNodes[connectedNode].layer != connectionLayer.onlineOffline && userInformation.connectedNodes[connectedNode].layer != LayerManager.lM.activeLayer)
            {
                continue;
            }

            if(userInformation.connectedNodes[connectedNode].type == connectionType.influenceOn || connectedNode.userInformation.connectedNodes[this].type == connectionType.influencedBy)
            {
                continue;
            }

            if(connectedNode.performingAction)
            {
                theorheticalActions++;
                continue;
            }

            if (connectedNode.userInformation.faction == LevelManager.lM.playerAllyFaction)
            {
                theorheticalActions++;
                possibleActions ++;
                hasAllyNeighbourAvail = true;

                if (connectedNode.userInformation.beliefs.x == this.userInformation.beliefs.x)
                {
                    hasLeftNeighbourAvail = true;
                    hasRightNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.x > this.userInformation.beliefs.x)
                {
                    hasRightNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.x < this.userInformation.beliefs.x)
                {
                    hasLeftNeighbourAvail = true;
                }

                if (connectedNode.userInformation.beliefs.y == this.userInformation.beliefs.y)
                {
                    hasUpNeighbourAvail = true;
                    hasDownNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.y > this.userInformation.beliefs.y)
                {
                    hasUpNeighbourAvail = true;
                }
                else if (connectedNode.userInformation.beliefs.y < this.userInformation.beliefs.y)
                {
                    hasDownNeighbourAvail = true;
                }
            }

            if (hasUpNeighbourAvail) //Main Up
            {
                hasUpNeighbourAvail = HUDManager.i.IsSpaceValid(userInformation.beliefs + Vector2.up);
            }

            if (hasDownNeighbourAvail) //Main Down
            {
                hasDownNeighbourAvail = HUDManager.i.IsSpaceValid(userInformation.beliefs + Vector2.down);
            }

            if (hasRightNeighbourAvail) //Main Right
            {
                hasRightNeighbourAvail = HUDManager.i.IsSpaceValid(userInformation.beliefs + Vector2.right);
            }

            if (hasLeftNeighbourAvail) //Main Left
            {
                hasLeftNeighbourAvail = HUDManager.i.IsSpaceValid(userInformation.beliefs + Vector2.left);
            }
        }

        but_DM.EnableButton(userInformation.userInfoHidden && hasAllyNeighbourAvail);
        but_Accuse.EnableButton(!userInformation.userInfoHidden && hasAllyNeighbourAvail);
        
        but_Left.EnableButton(hasLeftNeighbourAvail && !userInformation.userInfoHidden);
        but_Right.EnableButton(hasRightNeighbourAvail && !userInformation.userInfoHidden);
        but_Up.EnableButton(hasUpNeighbourAvail && !userInformation.userInfoHidden);
        but_Down.EnableButton(hasDownNeighbourAvail && !userInformation.userInfoHidden);

        bool connectActionAvailable = false;

        if(!userInformation.userInfoHidden && NodeManager.nM.centristNodes.Count > 0)
        {
            foreach(Node centristNode in NodeManager.nM.centristNodes)
            {
                if(!userInformation.connectedNodes.Contains(centristNode) && centristNode != this)
                {
                    connectActionAvailable = true;
                }
            }
        }

        but_Connect.EnableButton(connectActionAvailable);

        if (theorheticalActions > 0)
        {
            var factionColor = LevelManager.lM.levelFactions[LevelManager.lM.playerAllyFaction].color;
            if (possibleActions == 0)
            {
                allowanceRing.color = Color.clear;
            }
            else
            {
                float amountThrough = Mathf.Sqrt((Time.time / TimeManager.i.timeMultiplier) % 1.5f);
                allowanceRing.color = Color.Lerp(factionColor, Color.clear, amountThrough);
                allowanceRing.transform.position = Vector3.Lerp(accessRing.transform.position, accessRing.transform.position + Vector3.up, amountThrough);
                allowanceRing.transform.eulerAngles = new Vector3(-90, 0, 0);
            }
        }

        if(theorheticalActions == 0)
        {
            accessRing.color = Color.clear;
            allowanceRing.color = Color.clear;
        }

        if (isBanned)
        {
            accessRing.color = Color.red;
            bannedCover.SetActive(isBanned);
        }

        accessRing.color = LevelManager.lM.levelFactions[userInformation.faction].color;
    }

    public void ActionResult(ActionType aT, Faction actingFaction, Node actingNode, connectionLayer actingLayer)
    {
        if (aT == ActionType.DM)
        {
            userInformation.userInfoHidden = false;
        }

        if(aT == ActionType.Ban)
        {
            isBanned = true;
        }

        if (aT == ActionType.Left || aT == ActionType.Right)
        {
            if (userInformation.misinformerHori)
            {
                return;
            }

            if (actingLayer == connectionLayer.offline)
            {
                if (HUDManager.i.IsSpaceValid(userInformation.beliefs + (ActionManager.aM.actionInformation[aT].actionPosition * 2)))
                {
                    userInformation.beliefs += (ActionManager.aM.actionInformation[aT].actionPosition * 2);
                }
                else if (HUDManager.i.IsSpaceValid(userInformation.beliefs + ActionManager.aM.actionInformation[aT].actionPosition))
                {
                    userInformation.beliefs += ActionManager.aM.actionInformation[aT].actionPosition;
                }
            }
            else if (HUDManager.i.IsSpaceValid(userInformation.beliefs + ActionManager.aM.actionInformation[aT].actionPosition))
            {
                userInformation.beliefs += ActionManager.aM.actionInformation[aT].actionPosition;
            }
        }

        if (aT == ActionType.Up || aT == ActionType.Down)
        {
            if (userInformation.misinformerVert)
            {
                return;
            }

            if (actingLayer == connectionLayer.offline)
            {
                if (HUDManager.i.IsSpaceValid(userInformation.beliefs + (ActionManager.aM.actionInformation[aT].actionPosition * 2)))
                {
                    userInformation.beliefs += (ActionManager.aM.actionInformation[aT].actionPosition * 2);
                }
                else if (HUDManager.i.IsSpaceValid(userInformation.beliefs + ActionManager.aM.actionInformation[aT].actionPosition))
                {
                    userInformation.beliefs += ActionManager.aM.actionInformation[aT].actionPosition;
                }
            }
            else if(HUDManager.i.IsSpaceValid(userInformation.beliefs + ActionManager.aM.actionInformation[aT].actionPosition))
            {
                userInformation.beliefs += ActionManager.aM.actionInformation[aT].actionPosition;
            }
        }

        if (aT == ActionType.Connect)
        {
            var newConnectedNodeInfo = new connectedNodeInfo();
            newConnectedNodeInfo.layer = connectionLayer.onlineOffline;
            newConnectedNodeInfo.type = connectionType.mutual;
            userInformation.connectedNodes.Add(actingNode, newConnectedNodeInfo);
            actingNode.userInformation.connectedNodes.Add(actingNode, newConnectedNodeInfo);
        }

        NodeManager.nM.CheckNodeConnections();
        HUDManager.i.SyncPoliticalAxes();
        NodeManager.nM.DrawNodeConnectionLines();

        var particleSystemMain = playerPS.main;
        particleSystemMain.startColor = LevelManager.lM.levelFactions[actingFaction].color;
        playerPS.GetComponent<ParticleSystemRenderer>().material = LevelManager.lM.levelFactions[actingFaction].particleMaterial;
        playerPS.GetComponent<ParticleSystemRenderer>().trailMaterial = LevelManager.lM.levelFactions[actingFaction].particleMaterial;
        playerPS.Play();

        if(audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        audioSource.volume = 0.5f;
        audioSource.loop = false;
        audioSource.pitch = Random.Range(0.85f, 1.15f);
        audioSource.clip = actionComplete;
        audioSource.Play();
    }

    public void PlayActionAudio()
    {
        audioSource.volume = 0f;
        audioSource.loop = true;
        audioSource.pitch = actionPitch - 0.75f;
        audioSource.clip = actionReady;
        audioSource.Play();
    }

    public void SetActionAudio(float amountThrough)
    {
        audioSource.pitch = Mathf.Lerp(actionPitch - 0.75f, actionPitch, amountThrough);
        audioSource.volume = Mathf.Lerp(0f, 0.5f, amountThrough);
    }

    public void ShowMenu(bool show)
    {
        showMenu = show;
        menu.SetActive(show);

        if (show)
        {
            NodeManager.nM.CloseAllNodeMenus(this);
            HUDManager.i.SyncMenu(this);
        }
        //Buttons.SetActive(show);
    }

    [HideInInspector] public Node_UserInformation userInformation;
    [HideInInspector] public Node_PoliticalAxes politicalAxes;

    public bool showMenu;
    public bool isBanned;

    public int nodePrio;
    public bool performingAction;
    public int receivingActions;

    [Header("Object Assignements")]
    private UserInfo uI;

    [SerializeField] GameObject menu;
    private string nodeHandle;
    [SerializeField] TextMeshPro handleText;
    [SerializeField] GameObject bannedCover;

    [SerializeField] public UserButton but_DM;
    [SerializeField] public UserButton but_Accuse;
    [SerializeField] public UserButton but_Left;
    [SerializeField] public UserButton but_Right;
    [SerializeField] public UserButton but_Up;
    [SerializeField] public UserButton but_Down;
    [SerializeField] public UserButton but_Connect;

    [SerializeField] ParticleSystem playerPS;
    [SerializeField] ParticleSystem aIPs;

    private AudioSource audioSource;
    [SerializeField] AudioClip actionComplete;
    [SerializeField] AudioClip actionReady;
    private float actionPitch;

    [SerializeField] public SpriteRenderer nodeVisual;

    [SerializeField] SpriteRenderer accessRing;
    [SerializeField] SpriteRenderer allowanceRing;
}
