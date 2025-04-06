using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the enemy object pool and listens for spawn requests.
/// </summary>
public class S_EnemyPoolManager : MonoBehaviour
{
    // Dictionary to store a pool (queue) of enemy objects for each enemy type.
    private Dictionary<EnemyType, Queue<GameObject>> pool = new();

    // Reference to the enemy tracker to monitor active enemies.
    public S_EnemyTracker tracker;

    // Subscribe to the spawn request event when the object is enabled.
    private void OnEnable()
    {
        S_SpawnRequester.OnRequestEnemySpawn += HandleSpawnRequest;
    }

    // Unsubscribe from the spawn request event when the object is disabled.
    private void OnDisable()
    {
        S_SpawnRequester.OnRequestEnemySpawn -= HandleSpawnRequest;
    }

    // This function handles a spawn request by either reusing an enemy from the pool or instantiating a new one.
    private void HandleSpawnRequest(EnemyConfig config, Vector3 position)
    {
        // Get an enemy from the pool (or create a new one if the pool is empty).
        GameObject enemy = GetFromPool(config.enemyType, config.enemyPrefab, position);
        // Add the enemy to the tracker so we can keep count of active enemies.
        tracker.Add(config.enemyType, enemy);

        // Get the EnemyBase component to subscribe to its death event.
        var baseScript = enemy.GetComponent<EnemyBase>();
        if (baseScript != null)
        {
            // When the enemy dies, HandleEnemyKilled will be called.
            baseScript.OnKilled += HandleEnemyKilled;
        }
    }

    // This function obtains an enemy object from the pool. If none are available, it creates a new instance.
    private GameObject GetFromPool(EnemyType type, GameObject prefab, Vector3 pos)
    {
        // If there is no pool (queue) for this enemy type, create one.
        if (!pool.ContainsKey(type))
            pool[type] = new Queue<GameObject>();

        GameObject obj;

        // Check if there are any inactive enemy objects available in the pool.
        if (pool[type].Count > 0)
        {
            // Dequeue an enemy from the pool.
            obj = pool[type].Dequeue();
            // Set the enemy's position to the desired spawn position.
            obj.transform.position = pos;
            // Reset the enemy's rotation.
            obj.transform.rotation = Quaternion.identity;
            // Activate the enemy object so it becomes visible and interactive.
            obj.SetActive(true);
        }
        else
        {
            // If the pool is empty, instantiate a new enemy object.
            obj = Instantiate(prefab, pos, Quaternion.identity);
        }

        return obj;
    }

    // This function handles the enemy's death event.
    private void HandleEnemyKilled(EnemyBase enemy)
    {
        // Unsubscribe from the enemy's OnKilled event to avoid memory leaks.
        enemy.OnKilled -= HandleEnemyKilled;
        // Remove the enemy from the tracker since it is no longer active.
        tracker.Remove(enemy.gameObject);
        // Deactivate the enemy so that it is not visible or interactive.
        enemy.gameObject.SetActive(false);

        // Ensure there is a pool for this enemy type. If not, create one.
        if (!pool.ContainsKey(enemy.enemyType))
            pool[enemy.enemyType] = new Queue<GameObject>();

        // Enqueue the enemy back into the pool for reuse later.
        pool[enemy.enemyType].Enqueue(enemy.gameObject);
    }
}
