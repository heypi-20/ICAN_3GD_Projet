using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Projectile : MonoBehaviour
{
    private float lifetime;
    private float lifeTimer;

    public void InitializeProjectile(float projectileLifetime)
    {
        lifetime = projectileLifetime;

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

}
