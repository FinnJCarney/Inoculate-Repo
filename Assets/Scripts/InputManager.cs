using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
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
            return;
        }

        if (worldRaycastHitInfo.collider.tag == "MapScreen")
        {
            prevMapRelativePos = curMapRelativePos;

            var screenRelativePos = worldRaycastHitInfo.transform.position - worldRaycastHitInfo.point;
            Vector3 screenBounds = worldRaycastHitInfo.collider.bounds.size / 2;
            curMapRelativePos = new Vector3(screenRelativePos.x / screenBounds.x * 4 / 3, -(screenRelativePos.y / screenBounds.y)); // x is times by 4 / 3 as the map screen has a 4/3 ratio
            float cameraFrustrumHeight = Mathf.Tan(mCC.mapCamera.fieldOfView * 0.5f * Mathf.Deg2Rad);
            Vector3 rayShootPos = curMapRelativePos * cameraFrustrumHeight; //previous camera.orthographic
            mCC.MoveCursor(rayShootPos);

            if (mouseButtonDown)
            {
                Physics.Raycast(mCC.cursor.transform.position, Vector3.Normalize(mCC.cursor.transform.position - mCC.transform.position), out mapRayCastHitInfo);

                if (mapRayCastHitInfo.collider != null)
                {
                    if (mapRayCastHitInfo.collider.gameObject.tag == "Node")
                    {
                        //NodeManager.nM.CloseAllNodeMenus();
                        mapRayCastHitInfo.collider.transform.parent.GetComponent<Node>().ShowMenu(true);
                    }

                    if (mapRayCastHitInfo.collider.gameObject.tag == "Button")
                    {
                        var buttonInfo = mapRayCastHitInfo.collider.gameObject.GetComponent<UserButton>();
                        ActionManager.aM.PerformButtonAction(buttonInfo);
                    }
                }
            }

            if(mouseButtonHeld && !mouseButtonDown)
            {
                mCC.MoveCamera(prevMapRelativePos - curMapRelativePos);
            }

            if (Input.GetAxis("Mouse ScrollWheel") != 0)
            {
                mCC.ZoomCamera(Input.GetAxis("Mouse ScrollWheel"));
            }
        }

        if (worldRaycastHitInfo.collider.tag == "HUDScreen")
        {
            if(HUDManager.hM == null)
            {
                return;
            }

            CameraCursor hudCursor = HUDManager.hM.cursor;
            prevHUDRelativePos = curHUDRelativePos;

            var screenRelativePos = worldRaycastHitInfo.transform.position - worldRaycastHitInfo.point;
            Vector3 screenBounds = worldRaycastHitInfo.collider.bounds.size / 2;
            curMapRelativePos = new Vector3(screenRelativePos.x / screenBounds.x * 4.5f, -(screenRelativePos.y / screenBounds.y) * 10f); // x is times by 1 / 3 as the map screen has a 1 / 3 ratio
            Vector3 rayShootPos = curMapRelativePos * hudCursor.camera.orthographicSize; //previous camera.orthographic
            hudCursor.MoveCursor(rayShootPos);
            Debug.DrawRay(hudCursor.cursor.transform.position, hudCursor.camera.transform.forward, Color.red, 10f);

            if (mouseButtonDown)
            {
                Physics.Raycast(hudCursor.cursor.transform.position, hudCursor.camera.transform.forward, out HUDRayCastHitInfo);

                if (HUDRayCastHitInfo.collider != null)
                {
                    if (HUDRayCastHitInfo.collider.gameObject.tag == "Button")
                    {
                        var buttonInfo = HUDRayCastHitInfo.collider.gameObject.GetComponent<UserButton>();
                        ActionManager.aM.PerformButtonAction(buttonInfo);
                    }

                    if (HUDRayCastHitInfo.collider.gameObject.tag == "NewButton")
                    {
                        var buttonInfo = HUDRayCastHitInfo.collider.gameObject.GetComponent<ButtonAssigner>();
                        if(buttonInfo.reqMan == ButtonAssigner.RequestedManager.TimeManager)
                        {
                            TimeManager.tM.SetTimeScale(buttonInfo.value);
                        }
                        if(buttonInfo.reqMan == ButtonAssigner.RequestedManager.HUDManager)
                        {
                            NodeManager.nM.CloseAllNodeMenus(null);
                        }
                    }
                }
            }
        }

        if (worldRaycastHitInfo.collider.tag == "BigScreen")
        {
            CameraCursor camCursor = ScreenPlane.bigScreen.cameraCursor;
            prevBigScreenPos = curBigScreenPos;

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

            if (mouseButtonDown)
            {
                Physics.Raycast(camCursor.cursor.transform.position, camCursor.camera.transform.forward, out ScreenRayCastHitInfo);
            }

            if(mouseButtonHeld)
            {

                
            }
            
        }

        if (worldRaycastHitInfo.collider.gameObject.tag == "Button")
        {
            var buttonInfo = worldRaycastHitInfo.collider.gameObject.GetComponent<UserButton>();
            ActionManager.aM.PerformButtonAction(buttonInfo);
        }

        prevWorldMousePos = curWorldMousePos;
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
    private RaycastHit mapRayCastHitInfo;
    private RaycastHit HUDRayCastHitInfo;

    private RaycastHit ScreenRayCastHitInfo;

    private Vector3 curWorldMousePos;
    private Vector3 prevWorldMousePos;

    private Vector3 curMapRelativePos;
    private Vector3 prevMapRelativePos;

    private Vector3 curHUDRelativePos;
    private Vector3 prevHUDRelativePos;

    private Vector3 curBigScreenPos;
    private Vector3 prevBigScreenPos;

}
