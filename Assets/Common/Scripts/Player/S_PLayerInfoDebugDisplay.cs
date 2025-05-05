using TMPro;
using UnityEngine;
using System.Collections.Generic;

public class S_PLayerInfoDebugDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI scoreText; // The UI text element to display score, kill rate, and energy gain

    [Header("Score Settings")]
    public float score = 0f; // The current score value
    public float scoreDecayRate = 5f; // How fast the score decreases over time
    public float decayDelay = 3f; // Time before score starts to decay after the last kill

    [Header("Kill Rate Display")]
    public float killRateWindow = 1f; // The time unit for scaling kill rate (1 = per second, 60 = per minute)
    public float killRateAverageWindow = 2f; // Time window (in seconds) to average kills
    public float lerpSpeed = 5f; // Speed at which kill rate display catches up
    public float coolDownSpeed = 3f; // Speed at which kill rate drops back down

    [Header("Energy Change Rate Display")]
    public float energyUpdateInterval = 0.2f; // How often to check for energy change (in seconds)

    private float smoothedKillRate = 0f; // Smoothed kill rate for display
    private float lastScoreIncreaseTime; // Timestamp of last score increase
    private Queue<float> killTimestamps = new Queue<float>(); // Timestamps of recent kills

    private float lastEnergy = 0f; // Last recorded energy value
    private float lastCheckTime = 0f; // Last time energy was checked
    private float energyDeltaPerSecond = 0f; // Energy gained/lost per second

    private S_EnergyStorage energyStorage;

    void Start()
    {
        UpdateScoreText();
        lastScoreIncreaseTime = Time.time;
        energyStorage = FindObjectOfType<S_EnergyStorage>();

        if (energyStorage != null)
        {
            lastEnergy = energyStorage.currentEnergy;
            lastCheckTime = Time.time;
        }
    }

    private void OnEnable()
    {
        EnemyBase.OnEnemyKilled += HandleKill; // Subscribe to kill event
    }

    private void OnDisable()
    {
        EnemyBase.OnEnemyKilled -= HandleKill; // Unsubscribe
    }

    public void HandleKill(EnemyType enemyType)
    {
        AddScore();
        RecordKillTimestamp();
    }

    private void Update()
    {
        UpdateKillRate();
        UpdateScoreDecay();
        UpdateEnergyChangeRate();
        UpdateScoreText();
    }

    private void AddScore()
    {
        score += 10f;
        lastScoreIncreaseTime = Time.time;
    }

    private void ReduceScoreOverTime()
    {
        score -= scoreDecayRate * Time.deltaTime;
        score = Mathf.Max(0f, score);
    }

    private void UpdateScoreDecay()
    {
        if (Time.time - lastScoreIncreaseTime > decayDelay)
        {
            ReduceScoreOverTime();
        }
    }

    private void RecordKillTimestamp()
    {
        killTimestamps.Enqueue(Time.time);
    }

    private void UpdateKillRate()
    {
        // Remove old timestamps outside of average window
        while (killTimestamps.Count > 0 && Time.time - killTimestamps.Peek() > killRateAverageWindow)
        {
            killTimestamps.Dequeue();
        }

        float targetRate = killTimestamps.Count / Mathf.Max(0.01f, killRateAverageWindow); // Avoid divide by zero

        // Smoothly increase or decrease toward target rate
        smoothedKillRate = Mathf.Lerp(smoothedKillRate, targetRate, Time.deltaTime *
            (targetRate > smoothedKillRate ? lerpSpeed : coolDownSpeed));
    }

    private void UpdateEnergyChangeRate()
    {
        if (energyStorage == null) return;

        float currentTime = Time.time;

        if (currentTime - lastCheckTime >= energyUpdateInterval)
        {
            float currentEnergy = energyStorage.currentEnergy;
            float deltaEnergy = currentEnergy - lastEnergy;
            float deltaTime = currentTime - lastCheckTime;

            energyDeltaPerSecond = deltaEnergy / Mathf.Max(0.01f, deltaTime);

            lastEnergy = currentEnergy;
            lastCheckTime = currentTime;
        }
    }

    private void UpdateScoreText()
    {
        int displayedScore = Mathf.FloorToInt(score);
        float scaledKillRate = smoothedKillRate * killRateWindow;

        string unit = Mathf.Approximately(killRateWindow, 1f) ? "sec" :
                      Mathf.Approximately(killRateWindow, 60f) ? "min" :
                      killRateWindow + "s";

        scoreText.text = "Score: " + displayedScore +
                         $"\nKills/{unit}: {scaledKillRate:F2}" +
                         $"\nEnergy/sec: {energyDeltaPerSecond:F2}";
    }
}
