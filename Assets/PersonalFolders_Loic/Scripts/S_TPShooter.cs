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
    public float minDist = 5f;
    public float maxDist = 5f;

    [Header("Shoot Properties")]
    public float range;
    public Transform shootPoint;
    public float chargeTime;
    public float shootDelay;
    
    [Header("Enemy Components")]
    private NavMeshAgent agent;
    private S_CustomCharacterController findPlayer;
    private Transform player;
    private LineRenderer lr;
    private RaycastHit hit;

    private float teleportTimer;
    private float shootTimer;
    private float lerpTimer;

    private bool canShoot;
    private bool isCharging;
    private bool laserCharged = false;

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
    }
    
    void Update()
    {
        player = findPlayer.transform;
        
        lr.SetPosition(0, shootPoint.position);
        
        teleportTimer += Time.deltaTime;

        if (teleportTimer >= teleportCd) {
            Teleport();
            teleportTimer = 0;
        }
        
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < range) {
            if (!Physics.Raycast(transform.position, player.position - transform.position, out hit, range))
                return;
            if (!isCharging && canShoot)
                transform.LookAt(hit.point);
            if (canShoot)
                LaserAim();
        } else {
            lr.SetPosition(1, shootPoint.position);
        }
    }

    private void Teleport()
    {
        float randX = Random.Range(-minDist, maxDist);
        float randY = Random.Range(-minDist, maxDist);
        float randZ = Random.Range(-minDist, maxDist);
        
        Vector3 targetPosition = new Vector3(transform.position.x + randX, transform.position.y + randY, transform.position.z + randZ);
        NavMeshHit navMeshHit;
        if (NavMesh.SamplePosition(targetPosition, out navMeshHit, range, NavMesh.AllAreas)) {
            agent.enabled = false;
            transform.DOMove(navMeshHit.position, 0.1f)
                     .SetEase(Ease.InOutQuad)
                     .OnComplete(() =>
                     {
                         canShoot = true;
                         agent.enabled = true;
                         // agent.SetDestination(navMeshHit.position);
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
        if (!laserCharged) {
            if (hit.collider.CompareTag("Player")) {
                if (!isCharging) {
                    isCharging = true;
                    StartCoroutine(Shoot());
                    if (lerpTimer > 0f) {
                        lerpTimer = 0f;
                    }
                } else if (!laserCharged && isCharging) {
                    lr.SetPosition(1, hit.point);
                    lr.endWidth = Mathf.Min(lr.endWidth + 0.001f, 0.2f);
                    transform.LookAt(lr.GetPosition(1));    

                    lerpTimer += Time.deltaTime;
                    Color chargedColor = Color.red;
                    Color lerpColor = Color.Lerp(new Color(0.04f, 0.45f, 1f), chargedColor, Mathf.Clamp(lerpTimer, 0f , 1f));
                    lr.startColor = lerpColor;
                    lr.endColor = lerpColor;
                }
            } else {
                lr.SetPosition(1, shootPoint.position + transform.forward * range);
                if (lerpTimer > 0f) {
                    lerpTimer = 0f;
                }
            } 
        }
    }

    IEnumerator Shoot()
    {
        yield return new WaitForSeconds(chargeTime);
        laserCharged = true;
        
        yield return new WaitForSeconds(shootDelay);
        if (Physics.Raycast(transform.position, lr.transform.forward, out hit, range) && laserCharged) {
            if (hit.collider.CompareTag("Player")) {
                hit.collider.GetComponent<S_PlayerHitTrigger>().ReceiveDamage(enemyDamage);
            }
        }
        lr.startColor = new Color(0.04f, 0.45f, 1f);
        lr.endColor = new Color(0.04f, 0.45f, 1f);
        lr.endWidth = 0.05f;
        lr.SetPosition(1, shootPoint.position);
        
        laserCharged = false;
        isCharging = false;
        lerpTimer = 0;
        canShoot = false;
    }
}

