using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UniversalButton : MonoBehaviour
{
    public void PerformAction()
    {
        if(buttonEvent != null)
        {
            buttonEvent.Invoke();
        }
    }

    public void OnHover(bool hovered)
    {
        if(hovered)
        {
            buttonBack.color = hoverColor;
        }
        else
        {
            buttonBack.color = unhoveredColor;
        }
    }

    [SerializeField] private Image buttonBack;

    public RequestedManager reqMan;

    [SerializeField] Color unhoveredColor;
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

