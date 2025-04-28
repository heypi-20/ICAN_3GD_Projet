using DG.Tweening;
using UnityEngine;

public class S_JumpyCuby_Behavior : EnemyBase
{
    #region Components
    private Rigidbody rb;
    private S_EnemyGroundCheck groundCheck;
    private BoxCollider boxCollider;
    private Vector3 originalBoxSize;
    #endregion

    #region Jump Settings
    [Header("Jump Settings")]
    public float jumpForce = 10f;                   // upward impulse when jumping
    public float trackingForce = 5f;                // additional impulse toward target
    public float rotationSpeed = 1f;                // rotation lerp speed
    public bool enableTracking = false;             // track target during jump
    public Transform target;                        // assigned target
    public float squashDuration = 0.1f;             // time to compress
    public float stretchDuration = 0.1f;            // time to expand
    public float squashFactor = 0.8f;               // relative scale factor for squash
    public float stretchFactor = 1.2f;              // relative scale factor for stretch
    #endregion

    #region Boss Settings
    [Header("Boss Settings")]
    public bool bossMode = false;                   // enable shockwave on landing
    public float bossShakeRadius = 5f;              // radius of shockwave
    public float bossShakeUpForce = 5f;             // upward force of shockwave
    public float bossShakeOutwardForce = 5f;        // outward force of shockwave
    #endregion

    #region Timing Settings
    [Header("Jump Timing")]
    public float minJumpInterval = 1f;              // minimum time between jumps
    public float maxJumpInterval = 3f;              // maximum time between jumps
    private float nextJumpTime = 0f;                // timestamp for next jump
    #endregion

    #region Gravity Simulation
    [Header("Gravity Settings")]
    public float gravityForce = 9.81f;               // downward acceleration when airborne
    #endregion

    #region Private Flags
    private bool isSquashing = false;               // prevent overlapping squash animations
    private bool wasGrounded = false;               // track previous ground state
    private bool hasJustJumped = false;             // track jump state for shockwave
    #endregion

    #region Unity Callbacks
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        groundCheck = GetComponent<S_EnemyGroundCheck>();
        boxCollider = GetComponent<BoxCollider>();
        if (boxCollider != null)
            originalBoxSize = boxCollider.size;

        // auto-find target if not assigned
        if (target == null)
        {
            var player = FindObjectOfType<S_CustomCharacterController>();
            if (player != null)
                target = player.transform;
        }

        wasGrounded = false;
        hasJustJumped = false;
    }

    private void Update()
    {
        bool onGround = groundCheck != null && groundCheck.TriggerDetection();

        // landing detection after jump
        if (onGround && !wasGrounded && hasJustJumped)
        {
            if (bossMode)
                TriggerBossShockwave();
            ScheduleNextJump();
            hasJustJumped = false;
        }
        wasGrounded = onGround;

        // handle rotation only when airborne
        HandleRotation(!onGround);
        // handle jump when grounded
        if (onGround)
            HandleJumpCheck();
        // apply custom gravity when airborne
        if (!onGround)
            HandleGravity();
    }
    #endregion

    #region Update Handlers
    // rotate toward target if airborne
    private void HandleRotation(bool isAirborne)
    {
        if (!isAirborne || target == null) return;
        Quaternion startRot = transform.rotation;
        Vector3 dir = (target.position - transform.position).normalized;
        Quaternion endRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Lerp(startRot, endRot, rotationSpeed * Time.deltaTime);
    }

    // trigger jump sequence if time reached and grounded
    private void HandleJumpCheck()
    {
        if (isSquashing) return;
        if (Time.time >= nextJumpTime)
            StartSquashStretch();
    }

    // apply downward force when airborne
    private void HandleGravity()
    {
        rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
    }
    #endregion

    #region Jump Sequence
    private void ScheduleNextJump()
    {
        nextJumpTime = Time.time + Random.Range(minJumpInterval, maxJumpInterval);
    }

    private void StartSquashStretch()
    {
        isSquashing = true;
        Vector3 originalScale = transform.localScale;
        Vector3 squashScaleVec = originalScale * squashFactor;
        Vector3 stretchScaleVec = originalScale * stretchFactor;

        Sequence seq = DOTween.Sequence();
        // squash transform and collider
        seq.Append(transform.DOScale(squashScaleVec, squashDuration).SetEase(Ease.OutQuad));
        if (boxCollider != null)
            seq.Join(DOTween.To(() => boxCollider.size, x => boxCollider.size = x, originalBoxSize * squashFactor, squashDuration));

        // stretch and then jump
        seq.Append(transform.DOScale(stretchScaleVec, stretchDuration).SetEase(Ease.OutQuad));
        if (boxCollider != null)
            seq.Join(DOTween.To(() => boxCollider.size, x => boxCollider.size = x, originalBoxSize * stretchFactor, stretchDuration));

        seq.AppendCallback(() => { ExecuteJump(); hasJustJumped = true; });

        // restore original scale and collider
        seq.Append(transform.DOScale(originalScale, squashDuration).SetEase(Ease.InQuad));
        if (boxCollider != null)
            seq.Join(DOTween.To(() => boxCollider.size, x => boxCollider.size = x, originalBoxSize, squashDuration));

        seq.OnComplete(() => isSquashing = false);
    }

    private void ExecuteJump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        if (enableTracking && target != null)
        {
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            rb.AddForce(dirToTarget * trackingForce, ForceMode.Impulse);
        }
        else
        {
            Vector3 randDir = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;
            rb.AddForce(randDir * trackingForce, ForceMode.Impulse);
        }
        // next jump scheduled on next landing
    }
    #endregion

    #region Boss Shockwave
    // trigger spherical shockwave affecting other jumpy cubies
    private void TriggerBossShockwave()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, bossShakeRadius);
        foreach (var hit in hits)
        {
            if (hit.transform == transform) continue;
            var other = hit.GetComponent<S_JumpyCuby_Behavior>();
            if (other != null)
            {
                var otherRb = other.GetComponent<Rigidbody>();
                if (otherRb != null)
                {
                    Vector3 dir = (hit.transform.position - transform.position).normalized;
                    Vector3 force = dir * bossShakeOutwardForce + Vector3.up * bossShakeUpForce;
                    otherRb.AddForce(force, ForceMode.Impulse);
                }
            }
        }
    }
    #endregion

    #region Gizmos
    // draw shockwave radius when boss mode enabled
    private void OnDrawGizmosSelected()
    {
        if (!bossMode) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, bossShakeRadius);
    }
    #endregion
}
