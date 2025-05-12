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
        public float attackRange; // Radius of the attack range (unused for cone)
        public float attackDistance; // Distance of the attack
        public float attackDamage; // Damage per attack
        public float attackCooldown; // Cooldown time between attacks
        public float energyConsumption; // Energy consumption per attack
        public int dropBonus;
        public int WeakPointDropBonus;
        public float knockbackForce;

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
    public float dashSpeed = 10f; // Dash movement speed (units per second)
    public float StopDashDistance = 3f; // Distance from target at which to stop dash

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

    private IEnumerator AttackMovementCoroutine(Vector3 enemyPos, Collider hit, MeleeAttackLevel currentLevel)
    {
        Vector3 originPos = transform.position;
        float totalDistance = Vector3.Distance(originPos, enemyPos) - StopDashDistance;
        Vector3 direction = (enemyPos - attackOrigin.position).normalized;
        float remaining = totalDistance;
        MeleeAttackObserverEvent(PlayerStates.MeleeState.DashingBeforeMelee, GetCurrentAttackLevel().level);

        // Move until reaching stop distance
        while (remaining > 0f)
        {
            float step = dashSpeed * Time.deltaTime;
            // Clamp step to not overshoot
            float move = Mathf.Min(step, remaining);
            _characterController.Move(direction * move);
            remaining -= move;
            yield return null;
        }

        Rigidbody targetRB = hit.GetComponentInParent<Rigidbody>();
        if (targetRB != null)
        {
            Vector3 forceDirection = (hit.transform.position - transform.position).normalized;
            targetRB.AddForce(forceDirection * 30f, ForceMode.Impulse);
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
        yield return StartCoroutine(StartTimer(_attackCooldownTimer));

    }

    private IEnumerator StartTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        MeleeAttackObserverEvent(PlayerStates.MeleeState.EndMeleeAttack, GetCurrentAttackLevel().level);
        _inputManager.MeleeAttackInput = false;
    }

    private void PerformMeleeAttack(MeleeAttackLevel currentLevel)
    {
        Vector3 origin = attackOrigin.position;
        Vector3 forward = attackOrigin.forward;
        float maxDistance = currentLevel.attackDistance;

        // Cone detection: find candidates in a sphere covering the cone
        Collider[] candidates = Physics.OverlapSphere(origin, maxDistance + currentLevel.coneEndRadius, targetLayer);
        Collider bestHit = null;
        float bestProj = float.MaxValue;
        
        foreach (Collider col in candidates)
        {
            Vector3 toTarget = col.transform.position - origin;
            float proj = Vector3.Dot(toTarget, forward);
            if (proj < 0 || proj > maxDistance) continue; // outside cone length

            // interpolate radius at this distance
            float radiusAtDist = Mathf.Lerp(currentLevel.coneStartRadius, currentLevel.coneEndRadius, proj / maxDistance);
            // perpendicular distance from center line
            float perpDist = Vector3.Magnitude(toTarget - forward * proj);
            if (perpDist <= radiusAtDist)
            {
                if (proj < bestProj)
                {
                    bestProj = proj;
                    bestHit = col;
                }
            }
        }

        if (bestHit != null)
        {
            StartCoroutine(AttackMovementCoroutine(bestHit.transform.position, bestHit, currentLevel));
            return;
        }

        // If no hit, fire missed event
        MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackMissed, currentLevel.level); 
        StartCoroutine(StartTimer(_attackCooldownTimer));

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
            // Draw start circle
            Gizmos.DrawWireSphere(origin, currentLevel.coneStartRadius);
            // Draw end circle
            Vector3 endCenter = origin + forward * maxDistance;
            Gizmos.DrawWireSphere(endCenter, currentLevel.coneEndRadius);

            // Draw lines connecting radii
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
