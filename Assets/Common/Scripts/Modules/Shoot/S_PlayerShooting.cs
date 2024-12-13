using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab; // The prefab for the projectile
    public Transform shootingPoint; // The point from where the projectile is fired
    public float fireRate = 2f; // Projectiles per second
    public float projectileSpeed = 20f; // Speed of the projectile
    public float projectileLifetime = 5f; // Lifetime of the projectile in seconds
    public int maxCollisions = 3; // Maximum number of collisions for the projectile
    public float energyConsumptionPerShot = 5f; // Energy consumed per shot

    [Header("References")]
    private S_EnergyStorage energyStorage; // Reference to the energy storage system

    private float nextFireTime = 0f; // Time when the player can fire next

    public void Start()
    {
        energyStorage = GetComponent<S_EnergyStorage>();
    }

    private void Update()
    {
        HandleShootingInput();
    }

    private void HandleShootingInput()
    {
        if (Input.GetButton("Fire1")) // Continuously check for shooting input
        {
            if (Time.time >= nextFireTime) // Ensure cooldown has passed
            {
                if (HasEnoughEnergy())
                {
                    Shoot();
                    ConsumeEnergy();
                    nextFireTime = Time.time + (1f / fireRate); // Reset cooldown timer
                }
                else
                {
                    Debug.Log("Not enough energy to shoot!");
                }
            }
        }
    }

    private bool HasEnoughEnergy()
    {
        return energyStorage != null && energyStorage.currentEnergy >= energyConsumptionPerShot;
    }

    private void ConsumeEnergy()
    {
        if (energyStorage != null)
        {
            energyStorage.currentEnergy = Mathf.Max(0, energyStorage.currentEnergy - energyConsumptionPerShot);
        }
    }

    private void Shoot()
    {
        if (projectilePrefab != null && shootingPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, shootingPoint.position, shootingPoint.rotation);
            S_Projectile projectileScript = projectile.GetComponent<S_Projectile>();

            if (projectileScript != null)
            {
                projectileScript.Initialize(projectileSpeed, projectileLifetime, maxCollisions);
            }

            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.velocity = shootingPoint.forward * projectileSpeed;
            }
        }
        else
        {
            Debug.LogError("Projectile prefab or shooting point is not assigned!");
        }
    }
}
