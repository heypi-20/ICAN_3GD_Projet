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
    [Tooltip("Utiliser la souris pour contrôler la direction au lieu de RotatePlayer")]
    public bool FPSOption = false;
    [Tooltip("La limite de rotation verticale vers le haut")]
    public float maxLookUpAngle = 60f;
    [Tooltip("La limite de rotation verticale vers le bas")]
    public float maxLookDownAngle = -60f;

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
    private float verticalRotation = 0f;  // Rotation verticale actuelle de la caméra

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Initialiser l'état du curseur
        UpdateCursorState();
    }

    void Update()
    {
        HandleInput();  // Gérer les entrées utilisateur

        if (!FPSOption)
        {
            // RotatePlayer();  // Gérer la rotation du joueur si l'option est désactivée
        }
        else
        {
            RotateWithCamera();  // Gérer la rotation du joueur pour qu'elle suive la caméra en mode FPS
        }

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
        if (Input.GetKeyDown(jumpKey) && groundCheck.IsGrounded)
        {
            isJumping = true;
        }

        // Gérer la rotation avec la souris si l'option est activée
        if (FPSOption)
        {
            // RotateWithMouse();
            UpdateCursorState();  // Mettre à jour l'état du curseur en fonction du mode FPS
        }
    }

    // Déplacer le joueur
    private void MovePlayer()
    {
        if (FPSOption)
        {
            // Déplacement dans la direction de la caméra (FPS-style)
            moveDirection = Camera.main.transform.forward * verticalInput + Camera.main.transform.right * horizontalInput;
            moveDirection.y = 0f;  // Empêcher le déplacement vertical
            moveDirection.Normalize();
        }
        else
        {
            // Déplacement traditionnel
            moveDirection = new Vector3(horizontalInput, 0f, verticalInput).normalized;
            Quaternion lookRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 10f);
        }

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
        rb.velocity = new Vector3(rb.velocity.x * 0.5f, 0f, rb.velocity.z * 0.5f);

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
    void RotatePlayer()
    {
        Plane groundPlane = new Plane(Vector3.up, transform.position);
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (groundPlane.Raycast(ray, out float enter))  
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 directionToLook = hitPoint - transform.position;
            directionToLook.y = 0f;

            Quaternion targetRotation = Quaternion.LookRotation(directionToLook);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    // Gérer la rotation du joueur directement avec la souris (FPS-style)
    void RotateWithMouse()
    {
        float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;
        float mouseY = -Input.GetAxis("Mouse Y") * rotationSpeed * Time.deltaTime;

        // Rotation horizontale
        transform.Rotate(0, mouseX, 0);

        // Rotation verticale
        verticalRotation += mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, maxLookDownAngle, maxLookUpAngle);

        Camera.main.transform.localEulerAngles = new Vector3(verticalRotation, Camera.main.transform.localEulerAngles.y, 0);
    }

    // Gérer la rotation du joueur pour qu'elle suive la caméra en mode FPS
    void RotateWithCamera()
    {
        transform.forward = Camera.main.transform.forward;
    }

    // Mettre à jour l'état du curseur en fonction du mode FPS
    void UpdateCursorState()
    {
        if (FPSOption)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}
