using System.Linq;
using UnityEngine;

public class NodeGroupButton : AbstractButtonClass
{
    private void Start()
    {
        myNodeGroup = GetComponentInParent<NodeGroup>();
    }

    public override void PerformAction()
    {
        //myNodeGroup.PerformButtonAction(actionType);
    }

    public bool CheckActionAbility(int internalNodePossibleActions, int externalNodePossibleActions)
    {
        if(myNodeGroup == null)
        {
            myNodeGroup = GetComponentInParent<NodeGroup>();
        }

        if (!LevelManager.lM.levelFactions[LevelManager.lM.playerAllyFaction].availableActions.Contains<AbstractAction>(action))
        {
            return false;
        }

        return action.CheckActionAvailability(myNodeGroup, internalNodePossibleActions);
    }


    NodeGroup myNodeGroup;
    [SerializeField] public AbstractAction action;
}
