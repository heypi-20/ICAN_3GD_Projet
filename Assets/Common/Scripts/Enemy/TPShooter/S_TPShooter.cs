using System;
using System.Collections;
using System.Collections.Generic;
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
    public float negativeDist = 5f;
    public float positiveDist = 5f;
    public LayerMask validPosLayer;
    public LayerMask groundLayer;

    [Header("Shoot Properties")]
    public float range;
    public Transform shootPoint;
    public float laserRadius;
    public float chargeTime;
    public float shootDelay;
    public LayerMask playerLayer;
    
    [Header("Enemy Components")]
    private NavMeshAgent agent;
    private S_CustomCharacterController findPlayer;
    private Transform player;
    private LineRenderer lr;
    private RaycastHit hit;
    private RaycastHit laserHit;

    private float teleportTimer;
    private float shootTimer;
    private float lerpTimer;

    private bool playerInRange;
    private bool canShoot;
    private bool isCharging;
    private bool laserCharged;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        findPlayer = FindObjectOfType<S_CustomCharacterController>();
        if (findPlayer == null) {
            Debug.LogWarning("No Character Controller");
        }
        
        lr = GetComponent<LineRenderer>();
        
        lr.startColor = new Color(0.04f, 0.45f, 1f);
        lr.endColor = new Color(0.04f, 0.45f, 1f);
        lr.startWidth = 0.05f;
        lr.endWidth = 0.05f;
        lr.SetPosition(1, shootPoint.position);
        
        Vector3 groundHit = GroundCheck();
        if (groundHit != Vector3.zero) {
            transform.position = new Vector3(groundHit.x, groundHit.y + transform.localScale.y, groundHit.z);
        }
        
        agent.enabled = true;
    }
    
    private void Update()
    {
        player = findPlayer.transform;
        
        lr.SetPosition(0, shootPoint.position);
        
        teleportTimer += Time.deltaTime;
        
        if (teleportTimer >= teleportCd) {
            if (playerInRange)
                Teleport();
            else {
                Vector3 playerPos = GetRandomPointInSphere(player.position, range);
                TeleportTowardsPlayer(playerPos);
            }
            teleportTimer = 0;
        }
        
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < range) {
            if (!isCharging && canShoot)
                transform.LookAt(player.position);
            if (canShoot && Physics.Raycast(shootPoint.position, transform.forward, out hit, range))
                LaserHandler();
            playerInRange = true;
        } else {
            lr.SetPosition(1, shootPoint.position);
            playerInRange = false;
        }
    }

    private Vector3 GroundCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit groundHit, Mathf.Infinity)) {
            return groundHit.point;
        }
        return Vector3.zero;
    }
    
    private Vector3 GetRandomPointInSphere(Vector3 center, float radius)
    {
        return center + Random.insideUnitSphere * radius;
    }


    private void TeleportTowardsPlayer(Vector3 playerPos)
    {
        if (NavMesh.SamplePosition(playerPos, out NavMeshHit navMeshHit, range, validPosLayer)) {
            agent.enabled = false;
            transform.DOMove(navMeshHit.position, 0.1f)
                     .SetEase(Ease.InOutQuad)
                     .OnComplete(() =>
                     {
                         canShoot = true;
                         agent.enabled = true;
                     });
        
            Vector3 targetScale = new Vector3(1.1f, 1.5f, 1.1f); // Augmenter légèrement la taille
            // Créer l'animation
            transform.DOScale(targetScale, 0.5f) // Durée pour atteindre la taille cible (0.25s aller)
                     .SetEase(Ease.InOutQuad)    // Easing fluide pour un effet agréable
                     .SetLoops(-1, LoopType.Yoyo);
        }
    }

    private void Teleport()
    {
        float randX = Random.Range(-negativeDist, positiveDist);
        float randY = Random.Range(-negativeDist, positiveDist);
        float randZ = Random.Range(-negativeDist, positiveDist);
        
        Vector3 targetPosition = new Vector3(transform.position.x + randX, transform.position.y + randY, transform.position.z + randZ);
        DoMovement(targetPosition);
    }

    private void DoMovement(Vector3 targetPosition)
    {
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit navMeshHit, range, validPosLayer)) {
            agent.enabled = false;
            transform.DOMove(navMeshHit.position, 0.1f)
                     .SetEase(Ease.InOutQuad)
                     .OnComplete(() =>
                     {
                         canShoot = true;
                         agent.enabled = true;
                     });
        
            Vector3 targetScale = new Vector3(1.1f, 1.5f, 1.1f); // Augmenter légèrement la taille
            // Créer l'animation
            transform.DOScale(targetScale, 0.5f) // Durée pour atteindre la taille cible (0.25s aller)
                     .SetEase(Ease.InOutQuad)    // Easing fluide pour un effet agréable
                     .SetLoops(-1, LoopType.Yoyo);
        }
    }
    
    private void LaserHandler()
    {
        if (!laserCharged) {
            transform.LookAt(hit.point);    
            if (hit.collider.CompareTag("Player")) {
                if (!isCharging) {
                    isCharging = true;
                    StartCoroutine(Shoot());
                    if (lerpTimer > 0f) {
                        lerpTimer = 0f;
                    }
                } else if (isCharging) {
                    LaserCharge();
                }
            } else if (!hit.collider.CompareTag("Player")) {
                lr.SetPosition(1, hit.point);
                if (lerpTimer > 0f) {
                    lerpTimer = 0f;
                }
            } else {
                lr.SetPosition(1, shootPoint.position);
                if (lerpTimer > 0f) {
                    lerpTimer = 0f;
                }
            }
        }
    }

    private void LaserCharge()
    {
        lr.SetPosition(1, hit.point);
        lr.endWidth = Mathf.Min(lr.endWidth + 0.001f, 0.2f);

        lerpTimer += Time.deltaTime;
        Color chargedColor = Color.red;
        Color lerpColor = Color.Lerp(new Color(0.04f, 0.45f, 1f), chargedColor, Mathf.Clamp(lerpTimer, 0f , 1f));
        lr.startColor = lerpColor;
        lr.endColor = lerpColor;
    }

    IEnumerator Shoot()
    {
        yield return new WaitForSeconds(chargeTime);
        laserCharged = true;
        Vector3 finalLaserDirection = (hit.point - transform.position).normalized;

        yield return new WaitForSeconds(shootDelay);

        if (Physics.SphereCast(shootPoint.position, laserRadius, finalLaserDirection, out laserHit, range, playerLayer) && laserCharged) {
            if (laserHit.collider.CompareTag("Player")) {
                laserHit.collider.GetComponent<S_PlayerDamageReceiver>().ReceiveDamage(enemyDamage);
            }
        }
        
        ResetLaser();
    }

    private void ResetLaser()
    {
        lr.startColor = new Color(0.04f, 0.45f, 1f);
        lr.endColor = new Color(0.04f, 0.45f, 1f);
        lr.endWidth = 0.05f;
        lr.SetPosition(1, shootPoint.position);
        
        laserCharged = false;
        isCharging = false;
        lerpTimer = 0;
        canShoot = false;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, range);

        if (Physics.SphereCast(transform.position, laserRadius, transform.forward, out laserHit, range)) {
            Gizmos.color = Color.green;
            Vector3 sphereCastMidpoint = transform.position + (transform.forward * hit.distance);
            Gizmos.DrawWireSphere(sphereCastMidpoint, laserRadius);
            Gizmos.DrawSphere(hit.point, 0.1f);
            Debug.DrawLine(transform.position, sphereCastMidpoint, Color.green);
        } else {
            Gizmos.color = Color.red;
            Vector3 sphereCastMidpoint = transform.position + (transform.forward * (range-laserRadius));
            Gizmos.DrawWireSphere(sphereCastMidpoint, laserRadius);
            Debug.DrawLine(transform.position, sphereCastMidpoint, Color.red);
        }
    }
}
