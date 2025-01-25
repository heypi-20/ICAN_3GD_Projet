
using System;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening; 

[RequireComponent(typeof(Rigidbody))]
public class TpShooter : EnemyBase
{
    [Header("Enemy Properties")]
    public float teleportCd = 2f;
    public float minDist = 5f;
    public float maxDist = 5f;

    [Header("Shoot Properties")]
    public float fireRate;
    public float range;
    public Transform projectilePrefab;
    public Transform shootPoint;
    public float projectileSpeed = 10f;

    private Transform player;
    private RaycastHit hit;

    private float teleportTimer;
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
        
        teleportTimer += Time.deltaTime;

        if (teleportTimer >= teleportCd) {
            Teleport();
            teleportTimer = 0;
        }
        
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

    private void Teleport()
    {
        float randX = Random.Range(-minDist, maxDist);
        float randZ = Random.Range(-minDist, maxDist);
        
        Vector3 targetPosition = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
        transform.DOMove(targetPosition, 0.1f).SetEase(Ease.InOutQuad);
        
        Vector3 targetScale = new Vector3(1.1f, 1.5f, 1.1f); // Augmenter légèrement la taille
        // Créer l'animation
        transform.DOScale(targetScale, 0.5f) // Durée pour atteindre la taille cible (0.25s aller)
            .SetEase(Ease.InOutQuad)    // Easing fluide pour un effet agréable
            .SetLoops(-1, LoopType.Yoyo);
        SoundManager.Instance.Meth_Dashoot_Dash();
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
