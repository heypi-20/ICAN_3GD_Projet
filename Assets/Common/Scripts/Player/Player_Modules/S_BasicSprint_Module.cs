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
        public float sprintDamage;
        public float SprintingSpeedFOV;
        public int dropBonus;
    }
    [Header("Sprint Levels")]
    public List<SprintLevel> sprintLevels; // Liste des niveaux de sprint
    
    [Header("Sprint Settings")]
    public LayerMask enemyLayer; // Layer mask to filter only enemy objects for sprint damage
    public float rangeDistance;
    public Vector3 rangeOffset;
    public float pushForce;
    public float sprintRange = 3f;

    [Header("Acceleration/Deceleration Settings")]
    public float accelerationTime = 0.5f;
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public float decelerationTime = 0.5f;
    public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Camera Settings")]
    public CinemachineVirtualCamera cinemachineCamera; // Caméra du joueur pour ajuster le FOV
    private float normalFOV = 60f; // FOV normal
    public float fovTransitionTime = 0.2f; // Durée de transition du FOV
    
    [Header("Gizmos Settings")]
    public bool drawSprintDamageGizmo = true;

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

    public event Action<Enum,int> OnSprintStateChange;
    
    // Référence à la coroutine en cours pour éviter les doublons
    private Coroutine _currentCoroutine;
    
    //Use for event
    private int _previousSprintLevel = -1; // Stores the last sprint level
    private bool _hasStartedSprinting = false; // Tracks whether sprinting has started
    public HashSet<EnemyBase> alreadyDamagedEnemies = new HashSet<EnemyBase>();

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
        UpdateLevelDuringSprinting();
        HandleSprintDamage();
    }

    private void UpdateLevelDuringSprinting()
    {
        // Ensure sprint re-triggers if level changes while sprinting
        if (_isSprinting)
        {
            int currentLevel = _energyStorage.currentLevelIndex + 1;
            if (currentLevel != _previousSprintLevel)
            {
                _isSprinting = false; // Allow HandleSprint() to re-execute sprint logic
            }
            _previousSprintLevel = currentLevel;
        }
        else
        {
            _previousSprintLevel = _energyStorage.currentLevelIndex + 1;        
        }
    }
    

    private void SprintObserverEvent(Enum sprintState, int level)
    {
        // Step 1: If "StartSprinting" is received, immediately trigger it and mark sprint as started
        if (sprintState is PlayerStates.SprintState.StartSprinting)
        {
            if (!_hasStartedSprinting)
            {
                OnSprintStateChange?.Invoke(PlayerStates.SprintState.StartSprinting, level);
                
                _hasStartedSprinting = true; // Mark sprinting as started
            }
        }

        if (sprintState is not PlayerStates.SprintState.StopSprinting)
        {
            //Invoke isSprinting state
            OnSprintStateChange?.Invoke(PlayerStates.SprintState.IsSprinting, level);
        }

        // Step 4: When sprinting stops, reset everything
        if (sprintState is PlayerStates.SprintState.StopSprinting)
        {
            _hasStartedSprinting = false;
            OnSprintStateChange?.Invoke(PlayerStates.SprintState.StopSprinting, level);
        }


    }

    private void HandleSprintDamage()
    {
        if (!_isSprinting || _characterController.currentSpeed <= 1)
        {
            if (!_isSprinting)
            {
                alreadyDamagedEnemies.Clear();
            }

            return;
        }

        // Clean up: remove null or inactive enemies from the alreadyDamagedEnemies set
        alreadyDamagedEnemies.RemoveWhere(e => e == null || !e.gameObject.activeInHierarchy);

        // Get current sprint level for retrieving the damage range
        SprintLevel currentSprintLevel = GetCurrentSprintLevels();
        if (currentSprintLevel == null) return;

        // Overlap sphere detection at the player's position with the sprint damage range
        Vector3 forwardOffset = transform.forward * rangeDistance;
        Vector3 finalPosition = transform.position + forwardOffset+rangeOffset;
        Collider[] colliders = Physics.OverlapSphere(finalPosition, sprintRange, enemyLayer);

        // Create a set to store currently detected enemies
        HashSet<EnemyBase> currentlyDetectedEnemies = new HashSet<EnemyBase>();

        // Loop through all colliders detected
        foreach (Collider col in colliders)
        {
            // Check if the collider has an EnemyBase component and is active
            EnemyBase enemy = col.GetComponent<EnemyBase>();
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                currentlyDetectedEnemies.Add(enemy);
                // Only apply damage if this enemy was not already damaged in previous frames
                if (!alreadyDamagedEnemies.Contains(enemy))
                {
                    Rigidbody rb = col.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        // Apply push force to the enemy
                        Vector3 forceDirection = (rb.transform.position - finalPosition).normalized + Vector3.up;
                        forceDirection.Normalize(); 
                        rb.AddForce(forceDirection * pushForce, ForceMode.Impulse);
                        
                    }

                    // Apply damage to the enemy
                    enemy.ReduceHealth(currentSprintLevel.sprintDamage, currentSprintLevel.dropBonus);
                    // Mark this enemy as already damaged
                    alreadyDamagedEnemies.Add(enemy);
                }
            }
        }

        // Remove enemies that are no longer in detection range from the alreadyDamagedEnemies set
        alreadyDamagedEnemies.RemoveWhere(e => !currentlyDetectedEnemies.Contains(e));
    }

    
    
    // Gérer le démarrage et l'arrêt du sprint
    private void HandleSprint()
    {
        if (_inputManager.SprintInput&&GetCurrentSprintLevels()!=null)
        {
            // Commencer le sprint si ce n'est pas déjà le cas
            HandleEnergyConsumption();
            if (!_isSprinting)
            {
                _originalMoveSpeed = _characterController.moveSpeed;
                if (IsSprintCoroutineRunning()) StopCoroutine(_currentCoroutine);
                _currentCoroutine = StartCoroutine(AccelerateToSprintSpeed());
                _isSprinting = true;
                
                
                //Trigger event
                SprintObserverEvent(PlayerStates.SprintState.StartSprinting,_energyStorage.currentLevelIndex+1);
                SoundManager.Instance.Meth_Active_Sprint();
                UpdateCameraFOV(GetLevelFOV());
            }
            
            
        }
        else if (_isSprinting)
        {
            if(GetCurrentSprintLevels()==null)return;
            // Arrêter le sprint si l'input n'est plus actif
            if (IsSprintCoroutineRunning()) StopCoroutine(_currentCoroutine);
            _currentCoroutine = StartCoroutine(DecelerateToNormalSpeed());
            _isSprinting = false;
    
            //Trigger event
            SprintObserverEvent(PlayerStates.SprintState.StopSprinting,_energyStorage.currentLevelIndex+1);
            
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
    
    // Gizmos drawing to visualize the sprint damage range
    private void OnDrawGizmos()
    {
        if (!drawSprintDamageGizmo) return;
        Gizmos.color = Color.red;
        Vector3 forwardOffset = transform.forward * rangeDistance;
        Vector3 finalPosition = transform.position + forwardOffset+rangeOffset;

        Gizmos.DrawWireSphere(finalPosition, sprintRange);
    }
}
