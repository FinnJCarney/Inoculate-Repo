using System.Linq;
using UnityEngine;

public class ExecuteActionButton : AbstractButtonClass
{
    private void Start()
    {
        
    }

    public override void PerformAction()
    {
        EventManager.ExecuteEvent.Invoke();
    }

    private void HideButton()
    {
        //Hide Button
    }
}
