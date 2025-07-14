using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.UIElements;
using static UnityEditor.ShaderGraph.Internal.KeywordDependentCollection;

public class InputManager : MonoBehaviour
{

    private void Start()
    {
        iM = this;
    }

    private void InputUpdate()
    {
        prevWorldMousePos = curWorldMousePos;
        mouseButtonDown = Input.GetMouseButtonDown(0);
        mouseButtonHeld = Input.GetMouseButton(0);
        mouseButtonRightDown = Input.GetMouseButtonDown(1);

        curWorldMousePos = Input.mousePosition;
        curWorldMousePos.z = 1f;
        var worldMousePos = Camera.main.ScreenToWorldPoint(curWorldMousePos);
        Physics.Raycast(Camera.main.transform.position, (worldMousePos - Camera.main.transform.position).normalized, out worldRaycastHitInfo);
    }

    private void FunctionUpdate()
    {
        if (worldRaycastHitInfo.collider == null)
        {
            ClearCSV();
            return;
        }

        if (worldRaycastHitInfo.collider.tag == "HUDScreen")
        {
            if(HUDManager.hM == null)
            {
                return;
            }

            CameraCursor camCursor = ScreenPlane.smallScreen.cameraCursor;

            if (camCursor == null)
            {
                return;
            }

            prevSmallScreenPos = curSmallScreenPos;
            prevScreenRayCastHitInfo = ScreenRayCastHitInfo;

            var screenRelativePos = worldRaycastHitInfo.transform.position - worldRaycastHitInfo.point;
            Vector3 screenBounds = worldRaycastHitInfo.collider.bounds.size / 2;
            curSmallScreenPos = new Vector3(screenRelativePos.x / screenBounds.x * 4.5f, -(screenRelativePos.y / screenBounds.y) * 10f); // x is times by 1 / 3 as the map screen has a 1 / 3 ratio
            Vector3 rayShootPos = Vector3.zero;

            if (camCursor.ortho)
            {
                rayShootPos = curSmallScreenPos * camCursor.camera.orthographicSize;
            }
            else
            {
                float cameraFrustrumHeight = Mathf.Tan(camCursor.camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                rayShootPos = curSmallScreenPos * cameraFrustrumHeight;
            }

            camCursor.MoveCursor(rayShootPos);

            if (camCursor.ortho)
            {
                Physics.Raycast(camCursor.cursor.transform.position, camCursor.camera.transform.forward, out ScreenRayCastHitInfo);
                Debug.DrawRay(camCursor.cursor.transform.position, camCursor.camera.transform.forward, Color.red, 2f);
            }
            else
            {
                Physics.Raycast(camCursor.transform.position, Vector3.Normalize(camCursor.cursor.transform.position - camCursor.camera.transform.position), out ScreenRayCastHitInfo);
                Debug.DrawRay(camCursor.transform.position, Vector3.Normalize(camCursor.cursor.transform.position - camCursor.camera.transform.position), Color.red, 2f);
            }

            if (ScreenRayCastHitInfo.collider != null)
            {
                if (mouseButtonDown)
                {
                    if (ScreenRayCastHitInfo.collider.gameObject.tag == "Button")
                    {
                        var buttonInfo = ScreenRayCastHitInfo.collider.gameObject.GetComponent<UserButton>();
                        ActionManager.aM.PerformButtonAction(buttonInfo);
                    }

                    if (ScreenRayCastHitInfo.collider.gameObject.tag == "NewButton")
                    {
                        var buttonInfo = ScreenRayCastHitInfo.collider.gameObject.GetComponent<ButtonAssigner>();
                        if (buttonInfo.reqMan == ButtonAssigner.RequestedManager.TimeManager)
                        {
                            TimeManager.tM.SetTimeScale(buttonInfo.value);
                        }
                        if (buttonInfo.reqMan == ButtonAssigner.RequestedManager.HUDManager)
                        {
                            NodeManager.nM.CloseAllNodeMenus(null);
                        }
                    }

                    if (ScreenRayCastHitInfo.collider.gameObject.tag == "UniversalButton")
                    {
                        var universalButton = ScreenRayCastHitInfo.collider.gameObject.GetComponent<UniversalButton>();
                        universalButton.PerformAction();
                    }
                }

                if (ScreenRayCastHitInfo.collider.gameObject.tag == "UniversalButton")
                {
                    if ((prevScreenRayCastHitInfo.collider != null && prevScreenRayCastHitInfo.collider.gameObject.tag != "UniversalButton") || prevScreenRayCastHitInfo.collider == null)
                    {
                        var universalButton = ScreenRayCastHitInfo.collider.gameObject.GetComponent<UniversalButton>();
                        universalButton.OnHover(true);
                    }
                }
                else if ((prevScreenRayCastHitInfo.collider != null && prevScreenRayCastHitInfo.collider.gameObject.tag == "UniversalButton"))
                {
                    var universalButton = prevScreenRayCastHitInfo.collider.gameObject.GetComponent<UniversalButton>();
                    universalButton.OnHover(false);
                }
            }
            else if ((prevScreenRayCastHitInfo.collider != null && prevScreenRayCastHitInfo.collider.gameObject.tag == "UniversalButton"))
            {
                var universalButton = prevScreenRayCastHitInfo.collider.gameObject.GetComponent<UniversalButton>();
                universalButton.OnHover(false);
            }
        }

        if (worldRaycastHitInfo.collider.tag == "BigScreen")
        {
            CameraCursor camCursor = ScreenPlane.bigScreen.cameraCursor;

            if(camCursor == null)
            {
                return;
            }

            prevBigScreenPos = curBigScreenPos;
            prevScreenRayCastHitInfo = ScreenRayCastHitInfo;

            var screenRelativePos = worldRaycastHitInfo.transform.position - worldRaycastHitInfo.point;
            Vector3 screenBounds = worldRaycastHitInfo.collider.bounds.size / 2;
            curBigScreenPos = new Vector3(screenRelativePos.x / screenBounds.x * 4 / 3, -(screenRelativePos.y / screenBounds.y)); // Big screen has a 4/3 ratio
            Vector3 rayShootPos = Vector3.zero;

            if (camCursor.ortho)
            {
                rayShootPos = curBigScreenPos * camCursor.camera.orthographicSize * 10f;
            }
            else
            {
                float cameraFrustrumHeight = Mathf.Tan(camCursor.camera.fieldOfView * 0.5f * Mathf.Deg2Rad);
                rayShootPos = curBigScreenPos * cameraFrustrumHeight;
            }

            camCursor.MoveCursor(rayShootPos);

            if (camCursor.ortho)
            {
                Physics.Raycast(camCursor.cursor.transform.position, camCursor.camera.transform.forward, out ScreenRayCastHitInfo);
                Debug.DrawRay(camCursor.cursor.transform.position, camCursor.camera.transform.forward, Color.red, 2f);
            }
            else
            {
                Physics.Raycast(camCursor.transform.position, Vector3.Normalize(camCursor.cursor.transform.position - camCursor.camera.transform.position), out ScreenRayCastHitInfo);
                Debug.DrawRay(camCursor.transform.position, Vector3.Normalize(camCursor.cursor.transform.position - camCursor.camera.transform.position), Color.red, 2f);
            }

            if (ScreenRayCastHitInfo.collider != null)
            {
                if (ScreenRayCastHitInfo.collider.tag == "ScrollView")
                {
                    currentCSV = ScreenRayCastHitInfo.collider.gameObject.GetComponent<CustomScrollView>();
                }

                if (ScreenRayCastHitInfo.collider.tag == "ScrollbarHandle")
                {
                    currentCSV = ScreenRayCastHitInfo.collider.gameObject.GetComponent<CustomScrollHandle>().cSV;
                }

                if (ScreenRayCastHitInfo.collider.gameObject.tag == "UniversalButton")
                {
                    if (prevScreenRayCastHitInfo.collider != null && prevScreenRayCastHitInfo.collider.gameObject.tag != "UniversalButton")
                    {
                        var universalButton = ScreenRayCastHitInfo.collider.gameObject.GetComponent<UniversalButton>();
                        universalButton.OnHover(true);
                    }
                }
                else if (prevScreenRayCastHitInfo.collider != null && prevScreenRayCastHitInfo.collider.gameObject.tag == "UniversalButton")
                {
                    var universalButton = prevScreenRayCastHitInfo.collider.gameObject.GetComponent<UniversalButton>();
                    universalButton.OnHover(false);
                }

                if (mouseButtonDown)
                {
                    if (ScreenRayCastHitInfo.collider.gameObject.tag == "Node")
                    {
                        //NodeManager.nM.CloseAllNodeMenus();
                        ScreenRayCastHitInfo.collider.transform.parent.GetComponent<Node>().ShowMenu(true);
                    }

                    if (ScreenRayCastHitInfo.collider.gameObject.tag == "Button")
                    {
                        var buttonInfo = ScreenRayCastHitInfo.collider.gameObject.GetComponent<UserButton>();
                        ActionManager.aM.PerformButtonAction(buttonInfo);
                    }

                    if (ScreenRayCastHitInfo.collider.gameObject.tag == "UniversalButton")
                    {
                        var universalButton = ScreenRayCastHitInfo.collider.gameObject.GetComponent<UniversalButton>();
                        universalButton.PerformAction();
                    }

                    //MAKE NEW BUTTON TYPE HERE FOR TRIGERRING THE CONTACT BUTTON, SO THAT IT CAN SEND THE INFO FROM ITSELF TO THE SECTION MANAGER AND THEN DISPLAY THE CHAT LOG

                    if (ScreenRayCastHitInfo.collider.gameObject.tag == "ScrollbarHandle")
                    {
                        interactingWithScrollBar = true;
                        currentCSV = ScreenRayCastHitInfo.collider.gameObject.GetComponent<CustomScrollHandle>().cSV;
                    }
                }
            }

            if (interactingWithScrollBar)
            {
                if (!mouseButtonHeld && !mouseButtonDown)
                {
                    interactingWithScrollBar = false;
                }
                else
                {
                    currentCSV.MoveHandle((curBigScreenPos.y - prevBigScreenPos.y) * 500f);
                }
            }

            if (mouseButtonHeld && !mouseButtonDown)
            {
                if (StateManager.sM.gameState == GameState.Mission)
                {
                    mCC.MoveCamera(prevBigScreenPos - curBigScreenPos);
                }
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                if (currentCSV != null)
                {
                    currentCSV.MoveHandle(Input.GetAxis("Mouse ScrollWheel") * 200f);
                }

                if (StateManager.sM.gameState == GameState.Mission)
                {
                    mCC.ZoomCamera(Input.GetAxis("Mouse ScrollWheel"));
                }
            }


            if (worldRaycastHitInfo.collider.gameObject.tag == "Button")
            {
                var buttonInfo = worldRaycastHitInfo.collider.gameObject.GetComponent<UserButton>();
                ActionManager.aM.PerformButtonAction(buttonInfo);
            }

            prevWorldMousePos = curWorldMousePos;
        }
    }

    public void ClearCSV()
    {
        currentCSV = null;
    }

    public void Update()
    {
        InputUpdate();
        FunctionUpdate();
    }

    public void SetMCC(MapCameraController newMCC)
    {
        mCC = newMCC;
    }

    public static InputManager iM;

    private MapCameraController mCC;

    private bool mouseButtonDown;
    private bool mouseButtonHeld;
    private bool mouseButtonRightDown;
    private RaycastHit worldRaycastHitInfo;
    private RaycastHit HUDRayCastHitInfo;

    private RaycastHit ScreenRayCastHitInfo;
    private RaycastHit prevScreenRayCastHitInfo;

    private Vector3 curWorldMousePos;
    private Vector3 prevWorldMousePos;

    private Vector3 curSmallScreenPos;
    private Vector3 prevSmallScreenPos;

    private Vector3 curBigScreenPos;
    private Vector3 prevBigScreenPos;

    private CustomScrollView currentCSV;

    private bool interactingWithScrollBar;

}
