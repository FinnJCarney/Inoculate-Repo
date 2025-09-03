using UnityEngine;

public class UserActionButton : AbstractButtonClass
{
    private void Awake()
    {
        action = abstractActionObj.GetComponent<Action_UserAction>();
    }

    public override void PerformAction()
    {
        action.PerformUserAction();
    }


    [HideInInspector] public Action_UserAction action;
    [SerializeField] private GameObject abstractActionObj;
}
