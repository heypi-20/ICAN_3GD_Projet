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
    public float chaseRange = 10f;
    public float stoppingRange = 0.5f;
    
    private NavMeshAgent agent;
    private Transform findBody;
    private GameObject body;
    private S_CustomCharacterController findPlayer;
    private Transform player;
        
    private float idleTimer;
    private bool getNewPos;
    private float randX;
    private float randY;
    private float randZ;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        findBody = transform.GetChild(0);

        if (findBody.name != "Body") {
            return;
        }
        
        findPlayer = FindObjectOfType<S_CustomCharacterController>();
        if (findPlayer == null) {
            Debug.LogWarning("No Character Controller");
        }
    }

    private void Update()
    {
        player = findPlayer.transform;
        
        Vector3 effectiveEnemyPos = transform.position + findBody.localPosition;

        Vector3 effectiveXZ = new Vector3(effectiveEnemyPos.x, 0, effectiveEnemyPos.z);
        Vector3 playerXZ = new Vector3(player.position.x, 0, player.position.z);

        float dist = Vector3.Distance(effectiveXZ, playerXZ);

        if (dist < chaseRange) {
            getNewPos = true;
            idleTimer = 0f;
            Chase();
        } else {
            getNewPos = true;
            Idle();
        }
    }

    private void Idle()
    {
        idleTimer += Time.deltaTime;
    
        if (getNewPos)
        {
            randX = Random.Range(-minDist, maxDist);
            randY = Random.Range(-minDist, maxDist);
            randZ = Random.Range(-minDist, maxDist);
        
            Vector3 targetPosition = new Vector3(transform.position.x + randX, transform.position.y, transform.position.z + randZ);
            agent.SetDestination(targetPosition);
            getNewPos = false;
        }

        float desiredLocalY = randY;
        findBody.localPosition = new Vector3(
            findBody.localPosition.x,
            Mathf.Lerp(findBody.localPosition.y, desiredLocalY, Time.deltaTime * agent.speed),
            findBody.localPosition.z
        );
    
        if (idleTimer >= idleTime)
        {
            getNewPos = true;
            idleTimer = 0f;
        }
    }

    private void Chase()
    {
        Vector3 playerXZ = new Vector3(player.position.x, transform.position.y, player.position.z);
        agent.SetDestination(playerXZ);

        float desiredLocalY = player.position.y - transform.position.y;
        findBody.localPosition = new Vector3(
            findBody.localPosition.x,
            Mathf.Lerp(findBody.localPosition.y, desiredLocalY, Time.deltaTime * agent.speed),
            findBody.localPosition.z
        );
    }


    private void Attack()
    {
        
    }
}
