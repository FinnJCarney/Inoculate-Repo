using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDManager : MonoBehaviour
{
    private void Start()
    {
        var canvas = GetComponent<Canvas>();

        canvas.worldCamera = Camera.main;
    }
}
