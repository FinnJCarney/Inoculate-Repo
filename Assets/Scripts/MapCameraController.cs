using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        defaultY = this.transform.position.y;
        zoomAmount = 0;
        zoomDir = this.transform.forward;
    }

    public void MoveCamera(Vector3 vel)
    {
        Vector3 velocity = new Vector3(vel.x, 0, vel.y) * (50f + (25f * -zoomAmount)) * speed;

        transform.position += velocity;
    }

    public void ZoomCamera(float newZoomAmount)
    {
        var velocity = Vector3.zero;
        
        if (newZoomAmount + zoomAmount > 1f)
        {
            zoomAmount = 1;
        }
        else if (newZoomAmount + zoomAmount < -1f)
        {
            zoomAmount = -1;
        }
        else
        {
            zoomAmount += newZoomAmount;
            velocity += (zoomDir * newZoomAmount * zoomMagnitude);
        }

        transform.position += velocity;
    }

    private float defaultY; //Use this to clamp zooming? Idk
    private float zoomAmount;
    private Vector3 zoomDir;
    private float pinchDis;
    [SerializeField] float zoomMagnitude;
    [SerializeField] private float speed; 
}
