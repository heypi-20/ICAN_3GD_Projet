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
        public float attackRange; // Radius of the attack range
        public float attackDistance; // Distance of the attack
        public float attackDamage; // Damage per attack
        public float attackCooldown; // Cooldown time between attacks
        public float energyConsumption; // Energy consumption per attack
        public int dropBonus;
        public int WeakPointDropBonus; 

    }

    [Header("Attack Settings")]
    public Transform attackOrigin; // Origin of the attack (center of the capsule)
    public List<MeleeAttackLevel> attackLevels; // List of attack levels
    public LayerMask targetLayer; // Layer for objects that can be destroyed
    public float windupTime = 0.2f; // Windup time (delay before executing the attack after pressing the attack button)
    
    [Header("Dash Settings")]
    public float dashMoveDistance = 1.0f; // Total dash distance to move before attacking
    public float dashDuration = 0.5f;   // Duration over which to perform the dash movement

    [Header("Gizmos settings")]
    public bool drawGizmos = true;

    private S_InputManager _inputManager; // Reference to the input manager
    private S_EnergyStorage _energyStorage; // Reference to the energy storage
    private float _attackCooldownTimer; // Timer to manage the cooldown period
    public float currentAttackCD;
    private bool _isWindupInProgress = false; // Flag indicating whether windup is in progress
    private CharacterController _characterController;
    public event Action<Enum, int> OnAttackStateChange;
    
    private void Start()
    {
        // Initialize references
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
        
        // Check input, cooldown, and energy conditions, and ensure windup is not already in progress
        if (_inputManager.MeleeAttackInput && _attackCooldownTimer <= 0f && _energyStorage.currentEnergy >= currentLevel.energyConsumption 
            && S_PlayerStateObserver.Instance.LastMeleeState == null && !_isWindupInProgress)
        {
            _isWindupInProgress = true;
            // Trigger the start attack event
            MeleeAttackObserverEvent(PlayerStates.MeleeState.StartMeleeAttack, currentLevel.level);
            // Start the windup coroutine and wait for windupTime seconds before executing the attack logic
            StartCoroutine(WindupAndAttack(currentLevel));
            
        }

        // Decrease the cooldown timer
        if (_attackCooldownTimer > 0f)
        {
            _attackCooldownTimer -= Time.deltaTime;
        }
    }
    
    

    private IEnumerator WindupAndAttack(MeleeAttackLevel currentLevel)
    {
        //dash when ennemis is far
        if (!Physics.Raycast(attackOrigin.position, attackOrigin.forward,currentLevel.attackDistance, targetLayer))
        {
            StartCoroutine(AttackMovementCoroutine(dashDuration, dashMoveDistance));
        }
        
        // Wait for the windup time
        yield return new WaitForSeconds(windupTime);

        // Play the attack sound effect (executed after the windup ends)
        SoundManager.Instance.Meth_Active_CAC();
        
        // Execute the melee attack logic
        PerformMeleeAttack(currentLevel);
        _attackCooldownTimer = currentLevel.attackCooldown; // Reset the cooldown timer
        _energyStorage.RemoveEnergy(currentLevel.energyConsumption); // Consume energy

        // Start the cooldown timer coroutine
        yield return StartCoroutine(StartTimer(_attackCooldownTimer));

        _isWindupInProgress = false;
    }
    
    
    
    // Coroutine to move the character gradually forward
    private IEnumerator AttackMovementCoroutine(float duration, float moveDistance)
    {
        // Calculate constant speed: total distance divided by duration
        float dashSpeed = moveDistance / duration;
        float elapsed = 0f;
        MeleeAttackObserverEvent(PlayerStates.MeleeState.DashingBeforeMelee, GetCurrentAttackLevel().level);

        while (elapsed < duration)
        {
            // Move the character in the attack origin's forward direction
            _characterController.Move(attackOrigin.forward * (dashSpeed * Time.deltaTime));
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator StartTimer(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        MeleeAttackObserverEvent(PlayerStates.MeleeState.EndMeleeAttack, GetCurrentAttackLevel().level);
        _inputManager.MeleeAttackInput = false;
    }

    // Detection logic: First use OverlapCapsule to detect targets in the static area; if none are found, use CapsuleCast as a fallback.
    private void PerformMeleeAttack(MeleeAttackLevel currentLevel)
    {
        Vector3 capsuleStart = attackOrigin.position;
        Vector3 capsuleEnd = attackOrigin.position + attackOrigin.forward * currentLevel.attackDistance;
        float radius = currentLevel.attackRange * 0.5f;

        // First use OverlapCapsule to detect targets within the static area (consistent with the Gizmos display)
        Collider[] overlaps = Physics.OverlapCapsule(capsuleStart, capsuleEnd, radius, targetLayer);
        if (overlaps.Length > 0)
        {
            // Select the target that is closest along the attack direction from the detected targets
            Collider bestHit = null;
            float bestDist = float.MaxValue;
            Vector3 origin = attackOrigin.position;
            Vector3 forward = attackOrigin.forward;
            foreach (Collider col in overlaps)
            {
                float dist = Vector3.Dot(col.transform.position - origin, forward);
                if (dist >= 0 && dist < bestDist)
                {
                    bestDist = dist;
                    bestHit = col;
                }
            }
            if (bestHit != null)
            {
                MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackHit, currentLevel.level);
                
                Rigidbody targetRB = bestHit.GetComponentInParent<Rigidbody>();
                if (targetRB != null)
                {
                    Vector3 forceDirection = (targetRB.transform.position - transform.position).normalized;
                    targetRB.AddForce(forceDirection * 0f, ForceMode.Impulse);
                }

                if (bestHit.CompareTag("WeakPoint"))
                {
                    bestHit.GetComponentInParent<EnemyBase>()?.ReduceHealth(currentLevel.attackDamage * 100, currentLevel.dropBonus + currentLevel.WeakPointDropBonus);
                    Debug.Log("Touch Weak Point " + currentLevel.attackDamage * 100);
                }
                else
                {
                    bestHit.GetComponent<EnemyBase>()?.ReduceHealth(currentLevel.attackDamage, currentLevel.dropBonus);
                    Debug.Log("Didn't Touch Weak Point");
                }
                return;
            }
        }
        else
        {
            // If no target is detected in the static area, use CapsuleCast as a fallback (detection area consistent with OverlapCapsule)
            RaycastHit hit;
            bool didHit = Physics.CapsuleCast(capsuleStart, capsuleEnd, radius, attackOrigin.forward, out hit, 0, targetLayer);
            if (didHit)
            {
                
                Rigidbody targetRB = hit.collider.gameObject.GetComponent<Rigidbody>();
                if (targetRB != null)
                {
                    Vector3 forceDirection = (targetRB.transform.position - transform.position).normalized;
                    targetRB.AddForce(forceDirection * 20f, ForceMode.Impulse);
                }

                if (hit.collider.CompareTag("WeakPoint"))
                {
                    hit.collider.gameObject.GetComponentInParent<EnemyBase>()?.ReduceHealth(currentLevel.attackDamage * 100, currentLevel.dropBonus * 3);
                    MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackHitWeakness, currentLevel.level);

                }
                else
                {
                    hit.collider.gameObject.GetComponent<EnemyBase>()?.ReduceHealth(currentLevel.attackDamage, currentLevel.dropBonus);
                    MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackHit, currentLevel.level);
                }
                return;
            }
        }

        // If no target is detected, trigger the attack missed event
        MeleeAttackObserverEvent(PlayerStates.MeleeState.MeleeAttackMissed, currentLevel.level);
    }

    private MeleeAttackLevel GetCurrentAttackLevel()
    {
        if (_energyStorage == null)
        {
            _energyStorage = GetComponent<S_EnergyStorage>();
        }
        if (_energyStorage == null) return null;

        int currentLevelIndex = _energyStorage.currentLevelIndex + 1; // Adjust to match the attack levels
        return attackLevels.Find(level => level.level == currentLevelIndex);
    }

    // Gizmos drawing: Draw a capsule that matches the detection area used by OverlapCapsule.
    private void OnDrawGizmos()
    {
        if(!drawGizmos)return;
        if (attackOrigin != null && attackLevels != null && attackLevels.Count > 0)
        {
            MeleeAttackLevel currentLevel = GetCurrentAttackLevel();
            if (currentLevel != null)
            {
                Vector3 startPoint = attackOrigin.position;
                Vector3 endPoint = attackOrigin.position + attackOrigin.forward * currentLevel.attackDistance;
                float radius = currentLevel.attackRange * 0.5f;
                Gizmos.color = Color.red;
                DrawWireCapsule(startPoint, endPoint, radius);
            }
        }
    }

    // Method to draw a wireframe capsule
    private void DrawWireCapsule(Vector3 start, Vector3 end, float radius)
    {
        Vector3 up = end - start;
        float height = up.magnitude;
        if (height < Mathf.Epsilon)
        {
            Gizmos.DrawWireSphere(start, radius);
            return;
        }
        up /= height;
        Vector3 arbitrary = (Mathf.Abs(Vector3.Dot(up, Vector3.up)) > 0.99f) ? Vector3.forward : Vector3.up;
        Vector3 right = Vector3.Cross(up, arbitrary).normalized * radius;
        Vector3 forward = Vector3.Cross(right, up).normalized * radius;

        Vector3 startSphereCenter = start + up * radius;
        Vector3 endSphereCenter = end - up * radius;

        Gizmos.DrawWireSphere(startSphereCenter, radius);
        Gizmos.DrawWireSphere(endSphereCenter, radius);

        Gizmos.DrawLine(startSphereCenter + right, endSphereCenter + right);
        Gizmos.DrawLine(startSphereCenter - right, endSphereCenter - right);
        Gizmos.DrawLine(startSphereCenter + forward, endSphereCenter + forward);
        Gizmos.DrawLine(startSphereCenter - forward, endSphereCenter - forward);

        int segments = 10;
        float angleStep = 180f / segments;
        for (int i = 0; i <= segments; i++)
        {
            float angle = Mathf.Deg2Rad * i * angleStep;
            Vector3 offset = right * Mathf.Cos(angle) + forward * Mathf.Sin(angle);
            Gizmos.DrawLine(startSphereCenter + offset, endSphereCenter + offset);
        }
    }
}
