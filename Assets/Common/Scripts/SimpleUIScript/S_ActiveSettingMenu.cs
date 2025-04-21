using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_ActiveSettingMenu : MonoBehaviour
{
    // Reference to the setting menu GameObject
    public GameObject settingMenu;

    // Tracks whether the menu is currently active
    private bool isMenuActive = false;

    void Update()
    {
        // Toggle the menu when Escape key is pressed
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleMenu();
        }
    }

    // Activates or deactivates the menu, and pauses/unpauses the game
    public void ToggleMenu()
    {
        isMenuActive = !isMenuActive;
        settingMenu.SetActive(isMenuActive);

        // Pause game when menu is active, resume when not
        Time.timeScale = isMenuActive ? 0f : 1f;

        // Optional: Show or hide the cursor based on menu state
        Cursor.visible = isMenuActive;
        Cursor.lockState = isMenuActive ? CursorLockMode.None : CursorLockMode.Locked;
    }
}