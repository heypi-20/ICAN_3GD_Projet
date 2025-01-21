using System;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Rigidbody))]
public class S_Reaper : S_Enemy
{
    [Header("Enemy Properties")]
    public float idleSpeed = 5f;
    public float idleTime = 0.2f;
    public float minDistX = 5f;
    public float maxDistX = 5f;
    public float minDistZ = 5f;
    public float maxDistZ = 5f;
    public float rotationSpeed = 5f;
    
    private float idleTimer = 0f;
    private float moveDistX;
    private float moveDistZ;

    private void Start()
    {
        moveDistX = Random.Range(-minDistX, maxDistX);
        moveDistZ = Random.Range(-minDistZ, maxDistZ);
    }

    private void Update()
    {
        Idle();
    }

    private void Idle()
    {
        idleTimer += Time.deltaTime;

        if (idleTimer >= idleTime) {
            moveDistX = Random.Range(-minDistX, maxDistX);
            moveDistZ = Random.Range(-minDistZ, maxDistZ);
            idleTimer = 0f;
        }
        
        Vector3 moveDir = new Vector3(moveDistX, 0f, moveDistZ);
        
        transform.position = Vector3.MoveTowards(transform.position, moveDir, idleSpeed * Time.deltaTime);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(moveDir), rotationSpeed * Time.deltaTime);
    }

    private void Attack()
    {
        
    }
}

