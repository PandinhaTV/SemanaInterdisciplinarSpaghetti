using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMoviment : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 8f;
    public float gravity = -20f;

    private CharacterController controller;
    private InputSystem_Actions inputActions;
    private GrapplingHook grapplingHook;
    private Dash dash;
    private Animator animator;
    private Slide slide;
    public SpriteRenderer spriteRenderer;
    public Vector2 moveInput;
    public float verticalVelocity;
    private bool jumpPressed;
    private Vector3 externalVelocity = Vector3.zero;

    // Animation hashes — faster than passing strings every frame
    private static readonly int HashSpeed    = Animator.StringToHash("Speed");
    private static readonly int HashGrounded = Animator.StringToHash("Grounded");
    private static readonly int HashVertical = Animator.StringToHash("VerticalVelocity");

    void Awake()
    {
        controller    = GetComponent<CharacterController>();
        grapplingHook = GetComponent<GrapplingHook>();
        dash          = GetComponent<Dash>();
        animator      = GetComponentInChildren<Animator>(); // works if Animator is on a child sprite
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        slide = GetComponent<Slide>();
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled  += ctx => moveInput = Vector2.zero;
        inputActions.Player.Jump.performed += ctx => jumpPressed = true;
    }

    public void SetExternalVelocity(Vector3 v)
    {
        externalVelocity = v;
        verticalVelocity = v.y;
    }

    void OnEnable()  => inputActions.Enable();
    void OnDisable() => inputActions.Disable();

    void Update()
    {
        if (grapplingHook.IsGrappling)
        {
            externalVelocity = Vector3.zero;
            UpdateAnimations();
            return;
        }

        if (dash.IsDashing) return;
        if (slide != null && slide.IsSliding) return;
        if (controller.isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
            externalVelocity = Vector3.zero;
        }

        if (jumpPressed && controller.isGrounded)
            verticalVelocity = jumpForce;

        jumpPressed = false;

        verticalVelocity += gravity * Time.deltaTime;

        externalVelocity = Vector3.Lerp(externalVelocity, Vector3.zero, Time.deltaTime * 3f);

        Vector3 move = new Vector3(moveInput.x * moveSpeed + externalVelocity.x, 0, 0);
        move.y = verticalVelocity;

        controller.Move(move * Time.deltaTime);

        // Flip sprite based on movement direction
        if (moveInput.x > 0.1f)
            spriteRenderer.flipX = false;
        else if (moveInput.x < -0.1f)
            spriteRenderer.flipX = true;

        UpdateAnimations();
    }

    void UpdateAnimations()
    {
        if (animator == null) return;

        animator.SetFloat("Speed",           Mathf.Abs(moveInput.x));
        animator.SetBool ("Grounded",        controller.isGrounded);
        animator.SetFloat("VerticalVelocity", verticalVelocity);
    }

    // Called by StarThrow to trigger the attack animation
    public void PlayThrowAnimation()
    {
        if (animator == null) return;
        animator.SetTrigger("Throw");
    }
}