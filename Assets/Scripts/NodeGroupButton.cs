using System.Linq;
using UnityEngine;

public class NodeGroupButton : AbstractButtonClass
{
    private void Awake()
    {
        myNodeGroup = GetComponentInParent<NodeGroup>();
        action = abstractActionObj.GetComponent<Action>();
    }

    public override void PerformAction()
    {
        ActionManager.aM.PerfromGroupButtonAction(action, myNodeGroup);
    }


    NodeGroup myNodeGroup;
    [HideInInspector] public Action action;
    [SerializeField] private GameObject abstractActionObj;
}
