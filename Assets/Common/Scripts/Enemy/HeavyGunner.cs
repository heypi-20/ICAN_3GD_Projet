using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;

[RequireComponent(typeof(Rigidbody))]
public class HeavyGunner : EnemyBase
{
    [Header("Shoot Properties")]
    public float fireRate; // Fréquence de tir
    public float range; // Portée de détection du joueur
    public Transform projectilePrefab; // Préfabriqué du projectile
    public Transform shootPoint; // Point de départ du projectile
    public float projectileSpeed = 10f; // Vitesse du projectile

    [Header("Movement Properties")]
    public float moveSpeed = 3f; // Vitesse de déplacement de l'ennemi
    public float stopDistance = 5f; // Distance à laquelle l'ennemi s'arrête

    private Transform player; // Référence au joueur
    private RaycastHit hit; // Stocke les informations sur ce que le raycast touche
    private float shootTimer; // Timer pour gérer la fréquence de tir

    private void Start()
    {
        // Trouve le joueur dans la scène
        player = FindObjectOfType<S_CustomCharacterController>().transform;
    }

    // Update est appelé une fois par frame
    void Update()
    {
        if (player == null)
            return;

        // Déplace l'ennemi vers le joueur si nécessaire
        MoveTowardsPlayer();

        // Gestion du tir
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
        // Calcule la distance entre l'ennemi et le joueur
        float dist = Vector3.Distance(transform.position, player.position);

        // Si le joueur est à une distance supérieure à stopDistance, l'ennemi se déplace vers lui
        if (dist > stopDistance)
        {
            // Calcule la direction vers le joueur
            Vector3 direction = (player.position - transform.position).normalized;

            // Déplace l'ennemi
            transform.position += direction * moveSpeed * Time.deltaTime;

            // Fait regarder l'ennemi vers le joueur
            transform.LookAt(player.position);
        }
    }

    private void Shoot()
    {
        // Vérifie si le joueur est dans la ligne de mire
        if (Physics.Raycast(transform.position, transform.forward, out hit, range))
        {
            if (hit.transform == player)
            {
                // Instancie le projectile et lui donne une vitesse
                Transform projectile = Instantiate(projectilePrefab, shootPoint.position, transform.rotation);
                projectile.GetComponent<S_ProjectileSpeed>().speed = projectileSpeed;

                // Joue un son de tir
                SoundManager.Instance.Meth_Dashoot_Shoot();
            }
        }
    }
}