using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AIManager : MonoBehaviour
{
    private void Start()
    {
        timer = roundTimer;
    }

    private void Update()
    {
        timer -= Time.deltaTime;
        tMP.text = "Time till next action : " + (Mathf.Round(timer * 10) / 10);
        
        if(timer < 0f)
        {
            ActionManager.aM.PerformAIAction(numOfActionsPerTurn);
            timer = roundTimer;
        }
    }

    [SerializeField] int numOfActionsPerTurn;

    private float timer;
    [SerializeField] private float roundTimer;
    [SerializeField] private TextMeshProUGUI tMP;
}
