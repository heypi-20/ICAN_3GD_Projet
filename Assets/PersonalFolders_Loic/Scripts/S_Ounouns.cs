using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class S_Ounouns : EnemyBase
{
    [Header("Shoot Properties")]
    public float fireRate;                          // Time between shots
    public float range;                             // Detection and shooting range
    public Transform projectilePrefab;              // Prefab of the projectile
    public Transform shootPoint;                    // Starting point of the projectile
    public float projectileSpeed = 10f;             // Speed of the projectile

    [Header("Movement Properties")]
    public float stopDistance = 5f;                 // Distance at which enemy stops moving

    private NavMeshAgent agent;
    private S_CustomCharacterController findPlayer;
    private Transform player;
    private RaycastHit hit;
    private float shootTimer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        findPlayer = FindObjectOfType<S_CustomCharacterController>();
        if (findPlayer == null)
        {
            Debug.LogWarning("No Character Controller found in scene.");
        }
    }

    private void Update()
    {
        if (findPlayer == null) return;

        player = findPlayer.transform;

        MoveTowardsPlayer();

        shootTimer += Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < range && shootTimer >= fireRate)
        {
            Shoot();
            shootTimer = 0f;
        }
    }

    /// <summary>
    /// Moves the enemy toward the player if farther than stopDistance.
    /// Rotates to face the player horizontally only (Y axis).
    /// </summary>
    private void MoveTowardsPlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            agent.SetDestination(player.position);
        }

        // Look at the player horizontally (ignore vertical difference)
        Vector3 lookDirection = player.position - transform.position;
        lookDirection.y = 0f;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    /// <summary>
    /// Shoots a projectile toward the player if within line of sight.
    /// </summary>
    private void Shoot()
    {
        // Calculate direction from shootPoint to player
        Vector3 shootDirection = (player.position - shootPoint.position).normalized;

        // Check for line of sight to the player
        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range))
        {
            if (hit.transform == player)
            {
                // Instantiate and launch the projectile toward the player
                Transform projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
                projectile.GetComponent<S_ProjectileSpeed>().speed = projectileSpeed;

            }
        }
    }
}
