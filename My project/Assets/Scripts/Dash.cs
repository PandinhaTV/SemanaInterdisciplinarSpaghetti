using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Dash : MonoBehaviour
{
    [Header("Settings")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public AudioClip dashSound;
    [Header("Input")]
    public InputActionReference dashAction;

    private CharacterController controller;
    private SkillCheck skillCheck;
    private PlayerMoviment playerMoviment;
    private AudioSource audioSource;
    private bool isDashing = false;
    private bool onCooldown = false;
    private Vector3 dashDirection;

    public bool IsDashing => isDashing;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        skillCheck = FindObjectOfType<SkillCheck>();
        playerMoviment = GetComponent<PlayerMoviment>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.loop = false;
    }

    void OnEnable()
    {
        dashAction.action.Enable();
        dashAction.action.performed += OnDash;
    }

    void OnDisable()
    {
        dashAction.action.performed -= OnDash;
        dashAction.action.Disable();
    }

    void OnDash(InputAction.CallbackContext ctx)
    {
        if (isDashing || onCooldown) return;

        // Read horizontal from PlayerMoviment's moveInput instead of old Input class
        float horizontal = playerMoviment.moveInput.x;

        // If no input, dash in the direction the player is facing
        if (Mathf.Abs(horizontal) < 0.1f)
            horizontal = transform.localScale.x > 0 ? 1f : -1f;

        dashDirection = new Vector3(horizontal, 0f, 0f).normalized;

        skillCheck.StartSkillCheck(dashAction.action, (bool success) =>
        {
            if (!success) return;
            PlaySound(dashSound);
            StartCoroutine(PerformDash());
        });
    }

    IEnumerator PerformDash()
    {
        isDashing = true;

        // Tell PlayerMoviment to stop processing while dashing
        float elapsed = 0f;
        while (elapsed < dashDuration)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        isDashing = false;
        StartCoroutine(DashCooldown());
    }

    IEnumerator DashCooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(dashCooldown);
        onCooldown = false;
    }
    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.loop = false;
        audioSource.pitch = 1f;
        audioSource.PlayOneShot(clip);
    }
}