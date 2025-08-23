using System.Linq;
using UnityEngine;

public class NodeGroupButton : AbstractButtonClass
{
    private void Awake()
    {
        myNodeGroup = GetComponentInParent<NodeGroup>();
        action = abstractActionObj.GetComponent<AbstractAction>();
    }

    public override void PerformAction()
    {
        ActionManager.aM.PerfromGroupButtonAction(action, myNodeGroup);
    }


    NodeGroup myNodeGroup;
    [HideInInspector] public AbstractAction action;
    [SerializeField] private GameObject abstractActionObj;
}
