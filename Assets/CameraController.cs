using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        defaultY = this.transform.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 velocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")).normalized * Time.deltaTime * speed;



        transform.position += velocity;
    }

    public void MoveCamera(Vector3 vel)
    {
        Vector3 velocity = new Vector3(vel.x, 0, vel.y) * 0.02f;

        transform.position += velocity;
    }


    private float defaultY; //Use this to clamp zooming? Idk
    [SerializeField] private float speed; 
}
