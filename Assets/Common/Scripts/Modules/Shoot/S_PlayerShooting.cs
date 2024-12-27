using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_PlayerShooting : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab; // The prefab for the projectile
    public Transform shootingPoint; // The point from where the projectile is fired
    public float baseFireRate = 2f; // Base projectiles per second
    public float maxFireRate = 5f; // Maximum fire rate with bonuses
    public float projectileSpeed = 20f; // Speed of the projectile
    public float projectileLifetime = 5f; // Lifetime of the projectile in seconds
    public float baseEnergyConsumptionPerShot = 5f; // Base energy consumed per shot
    public float minEnergyConsumption = 1f; // Minimum energy consumption per shot
    public float maxEnergyConsumption = 10f; // Maximum energy consumption per shot

    [Header("Energy Influence Settings")]
    public float fireRatePercentage = 0.1f; // Percentage of current energy to affect fire rate
    public float fireRateMultiplier = 2f; // Multiplier for fire rate based on current energy
    [Space (20)]
    public float energyConsumptionPercentage = 0.1f; // Percentage of current energy to affect energy consumption
    public float energyConsumptionMultiplier = 5f; // Multiplier for energy consumption reduction based on current energy

    [Header("References")]
    private S_EnergyStorage energyStorage; // Reference to the energy storage system

    private float nextFireTime = 0f; // Time when the player can fire next

    private void Start()
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
                    nextFireTime = Time.time + (1f / GetCurrentFireRate()); // Reset cooldown timer
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
        return energyStorage != null && energyStorage.currentEnergy >= GetCurrentEnergyConsumption();
    }

    private void ConsumeEnergy()
    {
        if (energyStorage != null)
        {
            energyStorage.currentEnergy = Mathf.Max(0, energyStorage.currentEnergy - GetCurrentEnergyConsumption());
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
                projectileScript.InitializeProjectile(projectileLifetime);
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

    private float GetCurrentFireRate()
    {
        if (energyStorage != null)
        {
            float energyFactor = energyStorage.currentEnergy * fireRatePercentage;
            float fireRateBonus = energyFactor * fireRateMultiplier;
            return Mathf.Clamp(baseFireRate + fireRateBonus, baseFireRate, maxFireRate);
        }

        return baseFireRate;
    }

    private float GetCurrentEnergyConsumption()
    {
        if (energyStorage != null)
        {
            float energyFactor = energyStorage.currentEnergy * energyConsumptionPercentage;
            float consumptionReduction = energyFactor * energyConsumptionMultiplier;
            float energyConsumption = baseEnergyConsumptionPerShot - consumptionReduction;
            return Mathf.Clamp(energyConsumption, minEnergyConsumption, maxEnergyConsumption);
        }

        return baseEnergyConsumptionPerShot;
    }
}
