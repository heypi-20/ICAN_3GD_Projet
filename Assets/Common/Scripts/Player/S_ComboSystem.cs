using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class S_ComboSystem : MonoBehaviour
{
    [Serializable]
    public class ComboSettings
    {
        public int LevelToUnlock;
        public float minComboMultiplier;
        public float maxComboMultiplier;
        public float comboTime;
        public float GainPerKill;
    }

    [Header("Combo Settings")]
    public List<ComboSettings> comboSettings = new List<ComboSettings>();

    // Reference to the energy storage component, assumed to have currentLevelIndex for the current energy level.
    private S_EnergyStorage energyStorage;

    // Current combo state variables.
    private bool comboActive = false;
    public float currentComboMultiplier = 0f;
    private float comboTimer = 0f;
    private ComboSettings currentComboSetting = null;

    // Reference to the TextMeshProUGUI component to display combo information.
    public TextMeshProUGUI comboText;

    private void Start()
    {
        // Get energy storage component (modify as needed).
        energyStorage = FindObjectOfType<S_EnergyStorage>();
        // Subscribe to enemy kill event.
        EnemyBase.OnEnemyKillForCombo += StartCombo;
    }

    private void OnDestroy()
    {
        EnemyBase.OnEnemyKillForCombo -= StartCombo;
    }

    /// <summary>
    /// Called each time an enemy is killed.
    /// </summary>
    private void StartCombo()
    {
        // Get current energy level from energyStorage.
        int currentLevel = energyStorage.currentLevelIndex + 1;

        // Find the combo settings for the current energy level.
        ComboSettings newSetting = comboSettings.Find(s => s.LevelToUnlock == currentLevel);
        if (newSetting == null)
        {
            Debug.LogWarning("Combo setting not found for energy level " + currentLevel);
            return;
        }

        if (!comboActive)
        {
            // Start the combo for the first kill.
            currentComboSetting = newSetting;
            currentComboMultiplier = currentComboSetting.minComboMultiplier;
            comboTimer = currentComboSetting.comboTime;
            comboActive = true;
            // Display initial combo information.
            comboText.text = $"Combo started, multiplier: {currentComboMultiplier:F2}";
            Debug.Log("Combo started, initial multiplier: " + currentComboMultiplier);
        }
        else
        {
            // If combo is active, check if energy level has changed.
            if (currentComboSetting.LevelToUnlock != currentLevel)
            {
                // Update combo settings for new energy level while keeping current multiplier.
                currentComboSetting = newSetting;
                // Reset combo timer using new settings.
                comboTimer = currentComboSetting.comboTime;
                comboText.text = $"Energy level upgraded, combo updated, multiplier remains: {currentComboMultiplier:F2}";
                Debug.Log("Energy level upgraded, combo settings updated, current multiplier remains: " + currentComboMultiplier);
            }
            else
            {
                // Reset combo timer.
                comboTimer = currentComboSetting.comboTime;
            }

            // Increase combo multiplier, but do not exceed maximum.
            currentComboMultiplier += currentComboSetting.GainPerKill;
            if (currentComboMultiplier > currentComboSetting.maxComboMultiplier)
            {
                currentComboMultiplier = currentComboSetting.maxComboMultiplier;
            }
            comboText.text = $"Combo updated, multiplier: {currentComboMultiplier:F2}";
            Debug.Log("Combo updated, current multiplier: " + currentComboMultiplier);
        }
    }

    private void Update()
    {
        // If combo is active, decrease timer and update text.
        if (comboActive)
        {
            comboTimer -= Time.deltaTime;
            // Update text to show current multiplier and remaining time.
            comboText.text = $"Combo multiplier: {currentComboMultiplier:F2} | Time left: {comboTimer:F2}";
            if (comboTimer <= 0)
            {
                // Combo timed out, reset combo state.
                Debug.Log("Combo ended, final multiplier: " + currentComboMultiplier);
                ResetCombo();
            }
        }
        else
        {
            // Clear combo text when no combo is active.
            if (!string.IsNullOrEmpty(comboText.text))
            {
                comboText.text = "";
            }
        }
    }


    /// <summary>
    /// Resets the combo state.
    /// </summary>
    private void ResetCombo()
    {
        comboActive = false;
        currentComboMultiplier = 0f;
        comboTimer = 0f;
        currentComboSetting = null;
        comboText.text = "";
    }
}
