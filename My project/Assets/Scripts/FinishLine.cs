using UnityEngine;
using TMPro;

public class FinishLine : MonoBehaviour
{
    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public float timeLimit = 60f;          // seconds the player has to finish
    private float elapsedTime = 0f;
    private bool timerRunning = false;
    private bool finished = false;

    [Header("Finish Character")]
    public GameObject characterPrefab;         // assign the prefab, not a scene object
    public Transform characterSpawnPoint;

    [Header("Effects")]
    public string appearAnimationTrigger = "Appear";
    [Header("Nonna")]
    public Nonna nonna;
    void Start()
    {
        timerRunning = true;

        
    }

    void Update()
    {
        if (!timerRunning || finished) return;

        elapsedTime += Time.deltaTime;
        DisplayTime(elapsedTime);

        // Player ran out of time — show the character
        if (elapsedTime >= timeLimit)
        {
            timerRunning = false;
            Debug.Log("Time is up!");
            ShowCharacter();
        }
    }

    
    void OnTriggerEnter(Collider other)
    {
        if (finished) return;
        if (!other.CompareTag("Player")) return;

        timerRunning = false;
        finished = true;

        Debug.Log($"Finished in: {FormatTime(elapsedTime)}");

        if (elapsedTime >= timeLimit)
        {
            ShowCharacter();
        }
        else
        {
            Debug.Log("Fast enough!");
            // Tell nonna to stop if she appeared
            if (nonna != null)
                nonna.PlayerFinished();
        }
    }

    void ShowCharacter()
    {
        if (characterPrefab == null) return;

        Vector3 spawnPos = characterSpawnPoint != null 
            ? characterSpawnPoint.position 
            : transform.position;

        GameObject spawnedCharacter = Instantiate(characterPrefab, spawnPos, Quaternion.identity);

        // Play appear animation if the prefab has an Animator
        Animator anim = spawnedCharacter.GetComponent<Animator>();
        if (anim != null)
            anim.SetTrigger(appearAnimationTrigger);
    }

    void DisplayTime(float time)
    {
        if (timerText == null) return;

        // Turn the text red when running low on time
        float timeLeft = timeLimit - time;
        timerText.color = timeLeft <= 10f ? Color.red : Color.white;
        timerText.text = FormatTime(time);
    }

    string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        int milliseconds = Mathf.FloorToInt((time * 100f) % 100f);
        return $"{minutes:00}:{seconds:00}:{milliseconds:00}";
    }

    public void ResetTimer()
    {
        elapsedTime = 0f;
        timerRunning = true;
        finished = false;

       // if (characterToAppear != null)
         //   characterToAppear.SetActive(false);
    }
}