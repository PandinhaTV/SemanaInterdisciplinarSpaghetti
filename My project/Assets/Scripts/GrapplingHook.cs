using UnityEngine;
using UnityEngine.InputSystem;

public class GrapplingHook : MonoBehaviour
{
    [Header("Settings")]
    public float maxDistance = 15f;
    public float gravity = -25f;
    public float launchSpeed = 5f;
    public float releaseJumpForce = 6f;
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
    private SkillCheck skillCheck;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        skillCheck = FindObjectOfType<SkillCheck>();
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

        if (!Physics.Raycast(ropeOrigin.position, dir, out RaycastHit hit, maxDistance, grappleLayer))
            return;

        skillCheck.StartSkillCheck(fireAction.action, (bool success) =>
        {
            if (!success) return;

            grapplePoint = hit.point;
            ropeLength = Vector3.Distance(transform.position, grapplePoint);
            swingVelocity += dir * launchSpeed;
            isGrappling = true;
            ropeRenderer.enabled = true;
            ropeRenderer.SetPosition(0, ropeOrigin.position);
            ropeRenderer.SetPosition(1, grapplePoint);
        });
    }

    void StopGrapple()
    {
        if (!isGrappling) return;

        isGrappling = false;
        ropeRenderer.enabled = false;

        GetComponent<PlayerMoviment>().SetExternalVelocity(swingVelocity);
    }

    void Update()
    {
        Vector3 dir = GetAimDirection();
        Debug.DrawRay(ropeOrigin.position, dir * maxDistance, Color.red);

        if (!isGrappling) return;

        SimulateSwing();
        ropeRenderer.SetPosition(0, ropeOrigin.position);
    }

    void SimulateSwing()
    {
        swingVelocity.y += gravity * Time.deltaTime;

        Vector3 toPlayer = transform.position - grapplePoint;
        float currentDist = toPlayer.magnitude;

        if (currentDist > ropeLength)
        {
            Vector3 radial = toPlayer.normalized;
            float radialSpeed = Vector3.Dot(swingVelocity, radial);

            if (radialSpeed > 0)
                swingVelocity -= radial * radialSpeed;

            Vector3 corrected = grapplePoint + radial * ropeLength;
            Vector3 correction = corrected - transform.position;
            controller.Move(correction);
        }

        swingVelocity.z = 0f;
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