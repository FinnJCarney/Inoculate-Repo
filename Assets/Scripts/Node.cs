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

        uI = GetComponentInChildren<UserInfo>();

        uI.InitializeUserInfo(beliefClimateChange, cCHidden, beliefMinorityRights, mRHidden, beliefWealthInequality, wEHidden);

        ShowMenu(false);
        bannedCover.SetActive(isBanned);

        actionPitch = Random.Range(1.25f, 1.5f);

        politicalAxes.SyncPoliticalAxes(this);
    }

    private void Update()
    {
        //isAlly = (beliefClimateChange == BeliefStates.Believes && !cCHidden && !mRHidden && beliefMinorityRights == BeliefStates.Believes && beliefWealthInequality == BeliefStates.Believes && !wEHidden);

        int theorheticalActions = 0;
        int possibleActions = 0;

        bool hasLeftNeighbourAvail = false;
        bool hasRightNeighbourAvail = false;
        bool hasUpNeighbourAvail = false;
        bool hasDownNeighbourAvail = false;

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

            if (connectedNode.isAlly || connectedNode.isPlayer)
            {
                theorheticalActions++;
                possibleActions ++;

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

            if (!connectedNode.wEHidden && connectedNode.beliefWealthInequality == BeliefStates.Believes || !connectedNode.mRHidden && connectedNode.beliefMinorityRights == BeliefStates.Believes || !connectedNode.cCHidden && connectedNode.beliefClimateChange == BeliefStates.Believes)
            {
                possibleActions++;
            }
        }

        but_DM.EnableButton(!isPlayer && !isAlly);
        //but_Accuse.EnableButton(!cCHidden && !mRHidden && !wEHidden && !isAlly && (cCAllyNeighbourAvail || mRAllyNeighbourAvail || wEAllyNeighbourAvail));


        //New Stuff!
        but_Left.EnableButton(hasLeftNeighbourAvail);
        but_Right.EnableButton(hasRightNeighbourAvail);
        but_Up.EnableButton(hasUpNeighbourAvail);
        but_Down.EnableButton(hasDownNeighbourAvail);
        //End of New Stuff

        if (theorheticalActions > 0)
        {
            accessRing.color = Color.yellow;
            if(possibleActions == 0)
            {
                allowanceRing.color = Color.clear;
            }
            else
            {
                float amountThrough = Mathf.Sqrt(Time.time % 1.5f);
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

        if (isPlayer || isAlly)
        {
            accessRing.color = Color.blue;
        }
    }

    public void ActionResult(ActionType aT, bool playerActivated)
    {
        if (aT == ActionType.DM)
        {
            if (cCHidden)
            {
                cCHidden = false;
            }

            if (mRHidden)
            {
                mRHidden = false;
            }

            if (wEHidden)
            {
                wEHidden = false;
            }
        }

        if(aT == ActionType.Ban)
        {
            isBanned = true;
            NodeManager.nM.DrawNodeConnectionLines();
        }

        if (aT == ActionType.Educate_ClimateChange && beliefClimateChange != BeliefStates.Believes && !misinformerClimateChange)
        {
            beliefClimateChange -= 1;
        }

        if (aT == ActionType.Disinform_ClimateChange && beliefClimateChange != BeliefStates.Denies)
        {
            beliefClimateChange += 1;
        }

        if (aT == ActionType.Educate_MinorityRights && beliefMinorityRights != BeliefStates.Believes && !misinformerMinorityRights)
        {
            beliefMinorityRights -= 1;
        }

        if (aT == ActionType.Disinform_MinorityRights && beliefMinorityRights != BeliefStates.Denies)
        {
            beliefMinorityRights += 1;
        }

        if (aT == ActionType.Educate_WealthInequality && beliefWealthInequality != BeliefStates.Believes && !misinformerWealthInequality)
        {
            beliefWealthInequality -= 1;
        }

        if (aT == ActionType.Disinform_WealthInequality && beliefWealthInequality != BeliefStates.Denies)
        {
            beliefWealthInequality += 1;
        }

        if (aT == ActionType.Left)
        {
            userInformation.beliefs.x -= 1;
        }

        if (aT == ActionType.Right)
        {
            userInformation.beliefs.x += 1;
        }

        if (aT == ActionType.Up)
        {
            userInformation.beliefs.y += 1;
        }

        if (aT == ActionType.Down)
        {
            userInformation.beliefs.y -= 1;
        }

        NodeManager.nM.SyncAllPoliticalAxes();

        uI.SetBeliefs(beliefClimateChange, cCHidden, beliefMinorityRights, mRHidden, beliefWealthInequality, wEHidden);

        if(playerActivated)
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
        Buttons.SetActive(show);
    }

    [HideInInspector] public Node_UserInformation userInformation;
    [HideInInspector] public Node_PoliticalAxes politicalAxes;

    public bool showMenu;
    public bool isAlly;
    public bool isBanned;

    public int nodePrio;
    public bool performingAction;
    public int receivingActions;

    [Header("Gameplay Assignments")]

    [SerializeField] public bool isPlayer;
    [SerializeField] public bool misinformerClimateChange;
    [SerializeField] public bool misinformerMinorityRights;
    [SerializeField] public bool misinformerWealthInequality;
                     
    public bool cCHidden;
    public bool mRHidden;
    public bool wEHidden;

    [Header("Object Assignements")]
    private UserInfo uI;

    [SerializeField] GameObject menu;
    [SerializeField] string nodeHandle;
    [SerializeField] TextMeshPro handleText;
    [SerializeField] GameObject bannedCover;


    [SerializeField] GameObject Buttons;
    [SerializeField] UserButton but_DM;
    [SerializeField] UserButton but_Accuse;
    [SerializeField] UserButton but_ClimateChange;
    [SerializeField] UserButton but_MinorityRights;
    [SerializeField] UserButton but_WealthInequality;

    [SerializeField] UserButton but_Left;
    [SerializeField] UserButton but_Right;
    [SerializeField] UserButton but_Up;
    [SerializeField] UserButton but_Down;

    [SerializeField] ParticleSystem playerPS;
    [SerializeField] ParticleSystem aIPs;

    private AudioSource audioSource;
    [SerializeField] AudioClip actionComplete;
    [SerializeField] AudioClip actionReady;
    private float actionPitch;

    [SerializeField] public BeliefStates beliefClimateChange;
    [SerializeField] public BeliefStates beliefMinorityRights;
    [SerializeField] public BeliefStates beliefWealthInequality;

    
    [SerializeField] public List<Node> connectedNodes = new List<Node>();

    [SerializeField] SpriteRenderer accessRing;
    [SerializeField] SpriteRenderer allowanceRing;

    public enum BeliefStates
    {
        Believes,
        Unsure,
        Denies,
    }
}
