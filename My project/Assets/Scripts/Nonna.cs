using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine.SceneManagement;
public class Nonna : MonoBehaviour
{
    [Header("Settings")]
    public float appearAfterSeconds = 3f;  // time before nonna spawns
    public float chaseSpeed = 4f;
    public float killDistance = 1f;          // how close before she catches the player

    [Header("References")]
    public Transform player;
    public CharacterController playerController;

    [Header("Audio")]
    
    public AudioClip chaseSound;             // looping chase music/sound
    private AudioSource audioSource;

    [Header("Animation")]
    public Animator animator;
    
    public string appearAnimationTrigger = "Appear";

    private bool isChasing = true;
    private bool hasAppeared = false;
    private float timer = 0f;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        Appear();
    }

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.loop = false;

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        // Hide nonna at start
        gameObject.SetActive(false);
    }

    void Update()
    {
        //if (hasAppeared) return;

       // timer += Time.deltaTime;

        //if (timer >= appearAfterSeconds)
        //    Appear();
    }

    void Appear()
    {
        hasAppeared = true;
        gameObject.SetActive(true);

        // Play appear animation and sound
        if (animator != null)
            animator.SetTrigger(appearAnimationTrigger);

        

        // Start chasing after a short dramatic pause
        StartCoroutine(StartChaseAfterDelay(1.5f));
    }

    IEnumerator StartChaseAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        isChasing = true;

        // Start looping chase sound
        if (chaseSound != null)
        {
            audioSource.clip = chaseSound;
            audioSource.loop = true;
            audioSource.Play();
        }

        
    }

    void FixedUpdate()
    {
        if (!isChasing || player == null) return;

        // Move toward player
        Vector3 dir = (player.position - transform.position);
        dir.z = 0f;

        // Flip sprite based on direction
        if (spriteRenderer != null)
            spriteRenderer.flipX = dir.x < 0;

        Vector3 move = dir.normalized * chaseSpeed * Time.fixedDeltaTime;
        transform.position += move;

        // Catch the player
        if (dir.magnitude <= killDistance)
            CatchPlayer();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            CatchPlayer();
    }

    void CatchPlayer()
    {
        isChasing = false;
        audioSource.Stop();

        

        Debug.Log("Nonna caught the player!");

        // Add your game over / death logic here
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // Call this from FinishLine when player finishes in time
    public void PlayerFinished()
    {
        isChasing = false;
        hasAppeared = true; // stop the timer
        audioSource.Stop();
        gameObject.SetActive(false);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        audioSource.loop = false;
        audioSource.PlayOneShot(clip);
    }

    // Call this to reset the level
    public void Reset()
    {
        timer = 0f;
        hasAppeared = false;
        isChasing = false;
        audioSource.Stop();
        gameObject.SetActive(false);
    }
}