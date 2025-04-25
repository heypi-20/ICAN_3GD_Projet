using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class S_Dashooter : EnemyBase
{
    [Header("Dash Settings")]
    [Tooltip("Maximum distance for dash target search")] // Search radius for dash
    public float dashRadius = 5f;

    [Tooltip("Distance below which enemy is considered too close to player")] // Close threshold
    public float closeDistance = 2f;

    [Tooltip("Distance above which enemy is considered too far from player")] // Far threshold
    public float farDistance = 8f;

    [Tooltip("Random interval range (seconds) between dash actions")] // Dash interval range
    public Vector2 dashInterval = new Vector2(1f, 3f);

    [Tooltip("Speed at which enemy moves during dash (units/sec)")] // Movement speed during dash
    public float dashSpeed = 15f;

    [Tooltip("Time (seconds) enemy spends looking at dash target before dashing")] // Preparation duration
    public float prepareDuration = 1f;

    [Tooltip("Layer mask for ground detection")] // Ground layer mask
    public LayerMask groundLayerMask;

    [Tooltip("Maximum number of random samples for target search")] // Sampling attempts
    public int maxSpawnAttempts = 10;

    [Tooltip("Maximum height difference considered for vertical dash (units)")] // Max vertical difference
    public float maxVerticalDashHeight = 3f;

    [Tooltip("Number of attempts to find a high point for dash")] // Attempts for high point search
    public int highPointSearchAttempts = 5;

    [Tooltip("Minimum horizontal distance when sampling high point (units)")] // Min horizontal offset
    public float minHighPointHorizontal = 1f;

    [Tooltip("Rotation speed when facing target or player (rad/sec)")] // Rotation speed
    public float rotationSpeed = 5f;

    [Tooltip("Vertical offset applied when landing on high point (units)")] // Y offset on high point
    public float highPointYOffset = 0.5f;

    [Header("Fall Settings")]
    [Tooltip("Acceleration applied when in free fall (units/sec^2)")] // Gravity acceleration
    public float fallAcceleration = 30f;

    // Private fields
    private Transform player;               // Reference to player transform
    private float dashTimer;                // Countdown timer for next dash
    private Vector3 nextDashTarget;         // Next target position for dash
    private bool isPreparing;               // Flag indicating preparation phase
    private float prepareTimer;             // Countdown timer for preparation
    private bool isDashing;                 // Flag indicating dash in progress
    private Rigidbody rb;                   // Cached Rigidbody component
    private Collider col;                   // Cached Collider component
    private S_EnemyGroundCheck groundCheck; // Reference to ground check helper

    // Initialization
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        groundCheck = GetComponentInChildren<S_EnemyGroundCheck>();
        if (groundCheck != null)
        {
            groundCheck.physicCastCooldown = 0f; // Ensure immediate ground detection
        }
        rb.useGravity = true;
        rb.isKinematic = false;
        nextDashTarget = transform.position; // Initialize target to current position
    }

    // Cache player reference and schedule first dash
    private void Start()
    {
        player = FindObjectOfType<S_CustomCharacterController>()?.transform;
        ScheduleNextDash();
    }

    // Main update loop handles fall, preparation, and dash timing
    private void Update()
    {
        if (player == null || isDashing)
            return;

        // If not grounded, apply free fall acceleration
        if (groundCheck != null && !groundCheck.TriggerDetection())
        {
            rb.AddForce(Vector3.down * fallAcceleration, ForceMode.Acceleration);
            return;
        }

        // During preparation, rotate towards dash target
        if (isPreparing)
        {
            PrepareLook();
            return;
        }

        // Rotate to face player when idle
        FacePlayer();

        // Countdown to next dash
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f)
        {
            isPreparing = true;
            prepareTimer = prepareDuration;
        }
    }

    // Preparation phase: look at target and countdown
    private void PrepareLook()
    {
        Vector3 dir = nextDashTarget - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }

        prepareTimer -= Time.deltaTime;
        if (prepareTimer <= 0f)
        {
            isPreparing = false;
            StartCoroutine(DashTo(nextDashTarget)); // Begin dash
            ScheduleNextDash(); // Reset timer for next dash
        }
    }

    // Schedule next dash by randomizing timer and computing target
    private void ScheduleNextDash()
    {
        dashTimer = Random.Range(dashInterval.x, dashInterval.y);
        TryFindDashPoint(out nextDashTarget);
    }

    // Rotate to face the player horizontally
    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * rotationSpeed);
        }
    }

    // Determine a valid dash target: prefer high points in mid-range
    private bool TryFindDashPoint(out Vector3 result)
    {
        float dist = Vector3.Distance(player.position, transform.position);
        if (dist >= closeDistance && dist <= farDistance && TryFindHighPoint(out Vector3 highPos))
        {
            result = highPos;
            return true;
        }

        // Otherwise sample random points
        for (int i = 0; i < maxSpawnAttempts; i++)
        {
            Vector3 cand = CalculateCandidatePoint();
            if (ValidatePoint(cand, out Vector3 adjusted))
            {
                result = adjusted;
                return true;
            }
        }

        result = transform.position;
        return false;
    }

    // Generate a random candidate around the enemy, biased by player position
    private Vector3 CalculateCandidatePoint()
    {
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        float dist = Vector3.Distance(player.position, transform.position);
        Vector3[] options;

        // Choose direction based on distance: forward, angled, or side moves
        if (dist > farDistance)
            options = new[] { Quaternion.AngleAxis(-45, Vector3.up) * dirToPlayer, dirToPlayer, Quaternion.AngleAxis(45, Vector3.up) * dirToPlayer };
        else if (dist < closeDistance)
            options = new[] { Quaternion.AngleAxis(135, Vector3.up) * dirToPlayer, -dirToPlayer, Quaternion.AngleAxis(-135, Vector3.up) * dirToPlayer };
        else
            options = new[] { Quaternion.AngleAxis(90, Vector3.up) * dirToPlayer, Quaternion.AngleAxis(-90, Vector3.up) * dirToPlayer };

        Vector3 bias = options[Random.Range(0, options.Length)];
        float r = Random.Range(0f, dashRadius);
        return transform.position + bias * r;
    }

    // Validate candidate by projecting down to ground and applying vertical offset
    private bool ValidatePoint(Vector3 cand, out Vector3 finalPos)
    {
        finalPos = cand;
        float rayHeight = maxVerticalDashHeight + 1f;
        if (Physics.Raycast(cand + Vector3.up * rayHeight, Vector3.down, out RaycastHit hit, rayHeight + 1f, groundLayerMask))
        {
            finalPos = hit.point + Vector3.up * highPointYOffset;
        }
        return true;
    }

    // Attempt to find a nearby elevated point for tactical advantage
    private bool TryFindHighPoint(out Vector3 highPoint)
    {
        highPoint = transform.position;
        float searchHeight = maxVerticalDashHeight;

        for (int i = 0; i < highPointSearchAttempts; i++)
        {
            Vector2 circle = Random.insideUnitCircle.normalized * Random.Range(minHighPointHorizontal, dashRadius);
            Vector3 startPos = transform.position + new Vector3(circle.x, searchHeight + 1f, circle.y);
            if (Physics.Raycast(startPos, Vector3.down, out RaycastHit hit, searchHeight + 2f, groundLayerMask)
                && hit.point.y > transform.position.y + 0.1f)
            {
                highPoint = hit.point + Vector3.up * highPointYOffset;
                return true;
            }
        }
        return false;
    }

    // Perform the dash movement over time
    private IEnumerator DashTo(Vector3 target)
    {
        isDashing = true;
        col.enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;

        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, target);
        float traveled = 0f;

        while (traveled < dist)
        {
            float step = dashSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            traveled += step;
            yield return null;
        }

        transform.position = target;
        col.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        isDashing = false;
    }
}
