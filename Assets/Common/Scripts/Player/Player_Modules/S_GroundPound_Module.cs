using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.Serialization;
using DG.Tweening;

public class S_GroundPound_Module : MonoBehaviour
{
    [System.Serializable]
    public class GroundPoundLevel
    {
        public int level; // Niveau requis pour ce niveau
        public float maxSphereRange; // Portée initiale de la détection sphérique (ajustée dynamiquement)
        public float sphereDamage; // Dégâts fixes infligés par l'impact
        public float descentSpeed; // Vitesse maximale de descente
        public float energyConsumption; // Consommation d'énergie pour l'activation
        public int dropBonus;
    }

    [Header("Ground Pound Settings")]
    public List<GroundPoundLevel> groundPoundLevels; // Settings for each level of Ground Pound
    public LayerMask KillableTargetLayer;
    public LayerMask groundLayer; // Target layers for sphere detection
    public float angleThreshold = 75f; // Threshold angle to check if the camera is looking downward (0 = fully downward)
    public float minimumGroundDistance; // Minimum distance from the ground to activate the Ground Pound
    public float PushForce;
    
    private S_InputManager _inputManager; // Gestionnaire des entrées utilisateur
    private S_EnergyStorage _energyStorage; // Stockage d'énergie
    private Transform _cameraTransform; // Transform de la caméra (extrait de CinemachineBrain)
    private CharacterController _characterController; // Contrôleur de personnage
    private S_CustomCharacterController _customCharacterController;
    private bool _isGrounded = false; // Indique si le joueur a touché le sol
    private bool _isGroundPounding = false; // Indique si la compétence est en cours d'utilisation
    private float _dynamicSphereRange; // Portée dynamique basée sur la distance de chute
    public float DynamicSphereRange => _dynamicSphereRange;
    
    private LayerMask savedLayerMask;
    

    //Trigger event
    public event Action<Enum> OnGroundPoundStateChange;

    private void Start()
    {
        // Initialiser les références nécessaires
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _characterController = GetComponent<CharacterController>();
        _customCharacterController = GetComponent<S_CustomCharacterController>();
        _cameraTransform = FindObjectOfType<CinemachineBrain>().transform;
        
        //save layer mask, use to restore layer after ground pound
        savedLayerMask = _characterController.excludeLayers;

    }

    private void Update()
    {
        HandleGroundPound();
    }

    private void GroundPoundObserverEvent(Enum groundPoundState)
    {
        OnGroundPoundStateChange?.Invoke(groundPoundState);
    }

    private void HandleGroundPound()
    {
        if (_isGroundPounding) return; // Ignorer si la compétence est déjà active
        GroundPoundLevel currentLevel = GetCurrentGroundPoundLevel();
        if (currentLevel == null) return;
        // Vérifier l'entrée utilisateur et la quantité d'énergie disponible
        if (_inputManager.MeleeAttackInput)
        {

            if (IsLookingAtValidTarget(out float distanceToGround))
            {
                GroundPoundObserverEvent(PlayerStates.GroundPoundState.StartGroundPound);
                PerformGroundPound(currentLevel, distanceToGround);//Trigger Event
                _energyStorage.RemoveEnergy(currentLevel.energyConsumption); // Consommer l'énergie une fois
            }
        }
    }

