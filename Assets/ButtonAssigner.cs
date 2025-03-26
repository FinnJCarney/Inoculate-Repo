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
        if(reqMan == RequestedManager.TimeManager)
        {
            var button = GetComponent<Button>();

            button.onClick.AddListener(delegate { TimeManager.i.SetTimeScale(value); } );
        }
    }

    [SerializeField] private RequestedManager reqMan;

    [SerializeField] private float value;

    public enum RequestedManager
    { 
        None,
        TimeManager
    }

}
