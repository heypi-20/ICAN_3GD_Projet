﻿using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class S_Reaper_Behavior : EnemyBase
{
    [Header("Enemy Idle Properties")]
    public float idleSpeed = 5f;
    public float idleTime = 0.2f;
    public float rotationSpeed = 5f;
    public float minDistX = 5f;
    public float maxDistX = 5f;
    public float minDistZ = 5f;
    public float maxDistZ = 5f;

    [Header("Enemy Attack Properties")]
    public float attackingSpeed = 20f;
    public float targetingSpeed = 10f;
    public float attackRange = 5f;
    public float attackTime = 2f;
    public float attackCd = 3f;

    private Transform player;
    private RaycastHit hit;

    private float idleTimer;
    private float moveDistX;
    private float moveDistZ;

    private bool canAttack;
    private float attackTimer;
    private float rotateTimer;
    private float attackingTimer;
    private bool isAttacking;
    private Vector3 attackPos;

    private Rigidbody rb;

    private void Start()
    {
        moveDistX = Random.Range(-minDistX, maxDistX);
        moveDistZ = Random.Range(-minDistZ, maxDistZ);
        
        player = FindObjectOfType<S_CustomCharacterController>().transform;
        rb = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float dist = Vector3.Distance(transform.position, player.position);
        
        attackTimer += Time.deltaTime;

        if (attackTimer >= attackCd) {
            canAttack = true;
            attackTimer = 0f;
        }

        if (dist < attackRange && canAttack) {
            Attack();
        } else {
            Idle();
        }
    }

    private void Idle()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTime) {
            moveDistX = Random.Range(-minDistX, maxDistX);
            moveDistZ = Random.Range(-minDistZ, maxDistZ);
            idleTimer = 0f;
        }
        
        Vector3 moveDir = new Vector3(moveDistX, transform.position.y, moveDistZ);
        Vector3 lookDir = new Vector3(player.position.x - transform.position.x, transform.localScale.y/2, player.position.z - transform.position.z);

        transform.position = Vector3.MoveTowards(transform.position, moveDir, idleSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir), rotationSpeed * Time.deltaTime);
    }

    private void Attack()
    {
        if (!isAttacking) {
            rotateTimer += Time.deltaTime;

            Vector3 lookDir = new Vector3(player.position.x - transform.position.x, transform.localScale.y/2, player.position.z - transform.position.z);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(lookDir), targetingSpeed * Time.deltaTime);

            if (rotateTimer >= 2f) {
                attackPos = player.position - (transform.position - player.position);
                attackPos = new Vector3(attackPos.x, transform.position.y, attackPos.z);
                rotateTimer = 0f;
                isAttacking = true;
            }
        } else if (isAttacking) {
            attackingTimer += Time.deltaTime;

            if (attackingTimer >= attackTime) {
                transform.position = Vector3.MoveTowards(transform.position, attackPos, attackingSpeed * Time.deltaTime);

                if (IsGrounded()) {
                    attackPos.y = transform.position.y;
                }
            }
            if (attackingTimer >= attackTime * 2) {
                attackingTimer = 0f;
                isAttacking = false;
                canAttack = false;
            }
        }
    }
    
    void FixedUpdate()
    {
        // Check if the enemy is grounded
        if (IsGrounded())
        {
            rb.useGravity = false;                                      // Disable gravity when grounded
        } else
        {
            rb.useGravity = true; // Enable gravity when not grounded
        }
    }

    private bool IsGrounded()
    {
        // Use a raycast to detect the ground
        // Layer 8 = Ground
        return Physics.Raycast(transform.position, Vector3.down, 0.1f, 8);
    }
}
