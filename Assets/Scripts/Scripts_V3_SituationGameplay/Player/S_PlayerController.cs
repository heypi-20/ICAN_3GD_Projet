using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("La vitesse de déplacement du joueur")]
    public float moveSpeed = 5f;
    [Tooltip("Le coefficient de friction du joueur lorsqu'il touche le sol")]
    public float groundDrag = 5f;
    [Tooltip("Le multiplicateur de mouvement en l'air")]
    public float airMultiplier = 0.4f;

    [Header("Rotation")]
    [Tooltip("La vitesse de rotation du joueur en fonction de la position de la souris")]
    public float rotationSpeed = 1200f;

    [Header("Jump Settings")]
    [Tooltip("La force du saut")]
    public float jumpForce = 5f;
    [Tooltip("Force gravitationnelle supplémentaire appliquée pour des chutes plus réalistes")]
    public float extraGravity = 2f;
    

    [Header("Keybinds")]
    [Tooltip("La touche utilisée pour sauter")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Ground Check")]
    [Tooltip("Référence à un script de vérification du sol pour savoir si le joueur est au sol")]
    public S_GroundCheck groundCheck;

    private bool isJumping = false;  // Indique si le joueur est en train de sauter
    
    private int currentJumps = 0;    // Le nombre actuel de sauts effectués
    private Rigidbody rb;  // Référence au Rigidbody du joueur
    private float horizontalInput;  // Entrée horizontale de déplacement
    private float verticalInput;  // Entrée verticale de déplacement
    private Vector3 moveDirection;  // La direction dans laquelle le joueur se déplace

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        HandleInput();  // Gérer les entrées utilisateur
        RotatePlayer();  // Gérer la rotation du joueur
        SpeedControl();  // Contrôler la vitesse de déplacement

        // Appliquer le drag au sol
        if (groundCheck.IsGrounded)
        {
            rb.drag = groundDrag;
            currentJumps = 0;  // Réinitialiser les sauts lorsque le joueur touche le sol
        }
        else
        {
            rb.drag = 0;
        }
    }

    void FixedUpdate()
    {
        MovePlayer();  // Déplacer le joueur
        ApplyExtraGravity();  // Appliquer de la gravité supplémentaire

        if (isJumping)
        {
            Jump();  // Effectuer un saut
            isJumping = false;  // Réinitialiser l'état de saut
        }
    }

    // Gérer les entrées utilisateur
    private void HandleInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Gérer l'entrée pour le saut, seulement si le nombre de sauts autorisés n'est pas atteint
        if (Input.GetKeyDown(jumpKey)&&groundCheck.IsGrounded)
        {
            isJumping = true;
        }
    }

    // Déplacer le joueur
    private void MovePlayer()
    {
        // Calculer la direction du mouvement
        moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;

        // Appliquer la force de déplacement au sol
        if (groundCheck.IsGrounded)
        {
            rb.AddForce(moveDirection * moveSpeed * 10f, ForceMode.Force);
        }
        // Appliquer la force de déplacement en l'air
        else
        {
            rb.AddForce(moveDirection * moveSpeed * 10f * airMultiplier, ForceMode.Force);
        }
    }

    // Contrôler la vitesse pour éviter que le joueur ne dépasse la vitesse maximale
    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limiter la vitesse
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.velocity = new Vector3(limitedVel.x, rb.velocity.y, limitedVel.z);
        }
    }

    // Gérer le saut
    private void Jump()
    {
        // Réinitialiser la vitesse sur l'axe Y
        rb.velocity = new Vector3(rb.velocity.x*0.5f, 0f, rb.velocity.z*0.5f);

        // Appliquer la force du saut
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

        currentJumps++;  // Incrémenter le nombre de sauts
    }

    // Appliquer de la gravité supplémentaire lorsque le joueur est en chute
    private void ApplyExtraGravity()
    {
        if (!groundCheck.IsGrounded && rb.velocity.y < 0)
        {
            rb.AddForce(Vector3.down * extraGravity * 10f, ForceMode.Acceleration);
        }
    }

    // Gérer la rotation du joueur en fonction de la position de la souris
    private void RotatePlayer()
    {
        // Obtenir la position de la souris dans le monde
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            // Calculer la direction vers laquelle le joueur doit se tourner
            Vector3 directionToLook = hit.point - transform.position;
            directionToLook.y = 0f;  // S'assurer que la rotation est seulement sur le plan horizontal

            // Calculer la rotation cible
            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);

            // Appliquer une rotation lissée vers la direction cible
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }
}
