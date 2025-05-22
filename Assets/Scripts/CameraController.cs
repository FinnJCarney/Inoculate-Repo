using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        defaultY = this.transform.position.y;
        zoomAmount = 0;
        zoomDir = this.transform.forward;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized * Time.deltaTime * speed;


        if(Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            var newZoomAmount = Input.GetAxis("Mouse ScrollWheel");

            if(newZoomAmount + zoomAmount > 1f)
            {
                zoomAmount = 1;
            }
            else if(newZoomAmount + zoomAmount < -1f)
            {
                zoomAmount = -1;
            }
            else
            {
                zoomAmount += newZoomAmount;
                velocity += (zoomDir * newZoomAmount * zoomMagnitude);
            }
        }

        if(Input.touchCount > 1)
        {
            foreach(Touch touch in Input.touches)
            {
                if(touch.phase == TouchPhase.Began)
                {
                    pinchDis = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
                }
            }

            float newDistance = Vector2.Distance(Input.touches[0].position, Input.touches[1].position);
            float disChange = (pinchDis - newDistance) * 0.005f;
            Debug.Log(disChange);
            if (Mathf.Abs(disChange) > 0.01f)
            {
                if (disChange + zoomAmount > 1f)
                {
                    zoomAmount = 1;
                }
                else if (disChange + zoomAmount < -1f)
                {
                    zoomAmount = -1;
                }
                else
                {
                    zoomAmount += disChange;
                    velocity += (zoomDir * -disChange * zoomMagnitude);
                }
            }

            pinchDis = newDistance;
        }
        


        transform.position += velocity;
    }

    public void MoveCamera(Vector3 vel)
    {
        Vector3 velocity = new Vector3(vel.x, 0, vel.y) * (50f + (25f * -zoomAmount));

        transform.position += velocity;
    }


    private float defaultY; //Use this to clamp zooming? Idk
    private float zoomAmount;
    private Vector3 zoomDir;
    private float pinchDis;
    [SerializeField] float zoomMagnitude;
    [SerializeField] private float speed; 
}
