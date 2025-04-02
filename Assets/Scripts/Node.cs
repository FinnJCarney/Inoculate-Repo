using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    private void Start()
    {
        userInformation = GetComponent<Node_UserInformation>();

        nodeHandle = userInformation.NodeName != "" ? userInformation.NodeName : this.gameObject.name;

        NodeManager.nM.AddNodeToList(this);

        audioSource = GetComponent<AudioSource>();

        //if (isPlayer)
        //{
        //    nodeHandle = System.Environment.UserName;
        //}

        handleText.text = nodeHandle;

        menu.SetActive(false);
        bannedCover.SetActive(isBanned);

        actionPitch = Random.Range(1.25f, 1.5f);

        //politicalAxes.SyncPoliticalAxes(this);
    }

    private void Update()
    {
        int theorheticalActions = 0;
        int possibleActions = 0;

        bool hasLeftNeighbourAvail = false;
        bool hasRightNeighbourAvail = false;
        bool hasUpNeighbourAvail = false;
        bool hasDownNeighbourAvail = false;

        bool hasAllyNeighbourAvail = false;

        foreach (Node connectedNode in connectedNodes)
        {
            if (connectedNode.isBanned || !connectedNode.connectedNodes.Contains(this))
            {
                continue;
            }

            if(connectedNode.performingAction)
            {
                theorheticalActions++;
                continue;
            }

            if (connectedNode.userInformation.allyStatus == LevelManager.lM.playerAllyFaction)
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
                if(!connectedNodes.Contains(centristNode) && centristNode != this)
                {
                    connectActionAvailable = true;
                }
            }
        }

        but_Connect.EnableButton(connectActionAvailable);

        if (theorheticalActions > 0)
        {
            accessRing.color = Color.yellow;
            if(possibleActions == 0)
            {
                allowanceRing.color = Color.clear;
            }
            else
            {
                float amountThrough = Mathf.Sqrt((Time.time / TimeManager.i.timeMultiplier) % 1.5f);
                allowanceRing.color = Color.Lerp(accessRing.color, Color.clear, amountThrough);
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

        if (userInformation.allyStatus == LevelManager.lM.playerAllyFaction)
        {
            if(userInformation.allyStatus == AllyStatus.Red)
            {
                accessRing.color = Color.red;
            }

            else if (userInformation.allyStatus == AllyStatus.Yellow)
            {
                accessRing.color = Color.yellow;
            }

            else if (userInformation.allyStatus == AllyStatus.Green)
            {
                accessRing.color = Color.green;
            }

            else if (userInformation.allyStatus == AllyStatus.Blue)
            {
                accessRing.color = Color.blue;
            }
        }
    }

    public void ActionResult(ActionType aT, bool playerActivated, Node actingNode)
    {
        if (aT == ActionType.DM)
        {
            userInformation.userInfoHidden = false;
        }

        if(aT == ActionType.Ban)
        {
            isBanned = true;
        }

        if (aT == ActionType.Left && !userInformation.misinformerHori)
        {
            userInformation.beliefs.x -= 1;
        }

        if (aT == ActionType.Right && !userInformation.misinformerHori)
        {
            userInformation.beliefs.x += 1;
        }

        if (aT == ActionType.Up && !userInformation.misinformerVert)
        {
            userInformation.beliefs.y += 1;
        }

        if (aT == ActionType.Down && !userInformation.misinformerVert)
        {
            userInformation.beliefs.y -= 1;
        }

        if (aT == ActionType.Connect)
        {
            connectedNodes.Add(actingNode);
            actingNode.connectedNodes.Add(actingNode);
        }

        userInformation.beliefs.x = Mathf.Max(-2, Mathf.Min(2, userInformation.beliefs.x));
        userInformation.beliefs.y = Mathf.Max(-2, Mathf.Min(2, userInformation.beliefs.y));

        NodeManager.nM.CheckNodeConnections();
        HUDManager.i.SyncPoliticalAxes();
        NodeManager.nM.DrawNodeConnectionLines();

        if (playerActivated)
        {
            playerPS.Play();
        }
        else
        {
            aIPs.Play();
        }

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

    public void SetActionAudio(bool playerAction, float amountThrough)
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
    [SerializeField] string nodeHandle;
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
    
    [SerializeField] public List<Node> connectedNodes = new List<Node>();

    [SerializeField] SpriteRenderer accessRing;
    [SerializeField] SpriteRenderer allowanceRing;
}
