using System;
using UnityEngine;

public class S_ProjectileSpeed : MonoBehaviour
{
    [HideInInspector]
    public float speed;

    public float deathTime = 3f;
    
    private float deathTimer;
    
    // Update is called once per frame
    void Update()
    {
        transform.Translate(transform.forward * Time.deltaTime * speed, Space.World);
        
        deathTimer += Time.deltaTime;

        if (deathTimer >= deathTime) {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Destroy(gameObject);
    }
}
