using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;

public class UniversalButton : MonoBehaviour
{
    private void Start()
    {
        if (buttonEvent == null) { buttonEvent = new UnityEvent(); }

        if (reqMan == RequestedManager.LayerManager)
        {
            if (LevelManager.lM.allowedLayers != connectionLayer.onlineOffline)
            {
                Destroy(this.gameObject);
            }
            else
            {
                LayerManager.lM.buttonImage = buttonIcon;
                buttonEvent.AddListener(delegate { LayerManager.lM.ChangeLayer(); });
            }
        }

        if (reqMan == RequestedManager.NodeManager)
        {
            buttonEvent.AddListener(delegate { this.gameObject.GetComponent<UserIndicator>().node.ShowMenu(true); });
        }
    }

    public void PerformAction()
    {
        if(buttonEvent != null)
        {
            buttonEvent.Invoke();
        }
    }

    public void OnHover(bool hovered)
    {
        if(buttonBack == null)
        {
            return;
        }

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
    [SerializeField] private Image buttonIcon;

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
    LayerManager,
    NodeManager,
    None
}

