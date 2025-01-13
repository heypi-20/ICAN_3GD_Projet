using System;
using UnityEngine;
using UnityEngine.Serialization;

public class S_myCharacterController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 12f;

    public float AirControl = 0.5f;
    [Range(0, 1)]
    public float accelerationTime = 0.02f;
    public AnimationCurve accelerationCurve = AnimationCurve.Linear(0, 0, 1, 1);
    [Range(0, 1)]
    public float decelerationTime = 0.02f;
    public AnimationCurve decelerationCurve = AnimationCurve.Linear(0, 1, 1, 0);
    public float _directionLerpSpeed = 10f;
    [Header("Jump Settings")]
    public float jumpHeight = 3f;
    
    [Header("Ground Settings")]
    public float gravity = -19.62f;
    public float groundCheckDistance = 0.59f;
    public float groundCheckRadius = 0.49f;
    public LayerMask groundLayer;
    
    private CharacterController _controller;
    private S_InputManager _inputManager;
    
    private float _inputHorizontal_X;
    private float _inputVertical_Z;
    private Vector3 _inputDirection;
    
    //Smoth Speed var
    private float _accelerationTimer = 0f;
    private float _decelerationTimer = 0f;
    private Vector3 _smoothedDirection = Vector3.zero;
    
    
    public Vector3 velocity;
    
    private bool _isGrounded; // Indique si le joueur est au sol
    private float _timeSinceAirborne; // Temps écoulé depuis que le joueur a quitté le sol
    private bool _hasResetVelocity; // Indique si la vitesse verticale a déjà été réinitialisée
    private float _airborneThreshold = 0.05f; // Temps limite avant la réinitialisation de la vitesse après avoir quitté le sol
    [SerializeField]
    private float currentSpeed;
    private Vector3 _lastMoveDirection = Vector3.zero; 



    private void Start()
    {
        InitializeController();
    }

    private void Update()
    {
        ControllerInput();
        MovePlayer();
        HandleGravity();
        Jump();
    }
    // Initialiser le contrôleur et les variables nécessaires
    private void InitializeController()
    {
        _controller = GetComponent<CharacterController>();
        _inputManager = FindObjectOfType<S_InputManager>();

    }
    private void ControllerInput()
    {
        //movement input
        _inputHorizontal_X = _inputManager.MoveInput.x;
        _inputVertical_Z = _inputManager.MoveInput.y;
    }
    
    private bool _wasGrounded;
    private Vector3 _inertiaDirection;
    private void MovePlayer()
    {
        // 获取输入方向
        _inputDirection = (transform.right * _inputHorizontal_X + transform.forward * _inputVertical_Z);

        // 地面移动逻辑
        if (GroundCheck())
        {
            if (_inputDirection.magnitude > 0.1f)
            {
                _lastMoveDirection = _inputDirection;
            }

            SmoothSpeed();
            _wasGrounded = true;
        }
        else
        {
            if (_wasGrounded)
            {
                _inertiaDirection = _lastMoveDirection;
                _wasGrounded = false;
            }

            _inertiaDirection = _inputDirection.magnitude > 0.1f
                ? Vector3.Lerp(_inertiaDirection, _inputDirection, AirControl * Time.deltaTime)
                : _inertiaDirection;
        }

        // 应用移动
        Vector3 finalMoveDirection = GroundCheck() ? _lastMoveDirection : _inertiaDirection;
        _controller.Move(finalMoveDirection * (currentSpeed * Time.deltaTime));
    }


    private void SmoothSpeed()
    {
        if (_inputDirection.magnitude > 0) 
        {
            _decelerationTimer = 0f;
            _accelerationTimer += Time.deltaTime / accelerationTime;


            float curveValue = accelerationCurve.Evaluate(Mathf.Clamp01(_accelerationTimer));
            currentSpeed = curveValue * moveSpeed;
        }
        else 
        {
            _accelerationTimer = 0f;
            _decelerationTimer += Time.deltaTime / decelerationTime;


            float curveValue = decelerationCurve.Evaluate(Mathf.Clamp01(_decelerationTimer));
            currentSpeed = curveValue * currentSpeed;


            if (currentSpeed < 0.01f)
            {
                currentSpeed = 0f;
            }
        }
    }

    
    
    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight *-2f* gravity);

        }
        
    }
    // Gérer la gravité pour les mouvements verticaux
    private void HandleGravity()
    {
        if (GroundCheck()&&velocity.y<0)
        {
            // // Lorsque le joueur touche le sol
            // if (!_isGrounded)
            // {
            //     // Si le joueur vient d'atterrir, appliquer une gravité spéciale
            //     _isGrounded = true;
            //     velocity.y = -speed; // Réinitialiser la vitesse
            //     _timeSinceAirborne = 0f; // Réinitialiser le chronomètre
            //     _hasResetVelocity = false; // Réinitialiser le drapeau de réinitialisation
            // }
            // else
            // {
            //     // Si le joueur reste au sol, maintenir la vitesse
            //     velocity.y = -speed;
            // }
            velocity.y = -2;
        }
        else
        {
            // // Lorsque le joueur quitte le sol
            // if (_isGrounded)
            // {
            //     // Si le joueur vient de quitter le sol, démarrer le chronomètre
            //     _isGrounded = false;
            //     _timeSinceAirborne = 0f;
            // }
            //
            // // Mettre à jour le temps écoulé depuis que le joueur a quitté le sol
            // _timeSinceAirborne += Time.deltaTime;
            //
            // // Si le joueur est en l'air depuis plus longtemps que le seuil et que la vitesse n'a pas encore été réinitialisée
            // if (_timeSinceAirborne > _airborneThreshold && !_hasResetVelocity)
            // {
            //     velocity.y = 0; // Réinitialiser la vitesse verticale
            //     _hasResetVelocity = true; // Marquer comme réinitialisé
            // }

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
    private bool GroundCheck()
    {
        return Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);
    }



    // Dessiner les Gizmos pour visualiser la détection du sol
    private void OnDrawGizmos()
    {
        Gizmos.color = GroundCheck() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
