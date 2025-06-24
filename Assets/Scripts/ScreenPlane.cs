using UnityEngine;

public class ScreenPlane : MonoBehaviour
{
    private void Awake()
    {
        if(screenType == ScreenType.bigScreen)
        {
            bigScreen = this;
        }
        else if(screenType == ScreenType.smallScreen)
        {
            smallScreen = this;
        }
        else if(screenType == ScreenType.phoneScreen)
        {
            phoneScreen = this;
        }
    }

    public void SetCameraCursor(CameraCursor cC, bool orthoOrPersp)
    {
        cameraCursor = cC;
        ortho = orthoOrPersp;
    }

    [SerializeField] private ScreenType screenType;

    public static ScreenPlane bigScreen;
    public static ScreenPlane smallScreen;
    public static ScreenPlane phoneScreen;

    public bool ortho;

    public CameraCursor cameraCursor;
}

public enum ScreenType
{ 
    bigScreen,
    smallScreen,
    phoneScreen,
    none
}

