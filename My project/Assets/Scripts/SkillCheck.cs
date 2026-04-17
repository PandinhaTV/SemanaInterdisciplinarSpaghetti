using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
using Random = UnityEngine.Random;

public class SkillCheck : MonoBehaviour
{
    public float speed = 200f;
    public float currentAngle = 0f;

    public float successStart = 300f;
    public float successEnd = 340f;

    public Transform needle;

    public bool active = false;
    public bool skill = false;
    public float slowTimeScale = 0.05f;
    public float normalTimeScale = 1f;

    [Header("Timing")]
    public float inputLockDuration = 0.15f;
    public float cooldownDuration = 0.3f;

    private System.Action<bool> onComplete;
    private InputAction confirmAction;
    private bool inputLocked = false;
    private bool onCooldown = false;

    void Start()
    {
        // Start hidden
        gameObject.SetActive(false);
    }

    public void StartSkillCheck(InputAction action, System.Action<bool> onComplete)
    {
        if (active || onCooldown) return;

        this.onComplete = onComplete;
        this.confirmAction = action;

        // Show the skill check UI
        gameObject.SetActive(true);

        active = true;
        currentAngle = Random.Range(0f, 360f);
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        StartCoroutine(InputLock());
    }

    IEnumerator InputLock()
    {
        inputLocked = true;
        yield return new WaitForSecondsRealtime(inputLockDuration);
        inputLocked = false;
    }

    IEnumerator Cooldown()
    {
        onCooldown = true;
        yield return new WaitForSecondsRealtime(cooldownDuration);
        onCooldown = false;
    }

    void Update()
    {
        if (!active) return;

        currentAngle += speed * Time.unscaledDeltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        needle.rotation = Quaternion.Euler(0, 0, -currentAngle);

        if (!inputLocked && confirmAction != null && confirmAction.WasPressedThisFrame())
            CheckResult();
    }

    void CheckResult()
    {
        active = false;
        inputLocked = false;
        Time.timeScale = normalTimeScale;
        Time.fixedDeltaTime = 0.02f;

        skill = IsInSuccessZone();
        Debug.Log(skill ? "SUCCESS" : "FAIL");

        onComplete?.Invoke(skill);
        confirmAction = null;

        // Hide the skill check UI
        gameObject.SetActive(false);

        StartCoroutine(Cooldown());
    }

    bool IsInSuccessZone()
    {
        return currentAngle >= successStart && currentAngle <= successEnd;
    }
}