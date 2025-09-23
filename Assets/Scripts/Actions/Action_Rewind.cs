using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Action_Rewind : Action_UserAction
{

    [SerializeField] private float timeToRewind;

    public override bool PerformUserAction()
    {
        float cutOffTime = TimeManager.tM.gameTimeElapsed - timeToRewind;

        List<PastAction> pastActions = ActionManager.aM.pastActions;
        List<PastAction> actionsToRecreate = new List<PastAction>();
        List<CurrentAction> currentActions = ActionManager.aM.currentActions;
        List<factionTimer> factionTimers = LevelManager.lM.factionTimers;

        for (int i = factionTimers.Count - 1; i >= 0; i--) //Review why this can't go below 0
        {
            var adjustFactionTimer = factionTimers[i];
            adjustFactionTimer.timer = adjustFactionTimer.timer + timeToRewind;
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
                pastActions.Remove(currentActions[i].pastAction);
                ActionManager.aM.DestroyCurrentAction(currentActions[i]);
            }
        }

        ActionManager.aM.currentActions = currentActions;

        for (int i = pastActions.Count - 1; i >= 0; i--)
        {
            if (pastActions[i].timeStarted < cutOffTime && pastActions[i].timeCompleted != - 999f)
            {
                actionsToRecreate.Add(pastActions[i]);
            }

            if (pastActions[i].timeCompleted > cutOffTime)
            {
                pastActions[i].action.UndoNodeAction(pastActions[i].receivingNode);
                ActionManager.aM.CompletePastAction(pastActions[i], null, false);
            }

            if (pastActions[i].timeStarted > cutOffTime)
            {
                Debug.Log("Deleting Past Action");
                pastActions.Remove(pastActions[i]);
            }


        }

        //for (int i = actionsToRecreate.Count - 1; i >= 0; i--)
        //{
        //
        //    for (int j = actionsToRecreate[i].actingNodeGroups.Count - 1; j >= 0; j--)
        //    {
        //        if (actionsToRecreate[i].actingNodeGroups[j].timeOfTarget > cutOffTime)
        //        {
        //            actionsToRecreate[i].actingNodeGroups.RemoveAt(j);
        //        }
        //    }
        //
        //    for (int j = pastActions[i].receivingNodeGroups.Count - 1; j >= 0; j--)
        //    {
        //        if (actionsToRecreate[i].receivingNodeGroups[j].timeOfTarget > cutOffTime)
        //        {
        //            actionsToRecreate[i].receivingNodeGroups.RemoveAt(j);
        //        }
        //    }
        //
        //    float timerVal = cutOffTime - actionsToRecreate[i].timeStarted;
        //    ActionManager.aM.RecreatePastAction(actionsToRecreate[i], timerVal);
        //
        //}

        ActionManager.aM.pastActions = pastActions;

        TimeManager.tM.gameTimeElapsed = cutOffTime;

        return true;
    }
}

