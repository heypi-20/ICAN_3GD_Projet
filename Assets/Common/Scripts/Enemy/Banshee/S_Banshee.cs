using System.Collections;
using UnityEngine;

public class S_Banshee : EnemyBase
{
    [Header("Enemy Properties")]
    // The range of speeds Banshee can use while chasing
    public Vector2 chaseSpeedRange = new Vector2(5f, 15f);
    // The range of time intervals (in seconds) between speed switches
    public Vector2 speedSwitchIntervalRange = new Vector2(1f, 3f);

    public float rotationSpeed = 50f;
    public float avoidDist = 7f;
    public LayerMask obstacleLayer;

    [Header("Attack Properties")]
    public float range = 10f;
    public LayerMask playerLayer;
    
    [Header("Run Properties")]
    public float runTime = 2f;
    public float runSpeed = 50f;
    public float timeBfRun = 1f;
    
    private Transform player;
    private Rigidbody rb;

    private bool canAttack = true;
    private bool canRun;
    private bool isRunning;
    private float runTimer;
    
    private Vector3 runDirection;
    private bool hasRunDirection = false;

    // Current chase speed and timer until next speed change
    private float currentChaseSpeed;
    private float timeUntilNextSpeedChange;

    private void Start()
    {
        player = FindObjectOfType<S_CustomCharacterController>()?.transform;
        if (player == null) {
            Debug.LogWarning("No player found");
        }

        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // Initialize chase speed and switch interval
        PickNewChaseSpeed();
        PickNewSpeedSwitchInterval();
    }

    private void Update()
    {
        float dist = Vector3.Distance(player.position, transform.position);

        if (dist < range && canAttack && !isRunning) {
            Attack();
        } else if (dist > range && canAttack && !isRunning) {
            Chase();
        } else if (!canAttack && isRunning) {
            StartCoroutine(Waiter());
            
            if (canRun)
                Run();
        } 
    }

    private void Chase()
    {
        // Countdown to next speed change
        timeUntilNextSpeedChange -= Time.deltaTime;
        if (timeUntilNextSpeedChange <= 0f) {
            PickNewChaseSpeed();
            PickNewSpeedSwitchInterval();
        }

        Vector3 toPlayer = (player.position - transform.position).normalized;
        Vector3 avoidDirection = Vector3.zero;
        
        if (Physics.SphereCast(transform.position, transform.localScale.y/2f, transform.forward, out RaycastHit hit, avoidDist, obstacleLayer)) {
            Vector3 right = Vector3.Cross(Vector3.up, hit.normal).normalized;
            Vector3 repel = hit.normal * 0.5f;
            avoidDirection = (right + repel).normalized;
        }
        
        Vector3 chaseDirection = (toPlayer + avoidDirection * 1.5f).normalized;
        
        Quaternion targetRot = Quaternion.LookRotation(chaseDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        
        rb.velocity = chaseDirection * currentChaseSpeed;
    }

    private void Attack()
    {
        rb.velocity = Vector3.zero;
        
        Collider[] hits = Physics.OverlapSphere(transform.position, range, playerLayer);

        if (hits.Length == 0) {
            return;
        }
        foreach (Collider hit in hits) {
            if (hit.gameObject.CompareTag("Player")) {
                player.GetComponent<S_PlayerDamageReceiver>().ReceiveDamage(enemyDamage);
            }
        }
        canAttack = false;
        isRunning = true;
        runTimer = 0f;
    }

    private IEnumerator Waiter()
    {
        yield return new WaitForSeconds(timeBfRun);
        canRun = true;
    }

    private void Run()
    {
        runTimer += Time.deltaTime;

        if (!hasRunDirection) {
            runDirection = (transform.position - player.position).normalized;
            hasRunDirection = true;
        }

        if (Physics.SphereCast(transform.position, transform.localScale.y/2, runDirection, out RaycastHit hit, avoidDist, obstacleLayer)) {
            runDirection = Vector3.Reflect(runDirection, hit.normal).normalized;
        }
        
        Quaternion targetRot = Quaternion.LookRotation(runDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            
        rb.velocity = runDirection * runSpeed;
            
        if (runTimer >= runTime) {
            canAttack = true;
            isRunning = false;
            canRun = false;
            hasRunDirection = false;
            runTimer = 0;
        }
    }

    #region Chase Speed Switch Helpers

    /// <summary>
    /// Randomly pick a new chase speed from chaseSpeedRange
    /// </summary>
    private void PickNewChaseSpeed()
    {
        currentChaseSpeed = Random.Range(chaseSpeedRange.x, chaseSpeedRange.y);
    }

    /// <summary>
    /// Randomly choose the next speed switch interval from speedSwitchIntervalRange
    /// </summary>
    private void PickNewSpeedSwitchInterval()
    {
        timeUntilNextSpeedChange = Random.Range(speedSwitchIntervalRange.x, speedSwitchIntervalRange.y);
    }

    #endregion
}
