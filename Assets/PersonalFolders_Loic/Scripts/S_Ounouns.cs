using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class S_Ounouns : EnemyBase
{
    [Header("Shoot Properties")]
    public float fireRate;
    public float range;
    public Transform projectilePrefab;
    public Transform shootPoint;
    public float projectileSpeed = 10f;

    [Header("Movement Properties")]
    public float stopDistance = 5f;

    private NavMeshAgent agent;
    private S_CustomCharacterController findPlayer;
    private Transform player;
    private RaycastHit hit;
    private float shootTimer;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        findPlayer = FindObjectOfType<S_CustomCharacterController>();
        if (findPlayer == null) {
            Debug.LogWarning("No Character Controller");
        }
    }

    void Update()
    {
        player = findPlayer.transform;

        // Déplace l'ennemi vers le joueur si nécessaire
        MoveTowardsPlayer();

        shootTimer += Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);
        if (dist < range)
        {
            if (shootTimer >= fireRate)
            {
                Shoot();
                shootTimer = 0;
            }
        }
    }

    /// <summary>
    /// Déplace l'ennemi vers le joueur si la distance est supérieure à stopDistance.
    /// </summary>
    private void MoveTowardsPlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        // Si le joueur est à une distance supérieure à stopDistance, l'ennemi se déplace vers lui
        if (dist > stopDistance)
        {
            agent.SetDestination(player.position);

            // Fait regarder l'ennemi vers le joueur
            transform.LookAt(player.position);
        }
    }

    private void Shoot()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, range))
        {
            if (hit.transform == player)
            {
                Transform projectile = Instantiate(projectilePrefab, shootPoint.position, transform.rotation);
                projectile.GetComponent<S_ProjectileSpeed>().speed = projectileSpeed;

                SoundManager.Instance.Meth_Dashoot_Shoot();
            }
        }
    }
}

