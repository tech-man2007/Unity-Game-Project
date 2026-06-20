using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float slowMotion = 0.05f; 
    public float normalTime = 1f;
    public float transitionSpeed = 5f; 

    void Update()
    {
        // We calculate the math in a temporary variable first
        float newTime = Time.timeScale;

        if (Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0)
        {
            newTime += transitionSpeed * Time.unscaledDeltaTime;
        }
        else
        {
            newTime -= transitionSpeed * Time.unscaledDeltaTime;
        }

        // We clamp the math so it NEVER drops below 0.05, then safely apply it to Unity's Time
        Time.timeScale = Mathf.Clamp(newTime, slowMotion, normalTime);
        Time.fixedDeltaTime = Time.timeScale * 0.02f;
    }
}