using UnityEngine;

public class S_myCharacterController : MonoBehaviour
{
    public float speed = 12f;
    [Range(0, 1)]
    public float accelerationRate = 0.1f;
    [Range(0, 1)]
    public float decelerationRate = 0.1f;
    public float jumpHeight = 3f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.4f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private CharacterController _controller;
    private Vector3 _inputDirection;
    private Vector3 _smoothInputDirection;
    public Vector3 velocity;

    private bool _isGrounded; // Indique si le joueur est au sol
    private float _timeSinceAirborne; // Temps écoulé depuis que le joueur a quitté le sol
    private bool _hasResetVelocity; // Indique si la vitesse verticale a déjà été réinitialisée
    private float _airborneThreshold = 0.05f; // Temps limite avant la réinitialisation de la vitesse après avoir quitté le sol



    [SerializeField]
    private float currentSpeed;

    private void Start()
    {
        InitializeController();
    }

    private void Update()
    {
        HandlePlayerMovement();
        HandleGravity();
        UpdateCurrentSpeed();
        Jump();
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            velocity.y = Mathf.Sqrt(jumpHeight *-2f* gravity);

        }
        
    }

    // Initialiser le contrôleur et les variables nécessaires
    private void InitializeController()
    {
        _controller = GetComponent<CharacterController>();
        _smoothInputDirection = Vector3.zero;
    }

    // Gérer le mouvement du joueur
    private void HandlePlayerMovement()
    {
        RetrieveInputDirection();
        ApplyInputSmoothing();
        MovePlayer();
    }

    // Récupérer la direction d'entrée basée sur les axes horizontaux et verticaux
    private void RetrieveInputDirection()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        _inputDirection = transform.right * x + transform.forward * z;
        if(_inputDirection.magnitude > 1f)
        {
            _inputDirection.Normalize();
        }
    }

    // Appliquer une transition fluide entre les directions d'entrée
    private void ApplyInputSmoothing()
    {
        if (_inputDirection.magnitude > 0f)
        {
            // Transition vers la direction cible (accélération)
            _smoothInputDirection = Vector3.Lerp(_smoothInputDirection, _inputDirection, Time.deltaTime / accelerationRate);
        }
        else
        {
            // Transition vers zéro (décélération)
            _smoothInputDirection = Vector3.Lerp(_smoothInputDirection, Vector3.zero, Time.deltaTime / decelerationRate);

            // Si la magnitude est très faible, définir explicitement à zéro
            if (_smoothInputDirection.magnitude < 0.00001)
            {
                _smoothInputDirection = Vector3.zero;
            }
        }

        if (_smoothInputDirection.magnitude > 1f)
        {
            _smoothInputDirection.Normalize();
        }
    }

    // Déplacer le joueur en utilisant la direction lissée
    private void MovePlayer()
    {
        _controller.Move(_smoothInputDirection * (speed * Time.deltaTime));
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

    // Mettre à jour la vitesse actuelle pour surveiller le déplacement du joueur
    private void UpdateCurrentSpeed()
    {
        currentSpeed = _smoothInputDirection.magnitude * speed;
    }

    // Dessiner les Gizmos pour visualiser la détection du sol
    private void OnDrawGizmos()
    {
        Gizmos.color = GroundCheck() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
