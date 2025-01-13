using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(S_myCharacterController))]
[RequireComponent(typeof(S_EnergyStorage))]
public class S_BasicSpeedControl_Module : MonoBehaviour
{
    [Header("Speed Settings")]
    public float minSpeed = 2f; // Minimum speed when energy is 0
    public float maxSpeed = 12f; // Maximum speed when energy is at maximum
    public float speedPercentage = 0.01f; // Percentage multiplier applied to current energy
    public float speedMultiplier = 1f; // Final multiplier applied to the calculated speed

    [Header("Energy Consumption Settings")]
    public float minEnergyConsumptionRate = 1f; // Minimum energy consumption rate per second
    public float maxEnergyConsumptionRate = 5f; // Maximum energy consumption rate per second
    public float consumptionPercentage = 0.01f; // Percentage multiplier applied to current energy
    public float consumptionMultiplier = 1f; // Final multiplier applied to the calculated consumption rate

    private S_myCharacterController _characterController;
    private S_EnergyStorage _energyStorage;
    private S_BasicSprint_Module _basicSprint_Module;
    
    public float estimatedSpeedEnergyThreshold { get; private set; } // Speed threshold to reach max speed
    public float estimatedConsumptionEnergyThreshold { get; private set; } // Energy threshold to reach max consumption rate

    private void OnValidate()
    {
        // Calculate and display thresholds in the Editor
        EstimateEnergyThresholds(out float speedThreshold, out float consumptionThreshold);
        estimatedSpeedEnergyThreshold = speedThreshold;
        estimatedConsumptionEnergyThreshold = consumptionThreshold;
    }

    private void Start()
    {
        _characterController = GetComponent<S_myCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _basicSprint_Module ??= GetComponent<S_BasicSprint_Module>();
    }

    private void Update()
    {
        if (!CheckSprintState())
        {
            UpdateSpeedBasedOnEnergy();
            HandleEnergyConsumption();
        }
        
    }

    private bool CheckSprintState()
    {
        return _basicSprint_Module != null && (_basicSprint_Module._isSprinting || _basicSprint_Module.IsSprintCoroutineRunning());
    }

    private void UpdateSpeedBasedOnEnergy()
    {
        float calculatedSpeed = (Mathf.Max(_energyStorage.currentEnergy, 0f) * speedPercentage) * speedMultiplier;
        calculatedSpeed = Mathf.Clamp(calculatedSpeed, minSpeed, maxSpeed);
        _characterController.moveSpeed = calculatedSpeed;
    }

    private void HandleEnergyConsumption()
    {
        if (_characterController._inputDirection.magnitude > 0)
        {
            float calculatedConsumptionRate = (Mathf.Max(_energyStorage.currentEnergy, 0f) * consumptionPercentage) * consumptionMultiplier;
            calculatedConsumptionRate = Mathf.Clamp(calculatedConsumptionRate, minEnergyConsumptionRate, maxEnergyConsumptionRate);
            float energyToConsume = calculatedConsumptionRate * Time.deltaTime;
            _energyStorage.currentEnergy -= energyToConsume;
        }
    }

    // Optional: Estimate at what energy level the max speed and max consumption rate will be reached
    public void EstimateEnergyThresholds(out float speedEnergyThreshold, out float consumptionEnergyThreshold)
    {
        speedEnergyThreshold = maxSpeed / (speedPercentage * speedMultiplier);
        consumptionEnergyThreshold = maxEnergyConsumptionRate / (consumptionPercentage * consumptionMultiplier);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_BasicSpeedControl_Module))]
public class S_BasicSpeedControlModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        S_BasicSpeedControl_Module module = (S_BasicSpeedControl_Module)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Estimated Energy Thresholds", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Speed Energy Threshold:", module.estimatedSpeedEnergyThreshold.ToString("F2"));
        EditorGUILayout.LabelField("Consumption Energy Threshold:", module.estimatedConsumptionEnergyThreshold.ToString("F2"));
    }
}
#endif
