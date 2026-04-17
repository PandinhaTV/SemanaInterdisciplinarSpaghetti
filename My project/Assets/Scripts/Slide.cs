using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Slide : MonoBehaviour
{
    [Header("Settings")]
    public float slideSpeed = 12f;
    public float slideDuration = 0.4f;
    public float slideCooldown = 1f;
    public float slideColliderHeight = 0.5f; // crouched height during slide
    public AudioClip slideSound;
    [Header("Input")]
    public InputActionReference slideAction;

    private CharacterController controller;
    private SkillCheck skillCheck;
    private PlayerMoviment playerMoviment;

    private bool isSliding = false;
    private bool onCooldown = false;
    private bool isWaiting = false;

    private float originalHeight;
    private Vector3 originalCenter;
    private AudioSource audioSource;
    public bool IsSliding => isSliding;

    void Awake()
    {
        controller      = GetComponent<CharacterController>();
        skillCheck      = FindObjectOfType<SkillCheck>();
        playerMoviment  = GetComponent<PlayerMoviment>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f; // 2D sound
        audioSource.loop = false;
        // Save original collider values to restore after slide
        originalHeight  = controller.height;
        originalCenter  = controller.center;
    }

    void OnEnable()
    {
        slideAction.action.Enable();
        slideAction.action.performed += OnSlide;
    }

    void OnDisable()
    {
        slideAction.action.performed -= OnSlide;
        slideAction.action.Disable();
    }

    void OnSlide(InputAction.CallbackContext ctx)
    {
        // Only slide if grounded and moving
        if (isSliding || onCooldown || isWaiting) return;
        if (!controller.isGrounded) return;
        if (Mathf.Abs(playerMoviment.moveInput.x) < 0.1f) return;

        isWaiting = true;

        skillCheck.StartSkillCheck(slideAction.action, (bool success) =>
        {
            isWaiting = false;

            if (!success) return;
            PlaySound(slideSound);
            StartCoroutine(PerformSlide());
        });
    }

    IEnumerator PerformSlide()
    {
        isSliding = true;

        // Get slide direction from where player is facing
        float direction = playerMoviment.spriteRenderer.flipX ? -1f : 1f;

        // Crouch the collider so player fits under things
        controller.height = slideColliderHeight;
        controller.center = new Vector3(originalCenter.x, slideColliderHeight / 2f, originalCenter.z);

        float elapsed = 0f;
        while (elapsed < slideDuration)
        {
            // Slide speed tapers off toward the end
            float speedMultiplier = Mathf.Lerp(1f, 0.2f, elapsed / slideDuration);
            controller.Move(new Vector3(direction * slideSpeed * speedMultiplier, -2f, 0f) * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore collider
        controller.height = originalHeight;
        controller.center = originalCenter;

        isSliding = false;
        StartCoroutine(Cooldown());
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(slideCooldown);
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