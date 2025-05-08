using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class S_Ounouns : EnemyBase
{
    [Header("Movement Properties")]
    public float moveSpeed = 2f;
    public float stopDistance = 5f;
    public float rotationSpeed = 10f;
    
    [Header("Shoot Properties")]
    public float fireRate;                          // Time between shots
    public float range;                             // Detection and shooting range
    public Transform projectilePrefab;              // Prefab of the projectile
    public Transform shootPoint;                    // Starting point of the projectile
    public float projectileSpeed = 10f;             // Speed of the projectile
    
    private S_CustomCharacterController findPlayer;
    private Transform player;
    private RaycastHit hit;
    private float shootTimer;
    
    private S_EnemyGroundCheck groundCheck;
    private Rigidbody rb;

    private void Start()
    {
        findPlayer = FindObjectOfType<S_CustomCharacterController>();
        if (findPlayer == null) {
            Debug.LogWarning("No Character Controller found in scene.");
        }

        groundCheck = GetComponent<S_EnemyGroundCheck>();
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        player = findPlayer.transform;

        shootTimer += Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);
        
        MoveTowardsPlayer(dist);

        if (dist < range && shootTimer >= fireRate) {
            Shoot();
            shootTimer = 0f;
        }

        if (!groundCheck.TriggerDetection()) {
            transform.position += transform.up * (-9.81f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Moves the enemy toward the player if farther than stopDistance.
    /// Rotates to face the player horizontally only (Y axis).
    /// </summary>
    private void MoveTowardsPlayer(float dist)
    {
        Vector3 direction = (player.position - transform.position).normalized;
        // direction.y = 0;
        if (dist > stopDistance) {
            // Move toward the player
            rb.velocity = direction * (moveSpeed * Time.deltaTime);
        }

        Quaternion targetRot = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
    }

    /// <summary>   
    /// Shoots a projectile toward the player if within line of sight.
    /// </summary>
    private void Shoot()
    {
        // Calculate direction from shootPoint to player
        Vector3 shootDirection = (player.position - shootPoint.position).normalized;

        // Check for line of sight to the player
        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range)) {
            if (hit.transform == player) {
                // Instantiate and launch the projectile toward the player
                Transform projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
                projectile.GetComponent<S_ProjectileSpeed>().speed = projectileSpeed;
            }
        }
    }
}
