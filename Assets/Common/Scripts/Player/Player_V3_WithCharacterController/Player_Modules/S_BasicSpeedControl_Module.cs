using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(S_CustomCharacterController))]
[RequireComponent(typeof(S_EnergyStorage))]
public class S_BasicSpeedControl_Module : MonoBehaviour
{
    [Header("Speed Settings")]
    public float minSpeed = 2f;
    public float maxSpeed = 12f;
    public float speedPercentage = 0.01f;
    public float speedMultiplier = 1f;

    [Header("Energy Consumption Settings")]
    public float minEnergyConsumptionRate = 1f;
    public float maxEnergyConsumptionRate = 5f;
    public float consumptionPercentage = 0.01f;
    public float consumptionMultiplier = 1f;

    // Composant pour ajuster la vitesse de déplacement
    private S_CustomCharacterController _characterController;
    // Composant pour vérifier et modifier l'énergie disponible
    private S_EnergyStorage _energyStorage;
    // Référence au module de sprint pour vérifier l'état du sprint
    private S_BasicSprint_Module _basicSprint_Module;

    // Seuils d'énergie calculés pour atteindre les vitesses maximales
    public float estimatedSpeedEnergyThreshold { get; private set; }
    public float estimatedConsumptionEnergyThreshold { get; private set; }

    // Mise à jour des seuils affichés dans l'inspecteur lorsque les valeurs changent
    private void OnValidate()
    {
        EstimateEnergyThresholds(out float speedThreshold, out float consumptionThreshold);
        estimatedSpeedEnergyThreshold = speedThreshold;
        estimatedConsumptionEnergyThreshold = consumptionThreshold;
    }

    // Initialisation des composants nécessaires
    private void Start()
    {
        _characterController = GetComponent<S_CustomCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _basicSprint_Module ??= GetComponent<S_BasicSprint_Module>();
    }

    // Vérification de la vitesse et consommation d'énergie à chaque frame
    private void Update()
    {
        if (!CheckSprintState())
        {
            UpdateSpeedBasedOnEnergy();
            HandleEnergyConsumption();
        }
    }

    // Vérifie si le joueur est en train de sprinter pour éviter de modifier la vitesse normale
    private bool CheckSprintState()
    {
        return _basicSprint_Module != null && (_basicSprint_Module._isSprinting || _basicSprint_Module.IsSprintCoroutineRunning());
    }

    // Met à jour la vitesse du personnage en fonction de l'énergie actuelle
    private void UpdateSpeedBasedOnEnergy()
    {
        float calculatedSpeed = (Mathf.Max(_energyStorage.currentEnergy, 0f) * speedPercentage) * speedMultiplier;
        calculatedSpeed = Mathf.Clamp(calculatedSpeed, minSpeed, maxSpeed);
        _characterController.moveSpeed = calculatedSpeed;
    }

    // Gère la consommation d'énergie lorsque le joueur se déplace
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

    // Calcule les seuils d'énergie nécessaires pour atteindre la vitesse et la consommation maximales
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
