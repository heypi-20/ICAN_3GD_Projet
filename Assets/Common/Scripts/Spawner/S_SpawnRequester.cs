using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EnemyConfig
{
    public EnemyType enemyType;
    public GameObject enemyPrefab;

    public AnimationCurve spawnRateCurve;
    public int minRequiredLevel = 0;

    public int MinCount => Mathf.FloorToInt(spawnRateCurve.keys[0].time);
    public int MaxCount => Mathf.CeilToInt(spawnRateCurve.keys[^1].time);

    public float GetSpawnInterval(int currentCount)
    {
        float clampedCount = Mathf.Clamp(currentCount, MinCount, MaxCount);
        float spawnRate = spawnRateCurve.Evaluate(clampedCount);
        return 1f / Mathf.Max(spawnRate, 0.01f);
    }
}

public class S_SpawnRequester : MonoBehaviour
{
    public List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();
    public static event System.Action<EnemyConfig, Vector3> OnRequestEnemySpawn;

    private S_EnergyStorage playerEnergyStorage;
    private S_EnemyTracker tracker = new S_EnemyTracker();
    private S_EnemyPoolManager poolManager;

    private void Awake()
    {
        poolManager = FindObjectOfType<S_EnemyPoolManager>();
        poolManager.tracker = tracker; // ✅ 手动传入
    }
    private void Start()
    {
        playerEnergyStorage = FindObjectOfType<S_EnergyStorage>();

        if (playerEnergyStorage == null)
        {
            return;
        }

        foreach (var config in enemyConfigs)
        {
            tracker.RegisterType(config.enemyType);
            StartCoroutine(SpawnLoop(config));
        }
    }

    private IEnumerator SpawnLoop(EnemyConfig config)
    {
        while (true)
        {
            int playerLevel = playerEnergyStorage.currentLevelIndex + 1;
            if (playerLevel < config.minRequiredLevel)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            int currentCount = tracker.GetCount(config.enemyType);

            if (currentCount < config.MaxCount)
            {
                Vector3[] spawnPoints = S_ZoneManager.Instance.GetSpawnPointsByEnemyWithZoneWeight(config.enemyType);

                if (spawnPoints != null && spawnPoints.Length > 0)
                {
                    foreach (Vector3 pos in spawnPoints)
                    {
                        if (currentCount >= config.MaxCount) break;
                        OnRequestEnemySpawn?.Invoke(config, pos);
                        currentCount++;
                    }
                }
            }

            float waitTime = config.GetSpawnInterval(currentCount);
            yield return new WaitForSeconds(waitTime);
        }
    }
}
