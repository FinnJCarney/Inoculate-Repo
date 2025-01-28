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

        actionPitch = Random.Range(1.25f, 1.5f);
    }

    private void Update()
    {
        isAlly = (beliefClimateChange == BeliefStates.Believes && !cCHidden && !mRHidden && beliefMinorityRights == BeliefStates.Believes && beliefWealthInequality == BeliefStates.Believes && !wEHidden);

        bool cCAllyNeighbour = false;
        bool mRAllyNeighbour = false;
        bool wEAllyNeighbour = false;

        foreach(Node connectedNode in connectedNodes)
        {
            if (connectedNode.performingAction || connectedNode.isBanned)
            {
                continue;
            }

            if (!connectedNode.cCHidden && connectedNode.beliefClimateChange == BeliefStates.Believes)
            {
                cCAllyNeighbour = true;
            }

            if (!connectedNode.mRHidden && connectedNode.beliefMinorityRights == BeliefStates.Believes)
            {
                mRAllyNeighbour = true;
            }

            if (!connectedNode.wEHidden && connectedNode.beliefWealthInequality == BeliefStates.Believes)
            {
                wEAllyNeighbour = true;
            }
        }

        but_DM.EnableButton(!isPlayer && !isAlly && (cCHidden || mRHidden || wEHidden) && (cCAllyNeighbour || mRAllyNeighbour || wEAllyNeighbour));
        but_Accuse.EnableButton(!cCHidden && !mRHidden && !wEHidden && !isAlly && (cCAllyNeighbour || mRAllyNeighbour || wEAllyNeighbour));
        but_ClimateChange.EnableButton(cCAllyNeighbour && (cCHidden || beliefClimateChange != BeliefStates.Believes));
        but_MinorityRights.EnableButton(mRAllyNeighbour && (mRHidden || beliefMinorityRights != BeliefStates.Believes));
        but_WealthInequality.EnableButton(wEAllyNeighbour && (wEHidden || beliefWealthInequality != BeliefStates.Believes));

        if(isPlayer || isAlly)
        {
            accessRing.color = Color.blue;
        }
        
        else if ((cCAllyNeighbour && (cCHidden || beliefClimateChange != BeliefStates.Believes)) || (mRAllyNeighbour && (mRHidden || beliefMinorityRights != BeliefStates.Believes)) || wEAllyNeighbour && (wEHidden || beliefWealthInequality != BeliefStates.Believes))
        {
            accessRing.color = Color.yellow;
        }

        else
        {
            accessRing.color = Color.red;
        }

        if(isBanned)
        {
            accessRing.color = Color.clear;
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

    public enum BeliefStates
    {
        Believes,
        Unsure,
        Denies,
    }
}
