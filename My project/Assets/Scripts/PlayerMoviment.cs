using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoviment : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -20f;

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private GrapplingHook grapplingHook; // ADD THIS

    private Vector2 moveInput;
    private float verticalVelocity;
    private bool jumpPressed;
    private Vector3 externalVelocity = Vector3.zero;
    void Awake()
    {
        controller = GetComponent<CharacterController>();
        grapplingHook = GetComponent<GrapplingHook>(); // ADD THIS

        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
    }

    public void SetExternalVelocity(Vector3 v)
    {
        externalVelocity = v;
        verticalVelocity = v.y; // sync vertical so gravity continues naturally
    }
    void OnEnable() => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        if (grapplingHook.IsGrappling)
        {
            externalVelocity = Vector3.zero;
            return;
        }

        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            externalVelocity = Vector3.zero; // kill horizontal launch when landing
        }

        if (jumpPressed && controller.isGrounded)
            verticalVelocity = jumpForce;

        jumpPressed = false;

        verticalVelocity += gravity * Time.deltaTime;

        // Blend external (swing) velocity with normal movement over time
        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, Time.deltaTime * 3f);

        Vector3 move = new Vector3(moveInput.x * moveSpeed + externalVelocity.x, 0, 0);
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);
    }
}