using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Node : MonoBehaviour
{
    private void Start()
    {
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
    }

    private void Update()
    {
        isAlly = (beliefClimateChange == BeliefStates.Believes && !cCHidden && !mRHidden && beliefMinorityRights == BeliefStates.Believes && beliefWealthInequality == BeliefStates.Believes && !wEHidden);

        int possibleActions = 0;
        bool cCAllyNeighbour = false;
        bool cCAllyNeighbourAvail = false;
        bool mRAllyNeighbour = false;
        bool mRAllyNeighbourAvail = false;
        bool wEAllyNeighbour = false;
        bool wEAllyNeighbourAvail = false;

        foreach (Node connectedNode in connectedNodes)
        {
            if (connectedNode.isBanned)
            {
                continue;
            }

            if (!connectedNode.cCHidden && connectedNode.beliefClimateChange == BeliefStates.Believes)
            {
                cCAllyNeighbour = true;

                if (!connectedNode.performingAction)
                {
                    cCAllyNeighbourAvail = true;
                }
            }

            if (!connectedNode.mRHidden && connectedNode.beliefMinorityRights == BeliefStates.Believes)
            {
                mRAllyNeighbour = true;
                if (!connectedNode.performingAction)
                {
                    mRAllyNeighbourAvail = true;
                }
            }

            if (!connectedNode.wEHidden && connectedNode.beliefWealthInequality == BeliefStates.Believes)
            {
                wEAllyNeighbour = true;
                if (!connectedNode.performingAction)
                {
                    wEAllyNeighbourAvail = true;
                }
            }

            if (!connectedNode.wEHidden && connectedNode.beliefWealthInequality == BeliefStates.Believes || !connectedNode.mRHidden && connectedNode.beliefMinorityRights == BeliefStates.Believes || !connectedNode.cCHidden && connectedNode.beliefClimateChange == BeliefStates.Believes)
            {
                possibleActions++;
            }
        }

        but_DM.EnableButton(!isPlayer && !isAlly && (cCHidden || mRHidden || wEHidden) && (cCAllyNeighbourAvail || mRAllyNeighbourAvail || wEAllyNeighbourAvail));
        but_Accuse.EnableButton(!cCHidden && !mRHidden && !wEHidden && !isAlly && (cCAllyNeighbourAvail || mRAllyNeighbourAvail || wEAllyNeighbourAvail));
        but_ClimateChange.EnableButton(cCAllyNeighbourAvail && (cCHidden || beliefClimateChange != BeliefStates.Believes));
        but_MinorityRights.EnableButton(mRAllyNeighbourAvail && (mRHidden || beliefMinorityRights != BeliefStates.Believes));
        but_WealthInequality.EnableButton(wEAllyNeighbourAvail && (wEHidden || beliefWealthInequality != BeliefStates.Believes));

        if (isPlayer || isAlly)
        {
            accessRing.color = Color.blue;
        }

        if (isBanned)
        {
            accessRing.color = Color.red;
            bannedCover.SetActive(isBanned);
        }

        if (((cCAllyNeighbour && (cCHidden || beliefClimateChange != BeliefStates.Believes)) || (mRAllyNeighbour && (mRHidden || beliefMinorityRights != BeliefStates.Believes)) || wEAllyNeighbour && (wEHidden || beliefWealthInequality != BeliefStates.Believes)))
        {
            accessRing.color = Color.yellow;
            if (receivingActions < possibleActions)
            {
                float amountThrough = Mathf.Sqrt(Time.time % 1.5f);
                allowanceRing.color = Color.Lerp(accessRing.color, Color.clear, amountThrough);
                allowanceRing.transform.position = Vector3.Lerp(accessRing.transform.position, accessRing.transform.position + Vector3.up, amountThrough);
                allowanceRing.transform.eulerAngles = new Vector3(-90, 0, 0);
            }
            else
            {
                allowanceRing.color = Color.clear;
            }
        }

        if(!cCAllyNeighbour && !mRAllyNeighbour && !wEAllyNeighbour)
        {

            accessRing.color = Color.white;
            allowanceRing.color = Color.clear;
        }
        
        if(receivingActions == 0 && ((cCAllyNeighbour && !cCAllyNeighbour && (cCHidden || beliefClimateChange != BeliefStates.Believes)) || (mRAllyNeighbour && !mRAllyNeighbourAvail && (mRHidden || beliefMinorityRights != BeliefStates.Believes)) || wEAllyNeighbour && !wEAllyNeighbourAvail && (wEHidden || beliefWealthInequality != BeliefStates.Believes)))
        {
            accessRing.color = Color.yellow;
            allowanceRing.color = Color.clear;
            //allowanceRing.color = Color.red;
            //allowanceRing.transform.position = accessRing.transform.position;
            //allowanceRing.transform.eulerAngles = new Vector3(-70, Time.time * 50, 0);
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

    [SerializeField] ParticleSystem playerPS;
    [SerializeField] ParticleSystem aIPs;

    private AudioSource audioSource;
    [SerializeField] AudioClip actionComplete;
    [SerializeField] AudioClip actionReady;
    private float actionPitch;

    [SerializeField] public List<Node> connectedNodes = new List<Node>();

    [SerializeField] public BeliefStates beliefClimateChange;
    [SerializeField] public BeliefStates beliefMinorityRights;
    [SerializeField] public BeliefStates beliefWealthInequality;

    [SerializeField] SpriteRenderer accessRing;
    [SerializeField] SpriteRenderer allowanceRing;

    public enum BeliefStates
    {
        Believes,
        Unsure,
        Denies,
    }
}
