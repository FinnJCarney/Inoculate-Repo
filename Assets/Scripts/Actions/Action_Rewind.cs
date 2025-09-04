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
                ActionManager.aM.CompletePastAction(pastActions[i], null, false);
                
                if (pastActions[i].timeStarted < cutOffTime)
                {
                    for (int j = pastActions[i].actingNodeGroups.Count - 1; j >= 0; j--)
                    {
                        if (pastActions[i].actingNodeGroups[j].timeOfTarget > cutOffTime)
                        {
                            pastActions[i].actingNodeGroups.RemoveAt(j);
                        }
                    }

                    for (int j = pastActions[i].receivingNodeGroups.Count - 1; j >= 0; j--)
                    {
                        if (pastActions[i].receivingNodeGroups[j].timeOfTarget > cutOffTime)
                        {
                            pastActions[i].receivingNodeGroups.RemoveAt(j);
                        }
                    }

                    float timerVal = cutOffTime - pastActions[i].timeStarted;
                    if (pastActions[i].actingNodeGroups[pastActions[i].actingNodeGroups.Count - 1].nodeGroupTarget.nodesInGroup.Count > 0 && pastActions[i].receivingNodeGroups[pastActions[i].receivingNodeGroups.Count - 1].nodeGroupTarget.nodesInGroup.Count > 0)
                    {
                        ActionManager.aM.RecreatePastAction(pastActions[i], timerVal);
                    }
                }
            }
            else
            {
                pastActions.Remove(pastActions[i]);
            }
        }

        ActionManager.aM.pastActions = pastActions;

        TimeManager.tM.gameTimeElapsed = cutOffTime;

        return true;
    }
}

