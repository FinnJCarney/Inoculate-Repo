using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LayerManager : MonoBehaviour
{
    public void ChangeLayer()
    {
        online = !online;
        buttonImage.sprite = online ? onlineSpr : offlineSpr;
        VisualsManager.vM.SwapLayer(online);
    }

    public bool online;
    [SerializeField] private Image buttonImage;
    [SerializeField] private Sprite offlineSpr;
    [SerializeField] private Sprite onlineSpr;

}
