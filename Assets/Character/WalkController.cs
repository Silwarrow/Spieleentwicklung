using UnityEngine;
using UnityEngine.InputSystem;

// Stays enabled at all times, because it has to read the toggle input in both modes.
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(FlightController))]
public class WalkController : MonoBehaviour
{
    InputAction moveAction;
    InputAction jumpAction;
    InputAction toggleFlightAction;

    CharacterController controller;
    FlightController flightController;

    public float speed = 6f;
    public float turnSpeed = 12f;
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    float verticalVelocity;
    bool isFlying;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        flightController = GetComponent<FlightController>();

        //game starts in walk mode
        flightController.enabled = false;
    }

    void Start()
    {
        //you can find these actions under Edit > Project Settings... > Input System Package
        // throws a descriptive error if an action is missin
        moveAction = InputSystem.actions.FindAction("Move", true);
        jumpAction = InputSystem.actions.FindAction("Jump", true);
        toggleFlightAction = InputSystem.actions.FindAction("ToggleFlight", true);
    }

    void Update()
    {
        //toggle is read in Update, since button presses can be missed in FixedUpdate
        if (toggleFlightAction.WasPressedThisFrame())
        {
            SetFlying(!isFlying);
        }

        if (!isFlying)
        {
            Walk();
        }
    }

    void Walk()
    {
        Vector2 input = moveAction.ReadValue<Vector2>();

        //world space movement, same as FlightController (x -> world X, y -> world Z)
        Vector3 direction = Vector3.ClampMagnitude(new Vector3(input.x, 0f, input.y), 1f);

        //turn smoothly towards walking direction
        if (direction.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }

        //small downward force keeps isGrounded stable
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        if (controller.isGrounded && jumpAction.WasPressedThisFrame())
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 velocity = direction * speed + Vector3.up * verticalVelocity;
        controller.Move(velocity * Time.deltaTime);
    }

    void SetFlying(bool flying)
    {
        isFlying = flying;

        if (flying)
        {
            //CharacterController would fight FlightController's direct transform.position writes
            controller.enabled = false;
            flightController.enabled = true;
        }
        else
        {
            flightController.enabled = false;

            transform.rotation = Quaternion.identity;

            verticalVelocity = 0f;
            controller.enabled = true;
        }
    }
}
