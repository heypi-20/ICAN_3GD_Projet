using UnityEngine;

/// <summary>
/// Handles pausing and resuming the game by showing/hiding the pause menu,
/// adjusting timescale, and toggling cursor state.
/// </summary>
public class S_PauseMenuBehavior : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Assign your Pause Menu UI GameObject here.")]
    public GameObject pauseMenuUI;

    private bool isPaused = false;

    void Start()
    {
        // Ensure the pause menu is hidden at the start
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);
    }

    void Update()
    {
        // Listen for ESC key to toggle pause/resume
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

        // Freeze all in-game activity
        Time.timeScale = 0f;

        // Unlock and show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        isPaused = true;
    }

    /// <summary>
    /// Resumes the game: hides pause menu, resumes time, and hides the cursor.
    /// This can be hooked up directly to your Resume button's OnClick in the Inspector.
    /// </summary>
    public void ResumeGame()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);

        // Resume game activity
        Time.timeScale = 1f;

        // Lock and hide cursor (change as needed)
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isPaused = false;
    }
}