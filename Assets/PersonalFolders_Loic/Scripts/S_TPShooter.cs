using System;
using UnityEngine;
using DG.Tweening;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(LineRenderer))]
public class S_TPShooter : EnemyBase
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
    
    [Header("Enemy Components")]
    private NavMeshAgent agent;
    private Transform player;
    private LineRenderer lr;
    private RaycastHit hit;

    private float teleportTimer;
    private float shootTimer;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        player = FindObjectOfType<S_CustomCharacterController>().transform;
        lr = GetComponent<LineRenderer>();
        lr.startColor = Color.red;
        lr.endColor = Color.red;
        lr.SetPosition(1, shootPoint.position);
    }
    
    void Update()
    {
        if (player == null)
            return;
        
        lr.SetPosition(0, shootPoint.position);
        
        teleportTimer += Time.deltaTime;

        if (teleportTimer >= teleportCd) {
            Teleport();
            teleportTimer = 0;
        }
        
        shootTimer += Time.deltaTime;
        
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < range) {
            
            if (shootTimer >= fireRate) {
                Shoot();
                shootTimer = 0;
            } else {
                transform.LookAt(player.position);
                LaserAim();
            }
        }
    }

    private void Teleport()
    {
        float randX = Random.Range(-minDist, maxDist);
        float randY = Random.Range(-minDist, maxDist);
        float randZ = Random.Range(-minDist, maxDist);
        
        Vector3 targetPosition = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(targetPosition, out navMeshHit, range, NavMesh.AllAreas)) {
            agent.enabled = false;
            transform.DOMove(navMeshHit.position, 0.1f)
                     .SetEase(Ease.InOutQuad)
                     .OnComplete(() =>
                     {
                         agent.enabled = true;
                         agent.SetDestination(navMeshHit.position);
                     });
        
            Vector3 targetScale = new Vector3(1.1f, 1.5f, 1.1f); // Augmenter légèrement la taille
            // Créer l'animation
            transform.DOScale(targetScale, 0.5f) // Durée pour atteindre la taille cible (0.25s aller)
                     .SetEase(Ease.InOutQuad)    // Easing fluide pour un effet agréable
                     .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    private void LaserAim()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, range)) {
            if (hit.collider) {
                lr.SetPosition(1, hit.point);
            }
        }
    }

    private void Shoot()
    {
        if (Physics.Raycast(transform.position, transform.forward, out hit, range)) {
            if (hit.transform == player) {
                Transform projectile = Instantiate(projectilePrefab, shootPoint.position, transform.rotation);
                projectile.GetComponent<S_ProjectileSpeed>().speed = projectileSpeed;
            }
        }
    }
}

