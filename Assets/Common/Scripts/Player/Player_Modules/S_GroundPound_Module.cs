using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Cinemachine;
using DG.Tweening;
using UnityEngine.Serialization;

public class S_GroundPound_Module : MonoBehaviour
{
    [System.Serializable]
    public class GroundPoundLevel
    {
        public int level;
        public float maxSphereRange;
        public float sphereDamage;
        public float descentSpeed;
        public float energyConsumption;
        public int dropBonus;
    }

    [Header("Ground Pound Settings")]
    public List<GroundPoundLevel> groundPoundLevels;
    public LayerMask KillableTargetLayer;
    public LayerMask groundLayer;
    public float angleThreshold = 75f;
    public float minimumGroundDistance;

    [Header("Shared Skill Settings")]
    public float cooldown = 5f;
    public float bounceMultiplier = 0.5f;
    public float bounceCountStep = 10f;
    public float bounceLimite;
    public float waveExpansionSpeed = 10f;
    public float knockbackAngle = 45f;
    public float knockbackDistance = 5f;
    public float knockbackSpeed = 10f;

    private S_InputManager _inputManager;
    private S_EnergyStorage _energyStorage;
    private Transform _cameraTransform;
    private CharacterController _characterController;
    private S_CustomCharacterController _customCharacterController;

    private bool _isGrounded;
    private bool _isGroundPounding;
    private float _dynamicSphereRange;
    private float _cooldownTimer;
    private float _groundPoundStartHeight;
    private Vector3 _waveOrigin;
    private float _currentWaveRadius;
    private HashSet<EnemyBase> _hitEnemies;

    public event Action<Enum, int> OnGroundPoundStateChange;
    private LayerMask savedLayerMask;

    private void Start()
    {
        _inputManager = FindObjectOfType<S_InputManager>();
        _energyStorage = GetComponent<S_EnergyStorage>();
        _characterController = GetComponent<CharacterController>();
        _customCharacterController = GetComponent<S_CustomCharacterController>();
        _cameraTransform = FindObjectOfType<CinemachineBrain>().transform;
        savedLayerMask = _characterController.excludeLayers;
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        HandleGroundPound();
    }

    private void GroundPoundObserverEvent(Enum state, int level)
    {
        OnGroundPoundStateChange?.Invoke(state, level);
    }

    private void HandleGroundPound()
    {
        if (_isGroundPounding || _cooldownTimer > 0f)
            return;

        var currentLevel = GetCurrentGroundPoundLevel();
        if (currentLevel == null)
            return;

        if (_inputManager.MeleeAttackInput)
        {
            if (IsLookingAtValidTarget(out float dist))
            {
                GroundPoundObserverEvent(PlayerStates.GroundPoundState.StartGroundPound, currentLevel.level);
                PerformGroundPound(currentLevel, dist);
                _energyStorage.RemoveEnergy(currentLevel.energyConsumption);
            }
        }
    }

