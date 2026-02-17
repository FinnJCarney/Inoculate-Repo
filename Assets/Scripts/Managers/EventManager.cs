using UnityEngine;

public static class EventManager
{
    public delegate void ChangeState(LevelState state);
    public static ChangeState ChangeStateEvent;
}
