using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_myCharacterController : MonoBehaviour
{
    public float speed = 12f;
    [Range(0, 1)]
    public float accelerationRate = 0.1f;
    [Range(0, 1)]
    public float decelerationRate = 0.1f;
    public float gravity = -9.81f;
    public float groundCheckDistance = 0.4f;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private CharacterController controller;
    private Vector3 inputDirection;
    private Vector3 smoothInputDirection;
    public Vector3 velocity;

    private bool isGrounded; // Indique si le joueur est au sol
    private float timeSinceAirborne; // Temps écoulé depuis que le joueur a quitté le sol
    private bool hasResetVelocity; // Indique si la vitesse verticale a déjà été réinitialisée
    private float airborneThreshold = 0.05f; // Temps limite avant la réinitialisation de la vitesse après avoir quitté le sol



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
    }

    // Initialiser le contrôleur et les variables nécessaires
    private void InitializeController()
    {
        controller = GetComponent<CharacterController>();
        smoothInputDirection = Vector3.zero;
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
        inputDirection = transform.right * x + transform.forward * z;
        if(inputDirection.magnitude > 1f)
        {
            inputDirection.Normalize();
        }
    }

    // Appliquer une transition fluide entre les directions d'entrée
    private void ApplyInputSmoothing()
    {
        if (inputDirection.magnitude > 0f)
        {
            // Transition vers la direction cible (accélération)
            smoothInputDirection = Vector3.Lerp(smoothInputDirection, inputDirection, Time.deltaTime / accelerationRate);
        }
        else
        {
            // Transition vers zéro (décélération)
            smoothInputDirection = Vector3.Lerp(smoothInputDirection, Vector3.zero, Time.deltaTime / decelerationRate);

            // Si la magnitude est très faible, définir explicitement à zéro
            if (smoothInputDirection.magnitude < 0.00001)
            {
                smoothInputDirection = Vector3.zero;
            }
        }

        if (smoothInputDirection.magnitude > 1f)
        {
            smoothInputDirection.Normalize();
        }
    }

    // Déplacer le joueur en utilisant la direction lissée
    private void MovePlayer()
    {
        controller.Move(smoothInputDirection * speed * Time.deltaTime);
    }

    // Gérer la gravité pour les mouvements verticaux
    private void HandleGravity()
    {
        if (GroundCheck())
        {
            // Lorsque le joueur touche le sol
            if (!isGrounded)
            {
                // Si le joueur vient d'atterrir, appliquer une gravité spéciale
                isGrounded = true;
                velocity.y = -speed; // Réinitialiser la vitesse
                timeSinceAirborne = 0f; // Réinitialiser le chronomètre
                hasResetVelocity = false; // Réinitialiser le drapeau de réinitialisation
            }
            else
            {
                // Si le joueur reste au sol, maintenir la vitesse
                velocity.y = -speed;
            }
        }
        else
        {
            // Lorsque le joueur quitte le sol
            if (isGrounded)
            {
                // Si le joueur vient de quitter le sol, démarrer le chronomètre
                isGrounded = false;
                timeSinceAirborne = 0f;
            }

            // Mettre à jour le temps écoulé depuis que le joueur a quitté le sol
            timeSinceAirborne += Time.deltaTime;

            // Si le joueur est en l'air depuis plus longtemps que le seuil et que la vitesse n'a pas encore été réinitialisée
            if (timeSinceAirborne > airborneThreshold && !hasResetVelocity)
            {
                velocity.y = 0; // Réinitialiser la vitesse verticale
                hasResetVelocity = true; // Marquer comme réinitialisé
            }

            // Appliquer la gravité en continu
            AddGravityEffect();
        }
    }

    // Ajouter l'effet de gravité au vecteur de vitesse
    private void AddGravityEffect()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // Vérifier si le joueur est au sol
    private bool GroundCheck()
    {
        return Physics.SphereCast(transform.position, groundCheckRadius, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer);
    }

    // Mettre à jour la vitesse actuelle pour surveiller le déplacement du joueur
    private void UpdateCurrentSpeed()
    {
        currentSpeed = smoothInputDirection.magnitude * speed;
    }

    // Dessiner les Gizmos pour visualiser la détection du sol
    private void OnDrawGizmos()
    {
        Gizmos.color = GroundCheck() ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position + Vector3.down * groundCheckDistance, groundCheckRadius);
    }
}
