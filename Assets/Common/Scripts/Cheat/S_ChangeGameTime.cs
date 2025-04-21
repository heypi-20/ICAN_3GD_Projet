using UnityEngine;

/// <summary>
/// This script lets you control the game's time scale via the Inspector,
/// but only after pressing a key (e.g., 'T') to toggle activation during runtime.
/// Once activated, you can change 'targetTimeScale' in the Inspector to adjust game speed.
/// Press the same key again to disable time control and reset to normal speed.
/// </summary>
public class S_ChangeGameTime : MonoBehaviour
{
    [Tooltip("Target time scale to apply when activated.")]
    [Range(0f, 5f)]
    public float targetTimeScale = 1f;

    [Tooltip("Key used to toggle time scale control.")]
    public KeyCode activationKey = KeyCode.T;

    private bool timeControlActive = false;

    void Update()
    {
        // Toggle time control on key press
        if (Input.GetKeyDown(activationKey))
        {
            timeControlActive = !timeControlActive;

            if (timeControlActive)
            {
                Debug.Log("Time control activated. Adjust targetTimeScale in Inspector.");
            }
            else
            {
                Time.timeScale = 1f; // Reset to normal speed
                Debug.Log("Time control deactivated. Time scale reset to 1.");
            }
        }

        // Apply time scale if active
        if (timeControlActive && !Mathf.Approximately(Time.timeScale, targetTimeScale))
        {
            Time.timeScale = targetTimeScale;
        }
    }
}