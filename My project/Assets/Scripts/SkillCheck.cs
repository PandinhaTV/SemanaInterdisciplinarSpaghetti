using UnityEngine;
using UnityEngine.InputSystem;

public class SkillCheck : MonoBehaviour
{
    public float speed = 200f;
    public float currentAngle = 0f;

    public float successStart = 300f;
    public float successEnd = 340f;

    public Transform needle;

    private bool active = false;

    public void StartSkillCheck()
    {
        active = true;
        currentAngle = Random.Range(0f, 360f);
    }

    void Update()
    {
        if (!active) return;

        // Rotate
        currentAngle += speed * Time.deltaTime;
        if (currentAngle >= 360f) currentAngle -= 360f;

        // Apply rotation visually
        needle.rotation = Quaternion.Euler(0, 0, -currentAngle);

        // Input (Space key)
        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            CheckResult();
        }
    }

    void CheckResult()
    {
        active = false;

        if (IsInSuccessZone())
        {
            Debug.Log("SUCCESS");
        }
        else
        {
            Debug.Log("FAIL");
        }
    }

    bool IsInSuccessZone()
    {
        return currentAngle >= successStart && currentAngle <= successEnd;
    }
}