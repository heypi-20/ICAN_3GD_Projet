using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun_Simple : MonoBehaviour
{
    [Header("Références")]
    public Transform weaponMuzzle; // Point de sortie du projectile
    public GameObject projectilePrefab; // Préfabriqué du projectile

    [Header("Propriétés du tir")]
    public float cooldown = 0.5f; // Temps entre chaque tir

    private float lastShootTime; // Temps du dernier tir

    void Update()
    {
        // Détection de l'entrée utilisateur
        if (Input.GetMouseButtonDown(0) && Time.time > lastShootTime + cooldown)
        {
            Shoot();
            lastShootTime = Time.time;
        }
    }

    void Shoot()
    {
        // Instancier le projectile à l'emplacement du muzzle avec son orientation
        GameObject projectile = Instantiate(projectilePrefab, weaponMuzzle.position, weaponMuzzle.rotation);

        Debug.Log("Projectile tiré !");
    }
}
