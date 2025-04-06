using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    // Returns the spawn interval (in seconds) based on the current enemy count.
    public float GetSpawnInterval(int currentCount)
    {
        float clampedCount = Mathf.Clamp(currentCount, MinCount, MaxCount);
        float spawnRate = spawnRateCurve.Evaluate(clampedCount);
        return 1f / Mathf.Max(spawnRate, 0.01f);
    }
}

// Handles the timing and conditions for spawning enemies.
public class S_SpawnRequester : MonoBehaviour
{
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();  // List of enemy configurations.
    public static event System.Action<EnemyConfig, Vector3> OnRequestEnemySpawn;  // Event to request enemy spawn.

    private S_EnergyStorage playerEnergyStorage;   // Reference to the player's energy storage (holds player level).
    private S_EnemyTracker tracker = new S_EnemyTracker();  // Tracks the number of active enemies.
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
        while (true)
        {
            // Get the player's level.
            int playerLevel = playerEnergyStorage.currentLevelIndex + 1;
            // If the player's level is too low, wait 1 second and try again.
            if (playerLevel < config.minRequiredLevel)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            // Get the current number of active enemies of this type.
            int currentCount = tracker.GetCount(config.enemyType);

            // If the current count is less than the maximum allowed,
            // get spawn points for this enemy type.
            if (currentCount < config.MaxCount)
            {
                Vector3[] spawnPoints = S_ZoneManager.Instance.GetSpawnPointsByEnemyWithZoneWeight(config.enemyType);

                if (spawnPoints != null && spawnPoints.Length > 0)
                {
                    // Loop through each spawn point and request an enemy spawn.
                    foreach (Vector3 pos in spawnPoints)
                    {
                        if (currentCount >= config.MaxCount) break;
                        // Raise an event to request enemy spawn at the given position.
                        OnRequestEnemySpawn?.Invoke(config, pos);
                        currentCount++;
                    }
                }
            }

            // Get the wait time from the spawn rate curve based on current count.
            float waitTime = config.GetSpawnInterval(currentCount);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
