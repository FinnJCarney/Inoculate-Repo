using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class ButtonAssigner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var button = GetComponent<Button>();

        if (reqMan == RequestedManager.TimeManager)
        {
            button.onClick.AddListener(delegate { TimeManager.tM.SetTimeScale(value); } );
        }

        if(reqMan == RequestedManager.HUDManager)
        {
            button.onClick.AddListener(delegate { NodeManager.nM.CloseAllNodeMenus(null); });
        }
    }

    [SerializeField] private RequestedManager reqMan;

    [SerializeField] private float value;

    public enum RequestedManager
    { 
        None,
        TimeManager,
        HUDManager
    }

}
