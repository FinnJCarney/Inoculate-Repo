using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Action_Rewind : Action_UserAction
{

    [SerializeField] private float timeToRewind;

    public override bool PerformUserAction()
    {
        float cutOffTime = Mathf.Max(TimeManager.tM.gameTimeElapsed - timeToRewind, 0);

        List<PastAction> pastActions = ActionManager.aM.pastActions;
        List<CurrentAction> currentActions = ActionManager.aM.currentActions;
        List<factionTimer> factionTimers = LevelManager.lM.factionTimers;

        for (int i = factionTimers.Count - 1; i >= 0; i--)
        {
            var adjustFactionTimer = factionTimers[i];
            adjustFactionTimer.timer = (adjustFactionTimer.timer - timeToRewind) % LevelManager.lM.roundTimer;
            factionTimers[i] = adjustFactionTimer;
        }

        LevelManager.lM.factionTimers = factionTimers;

        for (int i = currentActions.Count - 1; i >= 0; i--)
        {
            var adjustedCurrentAction = currentActions[i];
            adjustedCurrentAction.timer += timeToRewind;
            currentActions[i] = adjustedCurrentAction;

            if (currentActions[i].timer > currentActions[i].timerMax)
            {
                ActionManager.aM.DestroyCurrentAction(currentActions[i]);
            }
        }

        for (int i = pastActions.Count - 1; i >= 0; i--)
        {
            if(pastActions[i].timeCompleted > cutOffTime)
            {
                pastActions[i].action.UndoNodeAction(pastActions[i].receivingNode);
                ActionManager.aM.UpdatePastAction(pastActions[i], null, false);
                
                if (pastActions[i].timeStarted < cutOffTime)
                {
                    Debug.Log("Is this being called");
                    float timerVal = cutOffTime - pastActions[i].timeStarted;
                    ActionManager.aM.RecreatePastAction(pastActions[i], timerVal);
                }
                
            }


            if (pastActions[i].timeStarted > cutOffTime)
            {
                pastActions.Remove(pastActions[i]);
            }
        }

        ActionManager.aM.pastActions = pastActions;

        TimeManager.tM.gameTimeElapsed = cutOffTime;

        return true;
    }
}

