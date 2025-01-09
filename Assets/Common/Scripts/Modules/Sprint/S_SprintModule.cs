using UnityEngine;

public class S_SprintModule : MonoBehaviour
{
    // Variables for configurable inputs and values
    public KeyCode sprintKey;
    public float sprintSpeed = 65f; // Base sprint speed (without energy factor)
    public float energyPercentage; // Percentage of current energy to use (e.g., 0.1 for 10%)
    public float energyMultiplier; // Multiplier for the energy factor
    public float sprintEnergyConsumptionRate; // Energy consumed per second while sprinting

    // References to other components
    private S_PlayerMultiCam playerMultiCam;
    private S_EnergyStorage energyStorage;

    private float currentSprintSpeed;
    private bool isSprinting;
    private float savedBaseSpeed;

    private void Awake()
    {
        InitializeReferences();
    }

    private void Update()
    {
        HandleSprintInput();

        if (isSprinting)
        {
            ConsumeEnergy();
        }
    }

    #region Initialization

    private void InitializeReferences()
    {
        playerMultiCam = GetComponent<S_PlayerMultiCam>();
        if (playerMultiCam == null)
        {
            Debug.LogError("S_PlayerMultiCam script not found on the same GameObject.");
        }

        energyStorage = GetComponent<S_EnergyStorage>();
        if (energyStorage == null)
        {
            Debug.LogError("S_EnergyStorage script not found on the same GameObject.");
        }

        if (playerMultiCam != null)
        {
            savedBaseSpeed = playerMultiCam.baseSpeed; // Save the initial base speed
        }
    }

    #endregion

    #region Sprint Logic

    private void HandleSprintInput()
    {
        if (Input.GetKey(sprintKey))
        {
            isSprinting = true;
            CalculateSprintSpeed();
        }
        else
        {
 
            isSprinting = false;
            ResetToSavedBaseSpeed();
        }

        ApplySpeedLerp();
    }

    private void TryToggleSprint()
    {
        if (energyStorage != null && energyStorage.currentEnergy > 0)
        {
            isSprinting = !isSprinting;
        }
    }

    private void CalculateSprintSpeed()
    {
        if (energyStorage != null && energyStorage.currentEnergy > 0)
        {
            float energyFactor = (energyStorage.currentEnergy * energyPercentage) * energyMultiplier;
            currentSprintSpeed = sprintSpeed + energyFactor;
        }
        else
        {
            currentSprintSpeed = sprintSpeed; // No energy factor, use base sprint speed
        }
    }

    private void ResetToSavedBaseSpeed()
    {
        currentSprintSpeed = savedBaseSpeed;
    }

    private void ApplySpeedLerp()
    {
        if (playerMultiCam != null)
        {
            float targetSpeed = isSprinting ? currentSprintSpeed : savedBaseSpeed;
            playerMultiCam.baseSpeed = targetSpeed;
        }
    }

    private void ConsumeEnergy()
    {
        if (energyStorage != null && energyStorage.currentEnergy > 0)
        {
            energyStorage.currentEnergy -= sprintEnergyConsumptionRate * Time.deltaTime;
            if (energyStorage.currentEnergy <= 0)
            {
                energyStorage.currentEnergy = 0;
                isSprinting = false; // Stop sprinting when out of energy
                ResetToSavedBaseSpeed();
            }
        }
    }

    #endregion
}
