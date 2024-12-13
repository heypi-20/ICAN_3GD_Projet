using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Projectile : MonoBehaviour
{
    private float lifetime;
    private int maxCollisions;

    private int currentCollisions = 0;
    private float lifeTimer;

    public void Initialize(float projectileLifetime, int projectileMaxCollisions)
    {
        lifetime = projectileLifetime;
        maxCollisions = projectileMaxCollisions;

        lifeTimer = lifetime;
    }

    private void Update()
    {
        // Reduce lifetime timer
        lifeTimer -= Time.deltaTime;

        // Destroy the projectile if the lifetime has elapsed
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Increment collision count
        currentCollisions++;

        // Check if the projectile has reached its maximum collisions
        if (currentCollisions >= maxCollisions)
        {
            Destroy(gameObject);
        }

        // Handle collision effects here (e.g., apply damage, trigger effects)
        Debug.Log($"Projectile collided with {other.name}");
    }
}
