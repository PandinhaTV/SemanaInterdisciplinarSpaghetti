using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDoubleJump : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerMoviment playerMoviment;
    [SerializeField] private CharacterController controller;
    [SerializeField] private GrapplingHook grapplingHook;
    private SkillCheck skillCheck;

    [Header("Settings")]
    [SerializeField] private int maxExtraJumps = 1;
    [SerializeField] private float doubleJumpForce = 8f;

    [Header("Input")]
    public InputActionReference jumpAction;

    private int extraJumpsLeft;
    private bool wasGrounded;
    private bool isWaitingForSkillCheck = false; // prevents queuing multiple checks

    void Awake()
    {
        if (playerMoviment == null)
            playerMoviment = GetComponent<PlayerMoviment>();

        if (controller == null)
            controller = GetComponent<CharacterController>();

        if (grapplingHook == null)
            grapplingHook = GetComponent<GrapplingHook>();

        skillCheck = FindObjectOfType<SkillCheck>();
    }

    void OnEnable()
    {
        jumpAction.action.Enable();
        jumpAction.action.performed += OnJumpPerformed;
    }

    void OnDisable()
    {
        jumpAction.action.performed -= OnJumpPerformed;
        jumpAction.action.Disable();
    }

    void Start()
    {
        extraJumpsLeft = maxExtraJumps;
        wasGrounded = controller.isGrounded;
    }

    void Update()
    {
        bool isGrounded = controller.isGrounded;

        // Reset extra jumps when landing
        if (isGrounded && !wasGrounded)
        {
            extraJumpsLeft = maxExtraJumps;
            isWaitingForSkillCheck = false;
        }

        wasGrounded = isGrounded;
    }

    private void OnJumpPerformed(InputAction.CallbackContext ctx)
    {
        // Block if grappling, grounded, no jumps left, or already in a skill check
        if (grapplingHook != null && grapplingHook.IsGrappling) return;
        if (controller.isGrounded) return;
        if (extraJumpsLeft <= 0) return;
        if (isWaitingForSkillCheck) return;

        isWaitingForSkillCheck = true;

        skillCheck.StartSkillCheck(jumpAction.action, (bool success) =>
        {
            isWaitingForSkillCheck = false;

            if (!success) return;

            // Reset and apply double jump force
            playerMoviment.verticalVelocity = 0f;
            playerMoviment.verticalVelocity = doubleJumpForce;

            extraJumpsLeft--;
        });
    }
}