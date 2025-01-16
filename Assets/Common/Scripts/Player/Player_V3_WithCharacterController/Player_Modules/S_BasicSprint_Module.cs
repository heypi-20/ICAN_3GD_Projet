using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(S_myCharacterController))]
[RequireComponent(typeof(S_EnergyStorage))]
public class S_BasicSprint_Module : MonoBehaviour
{
    [Header("Sprint Settings")]
    public float minSprintSpeed = 15f;
    public float maxSprintSpeed = 25f;
    public float sprintSpeedPercentage = 0.01f;
    public float sprintSpeedMultiplier = 1f;

    [Header("Acceleration/Deceleration Settings")]
    public float accelerationTime = 0.5f;
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float decelerationTime = 0.5f;
    public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Energy Consumption Settings")]
    public float minEnergyConsumptionRate = 2f;
    public float maxEnergyConsumptionRate = 8f;
    public float consumptionPercentage = 0.01f;
    public float consumptionMultiplier = 1f;

    // Composant pour ajuster la vitesse de déplacement
    private S_myCharacterController _characterController;
    // Composant pour vérifier et modifier l'énergie disponible
    private S_EnergyStorage _energyStorage;
    // Gestionnaire d'input pour détecter les actions du joueur
    private S_InputManager _inputManager;

    private S_BasicSpeedControl_Module _basicSpeedControlModule;

    // Stocker la vitesse initiale avant le sprint
    private float _originalMoveSpeed;
    // Indicateur si le joueur est en sprint
    public bool _isSprinting { get; private set; }
    // Vitesse de sprint calculée en fonction de l'énergie
    private float _sprintSpeed;
    // Référence à la coroutine en cours pour éviter les doublons
    private Coroutine _currentCoroutine;

    // Seuil de vitesse et consommation d'énergie affichés dans l'inspecteur
    public float estimatedSprintSpeedThreshold { get; private set; }
    public float estimatedConsumptionEnergyThreshold { get; private set; }

    // Mise à jour des seuils lorsqu'une valeur est modifiée dans l'inspecteur
    private void OnValidate()
    {
        EstimateEnergyThresholds(out float sprintThreshold, out float consumptionThreshold);
        estimatedSprintSpeedThreshold = sprintThreshold;
        estimatedConsumptionEnergyThreshold = consumptionThreshold;
    }

    
    private void Start()
    {
        // Initialisation des composants
        _characterController = GetComponent<S_myCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _inputManager = FindObjectOfType<S_InputManager>();
    }


    private void Update()
    {
        HandleSprint();
    }

    // Vérifie si une coroutine de sprint est en cours
    public bool IsSprintCoroutineRunning()
    {
        return _currentCoroutine != null;
    }

    // Gérer le démarrage et l'arrêt du sprint
    private void HandleSprint()
    {
        if (_inputManager.SprintInput)
        {
            // Commencer le sprint si ce n'est pas déjà le cas
            if (!_isSprinting)
            {
                _originalMoveSpeed = _characterController.moveSpeed;
                if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(AccelerateToSprintSpeed());
                _isSprinting = true;
                
                //TODO: Add FeedBack
            }
            HandleEnergyConsumption();
        }
        else if (_isSprinting)
        {
            // Arrêter le sprint si l'input n'est plus actif
            if (_currentCoroutine != null) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(DecelerateToNormalSpeed());
            _isSprinting = false;
            
            //TODO: Add FeedBack
        }
    }

    // Coroutine pour augmenter progressivement la vitesse jusqu'à la vitesse de sprint
    private IEnumerator AccelerateToSprintSpeed()
    {
        float timer = 0f;
        _sprintSpeed = Mathf.Clamp(
            (Mathf.Max(_energyStorage.currentEnergy, 0f) * sprintSpeedPercentage) * sprintSpeedMultiplier,
            minSprintSpeed,
            maxSprintSpeed
        );

        while (timer < accelerationTime)
        {
            // Interpolation de la vitesse selon la courbe d'accélération
            _characterController.moveSpeed = Mathf.Lerp(_originalMoveSpeed, _sprintSpeed, accelerationCurve.Evaluate(timer / accelerationTime));
            timer += Time.deltaTime;
            yield return null;
        }

        _characterController.moveSpeed = _sprintSpeed;
        _currentCoroutine = null;
    }

    // Coroutine pour réduire progressivement la vitesse jusqu'à la vitesse normale
    private IEnumerator DecelerateToNormalSpeed()
    {
        float timer = 0f;
        float startSpeed = _characterController.moveSpeed;

        while (timer < decelerationTime)
        {
            // Interpolation de la vitesse selon la courbe de décélération
            _characterController.moveSpeed = Mathf.Lerp(_originalMoveSpeed, startSpeed, decelerationCurve.Evaluate(timer / decelerationTime));
            timer += Time.deltaTime;
            yield return null;
        }

        _characterController.moveSpeed = _originalMoveSpeed;
        _currentCoroutine = null;
    }

    // Réduction de l'énergie pendant le sprint
    private void HandleEnergyConsumption()
    {
        float calculatedConsumptionRate = (Mathf.Max(_energyStorage.currentEnergy, 0f) * consumptionPercentage) * consumptionMultiplier;
        calculatedConsumptionRate = Mathf.Clamp(calculatedConsumptionRate, minEnergyConsumptionRate, maxEnergyConsumptionRate);
        _energyStorage.currentEnergy -= calculatedConsumptionRate * Time.deltaTime;
    }

    // Estimation des seuils d'énergie nécessaires pour le sprint
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
