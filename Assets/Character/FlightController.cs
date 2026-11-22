using UnityEngine;
using UnityEngine.InputSystem;


public class FlightController : MonoBehaviour
{
    InputAction flyAction;
    // Movement along X and Y axes.
    private float movementX;
    private float movementY;
    float speed = 5f;

    // Start is called before the first frame update.
    void Start()
    {
        Debug.Log("Start ran");
        transform.position += new Vector3(20f, 20f, 40f);
        flyAction = InputSystem.actions.FindAction("Fly");
    }
    
    void Update()
    {
        /*var kb = Keyboard.current;
        Debug.Log($"Update ran; Keyboard.current is {(kb == null ? "NULL" : "present")}");

        if (kb == null) return;

        //movementX = (kb.dKey.isPressed ? 1f : 0f) - (kb.aKey.isPressed ? 1f : 0f);
        //movementY = (kb.wKey.isPressed ? 1f : 0f) - (kb.sKey.isPressed ? 1f : 0f);

        if (movementX != 0 || movementY != 0)
            Debug.Log($"Polling sees movement: ({movementX}, {movementY})");
        */

        Vector2 movementVector = flyAction.ReadValue<Vector2>();
        //Debug.Log(movementVector);
        // Store the X and Y components of the movement.
        movementX = movementVector.x;
        movementY = movementVector.y;
        // Create a 3D movement vector using the X and Y inputs.
        Vector3 movement = new Vector3(movementVector.x, 0f, movementVector.y) * speed * Time.deltaTime;
        transform.position += movement;
        //Debug.Log(movementVector);
    }
}
