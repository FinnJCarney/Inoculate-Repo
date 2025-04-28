using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Inputmanger : MonoBehaviour
{

    private void InputUpdate()
    {
        prevMousePos = curMousePos;
        mouseButtonDown = Input.GetMouseButtonDown(0);
        mouseButtonHeld = Input.GetMouseButton(0);
        mouseButtonRightDown = Input.GetMouseButtonDown(1);

        curMousePos = Input.mousePosition;
        curMousePos.z = 1f;
        var worldMousePos = Camera.main.ScreenToWorldPoint(curMousePos);
        Physics.Raycast(Camera.main.transform.position, (worldMousePos - Camera.main.transform.position).normalized, out raycastHitInfo);
    }

    private void FunctionUpdate()
    {
        if (mouseButtonDown)
        {
            if (raycastHitInfo.collider != null)
            {
                if (raycastHitInfo.collider.gameObject.tag == "Node")
                {
                    //NodeManager.nM.CloseAllNodeMenus();
                    raycastHitInfo.collider.transform.parent.GetComponent<Node>().ShowMenu(true);
                }

                if(raycastHitInfo.collider.gameObject.tag == "Button")
                {
                    var buttonInfo = raycastHitInfo.collider.gameObject.GetComponent<UserButton>();
                    ActionManager.aM.PerformButtonAction(buttonInfo);
                }
            }

            prevMousePos = curMousePos;
        }

        if(mouseButtonHeld)
        {
            Camera.main.GetComponent<CameraController>().MoveCamera(Camera.main.ScreenToWorldPoint(prevMousePos) - Camera.main.ScreenToWorldPoint(curMousePos));
        }

        if(mouseButtonRightDown)
        {
            NodeManager.nM.CloseAllNodeMenus(null);
        }
    }

    public void Update()
    {
        InputUpdate();
        FunctionUpdate();
    }

    private bool mouseButtonDown;
    private bool mouseButtonHeld;
    private bool mouseButtonRightDown;
    private RaycastHit raycastHitInfo;

    private Vector3 curMousePos;
    private Vector3 prevMousePos;

}
