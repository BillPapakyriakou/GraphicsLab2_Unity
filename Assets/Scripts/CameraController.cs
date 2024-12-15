using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    float moveSpeed = 10f;     // movement speed on x and z axis 
    float heightSpeed = 10f;   // zoom speed  
    float rotationSpeed = 50f; // rotation speed

    void Update()
    {
        // Move Left and Right using A and D or left and right arrow keys
        float moveX = Input.GetAxis("Horizontal"); 
        float moveZ = 0f;

        // Move up and down using W and S
        if (Input.GetKey(KeyCode.W))  
        {
            moveZ = 1f;
        }
        else if (Input.GetKey(KeyCode.S))  
        {
            moveZ = -1f;
        }

        // Move up and down using Arrow-Up and Arrow-Down
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveZ = 1f;
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            moveZ = -1f;
        }

        // Zoom in & Zoom out (8 - 9)
        if (Input.GetKey(KeyCode.Alpha9))  
        {
            transform.position += Vector3.up * heightSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Alpha8)) 
        {
            transform.position -= Vector3.up * heightSpeed * Time.deltaTime;
        }

       
        Vector3 localMovement = transform.right * moveX + transform.up * moveZ;
        transform.Translate(localMovement * moveSpeed * Time.deltaTime, Space.World); 

        // Rotation around itself on the Y - Axis
        if (Input.GetKey(KeyCode.R))
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }

        // Rotation around itself on the X - Axis
        if (Input.GetKey(KeyCode.F))
        {
            transform.Rotate(Vector3.left, rotationSpeed * Time.deltaTime);
        }
    }

}
