using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.PackageManager;
using UnityEngine;

public class Inputmanger : MonoBehaviour
{

    public void Update()
    {
        var mousePos = Input.mousePosition;
        mousePos.z = 1f;
        var worldMousePos = Camera.main.ScreenToWorldPoint(mousePos);
        RaycastHit hitInfo;
        Physics.Raycast(Camera.main.transform.position, (worldMousePos - Camera.main.transform.position).normalized, out hitInfo);

        if (hitInfo.collider != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if(hitInfo.collider.gameObject.tag == "Node")
                {
                    hitInfo.collider.gameObject.GetComponent<Node>().ShowMenu(true);
                }
            }
        }
    }


}
