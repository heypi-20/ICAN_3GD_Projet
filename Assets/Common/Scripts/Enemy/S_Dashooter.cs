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
    [Range(0f,1f)]
    public float highPointChance = 0.8f;

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

    [Header("Laser Settings")]
    public float chargeDuration = 1f;
    public float chargeRotationSpeed = 180f;            // degrees per second
    [Tooltip("Min/max prediction time in seconds")]
    public Vector2 anticipationTimeRange = new Vector2(0.1f, 0.5f);
    public float laserSpeed = 20f;
    public LayerMask laserBlockMask;
    public TrailRenderer laserTrailPrefab;
    public float fireDistance = 7f;      // must be within this to fire
    public float maxBeamDistance = 7f;   // beam travel limit

    [Header("Rotation & Gravity")]
    public float rotationSpeed = 90f;    // degrees per second
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

    // for velocity-based prediction
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
        if (player != null)
            lastPlayerPos = player.position;
        ScheduleNextDash();
    }

    private void Update()
    {
        if (player == null || isDashing)
            return;

        // update approximate player velocity
        playerVelocity = (player.position - lastPlayerPos) / Time.deltaTime;
        lastPlayerPos = player.position;

        // if off ground, apply fall acceleration
        if (groundCheck != null && !groundCheck.TriggerDetection())
        {
            rb.AddForce(Vector3.down * fallAcceleration, ForceMode.Acceleration);
            return;
        }

        // preparing to dash: face target
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

        // idle: face player and countdown
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
        // 1) Dash to the chosen point
        yield return StartCoroutine(DashTo(nextDashTarget));

        // 2) Immediately face the player
        RotateTowards(player.position, rotationSpeed);

        // 3) If too far, skip firing
        float distToPlayer = Vector3.Distance(transform.position, player.position);
        if (distToPlayer > fireDistance)
        {
            ScheduleNextDash();
            yield break;
        }

        // 4) Charge laser while continuously facing the player
        float t = chargeDuration;
        while (t > 0f)
        {
            RotateTowards(player.position, chargeRotationSpeed);
            t -= Time.deltaTime;
            yield return null;
        }

        // 5) At fire moment, compute predicted direction and fire
        float predictTime = (anticipationTimeRange != Vector2.zero)
            ? Random.Range(anticipationTimeRange.x, anticipationTimeRange.y)
            : 0f;
        Vector3 predictedPos = player.position + playerVelocity * predictTime;
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
                // hit wall or ground
                if ((laserBlockMask & (1 << hitLayer)) != 0 ||
                    (groundLayerMask & (1 << hitLayer)) != 0)
                {
                    break;
                }
                // hit player
                if (hitLayer == playerLayer)
                {
                    var trigger = hitInfo.collider.GetComponent<S_PlayerHitTrigger>();
                    if (trigger != null)
                        trigger.ReceiveDamage(enemyDamage);
                    break;
                }
            }

            traveled += step;
            yield return null;
        }

        Destroy(trail.gameObject, 2f);

        // 6) Schedule next dash
        ScheduleNextDash();
    }

    private void ScheduleNextDash()
    {
        dashTimer = Random.Range(dashInterval.x, dashInterval.y);
        Vector3 candidate;
        if (Random.value <= highPointChance && TryFindHighPoint(out candidate))
            nextDashTarget = candidate;
        else
            nextDashTarget = FindValidHorizontal();
    }

    private Vector3 FindValidHorizontal()
    {
        for (int i = 0; i < horizontalAttempts; i++)
        {
            TryFindHorizontalPoint(out Vector3 cand);
            Collider[] hits = Physics.OverlapSphere(cand, col.bounds.extents.magnitude);
            int blockers = 0;
            foreach (var h in hits)
                if (((1 << h.gameObject.layer) & groundLayerMask) == 0)
                    blockers++;
            if (hits.Length == 0 || (float)blockers / hits.Length <= 0.1f)
                return cand;
        }
        // fallback: random direction at exactly minDashDistance
        Vector2 rnd = Random.insideUnitCircle.normalized;
        Vector3 dir = new Vector3(rnd.x, 0f, rnd.y);
        return transform.position + dir * minDashDistance;
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
        col.enabled = true;
        rb.isKinematic = false;
        rb.useGravity = true;
        isDashing = false;
    }

    private bool TryFindHighPoint(out Vector3 highPoint)
    {
        var candidates = new List<Vector3>();
        float halfX = (gridCountX - 1) * 0.5f * gridSpacing;
        float halfZ = (gridCountZ - 1) * 0.5f * gridSpacing;

        for (int ix = 0; ix < gridCountX; ix++)
        for (int iz = 0; iz < gridCountZ; iz++)
        {
            float ox = ix * gridSpacing - halfX;
            float oz = iz * gridSpacing - halfZ;
            if (new Vector2(ox, oz).magnitude < ignoreGridRadius) continue;

            Vector3 origin = transform.position + new Vector3(ox, gridHeight, oz);
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayLength, groundLayerMask))
            {
                Vector3 pt = hit.point + Vector3.up * highPointYOffset;
                float d = Vector3.Distance(transform.position, pt);
                if (d >= minDashDistance && d <= dashRadius
                    && !Physics.Raycast(pt + Vector3.up * 0.1f, Vector3.up, 1f, ceilingLayerMask)
                    && Vector3.Distance(player.position, pt) <= farDistance)
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

    private bool TryFindHorizontalPoint(out Vector3 result)
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
        {
            var rnd = Random.insideUnitCircle;
            dir = new Vector3(rnd.x, 0f, rnd.y).normalized;
        }

        float r = Random.Range(minDashDistance, dashRadius);
        result = transform.position + dir * r;
        result.y = transform.position.y;
        return true;
    }

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
        Gizmos.color = Color.cyan;
        float halfX = (gridCountX - 1) * 0.5f * gridSpacing;
        float halfZ = (gridCountZ - 1) * 0.5f * gridSpacing;

        for (int ix = 0; ix < gridCountX; ix++)
        for (int iz = 0; iz < gridCountZ; iz++)
        {
            float x = ix * gridSpacing - halfX;
            float z = iz * gridSpacing - halfZ;
            if (new Vector2(x, z).magnitude < ignoreGridRadius) continue;
            Vector3 p = transform.position + new Vector3(x, gridHeight, z);
            Gizmos.DrawWireSphere(p, 0.1f);
            Gizmos.DrawLine(p, p + Vector3.down * rayLength);
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, dashRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, minDashDistance);
    }
}
