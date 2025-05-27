using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class S_LaserShooterBoss : EnemyBase
{
    #region Inspector --------------------------------------------------------

    [Header("Charge Settings")]
    public float chargeDuration = 1f;
    public GameObject chargeEffect;

    [Header("Laser Settings")]
    public GameObject laserPrefab;
    public Transform laserOrigin;
    public int initialLaserCount = 1;
    public int maxLaserCount     = 5;
    public float homingSpeed          = 180f;
    public float homingSwitchInterval = 2f;
    public float homingRampDuration   = 1f;

    [Tooltip("Delay before the very first beam (index 0) begins homing, seconds.")]
    public float firstBeamHomingDelay = 0.5f;

    [Header("Non-Homing Drift Settings")]
    public float minDriftSpeed = 10f;
    public float maxDriftSpeed = 60f;

    [Header("Timing & Damage")]
    public float laserDuration = 5f;
    public float restDuration  = 2f;
    public float damagePerSecond = 10f;

    [Header("Raycast")]
    public LayerMask hitMask;
    public LayerMask groundLayerMask;
    public GameObject reflectEffectPrefab;
    public float beamLength = 50f;

    [Header("Proximity AoE")]
    public float aoeRange = 2f;
    [Tooltip("Damage per SECOND dealt while player stays in AoE.")]
    public float aoeDamage = 20f;                // now DPS
    public float aoeCooldown = 10f;
    public float aoeDelay    = 1f;
    [Tooltip("How long AoE damage & impact VFX last.")]
    public float aoeImpactDuration = 1f;
    public GameObject aoeWarningPrefab;
    public GameObject aoeImpactPrefab;

    #endregion ----------------------------------------------------------------

    #region Private-state -----------------------------------------------------

    private S_CustomCharacterController playerController;
    private Transform player;

    private GameObject aoeWarningInstance;
    private GameObject aoeImpactInstance;

    private float aoeTimer = 0f;

    private int   currentLaserCount;
    private int   currentMainIndex;
    private float nextMainSwitchTime;
    private readonly List<Coroutine> activeLasers = new();

    #endregion ----------------------------------------------------------------

    #region Unity lifecycle ---------------------------------------------------

    private void Awake ()
    {
        playerController = FindObjectOfType<S_CustomCharacterController>();
        if (playerController != null) player = playerController.transform;

        currentLaserCount = Mathf.Clamp(initialLaserCount, 1, maxLaserCount);

        // Pool AoE VFX
        if (aoeWarningPrefab != null)
        {
            aoeWarningInstance = Instantiate(aoeWarningPrefab, transform.position, Quaternion.identity, transform);
            aoeWarningInstance.SetActive(false);
        }
        if (aoeImpactPrefab != null)
        {
            aoeImpactInstance = Instantiate(aoeImpactPrefab, transform.position, Quaternion.identity, transform);
            aoeImpactInstance.SetActive(false);
        }
    }

    private void Start () => StartCoroutine(AttackRoutine());

    private void Update () => HandleProximityAoE();

    #endregion ----------------------------------------------------------------

    #region AoE logic ---------------------------------------------------------

    /// <summary>
    /// Check distance & trigger AoE cooldown.
    /// </summary>
    private void HandleProximityAoE ()
    {
        if (player == null) return;

        aoeTimer -= Time.deltaTime;
        if (aoeTimer > 0f) return;
        if (Vector3.Distance(transform.position, player.position) > aoeRange) return;

        if (aoeWarningInstance != null)
        {
            aoeWarningInstance.transform.position = transform.position;
            aoeWarningInstance.SetActive(true);
        }

        StartCoroutine(DelayedAoE());
        aoeTimer = aoeCooldown;
    }

    /// <summary>
    /// Waits aoeDelay, then deals DPS for aoeImpactDuration while showing impact VFX.
    /// </summary>
    private IEnumerator DelayedAoE ()
    {
        yield return new WaitForSeconds(aoeDelay);

        if (aoeWarningInstance != null)
            aoeWarningInstance.SetActive(false);

        if (aoeImpactInstance != null)
        {
            aoeImpactInstance.transform.position = transform.position;
            aoeImpactInstance.SetActive(true);
        }

        float t = 0f;
        while (t < aoeImpactDuration)
        {
            t += Time.deltaTime;

            if (player != null &&
                Vector3.Distance(transform.position, player.position) <= aoeRange)
            {
                var receiver = player.GetComponent<S_PlayerDamageReceiver>();
                if (receiver != null)
                    receiver.ReceiveDamage(aoeDamage * Time.deltaTime);
            }
            yield return null;
        }

        if (aoeImpactInstance != null)
            aoeImpactInstance.SetActive(false);
    }

    #endregion ----------------------------------------------------------------

    #region Laser logic -------------------------------------------------------

    private IEnumerator AttackRoutine ()
    {
        while (true)
        {
            if (chargeEffect != null) chargeEffect.SetActive(true);
            yield return new WaitForSeconds(chargeDuration);
            if (chargeEffect != null) chargeEffect.SetActive(false);

            float volleyStartTime = Time.time;
            currentMainIndex   = 0;
            nextMainSwitchTime = volleyStartTime + homingSwitchInterval;

            activeLasers.Clear();
            for (int i = 0; i < currentLaserCount; ++i)
            {
                Vector3 originPos = GetLaserOrigin();
                Vector3 dir = player != null
                    ? (player.position - originPos).normalized
                    : transform.forward;

                GameObject laserGO = Instantiate(laserPrefab, originPos, Quaternion.LookRotation(dir));

                float driftSpeed = Random.Range(minDriftSpeed, maxDriftSpeed)
                                 * (Random.value < .5f ? -1f : 1f);

                activeLasers.Add(StartCoroutine(
                    LaserBeamRoutine(laserGO, i, driftSpeed, volleyStartTime)));
            }

            yield return new WaitForSeconds(laserDuration);

            foreach (var c in activeLasers)
                if (c != null) StopCoroutine(c);

            currentLaserCount = Mathf.Min(currentLaserCount + 1, maxLaserCount);

            yield return new WaitForSeconds(restDuration);
        }
    }

    private IEnumerator LaserBeamRoutine (
        GameObject laser,
        int beamIndex,
        float signedDriftSpeed,
        float volleyStartTime)
    {
        Transform beamTransform = laser.transform;

        float homingRampTimer = 0f;
        bool  wasHomingLastFrame = beamIndex == currentMainIndex;

        GameObject reflectInstance = null;

        while (Time.time - volleyStartTime < laserDuration)
        {
            float elapsedSinceVolley = Time.time - volleyStartTime;

            if (Time.time >= nextMainSwitchTime)
            {
                nextMainSwitchTime += homingSwitchInterval;
                int newIndex;
                do { newIndex = Random.Range(0, currentLaserCount); }
                while (newIndex == currentMainIndex);
                currentMainIndex = newIndex;
            }

            bool isHoming = beamIndex == currentMainIndex;

            // First-beam delay
            if (beamIndex == 0 && elapsedSinceVolley < firstBeamHomingDelay)
                isHoming = false;

            Vector3 originPos = GetLaserOrigin();

            if (isHoming && player != null)
            {
                if (!wasHomingLastFrame)
                {
                    homingRampTimer    = 0f;
                    wasHomingLastFrame = true;
                }
                homingRampTimer += Time.deltaTime;

                float speed = homingSpeed;
                if (homingRampDuration > 0f)
                    speed *= Mathf.Clamp01(homingRampTimer / homingRampDuration);

                Vector3 targetDir = (player.position - originPos).normalized;
                beamTransform.rotation = Quaternion.RotateTowards(
                    beamTransform.rotation,
                    Quaternion.LookRotation(targetDir),
                    speed * Time.deltaTime);
            }
            else
            {
                beamTransform.Rotate(0f,
                                     signedDriftSpeed * Time.deltaTime,
                                     0f,
                                     Space.World);
                wasHomingLastFrame = false;
            }

            // Damage raycast
            if (Physics.Raycast(originPos, beamTransform.forward,
                                out RaycastHit hit, beamLength, hitMask,
                                QueryTriggerInteraction.Ignore))
            {
                var receiver = hit.collider.GetComponent<S_PlayerDamageReceiver>();
                if (receiver != null)
                    receiver.ReceiveDamage(damagePerSecond * Time.deltaTime);
            }

            // Ground reflect VFX
            if (reflectEffectPrefab != null &&
                Physics.Raycast(originPos, beamTransform.forward,
                                out RaycastHit groundHit,
                                beamLength, groundLayerMask))
            {
                if (reflectInstance == null)
                    reflectInstance = Instantiate(reflectEffectPrefab,
                                                  groundHit.point, Quaternion.identity);
                else
                    reflectInstance.transform.position = groundHit.point;
            }

            yield return null;
        }

        if (reflectInstance != null) Destroy(reflectInstance);
        Destroy(laser);
    }

    #endregion ----------------------------------------------------------------

    #region Helpers -----------------------------------------------------------

    private Vector3 GetLaserOrigin () =>
        laserOrigin != null ? laserOrigin.position : transform.position;

    private void OnDrawGizmosSelected ()
    {
        Gizmos.color = new Color(1f, 0f, 0f);
        Gizmos.DrawWireSphere(transform.position, aoeRange);
    }

    #endregion ----------------------------------------------------------------
}
