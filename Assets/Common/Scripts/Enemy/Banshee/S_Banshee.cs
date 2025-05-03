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
        Quaternion targetRot = Quaternion.LookRotation(toPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        
        if (Physics.SphereCast(transform.position, transform.localScale.y/2f, transform.forward, out RaycastHit hit, avoidDist, obstacleLayer)) {
            Vector3 avoidDir = Vector3.ProjectOnPlane(toPlayer, hit.normal).normalized;
            rb.velocity = avoidDir * currentChaseSpeed;
            Debug.Log($"Avoiding at speed {currentChaseSpeed}");
        } else {
            rb.velocity = toPlayer * currentChaseSpeed;
            Debug.Log($"Chasing at speed {currentChaseSpeed}");
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

    private IEnumerator Waiter()
    {
        yield return new WaitForSeconds(timeBfRun);
        canRun = true;
    }

    private void Run()
    {
        Debug.Log("Running");
        runTimer += Time.deltaTime;
            
        Vector3 awayFromPlayer = (transform.position - player.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(awayFromPlayer);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
            
        rb.velocity = awayFromPlayer * runSpeed;
            
        if (runTimer >= runTime) {
            Debug.Log("Finished running");
            canAttack = true;
            isRunning = false;
            canRun = false;
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
        Debug.Log($"New chase speed: {currentChaseSpeed}");
    }

    /// <summary>
    /// Randomly choose the next speed switch interval from speedSwitchIntervalRange
    /// </summary>
    private void PickNewSpeedSwitchInterval()
    {
        timeUntilNextSpeedChange = Random.Range(speedSwitchIntervalRange.x, speedSwitchIntervalRange.y);
        Debug.Log($"Next speed change in: {timeUntilNextSpeedChange} seconds");
    }

    #endregion
}
