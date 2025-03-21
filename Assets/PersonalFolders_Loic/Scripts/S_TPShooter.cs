﻿using System;
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
    
    private bool canShoot = true;
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
        
        lr.startColor = new Color(1f, 0.36f, 0f);
        lr.endColor = new Color(1f, 0.36f, 0f);
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
            canShoot = true;
            teleportTimer = 0;
        }
        
        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < range) {
            transform.LookAt(player.position);
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
                if (!laserCharged) {
                    lr.SetPosition(1, hit.point);
                    lr.endWidth = Mathf.Min(lr.endWidth + 0.001f, 0.2f);
                    
                    lerpTimer += Time.deltaTime;
                    Color chargedColor = Color.red;
                    Color lerpColor = Color.Lerp(new Color(1f, 0.36f, 0f), chargedColor, Mathf.PingPong(lerpTimer, 1));
                    lr.startColor = lerpColor;
                    lr.endColor = lerpColor;
                }
                if (!canShoot)
                    return;
                if (!isCharging) {
                    isCharging = true;
                    StartCoroutine(Shoot(shootDelay));
                }
            }
        } else {
            lr.SetPosition(1, transform.position + transform.forward * range);
        }
    }

    IEnumerator Shoot(float delay)
    {
        yield return new WaitForSeconds(delay);
        laserCharged = true;
        yield return new WaitForSeconds(delay/2f);
        
        if (Physics.Raycast(transform.position, transform.forward, out hit, range)) {
            if (hit.collider && hit.collider.CompareTag("Player")) {
                hit.collider.GetComponent<S_EnergyStorage>().RemoveEnergy(enemyDamage);
            }
        }
        
        lr.startColor = new Color(1f, 0.36f, 0f);
        lr.endColor = new Color(1f, 0.36f, 0f);
        lr.endWidth = 0.05f;
        laserCharged = false;
        isCharging = false;
        lerpTimer = 0;
    }
}

