
using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening; 

[RequireComponent(typeof(Rigidbody))]
public class HeavyGunner : EnemyBase
{
    [Header("Shoot Properties")]
    public float fireRate;
    public float range;
    public Transform projectilePrefab;
    public Transform shootPoint;
    public float projectileSpeed = 10f;

    private Transform player;
    private RaycastHit hit;

    private float shootTimer;

    private void Start()
    {
        player = FindObjectOfType<S_CustomCharacterController>().transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;
        
        shootTimer += Time.deltaTime;
        
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < range) {
            transform.LookAt(player.position);
            
            if (shootTimer >= fireRate) {
                Shoot();
                shootTimer = 0;
            }
        }
    }

    private void Shoot()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, range)) {
            if (hit.transform == player) {
                Transform projectile = Instantiate(projectilePrefab, shootPoint.position, transform.rotation);
                projectile.GetComponent<S_ProjectileSpeed>().speed = projectileSpeed;
                SoundManager.Instance.Meth_Dashoot_Shoot();
            }
        }
    }
}