    public bool IsLookingAtValidTarget(out float distanceToGround)
    {
        distanceToGround = 0f;
        

        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float verticalDistance = Mathf.Abs(hit.point.y - transform.position.y);
            
            float angle = Vector3.Angle(_cameraTransform.forward, Vector3.down);

            if (verticalDistance > minimumGroundDistance&&angle < angleThreshold)
            {
                distanceToGround = verticalDistance;
                return true;
            }
        }
        return false;
    }

    private void PerformGroundPound(GroundPoundLevel currentLevel, float distanceToGround)
    {
        _isGroundPounding = true; // Marquer la compétence comme active
        StopAllCoroutines(); // Arrêter les coroutines précédentes si nécessaire

        // Ajuster la portée dynamique pour respecter la limite maximale
        _dynamicSphereRange = Mathf.Min(distanceToGround, currentLevel.maxSphereRange);
        
        GroundPoundObserverEvent(PlayerStates.GroundPoundState.isGroundPounding);//Trigger event
        StartCoroutine(MoveToGround(currentLevel.descentSpeed));
    }

    private IEnumerator MoveToGround(float maxSpeed)
    {
        float currentSpeed = 0f; // Initialiser la vitesse actuelle
        float acceleration = maxSpeed / 1f; // Une accélération basée sur le temps total de descente estimé

        while (!_isGrounded)
        {
            if (_customCharacterController.GroundCheck())
            {
                _isGrounded = true;
                break; // Quitter la boucle une fois au sol
            }
            // Augmenter dynamiquement la vitesse jusqu'à la vitesse maximale
            currentSpeed = Mathf.Min(currentSpeed + acceleration*Time.deltaTime, maxSpeed);

            // Déplacement dans la direction de la caméra
            Vector3 direction = _cameraTransform.forward;
            direction.Normalize();
            _characterController.excludeLayers = KillableTargetLayer+savedLayerMask;
            _characterController.Move(direction * (currentSpeed * Time.deltaTime));
            yield return null;
        }

        if (_isGrounded)
        {
            TriggerGroundPoundEffect(); // Exécuter l'effet au sol
            GroundPoundObserverEvent(PlayerStates.GroundPoundState.EndGroundPound);
            
            //add feedback
            SoundManager.Instance.Meth_Pillonage_Explosion();

        }
        _characterController.excludeLayers = savedLayerMask;
        _isGroundPounding = false; // Réinitialiser l'état de la compétence
    }

    private void TriggerGroundPoundEffect()
    {
        GroundPoundLevel currentLevel = GetCurrentGroundPoundLevel();
        if (currentLevel == null) return;
        
        // Effectuer une détection sphérique avec la portée dynamique
        Collider[] hits = Physics.OverlapSphere(transform.position, _dynamicSphereRange, KillableTargetLayer);

        foreach (Collider hit in hits)
        {
            // attack enemy logique
            hit.GetComponent<EnemyBase>()?.ReduceHealth(GetCurrentGroundPoundLevel().sphereDamage,currentLevel.dropBonus);
            Vector3 direction =(hit.transform.position - transform.position).normalized;
            hit.GetComponent<Rigidbody>()?.AddForce(direction * PushForce, ForceMode.Impulse);
        }

        _isGrounded = false; // Réinitialiser l'état de contact avec le sol
    }

    private GroundPoundLevel GetCurrentGroundPoundLevel()
    {
        if (_energyStorage == null) return null;

        int currentLevelIndex = _energyStorage.currentLevelIndex + 1; // Supposons que les niveaux commencent à 1
        return groundPoundLevels.Find(level => level.level == currentLevelIndex);
    }

    private void OnDrawGizmos()
    {
        if (_cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                _cameraTransform = mainCamera.transform;
            }
            else
            {
                return;
            }
        }

        if (groundPoundLevels != null && groundPoundLevels.Count > 0)
        {
            GroundPoundLevel currentLevel = GetCurrentGroundPoundLevel();
            if (currentLevel != null)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(transform.position, _dynamicSphereRange);
            }
        }

        // Visualiser la distance minimale au sol avec une ligne verte
        Gizmos.color = Color.green;
        Vector3 startPoint = transform.position;
        Vector3 endPoint = transform.position + Vector3.down * minimumGroundDistance;
        Gizmos.DrawLine(startPoint, endPoint);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPoint, 0.1f);

        // Visualiser le rayon de la caméra
        Gizmos.color = Color.yellow;
        Vector3 rayEndPoint = _cameraTransform.position + _cameraTransform.forward * 10f;
        Gizmos.DrawLine(_cameraTransform.position, rayEndPoint);

        // Ajouter une sphère pour marquer l'endroit où le rayon frappe un objet valide
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.2f);
        }
    }
}
