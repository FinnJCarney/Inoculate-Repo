using DG.Tweening;
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

        NodeManager.nM.AddNodeToList(userInformation);
        LevelManager.lM.nodeGroups[userInformation.beliefs].AddNodeToGroup(userInformation);

        audioSource = GetComponent<AudioSource>();

        handleText.text = userInformation.name;

        if(userInformation.toCapture)
        {
            handleText.color = Color.lightGoldenRodYellow;
        }

        menu.SetActive(false);
        bannedCover.SetActive(isBanned);

        actionPitch = Random.Range(1.25f, 1.5f);

        if(!LevelManager.lM.CheckValidSpace(userInformation.beliefs))
        {
            Debug.Log(userInformation.name + " has an invalid starting position");
        }
    }

    public void UpdateNodeColour()
    {
        nodeVisual.color = Color.Lerp(LevelManager.lM.GiveAverageColor(userInformation.beliefs), Color.white, 0.5f);
        accessRing.color = LevelManager.lM.levelFactions[userInformation.faction].color;
    }

    private void Update()
    {
        UpdateNodeColour();

        int theorheticalActions = 0;
        int possibleActions = 0;

        bool hasLeftNeighbourAvail = false;
        bool hasRightNeighbourAvail = false;
        bool hasUpNeighbourAvail = false;
        bool hasDownNeighbourAvail = false;

        bool hasAllyNeighbourAvail = false;

        foreach (Node_UserInformation connectedNode in userInformation.connectedNodes.Keys)
        {
            //Debug.Log("Checking for Node " + name + " connectedNode = " + connectedNode.name);
            if (connectedNode.isBanned)
            {
                continue;
            }

            if(userInformation.connectedNodes[connectedNode].type == connectionType.influenceOn || connectedNode.connectedNodes[userInformation].type == connectionType.influencedBy)
            {
                continue;
            }

            if(connectedNode.nodeCore.performingAction)
            {
                theorheticalActions++;
                continue;
            }

            if (connectedNode.faction == LevelManager.lM.playerAllyFaction)
            {
                theorheticalActions++;
                possibleActions ++;
                hasAllyNeighbourAvail = true;

                //if (connectedNode.userInformation.beliefs.x == this.userInformation.beliefs.x)
                //{
                //    hasLeftNeighbourAvail = true;
                //    hasRightNeighbourAvail = true;
                //}
                //else if (connectedNode.userInformation.beliefs.x > this.userInformation.beliefs.x)
                //{
                //    hasRightNeighbourAvail = true;
                //}
                //else if (connectedNode.userInformation.beliefs.x < this.userInformation.beliefs.x)
                //{
                //    hasLeftNeighbourAvail = true;
                //}
                //
                //if (connectedNode.userInformation.beliefs.y == this.userInformation.beliefs.y)
                //{
                //    hasUpNeighbourAvail = true;
                //    hasDownNeighbourAvail = true;
                //}
                //else if (connectedNode.userInformation.beliefs.y > this.userInformation.beliefs.y)
                //{
                //    hasUpNeighbourAvail = true;
                //}
                //else if (connectedNode.userInformation.beliefs.y < this.userInformation.beliefs.y)
                //{
                //    hasDownNeighbourAvail = true;
                //}
            }
        }

        if (hasAllyNeighbourAvail)
        {
            Vector2 movement = new Vector2(12f, 12f);
            hasUpNeighbourAvail = LevelManager.lM.CheckValidSpace(userInformation.beliefs + new Vector2(0, 12f));

            hasDownNeighbourAvail = LevelManager.lM.CheckValidSpace(userInformation.beliefs + new Vector2(0, -12f));

            hasRightNeighbourAvail = LevelManager.lM.CheckValidSpace(userInformation.beliefs + (Vector2.right * movement));

            hasLeftNeighbourAvail = LevelManager.lM.CheckValidSpace(userInformation.beliefs + (Vector2.left * movement));
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
                float amountThrough = Mathf.Sqrt(Time.unscaledTime % 2f);
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
    }


    //public void ActionResult(ActionType aT, Faction actingFaction, Node actingNode, connectionLayer actingLayer, Bleat bleat)
    //{
    //    bool actionSuccessful = false;

    //    if (aT == ActionType.DM)
    //    {
    //        userInformation.userInfoHidden = false;
    //    }

    //    if(aT == ActionType.Ban)
    //    {
    //        isBanned = true;
    //    }

    //    if (aT == ActionType.Left || aT == ActionType.Right || aT == ActionType.Up || aT == ActionType.Down)
    //    {
    //        Vector2 originalBeliefs = userInformation.beliefs;

    //        if (userInformation.misinformerHori && (aT == ActionType.Left || aT == ActionType.Right))
    //        {
    //            return;
    //        }
    //        else
    //        {
    //            if (aT == ActionType.Right)
    //            {
    //                if (LevelManager.lM.CheckValidSpace(userInformation.beliefs + new Vector2(12f, 0f)))
    //                {
    //                    userInformation.beliefs += new Vector2(12f, 0f);
    //                    actionSuccessful = true;
    //                }
    //            }
    //            else if(aT == ActionType.Left)
    //            {
    //                if (LevelManager.lM.CheckValidSpace(userInformation.beliefs + new Vector2(-12f, 0f)))
    //                {
    //                    userInformation.beliefs += new Vector2(-12f, 0f);
    //                    actionSuccessful = true;
    //                }
    //            }
    //        }

    //        if (userInformation.misinformerVert && (aT == ActionType.Up || aT == ActionType.Down))
    //        {
    //            return;
    //        }
    //        else
    //        {
    //            if (aT == ActionType.Up)
    //            {
    //                if (LevelManager.lM.CheckValidSpace(userInformation.beliefs + new Vector2(0f, 12f)))
    //                {
    //                    userInformation.beliefs += new Vector2(0f, 12f);
    //                    actionSuccessful = true;
    //                }
    //            }
    //            else if(aT == ActionType.Down)
    //            {
    //                if (LevelManager.lM.CheckValidSpace(userInformation.beliefs + new Vector2(0f, -12f)))
    //                {
    //                    userInformation.beliefs += new Vector2(0f, -12f);
    //                    actionSuccessful = true;
    //                }
    //            }
    //        }

    //        if(originalBeliefs != userInformation.beliefs)
    //        {
    //            LevelManager.lM.nodeGroups[originalBeliefs].RemoveNodeFromGroup(userInformation);
    //            LevelManager.lM.nodeGroups[userInformation.beliefs].AddNodeToGroup(userInformation);
    //        }
    //    }

    //    if (aT == ActionType.Connect)
    //    {
    //        var newConnectedNodeInfo = new connectedNodeInfo();
    //        newConnectedNodeInfo.layer = connectionLayer.onlineOffline;
    //        newConnectedNodeInfo.type = connectionType.mutual;
    //        userInformation.connectedNodes.Add(actingNode, newConnectedNodeInfo);
    //        actingNode.userInformation.connectedNodes.Add(actingNode, newConnectedNodeInfo);
    //    }

    //    NodeManager.nM.CheckNodeConnections();
    //    NodeManager.nM.DrawNodeGroupConnectionLines();

    //    var particleSystemMain = playerPS.main;
    //    particleSystemMain.startColor = LevelManager.lM.levelFactions[actingFaction].color;
    //    playerPS.GetComponent<ParticleSystemRenderer>().material = LevelManager.lM.levelFactions[actingFaction].particleMaterial;
    //    playerPS.GetComponent<ParticleSystemRenderer>().trailMaterial = LevelManager.lM.levelFactions[actingFaction].particleMaterial;
    //    playerPS.Play();

    //    if(audioSource.isPlaying)
    //    {
    //        audioSource.Stop();
    //    }

    //    if(actionSuccessful)
    //    {
    //        bleat.CreateResponse(this.userInformation);
    //    }

    //    audioSource.volume = 0.5f;
    //    audioSource.loop = false;
    //    audioSource.pitch = Random.Range(0.85f, 1.15f);
    //    audioSource.clip = actionComplete;
    //    audioSource.Play();
    //}

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
            HUDManager.hM.SyncMenu(this);
        }
        //Buttons.SetActive(show);
    }
    [HideInInspector] public Node_UserInformation userInformation;

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

    [SerializeField] private SpriteRenderer accessRing;
    [SerializeField] private SpriteRenderer allowanceRing;
}
