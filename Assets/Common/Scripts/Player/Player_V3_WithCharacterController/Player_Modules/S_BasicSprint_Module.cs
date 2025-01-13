using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(S_myCharacterController))]
[RequireComponent(typeof(S_EnergyStorage))]
public class S_BasicSprint_Module : MonoBehaviour
{
    [Header("Sprint Settings")]
    public float minSprintSpeed = 15f; // Minimum sprint speed
    public float maxSprintSpeed = 25f; // Maximum sprint speed
    public float sprintSpeedPercentage = 0.01f; // Percentage multiplier applied to current energy
    public float sprintSpeedMultiplier = 1f; // Final multiplier applied to the calculated sprint speed

    [Header("Acceleration/Deceleration Settings")]
    public float accelerationTime = 0.5f; // Time to accelerate to sprint speed
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float decelerationTime = 0.5f; // Time to decelerate back to normal speed
    public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Energy Consumption Settings")]
    public float minEnergyConsumptionRate = 2f; // Minimum energy consumption rate per second
    public float maxEnergyConsumptionRate = 8f; // Maximum energy consumption rate per second
    public float consumptionPercentage = 0.01f; // Percentage multiplier applied to current energy
    public float consumptionMultiplier = 1f; // Final multiplier applied to the calculated consumption rate

    private S_myCharacterController _characterController;
    private S_EnergyStorage _energyStorage;
    private S_InputManager _inputManager;

    private float _originalMoveSpeed;
    public bool _isSprinting{ get; private set; }
    private float _sprintSpeed;

    private Coroutine _currentCoroutine;

    public float estimatedSprintSpeedThreshold { get; private set; } // Sprint speed threshold to reach max speed
    public float estimatedConsumptionEnergyThreshold { get; private set; } // Energy threshold to reach max consumption rate

    private void OnValidate()
    {
        // Calculate and display thresholds in the Editor
        EstimateEnergyThresholds(out float sprintThreshold, out float consumptionThreshold);
        estimatedSprintSpeedThreshold = sprintThreshold;
        estimatedConsumptionEnergyThreshold = consumptionThreshold;
    }

    private void Start()
    {
        _characterController = GetComponent<S_myCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _inputManager = FindObjectOfType<S_InputManager>();
    }

    private void Update()
    {
        HandleSprint();
    }
    public bool IsSprintCoroutineRunning()
    {
        return _currentCoroutine != null;
    }

    private void HandleSprint()
    {
        if (_inputManager.SprintInput)
        {
            if (!_isSprinting)
            {
                _originalMoveSpeed = _characterController.moveSpeed; // Save the current move speed
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(AccelerateToSprintSpeed());
                _isSprinting = true;
            }
            HandleEnergyConsumption();
        }
        else if (_isSprinting)
        {
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(DecelerateToNormalSpeed());
            _isSprinting = false;
        }
    }

    private IEnumerator AccelerateToSprintSpeed()
    {
        float timer = 0f;
        _sprintSpeed = Mathf.Clamp((Mathf.Max(_energyStorage.currentEnergy, 0f) * sprintSpeedPercentage) * sprintSpeedMultiplier, minSprintSpeed, maxSprintSpeed);
        while (timer < accelerationTime)
        {
            _characterController.moveSpeed = Mathf.Lerp(_originalMoveSpeed, _sprintSpeed, accelerationCurve.Evaluate(timer / accelerationTime));
            timer += Time.deltaTime;
            yield return null;
        }
        _characterController.moveSpeed = _sprintSpeed;

        _currentCoroutine = null; // 协程结束时清空引用
    }

    private IEnumerator DecelerateToNormalSpeed()
    {
        float timer = 0f;
        float startSpeed = _characterController.moveSpeed;
        while (timer < decelerationTime)
        {
            _characterController.moveSpeed = Mathf.Lerp(startSpeed, _originalMoveSpeed, timer / decelerationTime);
            timer += Time.deltaTime;
            yield return null;
        }
        _characterController.moveSpeed = _originalMoveSpeed;

        _currentCoroutine = null; // 协程结束时清空引用
    }

    private void HandleEnergyConsumption()
    {
        float calculatedConsumptionRate = (Mathf.Max(_energyStorage.currentEnergy, 0f) * consumptionPercentage) * consumptionMultiplier;
        calculatedConsumptionRate = Mathf.Clamp(calculatedConsumptionRate, minEnergyConsumptionRate, maxEnergyConsumptionRate);
        _energyStorage.currentEnergy -= calculatedConsumptionRate * Time.deltaTime;
    }

    // Optional: Estimate at what energy level the max sprint speed and max consumption rate will be reached
    public void EstimateEnergyThresholds(out float sprintSpeedThreshold, out float consumptionEnergyThreshold)
    {
        sprintSpeedThreshold = maxSprintSpeed / (sprintSpeedPercentage * sprintSpeedMultiplier);
        consumptionEnergyThreshold = maxEnergyConsumptionRate / (consumptionPercentage * consumptionMultiplier);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(S_BasicSprint_Module))]
public class S_BasicSprintModuleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        S_BasicSprint_Module module = (S_BasicSprint_Module)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Estimated Sprint Thresholds", EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Sprint Speed Threshold:", module.estimatedSprintSpeedThreshold.ToString("F2"));
        EditorGUILayout.LabelField("Consumption Energy Threshold:", module.estimatedConsumptionEnergyThreshold.ToString("F2"));
    }
}
#endif
