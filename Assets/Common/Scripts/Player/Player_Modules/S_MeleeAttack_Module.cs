using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class S_MeleeAttack_Module : MonoBehaviour
{
    [System.Serializable]
    public class MeleeAttackLevel
    {
        public int level; // Required level for this attack level
        public float attackRange; // Instant hit range threshold (skip dash if within this distance)
        public float attackDistance; // Max distance of the attack (cone length)
        public float attackDamage; // Damage per attack
        public float attackCooldown; // Cooldown time between attacks
        public float energyConsumption; // Energy consumption per attack
        public int dropBonus; // Drop bonus on hit
        public int WeakPointDropBonus; // Additional drop bonus for weak point hit
        public float knockbackForce; // Knockback force applied on hit

        // Cone-specific radii
        public float coneStartRadius; // Radius at the origin of the cone
        public float coneEndRadius;   // Radius at the end of the cone
    }

    [Header("Attack Settings")]
    public Transform attackOrigin; // Origin of the attack (center of the cone)
    public List<MeleeAttackLevel> attackLevels; // List of attack levels
    public LayerMask targetLayer; // Layer for objects that can be damaged
    public float windupTime = 0.2f; // Windup time before executing the attack
    
    [Header("Dash Settings")]
    public AnimationCurve dashSpeedCurve = AnimationCurve.Linear(0, 0, 1, 10);
    public float StopDashDistance = 3f;

    [Header("Gizmos Settings")]
    public bool drawGizmos = true;

    private S_InputManager _inputManager;
    private S_EnergyStorage _energyStorage;
    private float _attackCooldownTimer;
    public float currentAttackCD;
    private bool _isWindupInProgress = false;
    private CharacterController _characterController;
    public event Action<Enum, int> OnAttackStateChange;

    private void Start()
    {
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        HandleMeleeAttack();
    }

    private void MeleeAttackObserverEvent(Enum attackState, int currentLevel)
    {
        OnAttackStateChange?.Invoke(attackState, currentLevel);
    }

    private void HandleMeleeAttack()
    {
        MeleeAttackLevel currentLevel = GetCurrentAttackLevel();
        currentAttackCD = currentLevel.attackCooldown;
        if (S_PlayerStateObserver.Instance.LastGroundPoundState != null)
        {
            _inputManager.MeleeAttackInput = false;
            return;
        }
        
        if (_inputManager.MeleeAttackInput 
            && _attackCooldownTimer <= 0f 
            && S_PlayerStateObserver.Instance.LastMeleeState == null 
            && !_isWindupInProgress)
        {
            _isWindupInProgress = true;
            MeleeAttackObserverEvent(PlayerStates.MeleeState.StartMeleeAttack, currentLevel.level);
            StartCoroutine(WindupAndAttack(currentLevel));
        }

        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }
    }

    private IEnumerator WindupAndAttack(MeleeAttackLevel currentLevel)
    {
        yield return new WaitForSeconds(windupTime);
        PerformMeleeAttack(currentLevel);
        _attackCooldownTimer = currentLevel.attackCooldown;
        _energyStorage.RemoveEnergy(currentLevel.energyConsumption);
        _isWindupInProgress = false;
    }

    private IEnumerator AttackMovementCoroutine(Vector3 targetPos, Collider hit, MeleeAttackLevel currentLevel)
    {
        MeleeAttackObserverEvent(PlayerStates.MeleeState.DashingBeforeMelee, currentLevel.level);

        float remaining = Vector3.Distance(transform.position, targetPos) - StopDashDistance;
        float elapsed = 0f;

        while (remaining > 0f)
        {
            float currentSpeed = dashSpeedCurve.Evaluate(elapsed);

            float step = currentSpeed * Time.deltaTime;
            float move = Mathf.Min(step, remaining);

            Vector3 dir = (targetPos - transform.position).normalized;
            _characterController.Move(dir * move);

            remaining -= move;
            elapsed += Time.deltaTime;
            yield return null;
        }

        ApplyHitEffect(hit, currentLevel);
        yield return StartCoroutine(StartTimer(_attackCooldownTimer));
    }


    private void ApplyHitEffect(Collider hit, MeleeAttackLevel currentLevel)
    {
        Rigidbody targetRB = hit.GetComponentInParent<Rigidbody>();
        if (targetRB != null)
        {
            Vector3 forceDirection = (hit.transform.position - transform.position).normalized;
            targetRB.AddForce(forceDirection * currentLevel.knockbackForce, ForceMode.Impulse);
        }

        if (hit.CompareTag("WeakPoint"))
        {
            MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackHitWeakness, currentLevel.level);
            hit.GetComponentInParent<EnemyBase>()?.ReduceHealth(currentLevel.attackDamage * 100, currentLevel.dropBonus + currentLevel.WeakPointDropBonus);
        }
        else
        {
            MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackHit, currentLevel.level);
            hit.GetComponent<EnemyBase>()?.ReduceHealth(currentLevel.attackDamage, currentLevel.dropBonus);
        }
    }

    private IEnumerator StartTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        MeleeAttackObserverEvent(PlayerStates.MeleeState.EndMeleeAttack, GetCurrentAttackLevel().level);
        _inputManager.MeleeAttackInput = false;
    }

    private void PerformMeleeAttack(MeleeAttackLevel level)
    {
        Vector3 origin = attackOrigin.position;
        Vector3 forward = attackOrigin.forward;
        float maxDist = level.attackDistance;

        // Collect colliders in bounding sphere
        var candidates = Physics.OverlapSphere(origin, maxDist + level.coneEndRadius, targetLayer);
        Collider best = null;
        float bestProj = float.MaxValue;
        Vector3 bestHitPoint = Vector3.zero;

        foreach (var col in candidates)
        {
            // Use collider center for projection
            Vector3 center = col.bounds.center;
            float proj = Vector3.Dot(center - origin, forward);
            if (proj < 0f || proj > maxDist) continue;

            // Compute cone radius and axis point
            float radiusAt = Mathf.Lerp(level.coneStartRadius, level.coneEndRadius, proj / maxDist);
            Vector3 axisPoint = origin + forward * proj;

            // Find closest point on collider to axis
            Vector3 closestOnCol = col.ClosestPoint(axisPoint);
            float perpDist = Vector3.Distance(closestOnCol, axisPoint);
            if (perpDist <= radiusAt && proj < bestProj)
            {
                bestProj = proj;
                best = col;
            }
        }

        if (best != null)
        {
            if (bestProj <= level.attackRange)
            {
                ApplyHitEffect(best, level);
                StartCoroutine(StartTimer(_attackCooldownTimer));
            }
            else
            {
                Vector3 targetCenter = best.bounds.center;
                StartCoroutine(AttackMovementCoroutine(targetCenter, best, level));
            }
        }
        else
        {
            OnAttackStateChange?.Invoke(PlayerStates.MeleeState.MeleeAttackMissed, level.level);
            StartCoroutine(StartTimer(_attackCooldownTimer));
        }
    }

    private MeleeAttackLevel GetCurrentAttackLevel()
    {
        if (_energyStorage == null)
            _energyStorage = GetComponent<S_EnergyStorage>();
        if (_energyStorage == null) return null;

        int currentLevelIndex = _energyStorage.currentLevelIndex + 1;
        return attackLevels.Find(level => level.level == currentLevelIndex);
    }

    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;
        if (attackOrigin != null && attackLevels != null && attackLevels.Count > 0)
        {
            MeleeAttackLevel currentLevel = GetCurrentAttackLevel();
            if (currentLevel == null) return;

            Vector3 origin = attackOrigin.position;
            Vector3 forward = attackOrigin.forward;
            float maxDistance = currentLevel.attackDistance;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(origin, currentLevel.coneStartRadius);
            Vector3 endCenter = origin + forward * maxDistance;
            Gizmos.DrawWireSphere(endCenter, currentLevel.coneEndRadius);

            Vector3[] dirs = { Vector3.right, Vector3.up, Vector3.left, Vector3.down };
            foreach (var dir in dirs)
            {
                Vector3 startPoint = origin + dir * currentLevel.coneStartRadius;
                Vector3 endPoint = endCenter + dir * currentLevel.coneEndRadius;
                Gizmos.DrawLine(startPoint, endPoint);
            }
        }
    }
}
