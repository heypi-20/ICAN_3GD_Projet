using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_AutoDestruction : MonoBehaviour
{
    public float lifeTimer=5f;
    
    private void Update()
    {

        lifeTimer -= Time.deltaTime;
        // Destroy the projectile if the lifetime has elapsed
        if (lifeTimer <= 0)
        {
            Destroy(gameObject);
        }
        
    }
    
}
