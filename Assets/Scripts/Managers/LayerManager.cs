using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerManager : MonoBehaviour
{
    private void Start()
    {
        LayerManager.lM = this;
    }

    public void SetLayer(connectionLayer startingLayer, connectionLayer layerOptions)
    {
        if(startingLayer == connectionLayer.online)
        {
            online = true;
        }
        else if(startingLayer == connectionLayer.offline)
        {
            online = false;
        }

        activeLayer = online ? connectionLayer.online : connectionLayer.offline;

        if (buttonImage != null)
        {
            buttonImage.sprite = online ? onlineSpr : offlineSpr;
        }

        VisualsManager.vM.SwapLayer(online);
    }

    public void ChangeLayer()
    {
        online = !online;
        activeLayer = online ? connectionLayer.online : connectionLayer.offline;
        if (buttonImage != null)
        {
            buttonImage.sprite = online ? onlineSpr : offlineSpr;
        }
        VisualsManager.vM.SwapLayer(online);
    }

    public static LayerManager lM;

    public bool online;
    [SerializeField] public Image buttonImage;
    [SerializeField] private Sprite offlineSpr;
    [SerializeField] private Sprite onlineSpr;

    public connectionLayer activeLayer;

}
