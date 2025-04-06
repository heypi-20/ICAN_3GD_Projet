using System.Collections.Generic;
using UnityEngine;

public class S_EnemyPoolManager : MonoBehaviour
{
    private Dictionary<EnemyType, Queue<GameObject>> pool = new();
    public S_EnemyTracker tracker;

    private void OnEnable()
    {
        S_SpawnRequester.OnRequestEnemySpawn += HandleSpawnRequest;
    }

    private void OnDisable()
    {
        S_SpawnRequester.OnRequestEnemySpawn -= HandleSpawnRequest;
    }

    private void HandleSpawnRequest(EnemyConfig config, Vector3 position)
    {
        GameObject enemy = GetFromPool(config.enemyType, config.enemyPrefab, position);
        tracker.Add(config.enemyType, enemy);

        var baseScript = enemy.GetComponent<EnemyBase>();
        if (baseScript != null)
        {
            baseScript.OnKilled += HandleEnemyKilled;
        }
    }

    private GameObject GetFromPool(EnemyType type, GameObject prefab, Vector3 pos)
    {
        if (!pool.ContainsKey(type))
            pool[type] = new Queue<GameObject>();

        GameObject obj;

        if (pool[type].Count > 0)
        {
            obj = pool[type].Dequeue();
            obj.transform.position = pos;
            obj.transform.rotation = Quaternion.identity;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, pos, Quaternion.identity);
        }

        return obj;
    }

    private void HandleEnemyKilled(EnemyBase enemy)
    {
        enemy.OnKilled -= HandleEnemyKilled;
        tracker.Remove(enemy.gameObject);

        enemy.gameObject.SetActive(false);

        if (!pool.ContainsKey(enemy.enemyType))
            pool[enemy.enemyType] = new Queue<GameObject>();

        pool[enemy.enemyType].Enqueue(enemy.gameObject);
    }
}