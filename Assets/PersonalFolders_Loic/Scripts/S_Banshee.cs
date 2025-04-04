using System;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class S_Banshee : EnemyBase
{
    [Header("Enemy Properties")]
    public float idleTime = 2f;
    public float minDist = 5f;
    public float maxDist = 5f;
    public float elevationRange = 3f;
    public float chaseRange = 10f;
    public float stoppingRange = 0.5f;
    
    private NavMeshAgent agent;
    private Transform findBody;
    private GameObject body;
    private S_CustomCharacterController findPlayer;
    private Transform player;
        
    private float idleTimer;
    private bool getNewPos = true;
    private float randX;
    private float randY;
    private float randZ;
    private float desiredY;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        findBody = transform.GetChild(0);

        if (findBody.name != "Body") {
            Debug.LogWarning("The body object should be named 'Body' and the 1st child in the hierarchy.");
            return;
        }
        
        findPlayer = FindObjectOfType<S_CustomCharacterController>();
        if (findPlayer == null) {
            Debug.LogWarning("No Character Controller");
        }

        findBody.position = new Vector3(transform.position.x, 10f, transform.position.z);
    }

    private void Update()
    {
        player = findPlayer.transform;

        float dist = Vector3.Distance(findBody.position, player.position);

        if (dist < chaseRange) {
            idleTimer = 0f;
            Chase();
        } else {
            Idle();
        }
    }

    private void Idle()
    {
        if (!getNewPos)
            idleTimer += Time.deltaTime;
        else if (getNewPos) {
            randX = Random.Range(-minDist, maxDist);
            randY = Random.Range(-elevationRange, elevationRange);
            randZ = Random.Range(-minDist, maxDist);
            
            Vector3 targetPosition = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
            agent.SetDestination(targetPosition);
            
            desiredY = findBody.localPosition.y + randY;
            getNewPos = false;
        }
        
        findBody.localPosition = new Vector3(
            0f,
            Mathf.Lerp(findBody.localPosition.y, desiredY, Time.deltaTime * (agent.speed/2f)),
            0f
        );
    
        if (idleTimer >= idleTime) {
            getNewPos = true;
            idleTimer = 0f;
        }
    }

    private void Chase()
    {
        if (Vector3.Distance(player.position, findBody.position) < stoppingRange) {
            Attack();
        }
        
        Vector3 playerXZ = new Vector3(player.position.x, transform.position.y, player.position.z);
        agent.SetDestination(playerXZ);

        float desiredLocalY = player.position.y - findBody.localPosition.y;
        findBody.localPosition = new Vector3(
            0f,
            Mathf.Lerp(findBody.localPosition.y, desiredLocalY, Time.deltaTime * agent.speed),
            0f
        );
    }

    private void Attack()
    {
        
    }
}
