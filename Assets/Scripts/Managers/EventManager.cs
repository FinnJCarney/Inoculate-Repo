using UnityEngine;

public static class EventManager
{
    public delegate void ExecuteFunction();
    public static ExecuteFunction ExecuteEvent;
    public static Event ReturnToPlanningState;
}
