using UnityEngine;

/// <summary>
/// Handles pausing and resuming the game by showing/hiding the pause menu,
/// adjusting timescale, and toggling cursor state. Ignores pause if settings menu is open.
/// </summary>
public class S_PauseMenuBehavior : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Assign your Pause Menu UI GameObject here.")]
    public GameObject pauseMenuUI;

    public GameObject settingsMenuUI;

    private bool isPaused = false;

    void Start()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }
    

    void Update()
    {
        // If Settings Menu is open, ignore pause toggle
        if (settingsMenuUI != null && settingsMenuUI.activeSelf)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    /// <summary>
    /// Pauses the game: shows pause menu, stops time, and shows the cursor.
    /// </summary>
    public void PauseGame()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(true);

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isPaused = true;
    }

    /// <summary>
    /// Resumes the game: hides pause menu, resumes time, and hides the cursor.
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPaused = false;
    }
}