using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using DG.Tweening;

[RequireComponent(typeof(S_CustomCharacterController))]
[RequireComponent(typeof(S_EnergyStorage))]
public class S_BasicSprint_Module : MonoBehaviour
{
    [Serializable]
    public class SprintLevel
    {
        public int level; // Correspond au niveau d'énergie dans S_EnergyStorage
        public float sprintSpeed; // Vitesse de sprint pour ce niveau
        public float energyConsumptionRate; // Consommation d'énergie pour ce niveau
        public float SprintingSpeedFOV;
    }
    [Header("Sprint Levels")]
    public List<SprintLevel> sprintLevels; // Liste des niveaux de sprint

    [Header("Acceleration/Deceleration Settings")]
    public float accelerationTime = 0.5f;
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float decelerationTime = 0.5f;
    public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Camera Settings")]
    public CinemachineVirtualCamera cinemachineCamera; // Caméra du joueur pour ajuster le FOV
    private float sprintFOV = 90f; // FOV pendant le sprint
    private float normalFOV = 60f; // FOV normal
    public float fovTransitionTime = 0.2f; // Durée de transition du FOV

    // Composant pour ajuster la vitesse de déplacement
    private S_CustomCharacterController _characterController;
    // Composant pour vérifier et modifier l'énergie disponible
    private S_EnergyStorage _energyStorage;
    // Gestionnaire d'input pour détecter les actions du joueur
    private S_InputManager _inputManager;

    // Stocker la vitesse initiale avant le sprint
    private float _originalMoveSpeed;
    // Indicateur si le joueur est en sprint
    public bool _isSprinting { get; private set; }
    // Référence à la coroutine en cours pour éviter les doublons
    private Coroutine _currentCoroutine;

    private void Start()
    {
        // Initialisation des composants
        _characterController = GetComponent<S_CustomCharacterController>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _inputManager = FindObjectOfType<S_InputManager>();

        // Initialiser le FOV de la caméra
        if (cinemachineCamera != null)
        {
            normalFOV = cinemachineCamera.m_Lens.FieldOfView;
        }
    }

    private void Update()
    {
        HandleSprint();
    }

    // Gérer le démarrage et l'arrêt du sprint
    private void HandleSprint()
    {
        if (_inputManager.SprintInput&&GetCurrentSprintLevels()!=null)
        {
            // Commencer le sprint si ce n'est pas déjà le cas
            Debug.Log("Consomme energy");
            HandleEnergyConsumption();
            if (!_isSprinting)
            {
                _originalMoveSpeed = _characterController.moveSpeed;
                if (IsSprintCoroutineRunning()) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(AccelerateToSprintSpeed());
                _isSprinting = true;

                // Ajouter le feedback FOV
                SoundManager.Instance.Meth_Active_Sprint();
                UpdateCameraFOV(GetLevelFOV());
            }
            
            
        }
        else if (_isSprinting)
        {
            // Arrêter le sprint si l'input n'est plus actif
            if (IsSprintCoroutineRunning()) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(DecelerateToNormalSpeed());
            _isSprinting = false;
    
            // Ajouter le feedback FOV
            SoundManager.Instance.Meth_Desactive_Sprint();
            UpdateCameraFOV(normalFOV);
        }
    }
    // Vérifie si une coroutine de sprint est en cours
    public bool IsSprintCoroutineRunning()
    {
        return _currentCoroutine != null;
    }

    // Coroutine pour augmenter progressivement la vitesse jusqu'à la vitesse de sprint
    private IEnumerator AccelerateToSprintSpeed()
    {
        float timer = 0f;
        float targetSprintSpeed = GetCurrentSprintLevels().sprintSpeed;

        while (timer < accelerationTime)
        {
            // Interpolation de la vitesse selon la courbe d'accélération
            timer += Time.deltaTime;
            targetSprintSpeed = GetCurrentSprintLevels().sprintSpeed;
            float curveValue = accelerationCurve.Evaluate(Mathf.Clamp01(timer / accelerationTime));
            _characterController.moveSpeed = Mathf.Lerp(_originalMoveSpeed, targetSprintSpeed, curveValue);
            yield return null;
        }

        _characterController.moveSpeed = targetSprintSpeed;
        _currentCoroutine = null;
    }

    // Coroutine pour réduire progressivement la vitesse jusqu'à la vitesse normale
    private IEnumerator DecelerateToNormalSpeed()
    {
        float timer = 0f;
        float startSpeed = _characterController.moveSpeed;

        while (timer < decelerationTime)
        {
            // Calculer la progression temporelle et l'obtenir via la courbe de décélération
            float curveValue = decelerationCurve.Evaluate(Mathf.Clamp01(timer / decelerationTime));

            // Interpoler entre la vitesse initiale et la vitesse normale à l'aide de la valeur de la courbe
            _characterController.moveSpeed = Mathf.Lerp(startSpeed,_originalMoveSpeed , 1f - curveValue);

            timer += Time.deltaTime;
            yield return null;
        }

        _characterController.moveSpeed = _originalMoveSpeed;
        _currentCoroutine = null;
    }

    // Réduction de l'énergie pendant le sprint
    private void HandleEnergyConsumption()
    {
        float energyConsumptionRate = GetCurrentSprintLevels().energyConsumptionRate;
        _energyStorage.currentEnergy -=energyConsumptionRate*Time.deltaTime;
    }

    private void UpdateCameraFOV(float targetFOV)
    {
        if (cinemachineCamera != null)
        {
            DOTween.Kill(cinemachineCamera); // Arrête les animations FOV précédentes
            DOTween.To(
                () => cinemachineCamera.m_Lens.FieldOfView, // Getter pour le FOV actuel
                x => cinemachineCamera.m_Lens.FieldOfView = x, // Setter pour appliquer le nouveau FOV
                targetFOV, // Valeur cible
                fovTransitionTime // Durée de la transition
            ).SetEase(Ease.OutQuad);
        }
    }

    // Obtient la vitesse de sprint pour le niveau d'énergie actuel
    private SprintLevel GetCurrentSprintLevels()
    {
        int currentLevel = _energyStorage.currentLevelIndex + 1; // Ajuste pour correspondre au niveau dans SprintLevel
        return sprintLevels.Find(level => level.level == currentLevel);;
    }
    

    private float GetLevelFOV()
    {
        int currentLevel = _energyStorage.currentLevelIndex + 1;
        SprintLevel level = sprintLevels.Find(l => l.level == currentLevel);
        return level != null ? level.SprintingSpeedFOV : 0f;
    }
}
