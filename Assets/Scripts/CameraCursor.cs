using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraCursor : MonoBehaviour
{
    private void Start()
    {
        if (screenType == ScreenType.bigScreen)
        {
            ScreenPlane.bigScreen.SetCameraCursor(this, ortho);
        }
        else if (screenType == ScreenType.smallScreen)
        {
            ScreenPlane.smallScreen.SetCameraCursor(this, ortho);
        }
        else if (screenType == ScreenType.phoneScreen)
        {
            ScreenPlane.phoneScreen.SetCameraCursor(this, ortho);
        }
    }

    public void MoveCursor(Vector3 pos) // Move Cursor to appropriate part of screen
    {
        cursor.transform.localPosition = new Vector3(pos.x, pos.y, 1f);
    }

    [SerializeField] private ScreenType screenType;

    public Camera camera;

    public bool ortho;

    [SerializeField] public GameObject cursor;
}
