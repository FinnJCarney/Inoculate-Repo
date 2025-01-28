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
        
        if(timer < 0f)
        {
            ActionManager.aM.PerformAIAction(numOfActionsPerTurn);
            timer = roundTimer * (Random.Range(0.5f, 1.5f));
        }
    }

    [SerializeField] int numOfActionsPerTurn;

    private float timer;
    [SerializeField] private float roundTimer;
}
