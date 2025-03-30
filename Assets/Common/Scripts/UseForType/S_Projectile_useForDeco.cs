using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_Projectile_useForDeco : MonoBehaviour
{
    private float lifetime;
    private float lifeTimer;
    private float bulletSpeed;

    public void InitializeProjectile(float projectileLifetime, float speed)
    {
        lifetime = projectileLifetime;
        bulletSpeed = speed;
        lifeTimer = lifetime;
    }

    private void Update()
    {

        lifeTimer -= Time.deltaTime;
        transform.Translate(Vector3.forward * (bulletSpeed * Time.deltaTime), Space.Self);
        // Destroy the projectile if the lifetime has elapsed
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
        }
        
    }

}
