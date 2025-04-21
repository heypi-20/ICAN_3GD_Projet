using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class S_OunounsBehavior : EnemyBase
{
    [Header("Shoot Properties")]
    public float fireRate; // Time between each shot
    public float range; // Shooting and detection range
    public Transform projectilePrefab; // Projectile to shoot
    public Transform shootPoint; // Where the projectile is spawned
    public float projectileSpeed = 10f; // Speed of the projectile

    [Header("Movement Properties")]
    public float moveSpeed = 3f; // Movement speed of the enemy
    public float stopDistance = 5f; // Stop moving when closer than this distance

    private Transform player; // Reference to the player
    private RaycastHit hit; // Raycast hit info
    private float shootTimer; // Timer to handle fire rate

    private void Start()
    {
        // Find the player in the scene
        player = FindObjectOfType<S_CustomCharacterController>().transform;
    }

    private void Update()
    {
        if (player == null)
            return;

        // Move towards the player if needed
        MoveTowardsPlayer();

        // Handle shooting if within range
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
    /// Moves the enemy toward the player, but only rotates horizontally (ignores vertical axis).
    /// </summary>
    private void MoveTowardsPlayer()
    {
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist > stopDistance)
        {
            Vector3 direction = (player.position - transform.position).normalized;

            // Move toward the player
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Rotate to face the player horizontally (Y axis only)
            Vector3 lookDirection = player.position - transform.position;
            lookDirection.y = 0f; // Ignore vertical difference
            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.LookRotation(lookDirection);
        }
    }

    /// <summary>
    /// Shoots a projectile toward the player if there is a clear line of sight.
    /// </summary>
    private void Shoot()
    {
        // Calculate direction from shootPoint to the player
        Vector3 shootDirection = (player.position - shootPoint.position).normalized;

        // Check if the player is directly in sight
        if (Physics.Raycast(shootPoint.position, shootDirection, out hit, range))
        {
            if (hit.transform == player)
            {
                // Instantiate the projectile and make it face the player
                Transform projectile = Instantiate(projectilePrefab, shootPoint.position, Quaternion.LookRotation(shootDirection));
                projectile.GetComponent<S_ProjectileSpeed>().speed = projectileSpeed;
            }
        }
    }
}
