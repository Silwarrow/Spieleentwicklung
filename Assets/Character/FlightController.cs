using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;


public class FlightController : MonoBehaviour
{
    InputAction flyAction;
    
    float speed = 30f;

    float xRotationAngle;
    float zRotationAngle;

    // Start is called before the first frame update.
    void Start()
    {
        //you can find this action under Edit > Project Settings... > Input System Package
        flyAction = InputSystem.actions.FindAction("Fly");
    }
    
    void FixedUpdate()
    {
        //read current input from input device (for keyboard: A/D, W/S)
        Vector2 movementVector = flyAction.ReadValue<Vector2>();

        //rotate player based on input (may switch to something more intuitive later)
        //research to do on: yaw, pitch, roll and possible implementations in unity
        //to do: follow notes on figma further, see if possible to implement

        //convert input to target angles
        xRotationAngle = movementVector.y * 10;
        zRotationAngle = movementVector.x * -45;
        Quaternion targetRotation = Quaternion.Euler(xRotationAngle, 0, zRotationAngle);
        //tilt player slightly towards target rotation
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, 0.5f);


        //move player in world based on input
        //to do: figure out how to move in y direction (up/down)

        Vector3 movement = new Vector3(movementVector.x, 0f, movementVector.y) * speed * Time.deltaTime;
        transform.position += movement;

        //to do: research ray casting
    }
}
