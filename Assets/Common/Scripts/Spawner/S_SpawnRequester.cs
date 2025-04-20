using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

// Holds configuration for enemy spawning.
[System.Serializable]
public class EnemyConfig
{
    public EnemyType enemyType;       // Type of the enemy.
    public GameObject enemyPrefab;    // Prefab used to create the enemy.

    public AnimationCurve spawnRateCurve;  // Curve to control spawn frequency based on enemy count.
    public int minRequiredLevel = 0;         // Minimum player level required to spawn this enemy.

    // The smallest enemy count from the curve.
    public int MinCount => Mathf.FloorToInt(spawnRateCurve.keys[0].time);
    // The largest enemy count from the curve.
    public int MaxCount => Mathf.CeilToInt(spawnRateCurve.keys[^1].time);
    
}

// Handles the timing and conditions for spawning enemies.
public class S_SpawnRequester : MonoBehaviour
{
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();  // List of enemy configurations.
    public static event Action<EnemyConfig, Vector3> OnRequestEnemySpawn;  // Event to request enemy spawn.

    private S_EnergyStorage playerEnergyStorage;   // Reference to the player's energy storage (holds player level).
    private readonly S_EnemyTracker tracker = new S_EnemyTracker();  // Tracks the number of active enemies.
    private S_EnemyPoolManager poolManager;          // Manages enemy object pooling.

    private void Awake()
    {
        // Find the pool manager in the scene and assign our tracker to it.
        poolManager = FindObjectOfType<S_EnemyPoolManager>();
        poolManager.tracker = tracker;
    }

    private void Start()
    {
        // Find the player energy storage to get the player's level.
        playerEnergyStorage = FindObjectOfType<S_EnergyStorage>();
        if (playerEnergyStorage == null)
        {
            return;
        }

        // For each enemy configuration, register its type and start its spawn loop.
        foreach (var config in enemyConfigs)
        {
            tracker.RegisterType(config.enemyType);
            StartCoroutine(SpawnLoop(config));
        }
    }

    // Continuously spawns enemies based on the configuration.
    private IEnumerator SpawnLoop(EnemyConfig config)
    {
        float timer = 0f;

        while (true)
        {
            int playerLevel = playerEnergyStorage.currentLevelIndex + 1;
            if (playerLevel < config.minRequiredLevel)
            {
                yield return null;
                continue;
            }

            int currentCount = tracker.GetCount(config.enemyType);

            float clampedCount = Mathf.Clamp(currentCount, config.MinCount, config.MaxCount);
            float spawnRate = config.spawnRateCurve.Evaluate(clampedCount);

            if (spawnRate <= 0f)
            {
                yield return null;
                continue;
            }

            float interval = 1f / spawnRate;
            timer += Time.deltaTime;

            // Once enough time has passed, attempt to spawn
            if (timer >= interval)
            {
                Vector3[] spawnPoints = S_ZoneManager.Instance.GetSpawnPointsByEnemyWithZoneWeight(config.enemyType);
                if (spawnPoints != null && spawnPoints.Length > 0 && currentCount < config.MaxCount)
                {
                    Vector3 pos = spawnPoints[Random.Range(0, spawnPoints.Length)];
                    OnRequestEnemySpawn?.Invoke(config, pos);
                    timer = 0f; // reset timer after spawn
                }
            }

            yield return null; // Check every frame
        }
    }
}
