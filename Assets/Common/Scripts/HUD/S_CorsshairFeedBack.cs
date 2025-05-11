using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class S_CrosshairFeedback : MonoBehaviour
{
    [Header("Ray Settings")]
    public Transform rayOrigin;
    public float maxDistance = 100f;
    public LayerMask obstacleLayer;
    public LayerMask enemyLayer;

    [Header("UI Targets")]
    public TargetData[] targets;

    [Header("Hit Feedback")]
    public GameObject hitFeedbackObject;
    public float hitFeedbackDuration = 0.5f;

    [Header("Kill Feedback")]
    [Tooltip("Object to show when an enemy is killed")]
    public GameObject killFeedbackObject;
    public float killFeedbackDuration = 1f;

    private bool isTargetingEnemy;
    private Coroutine hitFeedbackCoroutine;
    private Coroutine killFeedbackCoroutine;

    [Serializable]
    public class TargetData
    {
        public RectTransform uiElement;
        public Vector3 targetPosition;
        [HideInInspector] public Vector3 originalPosition;
    }

    private void Start()
    {
        // cache original positions
        foreach (var td in targets)
            if (td.uiElement != null)
                td.originalPosition = td.uiElement.localPosition;

        // ensure feedback objects are hidden initially
        if (hitFeedbackObject != null) hitFeedbackObject.SetActive(false);
        if (killFeedbackObject != null) killFeedbackObject.SetActive(false);

        // subscribe
        var obs = S_PlayerStateObserver.Instance;
        obs.OnShootStateEvent       += HandleShootEvent;
        obs.OnSprintStateEvent      += HandleShootEvent;
        obs.OnMeleeAttackStateEvent += HandleShootEvent;
        EnemyBase.OnEnemyKilled     += HandleEnemyKilled;
    }

    private void OnDestroy()
    {
        var obs = S_PlayerStateObserver.Instance;
        if (obs != null)
        {
            obs.OnShootStateEvent       -= HandleShootEvent;
            obs.OnSprintStateEvent      -= HandleShootEvent;
            obs.OnMeleeAttackStateEvent -= HandleShootEvent;
        }
        EnemyBase.OnEnemyKilled -= HandleEnemyKilled;
    }

    private void Update()
    {
        bool hitEnemy = CheckHitEnemy();

        if (hitEnemy && !isTargetingEnemy)
        {
            foreach (var td in targets)
                if (td.uiElement != null)
                    td.uiElement.localPosition = td.targetPosition;
            isTargetingEnemy = true;
        }
        else if (!hitEnemy && isTargetingEnemy)
        {
            foreach (var td in targets)
                if (td.uiElement != null)
                    td.uiElement.localPosition = td.originalPosition;
            isTargetingEnemy = false;
        }
    }

    private bool CheckHitEnemy()
    {
        if (Physics.Raycast(rayOrigin.position, rayOrigin.forward,
            out RaycastHit hit, maxDistance, obstacleLayer | enemyLayer))
        {
            int mask = 1 << hit.collider.gameObject.layer;
            if ((obstacleLayer.value & mask) != 0) return false;
            return (enemyLayer.value & mask) != 0;
        }
        return false;
    }

    private void HandleShootEvent(Enum state, int level)
    {
        if (state.Equals(PlayerStates.ShootState.hitEnemy) ||
            state.Equals(PlayerStates.MeleeState.MeleeAttackHit) ||
            state.Equals(PlayerStates.SprintState.SprintHit))
        {
            if (hitFeedbackCoroutine == null)
                hitFeedbackCoroutine = StartCoroutine(HitFeedbackCoroutine());
        }
    }

    private IEnumerator HitFeedbackCoroutine()
    {
        hitFeedbackObject.SetActive(true);
        yield return new WaitForSeconds(hitFeedbackDuration);
        hitFeedbackObject.SetActive(false);
        hitFeedbackCoroutine = null;
    }

    private void HandleEnemyKilled(EnemyType type)
    {
        // if one is running, stop it
        if (killFeedbackCoroutine != null)
            StopCoroutine(killFeedbackCoroutine);
        killFeedbackCoroutine = StartCoroutine(KillFeedbackCoroutine());
    }

    private IEnumerator KillFeedbackCoroutine()
    {
        killFeedbackObject.SetActive(true);
        yield return new WaitForSeconds(killFeedbackDuration);
        killFeedbackObject.SetActive(false);
        killFeedbackCoroutine = null;
    }
}
