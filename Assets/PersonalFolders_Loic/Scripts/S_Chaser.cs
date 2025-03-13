using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Rigidbody))]
public class S_Chaser : EnemyBase
{
    [Header("Enemy Properties")]
    public float maxChaseDistance = 5f;
    
    private NavMeshAgent agent;

    private Transform target;
    private S_CustomCharacterController findPlayer;
    
    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        findPlayer = FindObjectOfType<S_CustomCharacterController>();
        if (findPlayer == null) {
            Debug.LogWarning("No Character Controller");
        }
    }

    private void Update()
    {
        target = findPlayer.transform;

        if (Vector3.Distance(target.position, transform.position) < maxChaseDistance) {
            agent.SetDestination(target.position);
        }
    }
}