    public bool IsLookingAtValidTarget(out float distanceToGround)
    {
        distanceToGround = 0f;
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            float vertDist = Mathf.Abs(hit.point.y - transform.position.y);
            float angle = Vector3.Angle(_cameraTransform.forward, Vector3.down);
            if (vertDist > minimumGroundDistance && angle < angleThreshold)
            {
                distanceToGround = vertDist;
                return true;
            }
        }
        return false;
    }

    private void PerformGroundPound(GroundPoundLevel level, float dist)
    {
        _isGroundPounding = true;
        StopAllCoroutines();

        _dynamicSphereRange = Mathf.Min(dist, level.maxSphereRange);
        _groundPoundStartHeight = transform.position.y;

        GroundPoundObserverEvent(PlayerStates.GroundPoundState.isGroundPounding, level.level);
        StartCoroutine(MoveToGround(level.descentSpeed, level));
    }

    private IEnumerator MoveToGround(float maxSpeed, GroundPoundLevel level)
    {
        float curSpeed = 0f;
        float accel = maxSpeed;

        while (!_isGrounded)
        {
            if (_customCharacterController.GroundCheck())
            {
                _isGrounded = true;
                break;
            }
            curSpeed = Mathf.Min(curSpeed + accel * Time.deltaTime, maxSpeed);
            var dir = _cameraTransform.forward.normalized;
            _characterController.excludeLayers = KillableTargetLayer | savedLayerMask;
            _characterController.Move(dir * (curSpeed * Time.deltaTime));
            yield return null;
        }

        if (_isGrounded)
        {
            float impactY = transform.position.y;
            _waveOrigin = transform.position;
            TriggerGroundPoundEffect();
            GroundPoundObserverEvent(PlayerStates.GroundPoundState.EndGroundPound, level.level);

            float traveled = _groundPoundStartHeight - impactY;
            float bounceHeight = traveled * bounceMultiplier;
            if (bounceHeight > bounceLimite)
            {
                bounceHeight = bounceLimite;
            }
            _customCharacterController.velocity.y = bounceHeight;
            StartCoroutine(BounceCoroutine(bounceHeight));

            _cooldownTimer = cooldown;
        }

        _isGroundPounding = false;
        _isGrounded = false;
    }

    private IEnumerator BounceCoroutine(float height)
    {
        float moved = 0f;
        while (moved < height)
        {
            float step = bounceCountStep * Time.deltaTime;
            moved += step;
            yield return null;
        }
        _characterController.excludeLayers = savedLayerMask;

    }

    private void TriggerGroundPoundEffect()
    {
        var lvl = GetCurrentGroundPoundLevel();
        if (lvl == null)
            return;

        _hitEnemies = new HashSet<EnemyBase>();
        StartCoroutine(DamageWaveCoroutine(_dynamicSphereRange, waveExpansionSpeed, lvl.sphereDamage, lvl.dropBonus));
    }

    private IEnumerator DamageWaveCoroutine(float maxRange, float speed, float damage, int dropBonus)
    {
        float radius = 0f;
        while (radius < maxRange)
        {
            radius += speed * Time.deltaTime;
            _currentWaveRadius = radius;

            Collider[] hits = Physics.OverlapSphere(_waveOrigin, radius, KillableTargetLayer);
            foreach (var col in hits)
            {
                if (col.TryGetComponent<EnemyBase>(out var enemy) && !_hitEnemies.Contains(enemy))
                {
                    enemy.ReduceHealth(damage, dropBonus);
                    _hitEnemies.Add(enemy);
                    if (enemy.GetComponents<S_LaserShooterBoss>() == null)
                    {
                        Vector3 dirToEnemy = (enemy.transform.position - _waveOrigin).normalized;
                        Vector3 horiz = new Vector3(dirToEnemy.x, 0, dirToEnemy.z).normalized;
                        float rad = knockbackAngle * Mathf.Deg2Rad;
                        Vector3 dir3D = (horiz * Mathf.Cos(rad) + Vector3.up * Mathf.Sin(rad)).normalized;
                        StartCoroutine(KnockbackCoroutine(enemy.transform, dir3D, knockbackDistance, knockbackSpeed));
                    }
                    
                }
            }
            yield return null;
        }
        _currentWaveRadius = 0f;
    }

    private IEnumerator KnockbackCoroutine(Transform enemy, Vector3 dir, float dist, float speed)
    {
        float moved = 0f;
        while (moved < dist)
        {
            float step = speed * Time.deltaTime;
            enemy.Translate(dir * step, Space.World);
            moved += step;
            yield return null;
        }
    }

    private GroundPoundLevel GetCurrentGroundPoundLevel()
    {
        if (groundPoundLevels == null || groundPoundLevels.Count == 0) return null;

        int idx = _energyStorage.currentLevelIndex + 1;

        var lvl = groundPoundLevels.FirstOrDefault(l => l.level == idx);
        if (lvl != null) return lvl;

        lvl = groundPoundLevels.Where(l => l.level < idx)
            .OrderByDescending(l => l.level)
            .FirstOrDefault();
        if (lvl != null) return lvl;

        return null;
    }

    private void OnDrawGizmos()
    {
        if (_cameraTransform == null)
        {
            var cam = Camera.main;
            if (cam != null) _cameraTransform = cam.transform;
            else return;
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _dynamicSphereRange);

        Gizmos.color = Color.green;
        var start = transform.position;
        var end = transform.position + Vector3.down * minimumGroundDistance;
        Gizmos.DrawLine(start, end);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(end, 0.1f);

        Gizmos.color = Color.yellow;
        var rayEnd = _cameraTransform.position + _cameraTransform.forward * 10f;
        Gizmos.DrawLine(_cameraTransform.position, rayEnd);
        if (Physics.Raycast(_cameraTransform.position, _cameraTransform.forward, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.2f);
        }

        // Draw expanding damage wave at impact point
        if (_currentWaveRadius > 0f)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(_waveOrigin, _currentWaveRadius);
        }
    }
}
