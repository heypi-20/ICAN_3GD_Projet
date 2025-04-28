using System;
using UnityEngine;

[ExecuteInEditMode]
public class CheckVar : MonoBehaviour
{
    private void OnValidate()
    {
        S_Banshee script = gameObject.GetComponent<S_Banshee>();
        
        if (script.avoidDist > script.range)
        {
            Debug.LogWarning($"[Example] valueA ({script.avoidDist}) is greater than valueB ({script.range})", this);
        }
    }
}

public class S_Banshee : EnemyBase
{
    [Header("Enemy Properties")]
    public float speed = 10f;
    public float rotationSpeed = 50f;
    public float avoidDist = 7f;
    public LayerMask obstacleLayer;

    [Header("Attack Properties")]
    public float range = 10f;
    public float runTime = 2f;
    public LayerMask playerLayer;
    
    private Transform player;

    private Rigidbody rb;

    private bool canAttack = true;
    private bool isRunning;
    private float runTimer;

    
    
    private void Start()
    {
        player = FindObjectOfType<S_CustomCharacterController>().transform;
        if (player == null) {
            Debug.LogWarning("No player found");
        }
        
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    private void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist < range && canAttack && !isRunning) {
            Attack();
        } else if (dist > range && canAttack && !isRunning) {
            Chase();
        } else if (!canAttack && isRunning) {
            Run();
        } 
    }

    private void Chase()
    {
        Vector3 toPlayer = (player.position - transform.position).normalized;

        Quaternion targetRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        
        if (Physics.SphereCast(transform.position, transform.localScale.y/2f, transform.forward, out RaycastHit hit, avoidDist, obstacleLayer)) {
            Vector3 avoidDir = Vector3.ProjectOnPlane(toPlayer, hit.normal).normalized;
            rb.velocity = avoidDir * speed;
            Debug.Log("Avoiding");
        } else {
            rb.velocity = toPlayer * speed;
            Debug.Log("Chasing");
        }

        if (transform.position.y < transform.localScale.y / 2) {
            transform.position = new Vector3(transform.position.x, transform.localScale.y / 2, transform.position.z);
        }
    }

    private void Attack()
    {
        rb.velocity = Vector3.zero;
        Debug.Log("Attacking");
        
        Collider[] hits = Physics.OverlapSphere(transform.position, range, playerLayer);

        if (hits.Length == 0) {
            return;
        }
        foreach (Collider hit in hits) {
            if (hit.gameObject.CompareTag("Player")) {
                player.GetComponent<S_PlayerHitTrigger>().ReceiveDamage(enemyDamage);
            }
        }
        canAttack = false;
        isRunning = true;
        runTimer = 0f;
    }

    private void Run()
    {
        Debug.Log("Running");
        runTimer += Time.deltaTime;
            
        Vector3 awayFromPlayer = (transform.position - player.position).normalized;

        Quaternion targetRot = Quaternion.LookRotation(awayFromPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            
        rb.velocity = awayFromPlayer * speed;
            
        if (runTimer >= runTime) {
            Debug.Log("finished run");
            canAttack = true;
            runTimer = 0;
            isRunning = false;
        }
    }
}
