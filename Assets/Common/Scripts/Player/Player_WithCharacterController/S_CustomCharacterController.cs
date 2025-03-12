using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.Serialization;

public class S_CustomCharacterController : MonoBehaviour
{
    
    [Header("Movement Settings")]
    public float moveSpeed = 12f;
    public float AirControl = 5f;
    [Range(0, 1)]
    public float accelerationTime = 0.1f;
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0, 1)]
    public float decelerationTime = 0.3f;
    public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);
    public float _directionLerpSpeed = 10f;
    
    [Header("Ground Settings")]
    public float gravity = -19.62f;
    public float groundCheckDistance = 0.59f;
    public float groundCheckRadius = 0.49f;
    public LayerMask groundLayer;
    public float groundCheckBufferTime = 0.1f; // Durée du buffer avant de considérer que le joueur n'est plus au sol
    
    private float lastGroundedTime = 0f; // Dernière fois où le joueur était au sol

    // Composants
    private CharacterController _controller;
    private S_InputManager _inputManager;
    
    
    // Valeurs d'entrée
    private float _inputHorizontal_X;
    private float _inputVertical_Z;
    
    
    // Accéder aux valeurs d'entrée dans d'autres scripts pour gérer la vitesse
    public Vector3 _inputDirection{ get; private set; }
    [Header("Debugger")]
    // Actuellement utilisé pour appliquer la gravité et le saut ==== En cours de travail (WIP) ====
    public Vector3 velocity;
    // Variables pour la logique de mouvement au sol du joueur
    public float currentSpeed;
    private Vector3 _lastMoveDirection = Vector3.zero; 
    
    // Variables privées pour la logique d'accélération et de décélération du joueur
    private float _accelerationTimer = 0f;
    private float _decelerationTimer = 0f;
    
    // Variables privées pour le lissage de la direction du joueur au sol
    private Vector3 _smoothedDirection = Vector3.zero;
    
    // Variables privées pour la logique de mouvement en l'air du joueur
    private bool _wasGrounded;
    private Vector3 _inertiaDirection;
    private float _airborneSpeed;

    public event Action<Enum,Vector2> OnMoveStateChange;
    private bool isMovingUseForEvent = false; 
    private Vector2 _previousDirection = Vector2.zero;

    private void Start()
    {
        InitializeController();
    }

    private void Update()
    {
        ControllerInput();
        MovePlayer();
        HandleGravity();
        MovementObserverEvent();
    }

    private void MovementObserverEvent()
    {
        Vector2 currentDirection = new Vector2(_inputHorizontal_X, _inputVertical_Z);

        // Check if the player starts moving
        if (!isMovingUseForEvent && currentDirection.magnitude > 0.1f)
        {
            isMovingUseForEvent = true;
            _previousDirection = Vector2.zero; // Save the initial direction
            OnMoveStateChange?.Invoke(PlayerStates.MoveState.StartMoving, Vector2.zero);
        }

        // Check if the player stops moving
        if (isMovingUseForEvent && currentDirection.magnitude <= 0.1f)
        {
            isMovingUseForEvent = false;
            _previousDirection = Vector2.zero; // Reset direction
            OnMoveStateChange?.Invoke(PlayerStates.MoveState.StopMoving, Vector2.zero);
            return;
        }

        // Trigger "Moving" only if the direction changes
        if (isMovingUseForEvent && !_previousDirection.Equals(currentDirection))
        {
            _previousDirection = currentDirection;
            OnMoveStateChange?.Invoke(PlayerStates.MoveState.IsMoving, currentDirection);
        }
    }


    private void InitializeController()
    {
        // Initialisation : obtention des composants nécessaires
        _controller = GetComponent<CharacterController>();
        _inputManager = FindObjectOfType<S_InputManager>();
    }
    private void ControllerInput()  
    {
        // Obtenir les valeurs d'entrée depuis le gestionnaire d'entrée (Input Manager)
        _inputHorizontal_X = _inputManager.MoveInput.x;
        _inputVertical_Z = _inputManager.MoveInput.y;
    }
    

    private void MovePlayer()
    {
        // Calculer la direction vers l'avant en fonction des valeurs d'entrée
        _inputDirection = (transform.right * _inputHorizontal_X + transform.forward * _inputVertical_Z);

        // Vérification si le joueur est au sol ; si oui, exécuter la logique de déplacement normale
        if (GroundCheck())
        {
            
            // Rendre les rotations du joueur plus fluides
            _smoothedDirection = Vector3.Lerp(_smoothedDirection, _inputDirection, Time.deltaTime * _directionLerpSpeed);
            
            // Enregistrer la direction juste avant que le joueur relâche la touche
            if (_inputDirection.magnitude > 0.1f)
            {
                _lastMoveDirection = _smoothedDirection;
            }
            
            // Gérer l'accélération et la décélération
            SmoothSpeed();
            
            // Réinitialiser la variable _wasGrounded
            _wasGrounded = true;
        }
        else // Lorsque le joueur n'est pas au sol
        {
            // Sauvegarder la direction et la vitesse d'inertie au moment où le joueur quitte le sol
            if (_wasGrounded)
            {
                // Si aucune direction d'entrée, rester immobile
                _inertiaDirection = _inputDirection.magnitude > 0.1f ? _inputDirection : Vector3.zero;
                // Obtenir la vitesse actuelle de déplacement
                _airborneSpeed = currentSpeed;
                // Après avoir sauvegardé les valeurs, définir _wasGrounded à false pour éviter de remplacer les valeurs sauvegardées à la prochaine frame
                _wasGrounded = false;
            }

            // Logique de mouvement en l'air, vérifier si une touche de direction est pressée
            if (_inputDirection.magnitude > 0.1f)
            {
                // Changer la direction en l'air, la sensibilité est contrôlée par AirControl
                _inertiaDirection = Vector3.Lerp(_inertiaDirection, _inputDirection, AirControl * Time.deltaTime);
                
                // Sauvegarder la dernière direction de déplacement pour gérer la décélération après l'atterrissage
                _lastMoveDirection = _inertiaDirection;
            }

            // Mettre à jour la vitesse en l'air
            _airborneSpeed = Mathf.Lerp(_airborneSpeed, moveSpeed, AirControl * Time.deltaTime);
        }

        // Appliquer la direction et la vitesse finale
        Vector3 finalMoveDirection = GroundCheck() ? _lastMoveDirection : _inertiaDirection;
        float finalSpeed = GroundCheck() ? currentSpeed : _airborneSpeed;
        _controller.Move(finalMoveDirection * (finalSpeed * Time.deltaTime));
        
    }

    private float _initialDecelerationSpeed=0f;
    private void SmoothSpeed()
    {
        if (_inputDirection.magnitude > 0) 
        {
            // Réinitialiser le compteur de décélération
            _decelerationTimer = 0f;
            // Définir le temps nécessaire pour atteindre la vitesse maximale, basé sur accelerationTime
            _accelerationTimer += Time.deltaTime / accelerationTime;
            // Utiliser une courbe d'accélération pour ajuster la vitesse de progression
            float curveValue = accelerationCurve.Evaluate(Mathf.Clamp01(_accelerationTimer));
            // Mettre à jour currentSpeed pour refléter un effet d'accélération plus fluide
            currentSpeed = Mathf.Lerp(0f, moveSpeed, curveValue);
        }
        else 
        {
            if (_decelerationTimer == 0f)
            {
                _initialDecelerationSpeed=currentSpeed;
            }
            // Identique à la logique d'accélération
            _accelerationTimer = 0f;
            _decelerationTimer += Time.deltaTime / decelerationTime;
            float curveValue = decelerationCurve.Evaluate(Mathf.Clamp01(_decelerationTimer));
            currentSpeed = Mathf.Lerp(0f, _initialDecelerationSpeed, curveValue);
            
            // Fixer la vitesse à zéro si elle devient très faible pour éviter des erreurs
            if (currentSpeed < 0.01f)
            {
                currentSpeed = 0f;
            }
        }
    }
    private void HandleGravity()
    {
        if (GroundCheck()&&velocity.y<0)
        {
            velocity.y = -10;
        }
        else
        {
            // Appliquer la gravité en continu
            AddGravityEffect();
        }

        if (_controller.collisionFlags == CollisionFlags.Above)
        {
            velocity.y = -2;
        }
    }

    // Ajouter l'effet de gravité au vecteur de vitesse
    private void AddGravityEffect()
    {
        velocity.y += gravity * Time.deltaTime;
        _controller.Move(velocity * Time.deltaTime);
    }

    // Vérifier si le joueur est au sol
    public bool GroundCheck()
    {
        bool isGrounded = Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);

        if (isGrounded)
        {
            lastGroundedTime = Time.time; // Met à jour le dernier moment où le joueur était au sol
            return true;
        }

        // Si le joueur était récemment au sol, considère qu'il est encore "au sol" pour un temps défini
        return Time.time - lastGroundedTime <= groundCheckBufferTime;
    }



    // Dessiner les Gizmos pour visualiser la détection du sol
    private void OnDrawGizmos()
    {
        Gizmos.color = GroundCheck() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
