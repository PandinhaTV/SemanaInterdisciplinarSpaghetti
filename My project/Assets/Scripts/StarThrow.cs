using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class StarThrow : MonoBehaviour
{
    [Header("Settings")]
    public float throwSpeed = 15f;
    public float throwCooldown = 0.5f;

    [Header("Star Prefab")]
    public GameObject starPrefab;
    public Transform throwOrigin;          // empty child at the hand/front of player

    [Header("Input")]
    public InputActionReference throwAction;

    private SpriteRenderer spriteRenderer;
    private SkillCheck skillCheck;
    private PlayerMoviment playerMoviment;
    private bool onCooldown = false;
    private bool isWaiting = false;

    void Awake()
    {
        skillCheck      = FindObjectOfType<SkillCheck>();
        playerMoviment  = GetComponent<PlayerMoviment>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    void OnEnable()
    {
        throwAction.action.Enable();
        throwAction.action.performed += OnThrow;
    }

    void OnDisable()
    {
        throwAction.action.performed -= OnThrow;
        throwAction.action.Disable();
    }

    void OnThrow(InputAction.CallbackContext ctx)
    {
        if (onCooldown || isWaiting) return;

        isWaiting = true;

        skillCheck.StartSkillCheck(throwAction.action, (bool success) =>
        {
            isWaiting = false;

            if (!success) return;

            // Play throw animation on the player
            playerMoviment.PlayThrowAnimation();

            // Spawn and launch the star
            SpawnStar();

            StartCoroutine(Cooldown());
        });
    }

    void SpawnStar()
    {
        if (starPrefab == null) return;

        Vector3 spawnPos = throwOrigin != null ? throwOrigin.position : transform.position;

        // Throw in the direction the player is facing
        float direction = spriteRenderer.flipX ? -1f : 1f;

        GameObject star = Instantiate(starPrefab, spawnPos, Quaternion.identity);
        Rigidbody rb = star.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = new Vector3(direction * throwSpeed, 0f, 0f);
            rb.useGravity = false; // straight horizontal throw
        }

        // Destroy star after 3 seconds so it doesn't linger
        Destroy(star, 3f);
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSeconds(throwCooldown);
        onCooldown = false;
    }
}