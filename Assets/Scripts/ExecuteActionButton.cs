using System.Linq;
using UnityEngine;

public class ExecuteActionButton : AbstractButtonClass
{
    public override void PerformAction()
    {
        EventManager.ChangeStateEvent.Invoke(LevelState.Executing);
    }
}
