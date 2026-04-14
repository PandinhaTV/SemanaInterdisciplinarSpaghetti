using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    [Header("Settings")]
    public float maxDistance = 15f;
    public float gravity = -25f;
    public float launchSpeed = 5f;      // initial boost when grapple connects
    public float releaseJumpForce = 6f; // upward boost when you let go
    public LayerMask grappleLayer;

    [Header("Rope Visual")]
    public LineRenderer ropeRenderer;
    public Transform ropeOrigin;

    [Header("Input")]
    public InputActionReference fireAction;

    public bool IsGrappling => isGrappling;

    private Vector3 grapplePoint;
    private float ropeLength;
    private bool isGrappling = false;
    private Vector3 swingVelocity = Vector3.zero;

    private CharacterController controller;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }

    void OnEnable()
    {
        fireAction.action.Enable();
        fireAction.action.performed += OnFire;
        fireAction.action.canceled += OnFireReleased;
    }

    void OnDisable()
    {
        fireAction.action.performed -= OnFire;
        fireAction.action.canceled -= OnFireReleased;
        fireAction.action.Disable();
    }

    void OnFire(InputAction.CallbackContext ctx) => TryGrapple();
    void OnFireReleased(InputAction.CallbackContext ctx) => StopGrapple();

    void TryGrapple()
    {
        Vector3 dir = GetAimDirection();

        if (Physics.Raycast(ropeOrigin.position, dir, out RaycastHit hit, maxDistance, grappleLayer))
        {
            grapplePoint = hit.point;
            ropeLength = Vector3.Distance(transform.position, grapplePoint);

            // Inherit any existing movement as swing momentum
            // and give a small boost toward the grapple point
            swingVelocity += dir * launchSpeed;

            isGrappling = true;
            ropeRenderer.enabled = true;
            ropeRenderer.SetPosition(0, ropeOrigin.position);
            ropeRenderer.SetPosition(1, grapplePoint);
        }
    }

    void StopGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        ropeRenderer.enabled = false;

        // Pass current swing velocity to the movement script so the player
        // flies through the air after releasing
        GetComponent<PlayerMoviment>().SetExternalVelocity(swingVelocity);
    }

    void Update()
    {
        Vector3 dir = GetAimDirection();
        Debug.DrawRay(ropeOrigin.position, dir * maxDistance, Color.red);
        if (!isGrappling) return;

        SimulateSwing();
        ropeRenderer.SetPosition(0, ropeOrigin.position); // rope follows player
    }

    void SimulateSwing()
    {
        // 1. Apply gravity
        swingVelocity.y += gravity * Time.deltaTime;

        // 2. Get direction from grapple point to player
        Vector3 toPlayer = transform.position - grapplePoint;
        float currentDist = toPlayer.magnitude;

        // 3. Constrain to rope length — if too far, pull back
        if (currentDist > ropeLength)
        {
            // Project velocity onto the tangent of the circle (remove outward component)
            Vector3 radial = toPlayer.normalized;
            float radialSpeed = Vector3.Dot(swingVelocity, radial);

            if (radialSpeed > 0) // only cancel outward velocity
                swingVelocity -= radial * radialSpeed;

            // Snap position to rope length
            Vector3 corrected = grapplePoint + radial * ropeLength;
            Vector3 correction = corrected - transform.position;
            controller.Move(correction);
        }

        // 4. Flatten Z — keep it in 2D plane
        swingVelocity.z = 0f;

        // 5. Move the player
        controller.Move(swingVelocity * Time.deltaTime);
    }

    Vector3 GetAimDirection()
    {
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 stick = gamepad.rightStick.ReadValue();
            if (stick.magnitude > 0.2f)
                return new Vector3(stick.x, stick.y, 0f).normalized;
        }

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        float camZ = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, camZ)
        );
        mouseWorld.z = transform.position.z;

        Vector3 dir = mouseWorld - transform.position;
        dir.z = 0f;

        if (dir.sqrMagnitude < 0.001f) return Vector3.right;
        return dir.normalized;
    }
}