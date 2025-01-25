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

        if (isPlayer)
        {
            nodeHandle = System.Environment.UserName;
        }

        handleText.text = nodeHandle;

        uI = GetComponentInChildren<UserInfo>();

        uI.InitializeUserInfo(nodeHandle, beliefClimateChange, cCHidden, beliefMinorityRights, mRHidden, beliefWealthInequality, wEHidden);

        ShowMenu(false);
    }

    private void Update()
    {
        bool cCAllyNeighbour = false;
        bool mRAllyNeighbour = false;
        bool wEAllyNeighbour = false;

        foreach(Node connectedNode in connectedNodes)
        {
            if(connectedNode.performingAction)
            {
                continue;
            }

            if(connectedNode.cCHidden != true && connectedNode.beliefClimateChange == BeliefStates.Believes)
            {
                cCAllyNeighbour = true;
            }

            if (connectedNode.mRHidden != true && connectedNode.beliefMinorityRights == BeliefStates.Believes)
            {
                mRAllyNeighbour = true;
            }

            if (connectedNode.wEHidden != true && connectedNode.beliefWealthInequality == BeliefStates.Believes)
            {
                wEAllyNeighbour = true;
            }
        }

        but_DM.EnableButton(cCHidden && cCAllyNeighbour || mRHidden && mRAllyNeighbour || wEHidden && wEAllyNeighbour);
        but_Educate.EnableButton(cCHidden && cCAllyNeighbour || mRHidden && mRAllyNeighbour || wEHidden && wEAllyNeighbour);
        but_Accuse.EnableButton(!isPlayer);
        but_ClimateChange.EnableButton(cCAllyNeighbour && (cCHidden || beliefClimateChange != BeliefStates.Believes));
        but_MinorityRights.EnableButton(mRAllyNeighbour && (mRHidden || beliefMinorityRights != BeliefStates.Believes));
        but_WealthInequality.EnableButton(wEAllyNeighbour && (wEHidden || beliefWealthInequality != BeliefStates.Believes));
    }

    public void DMResult()
    {
        if (cCHidden)
        {
            cCHidden = false;
        }
        else if (mRHidden)
        {
            mRHidden = false;
        }
        else if (wEHidden)
        {
            wEHidden = false;
        }

        uI.SetBeliefs(beliefClimateChange, cCHidden, beliefMinorityRights, mRHidden, beliefWealthInequality, wEHidden);
    }

    public void ClimateChangeEducationResult()
    {
        if(beliefClimateChange != BeliefStates.Believes)
        {
            beliefClimateChange -= 1;
        }
    }

    public void MinorityRightsEducationResult()
    {
        if (beliefMinorityRights != BeliefStates.Believes)
        {
            beliefMinorityRights -= 1;
        }
    }

    public void WealthInequalityEducationResult()
    {
        if (beliefWealthInequality != BeliefStates.Believes)
        {
            beliefWealthInequality -= 1;
        }
    }





    public void ShowMenu(bool show)
    {
        menu.SetActive(show);
        Buttons.SetActive(show);
    }

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
    [SerializeField] UserButton but_Educate;
    [SerializeField] UserButton but_Accuse;
    [SerializeField] UserButton but_ClimateChange;
    [SerializeField] UserButton but_MinorityRights;
    [SerializeField] UserButton but_WealthInequality;

    [SerializeField] public List<Node> connectedNodes = new List<Node>();

    [SerializeField] public BeliefStates beliefClimateChange;
    [SerializeField] public BeliefStates beliefMinorityRights;
    [SerializeField] public BeliefStates beliefWealthInequality;

    public enum BeliefStates
    {
        Believes,
        Unsure,
        Denies,
    }
}
