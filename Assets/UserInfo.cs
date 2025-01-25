using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UserInfo : MonoBehaviour
{
    public void InitializeUserInfo(string uN, Node.BeliefStates cC, bool hideCC, Node.BeliefStates mR, bool hideMR, Node.BeliefStates wE, bool hideWE)
    {
        userName.text = uN;

        SetBeliefs(cC, hideCC, mR, hideMR, wE, hideWE);
    }

    public void SetBeliefs(Node.BeliefStates cC, bool hideCC, Node.BeliefStates mR, bool hideMR, Node.BeliefStates wE, bool hideWE)
    {
        if (hideCC)
        {
            beliefClimateChange.text = hidden;
            beliefClimateChange.color = unsureColor;
        }
        else
        {
            if (cC == Node.BeliefStates.Believes)
            {
                beliefClimateChange.text = belief;
                beliefClimateChange.color = believesColor;
            }

            else if (cC == Node.BeliefStates.Unsure)
            {
                beliefClimateChange.text = unsure;
                beliefClimateChange.color = unsureColor;

            }

            else if (cC == Node.BeliefStates.Denies)
            {
                beliefClimateChange.text = denies;
                beliefClimateChange.color = deniesColor;
            }
        }

        if (hideMR)
        {
            beliefMinorityRights.text = hidden;
            beliefMinorityRights.color = unsureColor;
        }
        else
        {
            if (mR == Node.BeliefStates.Believes)
            {
                beliefMinorityRights.text = belief;
                beliefMinorityRights.color = believesColor;
            }

            else if (mR == Node.BeliefStates.Unsure)
            {
                beliefMinorityRights.text = unsure;
                beliefMinorityRights.color = unsureColor;

            }

            else if (mR == Node.BeliefStates.Denies)
            {
                beliefMinorityRights.text = denies;
                beliefMinorityRights.color = deniesColor;
            }
        }

        if (hideWE)
        {
            beliefWealthInequality.text = hidden;
            beliefWealthInequality.color = unsureColor;
        }

        else
        {

            if (wE == Node.BeliefStates.Believes)
            {
                beliefWealthInequality.text = belief;
                beliefWealthInequality.color = believesColor;
            }

            else if (wE == Node.BeliefStates.Unsure)
            {
                beliefWealthInequality.text = unsure;
                beliefWealthInequality.color = unsureColor;

            }

            else if (wE == Node.BeliefStates.Denies)
            {
                beliefWealthInequality.text = denies;
                beliefWealthInequality.color = deniesColor;
            }
        }
    }



    [SerializeField] private TextMeshProUGUI userName;

    [SerializeField] public ConnectionContainers connectionContainers;

    [SerializeField] TextMeshProUGUI beliefClimateChange;
    [SerializeField] TextMeshProUGUI beliefMinorityRights;
    [SerializeField] TextMeshProUGUI beliefWealthInequality;

    [SerializeField] string belief;
    [SerializeField] string unsure;
    [SerializeField] string denies;
    [SerializeField] string hidden;

    [SerializeField] Color unsureColor;
    [SerializeField] Color believesColor;
    [SerializeField] Color deniesColor;


}

public struct ConnectionContainers
{
    public GameObject connectionObj;
}

