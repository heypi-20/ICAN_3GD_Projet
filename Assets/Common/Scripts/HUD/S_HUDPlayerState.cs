using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;

public class S_HUDPlayerState : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI killRateText;
    public TextMeshProUGUI energyRateText;

    [Header("Kill Rate Settings")]
    [Tooltip("Time window (seconds) used to compute kill rate")]
    public float killRateAverageWindow = 2f;
    [Tooltip("Display kills per minute instead of per second")]
    public bool showKillRatePerMinute = false;
    [Tooltip("Text to display when kill rate exceeds threshold")]
    public string killRateThresholdText = "MAX !!!";
    [Tooltip("Kill rate threshold for triggering text change and animation")]
    public float killRateThreshold = 5f;
    [Tooltip("Shake frequency (punches per second) when threshold exceeded")]
    public float killRateShakeFrequency = 2f;

    [Header("Energy Rate Settings")]
    [Tooltip("Interval (seconds) at which to sample energy change")]
    public float energyUpdateInterval = 0.2f;
    [Tooltip("Display energy change per minute instead of per second")]
    public bool showEnergyRatePerMinute = false;
    [Tooltip("Text to display when energy rate exceeds threshold")]
    public string energyRateThresholdText = "MAX !!!";
    [Tooltip("Energy rate threshold for triggering text change and animation")]
    public float energyRateThreshold = 10f;
    [Tooltip("Shake frequency (punches per second) when threshold exceeded")]
    public float energyRateShakeFrequency = 2f;

    [Header("Kill Rate Smoothing")]
    [Tooltip("Lerp speed when kill rate is increasing")]
    public float killRateLerpSpeed = 5f;
    [Tooltip("Lerp speed when kill rate is decreasing")]
    public float killRateCoolDownSpeed = 3f;

    [Header("Energy Rate Smoothing")]
    [Tooltip("Lerp speed when energy rate is increasing")]
    public float energyRateLerpSpeed = 5f;
    [Tooltip("Lerp speed when energy rate is decreasing")]
    public float energyRateCoolDownSpeed = 3f;

    // —— Internal State —— 
    private Queue<float> killTimestamps = new Queue<float>();
    private float smoothedKillRate = 0f;
    private bool isKillRateShaking = false;
    private Tweener killRateTween;

    private S_EnergyStorage energyStorage;
    private float lastEnergy = 0f;
    private float lastEnergyCheckTime = 0f;
    private float rawEnergyRate = 0f;
    private float smoothedEnergyRate = 0f;
    private bool isEnergyShaking = false;
    private Tweener energyRateTween;

    void Start()
    {
        // Initialize energy tracking
        energyStorage = FindObjectOfType<S_EnergyStorage>();
        if (energyStorage != null)
        {
            lastEnergy = energyStorage.currentEnergy;
            lastEnergyCheckTime = Time.time;
            smoothedEnergyRate = 0f;
        }
    }

    void OnEnable()
    {
        // Subscribe to enemy kill event
        EnemyBase.OnEnemyKilled += HandleKill;
    }

    void OnDisable()
    {
        // Unsubscribe from enemy kill event
        EnemyBase.OnEnemyKilled -= HandleKill;
    }

    private void HandleKill(EnemyType enemyType)
    {
        // Record timestamp for each kill
        killTimestamps.Enqueue(Time.time);
    }

    void Update()
    {
        UpdateKillRate();
        UpdateEnergyRate();
    }

    private void UpdateKillRate()
    {
        float now = Time.time;

        // Remove kill timestamps outside of the averaging window
        while (killTimestamps.Count > 0 && now - killTimestamps.Peek() > killRateAverageWindow)
            killTimestamps.Dequeue();

        // Calculate target kill rate (kills per second)
        float targetKillRate = killTimestamps.Count / Mathf.Max(0.01f, killRateAverageWindow);

        // If showing per minute, scale by 60
        if (showKillRatePerMinute)
            targetKillRate *= 60f;

        // Smoothly interpolate towards the target rate
        float killSpeed = targetKillRate > smoothedKillRate ? killRateLerpSpeed : killRateCoolDownSpeed;
        smoothedKillRate = Mathf.Lerp(smoothedKillRate, targetKillRate, Time.deltaTime * killSpeed);

        // Threshold check and animation
        if (smoothedKillRate > killRateThreshold)
        {
            killRateText.text = killRateThresholdText;
            if (!isKillRateShaking)
            {
                isKillRateShaking = true;
                killRateTween = killRateText.rectTransform
                    .DOPunchScale(Vector3.one * 0.2f, 1f, (int)killRateShakeFrequency)
                    .SetLoops(-1, LoopType.Restart);
            }
        }
        else
        {
            killRateText.text = smoothedKillRate.ToString("F0");
            if (isKillRateShaking)
            {
                isKillRateShaking = false;
                killRateTween.Kill();
                killRateText.rectTransform.localScale = Vector3.one;
            }
        }
    }

    private void UpdateEnergyRate()
    {
        if (energyStorage == null) return;

        float now = Time.time;

        // Sample raw energy rate at defined intervals
        if (now - lastEnergyCheckTime >= energyUpdateInterval)
        {
            float currentEnergy = energyStorage.currentEnergy;
            float delta = currentEnergy - lastEnergy;
            float dt = now - lastEnergyCheckTime;
            rawEnergyRate = delta / Mathf.Max(0.01f, dt);

            lastEnergy = currentEnergy;
            lastEnergyCheckTime = now;
        }

        // If showing per minute, scale by 60
        float displayedRawRate = rawEnergyRate;
        if (showEnergyRatePerMinute)
            displayedRawRate *= 60f;

        // Smoothly interpolate towards the displayed raw rate
        float energySpeed = displayedRawRate > smoothedEnergyRate ? energyRateLerpSpeed : energyRateCoolDownSpeed;
        smoothedEnergyRate = Mathf.Lerp(smoothedEnergyRate, displayedRawRate, Time.deltaTime * energySpeed);

        // Threshold check and animation
        if (Mathf.Abs(smoothedEnergyRate) > energyRateThreshold)
        {
            energyRateText.text = energyRateThresholdText;
            if (!isEnergyShaking)
            {
                isEnergyShaking = true;
                energyRateTween = energyRateText.rectTransform
                    .DOPunchScale(Vector3.one * 0.2f, 1f, (int)energyRateShakeFrequency)
                    .SetLoops(-1, LoopType.Restart);
            }
        }
        else
        {
            energyRateText.text = smoothedEnergyRate.ToString("F0");
            if (isEnergyShaking)
            {
                isEnergyShaking = false;
                energyRateTween.Kill();
                energyRateText.rectTransform.localScale = Vector3.one;
            }
        }
    }
}
