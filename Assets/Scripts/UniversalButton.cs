using UnityEngine;
using UnityEngine.Events;

public class UniversalButton : MonoBehaviour
{
    public void PerformAction()
    {

    }

    public void OnHover()
    {

    }


    public RequestedManager reqMan;

    [SerializeField] Color hoverColor;

    public UnityEvent buttonEvent;
}

public enum RequestedManager
{ 
    ActionManager,
    TimeManager,
    StateManager,
    LevelSelectManager,
    None
}

