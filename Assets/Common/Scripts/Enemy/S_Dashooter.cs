using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class S_Dashooter : EnemyBase
{
    [Header("Dash Settings")]
    public float dashRadius = 5f;
    public float minDashDistance = 1f;
    public float closeDistance = 2f;
    public float farDistance = 8f;
    public Vector2 dashInterval = new Vector2(1f, 3f);
    public float dashSpeed = 15f;
    public float prepareDuration = 1f;
    [Range(0f,1f)] public float highPointChance = 0.8f;

    [Header("High-Point Grid")]
    public int gridCountX = 5;
    public int gridCountZ = 5;
    public float gridSpacing = 1f;
    public float gridHeight = 5f;
    public float rayLength = 10f;
    public LayerMask groundLayerMask;
    public LayerMask ceilingLayerMask;
    public float highPointYOffset = 0.5f;
    public float ignoreGridRadius = 1f;

    [Header("Teleport Settings")]
    public float teleportDistanceThreshold = 15f;
    public float teleportSearchRadius = 5f;
    public float teleportSearchHeight = 10f;
    public int teleportSearchAttempts = 20;

    [Header("Laser Settings")]
    public float chargeDuration = 1f;
    public float chargeRotationSpeed = 180f;
    [Range(0f,1f)] public float predictionAccuracy = 1f;
    public float laserSpeed = 20f;
    public LayerMask laserBlockMask;
    public TrailRenderer laserTrailPrefab;
    public float fireDistance = 7f;
    public float maxBeamDistance = 7f;

    [Header("Rotation & Gravity")]
    public float rotationSpeed = 90f;
    public float fallAcceleration = 30f;

    private Rigidbody rb;
    private Collider col;
    private S_EnemyGroundCheck groundCheck;
    private Transform player;
    private float dashTimer;
    private Vector3 nextDashTarget;
    private bool isPreparing;
    private float prepareTimer;
    private bool isDashing;
    private Vector3 lastPlayerPos;
    private Vector3 playerVelocity;

    private const int horizontalAttempts = 10;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
        groundCheck = GetComponentInChildren<S_EnemyGroundCheck>();
        if (groundCheck != null) groundCheck.physicCastCooldown = 0f;
        nextDashTarget = transform.position;
    }

    private void Start()
    {
        player = FindObjectOfType<S_CustomCharacterController>()?.transform;
        if (player != null) lastPlayerPos = player.position;
        ScheduleNextDash();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        isDashing = false;
    }

    private void Update()
    {
        if (player == null || isDashing) return;

        // compute approximate player velocity
        playerVelocity = (player.position - lastPlayerPos) / Time.deltaTime;
        lastPlayerPos = player.position;

        // if off ground, apply gravity
        if (groundCheck != null && !groundCheck.TriggerDetection())
        {
            rb.AddForce(Vector3.down * fallAcceleration, ForceMode.Acceleration);
            return;
        }

        // preparing to dash
        if (isPreparing)
        {
            RotateTowards(nextDashTarget, rotationSpeed);
            prepareTimer -= Time.deltaTime;
            if (prepareTimer <= 0f)
            {
                isPreparing = false;
                StartCoroutine(DashAndFire());
            }
            return;
        }

        // idle: face player, count down
        RotateTowards(player.position, rotationSpeed);
        dashTimer -= Time.deltaTime;
        if (dashTimer <= 0f)
        {
            ScheduleNextDash();
            isPreparing = true;
            prepareTimer = prepareDuration;
        }
    }

    private IEnumerator DashAndFire()
    {
        // 1) Dash
        yield return StartCoroutine(DashTo(nextDashTarget));

        // 2) Face player
        RotateTowards(player.position, rotationSpeed);

        // 3) Skip if too far
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > fireDistance)
        {
            ScheduleNextDash();
            yield break;
        }

        // 4) Charge laser
        float t = chargeDuration;
        while (t > 0f)
        {
            RotateTowards(player.position, chargeRotationSpeed);
            t -= Time.deltaTime;
            yield return null;
        }

        // 5) Fire with prediction
        float travelTime = distToPlayer / laserSpeed;
        Vector3 predictedPos = player.position + playerVelocity * (travelTime * predictionAccuracy);
        Vector3 fireDir = (predictedPos - transform.position).normalized;

        var trail = Instantiate(
            laserTrailPrefab,
            transform.position,
            Quaternion.LookRotation(fireDir)
        );

        int playerLayer = LayerMask.NameToLayer("Player");
        int mask = laserBlockMask | groundLayerMask | (1 << playerLayer);

        float traveled = 0f;
        while (traveled < maxBeamDistance)
        {
            float step = laserSpeed * Time.deltaTime;
            Vector3 prev = trail.transform.position;
            trail.transform.position += fireDir * step;

            if (Physics.Raycast(prev, fireDir, out RaycastHit hitInfo, step, mask, QueryTriggerInteraction.Ignore))
            {
                int hitLayer = hitInfo.collider.gameObject.layer;
                if ((laserBlockMask & (1 << hitLayer)) != 0 ||
                    (groundLayerMask & (1 << hitLayer)) != 0)
                {
                    break;
                }
                if (hitLayer == playerLayer)
                {
                    var trigger = hitInfo.collider.GetComponent<S_PlayerHitTrigger>();
                    if (trigger != null) trigger.ReceiveDamage(enemyDamage);
                    break;
                }
            }

            traveled += step;
            yield return null;
        }

        Destroy(trail.gameObject, 2f);

        // 6) Next dash
        ScheduleNextDash();
    }

    private IEnumerator DashTo(Vector3 target)
    {
        isDashing = true;
        col.enabled = false;
        rb.useGravity = false;
        rb.isKinematic = true;

        float dist = Vector3.Distance(transform.position, target);
        float moved = 0f;
        while (moved < dist)
        {
            float step = dashSpeed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            moved += step;
            yield return null;
        }
        transform.position = target;

        isDashing = false;
        rb.isKinematic = false;
        col.enabled = true;
        rb.useGravity = true;
        
    }

    private void ScheduleNextDash()
    {
        dashTimer = Random.Range(dashInterval.x, dashInterval.y);
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        Vector3 candidate;

        // Teleport if too far
        if (distToPlayer >= teleportDistanceThreshold && TryFindTeleportPoint(out candidate))
        {
            nextDashTarget = candidate;
            return;
        }

        // High point or horizontal
        if (Random.value <= highPointChance && TryFindHighPoint(out candidate))
            nextDashTarget = candidate;
        else
            nextDashTarget = FindValidHorizontal();
    }

    // --- Landing Validation ---
    private bool ValidateLandingPoint(Vector3 basePos, out Vector3 landingPoint)
    {
        float upCastHeight = gridHeight;
        float downMaxDist = gridHeight + 50f;
        Vector3 origin = basePos + Vector3.up * upCastHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hitDown, downMaxDist, groundLayerMask))
        {
            Vector3 pt = hitDown.point + Vector3.up * highPointYOffset;
            float charHeight = col.bounds.size.y;
            Vector3 headOrigin = pt + Vector3.up * 0.1f;
            if (!Physics.Raycast(headOrigin, Vector3.up, charHeight, ceilingLayerMask))
            {
                landingPoint = pt;
                return true;
            }
        }

        landingPoint = Vector3.zero;
        return false;
    }

    // --- Point Selection ---
    private bool TryFindTeleportPoint(out Vector3 teleportPoint)
    {
        for (int i = 0; i < teleportSearchAttempts; i++)
        {
            Vector2 rnd = Random.insideUnitCircle * teleportSearchRadius;
            Vector3 basePos = new Vector3(
                player.position.x + rnd.x,
                player.position.y + teleportSearchHeight,
                player.position.z + rnd.y
            );
            if (ValidateLandingPoint(basePos, out Vector3 validPt))
            {
                teleportPoint = validPt;
                return true;
            }
        }
        teleportPoint = Vector3.zero;
        return false;
    }

    private Vector3 FindValidHorizontal()
    {
        for (int i = 0; i < horizontalAttempts; i++)
        {
            TryFindHorizontalPoint(out Vector3 candBase);
            if (ValidateLandingPoint(candBase, out Vector3 validPt))
                return validPt;
        }
        // fallback
        Vector2 rnd = Random.insideUnitCircle.normalized;
        Vector3 dir = new Vector3(rnd.x, 0f, rnd.y);
        Vector3 fallbackBase = transform.position + dir * minDashDistance;
        if (ValidateLandingPoint(fallbackBase, out Vector3 fallbackPt))
            return fallbackPt;
        return transform.position + Vector3.up * highPointYOffset;
    }

    private bool TryFindHighPoint(out Vector3 highPoint)
    {
        var candidates = new List<Vector3>();
        float halfX = (gridCountX - 1) * 0.5f * gridSpacing;
        float halfZ = (gridCountZ - 1) * 0.5f * gridSpacing;

        for (int ix = 0; ix < gridCountX; ix++)
        for (int iz = 0; iz < gridCountZ; iz++)
        {
            Vector2 offs = new Vector2(
                ix * gridSpacing - halfX,
                iz * gridSpacing - halfZ
            );
            if (offs.magnitude < ignoreGridRadius) continue;

            Vector3 basePos = transform.position + new Vector3(offs.x, 0f, offs.y);
            Vector3 origin = basePos + Vector3.up * gridHeight;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, Mathf.Infinity, groundLayerMask))
            {
                Vector3 pt = hit.point + Vector3.up * highPointYOffset;
                float d = Vector3.Distance(transform.position, pt);
                if (d >= minDashDistance && d <= dashRadius
                    && Vector3.Distance(player.position, pt) <= farDistance
                    && !Physics.Raycast(pt + Vector3.up * 0.1f, Vector3.up, col.bounds.size.y, ceilingLayerMask))
                {
                    candidates.Add(pt);
                }
            }
        }

        if (candidates.Count > 0)
        {
            highPoint = candidates[Random.Range(0, candidates.Count)];
            return true;
        }
        highPoint = Vector3.zero;
        return false;
    }

    private void TryFindHorizontalPoint(out Vector3 result)
    {
        float dist = Vector3.Distance(player.position, transform.position);
        Vector3 toP = (player.position - transform.position).normalized;
        Vector3 dir;

        if (dist > farDistance)
            dir = toP;
        else if (dist < closeDistance)
            dir = -toP;
        else if (Random.value < 0.8f)
            dir = Vector3.Cross(toP, Vector3.up) * (Random.value < 0.5f ? 1f : -1f);
        else
            dir = new Vector3(Random.Range(-1f,1f), 0f, Random.Range(-1f,1f)).normalized;

        float r = Random.Range(minDashDistance, dashRadius);
        result = transform.position + dir * r;
        result.y = transform.position.y;
    }

    // --- Utilities ---
    private void RotateTowards(Vector3 target, float maxDegPerSec)
    {
        Vector3 d = target - transform.position;
        d.y = 0f;
        if (d.sqrMagnitude < 0.001f) return;
        Quaternion targetRot = Quaternion.LookRotation(d);
        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRot,
            maxDegPerSec * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        // draw grid sample points
        Gizmos.color = Color.cyan;
        float halfX = (gridCountX - 1) * 0.5f * gridSpacing;
        float halfZ = (gridCountZ - 1) * 0.5f * gridSpacing;

        for (int ix = 0; ix < gridCountX; ix++)
        for (int iz = 0; iz < gridCountZ; iz++)
        {
            Vector2 offs = new Vector2(
                ix * gridSpacing - halfX,
                iz * gridSpacing - halfZ
            );
            if (offs.magnitude < ignoreGridRadius) continue;

            Vector3 p = transform.position + new Vector3(offs.x, gridHeight, offs.y);
            Gizmos.DrawWireSphere(p, 0.1f);
            Gizmos.DrawLine(p, p + Vector3.down * rayLength);
        }

        // dash radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, minDashDistance);

        // teleport search around player
        if (player != null)
        {
            Gizmos.color = Color.blue;
            Vector3 basePos = player.position;
            Vector3 topPos  = player.position + Vector3.up * teleportSearchHeight;
            Gizmos.DrawLine(basePos, topPos);

            Gizmos.color = new Color(0f, 0.5f, 1f, 0.3f);
            Gizmos.DrawWireSphere(topPos, teleportSearchRadius);

            Gizmos.color = new Color(0f, 0.5f, 1f, 0.1f);
            Gizmos.DrawSphere(basePos, teleportSearchRadius);
        }
    }
}
